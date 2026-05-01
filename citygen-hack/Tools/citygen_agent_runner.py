#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import sys
import time
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_IMPORT_FOLDER = "citygen-hack/Assets/CityGen/Imports"
DEFAULT_RUN_FOLDER = "citygen-hack/Docs/Intentspace/Runs"
DEFAULT_PACKET_FOLDER = "citygen-hack/Docs/Intentspace/Packets"


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
    parser.add_argument("--packet-folder", default=DEFAULT_PACKET_FOLDER)
    parser.add_argument("--claim-content", default="I will take this contract and produce repo artifacts.")
    parser.add_argument("--dry-run", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    workspace_root = Path(args.workspace_root).resolve()
    run_folder = resolve_path(workspace_root, args.run_folder)
    run_folder.mkdir(parents=True, exist_ok=True)

    completed_tasks = 0
    emit_event(
        "runner.started",
        f"Runner started for agent `{args.agent_name}` on parent `{args.parent_id}`.",
        backend=args.backend,
        roleFilter=args.role_filter,
        maxTasks=args.max_tasks,
        once=args.once,
    )

    while True:
        packet_folder = resolve_path(workspace_root, args.packet_folder)
        packet_folder.mkdir(parents=True, exist_ok=True)
        imported_path = (
            Path(args.import_json).resolve()
            if args.import_json
            else run_import_snapshot(workspace_root, args.parent_id, args.import_folder, args.agent_name, args.dry_run)
        )
        emit_event("import.completed", f"Imported work snapshot from `{imported_path}`.")

        routed = resume_promised_work(
            workspace_root=workspace_root,
            imported_path=imported_path,
            packet_folder=packet_folder,
            agent_name=args.agent_name,
            role_filter=args.role_filter,
            intent_id=args.intent_id,
        )

        if routed is None:
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
                emit_event("runner.finished", "No eligible work items found for one-shot run.")
                return 0
            emit_event("runner.idle", "No eligible work items found. Waiting for the next poll.")
            time.sleep(max(args.poll_seconds, 1.0))
            continue

        emit_event(
            "work.selected",
            f"Selected `{routed.content}` ({routed.intent_id}).",
            intentId=routed.intent_id,
            promiseId=routed.promise_id,
        )

        run_dir = create_run_dir(run_folder, routed, args.agent_name)
        emit_event("worker.launch", f"Launching `{args.backend}` worker for `{routed.content}`.", runDir=str(run_dir))
        try:
            result = run_worker(
                backend=args.backend,
                backend_model=args.backend_model,
                workspace_root=workspace_root,
                routed=routed,
                run_dir=run_dir,
                agent_name=args.agent_name,
                dry_run=args.dry_run,
            )
        except Exception as exc:  # noqa: BLE001
            result = {
                "status": "blocked",
                "summary": f"Worker launch failed: {exc}",
                "artifact_paths": [routed.packet_relative_path],
                "notes": f"Backend `{args.backend}` failed before completion.",
            }

        result_path = run_dir / "worker-result.json"
        result_path.write_text(json.dumps(result, indent=2), encoding="utf-8")
        summary_path = run_dir / "summary.md"
        summary_path.write_text(str(result.get("summary", "")).strip() + "\n", encoding="utf-8")
        artifacts_path = run_dir / "artifacts.json"
        artifacts_path.write_text(json.dumps(result.get("artifact_paths", []), indent=2), encoding="utf-8")
        emit_event(
            "worker.result",
            f"Worker returned `{result.get('status', 'unknown')}` for `{routed.content}`.",
            intentId=routed.intent_id,
            promiseId=routed.promise_id,
            runDir=str(run_dir),
        )

        if not args.dry_run:
            if result.get("status") == "complete":
                post_complete(
                    workspace_root=workspace_root,
                    routed=routed,
                    summary_path=summary_path,
                    artifact_paths=result.get("artifact_paths", []),
                    agent_name=args.agent_name,
                )
                emit_event("intent.complete_posted", f"Posted COMPLETE for `{routed.content}`.", intentId=routed.intent_id)
            else:
                post_blocked_update(
                    workspace_root=workspace_root,
                    routed=routed,
                    summary_path=summary_path,
                    artifact_paths=result.get("artifact_paths", []),
                    agent_name=args.agent_name,
                )
                emit_event("intent.blocked_posted", f"Posted blocked update for `{routed.content}`.", intentId=routed.intent_id)

        completed_tasks += 1
        if args.once:
            emit_event("runner.finished", "Runner finished one-shot execution.", completedTasks=completed_tasks)
            return 0
        if args.max_tasks > 0 and completed_tasks >= args.max_tasks:
            emit_event("runner.finished", f"Runner reached max task limit ({args.max_tasks}).", completedTasks=completed_tasks)
            return 0

        emit_event("runner.sleep", f"Runner sleeping for {max(args.poll_seconds, 1.0):0.###} seconds.")
        time.sleep(max(args.poll_seconds, 1.0))


def resolve_path(workspace_root: Path, raw_path: str) -> Path:
    path = Path(raw_path)
    return path if path.is_absolute() else (workspace_root / path).resolve()


def read_json(path: Path) -> dict[str, Any]:
    return json.loads(path.read_text(encoding="utf-8"))


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


def resume_promised_work(
    *,
    workspace_root: Path,
    imported_path: Path,
    packet_folder: Path,
    agent_name: str,
    role_filter: str,
    intent_id: str,
) -> RoutedWork | None:
    imported = read_json(imported_path)
    items = imported.get("Items")
    if not isinstance(items, list):
        return None

    for item in items:
        item_intent_id = str(item.get("IntentId", ""))
        if intent_id and item_intent_id != intent_id:
            continue

        if str(item.get("LatestState", "")) != "PROMISE":
            continue

        if role_filter:
            item_role = str(item.get("RoleHint", ""))
            if item_role != role_filter:
                continue

        packet_path = packet_folder / f"{item_intent_id.replace(':', '_')}-{agent_name}.md"
        if not packet_path.exists():
            continue

        promise_id = extract_promise_id(packet_path)
        if not promise_id:
            continue

        emit_event("work.resumed", f"Resuming promised task `{item.get('Content', '')}`.", intentId=item_intent_id, promiseId=promise_id)

        return RoutedWork(
            intent_id=item_intent_id,
            content=str(item.get("Content", "")),
            promise_id=promise_id,
            packet_path=packet_path.resolve(),
            packet_relative_path=str(packet_path.resolve().relative_to(workspace_root)).replace("\\", "/"),
        )

    return None


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
        resolve_backend_executable("codex"),
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
        resolve_backend_executable("claude"),
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


def extract_promise_id(packet_path: Path) -> str:
    try:
        for line in packet_path.read_text(encoding="utf-8").splitlines():
            if line.startswith("- Promise ID:"):
                value = line.split("`")
                if len(value) >= 2:
                    return value[1].strip()
    except OSError:
        return ""

    return ""


def resolve_backend_executable(name: str) -> str:
    found = shutil.which(name)
    if found:
        return found

    home = Path.home()
    candidates: dict[str, list[Path]] = {
        "codex": [
            Path("/opt/homebrew/bin/codex"),
            Path("/usr/local/bin/codex"),
            home / ".local" / "bin" / "codex",
        ],
        "claude": [
            home / ".local" / "bin" / "claude",
            Path("/opt/homebrew/bin/claude"),
            Path("/usr/local/bin/claude"),
        ],
    }

    for candidate in candidates.get(name, []):
        if candidate.exists():
            return str(candidate)

    raise FileNotFoundError(f"{name} executable not found")


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


def emit_event(event_name: str, message: str, **fields: Any) -> None:
    payload = {
        "eventName": event_name,
        "message": message,
        **fields,
    }
    print(json.dumps(payload), flush=True)


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except KeyboardInterrupt:
        raise SystemExit(130)
