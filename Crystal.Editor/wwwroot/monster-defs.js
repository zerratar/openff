// The mod's monsters and formations in the editor. A monster (defs/monsters/<id>.json)
// starts from one of the game's - its base, whose family gives the battle model - and
// changes what it names: the name, whose texture it wears (a monster of the same family),
// and any field of the record by the game's own name, the base's value greyed in. A
// formation (defs/formations/<id>.json) is up to four slots of a monster with a count,
// the game's or the mod's - what an Encounter or a startBattle fights. Every change saves
// itself, as the items do.
//
// Server: /api/project/monsters (list, ?id=), /save, /new, /delete; the same under
// /api/project/formations; /api/monsters and /api/formations for the pickers.

'use strict';

/// The game's monsters and the mod's, cached for the session.
async function monstersForPicker() {
  if (!state.monsters) state.monsters = await api('/api/monsters');
  return state.monsters || [];
}

/// A select of monsters: the game's (grouped by family) and, unless gameOnly, the mod's.
function monsterPicker(current, onPick, { gameOnly = false, family = null, none = false } = {}) {
  const pick = document.createElement('select');
  pick.className = 'item-base-pick';
  const fill = list => {
    pick.textContent = '';
    if (none) {
      const o = document.createElement('option');
      o.value = '-1';
      o.textContent = '(none)';
      pick.append(o);
    }
    const shown = list.filter(m => (!gameOnly || !m.mod) && (family === null || m.family === family));
    const game = shown.filter(m => !m.mod), mod = shown.filter(m => m.mod);
    const group = (label, items) => {
      if (!items.length) return;
      const g = document.createElement('optgroup');
      g.label = label;
      for (const m of items) {
        const o = document.createElement('option');
        o.value = String(m.id);
        o.textContent = `${m.name}  (${m.id}${m.level ? ', L' + m.level : ''}${m.hp ? ', ' + m.hp + ' HP' : ''})`;
        g.append(o);
      }
      pick.append(g);
    };
    group('the game\'s', game);
    group('the mod\'s', mod);
    pick.value = String(current === null || current === undefined ? -1 : current);
  };
  monstersForPicker().then(fill).catch(() => {});
  pick.onchange = () => onPick(parseInt(pick.value, 10));
  return pick;
}

/// The tree's counts after a definition came or went (the items' helper covers the rest).
async function monstersCountChanged() {
  try {
    const m = await api('/api/project/monsters');
    const f = await api('/api/project/formations');
    if (typeof projectState !== 'undefined' && projectState.project) { projectState.project.monsters = (m.monsters || []).length; projectState.project.formations = (f.formations || []).length; }
    state.monsters = null;
    state.formations = null;
    if (typeof drawProjectTree === 'function') drawProjectTree();
  } catch (e) { /* the count is a nicety */ }
}

// ------------------------------------------------------------------ monsters

/// The inspector's form for one monster definition (the server's Describe shape).
function monsterDefinitionPanel(def, onSaved) {
  const panel = document.createElement('div');
  panel.className = 'scene-object item-def';
  const model = {
    id: def.id, number: def.number, base: def.base, name: def.name || '', look: def.look || 0,
    fields: Object.fromEntries((def.fields || []).filter(f => f.value !== null && f.value !== undefined).map(f => [f.name, f.value]))
  };
  let timer = null;
  const save = () => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      try {
        const r = await api('/api/project/monsters/save', model);
        if (!r.ok) throw new Error(r.error);
        monstersCountChanged();
        if (onSaved) onSaved(r.monster);
      } catch (e) { say('monster: ' + e.message, 'bad'); }
    }, 250);
  };

  const head = document.createElement('div');
  head.className = 'object-head';
  const title = document.createElement('input');
  title.type = 'text';
  title.className = 'object-name';
  title.value = model.name;
  title.placeholder = 'the monster\'s name, as the battle shows it';
  title.oninput = () => { model.name = title.value; save(); };
  head.append(title);
  panel.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `monster ${def.number} · ${def.file}`;
  sub.title = 'The number is the monster id the game knows it by - a formation names it, the bestiary would - given once, kept for good.';
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
  const number = (value, placeholder, onChange) => {
    const n = document.createElement('input');
    n.type = 'number';
    n.value = value === null || value === undefined ? '' : value;
    n.placeholder = placeholder === null || placeholder === undefined ? '' : String(placeholder);
    n.oninput = () => onChange(n.value === '' ? null : parseInt(n.value, 10));
    return n;
  };

  // ---- what it is: the base and the look
  const what = card('Monster');
  const basePick = monsterPicker(model.base, id => { if (id >= 0) { model.base = id; model.look = 0; save(); setTimeout(() => inspectAsset('monsters', model.id), 400); } }, { gameOnly: true });
  row(what, 'Based on', basePick, 'The game\'s monster this one starts from: its family is the battle model, its record the values below');
  const family = document.createElement('p');
  family.className = 'none';
  family.textContent = def.baseName ? `${def.baseName}, family ${def.family}: the battle model is the family's` : `no monster ${def.base} in the game's tables`;
  what.append(family);
  const lookPick = document.createElement('select');
  lookPick.className = 'item-base-pick';
  const own = document.createElement('option');
  own.value = '0';
  own.textContent = `the base's (${def.baseName || def.base})`;
  lookPick.append(own);
  for (const m of def.familyMonsters || []) {
    if (m.id === def.base) continue;
    const o = document.createElement('option');
    o.value = String(m.id);
    o.textContent = `${m.name}'s  (${m.id})`;
    lookPick.append(o);
  }
  lookPick.value = String(model.look || 0);
  lookPick.onchange = () => { model.look = parseInt(lookPick.value, 10) || 0; save(); };
  row(what, 'Look', lookPick, 'Whose texture it wears: the family\'s model with another family member\'s colours - a recolour one number away');

  // ---- the record's fields, by block
  const groups = new Map();
  for (const f of def.fields || []) {
    if (f.name === 'familyId' || f.name === 'textId' || f.name === 'modelId' || f.name === 'drawMapId') continue;
    const key = f.group || 'record';
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key).push(f);
  }
  const labels = { head: 'Level and HP', body: 'Body - stats', attack: 'Attack', defence: 'Defence', 'magic defence': 'Magic defence', special: 'Special actions', drops: 'Drops - exp, gil, items' };
  const tips = {
    level: 'The monster\'s level', size: '0 small (six fit), 1 medium (three), 2 large', maxHp: 'Hit points',
    aggressivity: 'Attack power', hitProbability: 'Hit %', phylacticPower: 'Defence', avoidanceNumber: 'Evasion',
    magicPhylacticPower: 'Magic defence', weakType: 'Weakness bits', dropProbability: 'Drop chance', dropTable: 'Which drop table (monster.chaindata chain 1)', gil: 'Gil on defeat', exp: 'Experience on defeat',
    special1Id: 'The first special action (its id in chain 3)', special1Probability: 'How often, %', special1StartHp: 'Used once HP falls to this',
    actionNumber: 'How many actions a turn', devide: 'Divides when hit (Zombies)', aiLevel: 'How cleverly it picks', magicSkill: 'Magic skill', weight: 'Weight'
  };
  for (const [group, list] of groups) {
    const c = card(labels[group] || group);
    for (const f of list) {
      const input = number(f.value, f.baseValue, v => { if (v === null) delete model.fields[f.name]; else model.fields[f.name] = v; save(); });
      row(c, f.name, input, tips[f.name] || 'The game\'s field, by its own name; blank keeps the base\'s value');
    }
  }
  const note = document.createElement('p');
  note.className = 'none';
  note.textContent = 'Each field by the game\'s own name (mon.MonsterParameter), the base\'s value greyed in. Set one to change it; clear it to go back to the base\'s.';
  panel.append(note);

  // ---- the file, and the end
  const actions = document.createElement('div');
  actions.className = 'component';
  const json = document.createElement('button');
  json.className = 'wide-button';
  json.textContent = 'Open as JSON';
  json.title = `${def.file} as text - what Export to OpenFF copies into the mod`;
  json.onclick = () => openDoc('code', def.file);
  actions.append(json);
  const formation = document.createElement('button');
  formation.className = 'wide-button';
  formation.textContent = 'New formation with it…';
  formation.title = 'A formation of the mod\'s own with this monster in its first slot - what an Encounter fights';
  formation.onclick = () => newFormationDialog(model.number);
  actions.append(formation);
  const remove = document.createElement('button');
  remove.className = 'wide-button';
  remove.textContent = 'Delete this monster';
  remove.title = 'Removes the definition file. A formation that names it would then name a monster the game has no record for.';
  remove.onclick = async () => {
    if (!confirm(`Delete the monster "${model.name || model.id}" (${def.file})?`)) return;
    const r = await api('/api/project/monsters/delete', { id: model.id });
    if (!r.ok) { say('monster: not deleted', 'bad'); return; }
    if (typeof clearInspected === 'function') clearInspected();
    monstersCountChanged();
    loadList();
    say(`"${model.name || model.id}" deleted`, 'good');
  };
  actions.append(remove);
  panel.append(actions);
  return panel;
}

/// New monster: a name and a base; the definition is written and opened in the inspector.
function newMonsterDialog() {
  if (typeof dialog !== 'function') return;
  const body = dialog('New monster');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A monster of the mod\'s own. It starts as a copy of one of the game\'s - pick the one it should look like: the base\'s family is the battle model - and changes what you set afterwards: the name, whose colours it wears, level, HP, attack, drops.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: 'Goblin Chief' });
  section(body, 'Based on');
  let base = 1;
  body.append(monsterPicker(base, id => { base = id; }, { gameOnly: true }));
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const n = name.value.trim();
    if (!n) { problem.textContent = 'A monster needs a name.'; return; }
    try {
      const r = await api('/api/project/monsters/new', { name: n, base });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      monstersCountChanged();
      if (browseKind !== 'monsters') selectKind('monsters'); else await loadList();
      inspectAsset('monsters', r.monster.id);
      say(`"${n}" is monster ${r.monster.number} of the mod`, 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  name.onkeydown = e => { if (e.key === 'Enter') go.click(); };
  actions.append(go);
  body.append(actions);
  setTimeout(() => name.focus(), 0);
}

// ---------------------------------------------------------------- formations

/// The inspector's form for a formation: the name and up to four slots of monster, min, max.
function formationPanel(def, onSaved) {
  const panel = document.createElement('div');
  panel.className = 'scene-object item-def';
  const model = { id: def.id, number: def.number, name: def.name || '', slots: (def.slots || []).map(s => ({ monster: s.monster, min: s.min, max: s.max })) };
  let timer = null;
  const save = () => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      try {
        const r = await api('/api/project/formations/save', model);
        if (!r.ok) throw new Error(r.error);
        monstersCountChanged();
        if (onSaved) onSaved(r.formation);
      } catch (e) { say('formation: ' + e.message, 'bad'); }
    }, 250);
  };

  const head = document.createElement('div');
  head.className = 'object-head';
  const title = document.createElement('input');
  title.type = 'text';
  title.className = 'object-name';
  title.value = model.name;
  title.placeholder = 'a name for the editor - the game shows the monsters\'';
  title.oninput = () => { model.name = title.value; save(); };
  head.append(title);
  panel.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = `formation ${def.number} · ${def.file}`;
  sub.title = 'The number is the monster party id: an Encounter\'s Formation, Game.Battle.Start(number), a CastScript\'s battle command.';
  panel.append(sub);

  const slots = document.createElement('div');
  slots.className = 'component';
  const h = document.createElement('div');
  h.className = 'behaviour-header';
  h.textContent = 'Slots - up to four; each brings min to max of its monster (six fighters at most, three of the medium size)';
  slots.append(h);
  const draw = () => {
    slots.querySelectorAll('.formation-slot').forEach(e => e.remove());
    model.slots.forEach((slot, i) => {
      const r = document.createElement('div');
      r.className = 'formation-slot';
      const pick = monsterPicker(slot.monster, id => { slot.monster = id; save(); });
      const min = document.createElement('input');
      min.type = 'number'; min.min = '0'; min.max = '6'; min.value = slot.min; min.title = 'At least this many';
      min.oninput = () => { slot.min = parseInt(min.value, 10) || 0; save(); };
      const max = document.createElement('input');
      max.type = 'number'; max.min = '1'; max.max = '6'; max.value = slot.max; max.title = 'At most this many (the count is drawn between)';
      max.oninput = () => { slot.max = Math.max(1, parseInt(max.value, 10) || 1); save(); };
      const x = document.createElement('button');
      x.className = 'shut';
      x.textContent = '×';
      x.title = 'Remove this slot';
      x.onclick = () => { model.slots.splice(i, 1); draw(); save(); };
      const dash = document.createElement('i');
      dash.textContent = '–';
      r.append(pick, min, dash, max, x);
      slots.append(r);
    });
    add.disabled = model.slots.length >= 4;
  };
  const add = document.createElement('button');
  add.textContent = 'Add slot';
  add.onclick = () => { model.slots.push({ monster: 1, min: 1, max: 1 }); draw(); save(); };
  slots.append(add);
  panel.append(slots);
  draw();

  const actions = document.createElement('div');
  actions.className = 'component';
  const json = document.createElement('button');
  json.className = 'wide-button';
  json.textContent = 'Open as JSON';
  json.onclick = () => openDoc('code', def.file);
  actions.append(json);
  const remove = document.createElement('button');
  remove.className = 'wide-button';
  remove.textContent = 'Delete this formation';
  remove.onclick = async () => {
    if (!confirm(`Delete the formation "${model.name || model.id}" (${def.file})?`)) return;
    const r = await api('/api/project/formations/delete', { id: model.id });
    if (!r.ok) { say('formation: not deleted', 'bad'); return; }
    if (typeof clearInspected === 'function') clearInspected();
    monstersCountChanged();
    loadList();
    say(`"${model.name || model.id}" deleted`, 'good');
  };
  actions.append(remove);
  panel.append(actions);
  return panel;
}

/// New formation: a name and a first monster.
function newFormationDialog(firstMonster) {
  if (typeof dialog !== 'function') return;
  const body = dialog('New formation');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A monster party of the mod\'s own: up to four slots of a monster with a count. An Encounter placed on a map fights it; so does Game.Battle.Start(number) from C#.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: 'Goblin Chief and two Goblins' });
  section(body, 'First monster');
  let monster = firstMonster === undefined || firstMonster === null ? 1 : firstMonster;
  body.append(monsterPicker(monster, id => { monster = id; }));
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const n = name.value.trim();
    if (!n) { problem.textContent = 'A formation needs a name.'; return; }
    try {
      const r = await api('/api/project/formations/new', { name: n, monsters: [monster] });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      monstersCountChanged();
      if (browseKind !== 'formations') selectKind('formations'); else await loadList();
      inspectAsset('formations', r.formation.id);
      say(`"${n}" is formation ${r.formation.number} of the mod`, 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  name.onkeydown = e => { if (e.key === 'Enter') go.click(); };
  actions.append(go);
  body.append(actions);
  setTimeout(() => name.focus(), 0);
}
