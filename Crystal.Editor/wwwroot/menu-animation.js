// Crystal Style Sheets in motion, in the editor: transitions and @keyframes animations, a port of
// Shared/Text/MenuAnimation.cs kept to the same rules (see that file for the language) - and the
// canvas's Play, which runs the animator over the screen's cascade frame by frame, as the client's
// screen does, and puts on the canvas what it shows.
//
// What moves is the look (opacity, colours, tints, the background's colours, gradients, borders,
// corners and shadows, the text's lettering), never the layout. A value moves from one to another
// when the two have the same shape (the same words, numbers and colours in the same places);
// otherwise it changes half way, as CSS changes what it cannot interpolate.

// ------------------------------------------------------------------ the values

/// The properties that move: the look, not the layout nor what says how things move.
function animatable(property) {
  if (!property) return false;
  if (STYLE_LAYOUT.includes(property)) return false;
  if (TRANSITION_PROPS.includes(property) || ANIMATION_PROPS.includes(property)) return false;
  // A panel of its own (the game's window made or taken away) and display (the layout's) do not move.
  return property !== 'display' && property !== '-ff-panel';
}

/// A palette word's colour ([r, g, b, a]), for colours to move between the game's words and #hex: the game's text palette (app.js).
function menuPalette(word) {
  if (typeof GAME_TEXT_COLOURS === 'undefined') return null;
  const hex = GAME_TEXT_COLOURS[String(word || '').toLowerCase().replace(/[-\s]/g, '')];
  return hex ? bgColour(hex) : null;
}

const ANIM_TOKEN = /#[0-9a-fA-F]{3,8}\b|rgba?\([^)]*\)|-?(?:\d+\.?\d*|\.\d+)(?:[a-zA-Z%]+)?|[A-Za-z][\w-]*/g;

/// A value as its shape (the text with its numbers and colours taken out) and those numbers and colours.
function animSplit(value) {
  const slots = [];
  const text = String(value ?? '');
  let shape = '';
  let at = 0;
  for (const m of text.matchAll(ANIM_TOKEN)) {
    shape += text.slice(at, m.index);
    at = m.index + m[0].length;
    const t = m[0];
    if (t[0] === '#' || /^rgb/i.test(t)) {
      const c = bgColour(t);
      if (c) { slots.push({ colour: c }); shape += '\u0001C'; continue; }
    } else if (/[\d.-]/.test(t[0])) {
      const n = /^(-?(?:\d+\.?\d*|\.\d+))([a-zA-Z%]*)$/.exec(t);
      if (n) {
        slots.push({ number: parseFloat(n[1]), unit: n[2] });
        shape += '\u0001N' + n[2];
        continue;
      }
    } else if (t.toLowerCase() !== 'transparent' && menuPalette(t)) {
      slots.push({ colour: menuPalette(t) });
      shape += '\u0001C';
      continue;
    } else if (t.toLowerCase() === 'transparent') {
      slots.push({ colour: [0, 0, 0, 0] });
      shape += '\u0001C';
      continue;
    }
    shape += t;
  }
  shape += text.slice(at);
  return { shape: shape.replace(/\s+/g, ' ').trim(), slots };
}

/// Two colours mixed, premultiplied as CSS mixes them (a colour fading from transparent keeps its hue).
function animMix(a, b, t) {
  const aa = a[3] / 255, ba = b[3] / 255;
  const alpha = aa + (ba - aa) * t;
  const ch = i => {
    const ca = a[i] * aa, cb = b[i] * ba;
    const v = alpha <= 0.0001 ? 0 : (ca + (cb - ca) * t) / alpha;
    return Math.max(0, Math.min(255, Math.round(v)));
  };
  return [ch(0), ch(1), ch(2), Math.max(0, Math.min(255, Math.round(alpha * 255)))];
}

/// #rrggbb, or #rrggbbaa when it is not opaque.
function animColourText(c) {
  const hex = n => n.toString(16).padStart(2, '0');
  return '#' + hex(c[0]) + hex(c[1]) + hex(c[2]) + (c[3] === 255 ? '' : hex(c[3]));
}

function animNumberText(v) {
  const r = Math.round(v * 1000) / 1000;
  return String(Object.is(r, -0) ? 0 : r);
}

/// The value a fraction t of the way from a to b: numbers and colours moved where the two have the same shape, else a until half way, then b.
function animInterpolate(a, b, t) {
  if (t <= 0) return a;
  if (t >= 1) return b;
  if (a == null || b == null || a === b) return t < 0.5 ? a : b;
  const pa = animSplit(a), pb = animSplit(b);
  if (pa.shape !== pb.shape || pa.slots.length !== pb.slots.length || !pa.slots.length) return t < 0.5 ? a : b;
  let out = '';
  let slot = 0;
  for (let i = 0; i < pa.shape.length; i++) {
    if (pa.shape[i] !== '\u0001') { out += pa.shape[i]; continue; }
    const kind = pa.shape[++i];
    const x = pa.slots[slot], y = pb.slots[slot];
    slot++;
    if (kind === 'N') out += animNumberText(x.number + (y.number - x.number) * t);   // the unit follows in the shape
    else out += animColourText(animMix(x.colour, y.colour, t));
  }
  return out;
}

// ------------------------------------------------------------------ time

/// "0.3s" / "300ms" in seconds; null when it is not a time.
function animSeconds(text) {
  const t = String(text ?? '').trim().toLowerCase();
  if (!t) return null;
  if (t.endsWith('ms')) { const n = Number(t.slice(0, -2)); return t.length > 2 && Number.isFinite(n) ? n / 1000 : null; }
  if (t.endsWith('s')) { const n = Number(t.slice(0, -1)); return t.length > 1 && Number.isFinite(n) ? n : null; }
  return null;
}

function animBezier(x1, y1, x2, y2) {
  const coord = (s, p1, p2) => 3 * (1 - s) * (1 - s) * s * p1 + 3 * (1 - s) * s * s * p2 + s * s * s;
  const slope = (s, p1, p2) => 3 * (1 - s) * (1 - s) * p1 + 6 * (1 - s) * s * (p2 - p1) + 3 * s * s * (1 - p2);
  return x => {
    if (x <= 0) return 0;
    if (x >= 1) return 1;
    let s = x;
    for (let i = 0; i < 8; i++) {
      const d = coord(s, x1, x2) - x, sl = slope(s, x1, x2);
      if (Math.abs(d) < 1e-6) break;
      if (Math.abs(sl) < 1e-6) break;
      s -= d / sl;
    }
    if (s < 0 || s > 1 || Math.abs(coord(s, x1, x2) - x) > 1e-4) {
      let lo = 0, hi = 1;
      for (let i = 0; i < 30; i++) { s = (lo + hi) / 2; if (coord(s, x1, x2) < x) lo = s; else hi = s; }
    }
    return coord(s, y1, y2);
  };
}

function animSteps(n, start) {
  return x => Math.min(1, Math.max(0, (start ? Math.ceil(x * n) : Math.floor(x * n)) / n));
}

/// A timing function: the progress shown at a fraction of the time.
function animTiming(text) {
  const t = String(text ?? 'ease').trim().toLowerCase();
  switch (t) {
    case 'linear': return x => x;
    case 'ease': return animBezier(0.25, 0.1, 0.25, 1);
    case 'ease-in': return animBezier(0.42, 0, 1, 1);
    case 'ease-out': return animBezier(0, 0, 0.58, 1);
    case 'ease-in-out': return animBezier(0.42, 0, 0.58, 1);
    case 'step-start': return animSteps(1, true);
    case 'step-end': return animSteps(1, false);
  }
  let m = /^cubic-bezier\(\s*([-\d.]+)\s*,\s*([-\d.]+)\s*,\s*([-\d.]+)\s*,\s*([-\d.]+)\s*\)$/.exec(t);
  if (m) {
    const n = i => parseFloat(m[i]);
    return animBezier(Math.min(1, Math.max(0, n(1))), n(2), Math.min(1, Math.max(0, n(3))), n(4));
  }
  m = /^steps\(\s*(\d+)\s*(?:,\s*(start|end|jump-start|jump-end)\s*)?\)$/.exec(t);
  if (m) return animSteps(Math.max(1, parseInt(m[1], 10)), (m[2] || '').endsWith('start'));
  return animBezier(0.25, 0.1, 0.25, 1);
}

function animIsTiming(word) {
  const w = String(word).trim().toLowerCase();
  return ['linear', 'ease', 'ease-in', 'ease-out', 'ease-in-out', 'step-start', 'step-end'].includes(w) || w.startsWith('cubic-bezier(') || w.startsWith('steps(');
}

// ------------------------------------------------------------------ transitions and animations

/// A frame's transitions, from its computed properties (a Map): [{ property, duration, delay, timing }].
function animTransitions(computed) {
  let list = [];
  const shorthand = computed.get('transition');
  if (shorthand !== undefined && shorthand.trim().toLowerCase() !== 'none') {
    for (const item of cssCommaList(shorthand)) {
      const t = { property: 'all', duration: 0, delay: 0, timing: 'ease' };
      let timeSeen = false;
      for (const word of cssWords(item)) {
        const s = animSeconds(word);
        if (s !== null) { if (!timeSeen) { t.duration = s; timeSeen = true; } else t.delay = s; }
        else if (animIsTiming(word)) t.timing = word;
        else t.property = word.toLowerCase();
      }
      list.push(t);
    }
  }
  const longhand = name => computed.has(name) ? cssCommaList(computed.get(name)) : null;
  const props = longhand('transition-property'), durations = longhand('transition-duration'), timings = longhand('transition-timing-function'), delays = longhand('transition-delay');
  if (props) {
    const old = list;
    list = props.map((p, i) => ({ property: p.trim().toLowerCase(), duration: i < old.length ? old[i].duration : 0, delay: i < old.length ? old[i].delay : 0, timing: i < old.length ? old[i].timing : 'ease' }));
  }
  list.forEach((t, i) => {
    if (durations && durations.length) t.duration = animSeconds(durations[i % durations.length]) ?? t.duration;
    if (timings && timings.length) t.timing = timings[i % timings.length];
    if (delays && delays.length) t.delay = animSeconds(delays[i % delays.length]) ?? t.delay;
  });
  return list.filter(t => t.duration > 0 && t.property !== 'none');
}

/// A frame's animations, from its computed properties: [{ name, duration, delay, timing, count, direction, fill, paused }].
function animAnimations(computed) {
  const fresh = name => ({ name, duration: 0, delay: 0, timing: 'ease', count: 1, direction: 'normal', fill: 'none', paused: false });
  let list = [];
  const shorthand = computed.get('animation');
  if (shorthand !== undefined && shorthand.trim().toLowerCase() !== 'none') {
    for (const item of cssCommaList(shorthand)) {
      const a = fresh(null);
      let timeSeen = false;
      for (const word of cssWords(item)) {
        const w = word.toLowerCase();
        const s = animSeconds(w);
        const n = Number(w);
        if (s !== null) { if (!timeSeen) { a.duration = s; timeSeen = true; } else a.delay = s; }
        else if (animIsTiming(w)) a.timing = w;
        else if (w === 'infinite') a.count = Infinity;
        else if (w !== '' && Number.isFinite(n)) a.count = Math.max(0, n);
        else if (['normal', 'reverse', 'alternate', 'alternate-reverse'].includes(w)) a.direction = w;
        else if ((w === 'none' && a.name !== null) || w === 'forwards' || w === 'backwards' || w === 'both') a.fill = w;
        else if (w === 'paused') a.paused = true;
        else if (w === 'running') a.paused = false;
        else a.name = word.replace(/^["']+|["']+$/g, '');
      }
      if (a.name !== null) list.push(a);
    }
  }
  const longhand = name => computed.has(name) ? cssCommaList(computed.get(name)) : null;
  const names = longhand('animation-name');
  if (names) {
    const old = list;
    list = names.filter(n => n.toLowerCase() !== 'none').map((n, i) => i < old.length ? { ...old[i], name: n.replace(/^["']+|["']+$/g, '') } : fresh(n.replace(/^["']+|["']+$/g, '')));
  }
  const each = (name, put) => {
    const values = longhand(name);
    if (!values || !values.length) return;
    list.forEach((a, i) => put(a, values[i % values.length].trim().toLowerCase()));
  };
  each('animation-duration', (a, v) => { a.duration = animSeconds(v) ?? a.duration; });
  each('animation-timing-function', (a, v) => { a.timing = v; });
  each('animation-delay', (a, v) => { a.delay = animSeconds(v) ?? a.delay; });
  each('animation-iteration-count', (a, v) => { const n = Number(v); a.count = v === 'infinite' ? Infinity : v !== '' && Number.isFinite(n) ? Math.max(0, n) : a.count; });
  each('animation-direction', (a, v) => { a.direction = v; });
  each('animation-fill-mode', (a, v) => { a.fill = v; });
  each('animation-play-state', (a, v) => { a.paused = v === 'paused'; });
  return list.filter(a => a.duration > 0 && a.count > 0);
}

// ------------------------------------------------------------------ the animator

/// What a screen's frames show as time goes on (MenuAnimation.Animator): handed each frame's computed properties
/// and the time, it gives back those properties with the transitions and animations applied.
class MenuAnimator {
  constructor() {
    this.frames = new Map();   // element -> { target, shown, tweens, animations }
    this.first = true;
    this.active = false;
  }

  /// `frames`: [[element, computed Map]] in the layout's order; `keyframes`: the sheets' (a Map); `now` in seconds.
  /// The first call only sets things down: nothing transitions from before, but animations start.
  step(frames, keyframes, now) {
    const shown = new Map();
    let active = false;
    const seen = new Set();
    for (const [frame, computed] of frames) {
      seen.add(frame);
      let state = this.frames.get(frame);
      if (!state) { state = { target: new Map(), shown: new Map(), tweens: new Map(), animations: new Map() }; this.frames.set(frame, state); }
      const result = new Map(computed);

      // Transitions: a property whose value changed moves from what was shown to the new value.
      const transitions = animTransitions(computed);
      const names = new Set([...computed.keys(), ...state.target.keys()].filter(animatable));
      for (const name of names) {
        const to = computed.get(name), before = state.target.get(name);
        if (!this.first && to !== before && transitions.length) {
          let t = null;
          for (const x of transitions) if (x.property === name || x.property === 'all' || name.startsWith(x.property + '-')) t = x;
          const from = state.shown.has(name) ? state.shown.get(name) : before;
          if (t && from != null && to != null && from !== to) state.tweens.set(name, { from, to, start: now + t.delay, duration: t.duration, ease: animTiming(t.timing) });
          else state.tweens.delete(name);
        } else if (to !== before) state.tweens.delete(name);
        if (to === undefined) state.target.delete(name); else state.target.set(name, to);
      }
      for (const [name, w] of [...state.tweens]) {
        const p = w.duration <= 0 ? 1 : (now - w.start) / w.duration;
        if (p >= 1) { state.tweens.delete(name); continue; }
        active = true;
        result.set(name, p <= 0 ? w.from : animInterpolate(w.from, w.to, w.ease(p)));
      }

      // Animations: each started as it is declared, over what the transitions leave.
      const keys = new Set();
      for (const a of animAnimations(computed)) {
        const stops = keyframes && keyframes.get(a.name);
        if (!stops || !stops.length) continue;
        const key = `${a.name}|${a.duration}|${a.delay}|${a.count}|${a.direction}`;
        keys.add(key);
        let run = state.animations.get(key);
        if (!run) { run = { start: now }; state.animations.set(key, run); }
        const elapsed = now - run.start - a.delay;
        const done = Number.isFinite(a.count) && elapsed >= a.duration * a.count;
        if (!done && !a.paused) active = true;
        const progress = animProgress(a, elapsed);
        if (progress === null) continue;
        for (const [k, v] of animSample(stops, progress, result, a.timing)) result.set(k, v);
      }
      for (const key of [...state.animations.keys()]) if (!keys.has(key)) state.animations.delete(key);

      state.shown.clear();
      for (const [k, v] of result) if (animatable(k)) state.shown.set(k, v);
      shown.set(frame, result);
    }
    for (const frame of [...this.frames.keys()]) if (!seen.has(frame)) this.frames.delete(frame);
    this.first = false;
    this.active = active;
    return shown;
  }
}

/// Where in its keyframes an animation is (0..1, the direction applied), or null when it shows nothing.
function animProgress(a, elapsed) {
  if (elapsed < 0) {
    if (a.fill !== 'backwards' && a.fill !== 'both') return null;
    return animDirected(a, 0, 0);
  }
  const total = a.duration * a.count;
  if (Number.isFinite(a.count) && elapsed >= total) {
    if (a.fill !== 'forwards' && a.fill !== 'both') return null;
    const lastIteration = Math.ceil(a.count) - 1;
    const frac = a.count - Math.floor(a.count);
    return animDirected(a, frac === 0 ? 1 : frac, frac === 0 ? lastIteration : Math.floor(a.count));
  }
  const iteration = Math.floor(elapsed / a.duration);
  const within = (elapsed - iteration * a.duration) / a.duration;
  return animDirected(a, within, iteration);
}

function animDirected(a, p, iteration) {
  const odd = Math.trunc(iteration) % 2 === 1;
  switch (a.direction) {
    case 'reverse': return 1 - p;
    case 'alternate': return odd ? 1 - p : p;
    case 'alternate-reverse': return odd ? p : 1 - p;
    default: return p;
  }
}

/// The keyframes' values at a progress: each property between the stops around it that give it (from and to fall back on the frame's own), eased by the animation's timing.
function animSample(stops, progress, under, timing) {
  const values = new Map();
  const ease = animTiming(timing);
  const names = new Set();
  for (const s of stops) for (const k of s.values.keys()) if (animatable(k)) names.add(k);
  for (const name of names) {
    const points = stops.filter(s => s.values.has(name)).map(s => [s.offset, s.values.get(name)]).sort((a, b) => a[0] - b[0]);
    const own = under.get(name);
    if (!points.length) continue;
    if (points[0][0] > 0 && own !== undefined) points.unshift([0, own]);
    if (points[points.length - 1][0] < 1 && own !== undefined) points.push([1, own]);
    let [o0, v0] = points[0];
    if (progress <= o0) { values.set(name, v0); continue; }
    let set = false;
    for (let i = 1; i < points.length; i++) {
      const [o1, v1] = points[i];
      if (progress <= o1) {
        const local = o1 - o0 <= 0 ? 1 : (progress - o0) / (o1 - o0);
        values.set(name, animInterpolate(v0, v1, ease(local)));
        set = true;
        break;
      }
      [o0, v0] = [o1, v1];
    }
    if (!set) values.set(name, points[points.length - 1][1]);
  }
  return values;
}

// ------------------------------------------------------------------ Play on the canvas

/// The canvas's motion: the animator run over the open screen each frame of the browser's, while Play is on.
const menuMotion = { on: false, animator: null, node: null, screen: null, raf: 0, looks: new Map(), start: 0 };

/// A frame's look as the motion shows it now (null when it is not playing, or has not stepped yet).
function menuMotionLook(element) {
  if (!menuMotion.on) return null;
  const entry = menuMotion.looks.get(element);
  return entry ? entry.look : null;
}

/// A look's text for comparing: what it carries, with its opacity apart (a change of that alone is cheap to show).
function lookSignature(look) {
  let key = '', opacity = '1';
  for (const child of look.children) {
    if (child.tagName === 'frame') continue;
    if (child.tagName === 'opacity') { opacity = (child.getAttribute('value') ?? child.textContent).trim(); continue; }
    key += `<${child.tagName}>${child.getAttribute('value') ?? child.textContent}`;
  }
  return { key, opacity };
}

function menuMotionPlaying() { return menuMotion.on; }

function startMenuMotion(node) {
  stopMenuMotion(true);
  menuMotion.on = true;
  menuMotion.node = node;
  menuMotion.animator = new MenuAnimator();
  menuMotion.looks = new Map();
  menuMotion.start = performance.now();
  const tick = () => {
    if (!menuMotion.on) return;
    if (!node.isConnected || menu.node !== node) { stopMenuMotion(); return; }
    try { stepMenuMotion(node); } catch (error) { console.error(error); }
    menuMotion.raf = requestAnimationFrame(tick);
  };
  tick();
  showMotionButton(node);
}

/// Stopped: the static cascade again (`quiet`: nothing drawn, for a restart).
function stopMenuMotion(quiet) {
  const was = menuMotion.on;
  menuMotion.on = false;
  if (menuMotion.raf) cancelAnimationFrame(menuMotion.raf);
  menuMotion.raf = 0;
  menuMotion.animator = null;
  menuMotion.looks = new Map();
  const node = menuMotion.node;
  menuMotion.node = null;
  if (node) showMotionButton(node);
  if (was && !quiet && node && node.isConnected && menu.node === node) redraw(node, true);
}

function showMotionButton(node) {
  const button = $('.motion', node);
  if (!button) return;
  button.classList.toggle('on', menuMotion.on && menuMotion.node === node);
  button.textContent = menuMotion.on && menuMotion.node === node ? '■ Stop' : '▶ Play';
}

/// One step: the cascade as it is now (the states, the sheets), through the animator, onto the canvas's boxes whose look changed.
function stepMenuMotion(node) {
  const picker = $('.screens', node);
  const screen = menu.screens && menu.screens[(picker && picker.value) || 0];
  if (!screen) return;
  if (menuMotion.screen !== screen) { menuMotion.screen = screen; menuMotion.animator = new MenuAnimator(); menuMotion.looks = new Map(); }
  const styled = menuStyled(screen);
  const list = [[screen, styled.screenValues]];
  const order = [];
  const walk = parent => {
    for (const frame of parent.children) {
      if (frame.tagName !== 'frame') continue;
      const values = styled.computed.get(frame);
      if (!values) continue;
      list.push([frame, values]);
      order.push(frame);
      walk(frame);
    }
  };
  walk(screen);
  const shown = menuMotion.animator.step(list, styled.keyframes, (performance.now() - menuMotion.start) / 1000);

  // Each frame's opacity its parents' times its own; its look baked from what is shown - again only where that changed.
  const opacity = new Map([[screen, styleOpacity(shown.get(screen) || styled.screenValues, 1)]]);
  const boxes = new Map();
  for (const box of $$('.canvas .widget', node)) if (box.frameElement) boxes.set(box.frameElement, box);
  for (const frame of order) {
    const values = shown.get(frame);
    const mine = styleOpacity(values, opacity.get(frame.parentElement) ?? 1);
    opacity.set(frame, mine);
    const key = [...values].map(([k, v]) => `${k}:${v}`).join(';') + '|' + Math.round(mine * 1000);
    let entry = menuMotion.looks.get(frame);
    if (!entry || entry.key !== key) {
      const look = frameLookOf(frame, values, mine);
      entry = { key, look, signature: lookSignature(look) };
      menuMotion.looks.set(frame, entry);
    }
    const box = boxes.get(frame);
    if (box && typeof restyleFrameBox === 'function') restyleFrameBox(box, entry.look, entry.signature);
  }
}
