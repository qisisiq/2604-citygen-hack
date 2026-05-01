# Agent Runner

This is the first end-to-end local worker loop for CityGen.

It does:

1. poll a parent intent in Intentspace
2. select an eligible contract
3. post a visible `PROMISE`
4. write a local packet for the worker
5. launch a local AI CLI
6. collect a structured result
7. post `COMPLETE` on success
8. post a blocked update on failure

The runner script is:

- `Tools/citygen_agent_runner.py`

## Recommended First Worker

Run one worker against the semantic-graph branch:

```bash
python3 citygen-hack/Tools/citygen_agent_runner.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --parent-id intent-850dcf0e-e621-405c-b550-61b60afedba1 \
  --agent-name infra-agent-01 \
  --role-filter 'graph-systems agent' \
  --backend codex \
  --poll-seconds 90
```

That branch contains:

- `Implement InfrastructureAgent v1`
- `Implement GovernanceAgent v1`
- `Implement DistrictAgent v1`
- `Implement CirculationAgent v1`

With the `graph-systems agent` filter, the first eligible default claim target is
currently `Implement InfrastructureAgent v1`.

## One-Shot Mode

If you want a single pass instead of a daemon-like poll loop:

```bash
python3 citygen-hack/Tools/citygen_agent_runner.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --parent-id intent-850dcf0e-e621-405c-b550-61b60afedba1 \
  --agent-name infra-agent-01 \
  --role-filter 'graph-systems agent' \
  --backend codex \
  --once
```

## Dry Run

Use this to confirm selection and packet generation without posting a live
promise or launching a worker:

```bash
python3 citygen-hack/Tools/citygen_agent_runner.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --parent-id intent-850dcf0e-e621-405c-b550-61b60afedba1 \
  --agent-name infra-agent-01 \
  --role-filter 'graph-systems agent' \
  --backend codex \
  --once \
  --dry-run
```

## Backend Choice

Supported backends:

- `codex`
- `claude`

Codex is the default because `codex exec` supports a clean non-interactive
structured-output flow.

## Output Files

The runner writes local execution records under:

- `Docs/Intentspace/Runs/`

Each run stores:

- the worker prompt
- structured worker output
- summary markdown
- artifact path JSON
- stdout/stderr logs from the AI CLI

## Important Limitation

The success path is fully connected.

The failure path is only partially connected:

- if the worker reports `blocked`, the runner posts a blocked update under the contract
- the original `PROMISE` remains visible and may still need human judgment

That is acceptable for now, but it is not full autonomous recovery.
