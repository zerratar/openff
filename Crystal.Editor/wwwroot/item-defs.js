// The mod's item definitions in the editor: defs/items/<id>.json each - an item of the
// mod's own that starts from one of the game's (its base) and changes what it names. The
// inspector shows one as a form laid out like a scene object's: the name and the caption,
// the base item, the prices, then every field of the base's record by the game's own name
// with the base's value greyed in, so a field left alone stays the base's. Every change
// saves itself (the file is small; there is no Save). New item… asks for a name and a base
// and gives the definition the next number free - what the game will know it by for good.
//
// Server: /api/project/items (list, ?id= one), /api/project/items/save, /new, /delete.

/// The tree's count beside Items, after a definition came or went.
async function itemsCountChanged() {
  try {
    const r = await api('/api/project/items');
    if (typeof projectState !== 'undefined' && projectState.project) projectState.project.items = (r.items || []).length;
    if (typeof drawProjectTree === 'function') drawProjectTree();
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
  note.textContent = 'Each field by the game\'s own name (itm.*Parameter), the base\'s value greyed in. Set one to change it; clear it to go back to the base\'s. usedPower is a consumable\'s effect strength, aggressivity a weapon\'s attack, phylacticPower armour\'s defence, equipJob the job bits.';
  fields.append(note);
  for (const f of def.fields || []) {
    if (f.name === 'buy' || f.name === 'price') continue;
    const input = number(f.value, f.baseValue, v => { if (v === null) delete model.fields[f.name]; else model.fields[f.name] = v; save(); });
    row(fields, f.name, input);
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
