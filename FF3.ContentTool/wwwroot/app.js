// The editor's front end.
//
// The server decodes and compiles; this side is the interface. Menus are edited as
// XML in the browser - the DOM already knows how to parse and serialise it - so the
// canvas is a view over the same tree that gets posted back.

'use strict';

const state = { kind: 'map', browse: 'map', files: [], name: null, pane: null };

const $ = (selector, root = document) => root.querySelector(selector);
const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];

async function api(path, body) {
  const response = await fetch(path, body === undefined ? undefined : {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify(body)
  });
  if (!response.ok) {
    const detail = await response.json().catch(() => ({ error: response.statusText }));
    throw new Error(detail.error || response.statusText);
  }
  const result = await response.json();

  // Anything that saved has changed what the project holds, so the Project menu is
  // now out of date - "Install into the game" would stay greyed out until the page
  // was reloaded, which is exactly how this was found. Central rather than in each
  // editor, because there are seven of them and the next one would forget.
  if (path.includes('/save') && result && result.ok !== false) {
    projectChanged();
  }
  return result;
}

/// Marks the project as needing a look, once per turn of the event loop.
///
/// A single save is several calls in some editors, and each one would otherwise
/// re-read the whole override directory.
let projectPending = false;
function projectChanged() {
  if (projectPending || typeof refreshProject !== 'function') return;
  projectPending = true;
  setTimeout(() => {
    projectPending = false;
    if (typeof resetOps === 'function') resetOps();
    refreshProject();
  }, 0);
}

function say(message, tone) {
  const status = $('#status');
  status.textContent = message || '';
  status.className = tone || '';
}

// ------------------------------------------------------------------- file list

async function loadList() {
  if (state.browse === 'image') {
    state.images = await api('/api/images');
    state.files = state.images.map(image => ({
      name: image.name, overridden: image.overridden
    }));
  } else if (state.browse === 'texture') {
    const textures = await api('/api/textures');
    state.textureFormats = textures.formats;
    state.files = textures.packages.map(p => ({ name: p.name, overridden: false, kind: p.kind }));
  } else if (state.browse === 'cell') {
    state.cells = await api('/api/cells');
    state.files = state.cells.map(c => ({ name: c.name, overridden: false }));
  } else if (state.browse === 'model') {
    state.models = await api('/api/models');
    state.files = state.models.map(m => ({ name: m.name, overridden: false }));
  } else if (state.browse === 'map') {
    const maps = await api('/api/maps');
    state.files = maps.map(name => ({ name, overridden: false }));
  } else if (state.browse === 'audio') {
    // Sounds are not archive entries - they are XNBs beside the game - so the list
    // comes from somewhere else and carries more with it.
    state.audio = await api('/api/audio');
    state.files = state.audio.map(sound => ({ name: sound.name, overridden: false }));
  } else {
    state.files = await api(`/api/list?kind=${state.browse}`);
  }
  drawList();
}

function drawList() {
  const filter = $('#filter').value.trim().toLowerCase();
  const list = $('#files');

  // Where you were reading. Emptying a list drops its scroll to the top, and a redraw
  // is nearly always something other than "take me somewhere else" - a file being
  // marked overridden, a thumbnail arriving - so put it back.
  const scrolled = list.scrollTop;
  list.textContent = '';

  // Pictures are only fetched for cells that are actually on screen, and only in the
  // grid - a one-line row is not worth drawing a model for.
  if (state.thumbWatcher) state.thumbWatcher.disconnect();
  state.thumbWatcher = fileView === 'grid' ? watchThumbnails(list, state.browse) : null;

  for (const file of state.files) {
    if (filter && !file.name.toLowerCase().includes(filter)) continue;
    const item = document.createElement('li');
    item.append(icon(state.browse));
    const label = document.createElement('span');
    // The grid has no room for a folder, and in a list the folder is worth keeping.
    // In the grid there is one line to read, and ".nmdp.lz" fills it - the icon
    // already says what kind of thing it is.
    label.textContent = fileView === 'grid'
      ? shortName(file.name).replace(/\.(nmdp\.lz|lz|NCER|NSCR|hich|script|pak|msd|xbn)$/i, '')
      : file.name;
    item.append(label);
    item.title = file.name;
    item.dataset.name = file.name;
    if (state.thumbWatcher) {
      item.dataset.thumbFor = file.name;
      state.thumbWatcher.observe(item);
    }
    const open_ = docs.has(docId(state.browse, file.name));
    const looking = inspected
      && inspected.kind === state.browse && inspected.name === file.name;
    item.className = (file.overridden ? 'overridden ' : '')
      + (open_ ? 'open ' : '')
      + (looking ? 'on' : '');
    // A single click shows it in the inspector without disturbing what is open;
    // a double click is what opens it.
    item.onclick = () => inspectAsset(state.browse, file.name);
    item.ondblclick = () => openDoc(state.browse, file.name);

    // A model can be dragged onto an open map, which is the shortest way from "that
    // one" to "there".
    if (state.browse === 'model') {
      item.draggable = true;
      item.ondragstart = (event) => {
        event.dataTransfer.setData('text/ff3-model', modelNameOf(file.name));
        event.dataTransfer.effectAllowed = 'copy';
      };
    }
    list.append(item);
  }

  list.scrollTop = scrolled;
}

/// Which file is selected, and which are open, without rebuilding the list.
///
/// Clicking a file used to redraw the whole thing, and a redraw starts by emptying it,
/// which throws away where you were scrolled to. The one place a click must never move
/// the view is the thing you have just clicked on.
function markList() {
  for (const item of $('#files').children) {
    const name = item.dataset.name;
    item.classList.toggle('open', docs.has(docId(state.browse, name)));
    item.classList.toggle('on',
      Boolean(inspected) && inspected.kind === state.browse && inspected.name === name);
  }
}

/// The name a .hich row holds, out of the package a model lives in: files/o001.nmdp.lz
/// is the model o001.
function modelNameOf(packageName) {
  return packageName.replace(/^.*\//, '').replace(/\.nmdp\.lz$/i, '');
}

function markOverridden(name, overridden) {
  const file = state.files.find(f => f.name === name);
  if (file) file.overridden = overridden;
  drawList();
  const badge = state.pane && $('.badge.override', state.pane);
  if (badge) badge.classList.toggle('on', overridden);
}

/// Runs the right view for a kind. The shell decides where it lands.
async function dispatchOpen(kind, name) {
  if (kind === 'map') await openMap(name);
  else if (kind === 'script') await openScript(name);
  else if (kind === 'menu') await openMenu(name);
  else if (kind === 'table') await openTable(name);
  else if (kind === 'audio') await openAudio(name);
  else if (kind === 'image') await openImage(name);
  else if (kind === 'texture') await openTexture(name);
  else if (kind === 'model') await openModel(name);
  else if (kind === 'cell') await openCell(name);
  else await openText(name);
}

/// Reloads the document in front of you - what revert and "add character" want.
async function open(name) {
  return openDoc(state.kind, name, true);
}

function view(id, name, overridden) {
  const pane = state.pane || $('#docs');
  pane.textContent = '';
  const node = $(`#view-${id}`).content.cloneNode(true).firstElementChild;
  $('.name', node).textContent = name;
  // Not every view has an override badge or a revert button - audio has neither.
  const badge = $('.badge.override', node);
  if (badge) badge.classList.toggle('on', overridden);
  const revert = $('.revert', node);
  if (revert) {
    revert.onclick = async () => {
      if (!confirm(`Throw away your changes to ${name} and go back to the shipped file?`)) return;
      await api('/api/revert', { name });
      markOverridden(name, false);
      await open(name);
      say('reverted', 'good');
    };
  }
  pane.append(node);
  return node;
}

// ---------------------------------------------------------------------- scripts

async function openScript(name) {
  const data = await api(`/api/script?name=${encodeURIComponent(name)}`);
  setDocData(data);
  const node = view('script', name, data.overridden);
  const text = $('textarea', node);
  const gutter = $('.gutter', node);
  const problems = $('.problems', node);

  text.value = data.source;
  numberLines();

  text.addEventListener('input', numberLines);
  text.addEventListener('scroll', () => { gutter.scrollTop = text.scrollTop; });

  function numberLines() {
    const lines = text.value.split('\n').length;
    gutter.textContent = Array.from({ length: lines }, (_, i) => i + 1).join('\n');
    gutter.scrollTop = text.scrollTop;
  }

  function showProblems(list) {
    problems.textContent = '';
    for (const problem of list) {
      const item = document.createElement('li');
      const where = document.createElement('b');
      where.textContent = `line ${problem.line}`;
      item.append(where, document.createTextNode(problem.message));
      item.onclick = () => goToLine(text, problem.line, problem.column);
      problems.append(item);
    }
  }

  async function compile(save) {
    say(save ? 'compiling and saving…' : 'compiling…');
    const result = await api('/api/script/save', { name, source: text.value, save });
    showProblems(result.problems || []);
    if (result.ok) {
      if (save) markOverridden(name, true);
      say(save ? `saved, ${result.bytes} bytes` : 'compiles cleanly', 'good');
    } else {
      say(`${result.problems.length} problem(s)`, 'bad');
    }
  }

  $('.check', node).onclick = () => compile(false).catch(e => say(e.message, 'bad'));
  $('.save', node).onclick = () => compile(true).catch(e => say(e.message, 'bad'));

  // Highlighting, completion and the signature strip. Defined in script-editor.js;
  // the editor still works without it, just plainer.
  if (typeof enhanceScriptEditor === 'function') {
    await enhanceScriptEditor(node);
  }
}

function goToLine(text, line, column) {
  const lines = text.value.split('\n');
  let at = 0;
  for (let i = 0; i < line - 1 && i < lines.length; i++) at += lines[i].length + 1;
  at += Math.max(0, (column || 1) - 1);
  text.focus();
  text.setSelectionRange(at, at);
  // Put the line roughly in the middle rather than at the very bottom.
  const lineHeight = text.scrollHeight / Math.max(1, lines.length);
  text.scrollTop = Math.max(0, (line - 1) * lineHeight - text.clientHeight / 2);
}

// ------------------------------------------------------------------------ menus

const menu = {
  doc: null, screens: [], screen: null, selected: null, name: null,
  zoom: 2, preview: false, messages: {},
  xmlOpen: true, xmlHeight: 260
};

async function openMenu(name) {
  const data = await api(`/api/menu?name=${encodeURIComponent(name)}`);
  const node = view('menu', name, data.overridden);

  menu.name = name;
  menu.doc = new DOMParser().parseFromString(data.xml, 'application/xml');
  menu.selected = null;

  const error = menu.doc.querySelector('parsererror');
  if (error) throw new Error('the menu did not parse as XML');

  const found = [...menu.doc.documentElement.children].filter(e => e.tagName === 'menu');
  menu.screens = found.length ? found : [menu.doc.documentElement];

  const picker = $('.screens', node);
  picker.textContent = '';
  menu.screens.forEach((screen, index) => {
    const option = document.createElement('option');
    option.value = index;
    option.textContent = childText(screen, 'name') || `screen ${index}`;
    picker.append(option);
  });
  picker.onchange = () => {
    menu.selected = null;
    redraw(node);
  };

  const zoom = $('.zoom', node);
  zoom.value = String(menu.zoom);
  zoom.onchange = () => { menu.zoom = Number(zoom.value); redraw(node); };

  const preview = $('.preview', node);
  preview.checked = menu.preview;
  preview.onchange = async () => {
    menu.preview = preview.checked;
    if (menu.preview) await loadMessages(node);
    redraw(node);
  };

  // The XML panel under the canvas. It shows whichever thing is selected - one widget
  // if there is one, the whole file if there is not - so there is no XML mode to enter
  // and leave, and the canvas stays visible either way. Clicking an empty spot selects
  // nothing, which is how you get back to the whole file.
  const canvasWrap = $('.canvas-wrap', node);
  applyXmlHeight(node);

  $('.xml-collapse', node).onclick = () => {
    menu.xmlOpen = !menu.xmlOpen;
    applyXmlHeight(node);
    refreshXmlPanel(node);
  };

  $('.xml-grip', node).onpointerdown = event => {
    if (!menu.xmlOpen) return;
    event.preventDefault();
    const startY = event.clientY;
    const startHeight = menu.xmlHeight;
    // On the window rather than the grip: the pointer leaves a 6px strip immediately.
    const move = e => {
      menu.xmlHeight = Math.max(90, Math.min(760, startHeight + (startY - e.clientY)));
      applyXmlHeight(node);
    };
    const up = () => {
      window.removeEventListener('pointermove', move);
      window.removeEventListener('pointerup', up);
    };
    window.addEventListener('pointermove', move);
    window.addEventListener('pointerup', up);
  };

  $('.apply', node).onclick = () => applyXml(node);

  // Anywhere in the canvas area that is not a widget. Selecting nothing is a real
  // choice here rather than just the absence of one - it is what puts the whole file
  // in the panel, and it is the only way to stop the arrow keys nudging something.
  canvasWrap.onpointerdown = event => {
    if (event.target.closest('.widget')) return;
    if (!menu.selected) return;
    menu.selected = null;
    $$('.widget', node).forEach(w => w.classList.remove('on'));
    showProperties(node, null);
  };

  $('.save', node).onclick = async () => {
    const xml = new XMLSerializer().serializeToString(menu.doc);
    const result = await api('/api/menu/save', { name, xml });
    if (result.ok) {
      markOverridden(name, true);
      say(`saved, ${result.bytes} bytes`, 'good');
    } else {
      say(result.error, 'bad');
    }
  };

  $('.duplicate', node).onclick = () => {
    if (!menu.selected) return say('select a widget first', 'bad');
    const copy = menu.selected.cloneNode(true);
    setChildText(copy, 'id', (childText(copy, 'id') || 'widget') + '_copy');
    setChildText(copy, 'y', String(number(childText(copy, 'y')) + 8));
    menu.selected.after(copy);
    menu.selected = copy;
    redraw(node);
    say('duplicated - it needs a unique id', 'good');
  };

  $('.remove', node).onclick = () => {
    if (!menu.selected) return say('select a widget first', 'bad');
    if (!confirm('Delete this widget and everything nested in it?')) return;
    menu.selected.remove();
    menu.selected = null;
    redraw(node);
  };

  if (menu.preview) await loadMessages(node);
  redraw(node);
}

/// Redraws whichever screen the picker is on, keeping the selection.
// What is behind a screen, once, because it is the same for every redraw.
const menuBackgrounds = new Map();

/// Draws the panel a screen sits on, behind its widgets.
///
/// The background is a cell bank rather than a picture - the parts are blitted from a
/// sheet every menu shares, menu_bg_01.NCGR, which is why looking for a sheet of the
/// same name found nothing. Its parts are laid out around an origin at the centre of an
/// 800 by 480 screen, and a .xbn lays out in 480 by 320 from the top left, so the whole
/// thing is moved and scaled by the same numbers the font uses.
async function drawMenuBackground(node, screenName) {
  const canvas = $('.canvas', node);
  const existing = $('.menu-bg', canvas);
  if (existing) existing.remove();
  if (!menu.preview || !screenName) return;

  let entry = menuBackgrounds.get(screenName);
  if (entry === undefined) {
    try {
      const found = await api(
        `/api/menu/background?screen=${encodeURIComponent(screenName)}`);
      entry = found.bank || null;
    } catch (error) {
      entry = null;
    }
    menuBackgrounds.set(screenName, entry);
  }
  if (!entry) return;

  let bank = menuBackgrounds.get('bank:' + entry);
  if (!bank) {
    try {
      bank = await api(`/api/cell?name=${encodeURIComponent(entry)}`);
    } catch (error) {
      return;
    }
    menuBackgrounds.set('bank:' + entry, bank);
  }
  if (!bank.cells || !bank.cells.length) return;

  const sheet = new Image();
  await new Promise((resolve) => {
    sheet.onload = resolve;
    sheet.onerror = resolve;
    sheet.src = `/api/image?name=${encodeURIComponent(bank.sheet)}`;
  });
  if (!sheet.width) return;

  const drawn = drawCell(bank.cells[0], sheet, true, false);
  const box = cellBounds(bank.cells[0], true);
  drawn.className = 'menu-bg';
  // The parts are laid out for an 800x480 screen and carry the squash flag, which is
  // 0.6 across and 2/3 down - the same conversion the font needs, and drawCell has
  // already applied it. So these are menu units, centred on the middle of a 480x320
  // screen, and all that is left is moving the origin to the corner. Scaling here as
  // well put the panels at half size in the wrong place.
  drawn.style.left = `${box.x + 240}px`;
  drawn.style.top = `${box.y + 160}px`;
  drawn.style.width = `${box.width}px`;
  drawn.style.height = `${box.height}px`;
  canvas.prepend(drawn);
}

/// Puts the split at the height it is set to, or shuts it down to its own bar.
function applyXmlHeight(node) {
  const split = $('.xml-split', node);
  if (!split) return;
  split.classList.toggle('shut', !menu.xmlOpen);
  split.style.height = menu.xmlOpen ? `${menu.xmlHeight}px` : '';
  const button = $('.xml-collapse', node);
  button.textContent = menu.xmlOpen ? '\u25be' : '\u25b4';
  button.title = menu.xmlOpen ? 'collapse' : 'expand';
}

/// Fills the panel from whatever is selected.
///
/// Never while it is being typed in - this runs on every redraw, and a redraw happens
/// while a widget is being dragged, so reserialising under the cursor would throw the
/// edit away mid-word.
function refreshXmlPanel(node) {
  const area = $('.xml', node);
  if (!area || !menu.xmlOpen || document.activeElement === area) return;
  const target = menu.selected || menu.doc;
  $('.xml-what', node).textContent = menu.selected
    ? (childText(menu.selected, 'id') || 'this widget')
    : 'the whole file';
  area.value = formatXml(new XMLSerializer().serializeToString(target));
}

/// Applies what is in the panel back to whatever it was showing.
function applyXml(node) {
  const area = $('.xml', node);
  try {
    const parsed = new DOMParser().parseFromString(area.value, 'application/xml');
    if (parsed.querySelector('parsererror')) throw new Error('that is not valid XML');
    if (menu.selected) {
      // importNode, because the parse produced a node belonging to another document.
      const replacement = menu.doc.importNode(parsed.documentElement, true);
      menu.selected.replaceWith(replacement);
      menu.selected = replacement;
      redraw(node);
      say('widget replaced - press Save to write it', 'good');
    } else {
      menu.doc = parsed;
      const found = [...menu.doc.documentElement.children].filter(e => e.tagName === 'menu');
      menu.screens = found.length ? found : [menu.doc.documentElement];
      redraw(node);
      say('applied - press Save to write it', 'good');
    }
  } catch (error) {
    say(error.message, 'bad');
  }
}

function redraw(node) {
  const picker = $('.screens', node);
  drawScreen(node, menu.screens[picker.value || 0], menu.selected);
}

/// The message behind every Text widget in the file, for the preview.
async function loadMessages(node) {
  const ids = new Set();
  for (const screen of menu.screens) {
    for (const frame of collectFrames(screen)) {
      const id = textMessageId(frame.element);
      if (id !== null) ids.add(id);
    }
  }
  if (!ids.size) return;

  const result = await api('/api/messages', { ids: [...ids] });
  menu.messages = result.messages || {};
  if (!result.available) {
    say('start the editor with --text=<decoded msd dir> to see the real labels', 'bad');
  }
}

/// Which of the two fonts a Text widget uses, from its second parameter.
///
/// Not the widget's height, which is what this guessed at first - a 56 tall button
/// would then have taken the large font and drawn "Equipment" at 72% of its box
/// instead of 51%. MBText reads it as a threshold:
///
///   if (list[1].nodeValueInt() > 8) flagOn(4); else flagOff(4);
///   int font = 1; if (flagCheck(4)) font = 0;
///
/// so over 8 picks font 0 and anything else font 1 - the large and small faces, 16
/// and 12. Every command in the main menu passes 8, which is why they are small.
function textFontSize(element) {
  const behavior = [...element.children].find(e => e.tagName === 'behavior');
  if (!behavior) return 12;
  const parameters = [...behavior.children].filter(e => e.tagName === 'parameter');
  if (parameters.length < 2) return 12;
  const value = parseInt(
    parameters[1].getAttribute('value') ?? parameters[1].textContent, 10);
  return !Number.isNaN(value) && value > 8 ? 16 : 12;
}

/// How MBText places its string in the widget, from the third parameter:
/// 0 left, 1 right, 2 centre, 3 flexible, 4 button, 5 menu - and button and menu
/// centre the same way centre does.
function textAlignment(element) {
  const behavior = [...element.children].find(e => e.tagName === 'behavior');
  if (!behavior) return 0;
  const parameters = [...behavior.children].filter(e => e.tagName === 'parameter');
  if (parameters.length < 3) return 0;
  const value = parseInt(
    parameters[2].getAttribute('value') ?? parameters[2].textContent, 10);
  return Number.isNaN(value) ? 0 : value;
}

/// A Text widget's first parameter is the message id it draws. A negative one means
/// the widget is filled in at runtime - a party member's name, for instance.
function textMessageId(element) {
  const behavior = [...element.children].find(e => e.tagName === 'behavior');
  if (!behavior || (behavior.getAttribute('value') ?? behavior.textContent) !== 'Text') return null;
  const parameter = [...behavior.children].find(e => e.tagName === 'parameter');
  if (!parameter) return null;
  const id = parseInt(parameter.getAttribute('value') ?? parameter.textContent, 10);
  return Number.isNaN(id) ? null : id;
}

/// Indentation for the XML view - the serialiser hands back one long line.
function formatXml(xml) {
  const NL = String.fromCharCode(10);
  const parts = xml.replace(/></g, '>' + NL + '<').split(NL);
  let depth = 0;
  return parts.map(part => {
    const closing = part.startsWith('</');
    if (closing) depth--;
    const line = '  '.repeat(Math.max(0, depth)) + part;
    const selfContained = part.endsWith('/>') || part.indexOf('</') > 0;
    if (!closing && part.startsWith('<') && !part.startsWith('<?') && !selfContained) depth++;
    return line;
  }).join(NL);
}

function childText(element, tag) {
  const child = [...element.children].find(e => e.tagName === tag);
  if (!child) return null;
  return child.getAttribute('value') ?? child.textContent;
}

function setChildText(element, tag, value) {
  let child = [...element.children].find(e => e.tagName === tag);
  if (!child) {
    child = element.ownerDocument.createElement(tag);
    element.prepend(child);
  }
  if (child.hasAttribute('value')) child.setAttribute('value', value);
  else child.textContent = value;
}

const number = value => {
  const parsed = parseInt(value, 10);
  return Number.isNaN(parsed) ? 0 : parsed;
};

/// Every frame in a screen, with the absolute position the game would draw it at.
function collectFrames(screen) {
  const frames = [];
  const walk = (element, parentX, parentY) => {
    for (const child of element.children) {
      if (child.tagName !== 'frame') continue;
      const x = parentX + number(childText(child, 'x'));
      const y = parentY + number(childText(child, 'y'));
      frames.push({
        element: child,
        x, y,
        width: number(childText(child, 'width')),
        height: number(childText(child, 'height')),
        id: childText(child, 'id'),
        behavior: childText(child, 'behavior')
      });
      walk(child, x, y);
    }
  };
  walk(screen, 0, 0);
  return frames;
}

function drawScreen(node, screen, select) {
  const canvas = $('.canvas', node);
  canvas.textContent = '';
  if (!screen) return;
  const frames = collectFrames(screen);

  const width = Math.max(256, ...frames.map(f => f.x + Math.max(f.width, 8))) + 16;
  const height = Math.max(192, ...frames.map(f => f.y + Math.max(f.height, 8))) + 16;
  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;
  canvas.style.transform = `scale(${menu.zoom})`;
  canvas.classList.toggle('preview', menu.preview);

  // The scaled canvas still has its unscaled size as far as layout is concerned,
  // so the wrapper carries the real one and scrolling works.
  const scale = $('.canvas-scale', node);
  scale.style.width = `${width * menu.zoom}px`;
  scale.style.height = `${height * menu.zoom}px`;

  drawMenuBackground(node, childText(screen, 'name'));

  for (const frame of frames) {
    const box = document.createElement('div');
    box.className = 'widget';
    box.style.left = `${frame.x}px`;
    box.style.top = `${frame.y}px`;
    box.style.width = `${Math.max(frame.width, 8)}px`;
    box.style.height = `${Math.max(frame.height, 8)}px`;
    box.title = `${frame.id || '(no id)'}  ${frame.width}x${frame.height}`
      + (frame.behavior ? `  ${frame.behavior}` : '');

    // Alignment 4 is the one kind of box that belongs to the widget rather than to
    // the background art, so it is the one that follows a resize. Drawing it is the
    // only way to see that without running the game.
    if (menu.preview && textMessageId(frame.element) !== null
        && textAlignment(frame.element) === 4) {
      drawButtonWindow(frame.width, frame.height, false)
        .then(window => { if (window && box.isConnected) box.prepend(window); })
        .catch(() => {});
    }

    const label = document.createElement('span');
    label.className = 'label';
    if (menu.preview) {
      const id = textMessageId(frame.element);
      const text = id === null ? null : menu.messages[id];
      if (text != null) {
        label.textContent = text;
        // In the game's own font, once it arrives. The text stays as a fallback, so a
        // font that will not load leaves a readable preview rather than an empty one.
        drawFontText(text, textFontSize(frame.element), '#f4f4f4')
          .then(canvas => {
            if (!canvas || !label.isConnected) return;
            label.textContent = '';
            // Where MBText would put it. Across, that is the alignment: left,
            // right, or centred in the declared width. Down, it is the same for
            // every alignment - mbtSetAlignment centres the text in the widget's
            // height whenever that height is set at all:
            //
            //   if (height > 0) num1 = (height - textHeight) / 2;
            //
            // which is why the commands looked pinned to the top of their buttons.
            // The menu's commands are 56 tall and the text is 12, so they belong
            // 22 units down.
            const align = textAlignment(frame.element);
            const room = frame.width - canvas.width;
            if (align === 1) canvas.style.marginLeft = `${Math.round(room)}px`;
            else if (align !== 0) canvas.style.marginLeft = `${Math.round(room / 2)}px`;

            // StringHeight returns the size itself, so that is the game's own
            // answer for how tall a line is.
            const size = textFontSize(frame.element);
            if (frame.height > 0) {
              canvas.style.marginTop = `${Math.round((frame.height - size) / 2)}px`;
            }
            label.append(canvas);
          })
          .catch(() => {});
      } else if (id !== null && id >= 0) {
        // A message this file's language does not have - naming the id is more
        // use than naming the behaviour, because the id is what to go and look up.
        label.textContent = `«msg ${id}»`;
        label.classList.add('dynamic');
      } else if (frame.behavior) {
        // Drawn from the game's own state - a gold total, an item list - so
        // there is nothing to show but what will fill it.
        label.textContent = `«${frame.behavior}»`;
        label.classList.add('dynamic');
      }
    } else {
      label.textContent = frame.id || frame.behavior || '';
    }
    box.append(label);

    box.onpointerdown = event => startDrag(event, node, screen, frame, box);
    canvas.append(box);

    if (select && frame.element === select) selectFrame(node, screen, frame, box);
  }

  if (!select) showProperties(node, null);
}

function startDrag(event, node, screen, frame, box) {
  event.preventDefault();
  box.setPointerCapture(event.pointerId);
  selectFrame(node, screen, frame, box);

  const startX = event.clientX;
  const startY = event.clientY;
  const originX = number(childText(frame.element, 'x'));
  const originY = number(childText(frame.element, 'y'));
  box.classList.add('dragging');

  const move = moveEvent => {
    const dx = Math.round((moveEvent.clientX - startX) / menu.zoom);
    const dy = Math.round((moveEvent.clientY - startY) / menu.zoom);
    setChildText(frame.element, 'x', String(originX + dx));
    setChildText(frame.element, 'y', String(originY + dy));
    box.style.left = `${frame.x + dx}px`;
    box.style.top = `${frame.y + dy}px`;
    showProperties(node, frame, screen);
  };

  const up = () => {
    box.classList.remove('dragging');
    box.removeEventListener('pointermove', move);
    box.removeEventListener('pointerup', up);
    drawScreen(node, screen, frame.element);
  };

  box.addEventListener('pointermove', move);
  box.addEventListener('pointerup', up);
}

function selectFrame(node, screen, frame, box) {
  $$('.widget', node).forEach(w => w.classList.remove('on'));
  box.classList.add('on');
  menu.selected = frame.element;
  showProperties(node, frame, screen);
}

function showProperties(node, frame, screen) {
  if (!activeDoc) return;

  if (!frame) {
    activeDoc.selection = null;
    activeDoc.inspect = null;
    drawInspector();
    refreshXmlPanel(node);
    return;
  }

  // Held on the document so the inspector can rebuild it whenever it redraws -
  // the panel is shared now, and it is redrawn for reasons this view knows nothing
  // about.
  activeDoc.menuFrame = { frame, screen, node };
  activeDoc.selection = 'widget:' + (frame.id || '?');
  activeDoc.inspect = () => buildWidget(activeDoc.menuFrame);
  drawInspector();
  refreshXmlPanel(node);
}

/// One widget's properties, for the inspector.
function buildWidget(held) {
  const panel = document.createElement('div');
  if (!held) return panel;
  const { frame, screen, node } = held;

  const title = document.createElement('h2');
  title.textContent = frame.id || '(no id)';
  panel.append(title);

  const field = (label, tag) => {
    const wrap = document.createElement('label');
    wrap.textContent = label;
    const input = document.createElement('input');
    input.value = childText(frame.element, tag) ?? '';
    input.oninput = () => {
      setChildText(frame.element, tag, input.value);
      drawScreen(node, screen, frame.element);
    };
    wrap.append(input);
    panel.append(wrap);
  };

  field('id', 'id');
  field('x (relative to its parent)', 'x');
  field('y', 'y');
  field('width', 'width');
  field('height', 'height');
  field('up', 'up');
  field('down', 'down');
  field('left', 'left');
  field('right', 'right');

  // The XML itself is in the panel under the canvas, where it has room to be read.
  return panel;
}

document.addEventListener('keydown', event => {
  if (state.kind !== 'menu' || !menu.selected) return;
  if (event.target.matches('input, textarea')) return;
  // The arrow keys belong to whichever panel is being used. Nudging a widget while
  // somebody is walking the file list is not what they asked for.
  if (event.target.closest && event.target.closest('#project')) return;
  const step = event.shiftKey ? 8 : 1;
  const moves = { ArrowLeft: [-step, 0], ArrowRight: [step, 0], ArrowUp: [0, -step], ArrowDown: [0, step] };
  const move = moves[event.key];
  if (!move) return;
  event.preventDefault();
  setChildText(menu.selected, 'x', String(number(childText(menu.selected, 'x')) + move[0]));
  setChildText(menu.selected, 'y', String(number(childText(menu.selected, 'y')) + move[1]));
  redraw($('.view'));
});

// ------------------------------------------------------------------------- text

/// Makes a box exactly as tall as what is in it.
///
/// The row count only counts newlines, and a line longer than the box wraps into
/// several - so a message sized from its newlines alone comes out too short as soon as
/// the panel is anything but narrow.
function fitToText(area) {
  if (!area.isConnected || !area.clientWidth) return;
  area.style.height = 'auto';
  area.style.height = `${Math.max(30, area.scrollHeight)}px`;
}

async function openText(name) {
  const data = await api(`/api/text?name=${encodeURIComponent(name)}`);
  const node = view('text', name, data.overridden);
  const list = $('.messages', node);
  const messages = data.messages;

  // A file can hold the same id more than once, and 73 of the 210 English ones do -
  // 545 rows between them, every one word for word the same as the first. getMessage
  // scans from the start and returns the first hit, so only the first is ever read.
  // Editing one of the others changes a file and nothing else.
  const seenIds = new Set();

  for (const message of messages) {
    const row = document.createElement('div');
    row.className = 'message';
    // So a line quoted somewhere else can be opened where it is written.
    row.dataset.messageId = message.id;

    const repeat = seenIds.has(message.id);
    seenIds.add(message.id);
    if (repeat) row.classList.add('dead');

    const id = document.createElement('div');
    id.className = 'id';
    id.textContent = message.id;
    if (repeat) {
      const note = document.createElement('em');
      note.className = 'dead-note';
      note.textContent = 'never read';
      note.title = 'This id appears earlier in the file, and the game takes the first '
        + 'one it finds. Editing this copy changes nothing in the game.';
      id.append(note);
    }
    if (message.encoding) {
      const note = document.createElement('em');
      note.textContent = message.encoding;
      note.title = 'this message is not valid UTF-8 and is held byte for byte';
      id.append(note);
    }

    const pages = document.createElement('div');
    message.pages.forEach((page, index) => {
      const area = document.createElement('textarea');
      area.value = page;
      area.rows = Math.max(1, page.split('\n').length);
      area.oninput = () => {
        message.pages[index] = area.value;
        fitToText(area);
      };
      pages.append(area);
      // Once it is in the page and has a width, it can be made as tall as its text -
      // which is not the number of newlines, because a long line wraps.
      requestAnimationFrame(() => fitToText(area));
    });

    row.append(id, pages);
    list.append(row);
  }

  $('.save', node).onclick = async () => {
    const result = await api('/api/text/save', { name, messages });
    markOverridden(name, true);
    say(`saved, ${result.bytes} bytes`, 'good');
  };
}

// ----------------------------------------------------------------------- tables

async function openTable(name) {
  const data = await api(`/api/table?name=${encodeURIComponent(name)}`);
  const node = view('table', name, data.overridden);
  const file = data.file;

  if (!data.family) {
    say('this file has no known record layout - see Docs/Tables.md', 'bad');
  }

  const picker = $('.chains', node);
  const decoded = file.chains.filter(chain => chain.records);
  picker.textContent = '';
  for (const chain of decoded) {
    const option = document.createElement('option');
    option.value = chain.index;
    option.textContent = `${chain.label} (${chain.records.length})`;
    picker.append(option);
  }

  const note = $('.note', node);
  const showNote = () => {
    const chain = decoded.find(c => c.index === Number(picker.value));
    const text = chain && data.notes ? data.notes[chain.label] : null;
    note.textContent = text || '';
    note.classList.toggle('on', Boolean(text));
  };

  const showPadding = $('.pads input', node);
  const rowFilter = $('.rowfilter', node);
  const draw = () => drawTable(node, decoded.find(c => c.index === Number(picker.value)),
    showPadding.checked, rowFilter.value.trim().toLowerCase());

  picker.onchange = () => { showNote(); draw(); };
  showPadding.onchange = draw;
  rowFilter.oninput = draw;

  $('.save', node).onclick = async () => {
    const result = await api('/api/table/save', { name, file });
    markOverridden(name, true);
    say(`saved, ${result.bytes} bytes`, 'good');
  };

  if (decoded.length) { showNote(); draw(); }
  else say('nothing in this file has a known layout', 'bad');
}

/// Padding fields are real - they round trip - but they are noise while editing.
const isPadding = field => /^_pad|^unnamed/.test(field);

/// The text annotations the server writes in are read only: the id is what is saved.
const isAnnotation = field => field.endsWith('Text');

function drawTable(node, chain, withPadding, filter) {
  const table = $('.grid', node);
  table.textContent = '';
  if (!chain) return;

  const fields = Object.keys(chain.records[0] || {})
    .filter(f => withPadding || !isPadding(f));

  const head = document.createElement('tr');
  const corner = document.createElement('th');
  corner.className = 'row';
  corner.textContent = '#';
  head.append(corner);
  for (const field of fields) {
    const cell = document.createElement('th');
    cell.textContent = field;
    head.append(cell);
  }
  table.append(head);

  chain.records.forEach((record, index) => {
    if (filter && !JSON.stringify(record).toLowerCase().includes(filter)) return;

    const row = document.createElement('tr');
    const number = document.createElement('td');
    number.className = 'row';
    number.textContent = index;
    row.append(number);

    for (const field of fields) {
      const cell = document.createElement('td');
      const value = record[field];

      if (Array.isArray(value)) {
        // An array field is edited as a comma separated list, which keeps the
        // grid one cell per field however long the array is.
        const input = document.createElement('input');
        input.value = value.join(', ');
        input.oninput = () => {
          const parts = input.value.split(',').map(p => Number(p.trim()));
          if (parts.length === value.length && parts.every(n => Number.isFinite(n))) {
            record[field] = parts;
            input.style.color = '';
          } else {
            input.style.color = 'var(--bad)';
          }
        };
        cell.append(input);
      } else {
        const input = document.createElement('input');
        input.value = value ?? '';
        if (isAnnotation(field)) {
          cell.className = 'text';
          input.readOnly = true;
          input.title = 'from the message this id points at - edit it under Text';
        } else {
          input.oninput = () => {
            const parsed = Number(input.value);
            record[field] = Number.isFinite(parsed) ? parsed : input.value;
          };
        }
        cell.append(input);
      }
      row.append(cell);
    }
    table.append(row);
  });
}

// ----------------------------------------------------------------------- images

async function openImage(name) {
  const image = state.images.find(i => i.name === name);
  setDocData(image);
  const node = view('image', name, image.overridden);

  const picture = $('.picture', node);
  const wrap = $('.image-wrap', node);
  // Cache-bust, so a replaced picture is the one that shows.
  picture.src = `/api/image?name=${encodeURIComponent(name)}&t=${Date.now()}`;

  $('.facts', node).textContent = image.problem
    ? image.problem
    : `${image.width} × ${image.height}  ·  ${image.colour}, ${image.depth}-bit  ·  `
      + `${(image.bytes / 1024).toFixed(1)} kB`;

  const checker = $('.checker', node);
  const paint = () => wrap.classList.toggle('checker', checker.checked);
  checker.onchange = paint;
  paint();

  const picker = $('.picker', node);
  $('.replace', node).onclick = () => picker.click();
  picker.onchange = async () => {
    const file = picker.files[0];
    if (!file) return;
    say('uploading…');
    const response = await fetch(`/api/image/upload?name=${encodeURIComponent(name)}`, {
      method: 'POST',
      body: file
    });
    const result = await response.json();
    if (!result.ok) {
      say(result.error, 'bad');
      return;
    }
    markOverridden(name, true);
    picture.src = `/api/image?name=${encodeURIComponent(name)}&t=${Date.now()}`;
    say(result.resized
      ? `replaced, but it is ${result.width}×${result.height} and the original was `
        + `${result.wasWidth}×${result.wasHeight} - the game may expect the old size`
      : `replaced, ${result.width}×${result.height}`,
      result.resized ? 'bad' : 'good');
  };
}

// --------------------------------------------------------------------- textures

// A package holds anything from one texture to a dozen, so this shows the lot as a
// gallery and puts the details of whichever you click beside it. Each thumbnail is a
// separate decode on the server, which is why they are <img> tags rather than one
// sheet - the browser asks for them as it draws them.

async function openTexture(name) {
  const node = view('texture', name, false);
  const gallery = $('.gallery', node);
  const facts = $('.facts', node);

  const textures = await api(`/api/texture?name=${encodeURIComponent(name)}`);
  if (textures.error) {
    facts.textContent = textures.error;
    gallery.textContent = '';
    return;
  }
  if (!textures.length) {
    facts.textContent = 'geometry only';
    gallery.textContent = '';
    const empty = document.createElement('p');
    empty.className = 'empty';
    empty.textContent = 'This package has no textures of its own - it is a model that '
      + 'uses a shared .ntxp. 463 of the 833 models are like this.';
    gallery.append(empty);

    // The textures are almost always in the .ntxp of the same name, so say which
    // one and take you there rather than leaving you to guess.
    const sibling = name.replace(/\.nmdp\.lz$/, '.ntxp.lz');
    if (sibling !== name && state.files.some(f => f.name === sibling)) {
      const link = document.createElement('button');
      link.textContent = `Open ${sibling}`;
      link.onclick = () => open(sibling);
      empty.after(link);
    }
    return;
  }

  facts.textContent = textures.length === 1 ? '1 texture' : `${textures.length} textures`;

  const checker = $('.checker', node);
  const paint = () => gallery.classList.toggle('checker', checker.checked);
  checker.onchange = paint;
  paint();

  let chosen = null;
  for (const texture of textures) {
    const cell = document.createElement('figure');
    cell.className = 'cell';

    const picture = document.createElement('img');
    picture.loading = 'lazy';
    picture.alt = texture.name;
    picture.src = `/api/texture/png?name=${encodeURIComponent(name)}&index=${texture.index}`;
    const caption = document.createElement('figcaption');
    caption.textContent = texture.name;
    cell.append(picture, caption);

    cell.onclick = () => {
      if (chosen) chosen.classList.remove('on');
      chosen = cell;
      cell.classList.add('on');
      if (!activeDoc) return;
      activeDoc.selection = `texture:${texture.index}`;
      activeDoc.inspect = () => buildTexture(name, texture);
      drawInspector();
    };
    gallery.append(cell);
  }

  gallery.firstElementChild.onclick();
}

function buildTexture(packageName, texture) {
  const detail = document.createElement('div');

  const big = document.createElement('img');
  big.className = 'big';
  big.src = `/api/texture/png?name=${encodeURIComponent(packageName)}&index=${texture.index}`;
  detail.append(big);

  const facts = document.createElement('dl');
  const fact = (label, value) => {
    if (value === null || value === undefined || value === '') return;
    const dt = document.createElement('dt');
    dt.textContent = label;
    const dd = document.createElement('dd');
    dd.textContent = value;
    facts.append(dt, dd);
  };

  fact('name', texture.name);
  fact('size', `${texture.width} × ${texture.height}`);
  fact('format', texture.format);
  fact('what that means', (state.textureFormats || {})[texture.format]);
  fact('palette', texture.palette);
  fact('problem', texture.problem);
  detail.append(facts);

  const save = document.createElement('a');
  save.className = 'button';
  save.textContent = 'Save as PNG';
  save.href = big.src;
  save.download = `${packageName.replace(/\.lz$/, '')}.${texture.name}.png`;
  detail.append(save);

  const note = document.createElement('p');
  // Not .note - that class is already a hidden-until-toggled banner elsewhere.
  note.className = 'caveat';
  note.textContent = 'Read-only for now. Putting a texture back means writing a TEX0 - '
    + 're-quantising to a palette, or to 4×4 blocks - which is a bigger job than reading one.';
  detail.append(note);
  return detail;
}

// ------------------------------------------------------------------------ cells

// A cell bank is a cutting plan: each part copies a rectangle out of a sheet to a
// position. Composing one is a canvas drawImage per part, with the same four flags the
// game's own draw call applies - flip across, flip down, half size, and the 0.6 x 2/3
// squash that nearly every part carries.

async function openCell(name) {
  const node = view('cell', name, false);
  const list = $('.cell-list', node);
  const facts = $('.facts', node);

  const bank = await api(`/api/cell?name=${encodeURIComponent(name)}`);
  setDocData(bank);
  if (bank.error) {
    facts.textContent = bank.error;
    return;
  }

  if (bank.kind === 'screen') { showScreen(bank, facts, list); return; }
  if (bank.kind === 'animation') { showAnimation(bank, facts, list); return; }

  const parts = (bank.cells || []).reduce((n, c) => n + c.parts.length, 0);
  facts.textContent = `${bank.cells.length} cell(s), ${parts} part(s)`
    + (bank.sheet ? `  \u00b7  from ${bank.sheet} (${bank.sheetFrom})` : '');

  if (!bank.sheet) {
    const problem = document.createElement('p');
    problem.className = 'empty';
    problem.textContent = bank.problem || 'no sheet for this bank';
    list.append(problem);
    return;
  }

  const sheet = new Image();
  sheet.src = `/api/image?name=${encodeURIComponent(bank.sheet)}`;
  await new Promise(done => { sheet.onload = done; sheet.onerror = done; });

  const squash = $('.squash', node);
  const outlines = $('.outlines', node);
  const paint = () => {
    list.textContent = '';
    for (const cell of bank.cells) {
      const figure = document.createElement('figure');
      figure.className = 'cell-cell';
      const canvas = drawCell(cell, sheet, squash.checked, outlines.checked);
      const caption = document.createElement('figcaption');
      caption.textContent = `cell ${cell.index} \u00b7 ${cell.parts.length} part(s)`;
      figure.append(canvas, caption);
      figure.onclick = () => {
        if (!activeDoc) return;
        activeDoc.selection = `cell:${cell.index}`;
        activeDoc.inspect = () => buildCellParts(cell, bank);
        drawInspector();
      };
      list.append(figure);
    }
    if (bank.cells.length) list.firstElementChild.onclick();
  };
  squash.onchange = paint;
  outlines.onchange = paint;
  paint();
}

// The game multiplies by 0.6 across and 2/3 down when flag 8 is set, and halves the
// whole part when flag 4 is. Both apply to the destination only - the source rectangle
// is always the stored one.
function drawCell(cell, sheet, applySquash, showOutlines) {
  const box = cellBounds(cell, applySquash);
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.ceil(box.width));
  canvas.height = Math.max(1, Math.ceil(box.height));
  const g = canvas.getContext('2d');
  g.imageSmoothingEnabled = false;

  for (const part of cell.parts) {
    const [dx, dy, dw, dh] = partRect(part, applySquash);
    g.save();
    g.translate(dx - box.x, dy - box.y);
    if (part.flipX) { g.translate(dw, 0); g.scale(-1, 1); }
    if (part.flipY) { g.translate(0, dh); g.scale(1, -1); }
    try {
      g.drawImage(sheet, part.sourceX, part.sourceY, part.width, part.height, 0, 0, dw, dh);
    } catch (error) {
      // A part can ask for a rectangle bigger than the sheet - 15 in the game do,
      // where the art was replaced at a smaller size and the table left alone.
    }
    if (showOutlines) {
      g.strokeStyle = 'rgba(110,168,254,.8)';
      g.lineWidth = 1;
      g.strokeRect(0.5, 0.5, dw - 1, dh - 1);
    }
    g.restore();
  }
  return canvas;
}

function partRect(part, applySquash) {
  const sx = applySquash && part.squash ? 0.6 : 1;
  const sy = applySquash && part.squash ? 2 / 3 : 1;
  const half = part.half ? 0.5 : 1;
  return [part.x * sx, part.y * sy, part.width * sx * half, part.height * sy * half];
}

function cellBounds(cell, applySquash) {
  let x0 = Infinity, y0 = Infinity, x1 = -Infinity, y1 = -Infinity;
  for (const part of cell.parts) {
    const [dx, dy, dw, dh] = partRect(part, applySquash);
    x0 = Math.min(x0, dx); y0 = Math.min(y0, dy);
    x1 = Math.max(x1, dx + dw); y1 = Math.max(y1, dy + dh);
  }
  if (!isFinite(x0)) return { x: 0, y: 0, width: 1, height: 1 };
  return { x: x0, y: y0, width: x1 - x0, height: y1 - y0 };
}

function buildCellParts(cell, bank) {
  const detail = document.createElement('div');
  const heading = document.createElement('h3');
  heading.textContent = `cell ${cell.index}`;
  detail.append(heading);

  const table = document.createElement('table');
  table.className = 'parts';
  table.innerHTML = '<thead><tr><th>at</th><th>size</th><th>from</th><th>flags</th></tr></thead>';
  const body = document.createElement('tbody');
  for (const part of cell.parts) {
    const flags = [];
    if (part.flipX) flags.push('flip \u2192');
    if (part.flipY) flags.push('flip \u2193');
    if (part.half) flags.push('half');
    if (part.squash) flags.push('squash');
    const row = document.createElement('tr');
    row.innerHTML = `<td>${part.x},${part.y}</td><td>${part.width}\u00d7${part.height}</td>`
      + `<td>${part.sourceX},${part.sourceY}</td><td>${flags.join(' ') || '\u2014'}</td>`;
    if (bank.sheetWidth && (part.sourceX + part.width > bank.sheetWidth
      || part.sourceY + part.height > bank.sheetHeight)) {
      row.className = 'hidden-part';
      row.title = `asks for more than the sheet has (${bank.sheetWidth}\u00d7${bank.sheetHeight})`;
    }
    body.append(row);
  }
  table.append(body);
  detail.append(table);
  return detail;
}

function showScreen(bank, facts, list) {
  const screen = bank.screen;
  facts.textContent = `a real screen: ${screen.width}\u00d7${screen.height}, `
    + `${screen.tiles.length} tiles, colour mode ${screen.colourMode}, format ${screen.format}`;
  const note = document.createElement('p');
  note.className = 'empty';
  note.textContent = 'This is one of the only 3 .NSCR files that really is a screen - a '
    + 'tile map rather than a cutting plan. The other 106 are cell banks.';
  list.append(note);
}

function showAnimation(bank, facts, list) {
  const sequences = bank.animation.sequences;
  facts.textContent = `${sequences.length} sequence(s)`;
  const modes = ['forward', 'loop', 'back and forth', 'loop back and forth'];
  const table = document.createElement('table');
  table.className = 'parts';
  table.innerHTML = '<thead><tr><th>sequence</th><th>frames</th><th>loops from</th>'
    + '<th>play</th><th>cells</th></tr></thead>';
  const body = document.createElement('tbody');
  for (const sequence of sequences) {
    const row = document.createElement('tr');
    row.innerHTML = `<td>${sequence.index}</td><td>${sequence.frames.length}</td>`
      + `<td>${sequence.loopFrom}</td>`
      + `<td>${modes[sequence.playMode] || sequence.playMode}</td>`
      + `<td>${sequence.frames.slice(0, 12).map(f => f.cell).join(', ')}`
      + `${sequence.frames.length > 12 ? ' \u2026' : ''}</td>`;
    body.append(row);
  }
  table.append(body);
  list.append(table);
}

// ----------------------------------------------------------------------- models

// The server hands over triangles, not display lists, so this only has to hang a
// viewer on the canvas and list what the model is made of. The part list doubles as
// the check that the decode is right: shape, node, material and texture per group,
// against the counts the model records for itself.

async function openModel(name) {
  const node = view('model', name, false);
  const canvas = $('.scene', node);
  const facts = $('.facts', node);

  const model = await api(`/api/model?name=${encodeURIComponent(name)}`);
  setDocData(model);
  if (model.error || model.problem) {
    facts.textContent = model.error || model.problem;
    return;
  }

  const viewer = makeModelViewer(canvas, say);
  if (!viewer) return;

  const triangles = model.indices.length / 3;
  facts.textContent = `${model.groups.length} part(s)  ·  `
    + `${(model.buffer.length / 8).toLocaleString()} vertices  ·  `
    + `${triangles.toLocaleString()} triangles  ·  `
    + `the model says ${model.vertices.toLocaleString()} vertices, `
    + `${model.triangles.toLocaleString()} triangles and ${model.quads.toLocaleString()} quads`;

  // The parts are listed in the hierarchy; picking one there shows it here.
  if (activeDoc) {
    activeDoc.inspect = (ref) => ref && ref.startsWith('part:')
      ? buildModelPart(model.groups[Number(ref.slice(5))], name) : null;
  }

  if (model.groups.some(g => g.hidden)) {
    facts.textContent += '  ·  one part is switched off by its node';
  }

  // Drag to orbit, wheel to zoom.
  let dragging = false;
  let lastX = 0;
  let lastY = 0;
  canvas.onpointerdown = (e) => {
    dragging = true;
    lastX = e.clientX;
    lastY = e.clientY;
    canvas.setPointerCapture(e.pointerId);
  };
  canvas.onpointermove = (e) => {
    if (!dragging) return;
    viewer.orbit(e.clientX - lastX, e.clientY - lastY);
    lastX = e.clientX;
    lastY = e.clientY;
  };
  canvas.onpointerup = (e) => {
    dragging = false;
    canvas.releasePointerCapture(e.pointerId);
  };
  canvas.onwheel = (e) => {
    e.preventDefault();
    viewer.zoom(Math.sign(e.deltaY));
  };

  $('.recentre', node).onclick = () => viewer.reset();
  $('.hidden-parts', node).onchange = (e) => viewer.setShowHidden(e.target.checked);
  window.addEventListener('resize', () => viewer.redraw());

  await viewer.show(model, name);
}

// Shared with script-editor.js and map-editor.js, which both load after this file.
/// One part of a model, for the inspector. The same facts the parts table used to
/// carry, for whichever part is picked in the hierarchy.
function buildModelPart(group, packageName) {
  const panel = document.createElement('div');
  if (!group) return panel;

  const title = document.createElement('h3');
  title.textContent = group.shape || 'part';
  panel.append(title);

  if (group.texture) {
    const stage = document.createElement('div');
    stage.className = 'preview-stage checker';
    const picture = document.createElement('img');
    picture.alt = group.texture;
    picture.src = `/api/model/texture?name=${encodeURIComponent(packageName)}`
      + `&texture=${encodeURIComponent(group.texture)}`;
    picture.onerror = () => stage.remove();
    stage.append(picture);
    panel.append(stage);
  }

  const notes = [];
  if (group.billboard === 1) notes.push('turns to face you');
  if (group.billboard === 2) notes.push('turns about its vertical axis');
  if (group.translucent) notes.push('drawn in the translucent pass');
  if (group.hidden) notes.push('switched off by its node - never drawn');

  panel.append(factList([
    ['node', group.node],
    ['material', group.material || 'none'],
    ['texture', group.texture || 'none'],
    ['triangles', group.count ? group.count / 3 : 0],
    ['tint', '#' + (group.colour >>> 0).toString(16).padStart(6, '0')],
    ['alpha', Math.round((group.alpha ?? 1) * 100) + '%'],
    ['notes', notes.join(' · ')]
  ]));
  return panel;
}

function escapeHtml(text) {
  return String(text).replace(/[&<>"]/g, c =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
}

// ------------------------------------------------------------------------ audio

async function openAudio(name) {
  const sound = state.audio.find(s => s.name === name);
  if (!sound) throw new Error('no sound called ' + name);

  const node = view('audio', name, false);
  const facts = $('.facts', node);

  const fact = (label, value) => {
    const dt = document.createElement('dt');
    dt.textContent = label;
    const dd = document.createElement('dd');
    dd.textContent = value;
    facts.append(dt, dd);
  };

  fact('kind', sound.kind === 'bgm' ? 'music' : 'sound effect');
  fact('length', `${(sound.milliseconds / 1000).toFixed(2)} s`);
  fact('format', `${sound.sampleRate} Hz, ${sound.channels === 2 ? 'stereo' : 'mono'}`);
  fact('parts', sound.parts.length === 2
    ? 'intro (_0) and loop (_1)'
    : `one part (_${sound.parts[0]})`);
  if (sound.loopAt >= 0) {
    fact('loops at', `${(sound.loopAt / 1000).toFixed(2)} s   (sound/${name}.dat)`);
  }

  const players = $('.players', node);
  for (const part of sound.parts) {
    const row = document.createElement('div');
    row.className = 'player';
    const label = document.createElement('span');
    label.textContent = sound.parts.length === 2
      ? (part === 0 ? 'intro' : 'loop')
      : 'sound';
    const audio = document.createElement('audio');
    audio.controls = true;
    audio.preload = 'none';
    audio.src = `/api/audio/wav?name=${encodeURIComponent(name)}&part=${part}`;
    row.append(label, audio);
    players.append(row);
  }

  $('.call', node).textContent = sound.call || 'not reachable from a script by number';

  const uses = $('.uses', node);
  const found = await api(`/api/audio/uses?name=${encodeURIComponent(name)}`);
  uses.textContent = '';
  if (!found.length) {
    const item = document.createElement('li');
    item.className = 'empty';
    item.textContent = 'no script plays this by number - it may be started by the '
      + 'engine itself, from a menu, or in battle';
    uses.append(item);
    return;
  }
  for (const use of found) {
    const item = document.createElement('li');
    const link = document.createElement('a');
    link.textContent = use.script;
    link.onclick = () => { location.hash = hashFor('script', use.script); };
    const count = document.createElement('b');
    count.textContent = use.count === 1 ? 'once' : `${use.count} times`;
    item.append(link, count);
    uses.append(item);
  }
}
