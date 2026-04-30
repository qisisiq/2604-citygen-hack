**Short Answer**

Yes, this is possible, but not because Intentspace does it by itself.

Intentspace would be the coordination layer:
- one agent posts “we need 12 hospital beds for General Wards”
- another agent searches approved asset sources
- another imports selected assets into Unity
- another places them using layout rules
- another validates scale, collisions, and navigation

So the actual internet access, asset download, import, and placement would come from tools you wire into the system, not from Intentspace alone.

**Important Constraint**

I would not design this as “crawl the internet freely and drop models into the scene.”

I would design it as:
- query approved APIs or curated sources
- fetch assets with license and metadata
- import into a local cache
- normalize scale/pivot/materials
- place by semantic anchors
- validate before keeping them

That is much safer and much more controllable.

**How Size / Scale / Placement Would Work**

You need a strict placement contract.

For scale:
- define Unity world scale as `1 unit = 1 meter`
- prefer assets with explicit dimensions or glTF-style meter conventions
- after import, measure `Renderer.bounds` or mesh bounds
- compare that against a target envelope from your archetype
- rescale to fit that envelope

Example:
- hospital bed archetype target: `2.1m x 1.0m x 0.8m`
- imported asset bounds: `1.4m x 0.7m x 0.5m`
- importer rescales it automatically to match the target class

For placement:
- don’t place assets by raw world coordinates first
- place them onto semantic anchors like:
  - `floor_center`
  - `wall_segment`
  - `corner`
  - `bed_socket`
  - `altar_socket`
  - `corridor_edge`
  - `balcony_edge`
- each room/district generates anchor points with orientation and allowed footprint
- the placement agent snaps assets onto those anchors

For validation:
- overlap check
- doorway clearance check
- corridor width check
- headroom check
- pivot/orientation sanity check
- style/category check
- optional screenshot review pass

That means the system is not asking “where should this mesh go?”  
It is asking “which approved anchor can accept an asset of this category and footprint?”

**Would You Need MCP?**

Not strictly.

You need some way for agents to access:
- asset search
- asset download
- metadata inspection
- Unity import
- scene placement
- validation

That can be done with:
- normal Python/HTTP scripts
- Unity editor scripts
- a local service
- MCP tools

MCP is the cleanest option if you want the agent to call these capabilities directly and consistently. It is especially useful if you want multiple agents to share the same controlled tool surface.

A good MCP surface would expose tools like:
- `search_assets`
- `download_asset`
- `inspect_asset_bounds`
- `import_asset_to_unity`
- `place_prefab`
- `validate_scene_blockout`

So: not required, but probably the right architecture if you want this to become a real agent workflow.

**What Intentspace’s Role Should Be**

Use Intentspace for:
- visible asset requests
- candidate selection discussion
- completion records
- artifact references
- approval/rejection history

Do not use Intentspace as:
- the asset database
- the mesh store
- the Unity runtime

The actual files should stay local in the Unity project or a managed asset cache.

**Best Practical Architecture**

Start with 4 layers:

- `Asset request agent`
  - turns district/room needs into asset specs
- `Asset sourcing agent`
  - queries approved APIs or curated catalogs
- `Import + normalization agent`
  - imports models, fixes scale/pivot/materials, creates prefabs
- `Placement + validation agent`
  - places prefabs into anchor slots and checks collisions/navigation

That is a much better first system than trying to do full open-ended internet crawling.

**What I Recommend First**

Before connecting outside APIs, build this locally first:
- define asset archetypes with target dimensions
- define anchor sockets in generated rooms/districts
- build an importer that normalizes prefab scale/pivot
- build a placement validator
- use a tiny curated test library of 10-20 assets

Once that works, add external asset search.

That way you solve:
- scale
- placement
- validation
- replacement/regeneration

before introducing internet uncertainty and licensing issues.

**For Your Current Project**

Yes, this fits the system we’re building.

The next good implementation step would be:
1. add `AssetSpec`, `AnchorPoint`, and `PlacementCandidate` schema types
2. add a local asset registry and prefab importer
3. add a placement validator in Unity
4. then add an Intentspace-facing asset request/export flow

If you want, I can implement that next:
- first the Unity-side asset placement schema and anchor system, or
- first the Intentspace-side asset request contract docs and export format.