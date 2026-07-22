# Team 1 ABN Lookup — Technical Documentation

## Overview

This implementation is a Blazor WebAssembly demo that provides an accessible ABN lookup experience using local fixture data. The app is intentionally lightweight and does not call the live Australian Business Register.

## Architecture

The implementation is organised into two main layers:

- Core layer: pure logic for validation, lookup, and recent-search handling
- Web layer: Blazor UI components and app shell

### Project structure

- src/Core/Abn.cs
  - contains ABN validation logic using the ATO checksum algorithm
  - normalises whitespace and validates user input before lookup
- src/Core/Lookup.cs
  - loads fixture data from embedded resources
  - performs local lookup operations
  - manages recent-search state helpers
- src/Web/Components/AbnLookup.razor
  - renders the ABN form, results area, recent searches, and copy action
  - uses accessible markup and ARIA patterns
- src/Web/App.razor
  - hosts the page header and composes the main feature views
- src/Web/wwwroot/styles.css
  - provides layout and visual styling, including focus visibility and result states
- tests/AbnTests.cs
  - covers validation, lookup, and recent-search behaviour

## Validation flow

The core validation path is:

1. The user enters an ABN.
2. The input is passed to Abn.Validate.
3. The validator normalises whitespace and performs boundary checks.
4. If the ABN is invalid, the UI receives a structured error reason.
5. If the ABN is valid, it is normalised and used for fixture lookup.

### Validation rules

The validation logic checks:
- empty input
- non-text input
- non-digit characters
- incorrect length
- checksum failure

## Lookup flow

Once validation passes:

1. the normalised ABN is checked against local fixture data
2. a found result, not-found state, or validation error is returned
3. the UI updates the visible results area and recent-search list

## UI behaviour

The main component handles:
- form submission
- loading state while a search is in progress
- empty-state guidance before any search
- result rendering for found and not-found states
- copy-to-clipboard feedback
- recent-search interactions

## Accessibility considerations

The UI is built with accessibility in mind:
- visible label for the input field
- keyboard operable buttons and form controls
- error announcements using role="alert"
- status updates using live regions
- visible focus styling
- text-based status communication instead of relying on colour alone

## Data source

The app uses sample records from the fixtures folder rather than any remote API. This keeps the experience deterministic and safe for demos and automated tests.

## Testing approach

The project uses xUnit for core behaviour tests. The test suite covers:
- valid ABN acceptance
- invalid ABN rejection
- whitespace handling
- lookup success and not-found behaviour
- recent-search list behaviour

## Running locally

From the team project folder, run:

```bash
dotnet test
dotnet run --project src/Web
```

## Notes for future changes

When extending the implementation, keep the following principles in place:
- keep business rules in the Core layer
- keep UI components thin and accessible
- validate input at the boundary
- prefer local fixture data for demos and tests
