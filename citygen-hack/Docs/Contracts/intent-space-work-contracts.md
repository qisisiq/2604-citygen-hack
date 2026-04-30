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
