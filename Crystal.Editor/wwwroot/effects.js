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

/// A value between a key's neighbours by age: keys are [age, ...values], straight lines between.
function effectKeys(keys, age, width) {
  if (!keys || !keys.length) return null;
  if (age <= keys[0][0]) return keys[0].slice(1, 1 + width);
  const last = keys[keys.length - 1];
  if (age >= last[0]) return last.slice(1, 1 + width);
  for (let i = 1; i < keys.length; i++) {
    const b = keys[i];
    if (age > b[0]) continue;
    const a = keys[i - 1];
    const t = b[0] === a[0] ? 1 : (age - a[0]) / (b[0] - a[0]);
    const out = new Array(width);
    for (let j = 0; j < width; j++) out[j] = a[1 + j] + (b[1 + j] - a[1 + j]) * t;
    return out;
  }
  return last.slice(1, 1 + width);
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
    for (let i = 0; i < (em.count || 0); i++) {
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
          const s = range(t.speed.value);
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
      p.size = range(t.size) || 1;
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
        for (let k = 0; k < 3; k++) { p.vel[k] += p.grav[k]; p.base[k] += p.vel[k]; }
        p.local = p.base.slice();
        if (t.orbit) {
          p.radius += t.orbit.grow || 0;
          p.angle += t.orbit.turn || 0;
          p.local[0] += p.radius * Math.sin(p.angle * EFFECT_DEG);
          p.local[2] += p.radius * Math.cos(p.angle * EFFECT_DEG);
        }
      }
      const origin = t.space === 'local' ? e.at : [0, 0, 0];
      p.pos = [p.local[0] + origin[0], p.local[1] + origin[1], p.local[2] + origin[2]];
      if (!p.prev) p.prev = p.pos.slice();
      p.shown = g.age < L;
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
        const colour = effectKeys(t.colour, g.age, 4) || [255, 255, 255, 255];
        const scale = effectKeys(t.scale, g.age, 2) || [1, 1];
        const cell = effectFrame(tex.frames, g.age);
        for (const p of g.parts) {
          if (p.shown && colour[3] > 0) out.push({ p, pos: p.pos, prev: p.prev, w: p.size * scale[0] / 2, h: p.size * scale[1] / 2, colour, cell, tex, blend: (t.render || {}).blend });
          if (t.trail && p.trail.length) {
            // After-images: the particle where it was, each darker by the trail's colour toward its end (the last one never shows, as in the game).
            const n = t.trail.count, d = colour.map((c, k) => (c - Math.max(0, Math.min(255, c + (t.trail.colour[k] || 0)))) / (n + 1));
            for (let k = 1; k < n && k <= p.trail.length; k++) {
              const was = p.trail[k - 1];
              if (!was || !was.shown) continue;
              const c = colour.map((v, j) => v - k * d[j]);
              if (c[3] > 0) out.push({ p: null, pos: was.pos, prev: was.pos, w: p.size * scale[0] / 2, h: p.size * scale[1] / 2, colour: c, cell, tex, blend: (t.render || {}).blend });
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
      .map(m => ({ track: m.track, pos: m.at, prev: m.prev || m.at, frame: m.steps - 1 }));
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

// ------------------------------------------------------------------ the Stage

const EFFECT_VERTEX = `
attribute vec3 centre;
attribute vec3 before;
attribute vec2 corner;
attribute vec2 extent;
attribute vec2 uv;
attribute vec4 colour;
uniform mat4 view;
uniform mat4 projection;
uniform float between;
varying vec2 vUv;
varying vec4 vColour;
void main() {
  vec4 p = view * vec4(mix(before, centre, between), 1.0);
  p.xy += corner * extent;
  gl_Position = projection * p;
  vUv = uv;
  vColour = colour;
}`;

const EFFECT_FRAGMENT = `
precision mediump float;
uniform sampler2D picture;
varying vec2 vUv;
varying vec4 vColour;
void main() {
  vec4 c = texture2D(picture, vUv) * vColour;
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
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  gl_Position = projection * view * world * palette[int(mindex + 0.5)] * vec4(position, 1.0);
  vCoord = coord;
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
  const attr = { centre: a('centre'), before: a('before'), corner: a('corner'), extent: a('extent'), uv: a('uv'), colour: a('colour') };
  const unif = { view: u('view'), projection: u('projection'), between: u('between'), picture: u('picture') };
  const lineAttr = { position: gl.getAttribLocation(lines, 'position'), colour: gl.getAttribLocation(lines, 'colour') };
  const lineUnif = { view: gl.getUniformLocation(lines, 'view'), projection: gl.getUniformLocation(lines, 'projection') };
  const quadBuffer = gl.createBuffer(), lineBuffer = gl.createBuffer();
  const modelProgram = effectCompile(gl, EFFECT_MODEL_VERTEX, EFFECT_MODEL_FRAGMENT);
  const ma = n => gl.getAttribLocation(modelProgram, n), mu = n => gl.getUniformLocation(modelProgram, n);
  const modelAttr = { position: ma('position'), coord: ma('coord'), colour: ma('colour'), mindex: ma('mindex') };
  const modelUnif = { view: mu('view'), projection: mu('projection'), world: mu('world'), palette: mu('palette'), picture: mu('picture'), textured: mu('textured'), tint: mu('tint'), alpha: mu('alpha') };

  // The ground the target stands on, and the target where the battle has it: the hit point (the origin, where
  // the effect plays) is so far up from its feet and so far toward the camera (TurnSystem.setHitEffectPosition) -
  // 5 and 9 for the party, the monster's own offsets for a monster.
  let target = { toward: 9, up: 5, scale: 1, turn: 0, model: null }, lineCount = 0;
  function layGround() {
    const ground = [], GROUND = -target.up, Z = -target.toward;
    const put = (x1, y1, z1, x2, y2, z2, c) => ground.push(x1, y1, z1, ...c, x2, y2, z2, ...c);
    for (let i = -30; i <= 30; i += 5) {
      const c = i === 0 ? [0.35, 0.4, 0.48, 1] : [0.2, 0.23, 0.28, 1];
      put(i, GROUND, Z - 30, i, GROUND, Z + 30, c);
      put(-30, GROUND, Z + i, 30, GROUND, Z + i, c);
    }
    if (!target.model) {
      // No model: a post of its height.
      const post = [0.55, 0.45, 0.3, 1], H = GROUND + 10;
      for (const [dx, dz] of [[-2, -1], [2, -1], [2, 1], [-2, 1]]) put(dx, GROUND, Z + dz, dx, H, Z + dz, post);
      put(-2, H, Z - 1, 2, H, Z - 1, post); put(-2, H, Z + 1, 2, H, Z + 1, post);
      put(-2, H, Z - 1, -2, H, Z + 1, post); put(2, H, Z - 1, 2, H, Z + 1, post);
    }
    const mark = [0.43, 0.66, 1, 1];
    put(-0.8, 0, 0, 0.8, 0, 0, mark); put(0, -0.8, 0, 0, 0.8, 0, mark); put(0, 0, -0.8, 0, 0, 0.8, mark);
    gl.bindBuffer(gl.ARRAY_BUFFER, lineBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(ground), gl.STATIC_DRAW);
    lineCount = ground.length / 7;
  }
  layGround();

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
  function drawModel(m, world, frame, translucent, view, projection) {
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
    const pose = m.pose, per = pose ? pose.counts.reduce((a, b) => a + b, 0) : 0;
    const f = pose ? Math.max(0, Math.min(pose.frames - 1, m.loop ? frame % pose.frames : frame)) : 0;
    let slot = 0;
    for (let gi = 0; gi < m.groups.length; gi++) {
      const g = m.groups[gi], count = pose ? (pose.counts[gi] || 1) : 1;
      const first = slot;
      slot += count;
      if (g.hidden || Boolean(g.translucent) !== translucent) continue;
      const palette = new Float32Array(32 * 16);
      for (let k = 0; k < 32; k++) palette.set(IDENTITY, k * 16);
      if (pose) for (let k = 0; k < count && k < 32; k++) palette.set(effectMat43(pose.matrices, (f * per + first + k) * 12), k * 16);
      gl.uniformMatrix4fv(modelUnif.palette, false, palette);
      const textured = Boolean(g.picture && g.picture.ready);
      gl.uniform1i(modelUnif.textured, textured ? 1 : 0);
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, textured ? g.picture.gl : blank);
      const c = g.colour === undefined ? 0xFFFFFF : g.colour;
      gl.uniform3f(modelUnif.tint, ((c >> 16) & 255) / 255, ((c >> 8) & 255) / 255, (c & 255) / 255);
      gl.uniform1f(modelUnif.alpha, g.alpha === undefined ? 1 : g.alpha);
      gl.drawElements(gl.TRIANGLES, g.count, m.indexType, g.start * m.indexSize);
    }
    for (const loc of Object.values(modelAttr)) gl.disableVertexAttribArray(loc);
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
    const yaw = camera.yaw * EFFECT_DEG, pitch = camera.pitch * EFFECT_DEG;
    const jolt = camera.jolt || [0, 0];
    const look = [camera.target[0] + jolt[0], camera.target[1] + jolt[1], camera.target[2]];
    const eye = [look[0] + camera.distance * Math.cos(pitch) * Math.sin(yaw), look[1] + camera.distance * Math.sin(pitch), look[2] + camera.distance * Math.cos(pitch) * Math.cos(yaw)];
    return { view: effectLookAt(eye, look), projection: effectPerspective(40 * EFFECT_DEG, w / h, 0.5, 500) };
  }

  let dragging = null;
  canvas.addEventListener('pointerdown', e => { dragging = { x: e.clientX, y: e.clientY }; canvas.setPointerCapture(e.pointerId); });
  canvas.addEventListener('pointermove', e => {
    if (!dragging) return;
    camera.yaw -= (e.clientX - dragging.x) * 0.4;
    camera.pitch = Math.max(-10, Math.min(85, camera.pitch + (e.clientY - dragging.y) * 0.3));
    dragging = { x: e.clientX, y: e.clientY };
  });
  canvas.addEventListener('pointerup', () => { dragging = null; });
  canvas.addEventListener('wheel', e => { e.preventDefault(); camera.distance = Math.max(8, Math.min(200, camera.distance * (e.deltaY > 0 ? 1.1 : 0.9))); }, { passive: false });

  /// What stands at the hit point: { model (a game model's name, or none), scale, toward, up, turn }.
  function setTarget(t) {
    target = { toward: t.toward || 0, up: t.up || 0, scale: t.scale || 1, turn: t.turn || 0, model: t.model || null };
    camera.target = [0, 1, -target.toward / 3];
    layGround();
  }

  // The caster stands at the party's side, on the target's ground, as the battle stands them apart; its hit point
  // (9 toward the camera, 5 up, as the party's) is the caster anchor.
  const CASTER_X = 30;
  function caster() {
    return { feet: [CASTER_X, -target.up, -target.toward], anchor: [CASTER_X, -target.up + 5, -target.toward + 9] };
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

    // The scene's models - the target, the effect's own - opaque first, then the translucent over them.
    const scene = [];
    if (target.model) {
      const m = loadModel('target:' + target.model, `/api/model?name=${encodeURIComponent(target.model)}`,
        tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(target.model)}&texture=${encodeURIComponent(tex)}`));
      if (!m.idle) {
        // Standing as it stands in battle: the first motion (101, the wait) of the pack that fits the model best, looping.
        m.idle = true;
        api(`/api/model/motions?name=${encodeURIComponent(target.model)}`).then(packs => {
          const pack = (packs || []).find(p => p.motions && p.motions.length);
          if (!pack) return null;
          const motion = pack.motions.find(x => x.id === 101) || pack.motions[0];
          return api(`/api/model/pose?name=${encodeURIComponent(target.model)}&pack=${encodeURIComponent(pack.name)}&index=${motion.index}`);
        }).then(pose => { if (pose && pose.matrices) { m.pose = pose; m.loop = true; } }).catch(() => {});
      }
      scene.push({ m, world: placed([0, -target.up, -target.toward], target.scale, target.turn), frame: Math.floor(performance.now() / 1000 * 30) });
    }
    // The caster: Luneth at the party's side.
    {
      const c = caster(), luneth = 'files/j101.nmdp.lz';
      const m = loadModel('caster:' + luneth, `/api/model?name=${encodeURIComponent(luneth)}`,
        tex => wsUrl(`/api/model/texture?name=${encodeURIComponent(luneth)}&texture=${encodeURIComponent(tex)}`));
      scene.push({ m, world: placed(c.feet, 1, -90), frame: 0 });
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
      scene.push({ m, world: placed(pos, e.track.scale || 1, e.track.yaw || 0), frame: e.frame });
    }
    for (const it of scene) drawModel(it.m, it.world, it.frame, false, view, projection);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    gl.depthMask(false);
    for (const it of scene) drawModel(it.m, it.world, it.frame, true, view, projection);
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
    const stride = 17;
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
          q.colour[0] / 255, q.colour[1] / 255, q.colour[2] / 255, q.colour[3] / 255, 0], n);
        n += stride;
      }
    }
    gl.bindBuffer(gl.ARRAY_BUFFER, quadBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, data, gl.DYNAMIC_DRAW);
    const bind = (loc, size, offset) => { gl.enableVertexAttribArray(loc); gl.vertexAttribPointer(loc, size, gl.FLOAT, false, stride * 4, offset * 4); };
    bind(attr.centre, 3, 0); bind(attr.before, 3, 3); bind(attr.corner, 2, 6); bind(attr.extent, 2, 8); bind(attr.uv, 2, 10); bind(attr.colour, 4, 12);
    gl.activeTexture(gl.TEXTURE0);
    // Runs of one texture and one blend, in order - blending needs the game's order, not one batch per texture.
    // Additive (render.blend) adds the quad's light to what is behind it, as the client draws it (SourceAlpha, One).
    const same = (a, b) => (a.tex || {}).image === (b.tex || {}).image && (a.blend === 'additive') === (b.blend === 'additive');
    let from = 0;
    for (let i = 1; i <= quads.length; i++) {
      if (i < quads.length && same(quads[i], quads[from])) continue;
      gl.blendFunc(gl.SRC_ALPHA, quads[from].blend === 'additive' ? gl.ONE : gl.ONE_MINUS_SRC_ALPHA);
      gl.bindTexture(gl.TEXTURE_2D, texture((quads[from].tex || {}).image));
      gl.drawArrays(gl.TRIANGLES, from * 6, (i - from) * 6);
      from = i;
    }
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    for (const loc of Object.values(attr)) gl.disableVertexAttribArray(loc);
    gl.depthMask(true);
  }

  return { draw, camera, setTarget, caster };
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
  for (const [value, text] of [['post', 'a post'], ['-1', 'Luneth (the party)']]) { const o = document.createElement('option'); o.value = value; o.textContent = text; targetPick.append(o); }
  controls.append(loopBox, fpsPick, targetPick, count);
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

  // The monsters to stand at the hit point; the Goblin to start with.
  async function setTarget(value) {
    effectTarget = value;
    if (value === 'post') { stage.setTarget({ toward: 9, up: 5 }); return; }
    const t = await api(`/api/effect/target?monster=${encodeURIComponent(value)}`);
    if (t.error) { say(t.error, 'bad'); return; }
    stage.setTarget(t);
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
  function applyDef(next, notesOf = [], { restart = false } = {}) {
    const at = player ? Math.max(0, player.frame) : 0;
    def = next;
    json.textContent = JSON.stringify(def, null, 1).replace(/\[\s+([-\d.,\s]+?)\s+\]/g, (m, inner) => '[' + inner.replace(/\s+/g, ' ').trim() + ']')
      + (notesOf && notesOf.length ? '\n\n' + notesOf.map(n => '// ' + n).join('\n') : '');
    const anchors = { caster: stage.caster().anchor };
    player = makeEffectPlayer(def, { seed: 7, anchors });
    // How long a pass takes: played through once, to its end.
    const probe = makeEffectPlayer(def, { seed: 7, anchors });
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
    facts.textContent = pack.note;
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
