# Team 1 ABN lookup — requirements spec

## Summary

Provide a simple, accessible way for users to look up an ABN, understand the result clearly, and revisit recent searches during the same session without relying on a live service.

## Functional requirements

1. The service must allow a user to enter an ABN and submit it for lookup.
2. The system must validate the ABN before lookup and show a clear message when the input is invalid.
3. The system must accept ABNs with spaces and treat them the same as the unspaced version.
4. When a valid ABN matches a record in the sample data, the system must show the business name, ABN status, entity type, location, and GST registration status.
5. When a valid ABN does not match a record in the sample data, the system must show a clear “no business found” message.
6. The system must display recent ABN searches for the current session and allow the user to clear them.
7. The system must provide a button to copy a displayed ABN to the clipboard and confirm that the action completed.
8. The interface must expose the ABN field and interactive controls with accessible names so they can be used by keyboard and screen-reader users.

## Non-functional requirements

### Accessibility
- The ABN input must have a visible label and an associated accessible name.
- The form and all interactive controls must be operable by keyboard.
- Validation errors and result updates must be announced to assistive technology using appropriate live/alert patterns.
- Status information must be conveyed using text, not colour alone.
- Focus states must remain visible for keyboard users.

### Security
- Input must be validated at the boundary before any lookup occurs.
- The app must use only the provided local sample data and must not persist user searches to storage.
- User input must be rendered safely and must not be inserted into the UI through unsafe HTML construction.

### Performance / reliability
- The lookup experience must work quickly and predictably using the local fixture data.
- The interface must remain usable even when the input is invalid or when no result is found.

## Acceptance criteria

1. Given a valid ABN that exists in the sample data, when the user searches, then the matching business details are shown.
2. Given a valid ABN that does not exist in the sample data, when the user searches, then a clear no-business-found message is shown.
3. Given an ABN with spaces between digit groups, when the user searches, then it is treated the same as the unspaced version.
4. Given an ABN that contains non-digit characters, when the user searches, then a helpful validation error is shown.
5. Given an ABN that is not exactly 11 digits, when the user searches, then a specific validation error is shown.
6. Given an ABN that fails the checksum, when the user searches, then a checksum-related validation error is shown.
7. Given a cancelled ABN result, when the user views the result, then the status is shown as text and is easy to identify.
8. Given the user has searched at least once, when they look below the form, then they see their recent searches for the current session.
9. Given the user has searched at least once, when they choose to clear recent searches, then the recent-search list is removed.
10. Given a result is shown, when the user activates the copy button, then the ABN is copied and the user receives an accessible confirmation.
11. Given the user is using the page with a keyboard only, when they interact with the input and controls, then they can complete the flow without a mouse.

## Out of scope

- Filtering results by state or territory.
- Empty and loading-state enhancements beyond basic guidance.
- AI-generated summaries of the business.
- Any integration with the live Australian Business Register.

## Open questions

- Should the recent-search list be limited to a specific number of entries, and if so, what should that number be?
- Should the copy action be available only for successful results, or also for failed/not-found states?
- Do we need a formal wording for the “no business found” message that should be used consistently across the experience?
- Should the app support additional ABN formatting such as punctuation beyond spaces in a future phase?
