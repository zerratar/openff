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

// ---------------------------------------------------------------- documents
//
// Documents live in groups, and a group is one tab bar with one stack of panes under
// it. There is one group until a tab is dragged out to the side, and then there are
// two, which is what lets a script and its map be read together. Only one document is
// focused however many are showing - the hierarchy and the inspector follow that one.
//
// A single click opens a PREVIEW tab: italic, one per group, and replaced by the next
// thing you click. It becomes a real tab the moment you change anything in it, double
// click it, or double click it in the project list. Skimming twenty files leaves one
// tab behind rather than twenty.

const docs = new Map();
let groups = [];
let activeDoc = null;
let activeGroup = null;

function docId(kind, name) {
  return `${kind}|${name}`;
}

function shortName(name) {
  const cut = name.lastIndexOf('/');
  return cut < 0 ? name : name.slice(cut + 1);
}

function makeGroup() {
  const el = document.createElement('div');
  el.className = 'doc-group';

  const tabsEl = document.createElement('div');
  tabsEl.className = 'doc-tabs';

  const panesEl = document.createElement('div');
  panesEl.className = 'doc-panes';

  el.append(tabsEl, panesEl);

  // A second group arrives with a bar in front of it, so the split can be moved.
  if (groups.length) {
    const bar = document.createElement('div');
    bar.className = 'splitter vertical group-split';
    bar.onpointerdown = (event) => {
      event.preventDefault();
      bar.setPointerCapture(event.pointerId);
      const move = (e) => {
        const area = $('#doc-area').getBoundingClientRect();
        const first = groups[0].el;
        const width = Math.max(160, Math.min(area.width - 160, e.clientX - area.left));
        first.style.flex = `0 0 ${width}px`;
        layoutChanged();
      };
      const up = () => {
        bar.removeEventListener('pointermove', move);
        bar.removeEventListener('pointerup', up);
      };
      bar.addEventListener('pointermove', move);
      bar.addEventListener('pointerup', up);
    };
    $('#doc-area').append(bar);
  }

  $('#doc-area').append(el);

  const group = { el, tabsEl, panesEl, docs: [], preview: null, active: null };
  groups.push(group);

  // Dropping onto a tab bar moves the document into that group.
  tabsEl.addEventListener('dragover', event => {
    event.preventDefault();
    tabsEl.classList.add('drop');
  });
  tabsEl.addEventListener('dragleave', () => tabsEl.classList.remove('drop'));
  tabsEl.addEventListener('drop', event => {
    event.preventDefault();
    tabsEl.classList.remove('drop');
    const doc = docs.get(event.dataTransfer.getData('text/plain'));
    if (doc) moveDoc(doc, group);
  });

  // Clicking anywhere in a group focuses whatever that group is showing.
  el.addEventListener('pointerdown', () => {
    if (group.active && group.active !== activeDoc) activate(group.active.id);
  }, true);

  return group;
}

function removeGroup(group) {
  if (groups.length < 2) return;
  group.el.remove();
  groups = groups.filter(g => g !== group);
  $$('.group-split').forEach(bar => bar.remove());
  // Whatever is left takes the whole width back.
  groups.forEach(g => { g.el.style.flex = ''; });
  if (activeGroup === group) activeGroup = groups[0];
  layoutChanged();
}

function moveDoc(doc, group) {
  if (doc.group === group) return;
  const from = doc.group;
  from.docs = from.docs.filter(d => d !== doc);
  if (from.preview === doc) from.preview = null;
  if (from.active === doc) from.active = from.docs[from.docs.length - 1] || null;

  doc.group = group;
  group.docs.push(doc);
  group.panesEl.append(doc.pane);
  if (doc.preview) {
    // One preview per group, and it did not follow the document here.
    if (group.preview && group.preview !== doc) closeDoc(group.preview.id);
    group.preview = doc;
  }

  if (!from.docs.length) removeGroup(from);
  activate(doc.id);
  layoutChanged();
}

/// Opens an asset. Single clicks ask for a preview; anything else pins it.
async function openDoc(kind, name, options = {}) {
  const settings = options === true ? { reload: true } : options;
  const id = docId(kind, name);
  const existing = docs.get(id);

  // An empty pane means the last run left it broken, so rebuild rather than show it.
  if (existing && !settings.reload && existing.pane.childElementCount > 0) {
    if (!settings.preview) pinDoc(existing);
    activate(id);
    return existing;
  }

  if (!groups.length) makeGroup();
  const group = existing ? existing.group : (activeGroup || groups[0]);

  // A preview replaces the group's previous preview rather than piling up.
  if (!existing && settings.preview && group.preview) {
    closeDoc(group.preview.id);
  }

  const doc = existing || {
    id, kind, name, selection: null, data: null, group,
    preview: Boolean(settings.preview),
    pane: Object.assign(document.createElement('div'), { className: 'doc-pane' })
  };

  if (!existing) {
    group.panesEl.append(doc.pane);
    group.docs.push(doc);
    docs.set(id, doc);
    if (doc.preview) group.preview = doc;
    watchForEdits(doc);
  }

  doc.pane.textContent = '';
  doc.selection = null;

  // Anything a view hung on its document belongs to the DOM that has just been thrown
  // away. Leaving them behind is what made a reverted map come back blank: the scene
  // viewer was still pointing at a canvas that had been removed, and the rebuild saw
  // one already there and skipped making a new one.
  doc.scene3d = null;
  doc.onShow = null;
  doc.inspect = null;
  doc.data = null;
  doc.mode = null;

  activate(id);

  say('loading…');
  try {
    await runView(doc);
    say('');
  } catch (error) {
    doc.pane.textContent = '';
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

/// Changing anything in a document is what turns a preview into a real tab.
function watchForEdits(doc) {
  const pin = () => pinDoc(doc);
  doc.pane.addEventListener('input', pin, true);
  doc.pane.addEventListener('change', pin, true);
  doc.pane.addEventListener('pointerdown', event => {
    if (event.target.closest('button, .pin, canvas')) pin();
  }, true);
}

function pinDoc(doc) {
  if (!doc || !doc.preview) return;
  doc.preview = false;
  if (doc.group.preview === doc) doc.group.preview = null;
  drawDocTabs();
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

  // Going to a document means the panel is about that document again.
  inspected = null;
  inspectedViewer = null;

  activeDoc = doc;
  activeGroup = doc.group;
  doc.group.active = doc;
  state.kind = doc.kind;
  state.name = doc.name;
  state.pane = doc.pane;

  for (const other of docs.values()) {
    other.pane.classList.toggle('on', other === other.group.active);
  }
  for (const group of groups) {
    group.el.classList.toggle('focused', group === activeGroup);
  }

  $('#no-docs').hidden = docs.size > 0;
  drawDocTabs();
  drawHierarchy();
  drawInspector();
  syncHash();
  if (doc.onShow) doc.onShow();
}

function closeDoc(id) {
  const doc = docs.get(id);
  if (!doc) return;

  const group = doc.group;
  doc.pane.remove();
  docs.delete(id);
  group.docs = group.docs.filter(d => d !== doc);
  if (group.preview === doc) group.preview = null;

  if (group.active === doc) {
    group.active = group.docs[group.docs.length - 1] || null;
  }

  if (!group.docs.length && groups.length > 1) {
    removeGroup(group);
  }

  if (activeDoc === doc) {
    activeDoc = null;
    const next = (activeGroup && activeGroup.active)
      || groups.map(g => g.active).find(Boolean);
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
  for (const group of groups) {
    group.tabsEl.textContent = '';
    for (const doc of group.docs) {
      group.tabsEl.append(makeTab(doc, group));
    }
  }
  // The file list shows which files are open, so it changes whenever the tabs do.
  markList();
}

function makeTab(doc, group) {
  const tab = document.createElement('div');
  tab.className = 'doc-tab'
    + (doc === group.active ? ' on' : '')
    + (doc === activeDoc ? ' focused' : '')
    + (doc.preview ? ' preview' : '');
  tab.title = doc.name + (doc.preview ? '  (preview - double click to keep)' : '');
  tab.draggable = true;

  const kind = icon(doc.kind);
  const label = document.createElement('span');
  label.textContent = shortName(doc.name);

  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  shut.title = 'close';
  shut.onpointerdown = (event) => event.stopPropagation();
  shut.onclick = (event) => {
    event.stopPropagation();
    closeDoc(doc.id);
  };

  tab.append(kind, label, shut);

  // The whole tab reacts, not just its name.
  tab.onclick = () => activate(doc.id);

  // Middle click closes it. The pointerdown has to be swallowed as well, or the
  // browser starts its own autoscroll on the tab strip instead.
  tab.onpointerdown = (event) => {
    if (event.button === 1) event.preventDefault();
  };
  tab.onauxclick = (event) => {
    if (event.button !== 1) return;
    event.preventDefault();
    closeDoc(doc.id);
  };
  tab.ondblclick = () => pinDoc(doc);
  tab.ondragstart = (event) => {
    event.dataTransfer.setData('text/plain', doc.id);
    event.dataTransfer.effectAllowed = 'move';
    document.body.classList.add('dragging-tab');
  };
  tab.ondragend = () => document.body.classList.remove('dragging-tab');
  return tab;
}

/// Panes that hold a canvas have to be told when their size changed.
function layoutChanged() {
  for (const doc of docs.values()) {
    if (doc.onShow && doc === doc.group.active) doc.onShow();
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
      row.append(icon(child.icon || 'file'));
      const text = document.createElement('span');
      text.textContent = child.label;
      row.append(text);
      if (child.note) {
        const tag = document.createElement('i');
        tag.textContent = child.note;
        row.append(tag);
      }
      row.onclick = () => {
        clearInspected();
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
        children: [{
          label: shortName(scene.terrain), ref: 'terrain', note: 'model', icon: 'terrain'
        }]
      });
    }
    groups.push({
      label: 'Characters',
      children: scene.objects.map(o => ({
        label: o.name,
        note: o.hasScript ? `${o.instructions}` : 'no script',
        ref: `object:${o.index}`,
        icon: 'character',
        reveal: () => revealObject(doc, o)
      }))
    });
    groups.push({
      label: 'Logic',
      children: scene.logic.map(o => ({
        label: o.name, ref: `object:${o.index}`, note: `cast ${o.cast}`, icon: 'logic'
      }))
    });
    groups.push({
      label: 'Exits',
      children: scene.exits.map((e, i) => ({
        label: `${e.to || '?'} at ${e.x},${e.z}`,
        ref: `exit:${i}`,
        icon: 'exit',
        reveal: () => {
          if (doc.scene3d && doc.mode === '3d') doc.scene3d.focusExit(i);
          const view = $('.view', doc.pane);
          if (view) drawSceneTags(view, doc);
        }
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
        ref: `part:${i}`,
        icon: 'part'
      }))
    }];
  }

  if (doc.kind === 'cell' && data.cells) {
    return [{
      label: 'Cells',
      children: data.cells.map(c => ({
        label: `cell ${c.index}`, note: `${c.parts.length}`, ref: `cell:${c.index}`,
        icon: 'cell'
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
        icon: 'logic',
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

// ------------------------------------------------------------------ inspecting
//
// Clicking an asset in the project shows it here without opening anything - a look at
// what something is before deciding to work on it. For a model that means a real
// viewer, small but live, because a picture of a character tells you much less than
// being able to turn it round. Double clicking is what opens a tab.

let inspected = null;
let inspectedViewer = null;

async function inspectAsset(kind, name, options = {}) {
  inspected = { kind, name, data: null };
  markList();
  if (options.reveal) revealInList(name);
  drawInspector();

  // Details come from whatever endpoint knows about that kind. A failure here is not
  // worth a red line - the panel just shows less.
  try {
    if (kind === 'model') {
      inspected.data = await api(`/api/model?name=${encodeURIComponent(name)}`);
    } else if (kind === 'cell') {
      inspected.data = await api(`/api/cell?name=${encodeURIComponent(name)}`);
    } else if (kind === 'texture') {
      inspected.data = await api(`/api/texture?name=${encodeURIComponent(name)}`);
    } else if (kind === 'image') {
      inspected.data = (state.images || []).find(i => i.name === name) || null;
    } else if (kind === 'map') {
      inspected.data = await api(`/api/map/scene?name=${encodeURIComponent(name)}`);
    } else if (kind === 'audio') {
      inspected.data = (state.audio || []).find(s => s.name === name) || null;
    }
  } catch (error) {
    inspected.problem = error.message;
  }

  if (inspected && inspected.name === name) drawInspector();
}

function clearInspected() {
  if (!inspected) return;
  inspected = null;
  inspectedViewer = null;
  markList();
  drawInspector();
}

// ---------------------------------------------------------------- moving about
//
// The arrow keys walk the file list, which is the only way a grid of 800 icons is
// bearable. Left and right run along a row and carry on into the next one, so holding
// one down covers everything in order - the same as the list, which is a grid one wide.
//
// Only the keys scroll. A click cannot move the view, because you clicked on something
// you could already see.

/// Brings a file into view, if it is out of it. Nothing moves when it is already there.
function revealInList(name) {
  const item = $(`#files li[data-name="${cssEscape(name)}"]`);
  if (!item) return;
  const list = $('#files');
  // Measured, not read off offsetTop, because the list is not necessarily what an
  // offset is relative to.
  const box = list.getBoundingClientRect();
  const at = item.getBoundingClientRect();
  if (at.top < box.top) list.scrollTop += at.top - box.top - 4;
  else if (at.bottom > box.bottom) list.scrollTop += at.bottom - box.bottom + 4;
}

function cssEscape(value) {
  return window.CSS && CSS.escape ? CSS.escape(value) : value.replace(/["\\]/g, '\\$&');
}

/// How many files sit on one row. One, until the grid wraps them.
function listColumns(items) {
  if (items.length < 2) return 1;
  const first = items[0].offsetTop;
  let columns = 1;
  while (columns < items.length && items[columns].offsetTop === first) columns++;
  return columns;
}

function moveInList(key) {
  const items = Array.from($('#files').children);
  if (!items.length) return;

  let at = items.findIndex(item => item.classList.contains('on'));
  const columns = listColumns(items);

  if (at < 0) {
    at = 0;
  } else if (key === 'ArrowRight') {
    at = Math.min(items.length - 1, at + 1);
  } else if (key === 'ArrowLeft') {
    at = Math.max(0, at - 1);
  } else if (key === 'ArrowDown') {
    at = Math.min(items.length - 1, at + columns);
  } else if (key === 'ArrowUp') {
    at = at - columns < 0 ? at : at - columns;
  } else if (key === 'Home') {
    at = 0;
  } else if (key === 'End') {
    at = items.length - 1;
  }

  inspectAsset(state.browse, items[at].dataset.name, { reveal: true });
}

document.addEventListener('keydown', (event) => {
  if (event.ctrlKey || event.altKey || event.metaKey) return;

  const inside = event.target.closest && event.target.closest('#project');
  if (!inside) return;
  // The filter box wants its own arrow keys for the text in it.
  if (event.target.tagName === 'INPUT' && event.key !== 'ArrowDown') return;

  if (event.key === 'Enter') {
    if (inspected) openDoc(inspected.kind, inspected.name);
    event.preventDefault();
    return;
  }

  if (!['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Home', 'End']
    .includes(event.key)) {
    return;
  }

  event.preventDefault();
  // Down out of the filter box lands on the list rather than skipping a file.
  if (event.target.tagName === 'INPUT') {
    $('#files').focus();
    if (!inspected) {
      const first = $('#files').firstElementChild;
      if (first) inspectAsset(state.browse, first.dataset.name, { reveal: true });
      return;
    }
  }
  moveInList(event.key);
});

/// The preview and facts for whatever was clicked in the project.
function drawInspectedAsset(box) {
  const { kind, name, data } = inspected;

  const heading = document.createElement('h2');
  heading.textContent = shortName(name);
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = name;
  box.append(heading, sub);

  const preview = previewFor(kind, name, data);
  if (preview) box.append(preview);

  box.append(factList(inspectedFacts(kind, data)));

  if (inspected.problem) {
    const problem = document.createElement('p');
    problem.className = 'none';
    problem.textContent = inspected.problem;
    box.append(problem);
  }

  const open = document.createElement('button');
  open.className = 'wide-button';
  open.textContent = 'Open';
  open.onclick = () => openDoc(kind, name);
  box.append(open);

  const hint = document.createElement('p');
  hint.className = 'caveat';
  hint.textContent = 'Double clicking it in the project opens it too.';
  box.append(hint);
}

/// A live viewer for a model, a picture for anything that is one, nothing otherwise.
function previewFor(kind, name, data) {
  if (kind === 'model' && data && data.buffer && data.buffer.length) {
    const stage = document.createElement('div');
    stage.className = 'preview-stage';
    const canvas = document.createElement('canvas');
    stage.append(canvas);

    // Built after it is in the page, or it has no size to render at.
    requestAnimationFrame(() => {
      const viewer = makeModelViewer(canvas, () => {});
      if (!viewer) return;
      inspectedViewer = viewer;
      viewer.show(data, name);
      turnable(canvas, viewer);
    });

    const note = document.createElement('p');
    note.className = 'caveat';
    note.textContent = 'drag to turn, wheel to zoom';
    const wrap = document.createElement('div');
    wrap.append(stage, note);
    return wrap;
  }

  if (kind === 'image' || kind === 'texture' || kind === 'cell') {
    const stage = document.createElement('div');
    stage.className = 'preview-stage checker';
    const picture = document.createElement('img');
    picture.alt = shortName(name);
    picture.src = kind === 'image'
      ? `/api/image?name=${encodeURIComponent(name)}`
      : kind === 'texture'
        ? `/api/texture/png?name=${encodeURIComponent(name)}&index=0`
        : (data && data.sheet
            ? `/api/image?name=${encodeURIComponent(data.sheet)}` : '');
    if (!picture.src) return null;
    picture.onerror = () => stage.remove();
    stage.append(picture);
    return stage;
  }
  return null;
}

function turnable(canvas, viewer) {
  let dragging = false;
  let lastX = 0;
  let lastY = 0;
  canvas.onpointerdown = (event) => {
    dragging = true;
    lastX = event.clientX;
    lastY = event.clientY;
    canvas.setPointerCapture(event.pointerId);
  };
  canvas.onpointermove = (event) => {
    if (!dragging) return;
    viewer.orbit(event.clientX - lastX, event.clientY - lastY);
    lastX = event.clientX;
    lastY = event.clientY;
  };
  canvas.onpointerup = (event) => {
    dragging = false;
    canvas.releasePointerCapture(event.pointerId);
  };
  canvas.onwheel = (event) => {
    event.preventDefault();
    viewer.zoom(Math.sign(event.deltaY));
  };
}

function inspectedFacts(kind, data) {
  const facts = [['kind', kind]];
  if (!data) return facts;

  if (kind === 'model' && data.groups) {
    facts.push(['parts', data.groups.length]);
    facts.push(['vertices', data.vertices]);
    facts.push(['triangles', data.triangles]);
    facts.push(['quads', data.quads]);
    facts.push(['nodes', (data.nodes || []).length]);
    const textures = [...new Set(data.groups.map(g => g.texture).filter(Boolean))];
    facts.push(['textures', textures.join(', ') || 'none']);
  } else if (kind === 'map' && data.objects) {
    facts.push(['terrain', data.terrain ? shortName(data.terrain) : 'none']);
    facts.push(['characters', data.objects.length]);
    facts.push(['logic casts', data.logic.length]);
    facts.push(['exits', data.exits.length]);
  } else if (kind === 'image' && data.width) {
    facts.push(['size', `${data.width} × ${data.height}`]);
    facts.push(['colour', `${data.colour}, ${data.depth}-bit`]);
    facts.push(['bytes', data.bytes]);
  } else if (kind === 'texture' && Array.isArray(data)) {
    facts.push(['textures', data.length]);
    if (data[0]) {
      facts.push(['first', `${data[0].name} · ${data[0].width}×${data[0].height} · ${data[0].format}`]);
    }
  } else if (kind === 'cell' && data.cells) {
    facts.push(['cells', data.cells.length]);
    facts.push(['sheet', data.sheet ? shortName(data.sheet) : 'none']);
  } else if (kind === 'audio' && data.name) {
    facts.push(['length', data.length]);
    facts.push(['format', data.format]);
  }
  return facts;
}

// ---------------------------------------------------------------- inspector

function inspectorBody() {
  return $('#inspector');
}

/// Facts about the open asset, and about whatever is selected inside it.
function drawInspector() {
  const box = inspectorBody();
  box.textContent = '';

  // Something clicked in the project takes the panel, without disturbing what is open.
  if (inspected) {
    drawInspectedAsset(box);
    return;
  }

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

  const materials = materialsFor(doc);
  if (materials) box.append(materials);

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

/// What a model is painted with: one swatch per material, with the picture it uses.
/// Seeing the texture beside the material is how a wrongly coloured character gives
/// itself away - the atlas is blue, the model is not.
function materialsFor(doc) {
  const data = doc.data;
  if (doc.kind !== 'model' || !data || !data.groups) return null;

  const seen = new Map();
  for (const group of data.groups) {
    const key = group.material || group.shape;
    if (!seen.has(key)) seen.set(key, group);
  }

  const box = document.createElement('div');
  const heading = document.createElement('h3');
  heading.textContent = 'Materials';
  box.append(heading);

  for (const [name, group] of seen) {
    const row = document.createElement('div');
    row.className = 'material';

    const thumb = document.createElement('div');
    thumb.className = 'material-thumb';
    if (group.texture) {
      const picture = document.createElement('img');
      picture.loading = 'lazy';
      picture.alt = group.texture;
      picture.src = `/api/model/texture?name=${encodeURIComponent(doc.name)}`
        + `&texture=${encodeURIComponent(group.texture)}`;
      thumb.append(picture);
    } else {
      // No texture means the material's own colour is all there is.
      thumb.style.background = '#' + (group.colour >>> 0).toString(16).padStart(6, '0');
    }

    const about = document.createElement('div');
    about.className = 'material-about';
    const title = document.createElement('b');
    title.textContent = name || '(unnamed)';
    const note = document.createElement('span');
    note.textContent = group.texture || 'no texture';
    const tint = document.createElement('span');
    tint.className = 'material-tint';
    const dot = document.createElement('i');
    dot.style.background = '#' + (group.colour >>> 0).toString(16).padStart(6, '0');
    tint.append(dot, document.createTextNode(
      'tint · alpha ' + Math.round((group.alpha ?? 1) * 100) + '%'
      + (group.translucent ? ' · translucent' : '')));
    about.append(title, note, tint);

    row.append(thumb, about);
    row.onclick = () => {
      if (!group.texture) return;
      openDoc('texture', doc.name);
    };
    box.append(row);
  }
  return box;
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

let lastLine = null;

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

  lastLine = line;
  setStatusBar(text, tone);

  if (tone === 'bad') {
    consoleCount++;
    $('#console-count').textContent = consoleCount;
    $('#console-count').className = 'on';
  }
}

function setStatusBar(text, tone) {
  $('#statusbar-text').textContent = text || 'ready';
  $('#statusbar').className = tone || '';
}

// say() now reports along the bottom rather than in the header, and everything it says
// is kept in the console rather than being wiped by the next message. "loading…" is the
// exception: it is noise a second later, so it shows and is not recorded.
say = function (message, tone) {
  setStatusBar(message, tone);
  if (message && message !== 'loading…') logLine(message, tone);
};

window.addEventListener('error', event => logLine(event.message, 'bad'));
window.addEventListener('unhandledrejection', event =>
  logLine(String(event.reason && event.reason.message || event.reason), 'bad'));

// -------------------------------------------------------------------- undo
//
// Per document, because undoing on a map should not reach into a script. Views record
// their own steps: a step is a label and the two functions that put things back and
// forward again, which keeps the stack out of the business of knowing what a map is.
//
// Fields and text areas are left alone. The browser's own undo is better than anything
// this could do inside one, and taking Ctrl+Z off a half-typed line would be worse than
// not having it at all.

function pushUndo(doc, label, undo, redo) {
  if (!doc) return;
  doc.undo = doc.undo || [];
  doc.redo = [];
  doc.undo.push({ label, undo, redo });
  if (doc.undo.length > 200) doc.undo.shift();
}

function undoLast() {
  const doc = activeDoc;
  if (!doc || !doc.undo || !doc.undo.length) {
    say('nothing to undo');
    return;
  }
  const step = doc.undo.pop();
  step.undo();
  (doc.redo = doc.redo || []).push(step);
  say('undone: ' + step.label);
}

function redoLast() {
  const doc = activeDoc;
  if (!doc || !doc.redo || !doc.redo.length) {
    say('nothing to redo');
    return;
  }
  const step = doc.redo.pop();
  step.redo();
  doc.undo.push(step);
  say('redone: ' + step.label);
}

document.addEventListener('keydown', event => {
  if (!(event.ctrlKey || event.metaKey)) return;
  const key = event.key.toLowerCase();
  if (key !== 'z' && key !== 'y') return;
  // Inside something you can type in, the browser's own undo wins.
  if (event.target.matches('input, textarea, [contenteditable]')) return;

  event.preventDefault();
  if (key === 'y' || event.shiftKey) redoLast();
  else undoLast();
});

// ------------------------------------------------------------------ project

let browseKind = 'map';

function drawProjectTree() {
  const tree = $('#project-tree');
  tree.textContent = '';

  for (const kind of KINDS) {
    const row = document.createElement('div');
    row.className = 'row' + (kind.id === browseKind ? ' on' : '');
    row.append(icon(kind.id));
    const label = document.createElement('span');
    label.textContent = kind.label;
    row.append(label);
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
        layoutChanged();
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

// The project list shows either one file per line or a wrapped grid of icons. Which
// one is remembered, because it is a habit rather than a per-file choice.
let fileView = 'list';
try {
  fileView = localStorage.getItem('ff3-editor-files') || 'list';
} catch (error) {
  fileView = 'list';
}

function setFileView(which) {
  fileView = which;
  $('#files').classList.toggle('grid', which === 'grid');
  $('#view-list').classList.toggle('on', which === 'list');
  $('#view-grid').classList.toggle('on', which === 'grid');
  try {
    localStorage.setItem('ff3-editor-files', which);
  } catch (error) {
    // Not being able to remember the choice is not worth interrupting anyone over.
  }
}

$('#view-list').onclick = () => setFileView('list');
$('#view-grid').onclick = () => setFileView('grid');
setFileView(fileView);

// Clicking the status line opens the console at the row it is showing.
$('#statusbar').onclick = () => {
  $$('#bottom-tabs button').forEach(b =>
    b.classList.toggle('on', b.dataset.bottom === 'console'));
  $$('.bottom-page').forEach(p => p.classList.toggle('on', p.dataset.page === 'console'));
  consoleCount = 0;
  $('#console-count').textContent = '';
  $('#console-count').className = '';
  if (!lastLine) return;
  $$('.console-line.picked').forEach(l => l.classList.remove('picked'));
  lastLine.classList.add('picked');
  lastLine.scrollIntoView({ block: 'center' });
};

$('#hierarchy-filter').addEventListener('input', drawHierarchy);
$('#filter').addEventListener('input', drawList);

window.addEventListener('hashchange', () => {
  const { kind, name } = readHash();
  if (activeDoc && (kind || 'map') === activeDoc.kind && name === activeDoc.name) return;
  if (!activeDoc && !name && (kind || 'map') === browseKind) return;
  applyHash().catch(error => say(error.message, 'bad'));
});

// Dropping a tab on the right of the document area splits it in two.
const hint = $('#split-hint');
$('#doc-area').addEventListener('dragover', event => {
  const box = $('#doc-area').getBoundingClientRect();
  const nearEdge = event.clientX > box.right - 140;
  hint.classList.toggle('on', nearEdge && groups.length < 2);
  if (nearEdge) event.preventDefault();
});
$('#doc-area').addEventListener('dragleave', () => hint.classList.remove('on'));
$('#doc-area').addEventListener('drop', event => {
  const box = $('#doc-area').getBoundingClientRect();
  hint.classList.remove('on');
  if (event.clientX <= box.right - 140 || groups.length > 1) return;
  event.preventDefault();
  const doc = docs.get(event.dataTransfer.getData('text/plain'));
  if (doc && doc.group.docs.length > 1) moveDoc(doc, makeGroup());
});

makeGroup();
loadSizes();
wireSplitters();
drawProjectTree();
drawHierarchy();
drawInspector();

api('/api/status')
  .then(status => {
    // The header keeps the workspace summary; it is true all session and would only
    // be wiped by the next thing that happened if say() owned it.
    state.language = status.language || 'en';
    state.messagePrefix = status.messagePrefix || `${state.language}.lproj/`;
    $('#status').textContent = `${status.files} files  ·  overrides in ${status.overrides}`;
  })
  .then(applyHash)
  .catch(error => say(error.message, 'bad'));
