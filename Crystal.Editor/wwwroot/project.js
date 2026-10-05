// The menu bar, the start page, and the projects behind them.
//
// A project is one mod: a directory with a name, a note of which games it is for, and
// the edited files under it. Editing already wrote to an override directory - this is
// that directory made into a thing you can name, switch between, and hand to somebody.
//
// A project can target two games, and the server then has a session open for each;
// projectState.workspaces lists them, state.ws (app.js) says which one the page is
// talking about, and everything here that acts on "the game" acts on that one.

'use strict';

let projectState = { project: null, targets: [], mod: null, workspaces: [], active: null, missing: {}, available: [] };

// A target is a game and a kind of mod at once: ours = FF3 in OpenFF, oursff4 = FF4 in
// OpenFF, steam = FF3 on Steam, ff4steam = FF4 on Steam. The page shows the two apart -
// the tabs above the libraries are games; the kind is the project's.
const OURS = ['ours', 'oursff4'];
function isOursTarget(target) { return OURS.includes(target); }
function gameOfTarget(target) { return target === 'oursff4' || target === 'ff4steam' ? 'ff4' : 'ff3'; }
function kindOfTarget(target) { return isOursTarget(target) ? 'openff' : 'steam'; }
function targetFor(game, kind) {
  return kind === 'openff' ? (game === 'ff4' ? 'oursff4' : 'ours') : (game === 'ff4' ? 'ff4steam' : 'steam');
}
/// Whether the project makes an OpenFF mod: it has an OpenFF target (code, scenes, export
/// to the client all hang off this).
function isOpenFFProject(project = projectState.project) {
  return !!project && (project.targets || []).some(isOursTarget);
}
/// The OpenFF workspaces open right now (for a scene file, the game it is opened under).
function oursWorkspaces() {
  return (projectState.workspaces || []).filter(w => isOursTarget(w.target));
}

// ------------------------------------------------------------------ the menu bar

/// Builds one menu. `items` are {label, run, checked, disabled, note} or the string '-'.
function buildMenu(label, items) {
  const menu = document.createElement('div');
  menu.className = 'menu';

  const button = document.createElement('button');
  button.className = 'menu-title';
  button.textContent = label;
  menu.append(button);

  const list = document.createElement('div');
  list.className = 'menu-items';
  menu.append(list);

  for (const item of items) {
    if (item === '-') {
      const rule = document.createElement('div');
      rule.className = 'menu-rule';
      list.append(rule);
      continue;
    }
    if (item.heading) {
      const head = document.createElement('div');
      head.className = 'menu-heading';
      head.textContent = item.heading;
      list.append(head);
      continue;
    }
    const entry = document.createElement('button');
    entry.className = 'menu-item';
    entry.textContent = item.label;
    if (item.checked) entry.classList.add('checked');
    if (item.disabled) entry.disabled = true;
    if (item.note) entry.title = item.note;
    entry.onclick = () => { closeMenus(); item.run(); };
    list.append(entry);
  }

  button.onclick = event => {
    event.stopPropagation();
    const wasOpen = menu.classList.contains('open');
    closeMenus();
    if (!wasOpen) menu.classList.add('open');
  };
  return menu;
}

function closeMenus() {
  $$('#menubar .menu').forEach(m => m.classList.remove('open'));
}

document.addEventListener('click', closeMenus);
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeMenus(); });

/// Rebuilds the bar from whatever is currently open.
function drawMenuBar() {
  const bar = $('#menubar');
  if (!bar) return;
  bar.textContent = '';

  const open = projectState.project;
  const edited = totalEdited();

  bar.append(buildMenu('File', [
    { label: 'Create a mod…', run: () => newProjectDialog() },
    { label: 'Open project…', run: openProjectDialog,
      note: 'Pick one of your projects; the bin on a row deletes it (to the Recycle Bin)' },
    { label: 'Close project', run: closeProject, disabled: !open,
      note: 'Back to the games as they are; the project stays on disk to open again' },
    { label: 'Sample projects…', run: sampleProjectsDialog,
      note: 'Create a mod from one of the samples: a copy of your own to read and change' },
    { label: 'Install sample mods…', run: () => showStartPage('samples'),
      note: 'The sample browser: install a sample into OpenFF to play it, update or uninstall one' },
    { label: 'Start page', run: showStartPage },
    '-',
    { label: 'Guide', run: () => openGuide(), note: 'The modding guide: short tutorials with pictures, and the reference' },
    '-',
    {
      label: edited ? `Changes… (${edited})` : 'Changes…',
      run: changesDialog,
      disabled: !edited,
    },
    { label: 'Project settings…', run: projectSettingsDialog, disabled: !open },
    { label: open && open.sample ? `Update from the ${open.sample} sample…` : 'Update from the sample…', run: updateFromSampleDialog, disabled: !(open && open.sample),
      note: open && open.sample ? 'The sample\'s files as this version of Crystal has them, over the project\'s - your own files are kept' : 'For a project made from one of the sample projects' },
    '-',
    { label: 'Export as .zip…', run: exportProject, disabled: !open,
      note: 'The project as an upload: files, manifest and a README' },
    { label: 'Export to OpenFF…', run: exportToOpenFF, disabled: !open,
      note: 'Writes the mod into the OpenFF client\'s mods folder, ready to play' },
    '-',
    { label: open && open.code ? 'Build C# code' : 'Add C# code…', run: open && open.code ? buildCode : addCode, disabled: !open,
      note: open && open.code ? 'dotnet build of code/; the output goes with Export to OpenFF' : 'A csproj and a starting class under code/, referencing the OpenFF engine' },
    { label: 'Open C# code in editor', run: openCode, disabled: !(open && open.code),
      note: 'Visual Studio, Rider, VS Code - whatever opens .csproj here' },
    { label: 'OpenFF API reference…', run: apiReferenceDialog,
      note: 'Game.Hero, Game.Dialogue, Game.Magic... what a mod\'s C# can call, from the engine\'s own docs' },
    '-',
    { label: 'Run in OpenFF', run: () => runInOpenFF(), disabled: !(open && open.client),
      note: open && open.client ? 'Export to the mods folder and start the client with this mod alone (a running client hot-reloads)' : 'No OpenFF client found: build OpenFF or start OpenFF.exe once' },
    { label: 'Run with the other mods', run: () => runInOpenFF({ withOthers: true }), disabled: !(open && open.client),
      note: 'The same, with every other enabled mod of the client\'s mods folder playing too - for an add-on to another mod' },
    { label: 'Show project folder', run: () => revealProject(), disabled: !open },
  ]));

  // The Steam side: one install/remove pair per Steam copy the project has open. With
  // one it reads as it always did; with two, each line says which game it means. The
  // OpenFF side has nothing to install - the client plays the export - so it gets its
  // own lines, with the games it covers.
  const many = projectState.workspaces.length > 1;
  const steamOpen = projectState.workspaces.filter(w => !isOursTarget(w.target));
  const oursOpen = oursWorkspaces();
  const items = [];
  if (open && oursOpen.length) {
    const games = oursOpen.map(w => w.game.toUpperCase()).join(' + ');
    items.push({ heading: `OpenFF mod · ${games}` });
    items.push({ label: 'Export to OpenFF…', run: exportToOpenFF,
      note: `Writes the mod into the client's mods folder: ${oursOpen.map(w => w.game + '/files').join(', ')}, the code and the scenes` });
    items.push({ label: 'Run in OpenFF', run: () => runInOpenFF(), disabled: !open.client,
      note: open.client ? 'Export and start the client with this mod alone (a running client hot-reloads)' : 'No OpenFF client found: build OpenFF or start OpenFF.exe once' });
    items.push({ label: 'Run with the other mods', run: () => runInOpenFF({ withOthers: true }), disabled: !open.client,
      note: 'The same, with every other enabled mod playing too - for an add-on to another mod' });
  }
  for (const w of steamOpen) {
    const mod = projectState.mods[w.target];
    const suffix = many ? ` — ${w.label}` : '';
    if (many) items.push({ heading: w.label });
    items.push({
      label: `Install into the game${suffix}`,
      disabled: !mod || !mod.canInstall || !mod.edited.length,
      note: mod && !mod.canInstall ? mod.why : undefined,
      run: () => runMod('/api/mod/install', 'install', w.target),
    });
    items.push({
      label: `Remove from the game${suffix}`,
      disabled: !mod || !mod.canInstall || (!mod.installed.length && !mod.changed.length),
      run: () => runMod('/api/mod/uninstall', 'uninstall', w.target),
    });
  }
  if (many) {
    items.push('-');
    items.push({ heading: 'Default game (what the command line opens first)' });
    for (const w of projectState.workspaces) {
      items.push({
        label: w.label,
        checked: open && open.active === w.target,
        run: () => setTarget(w.target),
      });
    }
  }
  bar.append(buildMenu('Project', items.length ? items : [
    { label: 'No project open', disabled: true, run: () => {} },
  ]));

  bar.append(buildMenu('Help', [
    { label: 'Guide', run: () => openGuide(), note: 'Short tutorials with pictures: a first mod, a map of your own, a weapon, a cutscene' },
    { label: 'Guide: the map editor', run: () => openGuide('map-editor.html') },
    { label: 'Guide: items and weapons', run: () => openGuide('items.html') },
    { label: 'Guide: cutscenes', run: () => openGuide('cutscenes.html') },
    { label: 'Guide: reference', run: () => openGuide('reference.html'), note: 'Every component, every clip, every file - to look things up' },
    '-',
    { label: 'OpenFF API reference…', run: apiReferenceDialog,
      note: 'Game.Hero, Game.Dialogue, Game.Magic... what a mod\'s C# can call, from the engine\'s own docs' },
    { label: 'Sample projects…', run: sampleProjectsDialog },
  ]));

  const where = $('#project-name');
  if (where) {
    where.textContent = open
      ? open.name + (many ? '' : ` · ${describeTarget(open.active)}`)
      : 'no project';
    where.title = open ? open.directory : 'File ▸ New project…';
    where.onclick = open ? projectSettingsDialog : newProjectDialog;
  }
}

function describeTarget(name) {
  const found = projectState.targets.find(t => t.name === name);
  return found ? found.label : name;
}

function totalEdited() {
  return Object.values(projectState.mods || {})
    .reduce((sum, mod) => sum + ((mod && mod.edited.length) || 0), 0);
}

// ------------------------------------------------------------------ the dialogs

/// The shell every dialog here shares. Returns the body to fill.
function dialog(title, { onClose, wide } = {}) {
  const veil = document.createElement('div');
  veil.className = 'picker-veil';
  const box = document.createElement('div');
  box.className = 'picker dialog' + (wide ? ' wide' : '');
  const head = document.createElement('div');
  head.className = 'picker-head';
  const name = document.createElement('strong');
  name.textContent = title;
  name.style.flex = '1';
  // The × in the corner, as the model picker has and every window does.
  const close = document.createElement('button');
  close.className = 'shut';
  close.textContent = '×';
  close.title = 'Close (Esc)';
  const body = document.createElement('div');
  body.className = 'dialog-body';

  head.append(name, close);
  box.append(head, body);
  veil.append(box);
  document.body.append(veil);

  const shut = () => { veil.remove(); if (onClose) onClose(); };
  close.onclick = shut;
  veil.onclick = e => { if (e.target === veil) shut(); };
  document.addEventListener('keydown', function esc(e) {
    if (e.key === 'Escape') { shut(); document.removeEventListener('keydown', esc); }
  });
  body.close = shut;
  return body;
}

function field(parent, label, value = '', { multiline = false, placeholder = '' } = {}) {
  const wrap = document.createElement('label');
  wrap.className = 'dialog-field';
  wrap.textContent = label;
  const input = document.createElement(multiline ? 'textarea' : 'input');
  input.value = value;
  if (placeholder) input.placeholder = placeholder;
  wrap.append(input);
  parent.append(wrap);
  return input;
}

function section(parent, text) {
  const head = document.createElement('div');
  head.className = 'dialog-section';
  head.textContent = text;
  parent.append(head);
  return head;
}

function errorLine(parent) {
  const line = document.createElement('p');
  line.className = 'dialog-error';
  parent.append(line);
  return line;
}

/// The games and the kinds of mod as a grid: a row per game (FF3, FF4), a column per
/// kind (an OpenFF mod the client plays; a Steam mod copied into the Steam game), and
/// in each cell a checkbox with where that content was found (or that it was not). A
/// project may tick any of the four; each tick is one target. Returns a function
/// giving the checked target names.
function gameRows(parent, checked) {
  const grid = document.createElement('div');
  grid.className = 'dialog-matrix';
  const head = (text, note) => {
    const cell = document.createElement('div');
    cell.className = 'matrix-head';
    const b = document.createElement('b');
    b.textContent = text;
    cell.append(b);
    if (note) {
      const s = document.createElement('span');
      s.textContent = note;
      cell.append(s);
    }
    grid.append(cell);
  };
  head('');
  head('OpenFF mod', 'plays in the OpenFF client; may use both games; can carry C# code');
  head('Steam mod', 'files copied into the Steam game with Project ▸ Install');
  const boxes = [];
  for (const game of ['ff3', 'ff4']) {
    const label = document.createElement('div');
    label.className = 'matrix-game';
    label.textContent = game.toUpperCase();
    grid.append(label);
    for (const kind of ['openff', 'steam']) {
      const name = targetFor(game, kind);
      const target = projectState.targets.find(t => t.name === name) || { name, content: null };
      const cell = document.createElement('label');
      cell.className = 'dialog-game matrix-cell' + (target.content ? '' : ' missing');
      const box = document.createElement('input');
      box.type = 'checkbox';
      box.checked = checked.includes(name) && !!target.content;
      box.disabled = !target.content;
      cell.classList.toggle('checked', box.checked);
      box.onchange = () => cell.classList.toggle('checked', box.checked);
      const where = document.createElement('span');
      where.className = 'where';
      where.textContent = target.content || 'not found on this machine';
      where.title = target.content ? `${describeTarget(name)} · ${target.content}` : describeTarget(name);
      cell.append(box, where);
      boxes.push({ box, name });
      grid.append(cell);
    }
  }
  parent.append(grid);
  return () => boxes.filter(b => b.box.checked).map(b => b.name);
}

async function openProjectDialog() {
  const body = dialog('Open project');
  let projects = [];
  try {
    projects = await api('/api/projects');
  } catch (error) {
    say(error.message, 'bad');
  }

  if (!projects.length) {
    const empty = document.createElement('p');
    empty.className = 'dialog-note';
    empty.textContent = 'No projects yet. File ▸ New project… makes one.';
    body.append(empty);
    return;
  }

  const list = document.createElement('div');
  list.className = 'dialog-list';
  for (const project of projects) {
    const line = document.createElement('div');
    line.className = 'project-line';
    const row = document.createElement('button');
    row.className = 'dialog-row project-row';
    if (project.current) row.classList.add('checked');
    const title = document.createElement('strong');
    title.append(projectKindIcon(project), document.createTextNode(project.name));
    if (project.current) {
      const badge = document.createElement('em');
      badge.className = 'project-open';
      badge.textContent = 'open';
      title.append(badge);
    }
    const where = document.createElement('span');
    where.textContent = project.targets.map(describeTarget).join(', ')
      + '  ·  ' + project.directory;
    row.append(title, where);
    row.onclick = () => openProjectAt(project.directory, body);
    const bin = document.createElement('button');
    bin.className = 'project-delete';
    bin.title = `Delete ${project.name}…`;
    bin.append(icon('trash'));
    bin.onclick = () => deleteProjectDialog(project, body);
    line.append(row, bin);
    list.append(line);
  }
  body.append(list);
}

/// A project made from a sample brought up to the sample as it is now (after an update of OpenFF): the sample's files
/// copied over the project's - its own files kept - after a look at what that changes.
async function updateFromSampleDialog() {
  const open = projectState.project;
  if (!open || !open.sample) return;
  let plan;
  try {
    plan = await api('/api/project/sample/compare', {});
    if (!plan.ok) throw new Error(plan.error);
  } catch (error) { say(error.message, 'bad'); return; }
  const body = dialog(`Update from the ${plan.sample} sample`, { wide: true });
  const note = document.createElement('p');
  note.className = 'dialog-note';
  const total = plan.added.length + plan.changed.length;
  note.textContent = total === 0
    ? `${open.name} has every file of the ${plan.sample} sample as it is now - nothing to update.`
    : `The ${plan.sample} sample's files go over ${open.name}'s: ${plan.added.length} new, ${plan.changed.length} different from the sample (${plan.same} already the same). Files of the project's own are kept, and so are its settings.`;
  body.append(note);
  if (plan.changed.length) {
    const warn = document.createElement('p');
    warn.className = 'dialog-note sample-warn';
    warn.textContent = 'These are replaced by the sample\'s - any change you made to them is lost:';
    body.append(warn, fileList(plan.changed));
  }
  if (plan.added.length) {
    const added = document.createElement('p');
    added.className = 'dialog-note';
    added.textContent = 'These are added:';
    body.append(added, fileList(plan.added));
  }
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const cancel = document.createElement('button');
  cancel.textContent = total === 0 ? 'Close' : 'Not now';
  cancel.onclick = () => body.close();
  actions.append(cancel);
  if (total > 0) {
    const go = document.createElement('button');
    go.className = plan.changed.length ? 'danger' : 'primary';
    go.textContent = 'Update the project';
    go.onclick = async () => {
      go.disabled = true;
      try {
        const result = await api('/api/project/sample/update', {});
        if (!result.ok) throw new Error(result.error);
        body.close();
        await reloadEverything(`${open.name} is up to date with the ${result.sample} sample: ${result.added.length + result.changed.length} file(s)` + (open.code ? ' - Build C# code to play it' : ''));
      } catch (error) {
        say(error.message, 'bad');
        go.disabled = false;
      }
    };
    actions.append(go);
  }
  body.append(actions);
}

function fileList(files) {
  const list = document.createElement('ul');
  list.className = 'sample-files';
  for (const f of files.slice(0, 200)) {
    const li = document.createElement('li');
    li.textContent = f;
    list.append(li);
  }
  if (files.length > 200) {
    const li = document.createElement('li');
    li.textContent = `... and ${files.length - 200} more`;
    list.append(li);
  }
  return list;
}

/// The open project closed: the editor goes back to every game as it is, as when it starts without one.
async function closeProject() {
  const open = projectState.project;
  if (!open) return;
  try {
    const result = await api('/api/project/close', {});
    if (!result.ok) throw new Error(result.error);
    await reloadEverything(`${result.closed || open.name} is closed - it stays on disk; File ▸ Open project… opens it again`);
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// A project deleted, after asking: its folder to the Recycle Bin (so it can be taken back from there), closed
/// first if it is the one open; and, if ticked, the mod Export to OpenFF made of it out of the client's mods folder.
function deleteProjectDialog(project, listBody) {
  const body = dialog(`Delete ${project.name}?`);
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = `The project's folder goes to the Recycle Bin - its edits, code and assets with it - and you can restore it from there.`
    + (project.current ? ' It is the project open now: it is closed first.' : '');
  const where = document.createElement('p');
  where.className = 'dialog-note project-path';
  where.textContent = project.directory;
  const modLabel = document.createElement('label');
  modLabel.className = 'dialog-row check';
  const removeMod = document.createElement('input');
  removeMod.type = 'checkbox';
  modLabel.append(removeMod, document.createTextNode("Also remove the mod Export to OpenFF made of it from the client's mods folder"));
  // Installed in a Steam / GOG game: its backups of the game's files go with the project, so it is taken out first.
  const installedIn = project.installedIn || [];
  const uninstallLabel = document.createElement('label');
  uninstallLabel.className = 'dialog-row check';
  const uninstall = document.createElement('input');
  uninstall.type = 'checkbox';
  uninstall.checked = true;
  uninstallLabel.append(uninstall, document.createTextNode(`First uninstall it from ${installedIn.join(' and ')} - the game's own files put back (the backups of them go with the project)`));
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const cancel = document.createElement('button');
  cancel.textContent = 'Keep it';
  cancel.onclick = () => body.close();
  const go = document.createElement('button');
  go.className = 'danger';
  go.textContent = 'Delete project';
  go.onclick = async () => {
    go.disabled = true;
    try {
      const result = await api('/api/project/delete', { directory: project.directory, removeMod: removeMod.checked, uninstall: installedIn.length > 0 && uninstall.checked });
      if (!result.ok) throw new Error(result.error);
      body.close();
      if (listBody) listBody.close();
      const said = (result.where === 'recycle bin' ? `${result.name} is in the Recycle Bin` : `${result.name} is deleted - kept in ${result.where} in case you want it back`)
        + (result.uninstalled && result.uninstalled.length ? `, uninstalled from ${result.uninstalled.join(', ')}` : '')
        + (result.mod ? ', and its mod is out of the mods folder' : '')
        + (result.modError ? ` (its mod could not be removed: ${result.modError})` : '');
      if (project.current) await reloadEverything(said);
      else {
        say(said, 'good');
        // The start page lists the projects: drawn again without the one gone.
        if (typeof drawStartPage === 'function') drawStartPage();
      }
    } catch (error) {
      say(error.message, 'bad');
      go.disabled = false;
    }
  };
  actions.append(cancel, go);
  body.append(note, where, modLabel);
  if (installedIn.length) body.append(uninstallLabel);
  body.append(actions);
  cancel.focus();
}

/// The modding guide: HTML pages Crystal serves from its own Guide folder (the release zip's
/// Guide\, or the repository's Docs\Guide), in a new browser tab; a page name opens that page.
function openGuide(page) {
  window.open(wsUrl('/guide/' + (page || 'index.html')), '_blank');
}

/// The mark of what kind of mod a project is, wherever projects are listed: the OpenFF
/// folder for an OpenFF mod, the game for a Steam mod, both for one that is both.
function projectKindIcon(project) {
  const kinds = new Set((project.targets || []).map(kindOfTarget));
  const wrap = document.createElement('span');
  wrap.className = 'project-kind';
  if (kinds.has('openff')) {
    const i = icon('mod');
    i.setAttribute('title', 'OpenFF mod');
    wrap.append(i);
  }
  if (kinds.has('steam')) {
    const i = icon('steam');
    i.setAttribute('title', 'Steam mod');
    wrap.append(i);
  }
  wrap.title = [kinds.has('openff') && 'OpenFF mod', kinds.has('steam') && 'Steam mod'].filter(Boolean).join(' and ');
  return wrap;
}

async function openProjectAt(directory, body) {
  try {
    const result = await api('/api/project/open', { directory });
    if (!result.ok) throw new Error(result.error);
    if (body) body.close();
    await reloadEverything(`opened ${result.name}`);
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// Name, author, version, description - and which games. Adding a game opens it;
/// removing one closes it but leaves its edits on disk.
function projectSettingsDialog() {
  const open = projectState.project;
  if (!open) return;
  const body = dialog('Project settings');

  const columns = document.createElement('div');
  columns.className = 'dialog-columns';
  const name = field(columns, 'Name', open.name || '');
  const author = field(columns, 'Author', open.author || '');
  const version = field(columns, 'Version', open.version || '1.0');
  body.append(columns);
  const description = field(body, 'Description', open.description || '', { multiline: true });

  section(body, 'What kind of mod, for which games');
  const picked = gameRows(body, open.targets || []);
  for (const [target, why] of Object.entries(projectState.missing || {})) {
    const warn = document.createElement('p');
    warn.className = 'dialog-error';
    warn.textContent = `${describeTarget(target)}: ${why}`;
    body.append(warn);
  }

  section(body, 'On disk');
  const where = document.createElement('p');
  where.className = 'dialog-note';
  where.textContent = open.directory;
  body.append(where);
  const paths = document.createElement('div');
  paths.className = 'dialog-list';
  for (const w of projectState.workspaces) {
    const row = document.createElement('p');
    row.className = 'dialog-note';
    row.textContent = `${w.label}: edits in ${w.overrides}`;
    paths.append(row);
  }
  body.append(paths);

  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Save';
  go.onclick = async () => {
    const targets = picked();
    if (!targets.length) { problem.textContent = 'A project needs at least one game.'; return; }
    go.disabled = true;
    try {
      const result = await api('/api/project/save', {
        name: name.value, author: author.value,
        version: version.value, description: description.value, targets,
      });
      if (!result.ok) throw new Error(result.error);
      body.close();
      await reloadEverything('settings saved');
    } catch (error) {
      problem.textContent = error.message;
      go.disabled = false;
    }
  };
  const reveal = document.createElement('button');
  reveal.textContent = 'Show folder';
  reveal.onclick = () => revealProject();
  const exportButton = document.createElement('button');
  exportButton.textContent = 'Export as .zip';
  exportButton.onclick = exportProject;
  const openffButton = document.createElement('button');
  openffButton.textContent = 'Export to OpenFF';
  openffButton.title = 'Write the mod into the OpenFF client\'s mods folder';
  openffButton.onclick = exportToOpenFF;
  const codeButton = document.createElement('button');
  if (open.code) {
    codeButton.textContent = 'Build C# code';
    codeButton.title = 'dotnet build of code/; then Export to OpenFF carries the assembly';
    codeButton.onclick = buildCode;
  } else {
    codeButton.textContent = 'Add C# code';
    codeButton.title = 'A csproj and a starting class under code/, referencing the OpenFF engine';
    codeButton.onclick = addCode;
  }
  const runButton = document.createElement('button');
  runButton.textContent = 'Run in OpenFF';
  runButton.title = open.client ? 'Export to the mods folder and start the client' : 'No OpenFF client found';
  runButton.disabled = !open.client;
  runButton.onclick = () => { body.close(); runInOpenFF(); };
  const grow = document.createElement('span');
  grow.className = 'grow';
  actions.append(go, grow, reveal, exportButton, openffButton, codeButton, runButton);
  body.append(actions);
}

/// Export to the client's mods folder and start the client; a running client hot-reloads the
/// code (and the scene files apply on the next map).
/// Export and start the client; with a map (and a spot), straight onto that map at that
/// spot - "play here" from the map that is open.
async function runInOpenFF(where = {}) {
  try {
    say('exporting…');
    const result = await api('/api/project/run', where);
    if (!result.ok) throw new Error(result.error);
    const at = where.map ? ` on ${where.map}${where.pos ? ' at ' + where.pos.map(Math.round).join(', ') : ''}` : '';
    const alone = where.withOthers ? ' with the other mods' : ' with this mod alone';
    say(result.started
      ? `exported to ${result.path} - OpenFF is starting${alone}${at}`
      : `exported to ${result.path} - OpenFF is already running: it picks the code up and re-reads the scene when the map is entered again, but models, textures and definitions are read as it starts - close the game and Play again to see those${where.map ? ' (and for a start on ' + where.map + ')' : ''}`, 'warn');
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// The engine's API from its XML docs: a searchable list of types and members.
async function apiReferenceDialog() {
  const body = dialog('OpenFF API reference', { wide: true });
  const search = document.createElement('input');
  search.placeholder = 'Search - Hero, Say, Cast, MapEntered…';
  search.className = 'api-search';
  const list = document.createElement('div');
  list.className = 'api-list';
  body.append(search, list);
  let types = [];
  try {
    const result = await api('/api/openff/reference');
    types = result.types || [];
    if (!types.length) {
      const none = document.createElement('p');
      none.className = 'dialog-note';
      none.textContent = 'No engine documentation found: build OpenFF (OpenFF.Engine.xml sits beside OpenFF.Engine.dll).';
      list.append(none);
      return;
    }
  } catch (error) {
    say(error.message, 'bad');
    return;
  }
  const render = () => {
    const q = search.value.trim().toLowerCase();
    list.innerHTML = '';
    let shown = 0;
    for (const type of types) {
      const typeHit = !q || type.name.toLowerCase().includes(q) || (type.summary || '').toLowerCase().includes(q);
      const members = (type.members || []).filter(m => !q || typeHit || m.name.toLowerCase().includes(q) || (m.summary || '').toLowerCase().includes(q));
      if (!typeHit && !members.length) continue;
      if (shown++ > 60) break;
      const block = document.createElement('div');
      block.className = 'api-type';
      const head = document.createElement('div');
      head.className = 'api-type-head';
      const name = document.createElement('b');
      name.textContent = type.name;
      head.append(name);
      if (type.summary) {
        const sum = document.createElement('span');
        sum.textContent = type.summary;
        head.append(sum);
      }
      block.append(head);
      const rows = document.createElement('div');
      rows.className = 'api-members';
      for (const m of (q && !typeHit ? members : (type.members || []))) {
        const row = document.createElement('div');
        row.className = 'api-member';
        const sig = document.createElement('code');
        sig.textContent = m.signature || m.name;
        row.append(sig);
        if (m.summary) {
          const s = document.createElement('span');
          s.textContent = m.summary;
          row.append(s);
        }
        rows.append(row);
      }
      block.append(rows);
      list.append(block);
    }
    if (!shown) {
      const none = document.createElement('p');
      none.className = 'dialog-note';
      none.textContent = 'Nothing matches.';
      list.append(none);
    }
  };
  search.oninput = render;
  render();
  search.focus();
}

/// Everything the project has changed, per game, and a way to throw any of it away.
///
/// Reverting is two things at once, and the dialog says so: the edit is deleted, and
/// if it had been installed the game's own file goes back at the same time. Undoing
/// only the first half is the trap - the file would read as shipped everywhere except
/// in the game, which is the one place it matters.
async function changesDialog(target = state.ws) {
  const body = dialog('Changes');
  const many = projectState.workspaces.length > 1;
  const ws = target || (projectState.workspaces[0] && projectState.workspaces[0].target);

  if (many) {
    const tabs = document.createElement('div');
    tabs.className = 'ws-tabs';
    for (const w of projectState.workspaces) {
      const tab = document.createElement('button');
      tab.className = 'ws-tab ' + w.game + (w.target === ws ? ' on' : '');
      const game = document.createElement('b');
      game.textContent = w.game.toUpperCase();
      const count = document.createElement('span');
      const mod = projectState.mods[w.target];
      count.textContent = `${(mod && mod.edited.length) || 0} edited`;
      tab.append(game, count);
      tab.onclick = () => { body.close(); changesDialog(w.target); };
      tabs.append(tab);
    }
    body.append(tabs);
  }

  let mod;
  try {
    mod = await api(`/api/mod/status?ws=${encodeURIComponent(ws || '')}`);
  } catch (error) {
    say(error.message, 'bad');
    return;
  }

  if (!mod.edited.length) {
    const empty = document.createElement('p');
    empty.className = 'dialog-note';
    empty.textContent = many
      ? `Nothing edited for ${describeTarget(ws)} yet.`
      : 'Nothing edited in this project yet.';
    body.append(empty);
    return;
  }

  const inGame = new Set(mod.installed);
  const stale = new Set(mod.changed);

  const all = document.createElement('label');
  all.className = 'dialog-check';
  const every = document.createElement('input');
  every.type = 'checkbox';
  all.append(every, document.createTextNode(
    `Select all (${mod.edited.length} file${mod.edited.length === 1 ? '' : 's'})`));
  body.append(all);

  const list = document.createElement('div');
  list.className = 'dialog-list changes';
  const boxes = [];
  for (const name of mod.edited) {
    const row = document.createElement('label');
    row.className = 'dialog-row check';
    const box = document.createElement('input');
    box.type = 'checkbox';
    boxes.push({ box, name });

    const text = document.createElement('span');
    text.className = 'changes-name';
    text.textContent = name;

    const mark = document.createElement('span');
    mark.className = 'changes-where';
    mark.textContent = stale.has(name) ? 'in the game, but changed since'
      : inGame.has(name) ? 'in the game'
      : mod.canInstall ? 'not in the game yet'
      : 'live — this build reads it directly';

    row.append(box, text, mark);
    list.append(row);
  }
  body.append(list);

  every.onchange = () => boxes.forEach(b => { b.box.checked = every.checked; });

  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Revert selected';
  go.onclick = async () => {
    const names = boxes.filter(b => b.box.checked).map(b => b.name);
    if (!names.length) return say('nothing selected', 'bad');
    const what = names.length === 1 ? names[0] : `${names.length} files`;
    const alsoGame = names.some(n => inGame.has(n));
    if (!confirm(`Throw away the edits to ${what}?`
      + (alsoGame ? '\n\nThe game\'s own files go back at the same time.' : ''))) return;
    try {
      const result = await api(`/api/mod/revert?ws=${encodeURIComponent(ws || '')}`, { names });
      if (!result.ok) throw new Error(result.error);
      const notes = (result.notes || []).join('  ·  ');
      say(`${result.reverted.length} reverted`
        + (result.restored.length ? `, ${result.restored.length} put back in the game` : '')
        + (result.removed.length ? `, ${result.removed.length} removed from the game` : '')
        + (notes ? '  ·  ' + notes : ''), notes ? 'bad' : 'good');
      body.close();
      // The open tabs are showing what was just thrown away.
      if (typeof docs !== 'undefined' && typeof closeDoc === 'function') {
        for (const id of [...docs.keys()]) closeDoc(id);
      }
      await refreshProject();
      if (typeof drawProjectTree === 'function') drawProjectTree();
    } catch (error) {
      say(error.message, 'bad');
    }
  };
  body.append(go);
}

// ------------------------------------------------------------------ the actions

async function setTarget(target) {
  try {
    const result = await api('/api/project/target', { target });
    if (!result.ok) throw new Error(result.error);
    await refreshProject();
    say(`${describeTarget(target)} is the default now`, 'good');
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function runMod(endpoint, kind, target) {
  const label = describeTarget(target);
  const asking = kind === 'install'
    ? `Copy your edits over ${label}'s own files?\n\n`
      + 'The originals are kept, and "Remove from the game" puts them back.'
    : `Put ${label}'s original files back?\n\nYour edits stay in the project.`;
  if (!confirm(asking)) return;
  try {
    const result = await api(`${endpoint}?ws=${encodeURIComponent(target || '')}`, {});
    if (!result.ok) throw new Error(result.error);
    const notes = (result.notes || []).join('  ·  ');
    const summary = kind === 'install'
      ? `${result.wrote.length} file(s) written into ${label}`
      : `${result.restored.length} restored, ${result.removed.length} removed`
        + (result.skipped.length ? `, ${result.skipped.length} left alone` : '');
    say(summary + (notes ? '  ·  ' + notes : ''), notes ? 'bad' : 'good');
  } catch (error) {
    say(error.message, 'bad');
  }
  await refreshProject();
}

async function addCode() {
  try {
    say('writing the project…');
    const result = await api('/api/project/code/create', {});
    if (!result.ok) throw new Error(result.error);
    say(`C# project made: ${result.path} - it is under OpenFF mod in the project tree; Build C# code, or open it in your editor`, 'good');
    await refreshProject();
    // The mod folder lists the new files; open the starting class so the way in is plain.
    if (typeof selectKind === 'function') {
      await selectKind('code');
      const starter = (state.files || []).find(f => /\/Mod\.cs$/i.test(f.name));
      if (starter && typeof openDoc === 'function') await openDoc('code', starter.name);
    }
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function buildCode() {
  try {
    say('building…');
    const result = await api('/api/project/code/build', {});
    // Every problem goes to the console, and to the open file it is in.
    for (const p of result.problems || []) {
      logLine(`${p.file}(${p.line},${p.column}): ${p.kind} ${p.code}: ${p.message}`, p.kind === 'error' ? 'bad' : undefined);
    }
    if (typeof showBuildProblems === 'function') await showBuildProblems(result.problems || []);
    if (!result.ok) {
      say('build failed: ' + (result.output || result.error || '').split('\n')[0], 'bad');
      if (result.output) console.log(result.output);
      return;
    }
    const warnings = (result.problems || []).filter(p => p.kind === 'warning').length;
    say(`built ${result.assemblies.join(', ')}${warnings ? ` with ${warnings} warning(s)` : ''} - Export to OpenFF to play it (the client hot-reloads a running game)`, 'good');
    if (typeof invalidateCatalog === 'function') invalidateCatalog();
    if (typeof drawInspector === 'function') drawInspector();
    // The list marks files changed since the last build; there are none now.
    if (typeof browseKind !== 'undefined' && browseKind === 'code' && typeof loadList === 'function') loadList().catch(() => {});
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function openCode() {
  try {
    const result = await api('/api/project/code/open', {});
    if (!result.ok) throw new Error(result.error);
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function exportToOpenFF() {
  try {
    say('writing…');
    const result = await api('/api/project/export-openff', {});
    if (!result.ok) throw new Error(result.error);
    say(`exported to ${result.path} (${result.files} file(s)) - start the client to play it`, 'good');
    await revealProject(result.path);
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function exportProject() {
  try {
    say('packing…');
    const result = await api('/api/project/export', {});
    if (!result.ok) throw new Error(result.error);
    const kb = Math.max(1, Math.round(result.bytes / 1024));
    say(`exported ${result.path} (${kb} KB)`, 'good');
    await revealProject(result.path);
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function revealProject(path) {
  try {
    const result = await api('/api/project/reveal', path ? { path } : {});
    if (!result.ok) throw new Error(result.error);
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// Re-reads what is open. Switching project changes the content underneath every open
/// tab, so those go too rather than being left showing another project's data.
async function reloadEverything(message) {
  // Closing the old project's tabs must not be written down as the new project's session.
  if (typeof sessionHold !== 'undefined') sessionHold = true;
  if (typeof docs !== 'undefined' && typeof closeDoc === 'function') {
    for (const id of [...docs.keys()]) closeDoc(id);
  }
  state.ws = null;
  state.files = [];
  await refreshProject();
  if (typeof resetOps === 'function') resetOps();
  if (typeof sessionHold !== 'undefined') sessionHold = false;
  // The project opened comes back as it was left; otherwise the panel just reloads.
  const restored = typeof restoreSession === 'function' ? await restoreSession() : false;
  if (!restored && typeof selectKind === 'function') await selectKind(typeof browseKind !== 'undefined' ? browseKind : 'map');
  if (typeof drawHierarchy === 'function') drawHierarchy();
  if (typeof drawInspector === 'function') drawInspector();
  if (message) say(message, 'good');
}

async function refreshProject() {
  try {
    const [status, targets] = await Promise.all([
      api('/api/status'),
      api('/api/targets'),
    ]);
    const open = status.workspaces || [];
    // The page's current game: keep it if it is still open, else the server's active one.
    if (!open.some(w => w.target === state.ws)) state.ws = status.active || (open[0] && open[0].target) || null;
    const mods = {};
    await Promise.all(open.map(async w => {
      mods[w.target] = await api(`/api/mod/status?ws=${encodeURIComponent(w.target)}`).catch(() => null);
    }));
    projectState = {
      project: status.project, targets, mods, workspaces: open,
      active: status.active, missing: status.missing || {},
      // Every game on the machine, open or not, for the tabs above the libraries.
      available: status.available || [],
      mod: mods[state.ws] || null,
    };
    const current = open.find(w => w.target === state.ws) || {};
    state.language = status.language || 'en';
    state.messagePrefix = status.messagePrefix || `${state.language}.lproj/`;
    $('#summary').textContent = open.length > 1
      ? open.map(w => `${w.game.toUpperCase()} ${w.files}`).join('  ·  ') + ' files'
      : `${status.files} files  ·  ${status.contentDirectory}`;
    showModState(projectState.mod, current);
  } catch (error) {
    say(error.message, 'bad');
  }
  drawMenuBar();
  drawServiceButton();
  drawStartPage();
  if (typeof drawProjectTree === 'function') drawProjectTree();
}

/// The cog in the header: the mod's GameService. Open when there is one; the stub when
/// there is code but no service; the whole C# project when there is no code. An OpenFF
/// mod's entry point should never be more than a click away, whatever the page shows.
function drawServiceButton() {
  const button = $('#service-button');
  if (!button) return;
  const project = projectState.project;
  if (!project || !isOpenFFProject(project)) {
    button.hidden = true;
    return;
  }
  button.hidden = false;
  button.innerHTML = '';
  button.append(icon('service'));
  button.classList.toggle('stub', !project.service);
  if (project.service) {
    button.title = `GameService: ${project.service} - the mod's entry point. Click to open it.`;
    button.onclick = () => { if (typeof openDoc === 'function') openDoc('code', project.service); };
  } else if (project.code) {
    button.title = 'The code has no GameService yet - click to write the stub (the mod\'s entry point: Start, map events, saving)';
    button.onclick = () => createServiceStub();
  } else {
    button.title = 'No C# code yet - click to add the project and its starting GameService';
    button.onclick = () => addCode();
  }
}

/// A GameService where there is code but none: Mod.cs, or Service.cs when that name is taken.
async function createServiceStub() {
  try {
    let made = await api('/api/project/file/new', { name: 'Mod', template: 'service' });
    if (!made.ok && /already/.test(made.error || '')) made = await api('/api/project/file/new', { name: 'Service', template: 'service' });
    if (!made.ok) throw new Error(made.error);
    say(`${made.name} written - the mod's GameService; Build, then Export or Run in OpenFF`, 'good');
    await refreshProject();
    if (typeof projectChanged === 'function') projectChanged();
    if (typeof openDoc === 'function') await openDoc('code', made.name);
  } catch (error) {
    say(error.message, 'bad');
  }
}

/// Whether the edits are in the game yet. Hidden for our own build, which reads the
/// project directly and so has nothing to install.
function showModState(mod, workspace) {
  const box = $('#mod');
  if (!box) return;
  if (!mod || !mod.canInstall) {
    box.hidden = true;
    return;
  }
  box.hidden = false;

  const pending = mod.pending.length;
  const installed = mod.installed.length;
  const game = projectState.workspaces.length > 1 && workspace && workspace.game
    ? workspace.game.toUpperCase() + ': ' : '';
  let text;
  if (!mod.edited.length && !installed) text = 'nothing edited yet';
  else if (pending) text = `${pending} edit${pending === 1 ? '' : 's'} not in the game yet`;
  else if (installed) text = `${installed} file${installed === 1 ? '' : 's'} in the game`;
  else text = 'nothing to install';
  if (mod.changed.length) text += ` · ${mod.changed.length} changed since`;
  $('#mod-state').textContent = game + text;
  box.title = `originals kept in ${mod.backup}`;
}

// ------------------------------------------------------------------ the start page

/// What the document area shows when nothing is open. It is drawn on every refresh
/// and hides itself whenever a document is open, so it costs nothing to keep current.

// The start page (drawStartPage, showStartPage) and Create a mod (newProjectDialog) are in hub.js.
