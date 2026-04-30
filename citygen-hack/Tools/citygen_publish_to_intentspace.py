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


def build_payload(export_data: dict, relative_export_path: str) -> dict:
    return {
        "kind": "unity.citygen.export",
        "cityName": export_data.get("CityName"),
        "seed": export_data.get("Seed"),
        "macroShape": export_data.get("MacroShape"),
        "summary": export_data.get("Summary"),
        "claimedIdentity": export_data.get("ClaimedIdentity"),
        "actualIdentity": export_data.get("ActualIdentity"),
        "artifactPaths": [relative_export_path, *export_data.get("ArtifactPaths", [])],
        "validationIssues": export_data.get("ValidationIssues", []),
        "landmarks": export_data.get("Landmarks", []),
        "bands": export_data.get("Bands", []),
        "intentSpace": export_data.get("IntentSpace", {}),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace-root", required=True)
    parser.add_argument("--export-json", required=True)
    parser.add_argument("--title", required=True)
    parser.add_argument("--parent-id", default="")
    args = parser.parse_args()

    workspace_root = Path(args.workspace_root).resolve()
    export_path = Path(args.export_json).resolve()
    relative_export_path = str(export_path.relative_to(workspace_root))

    load_sdk()
    from http_space_tools import HttpSpaceToolSession

    endpoint = resolve_station_endpoint(workspace_root)
    agent_name = resolve_agent_name(workspace_root)
    observatory_url = resolve_observatory_url(workspace_root)
    export_data = read_json(export_path)

    session = HttpSpaceToolSession(
        endpoint=endpoint,
        workspace=workspace_root,
        agent_name=agent_name,
    )
    session.connect()

    target_parent = args.parent_id or session.current_space_id or session.declared_default_space_id or "root"
    payload = build_payload(export_data, relative_export_path)

    message = session.intent(
        args.title,
        parent_id=target_parent,
        payload=payload,
    )

    posted = session.post_and_confirm(
        message,
        step="intent.unity-citygen-export",
        confirm_space_id=target_parent,
        artifact_filename="unity-citygen-export-intent.json",
        timeout=6.0,
    )

    result = {
        "status": "ok",
        "intentId": posted.get("intentId"),
        "parentId": target_parent,
        "spaceId": session.current_space_id,
        "observatoryUrl": observatory_url,
        "workspaceRoot": str(workspace_root),
        "exportJson": str(export_path),
    }
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(json.dumps({"status": "error", "error": str(exc)}), file=sys.stderr)
        raise SystemExit(1)
