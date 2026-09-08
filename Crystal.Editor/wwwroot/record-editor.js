// The game's own records as forms in the inspector: click an item, a spell, a monster or an
// encounter group in Game data and its record opens beside the grid - every field by the
// game's own name with the shipped value greyed in, edits saved into the project's copy of
// the table (an override, as the Tables grid writes; a Steam mod installs it, an OpenFF mod
// carries it in its files). The same field tables the mod's definitions are made with, so
// the two forms read alike.
//
// Server: /api/record?kind=item|monster|party&id=, /api/record/save.

'use strict';

/// Which Game data page's rows open which record kind (the first column is the id).
const RECORD_PAGES = { 'Items': 'item', 'Spells': 'item', 'Monsters': 'monster', 'Encounter groups': 'party' };

/// The inspector's form for one record (the server's Describe shape).
function recordPanel(def, onSaved) {
  const panel = document.createElement('div');
  panel.className = 'scene-object item-def record-form';

  const head = document.createElement('div');
  head.className = 'object-head';
  const title = document.createElement('div');
  title.className = 'object-name static';
  title.textContent = def.kind === 'party' ? `Encounter group ${def.id}` : (def.name || `${def.kind} ${def.id}`);
  head.append(title);
  panel.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `${def.kind === 'item' ? def.chain + ' ' : ''}${def.kind} ${def.id} · ${def.file}${def.overridden ? ' · changed by this project' : ''}`;
  panel.append(sub);

  const card = label => {
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

  let timer = null;
  const pending = { fields: {}, slots: null };
  const save = () => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      const body = { kind: def.kind, id: def.id, fields: pending.fields, slots: pending.slots };
      pending.fields = {};
      try {
        const r = await api('/api/record/save', body);
        if (!r.ok) throw new Error(r.error);
        if (r.refused && r.refused.length) say(`not a field here: ${r.refused.join(', ')}`, 'warn');
        else say(`saved ${shortName(def.file)}`, 'good');
        markOverridden(def.file, true);
        sub.textContent = `${def.kind === 'item' ? def.chain + ' ' : ''}${def.kind} ${def.id} · ${def.file} · changed by this project`;
        if (onSaved) onSaved(r.record);
      } catch (e) { say('record: ' + e.message, 'bad'); }
    }, 350);
  };

  if (def.kind === 'party') {
    const slots = card('Slots - up to four; each brings min to max of its monster');
    const model = def.slots.map(s => ({ monster: s.monster, min: s.min, max: s.max }));
    pending.slots = model;
    model.forEach((slot, i) => {
      const r = document.createElement('div');
      r.className = 'formation-slot';
      const pick = typeof monsterPicker === 'function'
        ? monsterPicker(slot.monster, id => { slot.monster = id; save(); }, { none: true })
        : document.createElement('span');
      const min = document.createElement('input');
      min.type = 'number'; min.min = '0'; min.max = '6'; min.value = slot.min;
      min.placeholder = String(def.slots[i].shippedMin);
      min.oninput = () => { slot.min = parseInt(min.value, 10) || 0; save(); };
      const max = document.createElement('input');
      max.type = 'number'; max.min = '0'; max.max = '6'; max.value = slot.max;
      max.placeholder = String(def.slots[i].shippedMax);
      max.oninput = () => { slot.max = parseInt(max.value, 10) || 0; save(); };
      const dash = document.createElement('i');
      dash.textContent = '–';
      r.append(pick, min, dash, max);
      r.title = def.slots[i].shippedMonster >= 0 ? `Shipped: monster ${def.slots[i].shippedMonster}, ${def.slots[i].shippedMin}-${def.slots[i].shippedMax}` : 'Shipped: empty';
      slots.append(r);
    });
    const note = document.createElement('p');
    note.className = 'none';
    note.textContent = 'A slot with no monster or a max of 0 is empty. The battle draws each slot\'s count between min and max; six fighters at most, three of the medium size.';
    slots.append(note);
  } else {
    // The words, read-only here: they live in the text files.
    if (def.name !== undefined) {
      const words = card(def.kind === 'item' ? 'Name and caption' : 'Name');
      const name = document.createElement('p');
      name.className = 'sub';
      name.textContent = `"${def.name || '?'}" - message ${def.nameId}` + (def.kind === 'item' && def.caption ? ` · "${def.caption}" - message ${def.captionId}` : '');
      words.append(name);
      const open = document.createElement('button');
      open.textContent = 'Edit the words in Text';
      open.title = def.kind === 'item' ? 'eureka_item.msd holds the names and captions' : 'eureka_battle.msd holds the monsters\' names';
      open.onclick = () => openDoc('text', def.kind === 'item' ? 'en.lproj/eureka_item.msd' : 'en.lproj/eureka_battle.msd');
      words.append(open);
    }
    // The fields by block, the shipped value greyed in.
    const groups = new Map();
    for (const f of def.fields || []) {
      const key = f.group || 'record';
      if (!groups.has(key)) groups.set(key, []);
      groups.get(key).push(f);
    }
    const labels = { record: 'Record - the game\'s fields', shop: 'Shop', magic: 'Magic', head: 'Level and HP', body: 'Body - stats', attack: 'Attack', defence: 'Defence', 'magic defence': 'Magic defence', special: 'Special actions', drops: 'Drops - exp, gil, items' };
    for (const [group, list] of groups) {
      const c = card(labels[group] || group);
      for (const f of list) {
        const input = document.createElement('input');
        input.type = 'number';
        input.value = f.value === null || f.value === undefined ? '' : f.value;
        input.placeholder = f.shipped === null || f.shipped === undefined ? '' : String(f.shipped);
        if (f.shipped !== null && f.shipped !== undefined && f.value !== f.shipped) input.classList.add('changed');
        input.oninput = () => {
          const v = input.value === '' ? (f.shipped ?? 0) : parseInt(input.value, 10);
          if (Number.isNaN(v)) return;
          pending.fields[f.name] = v;
          input.classList.toggle('changed', f.shipped !== null && f.shipped !== undefined && v !== f.shipped);
          save();
        };
        row(c, f.name, input, `The game's field by its own name; shipped value ${f.shipped ?? '?'}. Clear it to put the shipped value back.`);
      }
    }
  }

  const actions = document.createElement('div');
  actions.className = 'component';
  const grid = document.createElement('button');
  grid.className = 'wide-button';
  grid.textContent = 'Open the table (every byte)';
  grid.title = `${def.file} in the Tables grid`;
  grid.onclick = () => openDoc('table', def.file);
  actions.append(grid);
  const revert = document.createElement('button');
  revert.className = 'wide-button';
  revert.textContent = `Revert ${shortName(def.file)} to the shipped file`;
  revert.title = 'Throws away every change this project made to the whole table, not just this record';
  revert.onclick = async () => {
    if (!confirm(`Put ${def.file} back as the game shipped it? Every record changed in it goes back.`)) return;
    const r = await api('/api/revert', { name: def.file });
    if (!r.ok) { say('not reverted', 'bad'); return; }
    markOverridden(def.file, false);
    say(`${shortName(def.file)} reverted`, 'good');
    inspectAsset('record', `${def.kind}:${def.id}`);
  };
  actions.append(revert);
  panel.append(actions);
  return panel;
}
