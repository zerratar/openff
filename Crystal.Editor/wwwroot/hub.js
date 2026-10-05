// The start page: Crystal's hub, as Unity Hub is one. "What do you want to do today?" - install a sample mod to play,
// create a mod (empty, or from a sample), open a project - over three pages: Home (those actions, the open project, the
// projects worked on last), Projects (every project, filterable) and Samples (the sample browser: each sample's picture,
// version, tags and how it stands in OpenFF's mods folder, with Install, Update, Uninstall and Create a mod from it).
//
// Two kinds of mod come out of it, kept apart on purpose: a sample *installed* is the sample as it ships, in the client's
// mods folder with a sample.json, and a later release updates it (here, or from OpenFF's MODS list); a mod *created* from
// a sample is a project of your own - a copy to change - and nothing ever overwrites it.
//
// The Create a mod dialog is here too: a starting point, then the project's name, author, version and description.
//
// Mods is Crystal as a mod manager: every mod in OpenFF's mods folder (on / off, update, remove) and, for each Steam or GOG
// copy of the games - which have no mod list of their own - what Crystal's projects installed into it (uninstall puts the
// originals back) and how many of the game's files differ from the release with no install of Crystal's behind them.

const START_KEY = 'crystal-start-page';
const START_TAB = 'crystal-start-tab';
// The last /api/samples answer (each sample hashed against its install - not redone on every redraw).
let hubSamples = null;
let hubSampleFilter = { text: '', chip: 'all' };
let hubProjectFilter = '';

function hubEl(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text != null) node.textContent = text;
  return node;
}

function startPageHidden() {
  try { return sessionStorage.getItem(START_KEY) === 'closed'; } catch (error) { return false; }
}

/// The start page shown again (File ▸ Start page), on one of its pages.
function showStartPage(tab) {
  try { sessionStorage.removeItem(START_KEY); } catch (error) { }
  if (tab) hubSetTab(tab, false);
  drawStartPage();
}

function hubTab() {
  try { return sessionStorage.getItem(START_TAB) || 'home'; } catch (error) { return 'home'; }
}

function hubSetTab(tab, draw = true) {
  try { sessionStorage.setItem(START_TAB, tab); } catch (error) { }
  if (draw) drawStartPage();
}

async function hubLoadSamples(force) {
  if (hubSamples && !force) return hubSamples;
  try { hubSamples = await api('/api/samples'); } catch (error) { hubSamples = { samples: [], error: error.message }; }
  return hubSamples;
}

function hubUpdates() {
  return ((hubSamples && hubSamples.samples) || []).filter(s => s.install && (s.install.state === 'update' || s.install.state === 'copy'));
}

async function drawStartPage() {
  const start = $('#no-docs');
  if (!start) return;

  if (startPageHidden()) {
    start.textContent = '';
    const quiet = hubEl('p', 'start-quiet', 'Nothing open. ');
    const back = hubEl('a', null, 'Start page');
    back.href = '#';
    back.onclick = event => { event.preventDefault(); showStartPage(); };
    quiet.append(back);
    start.append(quiet);
    return;
  }

  const tab = hubTab();
  let projects = [];
  try { projects = await api('/api/projects'); } catch (error) { /* the page is a convenience */ }
  await hubLoadSamples(false);

  start.textContent = '';
  const card = hubEl('div', 'start-card hub');

  const brand = hubEl('div', 'start-brand');
  const title = hubEl('h2', null, 'Crystal');
  const tag = hubEl('span', null, 'the OpenFF editor · Final Fantasy III & IV (3D)');
  const shut = hubEl('button', 'shut', '×');
  shut.title = 'Close the start page (File ▸ Start page brings it back)';
  shut.onclick = () => {
    try { sessionStorage.setItem(START_KEY, 'closed'); } catch (error) { }
    drawStartPage();
  };
  brand.append(title, tag, shut);
  card.append(brand);

  // The pages, as tabs under the name.
  const nav = hubEl('div', 'hub-nav');
  const updates = hubUpdates().length;
  for (const [id, label, count] of [['home', 'Home'], ['projects', 'Projects', projects.length], ['samples', 'Samples', updates], ['mods', 'Mods']]) {
    const b = hubEl('button', 'hub-tab' + (tab === id ? ' on' : ''), label);
    if (count) {
      const badge = hubEl('span', 'hub-count' + (id === 'samples' ? ' update' : ''), String(count));
      badge.title = id === 'samples' ? `${count} installed sample(s) with an update` : `${count} project(s)`;
      b.append(badge);
    }
    b.onclick = () => hubSetTab(id);
    nav.append(b);
  }
  card.append(nav);

  const page = hubEl('div', 'hub-page');
  card.append(page);
  start.append(card);

  if (tab === 'projects') hubProjectsPage(page, projects);
  else if (tab === 'mods') hubModsPage(page);
  else if (tab === 'samples') hubSamplesPage(page);
  else hubHomePage(page, projects);
}

// ------------------------------------------------------------------------- Home

function hubHomePage(page, projects) {
  const open = projectState.project;
  if (open) page.append(hubOpenProject(open));

  page.append(hubEl('h3', 'hub-question', 'What do you want to do today?'));
  const tiles = hubEl('div', 'hub-tiles');
  const samples = (hubSamples && hubSamples.samples) || [];
  const installed = samples.filter(s => s.install && ['installed', 'update', 'copy'].includes(s.install.state)).length;
  const updates = hubUpdates().length;
  const tile = (iconName, heading, text, run, badge) => {
    const b = hubEl('button', 'hub-tile');
    const i = icon(iconName);
    const h = hubEl('b', null, heading);
    if (badge) h.append(hubEl('span', 'hub-count update', badge));
    b.append(i, h, hubEl('span', null, text));
    b.onclick = run;
    tiles.append(b);
  };
  tile('download', 'Install a sample mod',
    `Play a finished mod in OpenFF - Rogue Mode, Mastery, the Showcase... ${samples.length ? `${samples.length} samples, ${installed} installed.` : ''} Installed samples stay up to date.`,
    () => hubSetTab('samples'), updates ? String(updates) : null);
  tile('add', 'Create a mod',
    'A project of your own: start empty, or from a copy of a sample to read and change.',
    () => newProjectDialog());
  tile('folder', 'Open a project',
    projects.length ? `${projects.length} project${projects.length === 1 ? '' : 's'} on this machine.` : 'None yet - create one first.',
    () => hubSetTab('projects'));
  page.append(tiles);

  const columns = hubEl('div', 'start-columns');
  const recent = hubEl('div', 'start-projects');
  recent.append(hubEl('h4', null, open ? 'Other projects' : 'Recent projects'));
  const list = hubEl('div', 'start-list');
  const others = projects.filter(p => !(open && p.current));
  if (!others.length) list.append(hubEl('p', 'dialog-note', open ? 'No other projects.' : 'None yet.'));
  for (const project of others.slice(0, 6)) list.append(hubProjectRow(project, false));
  if (others.length > 6) {
    const more = hubEl('a', 'hub-more', `All ${others.length} projects…`);
    more.href = '#';
    more.onclick = event => { event.preventDefault(); hubSetTab('projects'); };
    list.append(more);
  }
  recent.append(list);
  columns.append(recent);

  const tips = hubEl('div');
  tips.append(hubEl('h4', null, 'How it works'));
  const ul = hubEl('ul', 'start-tips');
  for (const [strong, rest] of [
    ['Browse', ' the libraries in the panel below - maps, scripts, text, tables, models, sound. Click to inspect, double-click to open.'],
    ['Edit and save.', ' Every save goes into the project, never into the game.'],
    ['FF3 and FF4', ' are the tabs above the libraries: whose content the panel shows.'],
    ['Mod', ', the tab beside the games, is the project\'s own side: its C# code, scenes, items, characters, monsters and strings. Build compiles; Export to OpenFF writes the mod.'],
    ['Help ▸ Guide', ' is the modding guide - short tutorials with pictures.'],
  ]) {
    const li = hubEl('li');
    li.append(hubEl('b', null, strong), document.createTextNode(rest));
    ul.append(li);
  }
  tips.append(ul);
  columns.append(tips);
  page.append(columns);
}

/// The project open now: its games, and the things done with it.
function hubOpenProject(open) {
  const box = hubEl('div', 'start-project open');
  box.append(hubEl('div', 'start-eyebrow', 'Open project'));
  box.append(hubEl('h3', null, open.name + (open.version ? `  v${open.version}` : '')));
  box.append(hubEl('div', 'meta', [open.author && `by ${open.author}`, open.description].filter(Boolean).join(' — ') || open.directory));

  const games = hubEl('div', 'start-games');
  for (const w of projectState.workspaces) {
    const g = hubEl('button', 'start-game');
    const mod = projectState.mods[w.target];
    const edited = (mod && mod.edited.length) || 0;
    const s = hubEl('span', null, `${w.files} files · ${w.contentDirectory}`);
    s.title = w.contentDirectory;
    g.append(hubEl('b', null, `${w.label}${edited ? `  ·  ${edited} edited` : ''}`), s);
    g.onclick = () => { if (typeof selectWorkspace === 'function') selectWorkspace(w.target); };
    games.append(g);
  }
  for (const [target, why] of Object.entries(projectState.missing || {})) {
    const g = hubEl('div', 'start-game missing');
    g.append(hubEl('b', null, describeTarget(target)), hubEl('span', null, why));
    games.append(g);
  }
  box.append(games);

  const actions = hubEl('div', 'start-actions');
  const button = (label, run) => { const b = hubEl('button', null, label); b.onclick = run; return b; };
  const edited = totalEdited();
  actions.append(
    button(edited ? `Changes (${edited})` : 'Changes', changesDialog),
    button('Settings', projectSettingsDialog),
    button('Export .zip', exportProject),
    button('Export to OpenFF', exportToOpenFF),
  );
  if (open.sample) actions.append(button(`Update from the ${open.sample} sample…`, updateFromSampleDialog));
  box.append(actions);
  return box;
}

function hubProjectRow(project, full) {
  const line = hubEl('div', 'project-line');
  const row = hubEl('button', full ? 'dialog-row project-row' : 'start-row');
  const name = hubEl(full ? 'strong' : 'b');
  name.append(projectKindIcon(project), document.createTextNode(project.name));
  if (project.current) name.append(hubEl('em', 'project-open', 'open'));
  row.append(name, hubEl('span', null, project.targets.map(describeTarget).join(', ') + (full ? '  ·  ' + project.directory : '')));
  row.title = project.directory;
  row.onclick = () => openProjectAt(project.directory);
  line.append(row);
  if (full) {
    const bin = hubEl('button', 'project-delete');
    bin.title = `Delete ${project.name}…`;
    bin.append(icon('trash'));
    bin.onclick = () => deleteProjectDialog(project);
    line.append(bin);
  }
  return line;
}

// --------------------------------------------------------------------- Projects

function hubProjectsPage(page, projects) {
  const bar = hubEl('div', 'hub-bar');
  const search = hubEl('input', 'hub-search');
  search.type = 'search';
  search.placeholder = 'Filter projects';
  search.value = hubProjectFilter;
  const make = hubEl('button', 'primary', 'Create a mod…');
  make.onclick = () => newProjectDialog();
  bar.append(search, make);
  page.append(bar);

  const list = hubEl('div', 'dialog-list hub-projects');
  const fill = () => {
    list.textContent = '';
    const words = hubProjectFilter.toLowerCase().split(/\s+/).filter(Boolean);
    const shown = projects.filter(p => words.every(w => (p.name + ' ' + p.directory + ' ' + p.targets.map(describeTarget).join(' ')).toLowerCase().includes(w)));
    if (!projects.length) list.append(hubEl('p', 'dialog-note', 'No projects yet. Create a mod makes one - empty, or from a sample.'));
    else if (!shown.length) list.append(hubEl('p', 'dialog-note', 'No project matches.'));
    for (const project of shown) list.append(hubProjectRow(project, true));
  };
  search.oninput = () => { hubProjectFilter = search.value; fill(); };
  fill();
  page.append(list);
}

// ---------------------------------------------------------------------- Samples

const HUB_STATES = {
  none: null,
  installed: { label: 'Installed', tone: 'good', note: 'In OpenFF\'s mods folder as the sample ships' },
  update: { label: 'Update', tone: 'mark', note: 'This version of the sample is newer than the one installed' },
  copy: { label: 'Old copy', tone: 'mark', note: 'A copy made by hand of an older sample: Update makes it the sample as it ships, kept up to date from then on' },
  build: { label: 'Needs a build', tone: 'dim', note: 'The sample has C# code and no built assembly here - build it (its install.cmd) to install it' },
  own: { label: 'Your own', tone: 'dim', note: 'The mods folder has a mod of yours under this name (a project\'s export): it is never overwritten' },
};

function hubSamplesPage(page) {
  const result = hubSamples || {};
  const samples = result.samples || [];

  const bar = hubEl('div', 'hub-bar');
  const search = hubEl('input', 'hub-search');
  search.type = 'search';
  search.placeholder = 'Filter samples';
  search.value = hubSampleFilter.text;
  const refresh = hubEl('button', null, 'Refresh');
  refresh.title = 'Look at the samples and the mods folder again';
  refresh.onclick = async () => { await hubLoadSamples(true); drawStartPage(); };
  bar.append(search, refresh);
  page.append(bar);

  const chips = hubEl('div', 'hub-chips');
  const tags = [...new Set(samples.flatMap(s => s.tags || []))].sort();
  const chipList = [['all', 'All'], ['installed', 'Installed'], ['update', 'Updates'], ['code', 'C# code'], ['nocode', 'No code'], ...tags.map(t => ['#' + t, t])];
  for (const [id, label] of chipList) {
    const c = hubEl('button', 'hub-chip' + (hubSampleFilter.chip === id ? ' on' : ''), label);
    c.onclick = () => { hubSampleFilter.chip = hubSampleFilter.chip === id ? 'all' : id; drawStartPage(); };
    chips.append(c);
  }
  page.append(chips);

  page.append(hubEl('p', 'dialog-note hub-where', result.mods
    ? `Installs go to OpenFF's mods folder: ${result.mods}`
    : 'No OpenFF client is known yet: start OpenFF once and its mods folder is found. Create a mod from a sample works meanwhile.'));

  const grid = hubEl('div', 'hub-grid');
  const fill = () => {
    grid.textContent = '';
    const words = hubSampleFilter.text.toLowerCase().split(/\s+/).filter(Boolean);
    const chip = hubSampleFilter.chip;
    const shown = samples.filter(s => {
      const state = s.install ? s.install.state : 'none';
      if (chip === 'installed' && !['installed', 'update', 'copy'].includes(state)) return false;
      if (chip === 'update' && !['update', 'copy'].includes(state)) return false;
      if (chip === 'code' && !s.code) return false;
      if (chip === 'nocode' && s.code) return false;
      if (chip.startsWith('#') && !(s.tags || []).includes(chip.slice(1))) return false;
      const text = [s.name, s.id, s.description, ...(s.tags || [])].join(' ').toLowerCase();
      return words.every(w => text.includes(w));
    });
    if (!samples.length) grid.append(hubEl('p', 'dialog-note', 'No samples were found beside Crystal' + (result.folder ? ' in ' + result.folder : '') + '. The release zip carries them in Samples\\.'));
    else if (!shown.length) grid.append(hubEl('p', 'dialog-note', 'No sample matches.'));
    for (const sample of shown) grid.append(hubSampleCard(sample, !!result.mods));
  };
  search.oninput = () => { hubSampleFilter.text = search.value; fill(); };
  fill();
  page.append(grid);
}

function hubPreview(sample, className) {
  const frame = hubEl('div', className);
  if (sample && sample.preview) {
    const img = hubEl('img');
    img.src = '/api/samples/preview?id=' + encodeURIComponent(sample.id);
    img.alt = '';
    img.loading = 'lazy';
    frame.append(img);
  } else {
    frame.classList.add('blank');
    frame.append(icon(sample ? 'mod' : 'add'));
  }
  return frame;
}

function hubSampleCard(sample, canInstall) {
  const install = sample.install || { state: 'none' };
  const card = hubEl('div', 'hub-sample');
  card.append(hubPreview(sample, 'hub-shot'));

  const head = hubEl('div', 'hub-sample-head');
  head.append(hubEl('b', null, sample.name));
  if (sample.version) head.append(hubEl('span', 'hub-version', 'v' + sample.version));
  const state = HUB_STATES[install.state];
  if (state) {
    const badge = hubEl('span', 'hub-state ' + state.tone,
      install.state === 'update' && install.version ? `${state.label} from v${install.version}` : state.label);
    badge.title = state.note;
    head.append(badge);
  }
  card.append(head);

  const what = hubEl('p', 'hub-sample-text', sample.description || '');
  what.title = sample.description || '';
  card.append(what);

  const tags = hubEl('div', 'hub-tags');
  if (sample.code) tags.append(hubEl('span', 'hub-tag csharp', 'C#'));
  for (const t of sample.tags || []) {
    const chip = hubEl('span', 'hub-tag', t);
    chip.onclick = () => { hubSampleFilter.chip = '#' + t; drawStartPage(); };
    tags.append(chip);
  }
  if ((sample.games || []).includes('ff4')) tags.append(hubEl('span', 'hub-tag', 'FF4'));
  card.append(tags);

  const actions = hubEl('div', 'hub-sample-actions');
  const s = install.state;
  if (canInstall && (s === 'none' || s === 'update' || s === 'copy')) {
    const go = hubEl('button', 'primary', s === 'none' ? 'Install' : 'Update');
    go.title = s === 'none' ? 'Copy the sample into OpenFF\'s mods folder to play it - kept up to date' : state.note;
    go.onclick = () => hubInstall(sample, go);
    actions.append(go);
  }
  if (install.managed && (s === 'installed' || s === 'update')) {
    const out = hubEl('button', null, 'Uninstall');
    out.title = 'Take it out of OpenFF\'s mods folder (the sample stays here to install again)';
    out.onclick = () => hubUninstall(sample, out);
    actions.append(out);
  }
  const make = hubEl('button', null, 'Create a mod from it…');
  make.title = 'A project of your own, a copy of the sample to read and change - never overwritten by an update';
  make.onclick = () => newProjectDialog({ from: sample.id });
  actions.append(make);
  card.append(actions);
  return card;
}

async function hubInstall(sample, button) {
  const install = sample.install || {};
  const replaced = (install.changed || []);
  const removed = install.state === 'copy' ? (install.extra || []) : [];
  if (replaced.length || removed.length) {
    // Files of the player's that the update replaces or takes away: said first.
    const body = dialog(`Update ${sample.name}?`, { wide: true });
    if (replaced.length) {
      body.append(hubEl('p', 'dialog-note sample-warn', `These ${replaced.length} file(s) of the installed ${sample.name} differ from the sample and are replaced:`));
      body.append(fileList(replaced));
    }
    if (removed.length) {
      body.append(hubEl('p', 'dialog-note', `These ${removed.length} file(s) the sample no longer has are taken away:`));
      body.append(fileList(removed));
    }
    body.append(hubEl('p', 'dialog-note', 'To change a sample and keep the changes, Create a mod from it instead - a project of your own is never updated over.'));
    const actions = hubEl('div', 'dialog-actions');
    const cancel = hubEl('button', null, 'Cancel');
    cancel.onclick = () => body.close();
    const go = hubEl('button', 'primary', 'Update');
    go.onclick = () => { body.close(); hubDoInstall(sample, button); };
    actions.append(cancel, go);
    body.append(actions);
    return;
  }
  await hubDoInstall(sample, button);
}

async function hubDoInstall(sample, button) {
  const was = sample.install ? sample.install.state : 'none';
  if (button) button.disabled = true;
  try {
    const result = await api('/api/samples/install', { id: sample.id });
    if (!result.ok) throw new Error(result.error);
    say(was === 'none'
      ? `${sample.name} is installed - OpenFF plays it from its next start (the MODS list on the title turns it off)`
      : `${sample.name} is updated to v${sample.version || '?'} - OpenFF takes it in at its next start`, 'good');
  } catch (error) {
    say(`${sample.name} was not installed: ${error.message}`, 'bad');
  }
  await hubLoadSamples(true);
  drawStartPage();
}

async function hubUninstall(sample, button) {
  button.disabled = true;
  try {
    const result = await api('/api/samples/uninstall', { id: sample.id });
    if (!result.ok) throw new Error(result.error);
    say(`${sample.name} is out of OpenFF's mods folder`, 'good');
  } catch (error) {
    say(`${sample.name} was not uninstalled: ${error.message}`, 'bad');
  }
  await hubLoadSamples(true);
  drawStartPage();
}

// ----------------------------------------------------------------- Create a mod

/// A new project: a starting point (empty, or one of the samples - a copy of it, the project's own), then its name,
/// author, version and description; an empty one picks its games and kinds, a sample's are the sample's.
async function newProjectDialog({ from } = {}) {
  const body = dialog('Create a mod', { wide: true });
  const samples = ((await hubLoadSamples(false)).samples) || [];
  const first = from ? samples.find(s => s.id === from) || null : null;
  let picked = null;

  const layout = hubEl('div', 'hub-create');
  const starts = hubEl('div', 'hub-starts');
  starts.append(hubEl('div', 'dialog-section', 'Starting point'));
  const startList = hubEl('div', 'hub-start-list');
  starts.append(startList);
  const form = hubEl('div', 'hub-form');
  layout.append(starts, form);
  body.append(layout);

  const name = field(form, 'Name', '', { placeholder: 'e.g. Harder Bosses' });
  const columns = hubEl('div', 'dialog-columns');
  const author = field(columns, 'Author', '', { placeholder: 'your name or handle' });
  const version = field(columns, 'Version', '1.0', { placeholder: '1.0' });
  form.append(columns);
  const description = field(form, 'Description', '', { multiline: true, placeholder: 'What the mod does. Ends up in the README when you export.' });
  const kindHead = section(form, 'What kind of mod, for which games');
  const kinds = hubEl('div');
  form.append(kinds);
  let pickedTargets = () => [];

  const rows = [];
  const choose = sample => {
    const wasDefault = !name.value.trim() || (picked && name.value === picked.name);
    const versionDefault = !version.value.trim() || version.value === ((picked && picked.version) || '1.0');
    picked = sample;
    rows.forEach(r => r.row.classList.toggle('checked', r.sample === sample));
    if (wasDefault) name.value = sample ? sample.name : '';
    if (versionDefault) version.value = (sample && sample.version) || '1.0';
    description.placeholder = sample && sample.description ? sample.description : 'What the mod does. Ends up in the README when you export.';
    kinds.textContent = '';
    if (sample) {
      kindHead.textContent = 'What it starts from';
      const games = (sample.games && sample.games.length ? sample.games : ['ff3']).map(g => g.toUpperCase()).join(' and ');
      kinds.append(hubEl('p', 'dialog-note', `A copy of the ${sample.name} sample${sample.version ? ' v' + sample.version : ''}: an OpenFF mod of ${games}`
        + (sample.code ? ', with its C# code under code/ - Build C# code, then Run in OpenFF.' : '; Run in OpenFF plays it.')
        + ' It is yours to change: an update of the sample never touches it (File ▸ Update from the sample brings its files up when you want).'));
      pickedTargets = () => [];
    } else {
      kindHead.textContent = 'What kind of mod, for which games';
      // An OpenFF mod of both games when the client's content is here, else a Steam mod of whatever is installed.
      const found = n => (projectState.targets.find(t => t.name === n) || {}).content;
      const preset = found('ours') ? ['ours', 'oursff4'].filter(found) : ['steam', 'ff4steam'].filter(found);
      pickedTargets = gameRows(kinds, preset);
    }
  };
  const start = (sample, heading, text) => {
    const row = hubEl('button', 'hub-start');
    row.append(hubPreview(sample, 'hub-thumb'));
    const words = hubEl('div');
    const h = hubEl('b', null, heading);
    if (sample && sample.version) h.append(hubEl('span', 'hub-version', 'v' + sample.version));
    if (sample && sample.code) h.append(hubEl('span', 'hub-tag csharp', 'C#'));
    words.append(h, hubEl('span', null, text));
    row.append(words);
    row.title = text;
    row.onclick = () => choose(sample);
    rows.push({ row, sample });
    startList.append(row);
  };
  start(null, 'Empty mod', 'Nothing in it yet: the games\' content to change, and your own code, scenes and items to add.');
  for (const sample of samples) start(sample, sample.name, (sample.tags || []).join(', ') || sample.description || '');
  choose(first);

  const problem = errorLine(body);
  const actions = hubEl('div', 'dialog-actions');
  const go = hubEl('button', 'primary', 'Create');
  go.onclick = async () => {
    if (!name.value.trim()) { problem.textContent = 'The mod needs a name.'; name.focus(); return; }
    const details = { author: author.value.trim(), version: version.value.trim(), description: description.value.trim() };
    go.disabled = true;
    try {
      let said;
      if (picked) {
        const r = await api('/api/samples/open', { id: picked.id, name: name.value.trim() });
        if (!r.ok) throw new Error(r.error);
        said = `${r.name} is yours now - a copy of the ${picked.name} sample${r.code ? '; Build C# code, then Run in OpenFF' : '; Run in OpenFF plays it'}`;
      } else {
        const targets = pickedTargets();
        if (!targets.length) { problem.textContent = 'Pick at least one game.'; go.disabled = false; return; }
        const r = await api('/api/project/create', { name: name.value.trim(), targets });
        if (!r.ok) throw new Error(r.error);
        said = `created ${r.name}`;
      }
      const save = Object.fromEntries(Object.entries(details).filter(([, v]) => v));
      if (Object.keys(save).length) await api('/api/project/save', save);
      body.close();
      hubSetTab('home', false);
      await reloadEverything(said);
    } catch (error) {
      problem.textContent = error.message;
      go.disabled = false;
    }
  };
  actions.append(go);
  body.append(actions);
  name.focus();
}

/// File ▸ Sample projects…: Create a mod with the first sample as its starting point.
async function sampleProjectsDialog() {
  const samples = ((await hubLoadSamples(false)).samples) || [];
  newProjectDialog({ from: samples.length ? samples[0].id : undefined });
}

// ------------------------------------------------------------------------- Mods

let hubMods = null;

let hubModsPoll = null;

// The list, and the game files checked against the releases: the editor checks in the background (a first check reads
// every file of a game, a minute or more), so the page asks again every so often while one is running.
async function hubLoadMods(force, fresh) {
  if (hubMods && !force) return hubMods;
  try { hubMods = await api('/api/mods?check=1' + (fresh ? '&fresh=1' : '')); } catch (error) { hubMods = { openff: [], games: [], error: error.message }; }
  hubPollMods();
  return hubMods;
}

function hubPollMods() {
  if (hubModsPoll || !hubMods || !(hubMods.games || []).some(g => g.checking)) return;
  hubModsPoll = setTimeout(async () => {
    hubModsPoll = null;
    try { hubMods = await api('/api/mods?check=1'); } catch (error) { return; }
    if (hubTab() === 'mods' && !startPageHidden()) drawStartPage();
    hubPollMods();
  }, 1500);
}

function hubModsPage(page) {
  if (!hubMods) {
    page.append(hubEl('p', 'dialog-note', 'Looking at the mods folder and the games…'));
    hubLoadMods(false).then(() => { if (hubTab() === 'mods') drawStartPage(); });
    return;
  }
  const result = hubMods;
  const bar = hubEl('div', 'hub-bar');
  bar.append(hubEl('p', 'dialog-note hub-grow', 'Every mod in place: OpenFF\'s mods folder, and what Crystal installed into each Steam or GOG copy of the games.'));
  const refresh = hubEl('button', null, 'Refresh');
  refresh.onclick = async () => { refresh.disabled = true; await hubLoadMods(true, true); drawStartPage(); };
  bar.append(refresh);
  page.append(bar);
  if (result.error) page.append(hubEl('p', 'dialog-error', result.error));

  // OpenFF
  const openff = hubEl('section', 'hub-place');
  const head = hubEl('div', 'hub-place-head');
  head.append(icon('mod'), hubEl('b', null, 'OpenFF'));
  if (result.mods) {
    const where = hubEl('span', 'hub-place-path', result.mods);
    where.title = result.mods;
    head.append(where, hubRevealButton(result.mods));
  }
  openff.append(head);
  if (!result.mods) openff.append(hubEl('p', 'dialog-note', 'No OpenFF client is known yet - start OpenFF once and its mods folder is found.'));
  else if (!result.openff.length) openff.append(hubEl('p', 'dialog-note', 'No mods in the folder. Install one from the Samples tab, or Export to OpenFF from a project.'));
  for (const mod of result.openff) openff.append(hubOpenFFRow(mod));
  if (result.openff.length) openff.append(hubEl('p', 'dialog-note hub-place-foot', 'OpenFF takes changes in at its next start; its MODS list on the title does the same.'));
  page.append(openff);

  // Steam and GOG
  if (!result.games.length) {
    const none = hubEl('section', 'hub-place');
    none.append(hubEl('p', 'dialog-note', 'No Steam or GOG copy of Final Fantasy III or IV was found on this machine.'));
    page.append(none);
  }
  for (const game of result.games) page.append(hubGameSection(game));
}

function hubRevealButton(path) {
  const b = hubEl('button', 'hub-small', 'Open folder');
  b.onclick = async () => {
    try { const r = await api('/api/mods/reveal', { path }); if (!r.ok) throw new Error(r.error); } catch (error) { say(error.message, 'bad'); }
  };
  return b;
}

function hubBadge(text, tone, title) {
  const b = hubEl('span', 'hub-state ' + (tone || 'dim'), text);
  if (title) b.title = title;
  return b;
}

function hubOpenFFRow(mod) {
  const row = hubEl('div', 'hub-mod' + (mod.enabled ? '' : ' off'));
  const toggle = hubEl('input');
  toggle.type = 'checkbox';
  toggle.checked = mod.enabled;
  toggle.title = mod.enabled ? 'On - untick to leave it out of OpenFF' : 'Off - tick to have OpenFF load it';
  toggle.onchange = async () => {
    try {
      const r = await api('/api/mods/enable', { key: mod.key, enabled: toggle.checked });
      if (!r.ok) throw new Error(r.error);
      mod.enabled = toggle.checked;
      row.classList.toggle('off', !mod.enabled);
      say(`${mod.name} is ${mod.enabled ? 'on' : 'off'} - OpenFF takes it in at its next start`, 'good');
    } catch (error) { toggle.checked = mod.enabled; say(error.message, 'bad'); }
  };
  const words = hubEl('div', 'hub-mod-words');
  const name = hubEl('b', null, mod.name);
  if (mod.version) name.append(hubEl('span', 'hub-version', 'v' + mod.version));
  if (mod.kind === 'sample') {
    name.append(hubBadge('Sample', 'good', `The ${mod.sample} sample, installed${mod.managed ? '' : ' by hand'} - kept up to date`));
    if (mod.state === 'update') name.append(hubBadge('Update', 'mark', 'The shipped sample is newer'));
    if (mod.state === 'copy') name.append(hubBadge('Old copy', 'mark', 'A copy made by hand of an older sample'));
  } else if (mod.kind === 'project') {
    name.append(hubBadge('Project', 'dim', `Exported from the project ${mod.project || ''} - yours, never updated over`));
  }
  if (mod.code) name.append(hubEl('span', 'hub-tag csharp', 'C#'));
  const sub = hubEl('span', null, mod.description || mod.directory);
  sub.title = mod.directory;
  words.append(name, sub);
  const actions = hubEl('div', 'hub-mod-actions');
  if (mod.kind === 'sample' && (mod.state === 'update' || mod.state === 'copy')) {
    const up = hubEl('button', 'primary hub-small', 'Update');
    up.onclick = async () => {
      await hubLoadSamples(true);
      const sample = ((hubSamples && hubSamples.samples) || []).find(s => s.id === mod.sample);
      if (!sample) { say(`The ${mod.sample} sample is not here to update from`, 'bad'); return; }
      await hubInstall(sample, up);
      await hubLoadMods(true);
      drawStartPage();
    };
    actions.append(up);
  }
  if (mod.projectDirectory) {
    const open = hubEl('button', 'hub-small', 'Open project');
    open.onclick = () => openProjectAt(mod.projectDirectory);
    actions.append(open);
  }
  const remove = hubEl('button', 'hub-small', 'Remove');
  remove.title = mod.managed ? 'Uninstall the sample (it can be installed again from Samples)' : 'Take the mod out of the mods folder, to the Recycle Bin';
  remove.onclick = () => hubConfirm(`Remove ${mod.name}?`,
    mod.managed ? `${mod.name} is uninstalled from OpenFF's mods folder. The sample stays in Crystal's Samples to install again.`
      : `The folder ${mod.directory} goes to the Recycle Bin - restore it from there if you want it back.` + (mod.kind === 'project' ? ' The project itself is not touched; Export to OpenFF puts the mod back.' : ''),
    'Remove', async () => {
      const r = await api('/api/mods/remove', { key: mod.key });
      if (!r.ok) throw new Error(r.error);
      say(r.where === 'uninstalled' ? `${mod.name} is uninstalled` : r.where === 'recycle bin' ? `${mod.name} is in the Recycle Bin` : `${mod.name} is moved to ${r.where}`, 'good');
      await hubLoadMods(true);
      hubSamples = null;
      drawStartPage();
    });
  actions.append(remove);
  row.append(toggle, words, actions);
  return row;
}

function hubGameSection(game) {
  const section = hubEl('section', 'hub-place');
  const head = hubEl('div', 'hub-place-head');
  head.append(icon('steam'), hubEl('b', null, game.name), hubBadge(game.store, 'dim'));
  const where = hubEl('span', 'hub-place-path', game.path);
  where.title = game.path;
  head.append(where, hubRevealButton(game.path));
  section.append(head);
  if (!game.mods.length) section.append(hubEl('p', 'dialog-note', 'No project of Crystal\'s is installed in it.'));
  for (const mod of game.mods) {
    const row = hubEl('div', 'hub-mod');
    row.append(hubEl('span', 'hub-mod-mark'));
    const words = hubEl('div', 'hub-mod-words');
    const label = mod.name || 'Edits without a project';
    const name = hubEl('b', null, label);
    if (mod.orphan) name.append(hubBadge(mod.name ? 'Project gone' : 'Edits gone', 'mark', "Known from the game folder's crystal-installs.json: the project was deleted while installed"
      + (mod.restorable ? ' - its backups are still there, so Uninstall puts the originals back' : " - its backups went with it; Steam's Verify integrity of game files puts the originals back")));
    if (mod.changed) name.append(hubBadge(`${mod.changed} changed since`, 'mark', 'Files the game has now that are not what Crystal wrote - a game update, Verify integrity, another tool. Uninstall forgets those that are the original again and leaves the rest as they are'));
    const sub = hubEl('span', null, `${mod.files} file${mod.files === 1 ? '' : 's'} installed · the originals kept beside the edits`);
    sub.title = mod.edits;
    words.append(name, sub);
    const actions = hubEl('div', 'hub-mod-actions');
    if (mod.projectDirectory) {
      const open = hubEl('button', 'hub-small', 'Open project');
      open.onclick = () => openProjectAt(mod.projectDirectory);
      actions.append(open);
    }
    if (mod.orphan && !mod.restorable) {
      const forget = hubEl('button', 'hub-small', 'Forget');
      forget.title = "Take it off the game folder's list - verify the game in Steam to put its files back";
      forget.onclick = () => hubConfirm(`Forget ${label}?`,
        `Its backups of the game's files are gone, so Crystal cannot put them back. Steam ▸ Library ▸ ${game.name} ▸ Properties ▸ Installed Files ▸ Verify integrity of game files does. Forget takes it off the list in the game folder.`,
        'Forget', async () => {
          const r = await api('/api/mods/forget', { content: game.path, edits: mod.edits });
          if (!r.ok) throw new Error(r.error);
          await hubLoadMods(true);
          drawStartPage();
        });
      actions.append(forget);
      row.append(words, actions);
      section.append(row);
      continue;
    }
    const out = hubEl('button', 'hub-small', 'Uninstall');
    out.title = 'Put the game\'s own files back (Project ▸ Remove); the edits stay where they are';
    out.onclick = () => hubConfirm(`Uninstall ${label} from ${game.name}?`,
      `The game's own files go back where ${label} replaced them, and the files it added are taken out. The edits stay in ${mod.name ? 'the project' : 'Crystal'} to install again.`,
      'Uninstall', async () => {
        const r = await api('/api/mods/uninstall', { content: game.path, edits: mod.edits });
        if (!r.ok) throw new Error(r.error || 'not uninstalled');
        say(`${label}: ${r.restored} original(s) put back, ${r.removed} added file(s) taken out` + (r.skipped && r.skipped.length ? `, ${r.skipped.length} left as they are (changed since)` : ''), 'good');
        await hubLoadMods(true);
        if (typeof refreshProject === 'function') refreshProject();
        drawStartPage();
      });
    actions.append(out);
    row.append(words, actions);
    section.append(row);
  }
  if (game.other && game.other.length) {
    const warn = hubEl('details', 'hub-other');
    warn.append(hubEl('summary', null, `${game.other.length} other file${game.other.length === 1 ? '' : 's'} differ from the ${game.store} release`
      + (game.otherVersion ? ' - likely another version of the game' : ' - not installed by Crystal (another mod tool, or a damaged download)')));
    warn.append(hubEl('p', 'dialog-note', 'Steam ▸ Library ▸ the game ▸ Properties ▸ Installed Files ▸ Verify integrity of game files puts them back. Uninstall Crystal\'s own installs above first.'));
    warn.append(fileList(game.other.slice(0, 200)));
    section.append(warn);
  } else if (game.other) {
    section.append(hubEl('p', 'dialog-note hub-place-foot', game.mods.length ? 'Every other file is as the release ships it.' : `Every file is as the ${game.store} release ships it.`));
  }
  if (game.missing) section.append(hubEl('p', 'dialog-note hub-place-foot', `${game.missing} file(s) of the release are missing.`));
  if (game.checking) section.append(hubEl('p', 'dialog-note hub-place-foot', `Checking the game's files against the release… ${Math.round((game.progress || 0) * 100)}%`));
  return section;
}

function hubConfirm(title, text, verb, run) {
  const body = dialog(title);
  body.append(hubEl('p', 'dialog-note', text));
  const problem = errorLine(body);
  const actions = hubEl('div', 'dialog-actions');
  const cancel = hubEl('button', null, 'Keep it');
  cancel.onclick = () => body.close();
  const go = hubEl('button', 'danger', verb);
  go.onclick = async () => {
    go.disabled = true;
    try { await run(); body.close(); } catch (error) { problem.textContent = error.message; go.disabled = false; }
  };
  actions.append(cancel, go);
  body.append(actions);
  cancel.focus();
}
