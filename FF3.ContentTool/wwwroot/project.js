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
    { label: 'Show project folder', run: () => revealProject(), disabled: !open },
  ]));

  // One entry per game the project has open. With one game it reads as it always
  // did; with two, each line says which game it means.
  const many = projectState.workspaces.length > 1;
  const items = [];
  for (const w of projectState.workspaces) {
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
  const close = document.createElement('button');
  close.textContent = 'Close';
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

/// A row per game: a checkbox, the game's name, and where it was found (or that it
/// was not). Returns a function giving the checked target names.
function gameRows(parent, checked) {
  const list = document.createElement('div');
  list.className = 'dialog-list';
  const boxes = [];
  for (const target of projectState.targets) {
    const row = document.createElement('label');
    row.className = 'dialog-game' + (target.content ? '' : ' missing');
    const box = document.createElement('input');
    box.type = 'checkbox';
    box.checked = checked.includes(target.name) && !!target.content;
    box.disabled = !target.content;
    row.classList.toggle('checked', box.checked);
    box.onchange = () => row.classList.toggle('checked', box.checked);
    const what = document.createElement('div');
    what.className = 'what';
    const name = document.createElement('b');
    name.textContent = target.label;
    const where = document.createElement('span');
    where.textContent = target.content || 'not found on this machine';
    where.title = target.content || '';
    what.append(name, where);
    row.append(box, what);
    boxes.push({ box, name: target.name });
    list.append(row);
  }
  parent.append(list);
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

  section(body, 'For which games');
  const picked = gameRows(body, ['steam']);

  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A project is a folder: project.json plus the files you edit. Targeting both '
    + 'games opens them side by side, each with its own edits - the two name their files '
    + 'alike, so they are kept apart.';
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
    row.className = 'dialog-row';
    if (project.current) row.classList.add('checked');
    const title = document.createElement('strong');
    title.textContent = project.name;
    const where = document.createElement('span');
    where.textContent = project.targets.map(describeTarget).join(', ')
      + '  ·  ' + project.directory;
    row.append(title, where);
    row.onclick = () => openProjectAt(project.directory, body);
    list.append(row);
  }
  body.append(list);
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

  section(body, 'Games');
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
  const grow = document.createElement('span');
  grow.className = 'grow';
  actions.append(go, grow, reveal, exportButton);
  body.append(actions);
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
  if (typeof docs !== 'undefined' && typeof closeDoc === 'function') {
    for (const id of [...docs.keys()]) closeDoc(id);
  }
  state.ws = null;
  state.files = [];
  await refreshProject();
  if (typeof resetOps === 'function') resetOps();
  if (typeof selectKind === 'function') await selectKind(typeof browseKind !== 'undefined' ? browseKind : 'map');
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
  drawStartPage();
  if (typeof drawProjectTree === 'function') drawProjectTree();
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
async function drawStartPage() {
  const start = $('#no-docs');
  if (!start) return;
  start.textContent = '';

  const card = document.createElement('div');
  card.className = 'start-card';

  const brand = document.createElement('div');
  brand.className = 'start-brand';
  const title = document.createElement('h2');
  title.textContent = 'Crystal';
  const tag = document.createElement('span');
  tag.textContent = 'the OpenFF editor · Final Fantasy III & IV (3D)';
  brand.append(title, tag);
  card.append(brand);

  const open = projectState.project;
  const box = document.createElement('div');
  box.className = 'start-project';
  if (open) {
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

  const recent = document.createElement('div');
  const recentHead = document.createElement('h4');
  recentHead.textContent = 'Projects';
  recent.append(recentHead);
  const list = document.createElement('div');
  list.className = 'start-list';
  recent.append(list);
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
    ['Two games', ' can be open at once; the tabs above the libraries switch between them, and every document tab says which game it is.'],
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
    const projects = await api('/api/projects');
    if (!projects.length) {
      const none = document.createElement('p');
      none.className = 'dialog-note';
      none.textContent = 'None yet.';
      list.append(none);
    }
    for (const project of projects.slice(0, 8)) {
      const row = document.createElement('button');
      row.className = 'start-row';
      const b = document.createElement('b');
      b.textContent = project.name + (project.current ? '  (open)' : '');
      const s = document.createElement('span');
      s.textContent = project.targets.map(describeTarget).join(', ');
      row.append(b, s);
      row.onclick = () => { if (!project.current) openProjectAt(project.directory); };
      list.append(row);
    }
  } catch (error) {
    // The start page is a convenience; a failed listing is not worth a message.
  }
}
