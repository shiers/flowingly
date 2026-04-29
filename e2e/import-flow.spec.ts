import { test, expect } from '@playwright/test';

const VALID_INPUT = `Hi Patricia, please create an expense claim
<expense>
  <cost_centre>DEV632</cost_centre>
  <total>35,000</total>
  <payment_method>personal card</payment_method>
</expense>
<vendor>Seaside Steakhouse</vendor>
<description>development team's project end celebration</description>
<date>27 April 2022</date>`;

const MISSING_TOTAL_INPUT = `Hi Patricia, please process this.
<expense>
  <cost_centre>DEV632</cost_centre>
</expense>`;

// ---------------------------------------------------------------------------
// Happy path — full valid input
// ---------------------------------------------------------------------------

test('parses valid input and displays JSON output with expected fields', async ({ page }) => {
  await page.goto('/');

  // Paste sample input
  await page.getByLabel('Email or text input').fill(VALID_INPUT);

  // Submit and wait for the result panel to appear
  await page.getByRole('button', { name: 'Submit' }).click();
  await expect(page.getByLabel('Parsed JSON output')).toBeVisible({ timeout: 10_000 });

  const json = await page.getByLabel('Parsed JSON output').textContent();
  const data = JSON.parse(json ?? '{}');

  // Required fields from the spec
  expect(data.costCentre).toBe('DEV632');
  expect(data.totalIncludingTax).toBe(35000);
  expect(data.totalExcludingTax).toBe(30434.78);
  expect(data.salesTax).toBe(4565.22);
});

// ---------------------------------------------------------------------------
// Validation error — missing <total>
// ---------------------------------------------------------------------------

test('displays MISSING_TOTAL error when <total> is absent', async ({ page }) => {
  await page.goto('/');

  await page.getByLabel('Email or text input').fill(MISSING_TOTAL_INPUT);
  await page.getByRole('button', { name: 'Submit' }).click();

  // Error panel should appear
  await expect(page.getByRole('alert')).toBeVisible({ timeout: 10_000 });

  const alertText = await page.getByRole('alert').textContent();
  expect(alertText).toContain('MISSING_TOTAL');

  // Result panel must not appear
  await expect(page.getByLabel('Parsed JSON output')).not.toBeVisible();
});

// ---------------------------------------------------------------------------
// Clear button resets the UI
// ---------------------------------------------------------------------------

test('clear button resets input and result', async ({ page }) => {
  await page.goto('/');

  await page.getByLabel('Email or text input').fill(VALID_INPUT);
  await page.getByRole('button', { name: 'Submit' }).click();
  await expect(page.getByLabel('Parsed JSON output')).toBeVisible({ timeout: 10_000 });

  await page.getByRole('button', { name: 'Clear' }).click();

  await expect(page.getByLabel('Email or text input')).toHaveValue('');
  await expect(page.getByLabel('Parsed JSON output')).not.toBeVisible();
});
