# Generator Architecture

## Principle

The generator is semantic first, geometric second.

## Runtime Shape

- `CitySeedData` defines a city profile.
- `CityContext` is the shared state passed through the pipeline.
- each generator module derives from `CityAgentBase`
- early agents produce graph data, not meshes

## First-Pass Pipeline

1. `OriginAgent`
2. `TaxonomyMixingAgent`
3. `MacroShapeAgent`
4. `VerticalZoningAgent`
5. `ValidationAgent`

## Later Pipeline

Add infrastructure, districts, circulation, local layout, room generation, and
geometry after the graph is stable.
