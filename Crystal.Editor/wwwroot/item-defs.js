// The mod's definitions in the editor - items, and the heroes (characters). Items: defs/items/<id>.json each - an item of the
// mod's own that starts from one of the game's (its base) and changes what it names. The
// inspector shows one as a form laid out like a scene object's: the name and the caption,
// the base item, the prices, then every field of the base's record by the game's own name
// with the base's value greyed in, so a field left alone stays the base's. Every change
// saves itself (the file is small; there is no Save). New item… asks for a name and a base
// and gives the definition the next number free - what the game will know it by for good.
//
// Server: /api/project/items (list, ?id= one), /api/project/items/save, /new, /delete.

/// The tree's counts beside Items and Characters, after a definition came or went.
async function itemsCountChanged() {
  // The item pickers ([ItemField] on a Chest and the like) list the mod's items with the game's,
  // fetched once; an item made, renamed or removed means fetching again.
  state.items = null;
  try {
    const r = await api('/api/project/items');
    const c = await api('/api/project/characters');
    const t = await api('/api/project/text');
    if (typeof projectState !== 'undefined' && projectState.project) { projectState.project.items = (r.items || []).length; projectState.project.characters = (c.characters || []).length; projectState.project.text = (t.lines || []).length; }
    if (typeof monstersCountChanged === 'function') await monstersCountChanged(); else if (typeof drawProjectTree === 'function') drawProjectTree();
  } catch (e) { /* the count is a nicety */ }
}

/// The list of the game's items for a base picker, cached; the mod's own (marked "(mod)")
/// are not offered as bases - a definition starts from the game's record.
async function gameItemsForPicker() {
  if (!state.items) state.items = await api('/api/items');
  return (state.items || []).filter(i => !/\(mod\)$/.test(i.name || ''));
}

function itemBasePicker(current, onPick) {
  const pick = document.createElement('select');
  pick.className = 'item-base-pick';
  const fill = (items) => {
    pick.textContent = '';
    const groups = new Map();
    for (const item of items) {
      const key = item.category || 'items';
      if (!groups.has(key)) groups.set(key, []);
      groups.get(key).push(item);
    }
    for (const [label, list] of groups) {
      const g = document.createElement('optgroup');
      g.label = label;
      for (const item of list) {
        const o = document.createElement('option');
        o.value = String(item.id);
        o.textContent = `${item.name || '?'}  (${item.id})`;
        g.append(o);
      }
      pick.append(g);
    }
    pick.value = String(current || '');
  };
  gameItemsForPicker().then(fill).catch(() => {});
  pick.onchange = () => onPick(parseInt(pick.value, 10) || 0);
  return pick;
}

/// The inspector's form for one definition (the server's Describe shape). onSaved runs
/// after a write, for the list to redraw with the new name.
function itemDefinitionPanel(def, onSaved) {
  const panel = document.createElement('div');
  panel.className = 'scene-object item-def';

  // The edited shape, written back as a whole on every change.
  const model = {
    id: def.id, number: def.number, base: def.base, name: def.name || '', caption: def.caption || '',
    buy: def.buy, sell: def.sell,
    fields: Object.fromEntries((def.fields || []).filter(f => f.value !== null && f.value !== undefined).map(f => [f.name, f.value]))
  };
  let timer = null;
  const save = () => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      try {
        const r = await api('/api/project/items/save', model);
        if (!r.ok) throw new Error(r.error);
        itemsCountChanged();
        if (onSaved) onSaved(r.item);
      } catch (e) { say('item: ' + e.message, 'bad'); }
    }, 250);
  };

  // ---- head: the name, the number, the file
  const head = document.createElement('div');
  head.className = 'object-head';
  const title = document.createElement('input');
  title.type = 'text';
  title.className = 'object-name';
  title.value = model.name;
  title.placeholder = 'the item\'s name, as the menus show it';
  title.oninput = () => { model.name = title.value; save(); };
  head.append(title);
  panel.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `item ${def.number} · ${def.file}`;
  sub.title = 'The number is the item id the game knows it by - a chest gives it, a save holds it - given once, kept for good.';
  panel.append(sub);

  const card = (label) => {
    const c = document.createElement('div');
    c.className = 'component';
    const h = document.createElement('div');
    h.className = 'behaviour-header';
    h.textContent = label;
    c.append(h);
    panel.append(c);
    return c;
  };
  const row = (parent, label, input, tip) => {
    const r = document.createElement('div');
    r.className = 'behaviour-field';
    const l = document.createElement('span');
    l.textContent = label;
    if (tip) r.title = tip;
    r.append(l, input);
    parent.append(r);
    return r;
  };
  const number = (value, placeholder, onChange) => {
    const n = document.createElement('input');
    n.type = 'number';
    n.value = value === null || value === undefined ? '' : value;
    n.placeholder = placeholder === null || placeholder === undefined ? '' : String(placeholder);
    n.oninput = () => onChange(n.value === '' ? null : parseInt(n.value, 10));
    return n;
  };

  // ---- what it is
  const what = card('Item');
  const caption = document.createElement('textarea');
  caption.rows = 2;
  caption.value = model.caption;
  caption.placeholder = def.baseCaption || 'what the menu says about it';
  caption.oninput = () => { model.caption = caption.value; save(); };
  row(what, 'Caption', caption, 'The line under the name in the item menu');
  const basePick = itemBasePicker(model.base, (id) => { if (id) { model.base = id; save(); setTimeout(() => inspectAsset('items', model.id), 400); } });
  row(what, 'Based on', basePick, `The game's record this one starts from (${def.chain || '?'}); the fields below that are left blank stay its`);
  const kind = document.createElement('p');
  kind.className = 'none';
  kind.textContent = def.chain ? `${def.chain}, from ${def.baseName || def.base}` : `no item ${def.base} in the game's tables`;
  what.append(kind);

  // ---- prices
  if (def.chain !== 'key item') {
    const prices = card('Shop');
    row(prices, 'Buy', number(model.buy, def.baseBuy, v => { model.buy = v; save(); }), 'What a shop sells it for; blank keeps the base\'s');
    row(prices, 'Sell', number(model.sell, def.baseSell, v => { model.sell = v; save(); }), 'What a shop pays for it; blank keeps the base\'s');
  }

  // ---- the record's fields
  const fields = card('Record - the game\'s fields');
  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'The base\'s values are greyed in; set a field to change it, clear it (the ↺) to go back to the base\'s. Flags and kinds are pickers; a plain number is a number.';
  fields.append(note);
  for (const f of def.fields || []) {
    if (f.name === 'buy' || f.name === 'price') continue;
    const meta = itemFieldMeta(f.name, def.chain);
    const set = v => { if (v === null) delete model.fields[f.name]; else model.fields[f.name] = v; save(); };
    let input;
    if (meta.kind === 'flags') input = flagsPicker(f.value, f.baseValue, meta.options, set);
    else if (meta.kind === 'enum') input = enumPicker(f.value, f.baseValue, meta.options, set);
    else if (meta.kind === 'bool') input = enumPicker(f.value, f.baseValue, [[0, 'No'], [1, 'Yes']], set);
    else if (meta.kind === 'model') input = weaponModelPicker(f.value, f.baseValue, set);
    else if (meta.kind === 'item') input = itemRefPicker(f.value, f.baseValue, meta.category, set);
    else input = number(f.value, f.baseValue, set);
    const r = row(fields, meta.label || f.name, input, meta.tip);
    if (meta.label && meta.label !== f.name) r.firstChild.title = f.name + (meta.tip ? ' - ' + meta.tip : '');
  }

  // ---- the file, and the end
  const actions = document.createElement('div');
  actions.className = 'component';
  const json = document.createElement('button');
  json.className = 'wide-button';
  json.textContent = 'Open as JSON';
  json.title = `${def.file} as text - what Export to OpenFF copies into the mod`;
  json.onclick = () => openDoc('code', def.file);
  actions.append(json);
  const remove = document.createElement('button');
  remove.className = 'wide-button';
  remove.textContent = 'Delete this item';
  remove.title = 'Removes the definition file. A save that holds the item would then hold an item the game has no record for.';
  remove.onclick = async () => {
    if (!confirm(`Delete the item definition "${model.name || model.id}" (${def.file})?`)) return;
    const r = await api('/api/project/items/delete', { id: model.id });
    if (!r.ok) { say('item: not deleted', 'bad'); return; }
    if (typeof clearInspected === 'function') clearInspected();
    itemsCountChanged();
    loadList();
    say(`"${model.name || model.id}" deleted`, 'good');
  };
  actions.append(remove);
  panel.append(actions);
  return panel;
}

/// New item: a name and a base; the definition is written and opened in the inspector.
function newItemDialog() {
  if (typeof dialog !== 'function') return;
  const body = dialog('New item');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'An item of the mod\'s own. It starts as a copy of one of the game\'s - pick the closest: a potion for a potion, a sword for a sword - and changes only what you set in the inspector afterwards: name, caption, prices, the record\'s fields.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: 'Hi-Potion+' });
  section(body, 'Based on');
  let base = 5001;
  body.append(itemBasePicker(base, id => { base = id; }));
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const n = name.value.trim();
    if (!n) { problem.textContent = 'An item needs a name.'; return; }
    try {
      const r = await api('/api/project/items/new', { name: n, base });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      itemsCountChanged();
      await loadList();
      inspectAsset('items', r.item.id);
      say(`"${n}" is item ${r.item.number} of the mod`, 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  name.onkeydown = e => { if (e.key === 'Enter') go.click(); };
  actions.append(go);
  body.append(actions);
  setTimeout(() => name.focus(), 0);
}

// ---------------------------------------------------------------- characters

/// The inspector's form for a hero definition: which slot, the name, the starting job and
/// level as a game begins. data = { character, jobs, heroes } from /api/project/characters.
function characterDefinitionPanel(data, onSaved) {
  const def = data.character;
  const panel = document.createElement('div');
  panel.className = 'scene-object item-def';
  const model = { id: def.id, slot: def.slot, name: def.name || '', job: def.job === null || def.job === undefined ? null : String(def.job), level: def.level || 0, fixedJob: !!def.fixedJob, look: def.look === null || def.look === undefined ? -1 : def.look };
  let timer = null;
  const save = () => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      try {
        const body = { id: model.id, slot: model.slot, name: model.name };
        if (model.job !== null) body.job = model.job;
        if (model.level > 1) body.level = model.level;
        if (model.fixedJob) body.fixedJob = true;
        if (model.look >= 0) body.look = model.look;
        const r = await api('/api/project/characters/save', body);
        if (!r.ok) throw new Error(r.error);
        if (onSaved) onSaved(r.character);
      } catch (e) { say('character: ' + e.message, 'bad'); }
    }, 250);
  };

  const head = document.createElement('div');
  head.className = 'object-head';
  const title = document.createElement('input');
  title.type = 'text';
  title.className = 'object-name';
  title.value = model.name;
  title.placeholder = data.heroes[def.slot] || 'name';
  title.title = 'The hero\'s name as a game begins; the name entry still lets the player change it';
  title.oninput = () => { model.name = title.value; save(); };
  head.append(title);
  panel.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = def.file;
  panel.append(sub);

  const card = document.createElement('div');
  card.className = 'component';
  const h = document.createElement('div');
  h.className = 'behaviour-header';
  h.textContent = 'As a game begins';
  card.append(h);
  const row = (label, input, tip) => {
    const r = document.createElement('div');
    r.className = 'behaviour-field';
    const l = document.createElement('span');
    l.textContent = label;
    if (tip) r.title = tip;
    r.append(l, input);
    card.append(r);
  };
  const slot = document.createElement('select');
  data.heroes.forEach((hero, i) => {
    const o = document.createElement('option');
    o.value = String(i);
    o.textContent = `${i} - ${hero}`;
    slot.append(o);
  });
  slot.value = String(model.slot);
  slot.onchange = () => { model.slot = parseInt(slot.value, 10); save(); };
  row('Hero slot', slot, 'Which of the four heroes this defines');
  const job = document.createElement('select');
  const keep = document.createElement('option');
  keep.value = '';
  keep.textContent = 'the game\'s (Freelancer)';
  job.append(keep);
  for (const j of data.jobs) {
    const o = document.createElement('option');
    o.value = String(j.number);
    o.textContent = `${j.name}  (${j.number})`;
    job.append(o);
  }
  job.value = model.job === null ? '' : model.job;
  job.onchange = () => { model.job = job.value === '' ? null : job.value; save(); };
  row('Starting job', job, 'The job the hero begins in; the crystals still change it');
  const level = document.createElement('input');
  level.type = 'number';
  level.min = 1;
  level.max = 99;
  level.value = model.level > 1 ? model.level : '';
  level.placeholder = '1';
  level.oninput = () => { model.level = parseInt(level.value, 10) || 0; save(); };
  row('Starting level', level, 'Levelled the game\'s own way, one level at a time along the job\'s growth');
  const fixed = document.createElement('input');
  fixed.type = 'checkbox';
  fixed.checked = !!def.fixedJob;
  fixed.onchange = () => { model.fixedJob = fixed.checked; save(); };
  row('Fixed job', fixed, 'The hero keeps its job: the job menu refuses a change, as it does for a job not yet won - a class of its own rather than the job system');
  const look = document.createElement('select');
  const own = document.createElement('option');
  own.value = '';
  own.textContent = 'its own slot\'s';
  look.append(own);
  data.heroes.forEach((hero, i) => {
    const o = document.createElement('option');
    o.value = String(i);
    o.textContent = `${hero}'s figures (j${i + 1}xx)`;
    look.append(o);
  });
  look.value = def.look === null || def.look === undefined || def.look < 0 ? '' : String(def.look);
  look.onchange = () => { model.look = look.value === '' ? -1 : parseInt(look.value, 10); save(); };
  row('Look', look, 'Which hero\'s model set it wears - on the field, in battle, in the menus; every job has a figure in every set');
  panel.append(card);

  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'Applied when the client sets a party up (a --map start, the title\'s New Game); a save carries its own heroes. A character\'s own model, a fixed class instead of jobs, and heroes beyond the four are the next slice.';
  panel.append(note);

  const actions = document.createElement('div');
  actions.className = 'component';
  const json = document.createElement('button');
  json.className = 'wide-button';
  json.textContent = 'Open as JSON';
  json.onclick = () => openDoc('code', def.file);
  actions.append(json);
  const remove = document.createElement('button');
  remove.className = 'wide-button';
  remove.textContent = 'Delete this definition';
  remove.onclick = async () => {
    if (!confirm(`Delete the definition "${model.name || model.id}" (${def.file})?`)) return;
    const r = await api('/api/project/characters/delete', { id: model.id });
    if (!r.ok) { say('character: not deleted', 'bad'); return; }
    if (typeof clearInspected === 'function') clearInspected();
    itemsCountChanged();
    loadList();
  };
  actions.append(remove);
  panel.append(actions);
  return panel;
}

function newCharacterDialog() {
  if (typeof dialog !== 'function') return;
  const body = dialog('New character');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'One of the four heroes as a game begins: its name here, the starting job and level in the inspector after.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: 'Luneth' });
  section(body, 'Hero slot');
  const slot = document.createElement('select');
  ['Luneth', 'Arc', 'Refia', 'Ingus'].forEach((hero, i) => {
    const o = document.createElement('option');
    o.value = String(i);
    o.textContent = `${i} - ${hero}`;
    slot.append(o);
  });
  slot.className = 'item-base-pick';
  body.append(slot);
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const n = name.value.trim();
    if (!n) { problem.textContent = 'A character needs a name.'; return; }
    try {
      const r = await api('/api/project/characters/new', { name: n, slot: parseInt(slot.value, 10) });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      itemsCountChanged();
      await loadList();
      inspectAsset('characters', r.character.id);
      say(`"${n}" defines hero ${r.character.slot}`, 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  name.onkeydown = e => { if (e.key === 'Enter') go.click(); };
  actions.append(go);
  body.append(actions);
  setTimeout(() => name.focus(), 0);
}
// ---------------------------------------------------- what the record's fields mean
//
// The game's own enums (OpenFF/GlobalScope/itm/*.cs), so a field that is a set of flags or a
// kind is picked by name rather than typed as a number. A field not listed here is a plain
// number with a tip; one whose meaning has not been read says so.

const JOBS = [[1, 'Freelancer'], [2, 'Onion Knight'], [4, 'Warrior'], [8, 'Monk'], [16, 'White Mage'], [32, 'Black Mage'], [64, 'Red Mage'], [128, 'Ranger'], [256, 'Knight'], [512, 'Thief'], [1024, 'Scholar'], [2048, 'Geomancer'], [4096, 'Dragoon'], [8192, 'Viking'], [16384, 'Dark Knight'], [32768, 'Evoker'], [65536, 'Bard'], [131072, 'Black Belt'], [262144, 'Magus'], [524288, 'Devout'], [1048576, 'Summoner'], [2097152, 'Sage'], [4194304, 'Ninja']];
const ELEMENTS = [[1, 'Recovery'], [2, 'Poison'], [4, 'Absorption'], [8, 'Lightning'], [16, 'Ice'], [32, 'Fire'], [64, 'Water'], [128, 'Earth'], [256, 'Holy'], [512, 'Wind'], [1024, 'Dark']];
const CONDITIONS = [[1, 'Paralysis'], [2, 'Sleep'], [4, 'Confusion'], [8, 'Petrify'], [16, 'Toad'], [32, 'Silence'], [64, 'Mini'], [128, 'Blind'], [256, 'Poison'], [512, 'KO'], [1024, 'Near death']];
const ARMS = [[1, 'Grapple'], [2, 'Slash'], [4, 'Blow'], [8, 'Charge']];
const TARGET_SELECT = [[1, 'None'], [2, 'One enemy'], [4, 'Enemy group'], [8, 'All enemies'], [16, 'Random enemy'], [32, 'Random enemy group'], [64, 'Self'], [128, 'One ally'], [256, 'Ally group'], [512, 'All allies'], [1024, 'Random ally'], [2048, 'Random ally group'], [4096, 'Everyone']];
const TARGET_POSITION = [[0, 'Enemies'], [1, 'Enemies (abnormal)'], [2, 'Self'], [3, 'Allies']];
const WEAPON_KINDS = [[0, 'Unarmed'], [1, 'Knife'], [2, 'Sword'], [3, 'Club'], [4, 'Mace'], [5, 'Staff'], [6, 'Rod'], [7, 'Bow'], [8, 'Arrow'], [9, 'Book'], [10, 'Claw'], [11, 'Hammer'], [12, 'Axe'], [13, 'Spear'], [14, 'Throwing'], [15, 'Bell'], [16, 'Harp'], [17, 'Dark sword'], [18, 'Shuriken']];
const ARMOUR_KINDS = [[0, 'Shield'], [1, 'Helmet'], [2, 'Armour'], [3, 'Gauntlet']];
const MAGIC_KINDS = [[0, 'White'], [1, 'Black'], [2, 'Summon'], [3, 'Song'], [4, 'Terrain'], [5, 'Enemy']];
const CONSUMABLE_KINDS = [[0, 'Normal'], [1, 'Battle']];
const MAGIC_USE = [[0, 'Attack'], [1, 'Recovery'], [2, 'Assist'], [3, 'Special']];

function itemFieldMeta(name, chain) {
  const weapon = chain === 'weapon', armour = chain === 'armour', magic = chain === 'magic', consumable = chain === 'consumable';
  switch (name) {
    case 'system': return weapon ? { kind: 'enum', label: 'Kind', options: WEAPON_KINDS, tip: 'The weapon\'s kind: which motion set swings it, which jobs\' skills apply' }
      : armour ? { kind: 'enum', label: 'Kind', options: ARMOUR_KINDS, tip: 'Which slot it goes in' }
      : magic ? { kind: 'enum', label: 'School', options: MAGIC_KINDS }
      : consumable ? { kind: 'enum', label: 'Kind', options: CONSUMABLE_KINDS, tip: 'Battle: usable only in battle' }
      : { kind: 'number', label: 'system', tip: 'The record\'s kind byte' };
    case 'graphId': return weapon ? { kind: 'model', label: 'Model', tip: 'The weapon\'s 3D model in battle: w + this number (w015 is model 15); the picker lists the game\'s weapon models' }
      : { kind: 'number', label: 'Graphic', tip: 'The item\'s picture (icon) number in the menu' };
    case 'strength': return { kind: 'number', label: 'Strength +', tip: 'Added to the wearer\'s strength while equipped (0-255)' };
    case 'vitality': return { kind: 'number', label: 'Vitality +', tip: 'Added to the wearer\'s vitality while equipped' };
    case 'dexterity': return { kind: 'number', label: 'Agility +', tip: 'Added to the wearer\'s agility while equipped' };
    case 'intellect': return { kind: 'number', label: 'Intellect +', tip: 'Added to the wearer\'s intellect while equipped' };
    case 'mind': return { kind: 'number', label: 'Mind +', tip: 'Added to the wearer\'s mind while equipped' };
    case 'weight': return { kind: 'number', label: 'Weight', tip: 'Heavier equipment slows the turn (0-255)' };
    case 'useBattle': return { kind: 'bool', label: 'Usable in battle' };
    case 'useField': return { kind: 'bool', label: 'Usable on the field' };
    case 'allTarget': return { kind: 'bool', label: 'Targets everyone', tip: 'Affects the whole side rather than one' };
    case 'useItemId': return { kind: 'item', label: 'Casts', category: 'magic', tip: 'The spell an item casts when used (a consumable\'s effect, a weapon\'s Item use); none for nothing' };
    case 'targetPossible': return { kind: 'flags', label: 'Targets', options: TARGET_SELECT, tip: 'Which targets the cursor may pick' };
    case 'targetPosition': return { kind: 'enum', label: 'Default target', options: TARGET_POSITION, tip: 'Where the cursor starts' };
    case 'equipJob': return { kind: 'flags', label: 'Jobs', options: JOBS, tip: 'Which jobs may equip it' };
    case 'aggressivity': return { kind: 'number', label: magic ? 'Power' : 'Attack', tip: magic ? 'The spell\'s power' : 'The weapon\'s attack power' };
    case 'hitProbability': return { kind: 'number', label: 'Accuracy %', tip: 'Chance to hit, 0-100' };
    case 'optionProbability': return { kind: 'number', label: 'Status chance %', tip: 'Chance per hit to inflict the status below (0-100)' };
    case 'optionMagicItemId': return { kind: 'item', label: 'Casts when used', category: 'magic', tip: 'The spell cast when the weapon is used as an item in battle; none for nothing' };
    case 'armsAttribute': return { kind: 'flags', label: 'Damage type', options: ARMS, tip: 'How it hurts: some monsters are weak or hard against a type' };
    case 'atckType': return { kind: 'number', label: 'atckType', tip: 'Attack type bits; bit 4 marks a weapon the game\'s battle setup treats apart (a bow\'s arrow). Meaning not fully read - a raw number' };
    case 'atckOption': return { kind: 'flags', label: 'Inflicts', options: CONDITIONS, tip: 'The status a hit may inflict, at the chance above' };
    case 'equipOption': return { kind: 'flags', label: 'Guards against', options: CONDITIONS, tip: 'Statuses the equipment prevents' };
    case 'phylacticPower': return { kind: 'number', label: 'Defence' };
    case 'magicPhylacticPower': return { kind: 'number', label: 'Magic defence' };
    case 'avoidanceProbability': return { kind: 'number', label: 'Evasion %' };
    case 'magicAvoidanceProbability': return { kind: 'number', label: 'Magic evasion %' };
    case 'magicClass': return { kind: 'number', label: 'Level', tip: 'The spell\'s level, 1-8: which spell slot it takes' };
    case 'successProbability': return { kind: 'number', label: 'Accuracy %' };
    case 'magicUseKind': return { kind: 'enum', label: 'Use', options: MAGIC_USE };
    case 'magicType': return { kind: 'flags', label: 'Element', options: ELEMENTS };
    case 'changeCondition': return { kind: 'flags', label: 'Status', options: CONDITIONS, tip: 'The statuses it inflicts, or cures for a recovery item or spell' };
    case 'calculate': return { kind: 'number', label: 'calculate', tip: 'Which damage formula; meaning not fully read - a raw number' };
    case 'reflect': return { kind: 'bool', label: 'Reflectable' };
    case 'usedPower': return { kind: 'number', label: 'Power', tip: 'The effect\'s strength: a potion\'s HP, a spell item\'s power' };
    case 'itemType': return { kind: 'flags', label: 'Element', options: ELEMENTS, tip: 'The effect\'s element: Recovery heals' };
    default: return { kind: 'number', label: name };
  }
}

/// The base's value greyed in a select, the chosen one set; ↺ goes back to the base's.
function enumPicker(value, baseValue, options, onChange) {
  const wrap = document.createElement('span');
  wrap.className = 'field-pick';
  const select = document.createElement('select');
  const base = document.createElement('option');
  base.value = '';
  const baseLabel = options.find(o => o[0] === baseValue);
  base.textContent = `(base: ${baseLabel ? baseLabel[1] : baseValue})`;
  select.append(base);
  for (const [v, label] of options) {
    const o = document.createElement('option');
    o.value = String(v); o.textContent = label;
    select.append(o);
  }
  if (value !== null && value !== undefined && !options.some(o => o[0] === value)) {
    const o = document.createElement('option');
    o.value = String(value); o.textContent = value + ' (not a named value)';
    select.append(o);
  }
  select.value = value === null || value === undefined ? '' : String(value);
  select.onchange = () => onChange(select.value === '' ? null : parseInt(select.value, 10));
  wrap.append(select);
  return wrap;
}

/// One checkbox per flag; the base's flags shown greyed until one is touched, then the set is the mod's.
function flagsPicker(value, baseValue, options, onChange) {
  const wrap = document.createElement('div');
  wrap.className = 'field-flags';
  let current = value === null || value === undefined ? null : value;
  const boxes = [];
  const redraw = () => {
    const shown = current === null ? (baseValue || 0) : current;
    for (const [bit, box] of boxes) box.checked = (shown & bit) !== 0;
    wrap.classList.toggle('inherited', current === null);
  };
  for (const [bit, label] of options) {
    const l = document.createElement('label');
    const box = document.createElement('input');
    box.type = 'checkbox';
    box.onchange = () => {
      const shown = current === null ? (baseValue || 0) : current;
      current = box.checked ? (shown | bit) : (shown & ~bit);
      onChange(current);
      redraw();
    };
    l.append(box, document.createTextNode(' ' + label));
    wrap.append(l);
    boxes.push([bit, box]);
  }
  const reset = document.createElement('button');
  reset.textContent = '↺';
  reset.title = 'Back to the base\'s';
  reset.className = 'reset';
  reset.onclick = () => { current = null; onChange(null); redraw(); };
  wrap.append(reset);
  redraw();
  return wrap;
}

/// The game's weapon models (w###) for a weapon's graphId; the number is the model's.
function weaponModelPicker(value, baseValue, onChange) {
  const wrap = document.createElement('span');
  wrap.className = 'field-pick';
  const select = document.createElement('select');
  const fill = (models) => {
    select.textContent = '';
    const base = document.createElement('option');
    base.value = ''; base.textContent = `(base: w${String(baseValue).padStart(3, '0')})`;
    select.append(base);
    const weapons = (models || []).map(m => m.name).filter(n => /(^|\/)w\d{3}\.nmdp/i.test(n)).map(n => n.match(/w(\d{3})/i)[1]).sort();
    for (const num of weapons) {
      const o = document.createElement('option');
      o.value = String(parseInt(num, 10)); o.textContent = 'w' + num;
      select.append(o);
    }
    select.value = value === null || value === undefined ? '' : String(value);
    if (select.value === '' && value !== null && value !== undefined) { const o = document.createElement('option'); o.value = String(value); o.textContent = 'w' + String(value).padStart(3, '0') + ' (no such model)'; select.append(o); select.value = String(value); }
  };
  if (state.models) fill(state.models);
  else api('/api/models').then(list => { state.models = list; fill(list); }).catch(() => fill([]));
  select.onchange = () => onChange(select.value === '' ? null : parseInt(select.value, 10));
  const open = document.createElement('button');
  open.textContent = 'View';
  open.title = 'The model in the model viewer';
  open.onclick = () => { const v = select.value === '' ? baseValue : parseInt(select.value, 10); const name = (state.models || []).map(m => m.name).find(n => new RegExp('w' + String(v).padStart(3, '0') + '\\.nmdp', 'i').test(n)); if (name) openDoc('model', name); };
  wrap.append(select, open);
  return wrap;
}

/// An item of a category (the spells for a weapon's spell-on-hit) from the game's list and the mod's.
function itemRefPicker(value, baseValue, category, onChange) {
  const wrap = document.createElement('span');
  wrap.className = 'field-pick';
  const select = document.createElement('select');
  const fill = () => {
    select.textContent = '';
    const items = (state.items || []).filter(i => !category || (i.category || '').toLowerCase().startsWith(category));
    const baseItem = (state.items || []).find(i => i.id === baseValue);
    const base = document.createElement('option');
    base.value = ''; base.textContent = `(base: ${baseItem ? baseItem.name : baseValue === 0 || baseValue === null ? 'none' : baseValue})`;
    select.append(base);
    const none = document.createElement('option');
    none.value = '0'; none.textContent = 'none';
    select.append(none);
    for (const i of items) { const o = document.createElement('option'); o.value = String(i.id); o.textContent = i.name; select.append(o); }
    select.value = value === null || value === undefined ? '' : String(value);
  };
  if (state.items) fill();
  else api('/api/items').then(items => { state.items = items; fill(); }).catch(() => fill());
  select.onchange = () => onChange(select.value === '' ? null : parseInt(select.value, 10));
  wrap.append(select);
  return wrap;
}