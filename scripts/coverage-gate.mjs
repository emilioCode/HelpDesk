#!/usr/bin/env node
// Checks the line coverage of one part of the project against the rule in .github/coverage-thresholds.json.
//   node scripts/coverage-gate.mjs front    HelpDesk/ClientApp/coverage/HelpDesk/coverage-summary.json
//   node scripts/coverage-gate.mjs backend  <folder with coverage.cobertura.xml>
//
// The rule is "target" (80): the coverage a change needs to be merged. "minimum" is the floor of the moment: while a part is below the
// target, the floor is the coverage it has now, so it can only go up. When a part reaches the target, its minimum is set to the target.
// Exit code 1 when the coverage is under the minimum.
import { readFileSync, readdirSync, statSync, appendFileSync, existsSync } from 'node:fs';
import { join, relative } from 'node:path';

const root = new URL('..', import.meta.url).pathname;
const [kind, input] = process.argv.slice(2);
const rules = JSON.parse(readFileSync(join(root, '.github/coverage-thresholds.json'), 'utf8'));
const rule = rules[kind];
if (!rule || !input) { console.error('usage: coverage-gate.mjs <front|backend> <path>'); process.exit(2); }

let covered = 0, total = 0, note = '';

if (kind === 'backend') {
  // Cobertura, as written by coverlet: the first element carries the totals.
  const dir = input;
  const find = (d) => readdirSync(d).flatMap(f => { const p = join(d, f); return statSync(p).isDirectory() ? find(p) : (f === 'coverage.cobertura.xml' ? [p] : []); });
  const file = find(dir)[0];
  if (!file) { console.error('no coverage.cobertura.xml under ' + dir); process.exit(2); }
  const head = readFileSync(file, 'utf8').match(/<coverage [^>]*>/)[0];
  covered = Number(head.match(/lines-covered="(\d+)"/)[1]);
  total = Number(head.match(/lines-valid="(\d+)"/)[1]);
} else {
  // The runner reports only the files a test loads. Every other file of the app counts as untested, by its lines of code,
  // so the figure cannot hide code nobody tests. (Once every file has a test this adds nothing.)
  const summary = JSON.parse(readFileSync(input, 'utf8'));
  const seen = new Set(Object.keys(summary).filter(k => k !== 'total').map(k => k.replace(/^.*?\/src\//, 'src/')));
  covered = summary.total.lines.covered; total = summary.total.lines.total;
  const appDir = join(root, 'HelpDesk/ClientApp/src/app');
  const walk = (d) => readdirSync(d).flatMap(f => { const p = join(d, f); if (statSync(p).isDirectory()) return walk(p); return /\.ts$/.test(f) && !/\.(spec|d)\.ts$/.test(f) && !p.includes('/interfaces/') ? [p] : []; });
  let missing = 0, files = 0;
  for (const p of walk(appDir)) {
    if (seen.has(relative(join(root, 'HelpDesk/ClientApp'), p))) continue;
    files++;
    missing += readFileSync(p, 'utf8').split('\n').filter(l => l.trim() && !/^\s*(\/\/|\*|\/\*)/.test(l)).length;
  }
  total += missing;
  note = `${files} files no test loads count as untested (${missing} lines)`;
}

const pct = total ? 100 * covered / total : 0;
const ok = pct + 1e-9 >= rule.minimum;
const reached = pct + 1e-9 >= rule.target;
const line = `${kind}: ${pct.toFixed(1)}% of lines (${covered}/${total}). Minimum ${rule.minimum}%, target ${rule.target}%. ` +
  (reached ? 'Target reached.' : ok ? `Under the target by ${(rule.target - pct).toFixed(1)} points; the floor holds.` : 'UNDER THE MINIMUM.');
console.log(line); if (note) console.log(note);
if (process.env.GITHUB_STEP_SUMMARY && existsSync(process.env.GITHUB_STEP_SUMMARY)) {
  appendFileSync(process.env.GITHUB_STEP_SUMMARY, `### Coverage: ${kind}\n\n| Lines | Covered | Minimum | Target |\n|---|---|---|---|\n| ${pct.toFixed(1)}% | ${covered} / ${total} | ${rule.minimum}% | ${rule.target}% |\n\n${note ? note + '\n\n' : ''}${ok ? (reached ? 'Target reached.' : 'The floor holds; the target is still ahead.') : '**Under the minimum.**'}\n`);
}
process.exit(ok ? 0 : 1);
