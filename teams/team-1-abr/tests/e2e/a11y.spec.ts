import { test, expect } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";

// Automated accessibility checks (stretch goal S2).
//
// These run axe-core against our own app in each of its meaningful states and
// assert there are zero WCAG 2.2 A/AA violations. axe catches a large class of
// issues automatically (labels, contrast, roles, names) — but NOT everything
// (e.g. logical focus order, meaningful announcements). Those are covered by the
// keyboard tests in abn-lookup.spec.ts and the manual walkthrough in the
// accessibility worksheet.
const WCAG_22_AA = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22aa"];

async function scan(page: import("@playwright/test").Page) {
  return new AxeBuilder({ page }).withTags(WCAG_22_AA).analyze();
}

test.describe("accessibility (WCAG 2.2 AA) — our app", () => {
  test("initial page (pre-search instructions, browse list)", async ({
    page,
  }) => {
    await page.goto("/");
    await page.getByLabel(/australian business number/i).waitFor();

    const results = await scan(page);
    expect(results.violations).toEqual([]);
  });

  test("with a found result (result card + summary)", async ({ page }) => {
    await page.goto("/");
    await page.getByLabel(/australian business number/i).fill("51824753556");
    await page.getByRole("button", { name: /search/i }).click();
    await page.getByRole("article", { name: /search result/i }).waitFor();

    const results = await scan(page);
    expect(results.violations).toEqual([]);
  });

  test("with a validation error shown", async ({ page }) => {
    await page.goto("/");
    await page.getByLabel(/australian business number/i).fill("123");
    await page.getByRole("button", { name: /search/i }).click();
    await page.getByRole("alert").waitFor();

    const results = await scan(page);
    expect(results.violations).toEqual([]);
  });

  test("with a filtered browse list", async ({ page }) => {
    await page.goto("/");
    await page.getByLabel(/filter by state or territory/i).selectOption("VIC");

    const results = await scan(page);
    expect(results.violations).toEqual([]);
  });
});
