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
  // Both halves of the map's exits, so the add panel can say whether another one is
  // possible before offering to write it.
  mapState.exits = await api(`/api/map/exits?name=${encodeURIComponent(name)}`)
    .catch(() => null);
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

/// Every map name, for the boxes that want one. A datalist rather than a dropdown,
/// because 356 names are worth typing into rather than scrolling through.
function fillMapNames() {
  let names = $('#map-names');
  if (!names) {
    names = document.createElement('datalist');
    names.id = 'map-names';
    document.body.append(names);
  }
  // Only while the project is showing maps - any other list would fill it with the
  // wrong names.
  if (state.browse !== 'map') return;
  if (names.childElementCount === (state.files || []).length) return;
  names.textContent = '';
  for (const file of state.files) {
    const option = document.createElement('option');
    option.value = file.name;
    names.append(option);
  }
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

// While this is set, the next click in the 3D scene is a place rather than a
// selection. Adding a door to a building means putting it in the doorway, and no
// number typed into a box will land there - you have to point at it.
let placing = null;

/// Waits for a click on the ground, then hands back where it landed.
function placeOnMap(node, what, onPlaced) {
  const wrap = $('.scene-wrap', node);
  if (!wrap || wrap.hidden) {
    say('the 3D view has to be open to place something', 'bad');
    return;
  }
  placing = { onPlaced };
  wrap.classList.add('placing');
  say(`click where the ${what} goes`);
}

/// Takes the mode off, finding the pane itself - the click that ends it comes from
/// deep inside the scene, where the view node is not in scope.
function stopPlacing() {
  placing = null;
  for (const wrap of $$('.scene-wrap.placing')) wrap.classList.remove('placing');
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
  },
  {
    id: 'exit',
    label: 'Exit to another map',
    note: 'Writes both halves: the row in the map\u2019s .pak saying where it leads, and '
      + 'a region in the collision mesh that fires it. A map has twelve exit slots, '
      + 'because that is how many jump attributes the collision test walks.',
    objectsOnly: false,
    noModel: true
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
  // An OpenFF mod's own object comes first for an OpenFF project: nothing of the game's
  // is written for it - no .hich row, no cast, no script - so it is the light way in.
  const openff = openFFProject();
  if (openff) {
    const option = document.createElement('option');
    option.value = 'openff';
    option.textContent = 'OpenFF object - the mod\'s own, no script';
    kind.append(option);
  }
  for (const entry of BEHAVIOURS) {
    const option = document.createElement('option');
    option.value = entry.id;
    option.textContent = entry.label;
    kind.append(option);
  }
  kindLabel.append(kind);

  const kindNote = document.createElement('p');
  kindNote.className = 'none';

  // The OpenFF object's own: its name, and whether it has a model at all.
  const nameLabel = document.createElement('label');
  nameLabel.className = 'wide';
  nameLabel.textContent = 'name';
  const objectName = document.createElement('input');
  objectName.placeholder = 'Chest, Trigger, Spawn…';
  nameLabel.append(objectName);
  const noModelLabel = document.createElement('label');
  noModelLabel.className = 'wide toggle';
  const noModel = document.createElement('input');
  noModel.type = 'checkbox';
  noModelLabel.append(noModel, document.createTextNode(' no model - a spot with logic on it'));

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

  const pick = document.createElement('button');
  pick.type = 'button';
  pick.className = 'wide-button';
  // It only picks the position; "place it" read as if it created the thing.
  pick.textContent = 'Pick position on the map…';
  pick.title = 'Then click a spot in the 3D view; the x and z above fill in.';
  pick.onclick = () => placeOnMap(node, kind.value === 'exit' ? 'doorway' : 'object',
    (px, py, pz) => {
      x.value = String(px);
      z.value = String(pz);
      say(`at ${px}, ${pz}`);
    });

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

  // Where an exit leads, and the state that decides whether one can be added at all.
  const toLabel = document.createElement('label');
  toLabel.className = 'wide';
  toLabel.textContent = 'leads to which map';
  const to = document.createElement('input');
  to.setAttribute('list', 'map-names');
  to.value = '';
  toLabel.append(to);

  const arriveLabel = document.createElement('label');
  arriveLabel.textContent = 'arrival index';
  const arrive = document.createElement('input');
  arrive.value = '1';
  arrive.title = 'Which exit on the far map the player comes out of.';
  arriveLabel.append(arrive);

  const exitNote = document.createElement('p');
  exitNote.className = 'none';

  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Add to the map';

  // The action sits in a strip that stays at the bottom of the panel while the form
  // scrolls, so it is never below the fold.
  const actions = document.createElement('div');
  actions.className = 'sticky-actions';
  actions.append(go);

  panel.append(kindLabel, kindNote, nameLabel, noModelLabel, modelHead, choose, modelNote,
    xLabel, zLabel, pick, textLabel, itemLabel, goldLabel,
    toLabel, arriveLabel, exitNote, actions);

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
    nameLabel.hidden = id !== 'openff';
    noModelLabel.hidden = id !== 'openff';
    sub.textContent = id === 'openff'
      ? 'The mod\'s own object: nothing of the game\'s is written for it.'
      : 'Writes the row that places it, the call that boots it, and the cast that gives it behaviour.';
    if (id === 'openff') {
      kindNote.textContent = 'Saved in the mod\'s scenes/' + mapState.name + '.json, not in the game\'s data: '
        + 'a GameObject the OpenFF client makes when the map is entered, with the model standing there (or nothing, for a trigger or a spawn point). '
        + 'Rename it, swap its model, put children under it, attach the mod\'s C# behaviours - all in the inspector, any time.';
      textLabel.hidden = itemLabel.hidden = goldLabel.hidden = toLabel.hidden = arriveLabel.hidden = exitNote.hidden = true;
      modelHead.hidden = choose.hidden = modelNote.hidden = noModel.checked;
      chosenName.textContent = model;
      modelNote.textContent = 'Any of the game\'s models - it need not be one this map loads.';
      go.disabled = false;
      return;
    }
    const behaviour = BEHAVIOURS.find(b => b.id === id) || BEHAVIOURS[0];
    kindNote.textContent = behaviour.note;
    textLabel.hidden = id !== 'talk';
    itemLabel.hidden = id !== 'chest';
    goldLabel.hidden = id !== 'money';
    toLabel.hidden = id !== 'exit';
    arriveLabel.hidden = id !== 'exit';
    exitNote.hidden = id !== 'exit';
    modelHead.hidden = Boolean(behaviour.noModel);
    choose.hidden = Boolean(behaviour.noModel);
    modelNote.hidden = Boolean(behaviour.noModel);
    if (id === 'chest') fillItems();
    if (id === 'exit') {
      fillMapNames();
      const at = mapState.exits;
      go.disabled = !(at && at.canAdd);
      exitNote.textContent = !at
        ? 'This map will not say how its exits stand.'
        : !at.hasMesh
          ? 'This map has no collision mesh of its own - 23 maps borrow another\u2019s - '
            + 'so there is nothing here to put a trigger in.'
          : !at.hasTable
            ? 'This map has no jumps table, so there is nowhere to say where an exit leads.'
            : at.rows >= at.slots
              ? `This map already has all ${at.slots} exits.`
              : `It will be exit ${at.rows + 1} of at most ${at.slots}. `
                + `The mesh already triggers ${at.triggers.join(', ') || 'none'}.`;
    } else {
      go.disabled = false;
    }

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
  noModel.onchange = () => showFor(kind.value);

  choose.onclick = () => {
    const behaviour = BEHAVIOURS.find(b => b.id === kind.value);
    pickModel(model, (chosen) => {
      model = chosen;
      picked = true;
      showFor(kind.value);
    }, behaviour && behaviour.objectsOnly
      ? { only: isObjectModel, title: 'Choose an object', what: 'object models' }
      : {});
  };

  go.onclick = async () => {
    go.disabled = true;

    if (kind.value === 'openff') {
      // Nothing to ask the server: the object goes into the scene state, selected, and
      // the panel's Save writes the file.
      const doc = activeDoc;
      try {
        await loadSceneState(mapState.name);
        const object = addSceneObject(doc, {
          name: objectName.value.trim() || (noModel.checked ? 'Object' : model),
          model: noModel.checked ? null : model,
          at: [parseInt(x.value, 10) || 0, 0, parseInt(z.value, 10) || 0]
        });
        say(`${object.name} added to the OpenFF scene - Save objects in its panel keeps it`, 'good');
      } catch (error) {
        say(error.message, 'bad');
        go.disabled = false;
      }
      return;
    }

    if (kind.value === 'exit') {
      try {
        const result = await api('/api/map/exit/add', {
          name: mapState.name,
          x: parseInt(x.value, 10) || 0,
          y: 0,
          z: parseInt(z.value, 10) || 0,
          to: to.value.trim(),
          toIndex: parseInt(arrive.value, 10) || 0,
          rotationY: 0,
          conditionFlag: 1,
          kind: -1
        });
        if (!result.ok) {
          say(result.error, 'bad');
          go.disabled = false;
          return;
        }
        for (const note of result.notes || []) say(note);
        say(`exit ${result.slot} added`, 'good');
        markOverridden(`files/${mapState.name}.pak`, true);
        await open(mapState.name);
        if (activeDoc) {
          activeDoc.selection = `exit:${result.slot - 1}`;
          drawInspector();
        }
      } catch (error) {
        say(error.message, 'bad');
        go.disabled = false;
      }
      return;
    }

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
      if (doc.scene3d) doc.scene3d.selectExit(index);
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
  const mirrorBox = $('.mirror-toggle', node);
  const tools = $('.tools', node);
  const sizeBox = $('.gizmo-size', node);

  const show = async (mode) => {
    doc.mode = mode;
    $$('.mode', node).forEach(b => b.classList.toggle('on', b.dataset.mode === mode));
    flat.hidden = mode !== '2d';
    solid.hidden = mode !== '3d';
    recentre.hidden = mode !== '3d';
    gizmoBox.hidden = mode !== '3d';
    mirrorBox.hidden = mode !== '3d' || !(doc.scene3d && doc.scene3d.isField);
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
        // An exit's two halves come back tagged, because they are not rows in the
        // .hich and Save placement has nothing to do with them - each has its own
        // save in the panel.
        if (item.part) {
          const exit = mapState.data.exits[item.exit];
          const sceneExit = doc.data.scene.exits[item.exit];
          if (item.part === 'arrival' && exit) {
            exit.x = sceneExit.x;
            exit.y = sceneExit.y;
            exit.z = sceneExit.z;
            exit.rotationY = sceneExit.rotationY;
          } else if (exit) {
            exit.region = sceneExit.region;
          }
          drawInspector();
          return;
        }

        if (item.point !== undefined) {
          // One of the mod's objects: the 3D view moved its world copy; the object keeps
          // local numbers (relative to its parent), and its children come along.
          const moved = flattenSceneObjects(sceneState)[item.point];
          if (moved) {
            sceneSetWorld(moved.source, item.x, item.y, item.z, item.rotationY);
            syncSceneObjects(doc);
            doc.scene3d.selectPoint(item.point);
          }
          sceneState.dirty = true;
          drawInspector();
          return;
        }

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
      doc.scene3d.onFrame(() => {
        drawSceneTags(node, doc);
        mirrorBox.hidden = doc.mode !== '3d' || !doc.scene3d.isField;
        $('.mirror', node).checked = doc.scene3d.mirrorZ;
      });

      await doc.scene3d.load(scene, (item, kind) => {
        if (kind === 'exit') {
          mapState.selected = null;
          doc.selection = `exit:${item.index}`;
        } else if (kind === 'point') {
          mapState.selected = null;
          const picked = flattenSceneObjects(sceneState)[item.index];
          doc.selection = picked ? `scene:${picked.path}` : null;
        } else {
          mapState.selected = mapState.data.characters.find(c => c.index === item.index);
          doc.selection = `object:${item.index}`;
        }
        drawHierarchy();
        drawInspector();
      });
      say('');
      refreshScenePoints(doc);
    }
    doc.scene3d.redraw();
    if (doc.selection && doc.selection.startsWith('object:')) {
      doc.scene3d.select(Number(doc.selection.slice(7)));
    }
    if (doc.selection && doc.selection.startsWith('scene:')) {
      const index = flattenSceneObjects(sceneState).findIndex(i => 'scene:' + i.path === doc.selection);
      if (index >= 0) doc.scene3d.selectPoint(index);
    }
  };

  $$('.mode', node).forEach(button => {
    button.onclick = () => show(button.dataset.mode).catch(e => say(e.message, 'bad'));
  });
  recentre.onclick = () => doc.scene3d && doc.scene3d.reset();
  $('.mirror', node).onchange = (event) => doc.scene3d && doc.scene3d.setMirrorZ(event.target.checked);
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
    if (moved >= 4) return;

    // While something is being placed, a click is a position rather than a pick.
    if (placing) {
      const at = doc.scene3d.groundAt(event.clientX, event.clientY);
      if (!at) {
        say('that is not somewhere on the ground', 'bad');
        return;
      }
      const done = placing.onPlaced;
      stopPlacing();
      done(at[0], at[1], at[2]);
      return;
    }

    doc.scene3d.pickAt(event.clientX, event.clientY);
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
    if (openFFProject()) {
      const add = document.createElement('button');
      add.className = 'wide-button';
      add.textContent = 'Add an OpenFF object where the camera looks';
      add.title = 'The mod\'s own object: a spot with logic on it, or a model standing there - no script of the game\'s behind it. Drag it into place in the 3D view.';
      add.onclick = () => addSceneObject(doc);
      box.append(add);
    }
    behavioursSection(box, 'map', 'map');
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

  if (ref.startsWith('scene:')) {
    const found = findSceneObject(ref.slice(6));
    return found ? buildSceneObject(doc, found.source) : null;
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
// The three destinations that are not places. The game resolves each when the player
// goes through it - see the strcmp chain in the map jump - so they are correct rather
// than broken, and saying so matters when a fifth of the game's exits are not plain
// map names.
const SENTINELS = ['back_field_map', 'back_town_map', 'back_from_inv'];

// The number after the # in a destination is not an exit - it is which shared interior
// to draw. getCommonMdl turns it into a stage name, and these are all of them. So
// "t23_03#03" means: the characters, script and exits of t23_03, standing in the
// scenery of s02_03 - which is why t23_03 has a jumps table and no collision mesh of
// its own, and s02_03 has a mesh and no table. The two halves are in two maps.
const INTERIORS = [
  [-1, 'none - use the map\u2019s own scenery'],
  [1, '1 - s02_01'], [2, '2 - s02_02'], [3, '3 - s02_03'],
  [4, '4 - s02_04'], [5, '5 - s02_05'],
  [30, '30 - t30_01'],
  [99, '99 - keep the one already loaded']
];

function whatSentinel(name) {
  if (name === 'back_field_map') return 'Goes back to the world map you came from.';
  if (name === 'back_town_map') return 'Goes back to the town you came from.';
  if (name === 'back_from_inv') return 'Goes back to the field you boarded from.';
  return '';
}

// An exit is two halves. The row here says where the player lands, on which map, facing
// which way; what makes it fire is a region of the map's collision mesh carrying one of
// twelve jump attributes, in <map>_col.mcl.lz. Both are written together - see
// Editor/MapExits.cs - so an exit is never half made.
//
// Any of them can be removed. Slot N fires row N and nothing else joins them, so taking
// one from the middle shifts every row above it down - and the mesh attributes, this
// map's setMapJumpFlag calls and other maps' arrival indices all move with it. The panel
// lists what names the slot before you decide.
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
    modelNo: exit.modelNo === undefined ? -1 : exit.modelNo,
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

  // Beside the box rather than under it, because it belongs to the box: the name is
  // only half an answer until you have looked at the place.
  const row = document.createElement('span');
  row.className = 'with-button';
  const go = document.createElement('button');
  go.className = 'icon-button';
  go.title = 'open that map';
  go.append(icon('map'));
  // Not every destination is a map you can open. Of the game's 666 exits, 542 lead to
  // one; 122 lead to a world tile, which is a .flsc.lz and has no map to edit; and
  // three names are instructions resolved when the player goes through them.
  const openable = () => Boolean(draft.to)
    && (state.files || []).some(f => f.name === draft.to);
  const refresh = () => {
    go.disabled = !openable();
    go.title = openable() ? `open ${draft.to}`
      : SENTINELS.includes(draft.to) ? whatSentinel(draft.to)
        : draft.to ? `${draft.to} is not a map this can open - probably a world tile`
          : 'nowhere to open';
  };
  to.oninput = () => { draft.to = to.value.trim(); refresh(); fillArrivals(); };
  go.onclick = () => { if (openable()) openDoc('map', draft.to); };
  refresh();
  row.append(to, go);
  toLabel.append(row);
  panel.append(toLabel);

  fillMapNames();

  // Which of the far map's exits the player comes out of. A dropdown rather than a
  // number, because the far map knows how many it has and typing a number that is not
  // one of them is a link to nowhere.
  const arriveLabel = document.createElement('label');
  arriveLabel.className = 'wide';
  arriveLabel.textContent = 'arrives at which of its exits';
  const arrive = document.createElement('select');
  arriveLabel.append(arrive);
  panel.append(arriveLabel);

  const fillArrivals = async () => {
    arrive.textContent = '';
    let slots = [];
    if (draft.to && (state.files || []).some(f => f.name === draft.to)) {
      try {
        slots = (await api(`/api/map/exits?name=${encodeURIComponent(draft.to)}`))
          .rowsuggestions || [];
      } catch (error) {
        slots = [];
      }
    }
    if (!slots.length) {
      // Nothing to read - a world tile, a sentinel, or a map with no table. The number
      // still has to be settable, so it falls back to what it already is.
      const only = document.createElement('option');
      only.value = draft.toIndex;
      only.textContent = draft.toIndex + ' - ' + (draft.to
        ? 'cannot read that map\u2019s exits' : 'no destination');
      arrive.append(only);
      return;
    }
    slots.forEach((label, i) => {
      const option = document.createElement('option');
      option.value = i + 1;
      option.textContent = label;
      arrive.append(option);
    });
    // A row pointing past the end is a broken link, and hiding it would not mend it.
    if (draft.toIndex < 1 || draft.toIndex > slots.length) {
      const odd = document.createElement('option');
      odd.value = draft.toIndex;
      odd.textContent = draft.toIndex + ' - nothing there';
      arrive.append(odd);
    }
    arrive.value = String(draft.toIndex);
  };
  arrive.onchange = () => { draft.toIndex = parseInt(arrive.value, 10) || 0; };
  fillArrivals();

  const interiorLabel = document.createElement('label');
  interiorLabel.className = 'wide';
  interiorLabel.textContent = 'shared interior';
  const interior = document.createElement('select');
  for (const [value, label] of INTERIORS) {
    const option = document.createElement('option');
    option.value = value;
    option.textContent = label;
    interior.append(option);
  }
  interior.value = String(draft.modelNo);
  interior.onchange = () => { draft.modelNo = parseInt(interior.value, 10); };
  interior.title = 'Which scenery the destination is drawn in. The houses of a town '
    + 'share five interiors between them.';
  interiorLabel.append(interior);
  panel.append(interiorLabel);

  const whatIs = document.createElement('p');
  whatIs.className = 'none';
  whatIs.textContent = SENTINELS.includes(draft.to)
    ? whatSentinel(draft.to)
    : (state.files || []).some(f => f.name === draft.to)
      ? 'A map, so it can be opened and edited.'
      : draft.to
        ? 'Not a map with characters of its own - most likely a world tile, which is a '
          + '.flsc.lz and has nothing here to edit.'
        : 'This exit names nowhere.';
  panel.append(whatIs);

  // --------------------------------------------------------- where it lands

  const whereHead = document.createElement('h3');
  whereHead.textContent = 'Where people arrive here';
  panel.append(whereHead);
  const whereNote = document.createElement('p');
  whereNote.className = 'none';
  whereNote.textContent = 'This is not where this exit takes you - it is where the '
    + 'player appears when they come into ' + mapState.name + ' through this slot, '
    + 'which is what another map\u2019s "arrives at" points to. Where this one goes '
    + 'is above.';
  panel.append(whereNote);
  panel.append(field('x', 'x'), field('y', 'y'), field('z', 'z'),
    field('facing', 'rotationY',
      'Degrees. The file keeps it as a 16 bit angle where a whole turn is 65536.'));

  // --------------------------------------------------------------- the trigger
  //
  // The other half. The arrival above is where the player comes out; this is the
  // doorway they walk into, and it is a box in the collision mesh somewhere else
  // entirely - about 20 units away in the shipped maps. Moving one does not move the
  // other, so each gets its own gizmo and its own save.

  const doorHead = document.createElement('h3');
  doorHead.textContent = 'Doorway';
  panel.append(doorHead);

  const borrowed = doc.data.scene.borrowed;
  if (borrowed) {
    const shared = document.createElement('p');
    shared.className = 'none warn';
    const others = doc.data.scene.sharedWith || [];
    shared.textContent = `${mapState.name} has no scenery or collision of its own - it `
      + `is drawn in ${borrowed}, and so ${others.length === 0 ? 'nothing else is'
        : `are ${others.join(', ')}`}. This doorway lives in ${borrowed}'s mesh, so `
      + `moving it moves ${others.length ? 'theirs too' : 'it for anything else drawn there'}.`;
    panel.append(shared);
  }

  if (!exit.region) {
    const none = document.createElement('p');
    none.className = 'none';
    none.textContent = 'This exit has no region in the collision mesh, so nothing '
      + 'fires it. 37 of the shipped exits are like this - they are places the player '
      + 'arrives at rather than leaves by.';
    panel.append(none);
  } else {
    const region = {
      x: exit.region[0], y: exit.region[1], z: exit.region[2],
      width: exit.region[3], height: exit.region[4], depth: exit.region[5]
    };

    const which = document.createElement('div');
    which.className = 'segmented';
    for (const [id, label] of [['arrival', 'Move arrival'], ['region', 'Move doorway']]) {
      const button = document.createElement('button');
      button.textContent = label;
      button.className = (doc.scene3d && doc.scene3d.part() === id) ? 'on' : '';
      button.onclick = () => {
        if (doc.scene3d) doc.scene3d.selectExit(index, id);
        drawInspector();
      };
      which.append(button);
    }
    panel.append(which);

    const box = (label, key, hint) => {
      const wrap = document.createElement('label');
      wrap.textContent = label;
      const input = document.createElement('input');
      input.value = region[key];
      if (hint) input.title = hint;
      input.oninput = () => {
        const value = parseInt(input.value, 10);
        region[key] = Number.isNaN(value) ? 0 : value;
      };
      wrap.append(input);
      return wrap;
    };

    panel.append(box('x', 'x'), box('z', 'z'),
      box('width', 'width', 'The shipped doorways are about 19 across.'),
      box('depth', 'depth', 'About 12 deep.'),
      box('floor', 'y', 'The bottom of the box. It reaches below the floor so it still '
        + 'catches a player on slightly lower ground.'),
      box('height', 'height', 'About 54 tall.'));

    const move = document.createElement('button');
    move.textContent = 'Save doorway';
    move.onclick = async () => {
      move.disabled = true;
      try {
        const result = await api('/api/map/exit/region', {
          name: mapState.name, slot: index + 1,
          x: region.x, y: region.y, z: region.z,
          width: region.width, height: region.height, depth: region.depth
        });
        if (!result.ok) {
          say(result.error, 'bad');
          move.disabled = false;
          return;
        }
        for (const line of result.notes || []) say(line);
        say(`doorway for exit ${index + 1} saved`, 'good');
        await open(mapState.name);
      } catch (error) {
        say(error.message, 'bad');
        move.disabled = false;
      }
    };
    panel.append(move);
  }

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

  // What names this slot by number, so removing it is a decision made with the
  // consequences in front of you rather than after. Three kinds of thing can - the
  // region that fires it, this map's own setMapJumpFlag calls, and the exits on other
  // maps that arrive here - and the list arrives on its own.
  const used = document.createElement('h3');
  used.textContent = 'Referred to by';
  const refs = document.createElement('ul');
  refs.className = 'lines';
  const counting = document.createElement('li');
  counting.textContent = 'looking…';
  refs.append(counting);
  panel.append(used, refs);

  const exits = (doc.data.scene.exits || []).length;
  const remove = document.createElement('button');
  remove.className = 'danger';
  remove.textContent = 'Remove this exit';
  remove.onclick = async () => {
    const after = exits - (index + 1);
    if (!confirm(`Remove exit ${index + 1} and the region that fires it?`
      + (after > 0
        ? `\n\nThe ${after} exit${after === 1 ? '' : 's'} after it will move down a `
          + 'slot, and everything that names them by number moves with them.'
        : ''))) return;
    remove.disabled = true;
    try {
      const result = await api('/api/map/exit/delete',
        { name: mapState.name, slot: index + 1 });
      if (!result.ok) {
        say(result.error, 'bad');
        remove.disabled = false;
        return;
      }
      for (const line of result.notes || []) say(line);
      say(`exit ${index + 1} removed`, 'good');
      await open(mapState.name);
    } catch (error) {
      say(error.message, 'bad');
      remove.disabled = false;
    }
  };
  panel.append(remove);

  api(`/api/map/exit/references?name=${encodeURIComponent(mapState.name)}`
    + `&slot=${index + 1}`).then(found => {
    refs.textContent = '';
    if (!found.to.length) {
      const none = document.createElement('li');
      none.textContent = 'nothing names this slot by number';
      refs.append(none);
      return;
    }
    for (const one of found.to) {
      const row = document.createElement('li');
      row.textContent = `${one.kind} · ${one.what}`;
      row.title = one.file;
      refs.append(row);
    }
  }).catch(() => {
    refs.textContent = '';
    const failed = document.createElement('li');
    failed.textContent = 'could not work out what points here';
    refs.append(failed);
  });

  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'An exit is two halves. This row says where the player lands; '
    + 'what makes it fire is a region of ' + mapState.name + '_col.mcl.lz carrying one '
    + 'of twelve jump attributes. Both are written together, and removing one from the '
    + 'middle renumbers everything that named a slot after it.';
  panel.append(note);
  behavioursSection(panel, 'exit:' + index, 'exit');
  return panel;
}

/// Opens the text file a message is written in, and goes to it.
///
/// A map's lines live in one .msd per language, so which file is a question of which
/// language is being edited - the server says which one it read the text through, and
/// that is the one worth opening.
async function openMessage(id, cast) {
  const file = `${state.messagePrefix || 'en.lproj/'}${mapState.name}.msd`;
  if (!(state.files || []).some(f => f.name === file) && state.browse !== 'text') {
    // The project is showing something else, so its file list cannot confirm this.
    // Opening it is still the right move; a missing file reports itself.
  }
  let doc;
  try {
    doc = await openDoc('text', file);
  } catch (error) {
    say(`${file} would not open: ${error.message}`, 'bad');
    return;
  }
  const row = doc && $(`.message[data-message-id="${id}"]`, doc.pane);
  if (!row) {
    say(`message ${id} is not in ${file}`, 'bad');
    return;
  }
  // Marked until another one is opened. A highlight that fades is already gone by
  // the time you have finished scrolling to it and looked up.
  for (const lit of $$('.message.found', doc.pane)) lit.classList.remove('found');
  row.scrollIntoView({ block: 'center' });
  row.classList.add('found');
  say(`message ${id}, said by cast ${cast}`);
  const area = $('textarea', row);
  if (area) area.focus();
}

/// What names a cast in this map's script, and the lines it says.
///
/// A cast number is identity rather than position - the game finds one by scanning for
/// it - so nothing here has to be renumbered when a character is deleted. What this is
/// for is the other question: what would be left reaching for somebody who is gone.
function addCastReferences(panel, character) {
  const head = document.createElement('h3');
  head.textContent = 'Referred to by';
  const list = document.createElement('ul');
  list.className = 'lines';
  const waiting = document.createElement('li');
  waiting.textContent = 'looking…';
  list.append(waiting);
  panel.append(head, list);

  api(`/api/map/cast/references?name=${encodeURIComponent(mapState.name)}`
    + `&cast=${character.cast}`).then(found => {
    list.textContent = '';
    if (!found.to.length) {
      const none = document.createElement('li');
      none.textContent = 'nothing in the script names cast ' + character.cast;
      list.append(none);
    }
    for (const one of found.to) {
      const row = document.createElement('li');
      row.className = 'ref';
      const where = document.createElement('i');
      where.textContent = one.kind;
      const what = document.createElement('code');
      what.textContent = one.text;
      row.append(where, what);
      row.title = one.what + ' · line ' + one.line;
      // The script is a document like any other, and this is a line in it.
      row.onclick = async () => {
        const doc = await openDoc('script', `files/${mapState.name}.script`);
        if (doc) revealLine(doc, one.line);
      };
      list.append(row);
    }
    if (found.says.length) {
      const said = document.createElement('li');
      said.textContent = `says ${found.says.length} line`
        + `${found.says.length === 1 ? '' : 's'}: ${found.says.join(', ')}`;
      list.append(said);
    }
  }).catch(() => {
    list.textContent = '';
    const failed = document.createElement('li');
    failed.textContent = 'could not read the script';
    list.append(failed);
  });
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
    character.lines.forEach((line, at) => {
      const item = document.createElement('li');
      const words = document.createElement('span');
      words.textContent = line;
      item.append(words);

      // The id the line came from, so it can be opened where it is written rather
      // than hunted for in a file of two thousand.
      const id = character.lineIds && character.lineIds[at];
      if (id !== undefined) {
        const edit = document.createElement('button');
        edit.className = 'icon-button';
        edit.title = `edit message ${id}`;
        edit.append(icon('text'));
        edit.onclick = () => openMessage(id, character.cast);
        item.append(edit);
        item.classList.add('with-edit');
      }
      list.append(item);
    });
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

  addCastReferences(panel, character);
  behavioursSection(panel, 'object:' + character.index, character.model || 'character');
  return panel;
}

// ------------------------------------------------------------- behaviours (OpenFF)
//
// For an OpenFF project (one targeting ours or oursff4) with C# code: the mod's Behaviour
// types, attached to the selected object with their public fields filled in - Unity's
// inspector, in short. Saved per map as scenes/<map>.json in the project; Export to OpenFF
// carries it into the mod and the engine puts the behaviours on the objects when the map is
// entered. The file is keyed by the map's name alone, which both games share - a mod that
// puts behaviours on FF3's and FF4's d01_05 would meet in one file; per-game scene folders
// are the engine's to add.

const sceneState = { map: null, attachments: null, objects: null, catalog: null, dirty: false };

// ------------------------------------------------------ the mod's own objects
//
// sceneState.objects is the tree the file holds: each { name, x, y, z, rotationY, scale,
// model, tags, children }, a child's numbers relative to its parent (the offset turned by
// the parent's yaw and scaled by its scale, the yaw added, the scale multiplied - what the
// engine's SceneLoader does). Every object also gets a non-enumerable `parent`, so a path
// and a world transform can be worked out from the object alone. The 3D view is handed a
// flat list in world terms; a drag there comes back in world terms and is written into
// the object as local ones.

function fromSceneFile(list, parent) {
  return (list || []).filter(o => o && o.name).map(o => {
    const object = {
      name: String(o.name), x: o.x || 0, y: o.y || 0, z: o.z || 0,
      rotationY: o.yaw || 0, scale: o.scale > 0 ? o.scale : 1,
      model: o.model || null, tags: o.tags || [], children: []
    };
    Object.defineProperty(object, 'parent', { value: parent || null, writable: true, enumerable: false });
    object.children = fromSceneFile(o.children, object);
    return object;
  });
}

/// The objects as the file holds them: yaw is the engine's (0 = +z, 90 = +x).
function objectsForFile(list) {
  return (list || []).map(o => {
    const out = { name: o.name, x: o.x, y: o.y, z: o.z, yaw: o.rotationY || 0 };
    if (o.scale && o.scale !== 1) out.scale = o.scale;
    if (o.model) out.model = o.model;
    if (o.tags && o.tags.length) out.tags = o.tags;
    if (o.children && o.children.length) out.children = objectsForFile(o.children);
    return out;
  });
}

function scenePathOf(object) {
  const parts = [];
  for (let o = object; o; o = o.parent) parts.unshift(o.name);
  return parts.join('/');
}

/// Where an object stands in the world, its parents' transforms applied.
function sceneWorldOf(object) {
  if (!object.parent) {
    return { x: object.x, y: object.y, z: object.z, rotationY: object.rotationY || 0, scale: object.scale || 1 };
  }
  const p = sceneWorldOf(object.parent);
  const a = p.rotationY * Math.PI / 180;
  const c = Math.cos(a), s = Math.sin(a);
  const ox = object.x * p.scale, oy = object.y * p.scale, oz = object.z * p.scale;
  return {
    x: p.x + c * ox + s * oz, y: p.y + oy, z: p.z - s * ox + c * oz,
    rotationY: p.rotationY + (object.rotationY || 0), scale: p.scale * (object.scale || 1)
  };
}

/// Sets an object's local numbers from a world position (the inverse of sceneWorldOf).
function sceneSetWorld(object, wx, wy, wz, worldYaw) {
  if (!object.parent) {
    object.x = wx; object.y = wy; object.z = wz;
    if (worldYaw !== undefined) object.rotationY = worldYaw;
    return;
  }
  const p = sceneWorldOf(object.parent);
  const a = p.rotationY * Math.PI / 180;
  const c = Math.cos(a), s = Math.sin(a);
  const dx = wx - p.x, dy = wy - p.y, dz = wz - p.z;
  const k = p.scale || 1;
  object.x = (c * dx - s * dz) / k;
  object.y = dy / k;
  object.z = (s * dx + c * dz) / k;
  if (worldYaw !== undefined) object.rotationY = worldYaw - p.rotationY;
}

/// The tree as one list in world terms, parents before children, for the 3D view and the
/// hierarchy. `point` is the row's index (the 3D view's key for it); `source` the object.
function flattenSceneObjects(state, list, depth, out) {
  out = out || [];
  for (const object of (list || (state && state.objects) || [])) {
    const world = sceneWorldOf(object);
    out.push({
      point: out.length, path: scenePathOf(object), name: object.name, depth: depth || 0,
      x: world.x, y: world.y, z: world.z, rotationY: world.rotationY, scale: world.scale,
      model: object.model || null,
      package: object.model ? `files/${object.model}.nmdp.lz` : null,
      source: object
    });
    flattenSceneObjects(state, object.children, (depth || 0) + 1, out);
  }
  return out;
}

function findSceneObject(path) {
  return flattenSceneObjects(sceneState).find(i => i.path.toLowerCase() === (path || '').toLowerCase()) || null;
}

/// A fresh, unique name among siblings: Object, Object 2, Object 3...
function uniqueSceneName(siblings, wanted) {
  const base = (wanted || 'Object').replace(/[\/:]/g, ' ').trim() || 'Object';
  let name = base;
  let n = 2;
  while (siblings.some(o => o.name.toLowerCase() === name.toLowerCase())) name = base + ' ' + (n++);
  return name;
}

/// Hands the 3D view the objects as they stand now.
function syncSceneObjects(doc) {
  if (doc && doc.scene3d) doc.scene3d.setPoints(flattenSceneObjects(sceneState));
}

/// Renames or moves an object in the tree: every attachment on it or under it follows.
function retargetAttachments(oldPath, newPath) {
  const from = oldPath.toLowerCase();
  for (const a of (sceneState.attachments || [])) {
    const t = (a.target || '').toLowerCase();
    if (t === from) a.target = newPath;
    else if (t.startsWith(from + '/')) a.target = newPath + a.target.slice(oldPath.length);
    else if (t === 'point:' + from) a.target = newPath;
  }
}

function openFFProject() {
  const project = typeof projectState !== 'undefined' && projectState.project;
  return project && typeof isOpenFFProject === 'function' && isOpenFFProject(project) ? project : null;
}

async function loadSceneState(map) {
  if (sceneState.map !== map || !sceneState.attachments) {
    const scene = await api(`/api/project/scene?map=${encodeURIComponent(map)}`);
    if (!scene.ok) throw new Error(scene.error);
    sceneState.map = map;
    sceneState.attachments = scene.attachments || [];
    // The older files' "point:<name>" targets read as the object's path.
    for (const a of sceneState.attachments) if (/^point:/i.test(a.target || '')) a.target = a.target.slice(6);
    sceneState.objects = fromSceneFile(scene.objects || scene.points);
    sceneState.dirty = false;
  }
  if (!sceneState.catalog) {
    const catalog = await api('/api/project/code/catalog');
    if (!catalog.ok) throw new Error(catalog.error);
    sceneState.catalog = catalog;
  }
  return sceneState;
}

function invalidateCatalog() { sceneState.catalog = null; }

function behavioursSection(panel, target, what) {
  const project = openFFProject();
  if (!project || !mapState.name) return;
  const heading = document.createElement('h3');
  heading.textContent = 'Behaviours (OpenFF)';
  panel.append(heading);
  const box = document.createElement('div');
  box.className = 'behaviours';
  panel.append(box);
  if (!project.code) {
    const add = document.createElement('button');
    add.className = 'behaviour-add-button';
    add.textContent = 'Add C# code';
    add.title = 'Writes the mod\'s C# project and a starting GameService; Behaviour classes in it attach to this ' + what;
    add.onclick = () => { if (typeof addCode === 'function') addCode(); };
    box.append(add);
    return;
  }
  const map = mapState.name;
  loadSceneState(map).then(state => drawBehaviours(box, state, target, what)).catch(error => {
    const bad = document.createElement('p');
    bad.className = 'none warn';
    bad.textContent = error.message;
    box.append(bad);
  });
}

function drawBehaviours(box, state, target, what) {
  box.innerHTML = '';
  const catalog = state.catalog;
  const mine = state.attachments.filter(a => (a.target || '').toLowerCase() === target.toLowerCase());
  for (const attachment of mine) {
    box.append(behaviourCard(state, attachment, target));
  }
  // One wide button, as Unity's Add Component: the list of behaviours drops down from it
  // with a search box, and a name nobody has written yet becomes a new script.
  const add = document.createElement('button');
  add.className = 'behaviour-add-button';
  add.textContent = 'Add Behaviour';
  add.title = 'Attach one of the code\'s Behaviour classes to this ' + what + ', or write a new one';
  add.onclick = () => behaviourPicker(add, state, target, box, what);
  box.append(add);
  if (!catalog.built && behaviourChoices(catalog).length) {
    const note = document.createElement('p');
    note.className = 'none';
    note.textContent = 'Not built yet - the behaviours attach now and get their fields after Build.';
    box.append(note);
  }
  if (catalog.problems && catalog.problems.length) {
    const p = document.createElement('p');
    p.className = 'none warn';
    p.textContent = catalog.problems.join(' · ');
    box.append(p);
  }
  const actions = document.createElement('div');
  actions.className = 'behaviour-actions';
  const save = document.createElement('button');
  save.textContent = state.dirty ? 'Save behaviours' : 'Saved';
  save.disabled = !state.dirty;
  save.onclick = async () => {
    try {
      const result = await api('/api/project/scene/save', { map: state.map, attachments: state.attachments, objects: objectsForFile(state.objects) });
      if (!result.ok) throw new Error(result.error);
      state.dirty = false;
      say(`scenes/${state.map}.json saved (${result.count} attachment(s), ${flattenSceneObjects(state).length} object(s)) - Run in OpenFF to see it`, 'good');
      drawBehaviours(box, state, target, what);
      if (typeof projectChanged === 'function') projectChanged();
    } catch (error) {
      say(error.message, 'bad');
    }
  };
  actions.append(save);
  const others = state.attachments.length - mine.length;
  if (others > 0) {
    const note = document.createElement('span');
    note.className = 'dim';
    note.textContent = `${others} more on other objects of this map`;
    actions.append(note);
  }
  box.append(actions);
}

/// Every behaviour the picker can offer: the built ones with their fields, and the ones
/// the source declares but no build has seen yet (marked, so the fields' absence is
/// explained). A name in both is one entry.
function behaviourChoices(catalog) {
  const choices = (catalog.behaviours || []).map(b => ({
    name: b.name, fullName: b.fullName, summary: b.summary, fields: b.fields || [], built: true,
    file: (catalog.sources || []).find(s => s.kind === 'behaviour' && s.name === b.name)?.file || null,
  }));
  for (const s of (catalog.sources || [])) {
    if (s.kind !== 'behaviour' || choices.some(c => c.name === s.name)) continue;
    choices.push({ name: s.name, fullName: s.name, summary: null, fields: [], built: false, file: s.file, line: s.line });
  }
  choices.sort((a, b) => a.name.localeCompare(b.name));
  return choices;
}

/// The source file a behaviour lives in, when the code declares it.
function behaviourSource(catalog, name) {
  return (catalog.sources || []).find(s => s.kind === 'behaviour' && s.name === name) || null;
}

function attachBehaviour(state, target, choice) {
  const fields = {};
  for (const f of choice.fields) if (f.default !== null && f.default !== undefined) fields[f.name] = f.default;
  state.attachments.push({ target, behaviour: choice.name, fields });
  state.dirty = true;
}

/// Unity's Add Component list, for behaviours: a search box, one row per class with its
/// icon and name (the summary as a tooltip - the row is a name, not a paragraph), the
/// arrow keys and Enter, and at the bottom the way to a class that does not exist yet:
/// the text typed becomes a new Behaviour script, attached at once and opened.
function behaviourPicker(anchor, state, target, box, what) {
  document.querySelectorAll('.dropdown').forEach(p => p.remove());
  const catalog = state.catalog;
  const choices = behaviourChoices(catalog);
  const picker = document.createElement('div');
  picker.className = 'dropdown';
  const search = document.createElement('input');
  search.type = 'search';
  search.placeholder = 'search, or a name for a new one';
  search.autocomplete = 'off';
  const head = document.createElement('div');
  head.className = 'dropdown-head';
  head.textContent = 'Behaviour';
  const list = document.createElement('ul');
  list.className = 'dropdown-list';
  picker.append(search, head, list);

  // Fixed, under the button, as wide as it; above it when the panel's bottom is near.
  document.body.append(picker);
  const at = anchor.getBoundingClientRect();
  picker.style.width = Math.max(220, at.width) + 'px';
  picker.style.left = at.left + 'px';
  const height = Math.min(320, picker.offsetHeight || 320);
  if (at.bottom + height + 8 < window.innerHeight) picker.style.top = (at.bottom + 2) + 'px';
  else picker.style.top = Math.max(4, at.top - height - 2) + 'px';

  let rows = [];
  let cursor = 0;
  const close = () => {
    picker.remove();
    document.removeEventListener('pointerdown', outside, true);
  };
  const outside = e => { if (!picker.contains(e.target) && e.target !== anchor) close(); };
  document.addEventListener('pointerdown', outside, true);

  const pick = async row => {
    if (!row) return;
    if (row.create) {
      await createBehaviourScript(state, target, row.create, box, what);
      close();
      return;
    }
    attachBehaviour(state, target, row.choice);
    close();
    drawBehaviours(box, state, target, what);
  };

  const draw = () => {
    const query = search.value.trim();
    const q = query.toLowerCase();
    list.innerHTML = '';
    rows = [];
    const shown = choices.filter(c => !q || c.name.toLowerCase().includes(q) || (c.fullName || '').toLowerCase().includes(q));
    for (const choice of shown) {
      const li = document.createElement('li');
      li.append(icon('behaviour'));
      const name = document.createElement('span');
      name.className = 'dropdown-name';
      name.textContent = choice.name;
      li.append(name);
      if (!choice.built) {
        const note = document.createElement('span');
        note.className = 'dropdown-note';
        note.textContent = 'not built';
        li.append(note);
      }
      li.title = (choice.summary ? choice.summary + '\n' : '') + (choice.file || choice.fullName || '');
      li.onclick = () => pick({ choice });
      list.append(li);
      rows.push({ choice, element: li });
    }
    if (!shown.length && !query) {
      const li = document.createElement('li');
      li.className = 'dropdown-empty';
      li.textContent = catalog.code ? 'No Behaviour classes yet - type a name to write one.' : 'The project has no C# code yet.';
      list.append(li);
    }
    // The new-script row: whatever was typed, when nothing is called exactly that.
    const exact = shown.some(c => c.name.toLowerCase() === q);
    if (catalog.code && (!query || !exact)) {
      const li = document.createElement('li');
      li.className = 'dropdown-create' + (query ? '' : ' dim');
      li.append(icon('code'));
      const label = document.createElement('span');
      label.className = 'dropdown-name';
      const cleaned = query.replace(/[^A-Za-z0-9_]/g, '');
      label.textContent = cleaned ? `New Behaviour "${cleaned}"` : 'New Behaviour… (type its name)';
      li.append(label);
      li.title = cleaned ? `Writes code/${cleaned}.cs, attaches it here and opens it` : '';
      if (cleaned) {
        li.onclick = () => pick({ create: cleaned });
        list.append(li);
        rows.push({ create: cleaned, element: li });
      } else {
        list.append(li);
      }
    }
    cursor = Math.min(cursor, Math.max(0, rows.length - 1));
    mark();
  };
  const mark = () => {
    rows.forEach((r, i) => r.element.classList.toggle('on', i === cursor));
    const on = rows[cursor];
    if (on) on.element.scrollIntoView({ block: 'nearest' });
  };
  search.oninput = () => { cursor = 0; draw(); };
  search.onkeydown = e => {
    if (e.key === 'ArrowDown') { cursor = Math.min(rows.length - 1, cursor + 1); mark(); e.preventDefault(); }
    else if (e.key === 'ArrowUp') { cursor = Math.max(0, cursor - 1); mark(); e.preventDefault(); }
    else if (e.key === 'Enter') { pick(rows[cursor]); e.preventDefault(); }
    else if (e.key === 'Escape') { close(); e.preventDefault(); }
  };
  draw();
  search.focus();
}

/// A new Behaviour class from the picker: the file from the starter, attached to the object
/// straight away (its fields come with the next build), and opened in the code view.
async function createBehaviourScript(state, target, name, box, what) {
  try {
    const made = await api('/api/project/file/new', { name, template: 'behaviour' });
    if (!made.ok) throw new Error(made.error);
    attachBehaviour(state, target, { name, fields: [] });
    state.catalog = null;
    await loadSceneState(state.map);
    say(`${made.name} written and attached to this ${what} - Build to give it fields, Save behaviours to keep it on the map`, 'good');
    drawBehaviours(box, sceneState, target, what);
    if (typeof projectChanged === 'function') projectChanged();
    if (typeof openDoc === 'function') await openDoc('code', made.name);
  } catch (error) {
    say(error.message, 'bad');
  }
}

function behaviourCard(state, attachment, target) {
  const catalog = state.catalog;
  const type = catalog.behaviours.find(b => b.name === attachment.behaviour);
  const source = behaviourSource(catalog, attachment.behaviour);
  const card = document.createElement('div');
  card.className = 'behaviour-card' + (type ? '' : ' missing');
  const head = document.createElement('div');
  head.className = 'behaviour-head';
  head.append(icon('behaviour'));
  const name = document.createElement('b');
  name.textContent = attachment.behaviour;
  name.title = type ? (type.fullName + (type.summary ? '\n' + type.summary : '')) : source ? source.file + ' - not built yet' : 'not in the code';
  head.append(name);
  if (source && typeof openDoc === 'function') {
    const open = document.createElement('button');
    open.className = 'icon-button';
    open.append(icon('code'));
    open.title = 'Open ' + source.file;
    open.onclick = () => openDoc('code', source.file);
    head.append(open);
  }
  const remove = document.createElement('button');
  remove.textContent = '×';
  remove.title = 'Remove this behaviour';
  remove.onclick = () => {
    state.attachments.splice(state.attachments.indexOf(attachment), 1);
    state.dirty = true;
    drawBehaviours(card.parentElement, state, target, '');
  };
  head.append(remove);
  card.append(head);
  if (!type) {
    const p = document.createElement('p');
    p.className = 'none' + (source ? '' : ' warn');
    p.textContent = source
      ? 'Not built yet - Build the C# code and its fields appear here.'
      : 'This behaviour is not in the code (renamed or removed).';
    card.append(p);
    return card;
  }
  attachment.fields = attachment.fields || {};
  for (const f of (type.fields || [])) {
    const row = document.createElement('label');
    row.className = 'behaviour-field';
    row.title = (f.summary || '') + (f.summary ? ' ' : '') + '(' + f.typeName + ')';
    const label = document.createElement('span');
    label.textContent = f.name;
    row.append(label);
    const has = Object.prototype.hasOwnProperty.call(attachment.fields, f.name);
    const value = has ? attachment.fields[f.name] : f.default;
    let input;
    const changed = v => { attachment.fields[f.name] = v; state.dirty = true; markDirty(card); };
    if (f.type === 'bool') {
      input = document.createElement('input');
      input.type = 'checkbox';
      input.checked = !!value;
      input.onchange = () => changed(input.checked);
    } else if (f.type === 'enum') {
      input = document.createElement('select');
      for (const o of (f.options || [])) {
        const option = document.createElement('option');
        option.value = o;
        option.textContent = o;
        input.append(option);
      }
      input.value = value == null ? '' : String(value);
      input.onchange = () => changed(input.value);
    } else if (f.type === 'vector3' || f.type === 'vector2' || f.type === 'color') {
      input = document.createElement('div');
      input.className = 'behaviour-parts';
      const parts = f.type === 'vector3' ? ['x', 'y', 'z'] : f.type === 'vector2' ? ['x', 'y'] : ['r', 'g', 'b', 'a'];
      const current = Object.assign({}, value || {});
      for (const part of parts) {
        const n = document.createElement('input');
        n.type = 'number';
        n.step = f.type === 'color' ? '1' : 'any';
        n.placeholder = part;
        n.value = current[part] == null ? '' : current[part];
        n.oninput = () => { current[part] = Number(n.value); changed(Object.assign({}, current)); };
        input.append(n);
      }
    } else {
      input = document.createElement('input');
      input.type = f.type === 'int' || f.type === 'float' ? 'number' : 'text';
      if (f.type === 'float') input.step = 'any';
      input.value = value == null ? '' : value;
      input.oninput = () => changed(f.type === 'int' ? parseInt(input.value, 10) || 0 : f.type === 'float' ? Number(input.value) || 0 : input.value);
    }
    row.append(input);
    card.append(row);
  }
  return card;
}

/// Writes the scene file as it stands (objects and attachments) - the objects' own Save.
async function saveScene() {
  const state = sceneState;
  if (!state.map) return;
  try {
    const result = await api('/api/project/scene/save', { map: state.map, attachments: state.attachments || [], objects: objectsForFile(state.objects) });
    if (!result.ok) throw new Error(result.error);
    state.dirty = false;
    say(`scenes/${state.map}.json saved (${flattenSceneObjects(state).length} object(s), ${result.count} attachment(s))`, 'good');
    if (typeof projectChanged === 'function') projectChanged();
    drawHierarchy();
    drawInspector();
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// Objects and behaviours for the map that is open, into the 3D view and the hierarchy.
async function refreshScenePoints(doc) {
  if (!openFFProject() || !mapState.name) return;
  try {
    await loadSceneState(mapState.name);
    syncSceneObjects(doc);
    drawHierarchy();
  } catch (error) {
    console.warn(error);
  }
}

/// A new object where the camera looks (or at a given spot, or under a parent), named
/// uniquely and selected. With a model it stands there as that model; without one it is a
/// spot with logic on it - a trigger, a spawn point, a mark.
function addSceneObject(doc, options = {}) {
  const state = sceneState;
  if (!state.objects) state.objects = [];
  const parent = options.parent || null;
  const siblings = parent ? parent.children : state.objects;
  const name = uniqueSceneName(siblings, options.name || (options.model ? options.model : parent ? 'Child' : 'Object'));
  const object = {
    name, x: 0, y: 0, z: 0, rotationY: 0, scale: 1,
    model: options.model || null, tags: options.tags || [], children: []
  };
  Object.defineProperty(object, 'parent', { value: parent, writable: true, enumerable: false });
  siblings.push(object);
  if (!parent) {
    const spot = options.at || (doc && doc.scene3d ? doc.scene3d.viewCentre() : [0, 0, 0]);
    object.x = Math.round(spot[0]);
    object.y = Math.round(spot[1]);
    object.z = Math.round(spot[2]);
  }
  state.dirty = true;
  const path = scenePathOf(object);
  syncSceneObjects(doc);
  if (doc && doc.scene3d) {
    const index = flattenSceneObjects(state).findIndex(i => i.path === path);
    if (index >= 0) doc.scene3d.selectPoint(index);
  }
  if (doc) doc.selection = 'scene:' + path;
  mapState.selected = null;
  drawHierarchy();
  drawInspector();
  return object;
}

/// Takes an object (and its subtree) out, with every attachment on any of them.
function removeSceneObject(doc, object) {
  const siblings = object.parent ? object.parent.children : sceneState.objects;
  const index = siblings.indexOf(object);
  if (index < 0) return;
  const gone = flattenSceneObjects(sceneState, [object]).map(i => i.path.toLowerCase());
  siblings.splice(index, 1);
  sceneState.attachments = (sceneState.attachments || []).filter(a => !gone.includes((a.target || '').toLowerCase()));
  sceneState.dirty = true;
  if (doc) doc.selection = null;
  syncSceneObjects(doc);
  if (doc && doc.scene3d) doc.scene3d.selectPoint(null);
  drawHierarchy();
  drawInspector();
}

/// Moves an object under another parent (or to the root), keeping its place in the world.
function reparentSceneObject(doc, object, parent) {
  if (parent === object.parent) return;
  for (let p = parent; p; p = p.parent) if (p === object) return; // not under itself
  const world = sceneWorldOf(object);
  const oldPath = scenePathOf(object);
  const from = object.parent ? object.parent.children : sceneState.objects;
  from.splice(from.indexOf(object), 1);
  object.parent = parent;
  const to = parent ? parent.children : sceneState.objects;
  object.name = uniqueSceneName(to, object.name);
  to.push(object);
  sceneSetWorld(object, world.x, world.y, world.z, world.rotationY);
  const p = parent ? sceneWorldOf(parent) : { scale: 1 };
  object.scale = world.scale / (p.scale || 1);
  retargetAttachments(oldPath, scenePathOf(object));
  sceneState.dirty = true;
  if (doc) doc.selection = 'scene:' + scenePathOf(object);
  syncSceneObjects(doc);
  drawHierarchy();
  drawInspector();
}

/// The inspector for one of the mod's objects: its name, model, place, tags, parent,
/// children, behaviours, and a way out. Everything here is the file's - no game data behind it.
function buildSceneObject(doc, object) {
  const panel = document.createElement('div');
  const path = scenePathOf(object);
  const title = document.createElement('h2');
  title.textContent = object.name;
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = (object.model ? object.model + ' · ' : '') + 'OpenFF object · ' + mapState.name + '/' + path;
  panel.append(title, sub);

  const redraw = () => {
    sceneState.dirty = true;
    syncSceneObjects(doc);
    drawHierarchy();
    drawInspector();
  };

  // ------------------------------------------------------------------ model

  const modelHead = document.createElement('h3');
  modelHead.textContent = 'Model';
  panel.append(modelHead);
  const choose = document.createElement('button');
  choose.className = 'model-choice';
  const chosenName = document.createElement('span');
  chosenName.textContent = object.model || '(none - a spot with logic on it)';
  choose.append(icon('model'), chosenName);
  choose.onclick = () => pickModel(object.model || 'n011', (model) => {
    if (model === object.model) return;
    object.model = model;
    redraw();
  });
  panel.append(choose);
  if (object.model) {
    const none = document.createElement('button');
    none.className = 'wide-button';
    none.textContent = 'No model - logic only';
    none.onclick = () => { object.model = null; redraw(); };
    panel.append(none);
  }
  const modelNote = document.createElement('p');
  modelNote.className = 'none';
  modelNote.textContent = object.model
    ? 'The game shows it as a plain character - the model standing there, no cast, no script. Swap it any time; the mod\'s behaviours are what it does.'
    : 'Nothing is drawn for it in the game; its behaviours run at this spot (a Trigger, a spawn point). Pick a model to make it visible.';
  panel.append(modelNote);

  // ------------------------------------------------------------- place

  const placeHead = document.createElement('h3');
  placeHead.textContent = object.parent ? 'Place (relative to ' + object.parent.name + ')' : 'Place';
  panel.append(placeHead);
  const grid = document.createElement('div');
  grid.className = 'point-grid';
  const field = (label, key, step, title) => {
    const wrap = document.createElement('label');
    wrap.className = 'behaviour-field';
    if (title) wrap.title = title;
    const span = document.createElement('span');
    span.textContent = label;
    const input = document.createElement('input');
    input.type = key === 'name' || key === 'tags' ? 'text' : 'number';
    if (step) input.step = step;
    input.value = key === 'tags' ? (object.tags || []).join(' ') : object[key];
    input.onchange = () => {
      if (key === 'name') {
        const fresh = input.value.trim().replace(/[\/:]/g, ' ').trim();
        const siblings = object.parent ? object.parent.children : sceneState.objects;
        if (!fresh || fresh.toLowerCase() === 'map' || siblings.some(o => o !== object && o.name.toLowerCase() === fresh.toLowerCase())) { input.value = object.name; return; }
        // Attachments on it and under it follow the rename.
        const was = scenePathOf(object);
        object.name = fresh;
        retargetAttachments(was, scenePathOf(object));
        doc.selection = 'scene:' + scenePathOf(object);
      } else if (key === 'tags') {
        object.tags = input.value.split(/[ ,]+/).map(s => s.trim()).filter(Boolean);
      } else if (key === 'scale') {
        object.scale = Number(input.value) > 0 ? Number(input.value) : 1;
      } else {
        object[key] = Number(input.value) || 0;
      }
      redraw();
    };
    wrap.append(span, input);
    grid.append(wrap);
  };
  field('name', 'name', null, 'Free to change; behaviours attached to it follow. Not / or :');
  field('x', 'x', '1');
  field('y', 'y', '1');
  field('z', 'z', '1');
  field('yaw', 'rotationY', '1', '0 faces +z, 90 faces +x');
  field('scale', 'scale', '0.1', '1 is the model\'s own size');
  field('tags', 'tags', null, 'Words a mod finds it by: Game.World.Legacy.WithTag("chest")');
  panel.append(grid);

  // The parent: a list of every other object, and the root.
  const parentWrap = document.createElement('label');
  parentWrap.className = 'behaviour-field';
  const parentLabel = document.createElement('span');
  parentLabel.textContent = 'parent';
  const parentPick = document.createElement('select');
  const rootOption = document.createElement('option');
  rootOption.value = '';
  rootOption.textContent = '(none - top level)';
  parentPick.append(rootOption);
  const mine = flattenSceneObjects(sceneState, [object]).map(i => i.source);
  for (const item of flattenSceneObjects(sceneState)) {
    if (mine.includes(item.source)) continue;
    const option = document.createElement('option');
    option.value = item.path;
    option.textContent = ' '.repeat(item.depth * 2) + item.path;
    parentPick.append(option);
  }
  parentPick.value = object.parent ? scenePathOf(object.parent) : '';
  parentPick.onchange = () => {
    const found = parentPick.value ? findSceneObject(parentPick.value) : null;
    reparentSceneObject(doc, object, found ? found.source : null);
  };
  parentWrap.append(parentLabel, parentPick);
  panel.append(parentWrap);

  const hint = document.createElement('p');
  hint.className = 'none';
  hint.textContent = 'Drag the arrows in the 3D view to move it; the turn ring sets the yaw. '
    + (object.parent ? 'Its numbers are relative to its parent, so moving the parent moves it too. ' : '')
    + 'In a mod: Game.World.Legacy.Find("' + mapState.name + '/' + path + '")' + (object.tags && object.tags.length ? ' or .WithTag("' + object.tags[0] + '")' : '') + '.';
  panel.append(hint);

  // --------------------------------------------------------- children

  const kidsHead = document.createElement('h3');
  kidsHead.textContent = 'Children';
  panel.append(kidsHead);
  if (object.children.length) {
    const list = document.createElement('ul');
    list.className = 'scene-children';
    for (const child of object.children) {
      const li = document.createElement('li');
      li.append(icon(child.model ? 'model' : 'exit'));
      const name = document.createElement('a');
      name.href = '#';
      name.textContent = child.name + (child.model ? '  (' + child.model + ')' : '');
      name.onclick = (event) => {
        event.preventDefault();
        doc.selection = 'scene:' + scenePathOf(child);
        const index = flattenSceneObjects(sceneState).findIndex(i => i.source === child);
        if (doc.scene3d && index >= 0) doc.scene3d.selectPoint(index);
        drawHierarchy();
        drawInspector();
      };
      li.append(name);
      list.append(li);
    }
    panel.append(list);
  }
  const addChild = document.createElement('button');
  addChild.className = 'wide-button';
  addChild.textContent = 'Add a child object';
  addChild.title = 'A new object under this one, at its spot; it moves with it';
  addChild.onclick = () => addSceneObject(doc, { parent: object });
  panel.append(addChild);

  // ---------------------------------------------------------- actions

  const actions = document.createElement('div');
  actions.className = 'behaviour-actions';
  const save = document.createElement('button');
  save.textContent = sceneState.dirty ? 'Save objects' : 'Saved';
  save.disabled = !sceneState.dirty;
  save.onclick = saveScene;
  const remove = document.createElement('button');
  remove.textContent = object.children.length ? 'Delete with children' : 'Delete object';
  remove.onclick = () => removeSceneObject(doc, object);
  actions.append(save, remove);
  panel.append(actions);

  behavioursSection(panel, path, 'object');
  return panel;
}

function markDirty(card) {
  const box = card.closest('.behaviours');
  const save = box && box.querySelector('.behaviour-actions button');
  if (save) { save.textContent = 'Save behaviours'; save.disabled = false; }
}
