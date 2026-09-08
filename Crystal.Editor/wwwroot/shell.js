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
  { id: 'data', label: 'Game data' },
  { id: 'image', label: 'Images' },
  { id: 'texture', label: 'Textures' },
  { id: 'model', label: 'Models' },
  { id: 'cell', label: 'Cells' },
  { id: 'audio', label: 'Audio' },
  // The mod's own files sit under a folder of their own at the bottom, below a rule:
  // they belong to the project, not to either game, so they are the same whichever
  // game tab is current. Code is the C# (and project.json, the csproj); Scenes are the
  // maps the mod has put behaviours or objects on - each opens the map editor.
  { id: 'code', label: 'Code', mod: true },
  { id: 'scene', label: 'Scenes', mod: true },
  { id: 'items', label: 'Items', mod: true },
  { id: 'characters', label: 'Characters', mod: true }
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

function docId(kind, name, ws = state.ws) {
  // The mod's files are the project's, not a game's: one document however many games
  // are open, so opening Mod.cs from the FF4 tab finds the one opened from FF3's.
  if (kind === 'code') ws = null;
  return `${ws || ''}|${kind}|${name}`;
}

/// The games the project has open, from the last /api/status.
function workspaces() {
  return (typeof projectState !== 'undefined' && projectState.workspaces) || [];
}

function workspaceLabel(target) {
  const found = workspaces().find(w => w.target === target);
  return found ? found.game.toUpperCase() : (target || '');
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
  if (kind === 'scene') return openScene(name, options);
  // An item definition is edited in the inspector; opening it opens its file as text.
  if (kind === 'items') return openDoc('code', 'defs/items/' + name + '.json', options);
  if (kind === 'characters') return openDoc('code', 'defs/characters/' + name + '.json', options);
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
    id, kind, name, ws: state.ws, selection: null, data: null, group,
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

/// A scene file is a map with the mod's additions on it, so opening one opens the map -
/// in the map editor, under an OpenFF game (the current one when it is, else the
/// project's first), with the panel following to Maps as it does for any map. The file
/// itself is a click away in the inspector.
async function openScene(map, options = {}) {
  const ws = sceneWorkspace();
  if (ws && ws !== state.ws) {
    state.ws = ws;
    if (typeof resetOps === 'function') resetOps();
  }
  // The panel follows to Maps; an emptied list makes runView load the maps for it.
  browseKind = 'map';
  state.files = [];
  const doc = await openDoc('map', map, options);
  drawProjectTree();
  return doc;
}

/// The game a scene opens under: the current one if it is an OpenFF one, else the
/// project's first OpenFF game, else whatever is current.
function sceneWorkspace() {
  const ours = typeof oursWorkspaces === 'function' ? oursWorkspaces() : [];
  if (ours.some(w => w.target === state.ws)) return state.ws;
  return ours.length ? ours[0].target : state.ws;
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
  noteSession();
}

/// Runs the view for a document into its own pane.
async function runView(doc) {
  state.kind = doc.kind;
  state.name = doc.name;
  state.pane = doc.pane;
  state.ws = doc.ws;

  // The views read their listing from state.files, so make sure it is the right one -
  // the right kind, and the right game.
  if (state.browse !== doc.kind || !state.files.length || state.filesWs !== doc.ws) {
    state.browse = doc.kind;
    // The tree's highlight says which library the list shows; it moves with the list.
    browseKind = doc.kind;
    await loadList();
    drawProjectTree();
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
  if (doc.kind !== 'code' && doc.ws !== undefined && doc.ws !== state.ws) {
    // Focusing the other game's document makes that game current everywhere - the
    // inspector, the project list, the next thing opened. (A mod file belongs to no
    // game and leaves the current one alone.)
    state.ws = doc.ws;
    if (typeof resetOps === 'function') resetOps();
    browseKind = doc.kind;
    state.browse = doc.kind;
    loadList().then(drawProjectTree).catch(() => {});
  }

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
  // With two games open, a tab says which one it belongs to.
  const badge = document.createElement('b');
  badge.className = 'ws ' + (workspaceLabel(doc.ws) || '').toLowerCase();
  badge.textContent = workspaceLabel(doc.ws);
  badge.hidden = workspaces().length < 2 || doc.kind === 'code';

  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  shut.title = 'close';
  shut.onpointerdown = (event) => event.stopPropagation();
  shut.onclick = (event) => {
    event.stopPropagation();
    closeDoc(doc.id);
  };

  tab.append(badge, kind, label, shut);

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
/// Whether the hierarchy lists the game's characters the mod has replaced (off unless asked; remembered).
let hierarchyShowReplaced = (() => { try { return localStorage.getItem('hierarchy.replaced') === '1'; } catch (e) { return false; } })();

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
    // The game's characters the mod has replaced are out of the way unless asked for: the
    // group's count is what is left, and a toggle on the header shows the replaced too.
    const replaced = group.children.filter(c => c.replaced);
    const showReplaced = replaced.length > 0 && hierarchyShowReplaced;
    const visible = group.children.filter(c => !c.replaced || showReplaced || activeDoc.selection === c.ref);
    if (group.children.length) {
      const count = document.createElement('b');
      count.textContent = visible.length;
      if (replaced.length && !showReplaced) count.title = `${replaced.length} replaced by the mod's objects, hidden`;
      head.append(count);
    }
    if (replaced.length) {
      const toggle = document.createElement('label');
      toggle.className = 'group-toggle';
      toggle.title = `${replaced.length} of the game's characters are replaced by the mod's objects (Removed). Show them too?`;
      const box = document.createElement('input');
      box.type = 'checkbox';
      box.checked = showReplaced;
      box.onchange = () => { hierarchyShowReplaced = box.checked; try { localStorage.setItem('hierarchy.replaced', box.checked ? '1' : '0'); } catch (e) { /* no storage */ } drawHierarchy(); };
      toggle.onclick = event => event.stopPropagation();
      toggle.append(box, document.createTextNode(`replaced (${replaced.length})`));
      head.append(toggle);
    }
    // A group can take a right click (New object…) and a drop (to the top level).
    if (group.menu) head.oncontextmenu = event => { event.preventDefault(); showContextMenu(event, group.menu()); };
    if (group.drop) wireDrop(head, group.drop);
    tree.append(head);

    let shown = 0;
    for (const child of visible) {
      if (filter && !child.label.toLowerCase().includes(filter)) continue;
      shown++;
      const row = document.createElement('li');
      row.className = 'row' + (activeDoc.selection === child.ref ? ' on' : '') + (child.dim ? ' dim' : '');
      // A tree inside the group: children indented under their parent (scene objects).
      if (child.depth) row.style.paddingLeft = (20 + child.depth * 14) + 'px';
      row.append(icon(child.icon || 'file'));
      const text = document.createElement('span');
      text.textContent = child.label;
      row.append(text);
      if (child.badge) {
        const mark = document.createElement('em');
        mark.className = 'badge';
        mark.textContent = '\u25c6';
        mark.title = 'has behaviours (OpenFF)';
        row.append(mark);
      }
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
      // Right click: the row's own menu (new child, rename, delete...), with the row selected first.
      if (child.menu) {
        row.oncontextmenu = event => {
          event.preventDefault();
          if (activeDoc.selection !== child.ref) {
            clearInspected();
            activeDoc.selection = child.ref;
            drawHierarchy();
            drawInspector();
          }
          showContextMenu(event, child.menu());
        };
      }
      // Drag one row onto another to make it that one's child.
      if (child.drag) {
        row.draggable = true;
        row.ondragstart = event => {
          event.dataTransfer.setData('text/x-hierarchy', child.drag);
          event.dataTransfer.effectAllowed = 'move';
          row.classList.add('dragging');
        };
        row.ondragend = () => row.classList.remove('dragging');
      }
      if (child.drop) wireDrop(row, child.drop);
      tree.append(row);
    }

    if (filter && !shown) head.remove();
  }
}

/// Makes a hierarchy row a drop target for another row: the handler gets the dragged key.
function wireDrop(row, handler) {
  row.ondragover = event => {
    if (![...event.dataTransfer.types].includes('text/x-hierarchy')) return;
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';
    row.classList.add('drop');
  };
  row.ondragleave = () => row.classList.remove('drop');
  row.ondrop = event => {
    row.classList.remove('drop');
    const key = event.dataTransfer.getData('text/x-hierarchy');
    if (!key) return;
    event.preventDefault();
    handler(key);
  };
}

// ------------------------------------------------------------- context menu
//
// One small menu at the pointer, built from { label, run, disabled?, sep? } items; the
// next click anywhere, or Escape, takes it away. The same box the Add Behaviour list uses.

function showContextMenu(event, items) {
  document.querySelectorAll('.context-menu').forEach(m => m.remove());
  if (!items || !items.length) return;
  const menu = document.createElement('div');
  menu.className = 'dropdown context-menu';
  const list = document.createElement('ul');
  list.className = 'dropdown-list';
  const close = () => {
    menu.remove();
    document.removeEventListener('pointerdown', outside, true);
    document.removeEventListener('keydown', key, true);
  };
  const outside = e => { if (!menu.contains(e.target)) close(); };
  const key = e => { if (e.key === 'Escape') { close(); e.preventDefault(); } };
  for (const item of items) {
    if (item.sep) {
      const hr = document.createElement('li');
      hr.className = 'menu-sep';
      list.append(hr);
      continue;
    }
    const li = document.createElement('li');
    if (item.icon) li.append(icon(item.icon));
    const label = document.createElement('span');
    label.className = 'dropdown-name';
    label.textContent = item.label;
    li.append(label);
    if (item.disabled) li.classList.add('dim');
    else li.onclick = () => { close(); item.run(); };
    list.append(li);
  }
  menu.append(list);
  document.body.append(menu);
  const x = Math.min(event.clientX, window.innerWidth - menu.offsetWidth - 6);
  const y = Math.min(event.clientY, window.innerHeight - menu.offsetHeight - 6);
  menu.style.left = Math.max(4, x) + 'px';
  menu.style.top = Math.max(4, y) + 'px';
  setTimeout(() => {
    document.addEventListener('pointerdown', outside, true);
    document.addEventListener('keydown', key, true);
  }, 0);
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
    const openffProject = typeof openFFProject === 'function' && openFFProject();
    // The game's rows get a menu too: the behaviours, the script, and on an OpenFF
    // project the way across to the mod's own objects.
    const gameMenu = (ref, extra) => () => [
      { label: 'Add Behaviour…', icon: 'behaviour', disabled: !openffProject, run: () => {
        doc.selection = ref;
        drawHierarchy();
        drawInspector();
        const add = document.querySelector('#inspector .behaviour-add-button');
        if (add) add.click();
      } },
      ...(extra || []),
    ];
    groups.push({
      label: 'Characters',
      menu: () => [
        { label: 'Convert the map to OpenFF objects (exact - each keeps its cast)…', icon: 'mod', disabled: !openffProject || typeof convertMap !== 'function', run: () => convertMap(doc).catch(error => say(error.message, 'bad')) },
        { label: 'Convert to OpenFF scripts (each cast\'s code as the mod\'s own, editable)…', icon: 'code', disabled: !openffProject || typeof convertMap !== 'function', run: () => convertMap(doc, { scripts: true }).catch(error => say(error.message, 'bad')) },
        { label: 'Convert with components (Chest, Talk… where they fit)…', icon: 'behaviour', disabled: !openffProject || typeof convertMap !== 'function', run: () => convertMap(doc, { components: true }).catch(error => say(error.message, 'bad')) },
      ],
      children: scene.objects.map(o => ({
        label: o.name,
        note: o.hasScript ? `${o.instructions}` : 'no script',
        ref: `object:${o.index}`,
        icon: 'character',
        reveal: () => revealObject(doc, o),
        menu: gameMenu(`object:${o.index}`, [
          { label: 'Focus in view', icon: 'scene', run: () => revealObject(doc, o) },
          { label: o.hasScript ? `Open script at cast ${o.cast}` : 'Open script', icon: 'logic', run: async () => {
            const opened = await openDoc('script', `files/${shortName(doc.name)}.script`);
            const text = opened && opened.pane.querySelector('textarea');
            if (!text) return;
            const at = text.value.indexOf(`cast${o.cast}_main:`);
            if (at >= 0 && typeof goToLine === 'function') goToLine(text, text.value.slice(0, at).split('\n').length, 1);
          } },
          { sep: true },
          { label: 'Convert to OpenFF object (exact - keeps its cast)', icon: 'mod', disabled: !openffProject || typeof convertToSceneObject !== 'function', run: () => {
            const character = (typeof mapState !== 'undefined' && mapState.data && mapState.data.characters || []).find(c => c.index === o.index);
            if (character) convertToSceneObject(doc, character).catch(error => say(error.message, 'bad'));
          } },
          { label: 'Convert with components (Chest / Talk)', icon: 'behaviour', disabled: !openffProject || typeof convertToSceneObject !== 'function', run: () => {
            const character = (typeof mapState !== 'undefined' && mapState.data && mapState.data.characters || []).find(c => c.index === o.index);
            if (character) convertToSceneObject(doc, character, { components: true }).catch(error => say(error.message, 'bad'));
          } },
          { label: 'Restore the game\'s character (drop the stand-in)', icon: 'character', disabled: !openffProject || typeof restoreCharacter !== 'function'
            || !(typeof sceneState !== 'undefined' && (sceneState.attachments || []).some(a => a.behaviour === 'Removed' && (a.target || '').toLowerCase() === `object:${o.index}`)),
            run: () => restoreCharacter(doc, o.index) },
        ])
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
        },
        menu: gameMenu(`exit:${i}`, [
          { label: 'Focus in view', icon: 'scene', run: () => { if (doc.scene3d && doc.mode === '3d') doc.scene3d.focusExit(i); } },
          { label: e.to ? `Open ${e.to}` : 'Open the destination', icon: 'map', disabled: !e.to, run: () => openDoc('map', e.to) },
        ])
      }))
    });
    if (scene.terrain) {
      groups[0].children[0].menu = gameMenu('terrain', [
        { label: 'Open the terrain model', icon: 'model', run: () => openDoc('model', scene.terrain) },
        { label: 'New OpenFF object here', icon: 'exit', disabled: !openffProject, run: () => { if (typeof addSceneObject === 'function') addSceneObject(doc); } },
      ]);
    }
    // An OpenFF project's scene file: the mod's own objects as a tree, and a mark on
    // whatever carries behaviours - the game's things above, the mod's here.
    const openff = typeof sceneState !== 'undefined' && typeof mapState !== 'undefined'
      && sceneState.map === mapState.name && sceneState.objects;
    if (openff) {
      const carried = new Set((sceneState.attachments || []).map(a => (a.target || '').toLowerCase()));
      // A game character the mod has converted: taken off the map in the client, its
      // stand-in among the objects below.
      const removed = new Set((sceneState.attachments || []).filter(a => a.behaviour === 'Removed').map(a => (a.target || '').toLowerCase()));
      for (const group of groups) {
        for (const child of group.children) {
          if (carried.has(child.ref.toLowerCase())) child.badge = 'behaviour';
          if (removed.has(child.ref.toLowerCase())) { child.note = 'replaced'; child.dim = true; child.replaced = true; }
        }
      }
      const flat = typeof flattenSceneObjects === 'function' ? flattenSceneObjects(sceneState) : [];
      // Dropping a row on another makes it that one's child; on the group, top level.
      const dropOn = parent => key => {
        const dragged = findSceneObject(key);
        if (!dragged || dragged.source === parent) return;
        reparentSceneObject(doc, dragged.source, parent);
      };
      groups.push({
        label: 'Objects (OpenFF)',
        menu: () => [
          { label: 'New object', icon: 'exit', run: () => addSceneObject(doc) },
        ],
        drop: dropOn(null),
        children: flat.map((item, i) => ({
          label: item.name,
          note: item.model || (item.source.tags || []).join(' '),
          ref: `scene:${item.path}`,
          icon: item.model ? 'model' : 'exit',
          depth: item.depth,
          badge: carried.has(item.path.toLowerCase()) ? 'behaviour' : null,
          reveal: () => { if (doc.scene3d && doc.mode === '3d') doc.scene3d.focusPoint(i); },
          drag: item.path,
          drop: dropOn(item.source),
          menu: () => sceneObjectMenu(doc, item.source)
        })).concat([{
          label: '+ add an object',
          ref: 'scene:+',
          icon: 'exit',
          dim: true,
          reveal: () => { if (typeof addSceneObject === 'function') addSceneObject(doc); }
        }])
      });
    }
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

  if (doc.kind === 'code' && typeof outlineForCode === 'function') {
    return outlineForCode(doc);
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
  // Held locally as well: a double click inspects and then opens, and opening clears
  // the inspector while the details are still on their way. They land on this
  // object, which nobody shows any more, rather than on null.
  const me = { kind, name, data: null };
  inspected = me;
  markList();
  if (options.reveal) revealInList(name);
  drawInspector();

  // Details come from whatever endpoint knows about that kind. A failure here is not
  // worth a red line - the panel just shows less.
  try {
    if (kind === 'model') {
      me.data = await api(`/api/model?name=${encodeURIComponent(name)}`);
    } else if (kind === 'cell') {
      me.data = await api(`/api/cell?name=${encodeURIComponent(name)}`);
    } else if (kind === 'texture') {
      me.data = await api(`/api/texture?name=${encodeURIComponent(name)}`);
    } else if (kind === 'image') {
      me.data = (state.images || []).find(i => i.name === name) || null;
    } else if (kind === 'map') {
      me.data = await api(`/api/map/scene?name=${encodeURIComponent(name)}`);
    } else if (kind === 'audio') {
      me.data = (state.audio || []).find(s => s.name === name) || null;
    } else if (kind === 'code') {
      const file = await api(`/api/project/file?name=${encodeURIComponent(name)}`);
      if (file.ok === false) throw new Error(file.error);
      me.data = file;
    } else if (kind === 'scene') {
      const scene = await api(`/api/project/scene?map=${encodeURIComponent(name)}`);
      if (scene.ok === false) throw new Error(scene.error);
      me.data = scene;
    } else if (kind === 'items') {
      const item = await api(`/api/project/items?id=${encodeURIComponent(name)}`);
      if (item.ok === false) throw new Error(item.error);
      me.data = item.item;
    } else if (kind === 'characters') {
      const c = await api(`/api/project/characters?id=${encodeURIComponent(name)}`);
      if (c.ok === false) throw new Error(c.error);
      me.data = { character: c.character, jobs: c.jobs, heroes: c.heroes };
    }
  } catch (error) {
    me.problem = error.message;
  }

  if (inspected === me) drawInspector();
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
  const items = Array.from($('#files').children).filter(item => item.dataset.name);
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
      const first = $('#files li[data-name]');
      if (first) inspectAsset(state.browse, first.dataset.name, { reveal: true });
      return;
    }
  }
  moveInList(event.key);
});

/// The preview and facts for whatever was clicked in the project.
function drawInspectedAsset(box) {
  const { kind, name, data } = inspected;

  // An item definition is edited right here, as a scene object is: the form is the panel.
  if (kind === 'characters' && data && typeof characterDefinitionPanel === 'function') {
    box.append(characterDefinitionPanel(data, () => { loadList(); }));
    return;
  }
  if (kind === 'items' && data && typeof itemDefinitionPanel === 'function') {
    box.append(itemDefinitionPanel(data, () => { loadList(); }));
    if (inspected.problem) {
      const problem = document.createElement('p');
      problem.className = 'none';
      problem.textContent = inspected.problem;
      box.append(problem);
    }
    return;
  }

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
  open.textContent = kind === 'scene' ? 'Open map' : 'Open';
  if (kind === 'scene') open.title = 'The map in the map editor, with the mod\'s behaviours and objects on it';
  open.onclick = () => openDoc(kind, name);
  box.append(open);

  // The scene file itself, for reading or a careful edit by hand.
  if (kind === 'scene') {
    const json = document.createElement('button');
    json.className = 'wide-button';
    json.textContent = 'Open as JSON';
    json.title = `scenes/${name}.json as text - what Export to OpenFF copies into the mod`;
    json.onclick = () => openDoc('code', `scenes/${name}.json`);
    box.append(json);
  }

  // A mod file can go to the machine's own editor as well as to a tab here.
  if (kind === 'code') {
    const ide = document.createElement('button');
    ide.className = 'wide-button';
    ide.textContent = 'Open in IDE';
    ide.title = 'Whatever opens this kind of file here - Visual Studio, Rider, VS Code';
    ide.onclick = async () => {
      try {
        const result = await api('/api/project/file/open', { name });
        if (!result.ok) throw new Error(result.error);
        say(`opened ${shortName(name)} in your editor`, 'good');
      } catch (error) {
        say(error.message, 'bad');
      }
    };
    box.append(ide);
  }

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
    picture.src = wsUrl(kind === 'image'
      ? `/api/image?name=${encodeURIComponent(name)}`
      : kind === 'texture'
        ? `/api/texture/png?name=${encodeURIComponent(name)}&index=0`
        : (data && data.sheet
            ? `/api/image?name=${encodeURIComponent(data.sheet)}` : ''));
    if (!picture.src) return null;
    picture.onerror = () => stage.remove();
    stage.append(picture);
    return stage;
  }
  if (kind === 'code' && typeof codePeek === 'function') {
    return codePeek(name, data);
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
  } else if (kind === 'code' && typeof codeFacts === 'function') {
    return codeFacts(data);
  } else if (kind === 'scene') {
    facts[0] = ['kind', 'scene file'];
    facts.push(['map', data.map]);
    facts.push(['behaviours', (data.attachments || []).length]);
    const countObjects = list => (list || []).reduce((n, o) => n + 1 + countObjects(o && o.children), 0);
    facts.push(['objects', countObjects(data.objects)]);
    const names = [...new Set((data.attachments || []).map(a => a.behaviour).filter(Boolean))];
    if (names.length) facts.push(['classes', names.join(', ')]);
    const targets = [...new Set((data.attachments || []).map(a => a.target).filter(Boolean))];
    if (targets.length) facts.push(['on', targets.join(', ')]);
    facts.push(['file', `scenes/${data.map}.json`]);
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
  const facts = factsFor(doc);
  const materials = materialsFor(doc);
  const detail = doc.selection && doc.inspect ? doc.inspect(doc.selection) : null;

  // With something selected, the selection is the panel: the document's own facts fold
  // away at the bottom, where they are a click off rather than in the way. With nothing
  // selected they are the panel, as before.
  if (detail) {
    box.append(detail);
    const about = document.createElement('details');
    about.className = 'about-doc';
    const summary = document.createElement('summary');
    summary.textContent = 'About ' + shortName(doc.name);
    about.append(summary);
    const kind = document.createElement('p');
    kind.className = 'sub';
    kind.textContent = doc.name;
    about.append(kind);
    if (facts.length) about.append(factList(facts));
    if (materials) about.append(materials);
    box.append(about);
    return;
  }

  const heading = document.createElement('h2');
  heading.textContent = shortName(doc.name);
  const kind = document.createElement('p');
  kind.className = 'sub';
  kind.textContent = doc.name;
  box.append(heading, kind);
  if (facts.length) box.append(factList(facts));
  if (materials) box.append(materials);
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
      picture.src = wsUrl(`/api/model/texture?name=${encodeURIComponent(doc.name)}`
        + `&texture=${encodeURIComponent(group.texture)}`);
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
  } else if (doc.kind === 'code' && typeof codeFacts === 'function') {
    return codeFacts(data);
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

  // A tab per game above the libraries: FF3, FF4 - a filter on whose content the panel
  // shows. What kind of mod the edits become (OpenFF or Steam) is the project's affair
  // and is only written on a tab when the same game is open twice, once each way, as it
  // is with no project (every install open, the repository's Content beside Steam's
  // FF3). A game installed but not opened by the project sits greyed out; clicking it
  // opens the settings, where a tick adds it.
  const open = workspaces();
  const available = (typeof projectState !== 'undefined' && projectState.available) || [];
  const found = available.filter(a => a.found);
  const closedGames = [...new Set(found.map(a => a.game))].filter(g => !open.some(w => w.game === g));
  if (open.length + closedGames.length > 1) {
    const tabs = document.createElement('div');
    tabs.className = 'ws-tabs';
    const byGame = ['ff3', 'ff4'];
    const sorted = [...open].sort((a, b) => byGame.indexOf(a.game) - byGame.indexOf(b.game));
    for (const w of sorted) {
      const twice = open.filter(o => o.game === w.game).length > 1;
      const tab = document.createElement('button');
      tab.className = 'ws-tab ' + w.game + (w.target === state.ws ? ' on' : '');
      tab.title = `${w.label} · ${w.files} files · ${w.contentDirectory}`;
      const game = document.createElement('b');
      game.textContent = w.game.toUpperCase();
      tab.append(game);
      if (twice) {
        const where = document.createElement('span');
        where.textContent = w.mod === 'openff' ? 'OpenFF' : 'Steam';
        tab.append(where);
      }
      tab.onclick = () => selectWorkspace(w.target);
      tabs.append(tab);
    }
    for (const g of closedGames) {
      const tab = document.createElement('button');
      tab.className = 'ws-tab off ' + g;
      const project = (typeof projectState !== 'undefined' && projectState.project) || null;
      tab.title = project
        ? `${g.toUpperCase()} is installed, but this project does not open it. Project settings adds it - as part of the OpenFF mod, or as a Steam mod of its own.`
        : `${g.toUpperCase()} is installed but could not be opened.`;
      const game = document.createElement('b');
      game.textContent = g.toUpperCase();
      tab.append(game);
      if (project && typeof projectSettingsDialog === 'function') tab.onclick = () => projectSettingsDialog();
      else tab.disabled = true;
      tabs.append(tab);
    }
    tree.append(tabs);
  }

  const project = (typeof projectState !== 'undefined' && projectState.project) || null;
  const openff = project && typeof isOpenFFProject === 'function' && isOpenFFProject(project);
  let into = tree;
  for (const kind of KINDS) {
    if (kind.mod && into === tree) {
      // The mod's own folder, below a rule: it is the project's, not a game's. It
      // sticks to the panel's bottom, so a short panel that scrolls the libraries
      // still shows it - a folder nobody can see is the confusion this is here to end.
      const foot = document.createElement('div');
      foot.className = 'tree-foot';
      const rule = document.createElement('div');
      rule.className = 'tree-rule';
      foot.append(rule);
      const head = document.createElement('div');
      head.className = 'row mod' + (KINDS.some(k => k.mod && k.id === browseKind) ? ' open' : '');
      head.append(icon('mod'));
      const label = document.createElement('span');
      label.textContent = 'OpenFF mod';
      head.append(label);
      const note = document.createElement('i');
      if (!project) note.textContent = 'no project';
      else if (!openff) note.textContent = 'Steam only';
      if (note.textContent) head.append(note);
      head.title = !project
        ? 'The project\'s own files - its C# code and the maps it puts behaviours on. File ▸ New project… makes one.'
        : !openff
          ? `${project.name} is a Steam mod: files replaced in the game, no code. Tick FF3 or FF4 under OpenFF in Project settings to make it an OpenFF mod as well.`
          : `${project.name}: an OpenFF mod - its C# code, and the maps it puts behaviours and objects on. ${project.directory}`;
      head.onclick = () => selectKind('code');
      foot.append(head);
      tree.append(foot);
      into = foot;
    }
    const row = document.createElement('div');
    row.className = 'row' + (kind.id === browseKind ? ' on' : '') + (kind.mod ? ' sub' : '');
    row.append(icon(kind.mod ? (kind.id === 'scene' ? 'scene' : kind.id === 'items' ? 'item' : kind.id === 'characters' ? 'character' : 'code') : kind.id));
    const label = document.createElement('span');
    label.textContent = kind.label;
    row.append(label);
    if (kind.mod && project && openff) {
      const count = document.createElement('i');
      count.textContent = kind.id === 'scene' ? (project.scenes || '') : kind.id === 'items' ? (project.items || '') : kind.id === 'characters' ? (project.characters || '') : '';
      if (count.textContent) row.append(count);
      row.title = kind.id === 'scene'
        ? 'Maps this mod has put behaviours or objects on (scenes/<map>.json). Each opens in the map editor.'
        : kind.id === 'items'
          ? 'The mod\'s own items (defs/items/<id>.json): each starts from one of the game\'s and changes what it names; the client adds them to the game\'s item table.'
          : kind.id === 'characters'
            ? 'The heroes as a game begins (defs/characters/<id>.json): a slot\'s name, starting job and level.'
            : 'The C# code under code/, and project.json. Opens here, or in your IDE.';
    }
    row.onclick = () => selectKind(kind.id);
    into.append(row);
  }
  drawCodeActions();
}

/// The buttons beside the filter while the mod folder is showing: what the project
/// can do next. No project: make one. No code: add it. Code: a new file, a build, the
/// whole project in the IDE, and the folder on disk.
function drawCodeActions() {
  const strip = $('#code-actions');
  if (!strip) return;
  strip.textContent = '';
  strip.hidden = !KINDS.some(k => k.mod && k.id === browseKind);
  if (strip.hidden) return;

  const project = (typeof projectState !== 'undefined' && projectState.project) || null;
  const openff = project && typeof isOpenFFProject === 'function' && isOpenFFProject(project);
  const button = (label, title, run, primary) => {
    const b = document.createElement('button');
    b.textContent = label;
    b.title = title;
    if (primary) b.className = 'primary';
    b.onclick = run;
    strip.append(b);
    return b;
  };
  if (!project) {
    button('New project…', 'A project holds the edits, the C# code and the scene files', () => newProjectDialog(), true);
    return;
  }
  if (!openff) {
    button('Make it an OpenFF mod…', 'Tick FF3 or FF4 under OpenFF in the project settings: the client plays it, and it may carry code', () => projectSettingsDialog(), true);
    button('Folder', 'The project\'s folder in Explorer', () => revealProject());
    return;
  }
  if (browseKind === 'code') {
    if (!project.code) {
      button('Add C# code', 'Writes code/<Name>.csproj referencing the OpenFF engine, and a starting class', () => addCode(), true);
    } else {
      button('New file…', 'A new C# file under code/: a Behaviour, a GameService, or an empty frame', () => newCodeFileDialog());
      button('Build', 'dotnet build of code/; errors land under the file they are in', () => buildCode());
      button('Open in IDE', 'The C# project in Visual Studio, Rider, VS Code - whatever opens .csproj here', () => openCode());
    }
  } else {
    if (browseKind === 'characters' && typeof newCharacterDialog === 'function') {
      button('New character…', 'A hero slot\'s definition: the name, the starting job and level as a game begins', () => newCharacterDialog(), true);
    }
    if (browseKind === 'items' && typeof newItemDialog === 'function') {
      button('New item…', 'An item of the mod\'s own: starts from one of the game\'s, with a name, a caption, prices and any field of the record changed', () => newItemDialog(), true);
    }
    button('Export to OpenFF', 'Writes the mod - files per game, code, scenes, definitions - into the client\'s mods folder', () => exportToOpenFF());
    if (project.client) button('Run in OpenFF', 'Export and start the client; a running one hot-reloads the code and takes scene changes on the next map', () => runInOpenFF());
  }
  button('Folder', 'The project\'s folder in Explorer', () => revealProject());
}

/// Name and starter for a new C# file; opens it when made.
function newCodeFileDialog() {
  const body = dialog('New C# file');
  const name = field(body, 'Name', '', { placeholder: 'Greeter, or Quests/Fetch - .cs is added' });
  section(body, 'Start from');
  const list = document.createElement('div');
  list.className = 'dialog-list';
  const choices = [
    ['behaviour', 'A Behaviour', 'a script for one map object: Start, Update, public fields Crystal can edit'],
    ['service', 'A GameService', 'one instance for the whole run; hears the game\'s events'],
    ['empty', 'Empty', 'the usings and the namespace, nothing else'],
  ];
  let picked = 'behaviour';
  for (const [id, title, what] of choices) {
    const row = document.createElement('label');
    row.className = 'dialog-game' + (id === picked ? ' checked' : '');
    const radio = document.createElement('input');
    radio.type = 'radio';
    radio.name = 'starter';
    radio.checked = id === picked;
    radio.onchange = () => {
      picked = id;
      $$('.dialog-game', list).forEach(r => r.classList.toggle('checked', r === row));
    };
    const text = document.createElement('div');
    text.className = 'what';
    const b = document.createElement('b');
    b.textContent = title;
    const s = document.createElement('span');
    s.textContent = what;
    text.append(b, s);
    row.append(radio, text);
    list.append(row);
  }
  body.append(list);
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Create';
  go.onclick = async () => {
    if (!name.value.trim()) { problem.textContent = 'The file needs a name.'; name.focus(); return; }
    go.disabled = true;
    try {
      const result = await api('/api/project/file/new', { name: name.value.trim(), template: picked });
      if (!result.ok) throw new Error(result.error);
      body.close();
      await loadList();
      await openDoc('code', result.name);
      say(`made ${result.name}`, 'good');
    } catch (error) {
      problem.textContent = error.message;
      go.disabled = false;
    }
  };
  name.onkeydown = e => { if (e.key === 'Enter') go.click(); };
  actions.append(go);
  body.append(actions);
  name.focus();
}

/// Switches the project panel - and the next thing opened - to another game.
async function selectWorkspace(target) {
  if (target === state.ws) return;
  state.ws = target;
  state.files = [];
  if (typeof resetOps === 'function') resetOps();
  clearInspected();
  await selectKind(browseKind);
  syncHash();
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
  noteSession();
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
  if (saved.tree) root.setProperty('--tree', saved.tree + 'px');
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
      // Capture keeps the drag when the pointer leaves the 4px bar; the listeners sit
      // on the document for the same reason, so a capture refused still drags.
      try { bar.setPointerCapture(event.pointerId); } catch (error) { /* no active pointer */ }
      const move = (e) => {
        const box = $('#workbench').getBoundingClientRect();
        let px;
        if (which === 'left') px = e.clientX - box.left;
        else if (which === 'right') px = box.right - e.clientX;
        else if (which === 'tree') px = e.clientX - $('#project-tree').getBoundingClientRect().left;
        else px = box.bottom - e.clientY;
        // The tree inside the project panel is the narrow one: it holds a dozen words.
        px = which === 'tree'
          ? Math.max(110, Math.min(400, px))
          : Math.max(120, Math.min(which === 'bottom' ? 600 : 640, px));
        document.documentElement.style.setProperty('--' + which, px + 'px');
        saveSize(which, px);
        layoutChanged();
      };
      const up = () => {
        document.removeEventListener('pointermove', move);
        document.removeEventListener('pointerup', up);
      };
      document.addEventListener('pointermove', move);
      document.addEventListener('pointerup', up);
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

/// #/maps/d01_01 with one game open; #/ff4steam/maps/d01_01 with two, so a link says
/// which game it means.
function hashFor(kind, name, ws = state.ws) {
  const game = workspaces().length > 1 && ws ? `/${encodeURIComponent(ws)}` : '';
  return `#${game}/${slugFor(kind)}` + (name ? `/${encodeName(name)}` : '');
}

function readHash() {
  let raw = location.hash.replace(/^#\/?/, '');
  if (!raw) return { kind: null, name: null, ws: null };
  let ws = null;
  const first = raw.indexOf('/');
  const head = first < 0 ? raw : raw.slice(0, first);
  if (workspaces().some(w => w.target === decodeURIComponent(head))) {
    ws = decodeURIComponent(head);
    raw = first < 0 ? '' : raw.slice(first + 1);
  }
  const cut = raw.indexOf('/');
  const slug = cut < 0 ? raw : raw.slice(0, cut);
  const name = cut < 0 ? null : decodeURIComponent(raw.slice(cut + 1));
  return { kind: kindFor(slug), name: name || null, ws };
}

function syncHash() {
  const wanted = activeDoc
    ? hashFor(activeDoc.kind, activeDoc.name, activeDoc.ws)
    : hashFor(browseKind);
  if (location.hash !== wanted) history.replaceState(null, '', wanted);
  noteSession();
}

// ------------------------------------------------------------------ the session
//
// What is open is kept with the project (session.json beside project.json), so opening
// the project again opens the same tabs, focused on the same one, with the panel on the
// same library and game - the work is where it was left rather than behind a start page.
// Previews are not kept: they were a look, not a decision. Saves are debounced and held
// while tabs are being restored or a project is being switched, since either would
// otherwise write a half-state, or an empty one into the wrong project.

let sessionTimer = null;
let sessionHold = false;

function sessionSnapshot() {
  const list = [];
  groups.forEach((group, index) => {
    for (const doc of group.docs) {
      if (doc.preview) continue;
      list.push({ kind: doc.kind, name: doc.name, ws: doc.ws || null, group: index });
    }
  });
  const active = activeDoc && !activeDoc.preview ? { kind: activeDoc.kind, name: activeDoc.name } : null;
  return { docs: list, active, browse: browseKind, ws: state.ws || null };
}

function noteSession() {
  if (sessionHold) return;
  if (typeof projectState === 'undefined' || !projectState.project) return;
  clearTimeout(sessionTimer);
  sessionTimer = setTimeout(() => {
    sessionTimer = null;
    if (sessionHold || !projectState.project) return;
    api('/api/project/session', sessionSnapshot()).catch(() => {});
  }, 600);
}

/// Opens what the project's session.json says was open. True when it opened anything
/// or set the panel; false when there was nothing to go on, so the caller falls back.
async function restoreSession() {
  if (typeof projectState === 'undefined' || !projectState.project) return false;
  let session = null;
  try {
    const result = await api('/api/project/session');
    session = result && result.session;
  } catch (error) {
    return false;
  }
  if (!session) return false;
  const known = workspaces().map(w => w.target);
  const wanted = (session.docs || []).filter(d => d && d.kind && d.name && KINDS.some(k => k.id === d.kind));
  sessionHold = true;
  try {
    // A second group for tabs that sat in one, made when the first of them is opened.
    for (const d of wanted) {
      if (d.ws && known.includes(d.ws)) state.ws = d.ws;
      else if (d.ws && !known.includes(d.ws)) continue; // a game the project no longer targets
      while (groups.length <= Math.min(1, d.group || 0)) makeGroup();
      activeGroup = groups[Math.min(1, d.group || 0)] || groups[0];
      try {
        await openDoc(d.kind, d.name);
      } catch (error) {
        // A file that has gone since is not worth stopping the rest for.
      }
    }
    if (session.active) {
      const doc = docs.get(docId(session.active.kind, session.active.name));
      if (doc) activate(doc.id);
    }
    if (session.ws && known.includes(session.ws) && session.ws !== state.ws && !session.active) {
      state.ws = session.ws;
      state.files = [];
    }
    if (session.browse && KINDS.some(k => k.id === session.browse) && (session.browse !== browseKind || !state.files.length)) {
      await selectKind(session.browse);
    }
  } finally {
    sessionHold = false;
  }
  syncHash();
  return wanted.length > 0 || Boolean(session.browse);
}

async function applyHash() {
  const { kind, name, ws } = readHash();
  const wanted = kind || 'map';

  if (ws && ws !== state.ws) {
    state.ws = ws;
    state.files = [];
  }
  if (wanted !== browseKind || !state.files.length || state.filesWs !== state.ws) {
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

// The project, the targets and whether the edits are installed, in one go. It also
// fills the header, which is true all session and would only be wiped by the next
// thing that happened if say() owned it.
// A project's last session comes back first; the address bar, when it names something,
// then has its say on top of it (a link into a project opens that, beside the rest).
refreshProject()
  .then(async () => {
    // Read before the restore, which writes its own hash for what it focused. A reload
    // carries the hash of the very document the session focuses, and that is not a
    // request for anything more; a link to something else is.
    const asked = location.hash.replace(/^#\/?/, '') ? location.hash : '';
    const restored = await restoreSession();
    if (!restored || (asked && asked !== location.hash)) await applyHash();
  })
  .catch(error => say(error.message, 'bad'));
