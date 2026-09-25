// Optional browser smoke test. Uses an installed Playwright package and Chrome.
// CHAT_UI_URL defaults to the local frontend; API replies are mocked for UI tests.
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const browser = await chromium.launch({ channel: 'chrome', headless: true });
const snapshot = { refreshedAt: '2026-09-22T12:00:00Z', firstDate: '2021-08-21', lastDate: '2026-09-22' };
const requests = [];
try {
  const page = await browser.newPage({ viewport: { width: 1280, height: 950 } });
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  let configured = false;
  let delayReply = false;
  await page.route('**/chat/status*', route => route.fulfill({ json: { configured, snapshot }, headers: { 'access-control-allow-origin': '*' } }));
  await page.route('**/chat/stream', async route => {
    if (route.request().method() === 'OPTIONS') return route.fulfill({ status: 204, headers: {
      'access-control-allow-origin': '*', 'access-control-allow-methods': 'POST', 'access-control-allow-headers': 'content-type'
    } });
    requests.push(route.request().postDataJSON());
    if (delayReply) await new Promise(resolve => setTimeout(resolve, 1500));
    await route.fulfill({ headers: { 'access-control-allow-origin': '*' }, body: JSON.stringify({ type: 'complete', reply: {
      answer: 'The Way welcomed the most FNGs in this sample, followed by The Grid.',
      queries: [{ sql: 'SELECT secret_debug_sql FROM posts' }], snapshot,
      visualizations: [{ kind: 'table', title: 'Results', columns: [
        { key: 'ao', label: 'AO' }, { key: 'fngs', label: 'FNGs' }
      ], rows: [['The Way', 116], ['The Grid', 101]], truncated: false }]
    } }) + '\n', contentType: 'application/x-ndjson' });
  });
  const url = `${process.env.CHAT_UI_URL || 'http://localhost:5173'}/chat/southfork`;
  await page.goto(url);
  await page.getByText("Chat data for South Fork is not configured yet.", { exact: false }).waitFor();
  assert(await page.locator('#chat-question').isDisabled());
  configured = true;
  await page.reload();
  await page.getByRole('button', { name: 'Which AOs welcomed the most FNGs in 2025?' }).click();
  await page.getByRole('cell', { name: 'The Way', exact: true }).waitFor();
  assert.equal(await page.locator('tbody tr').count(), 2);
  assert(!(await page.locator('body').innerText()).includes('secret_debug_sql'));
  await page.locator('#chat-question').fill('And in 2024?');
  await page.locator('#chat-question').press('Enter');
  await page.waitForFunction(() => document.querySelectorAll('tbody tr').length === 4);
  assert.match(requests[0].visitorId, /^[0-9a-f-]{36}$/);
  assert.match(requests[0].conversationId, /^[0-9a-f-]{36}$/);
  assert.equal(requests[1].visitorId, requests[0].visitorId);
  assert.equal(requests[1].conversationId, requests[0].conversationId);
  assert.equal(requests[1].region, "southfork");
  assert.equal(requests[1].messages.length, 3);
  assert.equal(requests[1].messages[2].content, 'And in 2024?');
  if (process.env.CHAT_SCREENSHOT) await page.screenshot({ path: process.env.CHAT_SCREENSHOT, fullPage: true });
  await page.setViewportSize({ width: 390, height: 844 });
  assert(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth));
  await page.getByRole('button', { name: 'New chat', exact: true }).click();
  assert.equal(await page.locator('tbody tr').count(), 0);
  await page.goto(`${process.env.CHAT_UI_URL || 'http://localhost:5173'}/chat/asgard`);
  await page.getByRole('button', { name: 'Which AOs welcomed the most FNGs in 2025?' }).click();
  await page.getByRole('cell', { name: 'The Way', exact: true }).waitFor();
  assert.equal(requests.at(-1).region, 'asgard');
  assert.equal(requests.at(-1).messages.length, 1, 'Region changes start a fresh conversation');
  await page.getByRole('button', { name: 'New chat', exact: true }).click();
  delayReply = true;
  await page.locator('#chat-question').fill('Slow question');
  await page.locator('#chat-question').press('Enter');
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await page.getByRole('alert').waitFor();
  assert.equal(await page.locator('#chat-question').inputValue(), 'Slow question');
  assert.deepEqual(errors, []);
  console.log('PASS: setup state, suggestions, result table, hidden SQL, follow-up context, mobile layout, new chat, cancellation.');
} finally {
  await browser.close();
}
