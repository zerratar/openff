// The effect editor: one of the project's effects (defs/effects/<id>.json) made and changed without
// touching its JSON. The Hierarchy is the effect and its tracks (an eye mutes one on the Stage, a
// right click duplicates, deletes or moves it, a drag reorders); the Inspector the selected track's
// modules as cards - emission, the particle, shape, speed and spread, gravity, orbit, gather, trail,
// colour over life (a gradient), scale over life (a curve), the texture and its flipbook, the render
// - each switched on and off; a mesh's model, a sound, a flash, a shake theirs. Under the Stage, the
// timeline: a bar a track, dragged to move its start or stretch its emission, the playhead scrubbed.
//
// Every change plays at once on the Stage (effects.js's applyDef keeps the frame it was on), goes on
// the undo stack (Ctrl+Z, Ctrl+Y) and saves itself a moment later - as the items and monsters do. A
// game effect's view gets *Copy into the mod*; the Effects library *New effect…*; an effect's bar
// *Use for a spell…* (the spell's look in defs/spells), *Duplicate*, *Delete* and its JSON.
//
// Server: /api/project/effect/save, /new, /delete, /images, /image (upload), /spells, /spell;
// /api/effect/textures for the game's pictures.

'use strict';

const EFFECT_TRACK_TYPES = [
  { type: 'emitter', label: 'Emitter', hint: 'particles: bursts of them over the duration' },
  { type: 'mesh', label: 'Mesh', hint: 'a model: one of the game\'s effects\' own, or a glTF of the mod\'s' },
  { type: 'sound', label: 'Sound', hint: 'a sound of the game\'s at the track\'s frame' },
  { type: 'flash', label: 'Flash', hint: 'the screen flashed a colour' },
  { type: 'shake', label: 'Shake', hint: 'the camera shaken' },
];

const EFFECT_TRACK_COLOURS = { emitter: '#6ea8fe', mesh: '#d9a441', sound: '#b3548a', flash: '#c9c9c9', shake: '#39a190' };

function effectClone(v) { return JSON.parse(JSON.stringify(v)); }

/// A new track of a kind, as short as it can be.
function effectNewTrack(type, def) {
  const name = type + ' ' + ((def.tracks || []).filter(t => (t.type || 'emitter') === type).length + 1);
  switch (type) {
    case 'mesh': return { type, name, start: 0, anchor: 'target', model: '', scale: 1, life: 20 };
    case 'sound': return { type, name, start: 0, archive: 200, number: 8 };
    case 'flash': return { type, name, start: 0, colour: [255, 255, 255], frames: 3, interval: 2, count: 1 };
    case 'shake': return { type, name, start: 0, frames: 10, power: 0.3 };
    default: return {
      type: 'emitter', name, start: 0, anchor: 'target',
      emission: { duration: 10, interval: 2, count: 2, bursts: 5 }, life: 16, size: [2, 3],
      speed: { direction: [0, 1, 0], value: [0.2, 0.4] },
      colour: [[1, 255, 255, 255, 0], [4, 255, 255, 255, 255], [15, 255, 255, 255, 0]],
      render: { blend: 'additive' }
    };
  }
}

// ------------------------------------------------------------------ the editor on a document

/// Makes the open effect document an editor (called by openEffect for the project's own effects).
function effectEditor(doc) {
  const view = doc.effectView;
  const ed = { doc, view, name: view.name, def: effectClone(view.def), muted: new Set(), saving: null, dirty: false };
  doc.effectEditor = ed;
  doc.data = { effect: ed.def, name: ed.name };
  doc.inspect = ref => effectInspect(ed, ref);
  doc.selection = (ed.def.tracks || []).length ? 'track:0' : 'effect';

  /// What plays: the definition, the muted tracks left out.
  ed.preview = () => {
    const d = effectClone(ed.def);
    d.tracks = (d.tracks || []).filter((t, i) => !ed.muted.has(i));
    return d;
  };

  /// A change: made, played, undoable, saved. structural redraws the Inspector too (a module on or off, a track added).
  ed.change = (label, mutate, { structural = false, before = null } = {}) => {
    const was = before || effectClone(ed.def);
    mutate(ed.def);
    const now = effectClone(ed.def);
    pushUndo(doc, label, () => ed.load(effectClone(was)), () => ed.load(effectClone(now)));
    ed.refresh(structural);
  };
  /// A live edit (a field being typed into, a key being dragged): played at once, one undo step for the lot when it is let go.
  ed.live = mutate => { mutate(ed.def); ed.refresh(false, true); };

  ed.load = d => {
    ed.def = d;
    doc.data.effect = d;
    ed.refresh(true);
  };

  ed.refresh = (structural, quiet) => {
    view.apply(ed.preview());
    ed.timeline && ed.timeline.draw();
    if (!quiet) drawHierarchy();
    if (structural) {
      // Drawn again from the top: put it back where it was scrolled to (and once more when its canvases have their sizes).
      const panel = document.getElementById('inspector');
      const at = panel ? panel.scrollTop : 0;
      drawInspector();
      if (panel) { panel.scrollTop = at; requestAnimationFrame(() => { panel.scrollTop = at; }); }
    }
    ed.dirty = true;
    clearTimeout(ed.saving);
    ed.saving = setTimeout(() => ed.save(), 500);
  };

  ed.save = async () => {
    if (!ed.dirty) return;
    ed.dirty = false;
    try {
      const r = await api('/api/project/effect/save', { name: ed.name, effect: ed.def });
      if (r && r.ok === false) throw new Error(r.error);
      say('saved ' + shortName(ed.name), 'good');
    } catch (e) { ed.dirty = true; say('not saved: ' + e.message, 'bad'); }
  };

  // What the file holds now, when the tab comes back (the JSON edited in the code editor meanwhile).
  doc.onShow = async () => {
    if (ed.dirty) return;
    try {
      const r = await api(`/api/effect?name=${encodeURIComponent(ed.name)}`);
      if (r && r.effect && JSON.stringify(r.effect) !== JSON.stringify(ed.def)) { ed.def = r.effect; doc.data.effect = r.effect; view.apply(ed.preview()); ed.timeline && ed.timeline.draw(); drawHierarchy(); drawInspector(); }
    } catch (e) { }
  };

  effectBarButtons(ed);
  ed.timeline = effectTimeline(ed);
  view.onFrame = frame => ed.timeline && ed.timeline.playhead(frame);
  drawHierarchy();
  drawInspector();
  return ed;
}

function effectBarButtons(ed) {
  const bar = ed.view.bar;
  const add = (text, title, run) => {
    const b = document.createElement('button');
    b.textContent = text;
    b.title = title;
    b.onclick = run;
    bar.append(b);
    return b;
  };
  add('Use for a spell…', 'which spell plays this effect (or casts with it): the spell\'s look in defs/spells', () => effectSpellDialog(ed));
  add('Duplicate', 'a copy of this effect under a new name', async () => {
    try {
      const r = await api('/api/project/effect/new', { id: shortName(ed.name).replace(/\.json$/i, '') + '-copy', from: ed.name });
      if (!r.ok) throw new Error(r.error);
      await loadList();
      openDoc('effect', r.name);
    } catch (e) { say(e.message, 'bad'); }
  });
  add('Delete', 'the effect\'s file (a spell that names it plays its own again)', async () => {
    if (!confirm(`Delete ${shortName(ed.name)}?`)) return;
    try {
      await api('/api/project/effect/delete', { name: ed.name });
      closeDoc(ed.doc.id);
      await loadList();
      drawList();
      say('deleted', 'good');
    } catch (e) { say(e.message, 'bad'); }
  });
  add('JSON', 'the definition as text, in the code editor', () => openDoc('code', ed.name));
}

/// One of the game's effects: a button to copy it into the mod, where it can be changed.
function effectCopyButton(doc, member) {
  const view = doc.effectView;
  const project = typeof projectState !== 'undefined' && projectState.project;
  if (!project) return;
  const b = document.createElement('button');
  b.className = 'primary';
  b.textContent = 'Copy into the mod';
  b.title = 'this member of the pack as an effect of the mod\'s own (defs/effects), to change in the editor';
  b.onclick = async () => {
    const [category, m] = member().split('/').map(n => parseInt(n, 10));
    const suggested = ((view.pack.note || '').split(',')[0] || `e${category}-${m}`).trim().toLowerCase().replace(/[^a-z0-9]+/g, '-');
    const id = prompt('A name for the mod\'s copy (its file in defs/effects):', suggested || `e${category}-${m}`);
    if (!id) return;
    try {
      const r = await api('/api/project/effect/new', { id, category, member: m });
      if (!r.ok) throw new Error(r.error);
      await loadList();
      await openDoc('effect', r.name);
      say('copied into the mod: ' + r.name, 'good');
    } catch (e) { say(e.message, 'bad'); }
  };
  view.bar.append(b);
}

/// The Effects library's "New effect…": a name and what to start from.
function newEffectDialog() {
  const body = dialog('New effect');
  const name = field(body, 'Name', '', { placeholder: 'thunder-strike - its file in defs/effects' });
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'An effect of the mod\'s own. It starts as a spray of sparks of a soft picture of its own (glow.png, made beside it); change it in the Inspector, or start from one of the game\'s instead - open it in the Effects library and Copy into the mod.';
  body.append(note);
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    try {
      const r = await api('/api/project/effect/new', { id: name.value.trim() || 'effect' });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      await loadList();
      drawList();
      openDoc('effect', r.name);
    } catch (e) { problem.textContent = e.message; }
  };
  actions.append(go);
  body.append(actions);
  name.focus();
}

// ------------------------------------------------------------------ the Hierarchy

function effectTrackIcon(type) {
  return { mesh: 'model', sound: 'audio', flash: 'image', shake: 'scene' }[type] || 'effect';
}

/// The outline: the effect, then its tracks in order (shell.js's outlineFor asks).
function effectOutline(doc) {
  const ed = doc.effectEditor;
  if (!ed) return [];
  const def = ed.def;
  const tracks = def.tracks || [];
  const addMenu = () => EFFECT_TRACK_TYPES.map(k => ({ label: 'Add ' + k.label.toLowerCase(), icon: effectTrackIcon(k.type), run: () => effectAddTrack(ed, k.type) }));
  return [
    { label: 'Effect', children: [{ label: shortName(ed.name).replace(/\.json$/i, ''), ref: 'effect', icon: 'effect', note: (def.length || 0) + ' f' + (def.loop ? ', loops' : '') }] },
    {
      label: 'Tracks', note: String(tracks.length), menu: addMenu,
      children: tracks.map((t, i) => ({
        label: t.name || t.type || 'emitter',
        note: (t.type || 'emitter') + ' · ' + (t.start || 0),
        ref: 'track:' + i,
        icon: effectTrackIcon(t.type || 'emitter'),
        eye: {
          hidden: ed.muted.has(i),
          title: ed.muted.has(i) ? 'muted on the Stage (not in the file) - click to hear it again' : 'mute it on the Stage (the file keeps it)',
          toggle: () => { if (ed.muted.has(i)) ed.muted.delete(i); else ed.muted.add(i); ed.view.apply(ed.preview()); ed.timeline.draw(); }
        },
        menu: () => [
          { label: 'Duplicate', icon: effectTrackIcon(t.type), run: () => ed.change('duplicate a track', d => { const c = effectClone(d.tracks[i]); c.name = (c.name || 'track') + ' copy'; d.tracks.splice(i + 1, 0, c); }, { structural: true }) },
          { label: 'Move up', icon: 'file', disabled: i === 0, run: () => effectMoveTrack(ed, i, i - 1) },
          { label: 'Move down', icon: 'file', disabled: i === tracks.length - 1, run: () => effectMoveTrack(ed, i, i + 1) },
          { label: 'Delete', icon: 'file', run: () => { ed.change('delete a track', d => d.tracks.splice(i, 1), { structural: true }); ed.doc.selection = 'effect'; drawInspector(); } },
          ...addMenu()
        ],
        drag: 'track:' + i,
        drop: key => { const from = parseInt(String(key).split(':')[1], 10); if (!isNaN(from) && from !== i) effectMoveTrack(ed, from, i); }
      }))
    }
  ];
}

function effectAddTrack(ed, type) {
  ed.change('add a ' + type, d => { d.tracks = d.tracks || []; d.tracks.push(effectNewTrack(type, d)); }, { structural: true });
  ed.doc.selection = 'track:' + (ed.def.tracks.length - 1);
  drawHierarchy();
  drawInspector();
}

function effectMoveTrack(ed, from, to) {
  ed.change('move a track', d => { const [t] = d.tracks.splice(from, 1); d.tracks.splice(to, 0, t); }, { structural: true });
  ed.muted.clear();
  ed.doc.selection = 'track:' + to;
  drawHierarchy();
  drawInspector();
}

// ------------------------------------------------------------------ the Inspector

/// The selection's card: the effect's own settings, or a track's modules.
function effectInspect(ed, ref) {
  const box = document.createElement('div');
  box.className = 'effect-inspector';
  if (ref === 'effect') {
    effectHead(box, 'effect', shortName(ed.name).replace(/\.json$/i, ''), ed.def.from ? 'from ' + ed.def.from : '').readOnly = true;
    box.querySelector('.object-head').append(ownBadge('mod'));
    const card = effectCard(box, 'Timeline', 'effect', null);
    effectNumber(ed, card, 'Length', d => d.length || 0, (d, v) => { d.length = Math.max(0, Math.round(v)); }, { step: 1, hint: 'the frame the timeline ends on: loops stop there, and a looping effect starts again' });
    effectBool(ed, card, 'Loop', d => !!d.loop, (d, v) => { if (v) d.loop = true; else delete d.loop; });
    const addCard = effectCard(box, 'Add a track', 'effect', null);
    for (const k of EFFECT_TRACK_TYPES) {
      const b = document.createElement('button');
      b.className = 'wide-button';
      b.textContent = '+ ' + k.label;
      b.title = k.hint;
      b.onclick = () => effectAddTrack(ed, k.type);
      addCard.append(b);
    }
    return box;
  }
  const i = parseInt(String(ref).split(':')[1], 10);
  const t = (ed.def.tracks || [])[i];
  if (!t) return null;
  const type = t.type || 'emitter';
  const T = d => d.tracks[i];
  const nameInput = effectHead(box, effectTrackIcon(type), t.name || type, type);
  nameInput.onchange = () => ed.change('rename a track', d => { T(d).name = nameInput.value; });

  // When and where.
  const when = effectCard(box, 'Timing and place', 'scene', null);
  effectNumber(ed, when, 'Start', d => T(d).start || 0, (d, v) => { T(d).start = Math.max(0, Math.round(v)); }, { step: 1, hint: 'the frame it starts on' });
  if (type === 'emitter' || type === 'mesh') {
    effectNumber(ed, when, 'Stop', d => T(d).stop === undefined ? '' : T(d).stop, (d, v) => { if (v === '' || isNaN(v)) delete T(d).stop; else T(d).stop = Math.round(v); }, { step: 1, blank: true, hint: 'the frame a looping emission stops (blank: at the effect\'s end)' });
    effectSelect(ed, when, 'Anchor', d => T(d).anchor || 'target', (d, v) => { T(d).anchor = v; }, [['target', 'the target'], ['caster', 'the caster'], ['between', 'between them'], ['world', 'the world']]);
    effectVector(ed, when, 'Offset', d => T(d).offset || [0, 0, 0], (d, v) => { if (v.every(x => x === 0)) delete T(d).offset; else T(d).offset = v; });
    effectPathCard(ed, box, i);
  }

  if (type === 'emitter') {
    const em = effectCard(box, 'Emission', 'effect', null);
    const E = d => (T(d).emission = T(d).emission || {});
    effectNumber(ed, em, 'Duration', d => E(d).duration || 0, (d, v) => { E(d).duration = Math.max(0, Math.round(v)); }, { step: 1, hint: 'frames it emits for' });
    effectNumber(ed, em, 'Interval', d => E(d).interval || 0, (d, v) => { E(d).interval = Math.max(0, Math.round(v)); }, { step: 1, hint: 'frames between bursts (0 and 1 alike: every frame)' });
    effectNumber(ed, em, 'Count', d => E(d).count || 0, (d, v) => { E(d).count = Math.max(0, Math.round(v)); }, { step: 1, hint: 'particles a burst' });
    effectNumber(ed, em, 'Bursts', d => E(d).bursts || 0, (d, v) => { E(d).bursts = Math.max(0, Math.round(v)); }, { step: 1, hint: 'bursts in the duration' });
    effectBool(ed, em, 'Loop', d => !!E(d).loop, (d, v) => { if (v) E(d).loop = true; else delete E(d).loop; }, { hint: 'emit again after the duration, until the track stops' });
    effectModule(ed, box, i, 'countOverTime', 'Count over time', () => {
      const t = ed.def.tracks[i], c = (t.emission || {}).count || 4, D = Math.max(2, (t.emission || {}).duration || 10);
      return { keys: [[1, c], [Math.round(D / 2), c * 2], [D, 0]], smooth: true };
    }, card => {
      effectCurve(ed, card, i, {
        field: 'countOverTime', names: ['count'], colours: ['#d9a441'],
        span: () => Math.max(2, (ed.def.tracks[i].emission || {}).duration || 10),
        floor: Math.max(4, (ed.def.tracks[i].emission || {}).count || 4),
        presetScale: Math.max(1, (ed.def.tracks[i].emission || {}).count || 4),
        presets: { 'swell': [[0, 0.25], [0.5, 2], [1, 0]], 'burst, then trickle': [[0, 3], [0.2, 0.5], [1, 0.25]], 'build up': [[0, 0], [1, 2]], 'steady': [[0, 1], [1, 1]] }
      });
    });
    const emissionSpan = () => Math.max(2, (ed.def.tracks[i].emission || {}).duration || 10);
    effectModule(ed, box, i, 'sizeOverTime', 'Size over time', () => ({ keys: [[1, 1], [emissionSpan(), 1]], smooth: true }), card => {
      effectCurve(ed, card, i, {
        field: 'sizeOverTime', names: ['size'], colours: ['#e07a7a'], span: emissionSpan, floor: 1.5,
        presets: { 'grow': [[0, 0.3], [1, 1.5]], 'shrink': [[0, 1.5], [1, 0.3]], 'swell': [[0, 0.5], [0.5, 1.5], [1, 0.5]], 'constant': [[0, 1], [1, 1]] }
      });
    });
    effectModule(ed, box, i, 'speedOverTime', 'Speed over time', () => ({ keys: [[1, 1], [emissionSpan(), 1]], smooth: true }), card => {
      effectCurve(ed, card, i, {
        field: 'speedOverTime', names: ['speed'], colours: ['#e0b86a'], span: emissionSpan, floor: 1.5,
        presets: { 'faster': [[0, 0.5], [1, 2]], 'slower': [[0, 2], [1, 0.5]], 'burst': [[0, 3], [0.3, 1], [1, 1]], 'constant': [[0, 1], [1, 1]] }
      });
    });

    const part = effectCard(box, 'Particle', 'effect', null);
    effectNumber(ed, part, 'Life', d => T(d).life || 1, (d, v) => {
      T(d).life = Math.max(1, Math.round(v));
      // A flipbook made from its settings follows the life (over it, or looping to its end).
      const tx = T(d).texture;
      if (tx && tx.flipbook) tx.frames = effectFlipbookFrames(tx.flipbook, T(d).life);
    }, { step: 1, hint: 'frames a particle shows' });
    effectRange(ed, part, 'Size', d => T(d).size, (d, v) => { T(d).size = v; }, { hint: 'its full width, world units' });
    effectSelect(ed, part, 'Space', d => T(d).space || 'world', (d, v) => { if (v === 'local') T(d).space = 'local'; else delete T(d).space; }, [['world', 'world: left where it is born'], ['local', 'local: follows the emitter']]);

    effectModule(ed, box, i, 'shape', 'Shape', () => ({ box: [1, 1, 1] }), card => {
      effectVector(ed, card, 'Box', d => T(d).shape.box || [0, 0, 0], (d, v) => { T(d).shape.box = v; }, { hint: 'half its size each way: particles are born anywhere inside' });
    });
    effectModule(ed, box, i, 'speed', 'Speed', () => ({ direction: [0, 1, 0], value: [0.2, 0.4] }), card => {
      effectVector(ed, card, 'Direction', d => T(d).speed.direction || [0, 1, 0], (d, v) => { T(d).speed.direction = v; });
      effectRange(ed, card, 'Speed', d => T(d).speed.value, (d, v) => { T(d).speed.value = v; }, { hint: 'world units a frame' });
      effectVector(ed, card, 'Spread', d => T(d).speed.spread || [0, 0, 0], (d, v) => { if (v.every(x => x === 0)) delete T(d).speed.spread; else T(d).speed.spread = v; }, { hint: 'degrees either way about X, Y, Z (180: any way)' });
    });
    effectModule(ed, box, i, 'gravity', 'Gravity', () => ({ direction: [0, -1, 0], value: [0.02, 0.02] }), card => {
      effectVector(ed, card, 'Direction', d => T(d).gravity.direction || [0, -1, 0], (d, v) => { T(d).gravity.direction = v; });
      effectRange(ed, card, 'Pull', d => T(d).gravity.value, (d, v) => { T(d).gravity.value = v; }, { hint: 'world units a frame, each frame' });
    });
    effectModule(ed, box, i, 'orbit', 'Orbit', () => ({ radius: 1, grow: 0.1, turn: 6 }), card => {
      effectNumber(ed, card, 'Radius', d => T(d).orbit.radius || 0, (d, v) => { T(d).orbit.radius = v; });
      effectNumberOrCurve(ed, card, i, 'Grow', 'orbit.grow', { hint: 'the radius a frame (a curve: by its age)', signed: true, floor: 0.2, colour: '#6ac48a' });
      effectNumberOrCurve(ed, card, i, 'Turn', 'orbit.turn', { hint: 'degrees a frame about the upright (a curve: by its age)', signed: true, floor: 10, colour: '#b99af0' });
    });
    effectModule(ed, box, i, 'gather', 'Gather', () => ({ speed: 0.3, accel: 0.05, swirl: [0, 20, 0] }), card => {
      effectNumber(ed, card, 'Speed', d => T(d).gather.speed || 0, (d, v) => { T(d).gather.speed = v; }, { hint: 'toward the emitter, a frame' });
      effectNumber(ed, card, 'Accel', d => T(d).gather.accel || 0, (d, v) => { T(d).gather.accel = v; });
      effectVector(ed, card, 'Swirl', d => T(d).gather.swirl || [0, 0, 0], (d, v) => { T(d).gather.swirl = v; }, { hint: 'up to these degrees a frame about X, Y, Z' });
    });
    effectModule(ed, box, i, 'trail', 'Trail', () => ({ count: 4, colour: [-255, -255, -255, -255] }), card => {
      effectNumber(ed, card, 'Images', d => T(d).trail.count || 0, (d, v) => { T(d).trail.count = Math.max(0, Math.round(v)); }, { step: 1, hint: 'after-images behind it (the last never shows, as the game\'s)' });
      effectVector(ed, card, 'Fade', d => T(d).trail.colour || [0, 0, 0, 0], (d, v) => { T(d).trail.colour = v; }, { width: 4, hint: 'the colour and alpha taken off toward the trail\'s end' });
    });
    effectModule(ed, box, i, 'colour', 'Colour over life', () => [[1, 255, 255, 255, 0], [4, 255, 255, 255, 255], [Math.max(5, (ed.def.tracks[i].life || 16) - 1), 255, 255, 255, 0]], card => {
      effectGradient(ed, card, i);
    });
    effectModule(ed, box, i, 'scale', 'Scale over life', () => [[1, 0.5, 0.5], [Math.max(2, (ed.def.tracks[i].life || 16) - 1), 1, 1]], card => {
      effectCurve(ed, card, i, {
        field: 'scale', names: ['width', 'height'],
        presets: { 'grow': [[0, 0.3], [1, 1]], 'shrink': [[0, 1], [1, 0.2]], 'pop': [[0, 0.3], [0.25, 1.2], [1, 1]], 'ease out': [[0, 0], [0.2, 0.6], [0.5, 0.9], [1, 1]], 'pulse': [[0, 1], [0.25, 1.4], [0.5, 1], [0.75, 1.4], [1, 1]], 'constant': [[0, 1], [1, 1]] }
      });
    });
    effectModule(ed, box, i, 'speedOverLife', 'Speed over life', () => ({ keys: [[1, 1], [Math.max(2, (ed.def.tracks[i].life || 16) - 1), 0.1]], smooth: true }), card => {
      effectCurve(ed, card, i, {
        field: 'speedOverLife', names: ['speed'], colours: ['#e0b86a'], floor: 1,
        presets: { 'slow down': [[0, 1], [1, 0]], 'brake': [[0, 1], [0.3, 0.15], [1, 0]], 'speed up': [[0, 0.2], [1, 1]], 'stop and go': [[0, 1], [0.4, 0], [0.6, 0], [1, 1]], 'constant': [[0, 1], [1, 1]] }
      });
    });
    effectModule(ed, box, i, 'spin', 'Spin', () => ({ angle: [0, 360], speed: [-6, 6] }), card => {
      effectRange(ed, card, 'Angle', d => T(d).spin.angle, (d, v) => { T(d).spin.angle = v; }, { hint: 'the quad\'s turn as it is born, degrees (a range: each its own)' });
      effectRange(ed, card, 'Speed', d => T(d).spin.speed, (d, v) => { T(d).spin.speed = v; }, { hint: 'degrees a frame it turns by, either way' });
    });
    effectModule(ed, box, i, 'spinOverLife', 'Spin over life', () => ({ keys: [[1, 1], [Math.max(2, (ed.def.tracks[i].life || 16) - 1), 0]], smooth: true }), card => {
      effectCurve(ed, card, i, {
        field: 'spinOverLife', names: ['spin'], colours: ['#b99af0'], floor: 1,
        presets: { 'wind down': [[0, 1], [1, 0]], 'wind up': [[0, 0], [1, 1]], 'whirl': [[0, 0], [0.3, 1.5], [1, 0]], 'constant': [[0, 1], [1, 1]] }
      });
    });
    effectModule(ed, box, i, 'gravityOverLife', 'Gravity over life', () => ({ keys: [[1, 0], [Math.max(2, (ed.def.tracks[i].life || 16) - 1), 1]], smooth: true }), card => {
      effectCurve(ed, card, i, {
        field: 'gravityOverLife', names: ['pull'], colours: ['#6fc7e6'], floor: 1,
        presets: { 'float, then fall': [[0, 0], [0.4, 0], [1, 1.5]], 'fall at once': [[0, 1], [1, 1]], 'let go': [[0, 1], [1, 0]] }
      });
    });
    effectModule(ed, box, i, 'texture', 'Texture', () => ({ image: '', width: 1, height: 1 }), card => effectTextureCard(ed, card, i));
    const render = effectCard(box, 'Render', 'image', null);
    effectSelect(ed, render, 'Blend', d => (T(d).render || {}).blend || 'alpha', (d, v) => { T(d).render = T(d).render || {}; T(d).render.blend = v; }, [['alpha', 'alpha: over what is behind'], ['additive', 'additive: light added to it']]);
    effectSelect(ed, render, 'Tint', d => (T(d).render || {}).tint || 'multiply', (d, v) => { T(d).render = T(d).render || {}; if (v === 'recolour') T(d).render.tint = v; else delete T(d).render.tint; },
      [['multiply', 'multiply: the picture times the colour (the game\'s)'], ['recolour', 'recolour: the picture\'s brightness in the colour']],
      { hint: 'multiply keeps the picture\'s own colours where the colour is white, and can only darken them; recolour paints the picture in the colour - an orange flame blue' });
  } else if (type === 'mesh') {
    const mesh = effectCard(box, 'Model', 'model', null);
    effectText(ed, mesh, 'Model', d => T(d).model || '', (d, v) => { T(d).model = v; }, { hint: 'game:<pack>:0x<id> (one of the game\'s effects\' models), or a glTF of the mod\'s: assets/x.glb' });
    // Scale and yaw: numbers, or curves over the model's steps (its life, else the effect's length).
    const meshSpan = () => Math.max(2, ed.def.tracks[i].life || ed.def.length || 30);
    if (Array.isArray(T(ed.def).scale)) effectNumber(ed, mesh, 'Scale', d => T(d).scale[0], (d, v) => { T(d).scale = v; });
    else effectNumberOrCurve(ed, mesh, i, 'Scale', 'scale', { hint: 'the model\'s size (a curve: by its step)', span: meshSpan, floor: 1.5, fallback: 1, colour: '#e07a7a' });
    effectNumberOrCurve(ed, mesh, i, 'Yaw', 'yaw', { hint: 'degrees about the upright (a curve: by its step - a turn)', span: meshSpan, signed: true, floor: 90, colour: '#b99af0' });
    effectNumber(ed, mesh, 'Life', d => T(d).life === undefined ? '' : T(d).life, (d, v) => { if (v === '' || isNaN(v)) delete T(d).life; else T(d).life = Math.round(v); }, { step: 1, blank: true, hint: 'frames it shows (blank: while the effect plays)' });
    effectText(ed, mesh, 'Clip', d => T(d).clip || '', (d, v) => { if (v) T(d).clip = v; else delete T(d).clip; }, { hint: 'a glTF\'s animation by name (the game plays it; the Stage shows the bind pose)' });
    effectBool(ed, mesh, 'Loop', d => !!T(d).loop, (d, v) => { if (v) T(d).loop = true; else delete T(d).loop; });
  } else if (type === 'sound') {
    const card = effectCard(box, 'Sound', 'audio', null);
    effectNumber(ed, card, 'Archive', d => T(d).archive || 0, (d, v) => { T(d).archive = Math.round(v); }, { step: 1, hint: 'the game\'s sound archive (200 is the battle\'s own)' });
    effectNumber(ed, card, 'Number', d => T(d).number || 0, (d, v) => { T(d).number = Math.round(v); }, { step: 1 });
    effectNumber(ed, card, 'Volume', d => T(d).volume === undefined ? 127 : T(d).volume, (d, v) => { T(d).volume = Math.round(v); }, { step: 1 });
    effectBool(ed, card, 'Load first', d => !!T(d).load, (d, v) => { if (v) T(d).load = true; else delete T(d).load; }, { hint: 'the archive loaded before it plays (one the scene has not)' });
    const note = document.createElement('p');
    note.className = 'sub';
    note.textContent = 'The Stage does not play sounds; the game does.';
    card.append(note);
  } else if (type === 'flash') {
    const card = effectCard(box, 'Flash', 'image', null);
    effectColour(ed, card, 'Colour', d => T(d).colour || [255, 255, 255], (d, v) => { T(d).colour = v; });
    effectNumber(ed, card, 'Frames', d => T(d).frames || 8, (d, v) => { T(d).frames = Math.max(1, Math.round(v)); }, { step: 1, hint: 'frames it is on' });
    effectNumber(ed, card, 'Interval', d => T(d).interval === undefined ? 2 : T(d).interval, (d, v) => { T(d).interval = Math.max(0, Math.round(v)); }, { step: 1, hint: 'frames off between' });
    effectNumber(ed, card, 'Count', d => T(d).count || 1, (d, v) => { T(d).count = Math.max(1, Math.round(v)); }, { step: 1 });
  } else if (type === 'shake') {
    const card = effectCard(box, 'Shake', 'scene', null);
    effectNumber(ed, card, 'Frames', d => T(d).frames || 10, (d, v) => { T(d).frames = Math.max(1, Math.round(v)); }, { step: 1 });
    effectNumber(ed, card, 'Power', d => T(d).power === undefined ? 0.25 : T(d).power, (d, v) => { T(d).power = v; }, { hint: 'world units' });
  }
  return box;
}

// ------------------------------------------------------------------ cards and fields

function effectHead(box, iconName, title, sub) {
  const head = document.createElement('div');
  head.className = 'object-head';
  head.append(icon(iconName));
  const name = document.createElement('input');
  name.className = 'object-name';
  name.value = title;
  head.append(name);
  box.append(head);
  if (sub) {
    const s = document.createElement('p');
    s.className = 'sub';
    s.textContent = sub;
    box.append(s);
  }
  return name;
}

/// A card; with a switch when toggle is given (checked, onToggle).
function effectCard(box, title, iconName, toggle) {
  const card = document.createElement('div');
  card.className = 'component effect-card';
  const head = document.createElement('div');
  head.className = 'component-head';
  head.append(icon(iconName));
  const b = document.createElement('b');
  b.textContent = title;
  head.append(b);
  if (toggle) {
    const sw = document.createElement('input');
    sw.type = 'checkbox';
    sw.checked = toggle.checked;
    sw.title = toggle.checked ? 'on - untick to take it out' : 'off - tick to add it';
    sw.onchange = () => toggle.onToggle(sw.checked);
    head.append(sw);
    card.classList.toggle('off', !toggle.checked);
  }
  card.append(head);
  box.append(card);
  return card;
}

/// A module a track has or has not: a card with its switch; its fields when it is on. Switched off, its settings wait
/// in the track's `off` (the players read none of it) and come back when it is switched on again.
function effectModule(ed, box, i, key, title, make, fill) {
  const t = ed.def.tracks[i];
  const on = t[key] !== undefined;
  const card = effectCard(box, title, 'effect', {
    checked: on,
    onToggle: v => ed.change((v ? 'add ' : 'remove ') + title.toLowerCase(), d => {
      const tr = d.tracks[i];
      if (v) {
        const kept = tr.off && tr.off[key];
        tr[key] = kept !== undefined ? kept : make();
        if (tr.off) { delete tr.off[key]; if (!Object.keys(tr.off).length) delete tr.off; }
      } else {
        tr.off = tr.off || {};
        tr.off[key] = tr[key];
        delete tr[key];
      }
    }, { structural: true })
  });
  if (on) fill(card);
  return card;
}

function effectRow(card, label, hint) {
  const row = document.createElement('label');
  row.className = 'behaviour-field';
  const span = document.createElement('span');
  span.textContent = label;
  if (hint) { row.title = hint; span.title = hint; }
  row.append(span);
  card.append(row);
  return row;
}

/// An input that plays as it is typed into and is one undo step when it is left.
function effectWire(ed, input, read, write) {
  // The state before the edit: taken as the field is entered, or at its first keystroke when it was not
  // (a spinner clicked, a page without focus) - never after the live edit has already changed it.
  let before = null;
  input.addEventListener('focus', () => { before = effectClone(ed.def); });
  input.addEventListener('input', () => {
    if (!before) before = effectClone(ed.def);
    ed.live(d => write(d, read()));
  });
  input.addEventListener('change', () => {
    const was = before || effectClone(ed.def);
    before = null;
    ed.change('edit', d => write(d, read()), { before: was });
  });
}

function effectNumber(ed, card, label, get, set, { step = 0.01, hint = '', blank = false } = {}) {
  const row = effectRow(card, label, hint);
  const input = document.createElement('input');
  input.type = 'number';
  input.step = String(step);
  const v = get(ed.def);
  input.value = v === '' ? '' : String(v);
  if (blank) input.placeholder = '-';
  row.append(input);
  effectWire(ed, input, () => input.value === '' ? '' : parseFloat(input.value), (d, v) => { if (v === '' && !blank) return; if (v !== '' && isNaN(v)) return; set(d, v); });
  return input;
}

function effectText(ed, card, label, get, set, { hint = '' } = {}) {
  const row = effectRow(card, label, hint);
  const input = document.createElement('input');
  input.type = 'text';
  input.value = get(ed.def);
  row.append(input);
  let before = null;
  input.addEventListener('focus', () => { before = effectClone(ed.def); });
  input.addEventListener('change', () => ed.change('edit', d => set(d, input.value.trim()), { before }));
  return input;
}

function effectBool(ed, card, label, get, set, { hint = '' } = {}) {
  const row = effectRow(card, label, hint);
  const input = document.createElement('input');
  input.type = 'checkbox';
  input.checked = get(ed.def);
  row.append(input);
  input.onchange = () => ed.change(label.toLowerCase(), d => set(d, input.checked));
  return input;
}

function effectSelect(ed, card, label, get, set, options, { hint = '', structural = false } = {}) {
  const row = effectRow(card, label, hint);
  const select = document.createElement('select');
  for (const [value, text] of options) { const o = document.createElement('option'); o.value = value; o.textContent = text; select.append(o); }
  select.value = get(ed.def);
  row.append(select);
  select.onchange = () => ed.change(label.toLowerCase(), d => set(d, select.value), { structural });
  return select;
}

function effectVector(ed, card, label, get, set, { hint = '', width = 3 } = {}) {
  const row = effectRow(card, label, hint);
  const triple = document.createElement('div');
  triple.className = 'field-triple';
  const inputs = [];
  const v = get(ed.def);
  for (let k = 0; k < width; k++) {
    const input = document.createElement('input');
    input.type = 'number';
    input.step = '0.1';
    input.value = String(v[k] === undefined ? 0 : v[k]);
    input.title = ['x', 'y', 'z', 'w'][k];
    triple.append(input);
    inputs.push(input);
  }
  row.append(triple);
  const read = () => inputs.map(x => parseFloat(x.value) || 0);
  for (const input of inputs) effectWire(ed, input, read, (d, val) => set(d, val));
}

/// A range: a low and a high (a number is both), the particle drawing between them.
function effectRange(ed, card, label, get, set, { hint = '' } = {}) {
  const row = effectRow(card, label, hint);
  const triple = document.createElement('div');
  triple.className = 'field-triple';
  const v = get(ed.def);
  const lo = Array.isArray(v) ? v[0] : (v === undefined ? 1 : v), hi = Array.isArray(v) ? (v.length > 1 ? v[1] : v[0]) : lo;
  const a = document.createElement('input'), b = document.createElement('input');
  for (const [x, val, t] of [[a, lo, 'from'], [b, hi, 'to']]) { x.type = 'number'; x.step = '0.05'; x.value = String(val); x.title = t; triple.append(x); }
  row.append(triple);
  const read = () => [parseFloat(a.value) || 0, parseFloat(b.value) || 0];
  for (const input of [a, b]) effectWire(ed, input, read, (d, val) => set(d, val));
}

function effectColour(ed, card, label, get, set) {
  const row = effectRow(card, label);
  const input = document.createElement('input');
  input.type = 'color';
  const c = get(ed.def);
  input.value = '#' + c.slice(0, 3).map(x => Math.max(0, Math.min(255, x | 0)).toString(16).padStart(2, '0')).join('');
  row.append(input);
  const read = () => [1, 3, 5].map(k => parseInt(input.value.substr(k, 2), 16));
  effectWire(ed, input, read, (d, v) => set(d, v));
}

// ------------------------------------------------------------------ the path

function effectPathCard(ed, box, i) {
  const T = d => d.tracks[i];
  const p = T(ed.def).path;
  const kind = !p ? 'none' : p.from !== undefined ? 'span' : p.point ? 'point' : 'segments';
  const card = effectCard(box, 'Path', 'map', null);
  effectSelect(ed, card, 'Kind', () => kind, (d, v) => {
    if (v === 'none') delete T(d).path;
    else if (v === 'point') T(d).path = { point: [0, 0, 0] };
    else if (v === 'span') T(d).path = { from: 'caster', to: 'target', length: 12, arc: 4 };
    else T(d).path = { curve: true, segments: [[[0, 0, 6], [6, 3, 0], [9, 0, 0], [0, 0, -9]]], times: [0], length: 12, end: 'hold' };
  }, [['none', 'none: on the anchor'], ['point', 'a point off the anchor'], ['span', 'from one anchor to another (a bolt)'], ['segments', 'a line or a curve']], { structural: true });
  if (kind === 'point') effectVector(ed, card, 'Point', d => T(d).path.point, (d, v) => { T(d).path.point = v; });
  if (kind === 'span') {
    const anchors = [['caster', 'the caster'], ['target', 'the target'], ['between', 'between them'], ['world', 'the world']];
    effectSelect(ed, card, 'From', d => T(d).path.from, (d, v) => { T(d).path.from = v; }, anchors);
    effectSelect(ed, card, 'To', d => T(d).path.to || 'target', (d, v) => { T(d).path.to = v; }, anchors);
    effectNumber(ed, card, 'Frames', d => T(d).path.length || 1, (d, v) => { T(d).path.length = Math.max(1, Math.round(v)); }, { step: 1, hint: 'how long it takes to get there' });
    effectNumber(ed, card, 'Arc', d => T(d).path.arc || 0, (d, v) => { if (v) T(d).path.arc = v; else delete T(d).path.arc; }, { hint: 'how high it rises at the middle' });
  }
  if (kind === 'span' || kind === 'segments') {
    effectSelect(ed, card, 'After', d => T(d).path.end || 'hold', (d, v) => { if (v === 'hold') delete T(d).path.end; else T(d).path.end = v; }, [['hold', 'stay at the end'], ['repeat', 'start again'], ['pingpong', 'go back and forth']]);
  }
  if (kind === 'segments') {
    effectBool(ed, card, 'Curve', d => !!T(d).path.curve, (d, v) => { T(d).path.curve = v; }, { hint: 'Hermite segments (P0, P1 and their tangents); off: straight lines P0 to P1' });
    effectNumber(ed, card, 'Frames', d => T(d).path.length || 1, (d, v) => { T(d).path.length = Math.max(1, Math.round(v)); }, { step: 1, hint: 'the whole path\'s length' });
    const row = effectRow(card, 'Segments', 'P0, P1, T0, T1 per segment, world units - one per line as x y z; x y z; ...');
    const area = document.createElement('textarea');
    area.rows = Math.min(8, Math.max(2, T(ed.def).path.segments.length + 1));
    area.value = T(ed.def).path.segments.map(s => s.map(v => v.join(' ')).join('; ')).join('\n');
    row.append(area);
    let before = null;
    area.addEventListener('focus', () => { before = effectClone(ed.def); });
    area.addEventListener('change', () => {
      const segs = area.value.split('\n').map(l => l.trim()).filter(Boolean).map(l => l.split(';').map(v => v.trim().split(/[\s,]+/).map(Number)));
      if (!segs.every(s => s.length === 4 && s.every(v => v.length === 3 && v.every(n => !isNaN(n))))) { say('a segment is four points of three numbers', 'bad'); return; }
      ed.change('edit the path', d => {
        T(d).path.segments = segs;
        const n = segs.length, len = T(d).path.length || n;
        T(d).path.times = Array.from({ length: n }, (_, k) => Math.round(k * len / n));
      }, { before });
    });
  }
}

/// A canvas drawn at the screen's own pixels, so a scaled display (125%, 150%) draws it sharp: `width` x `height`
/// CSS pixels on the page, the backing store that times the pixel ratio, the context scaled to draw in CSS pixels.
function effectSharp(canvas, width, height) {
  const ratio = window.devicePixelRatio || 1;
  canvas.style.width = width + 'px';
  canvas.style.height = height + 'px';
  if (canvas.width !== Math.round(width * ratio)) canvas.width = Math.round(width * ratio);
  if (canvas.height !== Math.round(height * ratio)) canvas.height = Math.round(height * ratio);
  const g = canvas.getContext('2d');
  g.setTransform(ratio, 0, 0, ratio, 0, 0);
  return g;
}

// ------------------------------------------------------------------ presets, saved and copied

/// A curve or a gradient apart from the life it was made on: each key at its share of the life (0 the first frame,
/// 1 the last), its own tangents (if it has them) per share too - so it fits any life it is put on. { width, smooth, keys }.
function effectCurveToPreset(curve, life, width) {
  const keys = effectCurveKeys(curve) || [];
  const span = Math.max(1, (life || 16) - 1);
  const r = v => Math.round(v * 10000) / 10000;
  return {
    width, smooth: !Array.isArray(curve) && !!(curve && curve.smooth),
    keys: keys.map(k => {
      const share = r((k[0] - 1) / span), values = k.slice(1, 1 + width);
      return k.length >= 1 + 2 * width ? [share, ...values, ...k.slice(1 + width, 1 + 2 * width).map(v => r(v * span))] : [share, ...values];
    })
  };
}

/// A preset on a life: its keys at their frames, its values fitted to the curve's (one value onto two: both; two onto one: the first).
function effectPresetToCurve(preset, life, width) {
  const span = Math.max(1, (life || 16) - 1), from = preset.width || width;
  const fit = values => from === width ? values : from > width ? values.slice(0, width) : [...values, ...new Array(width - from).fill(values[values.length - 1])];
  const keys = [];
  for (const k of preset.keys || []) {
    const age = Math.max(1, Math.round(1 + k[0] * span));
    const values = fit(k.slice(1, 1 + from));
    const key = k.length >= 1 + 2 * from ? [age, ...values, ...fit(k.slice(1 + from, 1 + 2 * from)).map(v => Math.round(v / span * 1000) / 1000)] : [age, ...values];
    if (keys.length && keys[keys.length - 1][0] === age) keys[keys.length - 1] = key; else keys.push(key);
  }
  return preset.smooth ? { keys, smooth: true } : keys;
}

// The project's saved presets (defs/effects/presets.json), read once; the copied curve or gradient, for Paste.
let effectSavedPresets = null;
let effectClipboard = null;
async function effectLoadPresets(fresh) {
  if (effectSavedPresets && !fresh) return effectSavedPresets;
  try { const r = await api('/api/project/effect/presets'); effectSavedPresets = r && r.ok ? r.presets : { gradients: {}, curves: {} }; }
  catch (e) { effectSavedPresets = { gradients: {}, curves: {} }; }
  return effectSavedPresets;
}

/// The row under a curve or a gradient: its presets - the built-in shapes, then the project's saved ones - and Copy,
/// Paste and Save as preset. opts: kind ('gradients' or 'curves'), width, life(), get() (the curve as it is),
/// set(d, curve) (put one in its place), after() (drawn again).
function effectPresets(wrap, shapes, apply, opts = {}) {
  const row = document.createElement('div');
  row.className = 'effect-preset-row';
  const select = document.createElement('select');
  select.className = 'effect-presets';
  select.title = 'put a shape in place of the keys (the life stays; undo brings them back)';
  row.append(select);
  wrap.append(row);
  const ed = opts.ed;
  const kind = opts.kind;
  const fill = async () => {
    select.textContent = '';
    const head = document.createElement('option'); head.value = ''; head.textContent = 'preset…'; select.append(head);
    const names = Object.keys(shapes || {});
    if (names.length) {
      const group = document.createElement('optgroup'); group.label = 'built in';
      for (const n of names) { const o = document.createElement('option'); o.value = 'b:' + n; o.textContent = n; group.append(o); }
      select.append(group);
    }
    if (kind && ed) {
      const saved = (await effectLoadPresets())[kind] || {};
      const mine = Object.keys(saved).filter(n => kind !== 'curves' || true);
      if (mine.length) {
        const group = document.createElement('optgroup'); group.label = 'saved in this mod';
        for (const n of mine.sort()) { const o = document.createElement('option'); o.value = 's:' + n; o.textContent = n; group.append(o); }
        select.append(group);
      }
      const more = document.createElement('optgroup'); more.label = '—';
      for (const [v, t] of [['save', 'Save as preset…'], ...(mine.length ? [['manage', 'Remove a saved preset…']] : [])]) { const o = document.createElement('option'); o.value = v; o.textContent = t; more.append(o); }
      select.append(more);
    }
  };
  fill();
  // Drawn again whole (its smooth switch, its keys' fields) - the curve put in may be another kind.
  const put = (label, curve) => { ed.change(label, d => opts.set(d, curve), { structural: true }); if (opts.after) opts.after(); };
  select.onchange = async () => {
    const v = select.value;
    select.value = '';
    if (!v) return;
    if (v.startsWith('b:')) { apply(shapes[v.slice(2)]); return; }
    if (v.startsWith('s:')) {
      const p = ((await effectLoadPresets())[kind] || {})[v.slice(2)];
      if (p) put('a saved preset', effectPresetToCurve(p, opts.life(), opts.width));
      return;
    }
    if (v === 'save') effectSavePresetDialog(kind, effectCurveToPreset(opts.get(), opts.life(), opts.width), fill);
    if (v === 'manage') effectManagePresetsDialog(kind, fill);
  };
  if (!kind || !ed) return select;
  // Copy and Paste: this curve (or gradient) to another - another track's, another effect's - fitted to its life.
  const copy = document.createElement('button');
  copy.className = 'chip';
  copy.textContent = 'Copy';
  copy.title = 'copy these keys, to paste on another ' + (kind === 'gradients' ? 'gradient' : 'curve');
  copy.onclick = () => {
    effectClipboard = { kind, preset: effectCurveToPreset(opts.get(), opts.life(), opts.width) };
    try { if (navigator.clipboard) navigator.clipboard.writeText(JSON.stringify({ crystal: 'effect-' + kind, ...effectClipboard.preset })).catch(() => {}); } catch (e) { }
    say(kind === 'gradients' ? 'gradient copied' : 'curve copied', 'good');
    for (const b of document.querySelectorAll('#inspector .effect-preset-row .paste')) b.disabled = !(effectClipboard && effectClipboard.kind === b.dataset.kind);
  };
  const paste = document.createElement('button');
  paste.className = 'chip paste';
  paste.dataset.kind = kind;
  paste.textContent = 'Paste';
  paste.title = 'put the copied ' + (kind === 'gradients' ? 'gradient' : 'curve') + ' here, fitted to this life';
  paste.disabled = !(effectClipboard && effectClipboard.kind === kind);
  paste.onclick = () => { if (effectClipboard && effectClipboard.kind === kind) put('paste', effectPresetToCurve(effectClipboard.preset, opts.life(), opts.width)); };
  row.append(copy, paste);
  return select;
}

function effectSavePresetDialog(kind, preset, done) {
  const body = dialog(kind === 'gradients' ? 'Save the gradient as a preset' : 'Save the curve as a preset');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'Kept in this mod (defs/effects/presets.json), in the preset menu of every ' + (kind === 'gradients' ? 'colour gradient' : 'curve') + ': it fits whatever life it is put on. A name already saved is replaced.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: kind === 'gradients' ? 'blue flame fade' : 'quick brake' });
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Save';
  const run = async () => {
    if (!name.value.trim()) { problem.textContent = 'A preset needs a name.'; return; }
    const r = await api('/api/project/effect/preset', { kind, name: name.value.trim(), value: preset });
    if (!r.ok) { problem.textContent = r.error; return; }
    effectSavedPresets = r.presets;
    body.close();
    say('saved the preset ' + name.value.trim(), 'good');
    if (done) done();
  };
  go.onclick = run;
  name.addEventListener('keydown', e => { if (e.key === 'Enter') run(); });
  actions.append(go);
  body.append(actions);
  name.focus();
}

async function effectManagePresetsDialog(kind, done) {
  const body = dialog('Saved presets');
  const list = document.createElement('div');
  list.className = 'effect-preset-list';
  body.append(list);
  const draw = async () => {
    list.textContent = '';
    const saved = (await effectLoadPresets())[kind] || {};
    const names = Object.keys(saved).sort();
    if (!names.length) { const p = document.createElement('p'); p.className = 'dialog-note'; p.textContent = 'None saved.'; list.append(p); return; }
    for (const n of names) {
      const r = document.createElement('div');
      r.className = 'effect-preset-item';
      const label = document.createElement('span');
      label.textContent = n;
      const x = document.createElement('button');
      x.className = 'mini';
      x.textContent = 'Remove';
      x.onclick = async () => {
        const res = await api('/api/project/effect/preset', { kind, name: n, remove: true });
        if (res.ok) { effectSavedPresets = res.presets; draw(); if (done) done(); }
      };
      r.append(label, x);
      list.append(r);
    }
  };
  draw();
}

// ------------------------------------------------------------------ colour over life: a gradient

/// The colour keys ([age, r, g, b, a], 0-255) as a strip over the particle's life, a key a marker:
/// a click adds one, a drag moves it, a right click takes it out; the selected key's colour and alpha below.
function effectGradient(ed, card, i) {
  const wrap = document.createElement('div');
  wrap.className = 'effect-gradient';
  const canvas = document.createElement('canvas');
  canvas.style.height = '46px';
  wrap.append(canvas);
  const detail = document.createElement('div');
  detail.className = 'effect-key-detail';
  wrap.append(detail);
  card.append(wrap);
  // What the colour does to the picture, said where it is set.
  const how = document.createElement('p');
  how.className = 'sub effect-tint-note';
  const tint = () => ((ed.def.tracks[i].render || {}).tint) === 'recolour';
  how.textContent = tint()
    ? 'Recolour: the picture\'s brightness painted in these colours.'
    : 'Multiplies the picture: white keeps its own colours, a colour tints and darkens them. To paint it another colour, set Tint to recolour (Render).';
  wrap.append(how);
  let selected = 0, dragging = null, before = null, cw = 220;
  const K = d => effectCurveKeys(d.tracks[i].colour);
  const keys = () => K(ed.def);
  effectSmoothSwitch(ed, wrap, i, 'colour', 4, () => draw());
  // Presets: the alpha over the life, [share of the life, alpha]; the colours stay what the keys make them there.
  effectPresets(wrap, {
    'fade in and out': [[0, 0], [0.2, 255], [0.7, 255], [1, 0]],
    'fade out': [[0, 255], [1, 0]],
    'fade in': [[0, 0], [1, 255]],
    'flash': [[0, 255], [0.3, 0], [1, 0]],
    'pulse': [[0, 0], [0.25, 255], [0.5, 60], [0.75, 255], [1, 0]]
  }, shape => {
    ed.change('a colour preset', d => {
      const t = d.tracks[i], L = Math.max(2, t.life || 16);
      const made = shape.map(([at, a]) => { const age = Math.round(1 + at * (L - 1)); const c = effectKeys(t.colour, age, 4) || [255, 255, 255, 255]; return [age, c[0] | 0, c[1] | 0, c[2] | 0, a]; });
      if (Array.isArray(t.colour)) t.colour = made; else t.colour.keys = made;
    });
    selected = 0;
    draw();
  }, {
    ed, kind: 'gradients', width: 4, life: () => life(),
    get: () => ed.def.tracks[i].colour,
    set: (d, curve) => { d.tracks[i].colour = curve; },
    after: () => { selected = 0; draw(); }
  });
  const life = () => Math.max(2, ed.def.tracks[i].life || 16);
  const xOf = age => 8 + (cw - 16) * Math.max(0, Math.min(1, (age - 1) / Math.max(1, life() - 1)));
  const ageOf = x => Math.round(1 + (x - 8) / Math.max(1, cw - 16) * (life() - 1));

  function draw(fields = true) {
    cw = Math.max(120, wrap.clientWidth || 220);
    const g = effectSharp(canvas, cw, 46);
    g.clearRect(0, 0, cw, 46);
    // A checkerboard under the strip, so alpha shows.
    for (let x = 8; x < cw - 8; x += 6) for (let y = 4; y < 28; y += 6) { g.fillStyle = ((x + y) / 6) % 2 ? '#3a3f47' : '#23272e'; g.fillRect(x, y, 6, 6); }
    for (let x = 8; x < cw - 8; x++) {
      const age = 1 + (x - 8) / Math.max(1, cw - 16) * (life() - 1);
      const c = (effectKeys(ed.def.tracks[i].colour, age, 4) || [255, 255, 255, 255]).map(v => Math.max(0, Math.min(255, v)));
      g.fillStyle = `rgba(${c[0] | 0}, ${c[1] | 0}, ${c[2] | 0}, ${c[3] / 255})`;
      g.fillRect(x, 4, 1, 24);
    }
    keys().forEach((k, n) => {
      const x = xOf(k[0]);
      g.fillStyle = `rgb(${k[1] | 0}, ${k[2] | 0}, ${k[3] | 0})`;
      g.strokeStyle = n === selected ? '#6ea8fe' : '#d7dbe2';
      g.lineWidth = n === selected ? 2 : 1;
      g.beginPath(); g.moveTo(x, 30); g.lineTo(x - 6, 42); g.lineTo(x + 6, 42); g.closePath(); g.fill(); g.stroke();
    });
    g.fillStyle = '#8b93a1';
    g.font = '10px sans-serif';
    g.fillText('1', 2, 44);
    g.fillText(String(life()), cw - 14, 44);
    if (fields) showDetail();
  }
  function hit(x) {
    let best = -1, near = 9;
    keys().forEach((k, n) => { const d = Math.abs(xOf(k[0]) - x); if (d < near) { near = d; best = n; } });
    return best;
  }
  canvas.addEventListener('pointerdown', e => {
    const x = e.offsetX;
    const n = hit(x);
    before = effectClone(ed.def);
    if (e.button === 2) { if (n >= 0 && keys().length > 1) { ed.change('remove a colour key', d => K(d).splice(n, 1), { before }); selected = 0; draw(); } return; }
    if (n >= 0) { selected = n; dragging = n; canvas.setPointerCapture(e.pointerId); draw(); return; }
    const age = ageOf(x), c = effectKeys(ed.def.tracks[i].colour, age, 4) || [255, 255, 255, 255];
    ed.change('add a colour key', d => { K(d).push([age, ...c.map(v => Math.max(0, Math.min(255, Math.round(v))))]); K(d).sort((a, b) => a[0] - b[0]); }, { before });
    selected = keys().findIndex(k => k[0] === age);
    draw();
  });
  canvas.addEventListener('pointermove', e => {
    if (dragging === null) return;
    const age = Math.max(1, Math.min(life(), ageOf(e.offsetX)));
    ed.live(d => { K(d)[dragging][0] = age; });
    draw();
  });
  canvas.addEventListener('pointerup', () => {
    if (dragging === null) return;
    const k = keys()[dragging];
    ed.change('move a colour key', d => K(d).sort((a, b) => a[0] - b[0]), { before });
    selected = keys().indexOf(k);
    dragging = null;
    draw();
  });
  canvas.addEventListener('contextmenu', e => e.preventDefault());

  function showDetail() {
    detail.textContent = '';
    const k = keys()[selected];
    if (!k) return;
    // The selected key, a row a thing: where it is, its colour, its alpha (slider and number), and a way to take it out.
    const row = (label, hint) => { const r = document.createElement('div'); r.className = 'behaviour-field'; if (hint) r.title = hint; const s = document.createElement('span'); s.textContent = label; r.append(s); detail.append(r); return r; };
    const hex = q => '#' + [q[1], q[2], q[3]].map(v => Math.max(0, Math.min(255, v | 0)).toString(16).padStart(2, '0')).join('');
    const at = row('Key at frame', 'the age (frame of its life) the key is at');
    const age = document.createElement('input');
    age.type = 'number'; age.step = '1'; age.min = '1'; age.value = String(k[0]);
    at.append(age);
    const tint = row('Colour', 'the colour at this key (multiplies the picture, or paints it - Render > Tint)');
    const colour = document.createElement('input');
    colour.type = 'color';
    colour.value = hex(k);
    const code = document.createElement('code');
    code.className = 'effect-hex';
    code.textContent = hex(k);
    tint.append(colour, code);
    const see = row('Alpha', 'how much it shows at this key: 0 not at all, 255 fully');
    see.classList.add('effect-alpha');
    const alpha = document.createElement('input');
    alpha.type = 'range'; alpha.min = '0'; alpha.max = '255'; alpha.step = '1'; alpha.value = String(k[4]);
    const alphaNumber = document.createElement('input');
    alphaNumber.type = 'number'; alphaNumber.min = '0'; alphaNumber.max = '255'; alphaNumber.step = '1'; alphaNumber.value = String(k[4]);
    see.append(alpha, alphaNumber);
    const actions = document.createElement('div');
    actions.className = 'effect-key-actions';
    const remove = document.createElement('button');
    remove.className = 'chip';
    remove.textContent = 'Remove key';
    remove.title = 'take this key out (a right click on its marker does the same)';
    remove.disabled = keys().length < 2;
    remove.onclick = () => { if (keys().length < 2) return; ed.change('remove a colour key', d => K(d).splice(selected, 1)); selected = Math.max(0, selected - 1); draw(); };
    actions.append(remove);
    detail.append(actions);
    let was = null;
    const start = () => { if (!was) was = effectClone(ed.def); };
    const commit = () => { if (was) ed.change('edit a colour key', () => {}, { before: was }); was = null; };
    for (const x of [age, colour, alpha, alphaNumber]) { x.addEventListener('focus', start); x.addEventListener('pointerdown', start); x.addEventListener('change', () => { commit(); draw(); }); }
    colour.addEventListener('input', () => { start(); const v = [1, 3, 5].map(n => parseInt(colour.value.substr(n, 2), 16)); ed.live(d => { const q = K(d)[selected]; q[1] = v[0]; q[2] = v[1]; q[3] = v[2]; }); code.textContent = colour.value; drawStrip(); });
    const setAlpha = v => { start(); v = Math.max(0, Math.min(255, v | 0)); alpha.value = String(v); alphaNumber.value = String(v); ed.live(d => { K(d)[selected][4] = v; }); drawStrip(); };
    alpha.addEventListener('input', () => setAlpha(parseInt(alpha.value, 10)));
    alphaNumber.addEventListener('input', () => { const v = parseInt(alphaNumber.value, 10); if (!isNaN(v)) setAlpha(v); });
    age.addEventListener('input', () => { const v = parseInt(age.value, 10); if (!isNaN(v)) { start(); ed.live(d => { K(d)[selected][0] = Math.max(1, v); }); drawStrip(); } });
  }
  // The strip alone, while a key's fields are being moved (the fields keep their focus).
  const drawStrip = () => draw(false);
  requestAnimationFrame(() => draw());
}

// ------------------------------------------------------------------ a value over life: a curve

/// A field of a track by its path ('orbit.turn'), and put there (the objects on the way made).
function effectGet(o, path) { return String(path).split('.').reduce((a, k) => a == null ? a : a[k], o); }
function effectSet(o, path, v) {
  const ks = String(path).split('.');
  let a = o;
  for (const k of ks.slice(0, -1)) a = a[k] = a[k] && typeof a[k] === 'object' ? a[k] : {};
  a[ks[ks.length - 1]] = v;
}
/// Whether a value is a curve: { keys } or a list of keys ([[age, v], ...]).
function effectIsCurve(v) { return !!v && typeof v === 'object' && (!Array.isArray(v) ? Array.isArray(v.keys) : Array.isArray(v[0])); }

/// A number that may be a curve over life: its field and a *curve* chip beside it; with the chip on, the curve in its place
/// (keys from the number, flat), and off the number again (the curve's first value).
function effectNumberOrCurve(ed, card, i, label, path, { hint = '', step = 0.01, span = null, colour = '#6ea8fe', signed = false, floor = 1, fallback = 0 } = {}) {
  const T = d => d.tracks[i];
  const curve = effectIsCurve(effectGet(T(ed.def), path));
  const chip = document.createElement('button');
  chip.className = 'chip' + (curve ? ' on' : '');
  chip.textContent = 'curve';
  chip.title = curve ? 'a number again (the curve\'s first value)' : 'a curve over ' + (span ? 'the track\'s time' : 'the particle\'s life') + ' in place of the number';
  chip.onclick = e => {
    e.preventDefault();
    ed.change(curve ? 'a number again' : 'a curve', d => {
      const now = effectGet(T(d), path);
      if (curve) effectSet(T(d), path, (effectKeys(now, 1, 1) || [fallback])[0]);
      else {
        const v = now === undefined || now === '' ? fallback : Number(now) || 0;
        const L = Math.max(2, span ? span() : (T(d).life || 16));
        effectSet(T(d), path, { keys: [[1, v], [L, v]], smooth: true });
      }
    }, { structural: true });
  };
  if (!curve) {
    const input = effectNumber(ed, card, label, d => { const v = effectGet(T(d), path); return v === undefined ? fallback : v; }, (d, v) => effectSet(T(d), path, v), { hint, step });
    (input.closest('.behaviour-field') || input.parentNode).append(chip);
    return;
  }
  const row = effectRow(card, label, hint);
  row.append(chip);
  effectCurve(ed, card, i, { field: path, names: [label.toLowerCase()], colours: [colour], span, signed, floor });
}

/// A switch over a curve: straight lines between its keys (a list, as the game's are), or smooth
/// ({ keys, smooth }, a Hermite curve). Straight again drops the keys' own tangents.
function effectSmoothSwitch(ed, wrap, i, field, width, redraw) {
  const label = document.createElement('label');
  label.className = 'toggle';
  const box = document.createElement('input');
  box.type = 'checkbox';
  const c = effectGet(ed.def.tracks[i], field);
  box.checked = !!c && !Array.isArray(c) && !!c.smooth;
  label.append(box, ' smooth');
  label.title = 'a smooth curve through the keys (each key\'s tangent its own, or the curve\'s); off, straight lines as the game\'s effects have';
  box.onchange = () => {
    ed.change(box.checked ? 'smooth a curve' : 'straighten a curve', d => {
      const t = d.tracks[i], keys = effectCurveKeys(effectGet(t, field)) || [];
      effectSet(t, field, box.checked ? { keys, smooth: true } : keys.map(k => k.slice(0, 1 + width)));
    });
    redraw();
  };
  wrap.append(label);
  return box;
}

/// A curve of a track's over the particle's life: a line a value (width and height; the speed), a
/// key a square - a click adds one, a drag moves it, a right click takes it out. A smooth curve's
/// selected key shows its tangents, a handle a value to pull; *Auto* gives the key the curve's own.
/// opts: field, names and colours of the values, linkable (one value moving the others), floor (the
/// least the graph shows), presets ({ name: [[share of the life, value]] }).
function effectCurve(ed, card, i, opts = {}) {
  const field = opts.field || 'scale';
  const names = opts.names || ['width', 'height'];
  const colours = opts.colours || ['#e07a7a', '#6ac48a', '#6ea8fe'];
  const width = names.length;
  const wrap = document.createElement('div');
  wrap.className = 'effect-curve';
  const canvas = document.createElement('canvas');
  canvas.style.height = '96px';
  wrap.append(canvas);
  const K = d => effectCurveKeys(effectGet(d.tracks[i], field));
  const keys = () => K(ed.def) || [];
  const smooth = () => { const c = effectGet(ed.def.tracks[i], field); return !!c && !Array.isArray(c) && !!c.smooth; };
  const bar = document.createElement('div');
  bar.className = 'effect-curve-bar';
  wrap.append(bar);
  let linked = null;
  if (width > 1 && opts.linkable !== false) {
    const link = document.createElement('label');
    link.className = 'toggle';
    linked = document.createElement('input');
    linked.type = 'checkbox';
    linked.checked = keys().every(k => k.slice(2, 1 + width).every(v => v === k[1]));
    link.append(linked, ' linked');
    link.title = 'the ' + names.slice(1).join(' and ') + ' move' + (width > 2 ? '' : 's') + ' with the ' + names[0];
    bar.append(link);
  }
  effectSmoothSwitch(ed, bar, i, field, width, () => { selected = -1; draw(); });
  const auto = document.createElement('button');
  auto.className = 'mini';
  auto.textContent = 'Auto';
  auto.title = 'the selected key\'s tangents the curve\'s own again';
  auto.onclick = () => {
    if (selected < 0) return;
    ed.change('a key\'s tangents auto', d => { const k = K(d)[selected]; if (k) k.length = 1 + width; });
    draw();
  };
  bar.append(auto);
  card.append(wrap);
  let dragging = null, before = null, selected = -1, cw = 220;
  // Along the particle's life, or (span) another stretch - the emission's frames for a count over time.
  const life = () => Math.max(2, opts.span ? opts.span() : (ed.def.tracks[i].life || 16));
  const signed = !!opts.signed;
  const top = () => Math.max(opts.floor || 1.5, ...keys().map(k => Math.max(...k.slice(1, 1 + width).map(v => signed ? Math.abs(v) : v)))) * 1.15;
  const plot = () => Math.max(1, cw - 16);
  const xOf = age => 8 + plot() * Math.max(0, Math.min(1, (age - 1) / Math.max(1, life() - 1)));
  const ageOf = x => Math.round(1 + (x - 8) / plot() * (life() - 1));
  const ageAt = x => 1 + (x - 8) / plot() * (life() - 1);
  // Unsigned: 0 at the foot, the top above; signed: 0 across the middle, as far below as above.
  const yOf = v => signed ? 48 - 40 * v / top() : 88 - 80 * v / top();
  const valueRaw = y => signed ? (48 - y) / 40 * top() : (88 - y) / 80 * top();
  const valueOf = y => { const v = Math.round(valueRaw(y) * 100) / 100; return signed ? v : Math.max(0, v); };
  // A tangent's handle: so far along the age from its key, the slope's rise over it.
  const REACH = 26;
  const handle = (k, j, side) => {
    const m = effectSlope(keys(), keys().indexOf(k), j, width);
    const frames = REACH / plot() * (life() - 1);
    return [xOf(k[0]) + side * REACH, yOf(k[1 + j] + side * m * frames)];
  };

  function draw() {
    cw = Math.max(120, wrap.clientWidth || 220);
    const g = effectSharp(canvas, cw, 96);
    g.clearRect(0, 0, cw, 96);
    g.strokeStyle = '#2b3038';
    const guide = signed ? 0 : 1;
    g.beginPath(); g.moveTo(8, yOf(guide)); g.lineTo(cw - 8, yOf(guide)); g.stroke();
    g.fillStyle = '#8b93a1'; g.font = '10px sans-serif'; g.fillText(String(guide), 1, yOf(guide) - 2);
    const curve = effectGet(ed.def.tracks[i], field);
    for (let j = 0; j < width; j++) {
      g.strokeStyle = colours[j];
      g.lineWidth = 1.5;
      g.beginPath();
      // The curve itself, through every point of the age ...
      for (let x = 8; x <= cw - 8; x++) {
        const v = effectKeys(curve, ageAt(x), width) || new Array(width).fill(1);
        const y = yOf(v[j]);
        if (x === 8) g.moveTo(x, y); else g.lineTo(x, y);
      }
      g.stroke();
      // ... and a dot where the runtime reads it: a whole frame of age at a time.
      g.fillStyle = colours[j];
      g.globalAlpha = 0.55;
      for (let age = 1; age <= life(); age++) {
        const v = effectKeys(curve, age, width) || new Array(width).fill(1);
        g.fillRect(xOf(age) - 1, yOf(v[j]) - 1, 2, 2);
      }
      g.globalAlpha = 1;
      keys().forEach((k, n) => {
        g.fillStyle = colours[j];
        g.fillRect(xOf(k[0]) - 3, yOf(k[1 + j]) - 3, 6, 6);
        if (n === selected) { g.strokeStyle = '#ffffff'; g.lineWidth = 1; g.strokeRect(xOf(k[0]) - 4.5, yOf(k[1 + j]) - 4.5, 9, 9); }
      });
    }
    const k = keys()[selected];
    if (k && smooth()) {
      const own = k.length >= 1 + 2 * width;
      for (let j = 0; j < width; j++) {
        if (linked && linked.checked && j > 0) break;
        const [x1, y1] = handle(k, j, -1), [x2, y2] = handle(k, j, 1);
        g.strokeStyle = own ? '#ffffff' : '#8b93a1';
        g.lineWidth = 1;
        g.beginPath(); g.moveTo(x1, y1); g.lineTo(x2, y2); g.stroke();
        for (const [x, y] of [[x1, y1], [x2, y2]]) { g.beginPath(); g.arc(x, y, 3.5, 0, Math.PI * 2); g.fillStyle = colours[j]; g.fill(); g.stroke(); }
      }
    }
    auto.hidden = !(k && smooth());
    auto.disabled = !(k && k.length >= 1 + 2 * width);
  }
  function hit(x, y) {
    const k = keys()[selected];
    if (k && smooth()) {
      for (let j = 0; j < width; j++) for (const side of [-1, 1]) {
        const [hx, hy] = handle(k, j, side);
        if (Math.hypot(hx - x, hy - y) < 6) return { tangent: true, n: selected, j, side };
      }
    }
    let best = null, near = 8;
    keys().forEach((key, n) => { for (let j = 0; j < width; j++) { const d = Math.hypot(xOf(key[0]) - x, yOf(key[1 + j]) - y); if (d < near) { near = d; best = { n, j }; } } });
    return best;
  }
  canvas.addEventListener('pointerdown', e => {
    const h = hit(e.offsetX, e.offsetY);
    before = effectClone(ed.def);
    if (e.button === 2) {
      if (h && !h.tangent && keys().length > 1) { ed.change('remove a key', d => K(d).splice(h.n, 1), { before }); selected = -1; draw(); }
      return;
    }
    if (h) { dragging = h; if (!h.tangent) selected = h.n; canvas.setPointerCapture(e.pointerId); draw(); return; }
    const age = ageOf(e.offsetX), v = valueOf(e.offsetY);
    const at = effectKeys(effectGet(ed.def.tracks[i], field), age, width) || new Array(width).fill(v);
    ed.change('add a key', d => { K(d).push([age, ...at.map(() => v)]); K(d).sort((a, b) => a[0] - b[0]); }, { before });
    selected = keys().findIndex(k => k[0] === age);
    draw();
  });
  canvas.addEventListener('pointermove', e => {
    if (!dragging) return;
    if (dragging.tangent) {
      // The slope from the key to the pointer, in value a frame; the other side mirrors it (one tangent a value).
      const k = keys()[dragging.n];
      const dx = Math.max(4, Math.abs(e.offsetX - xOf(k[0]))) * dragging.side;
      const frames = dx / plot() * (life() - 1);
      const rise = valueRaw(e.offsetY) - k[1 + dragging.j];
      const m = Math.round(rise / frames * 1000) / 1000;
      ed.live(d => {
        const key = K(d)[dragging.n];
        const all = K(d);
        const slopes = [];
        for (let j = 0; j < width; j++) slopes.push(effectSlope(all, dragging.n, j, width));
        for (let j = 0; j < width; j++) if (j === dragging.j || (linked && linked.checked)) slopes[j] = m;
        key.length = 1 + width;
        key.push(...slopes);
      });
      draw();
      return;
    }
    const age = Math.max(1, Math.min(life(), ageOf(e.offsetX))), v = valueOf(e.offsetY);
    ed.live(d => {
      const k = K(d)[dragging.n];
      k[0] = age;
      if (linked && linked.checked) for (let j = 0; j < width; j++) k[1 + j] = v;
      else k[1 + dragging.j] = v;
    });
    draw();
  });
  canvas.addEventListener('pointerup', () => {
    if (!dragging) return;
    const was = dragging;
    dragging = null;
    if (was.tangent) { ed.change('pull a tangent', () => {}, { before }); draw(); return; }
    const k = keys()[was.n];
    ed.change('move a key', d => K(d).sort((a, b) => a[0] - b[0]), { before });
    selected = keys().indexOf(k);
    draw();
  });
  canvas.addEventListener('contextmenu', e => e.preventDefault());
  effectPresets(wrap, opts.presets || {}, shape => {
    ed.change('a preset', d => {
      const t = d.tracks[i], L = life(), k = opts.presetScale || 1;
      const made = shape.map(([at, v]) => [Math.round(1 + at * (L - 1)), ...new Array(width).fill(Math.round(v * k * 100) / 100)]);
      const was = effectGet(t, field);
      if (Array.isArray(was) || !was) effectSet(t, field, made); else was.keys = made;
    });
    if (linked) linked.checked = true;
    selected = -1;
    draw();
  }, {
    ed, kind: 'curves', width, life: () => life(),
    get: () => effectGet(ed.def.tracks[i], field),
    set: (d, curve) => effectSet(d.tracks[i], field, curve),
    after: () => { selected = -1; draw(); }
  });
  requestAnimationFrame(draw);
}

// ------------------------------------------------------------------ the texture and its flipbook

/// The frames a flipbook's settings make (texture.flipbook, the editor's; the players read texture.frames):
/// one cell; cells from..to a cell every `each` frames, looping or held on the last; or from..to spread over the life.
function effectFlipbookFrames(fb, life) {
  if (!fb) return null;
  if (fb.mode === 'still') return [[1, Math.max(0, fb.from | 0)]];
  const from = Math.max(0, fb.from | 0), to = Math.max(0, fb.to | 0);
  const cells = [];
  for (let c = from; ; c += to >= from ? 1 : -1) { cells.push(c); if (c === to) break; }
  // A particle is drawn from age 1 to its life - 1 (it is gone on the frame its life ends).
  const L = Math.max(1, life || 16), shown = Math.max(1, L - 1), each = Math.max(1, fb.each | 0);
  const out = [];
  if (fb.end === 'fit') {
    // Each cell an equal share of the ages it shows, the last on the last of them.
    cells.forEach((c, k) => { const age = cells.length > 1 ? 1 + Math.round(k * (shown - 1) / (cells.length - 1)) : 1; if (!out.length || out[out.length - 1][0] !== age) out.push([age, c]); else out[out.length - 1][1] = c; });
  } else if (fb.end === 'hold') {
    cells.forEach((c, k) => { if (1 + k * each <= L) out.push([1 + k * each, c]); });
  } else {
    for (let age = 1, k = 0; age <= L; age += each, k++) out.push([age, cells[k % cells.length]]);
  }
  return out;
}

/// The texture: its picture, the sheet cut into a grid of cells, and what the particle shows of it over its life - one
/// cell, a play through a run of them (looping, once, or over its life), or the game's own sequence - with the sheet to
/// click cells on and a preview that plays it. Every change applies at once.
function effectTextureCard(ed, card, i) {
  const T = d => d.tracks[i];
  const tex = T(ed.def).texture;
  const pick = document.createElement('button');
  pick.className = 'wide-button';
  pick.textContent = tex.image ? effectImageLabel(tex.image) + '  (' + tex.width + ' × ' + tex.height + ')' : 'Pick a picture…';
  pick.title = 'one of the game\'s effect pictures, or a PNG of the mod\'s';
  pick.onclick = () => effectTexturePicker(ed, (chosen) => ed.change('pick a picture', d => {
    const t = T(d).texture;
    t.image = chosen.image; t.width = chosen.width; t.height = chosen.height;
    t.cell = [0, 0, chosen.width, chosen.height];
    delete t.columns; delete t.frames; delete t.flipbook;
  }, { structural: true }));
  card.append(pick);
  if (!tex.image) return;

  const W = tex.width || 1, H = tex.height || 1;
  const cellOf = t => t.cell || [0, 0, t.width || 1, t.height || 1];
  const gridOf = t => {
    const c = cellOf(t);
    const cols = t.columns || 1;
    const rows = t.columns ? Math.max(1, Math.floor((H - c[1]) / Math.max(1, c[3]))) : 1;
    return { c, cols, rows, count: t.columns ? cols * rows : 1 };
  };
  const section = text => { const h = document.createElement('div'); h.className = 'effect-subhead'; h.textContent = text; card.append(h); return h; };
  const note = text => { const p = document.createElement('p'); p.className = 'sub'; p.textContent = text; card.append(p); return p; };
  // A change to the grid or the flipbook, the frames made again from the flipbook's settings.
  const apply = (label, mutate) => ed.change(label, d => {
    mutate(T(d).texture, d);
    const t = T(d).texture;
    if (t.flipbook) t.frames = effectFlipbookFrames(t.flipbook, T(d).life);
  }, { structural: true });

  // ---- the grid
  section('Grid');
  const g0 = gridOf(tex);
  const sizes = document.createElement('div');
  sizes.className = 'effect-chips';
  const splitLabel = document.createElement('span');
  splitLabel.textContent = 'Split into';
  sizes.append(splitLabel);
  for (const n of [1, 2, 4, 8]) {
    const b = document.createElement('button');
    b.className = 'chip' + (tex.columns === n && Math.round(W / n) === g0.c[2] && Math.round(H / n) === g0.c[3] || (n === 1 && !tex.columns) ? ' on' : '');
    b.textContent = n === 1 ? 'whole picture' : `${n} × ${n}`;
    b.title = n === 1 ? 'one picture, no cells' : `${n * n} cells of ${W / n} × ${H / n}`;
    b.onclick = () => apply('split the sheet', t => {
      if (n === 1) { t.cell = [0, 0, W, H]; delete t.columns; delete t.flipbook; delete t.frames; }
      else { t.cell = [0, 0, Math.floor(W / n), Math.floor(H / n)]; t.columns = n; }
    });
    sizes.append(b);
  }
  card.append(sizes);
  const sizeRow = effectRow(card, 'Cell size', 'a cell\'s width and height in pixels; the sheet is cut into as many as fit');
  const pair = document.createElement('div');
  pair.className = 'field-triple';
  const cw = document.createElement('input'), ch = document.createElement('input');
  for (const [x, v, t] of [[cw, g0.c[2], 'width'], [ch, g0.c[3], 'height']]) { x.type = 'number'; x.min = '1'; x.step = '1'; x.value = String(v); x.title = t; pair.append(x); }
  sizeRow.append(pair);
  const resize = () => apply('cell size', t => {
    const c = cellOf(t);
    const w = Math.max(1, parseInt(cw.value, 10) || c[2]), h = Math.max(1, parseInt(ch.value, 10) || c[3]);
    t.cell = [c[0], c[1], w, h];
    const cols = Math.max(1, Math.floor((W - c[0]) / w));
    if (w >= W && h >= H) { delete t.columns; } else t.columns = cols;
  });
  cw.onchange = resize; ch.onchange = resize;
  note(tex.columns ? `${g0.cols} across × ${g0.rows} down = ${g0.count} cells, numbered from the top left` : 'The whole picture is one cell.');
  const adv = document.createElement('details');
  adv.className = 'effect-advanced';
  const advSum = document.createElement('summary');
  advSum.textContent = 'Advanced: where the first cell starts, cells a row';
  adv.append(advSum);
  card.append(adv);
  effectVector(ed, adv, 'First cell', d => cellOf(T(d).texture), (d, v) => { T(d).texture.cell = v.map(x => Math.max(0, Math.round(x))); }, { width: 4, hint: 'x, y, width and height of the first cell, in pixels' });
  effectNumber(ed, adv, 'Cells a row', d => T(d).texture.columns || 0, (d, v) => { if (v > 0) T(d).texture.columns = Math.round(v); else delete T(d).texture.columns; }, { step: 1, hint: 'how many cells a row of the sheet has (0: one picture)' });

  // ---- the animation
  section('Animation');
  const fb = tex.flipbook;
  const frames = tex.frames || [];
  const mode = fb ? fb.mode : frames.length > 1 ? 'game' : 'still';
  const modes = document.createElement('div');
  modes.className = 'effect-chips';
  const last = Math.max(0, g0.count - 1);
  const choices = [['still', 'One cell', 'the same cell all its life'], ['play', 'Play', 'step through a run of cells']];
  if (frames.length > 1 && !fb) choices.push(['game', 'The game\'s own', 'the sequence it came with, as it is']);
  for (const [m, text, title] of choices) {
    const b = document.createElement('button');
    b.className = 'chip' + (mode === m ? ' on' : '');
    b.textContent = text;
    b.title = title;
    b.disabled = m === 'play' && g0.count < 2;
    b.onclick = () => {
      if (m === mode) return;
      if (m === 'game') return;
      const first = frames.length ? frames[0][1] : 0;
      apply(m === 'still' ? 'one cell' : 'play the cells', t => {
        t.flipbook = m === 'still' ? { mode: 'still', from: first } : { mode: 'play', from: 0, to: last, each: 2, end: 'fit' };
      });
    };
    modes.append(b);
  }
  card.append(modes);
  if (g0.count < 2) note('Split the picture into cells (above) to animate it.');

  let clickHint = null;
  if (mode === 'still') {
    effectNumber(ed, card, 'Cell', d => ((T(d).texture.flipbook || {}).from) || (T(d).texture.frames && T(d).texture.frames[0] ? T(d).texture.frames[0][1] : 0),
      (d, v) => { const t = T(d).texture; t.flipbook = { mode: 'still', from: Math.max(0, Math.min(last, Math.round(v))) }; t.frames = effectFlipbookFrames(t.flipbook, T(d).life); }, { step: 1, hint: 'which cell it shows (or click one on the sheet)' });
    clickHint = 'Click a cell on the sheet to show it.';
  } else if (mode === 'play') {
    const F = d => T(d).texture.flipbook;
    const set = (d, k, v) => { const t = T(d).texture; t.flipbook[k] = v; t.frames = effectFlipbookFrames(t.flipbook, T(d).life); };
    effectNumber(ed, card, 'First cell', d => F(d).from, (d, v) => set(d, 'from', Math.max(0, Math.min(last, Math.round(v)))), { step: 1, hint: 'the cell it starts on (or click one on the sheet)' });
    effectNumber(ed, card, 'Last cell', d => F(d).to, (d, v) => set(d, 'to', Math.max(0, Math.min(last, Math.round(v)))), { step: 1, hint: 'the cell it ends on (or Shift and click one on the sheet); lower than the first plays backwards' });
    effectSelect(ed, card, 'Plays', d => F(d).end || 'loop', (d, v) => set(d, 'end', v),
      [['loop', 'over and over'], ['hold', 'once, then holds the last'], ['fit', 'once, over its whole life']], { structural: true });
    if (fb.end !== 'fit') effectNumber(ed, card, 'Frames a cell', d => F(d).each || 1, (d, v) => set(d, 'each', Math.max(1, Math.round(v))), { step: 1, hint: 'how long each cell shows, in frames (30 a second)' });
    const n = Math.abs((fb.to | 0) - (fb.from | 0)) + 1, L = T(ed.def).life || 16, shownAges = Math.max(1, L - 1);
    note(fb.end === 'fit' ? `All ${n} cells, ${fb.from} to ${fb.to}, over the ${shownAges} frames it shows.` : `${n} cells, ${fb.each || 1} frame${(fb.each || 1) === 1 ? '' : 's'} each: ${n * (fb.each || 1)} frames a pass${fb.end === 'hold' ? ', then the last cell' : ''}; it shows for ${shownAges}.`);
    // The life too short for the run: which cell it ends on, and the ways to reach the last.
    const reached = effectFrame(T(ed.def).texture.frames, shownAges);
    if (fb.end !== 'fit' && n * (fb.each || 1) > shownAges) {
      const warn = note(`It dies on cell ${reached}, before cell ${fb.to} shows: fewer frames a cell, a longer life (Particle), or Plays: once, over its whole life.`);
      warn.classList.add('effect-warn');
    }
    clickHint = 'Click a cell on the sheet to start there, Shift and click to end there.';
  } else {
    const cellsUsed = [...new Set(frames.map(f => f[1]))];
    note(`The game's sequence: ${frames.length} steps over ages ${frames[0][0]}–${frames[frames.length - 1][0]}, cells ${Math.min(...cellsUsed)}–${Math.max(...cellsUsed)}. Choose Play to make your own.`);
  }

  // ---- the sheet, still: the cells played tinted, the first and the last marked; the preview beside it plays on ▶
  const view = document.createElement('div');
  view.className = 'effect-sheet-view';
  const sheet = document.createElement('canvas');
  sheet.className = 'effect-sheet';
  const previewBox = document.createElement('div');
  previewBox.className = 'effect-flip-preview';
  const preview = document.createElement('canvas');
  const caption = document.createElement('span');
  const playButton = document.createElement('button');
  playButton.className = 'chip';
  playButton.textContent = '▶ preview';
  playButton.title = 'play the flipbook here, over the particle\'s life';
  previewBox.append(preview, caption, playButton);
  view.append(sheet, previewBox);
  card.append(view);
  if (clickHint) note(clickHint);
  const image = new Image();
  const scale = Math.min(4, 220 / Math.max(W, H));
  const PREVIEW = 72;
  sheet.style.width = W * scale + 'px'; sheet.style.height = H * scale + 'px';
  preview.style.width = PREVIEW + 'px'; preview.style.height = PREVIEW + 'px';
  // What the settings pick: the first and the last cell of a run, or the one cell.
  const picked = () => {
    const t = T(ed.def).texture, f = t.frames || [[1, 0]];
    if (t.flipbook && t.flipbook.mode === 'play') return { first: t.flipbook.from, last: t.flipbook.to };
    if (t.flipbook && t.flipbook.mode === 'still') return { first: t.flipbook.from, last: null };
    return { first: f[0][1], last: f.length > 1 ? f[f.length - 1][1] : null };
  };
  function drawSheet() {
    if (!image.complete || !image.naturalWidth) return;
    const t = T(ed.def).texture, grid = gridOf(t), c = grid.c;
    const g = effectSharp(sheet, W * scale, H * scale);
    g.imageSmoothingEnabled = false;
    g.fillStyle = '#23272e'; g.fillRect(0, 0, W * scale, H * scale);
    g.drawImage(image, 0, 0, W * scale, H * scale);
    const inUse = new Set((t.frames || [[1, 0]]).map(f => f[1]));
    const { first, last } = picked();
    g.font = '10px system-ui, sans-serif';
    const at = n => {
      const gx = t.columns ? n % grid.cols : 0, gy = t.columns ? Math.floor(n / grid.cols) : 0;
      return [(c[0] + c[2] * gx) * scale, (c[1] + c[3] * gy) * scale, c[2] * scale, c[3] * scale];
    };
    for (let n = 0; n < grid.count; n++) {
      const [x, y, w, h] = at(n);
      if (!inUse.has(n) && grid.count > 1) { g.fillStyle = 'rgba(20, 22, 26, 0.6)'; g.fillRect(x, y, w, h); }
      g.strokeStyle = 'rgba(110, 168, 254, 0.55)';
      g.lineWidth = 1;
      g.strokeRect(x + 0.5, y + 0.5, w - 1, h - 1);
      if (grid.count > 1) { g.fillStyle = '#6ea8fe'; g.fillText(String(n), x + 3, y + 11); }
    }
    // The picked cells, marked: the first (or the one) in green, the last in amber, each with its word.
    const mark = (n, colour, word) => {
      if (n === null || n === undefined || n < 0 || n >= grid.count) return;
      const [x, y, w, h] = at(n);
      g.strokeStyle = colour; g.lineWidth = 2;
      g.strokeRect(x + 1, y + 1, w - 2, h - 2);
      if (grid.count > 1 && word) {
        const tw = g.measureText(word).width + 6;
        g.fillStyle = colour; g.fillRect(x + w - tw - 1, y + h - 13, tw, 12);
        g.fillStyle = '#10151c'; g.fillText(word, x + w - tw + 2, y + h - 3);
      }
    };
    if (last !== null && last !== first) { mark(first, '#6ac48a', 'first'); mark(last, '#d9a441', 'last'); }
    else mark(first, '#6ac48a', last === null ? '' : 'first');
  }
  function drawPreview(cell) {
    const t = T(ed.def).texture, grid = gridOf(t), c = grid.c;
    const g = effectSharp(preview, PREVIEW, PREVIEW);
    g.imageSmoothingEnabled = false;
    g.fillStyle = '#101216'; g.fillRect(0, 0, PREVIEW, PREVIEW);
    if (!image.complete || !image.naturalWidth) return;
    const gx = t.columns ? cell % grid.cols : 0, gy = t.columns ? Math.floor(cell / grid.cols) : 0;
    const fit = Math.min(PREVIEW / c[2], PREVIEW / c[3]);
    g.drawImage(image, c[0] + c[2] * gx, c[1] + c[3] * gy, c[2], c[3], (PREVIEW - c[2] * fit) / 2, (PREVIEW - c[3] * fit) / 2, c[2] * fit, c[3] * fit);
  }
  const still = () => { const cell = effectFrame(T(ed.def).texture.frames, 1); drawPreview(cell); caption.textContent = `cell ${cell}`; };
  image.onload = () => { drawSheet(); still(); };
  image.src = effectTextureUrl(tex.image, ed.name);
  sheet.addEventListener('click', e => {
    const t = T(ed.def).texture, grid = gridOf(t), c = grid.c;
    if (grid.count < 2 || (mode !== 'still' && mode !== 'play')) return;
    const r = sheet.getBoundingClientRect();
    const px = (e.clientX - r.left) / scale, py = (e.clientY - r.top) / scale;
    const gx = Math.floor((px - c[0]) / c[2]), gy = Math.floor((py - c[1]) / c[3]);
    if (gx < 0 || gy < 0 || gx >= grid.cols || gy >= grid.rows) return;
    const n = gy * grid.cols + gx;
    if (mode === 'still') apply('show a cell', tt => { tt.flipbook = { mode: 'still', from: n }; });
    else apply(e.shiftKey ? 'the last cell' : 'the first cell', tt => { tt.flipbook = { ...tt.flipbook, [e.shiftKey ? 'to' : 'from']: n }; });
  });
  sheet.style.cursor = mode === 'still' || mode === 'play' ? 'pointer' : 'default';
  // The preview plays only when asked: the cell the particle shows at each age of its life, 30 a second, until stopped.
  let playing = false;
  playButton.onclick = () => {
    playing = !playing;
    playButton.textContent = playing ? '■ stop' : '▶ preview';
    playButton.classList.toggle('on', playing);
    if (!playing) { still(); return; }
    let age = 1, lastTime = performance.now();
    const tick = now => {
      if (!playing || !preview.isConnected) return;
      const L = Math.max(1, (T(ed.def).life || 16) - 1);   // the ages it is drawn: 1 to life - 1
      if (now - lastTime >= 1000 / 30) { age = age >= L ? 1 : age + 1; lastTime = now; }
      const cell = effectFrame(T(ed.def).texture.frames, age);
      drawPreview(cell);
      caption.textContent = `age ${age} · cell ${cell}`;
      requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);
  };
}

function effectImageLabel(image) {
  const m = /^game:([^:]+):(.+)$/.exec(image || '');
  return m ? m[2].replace(/\.tga?$|\.tg$/i, '') + ' (' + m[1] + ')' : image;
}

let effectGameTextures = null;

/// The picker: the mod's pictures (and one uploaded), then every one of the game's effect pictures.
async function effectTexturePicker(ed, onPick) {
  const body = dialog('A picture for the track', { wide: true });
  const filter = field(body, 'Filter', '', { placeholder: 'fire, kira, smoke…' });
  const grid = document.createElement('div');
  grid.className = 'effect-picture-grid';
  body.append(grid);
  const upload = document.createElement('input');
  upload.type = 'file';
  upload.accept = 'image/png';
  const upWrap = document.createElement('label');
  upWrap.className = 'dialog-field';
  upWrap.textContent = 'A PNG of the mod\'s own (it goes beside the effects)';
  upWrap.append(upload);
  body.insertBefore(upWrap, grid);
  const close = () => { const shut = document.querySelector('.picker.dialog .shut'); if (shut) shut.click(); };
  upload.onchange = async () => {
    const f = upload.files[0];
    if (!f) return;
    const bytes = await f.arrayBuffer();
    const r = await fetch(wsUrl(`/api/project/effect/image?name=${encodeURIComponent(f.name)}`), { method: 'POST', body: bytes }).then(x => x.json());
    if (!r.ok) { say(r.error, 'bad'); return; }
    close();
    onPick(r);
  };
  let own = [];
  try { own = await api('/api/project/effect/images'); } catch (e) { own = []; }
  if (!effectGameTextures) { try { effectGameTextures = await api('/api/effect/textures'); } catch (e) { effectGameTextures = []; } }
  const all = [...own.map(o => ({ ...o, own: true })), ...effectGameTextures];
  function fill() {
    grid.textContent = '';
    const q = filter.value.trim().toLowerCase();
    let shown = 0;
    for (const t of all) {
      if (q && !(t.image || '').toLowerCase().includes(q)) continue;
      if (shown++ > 300) break;
      const tile = document.createElement('button');
      tile.className = 'effect-picture' + (t.own ? ' own' : '');
      tile.title = `${effectImageLabel(t.image)}  ${t.width}x${t.height}`;
      const img = new Image();
      img.loading = 'lazy';
      img.src = effectTextureUrl(t.image, ed.name);
      const cap = document.createElement('span');
      cap.textContent = effectImageLabel(t.image);
      tile.append(img, cap);
      tile.onclick = () => { close(); onPick(t); };
      grid.append(tile);
    }
  }
  filter.oninput = fill;
  fill();
  filter.focus();
}

// ------------------------------------------------------------------ which spell plays it

async function effectSpellDialog(ed) {
  const id = shortName(ed.name).replace(/\.json$/i, '');
  const body = dialog('Use ' + id + ' for a spell');
  let data;
  try { data = await api('/api/project/effect/spells'); } catch (e) { say(e.message, 'bad'); return; }
  const note = document.createElement('p');
  note.className = 'dialog-note';
  const using = (data.looks || []).filter(l => l.effect === id || l.cast === id);
  note.textContent = using.length
    ? 'Plays now for: ' + using.map(l => l.spell + (l.cast === id ? ' (its cast)' : '')).join(', ') + '.'
    : 'No spell plays it yet. The spell\'s look (defs/spells) gets "effect": "' + id + '" - the battle plays it where it would play the spell\'s own, on every target.';
  body.append(note);
  const wrap = document.createElement('label');
  wrap.className = 'dialog-field';
  wrap.textContent = 'Spell';
  const select = document.createElement('select');
  for (const s of data.spells || []) { const o = document.createElement('option'); o.value = s.name; o.textContent = `${s.name}  (${s.school} ${s.level})`; select.append(o); }
  wrap.append(select);
  body.append(wrap);
  const castWrap = document.createElement('label');
  castWrap.className = 'dialog-field';
  const cast = document.createElement('input');
  cast.type = 'checkbox';
  castWrap.append(cast, ' as its cast (the glow on the caster as it begins), not its effect');
  body.append(castWrap);
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const set = document.createElement('button');
  set.className = 'primary';
  set.textContent = 'Use it';
  set.onclick = async () => {
    try {
      const r = await api('/api/project/effect/spell', { spell: select.value, effect: id, cast: cast.checked });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      say(select.value + (cast.checked ? ' casts with ' : ' plays ') + id + ' (' + r.file + ')', 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  const unset = document.createElement('button');
  unset.textContent = 'Stop using it';
  unset.title = 'the spell plays its own again';
  unset.onclick = async () => {
    try {
      const r = await api('/api/project/effect/spell', { spell: select.value, effect: '', cast: cast.checked });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      say(select.value + ' plays its own again', 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  actions.append(unset, set);
  body.append(actions);
}

/// A magic item's card (item-defs.js): the effect it plays and the glow it casts with, from the mod's effects.
async function effectItemLook(panel, spellName, { castOnly = false } = {}) {
  const card = document.createElement('div');
  card.className = 'component';
  const head = document.createElement('div');
  head.className = 'behaviour-header';
  head.textContent = castOnly ? 'Cast (the glow as it begins)' : 'Look (the effect it plays)';
  card.append(head);
  panel.append(card);
  let own = [], looks = [];
  try {
    const list = await api('/api/effects');
    own = (list || []).filter(e => e.own).map(e => shortName(e.name).replace(/\.json$/i, ''));
    looks = (await api('/api/project/effect/spells')).looks || [];
  } catch (e) { }
  const look = () => looks.find(l => (l.spell || '').toLowerCase() === String(spellName()).toLowerCase()) || {};
  const pickRow = (label, key, extra, tip) => {
    const row = document.createElement('div');
    row.className = 'behaviour-field';
    row.title = tip;
    const span = document.createElement('span');
    span.textContent = label;
    const select = document.createElement('select');
    for (const [value, text] of [['', 'its base\'s'], ...extra, ...own.map(o => [o, o])]) { const o = document.createElement('option'); o.value = value; o.textContent = text; select.append(o); }
    select.value = look()[key] || '';
    select.onchange = async () => {
      try {
        const r = await api('/api/project/effect/spell', { spell: spellName(), effect: select.value, cast: key === 'cast' });
        if (!r.ok) throw new Error(r.error);
        looks = (await api('/api/project/effect/spells')).looks || [];
        say(spellName() + ': ' + (select.value || 'its base\'s') + ' (' + r.file + ')', 'good');
      } catch (e) { say(e.message, 'bad'); }
    };
    const open = document.createElement('button');
    open.className = 'mini';
    open.textContent = '↗';
    open.title = 'open the effect in the editor';
    open.onclick = () => { if (own.includes(select.value)) openDoc('effect', 'defs/effects/' + select.value + '.json'); };
    row.append(span, select, open);
    card.append(row);
  };
  if (!castOnly) pickRow('Effect', 'effect', [], 'what plays on the target: an effect of the mod\'s (defs/effects), or its base spell\'s');
  pickRow('Cast', 'cast', [['black', 'black magic\'s'], ['white', 'white magic\'s'], ['summon', 'a summon\'s'], ['none', 'none']], 'the glow on the caster as it begins');
}

// ------------------------------------------------------------------ the timeline

/// A row a track: its bar from its start over what it plays (an emitter's emission and its particles' life; a
/// mesh's life; a flash's, a shake's frames), a diamond for a sound, the playhead over them. Drag a bar to move its
/// start; its left edge to move the start and keep the end; its right edge to set where it ends - an emitter's
/// emission (its bursts spread to end there; a single burst, or Shift, its particles' life), a mesh's life, a flash's
/// or a shake's frames; the white mark (an emitter's last burst) to spread the bursts. A click elsewhere puts the
/// playhead there; a click on a row selects its track. Drawn at the screen's pixels (effectSharp).
function effectTimeline(ed) {
  const box = ed.view.timelineBox;
  box.textContent = '';
  const canvas = document.createElement('canvas');
  canvas.className = 'effect-timeline';
  box.append(canvas);
  const LABEL = 150, ROW = 20, RULER = 18, EDGE = 5;
  let frame = 0, drag = null, before = null, W = 600, H = 60, hover = null;
  const spanNow = () => Math.max(ed.view.length || 1, ed.def.length || 1, ...(ed.def.tracks || []).map(t => extent(t)[1])) + 2;
  // The scale holds still while a bar is dragged (the bar growing would move the frames under the pointer), and follows after.
  const span = () => drag && drag.span ? drag.span : spanNow();
  const type = t => t.type || 'emitter';
  /// When an emitter's last burst is born (its whole duration for a loop): the bar ends a particle's life after it.
  function lastBurst(t) {
    const em = t.emission || {};
    if (em.loop) return em.duration || 0;
    return Math.max(0, Math.min(Math.max(0, (em.duration || 1) - 1), Math.max(0, (em.bursts || 1) - 1) * Math.max(1, em.interval || 0)));
  }
  const spreads = t => type(t) === 'emitter' && ((t.emission || {}).loop || ((t.emission || {}).bursts || 1) > 1);
  function extent(t) {
    const s = t.start || 0;
    switch (type(t)) {
      case 'emitter': return [s, s + lastBurst(t) + (t.life || 1)];
      case 'mesh': return [s, s + (t.life !== undefined ? t.life : Math.max(1, (ed.def.length || 30) - s))];
      case 'flash': return [s, s + (t.frames || 8) * Math.max(1, t.count || 1)];
      case 'shake': return [s, s + (t.frames || 10)];
      default: return [s, s + 1];
    }
  }
  /// A track made to end on a frame (its start kept): what its right edge sets.
  function endAt(t, end, life) {
    const s = t.start || 0, len = Math.max(1, end - s);
    switch (type(t)) {
      case 'emitter': {
        const em = t.emission = t.emission || {};
        if (life || !spreads(t)) { t.life = Math.max(1, len - lastBurst(t)); return; }
        // The last burst where the particles' life ends it there: a loop's duration, else the bursts spread to it.
        const last = Math.max(0, len - (t.life || 1));
        if (em.loop) { em.duration = last; return; }
        const gaps = Math.max(1, (em.bursts || 1) - 1);
        if (last < gaps) {
          // The bursts a frame apart already: shorter than that, the particles' life gives way.
          em.interval = 1; em.duration = gaps + 1;
          t.life = Math.max(1, len - gaps);
          return;
        }
        em.interval = Math.max(1, Math.round(last / gaps)); em.duration = em.interval * gaps + 1;
        return;
      }
      case 'mesh': t.life = len; return;
      case 'flash': t.frames = Math.max(1, Math.round(len / Math.max(1, t.count || 1))); return;
      case 'shake': t.frames = len; return;
    }
  }
  const xOf = f => LABEL + (W - LABEL - 8) * f / span();
  const frameOf = x => Math.max(0, Math.round((x - LABEL) / Math.max(1, W - LABEL - 8) * span()));

  function draw() {
    const tracks = ed.def.tracks || [];
    W = Math.max(300, box.clientWidth || 600);
    H = RULER + ROW * Math.max(1, tracks.length) + 4;
    const g = effectSharp(canvas, W, H);
    g.fillStyle = '#14161a'; g.fillRect(0, 0, W, H);
    g.font = '11px system-ui, "Segoe UI", sans-serif';
    g.textBaseline = 'middle';
    // The ruler: every 5 frames a tick, every 30 a second.
    for (let f = 0; f <= span(); f += 5) {
      const x = Math.round(xOf(f));
      g.fillStyle = f % 30 === 0 ? '#8b93a1' : '#3a3f47';
      g.fillRect(x, 0, 1, f % 30 === 0 ? RULER : 6);
      if (f % 30 === 0 || span() < 40) { g.fillStyle = '#8b93a1'; g.fillText(String(f), x + 3, 9); }
    }
    // The effect's own end.
    g.fillStyle = 'rgba(217, 164, 65, 0.6)';
    g.fillRect(Math.round(xOf(ed.def.length || 0)), 0, 1, H);
    const selected = String(ed.doc.selection || '');
    tracks.forEach((t, i) => {
      const y = RULER + i * ROW;
      const [a, b] = extent(t);
      const on = selected === 'track:' + i, muted = ed.muted.has(i);
      g.fillStyle = on ? '#232a35' : (i % 2 ? '#181b20' : '#15181c');
      g.fillRect(0, y, W, ROW);
      g.fillStyle = muted ? '#5b616b' : '#d7dbe2';
      // The name, cut to the label's room.
      let name = t.name || type(t);
      while (name.length > 1 && g.measureText(name).width > LABEL - 12) name = name.slice(0, -1);
      g.fillText(name !== (t.name || type(t)) ? name.slice(0, -1) + '…' : name, 6, y + ROW / 2);
      const colour = EFFECT_TRACK_COLOURS[type(t)] || '#6ea8fe';
      g.globalAlpha = muted ? 0.35 : 1;
      const x0 = Math.round(xOf(a)), x1 = Math.round(xOf(b));
      if (type(t) === 'sound') {
        g.fillStyle = colour;
        g.beginPath(); g.moveTo(x0, y + 3); g.lineTo(x0 + 7, y + ROW / 2); g.lineTo(x0, y + ROW - 3); g.lineTo(x0 - 7, y + ROW / 2); g.closePath(); g.fill();
      } else {
        g.fillStyle = colour;
        g.fillRect(x0, y + 4, Math.max(3, x1 - x0), ROW - 8);
        // The edges a bar is resized by: a lighter grip at each end while the pointer is over the bar.
        if (hover && hover.n === i && hover.part !== 'body' || drag && drag.n === i) {
          g.fillStyle = 'rgba(255, 255, 255, 0.35)';
          g.fillRect(x0, y + 4, 3, ROW - 8);
          g.fillRect(x0 + Math.max(3, x1 - x0) - 3, y + 4, 3, ROW - 8);
        }
        if (spreads(t)) {
          // Within the bar, where the last burst is born (the particles' life follows it): dragged to spread the bursts.
          const e = Math.round(xOf(a + lastBurst(t)));
          g.fillStyle = 'rgba(255, 255, 255, 0.6)';
          g.fillRect(e - 1, y + 3, 2, ROW - 6);
        }
      }
      g.globalAlpha = 1;
      if (on) { g.strokeStyle = '#6ea8fe'; g.lineWidth = 1; g.strokeRect(x0 + 0.5, y + 3.5, Math.max(3, x1 - x0) - 1, ROW - 7); }
      // While dragged: what it has come to, beside it.
      if (drag && drag.n === i) {
        const em = t.emission || {};
        const say = drag.part === 'mark' ? `last burst ${a + lastBurst(t)} · every ${Math.max(1, em.interval || 0)}`
          : drag.part === 'body' ? `start ${a}`
          : type(t) === 'emitter' ? `${a}–${b} · ${spreads(t) && !drag.life ? `emits ${em.duration || 0} f, every ${Math.max(1, em.interval || 0)}` : `life ${t.life || 1}`}`
          : `${a}–${b}`;
        g.font = '11px system-ui, "Segoe UI", sans-serif';
        const tw = g.measureText(say).width + 10, tx = Math.min(W - tw - 4, x1 + 6);
        g.fillStyle = 'rgba(10, 12, 16, 0.9)'; g.fillRect(tx, y + 2, tw, ROW - 4);
        g.fillStyle = '#d7dbe2'; g.fillText(say, tx + 5, y + ROW / 2);
      }
    });
    g.fillStyle = '#e07a7a';
    g.fillRect(Math.round(xOf(frame)), 0, 2, H);
  }
  function playhead(f, redrawn) {
    if (!redrawn && f === frame) return;
    frame = f;
    draw();
  }
  function rowAt(y) { const n = Math.floor((y - RULER) / ROW); return n >= 0 && n < (ed.def.tracks || []).length ? n : -1; }
  /// What of a bar is under the pointer: its left or right edge, its last-burst mark, or its body.
  function partAt(x, y) {
    const n = rowAt(y);
    if (n < 0 || x < LABEL) return null;
    const t = ed.def.tracks[n];
    const [a, b] = extent(t);
    const x0 = xOf(a), x1 = Math.max(xOf(b), x0 + 3);
    if (x < x0 - EDGE || x > x1 + EDGE) return null;
    if (spreads(t) && Math.abs(x - xOf(a + lastBurst(t))) <= 4) return { n, part: 'mark' };
    if (type(t) !== 'sound') {
      if (Math.abs(x - x0) <= EDGE) return { n, part: 'left' };
      if (Math.abs(x - x1) <= EDGE) return { n, part: 'right' };
    }
    return { n, part: 'body' };
  }
  const CURSORS = { left: 'ew-resize', right: 'ew-resize', mark: 'col-resize', body: 'grab' };
  const TIPS = {
    left: 'drag: the start, keeping the end',
    right: 'drag: where it ends (an emitter: its emission; a single burst or Shift: the particles\' life)',
    mark: 'drag: the last burst - the bursts spread to it',
    body: 'drag: move the track (its start)'
  };
  canvas.addEventListener('pointerdown', e => {
    const hit = partAt(e.offsetX, e.offsetY);
    if (hit) {
      const t = ed.def.tracks[hit.n];
      const [a, b] = extent(t);
      before = effectClone(ed.def);
      const at = frameOf(e.offsetX);
      drag = { ...hit, span: spanNow(), grab: at - (hit.part === 'mark' ? a + lastBurst(t) : hit.part === 'right' ? b : a), end: b, moved: false, life: e.shiftKey };
      canvas.setPointerCapture(e.pointerId);
      canvas.style.cursor = hit.part === 'body' ? 'grabbing' : CURSORS[hit.part];
      ed.doc.selection = 'track:' + hit.n;
      drawHierarchy(); drawInspector();
      draw();
      return;
    }
    const n = rowAt(e.offsetY);
    if (n >= 0 && e.offsetX < LABEL) { ed.doc.selection = 'track:' + n; drawHierarchy(); drawInspector(); draw(); return; }
    ed.view.goTo(frameOf(e.offsetX));
  });
  canvas.addEventListener('pointermove', e => {
    if (!drag) {
      const hit = partAt(e.offsetX, e.offsetY);
      canvas.style.cursor = hit ? CURSORS[hit.part] : 'default';
      canvas.title = hit ? TIPS[hit.part] : '';
      const was = hover;
      hover = hit;
      if ((was && was.n) !== (hit && hit.n) || (was && was.part) !== (hit && hit.part)) draw();
      return;
    }
    const f = Math.max(0, frameOf(e.offsetX) - drag.grab);
    drag.moved = true;
    drag.life = drag.life || e.shiftKey;
    ed.live(d => {
      const t = d.tracks[drag.n];
      if (drag.part === 'body') t.start = f;
      else if (drag.part === 'right') endAt(t, Math.max((t.start || 0) + 1, f), drag.life);
      else if (drag.part === 'left') {
        t.start = Math.min(f, drag.end - 1);
        endAt(t, drag.end, drag.life);
        // The end where it was: the bursts' whole-frame spacing leaves a frame or two, and the particles' life takes it up.
        if (type(t) === 'emitter') t.life = Math.max(1, drag.end - t.start - lastBurst(t));
      }
      else {
        // The last burst moved: a loop's duration, else the bursts spread over it (the duration holds them all).
        const em = t.emission = t.emission || {};
        const to = Math.max(0, f - (t.start || 0));
        if (em.loop) em.duration = to;
        else {
          em.interval = Math.max(1, Math.round(to / Math.max(1, (em.bursts || 1) - 1)));
          em.duration = em.interval * ((em.bursts || 1) - 1) + 1;
        }
      }
    });
    draw();
  });
  const finish = () => {
    if (!drag) return;
    const was = drag;
    drag = null;
    canvas.style.cursor = 'default';
    if (was.moved) ed.change({ body: 'move a track', left: 'move a track\'s start', right: 'resize a track', mark: 'spread the bursts' }[was.part], () => {}, { before, structural: true });
    else draw();
  };
  canvas.addEventListener('pointerup', finish);
  canvas.addEventListener('pointercancel', finish);
  canvas.addEventListener('pointerleave', () => { if (!drag && hover) { hover = null; draw(); } });
  // Redrawn when the box's width changes, a frame later (the canvas's own height would feed the observer back).
  let width = 0;
  const resize = new ResizeObserver(() => requestAnimationFrame(() => { if (box.clientWidth !== width) { width = box.clientWidth; draw(); } }));
  resize.observe(box);
  draw();
  return { draw, playhead };
}
