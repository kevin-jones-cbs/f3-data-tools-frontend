// Run against the local Development frontend (API localhost:5055). All API responses are mocked.
// Uses installed Chrome; set PLAYWRIGHT_MODULE if Playwright is installed outside this repo.
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { gzipSync } from 'node:zlib';
const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const browser = await chromium.launch({ channel: 'chrome', headless: true });
const pack = data => {
  const raw = Buffer.from(JSON.stringify(data));
  const length = Buffer.alloc(4); length.writeInt32LE(raw.length);
  return Buffer.concat([length, gzipSync(raw)]).toString('base64');
};
const row = (name, count) => ({ paxName: name, totalPostCount: count, totalQCount: 1, firstPost: '2026-01-01T00:00:00', paxRegionData: { 'South Fork': { paxName: name, postCount: count, qCount: 1, firstPost: '2026-01-01T00:00:00' } } });
const all = { paxSectorData: Array.from({ length: 60 }, (_, i) => row(`Pax${String(i).padStart(2, '0')}`, 100)), totalPosts: 6000, totalPax: 60, activeLocations: 10, totalPax30Days: 12 };
const year = { ...all, paxSectorData: [row('Pax00', 5)], totalPosts: 5, totalPax: 1 };
let fail = false, empty = false;
const requests = [];
try {
  const page = await browser.newPage();
  const errors = []; page.on('pageerror', error => errors.push(error.message));
  await page.route('http://localhost:5055/**', async route => {
    const req = route.request();
    if (req.method() === 'OPTIONS') return route.fulfill({ status: 204 });
    const input = req.postDataJSON();
    if (input.Action !== 'GetSectorDataSummaryAsync') return route.fulfill({ json: [] });
    requests.push(input);
    await new Promise(resolve => setTimeout(resolve, 200));
    if (fail) return route.fulfill({ status: 500, body: 'failure' });
    const data = input.ThisYear ? (empty ? { ...year, paxSectorData: [], totalPosts: 0, totalPax: 0 } : year) : all;
    return route.fulfill({ body: pack(data) });
  });
  await page.goto(`${process.env.SECTOR_UI_URL || 'http://localhost:5187'}/sectordata/sacramento`);
  const allButton = page.getByRole('button', { name: 'All Time', exact: true });
  const yearButton = page.getByRole('button', { name: 'This Year', exact: true });
  await page.getByText('Total PAX All-Time', { exact: true }).waitFor();
  assert.equal(await allButton.getAttribute('aria-pressed'), 'true');
  assert.equal(requests[0].ThisYear, false);
  // Navigate away from the first page before switching to a smaller result set.
  const second = page.locator('a.page-link').filter({ hasText: /^2$/ });
  await second.click();
  await page.getByText('Pax59', { exact: true }).waitFor();
  await yearButton.click();
  assert.equal(await yearButton.isDisabled(), true);
  await page.getByText('Total PAX This Year', { exact: true }).waitFor();
  await page.getByText('Pax00', { exact: true }).waitFor();
  assert.equal(requests.at(-1).ThisYear, true);
  assert.equal(await yearButton.getAttribute('aria-pressed'), 'true');
  await page.getByText('Pax00', { exact: true }).click();
  await page.getByText('South Fork: 5', { exact: true }).waitFor();
  await allButton.click();
  await page.getByText('Total PAX All-Time', { exact: true }).waitFor();
  fail = true;
  await yearButton.click();
  await page.getByRole('alert').waitFor();
  assert.equal(await allButton.getAttribute('aria-pressed'), 'true');
  fail = false; empty = true;
  await yearButton.click();
  await page.getByText('Total PAX This Year', { exact: true }).waitFor();
  assert.equal(await page.getByText('Pax00', { exact: true }).count(), 0);
  assert.equal(await page.getByRole('alert').count(), 0);
  assert.deepEqual(errors, []);
  console.log('PASS: all-time default, request flags, loading, period toggle, region detail, error recovery, empty year.');
} finally { await browser.close(); }
