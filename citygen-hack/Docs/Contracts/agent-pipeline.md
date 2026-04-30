# Agent Pipeline Contract

Each generator module should read from and write to `CityContext`.

## Shared Rules

- deterministic from seed
- designer-tunable later
- logs what it decided and why
- does not create final meshes unless it is the geometry stage
- should produce outputs small enough to map to one Intentspace work contract
- should reference artifact paths rather than treating Intentspace as a geometry store

## First-Pass Order

1. `CityOriginAgent`
2. `TaxonomyMixingAgent`
3. `MacroShapeAgent`
4. `VerticalZoningAgent`
5. `InfrastructureAgent`
6. `GovernanceAgent`
7. `DistrictAgent`
8. `CirculationAgent`
9. `LocalLayoutAgent`
10. `RoomAgent`
11. `HistoryMutationAgent`
12. `NarrativeHookAgent`
13. `ValidationAgent`
14. `GeometryAgent`

## Prototype Cut

Build the first milestone with only:

1. `CityOriginAgent`
2. `TaxonomyMixingAgent`
3. `MacroShapeAgent`
4. `VerticalZoningAgent`
5. `ValidationAgent`

That is enough to validate the semantic graph before room and geometry detail.
