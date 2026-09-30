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
    if (structural) drawInspector();
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
    effectHead(box, 'effect', shortName(ed.name).replace(/\.json$/i, ''), ed.def.from ? 'from ' + ed.def.from : 'the mod\'s own').readOnly = true;
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

    const part = effectCard(box, 'Particle', 'effect', null);
    effectNumber(ed, part, 'Life', d => T(d).life || 1, (d, v) => { T(d).life = Math.max(1, Math.round(v)); }, { step: 1, hint: 'frames a particle shows' });
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
      effectNumber(ed, card, 'Grow', d => T(d).orbit.grow || 0, (d, v) => { T(d).orbit.grow = v; }, { hint: 'the radius a frame' });
      effectNumber(ed, card, 'Turn', d => T(d).orbit.turn || 0, (d, v) => { T(d).orbit.turn = v; }, { hint: 'degrees a frame about the upright' });
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
      effectCurve(ed, card, i);
    });
    effectModule(ed, box, i, 'texture', 'Texture', () => ({ image: '', width: 1, height: 1 }), card => effectTextureCard(ed, card, i));
    const render = effectCard(box, 'Render', 'image', null);
    effectSelect(ed, render, 'Blend', d => (T(d).render || {}).blend || 'alpha', (d, v) => { T(d).render = T(d).render || {}; T(d).render.blend = v; }, [['alpha', 'alpha: over what is behind'], ['additive', 'additive: light added to it']]);
  } else if (type === 'mesh') {
    const mesh = effectCard(box, 'Model', 'model', null);
    effectText(ed, mesh, 'Model', d => T(d).model || '', (d, v) => { T(d).model = v; }, { hint: 'game:<pack>:0x<id> (one of the game\'s effects\' models), or a glTF of the mod\'s: assets/x.glb' });
    effectNumber(ed, mesh, 'Scale', d => Array.isArray(T(d).scale) ? T(d).scale[0] : (T(d).scale || 1), (d, v) => { T(d).scale = v; });
    effectNumber(ed, mesh, 'Yaw', d => T(d).yaw || 0, (d, v) => { if (v) T(d).yaw = v; else delete T(d).yaw; }, { hint: 'degrees about the upright' });
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

/// A module a track has or has not: a card with its switch; its fields when it is on.
function effectModule(ed, box, i, key, title, make, fill) {
  const t = ed.def.tracks[i];
  const on = t[key] !== undefined;
  const card = effectCard(box, title, 'effect', {
    checked: on,
    onToggle: v => ed.change((v ? 'add ' : 'remove ') + title.toLowerCase(), d => { if (v) d.tracks[i][key] = make(); else delete d.tracks[i][key]; }, { structural: true })
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

/// A row of presets under a curve: a select whose choice hands its shape over, then goes back to its label.
function effectPresets(wrap, shapes, apply) {
  const select = document.createElement('select');
  select.className = 'effect-presets';
  select.title = 'put a shape in place of the keys (the life stays; undo brings them back)';
  for (const name of ['preset…', ...Object.keys(shapes)]) { const o = document.createElement('option'); o.value = name === 'preset…' ? '' : name; o.textContent = name; select.append(o); }
  select.onchange = () => { if (select.value) apply(shapes[select.value]); select.value = ''; };
  wrap.append(select);
  return select;
}

// ------------------------------------------------------------------ colour over life: a gradient

/// The colour keys ([age, r, g, b, a], 0-255) as a strip over the particle's life, a key a marker:
/// a click adds one, a drag moves it, a right click takes it out; the selected key's colour and alpha below.
function effectGradient(ed, card, i) {
  const wrap = document.createElement('div');
  wrap.className = 'effect-gradient';
  const canvas = document.createElement('canvas');
  canvas.height = 46;
  wrap.append(canvas);
  const detail = document.createElement('div');
  detail.className = 'effect-key-detail';
  wrap.append(detail);
  card.append(wrap);
  let selected = 0, dragging = null, before = null;
  const keys = () => ed.def.tracks[i].colour;
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
      t.colour = shape.map(([at, a]) => { const age = Math.round(1 + at * (L - 1)); const c = effectKeys(t.colour, age, 4) || [255, 255, 255, 255]; return [age, c[0] | 0, c[1] | 0, c[2] | 0, a]; });
    });
    selected = 0;
    draw();
  });
  const life = () => Math.max(2, ed.def.tracks[i].life || 16);
  const xOf = age => 8 + (canvas.width - 16) * Math.max(0, Math.min(1, (age - 1) / Math.max(1, life() - 1)));
  const ageOf = x => Math.round(1 + (x - 8) / Math.max(1, canvas.width - 16) * (life() - 1));

  function draw(fields = true) {
    canvas.width = Math.max(120, wrap.clientWidth || 220);
    const g = canvas.getContext('2d');
    g.clearRect(0, 0, canvas.width, canvas.height);
    // A checkerboard under the strip, so alpha shows.
    for (let x = 8; x < canvas.width - 8; x += 6) for (let y = 4; y < 28; y += 6) { g.fillStyle = ((x + y) / 6) % 2 ? '#3a3f47' : '#23272e'; g.fillRect(x, y, 6, 6); }
    for (let x = 8; x < canvas.width - 8; x++) {
      const c = effectKeys(keys(), ageOf(x), 4) || [255, 255, 255, 255];
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
    g.fillText(String(life()), canvas.width - 14, 44);
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
    if (e.button === 2) { if (n >= 0 && keys().length > 1) { ed.change('remove a colour key', d => d.tracks[i].colour.splice(n, 1), { before }); selected = 0; draw(); } return; }
    if (n >= 0) { selected = n; dragging = n; canvas.setPointerCapture(e.pointerId); draw(); return; }
    const age = ageOf(x), c = effectKeys(keys(), age, 4) || [255, 255, 255, 255];
    ed.change('add a colour key', d => { d.tracks[i].colour.push([age, ...c.map(v => Math.round(v))]); d.tracks[i].colour.sort((a, b) => a[0] - b[0]); }, { before });
    selected = keys().findIndex(k => k[0] === age);
    draw();
  });
  canvas.addEventListener('pointermove', e => {
    if (dragging === null) return;
    const age = Math.max(1, Math.min(life(), ageOf(e.offsetX)));
    ed.live(d => { d.tracks[i].colour[dragging][0] = age; });
    draw();
  });
  canvas.addEventListener('pointerup', () => {
    if (dragging === null) return;
    const k = keys()[dragging];
    ed.change('move a colour key', d => d.tracks[i].colour.sort((a, b) => a[0] - b[0]), { before });
    selected = keys().indexOf(k);
    dragging = null;
    draw();
  });
  canvas.addEventListener('contextmenu', e => e.preventDefault());

  function showDetail() {
    detail.textContent = '';
    const k = keys()[selected];
    if (!k) return;
    const row = document.createElement('div');
    row.className = 'behaviour-field';
    const age = document.createElement('input');
    age.type = 'number'; age.step = '1'; age.value = String(k[0]); age.title = 'the age (frame of its life) the key is at';
    const colour = document.createElement('input');
    colour.type = 'color';
    colour.value = '#' + [k[1], k[2], k[3]].map(v => Math.max(0, Math.min(255, v | 0)).toString(16).padStart(2, '0')).join('');
    const alpha = document.createElement('input');
    alpha.type = 'range'; alpha.min = '0'; alpha.max = '255'; alpha.value = String(k[4]); alpha.title = 'alpha';
    const label = document.createElement('span');
    label.textContent = 'key';
    row.append(label, age, colour, alpha);
    detail.append(row);
    let was = null;
    const start = () => { was = effectClone(ed.def); };
    const commit = () => ed.change('edit a colour key', () => {}, { before: was });
    for (const x of [age, colour, alpha]) { x.addEventListener('focus', start); x.addEventListener('pointerdown', start); x.addEventListener('change', () => { commit(); draw(); }); }
    colour.addEventListener('input', () => { const v = [1, 3, 5].map(n => parseInt(colour.value.substr(n, 2), 16)); ed.live(d => { const q = d.tracks[i].colour[selected]; q[1] = v[0]; q[2] = v[1]; q[3] = v[2]; }); drawStrip(); });
    alpha.addEventListener('input', () => { ed.live(d => { d.tracks[i].colour[selected][4] = parseInt(alpha.value, 10); }); drawStrip(); });
    age.addEventListener('input', () => { const v = parseInt(age.value, 10); if (!isNaN(v)) { ed.live(d => { d.tracks[i].colour[selected][0] = Math.max(1, v); }); drawStrip(); } });
  }
  // The strip alone, while a key's fields are being moved (the fields keep their focus).
  const drawStrip = () => draw(false);
  requestAnimationFrame(() => draw());
}

// ------------------------------------------------------------------ scale over life: a curve

/// The scale keys ([age, x, y]) as two lines over the particle's life - width and height - a key a
/// point: a click adds one, a drag moves it (up and down its value, across its age), a right click
/// takes it out. "Linked" moves the height with the width.
function effectCurve(ed, card, i) {
  const wrap = document.createElement('div');
  wrap.className = 'effect-curve';
  const canvas = document.createElement('canvas');
  canvas.height = 96;
  const link = document.createElement('label');
  link.className = 'toggle';
  const linked = document.createElement('input');
  linked.type = 'checkbox';
  linked.checked = (ed.def.tracks[i].scale || []).every(k => k[1] === k[2]);
  link.append(linked, ' linked: the height moves with the width');
  wrap.append(canvas, link);
  card.append(wrap);
  let dragging = null, before = null;
  // Presets: the size over the life, [share of the life, scale], both ways.
  effectPresets(wrap, {
    'grow': [[0, 0.3], [1, 1]],
    'shrink': [[0, 1], [1, 0.2]],
    'pop': [[0, 0.3], [0.25, 1.2], [1, 1]],
    'ease out': [[0, 0], [0.2, 0.6], [0.5, 0.9], [1, 1]],
    'pulse': [[0, 1], [0.25, 1.4], [0.5, 1], [0.75, 1.4], [1, 1]],
    'constant': [[0, 1], [1, 1]]
  }, shape => {
    ed.change('a scale preset', d => {
      const t = d.tracks[i], L = Math.max(2, t.life || 16);
      t.scale = shape.map(([at, v]) => [Math.round(1 + at * (L - 1)), v, v]);
    });
    linked.checked = true;
    draw();
  });
  const keys = () => ed.def.tracks[i].scale;
  const life = () => Math.max(2, ed.def.tracks[i].life || 16);
  const top = () => Math.max(1.5, ...keys().map(k => Math.max(k[1], k[2]))) * 1.15;
  const xOf = age => 8 + (canvas.width - 16) * Math.max(0, Math.min(1, (age - 1) / Math.max(1, life() - 1)));
  const ageOf = x => Math.round(1 + (x - 8) / Math.max(1, canvas.width - 16) * (life() - 1));
  const yOf = v => 88 - 80 * v / top();
  const valueOf = y => Math.max(0, Math.round((88 - y) / 80 * top() * 100) / 100);

  function draw() {
    canvas.width = Math.max(120, wrap.clientWidth || 220);
    const g = canvas.getContext('2d');
    g.clearRect(0, 0, canvas.width, canvas.height);
    g.strokeStyle = '#2b3038';
    g.beginPath(); g.moveTo(8, yOf(1)); g.lineTo(canvas.width - 8, yOf(1)); g.stroke();
    g.fillStyle = '#8b93a1'; g.font = '10px sans-serif'; g.fillText('1', 0, yOf(1) + 3);
    for (const [col, j] of [['#e07a7a', 1], ['#6ac48a', 2]]) {
      g.strokeStyle = col;
      g.lineWidth = 1.5;
      g.beginPath();
      for (let x = 8; x <= canvas.width - 8; x++) {
        const v = effectKeys(keys(), ageOf(x), 2) || [1, 1];
        const y = yOf(v[j - 1]);
        if (x === 8) g.moveTo(x, y); else g.lineTo(x, y);
      }
      g.stroke();
      for (const k of keys()) { g.fillStyle = col; g.fillRect(xOf(k[0]) - 3, yOf(k[j]) - 3, 6, 6); }
    }
  }
  function hit(x, y) {
    let best = null, near = 8;
    keys().forEach((k, n) => { for (const j of [1, 2]) { const d = Math.hypot(xOf(k[0]) - x, yOf(k[j]) - y); if (d < near) { near = d; best = { n, j }; } } });
    return best;
  }
  canvas.addEventListener('pointerdown', e => {
    const h = hit(e.offsetX, e.offsetY);
    before = effectClone(ed.def);
    if (e.button === 2) { if (h && keys().length > 1) { ed.change('remove a scale key', d => d.tracks[i].scale.splice(h.n, 1), { before }); draw(); } return; }
    if (h) { dragging = h; canvas.setPointerCapture(e.pointerId); return; }
    const age = ageOf(e.offsetX), v = valueOf(e.offsetY);
    ed.change('add a scale key', d => { d.tracks[i].scale.push([age, v, v]); d.tracks[i].scale.sort((a, b) => a[0] - b[0]); }, { before });
    draw();
  });
  canvas.addEventListener('pointermove', e => {
    if (!dragging) return;
    const age = Math.max(1, Math.min(life(), ageOf(e.offsetX))), v = valueOf(e.offsetY);
    ed.live(d => {
      const k = d.tracks[i].scale[dragging.n];
      k[0] = age;
      if (linked.checked) { k[1] = v; k[2] = v; } else k[dragging.j] = v;
    });
    draw();
  });
  canvas.addEventListener('pointerup', () => {
    if (!dragging) return;
    dragging = null;
    ed.change('move a scale key', d => d.tracks[i].scale.sort((a, b) => a[0] - b[0]), { before });
    draw();
  });
  canvas.addEventListener('contextmenu', e => e.preventDefault());
  requestAnimationFrame(draw);
}

// ------------------------------------------------------------------ the texture and its flipbook

function effectTextureCard(ed, card, i) {
  const T = d => d.tracks[i];
  const tex = T(ed.def).texture;
  const pick = document.createElement('button');
  pick.className = 'wide-button';
  pick.textContent = tex.image ? effectImageLabel(tex.image) + '  (' + tex.width + 'x' + tex.height + ')' : 'Pick a picture…';
  pick.title = 'one of the game\'s effect pictures, or a PNG of the mod\'s';
  pick.onclick = () => effectTexturePicker(ed, (chosen) => ed.change('pick a picture', d => {
    const t = T(d).texture;
    t.image = chosen.image; t.width = chosen.width; t.height = chosen.height;
    t.cell = [0, 0, chosen.width, chosen.height];
    delete t.columns; delete t.frames;
  }, { structural: true }));
  card.append(pick);
  if (!tex.image) return;
  // The sheet with its cell grid over it: the frames a flipbook steps through.
  const sheet = document.createElement('canvas');
  sheet.className = 'effect-sheet';
  card.append(sheet);
  const image = new Image();
  image.onload = () => {
    const cell = tex.cell || [0, 0, tex.width, tex.height], cols = tex.columns || 0;
    const scale = Math.min(4, 240 / Math.max(tex.width, tex.height));
    sheet.width = tex.width * scale; sheet.height = tex.height * scale;
    const g = sheet.getContext('2d');
    g.imageSmoothingEnabled = false;
    g.fillStyle = '#23272e'; g.fillRect(0, 0, sheet.width, sheet.height);
    g.drawImage(image, 0, 0, sheet.width, sheet.height);
    g.strokeStyle = 'rgba(110, 168, 254, 0.8)';
    g.fillStyle = '#6ea8fe';
    g.font = '10px sans-serif';
    const rows = cols ? Math.max(1, Math.floor((tex.height - cell[1]) / Math.max(1, cell[3]))) : 1;
    for (let n = 0; n < (cols ? cols * rows : 1); n++) {
      const gx = cols ? n % cols : 0, gy = cols ? Math.floor(n / cols) : 0;
      const x = (cell[0] + cell[2] * gx) * scale, y = (cell[1] + cell[3] * gy) * scale;
      g.strokeRect(x + 0.5, y + 0.5, cell[2] * scale - 1, cell[3] * scale - 1);
      if (cols) g.fillText(String(n), x + 2, y + 10);
    }
  };
  image.src = effectTextureUrl(tex.image, ed.name);
  effectVector(ed, card, 'Cell', d => T(d).texture.cell || [0, 0, T(d).texture.width, T(d).texture.height], (d, v) => { T(d).texture.cell = v.map(x => Math.max(0, Math.round(x))); }, { width: 4, hint: 'the first cell: its x, y, width and height in texels' });
  effectNumber(ed, card, 'Columns', d => T(d).texture.columns || 0, (d, v) => { if (v > 0) T(d).texture.columns = Math.round(v); else delete T(d).texture.columns; }, { step: 1, hint: 'cells a row of the sheet (0: one picture, no flipbook)' });
  const fb = document.createElement('div');
  fb.className = 'behaviour-header';
  fb.textContent = 'Flipbook';
  card.append(fb);
  const frames = T(ed.def).texture.frames || [];
  const list = document.createElement('p');
  list.className = 'sub';
  list.textContent = frames.length ? frames.map(f => `${f[0]}→${f[1]}`).join('  ') : 'one cell all its life';
  card.append(list);
  const row = effectRow(card, 'Cells', 'the flipbook: from cell, to cell, a frame each for so many frames');
  const triple = document.createElement('div');
  triple.className = 'field-triple';
  const from = document.createElement('input'), to = document.createElement('input'), each = document.createElement('input');
  for (const [x, v, t] of [[from, 0, 'from cell'], [to, Math.max(0, (T(ed.def).texture.columns || 1) - 1), 'to cell'], [each, 1, 'frames a cell']]) { x.type = 'number'; x.step = '1'; x.value = String(v); x.title = t; triple.append(x); }
  row.append(triple);
  const make = document.createElement('button');
  make.className = 'mini';
  make.textContent = '↻';
  make.title = 'lay the flipbook out: from the first cell to the last over and over, for the particle\'s life';
  row.append(make);
  make.onclick = () => ed.change('lay out the flipbook', d => {
    const a = parseInt(from.value, 10) || 0, b = parseInt(to.value, 10) || 0, n = Math.max(1, parseInt(each.value, 10) || 1);
    const out = [];
    const cells = b >= a ? b - a + 1 : a - b + 1, life = d.tracks[i].life || 16;
    for (let age = 1, k = 0; age <= life; age += n, k++) out.push([age, b >= a ? a + (k % cells) : a - (k % cells)]);
    d.tracks[i].texture.frames = out;
  }, { structural: true });
  if (frames.length) {
    const clear = document.createElement('button');
    clear.className = 'wide-button';
    clear.textContent = 'No flipbook';
    clear.onclick = () => ed.change('no flipbook', d => { delete d.tracks[i].texture.frames; }, { structural: true });
    card.append(clear);
  }
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
async function effectItemLook(panel, spellName) {
  const card = document.createElement('div');
  card.className = 'component';
  const head = document.createElement('div');
  head.className = 'behaviour-header';
  head.textContent = 'Look (the effect it plays)';
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
  pickRow('Effect', 'effect', [], 'what plays on the target: an effect of the mod\'s (defs/effects), or its base spell\'s');
  pickRow('Cast', 'cast', [['black', 'black magic\'s'], ['white', 'white magic\'s'], ['summon', 'a summon\'s'], ['none', 'none']], 'the glow on the caster as it begins');
}

// ------------------------------------------------------------------ the timeline

/// A row a track: its bar from its start over what it plays (an emitter's emission and its
/// particles' life; a mesh's life), a diamond for a moment (a sound, a flash, a shake); the
/// playhead over them. Drag a bar to move its start, its white mark (the last burst) to spread its bursts; a
/// click elsewhere puts the playhead there; a click on a bar selects the track.
function effectTimeline(ed) {
  const box = ed.view.timelineBox;
  box.textContent = '';
  const canvas = document.createElement('canvas');
  canvas.className = 'effect-timeline';
  box.append(canvas);
  const LABEL = 130, ROW = 18, RULER = 16;
  let frame = 0, drag = null, before = null;
  const span = () => Math.max(ed.view.length || 1, ed.def.length || 1, ...(ed.def.tracks || []).map(t => extent(t)[1])) + 2;
  /// When an emitter's last burst is born (its whole duration for a loop): the bar ends a particle's life after it.
  function lastBurst(t) {
    const em = t.emission || {};
    if (em.loop) return em.duration || 0;
    return Math.max(0, Math.min(Math.max(0, (em.duration || 1) - 1), Math.max(0, (em.bursts || 1) - 1) * Math.max(1, em.interval || 0)));
  }
  function extent(t) {
    const s = t.start || 0;
    switch (t.type || 'emitter') {
      case 'emitter': return [s, s + lastBurst(t) + (t.life || 1)];
      case 'mesh': return [s, s + (t.life !== undefined ? t.life : Math.max(1, (ed.def.length || 30) - s))];
      case 'flash': return [s, s + (t.frames || 8) * Math.max(1, t.count || 1)];
      case 'shake': return [s, s + (t.frames || 10)];
      default: return [s, s + 1];
    }
  }
  const xOf = f => LABEL + (canvas.width - LABEL - 8) * f / span();
  const frameOf = x => Math.max(0, Math.round((x - LABEL) / Math.max(1, canvas.width - LABEL - 8) * span()));

  function draw() {
    const tracks = ed.def.tracks || [];
    canvas.width = Math.max(300, box.clientWidth || 600);
    canvas.height = RULER + ROW * Math.max(1, tracks.length) + 4;
    const g = canvas.getContext('2d');
    g.fillStyle = '#14161a'; g.fillRect(0, 0, canvas.width, canvas.height);
    g.font = '10px sans-serif';
    // The ruler: every 5 frames a tick, every 30 a second.
    for (let f = 0; f <= span(); f += 5) {
      const x = xOf(f);
      g.fillStyle = f % 30 === 0 ? '#8b93a1' : '#3a3f47';
      g.fillRect(x, 0, 1, f % 30 === 0 ? RULER : 6);
      if (f % 30 === 0 || span() < 40) { g.fillStyle = '#8b93a1'; g.fillText(String(f), x + 2, 10); }
    }
    // The effect's own end.
    g.fillStyle = 'rgba(217, 164, 65, 0.6)';
    g.fillRect(xOf(ed.def.length || 0), 0, 1, canvas.height);
    const selected = String(ed.doc.selection || '');
    tracks.forEach((t, i) => {
      const y = RULER + i * ROW;
      const [a, b] = extent(t);
      const on = selected === 'track:' + i, muted = ed.muted.has(i);
      g.fillStyle = on ? '#232a35' : (i % 2 ? '#181b20' : '#15181c');
      g.fillRect(0, y, canvas.width, ROW);
      g.fillStyle = muted ? '#5b616b' : '#d7dbe2';
      g.fillText((t.name || t.type || 'emitter').slice(0, 20), 6, y + 12);
      const colour = EFFECT_TRACK_COLOURS[t.type || 'emitter'] || '#6ea8fe';
      g.globalAlpha = muted ? 0.35 : 1;
      if ((t.type || 'emitter') === 'sound') {
        const x = xOf(a);
        g.fillStyle = colour;
        g.beginPath(); g.moveTo(x, y + 3); g.lineTo(x + 6, y + 9); g.lineTo(x, y + 15); g.lineTo(x - 6, y + 9); g.closePath(); g.fill();
      } else {
        g.fillStyle = colour;
        g.fillRect(xOf(a), y + 4, Math.max(3, xOf(b) - xOf(a)), ROW - 8);
        if ((t.type || 'emitter') === 'emitter' && ((t.emission || {}).loop || ((t.emission || {}).bursts || 1) > 1)) {
          // Within the bar, where the last burst is born (the particles' life follows it): dragged to spread the bursts.
          const e = a + lastBurst(t);
          g.fillStyle = 'rgba(255, 255, 255, 0.55)';
          g.fillRect(xOf(e) - 1, y + 3, 2, ROW - 6);
        }
      }
      g.globalAlpha = 1;
      if (on) { g.strokeStyle = '#6ea8fe'; g.strokeRect(xOf(a) + 0.5, y + 3.5, Math.max(3, xOf(b) - xOf(a)) - 1, ROW - 7); }
    });
    playhead(frame, true);
  }
  function playhead(f, redrawn) {
    if (!redrawn && f === frame) return;
    if (!redrawn) { frame = f; draw(); return; }
    const g = canvas.getContext('2d');
    g.fillStyle = '#e07a7a';
    g.fillRect(xOf(frame), 0, 2, canvas.height);
  }
  function rowAt(y) { const n = Math.floor((y - RULER) / ROW); return n >= 0 && n < (ed.def.tracks || []).length ? n : -1; }
  canvas.addEventListener('pointerdown', e => {
    const n = rowAt(e.offsetY);
    if (n >= 0 && e.offsetX >= LABEL) {
      const t = ed.def.tracks[n];
      const [a, b] = extent(t);
      const emissionEnd = a + lastBurst(t);
      if (e.offsetX >= xOf(a) - 4 && e.offsetX <= xOf(b) + 4) {
        before = effectClone(ed.def);
        const em = t.emission || {};
        const stretching = (t.type || 'emitter') === 'emitter' && (em.loop || (em.bursts || 1) > 1) && Math.abs(e.offsetX - xOf(emissionEnd)) < 5;
        drag = { n, stretching, grab: frameOf(e.offsetX) - (stretching ? emissionEnd : a), moved: false };
        canvas.setPointerCapture(e.pointerId);
        ed.doc.selection = 'track:' + n;
        drawHierarchy(); drawInspector();
        draw();
        return;
      }
    }
    if (n >= 0 && e.offsetX < LABEL) { ed.doc.selection = 'track:' + n; drawHierarchy(); drawInspector(); draw(); return; }
    ed.view.goTo(frameOf(e.offsetX));
  });
  canvas.addEventListener('pointermove', e => {
    if (!drag) { canvas.style.cursor = rowAt(e.offsetY) >= 0 && e.offsetX >= LABEL ? 'grab' : 'default'; return; }
    const f = Math.max(0, frameOf(e.offsetX) - drag.grab);
    drag.moved = true;
    ed.live(d => {
      const t = d.tracks[drag.n];
      if (drag.stretching) {
        // The last burst moved: a loop's duration, else the bursts spread over it (the duration holds them all).
        const em = t.emission = t.emission || {};
        const to = Math.max(0, f - (t.start || 0));
        if (em.loop) em.duration = to;
        else {
          em.interval = Math.max(1, Math.round(to / Math.max(1, (em.bursts || 1) - 1)));
          em.duration = Math.max(em.duration || 0, em.interval * ((em.bursts || 1) - 1) + 1);
        }
      }
      else t.start = f;
    });
    draw();
  });
  canvas.addEventListener('pointerup', () => {
    if (!drag) return;
    if (drag.moved) ed.change(drag.stretching ? 'stretch an emission' : 'move a track', () => {}, { before, structural: true });
    drag = null;
  });
  // Redrawn when the box's width changes, a frame later (the canvas's own height would feed the observer back).
  let width = 0;
  const resize = new ResizeObserver(() => requestAnimationFrame(() => { if (box.clientWidth !== width) { width = box.clientWidth; draw(); } }));
  resize.observe(box);
  draw();
  return { draw, playhead };
}
