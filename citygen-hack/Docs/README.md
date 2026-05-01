# City Generator Docs

This folder separates the project into three layers:

- `Canon/`: fiction and design truth for the current city.
- `Contracts/`: the generator schema and agent pipeline contracts.
- `Intentspace/`: the coordination model, work template, and backlog guidance.
- `Prompts/`: prompt fragments split into reusable files.

Intentspace should sit above this as the coordination layer:

- Intentspace tracks visible work contracts and outputs.
- the Unity project stores code, schemas, seeds, and generated artifacts.
- each generator module should map cleanly to a small Intentspace work item.

Use these docs as the human-readable planning layer. The runtime data model
lives in `Assets/Scripts/CityGen/`.
