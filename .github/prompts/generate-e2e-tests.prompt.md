---
mode: agent
description: Generate, review, and strengthen Playwright end-to-end tests for web features.
---

# Generate Playwright end-to-end tests

You are helping create reliable browser-based regression tests for the web app using Playwright and TypeScript.

## Task
For the user journey, page, or workflow I point you at:

1. Identify the core user path and the key visible states (loading, success, validation, error, empty state).
2. Generate Playwright end-to-end tests that cover:
   - the happy path,
   - validation and error states,
   - edge cases such as empty or invalid input,
   - accessible behaviour such as keyboard operability and announced errors where relevant.
3. Use clear, behaviour-named tests and assert against visible user outcomes rather than implementation details.
4. After generating, review your own tests and list:
   - any behaviour still untested,
   - any test that is too brittle and how to strengthen it.

## Rules
- Do not call live sites, external services, or network-dependent endpoints.
- Reuse the existing Playwright configuration and helpers in the relevant test folder.
- Prefer locators based on visible text, labels, roles, and accessible names rather than brittle CSS selectors.
- Keep tests deterministic and resilient; avoid arbitrary timeouts and flakiness.
- If the flow has accessibility or keyboard issues, call them out and suggest the accessible fix.
- Output the test code plus a short “coverage gaps” list at the end.
