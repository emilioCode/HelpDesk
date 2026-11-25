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
| Front end (`HelpDesk/ClientApp/src/app`) | 16.6% | 16% | 80% |

The front end cannot take the 80% as a floor yet, because every change would fail until it is there. Make the two jobs **required
checks** in the branch protection of `develop` and `main` once they pass.

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

## Getting the front end to 80%

About 2,300 of the 2,886 lines have to be covered; 480 are. The lines that count most, by file:

| Lines | File |
|---|---|
| 511 | `components/shared/content/waiting/waiting.component.ts` (orders) |
| 243 | `.../content/user/user.component.ts` |
| 184 | `.../content/costumer/costumer.component.ts` |
| 183 | `.../content/empresa/empresa.component.ts` |
| 141 | `.../content/profile/profile.component.ts` |
| 133 | `.../content/calendar/calendar.component.ts` |
| 101 | `services/session.service.ts` |
| 95 | `.../content/report/report.component.ts` |

Raise the floor in `.github/coverage-thresholds.json` as each of these gets its tests.
