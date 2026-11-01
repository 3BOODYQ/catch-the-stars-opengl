# Project guidance

This is a beginner OpenGL learning project. Read `docs/learning-roadmap.md` for
the agreed progression. Its components are planned until corresponding source
code exists. Teach with small runnable lessons and explain the important lines.
Do not implement later lessons unless requested.

## Graphify

The project knowledge graph is in `graphify-out/`.

- For `/graphify` requests, use the installed graphify skill.
- For codebase questions, query `graphify-out/graph.json` first when it exists.
  Use the graph's actual vocabulary, `graphify query`, `graphify path`, and
  `graphify explain` to find relevant files and relationships. Pass this guidance
  to subagents exploring the project.
- Verify facts in the cited source files. If the graph is missing, stale, or
  cannot answer the question, inspect the relevant files directly and say so.
  Never treat roadmap concepts as implemented code.
- After changing code, run `graphify update .` and `graphify export html`.
- After changing docs, use `/graphify . --update` through the skill; terminal
  AST-only updates do not replace semantic extraction. Rebuild with `/graphify .`
  if the graph is missing.
- Preserve `.graphifyignore` exclusions for generated files, dependencies,
  private settings, and secrets. Keep generated output out of source control.
- Do not start a persistent watcher or call an external LLM service implicitly.
  The Codex skill can extract documentation using the current agent session.
