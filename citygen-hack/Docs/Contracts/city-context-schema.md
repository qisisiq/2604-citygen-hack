# City Context Schema

The first-pass runtime schema lives under `Assets/Scripts/CityGen/`.

## Core Types

- `CitySeedData`: design input for one city profile.
- `CityContext`: shared mutable state passed from agent to agent.
- `CityAgentBase`: abstract base for generator modules.

## Semantic Layers

- `CityOriginData`: why the city exists and how it started.
- `CityIdentitySummary`: official, actual, legacy, and emergent identities.
- `CityGridDefinition`: macro cylindrical grid and structural properties.
- `VerticalLogicProfile`: top-level zoning and gradient rules.
- `NavigationPrinciples`: key routes and landmarks.

## Spatial Graph

- `VerticalBandData`: coarse vertical strata.
- `DistrictNode`: semantic districts that occupy floors, rings, and sectors.
- `RouteEdge`: public, hidden, restricted, and service circulation links.
- `LandmarkNode`: wayfinding anchors.
- `InfrastructureFlow`: support-system dependencies between districts.

## Enums

- `CityFunction`
- `AccessLevel`
- `HistoricalLayer`
- `TaxonomyKind`
- `MacroShapeKind`
- `RouteKind`
- `InfrastructureKind`

## Context Responsibilities

`CityContext` should be the only object handed between generation agents. It
currently stores:

- seed data copied into runtime form
- generated bands, districts, routes, landmarks, and infrastructure flows
- narrative hooks
- validation issues
- generation log entries

## Design Constraint

This schema is intentionally data-first. Early agents should write semantic
data only. Geometry comes later.
