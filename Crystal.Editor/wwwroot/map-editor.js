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
  // The mod's own model files (assets/*.glb), which an OpenFF object may wear: the client
  // draws them directly. Not for a Steam mod, whose game knows only its own formats.
  if (!state.assets && typeof isOpenFFProject === 'function' && isOpenFFProject()) {
    try { state.assets = await api('/api/project/assets'); } catch (e) { state.assets = []; }
  }
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
    onShow: () => {
      // Two maps open: coming back to this one makes it the map the editor's state is
      // about again - the placement, the OpenFF scene (its pending save written first).
      if (mapState.name !== name) {
        mapState.name = name;
        mapState.data = data;
        mapState.selected = null;
        const pending = sceneState.dirty && sceneState.map && sceneState.map !== name ? saveScene({ quiet: true }).catch(() => {}) : Promise.resolve();
        pending.then(() => loadSceneState(name)).then(() => { syncSceneObjects(doc); drawHierarchy(); }).catch(() => {});
      }
      if (doc.scene3d && doc.mode === '3d') doc.scene3d.redraw();
    }
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

    // On an OpenFF project a dropped model is an object of the mod's own, there and then
    // - the shortest way from the library to the map. Shift held asks the game's way
    // (a .hich row and a cast) through the Add dialog instead.
    if (openFFProject() && !event.shiftKey) {
      loadSceneState(mapState.name).then(() => {
        const object = addSceneObject(doc, { model, name: model, at: [Math.round(at[0]), Math.round(at[1]), Math.round(at[2])] });
        say(`${object.name} placed at ${Math.round(at[0])}, ${Math.round(at[2])} - an OpenFF object (drop with Shift for one of the game's)`, 'good');
      }).catch(error => say(error.message, 'bad'));
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
  // An OpenFF object without a model - a spot with logic on it - is the model row's ×,
  // as in the inspector; noModel says where that stands.
  const noModel = { checked: false };

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

  const pickRow = document.createElement('div');
  pickRow.className = 'model-pick';
  const choose = document.createElement('button');
  choose.className = 'model-choice';
  const chosenName = document.createElement('span');
  choose.append(icon('model'), chosenName);
  const clearModel = document.createElement('button');
  clearModel.type = 'button';
  clearModel.className = 'clear-button';
  clearModel.textContent = '×';
  clearModel.title = 'No model - a spot with logic on it (a trigger, a spawn point)';
  pickRow.append(choose, clearModel);

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

  panel.append(kindLabel, kindNote, nameLabel, modelHead, pickRow, modelNote,
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
    clearModel.hidden = id !== 'openff' || noModel.checked;
    sub.textContent = id === 'openff'
      ? 'The mod\'s own object: nothing of the game\'s is written for it.'
      : 'Writes the row that places it, the call that boots it, and the cast that gives it behaviour.';
    if (id === 'openff') {
      kindNote.textContent = 'Saved in the mod\'s scenes/' + mapState.name + '.json, not in the game\'s data: '
        + 'a GameObject the OpenFF client makes when the map is entered, with the model standing there (or nothing, for a trigger or a spawn point). '
        + 'Rename it, swap its model, put children under it, attach the mod\'s C# behaviours - all in the inspector, any time.';
      textLabel.hidden = itemLabel.hidden = goldLabel.hidden = toLabel.hidden = arriveLabel.hidden = exitNote.hidden = true;
      modelHead.hidden = pickRow.hidden = modelNote.hidden = false;
      choose.classList.toggle('none-picked', noModel.checked);
      chosenName.textContent = noModel.checked ? 'None - a spot with logic on it' : model;
      choose.title = noModel.checked ? 'Nothing is drawn for it; its behaviours run at the spot. Click to give it a model.' : 'Click to swap the model; × for none';
      modelNote.textContent = noModel.checked ? 'A trigger, a spawn point, a place a behaviour watches: nothing is drawn, the object is still there to find by name or tag.' : 'Any of the game\'s models - it need not be one this map loads.';
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
    pickRow.hidden = Boolean(behaviour.noModel);
    choose.classList.remove('none-picked');
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

  kind.onchange = () => { noModel.checked = false; showFor(kind.value); };
  clearModel.onclick = () => { noModel.checked = true; showFor(kind.value); };

  choose.onclick = () => {
    const behaviour = BEHAVIOURS.find(b => b.id === kind.value);
    pickModel(model, (chosen) => {
      model = chosen;
      picked = true;
      noModel.checked = false;
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
        say(`${object.name} added to the OpenFF scene`, 'good');
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
    item.package = /\.(glb|gltf)$/i.test(model) ? model : `files/${model}.nmdp.lz`;
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
          sceneChanged();
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

  // Play here: an OpenFF project's shortest loop - this map, the selection's spot (or the
  // camera's), in the client, with the mod exported first.
  const play = $('.play', node);
  if (play) {
    play.hidden = !openFFProject();
    play.onclick = () => {
      let pos = null;
      const sel = doc.selection || '';
      if (sel.startsWith('scene:')) {
        const found = findSceneObject(sel.slice(6));
        if (found) pos = [found.x, found.y, found.z];
      } else if (sel.startsWith('object:')) {
        const c = mapState.data.characters.find(ch => ch.index === Number(sel.slice(7)));
        if (c) pos = [c.x, c.y, c.z];
      } else if (sel.startsWith('exit:')) {
        const e = (doc.data.scene.exits || [])[Number(sel.slice(5))];
        if (e) pos = [e.x, e.y, e.z];
      }
      // With nothing selected the hero arrives where the map's own entry puts them - not at
      // the camera's target, which drifts up as the camera flies (a hero put there stood on
      // the roofs, over everything) and is nowhere a player would start.
      runInOpenFF({ map: mapState.name, pos });
    };
  }
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
/// The map's random encounters on the terrain card: five groups of four monster parties
/// (the map's .pak, chain 2), each a picker over the game's parties and the mod's
/// formations; a tile type's land form sends the player to a group. Every change saves
/// into the map's .pak - a file of the project's, so a Steam mod carries it too.
function encountersSection(box, map) {
  const head = document.createElement('h3');
  head.textContent = 'Random encounters';
  box.append(head);
  const body = document.createElement('div');
  body.className = 'encounters';
  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'loading…';
  body.append(note);
  box.append(body);
  api(`/api/map/encounters?name=${encodeURIComponent(map)}`).then(async data => {
    body.textContent = '';
    if (!data.ok) {
      note.textContent = data.error === `no ${map}.pak` ? 'This map has no parameter file - no random encounters.' : data.error;
      body.append(note);
      return;
    }
    if (!state.formations) { try { state.formations = await api('/api/formations'); } catch (e) { state.formations = []; } }
    const groups = data.groups.map(g => g.slice());
    let timer = null;
    const save = () => {
      clearTimeout(timer);
      timer = setTimeout(async () => {
        try {
          const r = await api('/api/map/encounters/save', { name: map, groups });
          if (!r.ok) throw new Error(r.error);
          markOverridden(r.name, true);
          say(`saved ${shortName(r.name)} - the map's encounters`, 'good');
        } catch (e) { say('encounters: ' + e.message, 'bad'); }
      }, 400);
    };
    const intro = document.createElement('p');
    intro.className = 'none';
    intro.textContent = 'A fight on this map draws one of its group\'s four parties (blank slots skipped). Which ground fights which group is in the terrain\'s collision materials (attribute flags 20-24 - group 1 is the usual one); a map without such ground has no random fights.';
    body.append(intro);
    groups.forEach((slots, g) => {
      const row = document.createElement('div');
      row.className = 'encounter-group';
      const label = document.createElement('b');
      label.textContent = `Group ${g + 1}`;
      label.title = `The parties the ground marked for group ${g + 1} fights`;
      row.append(label);
      const picks = document.createElement('div');
      picks.className = 'encounter-slots';
      slots.forEach((party, s) => {
        const pick = document.createElement('select');
        const none = document.createElement('option');
        none.value = '0';
        none.textContent = '–';
        pick.append(none);
        const mod = document.createElement('optgroup');
        mod.label = 'the mod\'s formations';
        const game = document.createElement('optgroup');
        game.label = 'the game\'s parties';
        for (const entry of state.formations || []) {
          const option = document.createElement('option');
          option.value = entry.id;
          option.textContent = `${entry.id} · ${entry.name}`;
          (entry.mod ? mod : game).append(option);
        }
        if (mod.childElementCount) pick.append(mod);
        pick.append(game);
        pick.value = String(party || 0);
        if (pick.value !== String(party || 0)) {
          const unknown = document.createElement('option');
          unknown.value = String(party);
          unknown.textContent = `${party} · (not in the tables)`;
          pick.append(unknown);
          pick.value = String(party);
        }
        pick.title = `Slot ${s + 1} of group ${g + 1}`;
        pick.onchange = () => { groups[g][s] = parseInt(pick.value, 10) || 0; save(); };
        picks.append(pick);
      });
      row.append(picks);
      body.append(row);
    });
  }).catch(e => { note.textContent = e.message; });
}

function inspectRef(doc, ref) {
  const scene = doc.data && doc.data.scene;
  if (!scene) return null;

  if (ref === 'terrain') {
    const box = document.createElement('div');
    box.className = 'scene-object';
    box.append(inspectorHead('terrain', shortName(scene.terrain), 'terrain · the map itself, and its behaviours'));
    const heading = document.createElement('h3');
    heading.textContent = 'Model';
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
      const convertHead = document.createElement('h3');
      convertHead.textContent = 'OpenFF';
      box.append(convertHead);
      const convertNote = document.createElement('p');
      convertNote.className = 'none';
      convertNote.textContent = 'Every character the map\'s boot places becomes an object of the mod\'s own - same model, place, idle, recolour - and the original is taken off the map in the client. Exact: each keeps its own cast, so talk, branches, menus and chests run as before, 1:1. With components: a Chest or Talk stands in where the cast is one, editable in the inspector. Characters a scene places stay the game\'s, and are listed.';
      box.append(convertNote);
      const convert = document.createElement('button');
      convert.className = 'wide-button';
      convert.textContent = 'Convert the map\'s characters (exact)';
      convert.onclick = () => convertMap(doc).catch(error => say(error.message, 'bad'));
      box.append(convert);
      const convertParts = document.createElement('button');
      convertParts.className = 'wide-button';
      convertParts.textContent = 'Convert with components (Chest, Talk where they fit)';
      convertParts.onclick = () => convertMap(doc, { components: true }).catch(error => say(error.message, 'bad'));
      box.append(convertParts);
    }
    behavioursSection(box, 'map', 'map');
    encountersSection(box, doc.name);
    return cardify(box);
  }

  if (ref.startsWith('object:')) {
    const index = Number(ref.slice(7));
    const character = doc.data.characters.find(c => c.index === index);
    return character ? cardify(buildCharacter(character)) : null;
  }

  if (ref.startsWith('exit:')) {
    const index = Number(ref.slice(5));
    return cardify(buildExit(scene.exits[index], index));
  }

  if (ref.startsWith('scene:')) {
    const found = findSceneObject(ref.slice(6));
    return found ? cardify(buildSceneObject(doc, found.source)) : null;
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
  panel.className = 'scene-object';
  panel.append(inspectorHead('exit', `Exit to ${exit.to || '(nowhere)'}`,
    `slot ${index + 1} of ${(doc.data.scene.exits || []).length} · the game's own (.pak row and collision region)`));

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

  // The arrival spot is the exit's transform: where the player appears coming into this
  // map through this slot. (Where the exit leads is the card below; the doorway that fires
  // it is a box elsewhere in the mesh, with its own card.)
  panel.append(transformCard({
    read: key => draft[key],
    write: (key, value) => { draft[key] = Math.round(value); },
    rotation: 'rotationY',
    note: 'arrival',
    positionTitle: 'Where the player appears when they come into ' + mapState.name + ' through this slot',
    rotationTitle: 'Degrees. The file keeps it as a 16 bit angle where a whole turn is 65536.',
    hint: 'Where the player appears coming in through this slot - what another map\u2019s "arrives at" points to. Not where this exit takes you; that is Leads to. Save exit keeps it.'
  }));

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

  panel.className = 'scene-object';
  panel.append(inspectorHead('character', character.model || '(no model)',
    `${character.kindName} · cast ${character.cast} · the game's own (.hich row ${character.index})`));

  // -------------------------------------------------------------- transform
  //
  // Typed numbers move the character in the plan and the scene as they are typed; the
  // spell of typing is one undo step, recorded when the field is left.
  let before = null;
  panel.append(transformCard({
    read: key => character[key],
    write: (key, value) => {
      character[key] = Math.round(value);
      if (node) drawMap(node);
      if (activeDoc && activeDoc.scene3d) {
        const item = (activeDoc.data.scene.objects || []).find(o => o.index === character.index);
        if (item) {
          item.x = character.x;
          item.y = character.y;
          item.z = character.z;
          item.rotationY = character.rotationY;
          activeDoc.scene3d.redraw();
        }
      }
    },
    onFocus: () => { before = positionOf(character); },
    onBlur: () => { if (before) recordMove(activeDoc, character, before); before = null; },
    rotation: 'rotationY',
    rotationTitle: 'Facing in degrees, as the .hich row keeps it (the game negates it)',
    positionTitle: 'Whole units, as the .hich row keeps them',
  }));

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

  addTreasure(panel, character);
  addCastReferences(panel, character);
  if (openFFProject()) addConvert(panel, character);
  behavioursSection(panel, 'object:' + character.index, character.model || 'character');
  return panel;
}

// ----------------------------------------------------------- the game's chests
//
// A chest of the game's is one command in the map's boot cast - setTreasureItem(cast,
// item, group, index, 0, 0), or setTreasureMoney with gil in the item's place - so what
// it holds is edited by rewriting that one line and compiling the script again. The
// same card the OpenFF Chest has, on the game's own.

const TREASURE = /setTreasure(Item|Money)\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)/g;

/// The treasure command for a cast in a script's source, or null.
function treasureOf(source, cast) {
  TREASURE.lastIndex = 0;
  let m;
  while ((m = TREASURE.exec(source))) {
    if (Number(m[2]) === cast) {
      return { kind: m[1] === 'Item' ? 'item' : 'gil', value: Number(m[3]), group: Number(m[4]), index: Number(m[5]), text: m[0], at: m.index };
    }
  }
  return null;
}

/// The script's text with a cast's treasure command replaced.
function withTreasure(source, cast, kind, value) {
  const found = treasureOf(source, cast);
  if (!found) return source;
  const call = `setTreasure${kind === 'item' ? 'Item' : 'Money'}(${cast}, ${value}, ${found.group}, ${found.index}, 0, 0)`;
  return source.slice(0, found.at) + call + source.slice(found.at + found.text.length);
}

async function mapScriptSource() {
  const data = await api(`/api/script?name=${encodeURIComponent('files/' + mapState.name + '.script')}`);
  return data && data.source || '';
}

function addTreasure(panel, character) {
  const holder = document.createElement('div');
  panel.append(holder);
  mapScriptSource().then(source => {
    const found = treasureOf(source, character.cast);
    if (!found) return;
    // Built after cardify ran over the panel, so it is a card of its own from the start.
    const card = componentCard('logic', 'Treasure');
    holder.append(card);
    const note = document.createElement('p');
    note.className = 'none';
    note.textContent = `The map's script sets what this chest holds (${found.text}). Change it here; Save treasure rewrites that line and compiles the script.`;
    card.append(note);

    let kind = found.kind;
    let value = found.value;
    const kindRow = document.createElement('label');
    kindRow.className = 'behaviour-field';
    const kindLabel = document.createElement('span');
    kindLabel.textContent = 'Holds';
    const kindPick = document.createElement('select');
    for (const [v, l] of [['item', 'an item'], ['gil', 'gil']]) {
      const o = document.createElement('option');
      o.value = v;
      o.textContent = l;
      kindPick.append(o);
    }
    kindPick.value = kind;
    kindRow.append(kindLabel, kindPick);
    card.append(kindRow);

    const itemRow = document.createElement('label');
    itemRow.className = 'behaviour-field';
    const itemLabel = document.createElement('span');
    itemLabel.textContent = 'Item';
    const itemPick = document.createElement('select');
    const fill = () => {
      itemPick.textContent = '';
      for (const entry of state.items || []) {
        const o = document.createElement('option');
        o.value = entry.id;
        o.textContent = `${entry.name} · ${entry.category}`;
        itemPick.append(o);
      }
      if (kind === 'item' && ![...itemPick.options].some(o => Number(o.value) === value)) {
        const odd = document.createElement('option');
        odd.value = value;
        odd.textContent = `item ${value}`;
        itemPick.append(odd);
      }
      itemPick.value = String(kind === 'item' ? value : (itemPick.options[0] && itemPick.options[0].value) || 0);
    };
    if (state.items) fill();
    else api('/api/items').then(items => { state.items = items; fill(); }).catch(() => {});
    itemRow.append(itemLabel, itemPick);
    card.append(itemRow);

    const gilRow = document.createElement('label');
    gilRow.className = 'behaviour-field';
    const gilLabel = document.createElement('span');
    gilLabel.textContent = 'Gil';
    const gil = document.createElement('input');
    gil.type = 'number';
    gil.value = kind === 'gil' ? value : 100;
    gilRow.append(gilLabel, gil);
    card.append(gilRow);

    const show = () => { itemRow.hidden = kind !== 'item'; gilRow.hidden = kind !== 'gil'; };
    kindPick.onchange = () => { kind = kindPick.value; show(); };
    show();

    const save = document.createElement('button');
    save.textContent = 'Save treasure';
    save.onclick = async () => {
      save.disabled = true;
      value = kind === 'item' ? parseInt(itemPick.value, 10) || 0 : parseInt(gil.value, 10) || 0;
      try {
        const fresh = await mapScriptSource();
        const result = await api('/api/script/save', { name: `files/${mapState.name}.script`, source: withTreasure(fresh, character.cast, kind, value), save: true });
        if (!result.ok) throw new Error((result.problems || []).map(p => `line ${p.line}: ${p.message}`).join('; ') || 'the script did not compile');
        markOverridden(`files/${mapState.name}.script`, true);
        say(`cast ${character.cast} now holds ${kind === 'item' ? ((state.items || []).find(i => i.id === value) || {}).name || ('item ' + value) : value + ' gil'}`, 'good');
        drawInspector();
      } catch (error) {
        say(error.message, 'bad');
        save.disabled = false;
      }
    };
    card.append(save);
  }).catch(() => {});
}

// ------------------------------------------------- converting to an OpenFF object
//
// The game's character stays in the .hich and the script (other things there name it by
// cast), so converting is two things in the mod: an object of the mod's own at the same
// spot with the same model - a Chest with the same contents when it was a chest - and a
// Removed behaviour on the original, which takes the game's one off the map when it is
// entered. Everything about it is the mod's from then on.

function addConvert(panel, character) {
  const head = document.createElement('h3');
  head.textContent = 'OpenFF';
  panel.append(head);
  const replaced = (sceneState.attachments || []).some(a => (a.target || '').toLowerCase() === 'object:' + character.index && a.behaviour === 'Removed');
  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = replaced
    ? 'Replaced: a Removed behaviour on it takes it off the map in the OpenFF client; the mod\'s own object stands there instead.'
    : 'Make this the mod\'s own: an OpenFF object at the same spot with the same model and idle, the game\'s one taken off the map. Exact keeps its cast - talking to it runs the same script, 1:1; with components a Chest or Talk stands in for a chest or a plain talker, editable in the inspector.';
  panel.append(note);
  if (!replaced) {
    const go = document.createElement('button');
    go.className = 'wide-button';
    go.textContent = 'Convert to OpenFF object (exact)';
    go.onclick = () => convertToSceneObject(activeDoc, character).catch(error => say(error.message, 'bad'));
    panel.append(go);
    const parts = document.createElement('button');
    parts.className = 'wide-button';
    parts.textContent = 'Convert with components (Chest / Talk)';
    parts.onclick = () => convertToSceneObject(activeDoc, character, { components: true }).catch(error => say(error.message, 'bad'));
    panel.append(parts);
  }
}

/// The server's reading of what each of the map's characters would be as the mod's own
/// (MapConvert): chest, talk (one Talk per flag branch), prop, or unknown with the reason.
async function convertPlan() {
  const plan = await api(`/api/map/convert-plan?name=${encodeURIComponent(mapState.name)}`);
  if (!plan.ok) throw new Error(plan.error);
  return plan.casts || [];
}

/// Applies one cast's plan: the object at the character's spot with its model, the
/// components, and Removed on the original. Two ways: exact (the default) puts GameCast on
/// the stand-in, so it runs the game's own cast and behaves as the original did in every
/// respect - talk, branches, menus, items; components (options.components) puts the plan's
/// Chest / Talk / Wander on it instead, for a chest or a talker the mod means to edit.
/// Either way the boot's idle motion, random walk, recolour and flag-guard come along.
/// Returns the object, or null when the plan is unknown (a scene's actor, an unbooted row).
function applyCastPlan(doc, plan, options = {}) {
  if (!plan || plan.kind === 'unknown') return null;
  const already = (sceneState.attachments || []).some(a => (a.target || '').toLowerCase() === 'object:' + plan.index && a.behaviour === 'Removed');
  if (already && !options.again) return null;
  const components = Boolean(options.components) && ['chest', 'talk', 'prop'].includes(plan.kind);
  const model = plan.model || null;
  const object = addSceneObject(doc, {
    name: `${plan.model || 'object'} ${plan.cast}`,
    model,
    at: [plan.x, plan.y, plan.z]
  });
  // A .hich facing is negated by the game; the mod's yaw is the engine's own.
  object.rotationY = -(plan.rotationY || 0);
  // Spawned as a character (the walker; a chest model as the game's map object) so the cast
  // and its motions have what they had.
  object.character = Boolean(model) && (plan.character || !components || plan.kind === 'chest');
  // Booted with bootPlainCharacter: the light walker with the model's own scale and kind.
  object.plain = object.character && Boolean(plan.plain);
  const path = scenePathOf(object);
  if (!components) {
    // Exact: the cast, and every command the boot ran on it, replayed as the script's
    // own (Setup) - treasure, motions, radii, sign effects, recolours, the random walk.
    // Nothing is translated into a component, so nothing is approximated.
    sceneState.attachments.push({ target: path, behaviour: 'GameCast', fields: { Cast: plan.cast, OnBoot: plan.kind === 'actor', Setup: plan.setup || [] } });
    // Scripts: the cast's main as the mod's own code (CastScript) - the same lines, run by
    // the engine on the game's interpreter, editable in the inspector. A cast whose main
    // reaches outside itself (a call into the map's script, a jump to another function's
    // label) keeps the game's, with the reason in the plan.
    if (options.scripts && plan.kind !== 'actor' && plan.kind !== 'chest' && plan.main && plan.main.length && !plan.mainProblem) {
      sceneState.attachments.push({ target: path, behaviour: 'CastScript', fields: { Cast: plan.cast, Main: plan.main } });
    }
    if (plan.when && plan.kind !== 'actor') {
      // Booted only under flags: the flags count once, at the map's start, as the boot's test did.
      sceneState.attachments.push({ target: path, behaviour: 'WhenFlags', fields: { When: plan.when, Live: false } });
    }
    if (!already) sceneState.attachments.push({ target: 'object:' + plan.index, behaviour: 'Removed', fields: { StandIn: path } });
    return object;
  }
  if (plan.kind === 'chest') {
    sceneState.attachments.push({
      target: path, behaviour: 'Chest',
      fields: Object.assign(plan.treasure === 'item' ? { Item: plan.treasureValue, Count: 1, Gil: 0 } : { Item: 0, Gil: plan.treasureValue },
        { Flag: plan.treasureFlag || '', Animate: true })
    });
  } else if (plan.kind === 'talk') {
    for (const talk of plan.talks || []) {
      sceneState.attachments.push({ target: path, behaviour: 'Talk', fields: { Speaker: '', Lines: talk.lines || [], FaceHero: true, When: talk.when || '', Then: talk.then || '' } });
    }
  }
  if (plan.motionSet || plan.motionIndex) {
    // The idle the boot gives it: the same set and motion.
    sceneState.attachments.push({ target: path, behaviour: 'Motion', fields: { Set: plan.motionSet || '', Index: plan.motionIndex || 1001, Loop: plan.motionLoop !== false } });
  }
  if (plan.wander) {
    // The walk's gait as the boot named it (moveCharacter_StartRandom's second operand).
    const gaits = ['Default', 'Man', 'Woman', 'Boy', 'Girl', 'Uncle', 'Aunt', 'OldMan', 'OldWoman'];
    sceneState.attachments.push({ target: path, behaviour: 'Wander', fields: { Ai: 'Wander', Gait: gaits[plan.wanderGait] || 'Default' } });
  }
  // Booted only under flags: there only while they hold. A Chest keeps its own flag (the
  // open lid) rather than going away, so that one is left out of its When.
  const when = (plan.when || '').split(' ').filter(w => w && !(plan.kind === 'chest' && w === '!' + plan.treasureFlag)).join(' ');
  if (when) {
    sceneState.attachments.push({ target: path, behaviour: 'WhenFlags', fields: { When: when, Live: false } });
  }
  if (!already) sceneState.attachments.push({ target: 'object:' + plan.index, behaviour: 'Removed', fields: { StandIn: path } });
  return object;
}

async function convertToSceneObject(doc, character, options = {}) {
  await loadSceneState(mapState.name);
  const plans = await convertPlan().catch(() => []);
  const plan = plans.find(p => p.index === character.index) || {
    index: character.index, cast: character.cast, model: character.model, x: character.x, y: character.y, z: character.z, rotationY: character.rotationY,
    kind: 'script', character: !/^[ow]/i.test(character.model || '')
  };
  if (plan.kind === 'unknown') {
    // A scene's actor or an unbooted row: converting one would put it there all along.
    // Taken across as its cast all the same when asked outright, with the reason said.
    say(`${plan.reason} - taken across all the same; check it in play`, 'bad');
    plan.kind = 'script';
  }
  if (options.components && !['chest', 'talk', 'prop'].includes(plan.kind)) {
    say(`cast ${character.cast} uses ${(plan.commands || []).slice(0, 5).join(', ')} - no component does that; it keeps its own cast (GameCast)`, 'bad');
  }
  const object = applyCastPlan(doc, plan, { again: true, components: options.components });
  if (!object) return;
  sceneChanged('convert ' + object.name);
  syncSceneObjects(doc);
  drawHierarchy();
  drawInspector();
  const exact = !(options.components && ['chest', 'talk', 'prop'].includes(plan.kind));
  const what = exact
    ? plan.kind === 'actor'
      ? ` - a scene's actor: it appears when the scene boots cast ${character.cast}, where the scene puts it, and runs the cast itself (GameCast), 1:1`
      : ` - it runs the game's cast ${character.cast} itself (GameCast), 1:1`
    : plan.kind === 'chest' ? ' - a Chest with the same contents' : plan.kind === 'talk' ? ` - ${(plan.talks || []).length} Talk(s) with its lines` : ' - a bare model';
  say(`${object.name} is the mod's now${what}; the game's own is taken off the map in the client`, 'good');
}

/// Every character of the map the boot places, in one go, each running its own cast
/// (exact); the ones a scene places or nothing boots stay the game's and are listed.
async function convertMap(doc, options = {}) {
  await loadSceneState(mapState.name);
  let plans;
  try {
    plans = await convertPlan();
  } catch (error) {
    say(error.message, 'bad');
    return;
  }
  const done = [];
  const skipped = [];
  const left = [];
  for (const plan of plans) {
    const already = (sceneState.attachments || []).some(a => (a.target || '').toLowerCase() === 'object:' + plan.index && a.behaviour === 'Removed');
    if (already) { skipped.push(plan); continue; }
    if (plan.kind === 'unknown') { left.push(plan); continue; }
    const object = applyCastPlan(doc, plan, { components: options.components, scripts: options.scripts });
    if (object) done.push({ plan, object });
  }
  if (done.length) sceneChanged('convert map');
  syncSceneObjects(doc);
  drawHierarchy();
  drawInspector();
  say(`${done.length} of ${plans.length} character(s) are the mod's now${left.length ? `; ${left.length} stay the game's` : ''}${skipped.length ? `; ${skipped.length} were already converted` : ''}`, left.length ? undefined : 'good');

  if (typeof dialog !== 'function') return;
  const body = dialog(`Convert ${mapState.name} to OpenFF objects`);
  const summary = document.createElement('p');
  summary.className = 'dialog-note';
  const kinds = ['chest', 'talk', 'prop', 'script', 'actor'].map(k => [k, done.filter(d => d.plan.kind === k).length]).filter(([, n]) => n);
  const kindName = { talk: 'plain talkers', chest: 'chests', prop: 'props', script: 'with scripted casts', actor: "scenes' actors (they appear when their scene boots them)" };
  summary.textContent = `${done.length} converted`
    + (options.components
      ? ' into components: ' + kinds.map(([k, n]) => `${n} ${k === 'talk' ? 'talking (Talk)' : k === 'chest' ? 'chests (Chest)' : k === 'prop' ? 'props' : k === 'actor' ? kindName.actor + ' (GameCast)' : 'with their own cast (GameCast - no component does what they do)'}`).join(', ')
      : ', each running its own cast (GameCast) - talk, branches, menus, chests, scenes exactly as before; the analysis: ' + kinds.map(([k, n]) => `${n} ${kindName[k]}`).join(', '))
    + `${done.filter(d => d.plan.wander).length ? `; ${done.filter(d => d.plan.wander).length} wander` : ''}. `
    + 'Each is an object of the mod\'s own with the same model, place, idle motion and recolour; the original is taken off the map in the client (Removed). Everything saved.';
  body.append(summary);
  if (done.length && typeof undoLast === 'function') {
    // The whole conversion is one undo step; the button is that step, in plain sight.
    const undo = document.createElement('button');
    undo.className = 'wide-button';
    undo.textContent = `Undo this conversion (${done.length} object${done.length === 1 ? '' : 's'} back to the game's)`;
    undo.onclick = () => { undoLast(); const open = document.querySelector('.picker.dialog .shut'); if (open) open.click(); };
    body.append(undo);
    const tip = document.createElement('p');
    tip.className = 'dialog-note';
    tip.textContent = 'Later, one at a time: right-click a replaced character (tick "replaced" on the Characters header to list them) ▸ Restore the game\'s character.';
    body.append(tip);
  }
  if (!options.components && done.some(d => ['chest', 'talk', 'prop'].includes(d.plan.kind))) {
    const tip = document.createElement('p');
    tip.className = 'dialog-note';
    tip.textContent = 'To edit what a chest holds or a villager says in the inspector, convert with components instead (right-click Characters ▸ Convert with components): a Chest or Talk stands in for the cast, exact for those kinds too.';
    body.append(tip);
  }
  if (left.length) {
    const head = document.createElement('div');
    head.className = 'dialog-section';
    head.textContent = `Still the game's (${left.length})`;
    body.append(head);
    const list = document.createElement('div');
    list.className = 'dialog-list';
    for (const plan of left) {
      const row = document.createElement('div');
      row.className = 'dialog-row';
      const title = document.createElement('strong');
      title.textContent = `${plan.model} (cast ${plan.cast})`;
      const why = document.createElement('span');
      why.textContent = plan.reason || (plan.commands || []).join(', ');
      row.append(title, why);
      list.append(row);
    }
    body.append(list);
    const note = document.createElement('p');
    note.className = 'dialog-note';
    note.textContent = 'Nothing in the script places these rows, so there is nothing for an object to stand in for; they stay as they are.';
    body.append(note);
  }
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
      model: o.model || null, character: Boolean(o.character), plain: Boolean(o.plain), tags: o.tags || [], children: []
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
    if (o.model && o.character) out.character = true;
    if (o.model && o.character && o.plain) out.plain = true;
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
      // A model file of the mod's own (assets/hut.glb) is its own package; the view draws it through the same route.
      package: object.model ? (/\.(glb|gltf)$/i.test(object.model) ? object.model : `files/${object.model}.nmdp.lz`) : null,
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
  const move = s => {
    const t = (s || '').toLowerCase();
    if (t === from) return newPath;
    if (t.startsWith(from + '/')) return newPath + s.slice(oldPath.length);
    return null;
  };
  for (const a of (sceneState.attachments || [])) {
    const t = (a.target || '').toLowerCase();
    const moved = move(a.target);
    if (moved) a.target = moved;
    else if (t === 'point:' + from) a.target = newPath;
    // Fields that name the object (ObjectRef) follow too.
    const type = (sceneState.catalog && sceneState.catalog.behaviours || []).find(b => b.name === a.behaviour);
    for (const f of (type && type.fields || [])) {
      if (f.type !== 'object' || !a.fields || typeof a.fields[f.name] !== 'string') continue;
      const m = move(a.fields[f.name]);
      if (m) a.fields[f.name] = m;
    }
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
    // The loaded state is where undo counts from.
    sceneLastSnapshot = sceneSnapshot();
    sceneOpenStep = null;
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
  const others = state.attachments.length - mine.length;
  if (others > 0) {
    const note = document.createElement('p');
    note.className = 'none';
    note.textContent = `${others} more on other objects of this map`;
    box.append(note);
  }
}

// The scene file saves itself: a change (a field typed, an object moved, a behaviour
// added) is written a moment later, the way the session is - nobody should have to
// remember a Save button for every panel. The write says so in the status line only, and
// redraws nothing, since a redraw under a half-typed field would take the field away.
let sceneSaveTimer = null;
function sceneChanged(label) {
  sceneState.dirty = true;
  recordSceneStep(label);
  clearTimeout(sceneSaveTimer);
  sceneSaveTimer = setTimeout(() => {
    sceneSaveTimer = null;
    saveScene({ quiet: true }).catch(() => {});
  }, 700);
}

// Undo for the scene: every change is a step on the map document's stack (Ctrl+Z / Ctrl+Y
// with the rest of the map's edits), as a snapshot of the objects and attachments before
// and after. Changes within a second of each other fold into one step, so a number typed
// digit by digit or a gizmo drag is one undo, not twenty. Autosave without undo would be
// the wrong combination.
let sceneLastSnapshot = null;
let sceneLastStepAt = 0;
let sceneOpenStep = null;

function sceneSnapshot() {
  return JSON.stringify({ objects: objectsForFile(sceneState.objects || []), attachments: sceneState.attachments || [] });
}

function restoreSceneSnapshot(snapshot) {
  const data = JSON.parse(snapshot);
  sceneState.objects = fromSceneFile(data.objects);
  sceneState.attachments = data.attachments;
  sceneLastSnapshot = snapshot;
  sceneOpenStep = null;
  const doc = activeDoc;
  if (doc && doc.selection && doc.selection.startsWith('scene:') && !findSceneObject(doc.selection.slice(6))) doc.selection = null;
  syncSceneObjects(doc);
  if (doc && doc.scene3d && doc.selection && doc.selection.startsWith('scene:')) {
    const index = flattenSceneObjects(sceneState).findIndex(i => 'scene:' + i.path === doc.selection);
    doc.scene3d.selectPoint(index >= 0 ? index : null);
  }
  drawHierarchy();
  drawInspector();
  sceneState.dirty = true;
  clearTimeout(sceneSaveTimer);
  sceneSaveTimer = setTimeout(() => { sceneSaveTimer = null; saveScene({ quiet: true }).catch(() => {}); }, 700);
}

function recordSceneStep(label) {
  const doc = activeDoc;
  if (!doc || doc.kind !== 'map') return;
  const now = sceneSnapshot();
  if (sceneLastSnapshot === null) { sceneLastSnapshot = now; return; }
  if (now === sceneLastSnapshot) return;
  const at = Date.now();
  if (sceneOpenStep && at - sceneLastStepAt < 1000) {
    // The same spell of editing: the step's "after" moves on, its "before" stays.
    sceneOpenStep.after = now;
  } else {
    const step = { before: sceneLastSnapshot, after: now };
    sceneOpenStep = step;
    pushUndo(doc, label || 'scene change', () => restoreSceneSnapshot(step.before), () => restoreSceneSnapshot(step.after));
  }
  sceneLastStepAt = at;
  sceneLastSnapshot = now;
}

/// Every behaviour the picker can offer: the built ones with their fields, and the ones
/// the source declares but no build has seen yet (marked, so the fields' absence is
/// explained). A name in both is one entry.
function behaviourChoices(catalog) {
  const choices = (catalog.behaviours || []).map(b => ({
    name: b.name, fullName: b.fullName, summary: b.summary, fields: b.fields || [], built: true,
    engine: /^OpenFF\.Engine/i.test(b.assembly || ''),
    file: (catalog.sources || []).find(s => s.kind === 'behaviour' && s.name === b.name)?.file || null,
  }));
  for (const s of (catalog.sources || [])) {
    if (s.kind !== 'behaviour' || choices.some(c => c.name === s.name)) continue;
    choices.push({ name: s.name, fullName: s.name, summary: null, fields: [], built: false, engine: false, file: s.file, line: s.line });
  }
  // The engine's own first (Chest, Talk, Trigger), then the mod's, each by name.
  choices.sort((a, b) => (a.engine === b.engine ? 0 : a.engine ? -1 : 1) || a.name.localeCompare(b.name));
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
  sceneChanged('add ' + choice.name);
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
    // The engine's built-in components under their own heading, the mod's under theirs.
    let lastGroup = null;
    for (const choice of shown) {
      const group = choice.engine ? 'Built in' : (catalog.assemblies && catalog.assemblies[0] ? catalog.assemblies[0].replace(/\.dll$/i, '') : 'This mod');
      if (group !== lastGroup && shown.some(c => Boolean(c.engine) !== Boolean(shown[0].engine))) {
        const h = document.createElement('li');
        h.className = 'dropdown-group';
        h.textContent = group;
        list.append(h);
        lastGroup = group;
      }
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
    say(`${made.name} written and attached to this ${what} - Build to give it fields`, 'good');
    drawBehaviours(box, sceneState, target, what);
    if (typeof projectChanged === 'function') projectChanged();
    if (typeof openDoc === 'function') await openDoc('code', made.name);
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// "EmptyMessage" as "Empty Message": a field's name the way Unity labels it.
function spaceName(name) {
  return String(name || '').replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/_/g, ' ');
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
    sceneChanged('remove ' + attachment.behaviour);
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
    // [Header] over a group of fields, as Unity draws it.
    if (f.header) {
      const h = document.createElement('div');
      h.className = 'behaviour-header';
      h.textContent = f.header;
      card.append(h);
    }
    const row = document.createElement('label');
    row.className = 'behaviour-field';
    row.title = (f.tooltip || f.summary || '') + (f.tooltip || f.summary ? ' ' : '') + '(' + f.typeName + ')';
    const label = document.createElement('span');
    label.textContent = spaceName(f.name);
    row.append(label);
    const has = Object.prototype.hasOwnProperty.call(attachment.fields, f.name);
    const value = has ? attachment.fields[f.name] : f.default;
    let input;
    const changed = v => { attachment.fields[f.name] = v; sceneChanged(); };
    if (f.type === 'item') {
      // An item id: the game's item list to pick from, fetched once for the session.
      input = document.createElement('select');
      const none = document.createElement('option');
      none.value = '0';
      none.textContent = '(none)';
      input.append(none);
      const fill = () => {
        for (const entry of state.items || []) {
          const option = document.createElement('option');
          option.value = entry.id;
          option.textContent = `${entry.name} · ${entry.category}`;
          input.append(option);
        }
        input.value = String(value || 0);
      };
      if (state.items) fill();
      else api('/api/items').then(items => { state.items = items; fill(); }).catch(() => {});
      input.onchange = () => changed(parseInt(input.value, 10) || 0);
    } else if (f.type === 'formation') {
      // [FormationField]: the game's monster parties and the mod's formations to pick from.
      input = document.createElement('select');
      const none = document.createElement('option');
      none.value = '0';
      none.textContent = '(none)';
      input.append(none);
      const fill = () => {
        const game = document.createElement('optgroup');
        game.label = 'the game\'s parties';
        const mod = document.createElement('optgroup');
        mod.label = 'the mod\'s formations';
        for (const entry of state.formations || []) {
          const option = document.createElement('option');
          option.value = entry.id;
          option.textContent = `${entry.id} · ${entry.name}`;
          (entry.mod ? mod : game).append(option);
        }
        if (mod.childElementCount) input.append(mod);
        input.append(game);
        input.value = String(value || 0);
      };
      if (state.formations) fill();
      else api('/api/formations').then(list => { state.formations = list; fill(); }).catch(() => {});
      input.onchange = () => changed(parseInt(input.value, 10) || 0);
    } else if (f.type === 'object') {
      // A reference to another of the mod's objects on this map, by path.
      input = document.createElement('select');
      const none = document.createElement('option');
      none.value = '';
      none.textContent = '(none)';
      input.append(none);
      for (const item of flattenSceneObjects(sceneState)) {
        if (item.path.toLowerCase() === (target || '').toLowerCase()) continue;
        const option = document.createElement('option');
        option.value = item.path;
        option.textContent = ' '.repeat(item.depth * 2) + item.path + (item.model ? '  (' + item.model + ')' : '');
        input.append(option);
      }
      const current = value && typeof value === 'object' ? (value.path || value.Path || '') : (value || '');
      if (current && ![...input.options].some(o => o.value === current)) {
        const gone = document.createElement('option');
        gone.value = current;
        gone.textContent = current + '  (not on this map)';
        input.append(gone);
      }
      input.value = current;
      input.onchange = () => changed(input.value);
    } else if (f.type === 'flags') {
      // A flag expression ("0:14 !0:11", alternatives with |): typed, but with the map's own
      // flags to pick from - the ones its script tests and sets, with who does - and the
      // shape checked as it is typed.
      input = document.createElement('div');
      input.className = 'behaviour-flags';
      const text = document.createElement('input');
      text.type = 'text';
      text.value = value == null ? '' : value;
      text.placeholder = 'e.g. !0:14  or  0:14 !0:11 | 1:22';
      text.spellcheck = false;
      const check = () => {
        const bad = text.value.split(/[\s|,]+/).filter(Boolean).find(p => !/^!?\d+:\d+$/.test(p));
        text.classList.toggle('bad', Boolean(bad));
        text.title = bad ? `"${bad}" is not a flag - group:index, ! for off, | between alternatives` : (f.tooltip || '');
      };
      text.oninput = () => { check(); changed(text.value); };
      check();
      const pick = document.createElement('select');
      pick.title = 'Add one of the flags this map\'s script uses';
      const first = document.createElement('option');
      first.value = '';
      first.textContent = '+ flag…';
      pick.append(first);
      const fillFlags = list => {
        for (const u of list) {
          const who = [];
          if (u.chest >= 0) who.push(`chest ${u.chest}`);
          if (u.setBy.length) who.push('set by cast ' + u.setBy.join(', '));
          if (u.testedBy.length) who.push('tested by cast ' + u.testedBy.join(', '));
          for (const sense of ['', '!']) {
            const option = document.createElement('option');
            option.value = sense + u.flag;
            option.textContent = `${sense}${u.flag}  ·  ${sense ? 'off' : 'on'}${who.length ? ' · ' + who.join('; ') : ''}`;
            pick.append(option);
          }
        }
      };
      if (state.mapFlags && state.mapFlags.map === mapState.name) fillFlags(state.mapFlags.list);
      else api(`/api/map/flags?name=${encodeURIComponent(mapState.name)}`).then(r => { state.mapFlags = { map: mapState.name, list: r.flags || [] }; fillFlags(state.mapFlags.list); }).catch(() => {});
      pick.onchange = () => {
        if (!pick.value) return;
        text.value = (text.value.trim() ? text.value.trim() + ' ' : '') + pick.value;
        pick.value = '';
        check();
        changed(text.value);
      };
      input.append(text, pick);
    } else if (f.type === 'strings' && attachment.behaviour === 'CastScript' && f.name === 'Main' && typeof openCastCode === 'function') {
      // The cast's code: a short preview here, the script editor - completion, the
      // signature strip, a compile check - in a tab of its own.
      input = document.createElement('div');
      input.className = 'cast-code-preview';
      const lines = Array.isArray(value) ? value : [];
      const pre = document.createElement('pre');
      const shown = lines.filter(l => (l || '').trim().length).slice(0, 5);
      pre.textContent = shown.join('\n') + (lines.length > shown.length ? `\n… ${lines.length} lines` : '');
      if (!lines.length) pre.textContent = '(no code yet)';
      pre.title = 'The main function, one command per line; opens in the code editor';
      const edit = document.createElement('button');
      edit.className = 'primary';
      edit.textContent = lines.length ? 'Edit code…' : 'Write code…';
      edit.onclick = () => openCastCode(state, attachment, target);
      pre.onclick = edit.onclick;
      input.append(pre, edit);
    } else if (f.type === 'strings') {
      // A list of strings: one per line.
      input = document.createElement('textarea');
      input.rows = Math.min(6, Math.max(2, (value || []).length + 1));
      input.value = (value || []).join('\n');
      input.oninput = () => changed(input.value.split('\n').filter((l, i, a) => l.length || i < a.length - 1));
    } else if ((f.type === 'int' || f.type === 'float') && f.min != null && f.max != null) {
      // [Range]: a slider with the number beside it.
      input = document.createElement('div');
      input.className = 'behaviour-range';
      const slider = document.createElement('input');
      slider.type = 'range';
      slider.min = f.min;
      slider.max = f.max;
      slider.step = f.type === 'int' ? '1' : String((f.max - f.min) / 100);
      const number = document.createElement('input');
      number.type = 'number';
      number.min = f.min;
      number.max = f.max;
      number.step = f.type === 'int' ? '1' : 'any';
      slider.value = number.value = value == null ? f.min : value;
      slider.oninput = () => { number.value = slider.value; changed(Number(slider.value)); };
      number.oninput = () => { slider.value = number.value; changed(Number(number.value) || 0); };
      input.append(slider, number);
    } else if (f.type === 'bool') {
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
    if ((f.type === 'string' || f.type === 'strings') && 'value' in input) msdHint(card, input, () => f.type === 'strings' ? input.value.split('\n') : [input.value]);
  }
  if (attachment.behaviour === 'GameCast') gameCastNotes(card, state, attachment, target);
  return card;
}

/// Under a text field: what the game's .msd says for a value of the form "@1000142" (one
/// of the game's messages by id, said through the game's own message system, in every
/// language), so the id is not a number in a box. "@" alone on a Chest is its own message
/// for the contents. Refreshed as the field is typed in; nothing for plain text.
const msdHintCache = {};
function msdHint(card, input, values) {
  const hint = document.createElement('p');
  hint.className = 'none msd-hint';
  hint.hidden = true;
  card.append(hint);
  let timer = null;
  const refresh = async () => {
    const ids = [];
    for (const v of values()) {
      const m = /^\s*@(\d+)/.exec(v || '');
      if (m) ids.push(m[1]);
    }
    const plainAt = values().some(v => /^\s*@\s*$/.test(v || ''));
    if (!ids.length && !plainAt) { hint.hidden = true; return; }
    const missing = ids.filter(id => !(id in msdHintCache));
    if (missing.length) {
      try {
        const r = await api('/api/messages', { ids: missing.map(Number) });
        for (const id of missing) msdHintCache[id] = (r.messages || {})[id] || null;
      } catch (e) { for (const id of missing) msdHintCache[id] = null; }
    }
    const parts = ids.map(id => `@${id}: ${msdHintCache[id] == null ? 'no such message in the game\'s text' : '"' + msdHintCache[id].replace(/\s+/g, ' ').trim() + '"'}`);
    if (plainAt) parts.unshift('@: the game\'s own message for the contents ("The chest contained Potion."), in the player\'s language');
    hint.textContent = parts.join('  ·  ');
    hint.hidden = false;
  };
  input.addEventListener('input', () => { clearTimeout(timer); timer = setTimeout(refresh, 250); });
  refresh();
}

/// Under a GameCast's fields: what the cast does, read from the map's script - its lines
/// as the text file has them, the flags they hang on, the commands beyond talking - so the
/// stand-in is not a number in a box. The lines are the game's, not fields: the cast's
/// code says them. "Make it editable" swaps the GameCast for the analysis's Chest / Talk
/// components, where a chest or a talker can be one; "Open script" goes to the code.
function gameCastNotes(card, state, attachment, target) {
  const cast = parseInt(attachment.fields.Cast, 10) || 0;
  if (!cast || !mapState.data) return;
  const character = (mapState.data.characters || []).find(c => c.cast === cast && !/^logic$/i.test(c.kindName || ''));
  const box = document.createElement('div');
  box.className = 'cast-notes';
  const head = document.createElement('div');
  head.className = 'behaviour-header';
  head.textContent = `What cast ${cast} does (the game's script)`;
  box.append(head);
  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'The object runs this cast as the game does: talk to it and its code runs, with these lines and whatever else it does. To change the words or the contents, make it editable - a Talk or Chest of the mod\'s own stands in.';
  box.append(note);
  const lines = character ? (character.lines || []) : [];
  if (lines.length) {
    const list = document.createElement('div');
    list.className = 'cast-lines';
    for (const line of lines.slice(0, 12)) {
      const q = document.createElement('div');
      q.className = 'cast-line';
      q.textContent = line;
      list.append(q);
    }
    if (lines.length > 12) {
      const more = document.createElement('div');
      more.className = 'cast-line none';
      more.textContent = `… and ${lines.length - 12} more`;
      list.append(more);
    }
    box.append(list);
  } else {
    const none = document.createElement('p');
    none.className = 'none';
    none.textContent = character && character.hasScript ? 'No message lines - its code does other things (see the script).' : 'No code of its own.';
    box.append(none);
  }
  const actions = document.createElement('div');
  actions.className = 'cast-actions';
  const open = document.createElement('button');
  open.textContent = character && character.hasScript ? `Open script at cast ${cast}` : 'Open script';
  open.onclick = async () => {
    if (typeof openDoc !== 'function') return;
    const opened = await openDoc('script', `files/${mapState.name}.script`);
    const text = opened && opened.pane.querySelector('textarea');
    if (!text) return;
    const at = text.value.indexOf(`cast${cast}_main:`);
    if (at >= 0 && typeof goToLine === 'function') goToLine(text, text.value.slice(0, at).split('\n').length, 1);
  };
  actions.append(open);
  const editable = document.createElement('button');
  editable.textContent = 'Make it editable (Talk / Chest)';
  editable.title = 'Replace the GameCast with the components the script analysis finds - a Chest with the same contents, a Talk per branch with its lines';
  editable.onclick = () => makeCastEditable(state, attachment, target, cast).catch(error => say(error.message, 'bad'));
  actions.append(editable);
  box.append(actions);
  card.append(box);
}

/// Swaps a stand-in's GameCast for the analysis's components: Chest or Talk(s), as
/// "Convert with components" would have made them. A cast beyond those stays a GameCast.
async function makeCastEditable(state, attachment, target, cast) {
  const plans = await convertPlan();
  const plan = plans.find(p => p.cast === cast);
  if (!plan || !['chest', 'talk'].includes(plan.kind)) {
    say(plan ? `cast ${cast} uses ${(plan.commands || []).slice(0, 5).join(', ')} - no component does that; it stays a GameCast` : `no analysis for cast ${cast}`, 'bad');
    return;
  }
  const at = state.attachments.indexOf(attachment);
  const replacements = [];
  if (plan.kind === 'chest') {
    replacements.push({
      target, behaviour: 'Chest',
      fields: Object.assign(plan.treasure === 'item' ? { Item: plan.treasureValue, Count: 1, Gil: 0 } : { Item: 0, Gil: plan.treasureValue },
        { Flag: plan.treasureFlag || '', Animate: true })
    });
  } else {
    for (const talk of plan.talks || []) {
      replacements.push({ target, behaviour: 'Talk', fields: { Speaker: '', Lines: talk.lines || [], FaceHero: true, When: talk.when || '', Then: talk.then || '' } });
    }
  }
  state.attachments.splice(at, 1, ...replacements);
  sceneChanged('make editable ' + target);
  drawInspector();
  say(`${target}: ${replacements.length} ${plan.kind === 'chest' ? 'Chest' : 'Talk'} component(s) stand in for cast ${cast} now - its words and contents are fields`, 'good');
}

/// Writes the scene file as it stands (objects and attachments). Quiet is the autosave:
/// the status line only, no redraw.
async function saveScene(options = {}) {
  const state = sceneState;
  if (!state.map) return;
  try {
    const result = await api('/api/project/scene/save', { map: state.map, attachments: state.attachments || [], objects: objectsForFile(state.objects) });
    if (!result.ok) throw new Error(result.error);
    state.dirty = false;
    say(`scenes/${state.map}.json saved (${flattenSceneObjects(state).length} object(s), ${result.count} attachment(s))`, 'good');
    if (typeof projectChanged === 'function') projectChanged();
    if (!options.quiet) {
      drawHierarchy();
      drawInspector();
    }
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
  // Beside a given object: the same parent, right after it in the list, at its spot.
  const after = options.after || null;
  const parent = after ? after.parent : (options.parent || null);
  const siblings = parent ? parent.children : state.objects;
  const name = uniqueSceneName(siblings, options.name || (options.model ? options.model : parent && !after ? 'Child' : 'Object'));
  const object = {
    name, x: 0, y: 0, z: 0, rotationY: 0, scale: 1,
    model: options.model || null, character: Boolean(options.character), tags: options.tags || [], children: []
  };
  Object.defineProperty(object, 'parent', { value: parent, writable: true, enumerable: false });
  if (after) {
    siblings.splice(siblings.indexOf(after) + 1, 0, object);
    object.x = after.x;
    object.y = after.y;
    object.z = after.z;
    object.rotationY = after.rotationY || 0;
  } else {
    siblings.push(object);
  }
  if (!parent && !after) {
    const spot = options.at || (doc && doc.scene3d ? (doc.scene3d.viewGround ? doc.scene3d.viewGround() : doc.scene3d.viewCentre()) : [0, 0, 0]);
    object.x = Math.round(spot[0]);
    object.y = Math.round(spot[1]);
    object.z = Math.round(spot[2]);
  }
  sceneChanged('add ' + name);
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
  sceneChanged('delete ' + object.name);
  if (doc) doc.selection = null;
  syncSceneObjects(doc);
  if (doc && doc.scene3d) doc.scene3d.selectPoint(null);
  drawHierarchy();
  drawInspector();
}

/// Undoes one character's conversion: the Removed on the game's row goes, and the stand-in
/// it names (with its behaviours) is deleted, so the game's own character is back in the
/// client. The row reads as before in the hierarchy.
function restoreCharacter(doc, index) {
  const key = 'object:' + index;
  const removals = (sceneState.attachments || []).filter(a => a.behaviour === 'Removed' && (a.target || '').toLowerCase() === key);
  if (!removals.length) return false;
  for (const removal of removals) {
    const standIn = removal.fields && removal.fields.StandIn ? findSceneObject(removal.fields.StandIn) : null;
    sceneState.attachments.splice(sceneState.attachments.indexOf(removal), 1);
    if (standIn && standIn.source) {
      const object = standIn.source;
      const siblings = object.parent ? object.parent.children : sceneState.objects;
      const at = siblings.indexOf(object);
      const gone = flattenSceneObjects(sceneState, [object]).map(i => i.path.toLowerCase());
      if (at >= 0) siblings.splice(at, 1);
      sceneState.attachments = sceneState.attachments.filter(a => !gone.includes((a.target || '').toLowerCase()));
    }
  }
  sceneChanged('restore ' + key);
  if (doc) doc.selection = key;
  syncSceneObjects(doc);
  drawHierarchy();
  drawInspector();
  say(`${key} is the game's again; its stand-in is gone`, 'good');
  return true;
}

/// The right-click menu of an object in the hierarchy: what the inspector used to need
/// buttons for.
function sceneObjectMenu(doc, object) {
  const path = scenePathOf(object);
  return [
    { label: 'New object', icon: 'exit', run: () => addSceneObject(doc, { after: object }) },
    { label: 'New child object', icon: 'exit', run: () => addSceneObject(doc, { parent: object }) },
    { label: 'Duplicate', icon: 'model', run: () => duplicateSceneObject(doc, object) },
    { label: 'Rename', icon: 'code', run: () => {
      doc.selection = 'scene:' + path;
      drawHierarchy();
      drawInspector();
      const input = document.querySelector('#inspector .object-name');
      if (input) { input.focus(); input.select(); }
    } },
    { label: 'Focus in view', icon: 'scene', run: () => {
      const index = flattenSceneObjects(sceneState).findIndex(i => i.source === object);
      if (doc.scene3d && index >= 0) doc.scene3d.focusPoint(index);
    } },
    { sep: true },
    { label: 'Move to top level', icon: 'exit', disabled: !object.parent, run: () => reparentSceneObject(doc, object, null) },
    { sep: true },
    { label: object.children.length ? 'Delete with children' : 'Delete', icon: 'exit', run: () => removeSceneObject(doc, object) },
  ];
}

/// A copy beside the original - the subtree, its attachments (copied), a fresh name.
function duplicateSceneObject(doc, object) {
  const siblings = object.parent ? object.parent.children : sceneState.objects;
  const copy = fromSceneFile(objectsForFile([object]), object.parent)[0];
  copy.name = uniqueSceneName(siblings, object.name);
  siblings.splice(siblings.indexOf(object) + 1, 0, copy);
  // Attachments on the original and under it, targeted at the copy's paths.
  const from = scenePathOf(object).toLowerCase();
  const to = scenePathOf(copy);
  for (const a of [...(sceneState.attachments || [])]) {
    const t = (a.target || '').toLowerCase();
    if (t === from || t.startsWith(from + '/')) {
      sceneState.attachments.push({ target: to + a.target.slice(from.length), behaviour: a.behaviour, fields: JSON.parse(JSON.stringify(a.fields || {})) });
    }
  }
  sceneChanged();
  doc.selection = 'scene:' + to;
  syncSceneObjects(doc);
  const index = flattenSceneObjects(sceneState).findIndex(i => i.source === copy);
  if (doc.scene3d && index >= 0) doc.scene3d.selectPoint(index);
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
  sceneChanged('move ' + object.name);
  if (doc) doc.selection = 'scene:' + scenePathOf(object);
  syncSceneObjects(doc);
  drawHierarchy();
  drawInspector();
}

// ------------------------------------------------------- the inspector's shape
//
// Every object on a map - the game's characters and exits, the terrain, the mod's own -
// is inspected in the same shape, so the hand does not have to relearn the panel between
// a Steam mod and an OpenFF one: an icon and the name at the top, then Transform when the
// thing has a place, then one card per aspect (model, cast, where an exit leads...), the
// behaviours among them. The game's panels were written as headings and fields; cardify
// folds each heading and what follows it into a card, so they need not be rewritten.

/// The top of an inspector: the kind's icon and the name, editable when onRename is given.
function inspectorHead(iconName, name, sub, onRename) {
  const wrap = document.createDocumentFragment();
  const head = document.createElement('div');
  head.className = 'object-head';
  head.append(icon(iconName));
  const box = document.createElement('input');
  box.className = 'object-name';
  box.value = name;
  if (onRename) {
    box.title = 'Free to change';
    box.onchange = () => onRename(box);
  } else {
    box.readOnly = true;
    box.title = 'The game names it; the name is not edited here';
  }
  head.append(box);
  wrap.append(head);
  if (sub) {
    const p = document.createElement('p');
    p.className = 'sub';
    p.textContent = sub;
    wrap.append(p);
  }
  return wrap;
}

/// One card of the inspector: a heading with an icon, an optional note at its right, and
/// whatever is appended to it afterwards.
function componentCard(iconName, title, note) {
  const card = document.createElement('div');
  card.className = 'component';
  const head = document.createElement('div');
  head.className = 'component-head';
  head.append(icon(iconName));
  const name = document.createElement('b');
  name.textContent = title;
  head.append(name);
  if (note) {
    const i = document.createElement('i');
    i.textContent = note;
    head.append(i);
  }
  card.append(head);
  return card;
}

/// The Transform card: Position X Y Z, Rotation Y, and Scale when there is one. `read(key)`
/// gives the current number, `write(key, value)` takes a typed one; focus and blur are for
/// an undo record around a spell of typing.
function transformCard(spec) {
  const card = componentCard('terrain', 'Transform', spec.note);
  const number = (key, step) => {
    const input = document.createElement('input');
    input.type = 'number';
    input.step = step;
    input.value = spec.read(key);
    if (spec.onFocus) input.onfocus = () => spec.onFocus(key);
    if (spec.onBlur) input.onblur = () => spec.onBlur(key);
    input.oninput = () => {
      const value = Number(input.value);
      if (!Number.isNaN(value)) spec.write(key, value);
    };
    return input;
  };
  const row = (label, title, cells) => {
    const r = document.createElement('div');
    r.className = 'transform-row';
    r.title = title || '';
    const l = document.createElement('span');
    l.textContent = label;
    r.append(l);
    const box = document.createElement('div');
    box.className = 'transform-cells';
    for (const [axis, key, step] of cells) {
      const cell = document.createElement('label');
      const a = document.createElement('em');
      a.textContent = axis;
      cell.append(a, number(key, step));
      box.append(cell);
    }
    r.append(box);
    card.append(r);
  };
  const p = spec.position || { x: 'x', y: 'y', z: 'z' };
  row('Position', spec.positionTitle || 'World units', [['X', p.x, '1'], ['Y', p.y, '1'], ['Z', p.z, '1']]);
  if (spec.rotation) row('Rotation', spec.rotationTitle || 'Yaw in degrees', [['Y', spec.rotation, '1']]);
  if (spec.scale) row('Scale', spec.scaleTitle || '1 is the model\'s own size', [['', spec.scale, '0.1']]);
  if (spec.hint) {
    const hint = document.createElement('p');
    hint.className = 'none';
    hint.textContent = spec.hint;
    card.append(hint);
  }
  return card;
}

/// Folds a panel written as headings and fields into cards: each h3 and what follows it,
/// up to the next h3, becomes one component card with an icon guessed from the title.
function cardify(panel) {
  if (!panel || !panel.querySelector) return panel;
  const icons = [
    [/behaviour/i, 'behaviour'], [/model/i, 'model'], [/cast|script|what it says|referred/i, 'logic'],
    [/leads to|doorway|arrive|exit|condition/i, 'exit'], [/children|object/i, 'exit'], [/terrain/i, 'terrain'],
  ];
  let card = null;
  for (const node of [...panel.childNodes]) {
    if (node.nodeType !== 1) { if (card) card.append(node); continue; }
    if (node.tagName === 'H3') {
      const title = node.textContent;
      const iconName = (icons.find(([re]) => re.test(title)) || [null, 'file'])[1];
      card = componentCard(iconName, title);
      panel.insertBefore(card, node);
      node.remove();
      continue;
    }
    if (node.tagName === 'H2' || node.classList.contains('object-head') || node.classList.contains('sub') || node.classList.contains('component')) {
      card = null;
      continue;
    }
    if (card) card.append(node);
  }
  return panel;
}

/// The inspector for one of the mod's objects, laid out as Unity lays out a GameObject:
/// the name, then Transform, then the model, then tags and parent, then the components.
/// Everything here is the file's - no game data behind it - and every change saves itself.
/// Every tag the mod knows, in two kinds. The mod's tags (project.json, Unity's Tags &
/// Layers) are the vocabulary every scene offers whether an object carries them yet or not;
/// the rest are tags found on this map's objects and in the project's other scene files -
/// a map's own words. Fetched once per session; refreshed after Edit tags.
function knownTags() {
  const here = new Set();
  for (const item of flattenSceneObjects(sceneState)) for (const t of item.source.tags || []) here.add(t);
  const used = (state.projectTags || []).map(u => u.tag);
  const mod = new Set((state.modTags || []).map(t => t.toLowerCase()));
  return [...new Set([...used, ...here])].filter(t => !mod.has(t.toLowerCase())).sort((a, b) => a.localeCompare(b));
}

function modTags() {
  return [...(state.modTags || [])].sort((a, b) => a.localeCompare(b));
}

function loadProjectTags() {
  return api('/api/project/tags').then(r => { state.projectTags = r.tags || []; state.modTags = r.global || []; }).catch(() => {});
}

/// The mod's tag list written back (project.json).
function saveModTags(tags) {
  return api('/api/project/tags/global', { tags }).then(r => { state.modTags = r.global || tags; }).catch(e => say('tags: ' + e.message, 'bad'));
}

/// A tag renamed or removed (to = '') across every scene file of the project and the mod's
/// list, on the server; then the same on the map in hand, so what is on screen is what is
/// on disk (another open map reloads its scene when shown).
async function retagEverywhere(from, to) {
  const r = await api('/api/project/tags/retag', { from, to });
  let touched = 0;
  for (const o of flattenSceneObjects(sceneState).map(i => i.source)) {
    if (!o.tags || !o.tags.some(t => t.toLowerCase() === from.toLowerCase())) continue;
    o.tags = [...new Set(o.tags.map(t => t.toLowerCase() === from.toLowerCase() ? to : t).filter(Boolean))];
    touched++;
  }
  state.modTags = r.global || state.modTags;
  return { maps: r.maps || [], here: touched };
}

/// New tag: the name, and whether it is the mod's (every scene's picker offers it) or this
/// object's alone. Resolves to the name, or null.
function newTagDialog(defaultModWide = true) {
  return new Promise(resolve => {
    if (typeof dialog !== 'function') { resolve((prompt('New tag', '') || '').trim().replace(/\s+/g, '-') || null); return; }
    let settled = false;
    const body = dialog('New tag', { onClose: () => { if (!settled) { settled = true; resolve(null); } } });
    const note = document.createElement('p');
    note.className = 'dialog-note';
    note.textContent = 'One word, as a mod asks for it: Game.World.Legacy.WithTag("…"). A mod-wide tag is offered on every map of the mod; one that is not lives only on the objects that carry it.';
    body.append(note);
    const input = document.createElement('input');
    input.type = 'text';
    input.placeholder = 'tag';
    body.append(input);
    const wideRow = document.createElement('label');
    wideRow.className = 'dialog-check';
    const wide = document.createElement('input');
    wide.type = 'checkbox';
    wide.checked = defaultModWide;
    wideRow.append(wide, document.createTextNode(' Mod-wide - offered in every scene of the mod'));
    body.append(wideRow);
    const ok = document.createElement('button');
    ok.className = 'wide-button';
    ok.textContent = 'Add';
    const finish = async () => {
      const name = input.value.trim().replace(/\s+/g, '-');
      if (!name) return;
      settled = true;
      if (wide.checked && !(state.modTags || []).some(t => t.toLowerCase() === name.toLowerCase())) await saveModTags([...(state.modTags || []), name]);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      resolve(name);
    };
    ok.onclick = finish;
    input.onkeydown = e => { if (e.key === 'Enter') finish(); };
    body.append(ok);
    setTimeout(() => input.focus(), 0);
  });
}

/// The Tags row of an object, Unity's way: the tags as chips with an × each, and a picker
/// that adds one of the tags the mod already uses, a new one, or opens Edit tags - where a
/// tag is renamed or removed across every object on the map.
function tagsField(object, changed) {
  const row = document.createElement('div');
  row.className = 'behaviour-field tags-field';
  row.title = 'Words a mod finds it by: Game.World.Legacy.WithTag("chest")';
  const label = document.createElement('span');
  label.textContent = 'Tags';
  const body = document.createElement('div');
  body.className = 'tag-chips';
  const draw = () => {
    body.textContent = '';
    for (const tag of object.tags || []) {
      const chip = document.createElement('span');
      chip.className = 'tag-chip';
      chip.textContent = tag;
      const x = document.createElement('button');
      x.type = 'button';
      x.textContent = '×';
      x.title = 'Take this tag off the object';
      x.onclick = () => { object.tags = (object.tags || []).filter(t => t !== tag); changed(); draw(); };
      chip.append(x);
      body.append(chip);
    }
    const pick = document.createElement('select');
    pick.className = 'tag-pick';
    pick.title = 'Add a tag the mod already uses, a new one, or edit the tags';
    const head = document.createElement('option');
    head.value = '';
    head.textContent = (object.tags || []).length ? '+' : '+ tag…';
    pick.append(head);
    const have = new Set((object.tags || []).map(t => t.toLowerCase()));
    const useOf = tag => (state.projectTags || []).find(u => u.tag.toLowerCase() === tag.toLowerCase());
    const group = (label, tags) => {
      const list = tags.filter(t => !have.has(t.toLowerCase()));
      if (!list.length) return;
      const g = document.createElement('optgroup');
      g.label = label;
      for (const tag of list) {
        const option = document.createElement('option');
        option.value = 'tag:' + tag;
        const use = useOf(tag);
        option.textContent = tag + (use ? `  (${use.count} on ${use.maps.join(', ')})` : '');
        g.append(option);
      }
      pick.append(g);
    };
    group('Mod tags', modTags());
    group('On the maps', knownTags());
    const more = document.createElement('optgroup');
    more.label = '─────';
    const fresh = document.createElement('option');
    fresh.value = 'new';
    fresh.textContent = 'New tag…';
    more.append(fresh);
    const edit = document.createElement('option');
    edit.value = 'edit';
    edit.textContent = 'Edit tags…';
    more.append(edit);
    pick.append(more);
    pick.onchange = async () => {
      const v = pick.value;
      pick.value = '';
      if (v.startsWith('tag:')) { object.tags = [...(object.tags || []), v.slice(4)]; changed(); draw(); }
      else if (v === 'new') {
        const name = await newTagDialog();
        if (!name) return;
        if (!have.has(name.toLowerCase())) { object.tags = [...(object.tags || []), name]; changed(); }
        draw();
      }
      else if (v === 'edit') editTagsDialog(() => { changed(); draw(); });
    };
    body.append(pick);
  };
  if (!state.projectTags) loadProjectTags().then(draw);
  draw();
  row.append(label, body);
  return row;
}

/// Edit tags, as Unity's Tags & Layers is for a project, in two parts. The mod's tags: the
/// list every scene offers - add one, rename or remove one across every scene file of the
/// mod. This map's tags: the words on this map's objects alone - rename or remove across the
/// objects here that carry them, or make one mod-wide.
function editTagsDialog(done) {
  if (typeof dialog !== 'function') return;
  const body = dialog('Edit tags', { wide: true });
  const objects = flattenSceneObjects(sceneState).map(i => i.source);
  const counts = new Map();
  for (const o of objects) for (const t of o.tags || []) counts.set(t, (counts.get(t) || 0) + 1);
  const useOf = tag => (state.projectTags || []).find(u => u.tag.toLowerCase() === tag.toLowerCase());
  const isMod = tag => (state.modTags || []).some(t => t.toLowerCase() === tag.toLowerCase());
  const shutDialog = () => { const shut = document.querySelector('.picker.dialog .shut'); if (shut) shut.click(); };
  const finish = async (touchedHere, message) => {
    shutDialog();
    if (touchedHere) { sceneChanged('edit tags'); syncSceneObjects(activeDoc); drawHierarchy(); }
    if (message) say(message, 'good');
    await loadProjectTags();
    if (done) done();
    if (touchedHere && typeof drawInspector === 'function') drawInspector();
  };

  // ---- the mod's tags
  const modHead = document.createElement('div');
  modHead.className = 'behaviour-header';
  modHead.textContent = 'Mod tags - offered in every scene';
  body.append(modHead);
  const modNote = document.createElement('p');
  modNote.className = 'dialog-note';
  modNote.textContent = 'Kept in the project (project.json). Renaming or removing one here changes every object on every map of the mod that carries it; a mod\'s code asking for the old name (WithTag) is yours to update.';
  body.append(modNote);
  const modList = document.createElement('div');
  modList.className = 'dialog-list';
  for (const tag of modTags()) {
    const row = document.createElement('div');
    row.className = 'dialog-row tag-edit-row';
    const input = document.createElement('input');
    input.type = 'text';
    input.value = tag;
    const use = document.createElement('span');
    const u = useOf(tag);
    use.textContent = u ? `${u.count} object${u.count === 1 ? '' : 's'} on ${u.maps.join(', ')}` : 'not on any object yet';
    const rename = document.createElement('button');
    rename.type = 'button';
    rename.textContent = 'Rename';
    rename.title = 'Rename this tag on every object of every map, and in the mod\'s list';
    rename.hidden = true;
    input.oninput = () => { const v = input.value.trim().replace(/\s+/g, '-'); rename.hidden = !v || v === tag; };
    rename.onclick = async () => {
      const to = input.value.trim().replace(/\s+/g, '-');
      if (!to || to === tag) return;
      const r = await retagEverywhere(tag, to);
      finish(r.here > 0, `"${tag}" is "${to}" on ${r.maps.length} map file(s)`);
    };
    const x = document.createElement('button');
    x.type = 'button';
    x.textContent = '×';
    x.title = 'Remove this tag from the mod\'s list and from every object of every map';
    x.onclick = async () => {
      const carried = u ? ` and off ${u.count} object(s) on ${u.maps.join(', ')}` : '';
      if (!confirm(`Remove the tag "${tag}" from the mod's list${carried}?`)) return;
      const r = await retagEverywhere(tag, '');
      finish(r.here > 0, `"${tag}" removed (${r.maps.length} map file(s) changed)`);
    };
    row.append(input, use, rename, x);
    modList.append(row);
  }
  if (!modTags().length) {
    const none = document.createElement('p');
    none.className = 'dialog-note';
    none.textContent = 'The mod has no tags of its own yet.';
    modList.append(none);
  }
  const addRow = document.createElement('div');
  addRow.className = 'dialog-row tag-edit-row';
  const addInput = document.createElement('input');
  addInput.type = 'text';
  addInput.placeholder = 'new mod tag';
  const add = document.createElement('button');
  add.type = 'button';
  add.textContent = 'Add';
  const addIt = async () => {
    const name = addInput.value.trim().replace(/\s+/g, '-');
    if (!name || isMod(name)) return;
    await saveModTags([...(state.modTags || []), name]);
    finish(false, `"${name}" is a tag of the mod`);
  };
  add.onclick = addIt;
  addInput.onkeydown = e => { if (e.key === 'Enter') addIt(); };
  addRow.append(addInput, add);
  modList.append(addRow);
  body.append(modList);

  // ---- this map's own
  const mapHead = document.createElement('div');
  mapHead.className = 'behaviour-header';
  mapHead.textContent = `This map's tags - ${mapState.name}`;
  body.append(mapHead);
  const mapNote = document.createElement('p');
  mapNote.className = 'dialog-note';
  mapNote.textContent = 'Tags on this map\'s objects that are not the mod\'s. Renaming or removing one changes every object here that carries it; ↑ makes it a tag of the mod. Tags used only on other maps are greyed, for the spelling.';
  body.append(mapNote);
  const list = document.createElement('div');
  list.className = 'dialog-list';
  const renames = new Map();
  const removals = new Set();
  const local = [...counts.keys()].filter(t => !isMod(t)).sort((a, b) => a.localeCompare(b));
  for (const tag of local) {
    const row = document.createElement('div');
    row.className = 'dialog-row tag-edit-row';
    const input = document.createElement('input');
    input.type = 'text';
    input.value = tag;
    input.oninput = () => { const v = input.value.trim().replace(/\s+/g, '-'); if (v && v !== tag) renames.set(tag, v); else renames.delete(tag); };
    const use = document.createElement('span');
    use.textContent = `${counts.get(tag)} object${counts.get(tag) === 1 ? '' : 's'}`;
    const up = document.createElement('button');
    up.type = 'button';
    up.textContent = '↑';
    up.title = 'Make this a tag of the mod, offered in every scene';
    up.onclick = async () => { await saveModTags([...(state.modTags || []), tag]); finish(false, `"${tag}" is a tag of the mod`); };
    const x = document.createElement('button');
    x.type = 'button';
    x.textContent = '×';
    x.title = 'Remove this tag from every object on the map';
    x.onclick = () => { if (removals.has(tag)) { removals.delete(tag); row.classList.remove('struck'); } else { removals.add(tag); row.classList.add('struck'); } };
    row.append(input, use, up, x);
    list.append(row);
  }
  for (const u of (state.projectTags || []).filter(u => !counts.has(u.tag) && !isMod(u.tag))) {
    const row = document.createElement('div');
    row.className = 'dialog-row dim';
    row.textContent = `${u.tag}  ·  elsewhere: ${u.maps.join(', ')}`;
    list.append(row);
  }
  if (!local.length) {
    const none = document.createElement('p');
    none.className = 'dialog-note';
    none.textContent = 'No object on this map has a tag of its own.';
    list.append(none);
  }
  body.append(list);
  const apply = document.createElement('button');
  apply.className = 'wide-button';
  apply.textContent = 'Apply to this map';
  apply.onclick = () => {
    let touched = 0;
    for (const o of objects) {
      if (!o.tags || !o.tags.length) continue;
      const before = o.tags.join('\n');
      o.tags = [...new Set(o.tags.filter(t => !removals.has(t)).map(t => renames.get(t) || t))];
      if (o.tags.join('\n') !== before) touched++;
    }
    finish(touched > 0, touched ? `tags changed on ${touched} object(s)` : null);
  };
  body.append(apply);
}

function buildSceneObject(doc, object) {
  const panel = document.createElement('div');
  panel.className = 'scene-object';
  const path = scenePathOf(object);

  const redraw = () => {
    sceneChanged();
    syncSceneObjects(doc);
    drawHierarchy();
    drawInspector();
  };
  const changed = () => {
    sceneChanged();
    syncSceneObjects(doc);
    drawHierarchy();
  };

  // ------------------------------------------------------------------- name

  panel.append(inspectorHead(object.model ? 'model' : 'exit', object.name,
    'OpenFF object · ' + mapState.name + '/' + path, (box) => {
      const fresh = box.value.trim().replace(/[\/:]/g, ' ').trim();
      const siblings = object.parent ? object.parent.children : sceneState.objects;
      if (!fresh || fresh.toLowerCase() === 'map' || siblings.some(o => o !== object && o.name.toLowerCase() === fresh.toLowerCase())) { box.value = object.name; return; }
      const was = scenePathOf(object);
      object.name = fresh;
      retargetAttachments(was, scenePathOf(object));
      doc.selection = 'scene:' + scenePathOf(object);
      redraw();
    }));

  // -------------------------------------------------------------- transform

  panel.append(transformCard({
    read: key => object[key],
    write: (key, value) => {
      if (key === 'scale') object.scale = value > 0 ? value : 1;
      else object[key] = value || 0;
      changed();
    },
    rotation: 'rotationY',
    scale: 'scale',
    note: object.parent ? 'relative to ' + object.parent.name : null,
    positionTitle: object.parent ? 'In the parent\'s frame: turned by its yaw, scaled by its scale' : 'World units',
    rotationTitle: 'Yaw in degrees: 0 faces +z, 90 faces +x' + (object.parent ? '; added to the parent\'s' : ''),
    scaleTitle: '1 is the model\'s own size' + (object.parent ? '; multiplied by the parent\'s' : ''),
  }));

  // ------------------------------------------------------------------ model

  const modelRow = componentCard('model', 'Model');
  const pickRow = document.createElement('div');
  pickRow.className = 'model-pick';
  const choose = document.createElement('button');
  choose.className = 'model-choice';
  const chosenName = document.createElement('span');
  chosenName.textContent = object.model || 'None - a spot with logic on it';
  if (!object.model) chosenName.className = 'dim';
  choose.append(icon('model'), chosenName);
  choose.title = object.model
    ? 'Shown in the game as this model standing there - no cast, no script. Click to swap it.'
    : 'Nothing is drawn for it; its behaviours run at this spot. Click to give it a model.';
  choose.onclick = () => pickModel(object.model || 'n011', (model) => {
    if (model === object.model) return;
    object.model = model;
    redraw();
  });
  pickRow.append(choose);
  if (object.model) {
    const clear = document.createElement('button');
    clear.className = 'clear-button';
    clear.textContent = '×';
    clear.title = 'No model - a spot with logic on it';
    clear.onclick = () => { object.model = null; redraw(); };
    pickRow.append(clear);
  }
  modelRow.append(pickRow);
  if (object.model) {
    // A character has the game's walker behind it: it turns to the player, can wander,
    // is talked to the game's way. A plain figure just stands there.
    const kind = document.createElement('label');
    kind.className = 'toggle';
    kind.title = 'Spawned as a character (turns to the player, can wander, is talked to) rather than a plain figure. An object model (o…, w…: chests, signs, crates) is the game\'s map object whichever way - its lid and its opening come with it.';
    const box = document.createElement('input');
    box.type = 'checkbox';
    box.checked = Boolean(object.character);
    box.onchange = () => { object.character = box.checked; changed(); };
    kind.append(box, document.createTextNode(' character - walks, turns, is talked to'));
    modelRow.append(kind);
    if (object.character) {
      // The scripts have two ways of booting a character; the converter keeps whichever the
      // original had. Plain is bootPlainCharacter: the model's own scale (a child at 0.8) and kind.
      const plain = document.createElement('label');
      plain.className = 'toggle';
      plain.title = 'Made as the map scripts\' bootPlainCharacter makes a character: the light walker with the model\'s own scale and kind (a child\'s model at 0.8, the chocobo, the frog). Off: bootCharacter\'s kind, the full walker.';
      const pbox = document.createElement('input');
      pbox.type = 'checkbox';
      pbox.checked = Boolean(object.plain);
      pbox.onchange = () => { object.plain = pbox.checked; changed(); };
      plain.append(pbox, document.createTextNode(' plain - the scripts\' light walker, the model\'s own scale'));
      modelRow.append(plain);
    }
  }
  panel.append(modelRow);

  // ----------------------------------------------------------- tags, parent

  const misc = componentCard('scene', 'Scene', mapState.name + '/' + path);
  misc.append(tagsField(object, changed));

  const parentWrap = document.createElement('label');
  parentWrap.className = 'behaviour-field';
  parentWrap.title = 'Under another object it moves with it; changing this keeps it where it stands';
  const parentLabel = document.createElement('span');
  parentLabel.textContent = 'Parent';
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
  misc.append(parentWrap);
  panel.append(misc);

  // -------------------------------------------------------------- behaviours

  behavioursSection(panel, path, 'object');

  // --------------------------------------------------------- children, out

  // Children are made from the hierarchy (right click ▸ New child object, or drop a row
  // on this one); here they are listed for the way down.
  if (object.children.length) {
    const kidsHead = document.createElement('h3');
    kidsHead.textContent = 'Children';
    panel.append(kidsHead);
    const list = document.createElement('ul');
    list.className = 'scene-children';
    for (const child of object.children) {
      const li = document.createElement('li');
      li.append(icon(child.model ? 'model' : 'exit'));
      const link = document.createElement('a');
      link.href = '#';
      link.textContent = child.name + (child.model ? '  (' + child.model + ')' : '');
      link.onclick = (event) => {
        event.preventDefault();
        doc.selection = 'scene:' + scenePathOf(child);
        const index = flattenSceneObjects(sceneState).findIndex(i => i.source === child);
        if (doc.scene3d && index >= 0) doc.scene3d.selectPoint(index);
        drawHierarchy();
        drawInspector();
      };
      li.append(link);
      list.append(li);
    }
    panel.append(list);
  }

  const foot = document.createElement('div');
  foot.className = 'behaviour-actions';
  const remove = document.createElement('button');
  remove.textContent = object.children.length ? 'Delete with children' : 'Delete object';
  remove.onclick = () => removeSceneObject(doc, object);
  const find = document.createElement('span');
  find.className = 'dim';
  find.textContent = 'Game.World.Legacy.Find("' + mapState.name + '/' + path + '")';
  find.title = 'How a mod\'s code gets hold of it' + (object.tags && object.tags.length ? ', or .WithTag("' + object.tags[0] + '")' : '');
  foot.append(remove, find);
  panel.append(foot);
  return panel;
}
