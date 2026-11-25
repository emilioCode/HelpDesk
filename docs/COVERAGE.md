# Test coverage

## The rule

A change is merged when the line coverage of the back end and of the front end is **80% or more**. The rule lives in
`.github/coverage-thresholds.json` and is checked by `scripts/coverage-gate.mjs` in two jobs of the CI: **Backend coverage** and
**Front end coverage**. Each job keeps its report as an artifact for 14 days and writes a table in the summary of the run.

Each part has a `minimum` and a `target` (80):

- `minimum` is the floor that fails the job. While a part is below the target, its floor is the coverage it has now, so a change can
  never lower it. Raise the floor in the same change that raises the coverage.
- When a part reaches the target, its minimum is set to the target (the back end is there already).

| Part | Today | Minimum | Target |
|---|---|---|---|
| Back end (`HelpDesk`, `Core`, `Infrastructure`) | 87.9% | 80% | 80% |
| Front end (`HelpDesk/ClientApp/src/app`) | 97.0% | 80% | 80% |

Both parts reached the target, so both minimums are the target. A change that lowers either under 80% fails its job.

Make the two jobs **required checks** in the branch protection of `develop` and `main`.

## Measure it yourself

```sh
# front end: report in HelpDesk/ClientApp/coverage/ (open coverage/HelpDesk/index.html)
cd HelpDesk/ClientApp && npm run coverage

# back end: report under ./coverage/backend
dotnet test HelpDesk.Tests/HelpDesk.Tests.csproj --collect:"XPlat Code Coverage" --results-directory coverage/backend

# the rule, as the CI applies it
node scripts/coverage-gate.mjs front HelpDesk/ClientApp/coverage/HelpDesk/coverage-summary.json
node scripts/coverage-gate.mjs backend coverage/backend
```

The test runner reports only the files a test loads. The gate counts every other file of the front end as untested, by its lines of
code, so untested code cannot hide. Once every file has a test that adds nothing.

## What the front end figure says

The 97.0% counts the lines of the code and of the templates of every file of `src/app`. A template is covered when a test renders it, so
the figure rises with a screen that is drawn. The functions of the whole front end are at 89.0% and the branches at 89.4%.

The tests are in two kinds: the pieces that do not need the server (`ui/`, `services/`, `pipes/`) and the screens, which run on a fake of the server and of the real-time connection
(`src/app/testing/screen-testing.ts`: `openScreen`, `answer`, `settle`, `fakeHub`). Write a new screen test with those.

## What these tests do not cover

The browser battery (`e2e-scripts`) exercises the whole application on a real browser; the unit tests do not replace it.

- **Some handlers written in the templates.** Besides the tests that call the methods of a screen (`component.add()`), the `*.clicks.spec.ts` files press the buttons and type in the
  fields the way a person does (`press` and `typeIn` of `screen-testing.ts`), so a button wired to the wrong method is caught. They cover the lists, the forms, the order window, its
  devices, parts and activities. What is left are the variants of the same buttons on the other modes of the orders screen (inbox, finished) and
  a few handlers of the calendar and the header.
- **Files under 90% of lines:** `header.component.ts` (69%), `calendar.component.html` (75%), `calendar.component.ts` (87%), `tracking-page.component.html` (86%).
- **The calendar itself.** FullCalendar needs a real layout, so its `render` is replaced in the tests; what is checked is what the screen gives it (events, colors, addresses).
