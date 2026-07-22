# ABN Lookup — accessible companion demo (Team 1)

An accessible **Australian Business Number (ABN) lookup** built with Blazor WebAssembly.
Enter an ABN, see clearly-presented business details, browse the sample dataset by
state, and get a plain-language summary — all designed to meet **WCAG 2.2 AA**.

> ⚠️ **Fixtures only — never calls the live ABR.** All data comes from
> [`fixtures/abn-sample-data.json`](fixtures/abn-sample-data.json). The app and its tests
> never contact `abr.business.gov.au` or any live service.

---

## Features

| Story | Feature | Where |
| --- | --- | --- |
| 1 | Look up a business by ABN (name, status, type, location, GST) | [`AbnLookup.razor`](src/Web/Components/AbnLookup.razor) |
| 2 | Clear, accessible validation errors (`role="alert"`, `aria-invalid`) | [`Abn.cs`](src/Core/Abn.cs) |
| 3 | Accepts ABNs with spaces (e.g. `51 824 753 556`) | [`Abn.cs`](src/Core/Abn.cs) |
| 4 | Cancelled/inactive status shown as **text**, not colour alone | [`AbnLookup.razor`](src/Web/Components/AbnLookup.razor) |
| 5 | Recent searches (in-memory, session only, clearable) | [`Lookup.cs`](src/Core/Lookup.cs) |
| 6 | Copy the ABN to clipboard, with an announced confirmation | [`Lookup.cs`](src/Web/Lookup.cs) |
| 7 | Browse & filter the sample dataset by state/territory | [`BusinessBrowser.razor`](src/Web/Components/BusinessBrowser.razor) |
| 8 | Helpful empty & loading states (announced, no layout shift) | [`AbnLookup.razor`](src/Web/Components/AbnLookup.razor) |
| 9 | Offline automated summary of the business | [`Summary.cs`](src/Core/Summary.cs) |
| S2 | Automated accessibility checks (axe-core, WCAG 2.2 AA) | [`a11y.spec.ts`](tests/e2e/a11y.spec.ts) |

---

## Run it

```bash
dotnet run --project src/Web       # then open the printed http://localhost:5xxx URL
```

## Test it

```bash
# Unit tests (pure logic in src/Core) — 58 tests
dotnet test

# End-to-end + accessibility checks (Playwright + axe-core) — 10 tests
npm install                        # first time only
npx playwright install chromium    # first time only
npm run test:e2e
```

The e2e run auto-starts the app via [`playwright.config.ts`](playwright.config.ts); no need
to start it yourself. The accessibility specs scan every UI state (initial, found result,
validation error, filtered browse) and assert **zero WCAG 2.2 A/AA violations**.

---

## How it works

Pure logic is separated from UI so it's fast and deterministic to unit-test:

```
src/
  Core/                     pure, unit-tested logic — no UI, no network
    Abn.cs                  ABN validation (ATO checksum), normalise, format
    Lookup.cs               lookup + browse/filter over the embedded fixture
    Summary.cs              offline plain-language business summary
  Web/                      Blazor WebAssembly UI
    Components/
      AbnLookup.razor       lookup form, results, recent searches, summary
      BusinessBrowser.razor browse + state filter
    Lookup.cs               AbnLookup code-behind (loading state, clipboard)
    App.razor               composes the page
tests/
  *Tests.cs                 xUnit unit tests
  e2e/*.spec.ts             Playwright e2e + axe-core accessibility checks
fixtures/                   static sample data (4 records, incl. one Cancelled)
```

### ABN validation

An ABN is 11 digits, validated with the ATO weighting algorithm
(`[10,1,3,5,7,9,11,13,15,17,19]`, subtract 1 from the first digit, sum, valid if divisible
by 89). Input is treated as untrusted: type/length/format are checked before the checksum,
and errors are surfaced as accessible messages rather than exceptions.

### The "AI" summary

Story 9's summary is generated **offline** from the record's own fields — deterministic,
testable, and honouring the fixtures-only rule. It is labelled *"Automatically generated
from ABR sample data"* so its provenance is clear. (A real model integration would be a
follow-up beyond the event.)

---

## Accessibility

Built to **WCAG 2.2 AA**: visible labels, real `<button>`/`<form>` elements, keyboard
operability with a visible focus indicator, status conveyed by text (not colour alone),
`aria-live`/`role="alert"` announcements, and reserved result-area height to avoid layout
shift. Verified automatically with axe-core (see the e2e checks) plus a manual keyboard
walkthrough.

- Full findings & walkthrough: [`accessibility-worksheet.md`](accessibility-worksheet.md)
- Detailed report: [`ACCESSIBILITY_REPORT.md`](ACCESSIBILITY_REPORT.md)

---

## More docs

- [`spec.md`](spec.md) — requirements spec (functional / non-functional / acceptance).
- [`user-stories.md`](user-stories.md) — the backlog these features came from.
- [`docs/README.md`](docs/README.md) — the original hackathon objectives & starting point.
