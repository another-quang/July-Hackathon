import { test, expect } from '@playwright/test';

test.describe('ABN lookup', () => {
  test('finds a business for a valid ABN in the sample data', async ({ page }) => {
    await page.goto('/');

    await page.getByLabel(/australian business number/i).fill('51824753556');
    await page.getByRole('button', { name: /search/i }).click();

    // The result card should show the matching entity name.
    await expect(
      page.getByRole('heading', { name: /australian taxation office/i }),
    ).toBeVisible();
  });

  test('displays the matching business details for a valid ABN', async ({ page }) => {
    await page.goto('/');

    await page.getByLabel(/australian business number/i).fill('83914571673');
    await page.getByRole('button', { name: /search/i }).click();

    await expect(
      page.getByRole('heading', { name: /example manufacturing pty ltd/i }),
    ).toBeVisible();
    await expect(page.getByText(/active/i)).toBeVisible();
    await expect(page.getByText(/vic 3000/i)).toBeVisible();
  });

  test('shows an accessible error for an invalid ABN', async ({ page }) => {
    await page.goto('/');

    await page.getByLabel(/australian business number/i).fill('123');
    await page.getByRole('button', { name: /search/i }).click();

    // The error is announced via role="alert".
    await expect(page.getByRole('alert')).toContainText(/11 digits/i);
  });

  test('form controls are accessible and keyboard-operable (S2)', async ({
    page,
  }) => {
    await page.goto('/');

    // The input is reached by its visible <label> (not placeholder-only).
    const field = page.getByLabel(/australian business number/i);
    await expect(field).toBeVisible();

    // A real <button> exposes an accessible name via its role.
    await expect(page.getByRole('button', { name: /search/i })).toBeVisible();

    // Keyboard-only: focus the field, type, and submit with Enter — no mouse.
    await field.focus();
    await expect(field).toBeFocused();
    await field.fill('51824753556');
    await field.press('Enter');

    await expect(
      page.getByRole('heading', { name: /australian taxation office/i }),
    ).toBeVisible();
  });

  // TODO (C5): with Copilot, add a regression test for the "not found" path and
  // a keyboard-only navigation check (Tab to the field, type, Enter to submit).
});
