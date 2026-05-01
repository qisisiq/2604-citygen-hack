#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path


PRIORITY_RANK = {
    "high": 0,
    "medium": 1,
    "low": 2,
    "": 3,
}

ELIGIBLE_STATES = {
    "INTENT",
    "",
}


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


def select_item(items: list[dict], intent_id: str, role_filter: str) -> dict:
    if intent_id:
        for item in items:
            if item.get("IntentId") == intent_id:
                return item
        raise RuntimeError(f"intent id not found in import json: {intent_id}")

    candidates: list[dict] = []
    for item in items:
        latest_state = str(item.get("LatestState", ""))
        if latest_state not in ELIGIBLE_STATES:
            continue
        if role_filter:
            role_hint = str(item.get("RoleHint", ""))
            if role_hint != role_filter:
                continue
        candidates.append(item)

    if not candidates:
        raise RuntimeError("no eligible work items found")

    candidates.sort(
        key=lambda item: PRIORITY_RANK.get(str(item.get("Priority", "")).lower(), 3)
    )
    return candidates[0]


def render_packet(item: dict, workspace_root: Path, promise_id: str, agent_name: str) -> str:
    lines = [
        f"# AI Work Packet: {item.get('Content', 'Untitled Contract')}",
        "",
        "## IntentSpace",
        f"- Contract Intent ID: `{item.get('IntentId', '')}`",
        f"- Parent Intent ID: `{item.get('ParentId', '')}`",
        f"- Promise ID: `{promise_id}`",
        f"- Agent Name: `{agent_name}`",
        f"- Kind: `{item.get('Kind', '')}`",
        f"- Priority: `{item.get('Priority', '')}`",
        f"- Role Hint: `{item.get('RoleHint', '')}`",
        "",
        "## Goal",
        item.get("Goal", "") or "(none provided)",
        "",
        "## Summary",
        item.get("Summary", "") or "(none provided)",
        "",
        "## Inputs",
    ]

    inputs = item.get("Inputs", [])
    if inputs:
        lines.extend([f"- `{value}`" for value in inputs])
    else:
        lines.append("- (none provided)")

    lines.extend(
        [
            "",
            "## Outputs",
        ]
    )
    outputs = item.get("Outputs", [])
    if outputs:
        lines.extend([f"- `{value}`" for value in outputs])
    else:
        lines.append("- (none provided)")

    lines.extend(
        [
            "",
            "## Done Condition",
        ]
    )
    done_condition = item.get("DoneCondition", [])
    if done_condition:
        lines.extend([f"- {value}" for value in done_condition])
    else:
        lines.append("- (none provided)")

    lines.extend(
        [
            "",
            "## Artifact Paths",
        ]
    )
    artifact_paths = item.get("ArtifactPaths", [])
    if artifact_paths:
        lines.extend([f"- `{value}`" for value in artifact_paths])
    else:
        lines.append("- (none provided)")

    lines.extend(
        [
            "",
            "## Repo Rules",
            f"- Work in the workspace rooted at `{workspace_root}`.",
            "- Most Unity project files for this task live under `citygen-hack/`.",
            "- Make actual changes in repo files, not only in chat.",
            "- Keep Intentspace as coordination only.",
            "- When done, post COMPLETE or a work update with real artifact paths.",
            "",
            "## Suggested Completion Command",
            "```bash",
            "python3 citygen-hack/Tools/citygen_complete_promised_work.py \\",
            f"  --workspace-root {workspace_root} \\",
            f"  --intent-id {item.get('IntentId', '')} \\",
            f"  --promise-id {promise_id} \\",
            "  --summary-file /absolute/path/to/summary.md \\",
            "  --artifact-path citygen-hack/Assets/... \\",
            f"  --agent-name {agent_name}",
            "```",
        ]
    )

    return "\n".join(lines).strip() + "\n"


def build_promise_payload(item: dict, packet_relative_path: str) -> dict:
    return {
        "kind": "unity.citygen.ai-promise",
        "roleHint": item.get("RoleHint", ""),
        "priority": item.get("Priority", ""),
        "artifactPaths": [packet_relative_path],
        "workIntentId": item.get("IntentId", ""),
        "sourceKind": item.get("Kind", ""),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--workspace-root", required=True)
    parser.add_argument("--import-json", required=True)
    parser.add_argument("--agent-name", default="")
    parser.add_argument("--intent-id", default="")
    parser.add_argument("--role-filter", default="")
    parser.add_argument("--packet-dir", default="citygen-hack/Docs/Intentspace/Packets")
    parser.add_argument("--claim-content", default="I will take this contract and produce repo artifacts.")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    workspace_root = Path(args.workspace_root).resolve()
    import_json = Path(args.import_json).resolve()
    packet_dir = Path(args.packet_dir)
    if not packet_dir.is_absolute():
        packet_dir = (workspace_root / packet_dir).resolve()
    packet_dir.mkdir(parents=True, exist_ok=True)

    imported = read_json(import_json)
    items = imported.get("Items")
    if not isinstance(items, list):
        raise RuntimeError("import json does not contain Items")

    item = select_item(items, args.intent_id, args.role_filter)
    agent_name = resolve_agent_name(workspace_root, args.agent_name)
    promise_id = f"dry-run-promise-{item.get('IntentId', 'unknown')}"

    packet_name = f"{item.get('IntentId', 'intent').replace(':', '_')}-{agent_name}.md"
    packet_path = packet_dir / packet_name
    packet_relative_path = str(packet_path.relative_to(workspace_root)).replace("\\", "/")

    if not args.dry_run:
        load_sdk()
        from http_space_tools import HttpSpaceToolSession

        session = HttpSpaceToolSession(
            endpoint=resolve_station_endpoint(workspace_root),
            workspace=workspace_root,
            agent_name=agent_name,
        )
        session.connect()

        promise = session.post_and_confirm(
            session.promise(
                parent_id=item["IntentId"],
                intent_id=item["IntentId"],
                content=args.claim_content,
                payload=build_promise_payload(item, packet_relative_path),
            ),
            step="promise.unity-citygen-ai",
            confirm_space_id=item["IntentId"],
            artifact_filename="unity-citygen-ai-promise.json",
            timeout=6.0,
        )
        promise_id = promise.get("promiseId", promise_id)

    packet_text = render_packet(item, workspace_root, promise_id, agent_name)
    packet_path.write_text(packet_text, encoding="utf-8")

    result = {
        "status": "ok",
        "dryRun": args.dry_run,
        "agentName": agent_name,
        "intentId": item.get("IntentId", ""),
        "content": item.get("Content", ""),
        "promiseId": promise_id,
        "packetPath": str(packet_path),
        "packetRelativePath": packet_relative_path,
    }
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(json.dumps({"status": "error", "error": str(exc)}), file=sys.stderr)
        raise SystemExit(1)
