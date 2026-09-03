// The menu bar, and the projects behind it.
//
// A project is one mod: a directory with a name, a note of which game it is for, and
// the edited files under it. Editing already wrote to an override directory - this is
// that directory made into a thing you can name, switch between, and hand to somebody.
//
// Two menus rather than the usual four. File and Project are the ones with something
// behind them; an Edit menu whose items did nothing would be worse than not having it,
// since undo here belongs to whichever editor has focus.

'use strict';

let projectState = { project: null, targets: [], mod: null };

// ------------------------------------------------------------------ the menu bar

/// Builds one menu. `items` are {label, run, checked, disabled} or the string '-'.
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
    const entry = document.createElement('button');
    entry.className = 'menu-item';
    entry.textContent = item.label;
    if (item.checked) entry.classList.add('checked');
    if (item.disabled) entry.disabled = true;
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
  const mod = projectState.mod;

  const edited = (mod && mod.edited.length) || 0;

  bar.append(buildMenu('File', [
    { label: 'New project…', run: newProjectDialog },
    { label: 'Open project…', run: openProjectDialog },
    '-',
    {
      label: edited ? `Changes… (${edited})` : 'Changes…',
      run: changesDialog,
      disabled: !edited,
    },
    { label: 'Project details…', run: projectDetailsDialog, disabled: !open },
  ]));

  // Switching target changes which game the same edits are read on top of. It is on
  // the menu rather than a dropdown because it is rare and consequential.
  const targets = projectState.targets.map(t => ({
    label: `Edit against ${t.label}${t.content ? '' : '  (not found)'}`,
    checked: open && open.active === t.name,
    disabled: !open || !t.content,
    run: () => setTarget(t.name),
  }));

  bar.append(buildMenu('Project', [
    ...targets,
    '-',
    {
      label: 'Install into the game',
      disabled: !mod || !mod.canInstall || !mod.edited.length,
      run: () => runMod('/api/mod/install', 'install'),
    },
    {
      label: 'Remove from the game',
      disabled: !mod || !mod.canInstall || (!mod.installed.length && !mod.changed.length),
      run: () => runMod('/api/mod/uninstall', 'uninstall'),
    },
  ]));

  const where = $('#project-name');
  if (where) {
    where.textContent = open
      ? `${open.name} · ${describeTarget(open.active)}`
      : 'no project';
    where.title = open ? open.directory : 'File ▸ New project…';
  }
}

function describeTarget(name) {
  const found = projectState.targets.find(t => t.name === name);
  return found ? found.label : name;
}

// ------------------------------------------------------------------ the dialogs

/// The shell every dialog here shares. Returns the body to fill.
function dialog(title, { onClose } = {}) {
  const veil = document.createElement('div');
  veil.className = 'picker-veil';
  const box = document.createElement('div');
  box.className = 'picker dialog';
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
  body.close = shut;
  return body;
}

function field(parent, label, value = '') {
  const wrap = document.createElement('label');
  wrap.className = 'dialog-field';
  wrap.textContent = label;
  const input = document.createElement('input');
  input.value = value;
  wrap.append(input);
  parent.append(wrap);
  return input;
}

function newProjectDialog() {
  const body = dialog('New project');
  const name = field(body, 'Name', '');

  const what = document.createElement('div');
  what.className = 'dialog-field';
  what.textContent = 'For which game';
  const boxes = [];
  for (const target of projectState.targets) {
    const line = document.createElement('label');
    line.className = 'dialog-check';
    const box = document.createElement('input');
    box.type = 'checkbox';
    box.checked = target.name === 'steam' && !!target.content;
    box.disabled = !target.content;
    boxes.push({ box, name: target.name });
    line.append(box, document.createTextNode(
      target.content ? target.label : `${target.label} — not found on this machine`));
    what.append(line);
  }
  body.append(what);

  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = 'A project is a folder: project.json plus the files you edit. '
    + 'Targeting both is worth it for data — maps, text and scripts are largely the '
    + 'same in the two releases — but art is laid out for a different screen size in each.';
  body.append(note);

  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    const targets = boxes.filter(b => b.box.checked).map(b => b.name);
    if (!name.value.trim()) return say('the project needs a name', 'bad');
    if (!targets.length) return say('pick at least one game', 'bad');
    try {
      const result = await api('/api/project/create',
        { name: name.value.trim(), targets });
      if (!result.ok) throw new Error(result.error);
      body.close();
      await reloadEverything(`created ${result.name}`);
    } catch (error) {
      say(error.message, 'bad');
    }
  };
  body.append(go);
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
    row.onclick = async () => {
      try {
        const result = await api('/api/project/open', { directory: project.directory });
        if (!result.ok) throw new Error(result.error);
        body.close();
        await reloadEverything(`opened ${result.name}`);
      } catch (error) {
        say(error.message, 'bad');
      }
    };
    list.append(row);
  }
  body.append(list);
}

function projectDetailsDialog() {
  const open = projectState.project;
  if (!open) return;
  const body = dialog('Project details');
  const name = field(body, 'Name', open.name || '');
  const author = field(body, 'Author', open.author || '');
  const version = field(body, 'Version', open.version || '');
  const description = field(body, 'Description', open.description || '');

  const where = document.createElement('p');
  where.className = 'dialog-note';
  where.textContent = `Folder: ${open.directory}`;
  body.append(where);

  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Save';
  go.onclick = async () => {
    try {
      const result = await api('/api/project/save', {
        name: name.value, author: author.value,
        version: version.value, description: description.value,
      });
      if (!result.ok) throw new Error(result.error);
      body.close();
      await reloadEverything('saved');
    } catch (error) {
      say(error.message, 'bad');
    }
  };
  body.append(go);
}

/// Everything the project has changed, and a way to throw any of it away.
///
/// Reverting is two things at once, and the dialog says so: the edit is deleted, and
/// if it had been installed the game's own file goes back at the same time. Undoing
/// only the first half is the trap - the file would read as shipped everywhere except
/// in the game, which is the one place it matters.
async function changesDialog() {
  const body = dialog('Changes');
  let mod;
  try {
    mod = await api('/api/mod/status');
  } catch (error) {
    say(error.message, 'bad');
    return;
  }

  if (!mod.edited.length) {
    const empty = document.createElement('p');
    empty.className = 'dialog-note';
    empty.textContent = 'Nothing edited in this project yet.';
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
      const result = await api('/api/mod/revert', { names });
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
    await reloadEverything(`now editing against ${describeTarget(target)}`);
  } catch (error) {
    say(error.message, 'bad');
  }
}

async function runMod(endpoint, kind) {
  const asking = kind === 'install'
    ? 'Copy your edits over the game\'s own files?\n\n'
      + 'The originals are kept, and "Remove from the game" puts them back.'
    : 'Put the game\'s original files back?\n\nYour edits stay in the project.';
  if (!confirm(asking)) return;
  try {
    const result = await api(endpoint, {});
    if (!result.ok) throw new Error(result.error);
    const notes = (result.notes || []).join('  ·  ');
    const summary = kind === 'install'
      ? `${result.wrote.length} file(s) written into the game`
      : `${result.restored.length} restored, ${result.removed.length} removed`
        + (result.skipped.length ? `, ${result.skipped.length} left alone` : '');
    say(summary + (notes ? '  ·  ' + notes : ''), notes ? 'bad' : 'good');
  } catch (error) {
    say(error.message, 'bad');
  }
  await refreshProject();
}

/// Re-reads what is open. Switching project or target changes the content underneath
/// every open tab, so those go too rather than being left showing another game's data.
async function reloadEverything(message) {
  await refreshProject();
  // Every open tab is showing the other game's data now, so they go. Copying the
  // ids first, because closeDoc mutates the map it is iterating.
  if (typeof docs !== 'undefined' && typeof closeDoc === 'function') {
    for (const id of [...docs.keys()]) closeDoc(id);
  }
  if (typeof drawProjectTree === 'function') drawProjectTree();
  if (typeof drawHierarchy === 'function') drawHierarchy();
  if (typeof drawInspector === 'function') drawInspector();
  if (message) say(message, 'good');
}

async function refreshProject() {
  try {
    const [status, targets, mod] = await Promise.all([
      api('/api/status'),
      api('/api/targets'),
      api('/api/mod/status').catch(() => null),
    ]);
    projectState = { project: status.project, targets, mod };
    state.language = status.language || 'en';
    state.messagePrefix = status.messagePrefix || `${state.language}.lproj/`;
    $('#summary').textContent =
      `${status.files} files  ·  ${status.contentDirectory}`;
    showModState(mod);
  } catch (error) {
    say(error.message, 'bad');
  }
  drawMenuBar();
}

/// Whether the edits are in the game yet. Hidden for our own build, which reads the
/// project directly and so has nothing to install.
function showModState(mod) {
  const box = $('#mod');
  if (!box) return;
  if (!mod || !mod.canInstall) {
    box.hidden = true;
    return;
  }
  box.hidden = false;

  const pending = mod.pending.length;
  const installed = mod.installed.length;
  let text;
  if (!mod.edited.length && !installed) text = 'nothing edited yet';
  else if (pending) text = `${pending} edit${pending === 1 ? '' : 's'} not in the game yet`;
  else if (installed) text = `${installed} file${installed === 1 ? '' : 's'} in the game`;
  else text = 'nothing to install';
  if (mod.changed.length) text += ` · ${mod.changed.length} changed since`;
  $('#mod-state').textContent = text;
  box.title = `originals kept in ${mod.backup}`;
}
