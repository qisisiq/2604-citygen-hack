# AI Work Packet: Implement DistrictAgent v1

## IntentSpace
- Contract Intent ID: `intent-04d757a5-cc64-4d60-b869-f7ada3c0d667`
- Parent Intent ID: `intent-850dcf0e-e621-405c-b550-61b60afedba1`
- Promise ID: `dry-run-promise-intent-04d757a5-cc64-4d60-b869-f7ada3c0d667`
- Agent Name: `infra-agent-01`
- Kind: `unity.citygen.module`
- Priority: `high`
- Role Hint: `graph-systems agent`

## Goal
Convert bands and sectors into connected district nodes with clear identities.

## Summary
(none provided)

## Inputs
- `CityContext after InfrastructureAgent and GovernanceAgent`
- `Docs/Prompts/Agents/07-district-agent.md`

## Outputs
- `DistrictAgent.cs`
- `DistrictNode population`
- `Landmark assignments`

## Done Condition
- Each generated district has function, access, motifs, and dependencies.
- Districts reflect the city's mixed taxonomy.
- Districts are represented as graph nodes, not decorative labels.

## Artifact Paths
- `Assets/Scripts/CityGen/Agents/DistrictAgent.cs`
- `Assets/Scripts/CityGen/Data/DistrictNode.cs`
- `Assets/CityGen/Exports/`

## Repo Rules
- Work in the Unity repo rooted at `/Users/luella/Github/2604-citygen-hack`.
- Make actual changes in repo files, not only in chat.
- Keep Intentspace as coordination only.
- When done, post COMPLETE or a work update with real artifact paths.

## Suggested Completion Command
```bash
python3 citygen-hack/Tools/citygen_complete_promised_work.py \
  --workspace-root /Users/luella/Github/2604-citygen-hack \
  --intent-id intent-04d757a5-cc64-4d60-b869-f7ada3c0d667 \
  --promise-id dry-run-promise-intent-04d757a5-cc64-4d60-b869-f7ada3c0d667 \
  --summary-file /absolute/path/to/summary.md \
  --artifact-path citygen-hack/Assets/... \
  --agent-name infra-agent-01
```
