// The mod's job ladders in the editor: defs/jobs/<id>.json each - what a job teaches a hero
// on the mastery progression (FF5's way; Shared/Data/ModJobs.cs). The inspector shows one as
// a form: which job, its four battle commands (any of them a free slot), the passives it has
// from the start, and the ladder itself - steps of so many ABP each handing out an ability,
// FF3's by name or a passive of the mod's own. Every change saves itself. New ladder… picks
// a job that has none yet and starts it with the game's own commands, the third made free.
//
// Server: /api/project/jobs (list, ?id= one), /api/project/jobs/save, /new, /delete.

async function jobsCountChanged() {
  try {
    const j = await api('/api/project/jobs');
    if (typeof projectState !== 'undefined' && projectState.project) projectState.project.jobs = (j.ladders || []).length;
    if (typeof drawProjectTree === 'function') drawProjectTree();
  } catch (e) { /* the count is a nicety */ }
}

/// A select of the abilities a step or command can name. kind: 'command' (FF3's commands),
/// 'passive' (FF3's passives and the project's own), or 'any'. extra: an entry to put first.
function abilityPick(abilities, current, kind, onPick, ...extras) {
  const pick = document.createElement('select');
  pick.className = 'item-base-pick';
  for (const extra of extras.filter(Boolean)) {
    const o = document.createElement('option');
    o.value = extra.value;
    o.textContent = extra.label;
    pick.append(o);
  }
  const groups = [['Commands', a => !a.passive && !a.own], ['Passives', a => a.passive && !a.own], ['This mod\u2019s passives', a => a.own && a.passive], ['This mod\u2019s commands', a => a.own && !a.passive]];
  for (const [label, test] of groups) {
    const list = abilities.filter(a => test(a) && (kind === 'any' || (kind === 'command' ? !a.passive : a.passive)));
    if (!list.length) continue;
    const g = document.createElement('optgroup');
    g.label = label;
    for (const a of list) {
      const o = document.createElement('option');
      o.value = a.word;
      o.textContent = a.name + (a.works || a.own ? '' : '  (idle: the battle has no code for it)') + (a.word === 'black-magic' || a.word === 'white-magic' || a.word === 'magic' ? '  (' + a.word + ')' : '');
      g.append(o);
    }
    pick.append(g);
  }
  pick.value = current || (extra ? extra.value : '');
  if (pick.value !== (current || (extra ? extra.value : ''))) {
    // A word none of the catalogue's (a new passive typed in): keep it as its own option.
    const o = document.createElement('option');
    o.value = current;
    o.textContent = current;
    pick.append(o);
    pick.value = current;
  }
  pick.onchange = () => onPick(pick.value);
  return pick;
}

/// The inspector's form for a ladder. data = { ladder, jobs, abilities, gameCommands } from /api/project/jobs?id=.
function jobLadderPanel(data, onSaved) {
  const def = data.ladder;
  let abilities = data.abilities || [];
  const panel = document.createElement('div');
  panel.className = 'scene-object item-def';
  const model = {
    id: def.id, job: def.job === null || def.job === undefined ? '' : String(def.job), name: def.name || '',
    own: !!def.own, base: def.base === null || def.base === undefined ? '' : String(def.base), look: def.look === null || def.look === undefined ? '' : String(def.look),
    commands: (def.commands || []).slice(), innate: (def.innate || []).slice(), inherits: !!def.inherits,
    stats: Object.fromEntries((def.stats || []).map(s => [s.word, s.value | 0])),
    abilities: (def.abilities || []).map(a => ({ abp: a.abp, ability: a.ability || '', name: a.name || '', passive: !!a.passive, command: !!a.command, grants: (a.grants || []).slice(), carries: !!a.carries }))
  };
  let timer = null;
  const save = (then) => {
    clearTimeout(timer);
    timer = setTimeout(async () => {
      try {
        const body = { id: model.id, name: model.name };
        if (model.own) { body.base = model.base; if (model.look !== '' && model.look !== model.base) body.look = model.look; }
        else body.job = model.job;
        if (model.commands.length) body.commands = model.commands;
        if (model.innate.length) body.innate = model.innate;
        if (model.inherits) body.inherits = true;
        const stats = Object.fromEntries(Object.entries(model.stats).filter(([, v]) => v));
        if (Object.keys(stats).length) body.stats = stats;
        body.abilities = model.abilities.map(a => {
          const o = { abp: a.abp, ability: a.ability };
          if (a.name) o.name = a.name;
          if (a.passive) o.passive = true;
          if (a.command) o.command = true;
          if (a.grants.length) o.grants = a.grants;
          if (a.carries) o.carries = true;
          return o;
        });
        const r = await api('/api/project/jobs/save', body);
        if (!r.ok) throw new Error(r.error);
        if (r.abilities) abilities = r.abilities;
        if (onSaved) onSaved(r.ladder);
        if (then) then(r.ladder);
      } catch (e) { say('ladder: ' + e.message, 'bad'); }
    }, 250);
  };

  const head = document.createElement('div');
  head.className = 'object-head';
  const title = document.createElement('input');
  title.type = 'text';
  title.className = 'object-name';
  title.value = model.name;
  title.placeholder = def.jobName || 'name';
  title.title = 'The ladder\'s name in the Abilities menu; the job\'s own when empty';
  title.oninput = () => { model.name = title.value; save(); };
  head.append(title);
  panel.append(head);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = def.file;
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
  const row = (into, label, input, tip) => {
    const r = document.createElement('div');
    r.className = 'behaviour-field';
    const l = document.createElement('span');
    l.textContent = label;
    if (tip) r.title = tip;
    r.append(l, input);
    into.append(r);
    return r;
  };

  // ---- the job and its commands
  const jobCard = card(model.own ? 'A job of the mod\u2019s own' : 'The job');
  const jobPick = (current, onPick, none) => {
    const s = document.createElement('select');
    if (none) { const o = document.createElement('option'); o.value = ''; o.textContent = none; s.append(o); }
    for (const j of data.jobs) {
      const o = document.createElement('option');
      o.value = String(j.number);
      o.textContent = `${j.name}  (${j.number})`;
      s.append(o);
    }
    s.value = current;
    s.onchange = () => onPick(s.value);
    return s;
  };
  if (model.own) {
    const ownNote = document.createElement('p');
    ownNote.className = 'sub';
    ownNote.textContent = `Job ${def.number} in play - none of FF3\u2019s. It stands on an FF3 base: the game\u2019s party holds the base (growth, magic charges, equipment, battle motions) while the hero really has this job - its name, commands, ladder and stats. A hero takes it from the Abilities menu\u2019s Change job page once the base is open, or Game.Party.ChangeJob(id, "${def.id}").`;
    jobCard.append(ownNote);
    row(jobCard, 'Base', jobPick(model.base, v => { model.base = v; save(); }), 'The FF3 job it stands on');
    row(jobCard, 'Look', jobPick(model.look, v => { model.look = v; save(); }, 'the base\u2019s figures'), 'Whose figures it wears on the field, in battle and the menus (j<hero><job> models) - until someone models new ones');
  } else {
    row(jobCard, 'Job', jobPick(model.job, v => { model.job = v; save(); }), 'Which of FF3\'s jobs this ladder is for: its stats, models and equipment stay the game\'s');
  }
  const inherits = document.createElement('input');
  inherits.type = 'checkbox';
  inherits.checked = model.inherits;
  inherits.onchange = () => { model.inherits = inherits.checked; save(); };
  row(jobCard, 'Inherits', inherits, 'FF5\'s Freelancer and Mime: a hero holding this job has the innate passives of every job they have mastered (climbed to the top of)');
  const cmdNote = document.createElement('p');
  cmdNote.className = 'sub';
  cmdNote.textContent = 'The four battle commands of a hero holding the job. A free slot (*) takes any ability the hero has learned from any ladder; FF5 keeps Attack, the job\'s command and Item and frees one - two for the Freelancer, three for the Mime.';
  jobCard.append(cmdNote);
  const baseNumber = model.own ? model.base : model.job;
  const gameSet = (data.gameCommands && baseNumber !== '' && data.gameCommands[parseInt(baseNumber, 10)]) || ['attack', 'magic', 'guard', 'item'];
  for (let i = 0; i < 4; i++) {
    const current = (model.commands[i] || '').trim() || gameSet[i];
    const pick = abilityPick(abilities, current === '*' ? '*' : current, 'command', v => {
      while (model.commands.length < 4) model.commands.push('');
      model.commands[i] = v;
      save();
    }, { value: '*', label: '* free slot' });
    row(jobCard, 'Command ' + (i + 1), pick, i === 0 ? 'Attack, as the game has it' : i === 3 ? 'Item, as the game has it' : 'The game\'s own is ' + gameSet[i]);
  }

  // ---- FF5's stat modifiers
  const statsCard = card('Stat modifiers');
  const statsNote = document.createElement('p');
  statsNote.className = 'sub';
  statsNote.textContent = 'FF5\'s way: added to a hero\'s stats while they hold the job, on top of FF3\'s growth (a Knight +5 strength, -3 intellect). A job that inherits takes the best positive modifier of every mastered job; penalties never pass on. FF3\'s stats run 5 to about 30 by level 30, so a few points go a long way.';
  statsCard.append(statsNote);
  for (const [word, label, tip] of [['strength', 'Strength', 'Physical damage'], ['agility', 'Agility', 'Hits, evasion, turn order'], ['vitality', 'Vitality', 'HP gained at each level'], ['intellect', 'Intellect', 'Black magic; FF5\'s Magic is this and Mind'], ['mind', 'Mind', 'White magic']]) {
    const n = document.createElement('input');
    n.type = 'number';
    n.min = -30;
    n.max = 30;
    n.value = model.stats[word] || 0;
    n.oninput = () => { model.stats[word] = parseInt(n.value, 10) || 0; save(); };
    row(statsCard, label, n, tip);
  }

  // ---- innate passives
  const innateCard = card('Innate passives');
  const innateNote = document.createElement('p');
  innateNote.className = 'sub';
  innateNote.textContent = 'Passives the job has from the start, on top of the game\'s own (Knight\'s Cover, Scholar\'s Alchemy). Cover and Alchemy act in battle; the others are flags for scripts and grants.';
  innateCard.append(innateNote);
  const drawInnate = () => {
    innateCard.querySelectorAll('.behaviour-field, .wide-button').forEach(e => e.remove());
    model.innate.forEach((w, i) => {
      const pick = abilityPick(abilities, w, 'passive', v => { model.innate[i] = v; save(); });
      const r = row(innateCard, 'Innate ' + (i + 1), pick);
      const x = document.createElement('button');
      x.className = 'mini';
      x.textContent = '\u00d7';
      x.title = 'Take it out';
      x.onclick = () => { model.innate.splice(i, 1); save(); drawInnate(); };
      r.append(x);
    });
    const add = document.createElement('button');
    add.className = 'wide-button';
    add.textContent = '+ innate passive';
    add.onclick = () => {
      const first = abilities.find(a => a.passive);
      model.innate.push(first ? first.word : 'cover');
      save();
      drawInnate();
    };
    innateCard.append(add);
  };
  drawInnate();

  // ---- the ladder
  const ladderCard = card('The ladder');
  const ladderNote = document.createElement('p');
  ladderNote.className = 'sub';
  ladderNote.textContent = 'Each step costs so many ABP from the one before and teaches an ability for good. A passive of the mod\'s own is a word of your choosing with a name; it has no effect of the engine\'s beyond its grants and what scripts make of it (PartyMember.Abilities). Grants lend the hero the equipment and magic permissions of other jobs while the ability is set or innate - FF5\'s "Equip Swords"; a magic command grants this ladder\'s job unless told otherwise.';
  ladderCard.append(ladderNote);
  const steps = document.createElement('div');
  ladderCard.append(steps);
  const total = document.createElement('p');
  total.className = 'sub';
  ladderCard.append(total);
  const drawSteps = () => {
    steps.textContent = '';
    let sum = 0;
    model.abilities.forEach((a, i) => {
      sum += Math.max(0, a.abp | 0);
      const box = document.createElement('div');
      box.className = 'ladder-step';
      const headRow = document.createElement('div');
      headRow.className = 'ladder-head';
      const label = document.createElement('span');
      label.textContent = 'Level ' + (i + 1);
      const totalMark = document.createElement('span');
      totalMark.className = 'total';
      totalMark.textContent = sum + ' ABP in all';
      const up = document.createElement('button');
      up.className = 'mini';
      up.textContent = '\u2191';
      up.title = 'Earlier';
      up.disabled = i === 0;
      up.onclick = () => { model.abilities.splice(i - 1, 0, model.abilities.splice(i, 1)[0]); save(); drawSteps(); };
      const down = document.createElement('button');
      down.className = 'mini';
      down.textContent = '\u2193';
      down.title = 'Later';
      down.disabled = i === model.abilities.length - 1;
      down.onclick = () => { model.abilities.splice(i + 1, 0, model.abilities.splice(i, 1)[0]); save(); drawSteps(); };
      const x = document.createElement('button');
      x.className = 'mini';
      x.textContent = '\u00d7';
      x.title = 'Take this step out';
      x.onclick = () => { model.abilities.splice(i, 1); save(); drawSteps(); };
      headRow.append(label, totalMark, up, down, x);
      box.append(headRow);
      const abp = document.createElement('input');
      abp.type = 'number';
      abp.min = 0;
      abp.max = 9999;
      abp.value = a.abp;
      abp.oninput = () => { a.abp = parseInt(abp.value, 10) || 0; save(); };
      row(box, 'Costs (ABP)', abp, 'ABP this step costs from the one before');
      const known = abilities.find(k => k.word === (a.ability || '').trim().toLowerCase().replace(/[^a-z0-9]+/g, '-') || (k.name || '').toLowerCase() === (a.ability || '').toLowerCase());
      const own = !known || known.own;
      const pick = abilityPick(abilities, known ? known.word : a.ability, 'any', v => {
        if (v === '__own') { a.ability = a.ability && own ? a.ability : 'my-passive'; a.passive = true; a.command = false; if (!a.name) a.name = 'My passive'; }
        else if (v === '__command') { a.ability = a.ability && own ? a.ability : 'my-command'; a.passive = false; a.command = true; if (!a.name) a.name = 'My command'; }
        else { a.ability = v; a.passive = false; a.command = false; a.name = ''; }
        save(() => drawSteps());
      }, { value: '__own', label: 'a passive of the mod\u2019s own\u2026' }, { value: '__command', label: 'a battle command of the mod\u2019s own (C#)\u2026' });
      if (own) pick.value = a.command ? '__command' : '__own';
      row(box, 'Ability', pick, 'One of FF3\'s abilities, a passive of the mod\'s own, or a battle command of the mod\'s own - a BattleCommand class in its code, by the same word');
      if (own) {
        const word = document.createElement('input');
        word.type = 'text';
        word.value = a.ability;
        word.placeholder = 'equip-swords';
        word.title = a.command ? 'The word: the BattleCommand class in the code with this Word (its class name in lower-kebab unless it says otherwise)' : 'The word definitions and scripts know it by; the same word across ladders is the same passive';
        word.oninput = () => { a.ability = word.value; a.passive = !a.command; save(); };
        row(box, 'Word', word);
        const name = document.createElement('input');
        name.type = 'text';
        name.value = a.name;
        name.placeholder = 'Equip Swords';
        name.title = 'What the Abilities menu shows';
        name.oninput = () => { a.name = name.value; save(); };
        row(box, 'Name', name);
        if (a.command) {
          const note = document.createElement('p');
          note.className = 'sub';
          note.textContent = 'A command of the mod\'s own: learned and set into a free slot it shows in the battle\'s window under its name and is played as the hero\'s plain attack whose damage a BattleCommand class in the code decides (Name, Target, Damage). Without the class it strikes as a plain attack.';
          box.append(note);
        }
      }
      if (known && !known.works && !known.own) {
        const warn = document.createElement('p');
        warn.className = 'sub warn';
        warn.textContent = known.name + ' is in the game\'s table but the battle has no code for it; learning it shows a command that does nothing.';
        box.append(warn);
      }
      const grants = document.createElement('input');
      grants.type = 'text';
      grants.value = a.grants.join(', ');
      grants.placeholder = known && /magic|summon/.test(known.word) ? (def.jobName || 'this job') + ' (the default for a magic command)' : 'none';
      grants.title = 'Jobs whose equipment and magic the hero may use while this is set or innate, by name, comma-separated: knight, white-mage, thief';
      grants.oninput = () => { a.grants = grants.value.split(',').map(s => s.trim()).filter(Boolean); save(); };
      row(box, 'Grants', grants);
      const carries = document.createElement('input');
      carries.type = 'checkbox';
      carries.checked = !!a.carries;
      carries.onchange = () => { a.carries = carries.checked; save(); };
      row(box, 'Carries stats', carries, 'FF5\'s rule for Equip abilities and spell lists: while set, the hero has this ladder\'s positive stat modifiers where they beat the held job\'s');
      steps.append(box);
    });
    const add = document.createElement('button');
    add.className = 'wide-button';
    add.textContent = '+ step';
    add.onclick = () => {
      const last = model.abilities[model.abilities.length - 1];
      model.abilities.push({ abp: last ? Math.min(9999, Math.round(last.abp * 1.5)) : 10, ability: 'cover', name: '', passive: false, grants: [] });
      save(() => drawSteps());
      drawSteps();
    };
    steps.append(add);
    total.textContent = model.abilities.length ? `${model.abilities.length} step${model.abilities.length === 1 ? '' : 's'}, ${sum} ABP to master. ABP per battle won: the formation's abp, or one per monster.` : 'No steps yet: a hero holding the job earns ABP for nothing. + step adds the first.';
  };
  drawSteps();

  const actions = document.createElement('div');
  actions.className = 'component';
  const json = document.createElement('button');
  json.className = 'wide-button';
  json.textContent = 'Open as JSON';
  json.onclick = () => openDoc('code', def.file);
  actions.append(json);
  const remove = document.createElement('button');
  remove.className = 'wide-button';
  remove.textContent = 'Delete this ladder';
  remove.onclick = async () => {
    if (!confirm(`Delete the ladder "${model.name || model.id}" (${def.file})?`)) return;
    const r = await api('/api/project/jobs/delete', { id: model.id });
    if (!r.ok) { say('ladder: not deleted', 'bad'); return; }
    if (typeof clearInspected === 'function') clearInspected();
    jobsCountChanged();
    loadList();
  };
  actions.append(remove);
  panel.append(actions);
  return panel;
}

function newJobDialog() {
  if (typeof dialog !== 'function') return;
  const body = dialog('New job ladder');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'For heroes on the mastery progression. A ladder for one of FF3\'s jobs, or a job of the mod\'s own - FF5\'s Samurai, Berserker, Time Mage - standing on an FF3 base and wearing an FF3 job\'s figures. Either starts with the base\'s commands, the third made a free slot, and no steps - add them in the inspector after.';
  body.append(note);
  section(body, 'What');
  const which = document.createElement('select');
  which.className = 'item-base-pick';
  for (const [v, l] of [['ff3', 'A ladder for one of FF3\u2019s jobs'], ['own', 'A job of the mod\u2019s own']]) { const o = document.createElement('option'); o.value = v; o.textContent = l; which.append(o); }
  body.append(which);
  const ff3Part = document.createElement('div');
  section(ff3Part, 'Job');
  const pick = document.createElement('select');
  pick.className = 'item-base-pick';
  ff3Part.append(pick);
  body.append(ff3Part);
  const ownPart = document.createElement('div');
  ownPart.hidden = true;
  const name = field(ownPart, 'Name', '', { placeholder: 'Samurai' });
  section(ownPart, 'Stands on (FF3 job)');
  const basePick = document.createElement('select');
  basePick.className = 'item-base-pick';
  ownPart.append(basePick);
  section(ownPart, 'Wears the figures of');
  const lookPick = document.createElement('select');
  lookPick.className = 'item-base-pick';
  const lookNone = document.createElement('option');
  lookNone.value = '';
  lookNone.textContent = 'the base\u2019s';
  lookPick.append(lookNone);
  ownPart.append(lookPick);
  body.append(ownPart);
  which.onchange = () => { ff3Part.hidden = which.value !== 'ff3'; ownPart.hidden = which.value !== 'own'; if (which.value === 'own') name.focus(); };
  const problem = errorLine(body);
  api('/api/project/jobs').then(r => {
    const taken = new Set((r.ladders || []).filter(l => !l.own).map(l => l.job));
    for (const j of r.jobs || []) {
      const o = document.createElement('option');
      o.value = String(j.number);
      o.textContent = `${j.name}  (${j.number})` + (taken.has(j.number) ? '  - has a ladder' : '');
      o.disabled = taken.has(j.number);
      pick.append(o);
      basePick.append(o.cloneNode(true));
      basePick.lastChild.disabled = false;
      basePick.lastChild.textContent = `${j.name}  (${j.number})`;
      lookPick.append(basePick.lastChild.cloneNode(true));
    }
    const free = [...pick.options].find(o => !o.disabled);
    if (free) pick.value = free.value;
  }).catch(e => { problem.textContent = e.message; });
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    try {
      const own = which.value === 'own';
      if (own && !name.value.trim()) { problem.textContent = 'A job of the mod\u2019s own needs a name.'; return; }
      const r = await api('/api/project/jobs/new', own
        ? { name: name.value.trim(), base: parseInt(basePick.value, 10), look: lookPick.value === '' ? -1 : parseInt(lookPick.value, 10) }
        : { job: parseInt(pick.value, 10) });
      if (!r.ok) throw new Error(r.error);
      const shut = document.querySelector('.picker.dialog .shut');
      if (shut) shut.click();
      jobsCountChanged();
      await loadList();
      inspectAsset('jobs', r.ladder.id);
      say(own ? `${r.ladder.name} stands on the ${r.ladder.baseName}` : `${r.ladder.jobName} has a ladder`, 'good');
    } catch (e) { problem.textContent = e.message; }
  };
  name.onkeydown = e => { if (e.key === 'Enter') go.click(); };
  actions.append(go);
  body.append(actions);
}
