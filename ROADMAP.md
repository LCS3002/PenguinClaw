# PenguinClaw Roadmap

---

## v0.2 — Reliability & Observability

**Goal:** make the agent loop production-stable and testable without Rhino installed.

- **Vision loop** — `capture_and_assess(prompt)` tool: captures the active viewport, sends it as a base64 image message in the next LLM turn, and returns the model's assessment. Enables self-verification ("did the boolean work?") and iterative visual correction.
- **Streaming UI** — stream partial tokens from the LLM response to the React UI so long operations show progress instead of a spinner. Requires chunked HTTP response from the server.
- **Stop button** — cancel an in-progress agent loop from the UI. `CancellationToken` propagated from the HTTP request through the agent loop and into every `InvokeOnUiThread` dispatch.
- **Configurable tool timeout** — expose the 30-second UI thread timeout as a config value.
- **Error resilience** — distinguish recoverable errors (tool failure) from fatal errors (API key invalid, Rhino crash) and handle them differently in the agent loop.
- **Provider status indicator** — live green/red dot in the Settings tab that pings the configured provider on a 30-second interval.
- **Vision thumbnails** — when a viewport capture is part of the agent turn, show the image inline as a thumbnail in the chat bubble.
- **Test suite** — 80+ unit tests covering: tool JSON parsing, history trimming, keyword scoring, provider format conversion, all runnable without Rhino installed.
- **CI** — GitHub Actions: build the C# plugin (requires Rhino SDK nuget), run all tests, lint the React UI on every push.
- **Mac support** — Rhino 8 for Mac uses Mono/.NET 6+. Audit `HttpListener`, `WebView`, and GDI+ icon calls for Mac compatibility.

---

## v0.3 — Intelligence & Scale

**Goal:** make the agent smarter about context and GH workflows.

- **Multi-turn context compression** — when history exceeds `MaxHistory`, summarise older turns into a compact system block rather than discarding them entirely. Preserve object IDs and key operations.
- **Token usage display** — show input/output/cached token counts per turn in the Tools tab. Helps users understand cost and cache effectiveness.
- **GH definition builder improvements** — wire validation, component position layout, ability to read back the full canvas state as a JSON description for round-trip editing.
- **Lossless Grasshopper canvas round-trip** — `read_gh_canvas()` returns a full JSON description of all components and wires, enabling the agent to edit existing definitions rather than building from scratch.
- **Object history** — link RhinoDoc object GUIDs to their creation tool calls in the action log for reliable identity across undo/redo.
- **Configurable max iterations and history** per-session from the Settings tab.

---

## v1.0 — Production

**Goal:** vision-guided autonomous operation across complex multi-step workflows.

- **Vision-guided self-correction** — after each tool call batch, automatically capture the viewport and ask the model whether the result matches intent. If not, the agent corrects and retries without user input.
- **Full GH canvas read/write** — complete bidirectional control over Grasshopper definitions: read component parameters, set non-slider inputs, connect/disconnect wires, delete components.
- **Multi-model routing** — route different task types to different providers automatically: fast/cheap model for short context queries, capable model for complex geometric reasoning, vision-capable model for assessment steps.
- **Plugin distribution** — signed `.rhp` packaged for Rhino's Package Manager (`yak`).
- **Mac parity** — all features functional on Rhino 8 for Mac.
- **Rate limit handling** — automatic backoff and retry on 429 responses from Anthropic and Groq.
- **Multi-file sessions** — Worksession awareness: agent understands which file each object belongs to.
