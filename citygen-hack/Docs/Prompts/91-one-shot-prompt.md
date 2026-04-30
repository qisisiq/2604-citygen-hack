# One-Shot Prompt

Design and implement a Unity prototype for a multi-agent procedural vertical
city generator.

The city is a cylindrical sci-fi tower-city. It should be walkable at the
local level and legible at the macro level. It should feel historically layered
like Paris or Tokyo, but vertically organized.

Do not begin with meshes. Generate a semantic city graph first. The graph
should contain districts, functions, infrastructure, routes, history layers,
access levels, social hierarchy, and resource flows. Then convert that graph
into primitive Unity blockout geometry.

Use a mixed taxonomy profile with:

- temple/pilgrimage
- hospital/healing
- resource extraction
- military research
- organic megastructure

The first test city is The Ascending Ward, a sacred hospital city whose hidden
economy extracts labor and biological output from patients and residents.

Implement modular agents that read and write to a shared `CityContext`.
Prioritize coherence, navigation, vertical drama, and support-system logic over
final visuals.
