# Intentspace Operator Guide

This is the practical loop for coordinating CityGen work with Intentspace.

## 1. Post The Backlog Tree

Preview the backlog locally:

```bash
python3 citygen-hack/Tools/citygen_publish_backlog_to_intentspace.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --backlog-json /Users/luella/Github/2604-citygen-hack/citygen-hack/Assets/CityGen/Backlog/citygen-intentspace-backlog.json \
  --dry-run
```

Post it live:

```bash
python3 citygen-hack/Tools/citygen_publish_backlog_to_intentspace.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --backlog-json /Users/luella/Github/2604-citygen-hack/citygen-hack/Assets/CityGen/Backlog/citygen-intentspace-backlog.json
```

That creates:

- one top-level project intent
- child contracts under that top-level intent
- nested module intents under the semantic-graph contract

## 2. Import Work Items Into Unity

In Unity:

1. Open `Window > CityGen > IntentSpace Inbox`.
2. Set the target `CityGenerator`.
3. Click `Import Now`.
4. Review the imported backlog items.
5. Click `Adopt Work Item` on the contract you want the current local work to target.

Adopting a work item binds that intent into the generator state so exports can be
posted under the correct parent intent.

## 3. Make Sure The Work Happens In The Unity Repo

This is a workflow rule, not an automatic guarantee.

Use these constraints:

- Do the actual implementation in repo paths under `Assets/`, `Docs/`, `Packages/`, or `ProjectSettings/`.
- Treat Intentspace as coordination only.
- Require every work contract to include `Artifact paths`.
- Do not accept a work item as complete unless it points to real repo artifacts.

Good artifact references:

- `Assets/Scripts/CityGen/Agents/InfrastructureAgent.cs`
- `Assets/Scripts/CityGen/Data/RouteEdge.cs`
- `Assets/CityGen/Exports/ascending-ward-foundation-export.json`
- `Docs/Contracts/city-context-schema.md`

Bad artifact references:

- vague chat summaries with no file paths
- remote-only assets with no local import path
- geometry descriptions with no scene, prefab, or export reference

## 4. Publish Work Back Into The Matching Intent

There are two supported ways.

### A. Publish a Unity generator export

Use this when the current work produced a new city export or blockout snapshot.

In Unity:

1. Import the latest work items.
2. Click `Adopt Work Item` on the contract you are fulfilling.
3. Generate the latest context and blockout.
4. On `CityGenerator`, run `Publish IntentSpace Summary`.

That:

- writes a JSON export into `Assets/CityGen/Exports/`
- posts a child intent under the adopted work item
- includes artifact paths from the export data

### B. Publish a general work update from the repo

Use this when the work is code, schema, docs, or tooling and not just a city export.

Create a short summary file, then post it:

```bash
python3 citygen-hack/Tools/citygen_publish_work_update.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --parent-id INTENT_ID_HERE \
  --title "Completed InfrastructureAgent v1 foundation pass" \
  --summary-file /absolute/path/to/summary.md \
  --artifact-path citygen-hack/Assets/Scripts/CityGen/Agents/InfrastructureAgent.cs \
  --artifact-path citygen-hack/Assets/Scripts/CityGen/Data/InfrastructureFlow.cs \
  --artifact-path citygen-hack/Docs/Contracts/city-context-schema.md \
  --status complete \
  --work-kind implementation \
  --agent-role graph-systems
```

Use `--dry-run` first if you want to inspect the payload before posting it.

## 5. Recommended Completion Rule

A task should only be treated as complete when all three are true:

- repo changes exist at the declared artifact paths
- a summary/update has been posted under the matching intent
- Unity can import that work item back into the Inbox and show the linked artifacts
