# Team 2 Grant Finder — requirements spec

## Summary

Provide an accessible way for users to narrow grant results to the ones they are eligible for, compare them by size, and keep their profile available for a short session without storing personal or business data.

## Functional requirements

1. The application must allow a user to enter a business profile and submit it for grant eligibility checking.
2. The application must present grants in a clear order and identify which grants are eligible or ineligible.
3. The application must provide a toggle to show only eligible grants.
4. The application must allow the user to sort grants by dollar amount in descending order.
5. The application must keep the submitted profile in memory for the current session so the user can adjust one field and resubmit without re-entering everything.
6. The interface must expose form controls and result controls with accessible names so they can be used by keyboard and screen-reader users.

## Non-functional requirements

### Accessibility
- Every form field and interactive control must have a visible label and an accessible name.
- The eligible-only toggle, sort control, and result updates must be operable by keyboard.
- Changes to results, counts, and errors must be announced to assistive technology using appropriate live or alert patterns.
- Eligible and ineligible states must be conveyed using text, not colour alone.
- Focus states must remain visible for keyboard users.

### Security
- Input must be validated at the boundary before any eligibility check is performed.
- The application must not persist personal or business information to browser storage.
- User-entered values must be rendered safely and must not be inserted into the UI through unsafe HTML construction.
- The application must not include secrets or credentials in code.

### Performance / reliability
- The experience must respond quickly using the local sample data and remain predictable when filters or sorting change.
- The interface must remain usable when there are no matching results or when the user changes input.

## Acceptance criteria

1. Given results are shown, when the user turns on the eligible-only toggle, then ineligible grants are hidden and the visible result count updates.
2. Given results are shown, when the user chooses to sort by amount, then grants are reordered from highest to lowest value.
3. Given the user has submitted a profile once, when they change one field and submit again, then the form retains the latest values and the results update accordingly.
4. Given a grant is ineligible, when the user views that grant, then the reason it does not match is shown clearly.
5. Given the user is using the page with a keyboard only, when they operate the form controls and result filters, then they can complete the flow without a mouse.
6. Given invalid input is submitted, when the form is checked, then a specific validation message is shown and announced to assistive technology.

## Out of scope

- Linking out to live grant detail pages.
- Saving the profile beyond the current session.
- Additional filtering beyond eligible-only and amount sorting.
- AI-generated explanations or external integrations.

## Open questions

- Should the eligible-only toggle default to on or off when the page first loads?
- What should the experience show when no grants match the current filter?
- Should the default sort order be by amount or preserve the original grant order?
- Do we need a preferred wording for the grant-matching explanations used in the UI?
