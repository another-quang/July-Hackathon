[← Back to home](../../index.html)

# Accessibility findings worksheet — Team 1

Use this to record WCAG 2.2 AA findings. See the
[WCAG primer](../../docs/accessibility-wcag-primer.md) for the "top 10" checks and success
criteria references. This supports stretch goal **S2**.

Analyse three things:
1. The provided fixture [`fixtures/abr-sample-results.html`](fixtures/abr-sample-results.html)
   (it has planted issues).
2. Your own app (run `npm run test:e2e` for the Playwright accessibility checks + do a keyboard walkthrough).
3. *(Optional, observation only)* the live ABR in a browser — do **not** automate against it.

---

## Findings

| # | Where (fixture / our app / live ABR) | Issue | WCAG SC | Severity | Suggested fix | Status |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | fixture | Search field labelled by placeholder only | 1.3.1 / 3.3.2 | Serious | Add a real `<label for="…">` | ☑ open ☐ fixed |
| 2 | fixture | "Search" is a `<div onclick>` — not keyboard operable | 2.1.1 / 4.1.2 | Blocker | Use a `<button>` | ☑ open ☐ fixed |
| 3 | fixture | Low-contrast helper text (`#aaaaaa` on white ≈ 2.3:1) | 1.4.3 | Serious | Darken to ≥4.5:1 (e.g. `#595959` / our `#444`) | ☑ open ☐ fixed |
| 4 | fixture | Status shown by colour alone (green/red dot, `title` only) | 1.4.1 / 1.3.1 | Serious | Add visible text ("Active"/"Cancelled") beside the dot | ☑ open ☐ fixed |
| 5 | fixture | Result count `<p id="result-count">` updates with no live region | 4.1.3 | Serious | Wrap the count in `aria-live="polite"` | ☑ open ☐ fixed |
| 6 | fixture | Heading levels skip from `<h1>` to `<h3>` | 1.3.1 / 2.4.6 | Minor | Use `<h2>` for "Results" (no skipped level) | ☑ open ☐ fixed |
| 7 | fixture | Decorative `<img src="divider.png">` has no `alt` | 1.1.1 | Minor | Add `alt=""` so it's ignored by screen readers | ☑ open ☐ fixed |
| 8 | our app | Enhancement: focus is not moved to the result after Search; result is announced via `aria-live` but a keyboard user's focus stays on the button. Not an AA failure, but moving focus (or a focusable results heading) would improve flow | 2.4.3 (advisory) | Minor | Optionally set focus to the result card heading after a successful search | ☐ open ☑ noted |

> **Fixture (rows 1–7):** all 7 planted issues found. Left as `open` — the fixture is
> practice markup we analyse, not code we ship. Our own app implements the accessible
> counterpart for each (see below).
>
> **Our app (row 8):** axe-core found **0 WCAG 2.2 A/AA violations** across all states
> (see the automated check). Row 8 is an advisory enhancement, not a violation.

### How our app already avoids the fixture's issues

| Fixture issue | How our app does it right |
| --- | --- |
| 1 placeholder-only label | Visible `<label for="abn-input">` + hint via `aria-describedby` |
| 2 `<div onclick>` | Real `<form>` + `<button type="submit">` — keyboard operable |
| 3 low-contrast hint | Hint text uses `#444` (≥4.5:1 on white) |
| 4 colour-only status | Status shown as **text** ("Active"/"Cancelled"), colour is secondary |
| 5 silent count update | Results and the browse count sit in `aria-live="polite"` regions |
| 6 skipped heading | `h1` → `h2` (Look up / Browse) → `h3` (result) in order |
| 7 missing alt | No decorative `<img>`; any future image would need `alt` |

---

## Keyboard walkthrough checklist (our app)

- [x] I can Tab to the ABN field. *(first focusable element; covered by the keyboard e2e test)*
- [x] I can type and submit with Enter (no mouse). *(real `<form>`/`<button>`; e2e presses Enter)*
- [x] The focused element always has a **visible** focus indicator. *(`:focus-visible` outline in styles.css)*
- [x] After submitting, the result/error is announced. *(result in `aria-live="polite"`, error via `role="alert"`)*
- [x] I can Tab through results without getting trapped. *(native controls only — no custom widgets/traps)*

## Automated check (Playwright)

- [x] `npm run test:e2e` passes, including the accessibility assertions. *(**10/10 passed**; 4 axe-core WCAG 2.2 A/AA scans, 0 violations — see `tests/e2e/a11y.spec.ts`)*
- [x] If any check fails, record the issue above and fix it. *(none failed)*

**axe-core scan coverage (states scanned, all clean):** initial page (pre-search + browse list),
found result (result card + generated summary), validation-error state, filtered browse list.

## Reflection (for the show-and-tell)

- Which issue did **Copilot / axe** help you find or fix? _axe-core confirms the machine-checkable
  criteria (labels, contrast, roles/names) are clean across every state — fast regression safety net._
- Which issue needed a **human** (not caught by the automated checks)? _Focus-order flow after
  search (row 8) and "is the announcement actually meaningful" — axe can't judge these; they need a
  keyboard + screen-reader walkthrough._
