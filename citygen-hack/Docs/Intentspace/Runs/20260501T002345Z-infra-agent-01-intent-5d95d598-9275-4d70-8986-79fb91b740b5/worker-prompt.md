You are an automated CityGen worker.

Read the task packet at:
/Users/luella/Github/2604-citygen-hack/citygen-hack/Docs/Intentspace/Packets/intent-5d95d598-9275-4d70-8986-79fb91b740b5-infra-agent-01.md

IntentSpace context:
- intent id: intent-5d95d598-9275-4d70-8986-79fb91b740b5
- promise id: promise-6488711b-2ffe-4461-8bfd-087b3cc052f3
- agent name: infra-agent-01

Workspace rules:
- Work in the workspace rooted at /Users/luella/Github/2604-citygen-hack.
- Most Unity project files for this task live under `citygen-hack/`.
- Make actual repo changes if you can complete the contract.
- Do not modify `.intent-space/` unless the task explicitly requires it.
- Do not only describe changes. Do the work in the repo.

When you finish, return JSON matching the provided schema:
- `status`: `complete` or `blocked`
- `summary`: short markdown summary of what changed or why you were blocked
- `artifact_paths`: workspace-relative paths you actually created or changed
- `notes`: optional short extra note

If you cannot complete the contract safely or concretely, return `blocked` and explain why.
