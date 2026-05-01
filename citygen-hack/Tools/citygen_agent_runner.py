#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
import time
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_IMPORT_FOLDER = "citygen-hack/Assets/CityGen/Imports"
DEFAULT_RUN_FOLDER = "citygen-hack/Docs/Intentspace/Runs"


class CommandError(RuntimeError):
    def __init__(self, message: str, *, returncode: int, stdout: str, stderr: str) -> None:
        super().__init__(message)
        self.returncode = returncode
        self.stdout = stdout
        self.stderr = stderr


@dataclass
class RoutedWork:
    intent_id: str
    content: str
    promise_id: str
    packet_path: Path
    packet_relative_path: str


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace-root", required=True)
    parser.add_argument("--parent-id", required=True)
    parser.add_argument("--agent-name", required=True)
    parser.add_argument("--role-filter", default="")
    parser.add_argument("--intent-id", default="")
    parser.add_argument("--backend", choices=["codex", "claude"], default="codex")
    parser.add_argument("--backend-model", default="")
    parser.add_argument("--poll-seconds", type=float, default=90.0)
    parser.add_argument("--once", action="store_true")
    parser.add_argument("--max-tasks", type=int, default=0)
    parser.add_argument("--import-json", default="")
    parser.add_argument("--import-folder", default=DEFAULT_IMPORT_FOLDER)
    parser.add_argument("--run-folder", default=DEFAULT_RUN_FOLDER)
    parser.add_argument("--claim-content", default="I will take this contract and produce repo artifacts.")
    parser.add_argument("--dry-run", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    workspace_root = Path(args.workspace_root).resolve()
    run_folder = resolve_path(workspace_root, args.run_folder)
    run_folder.mkdir(parents=True, exist_ok=True)

    completed_tasks = 0

    while True:
        imported_path = (
            Path(args.import_json).resolve()
            if args.import_json
            else run_import_snapshot(workspace_root, args.parent_id, args.import_folder, args.agent_name, args.dry_run)
        )

        routed = route_work(
            workspace_root=workspace_root,
            imported_path=imported_path,
            agent_name=args.agent_name,
            role_filter=args.role_filter,
            intent_id=args.intent_id,
            claim_content=args.claim_content,
            dry_run=args.dry_run,
        )

        if routed is None:
            if args.once:
                return 0
            time.sleep(max(args.poll_seconds, 1.0))
            continue

        run_dir = create_run_dir(run_folder, routed, args.agent_name)
        result = run_worker(
            backend=args.backend,
            backend_model=args.backend_model,
            workspace_root=workspace_root,
            routed=routed,
            run_dir=run_dir,
            agent_name=args.agent_name,
            dry_run=args.dry_run,
        )

        result_path = run_dir / "worker-result.json"
        result_path.write_text(json.dumps(result, indent=2), encoding="utf-8")
        summary_path = run_dir / "summary.md"
        summary_path.write_text(str(result.get("summary", "")).strip() + "\n", encoding="utf-8")
        artifacts_path = run_dir / "artifacts.json"
        artifacts_path.write_text(json.dumps(result.get("artifact_paths", []), indent=2), encoding="utf-8")

        if not args.dry_run:
            if result.get("status") == "complete":
                post_complete(
                    workspace_root=workspace_root,
                    routed=routed,
                    summary_path=summary_path,
                    artifact_paths=result.get("artifact_paths", []),
                    agent_name=args.agent_name,
                )
            else:
                post_blocked_update(
                    workspace_root=workspace_root,
                    routed=routed,
                    summary_path=summary_path,
                    artifact_paths=result.get("artifact_paths", []),
                    agent_name=args.agent_name,
                )

        completed_tasks += 1
        if args.once:
            return 0
        if args.max_tasks > 0 and completed_tasks >= args.max_tasks:
            return 0

        time.sleep(max(args.poll_seconds, 1.0))


def resolve_path(workspace_root: Path, raw_path: str) -> Path:
    path = Path(raw_path)
    return path if path.is_absolute() else (workspace_root / path).resolve()


def run_import_snapshot(workspace_root: Path, parent_id: str, import_folder: str, agent_name: str, dry_run: bool) -> Path:
    folder = resolve_path(workspace_root, import_folder)
    folder.mkdir(parents=True, exist_ok=True)
    safe_name = agent_name.replace("/", "-").replace(" ", "-")
    output_path = folder / f"{safe_name}-work-items.json"

    if dry_run and output_path.exists():
        return output_path

    command = [
        sys.executable,
        "citygen-hack/Tools/citygen_import_from_intentspace.py",
        "--workspace-root",
        str(workspace_root),
        "--output-json",
        str(output_path),
        "--parent-id",
        parent_id,
    ]
    run_json_command(command, cwd=workspace_root)
    return output_path


def route_work(
    *,
    workspace_root: Path,
    imported_path: Path,
    agent_name: str,
    role_filter: str,
    intent_id: str,
    claim_content: str,
    dry_run: bool,
) -> RoutedWork | None:
    command = [
        sys.executable,
        "citygen-hack/Tools/citygen_route_work_to_ai.py",
        "--workspace-root",
        str(workspace_root),
        "--import-json",
        str(imported_path),
        "--agent-name",
        agent_name,
        "--claim-content",
        claim_content,
    ]

    if role_filter:
        command.extend(["--role-filter", role_filter])
    if intent_id:
        command.extend(["--intent-id", intent_id])
    if dry_run:
        command.append("--dry-run")

    try:
        payload = run_json_command(command, cwd=workspace_root)
    except CommandError as error:
        parsed = try_parse_json(error.stderr) or try_parse_json(error.stdout) or {}
        if parsed.get("error") == "no eligible work items found":
            return None
        raise

    return RoutedWork(
        intent_id=str(payload["intentId"]),
        content=str(payload["content"]),
        promise_id=str(payload["promiseId"]),
        packet_path=Path(str(payload["packetPath"])).resolve(),
        packet_relative_path=str(payload["packetRelativePath"]),
    )


def create_run_dir(run_folder: Path, routed: RoutedWork, agent_name: str) -> Path:
    timestamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    safe_agent = agent_name.replace("/", "-").replace(" ", "-")
    safe_intent = routed.intent_id.replace(":", "_")
    run_dir = run_folder / f"{timestamp}-{safe_agent}-{safe_intent}"
    run_dir.mkdir(parents=True, exist_ok=True)
    return run_dir


def run_worker(
    *,
    backend: str,
    backend_model: str,
    workspace_root: Path,
    routed: RoutedWork,
    run_dir: Path,
    agent_name: str,
    dry_run: bool,
) -> dict[str, Any]:
    schema = {
        "type": "object",
        "properties": {
            "status": {"type": "string", "enum": ["complete", "blocked"]},
            "summary": {"type": "string"},
            "artifact_paths": {"type": "array", "items": {"type": "string"}},
            "notes": {"type": "string"},
        },
        "required": ["status", "summary", "artifact_paths"],
        "additionalProperties": False,
    }

    schema_path = run_dir / "worker-output-schema.json"
    schema_path.write_text(json.dumps(schema, indent=2), encoding="utf-8")
    prompt_text = build_worker_prompt(workspace_root, routed.packet_path, routed.intent_id, routed.promise_id, agent_name)
    prompt_path = run_dir / "worker-prompt.md"
    prompt_path.write_text(prompt_text, encoding="utf-8")

    if dry_run:
        return {
            "status": "blocked",
            "summary": f"Dry run only. Selected contract `{routed.content}` for `{agent_name}`.",
            "artifact_paths": [routed.packet_relative_path],
            "notes": f"Worker backend `{backend}` was not launched.",
        }

    if backend == "codex":
        return run_codex_worker(workspace_root, backend_model, prompt_text, schema_path, run_dir)
    if backend == "claude":
        return run_claude_worker(workspace_root, backend_model, prompt_text, schema, run_dir)
    raise RuntimeError(f"unsupported backend: {backend}")


def build_worker_prompt(workspace_root: Path, packet_path: Path, intent_id: str, promise_id: str, agent_name: str) -> str:
    return f"""You are an automated CityGen worker.

Read the task packet at:
{packet_path}

IntentSpace context:
- intent id: {intent_id}
- promise id: {promise_id}
- agent name: {agent_name}

Workspace rules:
- Work in the workspace rooted at {workspace_root}.
- Most Unity project files for this task live under `citygen-hack/`.
- Make actual repo changes if you can complete the contract.
- Do not modify `.intent-space/` unless the task explicitly requires it.
- Do not only describe changes. Do the work in the repo.

When you finish, return JSON matching the provided schema:
- `status`: `complete` or `blocked`
- `summary`: short markdown summary of what changed or why you were blocked
- `artifact_paths`: workspace-relative paths you actually created or changed
- `notes`: optional short extra note

If you cannot complete the contract safely or concretely, return `blocked` and explain why.
"""


def run_codex_worker(workspace_root: Path, backend_model: str, prompt_text: str, schema_path: Path, run_dir: Path) -> dict[str, Any]:
    output_path = run_dir / "worker-last-message.json"
    stdout_path = run_dir / "worker-stdout.log"
    stderr_path = run_dir / "worker-stderr.log"

    command = [
        "codex",
        "exec",
        "--cd",
        str(workspace_root),
        "--full-auto",
        "--skip-git-repo-check",
        "--color",
        "never",
        "--output-schema",
        str(schema_path),
        "-o",
        str(output_path),
        "-",
    ]
    if backend_model:
        command.extend(["--model", backend_model])

    completed = subprocess.run(
        command,
        cwd=workspace_root,
        input=prompt_text,
        text=True,
        capture_output=True,
        env=build_worker_env(),
    )

    stdout_path.write_text(completed.stdout, encoding="utf-8")
    stderr_path.write_text(completed.stderr, encoding="utf-8")

    if completed.returncode != 0:
        raise CommandError(
            "codex worker failed",
            returncode=completed.returncode,
            stdout=completed.stdout,
            stderr=completed.stderr,
        )

    return json.loads(output_path.read_text(encoding="utf-8"))


def run_claude_worker(workspace_root: Path, backend_model: str, prompt_text: str, schema: dict[str, Any], run_dir: Path) -> dict[str, Any]:
    stdout_path = run_dir / "worker-stdout.log"
    stderr_path = run_dir / "worker-stderr.log"
    output_path = run_dir / "worker-last-message.json"

    command = [
        "claude",
        "-p",
        "--permission-mode",
        "dontAsk",
        "--output-format",
        "json",
        "--json-schema",
        json.dumps(schema),
    ]
    if backend_model:
        command.extend(["--model", backend_model])

    completed = subprocess.run(
        command,
        cwd=workspace_root,
        input=prompt_text,
        text=True,
        capture_output=True,
        env=build_worker_env(),
    )

    stdout_path.write_text(completed.stdout, encoding="utf-8")
    stderr_path.write_text(completed.stderr, encoding="utf-8")

    if completed.returncode != 0:
        raise CommandError(
            "claude worker failed",
            returncode=completed.returncode,
            stdout=completed.stdout,
            stderr=completed.stderr,
        )

    output_path.write_text(completed.stdout, encoding="utf-8")
    return json.loads(completed.stdout)


def build_worker_env() -> dict[str, str]:
    env = dict(os.environ)
    env.setdefault("TERM", "dumb")
    return env


def post_complete(
    *,
    workspace_root: Path,
    routed: RoutedWork,
    summary_path: Path,
    artifact_paths: list[Any],
    agent_name: str,
) -> None:
    command = [
        sys.executable,
        "citygen-hack/Tools/citygen_complete_promised_work.py",
        "--workspace-root",
        str(workspace_root),
        "--intent-id",
        routed.intent_id,
        "--promise-id",
        routed.promise_id,
        "--summary-file",
        str(summary_path),
        "--agent-name",
        agent_name,
    ]
    for artifact_path in normalize_artifact_paths(workspace_root, artifact_paths):
        command.extend(["--artifact-path", artifact_path])

    run_json_command(command, cwd=workspace_root)


def post_blocked_update(
    *,
    workspace_root: Path,
    routed: RoutedWork,
    summary_path: Path,
    artifact_paths: list[Any],
    agent_name: str,
) -> None:
    command = [
        sys.executable,
        "citygen-hack/Tools/citygen_publish_work_update.py",
        "--workspace-root",
        str(workspace_root),
        "--parent-id",
        routed.intent_id,
        "--title",
        f"Blocked: {routed.content}",
        "--summary-file",
        str(summary_path),
        "--kind",
        "unity.citygen.work-blocked",
        "--work-kind",
        "blocked",
        "--agent-role",
        agent_name,
        "--status",
        "blocked",
    ]
    for artifact_path in normalize_artifact_paths(workspace_root, artifact_paths):
        command.extend(["--artifact-path", artifact_path])

    run_json_command(command, cwd=workspace_root)


def normalize_artifact_paths(workspace_root: Path, values: list[Any]) -> list[str]:
    normalized: list[str] = []
    for value in values:
        if not isinstance(value, str) or not value:
            continue
        path = Path(value)
        if path.is_absolute():
            normalized.append(str(path.resolve().relative_to(workspace_root)))
        else:
            normalized.append(value.replace("\\", "/"))
    return normalized


def run_json_command(command: list[str], cwd: Path) -> dict[str, Any]:
    completed = subprocess.run(
        command,
        cwd=cwd,
        text=True,
        capture_output=True,
        env=build_worker_env(),
    )
    if completed.returncode != 0:
        raise CommandError(
            f"command failed: {' '.join(command)}",
            returncode=completed.returncode,
            stdout=completed.stdout,
            stderr=completed.stderr,
        )

    payload = try_parse_json(completed.stdout)
    if payload is None:
        raise RuntimeError(f"command did not return JSON: {' '.join(command)}")
    return payload


def try_parse_json(text: str) -> dict[str, Any] | None:
    stripped = text.strip()
    if not stripped:
        return None
    try:
        return json.loads(stripped)
    except json.JSONDecodeError:
        return None


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except KeyboardInterrupt:
        raise SystemExit(130)
