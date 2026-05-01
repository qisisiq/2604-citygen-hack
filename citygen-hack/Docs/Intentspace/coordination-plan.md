# Intentspace Coordination Plan

Use Intentspace as the coordination layer for the Unity city generator.

That means:

- Unity and the repo remain the source of truth for code, scenes, schemas, and generated artifacts.
- Intentspace holds work contracts, promises, completions, design discussion, and artifact references.
- Each posted intent should be small enough that one agent can pick it up, produce a concrete artifact, and report back.

## Current Status

The project is structurally ready for Intentspace, but the backlog had only been
described in prose.

The missing piece was a concrete task tree with:

- clear parent and child intents
- artifact paths
- done conditions
- dependency hints
- role hints for likely agent types

That task tree now lives here:

- `Assets/CityGen/Backlog/citygen-intentspace-backlog.json`

## How To Use It

1. Post one top-level project intent.
2. Post the child contracts under that project intent.
3. Let agents self-select contracts based on role fit.
4. Have each agent report code paths, JSON outputs, screenshots, or scene artifacts back into the matching intent.
5. Use imported work items in the Unity Inbox window as the local backlog view.

## Coordination Rule

Do not split work by district yet.

The current job is still generator construction, not content authoring for one
finished city. The right first decomposition is by system:

- semantic graph
- infrastructure
- districts
- circulation
- local layout
- rooms
- geometry
- asset placement
- validation
- editor tooling

Only after the generator can reliably produce district nodes and routes should
you create district-specific contracts such as `Refine Growth Crown` or `Block
out Pilgrim Intake Spiral`.
