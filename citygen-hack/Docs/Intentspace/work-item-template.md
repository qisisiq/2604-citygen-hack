# Intentspace Work Item Template

Use this shape for city-generator work contracts.

## Required Sections

- `Goal`
- `Inputs`
- `Outputs`
- `Non-goals`
- `Done condition`
- `Artifact paths`

## Optional Sections

- `Dependency hints`
- `Role hint`
- `Validation expectation`
- `Review notes`

## Example

```text
Intent: Implement InfrastructureAgent v1

Goal:
Generate infrastructure flows and support dependencies for each vertical band and
district.

Inputs:
- CityContext produced by MacroShapeAgent and VerticalZoningAgent
- The Ascending Ward canon
- city-context-schema.md

Outputs:
- InfrastructureAgent.cs
- InfrastructureFlow data entries
- generation log output
- one sample exported context JSON

Non-goals:
- final room placement
- final art assets
- runtime pathfinding

Done condition:
- each major band has water, waste, power, and maintenance support
- public-facing districts have visible support spaces
- restricted districts have hidden logistics
- output is deterministic from seed

Artifact paths:
- Assets/Scripts/CityGen/Agents/InfrastructureAgent.cs
- Assets/Scripts/CityGen/Data/InfrastructureFlow.cs
- Assets/CityGen/Exports/

Dependency hints:
- depends on VerticalZoningAgent v1
- unblocks DistrictAgent v1 and CirculationAgent v1

Role hint:
- graph-systems agent
```
