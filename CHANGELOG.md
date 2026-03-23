# Changelog

All notable changes to PenguinClaw are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [Unreleased]

---

## [0.1.0] — 2026-03-21

First public release.

### Added

#### Core Infrastructure
- Embedded HTTP server (`PenguinClawServer.cs`) on `localhost:8080` — `HttpListener`-based, background thread, each request dispatched to `ThreadPool`. Routes: `GET /health`, `GET /tools`, `POST /chat`, `GET /viewport`, `POST /settings`, `POST /rebuild-registry`, `GET /*` (static files).
- Dockable panel (`PenguinClawPanel.cs`) as an Eto `WebView` pointed at `localhost:8080`. WebView2 focus-loss recovery: reloads the WebView after a 3-second cooldown when Rhino regains focus, preventing the white-screen-on-tab-out bug.
- React UI (`penguinclaw/ui/App.jsx`) — Chat tab, Tools tab, Settings tab. PenguinMark SVG logo. Blue accent (`#2563EB`), off-white base (`#F7F6F2`). Built with Vite, output to `rhino_plugin/www/`, embedded into the DLL as manifest resources.
- Plugin entry point (`PenguinClawPlugin.cs`) starts the HTTP server and kicks off `RhinoCommandRegistry.Build()` on a background thread at load time.

#### AI Agent Loop
- ReAct loop (`PenguinClawAgent.cs`): receives user message + conversation history, calls the configured LLM with tool definitions, executes tool calls on the Rhino UI thread, feeds results back into the next turn. Terminates on `end_turn`, `error`, or after 25 iterations.
- `MaxTokens=8192`, `MaxIter=25`, `MaxHistory=30` messages.
- History trimming: never starts a trimmed history on an orphaned `tool_result` block.
- In-session scene state tracking: object IDs and types recorded after each tool call, injected into the system prompt for object-aware follow-ups ("move it", "scale the sphere").
- Persistent action log (`PenguinClawActionLog.cs`): up to 300 entries stored in `%APPDATA%/PenguinClaw/action_log.json`. Read-only tools are not logged. Last 25 entries formatted and appended to the system prompt as context across restarts.
- Prompt caching (Anthropic only): base system prompt, scan context, and last core tool definition each marked `cache_control: ephemeral`. After first request, cached blocks cost ~10% of normal price.

#### LLM Providers
- Anthropic provider (`AnthropicProvider`): sends Anthropic-format messages, supports `cache_control` blocks, handles `tool_use` and `end_turn` stop reasons. Default model: `claude-haiku-4-5`.
- OpenAI-compatible provider (`OpenAiCompatProvider`): shared implementation for Groq and Ollama. Converts Anthropic-format system blocks, messages (including `tool_result`/`tool_use` content), and tool definitions (`input_schema` → `parameters`) to OpenAI format.
- Groq backend via `OpenAiCompatProvider` at `https://api.groq.com/openai/v1`. Default model: `llama-3.3-70b-versatile`.
- Ollama backend via `OpenAiCompatProvider` at configurable URL (default `http://localhost:11434/v1`). Default model: `qwen2.5:7b`. No authentication required.
- Provider selection, API key, model override, and Ollama URL persisted in `%APPDATA%/PenguinClaw/config.json`. Fallback: `ANTHROPIC_API_KEY` environment variable.
- Human-readable API error messages for 401, 403, 429, 500, 503.

#### Core Tools (32 total, always sent to LLM)
- `get_selected_objects` — lists currently selected objects (ID, type, layer, name)
- `get_object_info` — type, layer, volume, area, bounding box for an object ID
- `get_volume` — volume of a Brep or Mesh
- `get_document_summary` — overview of all objects in the active document
- `select_objects_by_id` — selects objects by GUID array, replacing current selection
- `list_layers` — all layers with visibility and lock state
- `capture_viewport` — captures active viewport as PNG to `%TEMP%/penguinclaw/`, returns file path
- `run_rhino_command` — executes any Rhino command string via `RhinoApp.RunScript()`; returns resulting selected objects
- `execute_python_code` — runs Python code in the Rhino scripting engine with `doc` pre-set
- `move_object` — translates an object by (x, y, z) vector
- `scale_object` — scales an object uniformly in-place by a factor
- `rotate_object` — rotates an object around x/y/z axis by degrees
- `mirror_object` — mirrors an object across xy/xz/yz plane
- `array_linear` — creates N copies each offset by (dx, dy, dz)
- `array_polar` — creates N copies in a polar arrangement around a center point
- `delete_object` — deletes an object by ID
- `rename_object` — sets the name of an object by ID
- `undo` — calls `doc.BeginUndoRecord` / undoes N steps
- `redo` — redoes N steps
- `create_layer` — creates a new layer by name
- `set_current_layer` — sets the active layer
- `set_object_layer` — moves an object to a named layer
- `set_object_color` — sets object display colour (R, G, B 0–255)
- `boolean_union` — merges an array of solid Breps
- `boolean_difference` — subtracts cutter Breps from target Breps
- `boolean_intersection` — keeps the overlapping volume of two Brep sets
- `join_curves` — joins an array of open curves into a single polycurve
- `list_gh_sliders` — lists all Number Sliders on the active GH canvas
- `set_gh_slider` — sets a GH slider value by NickName, triggers new solution
- `list_gh_components` — lists all components on the active GH canvas
- `build_gh_definition` — programmatically builds a GH definition: creates components (slider, panel, toggle, or any component type by name) and wires them together

#### Dynamic Tool Registry
- `RhinoCommandRegistry`: builds a runtime index of every Rhino command and every installed GH component on a background thread at startup.
- Per-request keyword matching: tokenizes the user message, scores each `gh_comp_*` entry by token overlap, returns top-5 most relevant GH component tools. `rhino_cmd_*` entries excluded (covered by `run_rhino_command`).
- `PenguinClawScan` command: deep-indexes the Grasshopper `ComponentServer.ObjectProxies` registry, writes `scan_output.json`, rebuilds the in-memory registry via `POST /rebuild-registry`. Picks up third-party and user-installed GH plugins.
- Fallback command dictionary (80+ common Rhino commands with descriptions) for environments where `Command.GetCommandNames()` is unavailable.

#### Thread Safety
- All `RhinoDoc`/`RhinoApp`/Grasshopper calls dispatched to the UI thread via `RhinoApp.InvokeOnUiThread` + `ManualResetEventSlim` with a 30-second timeout.
- Grasshopper interop via reflection (`Assembly.Load("Grasshopper")`) — no hard compile-time GH.dll reference.

---

[Unreleased]: https://github.com/LCS3002/PenguinClaw/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/LCS3002/PenguinClaw/releases/tag/v0.1.0
