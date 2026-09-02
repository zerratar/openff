// The workbench: panels, document tabs, hierarchy, inspector, console and project.
//
// app.js owns the ten views - a script, a map, a table - and knows nothing about where
// they are put. This file owns the arrangement: which documents are open, which one is
// showing, what is in it, and what is selected. That split is why adding the whole
// layout did not mean rewriting any of the views.
//
// A document is one open asset. Its pane is built once and then kept, hidden rather
// than thrown away when you switch tabs, so a half-typed script is still there when you
// come back to it.

'use strict';

// The libraries, in the order they appear in the project tree. This list is also what
// the address bar is built from - the slug is the label lowercased - so the two cannot
// drift apart the way a second hand-written list would.
const KINDS = [
  { id: 'map', label: 'Maps' },
  { id: 'script', label: 'Scripts' },
  { id: 'menu', label: 'Menus' },
  { id: 'text', label: 'Text' },
  { id: 'table', label: 'Tables' },
  { id: 'image', label: 'Images' },
  { id: 'texture', label: 'Textures' },
  { id: 'model', label: 'Models' },
  { id: 'cell', label: 'Cells' },
  { id: 'audio', label: 'Audio' }
];

const docs = new Map();
let activeDoc = null;

// ---------------------------------------------------------------- documents

function docId(kind, name) {
  return `${kind}|${name}`;
}

function shortName(name) {
  const cut = name.lastIndexOf('/');
  return cut < 0 ? name : name.slice(cut + 1);
}

/// Opens an asset, or brings it forward if it is already open.
async function openDoc(kind, name, reload = false) {
  const id = docId(kind, name);
  const existing = docs.get(id);

  if (existing && !reload) {
    activate(id);
    return existing;
  }

  const doc = existing || {
    id, kind, name, selection: null, data: null,
    pane: Object.assign(document.createElement('div'), { className: 'doc-pane' })
  };

  if (!existing) {
    $('#docs').append(doc.pane);
    docs.set(id, doc);
  }

  doc.pane.textContent = '';
  doc.selection = null;
  drawDocTabs();
  activate(id);

  say('loading…');
  try {
    await runView(doc);
    say('');
  } catch (error) {
    doc.pane.innerHTML = '';
    const problem = document.createElement('p');
    problem.className = 'empty';
    problem.textContent = error.message;
    doc.pane.append(problem);
    say(error.message, 'bad');
  }

  drawHierarchy();
  drawInspector();
  return doc;
}

/// Runs the view for a document into its own pane.
async function runView(doc) {
  state.kind = doc.kind;
  state.name = doc.name;
  state.pane = doc.pane;

  // The views read their listing from state.files, so make sure it is the right one.
  if (state.browse !== doc.kind || !state.files.length) {
    state.browse = doc.kind;
    await loadList();
  }

  await dispatchOpen(doc.kind, doc.name);
}

function activate(id) {
  const doc = docs.get(id);
  if (!doc) return;

  activeDoc = doc;
  state.kind = doc.kind;
  state.name = doc.name;
  state.pane = doc.pane;

  for (const other of docs.values()) {
    other.pane.classList.toggle('on', other === doc);
  }
  $('#no-docs').hidden = docs.size > 0;
  drawDocTabs();
  drawHierarchy();
  drawInspector();
  syncHash();
  // A canvas that was hidden when it was sized has no size at all.
  if (doc.onShow) doc.onShow();
}

function closeDoc(id) {
  const doc = docs.get(id);
  if (!doc) return;

  doc.pane.remove();
  docs.delete(id);

  if (activeDoc === doc) {
    activeDoc = null;
    const next = [...docs.values()].pop();
    if (next) {
      activate(next.id);
      return;
    }
    state.name = null;
    state.pane = null;
    $('#no-docs').hidden = false;
    drawDocTabs();
    drawHierarchy();
    drawInspector();
    syncHash();
  } else {
    drawDocTabs();
  }
}

function drawDocTabs() {
  const bar = $('#doc-tabs');
  bar.textContent = '';

  for (const doc of docs.values()) {
    const tab = document.createElement('div');
    tab.className = 'doc-tab' + (doc === activeDoc ? ' on' : '');
    tab.title = doc.name;

    const label = document.createElement('span');
    label.textContent = shortName(doc.name);
    label.onclick = () => activate(doc.id);

    const shut = document.createElement('button');
    shut.className = 'shut';
    shut.textContent = '×';
    shut.title = 'close';
    shut.onclick = (event) => { event.stopPropagation(); closeDoc(doc.id); };

    const kind = document.createElement('i');
    kind.textContent = doc.kind;

    tab.append(kind, label, shut);
    bar.append(tab);
  }
}

// ---------------------------------------------------------------- hierarchy

/// What is inside the open document. Selecting a row fills the inspector.
function drawHierarchy() {
  const tree = $('#hierarchy');
  const note = $('#hierarchy-note');
  tree.textContent = '';
  note.textContent = '';

  if (!activeDoc) {
    tree.innerHTML = '<li class="empty-row">Nothing open.</li>';
    return;
  }

  note.textContent = shortName(activeDoc.name);
  const filter = $('#hierarchy-filter').value.trim().toLowerCase();
  const nodes = outlineFor(activeDoc);

  if (!nodes.length) {
    tree.innerHTML = '<li class="empty-row">This one has no parts to list.</li>';
    return;
  }

  for (const group of nodes) {
    const head = document.createElement('li');
    head.className = 'group';
    head.textContent = group.label;
    if (group.children.length) {
      const count = document.createElement('b');
      count.textContent = group.children.length;
      head.append(count);
    }
    tree.append(head);

    let shown = 0;
    for (const child of group.children) {
      if (filter && !child.label.toLowerCase().includes(filter)) continue;
      shown++;
      const row = document.createElement('li');
      row.className = 'row' + (activeDoc.selection === child.ref ? ' on' : '');
      row.textContent = child.label;
      if (child.note) {
        const tag = document.createElement('i');
        tag.textContent = child.note;
        row.append(tag);
      }
      row.onclick = () => {
        activeDoc.selection = child.ref;
        if (child.reveal) child.reveal();
        drawHierarchy();
        drawInspector();
      };
      tree.append(row);
    }

    if (filter && !shown) head.remove();
  }
}

/// The outline of a document, which is whatever that kind of asset is made of.
function outlineFor(doc) {
  const data = doc.data;
  if (!data) return [];

  if (doc.kind === 'map' && data.scene) {
    const scene = data.scene;
    const groups = [];
    if (scene.terrain) {
      groups.push({
        label: 'Terrain',
        children: [{ label: shortName(scene.terrain), ref: 'terrain', note: 'model' }]
      });
    }
    groups.push({
      label: 'Characters',
      children: scene.objects.map(o => ({
        label: o.name,
        note: o.hasScript ? `${o.instructions}` : 'no script',
        ref: `object:${o.index}`,
        reveal: () => revealObject(doc, o)
      }))
    });
    groups.push({
      label: 'Logic',
      children: scene.logic.map(o => ({
        label: o.name, ref: `object:${o.index}`, note: `cast ${o.cast}`
      }))
    });
    groups.push({
      label: 'Exits',
      children: scene.exits.map((e, i) => ({
        label: `${e.to || '?'} at ${e.x},${e.z}`, ref: `exit:${i}`
      }))
    });
    return groups;
  }

  if (doc.kind === 'model' && data.groups) {
    return [{
      label: 'Parts',
      children: data.groups.map((g, i) => ({
        label: g.shape || `part ${i}`,
        note: g.texture || '',
        ref: `part:${i}`
      }))
    }];
  }

  if (doc.kind === 'cell' && data.cells) {
    return [{
      label: 'Cells',
      children: data.cells.map(c => ({
        label: `cell ${c.index}`, note: `${c.parts.length}`, ref: `cell:${c.index}`
      }))
    }];
  }

  if (doc.kind === 'script' && data.source) {
    // The casts and functions a script declares, read off the source it is showing
    // rather than asked for again - the two would otherwise disagree while editing.
    const children = [];
    const pattern = /^\s*(cast|func|function|extern)\s+([^\s({=]+)/gm;
    let match;
    while ((match = pattern.exec(data.source))) {
      const line = data.source.slice(0, match.index).split('\n').length;
      children.push({
        label: `${match[1]} ${match[2]}`,
        note: `line ${line}`,
        ref: `line:${line}`,
        reveal: () => revealLine(doc, line)
      });
    }
    return children.length ? [{ label: 'Declarations', children }] : [];
  }

  return [];
}

function revealLine(doc, line) {
  const text = $('textarea', doc.pane);
  if (!text) return;
  const at = text.value.split('\n').slice(0, line - 1).join('\n').length;
  text.focus();
  text.setSelectionRange(at, at);
  text.scrollTop = Math.max(0, (line - 4) * 19);
}

function revealObject(doc, object) {
  if (doc.scene3d && doc.mode === '3d') {
    doc.scene3d.focus(object);
    return;
  }
  const pin = $(`.pin[data-index="${object.index}"]`, doc.pane);
  if (pin) pin.click();
}

// ---------------------------------------------------------------- inspector

function inspectorBody() {
  return $('#inspector');
}

/// Facts about the open asset, and about whatever is selected inside it.
function drawInspector() {
  const box = inspectorBody();
  box.textContent = '';

  if (!activeDoc) {
    box.innerHTML = '<p class="empty-row">Nothing open.</p>';
    return;
  }

  const doc = activeDoc;
  const heading = document.createElement('h2');
  heading.textContent = shortName(doc.name);
  const kind = document.createElement('p');
  kind.className = 'sub';
  kind.textContent = doc.name;
  box.append(heading, kind);

  const facts = factsFor(doc);
  if (facts.length) {
    box.append(factList(facts));
  }

  if (doc.selection && doc.inspect) {
    const detail = doc.inspect(doc.selection);
    if (detail) {
      const rule = document.createElement('hr');
      box.append(rule, detail);
    }
  }
}

function factList(pairs) {
  const list = document.createElement('dl');
  for (const [label, value] of pairs) {
    if (value === null || value === undefined || value === '') continue;
    const dt = document.createElement('dt');
    dt.textContent = label;
    const dd = document.createElement('dd');
    dd.textContent = value;
    list.append(dt, dd);
  }
  return list;
}

function factsFor(doc) {
  const data = doc.data;
  const facts = [['kind', doc.kind]];

  if (!data) return facts;

  if (doc.kind === 'map' && data.scene) {
    facts.push(['terrain', data.scene.terrain ? shortName(data.scene.terrain) : 'none']);
    facts.push(['characters', data.scene.objects.length]);
    facts.push(['logic casts', data.scene.logic.length]);
    facts.push(['exits', data.scene.exits.length]);
    facts.push(['script', shortName(data.script || '')]);
  } else if (doc.kind === 'model' && data.groups) {
    facts.push(['parts', data.groups.length]);
    facts.push(['vertices', data.vertices]);
    facts.push(['triangles', data.triangles]);
    facts.push(['quads', data.quads]);
    facts.push(['nodes', (data.nodes || []).length]);
  } else if (doc.kind === 'cell' && data.cells) {
    facts.push(['cells', data.cells.length]);
    facts.push(['parts', data.cells.reduce((n, c) => n + c.parts.length, 0)]);
    facts.push(['sheet', data.sheet ? shortName(data.sheet) : 'none']);
  } else if (doc.kind === 'script' && data.source) {
    facts.push(['lines', data.source.split('\n').length]);
  } else if (doc.kind === 'image' && data.width) {
    facts.push(['size', `${data.width} × ${data.height}`]);
    facts.push(['colour', `${data.colour}, ${data.depth}-bit`]);
    facts.push(['bytes', data.bytes]);
  }

  return facts;
}

/// Used by the views to hand the shell what they loaded.
function setDocData(data, extra) {
  if (!activeDoc) return;
  activeDoc.data = data;
  if (extra) Object.assign(activeDoc, extra);
}

// ------------------------------------------------------------------ console

let consoleCount = 0;

function logLine(text, tone) {
  if (!text) return;
  const line = document.createElement('div');
  line.className = 'console-line' + (tone ? ' ' + tone : '');
  const when = document.createElement('i');
  when.textContent = new Date().toLocaleTimeString();
  const body = document.createElement('span');
  body.textContent = text;
  line.append(when, body);

  const lines = $('#console-lines');
  lines.append(line);
  lines.scrollTop = lines.scrollHeight;

  if (tone === 'bad') {
    consoleCount++;
    $('#console-count').textContent = consoleCount;
    $('#console-count').className = 'on';
  }
}

// say() is the status line; everything it reports is worth keeping a record of, so it
// goes to the console too rather than being overwritten by the next message.
const baseSay = say;
say = function (message, tone) {
  baseSay(message, tone);
  if (message && message !== 'loading…') logLine(message, tone);
};

window.addEventListener('error', event => logLine(event.message, 'bad'));
window.addEventListener('unhandledrejection', event =>
  logLine(String(event.reason && event.reason.message || event.reason), 'bad'));

// ------------------------------------------------------------------ project

let browseKind = 'map';

function drawProjectTree() {
  const tree = $('#project-tree');
  tree.textContent = '';

  for (const kind of KINDS) {
    const row = document.createElement('div');
    row.className = 'row' + (kind.id === browseKind ? ' on' : '');
    row.textContent = kind.label;
    row.onclick = () => selectKind(kind.id);
    tree.append(row);
  }
}

async function selectKind(kind) {
  browseKind = kind;
  state.browse = kind;
  drawProjectTree();
  $('#filter').value = '';
  say('loading…');
  try {
    await loadList();
    say('');
  } catch (error) {
    say(error.message, 'bad');
  }
}

// ---------------------------------------------------------------- splitters

const SIZES = 'ff3-editor-panels';

function loadSizes() {
  let saved = {};
  try {
    saved = JSON.parse(localStorage.getItem(SIZES) || '{}');
  } catch (error) {
    saved = {};
  }
  const root = document.documentElement.style;
  if (saved.left) root.setProperty('--left', saved.left + 'px');
  if (saved.right) root.setProperty('--right', saved.right + 'px');
  if (saved.bottom) root.setProperty('--bottom', saved.bottom + 'px');
}

function saveSize(which, px) {
  let saved = {};
  try {
    saved = JSON.parse(localStorage.getItem(SIZES) || '{}');
  } catch (error) {
    saved = {};
  }
  saved[which] = px;
  try {
    localStorage.setItem(SIZES, JSON.stringify(saved));
  } catch (error) {
    // A browser refusing to store panel widths is not worth interrupting anyone over.
  }
}

function wireSplitters() {
  for (const bar of $$('.splitter')) {
    const which = bar.dataset.split;
    bar.onpointerdown = (event) => {
      event.preventDefault();
      bar.setPointerCapture(event.pointerId);
      const move = (e) => {
        const box = $('#workbench').getBoundingClientRect();
        let px;
        if (which === 'left') px = e.clientX - box.left;
        else if (which === 'right') px = box.right - e.clientX;
        else px = box.bottom - e.clientY;
        px = Math.max(120, Math.min(which === 'bottom' ? 600 : 640, px));
        document.documentElement.style.setProperty('--' + which, px + 'px');
        saveSize(which, px);
        if (activeDoc && activeDoc.onShow) activeDoc.onShow();
      };
      const up = () => {
        bar.removeEventListener('pointermove', move);
        bar.removeEventListener('pointerup', up);
      };
      bar.addEventListener('pointermove', move);
      bar.addEventListener('pointerup', up);
    };
  }
}

// ------------------------------------------------------------------ routing

function slugFor(kind) {
  const found = KINDS.find(k => k.id === kind);
  return found ? found.label.toLowerCase() : kind;
}

function kindFor(slug) {
  const found = KINDS.find(k => k.label.toLowerCase() === slug);
  return found ? found.id : null;
}

function encodeName(name) {
  return encodeURIComponent(name).replace(/%2F/g, '/');
}

function hashFor(kind, name) {
  return `#/${slugFor(kind)}` + (name ? `/${encodeName(name)}` : '');
}

function readHash() {
  const raw = location.hash.replace(/^#\/?/, '');
  if (!raw) return { kind: null, name: null };
  const cut = raw.indexOf('/');
  const slug = cut < 0 ? raw : raw.slice(0, cut);
  const name = cut < 0 ? null : decodeURIComponent(raw.slice(cut + 1));
  return { kind: kindFor(slug), name: name || null };
}

function syncHash() {
  const wanted = activeDoc
    ? hashFor(activeDoc.kind, activeDoc.name)
    : hashFor(browseKind);
  if (location.hash !== wanted) history.replaceState(null, '', wanted);
}

async function applyHash() {
  const { kind, name } = readHash();
  const wanted = kind || 'map';

  if (wanted !== browseKind || !state.files.length) {
    await selectKind(wanted);
  }
  if (name) {
    await openDoc(wanted, name);
  }
}

// ------------------------------------------------------------------ start up

$$('#bottom-tabs button').forEach(button => {
  button.onclick = () => {
    $$('#bottom-tabs button').forEach(b => b.classList.toggle('on', b === button));
    $$('.bottom-page').forEach(p =>
      p.classList.toggle('on', p.dataset.page === button.dataset.bottom));
    if (button.dataset.bottom === 'console') {
      consoleCount = 0;
      $('#console-count').textContent = '';
      $('#console-count').className = '';
    }
  };
});

$('#hierarchy-filter').addEventListener('input', drawHierarchy);
$('#filter').addEventListener('input', drawList);

window.addEventListener('hashchange', () => {
  const { kind, name } = readHash();
  if (activeDoc && (kind || 'map') === activeDoc.kind && name === activeDoc.name) return;
  if (!activeDoc && !name && (kind || 'map') === browseKind) return;
  applyHash().catch(error => say(error.message, 'bad'));
});

loadSizes();
wireSplitters();
drawProjectTree();
drawHierarchy();
drawInspector();

api('/api/status')
  .then(status => say(`${status.files} files  ·  overrides in ${status.overrides}`))
  .then(applyHash)
  .catch(error => say(error.message, 'bad'));
