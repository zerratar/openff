// The effect runtime's shared cases, played by Crystal's JS player (Crystal.Editor/wwwroot/effects.js).
//
//   node Tools/effect_cases.mjs            check every Tools/EffectCases/*.case.json against its expect
//   node Tools/effect_cases.mjs --write    write each case's expect from what the JS player draws
//
// A case is an effect, a seed and frames; its expect, per frame, the quads drawn then:
// [x, y, z, half width, half height, r, g, b, a, cell], rounded to 1e-6. The client's player
// (Shared/Effects/EffectPlayer.cs) is checked against the same expects by `crystal effect-cases`.

import { readFileSync, writeFileSync, readdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import vm from 'node:vm';

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, '..');
const context = vm.createContext({ console, Math });
vm.runInContext(readFileSync(join(root, 'Crystal.Editor', 'wwwroot', 'effects.js'), 'utf8') + '\n;globalThis.makeEffectPlayer = makeEffectPlayer;', context);
const makeEffectPlayer = context.makeEffectPlayer;

const round = v => Math.round(v * 1e6) / 1e6;
const write = process.argv.includes('--write');
const dir = join(here, 'EffectCases');
let failed = 0, checked = 0;
for (const file of readdirSync(dir).filter(f => f.endsWith('.case.json')).sort()) {
  const path = join(dir, file);
  const c = JSON.parse(readFileSync(path, 'utf8'));
  const player = makeEffectPlayer(c.effect, { seed: c.seed || 1 });
  const want = new Set(c.frames);
  const got = {};
  const last = Math.max(...c.frames);
  for (let f = 0; f <= last; f++) {
    player.step();
    if (want.has(f)) got[f] = player.particles().map(q => [q.pos[0], q.pos[1], q.pos[2], q.w, q.h, ...q.colour, q.cell].map(round));
  }
  if (write) {
    c.expect = got;
    writeFileSync(path, JSON.stringify(c, null, 1).replace(/\[\s+([-\d.,e\s]+?)\s+\]/g, (m, inner) => '[' + inner.replace(/\s+/g, ' ').trim() + ']') + '\n');
    console.log(`${file}: written, ${Object.values(got).reduce((n, q) => n + q.length, 0)} quad(s)`);
    continue;
  }
  const problems = [];
  for (const f of c.frames) {
    const a = (c.expect || {})[f] || [], b = got[f] || [];
    if (a.length !== b.length) { problems.push(`frame ${f}: ${b.length} quad(s), expected ${a.length}`); continue; }
    for (let i = 0; i < a.length; i++)
      for (let j = 0; j < a[i].length; j++)
        if (Math.abs(a[i][j] - b[i][j]) > 1e-4) { problems.push(`frame ${f} quad ${i} value ${j}: ${b[i][j]}, expected ${a[i][j]}`); break; }
  }
  checked++;
  if (problems.length) { failed++; console.log(`${file}: FAILED\n  ` + problems.slice(0, 8).join('\n  ')); }
  else console.log(`${file}: ok`);
}
if (!write) { console.log(`${checked - failed}/${checked} case(s) pass`); process.exit(failed ? 1 : 0); }
