// A weapon on a character, in the model viewer: the "on a character" toggle under a weapon or
// shield model puts one of the game's character models (j101...) under it, plays that
// character's battle motions through the viewer's transport, and poses the viewed model from
// the hand joint each frame exactly as the game does (btl.BattlePlayer.haveWeapon: the
// joint's matrix, R_te for the right hand, L_te the left, R_ude / L_ude the forearms for a
// shield; then the grip's turn and offset - RotZ 270 and (-0.6, 0, 0) for a right-hand
// weapon, RotZ 90 and (0.6, 0, 0) for the left, RotX 345 / 15 and (-+0.6, 0.25, -0.8) for a
// shield). When the viewed model is a glTF an item definition names, the item's fit
// (modelScale, modelRotation, modelOffset - what the client applies in battle) goes in front
// of all that, and the fields here edit and save it, so a Blender export is fitted to the
// attack animations by eye. The model on its own is the default; this is a toggle.

/// Wires the hand bar of a model view. `transport` is wireAnimation's controller (or null).
async function wireHandPreview(node, viewer, name, model, transport) {
  const bar = $('.hand-bar', node);
  if (!bar || !viewer.setCompanion) return;
  const isAsset = /\.(glb|gltf)$/i.test(name);
  const isWeaponModel = /(^|\/)w\d{3}\.nmdp\.lz$/i.test(name);
  if (!isAsset && !isWeaponModel) return;

  // The item definition whose look this model is, for the fit fields; none for the game's own w###.
  let item = null;
  if (isAsset && typeof openFFProject === 'function' && openFFProject()) {
    try {
      const r = await api('/api/project/items');
      const items = (r && r.items) || [];
      item = items.find(i => i.model && i.model.toLowerCase() === name.toLowerCase()) || null;
    } catch (error) { item = null; }
  }
  bar.hidden = false;

  const on = $('.hand-on', bar);
  const controls = $('.hand-controls', bar);
  const bodySelect = $('.hand-body', bar);
  const which = $('.hand-which', bar);
  const fit = $('.hand-fit', bar);
  const note = $('.hand-note', bar);
  const scale = $('.hand-scale', bar);
  const rot = [$('.hand-rx', bar), $('.hand-ry', bar), $('.hand-rz', bar)];
  const off = [$('.hand-ox', bar), $('.hand-oy', bar), $('.hand-oz', bar)];
  const reset = $('.hand-reset', bar);

  // The fit as the definition has it (identity for a model without one).
  const fitValues = {
    scale: item ? (item.modelScale || 1) : 1,
    rotation: item && item.modelRotation ? item.modelRotation.slice(0, 3) : [0, 0, 0],
    offset: item && item.modelOffset ? item.modelOffset.slice(0, 3) : [0, 0, 0]
  };
  fit.hidden = !item;
  if (item) {
    scale.value = fitValues.scale;
    rot.forEach((input, i) => input.value = fitValues.rotation[i] || 0);
    off.forEach((input, i) => input.value = fitValues.offset[i] || 0);
    note.textContent = `${item.name || item.id} - the fit saves to defs/items/${item.id}.json`;
    // A shield sits on the forearm.
    const graph = (item.fields || []).find(f => f.name === 'graphId');
    if (item.chain === 'armour' && graph && ((graph.value ?? graph.baseValue) > 0)) which.value = 'leftShield';
  } else if (isWeaponModel) {
    which.value = /w26\d|w27\d|w28\d/i.test(name) ? 'leftShield' : 'right';
    note.textContent = 'the game\'s own model, as the game holds it';
  } else {
    note.textContent = 'no item definition names this file yet - its fit is the identity';
  }

  // The character models: the j### the game has.
  const bodies = ((await api('/api/models').catch(() => [])) || [])
    .map(p => p.name).filter(n => /(^|\/)j\d{3}\.nmdp\.lz$/i.test(n));
  for (const body of bodies) {
    const option = document.createElement('option');
    option.value = body;
    option.textContent = shortName(body).replace(/\.nmdp\.lz$/i, '');
    bodySelect.append(option);
  }
  const remembered = (() => { try { return localStorage.getItem('ff3-editor-hand-body'); } catch (error) { return null; } })();
  if (remembered && bodies.includes(remembered)) bodySelect.value = remembered;
  else if (bodies.includes('files/j101.nmdp.lz')) bodySelect.value = 'files/j101.nmdp.lz';

  let joint = null;        // /api/model/joint for the body, pack, motion and hand showing
  let jointFor = '';       // its key, to skip a refetch
  let bodyName = null;

  const jointName = () => ({ right: 'R_te', left: 'L_te', rightShield: 'R_ude', leftShield: 'L_ude' })[which.value] || 'R_te';

  /// The grip: the turn and offset the game puts between the joint and the model (fixed point 2457 = 0.6, 1024 = 0.25, 3276 = 0.8).
  const grip = () => {
    switch (which.value) {
      case 'right': return multiply(translate(-0.6, 0, 0), rotateZ(270));
      case 'left': return multiply(translate(0.6, 0, 0), rotateZ(90));
      case 'rightShield': return multiply(translate(-0.6, 0.25, -0.8), rotateX(345));
      case 'leftShield': return multiply(translate(0.6, 0.25, -0.8), rotateX(15));
      default: return IDENTITY;
    }
  };

  /// The item's fit: scale, then turns about X, Y, Z, then the offset - WeaponMeshes.Fit's order.
  const fitMatrix = () => {
    let m = scaleMatrix(fitValues.scale);
    m = multiply(rotateX(fitValues.rotation[0] || 0), m);
    m = multiply(rotateY(fitValues.rotation[1] || 0), m);
    m = multiply(rotateZ(fitValues.rotation[2] || 0), m);
    return multiply(translate(fitValues.offset[0] || 0, fitValues.offset[1] || 0, fitValues.offset[2] || 0), m);
  };

  /// The joint's 4x3 for a frame as a column-major 4x4 (the DS row-vector layout is its transpose, which is this layout).
  const jointMatrix = (frame) => {
    if (!joint || !joint.matrices || !joint.frames) return IDENTITY;
    const f = Math.max(0, Math.min(joint.frames - 1, frame || 0));
    const d = joint.matrices, at = f * 12;
    return new Float32Array([
      d[at], d[at + 1], d[at + 2], 0,
      d[at + 3], d[at + 4], d[at + 5], 0,
      d[at + 6], d[at + 7], d[at + 8], 0,
      d[at + 9], d[at + 10], d[at + 11], 1
    ]);
  };

  /// The viewed model into the hand for a frame: joint x grip x fit (applied fit first).
  const place = (frame) => {
    if (!on.checked || !bodyName) { viewer.setAttach(null); return; }
    viewer.setAttach(multiply(jointMatrix(frame), multiply(grip(), fitMatrix())));
  };

  /// The joint track for what the transport shows, fetched when it changes.
  const trackJoint = async (frame, pose, pack, index) => {
    if (!on.checked || !bodyName) return;
    const key = `${bodyName}|${pack || ''}|${index}|${jointName()}`;
    if (key !== jointFor) {
      jointFor = key;
      const query = `/api/model/joint?name=${encodeURIComponent(bodyName)}&node=${encodeURIComponent(jointName())}`
        + (pack ? `&pack=${encodeURIComponent(pack)}&index=${index}` : '');
      const got = await api(query).catch(error => ({ error: error.message }));
      if (key !== jointFor) return;   // another change while this was out
      if (got.error) { say('hand: ' + got.error, 'bad'); joint = null; }
      else joint = got;
    }
    place(frame);
  };

  const companionTarget = { setPose: p => viewer.setCompanionPose(p), setFrame: f => viewer.setCompanionFrame(f) };

  const turnOn = async () => {
    bodyName = bodySelect.value;
    if (!bodyName) { say('no character models in this workspace', 'bad'); on.checked = false; return; }
    try { localStorage.setItem('ff3-editor-hand-body', bodyName); } catch (error) { /* not worth a word */ }
    say('loading the character\u2026');
    const body = await api(`/api/model?name=${encodeURIComponent(bodyName)}`);
    if (body.error || body.problem) { say(body.error || body.problem, 'bad'); on.checked = false; return; }
    await viewer.setCompanion(body, bodyName);
    jointFor = '';
    if (transport) await transport.retarget(bodyName, companionTarget, trackJoint);
    else await trackJoint(0, null, null, 0);
    say('');
  };

  const turnOff = async () => {
    bodyName = null;
    joint = null;
    jointFor = '';
    viewer.setAttach(null);
    await viewer.setCompanion(null);
    if (transport) await transport.retarget(name, viewer, null);
  };

  on.onchange = async () => {
    controls.hidden = !on.checked;
    if (on.checked) await turnOn(); else await turnOff();
  };
  bodySelect.onchange = async () => { if (on.checked) await turnOn(); };
  which.onchange = async () => {
    jointFor = '';
    if (!on.checked) return;
    const now = transport ? transport.current() : { pack: null, index: 0, frame: 0 };
    await trackJoint(now.frame, null, now.pack, now.index);
  };

  // The fit: every change places the model again at once and saves after a moment.
  let saveTimer = null;
  const saveFit = () => {
    if (!item) return;
    clearTimeout(saveTimer);
    saveTimer = setTimeout(async () => {
      try {
        const body = {
          id: item.id, number: item.number, base: item.base, name: item.name || '', caption: item.caption || '',
          buy: item.buy, sell: item.sell,
          model: item.model, modelScale: fitValues.scale, modelClip: item.modelClip || null,
          modelRotation: fitValues.rotation.slice(0, 3), modelOffset: fitValues.offset.slice(0, 3),
          fields: Object.fromEntries((item.fields || []).filter(f => f.value !== null && f.value !== undefined).map(f => [f.name, f.value]))
        };
        const r = await api('/api/project/items/save', body);
        if (!r.ok) throw new Error(r.error);
        item = r.item || item;
        say(`${item.name || item.id}: fit saved`, 'good');
      } catch (error) { say('fit: ' + error.message, 'bad'); }
    }, 500);
  };
  const refit = () => {
    fitValues.scale = Math.max(0.01, Number(scale.value) || 1);
    fitValues.rotation = rot.map(i => Number(i.value) || 0);
    fitValues.offset = off.map(i => Number(i.value) || 0);
    const now = transport ? transport.current() : { frame: 0 };
    place(now.frame);
    saveFit();
  };
  scale.oninput = refit;
  rot.forEach(i => i.oninput = refit);
  off.forEach(i => i.oninput = refit);
  reset.onclick = () => { scale.value = 1; rot.forEach(i => i.value = 0); off.forEach(i => i.value = 0); refit(); };

  // Opened from the item's Look card with "On a character…": start with the character on.
  if (window.handPreviewWanted) {
    which.value = window.handPreviewWanted === 'leftShield' || window.handPreviewWanted === 'rightShield' ? window.handPreviewWanted : which.value;
    window.handPreviewWanted = null;
    on.checked = true;
    controls.hidden = false;
    turnOn().catch(error => say(error.message, 'bad'));
  }

  // Arrow keys nudge the field under the cursor by its step; with Shift, ten steps.
  for (const input of [scale, ...rot, ...off]) {
    input.onkeydown = (e) => {
      if (e.key !== 'ArrowUp' && e.key !== 'ArrowDown') return;
      e.preventDefault();
      const step = (Number(input.step) || 1) * (e.shiftKey ? 10 : 1) * (e.key === 'ArrowUp' ? 1 : -1);
      input.value = String(Math.round((Number(input.value) + step) * 1000) / 1000);
      refit();
    };
  }
}

// ------------------------------------------------------------------ matrices (column-major 4x4, as the viewer's)

function translate(x, y, z) { return new Float32Array([1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, z, 1]); }
function scaleMatrix(k) { return new Float32Array([k, 0, 0, 0, 0, k, 0, 0, 0, 0, k, 0, 0, 0, 0, 1]); }
function rotateX(degrees) { const a = degrees * Math.PI / 180, c = Math.cos(a), s = Math.sin(a); return new Float32Array([1, 0, 0, 0, 0, c, s, 0, 0, -s, c, 0, 0, 0, 0, 1]); }
function rotateY(degrees) { const a = degrees * Math.PI / 180, c = Math.cos(a), s = Math.sin(a); return new Float32Array([c, 0, -s, 0, 0, 1, 0, 0, s, 0, c, 0, 0, 0, 0, 1]); }
function rotateZ(degrees) { const a = degrees * Math.PI / 180, c = Math.cos(a), s = Math.sin(a); return new Float32Array([c, s, 0, 0, -s, c, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]); }
