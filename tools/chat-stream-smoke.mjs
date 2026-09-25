import assert from 'node:assert/strict';
import http from 'node:http';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const snapshot = { refreshedAt: '2026-09-22', firstDate: '2023-01-01', lastDate: '2025-12-31' };
let finished = false;
let disconnected = false;
const server = http.createServer(async (req, res) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Headers', 'content-type');
  if (req.method === 'OPTIONS') { res.end(); return; }
  req.resume();
  res.setHeader('Content-Type', 'application/x-ndjson');
  res.on('close', () => { disconnected = true; });
  const emit = v => res.write(JSON.stringify(v) + '\n');
  emit({ type: 'status', text: 'Looking through the attendance records…' });
  let text = 'First streamed words.';
  emit({ type: 'answer', text });
  for (let i = 0; i < 8; i++) {
    await new Promise(r => setTimeout(r, 250));
    text += '\n\nAttendance is shown below for this historical period. '.repeat(3);
    emit({ type: 'answer', text });
  }
  finished = true;
  emit({ type: 'complete', reply: { answer: text, snapshot, visualizations: [{ kind: 'table', title: 'Results',
    columns: [{ key: 'day_of_week', label: 'Day Of Week' }, { key: 'start_date', label: 'Start Date' },
      { key: 'end_date', label: 'End Date' }, { key: 'consecutive_posting_days', label: 'Consecutive Posting Days' }],
    rows: [[5, '2023-11-13', '2023-11-26', 14]], truncated: false }] } });
  res.end();
});
await new Promise(r => server.listen(0, '127.0.0.1', r));
const port = server.address().port;
const browser = await chromium.launch({ channel: 'chrome', headless: true });
try {
  const page = await browser.newPage({ viewport: { width: 390, height: 844 } });
  await page.route('**/chat/status*', route => route.fulfill({ json: { configured: true, snapshot }, headers: { 'access-control-allow-origin': '*' } }));
  await page.route('**/chat/stream', route => route.continue({ url: `http://127.0.0.1:${port}/chat/stream` }));
  await page.goto(`${process.env.CHAT_UI_URL || 'http://localhost:5173'}/chat/southfork`);
  await page.getByRole('button', { name: 'Which AOs welcomed the most FNGs in 2025?' }).click();
  await page.getByText('First streamed words.', { exact: true }).waitFor();
  assert.equal(finished, false, 'First text must display before response completes');
  await page.getByRole('cell', { name: 'Friday', exact: true }).waitFor();
  await page.waitForTimeout(200);
  assert(await page.evaluate(() => window.scrollY > 100), 'Should scroll as answer grows');
  assert(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth), 'Page should not overflow horizontally');
  const styles = await page.locator('td').nth(1).evaluate(el => getComputedStyle(el).whiteSpace);
  assert.equal(styles, 'nowrap');
  const headerWidth = await page.locator('th').last().evaluate(el => el.getBoundingClientRect().width);
  assert(headerWidth < 240, 'Long heading should wrap rather than widen its column');
  await page.evaluate(() => window.scrollTo(0, 0));
  await page.waitForTimeout(100);
  await page.evaluate(() => {
    const node = document.querySelector('.answer-text');
    node.textContent += '\nMore streamed content. '.repeat(30);
  });
  await page.waitForTimeout(150);
  assert.equal(await page.evaluate(() => window.scrollY), 0, 'Reading earlier content should not snap back');
  console.log('PASS: real chunked response renders early, auto-scroll follows growth and respects scroll-up, weekday names, compact headers, unbroken dates, mobile containment.');
} finally {
  await browser.close(); server.closeAllConnections(); await new Promise(r => server.close(r));
}
