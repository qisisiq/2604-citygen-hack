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


def build_payload(item: dict, relative_backlog_path: str, depth: int) -> dict:
    payload = {
        "kind": item.get("kind", "unity.citygen.contract"),
        "title": item.get("title"),
        "summary": item.get("summary", ""),
        "goal": item.get("goal", ""),
        "inputs": item.get("inputs", []),
        "outputs": item.get("outputs", []),
        "nonGoals": item.get("nonGoals", []),
        "doneCondition": item.get("doneCondition", []),
        "artifactPaths": item.get("artifactPaths", []),
        "roleHint": item.get("roleHint", ""),
        "priority": item.get("priority", ""),
        "depth": depth,
        "sourceBacklogPath": relative_backlog_path,
        "childCount": len(item.get("children", [])),
    }
    return payload


def post_item(session, item: dict, parent_id: str, relative_backlog_path: str, depth: int, dry_run: bool, result_items: list[dict]) -> None:
    title = item.get("title", "Untitled CityGen Contract")
    payload = build_payload(item, relative_backlog_path, depth)

    if dry_run:
        item_id = f"dry-run:{depth}:{title}"
    else:
        message = session.intent(title, parent_id=parent_id, payload=payload)
        posted = session.post_and_confirm(
            message,
            step="intent.unity-citygen-backlog",
            confirm_space_id=parent_id,
            artifact_filename="unity-citygen-backlog-intent.json",
            timeout=6.0,
        )
        item_id = posted.get("intentId")

    result_items.append(
        {
            "title": title,
            "kind": payload["kind"],
            "depth": depth,
            "parentId": parent_id,
            "intentId": item_id,
        }
    )

    child_parent_id = item_id if item_id else parent_id
    for child in item.get("children", []):
        post_item(
            session=session,
            item=child,
            parent_id=child_parent_id,
            relative_backlog_path=relative_backlog_path,
            depth=depth + 1,
            dry_run=dry_run,
            result_items=result_items,
        )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace-root", required=True)
    parser.add_argument("--backlog-json", required=True)
    parser.add_argument("--parent-id", default="")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--skip-root", action="store_true")
    args = parser.parse_args()

    workspace_root = Path(args.workspace_root).resolve()
    backlog_path = Path(args.backlog_json).resolve()
    relative_backlog_path = str(backlog_path.relative_to(workspace_root))
    backlog_data = read_json(backlog_path)

    result_items: list[dict] = []
    session = None
    target_parent = args.parent_id or "root"

    if not args.dry_run:
        load_sdk()
        from http_space_tools import HttpSpaceToolSession

        endpoint = resolve_station_endpoint(workspace_root)
        agent_name = resolve_agent_name(workspace_root)

        session = HttpSpaceToolSession(
            endpoint=endpoint,
            workspace=workspace_root,
            agent_name=agent_name,
        )
        session.connect()

        target_parent = args.parent_id or session.current_space_id or session.declared_default_space_id or "root"

    if args.skip_root:
        for child in backlog_data.get("children", []):
            post_item(
                session=session,
                item=child,
                parent_id=target_parent,
                relative_backlog_path=relative_backlog_path,
                depth=1,
                dry_run=args.dry_run,
                result_items=result_items,
            )
    else:
        post_item(
            session=session,
            item=backlog_data,
            parent_id=target_parent,
            relative_backlog_path=relative_backlog_path,
            depth=0,
            dry_run=args.dry_run,
            result_items=result_items,
        )

    result = {
        "status": "ok",
        "dryRun": args.dry_run,
        "workspaceRoot": str(workspace_root),
        "backlogJson": str(backlog_path),
        "skipRoot": args.skip_root,
        "postedItems": result_items,
    }
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(json.dumps({"status": "error", "error": str(exc)}), file=sys.stderr)
        raise SystemExit(1)
