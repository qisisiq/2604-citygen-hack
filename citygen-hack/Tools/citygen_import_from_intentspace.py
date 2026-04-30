#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from datetime import datetime, timezone
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


def resolve_agent_name(workspace_root: Path) -> str:
    config_path = workspace_root / ".intent-space" / "config" / "station.json"
    if config_path.exists():
        config = read_json(config_path)
        agent_name = config.get("agentName")
        if isinstance(agent_name, str) and agent_name:
            return agent_name

    return "city-gen"


def resolve_observatory_url(workspace_root: Path) -> str:
    enrollment_path = workspace_root / ".intent-space" / "state" / "station-enrollment.json"
    if not enrollment_path.exists():
        return ""

    enrollment = read_json(enrollment_path)
    value = enrollment.get("observatory_url")
    return value if isinstance(value, str) else ""


def latest_state_from_thread(messages: list[dict]) -> tuple[str, str]:
    latest_state = "INTENT"
    summary = ""

    for message in sorted(messages, key=lambda item: item.get("timestamp", 0)):
        message_type = message.get("type")
        payload = message.get("payload") if isinstance(message.get("payload"), dict) else {}
        if message_type in {"PROMISE", "ACCEPT", "COMPLETE", "DECLINE", "ASSESS"}:
            latest_state = str(message_type)
            if message_type == "ASSESS":
                assessment = payload.get("assessment")
                if isinstance(assessment, str) and assessment:
                    latest_state = f"ASSESS:{assessment}"
            if not summary:
                summary = payload.get("summary") or payload.get("content") or payload.get("reason") or ""

    return latest_state, str(summary)


def normalize_item(session, message: dict, kind_filter: str | None) -> dict | None:
    payload = message.get("payload") if isinstance(message.get("payload"), dict) else {}
    kind = payload.get("kind") if isinstance(payload.get("kind"), str) else ""

    if kind_filter and kind != kind_filter:
        return None

    thread_messages = session.scan_full(message["intentId"]).get("messages", [])
    latest_state, summary = latest_state_from_thread(thread_messages)

    artifact_paths = payload.get("artifactPaths")
    if not isinstance(artifact_paths, list):
        artifact_paths = []

    tags = payload.get("tags")
    if not isinstance(tags, list):
        tags = []

    content = payload.get("content")
    if not isinstance(content, str):
        content = ""

    return {
        "IntentId": message.get("intentId", ""),
        "ParentId": message.get("parentId", ""),
        "SenderId": message.get("senderId", ""),
        "Content": content,
        "Kind": kind,
        "LatestState": latest_state,
        "Summary": summary,
        "ArtifactPaths": artifact_paths,
        "Tags": tags,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace-root", required=True)
    parser.add_argument("--output-json", required=True)
    parser.add_argument("--parent-id", default="")
    parser.add_argument("--kind-filter", default="")
    args = parser.parse_args()

    workspace_root = Path(args.workspace_root).resolve()
    output_json = Path(args.output_json).resolve()
    output_json.parent.mkdir(parents=True, exist_ok=True)

    load_sdk()
    from http_space_tools import HttpSpaceToolSession

    session = HttpSpaceToolSession(
        endpoint=resolve_station_endpoint(workspace_root),
        workspace=workspace_root,
        agent_name=resolve_agent_name(workspace_root),
    )
    session.connect()

    target_parent = args.parent_id or session.current_space_id or session.declared_default_space_id or "root"
    scan = session.scan_full(target_parent)
    items: list[dict] = []

    for message in scan.get("messages", []):
        if message.get("type") != "INTENT":
            continue
        if message.get("parentId") != target_parent:
            continue

        item = normalize_item(session, message, args.kind_filter or None)
        if item is not None:
            items.append(item)

    payload = {
        "ImportVersion": "0.1.0",
        "ImportedAtUtc": datetime.now(timezone.utc).isoformat(),
        "SpaceId": session.current_space_id or "",
        "ObservatoryUrl": resolve_observatory_url(workspace_root),
        "ParentFilter": target_parent,
        "Items": items,
    }

    output_json.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    print(json.dumps({"status": "ok", "count": len(items), "outputJson": str(output_json)}, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(json.dumps({"status": "error", "error": str(exc)}), file=sys.stderr)
        raise SystemExit(1)
