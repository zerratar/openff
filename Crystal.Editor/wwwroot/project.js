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
    { label: 'New project…', run: newProjectDialog },
    { label: 'Open project…', run: openProjectDialog },
    { label: 'Start page', run: showStartPage },
    '-',
    {
      label: edited ? `Changes… (${edited})` : 'Changes…',
      run: changesDialog,
      disabled: !edited,
    },
    { label: 'Project settings…', run: projectSettingsDialog, disabled: !open },
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
    { label: 'Run in OpenFF', run: runInOpenFF, disabled: !(open && open.client),
      note: open && open.client ? 'Export to the mods folder and start the client (a running client hot-reloads)' : 'No OpenFF client found: build OpenFF or start OpenFF.exe once' },
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
    items.push({ label: 'Run in OpenFF', run: runInOpenFF, disabled: !open.client,
      note: open.client ? 'Export and start the client (a running client hot-reloads)' : 'No OpenFF client found: build OpenFF or start OpenFF.exe once' });
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

function newProjectDialog() {
  const body = dialog('New project');

  const columns = document.createElement('div');
  columns.className = 'dialog-columns';
  const name = field(columns, 'Name', '', { placeholder: 'e.g. Harder Bosses' });
  const author = field(columns, 'Author', '', { placeholder: 'your name or handle' });
  body.append(columns);
  const description = field(body, 'Description', '',
    { multiline: true, placeholder: 'What the mod does. Ends up in the README when you export.' });

  section(body, 'What kind of mod, for which games');
  // An OpenFF mod of both games when the client's content is here, else a Steam mod of
  // whatever is installed - the choice most people open the editor for.
  const found = name => (projectState.targets.find(t => t.name === name) || {}).content;
  const preset = found('ours') ? ['ours', 'oursff4'].filter(found)
    : ['steam', 'ff4steam'].filter(found);
  const picked = gameRows(body, preset);

  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A project is a folder: project.json plus the files you edit. An OpenFF mod '
    + 'is played by the OpenFF client from its mods folder and may take from both games; a '
    + 'Steam mod is the game\'s own files, replaced. Each game opens as a tab above the '
    + 'libraries with its own edits - the two name their files alike, so they are kept apart.';
  body.append(note);

  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const targets = picked();
    if (!name.value.trim()) { problem.textContent = 'The project needs a name.'; name.focus(); return; }
    if (!targets.length) { problem.textContent = 'Pick at least one game.'; return; }
    go.disabled = true;
    try {
      const result = await api('/api/project/create', { name: name.value.trim(), targets });
      if (!result.ok) throw new Error(result.error);
      if (author.value.trim() || description.value.trim()) {
        await api('/api/project/save', { author: author.value.trim(), description: description.value.trim() });
      }
      body.close();
      await reloadEverything(`created ${result.name}`);
    } catch (error) {
      problem.textContent = error.message;
      go.disabled = false;
    }
  };
  actions.append(go);
  body.append(actions);
  name.focus();
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
    const row = document.createElement('button');
    row.className = 'dialog-row project-row';
    if (project.current) row.classList.add('checked');
    const title = document.createElement('strong');
    title.append(projectKindIcon(project), document.createTextNode(project.name));
    const where = document.createElement('span');
    where.textContent = project.targets.map(describeTarget).join(', ')
      + '  ·  ' + project.directory;
    row.append(title, where);
    row.onclick = () => openProjectAt(project.directory, body);
    list.append(row);
  }
  body.append(list);
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
    say(result.started
      ? `exported to ${result.path} - OpenFF is starting${at}`
      : `exported to ${result.path} - OpenFF is already running and picks the code up; re-enter the map for scene changes${where.map ? ' (close it for a start on ' + where.map + ')' : ''}`, 'good');
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
// The start page is a page: it has an × and stays away for the session once closed
// (File ▸ Start page brings it back), so the document area is not a brochure every time
// the last tab closes.
const START_KEY = 'crystal-start-page';
function startPageHidden() {
  try { return sessionStorage.getItem(START_KEY) === 'closed'; } catch (error) { return false; }
}
function showStartPage() {
  try { sessionStorage.removeItem(START_KEY); } catch (error) { }
  drawStartPage();
}

async function drawStartPage() {
  const start = $('#no-docs');
  if (!start) return;
  start.textContent = '';

  if (startPageHidden()) {
    const quiet = document.createElement('p');
    quiet.className = 'start-quiet';
    quiet.textContent = 'Nothing open. ';
    const back = document.createElement('a');
    back.href = '#';
    back.textContent = 'Start page';
    back.onclick = event => { event.preventDefault(); showStartPage(); };
    quiet.append(back);
    start.append(quiet);
    return;
  }

  const card = document.createElement('div');
  card.className = 'start-card';

  const brand = document.createElement('div');
  brand.className = 'start-brand';
  const title = document.createElement('h2');
  title.textContent = 'Crystal';
  const tag = document.createElement('span');
  tag.textContent = 'the OpenFF editor · Final Fantasy III & IV (3D)';
  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  shut.title = 'Close the start page (File ▸ Start page brings it back)';
  shut.onclick = () => {
    try { sessionStorage.setItem(START_KEY, 'closed'); } catch (error) { }
    drawStartPage();
  };
  brand.append(title, tag, shut);
  card.append(brand);

  const open = projectState.project;
  const box = document.createElement('div');
  box.className = 'start-project';
  if (open) {
    box.classList.add('open');
    // Said outright: this is the project that is open, not one of a list.
    const eyebrow = document.createElement('div');
    eyebrow.className = 'start-eyebrow';
    eyebrow.textContent = 'Open project';
    box.append(eyebrow);
    const name = document.createElement('h3');
    name.textContent = open.name + (open.version ? `  v${open.version}` : '');
    const meta = document.createElement('div');
    meta.className = 'meta';
    meta.textContent = [open.author && `by ${open.author}`, open.description].filter(Boolean).join(' — ')
      || open.directory;
    box.append(name, meta);

    const games = document.createElement('div');
    games.className = 'start-games';
    for (const w of projectState.workspaces) {
      const g = document.createElement('button');
      g.className = 'start-game';
      const b = document.createElement('b');
      const mod = projectState.mods[w.target];
      const edited = (mod && mod.edited.length) || 0;
      b.textContent = `${w.label}${edited ? `  ·  ${edited} edited` : ''}`;
      const s = document.createElement('span');
      s.textContent = `${w.files} files · ${w.contentDirectory}`;
      s.title = w.contentDirectory;
      g.append(b, s);
      g.onclick = () => { if (typeof selectWorkspace === 'function') selectWorkspace(w.target); };
      games.append(g);
    }
    for (const [target, why] of Object.entries(projectState.missing || {})) {
      const g = document.createElement('div');
      g.className = 'start-game missing';
      const b = document.createElement('b');
      b.textContent = describeTarget(target);
      const s = document.createElement('span');
      s.textContent = why;
      g.append(b, s);
      games.append(g);
    }
    box.append(games);

    const actions = document.createElement('div');
    actions.className = 'start-actions';
    const button = (label, run, primary) => {
      const b = document.createElement('button');
      b.textContent = label;
      if (primary) b.className = 'primary';
      b.onclick = run;
      return b;
    };
    const edited = totalEdited();
    actions.append(
      button(edited ? `Changes (${edited})` : 'Changes', changesDialog),
      button('Settings', projectSettingsDialog),
      button('Export .zip', exportProject),
      button('Export to OpenFF', exportToOpenFF),
    );
    box.append(actions);
  } else {
    const name = document.createElement('h3');
    name.textContent = 'No project open';
    const meta = document.createElement('div');
    meta.className = 'meta';
    const current = projectState.workspaces[0];
    meta.textContent = current
      ? `Browsing ${current.label} read-only: ${current.files} files in ${current.contentDirectory}. `
        + 'Edits still save to a scratch override; make a project to keep them under a name.'
      : 'Make a project to start editing.';
    box.append(name, meta);
    const actions = document.createElement('div');
    actions.className = 'start-actions';
    const make = document.createElement('button');
    make.className = 'primary';
    make.textContent = 'New project…';
    make.onclick = newProjectDialog;
    const openButton = document.createElement('button');
    openButton.textContent = 'Open project…';
    openButton.onclick = openProjectDialog;
    actions.append(make, openButton);
    box.append(actions);
  }
  card.append(box);

  const columns = document.createElement('div');
  columns.className = 'start-columns';

  // With nothing open, the projects are the point of the page. With one open they are
  // a way to switch, folded away under a heading that says so, so nobody reads the list
  // as "which of these did I open?".
  const recent = document.createElement(open ? 'details' : 'div');
  recent.className = 'start-projects';
  const recentHead = document.createElement(open ? 'summary' : 'h4');
  recentHead.textContent = open ? 'Switch to another project…' : 'Projects';
  recent.append(recentHead);
  const list = document.createElement('div');
  list.className = 'start-list';
  recent.append(list);
  if (open) {
    const more = document.createElement('div');
    more.className = 'start-actions small';
    const make = document.createElement('button');
    make.textContent = 'New project…';
    make.onclick = newProjectDialog;
    const browse = document.createElement('button');
    browse.textContent = 'Open project…';
    browse.onclick = openProjectDialog;
    more.append(make, browse);
    recent.append(more);
  }
  columns.append(recent);

  const tips = document.createElement('div');
  const tipsHead = document.createElement('h4');
  tipsHead.textContent = 'How it works';
  const ul = document.createElement('ul');
  ul.className = 'start-tips';
  for (const [strong, rest] of [
    ['Browse', ' the libraries in the panel below - maps, scripts, text, tables, models, sound. Click to inspect, double-click to open.'],
    ['Edit and save.', ' Every save goes into the project, never into the game.'],
    ['Project ▸ Install', ' copies the edits into the game and keeps the originals; Remove puts them back.'],
    ['FF3 and FF4', ' are the tabs above the libraries: whose content the panel shows. An OpenFF mod may open both and take from either; every document tab says which game it is.'],
    ['OpenFF mod', ', at the bottom of the tree, is the project\'s own: its C# code, and the maps it puts behaviours on. Open them here, or in your IDE; Build compiles; Export to OpenFF writes the mod.'],
    ['Export as .zip', ' packs the project with a README - what you upload.'],
  ]) {
    const li = document.createElement('li');
    const b = document.createElement('b');
    b.textContent = strong;
    li.append(b, document.createTextNode(rest));
    ul.append(li);
  }
  tips.append(tipsHead, ul);
  columns.append(tips);
  card.append(columns);
  start.append(card);

  try {
    const projects = (await api('/api/projects')).filter(p => !(open && p.current));
    if (!projects.length) {
      const none = document.createElement('p');
      none.className = 'dialog-note';
      none.textContent = open ? 'No other projects.' : 'None yet.';
      list.append(none);
    }
    for (const project of projects.slice(0, 8)) {
      const row = document.createElement('button');
      row.className = 'start-row';
      const b = document.createElement('b');
      b.append(projectKindIcon(project), document.createTextNode(project.name));
      const s = document.createElement('span');
      s.textContent = project.targets.map(describeTarget).join(', ');
      row.append(b, s);
      row.onclick = () => openProjectAt(project.directory);
      list.append(row);
    }
  } catch (error) {
    // The start page is a convenience; a failed listing is not worth a message.
  }
}
