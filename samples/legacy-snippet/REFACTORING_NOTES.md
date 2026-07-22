# S5 Legacy Modernisation Notes

## What Copilot changed
- Split the legacy single-function script into smaller pure functions:
  - `isGstRegistered`
  - `initializeStateCounts`
  - `calculateActivePercentage`
  - `calculateSummary`
  - `formatReport`
  - `buildReport`
- Added explicit TypeScript types for records, state values, and summary shape.
- Replaced mutation-heavy loops and string concatenation with clearer composition and line-based report formatting.
- Added a dedicated refactored test suite with behaviour-preservation and edge-case coverage.

## What had to be corrected manually
- Preserved legacy status handling exactly: any status other than `"Active"` must be counted as cancelled.
- Preserved legacy state handling exactly: unknown states must be ignored in the state totals.
- Preserved legacy GST semantics exactly: legacy uses loose equality (`== true`), so values like `1` and `"1"` are counted as GST-registered.
- Added Jest + TypeScript config (`jest.config.cjs`, `tsconfig.json`) and aligned TypeScript version for reliable test execution.

## Behaviour-preservation evidence
- Characterisation tests for legacy script pass.
- Refactored tests pass, including added parity edge cases.
- Direct output comparison between legacy and refactored scripts is an exact match (`OUTPUT_MATCH`).
