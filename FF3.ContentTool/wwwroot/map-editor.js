// The map view: what stands on a map, where, and what it says.
//
// Everything here is joined up from four files - see Editor/MapModel.cs. The canvas
// is a plan view: x across, z down, because that is what the game's coordinates mean
// and a top-down plan is the only honest thing to draw until the models decode.
//
// Dragging edits the .hich position and nothing else. The script that boots a
// character can override that position, and where it does, moving the pin will not
// move the character - the inspector says so rather than letting you find out in
// game.

'use strict';

const mapState = { name: null, data: null, zoom: 2, selected: null, showLogic: false };

async function openMap(name) {
  const data = await api(`/api/map?name=${encodeURIComponent(name)}`);
  const scene = await api(`/api/map/scene?name=${encodeURIComponent(name)}`);
  const node = view('map', name, data.overridden);
  const doc = activeDoc;

  mapState.name = name;
  mapState.data = data;
  mapState.selected = null;

  // Everything the hierarchy and the inspector need, handed over once rather than
  // fetched again by each of them.
  setDocData({ ...data, scene }, {
    mode: '3d',
    inspect: (ref) => inspectRef(doc, ref),
    onShow: () => { if (doc.scene3d && doc.mode === '3d') doc.scene3d.redraw(); }
  });

  drawLegend(node);
  wireModes(node, doc, scene);

  const zoom = $('.zoom', node);
  zoom.value = String(mapState.zoom);
  zoom.onchange = () => { mapState.zoom = Number(zoom.value); drawMap(node); };

  const logic = $('.showlogic', node);
  logic.checked = mapState.showLogic;
  logic.onchange = () => { mapState.showLogic = logic.checked; drawMap(node); };

  $('.save', node).onclick = async () => {
    const characters = data.characters.map(c => ({
      index: c.index, x: c.x, y: c.y, z: c.z, rotationY: c.rotationY
    }));
    const result = await api('/api/map/save', { name, characters });
    markOverridden(`files/${name}.hich`, true);
    $('.badge.override', node).classList.add('on');
    say(`placement saved, ${result.changed} entries`, 'good');
  };

  $('.revert', node).onclick = async () => {
    if (!confirm(`Throw away placement changes to ${name}?`)) return;
    await api('/api/revert', { name: `files/${name}.hich` });
    await open(name);
    say('reverted', 'good');
  };

  $('.add', node).onclick = () => showAdd(node);

  drawMap(node);
}

/// The form for a new character. Everything it needs is already on the map: a model
/// the map loads, somewhere to stand, and a line to say.
function showAdd(node) {
  if (activeDoc) {
    activeDoc.selection = 'add';
    activeDoc.inspect = (ref) => ref === 'add' ? buildAdd(node) : inspectRef(activeDoc, ref);
    drawInspector();
  }
}

function buildAdd(node) {
  const panel = document.createElement('div');

  const title = document.createElement('h2');
  title.textContent = 'Add a character';
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = 'Writes the .hich row, boots it in the script, gives it a cast '
    + 'that talks, and adds the line to every language.';
  panel.append(title, sub);

  const models = [...new Set(mapState.data.characters
    .filter(c => c.kind === 0 && c.model)
    .map(c => c.model))].sort();

  const modelLabel = document.createElement('label');
  modelLabel.className = 'wide';
  modelLabel.textContent = 'model (only ones this map already loads)';
  const model = document.createElement('select');
  for (const name of models) {
    const option = document.createElement('option');
    option.value = option.textContent = name;
    model.append(option);
  }
  modelLabel.append(model);

  const xLabel = document.createElement('label');
  xLabel.textContent = 'x';
  const x = document.createElement('input');
  x.value = '0';
  xLabel.append(x);

  const zLabel = document.createElement('label');
  zLabel.textContent = 'z';
  const z = document.createElement('input');
  z.value = '0';
  zLabel.append(z);

  const textLabel = document.createElement('label');
  textLabel.className = 'wide';
  textLabel.textContent = 'what it says';
  const text = document.createElement('textarea');
  text.value = 'Hello.';
  textLabel.append(text);

  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Add it';
  go.onclick = async () => {
    go.disabled = true;
    try {
      const result = await api('/api/map/add', {
        name: mapState.name,
        model: model.value,
        x: parseInt(x.value, 10) || 0,
        z: parseInt(z.value, 10) || 0,
        text: text.value
      });
      if (!result.ok) {
        say(result.error, 'bad');
        go.disabled = false;
        return;
      }
      say(`added cast ${result.cast}, message ${result.messageId}`, 'good');
      await open(mapState.name);
      const added = mapState.data.characters.find(c => c.cast === result.cast);
      if (added && activeDoc) {
        mapState.selected = added;
        activeDoc.selection = `object:${added.index}`;
        drawMap($('.view', activeDoc.pane));
        drawHierarchy();
        drawInspector();
      }
    } catch (error) {
      say(error.message, 'bad');
      go.disabled = false;
    }
  };

  panel.append(modelLabel, xLabel, zLabel, textLabel, go);
  return panel;
}

/// Everything worth drawing, and the box it all fits in.
function mapBounds(data, showLogic) {
  const points = [];
  for (const c of data.characters) {
    if (c.kind !== 0 && !showLogic) continue;
    if (c.x === 0 && c.z === 0 && c.kind !== 0) continue;
    points.push([c.x, c.z]);
  }
  for (const e of data.exits) points.push([e.x, e.z]);
  if (!points.length) points.push([0, 0]);

  const xs = points.map(p => p[0]);
  const zs = points.map(p => p[1]);
  const pad = 20;
  return {
    minX: Math.min(...xs) - pad, maxX: Math.max(...xs) + pad,
    minZ: Math.min(...zs) - pad, maxZ: Math.max(...zs) + pad
  };
}

function drawMap(node) {
  const data = mapState.data;
  const canvas = $('.map', node);
  const scale = $('.map-scale', node);
  canvas.textContent = '';

  const box = mapBounds(data, mapState.showLogic);
  const width = box.maxX - box.minX;
  const height = box.maxZ - box.minZ;

  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;
  canvas.style.transform = `scale(${mapState.zoom})`;
  scale.style.width = `${width * mapState.zoom}px`;
  scale.style.height = `${height * mapState.zoom}px`;

  const place = (element, x, z) => {
    element.style.left = `${x - box.minX}px`;
    element.style.top = `${z - box.minZ}px`;
  };

  for (const [index, exit] of data.exits.entries()) {
    const pin = document.createElement('div');
    pin.className = 'pin exit';
    pin.title = `exit to ${exit.to || '(nowhere)'}`;
    place(pin, exit.x, exit.z);
    pin.onclick = () => {
      if (!activeDoc) return;
      activeDoc.selection = `exit:${index}`;
      drawHierarchy();
      drawInspector();
    };

    const tag = document.createElement('span');
    tag.className = 'tag';
    tag.textContent = exit.to || '';
    pin.append(tag);
    canvas.append(pin);
  }

  for (const character of data.characters) {
    const logicOnly = character.kind !== 0;
    if (logicOnly && !mapState.showLogic) continue;

    const pin = document.createElement('div');
    pin.className = 'pin'
      + (logicOnly ? ' logic' : '')
      + (character.lines.length ? ' talks' : '');
    pin.title = `${character.model} · cast ${character.cast}`;
    place(pin, character.x, character.z);

    const tag = document.createElement('span');
    tag.className = 'tag';
    tag.textContent = character.model;
    pin.append(tag);

    pin.dataset.index = character.index;
    pin.onpointerdown = event => dragPin(event, node, character, pin, box);
    canvas.append(pin);

    if (mapState.selected === character) pin.classList.add('on');
  }
}

/// A character's placement, for the undo stack to hold on to.
function positionOf(character) {
  return {
    x: character.x, y: character.y, z: character.z, rotationY: character.rotationY
  };
}

function samePosition(a, b) {
  return a.x === b.x && a.y === b.y && a.z === b.z && a.rotationY === b.rotationY;
}

/// Puts a character where it was, in both views and in the inspector.
function applyPosition(doc, index, to) {
  const character = mapState.data.characters.find(c => c.index === index);
  if (character) Object.assign(character, to);

  const item = ((doc.data && doc.data.scene && doc.data.scene.objects) || [])
    .find(o => o.index === index);
  if (item) Object.assign(item, to);

  const view = $('.view', doc.pane);
  if (view && doc.mode === '2d') drawMap(view);
  if (doc.scene3d) doc.scene3d.redraw();
  drawInspector();
}

/// Records one move, unless nothing actually moved.
function recordMove(doc, character, before) {
  if (!doc || !character) return;
  const after = positionOf(character);
  if (samePosition(before, after)) return;
  const index = character.index;
  pushUndo(doc, `move ${character.model || 'character'}`,
    () => applyPosition(doc, index, before),
    () => applyPosition(doc, index, after));
}

/// The 2D and 3D buttons, and the scene behind the second of them.
function wireModes(node, doc, scene) {
  const flat = $('.map-wrap', node);
  const solid = $('.scene-wrap', node);
  const canvas = $('.scene', node);
  const recentre = $('.recentre', node);
  const gizmoBox = $('.gizmo-toggle', node);
  const tools = $('.tools', node);

  const show = async (mode) => {
    doc.mode = mode;
    $$('.mode', node).forEach(b => b.classList.toggle('on', b.dataset.mode === mode));
    flat.hidden = mode !== '2d';
    solid.hidden = mode !== '3d';
    recentre.hidden = mode !== '3d';
    gizmoBox.hidden = mode !== '3d';
    tools.hidden = mode !== '3d';
    $('.zoom', node).hidden = mode !== '2d';

    if (mode !== '3d') return;
    if (!doc.scene3d) {
      doc.scene3d = makeMapScene(canvas, say);
      if (!doc.scene3d) return;
      wireSceneInput(canvas, doc);
      say('loading the scene…');

      // Dragging an arrow moves the scene object; the character behind it is what
      // gets saved, so the two are kept in step here rather than at save time.
      doc.scene3d.onMove((item) => {
        const character = mapState.data.characters.find(c => c.index === item.index);
        if (!character) return;
        character.x = item.x;
        character.y = item.y;
        character.z = item.z;
        // The facing too. Leaving it out meant a turn never reached the character
        // behind the scene object, so there was nothing for undo to undo and nothing
        // for Save placement to write.
        character.rotationY = item.rotationY;
        drawInspector();
      });

      await doc.scene3d.load(scene, (item) => {
        mapState.selected = mapState.data.characters.find(c => c.index === item.index);
        doc.selection = `object:${item.index}`;
        drawHierarchy();
        drawInspector();
      });
      say('');
    }
    doc.scene3d.redraw();
    if (doc.selection && doc.selection.startsWith('object:')) {
      doc.scene3d.select(Number(doc.selection.slice(7)));
    }
  };

  $$('.mode', node).forEach(button => {
    button.onclick = () => show(button.dataset.mode).catch(e => say(e.message, 'bad'));
  });
  recentre.onclick = () => doc.scene3d && doc.scene3d.reset();
  $('.gizmo', node).onchange = (event) => {
    if (doc.scene3d) doc.scene3d.setGizmo(event.target.checked);
  };

  const useTool = (which) => {
    $$('.tool', node).forEach(b => b.classList.toggle('on', b.dataset.tool === which));
    if (doc.scene3d) doc.scene3d.setGizmoMode(which);
  };
  $$('.tool', node).forEach(button => {
    button.onclick = () => useTool(button.dataset.tool);
  });
  doc.useTool = useTool;
  show('3d');
}

/// Left drag orbits, middle or shift drags to pan, wheel zooms, a click picks.
///
/// Holding the right button flies, the way a scene view usually does: the mouse looks
/// around from where the camera already is rather than swinging it round a target, and
/// WASD walks it, with Q and E for down and up and shift to hurry. The keys are only
/// listened for while the button is held, so W and E stay free to swap the gizmo the
/// rest of the time.
function wireSceneInput(canvas, doc) {
  let dragging = false;
  let panning = false;
  let flying = false;
  let moved = 0;
  let lastX = 0;
  let lastY = 0;

  const held = new Set();
  let flight = 0;

  const flyStep = () => {
    if (!flying) return;
    const ahead = (held.has('w') ? 1 : 0) - (held.has('s') ? 1 : 0);
    const sideways = (held.has('d') ? 1 : 0) - (held.has('a') ? 1 : 0);
    const upward = (held.has('e') ? 1 : 0) - (held.has('q') ? 1 : 0);
    doc.scene3d.fly(ahead, sideways, upward, held.has('shift'));
    flight = requestAnimationFrame(flyStep);
  };

  const onKey = (event) => {
    if (!flying) return;
    const key = event.key.toLowerCase();
    if ('wasdqe'.includes(key) || key === 'shift') {
      event.preventDefault();
      if (event.type === 'keydown') held.add(key);
      else held.delete(key);
    }
  };

  const stopFlying = () => {
    if (!flying) return;
    flying = false;
    held.clear();
    cancelAnimationFrame(flight);
    window.removeEventListener('keydown', onKey);
    window.removeEventListener('keyup', onKey);
    canvas.style.cursor = '';
  };

  canvas.oncontextmenu = (event) => event.preventDefault();

  canvas.onpointerdown = (event) => {
    moved = 0;
    lastX = event.clientX;
    lastY = event.clientY;
    canvas.setPointerCapture(event.pointerId);

    if (event.button === 2) {
      flying = true;
      canvas.style.cursor = 'none';
      window.addEventListener('keydown', onKey);
      window.addEventListener('keyup', onKey);
      flight = requestAnimationFrame(flyStep);
      return;
    }

    // A gizmo under the cursor takes the press; the camera only gets what is left.
    if (event.button === 0 && doc.scene3d.beginDrag(event.clientX, event.clientY)) {
      pinDoc(doc);
      doc.movingFrom = mapState.selected ? positionOf(mapState.selected) : null;
      return;
    }
    dragging = true;
    panning = event.button === 1 || event.shiftKey;
  };

  canvas.onpointermove = (event) => {
    const dx = event.clientX - lastX;
    const dy = event.clientY - lastY;
    lastX = event.clientX;
    lastY = event.clientY;

    if (flying) {
      doc.scene3d.look(dx, dy);
      return;
    }
    if (doc.scene3d.dragging()) {
      doc.scene3d.dragTo(event.clientX, event.clientY);
      return;
    }
    if (!dragging) {
      canvas.style.cursor = doc.scene3d.hovering(event.clientX, event.clientY)
        ? (doc.scene3d.gizmoMode() === 'rotate' ? 'grab' : 'move') : '';
      return;
    }
    moved += Math.abs(dx) + Math.abs(dy);
    if (panning) doc.scene3d.pan(dx, dy);
    else doc.scene3d.orbit(dx, dy);
  };

  canvas.onpointerup = (event) => {
    canvas.releasePointerCapture(event.pointerId);
    if (event.button === 2) {
      stopFlying();
      return;
    }
    if (doc.scene3d.dragging()) {
      doc.scene3d.endDrag();
      if (doc.movingFrom) recordMove(doc, mapState.selected, doc.movingFrom);
      doc.movingFrom = null;
      say(doc.scene3d.gizmoMode() === 'rotate'
        ? 'turned - use Save placement to keep it'
        : 'moved - use Save placement to keep it', 'good');
      return;
    }
    dragging = false;
    // A click that did not really move is a pick, not the end of an orbit.
    if (moved < 4) doc.scene3d.pickAt(event.clientX, event.clientY);
  };

  // Letting go outside the canvas, or leaving the page mid-flight, still lands.
  canvas.onpointercancel = stopFlying;
  window.addEventListener('blur', stopFlying);

  canvas.onwheel = (event) => {
    event.preventDefault();
    doc.scene3d.zoom(Math.sign(event.deltaY));
  };

  // W and E swap the gizmo. This listens on the window rather than the pane, because
  // a pane only hears keys while something inside it holds focus - and the usual way
  // to reach for W is straight after moving the mouse, having clicked nothing.
  // It only acts when this document is the focused one and is showing its scene.
  window.addEventListener('keydown', (event) => {
    if (flying || event.ctrlKey || event.metaKey || event.altKey) return;
    if (activeDoc !== doc || doc.mode !== '3d') return;
    const target = event.target;
    if (target && target.matches
      && target.matches('input, textarea, [contenteditable]')) return;
    const key = event.key.toLowerCase();
    if (key !== 'w' && key !== 'e') return;
    event.preventDefault();
    doc.useTool(key === 'w' ? 'move' : 'rotate');
  });
}

/// What the pin colours mean. It used to live in the map's own inspector.
function drawLegend(node) {
  const legend = $('.legend', node);
  if (!legend) return;
  legend.textContent = '';
  for (const [colour, label] of [['#3d6b4d', 'talks'], ['#2b3a4f', 'silent'],
                                 ['#6b4a2b', 'exit']]) {
    const item = document.createElement('span');
    const dot = document.createElement('i');
    dot.style.background = colour;
    item.append(dot, document.createTextNode(label));
    legend.append(item);
  }
}

/// Turns a hierarchy or scene selection into something the inspector can show.
function inspectRef(doc, ref) {
  const scene = doc.data && doc.data.scene;
  if (!scene) return null;

  if (ref === 'terrain') {
    const box = document.createElement('div');
    const heading = document.createElement('h3');
    heading.textContent = 'Terrain';
    box.append(heading);
    const link = document.createElement('a');
    link.href = '#';
    link.textContent = `open ${shortName(scene.terrain)}`;
    link.style.color = 'var(--accent)';
    link.onclick = (event) => {
      event.preventDefault();
      openDoc('model', scene.terrain);
    };
    box.append(link);
    return box;
  }

  if (ref.startsWith('object:')) {
    const index = Number(ref.slice(7));
    const character = doc.data.characters.find(c => c.index === index);
    return character ? buildCharacter(character) : null;
  }

  if (ref.startsWith('exit:')) {
    return buildExit(scene.exits[Number(ref.slice(5))]);
  }
  return null;
}

function dragPin(event, node, character, pin, box) {
  event.preventDefault();
  pin.setPointerCapture(event.pointerId);
  selectCharacter(node, character, pin);

  const startX = event.clientX;
  const startY = event.clientY;
  const originX = character.x;
  const originZ = character.z;
  const before = positionOf(character);

  const move = moveEvent => {
    character.x = originX + Math.round((moveEvent.clientX - startX) / mapState.zoom);
    character.z = originZ + Math.round((moveEvent.clientY - startY) / mapState.zoom);
    pin.style.left = `${character.x - box.minX}px`;
    pin.style.top = `${character.z - box.minZ}px`;
    // The inspector shows the numbers being dragged, so it has to keep up.
    drawInspector();
  };

  const up = () => {
    recordMove(activeDoc, character, before);
    pin.removeEventListener('pointermove', move);
    pin.removeEventListener('pointerup', up);
  };

  pin.addEventListener('pointermove', move);
  pin.addEventListener('pointerup', up);
}

function selectCharacter(node, character, pin) {
  $$('.pin', node).forEach(p => p.classList.remove('on'));
  pin.classList.add('on');
  mapState.selected = character;
  if (activeDoc) {
    activeDoc.selection = `object:${character.index}`;
    if (activeDoc.scene3d) activeDoc.scene3d.select(character.index);
    drawHierarchy();
    drawInspector();
  }
}

function buildExit(exit) {
  const panel = document.createElement('div');
  if (!exit) return panel;

  const title = document.createElement('h2');
  title.textContent = `Exit to ${exit.to || '(nowhere)'}`;
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `arrives at index ${exit.toIndex} · needs flag ${exit.conditionFlag}`
    + ` · kind ${exit.kind}`;
  panel.append(title, sub);

  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'Exits live in the map’s .pak, under jumps. '
    + 'Edit them in the Tables view; they are not draggable here yet.';
  panel.append(note);
  return panel;
}

function buildCharacter(character) {
  const panel = document.createElement('div');
  const node = state.pane;

  const title = document.createElement('h2');
  title.textContent = character.model || '(no model)';
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `${character.kindName} · cast ${character.cast}`
    + (character.hasScript ? ` · ${character.instructions} instructions` : ' · no script');
  panel.append(title, sub);

  const field = (label, key) => {
    const wrap = document.createElement('label');
    wrap.textContent = label;
    const input = document.createElement('input');
    input.value = character[key];
    let before = null;
    input.onfocus = () => { before = positionOf(character); };
    input.onblur = () => {
      if (before) recordMove(activeDoc, character, before);
      before = null;
    };
    input.oninput = () => {
      const value = parseInt(input.value, 10);
      if (!Number.isNaN(value)) {
        character[key] = value;
        if (node) drawMap(node);
        if (activeDoc && activeDoc.scene3d) {
          const item = (activeDoc.data.scene.objects || [])
            .find(o => o.index === character.index);
          if (item) {
            item.x = character.x;
            item.y = character.y;
            item.z = character.z;
            item.rotationY = character.rotationY;
            activeDoc.scene3d.redraw();
          }
        }
      }
    };
    wrap.append(input);
    panel.append(wrap);
  };

  const position = document.createElement('h3');
  position.textContent = 'Position';
  panel.append(position);
  field('x', 'x');
  field('z', 'z');
  field('y (height)', 'y');
  field('facing', 'rotationY');

  const heading = document.createElement('h3');
  heading.textContent = 'What it says';
  panel.append(heading);

  if (!character.lines.length) {
    const none = document.createElement('p');
    none.className = 'none';
    none.textContent = character.hasScript
      ? 'Its cast runs code but shows no dialogue - it may move, open a shop, or '
        + 'trigger a scene.'
      : 'The map’s script has no cast with this number.';
    panel.append(none);
  } else {
    const list = document.createElement('ul');
    list.className = 'lines';
    for (const line of character.lines) {
      const item = document.createElement('li');
      item.textContent = line;
      list.append(item);
    }
    panel.append(list);
  }

  const behaviour = document.createElement('h3');
  behaviour.textContent = 'Behaviour';
  panel.append(behaviour);

  const link = document.createElement('a');
  link.href = '#';
  link.textContent = `open ${mapState.name}.script at cast ${character.cast}`;
  link.style.color = 'var(--accent)';
  link.onclick = async event => {
    event.preventDefault();
    const doc = await openDoc('script', `files/${mapState.name}.script`);
    const text = $('textarea', doc.pane);
    if (!text) return;
    const at = text.value.indexOf(`cast${character.cast}_main:`);
    if (at >= 0) goToLine(text, text.value.slice(0, at).split('\n').length, 1);
  };
  panel.append(link);
  return panel;
}
