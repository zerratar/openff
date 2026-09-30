// Spells: the game's and the mod's own in one list (Mod ▸ Spells), a spell's inspector, New spell…, Test in battle.
//
// A spell is a magic item: the mod's own are defs/items/<id>.json of the magic chain (item-defs.js edits their
// numbers), numbered from 20001; what it looks like when cast is its spell look, defs/spells (the Look card,
// effects-editor.js effectItemLook), and for the game's spells too. New spell… starts one from a game spell - its
// numbers, and if asked a copy of its effect in defs/effects to make its own - and Test in battle starts the client
// with the mod alone, the first hero in a job that casts the spell's school, the spell learned with full charges, in
// a battle: the Magic menu is the next thing.
//
// Server: /api/project/effect/spells (the game's spells, their effects, the looks), /api/project/items (the mod's),
// /api/project/items/new, /api/project/effect/new, /api/project/effect/spell, /api/project/test-spell.

'use strict';

let spellState = { game: [], mine: [], looks: [] };

const SPELL_SCHOOLS = { Black: 'black magic', White: 'white magic', Summon: 'summon' };
const spellSchoolWord = school => SPELL_SCHOOLS[school] || String(school || '').toLowerCase() || 'magic';

/// The list: the mod's spells (the magic items of defs/items), then the game's by school and level.
async function spellsForList() {
  const [spells, items] = await Promise.all([
    api('/api/project/effect/spells').catch(() => null),
    api('/api/project/items').catch(() => null)
  ]);
  spellState.game = (spells && spells.spells) || [];
  spellState.looks = (spells && spells.looks) || [];
  spellState.mine = ((items && items.ok && items.items) || []).filter(d => d.chain === 'magic');
  const lookOf = name => spellState.looks.find(l => String(l.spell || '').toLowerCase() === String(name || '').toLowerCase());
  const order = { Black: 0, White: 1, Summon: 2 };
  const game = spellState.game.slice().sort((a, b) => (order[a.school] ?? 3) - (order[b.school] ?? 3) || a.level - b.level || a.id - b.id);
  return [
    ...spellState.mine.map(d => ({
      name: 'mod:' + d.id, overridden: false, own: true, def: { name: d.name || d.id },
      note: `${d.number} · from ${d.baseName || d.base}` + (lookOf(d.name) ? ' · a look of its own' : '')
    })),
    ...game.map(s => ({
      name: 'game:' + s.id, overridden: false, def: { name: s.name },
      note: `${spellSchoolWord(s.school)} · level ${s.level}` + (lookOf(s.name) ? ' · looks changed' : '')
    }))
  ];
}

/// The inspector for one: a mod's spell as its item definition (with its Look), one of the game's as its facts and Look.
async function spellInspector(box, name) {
  if (name.startsWith('mod:')) {
    const id = name.slice(4);
    // Drawn again while the definition was on its way (the panel redrawn): only the last draw fills it.
    const token = {};
    box.spellDraw = token;
    const r = await api(`/api/project/items?id=${encodeURIComponent(id)}`).catch(() => null);
    if (box.spellDraw !== token) return;
    if (!r || r.ok === false) { const p = document.createElement('p'); p.className = 'none'; p.textContent = (r && r.error) || 'not found'; box.append(p); return; }
    const d = r.item;
    // A spell based on one of the eight summons is a summon of the mod's: its scripts are in Summons.
    const base = Number(d.base !== undefined ? d.base : d.definition && d.definition.base);
    if (base >= 4201 && base <= 4208) {
      const row = document.createElement('div');
      row.className = 'button-row';
      const open = document.createElement('button');
      open.className = 'primary';
      open.textContent = 'Open the summon';
      open.title = 'a summon of the mod\'s (based on ' + (d.baseName || base) + '): its scripts, model and effects in Summons';
      open.onclick = () => openDoc('summon', 'summon/' + (base - 4201) + '/' + id);
      row.append(open);
      box.append(row);
    }
    box.append(spellTestRow(d.number, (spellState.mine.find(m => m.id === id) || {}).baseName, d));
    if (typeof itemDefinitionPanel === 'function') box.append(itemDefinitionPanel(d, () => loadList()));
    return;
  }
  const id = parseInt(name.slice(5), 10);
  const s = spellState.game.find(x => x.id === id);
  if (!s) return;
  const head = document.createElement('div');
  head.className = 'object-head';
  head.append(icon('effect'));
  const title = document.createElement('input');
  title.className = 'object-name';
  title.value = s.name;
  title.readOnly = true;
  head.append(title);
  box.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub spell-note';
  sub.textContent = `The game's ${spellSchoolWord(s.school)}, level ${s.level} (number ${s.id})` + (s.category > 0 ? ` · plays effect ${s.category}/${s.member}` : '');
  box.append(sub);
  const actions = document.createElement('div');
  actions.className = 'button-row';
  const make = document.createElement('button');
  make.className = 'primary';
  make.textContent = 'Make a spell from this…';
  make.title = 'a spell of the mod\'s own that starts as ' + s.name + ': its numbers, and if you like a copy of its effect to change';
  make.onclick = () => newSpellDialog(s.id);
  actions.append(make);
  box.append(actions);
  box.append(spellTestRow(s.id, s.name, s));
  // Its look: the effect it plays and its caster's glow, changed for this mod (defs/spells) - the game's spells too.
  if (typeof effectItemLook === 'function') effectItemLook(box, () => s.name);
}

/// Test in battle: a formation to fight, and the button.
function spellTestRow(number, label, spell) {
  const card = document.createElement('div');
  card.className = 'component spell-test';
  const headRow = document.createElement('div');
  headRow.className = 'behaviour-header';
  headRow.textContent = 'Try it';
  card.append(headRow);
  const row = document.createElement('div');
  row.className = 'behaviour-field';
  const span = document.createElement('span');
  span.textContent = 'Against';
  const pick = document.createElement('select');
  pick.title = 'the formation the battle is against (the mod\'s own included)';
  const first = document.createElement('option'); first.value = '1'; first.textContent = 'Goblin x3'; pick.append(first);
  // Every formation, the game's and the mod's, by what it is.
  api('/api/formations').then(list => {
    pick.textContent = '';
    for (const f of list || []) { const o = document.createElement('option'); o.value = String(f.id); o.textContent = `${f.name}  (${f.id})${f.mod ? '  · mod' : ''}`; pick.append(o); }
    pick.value = '1';
  }).catch(() => {});
  row.append(span, pick);
  card.append(row);
  const go = document.createElement('button');
  go.className = 'wide-button';
  go.textContent = 'Test in battle';
  go.title = 'starts OpenFF with this mod alone: the first hero in a job that casts it, the spell learned with full charges, in a battle - then Magic, and cast it';
  go.onclick = async () => {
    const school = (spell && spell.school) || spellSchoolOf(spell);
    say('starting OpenFF to try ' + (label || 'the spell') + '…');
    const r = await api('/api/project/test-spell', { spell: number, school, formation: parseInt(pick.value, 10) || 1 }).catch(e => ({ ok: false, error: e.message }));
    if (!r.ok) { say(r.error, 'bad'); return; }
    say(`OpenFF is starting: ${r.job}, the spell learned, then the battle - choose Magic and cast it`, 'good');
  };
  card.append(go);
  const note = document.createElement('p');
  note.className = 'sub spell-note';
  note.textContent = 'OpenFF starts with this mod alone, in Ur; close the game when done (a test needs it closed to begin).';
  card.append(note);
  return card;
}

// A mod spell's school: its base spell's (what it started as).
function spellSchoolOf(d) {
  const base = spellState.game.find(s => s.id === (d && d.base));
  return base ? base.school : 'Black';
}

/// New spell…: a name, the game's spell it starts from, and whether to give it a copy of that spell's effect to change.
async function newSpellDialog(baseId) {
  if (!spellState.game.length) await spellsForList();
  const body = dialog('New spell');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A spell of the mod\'s own. It starts as one of the game\'s - its school, level, power, element and targets - and you change what you like after. With its own effect, a copy of that spell\'s effect is made for it and opens in the effect editor.';
  body.append(note);
  const name = field(body, 'Name', '', { placeholder: 'Firaja' });
  const label = document.createElement('label');
  label.className = 'dialog-field';
  label.textContent = 'Starts from';
  const pick = document.createElement('select');
  const bySchool = new Map();
  for (const s of spellState.game) { if (!bySchool.has(s.school)) bySchool.set(s.school, []); bySchool.get(s.school).push(s); }
  for (const [school, list] of bySchool) {
    const g = document.createElement('optgroup');
    g.label = spellSchoolWord(school);
    for (const s of list.sort((a, b) => a.level - b.level || a.id - b.id)) { const o = document.createElement('option'); o.value = String(s.id); o.textContent = `${s.name}  (level ${s.level})`; g.append(o); }
    pick.append(g);
  }
  pick.value = String(baseId || 4101);
  label.append(pick);
  body.append(label);
  const own = document.createElement('label');
  own.className = 'dialog-field toggle';
  const ownBox = document.createElement('input');
  ownBox.type = 'checkbox';
  ownBox.checked = true;
  own.append(ownBox, ' its own effect: a copy of that spell\'s, to change');
  body.append(own);
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const n = name.value.trim();
    if (!n) { problem.textContent = 'A spell needs a name.'; return; }
    const base = parseInt(pick.value, 10);
    const from = spellState.game.find(s => s.id === base);
    go.disabled = true;
    try {
      const r = await api('/api/project/items/new', { name: n, base });
      if (!r.ok) throw new Error(r.error);
      let effectName = null;
      if (ownBox.checked && from && from.category > 0) {
        const slug = n.toLowerCase().replace(/[^a-z0-9_-]+/g, '-').replace(/^-+|-+$/g, '') || 'spell';
        const e = await api('/api/project/effect/new', { id: slug, category: from.category, member: from.member > 0 ? from.member : 1 });
        if (!e.ok) throw new Error(e.error);
        effectName = e.name;
        const look = await api('/api/project/effect/spell', { spell: n, effect: shortName(e.name).replace(/\.json$/i, ''), cast: false });
        if (!look.ok) throw new Error(look.error);
      }
      body.close();
      say(`made the spell ${n} (${r.item.number})` + (effectName ? ', with its own effect' : ''), 'good');
      if (state.browse === 'spells') await loadList();
      if (typeof inspectAsset === 'function') inspectAsset('spells', 'mod:' + r.item.id, { reveal: true });
      if (effectName) openDoc('effect', effectName);
    } catch (e) { problem.textContent = e.message; go.disabled = false; }
  };
  actions.append(go);
  body.append(actions);
  name.focus();
}
