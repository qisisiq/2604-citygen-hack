Here’s the same plan as a checklist, with the Unity `.gitignore` work folded in.

**Project Checklist**

- [x] Treat the big prompt as 4 separate artifacts:
  - `city canon`
  - `generator contract`
  - `implementation backlog`
  - `review criteria`
- [x] Use Intentspace as the coordination layer, not the generator runtime or city-data store.
- [x] Keep Unity/repo files as the source of truth for code, assets, schemas, and generated artifacts.
- [x] Use Intentspace intents as small contracts, not one huge “build the whole city” task.

- [ ] Create one top-level project intent:
  - `Build procedural vertical city generator in Unity`
- [ ] Add child intents under it:
  - `Canon: The Ascending Ward`
  - `Define generator architecture and data schema`
  - `Scaffold Unity project and core runtime`
  - `Implement semantic graph pipeline v1`
  - `Implement validation and debug visualization v1`
  - `Implement primitive geometry blockout v1`
  - `Playtest seed and iterate on city logic`
- [ ] Decompose further only by generator module, not by district yet.

- [ ] Define a standard intent template for implementation work:
  - `Goal`
  - `Inputs`
  - `Outputs`
  - `Non-goals`
  - `Done condition`
  - `Artifact paths`

- [x] Start with a small first milestone:
  - `OriginAgent`
  - `TaxonomyMixingAgent`
  - `MacroShapeAgent`
  - `VerticalZoningAgent`
  - `ValidationAgent`
- [ ] Delay these until later:
  - district-specific interiors
  - room-level richness
  - final meshes/art
  - per-district “company/contractor” specialization

- [x] Create repo docs/artifact structure:
  - `docs/canon/`
  - `docs/contracts/`
  - `docs/seeds/`
  - `Assets/Scripts/CityGen/Core/`
  - `Assets/Scripts/CityGen/Data/`
  - `Assets/Scripts/CityGen/Agents/`
  - `Assets/Scripts/CityGen/Validation/`
  - `Assets/Scripts/CityGen/Debug/`
  - `Assets/CityGen/Profiles/`
  - `Assets/CityGen/Archetypes/`
  - `Assets/CityGen/Seeds/`

- [x] Create the first shared files:
  - `city-canon.md`
  - `generator-architecture.md`
  - `city-context-schema.md`
  - `ascending-ward.seed.json`

- [x] Keep the first playable prototype limited to:
  - semantic city graph
  - deterministic seed behavior
  - validation rules
  - gizmos/debug labels
  - primitive blockout geometry
