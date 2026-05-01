# Intentspace Work Contracts

## Role Of Intentspace

Use Intentspace as the coordination layer, not as the Unity runtime and not as
the city data store.

That means:

- the repo stores code, assets, schemas, seeds, and generated outputs
- Intentspace stores visible work, promises, completions, and discussion
- each generator module should correspond to a small contract-sized unit of work

## Mapping

- top-level intent: `Build procedural vertical city generator in Unity`
- child intent: `Define generator architecture and data schema`
- child intent: `Implement OriginAgent`
- child intent: `Implement TaxonomyMixingAgent`
- child intent: `Implement MacroShapeAgent`
- child intent: `Implement VerticalZoningAgent`
- child intent: `Implement ValidationAgent`

The concrete post-ready backlog now lives in:

- `Assets/CityGen/Backlog/citygen-intentspace-backlog.json`

Human-readable coordination notes live in:

- `Docs/Intentspace/coordination-plan.md`
- `Docs/Intentspace/ai-agent-workflow.md`
- `Docs/Intentspace/agent-runner.md`
- `Docs/Intentspace/operator-guide.md`
- `Docs/Intentspace/work-item-template.md`

## Output Convention

Each work intent should name:

- the target files
- the expected artifact paths
- the done condition
- the validation expectation

## Runtime Support

`CityContext` now includes lightweight Intentspace trace metadata:

- `IntentSpace.ProjectIntentId`
- `IntentSpace.ParentIntentId`
- `IntentSpace.CurrentWorkIntentId`
- `Artifacts`
- generation log entries that can carry the current work intent id

This is only for traceability. The Unity generator still runs locally and
deterministically.

## Posting Support

Use the backlog JSON as the structured source for project-wide work contracts.

The helper script for posting that backlog is:

- `Tools/citygen_publish_backlog_to_intentspace.py`

The helper script for posting general work updates is:

- `Tools/citygen_publish_work_update.py`

The helper scripts for AI promise and completion flow are:

- `Tools/citygen_route_work_to_ai.py`
- `Tools/citygen_complete_promised_work.py`

The helper script for the local end-to-end polling worker is:

- `Tools/citygen_agent_runner.py`
