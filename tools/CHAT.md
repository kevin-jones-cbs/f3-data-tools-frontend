# Chat UI

Open `/chat/southfork`. The backend startup instructions are in the sibling
repository at `f3-data-tools-backend/F3Lambda/Analytics/README.md`.

The member-facing UI shows prose, snapshot freshness, and structured results.
It deliberately does not render SQL or developer traces. Components use semantic
HTML tables with scoped CSS, alongside the existing Blazorise/Bootstrap styles;
there are no new frontend dependencies. `AnalyticsResult` is the presentation
boundary for adding charts using the existing Blazorise.Charts dependency later.

Optional browser verification (API/model responses are mocked):

```sh
# Requires an installed Playwright package; uses installed Google Chrome.
CHAT_UI_URL=http://localhost:5090 node tools/chat-ui-smoke.mjs
```

If Playwright is installed elsewhere, set `PLAYWRIGHT_MODULE` to its package
directory. Set `CHAT_SCREENSHOT` to an output path for a mock-data screenshot.
Checks cover setup state, question suggestions, tables, hidden SQL, follow-ups,
mobile overflow, new conversations, and cancellation. Backend tests separately
exercise the real DuckDB engine and OpenRouter protocol with a fake HTTP handler.
