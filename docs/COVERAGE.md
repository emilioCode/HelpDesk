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
| Front end (`HelpDesk/ClientApp/src/app`) | 95.7% | 80% | 80% |

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

The 95.7% counts the lines of the code and of the templates of every file of `src/app` (1,170 lines of templates, 1,750 of code). A template is covered when a test renders it, so
the figure rises with a screen that is drawn; the figure of the code alone is 96.4%, and the branches are at 88%.

The tests are in two kinds: the pieces that do not need the server (`ui/`, `services/`, `pipes/`) and the screens, which run on a fake of the server and of the real-time connection
(`src/app/testing/screen-testing.ts`: `openScreen`, `answer`, `settle`, `fakeHub`). Write a new screen test with those.

## What these tests do not cover

The browser battery (`e2e-scripts`) exercises the whole application on a real browser; the unit tests do not replace it.

- **The handlers written in the templates.** The screen tests call the methods of a screen directly (`component.add()`), they do not press its buttons. So the line figure
  counts a template as covered when it is drawn, but the functions of the templates (the `(click)="..."` of each button) are at about 0% for the big screens (functions of the whole front end: 74.5%).
  A test that presses the button would catch a wrong binding, such as a button wired to the wrong method.
- **Files under 90% of lines:** `header.component.ts` (69%), `ticket-parts.component.html` (69%), `calendar.component.html` (75%), `ticket-parts.component.ts` (82%),
  `mirrored-form.ts` (84%), `waiting.component.html` (86%), `tracking-page.component.html` (86%), `calendar.component.ts` (87%).
- **The calendar itself.** FullCalendar needs a real layout, so its `render` is replaced in the tests; what is checked is what the screen gives it (events, colors, addresses).
