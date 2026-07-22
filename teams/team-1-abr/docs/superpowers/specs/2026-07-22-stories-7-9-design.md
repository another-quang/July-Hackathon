# Design — User Stories 7–9 (ABN Lookup)

**Date:** 2026-07-22
**Team:** team-1-abr
**Scope:** Stories 7 (filter by state), 8 (empty & loading states), 9 (AI-generated summary)

## Context & constraints

The app is a **standalone Blazor WebAssembly** SPA — no backend, no server. All
business data comes from an embedded fixture (`fixtures/abn-sample-data.json`, four
records). Ground rules: **never call the live ABR**, use fixtures only, keep tests
**deterministic and offline**.

Architecture principle carried through all three stories: **all data/logic lives in
`Team1Abr.Core` as pure, unit-testable functions**; UI lives in `Team1Abr.Web`
components. New browse functionality is its own component for isolation.

---

## Story 9 — Automated business summary (offline)

**Decision:** generate the summary with a pure C# template function, not a live model.
A real API call from WASM would expose a key in the browser, add network dependency,
and make output non-deterministic (untestable). The offline generator honours
fixtures-only and is fully unit-testable. It is labelled honestly as automated.

### Core
New pure class `src/Core/Summary.cs`:

```csharp
namespace Team1Abr.Core;

public static class Summary
{
    /// <summary>Compose a short natural-language summary from a business record.</summary>
    public static string Generate(BusinessRecord record);
}
```

Behaviour:
- Sentence 1 (always): `"{EntityName} is a {EntityType} based in {State} (postcode {Postcode})."`
- Sentence 2, **Active**: `"Its ABN is currently active"` + GST clause.
- Sentence 2, **Cancelled** (leads with caution): `"Its ABN has been cancelled, so it
  may no longer be trading"` + GST clause.
- GST clause: `" and it is registered for GST."` / `". It is not registered for GST."`

### UI (`AbnLookup.razor`)
Inside the existing `aria-live="polite"` results region, within the result card, add:

```html
<div class="summary">
  <h4>Summary</h4>
  <p>@Summary.Generate(_record)</p>
  <p class="summary__note">Automatically generated from ABR sample data.</p>
</div>
```

Being inside the live region means screen readers hear it with the rest of the result.
The note keeps the claim honest (no implication of a live model or human authorship).

### Tests (`tests/AbnTests.cs`)
- Active record → text contains "active" and "registered for GST".
- Cancelled record (`40000000026`) → text contains "cancelled" and the trading caution;
  and "not registered for GST".
- All four fixtures → non-empty text that contains the entity name.

---

## Story 7 — Browse & filter by state

**Decision:** add a browse-all list view for the filter to act on (the app currently
only does single-ABN lookup, so there is no list to filter otherwise).

### Core (`src/Core/Lookup.cs` additions)
```csharp
public static IReadOnlyList<BusinessRecord> AllRecords();
public static IReadOnlyList<BusinessRecord> FilterByState(string? state);
public static IReadOnlyList<string> DistinctStates();
```
- `FilterByState`: null / empty / "All" → all records; otherwise case-insensitive
  match on `State`.
- `DistinctStates`: sorted, unique state codes present in the data (for the dropdown).

### UI — new component `src/Web/Components/BusinessBrowser.razor`
Self-contained (inline `@code`), composed in `App.razor` below `<AbnLookup />`.

- A visible `<label for="state-filter">Filter by state or territory</label>` and a
  native `<select id="state-filter">` populated with `All` + `DistinctStates()`.
  Native `<select>` → keyboard-operable by default (2.1.1), visible label (1.3.1/3.3.2).
- A filtered `<ul>` of records; each item shows **name, entity type, location, status**.
  Status is rendered as **text, not colour alone** (WCAG 1.4.1), reusing the
  `status status--active` / `status--cancelled` pattern from the result card.
- An `aria-live="polite"` region announcing the result count on change, e.g.
  "Showing 1 business in VIC." / "Showing 4 businesses." (singular/plural handled).

### Tests (`tests/AbnTests.cs`)
- `FilterByState("VIC")` → exactly the VIC record(s).
- `FilterByState(null)` and `FilterByState("All")` → all four records.
- `FilterByState` is case-insensitive (e.g. `"vic"` matches).
- `DistinctStates()` → sorted unique set `{ACT, NSW, QLD, VIC}`.

---

## Story 8 — Empty & loading states

Fixture lookups are instant (in-memory), so a brief simulated delay makes a real,
accessibly-announced loading state demonstrable without shifting layout.

### UI (`AbnLookup.razor` + `src/Web/Lookup.cs` code-behind)
- **Empty / pre-search:** when `_state == LookupStatus.Idle`, the results region shows
  brief guidance: *"Enter an ABN above and select Search to see business details. For
  example, try 51 824 753 556."*
- **Loading:** `HandleSubmit` and `HandleRecentSearch` become `async Task`. Set
  `_isLoading = true`, `await Task.Delay(300)`, compute the result, then
  `_isLoading = false` (with `StateHasChanged` as needed around the await). While
  loading, render `<p aria-busy="true">Searching…</p>` in a **fixed-height container so
  layout does not shift**, and set the Search button `disabled`.
- **No-results:** already announced via the `aria-live="polite"` region — kept as-is.

### Tests
Loading/empty are UI-only; no new Core logic. Existing `SearchAbn` tests cover the
lookup logic, and the C5 Playwright e2e will verify the loading/empty UX. No new unit
tests required for this story.

---

## Files

**Create**
- `src/Core/Summary.cs`
- `src/Web/Components/BusinessBrowser.razor`

**Modify**
- `src/Core/Lookup.cs` — add `AllRecords`, `FilterByState`, `DistinctStates`
- `src/Web/Components/AbnLookup.razor` — summary block, empty state, loading indicator
- `src/Web/Lookup.cs` — async loading state in `HandleSubmit` / `HandleRecentSearch`
- `src/Web/App.razor` — compose `<BusinessBrowser />`
- `src/Web/wwwroot/styles.css` — styles for summary, browser list, loading indicator
- `tests/AbnTests.cs` — Summary + filter unit tests

## Accessibility summary (WCAG 2.2 AA)
- All new controls have visible labels; native `<select>`/`<button>` for full keyboard use.
- Status conveyed by text, not colour alone (1.4.1).
- Dynamic content (summary, filter count, loading, results) in `aria-live` regions (4.1.3).
- Loading indicator uses reserved space to avoid layout shift.

## Out of scope
- Real AI model integration (deferred; offline generator used).
- Persisting browse/filter state or searches (session/in-memory only).
- Any network calls to the live ABR.
