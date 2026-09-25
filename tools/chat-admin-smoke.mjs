// Read-only browser check against existing local chat logs; makes no model calls.
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const browser = await chromium.launch({ channel: 'chrome', headless: true });
try {
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));
  await page.goto(`${process.env.CHAT_UI_URL || 'http://localhost:5173'}/admin/chats`);
  await page.getByLabel('Admin password').fill('incorrect-test-password');
  await page.getByRole('button', { name: 'Unlock', exact: true }).click();
  await page.getByRole('alert').filter({ hasText: 'Incorrect admin password.' }).waitFor();
  assert.equal(await page.locator('.chat-item').count(), 0);
  assert(process.env.CHAT_ADMIN_PASSWORD, 'Set CHAT_ADMIN_PASSWORD for admin smoke test');
  await page.getByLabel('Admin password').fill(process.env.CHAT_ADMIN_PASSWORD);
  await page.getByRole('button', { name: 'Unlock', exact: true }).click();
  await page.locator('.chat-item').first().waitFor();
  await page.locator('.chat-item').first().click();
  await page.getByRole('heading', { name: 'Chat details', exact: true }).waitFor();
  await page.getByRole('heading', { name: 'Conversation sent', exact: true }).waitFor();
  await page.getByRole('heading', { name: 'Model calls', exact: true }).waitFor();
  await page.locator('summary').filter({ hasText: /^Call 1/ }).click();
  await page.getByText('Reported cost:', { exact: false }).first().waitFor();
  await page.getByLabel('Search questions and answers').fill('no-such-chat-admin-smoke-9e35c3');
  await page.getByRole('button', { name: 'Refresh', exact: true }).click();
  await page.getByText('No saved chats match.', { exact: false }).waitFor();
  await page.getByLabel('Search questions and answers').fill('');
  await page.getByRole('button', { name: 'Refresh', exact: true }).click();
  await page.locator('.chat-item').first().waitFor();
  await page.setViewportSize({ width: 390, height: 844 });
  assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
  await page.getByRole('button', { name: 'Lock admin', exact: true }).click();
  await page.getByLabel('Admin password').waitFor();
  assert.equal(await page.locator('.chat-item').count(), 0);
  assert.deepEqual(errors, []);
  console.log('PASS: saved chats list, detail, model usage, search/empty state and mobile layout.');
} finally { await browser.close(); }
