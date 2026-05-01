#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


def load_sdk() -> None:
    candidates = [
        Path.home() / ".codex" / "skills" / "intent-space-agent-pack" / "sdk",
        Path.home() / ".claude" / "skills" / "intent-space-agent-pack" / "sdk",
        Path("marketplace") / "plugins" / "intent-space-agent-pack" / "sdk",
    ]

    for candidate in candidates:
        if candidate.exists():
            sys.path.insert(0, str(candidate))
            return

    raise RuntimeError("intent-space-agent-pack sdk not found")


def read_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def resolve_station_endpoint(workspace_root: Path) -> str:
    config_path = workspace_root / ".intent-space" / "config" / "station.json"
    enrollment_path = workspace_root / ".intent-space" / "state" / "station-enrollment.json"

    if config_path.exists():
        config = read_json(config_path)
        endpoint = config.get("endpoint")
        if isinstance(endpoint, str) and endpoint:
            return endpoint

    if enrollment_path.exists():
        enrollment = read_json(enrollment_path)
        endpoint = enrollment.get("station_endpoint") or enrollment.get("itp_endpoint")
        if isinstance(endpoint, str) and endpoint:
            return endpoint

    raise RuntimeError("no station endpoint found in .intent-space")


def resolve_agent_name(workspace_root: Path, override: str) -> str:
    if override:
        return override

    config_path = workspace_root / ".intent-space" / "config" / "station.json"
    if config_path.exists():
        config = read_json(config_path)
        agent_name = config.get("agentName")
        if isinstance(agent_name, str) and agent_name:
            return agent_name

    return "city-gen"


def read_summary(args: argparse.Namespace) -> str:
    if args.summary_file:
        return Path(args.summary_file).read_text(encoding="utf-8").strip()
    return (args.summary or "").strip()


def normalize_artifact_paths(workspace_root: Path, raw_paths: list[str]) -> list[str]:
    normalized: list[str] = []
    for raw_path in raw_paths:
        if not raw_path:
            continue
        path = Path(raw_path)
        if path.is_absolute():
            normalized.append(str(path.resolve().relative_to(workspace_root)))
        else:
            normalized.append(str(path).replace("\\", "/"))
    return normalized


def build_payload(summary: str, artifact_paths: list[str], status: str) -> dict:
    return {
        "kind": "unity.citygen.work-complete",
        "summary": summary,
        "status": status,
        "artifactPaths": artifact_paths,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace-root", required=True)
    parser.add_argument("--intent-id", required=True)
    parser.add_argument("--promise-id", required=True)
    parser.add_argument("--summary", default="")
    parser.add_argument("--summary-file", default="")
    parser.add_argument("--artifact-path", action="append", default=[])
    parser.add_argument("--status", default="complete")
    parser.add_argument("--agent-name", default="")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    workspace_root = Path(args.workspace_root).resolve()
    summary = read_summary(args)
    if not summary:
        raise RuntimeError("summary text is required")

    artifact_paths = normalize_artifact_paths(workspace_root, args.artifact_path)
    payload = build_payload(summary, artifact_paths, args.status)
    agent_name = resolve_agent_name(workspace_root, args.agent_name)

    if args.dry_run:
        result = {
            "status": "ok",
            "dryRun": True,
            "agentName": agent_name,
            "intentId": args.intent_id,
            "promiseId": args.promise_id,
            "payload": payload,
        }
        print(json.dumps(result, indent=2))
        return 0

    load_sdk()
    from http_space_tools import HttpSpaceToolSession

    session = HttpSpaceToolSession(
        endpoint=resolve_station_endpoint(workspace_root),
        workspace=workspace_root,
        agent_name=agent_name,
    )
    session.connect()

    complete = session.post_and_confirm(
        session.complete(
            promise_id=args.promise_id,
            parent_id=args.intent_id,
            summary=summary,
            payload=payload,
        ),
        step="complete.unity-citygen-ai",
        confirm_space_id=args.intent_id,
        artifact_filename="unity-citygen-ai-complete.json",
        timeout=6.0,
    )

    result = {
        "status": "ok",
        "dryRun": False,
        "agentName": agent_name,
        "intentId": args.intent_id,
        "promiseId": args.promise_id,
        "message": complete,
    }
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(json.dumps({"status": "error", "error": str(exc)}), file=sys.stderr)
        raise SystemExit(1)
