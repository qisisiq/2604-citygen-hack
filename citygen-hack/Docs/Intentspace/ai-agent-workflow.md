# AI Agent Workflow

`Adopt Work Item` is a local Unity convenience for binding a visible contract to
the current `CityGenerator` session.

It is not the real AI-agent lifecycle.

For an actual AI-agent loop, use this pattern:

1. import visible work items from Intentspace
2. select a contract for a specific AI role
3. post a `PROMISE` into that contract's subspace
4. generate a local work packet for the AI
5. do the actual implementation in the Unity repo
6. post `COMPLETE` with artifact paths
7. optionally assess the result

## Why This Is Better Than `Adopt Work Item`

`Adopt Work Item` only changes local editor state.

`PROMISE` and `COMPLETE` make the AI worker's claim and delivery visible in the
shared space, which is what turns the setup into a real multi-agent coordination
system.

## Route A Contract To An AI

First import the current backlog:

```bash
python3 citygen-hack/Tools/citygen_import_from_intentspace.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --output-json /Users/luella/Github/2604-citygen-hack/citygen-hack/Assets/CityGen/Imports/intentspace-work-items.json \
  --parent-id intent-b8def2d6-e500-4acd-a528-767e5520a004
```

Then route one contract to an AI worker:

```bash
python3 citygen-hack/Tools/citygen_route_work_to_ai.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --import-json /Users/luella/Github/2604-citygen-hack/citygen-hack/Assets/CityGen/Imports/intentspace-work-items.json \
  --agent-name infra-agent-01 \
  --role-filter graph-systems
```

That does two things:

- posts a visible `PROMISE` for the selected contract
- writes a local packet file under `Docs/Intentspace/Packets/`

If you already know the exact contract, route it explicitly:

```bash
python3 citygen-hack/Tools/citygen_route_work_to_ai.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --import-json /Users/luella/Github/2604-citygen-hack/citygen-hack/Assets/CityGen/Imports/intentspace-work-items.json \
  --agent-name infra-agent-01 \
  --intent-id intent-5d95d598-9275-4d70-8986-79fb91b740b5
```

## Complete The Promised Work

After the AI has made real repo changes, post `COMPLETE`:

```bash
python3 citygen-hack/Tools/citygen_complete_promised_work.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --intent-id intent-5d95d598-9275-4d70-8986-79fb91b740b5 \
  --promise-id PROMISE_ID_HERE \
  --summary-file /absolute/path/to/summary.md \
  --artifact-path citygen-hack/Assets/Scripts/CityGen/Agents/InfrastructureAgent.cs \
  --artifact-path citygen-hack/Assets/Scripts/CityGen/Data/InfrastructureFlow.cs \
  --agent-name infra-agent-01
```

## Practical Rule

The AI should never treat Intentspace as the code workspace.

The AI should treat Intentspace as:

- the shared contract board
- the visible promise log
- the visible delivery log

The Unity repo remains the place where implementation actually happens.
