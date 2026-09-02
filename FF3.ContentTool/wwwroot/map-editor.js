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
  // Every model that can go in a .hich row, so the panel can offer them all rather
  // than only the ones this map happens to use already.
  if (!state.placeable) state.placeable = await api('/api/models/placeable');
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
      index: c.index, x: c.x, y: c.y, z: c.z, rotationY: c.rotationY,
      model: c.model, cast: c.cast
    }));
    const result = await api('/api/map/save', { name, characters });
    markOverridden(`files/${name}.hich`, true);
    $('.badge.override', node).classList.add('on');
    say(`placement saved, ${result.changed} entries`, 'good');
  };

  $('.revert', node).onclick = async () => {
    if (!confirm(`Revert the whole of ${name} to the shipped files?

`
      + 'Every placement change on this map goes, not just the last one, and undo '
      + 'cannot bring them back.')) return;
    await api('/api/revert', { name: `files/${name}.hich` });
    await open(name);
    say('reverted', 'good');
  };

  $('.add', node).onclick = () => showAdd(node);
  wireDrop(node, doc);

  drawMap(node);
}

/// Dropping a model from the project onto the scene.
///
/// The drop works out where on the ground it landed and opens the add panel with the
/// model and the position already filled in, rather than writing something straight
/// away - what a dropped model should do is a question only the person dropping it can
/// answer, and a chest and a villager are the same drag.
function wireDrop(node, doc) {
  const scene = $('.scene', node);
  const wrap = $('.scene-wrap', node);
  if (!scene || !wrap) return;

  const held = (event) => event.dataTransfer
    && [...event.dataTransfer.types].includes('text/ff3-model');

  wrap.addEventListener('dragover', (event) => {
    if (!held(event)) return;
    event.preventDefault();
    event.dataTransfer.dropEffect = 'copy';
    wrap.classList.add('dropping');
  });

  wrap.addEventListener('dragleave', (event) => {
    if (event.target === wrap) wrap.classList.remove('dropping');
  });

  wrap.addEventListener('drop', (event) => {
    if (!held(event)) return;
    event.preventDefault();
    wrap.classList.remove('dropping');

    const model = event.dataTransfer.getData('text/ff3-model');
    if (!model) return;

    const at = doc.scene3d
      && doc.scene3d.groundAt(event.clientX, event.clientY);
    if (!at) {
      say('that is not somewhere on the ground', 'bad');
      return;
    }

    showAdd(node, { model, x: at[0], z: at[2] });
    say(`${model} at ${at[0]}, ${at[2]} - choose what it does and add it`);
  });
}

/// The form for something new on the map. What it needs depends on what it is: a
/// character needs a line to say, a chest needs something to hold, a prop needs
/// neither. The model, a place to stand and a free cast number are common to all.
function showAdd(node, start) {
  if (activeDoc) {
    activeDoc.selection = 'add';
    activeDoc.inspect = (ref) =>
      ref === 'add' ? buildAdd(node, start) : inspectRef(activeDoc, ref);
    drawInspector();
  }
}

// What can be added, and what each one writes. The wording matters more than it looks:
// "cast" and "hich row" mean nothing until you have read the formats, and the panel is
// where somebody finds out what they are.
const BEHAVIOURS = [
  {
    id: 'talk',
    label: 'Talks to the player',
    note: 'Turns to face the player and says a line. The line is written into every '
      + 'language this map has text for.',
    objectsOnly: false
  },
  {
    id: 'chest',
    label: 'Chest holding an item',
    note: 'Opens once and gives the item. It remembers being opened with a flag no '
      + 'other chest in the game uses - the editor finds a free one.',
    objectsOnly: true
  },
  {
    id: 'money',
    label: 'Chest holding gil',
    note: 'The same as a chest, with gil in it instead of an item.',
    objectsOnly: true
  },
  {
    id: 'prop',
    label: 'Stands there',
    note: 'Placed and booted, with an empty cast. Somewhere to hang behaviour on later.',
    objectsOnly: false
  }
];

/// Whether a model is set up as a map object rather than a person. The game switches
/// on the first letter of the name and only o and w go through setUpMapObject, which is
/// what a chest has to be - see Editor/AddEntity.cs.
function isObjectModel(name) {
  const first = (name || '')[0];
  return first === 'o' || first === 'w' || first === 'O' || first === 'W';
}

function buildAdd(node, start) {
  const panel = document.createElement('div');

  const title = document.createElement('h2');
  title.textContent = 'Add a game object';
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = 'Writes the row that places it, the call that boots it, and the '
    + 'cast that gives it behaviour.';
  panel.append(title, sub);

  // ------------------------------------------------------------- what it does

  const kindLabel = document.createElement('label');
  kindLabel.className = 'wide';
  kindLabel.textContent = 'behaviour';
  const kind = document.createElement('select');
  for (const entry of BEHAVIOURS) {
    const option = document.createElement('option');
    option.value = entry.id;
    option.textContent = entry.label;
    kind.append(option);
  }
  kindLabel.append(kind);

  const kindNote = document.createElement('p');
  kindNote.className = 'none';

  // ------------------------------------------------------------------- model

  const modelHead = document.createElement('h3');
  modelHead.textContent = 'Model';

  // Whatever this map already uses is the likeliest answer, so it starts there rather
  // than on the first name in an alphabetical list of 145.
  const here = [...new Set(mapState.data.characters
    .filter(c => c.kind === 0 && c.model)
    .map(c => c.model))].sort();
  let model = (start && start.model) || here[0] || 'n011';
  // Once a model has been picked on purpose, changing the behaviour leaves it alone -
  // unless it could not work at all. A dropped model counts as picked.
  let picked = Boolean(start && start.model);

  const choose = document.createElement('button');
  choose.className = 'model-choice';
  const chosenName = document.createElement('span');
  choose.append(icon('model'), chosenName);

  const modelNote = document.createElement('p');
  modelNote.className = 'none';

  // --------------------------------------------------------------- where

  const xLabel = document.createElement('label');
  xLabel.textContent = 'x';
  const x = document.createElement('input');
  x.value = String((start && start.x) ?? 0);
  xLabel.append(x);

  const zLabel = document.createElement('label');
  zLabel.textContent = 'z';
  const z = document.createElement('input');
  z.value = String((start && start.z) ?? 0);
  zLabel.append(z);

  // ------------------------------------------------------ what it says or holds

  const textLabel = document.createElement('label');
  textLabel.className = 'wide';
  textLabel.textContent = 'what it says';
  const text = document.createElement('textarea');
  text.value = 'Hello.';
  textLabel.append(text);

  const itemLabel = document.createElement('label');
  itemLabel.className = 'wide';
  itemLabel.textContent = 'what is in it';
  const item = document.createElement('select');
  itemLabel.append(item);

  const goldLabel = document.createElement('label');
  goldLabel.className = 'wide';
  goldLabel.textContent = 'how much gil';
  const gold = document.createElement('input');
  gold.value = '100';
  goldLabel.append(gold);

  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Add it';

  panel.append(kindLabel, kindNote, modelHead, choose, modelNote,
    xLabel, zLabel, textLabel, itemLabel, goldLabel, go);

  // The item list is worth fetching once and keeping - it is the whole item table, and
  // every chest anyone adds wants the same list.
  const fillItems = async () => {
    if (!state.items) {
      try {
        state.items = await api('/api/items');
      } catch (error) {
        state.items = [];
        say(error.message, 'bad');
      }
    }
    if (item.childElementCount) return;
    for (const entry of state.items) {
      const option = document.createElement('option');
      option.value = entry.id;
      option.textContent = `${entry.name} · ${entry.category} · ${entry.id}`;
      item.append(option);
    }
  };

  const showFor = (id) => {
    const behaviour = BEHAVIOURS.find(b => b.id === id) || BEHAVIOURS[0];
    kindNote.textContent = behaviour.note;
    textLabel.hidden = id !== 'talk';
    itemLabel.hidden = id !== 'chest';
    goldLabel.hidden = id !== 'money';
    if (id === 'chest') fillItems();

    // A chest has to be an object, and o001 is the chest: 377 of the game's 421
    // treasure rows use it, and the other 44 use o000. So an unpicked model becomes the
    // chest, and a picked one is only overridden when it is a person, which cannot work
    // at all.
    if (behaviour.objectsOnly && (!picked || !isObjectModel(model))) {
      const placeable = (state.placeable || []).map(p => p.model);
      const chest = placeable.includes('o001') ? 'o001'
        : (here.find(isObjectModel) || placeable.find(isObjectModel));
      if (chest) model = chest;
    }
    chosenName.textContent = model;
    modelNote.textContent = behaviour.objectsOnly
      ? 'A chest has to be an object model - one whose name starts with o or w. The '
        + 'game sets those up as map objects, and the treasure command looks its cast '
        + 'up in that list.'
      : (here.includes(model)
        ? 'This map already loads it.'
        : 'This map has no ' + model + ' yet - its id comes from the rest of the game.');
  };

  kind.onchange = () => showFor(kind.value);

  choose.onclick = () => {
    const behaviour = BEHAVIOURS.find(b => b.id === kind.value);
    pickModel(model, (chosen) => {
      model = chosen;
      picked = true;
      showFor(kind.value);
    }, behaviour.objectsOnly
      ? { only: isObjectModel, title: 'Choose an object', what: 'object models' }
      : {});
  };

  go.onclick = async () => {
    go.disabled = true;
    try {
      const result = await api('/api/map/add', {
        name: mapState.name,
        model,
        behaviour: kind.value,
        x: parseInt(x.value, 10) || 0,
        z: parseInt(z.value, 10) || 0,
        text: text.value,
        item: parseInt(item.value, 10) || 0,
        gold: parseInt(gold.value, 10) || 0
      });
      if (!result.ok) {
        say(result.error, 'bad');
        go.disabled = false;
        return;
      }

      let what = `added cast ${result.cast}`;
      if (result.messageId) what += `, message ${result.messageId}`;
      if (result.flagIndex) what += `, flag ${result.flagGroup},${result.flagIndex}`;
      say(what, 'good');
      for (const note of result.notes || []) say(note);

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

  showFor(kind.value);
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

/// Takes the camera to whatever is selected. Nothing selected is not an error worth
/// a red line, so it says so on the status line and stays where it is.
function focusSelection(doc) {
  const scene = doc.data && doc.data.scene;
  if (!doc.scene3d || !scene || !doc.selection) {
    say('nothing selected to focus on');
    return;
  }

  if (doc.selection.startsWith('object:')) {
    const index = Number(doc.selection.slice(7));
    const item = scene.objects.find(o => o.index === index);
    if (item) {
      doc.scene3d.focus(item);
      say(`focused ${item.name}`);
    }
    return;
  }

  if (doc.selection.startsWith('exit:')) {
    const index = Number(doc.selection.slice(5));
    doc.scene3d.focusExit(index);
    const exit = scene.exits[index];
    say(`focused the exit to ${(exit && exit.to) || '(nowhere)'}`);
    return;
  }

  if (doc.selection === 'terrain') {
    doc.scene3d.reset();
    say('framed the whole map');
  }
}

/// How big the arrows are, remembered between visits.
function gizmoScale() {
  try {
    return Number(localStorage.getItem('ff3-editor-gizmo')) || 1;
  } catch (error) {
    return 1;
  }
}

/// The floating labels over the scene. One per exit: the map it leads to, sitting
/// where the exit is. They are elements rather than geometry, so they stay legible
/// however far out the camera is and take their own clicks.
function drawSceneTags(node, doc) {
  const layer = $('.scene-tags', node);
  if (!layer || !doc.scene3d) return;

  const scene = doc.data && doc.data.scene;
  const exits = (scene && scene.exits) || [];
  const on = $('.gizmo', node).checked;

  // Built once, then only moved - rebuilding every frame would throw away the hover.
  if (layer.childElementCount !== exits.length) {
    layer.textContent = '';
    exits.forEach((exit, index) => {
      const tag = document.createElement('button');
      tag.className = 'scene-tag';
      tag.type = 'button';
      tag.innerHTML = '<span class="dot"></span>';
      const label = document.createElement('span');
      label.textContent = exit.to || '(nowhere)';
      tag.append(label);
      tag.title = `exit to ${exit.to || '(nowhere)'} · arrives facing ${exit.rotationY ?? 0}°`;
      tag.onclick = () => {
        mapState.selected = null;
        doc.selection = `exit:${index}`;
        drawHierarchy();
        drawInspector();
        drawSceneTags(node, doc);
      };
      layer.append(tag);
    });
  }

  layer.hidden = !on;
  if (!on) return;

  exits.forEach((exit, index) => {
    const tag = layer.children[index];
    const at = doc.scene3d.screenAt(exit.x, exit.y, exit.z);
    if (!at) {
      tag.style.display = 'none';
      return;
    }
    tag.style.display = '';
    tag.style.left = `${at[0]}px`;
    tag.style.top = `${at[1]}px`;
    tag.classList.toggle('on', doc.selection === `exit:${index}`);
  });
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

/// Swaps the model a row wears, in the record, the scene and the panels.
function applyModel(doc, index, model) {
  const character = mapState.data.characters.find(c => c.index === index);
  if (character) character.model = model;

  const item = ((doc.data && doc.data.scene && doc.data.scene.objects) || [])
    .find(o => o.index === index);
  if (item) {
    item.model = model;
    item.package = `files/${model}.nmdp.lz`;
    item.name = `${model} (cast ${item.cast})`;
  }

  const view = $('.view', doc.pane);
  if (view && doc.mode === '2d') drawMap(view);
  if (doc.scene3d) doc.scene3d.reload();
  drawHierarchy();
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
  const sizeBox = $('.gizmo-size', node);

  const show = async (mode) => {
    doc.mode = mode;
    $$('.mode', node).forEach(b => b.classList.toggle('on', b.dataset.mode === mode));
    flat.hidden = mode !== '2d';
    solid.hidden = mode !== '3d';
    recentre.hidden = mode !== '3d';
    gizmoBox.hidden = mode !== '3d';
    tools.hidden = mode !== '3d';
    sizeBox.hidden = mode !== '3d';
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

      // Exits are tags in the page rather than geometry in the scene: a label stays
      // the same size however far out you are, and reading "to t01_07" beats working
      // it out from a pin.
      doc.scene3d.onFrame(() => drawSceneTags(node, doc));

      await doc.scene3d.load(scene, (item, kind) => {
        if (kind === 'exit') {
          mapState.selected = null;
          doc.selection = `exit:${item.index}`;
        } else {
          mapState.selected = mapState.data.characters.find(c => c.index === item.index);
          doc.selection = `object:${item.index}`;
        }
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
    drawSceneTags(node, doc);
  };

  const size = $('.gsize', node);
  size.value = gizmoScale();
  size.oninput = () => {
    const scale = Number(size.value);
    try {
      localStorage.setItem('ff3-editor-gizmo', String(scale));
    } catch (error) {
      // Not remembering the size is not worth interrupting anyone over.
    }
    if (doc.scene3d) doc.scene3d.setGizmoScale(scale);
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

    // F frames whatever is selected, wherever the selection was made - the hierarchy
    // and the scene are one selection, so it does not matter which you clicked in.
    if (key === 'f') {
      event.preventDefault();
      focusSelection(doc);
      return;
    }

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
    const index = Number(ref.slice(5));
    return buildExit(scene.exits[index], index);
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

// An exit, and what can be done to one.
//
// Only half of an exit is data. The row here says where the player lands, on which map,
// facing which way; what makes it fire is a region of the map's collision mesh carrying
// one of twelve jump attributes, and that lives in <map>_col.mcl.lz, which nothing here
// reads yet. So an exit can be pointed somewhere else but not created - a new row with
// no region to trigger it would sit in the file and never fire.
function buildExit(exit, index) {
  const panel = document.createElement('div');
  if (!exit) return panel;

  const doc = activeDoc;
  const title = document.createElement('h2');
  title.textContent = `Exit to ${exit.to || '(nowhere)'}`;
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `slot ${index + 1} of ${(doc.data.scene.exits || []).length}`;
  panel.append(title, sub);

  // The row is edited in place and written when Save is pressed, so the numbers can be
  // moved around without each keystroke touching the file.
  const draft = {
    index,
    x: exit.x, y: exit.y, z: exit.z,
    rotationY: exit.rotationY || 0,
    to: exit.to || '',
    toIndex: exit.toIndex || 0,
    conditionFlag: exit.conditionFlag || 0,
    kind: exit.kind || 0
  };

  const field = (label, key, hint) => {
    const wrap = document.createElement('label');
    wrap.textContent = label;
    const box = document.createElement('input');
    box.value = draft[key];
    box.oninput = () => {
      const value = key === 'to' ? box.value : parseInt(box.value, 10);
      draft[key] = key === 'to' ? value : (Number.isNaN(value) ? 0 : value);
    };
    if (hint) box.title = hint;
    wrap.append(box);
    return wrap;
  };

  // ------------------------------------------------------------ where it goes

  const toHead = document.createElement('h3');
  toHead.textContent = 'Leads to';
  panel.append(toHead);

  const toLabel = document.createElement('label');
  toLabel.className = 'wide';
  toLabel.textContent = 'map';
  const to = document.createElement('input');
  to.value = draft.to;
  to.setAttribute('list', 'map-names');
  to.oninput = () => { draft.to = to.value.trim(); };
  toLabel.append(to);
  panel.append(toLabel);

  // Every map, so the name can be chosen rather than remembered. A datalist rather than
  // a dropdown, because 356 names are worth typing into.
  let names = $('#map-names');
  if (!names) {
    names = document.createElement('datalist');
    names.id = 'map-names';
    document.body.append(names);
  }
  if (names.childElementCount !== (state.files || []).length && state.browse === 'map') {
    names.textContent = '';
    for (const file of state.files) {
      const option = document.createElement('option');
      option.value = file.name;
      names.append(option);
    }
  }

  panel.append(field('arrival index', 'toIndex',
    'Which exit on the far map the player comes out of.'));

  // --------------------------------------------------------- where it lands

  const whereHead = document.createElement('h3');
  whereHead.textContent = 'Arrives at';
  panel.append(whereHead);
  panel.append(field('x', 'x'), field('y', 'y'), field('z', 'z'),
    field('facing', 'rotationY',
      'Degrees. The file keeps it as a 16 bit angle where a whole turn is 65536.'));

  // ------------------------------------------------------------- conditions

  const whenHead = document.createElement('h3');
  whenHead.textContent = 'Conditions';
  panel.append(whenHead);
  panel.append(field('flag', 'conditionFlag',
    'What has to be true to go through. 1 is a plain exit - 646 of the game’s 676 '
    + 'are - 2 wants the whole party Mini, 4 wants the whole party Toad, and 0 never '
    + 'opens at all.'),
    field('kind', 'kind',
      'The door. -1 is no door and just walks through; 0 and up name a door model; '
      + '800 and up name a story flag in group 0 that opens it for good.'));

  const save = document.createElement('button');
  save.className = 'primary';
  save.textContent = 'Save exit';
  save.onclick = async () => {
    save.disabled = true;
    try {
      const result = await api('/api/map/exit/save', { name: mapState.name, ...draft });
      if (!result.ok) {
        say(result.error, 'bad');
        save.disabled = false;
        return;
      }
      for (const note of result.notes || []) say(note);
      markOverridden(`files/${mapState.name}.pak`, true);
      say(`exit ${index + 1} now leads to ${draft.to || '(nowhere)'}`, 'good');
      await open(mapState.name);
    } catch (error) {
      say(error.message, 'bad');
      save.disabled = false;
    }
  };
  panel.append(save);

  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'Only half of an exit is data. This row says where the player '
    + 'lands; what makes it fire is a region of the map’s collision mesh carrying '
    + 'one of twelve jump attributes, in ' + mapState.name + '_col.mcl.lz, which '
    + 'nothing here reads yet. So an exit can be pointed somewhere else, but a new one '
    + 'cannot be made - a row with no region to trigger it would never fire.';
  panel.append(note);
  return panel;
}

function buildCharacter(character) {
  const panel = document.createElement('div');
  const node = state.pane;
  const doc = activeDoc;

  const title = document.createElement('h2');
  title.textContent = character.model || '(no model)';
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `${character.kindName} · cast ${character.cast}`;
  panel.append(title, sub);

  // ------------------------------------------------------------------ model

  const modelHead = document.createElement('h3');
  modelHead.textContent = 'Model';
  panel.append(modelHead);

  // A button rather than a dropdown: 145 names tell you nothing about which one is a
  // chest and which is a shopkeeper, and the picker shows them.
  const choose = document.createElement('button');
  choose.className = 'model-choice';
  const chosenName = document.createElement('span');
  chosenName.textContent = character.model || '(none)';
  choose.append(icon('model'), chosenName);
  choose.onclick = () => pickModel(character.model, (model) => {
    if (model === character.model) return;
    const was = character.model;
    applyModel(doc, character.index, model);
    pushUndo(doc, `model of cast ${character.cast}`,
      () => applyModel(doc, character.index, was),
      () => applyModel(doc, character.index, model));
    say('model changed - use Save placement to keep it', 'good');
  });
  panel.append(choose);

  const modelNote = document.createElement('p');
  modelNote.className = 'none';
  modelNote.textContent = 'A row names its model twice: as text, and as the number the '
    + 'game actually loads. Changing it here sets both.';
  panel.append(modelNote);

  // ------------------------------------------------------------------- cast

  const castHead = document.createElement('h3');
  castHead.textContent = 'Cast';
  panel.append(castHead);

  const castLabel = document.createElement('label');
  castLabel.textContent = 'number';
  const castBox = document.createElement('input');
  castBox.value = character.cast;
  castBox.onchange = () => {
    const value = parseInt(castBox.value, 10);
    if (Number.isNaN(value)) return;
    const was = character.cast;
    character.cast = value;
    drawHierarchy();
    drawInspector();
    pushUndo(doc, `cast of ${character.model}`,
      () => { character.cast = was; drawHierarchy(); drawInspector(); },
      () => { character.cast = value; drawHierarchy(); drawInspector(); });
    say('cast changed - use Save placement to keep it', 'good');
  };
  castLabel.append(castBox);
  panel.append(castLabel);

  const castNote = document.createElement('p');
  castNote.className = 'none';
  castNote.textContent = character.hasScript
    ? `The map's script has a cast ${character.cast}, and that code is what this `
      + `character does. It runs ${character.instructions} instructions.`
    : `The map's script has no cast ${character.cast}. The character will stand there `
      + `and do nothing until one is written, or until this points at a cast that exists.`;
  panel.append(castNote);

  // --------------------------------------------------------------- position

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

  // ------------------------------------------------------------- what it says

  const heading = document.createElement('h3');
  heading.textContent = 'What it says';
  panel.append(heading);

  if (!character.lines.length) {
    const none = document.createElement('p');
    none.className = 'none';
    none.textContent = character.hasScript
      ? 'Its cast runs code but shows no dialogue - it may move, open a shop, or '
        + 'trigger a scene.'
      : 'Nothing, because there is no cast with this number.';
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

  // --------------------------------------------------------------- behaviour

  const behaviour = document.createElement('h3');
  behaviour.textContent = 'Behaviour';
  panel.append(behaviour);

  const link = document.createElement('a');
  link.href = '#';
  link.style.color = 'var(--accent)';
  // Offering to jump to a cast that does not exist was the thing that made the panel
  // look like it was contradicting itself.
  link.textContent = character.hasScript
    ? `open ${mapState.name}.script at cast ${character.cast}`
    : `open ${mapState.name}.script`;
  link.onclick = async event => {
    event.preventDefault();
    const opened = await openDoc('script', `files/${mapState.name}.script`);
    const text = $('textarea', opened.pane);
    if (!text) return;
    const at = text.value.indexOf(`cast${character.cast}_main:`);
    if (at >= 0) goToLine(text, text.value.slice(0, at).split('\n').length, 1);
  };
  panel.append(link);
  return panel;
}
