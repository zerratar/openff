// Effects: the new effect format played in the browser, and the Effects library's view.
//
// The runtime half (makeEffectPlayer) plays a definition of Docs/Effects-Plan.md's format - what
// Shared/Effects/EffectImport.cs makes of the game's effects today, what the mods' own will be - a
// game step at a time (30 a second), the way eld does: emitters born on the timeline at their frame,
// riding their path, bursting groups of particles every interval for their duration; a particle
// moves by its speed (turned by its spread) and gravity, orbits, or gathers to the emitter, and
// takes its colour, scale and texture frame from keys by its age. Seeded, so a frame reached by
// playing and by scrubbing is the same frame. It is the reference the client's runtime is kept to.
//
// The view half draws it on a Stage - a ground, the target standing where the battle's hit point
// puts it, the battle's kind of camera - as camera-facing quads, alpha-blended over the scene
// without writing depth, as the game draws them; at 60 frames a second a quad is drawn between
// the two steps it stands at. Play, pause, a step, a scrubber, loop, and the particle count.
//
// Server: /api/effects, /api/effect?name=, /api/effect/import?category=&member=, /api/effect/texture.

'use strict';

// ------------------------------------------------------------------ the runtime

/// mulberry32: a small seeded generator, the same numbers for the same seed.
function effectRandom(seed) {
  let a = seed >>> 0;
  return () => {
    a = (a + 0x6D2B79F5) >>> 0;
    let t = a;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

const EFFECT_DEG = Math.PI / 180;

/// A curve's keys, as it is written: a list of [age, ...values] (straight lines between), or
/// { keys, smooth } - smooth, a Hermite curve through them. A smooth key may carry its tangents
/// after its values ([age, ...values, ...slopes], a slope in value per frame); without them it has
/// the curve's own: flat at the ends and at a turn, else as its neighbours lie, never overshooting.
function effectCurveKeys(curve) {
  if (!curve) return null;
  return Array.isArray(curve) ? curve : (Array.isArray(curve.keys) ? curve.keys : null);
}

/// A key's tangent on one of its values (j): its own, or the curve's.
function effectSlope(keys, i, j, width) {
  const k = keys[i];
  if (k.length >= 1 + 2 * width) return k[1 + width + j];
  if (i === 0 || i === keys.length - 1) return 0;
  const p = keys[i - 1], n = keys[i + 1];
  if (k[0] <= p[0] || n[0] <= k[0]) return 0;
  const left = (k[1 + j] - p[1 + j]) / (k[0] - p[0]), right = (n[1 + j] - k[1 + j]) / (n[0] - k[0]);
  if (left * right <= 0) return 0;
  const m = (n[1 + j] - p[1 + j]) / (n[0] - p[0]);
  return Math.sign(m) * Math.min(Math.abs(m), 3 * Math.abs(left), 3 * Math.abs(right));
}

/// A value of a curve by age: between a key's neighbours, straight or smooth.
function effectKeys(curve, age, width) {
  const keys = effectCurveKeys(curve);
  if (!keys || !keys.length) return null;
  const smooth = !Array.isArray(curve) && !!curve.smooth;
  if (age <= keys[0][0]) return keys[0].slice(1, 1 + width);
  const last = keys[keys.length - 1];
  if (age >= last[0]) return last.slice(1, 1 + width);
  for (let i = 1; i < keys.length; i++) {
    const b = keys[i];
    if (age > b[0]) continue;
    const a = keys[i - 1];
    const h = b[0] - a[0];
    const t = h === 0 ? 1 : (age - a[0]) / h;
    const out = new Array(width);
    if (!smooth || h === 0) {
      for (let j = 0; j < width; j++) out[j] = a[1 + j] + (b[1 + j] - a[1 + j]) * t;
      return out;
    }
    const t2 = t * t, t3 = t2 * t;
    const h00 = 2 * t3 - 3 * t2 + 1, h10 = t3 - 2 * t2 + t, h01 = -2 * t3 + 3 * t2, h11 = t3 - t2;
    for (let j = 0; j < width; j++)
      out[j] = h00 * a[1 + j] + h10 * h * effectSlope(keys, i - 1, j, width) + h01 * b[1 + j] + h11 * h * effectSlope(keys, i, j, width);
    return out;
  }
  return last.slice(1, 1 + width);
}

/// A number that may be a curve: itself, or the curve's value at the age ({ keys, smooth } or a list of [age, value]).
function effectNumberAt(v, age) {
  if (v && typeof v === 'object') return (effectKeys(v, age, 1) || [0])[0];
  return Number(v) || 0;
}

/// The frame of a flipbook at an age: the last [age, cell] at or before it.
function effectFrame(frames, age) {
  if (!frames || !frames.length) return 0;
  let cell = frames[0][1];
  for (const f of frames) { if (f[0] <= age) cell = f[1]; else break; }
  return cell;
}

/// A row vector turned about X, then Y, then Z (eld's EmmitController: RotX * RotY * RotZ).
function effectTurn(v, ax, ay, az) {
  let [x, y, z] = v;
  if (ax) { const c = Math.cos(ax * EFFECT_DEG), s = Math.sin(ax * EFFECT_DEG); [y, z] = [y * c - z * s, y * s + z * c]; }
  if (ay) { const c = Math.cos(ay * EFFECT_DEG), s = Math.sin(ay * EFFECT_DEG); [x, z] = [x * c + z * s, -x * s + z * c]; }
  if (az) { const c = Math.cos(az * EFFECT_DEG), s = Math.sin(az * EFFECT_DEG); [x, y] = [x * c - y * s, x * s + y * c]; }
  return [x, y, z];
}

/// Where a track's path puts it after `steps` steps (one point, or line / Hermite segments timed in steps).
function effectPathAt(path, steps) {
  if (!path) return [0, 0, 0];
  if (path.point) return path.point.slice();
  const segs = path.segments || [];
  if (!segs.length) return [0, 0, 0];
  const times = path.times || [];
  const dur = i => Math.max(1, i + 1 < segs.length ? (times[i + 1] || 0) - (times[i] || 0) : (path.length || 0) - (times[i] || 0));
  const total = segs.reduce((n, _, i) => n + dur(i), 0);
  let left = steps;
  if (path.end === 'repeat' && total > 0) left = steps % total;
  let forward = true;
  if (path.end === 'pingpong' && total > 0) { const k = Math.floor(steps / total); left = steps % total; forward = k % 2 === 0; }
  let i = 0;
  while (i < segs.length && left >= dur(i)) { left -= dur(i); i++; }
  let t;
  if (i >= segs.length) { i = segs.length - 1; t = 1; } else t = left / dur(i);
  if (!forward) { i = segs.length - 1 - i; t = 1 - t; }
  const [p0, p1, t0, t1] = segs[i];
  if (!path.curve) return [0, 1, 2].map(k => p0[k] + (p1[k] - p0[k]) * t);
  const t2 = t * t, t3 = t2 * t;
  const h00 = 2 * t3 - 3 * t2 + 1, h01 = -2 * t3 + 3 * t2, h10 = t3 - 2 * t2 + t, h11 = t3 - t2;
  return [0, 1, 2].map(k => h00 * p0[k] + h01 * p1[k] + h10 * t0[k] + h11 * t1[k]);
}

/// A player of one definition. step() is one game step; particles() what to draw after it.
function makeEffectPlayer(def, { seed = 1, anchor = [0, 0, 0], anchors = {}, faithfulSpread = true } = {}) {
  const tracks = (def.tracks || []).map(t => t.type ? t : { ...t, type: 'emitter' });
  let rand, frame, cycle, live, meshes, stopping, started = [];

  function reset() {
    rand = effectRandom(seed);
    frame = -1;            // the step just taken; 0 is the effect's first
    cycle = 0;             // the frame the timeline's current pass began on (a looping effect's)
    live = [];             // the emitters started, in the order they were
    meshes = [];           // the models started: where they are, how many steps they have played
    stopping = false;
  }

  const between = (lo, hi) => lo + (hi - lo) * rand();
  const range = r => Array.isArray(r) ? between(r[0], r.length > 1 ? r[1] : r[0]) : (r || 0);

  function start(track) {
    const em = track.emission || {};
    live.push({
      track, steps: 0,
      life: 0, next: em.interval || 0, made: 0, playing: true, stopped: false, done: false,
      groups: new Array(Math.max(0, em.bursts || 0)).fill(null),
      at: [0, 0, 0]
    });
  }

  /// An anchor by name: target (the default), caster (the target when there is none), between (their middle), world (the origin).
  function anchorOf(name) {
    const caster = anchors.caster || anchor;
    switch ((name || 'target').toLowerCase()) {
      case 'world': return [0, 0, 0];
      case 'caster': return caster;
      case 'between': return [0, 1, 2].map(k => (caster[k] + anchor[k]) / 2);
      default: return anchors[name] || anchor;
    }
  }

  /// A path from one anchor to another over its length in steps, rising by its arc at the middle.
  function spanAt(path, steps) {
    const from = anchorOf(path.from), to = anchorOf(path.to || 'target');
    const length = Math.max(1, path.length || 0);
    let s = steps / length;
    if (path.end === 'repeat') s -= Math.floor(s);
    else if (path.end === 'pingpong') { const k = Math.floor(s); s -= k; if (k % 2 !== 0) s = 1 - s; }
    else s = Math.min(1, s);
    const arc = path.arc || 0;
    return [from[0] + (to[0] - from[0]) * s, from[1] + (to[1] - from[1]) * s + arc * 4 * s * (1 - s), from[2] + (to[2] - from[2]) * s];
  }

  function where(e) {
    const t = e.track;
    const o = t.offset || [0, 0, 0];
    if (t.path && t.path.from !== undefined) {
      const q = spanAt(t.path, Math.max(0, e.steps - 1));
      return [q[0] + o[0], q[1] + o[1], q[2] + o[2]];
    }
    const p = effectPathAt(t.path, Math.max(0, e.steps - 1));
    const a = anchorOf(t.anchor);
    return [a[0] + o[0] + p[0], a[1] + o[1] + p[1], a[2] + o[2] + p[2]];
  }

  function makeGroup(e) {
    const t = e.track, em = t.emission || {};
    const local = t.space === 'local';
    const g = { age: 0, alive: true, parts: [] };
    // Count over time: how many a burst makes by the emitter's frame (keys [frame, count]), else the emission's count.
    const n = t.countOverTime ? Math.max(0, Math.floor((effectKeys(t.countOverTime, e.life, 1) || [em.count || 0])[0] + 0.5)) : (em.count || 0);
    for (let i = 0; i < n; i++) {
      const box = (t.shape && t.shape.box) || [0, 0, 0];
      let c = box.map(r => r ? -r + 2 * r * rand() : 0);
      const p = { base: c, prev: null, trail: [] };
      if (t.gather) {
        const len = Math.hypot(c[0], c[1], c[2]) || 1;
        const dir = c.map(v => -v / len);
        p.gather = {
          going: len > 0, left: len, done: 0,
          speed: dir.map(v => v * (t.gather.speed || 0)),
          add: dir.map(v => v * (t.gather.accel || 0)),
          turn: (t.gather.swirl || [0, 0, 0]).map(s => s ? s * rand() : 0)
        };
      } else {
        let v = [0, 0, 0];
        if (t.speed) {
          // Speed over time: the speed a particle is born with, times the curve at the emitter's frame.
          const s = range(t.speed.value) * (t.speedOverTime ? effectNumberAt(t.speedOverTime, e.life) : 1);
          v = (t.speed.direction || [0, 1, 0]).map(d => d * s);
          const sp = t.speed.spread;
          if (sp) {
            // The game's rule: each axis turned by up to its spread either way (faithfulSpread false: a full turn, as the port does).
            const a = sp.map(x => x ? (faithfulSpread ? -x + 2 * x * rand() : 360 * rand()) : 0);
            v = effectTurn(v, a[0], a[1], a[2]);
          }
        }
        p.vel = v;
        if (t.gravity) { const gv = range(t.gravity.value); p.grav = (t.gravity.direction || [0, -1, 0]).map(d => d * gv); }
        else p.grav = [0, 0, 0];
        if (t.orbit) { p.radius = t.orbit.radius || 0; p.angle = 360 * rand(); }
      }
      if (!local && !t.gather) p.base = [p.base[0] + e.at[0], p.base[1] + e.at[1], p.base[2] + e.at[2]];
      // Size over time: the size it is born with, times the curve at the emitter's frame.
      p.size = (range(t.size) || 1) * (t.sizeOverTime ? effectNumberAt(t.sizeOverTime, e.life) : 1);
      // Spin: the quad turned in the screen's plane, from its angle by its speed a frame (degrees).
      if (t.spin) { p.roll = range(t.spin.angle); p.spin = range(t.spin.speed); } else { p.roll = 0; p.spin = 0; }
      g.parts.push(p);
    }
    return g;
  }

  function stepGroup(e, g) {
    const t = e.track, L = t.life || 1, A = (t.trail && t.trail.count) || 0;
    if (g.age > L + A) { g.alive = false; return; }
    g.age++;
    for (const p of g.parts) {
      p.prev = p.pos ? p.pos.slice() : null;
      if (A) { p.trail.unshift(p.pos ? { pos: p.pos.slice(), shown: p.shown } : null); p.trail.length = Math.min(p.trail.length, A); }
      if (p.gather) {
        const q = p.gather;
        if (q.going) {
          for (let k = 0; k < 3; k++) p.base[k] += q.speed[k];
          q.done += Math.hypot(q.speed[0], q.speed[1], q.speed[2]);
          if (q.done >= q.left) { p.base = [0, 0, 0]; q.going = false; }
          else {
            for (let k = 0; k < 3; k++) q.speed[k] += q.add[k];
            q.speed = effectTurn(q.speed, q.turn[0], q.turn[1], q.turn[2]);
            q.add = effectTurn(q.add, q.turn[0], q.turn[1], q.turn[2]);
            p.base = effectTurn(p.base, q.turn[0], q.turn[1], q.turn[2]);
          }
        }
        p.local = p.base.slice();
      } else {
        // Speed over life: how much of its speed a step carries it at its age (1, all of it).
        const pace = t.speedOverLife ? (effectKeys(t.speedOverLife, g.age, 1) || [1])[0] : 1;
        // Gravity over life: how much of its pull reaches the particle at its age.
        const pull = t.gravityOverLife ? (effectKeys(t.gravityOverLife, g.age, 1) || [1])[0] : 1;
        for (let k = 0; k < 3; k++) { p.vel[k] += p.grav[k] * pull; p.base[k] += p.vel[k] * pace; }
        p.local = p.base.slice();
        if (t.orbit) {
          // Its grow and turn a number, or a curve over its life.
          p.radius += effectNumberAt(t.orbit.grow, g.age);
          p.angle += effectNumberAt(t.orbit.turn, g.age);
          p.local[0] += p.radius * Math.sin(p.angle * EFFECT_DEG);
          p.local[2] += p.radius * Math.cos(p.angle * EFFECT_DEG);
        }
      }
      const origin = t.space === 'local' ? e.at : [0, 0, 0];
      p.pos = [p.local[0] + origin[0], p.local[1] + origin[1], p.local[2] + origin[2]];
      if (!p.prev) p.prev = p.pos.slice();
      p.shown = g.age < L;
      // The spin: its angle as it is born, then its speed a step (times spin over life at its age).
      if (g.age === 1) { p.rollNow = p.roll; p.rollBefore = p.roll; }
      else { p.rollBefore = p.rollNow; p.rollNow += p.spin * (t.spinOverLife ? (effectKeys(t.spinOverLife, g.age, 1) || [1])[0] : 1); }
    }
  }

  function stepEmitter(e) {
    const t = e.track, em = t.emission || {};
    e.steps++;
    e.at = where(e);
    if (e.playing) {
      e.life++; e.next++;
      if (e.next >= (em.interval || 0) && e.made < e.groups.length) {
        e.groups[e.made] = makeGroup(e);
        e.made++;
        e.next = 0;
      }
      for (const g of e.groups) if (g && g.alive) stepGroup(e, g);
      if (e.life >= (em.duration || 0)) {
        if (em.loop && !e.stopped) { e.made = 0; e.life = 0; e.next = 0; }
        else e.playing = false;
      }
    } else {
      for (const g of e.groups) if (g && g.alive) stepGroup(e, g);
      const last = e.groups[e.made ? e.made - 1 : 0];
      if (!last || !last.alive) e.done = true;
    }
  }

  function step() {
    frame++;
    started = [];
    const length = def.length || 0;
    // The timeline: tracks start on their frame of the pass; a looping effect runs it again after its end.
    if (def.loop && frame - cycle > length) cycle = frame;
    const at = frame - cycle;
    for (const t of tracks) {
      if ((t.start || 0) !== at) continue;
      // A pass after the first leaves a looping emitter that is still going alone (the sequence's `first`).
      if (cycle > 0 && t.id !== undefined && live.some(e => e.track === t && !e.done && (t.emission || {}).loop)) continue;
      if (t.type === 'mesh') { if (cycle === 0 || def.loop) meshes.push({ track: t, steps: 0, at: [0, 0, 0], prev: null }); continue; }
      if (t.type !== 'emitter') { if (cycle === 0 || def.loop) started.push(t); continue; }   // a sound, a flash, a shake: the host plays it
      if (cycle === 0 || def.loop) start(t);
    }
    // A model plays its motion a frame a step, riding its path as an emitter does; the Stage ends it with its motion.
    for (const m of meshes) { m.steps++; m.prev = m.at; m.at = where(m); if (m.steps === 1) m.prev = m.at; }
    for (const e of live) {
      if (e.done) continue;
      if (!def.loop && at >= length && cycle === 0) e.stopped = true;
      if (e.track.stop !== undefined && at >= e.track.stop) e.stopped = true;
      stepEmitter(e);
    }
    if (live.length > 64) live = live.filter(e => !e.done);
  }

  /// Every quad to draw now, in the order the game draws them: emitter, group, particle, then its trail.
  function particles() {
    const out = [];
    for (const e of live) {
      if (e.done) continue;
      const t = e.track, tex = t.texture || {};
      const L = t.life || 1;
      for (const g of e.groups) {
        if (!g || !g.alive || g.age < 1) continue;
        const colour = (effectKeys(t.colour, g.age, 4) || [255, 255, 255, 255]).map(c => Math.max(0, Math.min(255, c)));
        const scale = effectKeys(t.scale, g.age, 2) || [1, 1];
        const cell = effectFrame(tex.frames, g.age);
        // The step before's size and colour: a colour only when no channel moved by more than a quarter (a fade, not a flash).
        const was = g.age > 1 ? g.age - 1 : g.age;
        const scaleBefore = effectKeys(t.scale, was, 2) || [1, 1];
        const colourWas = (effectKeys(t.colour, was, 4) || [255, 255, 255, 255]).map(c => Math.max(0, Math.min(255, c)));
        const colourBefore = colourWas.every((c, k) => Math.abs(c - colour[k]) <= 64) ? colourWas : colour;
        for (const p of g.parts) {
          const roll = p.rollNow || 0, rollBefore = p.rollBefore || 0;
          if (p.shown && colour[3] > 0) out.push({ p, pos: p.pos, prev: p.prev, w: p.size * scale[0] / 2, h: p.size * scale[1] / 2, wBefore: p.size * scaleBefore[0] / 2, hBefore: p.size * scaleBefore[1] / 2, colour, colourBefore, cell, tex, blend: (t.render || {}).blend, tint: (t.render || {}).tint, roll, rollBefore });
          if (t.trail && p.trail.length) {
            // After-images: the particle where it was, each darker by the trail's colour toward its end (the last one never shows, as in the game).
            const n = t.trail.count, d = colour.map((c, k) => (c - Math.max(0, Math.min(255, c + (t.trail.colour[k] || 0)))) / (n + 1));
            for (let k = 1; k < n && k <= p.trail.length; k++) {
              const was = p.trail[k - 1];
              if (!was || !was.shown) continue;
              const c = colour.map((v, j) => v - k * d[j]);
              if (c[3] > 0) out.push({ p: null, pos: was.pos, prev: was.pos, w: p.size * scale[0] / 2, h: p.size * scale[1] / 2, colour: c, cell, tex, blend: (t.render || {}).blend, tint: (t.render || {}).tint, roll, rollBefore: roll });
            }
          }
        }
      }
    }
    return out;
  }

  /// The models to draw now: each with its track, where it is (and was), and the frame of its motion (0 its first).
  function models() {
    if (frame >= (def.length || 0) && !def.loop && live.every(e => e.done)) return [];
    // A mesh with a life of its own shows for it; else while the effect plays.
    return meshes.filter(m => m.track.life === undefined || m.steps <= m.track.life)
      .map(m => {
        // Its scale (a number, x/y/z, or a curve over its steps) and its yaw (degrees, or a curve).
        const sc = m.track.scale;
        const scale = sc && typeof sc === 'object' && !Array.isArray(sc) ? effectNumberAt(sc, m.steps) : (sc === undefined ? 1 : sc);
        return { track: m.track, pos: m.at, prev: m.prev || m.at, frame: m.steps - 1, scale, yaw: effectNumberAt(m.track.yaw, m.steps) };
      });
  }

  reset();
  return {
    reset, step, particles, models,
    get frame() { return frame; },
    /// The tracks of other kinds that started on the last step (a sound, a flash, a shake).
    get started() { return started; },
    /// Whether everything it started has finished (and it does not loop).
    get finished() { return !def.loop && frame >= (def.length || 0) && live.every(e => e.done); }
  };
}

/// Several of one definition, as the battle plays a spell on each of a group: a player a target, at
/// its own anchor, each started `stagger` steps after the one before (TurnSystem.drawOnceMagicEffect:
/// half the spell's play frame). The same face as one player's.
function makeEffectGroup(def, targets, { seed = 1, anchors = {}, stagger = 0 } = {}) {
  const players = targets.map((anchor, k) => ({ delay: k * stagger, p: makeEffectPlayer(def, { seed: seed + k, anchor, anchors }) }));
  let frame = -1, started = [];
  const on = x => frame >= x.delay;
  return {
    reset() { frame = -1; started = []; for (const x of players) x.p.reset(); },
    step() { frame++; started = []; for (const x of players) if (on(x)) { x.p.step(); started.push(...x.p.started); } },
    particles: () => players.flatMap(x => on(x) ? x.p.particles() : []),
    models: () => players.flatMap(x => on(x) ? x.p.models() : []),
    get frame() { return frame; },
    get started() { return started; },
    get finished() { return players.every(x => on(x) && x.p.finished); }
  };
}

// ------------------------------------------------------------------ the Stage

const EFFECT_VERTEX = `
attribute vec3 centre;
attribute vec3 before;
attribute vec2 corner;
attribute vec2 extent;
attribute vec2 uv;
attribute vec4 colour;
attribute vec2 roll;
attribute vec2 extentBefore;
attribute vec4 colourBefore;
uniform mat4 view;
uniform mat4 projection;
uniform float between;
varying vec2 vUv;
varying vec4 vColour;
void main() {
  vec4 p = view * vec4(mix(before, centre, between), 1.0);
  // The spin: the corner turned about the quad's middle, in the screen's plane (roll: now, and the step before).
  float r = radians(mix(roll.y, roll.x, between));
  vec2 c = corner * mix(extentBefore, extent, between);
  p.xy += vec2(c.x * cos(r) - c.y * sin(r), c.x * sin(r) + c.y * cos(r));
  gl_Position = projection * p;
  vUv = uv;
  vColour = mix(colourBefore, colour, between);
}`;

const EFFECT_FRAGMENT = `
precision mediump float;
uniform sampler2D picture;
uniform bool recolour;
varying vec2 vUv;
varying vec4 vColour;
void main() {
  vec4 t = texture2D(picture, vUv);
  // Recolour (render.tint): the picture's brightness - its brightest channel - in the particle's colour; else the game's multiply.
  if (recolour) t.rgb = vec3(max(t.r, max(t.g, t.b)));
  vec4 c = t * vColour;
  if (c.a < 0.01) discard;
  gl_FragColor = c;
}`;

const EFFECT_LINE_VERTEX = `
attribute vec3 position;
attribute vec4 colour;
uniform mat4 view;
uniform mat4 projection;
varying vec4 vColour;
void main() { gl_Position = projection * view * vec4(position, 1.0); vColour = colour; }`;

const EFFECT_LINE_FRAGMENT = `
precision mediump float;
varying vec4 vColour;
void main() { gl_FragColor = vColour; }`;

const EFFECT_MODEL_VERTEX = `
attribute vec3 position;
attribute vec2 coord;
attribute vec3 colour;
attribute float mindex;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 world;
uniform mat4 palette[32];
uniform mat3 uvMatrix;
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  gl_Position = projection * view * world * palette[int(mindex + 0.5)] * vec4(position, 1.0);
  // A battle map's texture scroll (its .namp's SRT, as a transform of the coordinates the model was baked with).
  vCoord = (uvMatrix * vec3(coord, 1.0)).xy;
  vColour = colour;
}`;

const EFFECT_MODEL_FRAGMENT = `
precision mediump float;
uniform sampler2D picture;
uniform bool textured;
uniform vec3 tint;
uniform float alpha;
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  vec4 c = textured ? texture2D(picture, vCoord) : vec4(1.0);
  if (c.a < 0.05) discard;
  gl_FragColor = vec4(c.rgb * vColour * tint, c.a * alpha);
}`;

/// A 4x3 of the server's (rotation rows, then the translation - row vectors) as a column-major mat4.
function effectMat43(m, at = 0) {
  return [m[at], m[at + 1], m[at + 2], 0, m[at + 3], m[at + 4], m[at + 5], 0, m[at + 6], m[at + 7], m[at + 8], 0, m[at + 9], m[at + 10], m[at + 11], 1];
}

function effectCompile(gl, vs, fs) {
  const make = (kind, text) => {
    const s = gl.createShader(kind);
    gl.shaderSource(s, text);
    gl.compileShader(s);
    if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(s));
    return s;
  };
  const p = gl.createProgram();
  gl.attachShader(p, make(gl.VERTEX_SHADER, vs));
  gl.attachShader(p, make(gl.FRAGMENT_SHADER, fs));
  gl.linkProgram(p);
  if (!gl.getProgramParameter(p, gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(p));
  return p;
}

function effectPerspective(fov, aspect, near, far) {
  const f = 1 / Math.tan(fov / 2), nf = 1 / (near - far);
  return new Float32Array([f / aspect, 0, 0, 0, 0, f, 0, 0, 0, 0, (far + near) * nf, -1, 0, 0, 2 * far * near * nf, 0]);
}

function effectLookAt(eye, at) {
  const sub = (a, b) => [a[0] - b[0], a[1] - b[1], a[2] - b[2]];
  const norm = v => { const l = Math.hypot(v[0], v[1], v[2]) || 1; return v.map(x => x / l); };
  const cross = (a, b) => [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];
  const dot = (a, b) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
  const z = norm(sub(eye, at)), x = norm(cross([0, 1, 0], z)), y = cross(z, x);
  return new Float32Array([x[0], y[0], z[0], 0, x[1], y[1], z[1], 0, x[2], y[2], z[2], 0, -dot(x, eye), -dot(y, eye), -dot(z, eye), 1]);
}

/// The Stage: a canvas that draws a player's particles over a ground and a stand-in target.
function makeEffectStage(canvas, textureUrl) {
  const gl = canvas.getContext('webgl', { antialias: true, alpha: false, premultipliedAlpha: false });
  if (!gl) return null;
  const program = effectCompile(gl, EFFECT_VERTEX, EFFECT_FRAGMENT);
  const lines = effectCompile(gl, EFFECT_LINE_VERTEX, EFFECT_LINE_FRAGMENT);
  const a = n => gl.getAttribLocation(program, n), u = n => gl.getUniformLocation(program, n);
  const attr = { centre: a('centre'), before: a('before'), corner: a('corner'), extent: a('extent'), uv: a('uv'), colour: a('colour'), roll: a('roll'), extentBefore: a('extentBefore'), colourBefore: a('colourBefore') };
  const unif = { view: u('view'), projection: u('projection'), between: u('between'), picture: u('picture'), recolour: u('recolour') };
  const lineAttr = { position: gl.getAttribLocation(lines, 'position'), colour: gl.getAttribLocation(lines, 'colour') };
  const lineUnif = { view: gl.getUniformLocation(lines, 'view'), projection: gl.getUniformLocation(lines, 'projection') };
  const quadBuffer = gl.createBuffer(), lineBuffer = gl.createBuffer();
  const modelProgram = effectCompile(gl, EFFECT_MODEL_VERTEX, EFFECT_MODEL_FRAGMENT);
  const ma = n => gl.getAttribLocation(modelProgram, n), mu = n => gl.getUniformLocation(modelProgram, n);
  const modelAttr = { position: ma('position'), coord: ma('coord'), colour: ma('colour'), mindex: ma('mindex') };
  const modelUnif = { view: mu('view'), projection: mu('projection'), world: mu('world'), palette: mu('palette'), picture: mu('picture'), textured: mu('textured'), tint: mu('tint'), alpha: mu('alpha'), uvMatrix: mu('uvMatrix') };

  // The battle as the game lays it out (btl.Members.cs), in its units: the monsters' six places (the back row
  // first, as a party fills them), the party's four in the front row facing them, the battle map at the origin.
  // A hit point - where the battle plays an effect on one (TurnSystem.setHitEffectPosition) - is so far toward
  // the battle camera from its feet and so far up: 9 and 5 for the party, the monster's own offsets for a monster.
  const BATTLE_EYE = [131.12, 34.73, 46.83], BATTLE_AT = [116.71, 32.0, 40.44];
  const MONSTER_PLACES = [[-42, 0, -8], [-42, 0, -29], [-42, 0, 13], [-19, 0, -8], [-19, 0, -29], [-19, 0, 13]];
  const PARTY_PLACES = [[29, 0, -26], [29, 0, -8], [29, 0, 8], [29, 0, 26]];
  // A spell that can only hit a whole side plays once, at the side's point (AllEnemyMagicPosition, AllPlayerMagicPosition).
  const ALL_MONSTERS = [-36, 0, -5], ALL_PARTY = [24, 0, 0];
  let target = { toward: 9, up: 5, scale: 1, turn: 90, model: null, party: false, at: null }, lineCount = 0;
  // What is shown (show): the battle map, the monsters, the heroes (all four, the caster and the targets, the
  // caster, none), the floor's grid, the marks where the effect plays - any of them off, down to the effect alone.
  const SHOW_ALL = { map: true, monsters: true, heroes: 'all', floor: true, marks: true };
  let layout = { count: 'one', background: '', view: 'battle', show: { ...SHOW_ALL } };
  let figures = [], anchors = [[0, 0, 0]];
  function hitPoint(feet, toward, up) {
    const d = [BATTLE_EYE[0] - feet[0], BATTLE_EYE[1] - feet[1], BATTLE_EYE[2] - feet[2]];
    const l = Math.hypot(d[0], d[1], d[2]) || 1;
    return [feet[0] + d[0] / l * toward, feet[1] + d[1] / l * toward + up, feet[2] + d[2] / l * toward];
  }
  /// Who stands where (figures: feet and turn), and where the effect plays (anchors: a hit point each, or the side's point).
  function relayout() {
    const several = layout.count !== 'one';
    if (target.party) {
      const places = several ? PARTY_PLACES : [PARTY_PLACES[1]];
      figures = places.map(feet => ({ feet, turn: -90 }));
      anchors = layout.count === 'all' ? [ALL_PARTY] : places.map(feet => hitPoint(feet, 9, 5));
    } else {
      // A monster with a place of its own (a boss) stands there alone; else the party's places, three of them for a group.
      const places = target.at ? [target.at] : several ? MONSTER_PLACES.slice(0, 3) : [MONSTER_PLACES[0]];
      figures = places.map(feet => ({ feet, turn: target.turn }));
      anchors = layout.count === 'all' ? [ALL_MONSTERS] : places.map(feet => hitPoint(feet, target.toward, target.up));
    }
    camera.target = anchors[0].slice();
    layGround();
  }
  function layGround() {
    const ground = [];
    const put = (x1, y1, z1, x2, y2, z2, c) => ground.push(x1, y1, z1, ...c, x2, y2, z2, ...c);
    const show = layout.show || SHOW_ALL;
    // The battle's floor where no battle map is drawn.
    if (show.floor && !(layout.background && show.map)) {
      for (let i = -60; i <= 60; i += 5) {
        const c = i === 0 ? [0.35, 0.4, 0.48, 1] : [0.2, 0.23, 0.28, 1];
        put(i, 0, -45, i, 0, 45, c);
        if (i >= -45 && i <= 45) put(-60, 0, i, 60, 0, i, c);
      }
    }
    if (!target.model && !target.party && show.monsters) {
      // No model: a post of its height.
      const post = [0.55, 0.45, 0.3, 1], H = 10;
      for (const { feet: [X, , Z] } of figures) {
        for (const [dx, dz] of [[-2, -1], [2, -1], [2, 1], [-2, 1]]) put(X + dx, 0, Z + dz, X + dx, H, Z + dz, post);
        put(X - 2, H, Z - 1, X + 2, H, Z - 1, post); put(X - 2, H, Z + 1, X + 2, H, Z + 1, post);
        put(X - 2, H, Z - 1, X - 2, H, Z + 1, post); put(X + 2, H, Z - 1, X + 2, H, Z + 1, post);
      }
    }
    // A cross at each place the effect plays.
    const mark = [0.43, 0.66, 1, 1];
    if (show.marks) for (const [x, y, z] of anchors) { put(x - 0.8, y, z, x + 0.8, y, z, mark); put(x, y - 0.8, z, x, y + 0.8, z, mark); put(x, y, z - 0.8, x, y, z + 0.8, mark); }
    gl.bindBuffer(gl.ARRAY_BUFFER, lineBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(ground), gl.STATIC_DRAW);
    lineCount = ground.length / 7;
  }
  // Models: the target's and the effects' own, their buffers and textures loaded once.
  const models = new Map();
  function loadModel(key, url, textureOf) {
    let m = models.get(key);
    if (m) return m;
    m = { ready: false, groups: [], pose: null };
    models.set(key, m);
    api(url).then(r => {
      const bundle = r.model || r;
      if (!bundle || bundle.error || bundle.problem || !bundle.buffer) { m.problem = (bundle && (bundle.error || bundle.problem)) || 'no model'; return; }
      m.vertex = gl.createBuffer();
      gl.bindBuffer(gl.ARRAY_BUFFER, m.vertex);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(bundle.buffer), gl.STATIC_DRAW);
      m.mindex = gl.createBuffer();
      gl.bindBuffer(gl.ARRAY_BUFFER, m.mindex);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(bundle.matrixIndex && bundle.matrixIndex.length ? bundle.matrixIndex : new Array(bundle.buffer.length / 8).fill(0)), gl.STATIC_DRAW);
      m.index = gl.createBuffer();
      gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, m.index);
      const big = bundle.buffer.length / 8 > 65535;
      if (big) gl.getExtension('OES_element_index_uint');
      m.indexType = big ? gl.UNSIGNED_INT : gl.UNSIGNED_SHORT;
      m.indexSize = big ? 4 : 2;
      gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, big ? new Uint32Array(bundle.indices) : new Uint16Array(bundle.indices), gl.STATIC_DRAW);
      m.groups = (bundle.groups || []).map(g => ({ ...g, picture: g.texture ? modelTexture(textureOf(g.texture), g.wrapS, g.wrapT) : null }));
      m.pose = r.pose || null;
      m.loop = Boolean(r.loop);
      m.ready = true;
    }).catch(e => { m.problem = e.message; });
    return m;
  }
  function modelTexture(url, wrapS, wrapT) {
    const t = { gl: gl.createTexture(), ready: false };
    const image = new Image();
    image.onload = () => {
      gl.bindTexture(gl.TEXTURE_2D, t.gl);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
      const wrap = w => w === 'mirror' ? gl.MIRRORED_REPEAT : w === 'clamp' ? gl.CLAMP_TO_EDGE : gl.REPEAT;
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, wrap(wrapS));
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, wrap(wrapT));
      t.ready = true;
    };
    image.src = url;
    return t;
  }

  const IDENTITY = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1];
  /// One model's groups of a pass: the opaque ones (translucent false) or the translucent.
  function drawModel(m, world, frame, translucent, view, projection, opts = {}) {
    if (!m || !m.ready) return;
    gl.useProgram(modelProgram);
    gl.uniformMatrix4fv(modelUnif.view, false, view);
    gl.uniformMatrix4fv(modelUnif.projection, false, projection);
    gl.uniformMatrix4fv(modelUnif.world, false, world);
    gl.uniform1i(modelUnif.picture, 0);
    gl.bindBuffer(gl.ARRAY_BUFFER, m.vertex);
    gl.enableVertexAttribArray(modelAttr.position); gl.vertexAttribPointer(modelAttr.position, 3, gl.FLOAT, false, 32, 0);
    gl.enableVertexAttribArray(modelAttr.coord); gl.vertexAttribPointer(modelAttr.coord, 2, gl.FLOAT, false, 32, 12);
    gl.enableVertexAttribArray(modelAttr.colour); gl.vertexAttribPointer(modelAttr.colour, 3, gl.FLOAT, false, 32, 20);
    gl.bindBuffer(gl.ARRAY_BUFFER, m.mindex);
    gl.enableVertexAttribArray(modelAttr.mindex); gl.vertexAttribPointer(modelAttr.mindex, 1, gl.FLOAT, false, 4, 0);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, m.index);
    // Where a frame's matrices start: the groups' counts laid end to end, frame after frame.
    const pose = opts.pose || m.pose, per = pose ? pose.counts.reduce((a, b) => a + b, 0) : 0;
    const looping = opts.loop === undefined ? m.loop : opts.loop;
    const f = pose ? Math.max(0, Math.min(pose.frames - 1, looping ? frame % pose.frames : frame)) : 0;
    const alphaMul = opts.alpha === undefined ? 1 : opts.alpha;
    if (alphaMul <= 0) return;
    let slot = 0;
    for (let gi = 0; gi < m.groups.length; gi++) {
      const g = m.groups[gi], count = pose ? (pose.counts[gi] || 1) : 1;
      const first = slot;
      slot += count;
      // A battle map's animation at this frame (Namp.cs): the material's texture transform and alpha.
      const anim = m.anim, srt = anim && anim.srt && anim.srt.materials[g.material], fade = anim && anim.alpha && anim.alpha.materials[g.material];
      const shownAlpha = fade ? fade[frame % anim.alpha.length] : (g.alpha === undefined ? 1 : g.alpha);
      if (g.hidden || shownAlpha <= 0 || Boolean(g.translucent || (fade && shownAlpha < 1) || alphaMul < 1) !== translucent) continue;
      const t = srt ? srt[frame % anim.srt.length] : null;
      gl.uniformMatrix3fv(modelUnif.uvMatrix, false, t ? [t[0], t[1], 0, t[2], t[3], 0, t[4], t[5], 1] : [1, 0, 0, 0, 1, 0, 0, 0, 1]);
      const palette = new Float32Array(32 * 16);
      for (let k = 0; k < 32; k++) palette.set(IDENTITY, k * 16);
      if (pose) for (let k = 0; k < count && k < 32; k++) palette.set(effectMat43(pose.matrices, (f * per + first + k) * 12), k * 16);
      gl.uniformMatrix4fv(modelUnif.palette, false, palette);
      const textured = Boolean(g.picture && g.picture.ready);
      gl.uniform1i(modelUnif.textured, textured ? 1 : 0);
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, textured ? g.picture.gl : blank);
      const c = g.colour === undefined ? 0xFFFFFF : g.colour;
      const tint = opts.tint || [1, 1, 1];
      gl.uniform3f(modelUnif.tint, ((c >> 16) & 255) / 255 * tint[0], ((c >> 8) & 255) / 255 * tint[1], (c & 255) / 255 * tint[2]);
      gl.uniform1f(modelUnif.alpha, shownAlpha * alphaMul);
      gl.drawElements(gl.TRIANGLES, g.count, m.indexType, g.start * m.indexSize);
    }
    for (const loc of Object.values(modelAttr)) gl.disableVertexAttribArray(loc);
  }

  /// A character standing as it stands in battle: the first motion (101, the wait) of the pack that fits the model best, looping.
  function standing(m, name) {
    if (m.idle) return;
    m.idle = true;
    api(`/api/model/motions?name=${encodeURIComponent(name)}`).then(packs => {
      const pack = (packs || []).find(p => p.motions && p.motions.length);
      if (!pack) return null;
      const motion = pack.motions.find(x => x.id === 101) || pack.motions[0];
      return api(`/api/model/pose?name=${encodeURIComponent(name)}&pack=${encodeURIComponent(pack.name)}&index=${motion.index}`);
    }).then(pose => { if (pose && pose.matrices) { m.pose = pose; m.loop = true; } }).catch(() => {});
  }

  /// The world matrix of a model at a place, scaled, turned about Y by degrees.
  function placed(pos, scale, turn = 0) {
    const s = Array.isArray(scale) ? scale : [scale, scale, scale];
    const c = Math.cos(turn * EFFECT_DEG), n = Math.sin(turn * EFFECT_DEG);
    return new Float32Array([c * s[0], 0, -n * s[0], 0, 0, s[1], 0, 0, n * s[2], 0, c * s[2], 0, pos[0], pos[1], pos[2], 1]);
  }

  const textures = new Map();
  const blank = gl.createTexture();
  gl.bindTexture(gl.TEXTURE_2D, blank);
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0, gl.RGBA, gl.UNSIGNED_BYTE, new Uint8Array([255, 255, 255, 255]));
  function texture(key) {
    if (!key) return blank;
    let t = textures.get(key);
    if (t) return t.ready ? t.gl : blank;
    t = { gl: gl.createTexture(), ready: false };
    textures.set(key, t);
    const image = new Image();
    image.onload = () => {
      gl.bindTexture(gl.TEXTURE_2D, t.gl);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
      // The game's: nearest texels, clamped (eld's textures are small and meant to be seen as pixels).
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      t.ready = true;
    };
    image.src = textureUrl(key);
    return blank;
  }

  const camera = { yaw: 0, pitch: 16, distance: 52, target: [0, 1, -3] };
  function matrices() {
    const w = canvas.clientWidth || 1, h = canvas.clientHeight || 1;
    const ratio = window.devicePixelRatio || 1;
    if (canvas.width !== Math.round(w * ratio) || canvas.height !== Math.round(h * ratio)) { canvas.width = Math.round(w * ratio); canvas.height = Math.round(h * ratio); }
    const jolt = camera.jolt || [0, 0];
    if (stageApi.override) {
      // A script's camera (a summon's): its eye and target as the battle's camera has them, through the battle's lens.
      const o = stageApi.override;
      return { view: effectLookAt(o.eye, o.at), projection: effectPerspective(BATTLE_FOV * EFFECT_DEG, w / h, 5, 2000) };
    }
    if (layout.view === 'battle') {
      // The battle's camera as a spell plays (CameraBattlePosition/Target), its field of view as the port draws it.
      const look = [BATTLE_AT[0], BATTLE_AT[1] + jolt[1], BATTLE_AT[2] + jolt[0]];
      return { view: effectLookAt(BATTLE_EYE, look), projection: effectPerspective(BATTLE_FOV * EFFECT_DEG, w / h, 5, 2000) };
    }
    const yaw = camera.yaw * EFFECT_DEG, pitch = camera.pitch * EFFECT_DEG;
    const look = [camera.target[0] + jolt[0], camera.target[1] + jolt[1], camera.target[2]];
    const eye = [look[0] + camera.distance * Math.cos(pitch) * Math.sin(yaw), look[1] + camera.distance * Math.sin(pitch), look[2] + camera.distance * Math.cos(pitch) * Math.cos(yaw)];
    return { view: effectLookAt(eye, look), projection: effectPerspective((camera.fov || FREE_FOV) * EFFECT_DEG, w / h, 0.5, 2000) };
  }

  // The free camera, as the scene view's (map-editor.js wireSceneInput): a left drag orbits where it looks, the middle
  // button (or Shift and a drag) pans it in the screen's plane, the right button held flies - the mouse turns it on
  // the spot, W A S D walk, Q and E go down and up, Shift hurries - the wheel comes closer, and F or a double click
  // looks at the effect's first target. Leaving the battle's camera starts it where the battle's looks from, so the
  // picture does not jump.
  const FREE_FOV = 40, BATTLE_FOV = 21.4;
  const backOf = () => {
    const yaw = camera.yaw * EFFECT_DEG, pitch = camera.pitch * EFFECT_DEG;
    return [Math.cos(pitch) * Math.sin(yaw), Math.sin(pitch), Math.cos(pitch) * Math.cos(yaw)];
  };
  const eyeOf = () => { const k = backOf(); return camera.target.map((v, i) => v + k[i] * camera.distance); };
  function freed() {
    if (layout.view !== 'battle') return;
    // Looking as the battle's camera looks, through its lens, from where it stands: the picture stays as it was, and an
    // orbit swings round a point at the depth of the first target.
    const f = [BATTLE_AT[0] - BATTLE_EYE[0], BATTLE_AT[1] - BATTLE_EYE[1], BATTLE_AT[2] - BATTLE_EYE[2]];
    const l = Math.hypot(f[0], f[1], f[2]) || 1;
    const fw = f.map(v => v / l);
    const at = anchors[0] || BATTLE_AT;
    const depth = Math.max(10, (at[0] - BATTLE_EYE[0]) * fw[0] + (at[1] - BATTLE_EYE[1]) * fw[1] + (at[2] - BATTLE_EYE[2]) * fw[2]);
    camera.target = BATTLE_EYE.map((v, i) => v + fw[i] * depth);
    camera.distance = depth;
    camera.fov = BATTLE_FOV;
    camera.pitch = Math.asin(-fw[1]) / EFFECT_DEG;
    camera.yaw = Math.atan2(-fw[0], -fw[2]) / EFFECT_DEG;
    layout.view = 'free';
    if (stageApi.onView) stageApi.onView('free');
  }
  function frameTarget() {
    freed();
    camera.target = (anchors[0] || [0, 0, 0]).slice();
    // Far enough that a monster fills a good part of the view, through whatever lens the camera has.
    camera.distance = 44 * Math.tan(FREE_FOV / 2 * EFFECT_DEG) / Math.tan((camera.fov || FREE_FOV) / 2 * EFFECT_DEG);
  }
  let dragging = null, flying = false, flight = 0;
  const held = new Set();
  function flyStep() {
    if (!flying) return;
    const ahead = (held.has('w') ? 1 : 0) - (held.has('s') ? 1 : 0);
    const sideways = (held.has('d') ? 1 : 0) - (held.has('a') ? 1 : 0);
    const upward = (held.has('e') ? 1 : 0) - (held.has('q') ? 1 : 0);
    if (ahead || sideways || upward) {
      const k = backOf(), forward = k.map(v => -v);
      const right = [Math.cos(camera.yaw * EFFECT_DEG), 0, -Math.sin(camera.yaw * EFFECT_DEG)];
      const step = Math.max(0.3, camera.distance * 0.02) * (held.has('shift') ? 4 : 1);
      for (let i = 0; i < 3; i++) camera.target[i] += (forward[i] * ahead + right[i] * sideways + (i === 1 ? upward : 0)) * step;
    }
    flight = requestAnimationFrame(flyStep);
  }
  const onKey = e => {
    if (!flying) return;
    const key = e.key.toLowerCase();
    if ('wasdqe'.includes(key) || key === 'shift') {
      e.preventDefault();
      if (e.type === 'keydown') held.add(key); else held.delete(key);
    }
  };
  function stopFlying() {
    if (!flying) return;
    flying = false;
    held.clear();
    cancelAnimationFrame(flight);
    window.removeEventListener('keydown', onKey);
    window.removeEventListener('keyup', onKey);
    canvas.style.cursor = '';
  }
  canvas.tabIndex = 0;   // for F
  canvas.addEventListener('mousedown', e => { if (e.button === 1) e.preventDefault(); });   // no browser autoscroll
  canvas.addEventListener('pointerdown', e => {
    canvas.focus({ preventScroll: true });
    canvas.setPointerCapture(e.pointerId);
    if (e.button === 1) e.preventDefault();
    freed();
    if (e.button === 2) {
      flying = true;
      canvas.style.cursor = 'none';
      window.addEventListener('keydown', onKey);
      window.addEventListener('keyup', onKey);
      flight = requestAnimationFrame(flyStep);
      dragging = { x: e.clientX, y: e.clientY, button: 2 };
      return;
    }
    dragging = { x: e.clientX, y: e.clientY, button: e.button, pan: e.button === 1 || e.shiftKey };
  });
  canvas.addEventListener('pointermove', e => {
    if (!dragging) return;
    const dx = e.clientX - dragging.x, dy = e.clientY - dragging.y;
    dragging.x = e.clientX; dragging.y = e.clientY;
    if (flying) {
      // Turning on the spot: the eye stays put and the view swings round it.
      const eye = eyeOf();
      camera.yaw -= dx * 0.3;
      camera.pitch = Math.max(-85, Math.min(85, camera.pitch + dy * 0.25));
      const k = backOf();
      camera.target = eye.map((v, i) => v - k[i] * camera.distance);
      return;
    }
    if (dragging.pan) {
      // The camera's right and up in the world, and a pixel's worth of world at the distance it looks from.
      const k = backOf();
      const right = [Math.cos(camera.yaw * EFFECT_DEG), 0, -Math.sin(camera.yaw * EFFECT_DEG)];
      const up = [k[1] * right[2] - k[2] * right[1], k[2] * right[0] - k[0] * right[2], k[0] * right[1] - k[1] * right[0]];
      const perPixel = 2 * camera.distance * Math.tan((camera.fov || FREE_FOV) / 2 * EFFECT_DEG) / Math.max(1, canvas.clientHeight);
      for (let i = 0; i < 3; i++) camera.target[i] += (-right[i] * dx + up[i] * dy) * perPixel;
    } else {
      camera.yaw -= dx * 0.4;
      camera.pitch = Math.max(-85, Math.min(85, camera.pitch + dy * 0.3));
    }
  });
  const letGo = e => {
    if (e && e.button === 2) stopFlying();
    if (!e || e.type === 'pointercancel') stopFlying();
    dragging = null;
  };
  canvas.addEventListener('pointerup', letGo);
  canvas.addEventListener('pointercancel', letGo);
  window.addEventListener('blur', () => { stopFlying(); dragging = null; });
  canvas.addEventListener('contextmenu', e => e.preventDefault());
  canvas.addEventListener('wheel', e => {
    e.preventDefault();
    // A middle button held (a pan) nudges the wheel on many mice: not a zoom.
    if (dragging && dragging.pan && dragging.button === 1) return;
    freed();
    camera.distance = Math.max(3, Math.min(400, camera.distance * (e.deltaY > 0 ? 1.1 : 0.9)));
  }, { passive: false });
  canvas.addEventListener('keydown', e => { if (!flying && e.key.toLowerCase() === 'f' && !e.ctrlKey && !e.metaKey && !e.altKey) { e.preventDefault(); frameTarget(); } });
  canvas.addEventListener('dblclick', () => frameTarget());
  canvas.addEventListener('wheel', e => { e.preventDefault(); freed(); camera.distance = Math.max(8, Math.min(200, camera.distance * (e.deltaY > 0 ? 1.1 : 0.9))); }, { passive: false });

  /// Who the effect plays on: { model (a game model's name, or none), scale, toward, up, turn, party, at (a place of its own) }.
  function setTarget(t) {
    target = { toward: t.toward || 0, up: t.up || 0, scale: t.scale || 1, turn: t.turn === undefined ? 90 : t.turn, model: t.model || null, party: !!t.party, at: t.at || null };
    relayout();
  }
  /// How many, the battle map, the camera: { count: 'one' | 'group' | 'all', background: 'files/bNN.nmdp.lz' or '', view: 'battle' | 'free' }.
  function setLayout(next) {
    // From the battle's camera to the free one: it starts where the battle's looks from.
    if (layout.view === 'battle' && next.view === 'free' && typeof freed === 'function') freed();
    layout = { ...layout, ...next, show: { ...SHOW_ALL, ...layout.show, ...(next.show || {}) } };
    relayout();
  }

  // The caster: the party's first, in the front row; its hit point (9 toward the camera, 5 up) is the caster anchor.
  function caster() {
    const feet = PARTY_PLACES[0];
    return { feet, anchor: hitPoint(feet, 9, 5) };
  }

  function draw(quads, between, meshes = []) {
    const { view, projection } = matrices();
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.clearColor(0.078, 0.086, 0.102, 1);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.enable(gl.DEPTH_TEST);
    gl.depthFunc(gl.LEQUAL);
    gl.depthMask(true);
    gl.disable(gl.BLEND);

    gl.useProgram(lines);
    gl.uniformMatrix4fv(lineUnif.view, false, view);
    gl.uniformMatrix4fv(lineUnif.projection, false, projection);
    gl.bindBuffer(gl.ARRAY_BUFFER, lineBuffer);
    gl.enableVertexAttribArray(lineAttr.position);
    gl.vertexAttribPointer(lineAttr.position, 3, gl.FLOAT, false, 28, 0);
    gl.enableVertexAttribArray(lineAttr.colour);
    gl.vertexAttribPointer(lineAttr.colour, 4, gl.FLOAT, false, 28, 12);
    gl.drawArrays(gl.LINES, 0, lineCount);
    gl.disableVertexAttribArray(lineAttr.position);
    gl.disableVertexAttribArray(lineAttr.colour);

    // The scene's models - the battle map, the targets, the caster, the effect's own - opaque first, then the translucent over them.
    const scene = [];
    const show = layout.show || SHOW_ALL;
    if (layout.background && show.map) {
      const bg = layout.background;
      const m = loadModel('bg:' + bg, `/api/model?name=${encodeURIComponent(bg)}`,
        tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(bg)}&texture=${encodeURIComponent(tex)}`));
      if (m.anim === undefined) {
        m.anim = null;
        api(`/api/model/animation?name=${encodeURIComponent(bg)}`).then(r => { if (r && !r.none && (r.srt || r.alpha)) m.anim = r; }).catch(() => {});
      }
      scene.push({ m, world: placed([0, 0, 0], 1, 0), frame: Math.floor(performance.now() / 1000 * 30), opts: { tint: stageApi.mapTint || undefined } });
    }
    // The monsters the effect plays on (a post stands in for none).
    if (target.model && !target.party && show.monsters) {
      const m = loadModel('target:' + target.model, `/api/model?name=${encodeURIComponent(target.model)}`,
        tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(target.model)}&texture=${encodeURIComponent(tex)}`));
      standing(m, target.model);
      if (!(stageApi.hide && stageApi.hide.monsters)) for (const f of figures) scene.push({ m, world: placed(f.feet, target.scale, f.turn), frame: Math.floor(performance.now() / 1000 * 30) });
    }
    // The party: the four heroes in the front row as Onion Knights (j101, j201, j301, j401 - the battle's j<hero><job>),
    // the first the caster; the effect plays on them when the target is the party.
    PARTY_PLACES.forEach((feet, k) => {
      // Which of them: all four, the caster and those the effect plays on, the caster, or none.
      const targeted = target.party && figures.some(f => f.feet === feet);
      if (show.heroes === 'none' || (show.heroes === 'caster' && k > 0) || (show.heroes === 'targets' && k > 0 && !targeted)) return;
      const hero = `files/j${k + 1}01.nmdp.lz`;
      const m = loadModel('hero:' + hero, `/api/model?name=${encodeURIComponent(hero)}`,
        tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(hero)}&texture=${encodeURIComponent(tex)}`));
      standing(m, hero);
      const partyAlpha = stageApi.hide && stageApi.hide.party !== undefined ? stageApi.hide.party : 1;
      if (partyAlpha > 0) scene.push({ m, world: placed(feet, 1, -90), frame: Math.floor(performance.now() / 1000 * 30) + k * 7, opts: { alpha: partyAlpha } });
    });
    // The caller's own models (a summon): a game model at a place, turned and scaled, posed by a motion of a pack, faded.
    for (const x of stageApi.extras || []) {
      if (!x || !x.model) continue;
      const m = loadModel('extra:' + x.model, `/api/model?name=${encodeURIComponent(x.model)}`,
        tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(x.model)}&texture=${encodeURIComponent(tex)}`));
      let pose = null;
      if (x.pack) {
        m.poses = m.poses || new Map();
        const key = x.pack + '#' + x.index;
        if (!m.poses.has(key)) {
          m.poses.set(key, null);
          api(`/api/model/pose?name=${encodeURIComponent(x.model)}&pack=${encodeURIComponent(x.pack)}&index=${x.index || 0}`)
            .then(p => { if (p && p.matrices) m.poses.set(key, p); }).catch(() => {});
        }
        pose = m.poses.get(key);
      }
      scene.push({ m, world: placed(x.pos, x.scale || 1, x.yaw || 0), frame: x.frame || 0, opts: { pose, loop: !!x.loop, alpha: x.alpha === undefined ? 1 : x.alpha } });
    }
    for (const e of meshes) {
      const k = /^game:([^:]+):(.+)$/.exec(e.track.model || '');
      let m;
      if (k) {
        m = loadModel(e.track.model, `/api/effect/model?pack=${encodeURIComponent(k[1])}&id=${encodeURIComponent(k[2])}`,
          tex => wsUrl(`/api/effect/model/texture?pack=${encodeURIComponent(k[1])}&id=${encodeURIComponent(k[2])}&name=${encodeURIComponent(tex)}`));
      } else if (e.track.model) {
        // A glTF of the mod's (a path from the project's folder), in its bind pose on the Stage.
        const path = e.track.model;
        m = loadModel('gltf:' + path, `/api/model?name=${encodeURIComponent(path)}`,
          tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(path)}&texture=${encodeURIComponent(tex)}`));
      } else continue;
      // A model without a loop ends with its motion (eld.ImpModelDS: StopToDead at the motion's end).
      if (m.ready && m.pose && !m.loop && e.frame >= m.pose.frames) continue;
      const pos = [0, 1, 2].map(i => e.prev[i] + (e.pos[i] - e.prev[i]) * between);
      scene.push({ m, world: placed(pos, e.scale === undefined ? (e.track.scale || 1) : e.scale, e.yaw || 0), frame: e.frame });
    }
    for (const it of scene) drawModel(it.m, it.world, it.frame, false, view, projection, it.opts);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    gl.depthMask(false);
    for (const it of scene) drawModel(it.m, it.world, it.frame, true, view, projection, it.opts);
    gl.depthMask(true);
    gl.disable(gl.BLEND);

    if (!quads.length) return;
    // Particles as the game draws them: after the scene, alpha-blended, tested against its depth and never writing it.
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    gl.depthMask(false);
    gl.useProgram(program);
    gl.uniformMatrix4fv(unif.view, false, view);
    gl.uniformMatrix4fv(unif.projection, false, projection);
    gl.uniform1f(unif.between, between);
    gl.uniform1i(unif.picture, 0);
    const stride = 24;
    const data = new Float32Array(quads.length * 6 * stride);
    const corners = [[-1, 1, 0, 0], [-1, -1, 0, 1], [1, -1, 1, 1], [-1, 1, 0, 0], [1, -1, 1, 1], [1, 1, 1, 0]];
    let n = 0;
    for (const q of quads) {
      const tex = q.tex || {}, width = tex.width || 1, height = tex.height || 1;
      const cell = tex.cell || [0, 0, width, height], cols = tex.columns || 0;
      const gx = cols ? q.cell % cols : 0, gy = cols ? Math.floor(q.cell / cols) : 0;
      const u0 = (cell[0] + cell[2] * gx) / width, v0 = (cell[1] + cell[3] * gy) / height;
      const u1 = u0 + cell[2] / width, v1 = v0 + cell[3] / height;
      for (const [cx, cy, su, sv] of corners) {
        data.set([q.pos[0], q.pos[1], q.pos[2], q.prev[0], q.prev[1], q.prev[2], cx, cy, q.w, q.h, su ? u1 : u0, sv ? v1 : v0,
          q.colour[0] / 255, q.colour[1] / 255, q.colour[2] / 255, q.colour[3] / 255, q.roll || 0, q.rollBefore || 0,
          q.wBefore === undefined ? q.w : q.wBefore, q.hBefore === undefined ? q.h : q.hBefore,
          ...(q.colourBefore || q.colour).map(c => c / 255)], n);
        n += stride;
      }
    }
    gl.bindBuffer(gl.ARRAY_BUFFER, quadBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.DYNAMIC_DRAW);
    const bind = (loc, size, offset) => { gl.enableVertexAttribArray(loc); gl.vertexAttribPointer(loc, size, gl.FLOAT, false, stride * 4, offset * 4); };
    bind(attr.centre, 3, 0); bind(attr.before, 3, 3); bind(attr.corner, 2, 6); bind(attr.extent, 2, 8); bind(attr.uv, 2, 10); bind(attr.colour, 4, 12); bind(attr.roll, 2, 16); bind(attr.extentBefore, 2, 18); bind(attr.colourBefore, 4, 20);
    gl.activeTexture(gl.TEXTURE0);
    // Runs of one texture and one blend, in order - blending needs the game's order, not one batch per texture.
    // Additive (render.blend) adds the quad's light to what is behind it, as the client draws it (SourceAlpha, One).
    const same = (a, b) => (a.tex || {}).image === (b.tex || {}).image && (a.blend === 'additive') === (b.blend === 'additive') && (a.tint === 'recolour') === (b.tint === 'recolour');
    let from = 0;
    for (let i = 1; i <= quads.length; i++) {
      if (i < quads.length && same(quads[i], quads[from])) continue;
      gl.blendFunc(gl.SRC_ALPHA, quads[from].blend === 'additive' ? gl.ONE : gl.ONE_MINUS_SRC_ALPHA);
      gl.uniform1i(unif.recolour, quads[from].tint === 'recolour' ? 1 : 0);
      gl.bindTexture(gl.TEXTURE_2D, texture((quads[from].tex || {}).image));
      gl.drawArrays(gl.TRIANGLES, from * 6, (i - from) * 6);
      from = i;
    }
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    for (const loc of Object.values(attr)) gl.disableVertexAttribArray(loc);
    gl.depthMask(true);
  }

  const stageApi = {
    draw, camera, setTarget, setLayout, caster, onView: null,
    get targets() { return anchors.map(a => a.slice()); }, get layout() { return { ...layout }; },
    // For a caller that plays the battle itself (summons.js): its camera, its models, the map's tint, who is hidden.
    override: null, extras: [], mapTint: null, hide: null,
    /// Where the battle plays an effect on each: the monsters standing (their hit points) and the party's four.
    places() {
      return {
        monsters: target.party ? [] : figures.map(f => ({ feet: f.feet, hit: hitPoint(f.feet, target.toward, target.up) })),
        party: PARTY_PLACES.map(feet => ({ feet, hit: hitPoint(feet, 9, 5) }))
      };
    },
    BATTLE_EYE, BATTLE_AT, ALL_MONSTERS, ALL_PARTY
  };
  relayout();
  return stageApi;
}

// ------------------------------------------------------------------ the list's preview

/// An effect played in a small Stage beside its row in the list while the pointer rests there: its
/// member 1 (what a spell names), or the mod's own as written - on a post, from a free camera, looping.
const effectHoverState = { box: null, stage: null, own: null, player: null, name: null, timer: 0, defs: new Map(), running: false };
function effectHover(item, name) {
  item.addEventListener('mouseenter', () => {
    clearTimeout(effectHoverState.timer);
    effectHoverState.timer = setTimeout(() => effectHoverShow(item, name), 350);
  });
  item.addEventListener('mouseleave', () => { clearTimeout(effectHoverState.timer); effectHoverHide(); });
  item.addEventListener('mousedown', () => { clearTimeout(effectHoverState.timer); effectHoverHide(); });
}

async function effectHoverDef(name) {
  const h = effectHoverState;
  if (h.defs.has(name)) return h.defs.get(name);
  const pack = await api(`/api/effect?name=${encodeURIComponent(name)}`);
  let def = null;
  if (pack && pack.own) def = pack.effect;
  else if (pack && !pack.error) {
    const members = (pack.templates || []).filter(t => t.category >= 0 && t.member > 0);
    const m = members.find(t => t.category === pack.category && t.member === 1) || members[0];
    if (m) { const r = await api(`/api/effect/import?category=${m.category}&member=${m.member}`); if (r && !r.error) def = r.effect; }
  }
  h.defs.set(name, def ? { def, own: Boolean(pack.own) } : null);
  return h.defs.get(name);
}

async function effectHoverShow(item, name) {
  const h = effectHoverState;
  if (!item.isConnected) return;
  if (!h.box) {
    h.box = document.createElement('div');
    h.box.className = 'effect-hover';
    const canvas = document.createElement('canvas');
    h.label = document.createElement('span');
    h.box.append(canvas, h.label);
    document.body.append(h.box);
    h.stage = makeEffectStage(canvas, key => effectTextureUrl(key, h.own));
    if (!h.stage) return;
    h.stage.setLayout({ count: 'one', background: '', view: 'free' });
    Object.assign(h.stage.camera, { yaw: 60, pitch: 14, distance: 44 });
  }
  h.name = name;
  h.item = item;
  h.label.textContent = shortName(name) + ' · loading';
  const r = item.getBoundingClientRect();
  h.box.style.left = Math.max(8, Math.min(window.innerWidth - 300, r.left + 40)) + 'px';
  h.box.style.top = Math.max(8, r.top - 200) + 'px';
  h.box.hidden = false;
  const got = await effectHoverDef(name).catch(() => null);
  if (h.name !== name || h.box.hidden) return;
  if (!got) { h.label.textContent = shortName(name) + ' · nothing to play'; h.player = null; return; }
  h.own = got.own ? name : null;
  h.label.textContent = shortName(name);
  const caster = h.stage.caster().anchor, at = h.stage.targets[0];
  h.player = makeEffectGroup(got.def, h.stage.targets, { seed: 7, anchors: { caster } });
  // An effect that plays on the caster too (a bolt from it) is looked at from between the two.
  const both = (got.def.tracks || []).some(t => t.anchor === 'caster' || t.anchor === 'between' || (t.path && (t.path.from || t.path.to)));
  Object.assign(h.stage.camera, both ? { target: at.map((v, k) => (v + caster[k]) / 2), distance: 95, yaw: 20, pitch: 18 } : { target: at, distance: 44, yaw: 60, pitch: 14 });
  if (!h.running) {
    h.running = true;
    let last = performance.now(), owed = 0;
    const loop = now => {
      // Gone with its row (the list redrawn, another view) - the pointer never left it.
      if (h.item && !h.item.isConnected) effectHoverHide();
      if (h.box.hidden) { h.running = false; return; }
      owed += Math.min(0.25, (now - last) / 1000) * 30;
      last = now;
      if (h.player) {
        while (owed >= 1) { owed -= 1; if (h.player.finished) h.player.reset(); h.player.step(); }
        h.stage.draw(h.player.particles(), Math.max(0, Math.min(1, owed)), h.player.models());
      } else h.stage.draw([], 1, []);
      requestAnimationFrame(loop);
    };
    requestAnimationFrame(loop);
  }
}

function effectHoverHide() {
  const h = effectHoverState;
  h.name = null;
  if (h.box) h.box.hidden = true;
}

// ------------------------------------------------------------------ the view

/// "game:e331.efp:fire_tubu_32bit.tga" as the server's texture address; a PNG of the mod's, beside its definition (own).
function effectTextureUrl(key, own) {
  const m = /^game:([^:]+):(.+)$/.exec(key || '');
  if (!m) return own && key ? wsUrl(`/api/effect/own-texture?effect=${encodeURIComponent(own)}&name=${encodeURIComponent(key)}`) : '';
  return wsUrl(`/api/effect/texture?pack=${encodeURIComponent(m[1])}&name=${encodeURIComponent(m[2])}`);
}

/// What the Stage stood at the hit point last, for the next effect opened: the Goblin to start with.
let effectTarget = '1';
// The Stage's layout, kept from one effect to the next: how many targets, the battle map, the camera.
let effectLayout = { count: 'one', background: '', view: 'battle', show: { map: true, monsters: true, heroes: 'all', floor: true, marks: true } };

async function openEffect(name) {
  const doc = activeDoc;
  const node = view('effect', name, false);
  const facts = $('.facts', node);
  const pick = $('.effect-member', node);
  const body = $('.effect-body', node);

  const pack = await api(`/api/effect?name=${encodeURIComponent(name)}`);
  setDocData(pack);
  if (pack.error) { facts.textContent = pack.error; return; }

  // The members of the pack's own category first (1 is what a spell names), then its other templates by where effect.efi puts them.
  const members = (pack.templates || []).filter(t => t.category >= 0 && t.member > 0)
    .sort((a, b) => (a.category === pack.category ? 0 : 1) - (b.category === pack.category ? 0 : 1) || a.category - b.category || a.member - b.member);
  for (const t of members) {
    const o = document.createElement('option');
    o.value = `${t.category}/${t.member}`;
    o.textContent = `${t.category} / ${t.member} · ${t.kind}${t.texture ? ' · ' + t.texture.replace(/\.tg?a?$/i, '') : ''}${t.used && t.used.length ? ' · ' + t.used.join(', ') : ''}`;
    pick.append(o);
  }
  facts.textContent = `${pack.templates.length} template(s), ${pack.textures.length} texture(s)` + (pack.note ? `  ·  ${pack.note}` : '');
  // The mod's own: a badge, not a sentence.
  if (pack.own && typeof ownBadge === 'function') { facts.textContent = ''; facts.append(ownBadge('mod')); }

  const stageBox = document.createElement('div');
  stageBox.className = 'effect-stage';
  const canvas = document.createElement('canvas');
  canvas.className = 'scene';
  // A flash track's colour over the Stage (the game's screen flash), and a shake's jolt of the camera.
  const flash = document.createElement('div');
  flash.className = 'effect-flash';
  stageBox.append(canvas, flash);
  let flashLeft = 0, flashOn = 0, flashOff = 0, flashColour = '#fff', shakeLeft = 0, shakePower = 0;
  function playStarted() {
    for (const t of player.started || []) {
      const frames = t.frames || 8;
      if (t.type === 'flash') {
        const c = t.colour || [255, 255, 255];
        flashColour = `rgb(${c[0] | 0}, ${c[1] | 0}, ${c[2] | 0})`;
        flashOn = frames; flashOff = t.interval === undefined ? 2 : t.interval;
        flashLeft = (flashOn + flashOff) * Math.max(1, t.count || 1);
      } else if (t.type === 'shake') { shakeLeft = frames; shakePower = t.power === undefined ? 0.25 : t.power; }
    }
  }
  function stepScreen() {
    if (flashLeft > 0) {
      const at = (flashOn + flashOff) - (flashLeft % (flashOn + flashOff || 1));
      flash.style.background = flashColour;
      flash.style.opacity = at <= flashOn ? '0.8' : '0';
      flashLeft--;
    } else flash.style.opacity = '0';
    if (shakeLeft > 0) { shakeLeft--; stage.camera.jolt = [(Math.random() - 0.5) * 2 * shakePower, (Math.random() - 0.5) * 2 * shakePower]; }
    else stage.camera.jolt = null;
  }
  const controls = document.createElement('div');
  controls.className = 'effect-controls bar';
  const button = (text, title) => { const b = document.createElement('button'); b.textContent = text; b.title = title; controls.append(b); return b; };
  const play = button('Pause', 'play or pause (space)');
  const stepOne = button('Step', 'one game step (a thirtieth of a second)');
  const again = button('Restart', 'from the first frame');
  const scrub = Object.assign(document.createElement('input'), { type: 'range', min: 0, max: 60, value: 0, className: 'effect-scrub', title: 'the frame' });
  controls.append(scrub);
  const label = document.createElement('span');
  label.className = 'effect-frame';
  controls.append(label);
  const loopBox = Object.assign(document.createElement('label'), { className: 'toggle', title: 'start again when it ends' });
  const loop = Object.assign(document.createElement('input'), { type: 'checkbox', checked: true });
  loopBox.append(loop, ' loop');
  const fpsPick = document.createElement('select');
  fpsPick.title = 'the game steps 30 times a second; at 60 a particle is drawn between its two steps, as the client does';
  for (const f of [30, 60]) { const o = document.createElement('option'); o.value = String(f); o.textContent = `${f} fps`; fpsPick.append(o); }
  fpsPick.value = '60';
  const count = document.createElement('span');
  count.className = 'effect-count';
  const targetPick = document.createElement('select');
  targetPick.title = 'what stands at the hit point: the effect plays where the battle plays it on this one';
  for (const [value, text] of [['post', 'a post'], ['-1', 'the party']]) { const o = document.createElement('option'); o.value = value; o.textContent = text; targetPick.append(o); }
  const pickOf = (title, options) => {
    const s = document.createElement('select');
    s.title = title;
    for (const [value, text] of options) { const o = document.createElement('option'); o.value = value; o.textContent = text; s.append(o); }
    return s;
  };
  const countPick = pickOf('how many it plays on: one; each of a group, one after another as the battle plays a spell cast on all; or once for the whole side, as a spell that can only hit all',
    [['one', '1 target'], ['group', 'each of a group'], ['all', 'the whole side']]);
  const bgPick = pickOf('the battle map behind them (bNN, as the battle loads it)', [['', 'no map']]);
  const viewPick = pickOf('the battle\'s camera as a spell plays, or free (drag to turn, the wheel to come closer)', [['battle', 'battle camera'], ['free', 'free camera']]);
  // What the Stage shows: chips to switch the map, the monsters, the floor and the marks, which heroes, and the two ends at once.
  const showRow = document.createElement('div');
  showRow.className = 'effect-show';
  const showLabel = document.createElement('span');
  showLabel.textContent = 'Show';
  showRow.append(showLabel);
  const chips = {};
  const chip = (key, text, title) => {
    const b = document.createElement('button');
    b.className = 'chip';
    b.textContent = text;
    b.title = title;
    b.onclick = () => { effectLayout.show[key] = !effectLayout.show[key]; showChanged(); };
    chips[key] = b;
    showRow.append(b);
  };
  chip('map', 'Map', 'the battle map picked beside it');
  chip('monsters', 'Monsters', 'the monsters (or posts) the effect plays on');
  const heroPick = pickOf('which of the party stand in the front row', [['all', 'All heroes'], ['targets', 'Caster + targets'], ['caster', 'Caster'], ['none', 'No heroes']]);
  heroPick.onchange = () => { effectLayout.show.heroes = heroPick.value; showChanged(); };
  showRow.append(heroPick);
  chip('floor', 'Floor', 'the grid of the battle\'s floor, where no map is drawn');
  chip('marks', 'Marks', 'a cross where the effect plays');
  const only = document.createElement('button');
  only.className = 'chip';
  only.textContent = 'Effect only';
  only.title = 'nothing but the effect, over the dark';
  only.onclick = () => { effectLayout.show = { map: false, monsters: false, heroes: 'none', floor: false, marks: false }; showChanged(); };
  const all = document.createElement('button');
  all.className = 'chip';
  all.textContent = 'Everything';
  all.title = 'the map, the monsters, the four heroes, the floor and the marks';
  all.onclick = () => { effectLayout.show = { map: true, monsters: true, heroes: 'all', floor: true, marks: true }; showChanged(); };
  showRow.append(only, all);
  function showSync() {
    for (const [key, b] of Object.entries(chips)) b.classList.toggle('on', !!effectLayout.show[key]);
    heroPick.value = effectLayout.show.heroes || 'all';
  }
  function showChanged() { showSync(); stage.setLayout({ show: effectLayout.show }); }
  controls.append(loopBox, fpsPick, targetPick, countPick, bgPick, viewPick, count, showRow);
  const notes = document.createElement('details');
  notes.className = 'effect-notes';
  const summary = document.createElement('summary');
  summary.textContent = 'definition';
  const json = document.createElement('pre');
  notes.append(summary, json);
  // The editor's timeline (effects-editor.js) sits between the controls and the definition.
  const timelineBox = document.createElement('div');
  timelineBox.className = 'effect-timeline-box';
  body.append(stageBox, controls, timelineBox, notes);

  const stage = makeEffectStage(canvas, key => effectTextureUrl(key, pack.own ? name : null));
  if (!stage) { facts.textContent = 'this browser has no WebGL, so effects cannot be drawn'; return; }
  stage.setLayout(effectLayout);
  showSync();
  countPick.value = effectLayout.count;
  viewPick.value = effectLayout.view;
  const relaid = () => {
    effectLayout = { ...effectLayout, count: countPick.value, background: bgPick.value, view: viewPick.value };
    stage.setLayout(effectLayout);
    if (def) applyDef(def, null, { restart: true, keepNotes: true });
  };
  countPick.onchange = relaid; bgPick.onchange = relaid; viewPick.onchange = relaid;
  stage.onView = v => { viewPick.value = v; effectLayout.view = v; };
  // The battle maps: b01 ... b43 (the land forms' and the events' battles).
  api('/api/models').then(list => {
    for (const m of list || []) {
      const n = typeof m === 'string' ? m : (m.name || m.Name || '');
      const k = /(^|\/)b(\d\d)\.nmdp\.lz$/i.exec(n);
      if (!k) continue;
      const o = document.createElement('option'); o.value = n; o.textContent = 'battle map ' + k[2]; bgPick.append(o);
    }
    bgPick.value = effectLayout.background;
    if (bgPick.value !== effectLayout.background) bgPick.value = '';
  }).catch(() => {});

  // The monsters to stand at the hit point; the Goblin to start with.
  async function setTarget(value) {
    effectTarget = value;
    if (value === 'post') { stage.setTarget({ toward: 9, up: 5 }); }
    else {
      const t = await api(`/api/effect/target?monster=${encodeURIComponent(value)}`);
      if (t.error) { say(t.error, 'bad'); return; }
      stage.setTarget(t);
    }
    // The places moved: the effect plays at the new ones.
    if (def) applyDef(def, null, { restart: true, keepNotes: true });
  }
  targetPick.onchange = () => setTarget(targetPick.value);
  if (typeof monstersForPicker === 'function') {
    monstersForPicker().then(list => {
      const group = document.createElement('optgroup');
      group.label = 'monsters';
      for (const m of list.filter(x => !x.mod)) { const o = document.createElement('option'); o.value = String(m.id); o.textContent = m.name; group.append(o); }
      targetPick.append(group);
      targetPick.value = effectTarget;
      if (targetPick.value !== effectTarget) targetPick.value = 'post';
      setTarget(targetPick.value);
    }).catch(() => {});
  }

  let player = null, def = null, playing = true, last = performance.now(), owed = 0, length = 60;

  async function load(value) {
    let r;
    if (pack.own) r = { effect: pack.effect, notes: [] };   // the mod's own: its definition as written
    else {
      const [category, member] = value.split('/').map(n => parseInt(n, 10));
      r = await api(`/api/effect/import?category=${category}&member=${member}`);
    }
    if (r.error) { say(r.error, 'bad'); return; }
    applyDef(r.effect, r.notes, { restart: true });
  }

  /// A definition on the Stage: a new one from the start, or an edit of the one playing, on the frame it was at.
  function applyDef(next, notesOf = [], { restart = false, keepNotes = false } = {}) {
    const at = player ? Math.max(0, player.frame) : 0;
    def = next;
    if (!keepNotes) json.textContent = JSON.stringify(def, null, 1).replace(/\[\s+([-\d.,\s]+?)\s+\]/g, (m, inner) => '[' + inner.replace(/\s+/g, ' ').trim() + ']')
      + (notesOf && notesOf.length ? '\n\n' + notesOf.map(n => '// ' + n).join('\n') : '');
    const anchors = { caster: stage.caster().anchor };
    // One player a target; a group's each half the effect's length after the one before, as the battle staggers them.
    const stagger = stage.layout.count === 'group' ? Math.max(1, Math.floor((def.length || 30) / 2)) : 0;
    const make = () => makeEffectGroup(def, stage.targets, { seed: 7, anchors, stagger });
    player = make();
    // How long a pass takes: played through once, to its end.
    const probe = make();
    let n = 0;
    if (def.loop) n = (def.length || 0) + 1;
    else while (n < 900 && !probe.finished) { probe.step(); n++; }
    length = Math.max(1, n);
    scrub.max = String(length);
    if (restart) {
      owed = 0;
      playing = true;
      play.textContent = 'Pause';
    } else if (!playing) goTo(Math.min(at, length));
    else { for (let i = 0; i <= Math.min(at, length); i++) player.step(); }
    if (doc && doc.effectView && doc.effectView.onApply) doc.effectView.onApply();
  }

  function goTo(frame) {
    if (!player) return;
    player.reset();
    for (let i = 0; i <= frame; i++) player.step();
  }

  play.onclick = () => { playing = !playing; play.textContent = playing ? 'Pause' : 'Play'; };
  stepOne.onclick = () => { playing = false; play.textContent = 'Play'; if (player) { if (player.finished) player.reset(); player.step(); playStarted(); stepScreen(); } };
  again.onclick = () => { if (player) player.reset(); owed = 0; };
  scrub.oninput = () => { playing = false; play.textContent = 'Play'; goTo(parseInt(scrub.value, 10)); };
  pick.onchange = () => load(pick.value);

  function frameLoop(now) {
    if (!canvas.isConnected) return;   // the document was closed or rebuilt
    const dt = Math.min(0.25, (now - last) / 1000);
    last = now;
    let between = 1;
    if (player && playing) {
      owed += dt * 30;
      while (owed >= 1) {
        owed -= 1;
        if (player.finished) { if (loop.checked) player.reset(); else { playing = false; play.textContent = 'Play'; break; } }
        player.step();
        playStarted();
        stepScreen();
      }
      between = fpsPick.value === '60' ? Math.max(0, Math.min(1, owed)) : 1;
    }
    const quads = player ? player.particles() : [];
    // A step drawn between where it was and where it is: `between` 0 is the step before.
    stage.draw(quads, fpsPick.value === '60' && playing ? between : 1, player ? player.models() : []);
    if (player) {
      label.textContent = `frame ${Math.max(0, player.frame)} / ${length}`;
      if (playing) scrub.value = String(Math.max(0, player.frame));
      count.textContent = `${quads.length} particle(s)`;
      if (doc && doc.effectView && doc.effectView.onFrame) doc.effectView.onFrame(Math.max(0, player.frame), length);
    }
    requestAnimationFrame(frameLoop);
  }

  // What the editor drives (effects-editor.js): the definition playing, one to put in its place, the frame.
  if (doc) doc.effectView = {
    name, pack, stage, timelineBox, bar: $('.bar', node),
    get def() { return def; },
    get frame() { return player ? Math.max(0, player.frame) : 0; },
    get length() { return length; },
    get playing() { return playing; },
    apply: next => applyDef(next),
    goTo: frame => { playing = false; play.textContent = 'Play'; goTo(Math.max(0, Math.min(length, frame))); scrub.value = String(Math.max(0, frame)); },
    onFrame: null, onApply: null
  };

  if (pack.own) {
    pick.hidden = true;
    facts.textContent = '';
    facts.append(ownBadge('mod'));
    await load('');
    if (typeof effectEditor === 'function' && doc) effectEditor(doc);
  } else if (members.length) {
    const first = members.find(t => t.category === pack.category && t.member === 1) || members[0];
    pick.value = `${first.category}/${first.member}`;
    await load(pick.value);
  } else {
    facts.textContent += '  ·  no member of effect.efi names these templates';
  }
  // One of the game's: a copy into the mod is where making one's own starts.
  if (!pack.own && members.length && typeof effectCopyButton === 'function') effectCopyButton(doc, () => pick.value);
  requestAnimationFrame(frameLoop);
}
