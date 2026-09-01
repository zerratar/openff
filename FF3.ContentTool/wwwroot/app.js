// The editor's front end.
//
// The server decodes and compiles; this side is the interface. Menus are edited as
// XML in the browser - the DOM already knows how to parse and serialise it - so the
// canvas is a view over the same tree that gets posted back.

'use strict';

const state = { kind: 'script', files: [], name: null };

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
  return response.json();
}

function say(message, tone) {
  const status = $('#status');
  status.textContent = message || '';
  status.className = tone || '';
}

// ------------------------------------------------------------------- file list

async function loadList() {
  state.files = await api(`/api/list?kind=${state.kind}`);
  drawList();
}

function drawList() {
  const filter = $('#filter').value.trim().toLowerCase();
  const list = $('#files');
  list.textContent = '';

  for (const file of state.files) {
    if (filter && !file.name.toLowerCase().includes(filter)) continue;
    const item = document.createElement('li');
    item.textContent = file.name;
    item.title = file.name;
    item.className = (file.overridden ? 'overridden ' : '') + (file.name === state.name ? 'on' : '');
    item.onclick = () => open(file.name);
    list.append(item);
  }
}

function markOverridden(name, overridden) {
  const file = state.files.find(f => f.name === name);
  if (file) file.overridden = overridden;
  drawList();
  const badge = $('.badge.override');
  if (badge) badge.classList.toggle('on', overridden);
}

async function open(name) {
  state.name = name;
  drawList();
  say('loading…');
  try {
    if (state.kind === 'script') await openScript(name);
    else if (state.kind === 'menu') await openMenu(name);
    else if (state.kind === 'table') await openTable(name);
    else await openText(name);
    say('');
  } catch (error) {
    say(error.message, 'bad');
  }
}

function view(id, name, overridden) {
  const pane = $('#pane');
  pane.textContent = '';
  const node = $(`#view-${id}`).content.cloneNode(true).firstElementChild;
  $('.name', node).textContent = name;
  $('.badge.override', node).classList.toggle('on', overridden);
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

const menu = { doc: null, screens: [], screen: null, selected: null, name: null };

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
  picker.onchange = () => drawScreen(node, menu.screens[picker.value]);

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
    drawScreen(node, menu.screens[picker.value], copy);
    say('duplicated - it needs a unique id', 'good');
  };

  $('.remove', node).onclick = () => {
    if (!menu.selected) return say('select a widget first', 'bad');
    if (!confirm('Delete this widget and everything nested in it?')) return;
    menu.selected.remove();
    menu.selected = null;
    drawScreen(node, menu.screens[picker.value]);
  };

  drawScreen(node, menu.screens[0]);
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
  const frames = collectFrames(screen);

  const width = Math.max(256, ...frames.map(f => f.x + Math.max(f.width, 8))) + 16;
  const height = Math.max(192, ...frames.map(f => f.y + Math.max(f.height, 8))) + 16;
  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;

  for (const frame of frames) {
    const box = document.createElement('div');
    box.className = 'widget';
    box.style.left = `${frame.x}px`;
    box.style.top = `${frame.y}px`;
    box.style.width = `${Math.max(frame.width, 8)}px`;
    box.style.height = `${Math.max(frame.height, 8)}px`;
    box.title = `${frame.id || '(no id)'}  ${frame.width}x${frame.height}`
      + (frame.behavior ? `  ${frame.behavior}` : '');

    const label = document.createElement('span');
    label.className = 'label';
    label.textContent = frame.id || frame.behavior || '';
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
    const dx = Math.round(moveEvent.clientX - startX);
    const dy = Math.round(moveEvent.clientY - startY);
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
  const panel = $('.properties', node);
  panel.textContent = '';

  if (!frame) {
    const empty = document.createElement('p');
    empty.className = 'empty';
    empty.textContent = 'Select a widget.';
    panel.append(empty);
    return;
  }

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

  const xml = document.createElement('label');
  xml.textContent = 'this widget as XML';
  const area = document.createElement('textarea');
  area.value = new XMLSerializer().serializeToString(frame.element);
  area.spellcheck = false;
  area.onchange = () => {
    try {
      const parsed = new DOMParser().parseFromString(area.value, 'application/xml');
      if (parsed.querySelector('parsererror')) throw new Error('that is not valid XML');
      frame.element.replaceWith(menu.doc.importNode(parsed.documentElement, true));
      drawScreen(node, screen);
      say('widget replaced', 'good');
    } catch (error) {
      say(error.message, 'bad');
    }
  };
  xml.append(area);
  panel.append(xml);
}

document.addEventListener('keydown', event => {
  if (state.kind !== 'menu' || !menu.selected) return;
  if (event.target.matches('input, textarea')) return;
  const step = event.shiftKey ? 8 : 1;
  const moves = { ArrowLeft: [-step, 0], ArrowRight: [step, 0], ArrowUp: [0, -step], ArrowDown: [0, step] };
  const move = moves[event.key];
  if (!move) return;
  event.preventDefault();
  setChildText(menu.selected, 'x', String(number(childText(menu.selected, 'x')) + move[0]));
  setChildText(menu.selected, 'y', String(number(childText(menu.selected, 'y')) + move[1]));
  const node = $('.view');
  const picker = $('.screens', node);
  drawScreen(node, menu.screens[picker.value], menu.selected);
});

// ------------------------------------------------------------------------- text

async function openText(name) {
  const data = await api(`/api/text?name=${encodeURIComponent(name)}`);
  const node = view('text', name, data.overridden);
  const list = $('.messages', node);
  const messages = data.messages;

  for (const message of messages) {
    const row = document.createElement('div');
    row.className = 'message';

    const id = document.createElement('div');
    id.className = 'id';
    id.textContent = message.id;
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
      area.oninput = () => { message.pages[index] = area.value; };
      pages.append(area);
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

  const showPadding = $('.pads input', node);
  const rowFilter = $('.rowfilter', node);
  const draw = () => drawTable(node, decoded.find(c => c.index === Number(picker.value)),
    showPadding.checked, rowFilter.value.trim().toLowerCase());

  picker.onchange = draw;
  showPadding.onchange = draw;
  rowFilter.oninput = draw;

  $('.save', node).onclick = async () => {
    const result = await api('/api/table/save', { name, file });
    markOverridden(name, true);
    say(`saved, ${result.bytes} bytes`, 'good');
  };

  if (decoded.length) draw();
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

// -------------------------------------------------------------------- start up

$$('#kinds button').forEach(button => {
  button.onclick = async () => {
    $$('#kinds button').forEach(b => b.classList.toggle('on', b === button));
    state.kind = button.dataset.kind;
    state.name = null;
    // A filter left over from another kind reads as an empty folder.
    $('#filter').value = '';
    $('#pane').innerHTML = '<p class="empty">Pick a file on the left.</p>';
    await loadList();
  };
});

$('#filter').addEventListener('input', drawList);

api('/api/status')
  .then(status => say(`${status.files} files  ·  overrides in ${status.overrides}`))
  .then(loadList)
  .catch(error => say(error.message, 'bad'));
