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

// The commands (btl.SUMMON_BEHAVIOR's order) and what their numbers are, for the step editor: a label and a kind -
// 'n' a number, 'u' a position in world units (the battle's fixed point / 4096), 'e' an effect (a pack of the
// game's, or one of the mod's by its id), 'b' 0 or 1.
const SUMMON_COMMANDS = ['SET_DARK_SCREEN', 'IS_DARK_SCREEN', 'SET_NORMAL_SCREEN', 'IS_NORMAL_SCREEN', 'SET_FADE_OUT', 'IS_FADE_OUT', 'SET_FADE_IN', 'IS_FADE_IN',
  'SET_MODEL', 'IS_MODEL', 'SET_MOTION', 'IS_MOTION', 'START_MOTION', 'IS_MOTION_FRAME', 'DEL_SUMMON', 'REGISTER_PLAYERS',
  'SET_EFFECT', 'CLEAR_EFFECT', 'IS_EFFECT', 'DRAW_SUMMON_EFFECT', 'DRAW_SUMMON_EFFECT_TARGET_ALL', 'IS_END_SUMMON_EFFECT', 'CHANGE_CAMERA', 'SET_CAMERA_POSITION',
  'SET_CAMERA_TARGET', 'SET_MOVE_CAMERA_FRAME', 'MOVING_CAMERA', 'SET_BATTLE_CAMERA', 'SET_SUMMON_PARAMETER', 'SET_POSITION_AND_ROTATION', 'SHOW_MONSTERS', 'HIDE_MONSTERS',
  'SHOW_SUMMON', 'HIDE_SUMMON', 'SUMMON_ALPHA_RATE', 'SET_PLAYERS_ALPHA', 'APPEAR_PLAYERS', 'SET_TURN_FLAG', 'IS_TURN_FLAG', 'MOVE_CAMERA_AND_SUMMON_ALPHA',
  'FRAME_COUNT', 'SET_2D', 'IS_2D', 'DEAD_CHARACTERS', 'SUMMON_BEHAVIOR_END', 'SHAKE_CAMERA', 'ROTATE_CHARACTER', 'READY_MOVE_CHARACTER',
  'MOVE_CHARACTER', 'SET_FLASH', 'READY_AUTO_CAMERA', 'IS_AUTO_CAMERA', 'DRAW_TARGET_EFFECT', 'IS_SHAKE_CAMERA', 'SET_SHOW_PLAYER_WINDOW', 'CREATE_EFFECT_AND_SET_POSITION',
  'IS_END_MONSTER_EFFECT', 'IS_END_PLAYER_EFFECT', 'SET_MONSTERS_ALPHA', 'APPEAR_MONSTERS', 'DISAPPEAR_MONSTERS', 'SET_SE', 'PLAY_SE', 'CLEAR_SE'];
const SUMMON_GROUPS = [
  ['Screen', [0, 1, 2, 3, 4, 5, 6, 7, 49]],
  ['The summon', [28, 8, 9, 10, 11, 12, 13, 29, 32, 33, 34, 46, 47, 48, 14]],
  ['Effects', [16, 18, 19, 20, 55, 52, 21, 56, 57, 17]],
  ['Camera', [22, 23, 24, 25, 26, 50, 51, 45, 53, 27, 39]],
  ['The party and monsters', [31, 30, 15, 35, 36, 54, 58, 59, 60]],
  ['Timing and the turn', [40, 37, 38, 41, 42, 43, 44]],
  ['Sound', [61, 62, 63]]
];
const XYZ = w => [[w + ' x', 'u'], [w + ' y', 'u'], [w + ' z', 'u']];
const SUMMON_PARAMS = {
  0: [['frames', 'n'], ['red (0-31)', 'n'], ['green (0-31)', 'n'], ['blue (0-31)', 'n']], 2: [['frames', 'n']],
  4: [['to white', 'b'], ['frames', 'n']], 6: [['frames', 'n']],
  8: [['model (f###)', 'model']], 10: [['motions', 'motions']], 12: [['motion', 'motion'], ['loop', 'b'], ['blend frames', 'n']], 13: [['its frame', 'n']],
  16: [['effect pack', 'e']], 19: [['effect', 'e'], ['member', 'n'], ['on the ground', 'b'], ['scattered', 'b']],
  20: [['effect', 'e'], ['member', 'n'], ['over the party', 'b']],
  22: [...XYZ('eye'), ...XYZ('target')], 23: [...XYZ('eye from'), ...XYZ('eye to')], 24: [...XYZ('target from'), ...XYZ('target to')],
  25: [['frames', 'n']], 26: [['steps (a glide in so many)', 'n']], 28: [['monster (size, hit point)', 'monster']],
  29: [...XYZ('at'), ['turned (°)', 'n']], 34: [['alpha a step', 'n'], ['divided by', 'n']], 35: [['alpha', 'n'], ['shadow', 'n']], 36: [['frames', 'n']],
  37: [['effects end', 'b'], ['numbers end', 'b'], ['party ends', 'b'], ['enemies end', 'b']],
  39: [['alpha a step', 'n'], ['divided by', 'n'], ['camera steps', 'n']], 40: [['steps', 'n']],
  45: [['frames', 'n'], ['width x', 'u'], ['width y', 'u'], ['width z', 'u']], 46: [['x (°)', 'n'], ['y (°)', 'n'], ['z (°)', 'n']],
  47: [...XYZ('to'), ['frames', 'n']], 48: [['steps', 'n']], 49: [['times', 'n'], ['frames lit', 'n'], ['frames apart', 'n']],
  52: [['spell (its effect on each target)', 'spell']], 54: [['show', 'b']], 55: [['effect', 'e'], ['member', 'n'], ...XYZ('at')],
  58: [['alpha', 'n'], ['shadow', 'n']], 59: [['frames', 'n']], 60: [['frames', 'n']], 61: [['group', 'n']], 62: [['group', 'n'], ['number', 'n']]
};
const summonWords = name => String(name || '').toLowerCase().replace(/_/g, ' ');

/// A definition's steps (defs/summons: "do", "p", "again") as the runner's: a command number, seven values, again.
function summonStepsOf(def) {
  return (def.steps || []).map(st => {
    const op = SUMMON_COMMANDS.indexOf(String(st.do || '').toUpperCase().replace(/ /g, '_').replace('CRAETE', 'CREATE'));
    const p = new Array(7).fill(0);
    (st.p || []).slice(0, 7).forEach((v, i) => { p[i] = v; });
    return { op: op < 0 ? 44 : op, name: op < 0 ? String(st.do) : SUMMON_COMMANDS[op], p, again: !!st.again };
  });
}
/// The runner's steps back as a definition's (trailing zeros left off, as a copy writes them).
function summonDefSteps(steps) {
  return steps.map(st => {
    const out = { do: SUMMON_COMMANDS[st.op] || String(st.op) };
    let used = 7;
    while (used > 0 && (st.p[used - 1] === 0 || st.p[used - 1] === '' || st.p[used - 1] == null)) used--;
    if (used) out.p = st.p.slice(0, used);
    if (st.again) out.again = true;
    return out;
  });
}

/// The Summons page: the eight, each with its three outcomes, then the mod's new ones. `modOnly` for the Mod
/// folder's Summons: the mod's new summons and the eight whose outcomes it plays by steps of its own.
async function summonsForList({ modOnly = false } = {}) {
  const r = await api('/api/summons').catch(() => null);
  state.summons = r && r.ok ? r.summons : [];
  // A new summon of the mod's: summon/<level>/<its spell's id>; either kind marked the mod's.
  return state.summons.filter(s => !modOnly || s.spell || s.mine > 0).map(s => ({
    name: 'summon/' + s.level + (s.spell ? '/' + s.spell : ''), overridden: !!s.spell || s.mine > 0,
    def: { name: s.spell ? s.name : s.creature ? `${s.creature} (${s.name})` : s.name },
    note: (s.spell ? 'as ' + s.baseName + ' · ' : '') + s.outcomes.map(o => o.name + (o.mine && !s.spell ? ' \u25CF' : '')).join(' · ')
  }));
}

/// New summon…: a name and the summon it starts as - a spell of the mod's based on that summon's, with its three
/// outcomes' scripts copied from the base's to make its own.
async function newSummonDialog(level = 1) {
  const body = dialog('New summon');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A summon of the mod\'s own: a spell that an Evoker or a Summoner learns and casts like the game\'s eight. It starts as one of them - its level, its outcomes, their damage - with its three scripts copied to change: the model, the camera, the effects.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: 'Frost Wyrm' });
  const label = document.createElement('label');
  label.className = 'dialog-field';
  label.textContent = 'Starts as';
  const pick = document.createElement('select');
  ModSummonNames.forEach(([creature, spell], i) => { const o = document.createElement('option'); o.value = String(i); o.textContent = `${creature} (${spell}, level ${i + 1})`; pick.append(o); });
  pick.value = String(level);
  label.append(pick);
  body.append(label);
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const n = name.value.trim();
    if (!n) { problem.textContent = 'A summon needs a name.'; return; }
    go.disabled = true;
    try {
      const r = await api('/api/project/summon/new', { name: n, level: parseInt(pick.value, 10) || 0 });
      if (!r.ok) throw new Error(r.error);
      body.close();
      say(`made the summon ${n} (spell ${r.number}), its three outcomes its own`, 'good');
      if (state.browse === 'summon') await loadList();
      openDoc('summon', 'summon/' + r.level + '/' + r.spell);
    } catch (e) { problem.textContent = e.message; go.disabled = false; }
  };
  actions.append(go);
  body.append(actions);
  name.focus();
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
    case 10: return p1 >= 1000 ? `load a monster's motions b_f${String(p1 - 1000).padStart(3, '0')}` : `load its motions b_sm${String(p1).padStart(3, '0')}`;
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
    const def = defs.get(typeof category === 'string' ? 'mod:' + category : category + '/' + member);
    if (!def) return null;
    const player = makeEffectPlayer(def, { seed: 7 + S.effects.length, anchor: where });
    // Which step made it, and on which frame: the timeline draws its life under the step.
    S.effects.push({ player, owner, by: S.current, born: S.step, key: typeof category === 'string' ? 'mod:' + category : category + '/' + member });
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

let summonState = { layout: { background: 'files/b01.nmdp.lz' }, showList: false };

// The timeline's lanes: a command's group, as the step editor groups them.
const SUMMON_LANES = [['Screen', '#b99af0'], ['The summon', '#6ac48a'], ['Effects', '#e0b86a'], ['Camera', '#6ea8fe'], ['The party and monsters', '#e07a7a'], ['Timing and the turn', '#8b93a1'], ['Sound', '#6fc7e6']];
function summonLaneOf(op) {
  const g = SUMMON_GROUPS.findIndex(([, ops]) => ops.includes(op));
  return g < 0 ? 5 : g;
}
// A wait's length is its setter's number: the step whose frames a drag of the wait's end changes, and which of its numbers.
const SUMMON_WAITS = { 1: [0, 0], 3: [2, 0], 5: [4, 1], 7: [6, 0], 26: [25, 0], 53: [45, 0] };
const SUMMON_SELF_TIMED = { 40: 0, 36: 0 };

/// The Hierarchy's outline of a summon: its look, then its steps (shell.js outlineFor asks).
function summonOutline(doc) {
  const ed = doc.summonEditor;
  if (!ed) return [];
  const steps = ed.steps();
  const editable = ed.editable();
  return [
    { label: 'Summon', children: [{ label: 'Look', ref: 'look', icon: 'effect', note: editable ? 'the mod\'s' : 'the game\'s' }] },
    {
      label: 'Steps', note: String(steps.length),
      children: steps.map((st, i) => ({
        label: i + ' ' + summonWords(st.name),
        note: ed.firstAt(i) >= 0 ? 'f' + ed.firstAt(i) : '',
        ref: 'step:' + i,
        icon: ['image', 'model', 'effect', 'map', 'character', 'scene', 'audio'][summonLaneOf(st.op)],
        menu: editable ? () => [
          { label: 'Add a step after', icon: 'file', run: () => ed.insert(i + 1) },
          { label: 'Duplicate', icon: 'file', run: () => ed.duplicate(i) },
          { label: 'Move up', icon: 'file', disabled: i === 0, run: () => ed.move(i, i - 1) },
          { label: 'Move down', icon: 'file', disabled: i === steps.length - 1, run: () => ed.move(i, i + 1) },
          { label: 'Delete', icon: 'file', run: () => ed.remove(i) }
        ] : null
      }))
    }
  ];
}

/// A summon's view: its outcome, the Stage playing it, the timeline of its steps under it, the Inspector editing them.
async function openSummon(name) {
  const doc = activeDoc;
  const level = parseInt(String(name).split('/')[1], 10) || 0;
  // A new summon of the mod's: its spell's id (summon/<level>/<id>), played as its base's level.
  const spell = String(name).split('/')[2] || null;
  const node = view('summon', name, false);
  const body = $('.summon-body', node);
  const outcomePick = $('.summon-outcome', node);
  const facts = $('.facts', node);
  const bar = $('.bar', node);
  const list = await api('/api/summons').catch(() => null);
  const entry = list && list.ok ? list.summons.find(s => s.level === level && (s.spell || null) === spell) : null;
  $('.name', node).textContent = entry ? (entry.spell ? entry.name : entry.creature ? `${entry.creature} (${entry.name})` : entry.name) : name;
  if (spell) { $('.name', node).append(' ', ownBadge('mod', { title: 'a summon of the mod\'s: its base\'s level, outcomes and damage, its own scripts' })); $('.name', node).title = 'as ' + (entry ? entry.baseName : 'its base'); }
  for (const o of (entry ? entry.outcomes : [])) {
    const opt = document.createElement('option');
    opt.value = String(o.type);
    opt.textContent = `${o.name} (${o.kind}${o.kind === 'combine' ? ', a Summoner\'s' : ''})`;
    outcomePick.append(opt);
  }

  // The Stage, its controls, the timeline under them; the script list beside, when asked for.
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
  const scrub = Object.assign(document.createElement('input'), { type: 'range', min: 0, max: 60, value: 0, className: 'effect-scrub', title: 'the frame' });
  const label = document.createElement('span');
  label.className = 'effect-frame';
  const loopBox = Object.assign(document.createElement('label'), { className: 'toggle', title: 'start again when it ends' });
  const loop = Object.assign(document.createElement('input'), { type: 'checkbox', checked: true });
  loopBox.append(loop, ' loop');
  const mapPick = document.createElement('select');
  mapPick.title = 'the battle map it plays on';
  const camChip = document.createElement('button');
  camChip.className = 'chip';
  camChip.textContent = 'Free camera';
  camChip.title = 'look around freely (drag, the wheel, the right button to fly) instead of through the script\'s camera - then Set from camera on a camera step';
  const listChip = document.createElement('button');
  listChip.className = 'chip';
  listChip.textContent = 'Script list';
  listChip.title = 'the steps as a list beside the Stage (they are in the Hierarchy too)';
  controls.append(scrub, label, loopBox, mapPick, camChip, listChip);
  const timelineBox = document.createElement('div');
  timelineBox.className = 'effect-timeline-box summon-timeline-box';
  const split = document.createElement('div');
  split.className = 'summon-split';
  const left = document.createElement('div');
  left.className = 'summon-left';
  left.append(stageBox, controls, timelineBox);
  const stepsBox = document.createElement('div');
  stepsBox.className = 'summon-steps';
  split.append(left, stepsBox);
  body.append(split);

  // Pictures: the game's by pack, the mod's beside its effects (defs/effects - any of them names the folder).
  const stage = makeEffectStage(canvas, key => effectTextureUrl(key, 'defs/effects/summon.json'));
  if (!stage) { facts.textContent = 'this browser has no WebGL'; return; }
  stage.setLayout({ count: 'group', background: summonState.layout.background, view: 'battle', show: { map: true, monsters: true, heroes: 'all', floor: true, marks: false } });
  window.summonPlaces = () => stage.places();
  api('/api/effect/target?monster=1').then(t => { if (t && !t.error) stage.setTarget(t); }).catch(() => {});
  api('/api/models').then(models => {
    const none = document.createElement('option'); none.value = ''; none.textContent = 'no map'; mapPick.append(none);
    for (const m of models || []) { const k = /(^|\/)b(\d\d)\.nmdp\.lz$/i.exec(m.name || ''); if (k) { const o = document.createElement('option'); o.value = m.name; o.textContent = 'battle map ' + k[2]; mapPick.append(o); } }
    mapPick.value = summonState.layout.background;
    // The characters' models (f###), for the Look's model picker.
    summonState.models = (models || []).map(m => /(^|\/)f(\d{3})\.nmdp\.lz$/i.exec(m.name || '')).filter(Boolean).map(k => parseInt(k[2], 10));
  }).catch(() => {});
  mapPick.onchange = () => { summonState.layout.background = mapPick.value; stage.setLayout({ background: mapPick.value }); };

  let script = null, runner = null, length = 1, playing = true, owed = 0, last = performance.now(), firstAt = [], endAt = [], spans = [];
  const defs = new Map();
  let motionIndex = new Map();
  let mine = null, showGame = false, freeCam = false, saving = 0, gameSteps = null, type = 0;
  // What the steps playing make the summon: model, motions (and their list), monster and its offsets.
  const look = { model: null, motions: null, monster: -1, target: null, packKey: null, motionList: [], own: null, skeleton: null };

  // The project's models of its own (a glTF on a game model's skeleton, SummonModels.cs), and their files' clips.
  let ownModels = [];
  const readOwnModels = async () => { const r = await api('/api/project/own-models').catch(() => null); ownModels = (r && r.models) || []; };
  await readOwnModels();
  const ownFiles = new Map();   // gltf -> { rest (its box and clip names), clips: Map(name -> frames) }
  async function ownFile(gltf) {
    if (!ownFiles.has(gltf)) {
      const rest = await api(`/api/model/gltf-clip?name=${encodeURIComponent(gltf)}`).catch(() => null);
      ownFiles.set(gltf, { rest: rest && rest.ok ? rest : null, clips: new Map() });
    }
    return ownFiles.get(gltf);
  }
  // A clip's frames, fetched once (the Stage shows the rest pose until they come).
  function ownClip(file, gltf, clip) {
    if (!clip) return file.rest;
    if (!file.clips.has(clip)) {
      file.clips.set(clip, null);
      api(`/api/model/gltf-clip?name=${encodeURIComponent(gltf)}&clip=${encodeURIComponent(clip)}`)
        .then(r => file.clips.set(clip, r && r.ok ? r : file.rest)).catch(() => file.clips.set(clip, file.rest));
    }
    return file.clips.get(clip) || file.rest;
  }
  // Its size: the definition's scale, else about the base's height (twice its monster's hit point, as the client fits
  // it to the base model's box), stood on its feet.
  function ownFit(own, file) {
    const box = file && file.rest, h = box ? box.max[1] - box.min[1] : 0;
    const auto = h > 0 ? (2 * (look.target && look.target.up > 0 ? look.target.up : 20)) / h : 1;
    const scale = own.scale > 0 ? own.scale : auto;
    return { scale, lift: box ? -box.min[1] * scale : 0, auto };
  }
  // The summon as a glTF of the mod's: the clip the definition maps to the motion playing, else its rest pose.
  function ownExtra(sm) {
    const own = look.own, file = ownFiles.get(own.gltf);
    const clip = sm.motion ? (own.clips || {})[String(sm.motion.id)] : null;
    const posed = file ? ownClip(file, own.gltf, clip) : null;
    if (!posed) return null;
    return {
      model: own.gltf, pos: sm.pos.map(v => v / 4096), yaw: sm.yaw + ((own.rotation && own.rotation[1]) || 0), scale: sm.scale, alpha: sm.alpha / 100,
      skinned: { ...posed, id: own.gltf + '|' + (clip || '') }, fit: ownFit(own, file), frame: sm.motion ? sm.motion.frame : 0
    };
  }
  // A definition changed: written, read again, played again.
  let ownSaving = 0;
  function saveOwn(own, patch, quiet = false) {
    Object.assign(own, patch);
    clearTimeout(ownSaving);
    ownSaving = setTimeout(async () => {
      const r = await api('/api/project/own-models/save', { model: own.model, ...patch }).catch(e => ({ ok: false, error: e.message }));
      if (!r.ok) { say(r.error, 'bad'); return; }
      say('saved defs/models/' + own.model + '.json', 'good');
      await readOwnModels();
      rebuild(false).then(quiet ? () => { timeline.draw(); } : refreshPanels);
    }, quiet ? 250 : 0);
  }
  /// A glTF of the project's as the summon's model: a definition of its own (the next free f3NN, on the skeleton the
  /// summon has now), and SET_MODEL pointed at it.
  async function useOwnFile(gltf, modelAt) {
    const now = Number(current()[modelAt].p[0]) || 202;
    const was = ownModels.find(m => m.number === now);
    const base = was ? was.base : 'f' + String(now).padStart(3, '0');
    const r = await api('/api/project/own-models/save', { gltf, base }).catch(e => ({ ok: false, error: e.message }));
    if (!r.ok) { say(r.error, 'bad'); return; }
    await readOwnModels();
    say(`${gltf.replace(/^assets\//, '')} is the model ${r.model.model} now, on ${base}'s skeleton (defs/models/${r.model.model}.json)`, 'good');
    setParam(modelAt, 0, r.model.number);
  }
  // The outcome's name in battle: the mod's steps may give one ("name"), else a new summon's own name, else the game's.
  const outcomeName = () => (editable() && mine.def.name) || (spell && entry ? entry.name : script.name);
  const barButton = (text, title, cls) => { const b = document.createElement('button'); b.textContent = text; b.title = title; if (cls) b.className = cls; bar.append(b); return b; };
  const copyButton = barButton('Copy into the mod', 'this outcome\'s script as steps of the mod\'s own (defs/summons): edited here, played by the battle in place of the game\'s');
  const whose = document.createElement('button');
  whose.className = 'chip';
  whose.title = 'play the mod\'s steps, or the game\'s to compare';
  bar.append(whose);
  const testButton = barButton('Test in battle', 'OpenFF with this mod alone: an Evoker (a Summoner for the combine) casts it, this outcome forced, in a battle', 'primary');
  const removeButton = spell
    ? barButton('Use the base\'s steps', 'this outcome\'s steps taken out: the summon plays its base\'s script for it')
    : barButton('Delete the copy', 'the mod\'s steps taken out: the game\'s play again');
  camChip.onclick = () => {
    freeCam = !freeCam;
    camChip.classList.toggle('on', freeCam);
    if (freeCam && runner) { const S = runner.state; stage.lookFrom(S.eye.map(v => v / 4096), S.at.map(v => v / 4096)); }
  };
  listChip.onclick = () => { summonState.showList = !summonState.showList; syncBar(); drawSteps(); };

  const editable = () => !!mine && !showGame;
  const current = () => (editable() ? mine.steps : gameSteps) || [];
  const selectedIndex = () => { const m = /^step:(\d+)$/.exec(String(doc && doc.selection || '')); return m ? parseInt(m[1], 10) : -1; };
  function select(i, jump = true) {
    // The panel is about this summon again (not a row clicked in the project list).
    if (typeof clearInspected === 'function') clearInspected();
    if (doc) doc.selection = i >= 0 ? 'step:' + i : 'look';
    if (jump && i >= 0 && firstAt[i] >= 0) goTo(firstAt[i]);
    drawHierarchy(); drawInspector(); drawSteps(); timeline.draw();
  }
  function syncBar() {
    copyButton.hidden = !!mine;
    whose.hidden = !mine;
    removeButton.hidden = !mine;
    whose.textContent = showGame ? 'The game\'s' : 'The mod\'s';
    whose.classList.toggle('on', !showGame);
    listChip.classList.toggle('on', summonState.showList);
    stepsBox.hidden = !summonState.showList;
  }
  whose.onclick = () => { showGame = !showGame; syncBar(); rebuild(true).then(refreshPanels); };
  copyButton.onclick = async () => {
    const r = await api('/api/project/summon/copy', { level, type, spell });
    if (!r.ok) { say(r.error, 'bad'); return; }
    mine = { file: r.file, def: r.def, steps: summonStepsOf(r.def) };
    showGame = false;
    say('copied into the mod: ' + r.file + ' - its steps are yours to change', 'good');
    // The lists show it now: the Summons marked, the Mod folder's Summons with it.
    if (typeof loadList === 'function' && (state.browse === 'summon' || state.browse === 'summons')) loadList();
    if (typeof projectState !== 'undefined' && projectState.project) { projectState.project.summons = (projectState.project.summons || 0) + 1; if (typeof drawProjectTree === 'function') drawProjectTree(); }
    syncBar(); await rebuild(true); refreshPanels();
  };
  removeButton.onclick = async () => {
    if (!mine || !confirm('Take the mod\'s steps for this outcome out (' + mine.file + ')? ' + (spell ? 'Its base\'s script plays for it.' : 'The game\'s play again.'))) return;
    await api('/api/project/summon/delete', { file: mine.file });
    mine = null; syncBar(); await rebuild(true); refreshPanels();
  };
  testButton.onclick = async () => {
    say('starting OpenFF to try ' + (entry ? entry.creature || entry.name : 'the summon') + '…');
    const r = await api('/api/project/test-spell', { spell: script.number || 4201 + level, school: 'Summon', outcome: SUMMON_OUTCOMES[type], formation: 1 }).catch(e => ({ ok: false, error: e.message }));
    if (!r.ok) { say(r.error, 'bad'); return; }
    say(`OpenFF is starting: a ${r.job} with the summon, then the battle - choose Summon and cast it`, 'good');
  };
  function refreshPanels() { drawHierarchy(); drawInspector(); drawSteps(); timeline.draw(); }
  // A change to the steps: saved half a second on, played again from the frame it was on.
  function changed(quiet = false) {
    mine.def.steps = summonDefSteps(mine.steps);
    clearTimeout(saving);
    saving = setTimeout(async () => {
      const r = await api('/api/project/summon/save', { file: mine.file, def: mine.def }).catch(e => ({ ok: false, error: e.message }));
      say(r.ok ? 'saved ' + mine.file : r.error, r.ok ? 'good' : 'bad');
    }, 500);
    // A number being dragged keeps its Inspector (redrawing it would drop the drag).
    rebuild(false).then(quiet ? () => { drawHierarchy(); drawSteps(); timeline.draw(); } : refreshPanels);
  }
  let quietly = 0;
  const changedQuietly = () => { clearTimeout(quietly); quietly = setTimeout(() => changed(true), 120); };

  // An effect of the mod's saved in the effect editor: played again as it is now.
  const effectSaved = e => {
    if (!canvas.isConnected) { window.removeEventListener('crystal-effect-saved', effectSaved); return; }
    const m = /defs\/effects\/([^/]+)\.json$/i.exec(String(e.detail || ''));
    if (m && forgetEffect('mod:' + m[1])) rebuild(false).then(refreshPanels);
  };
  window.addEventListener('crystal-effect-saved', effectSaved);
  function forgetEffect(key) {
    if (!defs.has(key)) return false;
    lengths.delete(defs.get(key));
    defs.delete(key);
    return true;
  }

  async function load(t) {
    type = t;
    facts.textContent = 'loading…';
    script = await api(`/api/summon?level=${level}&type=${type}` + (spell ? '&spell=' + encodeURIComponent(spell) : ''));
    if (!script.ok) { facts.textContent = script.error; return; }
    gameSteps = script.steps;
    mine = script.mine ? { file: script.mine.file, def: script.mine.def, steps: summonStepsOf(script.mine.def) } : null;
    showGame = false;
    if (doc) doc.selection = 'look';
    syncBar();
    await rebuild(true);
    refreshPanels();
  }

  /// The runner made again from the steps playing (the mod's or the game's), their effects fetched, the pass measured.
  async function rebuild(restart) {
    const was = runner ? Math.max(0, runner.state.step + 1) : 0;
    const steps = current();
    // The summon as these steps make it: their model, motions and monster (its size and hit point).
    const first = op => { const st = steps.find(s => s.op === op); return st ? Number(st.p[0]) || 0 : 0; };
    const pad = n => String(n).padStart(3, '0');
    const modelNo = first(8), motionNo = first(10), monsterNo = first(28);
    // A model of the mod's own (defs/models, f300 and on): its glTF, on its base's skeleton and motions.
    look.own = ownModels.find(m => m.number === modelNo) || null;
    look.model = look.own ? look.own.gltf : modelNo > 0 ? 'files/f' + pad(modelNo) + '.nmdp.lz' : null;
    look.skeleton = look.own ? 'files/' + look.own.base + '.nmdp.lz' : look.model;
    if (look.own) await ownFile(look.own.gltf);
    look.motions = motionNo >= 1000 ? 'b_f' + pad(motionNo - 1000) : motionNo > 0 ? 'b_sm' + pad(motionNo) : null;
    if (monsterNo !== look.monster) {
      look.monster = monsterNo;
      look.target = monsterNo === script.monster ? script.target : await api('/api/effect/target?monster=' + monsterNo).then(t => t && !t.error ? t : script.target).catch(() => script.target);
    }
    const playing_ = { ...script, steps, model: look.model, motions: look.motions, target: look.target };
    const wanted = new Set();
    for (const st of steps) if (st.op === 19 || st.op === 20 || st.op === 55) wanted.add(typeof st.p[0] === 'string' ? 'mod:' + st.p[0] : st.p[0] + '/' + st.p[1]);
    for (const sp of script.spells || []) if (sp.category > 0) wanted.add(sp.category + '/' + (sp.member > 0 ? sp.member : 1));
    await Promise.all([...wanted].filter(k => !defs.has(k)).map(async k => {
      if (k.startsWith('mod:')) {
        const r = await api(`/api/effect?name=${encodeURIComponent('defs/effects/' + k.slice(4) + '.json')}`).catch(() => null);
        if (r && r.effect) defs.set(k, r.effect);
        return;
      }
      const [c, m] = k.split('/').map(Number);
      const r = await api(`/api/effect/import?category=${c}&member=${m}`).catch(() => null);
      if (r && !r.error) defs.set(k, r.effect);
    }));
    // The motions' pack, read again when the model or its motions change.
    const packKey = look.skeleton + '|' + look.motions;
    if (restart || packKey !== look.packKey) {
      look.packKey = packKey;
      motionIndex = new Map();
      look.motionList = [];
      if (look.skeleton && look.motions) {
        const packs = await api(`/api/model/motions?name=${encodeURIComponent(look.skeleton)}`).catch(() => []);
        const pack = (packs || []).find(p => new RegExp('(^|/)' + look.motions + '\\.ncap', 'i').test(p.name));
        if (pack) { motionIndex.set('pack', pack.name); for (const mo of pack.motions) motionIndex.set(mo.id, mo.index); look.motionList = pack.motions; }
      }
    }
    runner = makeSummonRun(playing_, defs);
    // A pass through it all: its length, where each step first runs and where it is done, each effect's life.
    const probe = makeSummonRun(playing_, defs);
    firstAt = new Array(steps.length).fill(-1);
    endAt = new Array(steps.length).fill(-1);
    let n = 0;
    while (n < 3000 && !probe.done) {
      probe.step();
      const S = probe.state;
      for (let i = 0; i <= S.current && i < steps.length; i++) if (firstAt[i] < 0) firstAt[i] = n;
      for (let i = 0; i < S.i && i < steps.length; i++) if (endAt[i] < 0) endAt[i] = n;
      n++;
    }
    length = Math.max(1, n);
    spans = probe.state.effects.map(e => ({ by: e.by, from: e.born, to: e.born + effectLengthOf(defs.get(e.key)), key: e.key }));
    for (let i = 0; i < steps.length; i++) if (endAt[i] < 0 && firstAt[i] >= 0) endAt[i] = length;
    scrub.max = String(length);
    facts.textContent = `${script.summon} · ${outcomeName()} · ${steps.length} steps, ${length} frames (${(length / 30).toFixed(1)} s)`;
    runner.reset();
    if (restart) { owed = 0; playing = true; play.textContent = 'Pause'; }
    else { for (let i = 0; i < Math.min(was, length); i++) runner.step(); }
  }
  // An effect's length in steps, played through once.
  const lengths = new Map();
  function effectLengthOf(def) {
    if (!def) return 1;
    if (lengths.has(def)) return lengths.get(def);
    const p = makeEffectPlayer(def, { seed: 7 });
    let n = 0;
    while (n < 900 && !p.finished) { p.step(); n++; }
    lengths.set(def, n);
    return n;
  }

  // ---- the editor's actions, for the Hierarchy's menus and the Inspector's buttons
  const ed = {
    steps: () => current(),
    editable,
    firstAt: i => firstAt[i],
    select: (i, jump) => select(i, jump),
    insert(at) { mine.steps.splice(at, 0, { op: 40, name: 'FRAME_COUNT', p: [10, 0, 0, 0, 0, 0, 0], again: false }); if (doc) doc.selection = 'step:' + at; changed(); },
    duplicate(i) { mine.steps.splice(i + 1, 0, JSON.parse(JSON.stringify(mine.steps[i]))); if (doc) doc.selection = 'step:' + (i + 1); changed(); },
    move(a, b) { const [x] = mine.steps.splice(a, 1); mine.steps.splice(b, 0, x); if (doc) doc.selection = 'step:' + b; changed(); },
    remove(i) { mine.steps.splice(i, 1); if (doc) doc.selection = 'look'; changed(); }
  };
  if (doc) {
    doc.summonEditor = ed;
    doc.inspect = ref => ref === 'look' ? lookCard() : /^step:\d+$/.test(ref) ? stepCard(parseInt(ref.slice(5), 10)) : null;
    doc.onShow = () => {
      let stale = false;
      for (const key of [...defs.keys()]) if (key.startsWith('mod:')) stale = forgetEffect(key) || stale;
      if (stale && script) rebuild(false).then(refreshPanels); else refreshPanels();
    };
  }

  /// A game effect made the mod's: its copy in defs/effects, the step pointed at it, opened in the effect editor.
  async function makeMine(i) {
    const st = mine.steps[i];
    const creature = (entry && entry.creature || 'summon').toLowerCase();
    const r = await api('/api/project/effect/new', { id: `${creature}-${st.p[0]}-${st.p[1] || 1}`, category: st.p[0], member: st.p[1] || 1 });
    if (!r.ok) { say(r.error, 'bad'); return; }
    st.p[0] = shortName(r.name).replace(/\.json$/i, '');
    if (st.op !== 16) st.p[1] = 1;   // a mod's effect plays as its category's member 1
    changed();
    say('the effect is the mod\'s now: ' + r.name, 'good');
    openDoc('effect', r.name);
  }
  const openEffectOf = st => typeof st.p[0] === 'string' ? openDoc('effect', 'defs/effects/' + st.p[0] + '.json') : openDoc('effect', 'files/e' + String(st.p[0]).padStart(3, '0') + '.efp');

  // ---- the Inspector: the Look (the summon at a glance, swapped in place) and a step's editor
  function card(box, title, iconName) {
    const c = document.createElement('div');
    c.className = 'component effect-card';
    const h = document.createElement('div');
    h.className = 'component-head';
    h.append(icon(iconName || 'effect'));
    const b = document.createElement('b');
    b.textContent = title;
    h.append(b);
    c.append(h);
    box.append(c);
    return c;
  }
  const field = (c, labelText, control, title) => { const r = document.createElement('label'); r.className = 'behaviour-field'; if (title) r.title = title; const s = document.createElement('span'); s.textContent = labelText; r.append(s, control); c.append(r); return r; };
  const readOnlyNote = box => {
    if (editable()) return;
    const p = document.createElement('p');
    p.className = 'sub summon-note';
    p.textContent = 'The game\'s script: Copy into the mod (above the Stage) to change it.';
    box.append(p);
  };
  // A step's number put in, the view played again (the mod's steps only).
  const setParam = (i, k, v) => { if (!editable()) return; mine.steps[i].p[k] = v; changed(); };
  // The pickers by what a number names: each calls back with the value (an effect with its member too).
  const PICK = {
    e: (i, k) => { const st = current()[i]; pickSummonEffect(st.p[0], Number(st.p[1]) || 1, (v, m) => { const p = mine.steps[i].p; p[0] = v; if (st.op !== 16) p[1] = m; changed(); }, { packOnly: st.op === 16 }); },
    model: (i, k) => pickSummonModel(Number(current()[i].p[k]) || 0, v => typeof v === 'string' ? useOwnFile(v, i) : setParam(i, k, v), ownModels),
    motions: (i, k) => pickSummonMotionPack(Number(current()[i].p[k]) || 0, v => setParam(i, k, v)),
    motion: (i, k) => pickSummonMotion(Number(current()[i].p[k]) || 0, look, v => setParam(i, k, v)),
    monster: (i, k) => pickSummonMonster(Number(current()[i].p[k]) || 0, v => setParam(i, k, v)),
    spell: (i, k) => pickSummonSpell(Number(current()[i].p[k]) || 0, v => setParam(i, k, v))
  };
  const PICK_WHAT = { e: 'an effect', model: 'a model', motions: 'the motions', motion: 'a motion', monster: 'a monster', spell: 'a spell' };
  /// A step's number as a row: its label dragged across to change it (Shift big, Alt fine), typed, and - for what
  /// names something - a picker beside it.
  function numberRow(c, labelText, kind, i, k, title) {
    const st = current()[i];
    const row = document.createElement('div');
    row.className = 'summon-num';
    if (title) row.title = title;
    let control;
    if (kind === 'e') {
      // A pack's number or one of the mod's effects by its id.
      const lab = document.createElement('span');
      lab.className = 'summon-num-label';
      lab.textContent = labelText;
      control = Object.assign(document.createElement('input'), { type: 'text', value: String(st.p[k]) });
      control.title = 'a pack of the game\'s by its number (367), or one of the mod\'s effects by its id';
      control.onchange = () => { const v = control.value.trim(); setParam(i, k, /^-?\d+$/.test(v) ? parseInt(v, 10) : v); };
      row.append(lab, control);
    } else {
      const units = kind === 'u';
      const shown = units ? Math.round((Number(st.p[k]) || 0) / 4096 * 100) / 100 : Number(st.p[k]) || 0;
      control = scrubNumber(labelText, shown, v => {
        if (!editable()) return;
        mine.steps[i].p[k] = units ? Math.round(v * 4096) : Math.round(v);
        changedQuietly();
      }, { step: units ? 0.01 : 1 });
      const input = control.querySelector('input');
      input.disabled = !editable();
      if (!editable()) control.classList.add('off');
      if (kind === 'model') {
        const own = ownModels.find(m => m.number === Number(st.p[k]));
        if (own) { const said = document.createElement('i'); said.className = 'summon-num-said'; said.textContent = own.gltf.replace(/^assets\//, ''); control.append(said); }
      }
      if (kind === 'motion') {
        const m = (look.motionList || []).find(x => x.id === Number(st.p[k]));
        const said = document.createElement('i');
        said.className = 'summon-num-said';
        said.textContent = m ? m.name : look.motionList.length ? 'not in ' + look.motions : '';
        control.append(said);
      }
      row.append(control);
    }
    if (PICK[kind]) {
      const b = Object.assign(document.createElement('button'), { type: 'button', className: 'summon-pick', textContent: '\u25CE', title: 'pick ' + PICK_WHAT[kind] + ' by looking at it' });
      b.disabled = !editable() || (kind === 'motion' && !look.motionList.length);
      b.onclick = () => PICK[kind](i, k);
      row.append(b);
    }
    c.append(row);
    return row;
  }
  const findStep = op => current().findIndex(st => st.op === op);

  function lookCard() {
    const box = document.createElement('div');
    const head = document.createElement('div');
    head.className = 'object-head';
    head.append(icon('effect'));
    const title = document.createElement('input');
    title.className = 'object-name';
    title.readOnly = true;
    title.value = (entry ? (entry.creature || entry.name) : 'Summon') + ' · ' + (script ? outcomeName() : '');
    head.append(title);
    if (editable()) head.append(ownBadge('mod'));
    box.append(head);
    readOnlyNote(box);
    const steps = current();
    // The summon itself: its name in battle, its model, motions and monster (its size and hit point).
    const who = card(box, 'The summon', 'model');
    const named = document.createElement('input');
    named.type = 'text';
    named.value = editable() ? mine.def.name || '' : '';
    named.placeholder = spell && entry ? entry.name : script.name;
    named.disabled = !editable();
    named.onchange = () => {
      const v = named.value.trim();
      if (v) mine.def.name = v; else delete mine.def.name;
      changed();
      title.value = (entry ? (entry.creature || entry.name) : 'Summon') + ' · ' + outcomeName();
    };
    field(who, 'Name', named, 'what the battle shows as it is cast; empty for ' + (spell ? 'the summon\'s own name' : 'the game\'s'));
    const modelAt0 = findStep(8), motionAt0 = findStep(10), monsterAt0 = findStep(28);
    if (modelAt0 >= 0 && motionAt0 >= 0 && monsterAt0 >= 0) {
      // A monster to summon: its family's model and motions, and the monster itself for its size and hit point.
      const as = document.createElement('select');
      as.disabled = !editable();
      const none = document.createElement('option'); none.value = ''; none.textContent = 'none (its own)'; as.append(none);
      as.append(Object.assign(document.createElement('option'), { value: '?', textContent: 'loading the monsters…', disabled: true }));
      field(who, 'As a monster', as, 'a monster of the game\'s (or the mod\'s) as the summon: its model, its motions and its size - the script\'s motions 101 (standing) and 201 (its attack) are the monster\'s too');
      api('/api/monsters').then(r => {
        const list = (r && (r.monsters || r)) || [];
        as.querySelector('option[value="?"]').remove();
        const steps2 = current();
        const model = Number(steps2[modelAt0].p[0]), motions = Number(steps2[motionAt0].p[0]), monster = Number(steps2[monsterAt0].p[0]);
        for (const m of list.filter(m => m.family > 0 && m.family < 201).sort((a, b) => a.name.localeCompare(b.name))) {
          const o = document.createElement('option'); o.value = String(m.id); o.textContent = `${m.name}  (f${String(m.family).padStart(3, '0')})`; o.dataset.family = String(m.family); as.append(o);
        }
        const now = list.find(m => m.id === monster && m.family === model && motions === 1000 + model);
        as.value = now ? String(now.id) : '';
      }).catch(() => {});
      as.onchange = () => {
        const o = as.selectedOptions[0];
        const st = mine.steps;
        if (!as.value) {
          // Back to the base's own: its model, motions and monster as the game's script has them.
          for (const [at, op] of [[modelAt0, 8], [motionAt0, 10], [monsterAt0, 28]]) { const g = (gameSteps || []).find(s => s.op === op); if (g) st[at].p[0] = g.p[0]; }
        } else {
          const family = parseInt(o.dataset.family, 10);
          st[modelAt0].p[0] = family;
          st[motionAt0].p[0] = 1000 + family;
          st[monsterAt0].p[0] = parseInt(as.value, 10);
        }
        changed();
      };
    }
    const modelAt = findStep(8), motionAt = findStep(10), monsterAt = findStep(28);
    if (modelAt >= 0) numberRow(who, 'Model', 'model', modelAt, 0, 'the model it loads (SET_MODEL f###): a summon\'s own f201-f208, or any of the game\'s');
    if (motionAt >= 0) numberRow(who, 'Motions', 'motions', motionAt, 0, 'the motions it plays (SET_MOTION): a summon\'s b_sm###, or a monster family\'s b_f### as 1000 + the family');
    if (monsterAt >= 0) numberRow(who, 'Monster', 'monster', monsterAt, 0, 'the monster record it takes its size and hit point from (SET_SUMMON_PARAMETER)');
    const own = modelAt >= 0 ? ownModels.find(m => m.number === Number(steps[modelAt].p[0])) : null;
    if (own) ownCard(box, own);
    // The caster's glow as it begins: the summon spell's cast (its look, defs/spells).
    // A summon's glow as it is cast (what plays after is the script's).
    if (typeof effectItemLook === 'function' && entry) effectItemLook(box, () => entry.name, { castOnly: true });
    // Every effect it plays: a picker, Open, and Make it mine.
    const fx = card(box, 'Its effects', 'effect');
    const known = document.createElement('datalist');
    known.id = 'summon-own-effects';
    api('/api/effects').then(l => { for (const e of l || []) if (e.own) { const o = document.createElement('option'); o.value = shortName(e.name).replace(/\.json$/i, ''); known.append(o); } }).catch(() => {});
    fx.append(known);
    steps.forEach((st, i) => {
      if (![19, 20, 55].includes(st.op)) return;
      const where = st.op === 19 ? 'at the summon' : st.op === 20 ? (st.p[2] ? 'over the party' : 'over the enemies') : 'at a point';
      const row = document.createElement('div');
      row.className = 'summon-effect-row';
      const name = document.createElement('span');
      name.textContent = `${i} · ${where}${firstAt[i] >= 0 ? ' · f' + firstAt[i] : ''}`;
      name.className = 'summon-effect-where';
      name.onclick = () => select(i);
      const input = document.createElement('input');
      input.type = 'text';
      input.value = typeof st.p[0] === 'string' ? st.p[0] : st.p[0] + '/' + (st.p[1] || 1);
      input.title = 'a pack and member of the game\'s (367/1), or one of the mod\'s effects by its id';
      input.setAttribute('list', 'summon-own-effects');
      input.disabled = !editable();
      input.onchange = () => {
        const v = input.value.trim(), m = /^(\d+)(?:\/(\d+))?$/.exec(v);
        if (m) { mine.steps[i].p[0] = parseInt(m[1], 10); mine.steps[i].p[1] = parseInt(m[2] || '1', 10); }
        else { mine.steps[i].p[0] = v; mine.steps[i].p[1] = 1; }
        changed();
      };
      const open = document.createElement('button');
      open.className = 'chip';
      open.textContent = 'Open';
      open.title = 'open the effect: the mod\'s in the effect editor, the game\'s in the Effects library';
      open.onclick = () => openEffectOf(st);
      const pick = Object.assign(document.createElement('button'), { type: 'button', className: 'summon-pick', textContent: '\u25CE', title: 'pick an effect by looking at it', disabled: !editable() });
      pick.onclick = () => PICK.e(i, 0);
      row.append(name, input, pick, open);
      if (editable() && typeof st.p[0] === 'number') {
        const mineB = document.createElement('button');
        mineB.className = 'chip';
        mineB.textContent = 'Make it mine';
        mineB.title = 'a copy of this effect in the mod (defs/effects), this step pointed at it, opened to change';
        mineB.onclick = () => makeMine(i);
        row.append(mineB);
      }
      fx.append(row);
    });
    // The outcome's effect on each target: a spell's (its look is what plays).
    steps.forEach((st, i) => {
      if (st.op !== 52) return;
      const c = card(box, 'On each target', 'monster');
      const sp = (script.spells || []).find(x => x.id === st.p[0]);
      const input = document.createElement('input');
      input.type = 'number'; input.step = '1'; input.value = String(st.p[0]); input.disabled = !editable();
      input.onchange = () => setParam(i, 0, parseInt(input.value, 10) || 0);
      const spellRow = field(c, 'Spell', input, 'the spell whose effect plays on each target, half its play frame apart (DRAW_TARGET_EFFECT) - a spell of the mod\'s works too');
      const pick = Object.assign(document.createElement('button'), { type: 'button', className: 'summon-pick', textContent: '\u25CE', title: 'pick a spell by looking at its effect', disabled: !editable() });
      pick.onclick = e => { e.preventDefault(); PICK.spell(i, 0); };
      spellRow.append(pick);
      const note = document.createElement('p');
      note.className = 'sub summon-note';
      note.textContent = (sp ? `${sp.name || 'spell ' + sp.id}: effect ${sp.category}/${sp.member}, on ${sp.party ? 'the party' : 'the enemies'}. ` : '') + 'Its look is what plays: give it one in Spells (or a spell of the mod\'s made for this summon), and this outcome shows it.';
      c.append(note);
      if (st.p[0] < 20000) {
        const open = document.createElement('button');
        open.className = 'chip';
        open.textContent = 'Open the spell';
        open.onclick = async () => { await selectKind('spells'); inspectAsset('spells', 'game:' + st.p[0]); };
        c.append(open);
      }
    });
    return box;
  }

  /// A model of the mod's own in the Look: its file, the skeleton it borrows, its size and turn, and which of its
  /// clips plays for each of the motions its pack has (none: the base's motion drives it, retargeted).
  function ownCard(box, own) {
    const c = card(box, 'Its glTF', 'model');
    const head = c.querySelector('.component-head');
    if (head) head.append(ownBadge('mod', { dot: true, title: 'defs/models/' + own.model + '.json' }));
    const row = (labelText, control, title) => { const r = document.createElement('div'); r.className = 'summon-num'; if (title) r.title = title; const l = document.createElement('span'); l.className = 'summon-num-label'; l.textContent = labelText; r.append(l, control); c.append(r); return r; };
    const pickButton = (title, run) => { const b = Object.assign(document.createElement('button'), { type: 'button', className: 'summon-pick', textContent: '\u25CE', title, disabled: !editable() }); b.onclick = run; return b; };
    const fileInput = Object.assign(document.createElement('input'), { type: 'text', value: own.gltf, disabled: !editable() });
    fileInput.onchange = () => saveOwn(own, { gltf: fileInput.value.trim() });
    row('File', fileInput, 'the glTF it is drawn as (the project\'s assets/)').append(pickButton('pick one of the project\'s model files', () => {
      const go = () => pickModel(own.gltf, v => saveOwn(own, { gltf: v }), { assetsOnly: true, title: 'Its glTF', what: 'model files of the project\'s' });
      if (Array.isArray(state.assets)) go(); else api('/api/project/assets').then(l => { state.assets = l || []; go(); }).catch(() => { state.assets = []; go(); });
    }));
    const baseInput = Object.assign(document.createElement('input'), { type: 'text', value: own.base, disabled: !editable() });
    baseInput.onchange = () => saveOwn(own, { base: baseInput.value.trim() });
    row('Skeleton', baseInput, 'the game\'s model whose skeleton and motions it borrows (the game loads its files in this one\'s place)').append(pickButton('pick the model it borrows from', () =>
      pickSummonModel(parseInt(own.base.replace(/\D/g, ''), 10) || 0, v => { if (typeof v === 'number') saveOwn(own, { base: 'f' + String(v).padStart(3, '0') }); })));
    const file = ownFiles.get(own.gltf);
    const fit = ownFit(own, file);
    const scale = scrubNumber('Scale', own.scale > 0 ? Math.round(own.scale * 1000) / 1000 : 0, v => { if (editable()) saveOwn(own, { scale: Math.max(0, v) }, true); }, { step: 0.01, min: 0, title: '0 fits it to the skeleton\'s height, as the game does' });
    const scaleRow = document.createElement('div'); scaleRow.className = 'summon-num'; scaleRow.append(scale);
    if (!(own.scale > 0)) { const said = document.createElement('i'); said.className = 'summon-num-said'; said.textContent = 'auto, about ' + fit.auto.toFixed(2); scale.append(said); }
    c.append(scaleRow);
    const turn = scrubNumber('Turn (°)', (own.rotation && own.rotation[1]) || 0, v => { if (editable()) saveOwn(own, { rotation: [(own.rotation && own.rotation[0]) || 0, v, (own.rotation && own.rotation[2]) || 0] }, true); }, { step: 1, title: 'turned about its up axis, when it faces the wrong way' });
    const turnRow = document.createElement('div'); turnRow.className = 'summon-num'; turnRow.append(turn); c.append(turnRow);
    // Its clips for the motions its skeleton's pack has.
    const names = file && file.rest ? file.rest.clips || [] : [];
    const clipsHead = document.createElement('p');
    clipsHead.className = 'sub summon-note';
    clipsHead.textContent = names.length ? 'Its own clips in place of the motions (none: the skeleton\'s motion moves it):' : 'The file has no clips of its own: the skeleton\'s motions move it.';
    c.append(clipsHead);
    if (names.length) for (const m of look.motionList || []) {
      const pick = document.createElement('select');
      pick.disabled = !editable();
      for (const [v, t] of [['', '(the skeleton\'s)'], ...names.map(n => [n, n])]) { const o = document.createElement('option'); o.value = v; o.textContent = t; pick.append(o); }
      pick.value = (own.clips || {})[String(m.id)] || '';
      pick.onchange = () => { const clips = { ...(own.clips || {}) }; if (pick.value) clips[String(m.id)] = pick.value; else delete clips[String(m.id)]; saveOwn(own, { clips }); };
      row(`${m.id}  ${m.name}`, pick, `what plays when the script starts motion ${m.id} (${m.frames || '?'} frames)`);
    }
  }

  function stepCard(i) {
    const steps = current();
    const st = steps[i];
    const box = document.createElement('div');
    if (!st) return box;
    const head = document.createElement('div');
    head.className = 'object-head';
    head.append(icon(['image', 'model', 'effect', 'map', 'character', 'scene', 'audio'][summonLaneOf(st.op)]));
    const title = document.createElement('input');
    title.className = 'object-name';
    title.readOnly = true;
    title.value = `Step ${i} · ${summonWords(st.name)}`;
    head.append(title);
    box.append(head);
    const sub = document.createElement('p');
    sub.className = 'sub summon-note';
    sub.textContent = (summonStepText(st) || '') + (firstAt[i] >= 0 ? ` · frames ${firstAt[i]}-${Math.max(firstAt[i], endAt[i])}` : ' · not reached');
    box.append(sub);
    readOnlyNote(box);
    const c = card(box, 'The command', 'scene');
    const cmd = document.createElement('select');
    for (const [group, ops] of SUMMON_GROUPS) {
      const g = document.createElement('optgroup'); g.label = group;
      for (const op of ops) { const o = document.createElement('option'); o.value = String(op); o.textContent = summonWords(SUMMON_COMMANDS[op]); g.append(o); }
      cmd.append(g);
    }
    cmd.value = String(st.op);
    cmd.disabled = !editable();
    cmd.onchange = () => { mine.steps[i].op = parseInt(cmd.value, 10); mine.steps[i].name = SUMMON_COMMANDS[mine.steps[i].op]; changed(); };
    field(c, 'Command', cmd);
    const againBox = document.createElement('input');
    againBox.type = 'checkbox'; againBox.checked = !!st.again; againBox.disabled = !editable();
    againBox.onchange = () => { mine.steps[i].again = againBox.checked; changed(); };
    field(c, 'Same step', againBox, 'runs in the same game step as the one before, once that one is done (else it waits for the next step)');
    const params = SUMMON_PARAMS[st.op] || [];
    if (params.length) {
      const pc = card(box, 'Its numbers', 'table');
      params.forEach(([labelText, kind], k) => {
        if (kind === 'b') {
          const input = document.createElement('input'); input.type = 'checkbox'; input.checked = !!st.p[k];
          input.onchange = () => setParam(i, k, input.checked ? 1 : 0);
          input.disabled = !editable();
          field(pc, labelText, input);
        } else numberRow(pc, labelText, kind, i, k);
      });
      if ([19, 20, 55, 16].includes(st.op)) {
        const row = document.createElement('div');
        row.className = 'button-row';
        const open = document.createElement('button'); open.className = 'chip'; open.textContent = 'Open the effect'; open.onclick = () => openEffectOf(st);
        row.append(open);
        if (editable() && st.op !== 16 && typeof st.p[0] === 'number') { const m = document.createElement('button'); m.className = 'chip'; m.textContent = 'Make it mine'; m.onclick = () => makeMine(i); row.append(m); }
        pc.append(row);
      }
      if ([22, 23, 24].includes(st.op) && editable()) {
        const take = document.createElement('button');
        take.className = 'wide-button';
        take.textContent = st.op === 22 ? 'Set from camera' : st.op === 23 ? 'Set "eye to" from camera' : 'Set "target to" from camera';
        take.title = 'turn on Free camera under the Stage, frame the shot, then this';
        take.onclick = () => {
          const v = stage.freeView(), fx = a => a.map(x => Math.round(x * 4096));
          const p = mine.steps[i].p;
          if (st.op === 22) p.splice(0, 6, ...fx(v.eye), ...fx(v.at));
          else if (st.op === 23) { const e = fx(v.eye); p[3] = e[0]; p[4] = e[1]; p[5] = e[2]; }
          else { const a = fx(v.at); p[3] = a[0]; p[4] = a[1]; p[5] = a[2]; }
          changed();
        };
        pc.append(take);
      }
    }
    if (editable()) {
      const row = document.createElement('div');
      row.className = 'button-row summon-step-tools';
      const tool = (text, run, off) => { const b = document.createElement('button'); b.className = 'chip'; b.textContent = text; b.disabled = !!off; b.onclick = run; row.append(b); };
      tool('+ step after', () => ed.insert(i + 1));
      tool('Duplicate', () => ed.duplicate(i));
      tool('↑', () => ed.move(i, i - 1), i === 0);
      tool('↓', () => ed.move(i, i + 1), i === steps.length - 1);
      tool('Delete', () => ed.remove(i));
      box.append(row);
    }
    return box;
  }

  // ---- the script list (a panel beside the Stage, off to start with)
  function drawSteps() {
    stepsBox.textContent = '';
    if (!summonState.showList || !script) return;
    const head = document.createElement('div');
    head.className = 'summon-steps-head';
    head.textContent = editable() ? 'The mod\'s steps' : 'The game\'s script';
    if (editable()) head.append(ownBadge('mod'));
    stepsBox.append(head);
    const picked = selectedIndex();
    current().forEach((st, i) => {
      const row = document.createElement('div');
      row.className = 'summon-step' + (st.again ? ' again' : '') + (i === picked ? ' picked' : '');
      row.dataset.index = String(i);
      const n = document.createElement('i'); n.textContent = String(i);
      const op = document.createElement('b'); op.textContent = summonWords(st.name);
      const words = document.createElement('span'); words.textContent = summonStepText(st) || (st.p.some(v => v) ? st.p.slice(0, 6).join(', ') : '');
      row.append(n, op, words);
      row.onclick = () => select(i);
      stepsBox.append(row);
    });
  }

  // ---- the timeline: a lane a group, a bar a step from where it starts to where it is done, effects' lives under
  const timeline = (() => {
    const canvas2 = document.createElement('canvas');
    canvas2.className = 'effect-timeline';
    timelineBox.append(canvas2);
    const LABEL = 150, ROW = 18, RULER = 16;
    let W = 600, drag = null, hoverI = -1;
    const lanes = () => SUMMON_LANES.length;
    const xOf = f => LABEL + (W - LABEL - 8) * f / Math.max(1, length);
    const frameOf = x => Math.max(0, Math.round((x - LABEL) / Math.max(1, W - LABEL - 8) * length));
    const barOf = i => {
      const a = firstAt[i], b = Math.max(a, endAt[i]);
      return { a, b, lane: summonLaneOf(current()[i].op) };
    };
    function draw() {
      if (!script) return;
      W = Math.max(300, timelineBox.clientWidth || 600);
      const H = RULER + ROW * lanes() + ROW + 4;
      const g = effectSharp(canvas2, W, H);
      g.fillStyle = '#14161a'; g.fillRect(0, 0, W, H);
      g.font = '11px system-ui, "Segoe UI", sans-serif';
      g.textBaseline = 'middle';
      for (let f = 0; f <= length; f += 10) {
        const x = Math.round(xOf(f));
        g.fillStyle = f % 30 === 0 ? '#8b93a1' : '#3a3f47';
        g.fillRect(x, 0, 1, f % 30 === 0 ? RULER : 5);
        if (f % 60 === 0) { g.fillStyle = '#8b93a1'; g.fillText(String(f), x + 3, 8); }
      }
      SUMMON_LANES.forEach(([name], l) => {
        const y = RULER + l * ROW;
        g.fillStyle = l % 2 ? '#181b20' : '#15181c';
        g.fillRect(0, y, W, ROW);
        g.fillStyle = '#d7dbe2';
        g.fillText(name, 6, y + ROW / 2);
      });
      const yLives = RULER + lanes() * ROW;
      g.fillStyle = '#121418'; g.fillRect(0, yLives, W, ROW);
      g.fillStyle = '#8b93a1'; g.fillText('effects\' lives', 6, yLives + ROW / 2);
      const picked = selectedIndex();
      current().forEach((st, i) => {
        const { a, b, lane } = barOf(i);
        if (a < 0) return;
        const y = RULER + lane * ROW;
        const colour = SUMMON_LANES[lane][1];
        const x0 = Math.round(xOf(a)), x1 = Math.round(xOf(b));
        g.fillStyle = colour;
        g.globalAlpha = i === picked ? 1 : i === hoverI ? 0.9 : 0.7;
        if (x1 - x0 < 3) { g.beginPath(); g.moveTo(x0, y + 3); g.lineTo(x0 + 4, y + ROW / 2); g.lineTo(x0, y + ROW - 3); g.lineTo(x0 - 4, y + ROW / 2); g.closePath(); g.fill(); }
        else g.fillRect(x0, y + 4, x1 - x0, ROW - 8);
        g.globalAlpha = 1;
        if (i === picked) { g.strokeStyle = '#ffffff'; g.lineWidth = 1; g.strokeRect(x0 - 4.5, y + 2.5, Math.max(8, x1 - x0 + 9), ROW - 5); }
      });
      // The effects the steps made, how long each lives.
      for (const s of spans) {
        g.fillStyle = s.by === picked ? '#e0b86a' : 'rgba(224, 184, 106, 0.45)';
        g.fillRect(Math.round(xOf(s.from)), yLives + 5, Math.max(2, Math.round(xOf(s.to) - xOf(s.from))), ROW - 10);
      }
      const S = runner && runner.state;
      if (S) { g.fillStyle = '#e07a7a'; g.fillRect(Math.round(xOf(Math.max(0, S.step))), 0, 2, H); }
    }
    function hit(x, y) {
      const lane = Math.floor((y - RULER) / ROW);
      if (lane < 0 || lane >= lanes() || x < LABEL) return null;
      let best = null, near = 7;
      current().forEach((st, i) => {
        const bar = barOf(i);
        if (bar.a < 0 || bar.lane !== lane) return;
        const x0 = xOf(bar.a), x1 = xOf(bar.b);
        const d = x < x0 ? x0 - x : x > x1 ? x - x1 : 0;
        const edge = Math.abs(x - x1) <= 5 && x1 - x0 >= 3;
        if (d <= near) { near = d; best = { i, edge }; }
      });
      return best;
    }
    // A wait's end dragged: its setter's frames (or its own count) grow or shrink by the frames it moved.
    function timedParam(i) {
      const st = current()[i];
      if (SUMMON_SELF_TIMED[st.op] !== undefined) return { at: i, k: SUMMON_SELF_TIMED[st.op] };
      const w = SUMMON_WAITS[st.op];
      if (!w) return null;
      for (let j = i - 1; j >= 0; j--) if (current()[j].op === w[0]) return { at: j, k: w[1] };
      return null;
    }
    canvas2.addEventListener('pointerdown', e => {
      const h = hit(e.offsetX, e.offsetY);
      if (!h) { if (e.offsetX >= LABEL) goTo(frameOf(e.offsetX)); return; }
      const t = h.edge && editable() ? timedParam(h.i) : null;
      if (t) { drag = { ...t, i: h.i, x: e.offsetX, base: Number(mine.steps[t.at].p[t.k]) || 0, perFrame: (W - LABEL - 8) / Math.max(1, length) }; canvas2.setPointerCapture(e.pointerId); return; }
      select(h.i);
    });
    canvas2.addEventListener('pointermove', e => {
      if (drag) {
        const frames = Math.round((e.offsetX - drag.x) / drag.perFrame);
        const v = Math.max(0, drag.base + frames);
        if (mine.steps[drag.at].p[drag.k] !== v) { mine.steps[drag.at].p[drag.k] = v; canvas2.title = `${v} frames`; }
        return;
      }
      const h = hit(e.offsetX, e.offsetY);
      canvas2.style.cursor = h ? (h.edge && editable() && timedParam(h.i) ? 'ew-resize' : 'pointer') : 'default';
      canvas2.title = h ? `${h.i} ${summonWords(current()[h.i].name)}: ${summonStepText(current()[h.i])}` + (h.edge && editable() && timedParam(h.i) ? ' - drag to change how long' : '') : '';
      const was = hoverI; hoverI = h ? h.i : -1; if (was !== hoverI) draw();
    });
    canvas2.addEventListener('pointerup', () => { if (drag) { drag = null; changed(); } });
    let width = 0;
    new ResizeObserver(() => requestAnimationFrame(() => { if (timelineBox.clientWidth !== width) { width = timelineBox.clientWidth; draw(); } })).observe(timelineBox);
    return { draw };
  })();

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

  let shownIndex = -1, lastDrawnStep = -1;
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
      stage.override = freeCam ? null : { eye: S.eye.map(v => v / 4096), at: S.at.map(v => v / 4096) };
      stage.mapTint = S.dark.on ? S.dark.colour : null;
      stage.hide = { monsters: S.monstersHidden, party: S.party.registered ? S.party.alpha : 0 };
      const sm = S.summon;
      stage.extras = sm.loaded && !sm.gone && sm.shown && look.model ? [look.own ? ownExtra(sm) : {
        model: look.model, pos: sm.pos.map(v => v / 4096), yaw: sm.yaw, scale: sm.scale, alpha: sm.alpha / 100,
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
      if (S.step !== lastDrawnStep) { lastDrawnStep = S.step; timeline.draw(); }
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
  syncBar();
  await load(0);
  requestAnimationFrame(frameLoop);
}


// ---------------------------------------------------------------- pickers
//
// A step's numbers that name something - an effect, a model, its motions, a motion, a monster, a spell - are picked
// by looking at it: a list with a filter, and beside it a small Stage playing the one under the pointer (the effect
// looping, the model in its motion). The number stays editable in its field; the picker is the button beside it.

/// A list to pick from, with a preview: items [{ value, label, note, group, search }], `preview(item, pane)` shows one
/// (and may return a function that stops it). Enter or a double click picks, Escape closes.
function summonPickList({ title, items, current, onChosen, preview = null, what = 'to pick from', extra = null }) {
  const veil = document.createElement('div');
  veil.className = 'picker-veil';
  const box = document.createElement('div');
  box.className = 'picker summon-picker';
  const head = document.createElement('div');
  head.className = 'picker-head';
  const strong = document.createElement('strong');
  strong.textContent = title;
  const filter = Object.assign(document.createElement('input'), { type: 'search', placeholder: 'filter', autocomplete: 'off' });
  const shut = Object.assign(document.createElement('button'), { className: 'shut', textContent: '×', title: 'close' });
  head.append(strong, filter, shut);
  const body = document.createElement('div');
  body.className = 'summon-picker-body';
  const list = document.createElement('div');
  list.className = 'summon-picker-list';
  const pane = document.createElement('div');
  pane.className = 'summon-picker-pane';
  body.append(list);
  if (preview) body.append(pane);
  const foot = document.createElement('div');
  foot.className = 'picker-foot summon-picker-foot';
  const count = document.createElement('span');
  const use = Object.assign(document.createElement('button'), { className: 'primary', textContent: 'Use' });
  foot.append(count, use);
  box.append(head, body, foot);
  veil.append(box);
  document.body.append(veil);

  let picked = items.find(it => String(it.value) === String(current)) || null;
  let stopPreview = null;
  const show = it => {
    if (!preview) return;
    if (stopPreview) { try { stopPreview(); } catch (e) { } stopPreview = null; }
    pane.textContent = '';
    if (it) stopPreview = preview(it, pane) || null;
  };
  const choose = it => { if (!it) return; close(); onChosen(it.value, it); };
  const draw = () => {
    const wanted = filter.value.trim().toLowerCase();
    list.textContent = '';
    let shown = 0, group = null;
    for (const it of items) {
      const text = (it.search || (it.label + ' ' + (it.note || '') + ' ' + it.value)).toLowerCase();
      if (wanted && !text.includes(wanted)) continue;
      if (it.group && it.group !== group) {
        group = it.group;
        const g = document.createElement('div');
        g.className = 'summon-picker-group';
        g.textContent = group;
        list.append(g);
      }
      shown++;
      const row = document.createElement('button');
      row.type = 'button';
      row.className = 'summon-picker-row' + (it === picked ? ' on' : '') + (String(it.value) === String(current) ? ' current' : '');
      const b = document.createElement('b'); b.textContent = it.label;
      row.append(b);
      if (it.badge) row.append(it.badge());
      if (it.note) { const n = document.createElement('i'); n.textContent = it.note; row.append(n); }
      row.onclick = () => { picked = it; for (const r of list.querySelectorAll('.on')) r.classList.remove('on'); row.classList.add('on'); show(it); use.disabled = false; };
      row.ondblclick = () => choose(it);
      list.append(row);
    }
    count.textContent = shown === items.length ? `${items.length} ${what}` : `${shown} of ${items.length}`;
  };
  const onKey = e => {
    if (e.key === 'Escape') close();
    else if (e.key === 'Enter' && picked) choose(picked);
  };
  function close() {
    if (stopPreview) { try { stopPreview(); } catch (e) { } }
    window.removeEventListener('keydown', onKey);
    veil.remove();
  }
  shut.onclick = close;
  veil.onclick = e => { if (e.target === veil) close(); };
  use.onclick = () => choose(picked);
  use.disabled = !picked;
  filter.oninput = draw;
  window.addEventListener('keydown', onKey);
  if (extra) extra(head, { close, redraw: draw, items });
  draw();
  show(picked);
  const on = list.querySelector('.on');
  if (on) on.scrollIntoView({ block: 'center' });
  filter.focus();
  return { close };
}

/// A small Stage in a picker's pane: an effect looping, or a model in a motion. Returns { stage, effect(def, own),
/// model(spec), label(text), stop }.
function summonPreviewStage(pane) {
  const canvas = document.createElement('canvas');
  canvas.className = 'summon-picker-stage';
  const caption = document.createElement('p');
  caption.className = 'summon-picker-caption';
  pane.append(canvas, caption);
  let own = null;
  const stage = makeEffectStage(canvas, key => effectTextureUrl(key, own));
  if (!stage) { caption.textContent = 'this browser has no WebGL'; return { effect() { }, model() { }, label() { }, stop() { }, pane }; }
  stage.setLayout({ count: 'one', background: '', view: 'free', show: { map: false, monsters: false, heroes: 'none', floor: true, marks: false } });
  let player = null, model = null, running = true, last = performance.now(), owed = 0, frame = 0;
  const loop = now => {
    if (!running || !canvas.isConnected) return;
    owed += Math.min(0.25, (now - last) / 1000) * 30;
    last = now;
    while (owed >= 1) { owed -= 1; frame++; if (player) { if (player.finished) player.reset(); player.step(); } }
    if (model) stage.extras = [{ ...model, frame: model.frames ? frame % model.frames : frame }];
    else stage.extras = [];
    stage.draw(player ? player.particles() : [], Math.max(0, Math.min(1, owed)), player ? player.models() : []);
    requestAnimationFrame(loop);
  };
  requestAnimationFrame(loop);
  return {
    stage, pane,
    label(text) { caption.textContent = text; },
    effect(def, ownName) {
      own = ownName || null;
      model = null;
      player = def ? makeEffectGroup(def, stage.targets, { seed: 7, anchors: { caster: stage.caster().anchor } }) : null;
      const at = stage.targets[0], caster = stage.caster().anchor;
      const both = def && (def.tracks || []).some(t => t.anchor === 'caster' || t.anchor === 'between' || (t.path && (t.path.from || t.path.to)));
      Object.assign(stage.camera, both ? { target: at.map((v, k) => (v + caster[k]) / 2), distance: 95, yaw: 20, pitch: 18 } : { target: at, distance: 64, yaw: 60, pitch: 14 });
    },
    model(spec) {
      player = null;
      frame = 0;
      const feet = (stage.places().monsters[0] || { feet: [0, 0, 0] }).feet;
      model = { pos: feet, yaw: spec.yaw === undefined ? 90 : spec.yaw, scale: spec.scale || 1, alpha: 1, loop: true, ...spec };
      Object.assign(stage.camera, { target: [feet[0], feet[1] + (spec.height || 6), feet[2]], distance: spec.distance || 55, yaw: 50, pitch: 10 });
    },
    stop() { running = false; }
  };
}

// The lists the pickers draw from, read once.
const summonPickData = {};
async function summonPickMonsters() {
  if (!summonPickData.monsters) { const r = await api('/api/monsters').catch(() => null); summonPickData.monsters = (r && (r.monsters || r)) || []; }
  return summonPickData.monsters;
}
// A family's name: the first of its monsters by id ("Dragon" for f075).
async function summonFamilyNames() {
  const names = new Map();
  for (const m of (await summonPickMonsters()).slice().sort((a, b) => a.id - b.id)) if (m.family > 0 && !names.has(m.family)) names.set(m.family, m.name);
  return names;
}
const summonPad = n => String(n).padStart(3, '0');
// The model a motion pack moves: a summon's (b_sm002 -> f202) or a family's (b_f075 -> f075).
const summonPackModel = motions => motions >= 1000 ? 'files/f' + summonPad(motions - 1000) + '.nmdp.lz' : 'files/f' + summonPad(200 + motions) + '.nmdp.lz';
const summonPackName = motions => motions >= 1000 ? 'files/b_f' + summonPad(motions - 1000) + '.ncap.lz' : 'files/b_sm' + summonPad(motions) + '.ncap.lz';
// A pack's motions, read once a pack.
async function summonPackMotions(model, pack) {
  const key = model + '|' + pack;
  summonPickData.packs = summonPickData.packs || new Map();
  if (!summonPickData.packs.has(key)) {
    const packs = await api(`/api/model/motions?name=${encodeURIComponent(model)}`).catch(() => []);
    const found = (packs || []).find(p => p.name.toLowerCase() === pack.toLowerCase());
    summonPickData.packs.set(key, found ? found.motions : []);
  }
  return summonPickData.packs.get(key);
}
// A monster's size and hit point (its battle scale, the height the camera looks at), read once a monster.
async function summonPickTarget(monster) {
  summonPickData.targets = summonPickData.targets || new Map();
  if (!(monster > 0)) return null;
  if (!summonPickData.targets.has(monster)) summonPickData.targets.set(monster, api('/api/effect/target?monster=' + monster).then(t => t && !t.error ? t : null).catch(() => null));
  return summonPickData.targets.get(monster);
}
// The monster that gives a model its size: a summon's own (f201...) or the first of a family's.
async function summonMonsterOfModel(modelNo) {
  if (modelNo >= 201 && modelNo <= 208) {
    if (!summonPickData.summons) { const r = await api('/api/summons').catch(() => null); summonPickData.summons = (r && r.summons) || []; }
    const s = summonPickData.summons.find(x => x.level === modelNo - 201 && !x.spell);
    return s ? s.monster : 0;
  }
  const m = (await summonPickMonsters()).filter(x => x.family === modelNo).sort((a, b) => a.id - b.id)[0];
  return m ? m.id : 0;
}
// A model on the preview Stage in a pack's motion (its first when none is named), at its battle size and framed by
// its monster's hit point.
async function summonPreviewModel(view, model, pack, motionId, words, monster = null) {
  view.label(words + ' · loading');
  const modelNo = parseInt(String(model).replace(/^.*\/f|\D+$/g, ''), 10) || 0;
  const target = typeof monster === 'object' && monster ? monster : await summonPickTarget(monster || await summonMonsterOfModel(modelNo));
  const motions = pack ? await summonPackMotions(model, pack) : [];
  const m = motions.find(x => x.id === motionId) || motions[0];
  // Framed by its hit point: how high it stands (up) and how deep it is (toward), at its battle scale.
  const up = target && target.up > 0 ? target.up : 6, toward = target && target.toward > 0 ? target.toward : 9;
  view.model({ model, pack: m ? pack : null, index: m ? m.index : 0, frames: m ? m.frames : 0, scale: target ? target.scale : 1, height: up, distance: Math.max(30, (up * 2 + toward) * 2.2) });
  view.label(words + (m ? ` · ${m.id} ${m.name}${m.frames ? ', ' + m.frames + ' frames' : ''}` : ''));
}

/// An effect: one of the mod's (its id) or a pack of the game's and its member. `packOnly` for SET_EFFECT (a pack to
/// load). Calls back (value, member).
async function pickSummonEffect(current, member, onChosen, { packOnly = false } = {}) {
  const all = await api('/api/effects').catch(() => []);
  const items = [];
  for (const e of all || []) {
    if (e.own) {
      if (packOnly) continue;
      const id = shortName(e.name).replace(/\.json$/i, '');
      items.push({ value: id, label: id, note: 'defs/effects', group: 'the mod\'s', own: e.name, badge: () => ownBadge('mod', { dot: true }) });
    } else if (e.category > 0) {
      items.push({ value: e.category, label: 'e' + summonPad(e.category), note: e.note || '', group: 'the game\'s', pack: e.name, templates: e.templates });
    }
  }
  let chosenMember = member || 1;
  summonPickList({
    title: packOnly ? 'An effect pack to load' : 'An effect', items, current, what: 'effects',
    onChosen: (value, it) => onChosen(value, it.own ? 1 : chosenMember),
    preview: (it, pane) => {
      const view = summonPreviewStage(pane);
      const members = document.createElement('div');
      members.className = 'summon-picker-members';
      pane.append(members);
      const play = async m => {
        view.label((it.own ? it.label : `${it.value}/${m}`) + ' · loading');
        const r = it.own
          ? await api(`/api/effect?name=${encodeURIComponent(it.own)}`).catch(() => null)
          : await api(`/api/effect/import?category=${it.value}&member=${m}`).catch(() => null);
        const def = r && !r.error ? r.effect : null;
        view.effect(def, it.own || null);
        view.label(def ? (it.own ? it.label : `${it.value}/${m}` + (it.note ? ' · ' + it.note : '')) : 'nothing to play');
      };
      if (!it.own && !packOnly) {
        // Its members, to play and to pick.
        api(`/api/effect?name=${encodeURIComponent(it.pack)}`).then(pack => {
          const ms = [...new Set(((pack && pack.templates) || []).filter(t => t.category === it.value && t.member > 0).map(t => t.member))].sort((a, b) => a - b);
          if (!ms.length) ms.push(1);
          if (!ms.includes(chosenMember)) chosenMember = String(it.value) === String(current) && ms.includes(member) ? member : ms[0];
          for (const m of ms) {
            const b = Object.assign(document.createElement('button'), { className: 'chip' + (m === chosenMember ? ' on' : ''), textContent: String(m), title: 'member ' + m });
            b.onclick = () => { chosenMember = m; for (const x of members.children) x.classList.toggle('on', x === b); play(m); };
            members.append(b);
          }
          play(chosenMember);
        }).catch(() => play(1));
      } else play(1);
      return () => view.stop();
    }
  });
}

/// A model: the mod's own (a glTF on a game skeleton, f300 and on) and its model files, the eight summons', the
/// monsters' (by family, named by their first monster), with pictures. A number back, or a file's path (assets/…) to
/// make a model of.
async function pickSummonModel(current, onChosen, own = null) {
  const names = await summonFamilyNames();
  const entries = [];
  if (own) {
    if (!Array.isArray(state.assets)) state.assets = await api('/api/project/assets').catch(() => []) || [];
    for (const m of own) entries.push({ model: 'f' + summonPad(m.number), label: 'f' + summonPad(m.number), note: m.gltf.replace(/^assets\//, ''), from: 'the mod\'s, on ' + m.base + '\'s skeleton', pkg: m.gltf });
    const used = new Set(own.map(m => m.gltf.toLowerCase()));
    for (const a of state.assets) if (/\.(glb|gltf)$/i.test(a.name) && !used.has(a.name.toLowerCase())) entries.push({ model: a.name, label: a.name.replace(/^assets\//, ''), note: 'make it a model', from: 'the project\'s file: made a model of its own', asset: true, pkg: a.name });
  }
  entries.push(...ModSummonNames.map(([creature], i) => ({ model: 'f' + summonPad(201 + i), label: 'f' + summonPad(201 + i), note: creature, from: 'a summon\'s model' })));
  for (const [family, name] of [...names].sort((a, b) => a[0] - b[0])) if (family < 201) entries.push({ model: 'f' + summonPad(family), label: 'f' + summonPad(family), note: name, from: name + '\'s family' });
  pickModel('f' + summonPad(current), m => onChosen(/^assets\//i.test(m) ? m : parseInt(String(m).replace(/\D/g, ''), 10)), { entries, title: 'The summon\'s model', what: 'models', importable: !!own });
}

/// Motions: the eight summons' packs (b_sm###) and the monsters' (b_f###, as 1000 + the family), each on its model.
async function pickSummonMotionPack(current, onChosen) {
  const names = await summonFamilyNames();
  const items = ModSummonNames.map(([creature], i) => ({ value: i + 1, label: 'b_sm' + summonPad(i + 1), note: creature, group: 'the summons\'' }));
  for (const [family, name] of [...names].sort((a, b) => a[0] - b[0])) if (family < 201) items.push({ value: 1000 + family, label: 'b_f' + summonPad(family), note: name, group: 'the monsters\'' });
  summonPickList({
    title: 'The summon\'s motions', items, current, onChosen, what: 'motion packs',
    preview: (it, pane) => { const view = summonPreviewStage(pane); summonPreviewModel(view, summonPackModel(it.value), summonPackName(it.value), 101, it.label); return () => view.stop(); }
  });
}

/// A motion of the loaded pack, played on the summon's model.
function pickSummonMotion(current, look, onChosen) {
  const items = (look.motionList || []).map(m => ({ value: m.id, label: String(m.id), note: `${m.name}${m.frames ? ' · ' + m.frames + ' frames' : ''}` }));
  summonPickList({
    title: 'A motion of ' + (look.motions || 'its pack'), items, current, onChosen, what: 'motions',
    preview: (it, pane) => { const view = summonPreviewStage(pane); summonPreviewModel(view, look.model, 'files/' + look.motions + '.ncap.lz', it.value, it.label, look.target); return () => view.stop(); }
  });
}

/// A monster (its size and hit point for the summon), shown as its family's model standing.
async function pickSummonMonster(current, onChosen) {
  const list = await summonPickMonsters();
  const items = list.slice().sort((a, b) => a.id - b.id).map(m => ({ value: m.id, label: m.name, note: `${m.id} · f${summonPad(m.family)}${m.level ? ' · L' + m.level : ''}`, group: m.mod ? 'the mod\'s' : 'the game\'s' }));
  summonPickList({
    title: 'The summon\'s monster', items, current, onChosen, what: 'monsters',
    preview: (it, pane) => {
      const view = summonPreviewStage(pane);
      const m = list.find(x => x.id === it.value);
      if (m && m.family > 0) summonPreviewModel(view, 'files/f' + summonPad(m.family) + '.nmdp.lz', m.family < 201 ? 'files/b_f' + summonPad(m.family) + '.ncap.lz' : null, 101, it.label, m.id);
      return () => view.stop();
    }
  });
}

/// A spell whose effect plays on each target (DRAW_TARGET_EFFECT), its effect playing.
async function pickSummonSpell(current, onChosen) {
  const r = await api('/api/project/effect/spells').catch(() => null);
  const spells = (r && r.spells) || [];
  const items = spells.filter(s => s.category > 0).sort((a, b) => a.id - b.id).map(s => ({ value: s.id, label: s.name || String(s.id), note: `${s.id} · effect ${s.category}/${s.member > 0 ? s.member : 1}`, group: spellSchoolWord ? spellSchoolWord(s.school) : '', spell: s }));
  // Grouped by school, in order.
  items.sort((a, b) => String(a.group).localeCompare(String(b.group)) || a.value - b.value);
  summonPickList({
    title: 'A spell to play on each target', items, current, onChosen, what: 'spells',
    preview: (it, pane) => {
      const view = summonPreviewStage(pane);
      const s = it.spell;
      api(`/api/effect/import?category=${s.category}&member=${s.member > 0 ? s.member : 1}`).then(x => { view.effect(x && !x.error ? x.effect : null); view.label(it.label + ' · ' + it.note); }).catch(() => {});
      return () => view.stop();
    }
  });
}

// The eight by level, as the client's ModSummons names them: the creature, its spell.
const ModSummonNames = [['Chocobo', 'Escape'], ['Shiva', 'Icen'], ['Ramuh', 'Spark'], ['Ifrit', 'Heatra'], ['Titan', 'Hyper'], ['Odin', 'Catastro'], ['Leviathan', 'Leviath'], ['Bahamut', 'Bahamur']];
