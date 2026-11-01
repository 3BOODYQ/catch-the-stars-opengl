# Research and coding tools to review

Reviewed on 2026-10-31 for this beginner C#/OpenTK/OpenGL project. These are
recommendations, not approval to install, connect, or remove services.

## What the terms mean

A skill supplies reusable instructions. An MCP server supplies callable tools
and data. A plugin can bundle both. They are complementary layers, so installing
three versions of the same capability is usually unnecessary.
[OpenAI explanation](https://developers.openai.com/plugins/concepts/skills).

## Recommended research setup

| Capability | Recommendation | Current evidence and limit |
| --- | --- | --- |
| Web research | Use the built-in search and page reader first. | Available and used in this session; no new search plugin needed. Prioritize official library documentation and original source repositories. |
| Library documentation | Use the connected Context7 for focused library questions. | The user connected it. Live lookups resolved `/glfw/glfw` and `/opentk/opentk` and returned official window/context lifecycle documentation. OpenTK is the game's selected C# library. |
| Original source and issues | Use the existing GitHub integration when needed. | GitHub tools are exposed in this session; repository/account authorization has not been tested. Public official repositories can also be read through web tools. |
| Project relationships | Keep Graphify. | Installed locally as graphifyy 0.9.97. Its initial graph describes documentation and plans; code relationships arrive with the lessons. |
| Prior decisions and progress | Keep claude-mem. | Work-state reads and writes work in this session. Use it for project continuity rather than duplicating every note in another memory system. |
| Additional web retrieval | Consider Exa only if built-in search proves insufficient. | First-party docs describe web/code search and fetching. The current plugin-directory search returned no Exa result; that is not proof of global unavailability. It is not callable in this session. |

Context7's documentation describes version-specific retrieval; it is an index,
so verify important results against the original documentation.
[Context7 source](https://github.com/upstash/context7).
The GLFW test returned the project's own
[quick-start documentation](https://github.com/glfw/glfw/blob/master/docs/quick.md).
Exa is a possible later search alternative, not a second mandatory search layer.
[Exa MCP documentation](https://exa.ai/docs/get-started/exa-mcp).

## Skills worth using by stage

These skills are already listed in the current session; no duplicate skill pack
needs installing just to obtain them.

| Stage | Skill | Purpose |
| --- | --- | --- |
| Research | `mattpocock-skills:research` | Investigate a question against primary sources and save a cited Markdown note. |
| Understand the project | `graphify` | Query documented concepts and, later, source relationships. |
| Recover prior context | `claude-mem:mem-search` | Find earlier decisions through search, timeline, and selected observations. |
| Agree the next lesson | `create-plan` | Produce a small actionable implementation plan. |
| Start coding | `source-driven-development` | Verify library APIs and versions against official docs before implementing. |
| Implement | `incremental-implementation` | Deliver one runnable lesson at a time. |
| Diagnose failures | `debugging-and-error-recovery` | Reproduce and diagnose the actual failure before fixing it. |
| Review | `code-review-and-quality` | Review correctness and maintainability after changes. |

Use the project's ECC instructions as the main workflow; select focused skills
for the task instead of layering several complete orchestration frameworks.

## Later, during coding

The compiler, CMake, CTest, clangd, a native debugger, and a frame debugger are
ordinary development tools; they do not each need an MCP wrapper. Verify their
installation and fit when implementation begins. RenderDoc is a candidate for
inspecting OpenGL frames once there are draw calls to debug.
[RenderDoc source and supported APIs](https://github.com/baldurk/renderdoc).

Browser automation is optional for interactive documentation or a future web
interface; it does not test a native OpenGL window. Database, payment,
deployment, and design-service connectors have no current role in these lessons.

## Suggested session sequence

1. State the lesson question and what success looks like.
2. Query Context7 for focused library documentation; search official sources for
   broader questions or when the documentation index lacks coverage.
3. Inspect relevant official repository examples or issues when necessary.
4. Save a concise source-cited finding and distinguish verified facts from plans.
5. Query/update Graphify for project structure and use claude-mem for continuity.
6. Enter the coding session with one runnable milestone and its acceptance checks.

No new session has been created. The phrase "create-session" is treated here as
the future transition into implementation, not as a request to invoke a specific
command or create a chat.

## Token efficiency without weakening quality

The currently approved toolkit is Ponytail full mode, built-in web search, GitHub,
Graphify, claude-mem, and Context7. Other candidates below are review notes only;
do not activate them until the user explicitly asks.

Prefer selective retrieval and lossless representation before lossy summaries.
Keep the same acceptance checks, tests, error handling, and review standards.
Fewer visible tokens do not by themselves prove lower total session cost or
unchanged task quality. No project-specific savings percentage has been measured.

| Tool or practice | How it can reduce context | Status and quality condition |
| --- | --- | --- |
| Ponytail | Avoid unnecessary features, abstractions, dependencies, and repeated work by choosing the smallest complete solution. | Already active in full mode. Required behavior, validation, security, error handling, and appropriate tests remain required; no fixed token savings are promised. |
| Graphify | Locate relevant relationships before reading source. | Working project graph. Verify the actual source and expand searches when the graph is incomplete or stale; refresh only changed documents/code. |
| claude-mem search | Retrieve an index, inspect nearby context, then fetch selected observations instead of replaying entire chats. | Memory/work-state tools are available. Verify old decisions against current files. |
| Smart Explore | Search symbols, outline files, then unfold complete relevant functions instead of loading every file. | A live test failed because the tree-sitter CLI is missing. Do not rely on it until repaired; use scoped `rg` and source reads meanwhile. |
| Caveman TOON tools | Losslessly re-encode suitable uniform JSON tables. | Tools are exposed in this session; no project benchmark performed. Use only when smaller, preserve original JSON, and check round-trip fidelity. |
| Caveman compression/retrieval | Reduce bulky text while retaining a recovery handle for the original. | Compression is lossy. Do not apply blanket compression to instructions, exact source, failure diagnostics, or review evidence; retrieve the original whenever details affect correctness. |
| Focused Context7 queries | Fetch the relevant API topic instead of a whole library manual. | GLFW lookup verified. Narrow retrieval can still omit prerequisites, so follow referenced documentation when needed. |
| RTK | Filter repetitive terminal output into a shorter representation. | Optional candidate, not configured by this work. Assess support and correctness for actual C++ build/test commands before adopting; keep full logs and preserve failure exit codes. |

Graphify documents local AST extraction and graph queries in its
[official repository](https://github.com/Graphify-Labs/graphify).
Smart Explore's publisher describes its structural retrieval tools and a
[benchmark on a different codebase](https://github.com/thedotmack/claude-mem/blob/main/docs/public/smart-explore-benchmark.mdx);
those numbers are not measurements for this project.
[RTK's official repository](https://github.com/rtk-ai/rtk) describes its CLI proxy;
its advertised reductions are not a guarantee for our workload. Caveman tool
descriptions in this session distinguish lossless TOON encoding from lossy text
compression and label local statistics as inferred rather than verified billing.

Use one focused skill for the current job. Delegate independent work with only
the context it needs; several agents rereading the same files can increase total
token use. Do not select a weaker model for correctness-critical work solely to
reduce costs. Preserve original evidence when handing off between sessions.

For this project, start with Ponytail full mode, the existing Graphify and memory
tools, and focused Context7 queries. Repair Smart Explore only when structural
code navigation is needed; assess RTK later if real build/test output is bulky.
