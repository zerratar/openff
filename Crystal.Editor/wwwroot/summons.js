// Summons: FF3's summon scripts played on the effect Stage, step for step as the battle plays them.
//
// A summon is a script of the game's (Crystal.Editor/Editor/Summons.cs reads files/summon_script_command.pack): one
// for each of the eight summons' three outcomes, a record a step - a command and its parameters. The battle steps
// through them once a game step (30 a second): btl.BaseSummon.run runs a record, and while it says it is done moves
// to the next, which runs in the same step when it is marked `again`; a record that is not done yet (a wait, a
// camera still gliding) runs again the next step. btl.SummonCommand does each command; this is a port of the two,
// the shared frame counter and the camera's whole-unit glide included, so what plays here is what the battle
// plays: the screen darkened and faded, the summon's model coming in and moving, the camera's cuts, glides and
// shakes, its effects at the summon, over a side or on each target, and the party coming back.
//
// The effects are the game's, imported into the effect format (/api/effect/import) and played by effects.js's
// player; positions are the battle's (fixed point / 4096), the Stage's own units.

'use strict';

const SUMMON_BATTLE_EYE = [537084, 142272, 191833], SUMMON_BATTLE_AT = [478028, 131072, 165657];
const SUMMON_OUTCOMES = ['white', 'black', 'combine'];

/// The Summons page: the eight, each with its three outcomes.
async function summonsForList() {
  const r = await api('/api/summons').catch(() => null);
  state.summons = r && r.ok ? r.summons : [];
  return state.summons.map(s => ({
    name: 'summon/' + s.level, overridden: false, def: { name: s.creature ? `${s.creature} (${s.name})` : s.name },
    note: s.outcomes.map(o => o.name).join(' · ')
  }));
}

/// One step as words: what the command does with its numbers.
function summonStepText(st) {
  const [p1, p2, p3, p4, p5, p6] = st.p;
  const at = (x, y, z) => `${(x / 4096).toFixed(1)}, ${(y / 4096).toFixed(1)}, ${(z / 4096).toFixed(1)}`;
  switch (st.op) {
    case 0: return `darken the map to (${p2}, ${p3}, ${p4}) over ${p1} frames`;
    case 2: return `the map back to normal over ${p1} frames`;
    case 4: return `fade out to ${p1 ? 'white' : 'black'} over ${p2} frames`;
    case 6: return `fade in over ${p1} frames`;
    case 8: return `load the summon's model f${String(p1).padStart(3, '0')}`;
    case 10: return `load its motions b_sm${String(p1).padStart(3, '0')}`;
    case 12: return `play motion ${p1}${p2 ? ', looping' : ''}`;
    case 13: return `wait for its motion's frame ${p1}`;
    case 16: return `load effect pack e${p1}`;
    case 19: return `effect ${p1}/${p2} at the summon`;
    case 20: return `effect ${p1}/${p2} over the ${p3 ? 'party' : 'enemies'}`;
    case 22: return `camera cut: eye ${at(p1, p2, p3)}, looking at ${at(p4, p5, p6)}`;
    case 23: return `camera eye from ${at(p1, p2, p3)} to ${at(p4, p5, p6)}`;
    case 24: return `camera target from ${at(p1, p2, p3)} to ${at(p4, p5, p6)}`;
    case 25: return `the glide takes ${p1} frames`;
    case 26: return `glide the camera (a ${p1}th a step)`;
    case 28: return `the summon is monster ${p1}`;
    case 29: return `place it at ${at(p1, p2, p3)}, turned ${p4}°`;
    case 34: return `fade the summon ${p1 > 0 ? 'in' : 'out'} by ${p2 ? p1 + '/' + p2 : p1} a step`;
    case 35: return `the party's alpha ${p1}`;
    case 36: return `the party comes back over ${p1} frames`;
    case 40: return `wait ${p1 + 2} steps (the frame counter)`;
    case 45: return `shake the camera ${p1} frames (${at(p2, p3, p4)})`;
    case 49: return `flash ${p1} times, ${p2} frames each, ${p3} apart`;
    case 52: return `spell ${p1}'s effect on each target`;
    case 55: return `effect ${p1}/${p2} at ${at(p3, p4, p5)}`;
    case 61: return `load sound group ${p1}`;
    case 62: return `play sound ${p1}/${p2}`;
    default: return '';
  }
}

/// The interpreter: a script, stepped. Effects are made through `spawn(category, member, where, owner)`, which
/// returns a player; `defs` are the imported effects by "category/member".
function makeSummonRun(script, defs, { seed = 7 } = {}) {
  const steps = script.steps;
  const target = script.target || {};
  let rand = effectRandom(seed);
  const trunc = Math.trunc;
  let S;
  function reset() {
    rand = effectRandom(seed);
    S = {
      i: 0, step: -1, done: false, frame: 0, moveFrame: 0,
      beforePos: [0, 0, 0], afterPos: [0, 0, 0], beforeAt: [0, 0, 0], afterAt: [0, 0, 0],
      auto: false, autoFrame: 0, autoMove: 0,
      eye: SUMMON_BATTLE_EYE.slice(), at: SUMMON_BATTLE_AT.slice(),
      shake: { frame: -1, root: null, width: [0, 0, 0] },
      fade: { colour: 0, level: 0, from: 0, to: 0, frames: 0, t: 0 },
      dark: { on: false, colour: [1, 1, 1], from: [1, 1, 1], to: [1, 1, 1], frames: 0, t: 0 },
      summon: { loaded: false, shown: false, alpha: 0, pos: [0, 0, 0], yaw: 0, scale: 1, motion: null, gone: false },
      monstersHidden: false,
      party: { registered: false, alpha: 0, appear: null },
      effects: [],             // { player, owner }
      flash: null,             // { count, on, off, t }
      counter: 0,              // TurnSystem.effectCounter_ (DRAW_TARGET_EFFECT's stagger)
      targets: null,           // DRAW_TARGET_EFFECT's per-target state
      current: 0               // the record the step stopped on
    };
  }
  const calc = (pos, to, from, frames) => frames === 0 ? to.map((v, k) => v - from[k]) : pos.map((v, k) => v + trunc((to[k] - from[k]) / frames));
  const toWorld = v => v.map(x => x / 4096);

  // An effect of the game's where it plays: the summon's hit point, a side's point, a place, or each target.
  function spawn(category, member, where, owner) {
    const def = defs.get(category + '/' + member);
    if (!def) return null;
    const player = makeEffectPlayer(def, { seed: 7 + S.effects.length, anchor: where });
    S.effects.push({ player, owner });
    return player;
  }
  // TurnSystem.setHitEffectPosition on the summon (a monster): so far toward the camera and so far up as its offsets say.
  function summonHit() {
    const feet = toWorld(S.summon.pos), eye = toWorld(S.eye);
    const d = eye.map((v, k) => v - feet[k]);
    const l = Math.hypot(d[0], d[1], d[2]) || 1;
    const toward = target.toward || 0, up = target.up || 0;
    return [feet[0] + d[0] / l * toward, feet[1] + d[1] / l * toward + up, feet[2] + d[2] / l * toward];
  }
  const ownersDone = owner => S.effects.every(e => e.owner !== owner || e.player.finished);

  // btl.SummonCommand, a command a function: true when the record is done.
  const H = {
    0: p => { S.dark.on = true; S.dark.from = S.dark.colour.slice(); S.dark.to = [p[1] / 31, p[2] / 31, p[3] / 31]; S.dark.frames = p[0]; S.dark.t = 0; return true; },
    1: () => S.dark.t >= S.dark.frames,
    2: p => { S.dark.from = S.dark.colour.slice(); S.dark.to = [1, 1, 1]; S.dark.frames = p[0]; S.dark.t = 0; return true; },
    3: () => { if (S.dark.t >= S.dark.frames) { S.dark.on = false; return true; } return false; },
    4: p => { S.fade = { colour: p[0], level: S.fade.level, from: S.fade.level, to: 1, frames: p[1], t: 0 }; return true; },
    5: () => S.fade.level >= 1,
    6: p => { S.fade = { colour: S.fade.colour, level: S.fade.level, from: S.fade.level, to: 0, frames: p[0], t: 0 }; return true; },
    7: () => S.fade.level <= 0,
    8: () => { S.summon.loaded = true; S.summon.shown = false; S.summon.gone = false; return true; },
    9: () => { S.summon.alpha = 0; S.summon.scale = target.scale || 1; return true; },
    10: () => true, 11: () => true,
    12: p => { S.summon.motion = { id: p[0], loop: p[1] !== 0, frame: 0 }; return true; },
    13: p => !!S.summon.motion && S.summon.motion.frame === p[0],
    14: () => { S.summon.gone = true; return true; },
    15: () => { S.party.registered = true; return true; },
    16: () => true, 17: () => true, 18: () => true,
    19: p => !!spawn(p[0], p[1], summonHit(), 'summon'),
    20: p => !!spawn(p[0], p[1], p[2] ? [24, 0, 0] : [-36, 0, -5], 'summon'),
    21: () => ownersDone('summon'),
    22: p => { S.eye = [p[0], p[1], p[2]]; S.at = [p[3], p[4], p[5]]; return true; },
    23: p => { S.beforePos = [p[0], p[1], p[2]]; S.afterPos = [p[3], p[4], p[5]]; return true; },
    24: p => { S.beforeAt = [p[0], p[1], p[2]]; S.afterAt = [p[3], p[4], p[5]]; return true; },
    25: p => { S.frame = p[0]; return true; },
    26: p => {
      S.frame--;
      S.eye = calc(S.eye, S.afterPos, S.beforePos, p[0]);
      S.at = calc(S.at, S.afterAt, S.beforeAt, p[0]);
      if (S.frame <= 0) { S.frame = 0; S.eye = S.afterPos.slice(); S.at = S.afterAt.slice(); return true; }
      return false;
    },
    27: () => { S.eye = SUMMON_BATTLE_EYE.slice(); S.at = SUMMON_BATTLE_AT.slice(); return true; },
    28: () => true,
    29: p => { S.summon.pos = [p[0], p[1], p[2]]; S.summon.yaw = p[3]; return true; },
    30: () => { S.monstersHidden = false; return true; },
    31: () => { S.monstersHidden = true; return true; },
    32: () => { S.summon.shown = true; return true; },
    33: () => { S.summon.shown = false; return true; },
    34: p => {
      S.summon.alpha = Math.max(0, Math.min(100, S.summon.alpha + (p[1] === 0 ? p[0] : trunc(p[0] / p[1]))));
      return S.summon.alpha === 0 || S.summon.alpha === 100;
    },
    35: p => { S.party.alpha = Math.max(0, Math.min(1, p[0] / 100)); return true; },
    36: p => {
      if (!S.party.appear) S.party.appear = { frames: Math.max(1, p[0]), t: 0, from: S.party.alpha };
      if (S.party.appear.t >= S.party.appear.frames) { S.party.alpha = 1; S.party.appear = null; return true; }
      return false;
    },
    37: () => true, 38: () => true,
    39: p => {
      let a = false;
      S.frame--;
      S.eye = calc(S.eye, S.afterPos, S.beforePos, p[2]);
      S.at = calc(S.at, S.afterAt, S.beforeAt, p[2]);
      if (S.frame <= 0) { S.frame = 0; S.eye = S.afterPos.slice(); S.at = S.afterAt.slice(); a = true; }
      S.summon.alpha = Math.max(0, Math.min(100, S.summon.alpha + trunc(p[0] / (p[1] || 1))));
      return a && (S.summon.alpha === 0 || S.summon.alpha === 100);
    },
    40: p => { if (S.frame++ > p[0]) { S.frame = 0; return true; } return false; },
    41: () => true, 42: () => true, 43: () => true, 44: () => true,
    45: p => { S.shake = { frame: p[0], root: S.eye.slice(), width: [p[1], p[2], p[3]] }; return true; },
    46: p => { S.summon.yaw = p[1]; return true; },
    47: p => { S.afterPos = [p[0], p[1], p[2]]; S.beforePos = S.summon.pos.slice(); S.moveFrame = p[3]; return true; },
    48: p => {
      S.moveFrame--;
      S.summon.pos = S.summon.pos.map((v, k) => v + trunc((S.afterPos[k] - S.beforePos[k]) / (p[0] || 1)));
      if (S.moveFrame <= 0) { S.moveFrame = 0; S.summon.pos = S.afterPos.slice(); return true; }
      return false;
    },
    49: p => { S.flash = { count: p[0], on: p[1], off: p[2], t: 0 }; return true; },
    50: () => { S.auto = true; S.autoFrame = S.frame; S.autoMove = S.autoFrame; return true; },
    51: () => S.auto,
    52: p => targetEffect(p[0]),
    53: () => S.shake.frame < 0,
    54: () => true,
    55: p => !!spawn(p[0], p[1], [p[2] / 4096, p[3] / 4096, p[4] / 4096], 'summon'),
    56: () => ownersDone('monsters'),
    57: () => ownersDone('party'),
    58: () => true, 59: () => true, 60: () => true, 61: () => true, 62: () => true, 63: () => true
  };

  // TurnSystem.drawOnceMagicEffect: the spell record's effect on each target, half its play frame apart; done when the
  // last has run half its play frame (the damage's moment).
  function targetEffect(spellId) {
    const sp = (script.spells || []).find(x => x.id === spellId);
    if (!sp) return true;
    const half = Math.trunc((sp.frame || 0) / 2);
    if (!S.targets || S.targets.spell !== spellId) {
      const places = (window.summonPlaces && window.summonPlaces()) || { monsters: [], party: [] };
      S.targets = { spell: spellId, list: (sp.party ? places.party : places.monsters).map(t => ({ hit: t.hit, started: false, counter: -1 })) };
    }
    if (S.counter > 0) S.counter--;
    let flag = true;
    for (const t of S.targets.list) {
      if (t.started) continue;
      if (S.counter > 0) { flag = false; continue; }
      spawn(sp.category, sp.member > 0 ? sp.member : 1, t.hit, sp.party ? 'party' : 'monsters');
      S.counter = half;
      t.started = true; t.counter = 0;
      flag = false;
    }
    for (const t of S.targets.list) {
      if (t.counter < 0) continue;
      if (t.counter >= half) t.counter = -1; else flag = false;
    }
    if (flag) S.targets = null;
    return flag;
  }

  // BaseSummon.run: records while each is done, the next in the same step when marked again.
  function run() {
    if (S.done) return;
    for (;;) {
      const r = steps[S.i];
      if (!r) { S.done = true; return; }
      S.current = S.i;
      const handler = H[r.op];
      const ok = handler ? handler(r.p) : true;
      if (!ok) return;
      if (r.op === 44) { S.done = true; return; }
      S.i++;
      if (!steps[S.i] || !steps[S.i].again) return;
    }
  }

  /// One game step: the auto camera, the script, then what runs on its own - the shake, fades, the motion, effects.
  function step() {
    S.step++;
    if (S.auto) {
      S.autoFrame--;
      S.eye = calc(S.eye, S.afterPos, S.beforePos, S.autoMove);
      S.at = calc(S.at, S.afterAt, S.beforeAt, S.autoMove);
      if (S.autoFrame <= 0) { S.autoFrame = 0; S.autoMove = 0; S.auto = false; S.eye = S.afterPos.slice(); S.at = S.afterAt.slice(); }
    }
    run();
    if (S.shake.frame >= 0) {
      S.shake.frame--;
      if (S.shake.frame <= 0) S.eye = S.shake.root.slice();
      else S.eye = S.shake.root.map((v, k) => v + Math.floor(rand() * Math.max(1, S.shake.width[k])) - trunc(S.shake.width[k] / 2));
      if (S.shake.frame <= 0) S.shake.frame = -1;
    }
    const f = S.fade;
    if (f.frames > 0 && f.t < f.frames) { f.t++; f.level = f.from + (f.to - f.from) * f.t / f.frames; } else if (f.frames === 0) f.level = f.to;
    const d = S.dark;
    if (d.t < d.frames) d.t++;
    const k = d.frames > 0 ? d.t / d.frames : 1;
    d.colour = d.from.map((v, j) => v + (d.to[j] - v) * k);
    if (S.summon.motion) S.summon.motion.frame++;
    if (S.party.appear) { S.party.appear.t++; S.party.alpha = Math.min(1, S.party.appear.from + (1 - S.party.appear.from) * S.party.appear.t / S.party.appear.frames); }
    if (S.targets) for (const t of S.targets.list) if (t.counter >= 0) t.counter++;
    for (const e of S.effects) if (!e.player.finished) e.player.step();
    if (S.flash) S.flash.t++;
  }

  reset();
  return {
    reset, step,
    get state() { return S; },
    get done() { return S.done; }
  };
}

let summonState = { layout: { background: 'files/b01.nmdp.lz' } };

/// A summon's view: its outcome, the Stage playing it, the steps beside.
async function openSummon(name) {
  const level = parseInt(String(name).split('/')[1], 10) || 0;
  const node = view('summon', name, false);
  const body = $('.summon-body', node);
  const outcomePick = $('.summon-outcome', node);
  const facts = $('.facts', node);
  const list = await api('/api/summons').catch(() => null);
  const entry = list && list.ok ? list.summons.find(s => s.level === level) : null;
  $('.name', node).textContent = entry ? (entry.creature ? `${entry.creature} (${entry.name})` : entry.name) : name;
  for (const o of (entry ? entry.outcomes : [])) {
    const opt = document.createElement('option');
    opt.value = String(o.type);
    opt.textContent = `${o.name} (${o.kind}${o.kind === 'combine' ? ', a Summoner\'s' : ''})`;
    outcomePick.append(opt);
  }

  const stageBox = document.createElement('div');
  stageBox.className = 'effect-stage summon-stage';
  const canvas = document.createElement('canvas');
  canvas.className = 'scene';
  const fade = document.createElement('div');
  fade.className = 'effect-flash summon-fade';
  const flash = document.createElement('div');
  flash.className = 'effect-flash';
  stageBox.append(canvas, fade, flash);
  const controls = document.createElement('div');
  controls.className = 'effect-controls bar';
  const button = (text, title) => { const b = document.createElement('button'); b.textContent = text; b.title = title; controls.append(b); return b; };
  const play = button('Pause', 'play or pause');
  const stepOne = button('Step', 'one game step');
  const again = button('Restart', 'from the first step');
  const scrub = Object.assign(document.createElement('input'), { type: 'range', min: 0, max: 60, value: 0, className: 'effect-scrub', title: 'the step' });
  const label = document.createElement('span');
  label.className = 'effect-frame';
  const loopBox = Object.assign(document.createElement('label'), { className: 'toggle', title: 'start again when it ends' });
  const loop = Object.assign(document.createElement('input'), { type: 'checkbox', checked: true });
  loopBox.append(loop, ' loop');
  const mapPick = document.createElement('select');
  mapPick.title = 'the battle map it plays on';
  controls.append(scrub, label, loopBox, mapPick);
  const split = document.createElement('div');
  split.className = 'summon-split';
  const left = document.createElement('div');
  left.className = 'summon-left';
  left.append(stageBox, controls);
  const stepsBox = document.createElement('div');
  stepsBox.className = 'summon-steps';
  split.append(left, stepsBox);
  body.append(split);

  const stage = makeEffectStage(canvas, key => effectTextureUrl(key, null));
  if (!stage) { facts.textContent = 'this browser has no WebGL'; return; }
  stage.setLayout({ count: 'group', background: summonState.layout.background, view: 'battle', show: { map: true, monsters: true, heroes: 'all', floor: true, marks: false } });
  window.summonPlaces = () => stage.places();
  // The Goblins stand where a battle's three stand (formation 1's monster).
  api('/api/effect/target?monster=1').then(t => { if (t && !t.error) stage.setTarget(t); }).catch(() => {});
  api('/api/models').then(models => {
    const none = document.createElement('option'); none.value = ''; none.textContent = 'no map'; mapPick.append(none);
    for (const m of models || []) { const k = /(^|\/)b(\d\d)\.nmdp\.lz$/i.exec(m.name || ''); if (k) { const o = document.createElement('option'); o.value = m.name; o.textContent = 'battle map ' + k[2]; mapPick.append(o); } }
    mapPick.value = summonState.layout.background;
  }).catch(() => {});
  mapPick.onchange = () => { summonState.layout.background = mapPick.value; stage.setLayout({ background: mapPick.value }); };

  let script = null, runner = null, length = 1, playing = true, owed = 0, last = performance.now(), firstAt = [];
  const defs = new Map();
  let motionIndex = new Map();

  async function load(type) {
    facts.textContent = 'loading…';
    script = await api(`/api/summon?level=${level}&type=${type}`);
    if (!script.ok) { facts.textContent = script.error; return; }
    // Every effect it plays, imported once: its summon effects by pack and member, and its spells' effects.
    const wanted = new Set();
    for (const st of script.steps) {
      if (st.op === 19 || st.op === 20 || st.op === 55) wanted.add(st.p[0] + '/' + st.p[1]);
    }
    for (const sp of script.spells || []) if (sp.category > 0) wanted.add(sp.category + '/' + (sp.member > 0 ? sp.member : 1));
    await Promise.all([...wanted].filter(k => !defs.has(k)).map(async k => {
      const [c, m] = k.split('/').map(Number);
      const r = await api(`/api/effect/import?category=${c}&member=${m}`).catch(() => null);
      if (r && !r.error) defs.set(k, r.effect);
    }));
    // The summon's motions: their indexes in its pack by the motion ids the script names.
    motionIndex = new Map();
    if (script.model && script.motions) {
      const packs = await api(`/api/model/motions?name=${encodeURIComponent(script.model)}`).catch(() => []);
      const pack = (packs || []).find(p => new RegExp('(^|/)' + script.motions + '\\.ncap', 'i').test(p.name));
      if (pack) { motionIndex.set('pack', pack.name); for (const mo of pack.motions) motionIndex.set(mo.id, mo.index); }
    }
    runner = makeSummonRun(script, defs);
    // A pass through it all, for its length and where each step first runs.
    const probe = makeSummonRun(script, defs);
    firstAt = new Array(script.steps.length).fill(-1);
    let n = 0;
    while (n < 3000 && !probe.done) { probe.step(); const s = probe.state; for (let i = 0; i <= s.current; i++) if (firstAt[i] < 0) firstAt[i] = n; n++; }
    length = Math.max(1, n);
    scrub.max = String(length);
    facts.textContent = `${script.summon} · ${script.name} · ${script.steps.length} steps, ${length} frames (${(length / 30).toFixed(1)} s)`;
    drawSteps();
    runner.reset();
    owed = 0; playing = true; play.textContent = 'Pause';
  }

  function drawSteps() {
    stepsBox.textContent = '';
    const head = document.createElement('div');
    head.className = 'summon-steps-head';
    head.textContent = 'The script';
    stepsBox.append(head);
    script.steps.forEach((st, i) => {
      const row = document.createElement('div');
      row.className = 'summon-step';
      row.dataset.index = String(i);
      const n = document.createElement('i');
      n.textContent = String(i);
      const op = document.createElement('b');
      op.textContent = st.name.toLowerCase().replace(/_/g, ' ');
      const words = document.createElement('span');
      words.textContent = summonStepText(st) || (st.p.some(v => v) ? st.p.slice(0, 6).join(', ') : '');
      if (st.again) row.classList.add('again');
      row.title = (st.again ? 'runs in the same step as the one before · ' : '') + (firstAt[i] >= 0 ? 'first runs at frame ' + firstAt[i] : 'not reached');
      row.append(n, op, words);
      row.onclick = () => { if (firstAt[i] >= 0) goTo(firstAt[i]); };
      stepsBox.append(row);
    });
  }

  function goTo(frame) {
    playing = false; play.textContent = 'Play';
    runner.reset();
    for (let i = 0; i < frame; i++) runner.step();
    scrub.value = String(frame);
  }
  play.onclick = () => { playing = !playing; play.textContent = playing ? 'Pause' : 'Play'; };
  stepOne.onclick = () => { playing = false; play.textContent = 'Play'; if (runner) { if (runner.done) runner.reset(); runner.step(); } };
  again.onclick = () => { if (runner) runner.reset(); owed = 0; };
  scrub.oninput = () => goTo(parseInt(scrub.value, 10) || 0);
  outcomePick.onchange = () => load(parseInt(outcomePick.value, 10) || 0);

  let shownIndex = -1;
  function frameLoop(now) {
    if (!canvas.isConnected) return;
    const dt = Math.min(0.25, (now - last) / 1000);
    last = now;
    if (runner && playing) {
      owed += dt * 30;
      while (owed >= 1) {
        owed -= 1;
        if (runner.done) { if (loop.checked) runner.reset(); else { playing = false; play.textContent = 'Play'; break; } }
        runner.step();
      }
    }
    if (runner) {
      const S = runner.state;
      // The Stage as the script leaves it: its camera, the map's tint, who is hidden, the summon.
      stage.override = { eye: S.eye.map(v => v / 4096), at: S.at.map(v => v / 4096) };
      stage.mapTint = S.dark.on ? S.dark.colour : null;
      stage.hide = { monsters: S.monstersHidden, party: S.party.registered ? S.party.alpha : 0 };
      const sm = S.summon;
      stage.extras = sm.loaded && !sm.gone && sm.shown && script.model ? [{
        model: script.model, pos: sm.pos.map(v => v / 4096), yaw: sm.yaw, scale: sm.scale, alpha: sm.alpha / 100,
        pack: sm.motion ? motionIndex.get('pack') : null, index: sm.motion ? (motionIndex.get(sm.motion.id) || 0) : 0,
        frame: sm.motion ? sm.motion.frame : 0, loop: sm.motion ? sm.motion.loop : false
      }] : [];
      const quads = [], meshes = [];
      for (const e of S.effects) { if (!e.player.finished) { quads.push(...e.player.particles()); meshes.push(...e.player.models()); } }
      stage.draw(quads, 1, meshes);
      fade.style.background = S.fade.colour ? '#fff' : '#000';
      fade.style.opacity = String(Math.max(0, Math.min(1, S.fade.level)));
      const fl = S.flash;
      let lit = false;
      if (fl && fl.count > 0) { const period = Math.max(1, fl.on + fl.off), k = Math.floor(fl.t / period); lit = k < fl.count && (fl.t % period) < fl.on; }
      flash.style.background = '#fff';
      flash.style.opacity = lit ? '0.8' : '0';
      scrub.value = String(Math.min(length, S.step + 1));
      label.textContent = `frame ${Math.max(0, S.step)} / ${length}`;
      if (S.current !== shownIndex) {
        shownIndex = S.current;
        for (const row of stepsBox.querySelectorAll('.summon-step')) row.classList.toggle('on', parseInt(row.dataset.index, 10) === shownIndex);
        const on = stepsBox.querySelector('.summon-step.on');
        if (on && playing) on.scrollIntoView({ block: 'nearest' });
      }
    }
    requestAnimationFrame(frameLoop);
  }
  outcomePick.value = '0';
  await load(0);
  requestAnimationFrame(frameLoop);
}
