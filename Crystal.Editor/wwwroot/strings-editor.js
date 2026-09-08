// The mod's text as a table: one row per line - the id, a copy button, the words, a remove
// button - with the JSON the file holds in a pane underneath (like the menu editor's XML),
// collapsible and editable both ways. Every change saves itself; the ids are checked as
// they are typed (a number, no twins, in the mod's range) so a slip shows before it costs
// an afternoon.

/// Remembered across files: whether the JSON pane is open and how tall.
const stringsUi = { jsonOpen: false, jsonHeight: 180 };

/// The mod's own ids start here (Shared/Data/ModText.cs); the game's own stop far below.
const STRINGS_FIRST_ID = 40000000;

/// The file's lines in the order the file has them: [{ id: "40000001", text }]. Both forms
/// the client reads - an object of id -> text, an array of { id, text } - come out the same.
function parseStrings(source) {
  const node = JSON.parse(source || '{}');
  const lines = [];
  // A line written per language ({ en, de, ... }): the table edits the English (or the
  // first) and keeps the others as they are - `others` goes back into the file untouched.
  const line = (id, value) => {
    if (value && typeof value === 'object' && !Array.isArray(value)) {
      const keys = Object.keys(value);
      const lang = keys.includes('en') ? 'en' : keys[0];
      const others = {};
      for (const k of keys) if (k !== lang) others[k] = value[k];
      return { id: String(id), text: String(value[lang] ?? ''), lang, others };
    }
    return { id: String(id), text: typeof value === 'string' ? value : String(value ?? '') };
  };
  if (Array.isArray(node)) {
    for (const item of node) if (item && item.id != null) lines.push(line(item.id, item.text));
  } else if (node && typeof node === 'object') {
    for (const [id, value] of Object.entries(node)) lines.push(line(id, value));
  }
  return lines;
}

/// A line's value as the file holds it: the text, or the per-language object.
function stringsValue(line) {
  if (!line.lang) return line.text;
  return Object.assign({ [line.lang]: line.text }, line.others || {});
}

/// The file as written back: the object form, one line per entry. While two rows share an
/// id (or one is not a number yet) the array form instead - an object would fold the twins
/// into one and lose a line between a save and the next opening; the client reads both.
function serializeStrings(lines) {
  if (!lines.length) return '{\n}\n';
  const ids = lines.map(l => String(l.id));
  const clean = ids.every((id, i) => /^\d+$/.test(id) && ids.indexOf(id) === i);
  if (!clean) {
    const body = lines.map(l => `  { "id": ${JSON.stringify(String(l.id))}, "text": ${JSON.stringify(stringsValue(l))} }`).join(',\n');
    return `[\n${body}\n]\n`;
  }
  const body = lines.map(l => `  ${JSON.stringify(String(l.id))}: ${JSON.stringify(stringsValue(l))}`).join(',\n');
  return `{\n${body}\n}\n`;
}

/// What is wrong with a row's id, or '' when nothing is.
function stringsIdProblem(id, lines, index) {
  if (!/^\d+$/.test(id)) return 'an id is a whole number';
  const n = Number(id);
  if (n > 4294967295) return 'too large for a message id';
  if (lines.some((l, i) => i !== index && l.id === id)) return 'two lines share this id - the first wins, the other is lost';
  if (n < STRINGS_FIRST_ID) return `below ${STRINGS_FIRST_ID}: the game's own ids live there, and a line with one of the game's ids is skipped in favour of the game's`;
  return '';
}

/// The view for defs/text/<name>.json.
async function openStrings(name) {
  const path = 'defs/text/' + name + '.json';
  const data = await api(`/api/project/file?name=${encodeURIComponent(path)}`);
  if (data.ok === false) throw new Error(data.error);
  const doc = activeDoc;
  const node = view('strings', name, false);
  const body = $('tbody', node);
  const json = $('textarea.xml', node);
  const saving = $('.badge.saving', node);
  const problemBadge = $('.badge.problem', node);
  const eol = (data.text || '').includes('\r\n') ? '\r\n' : '\n';

  let lines;
  let parseProblem = '';
  try { lines = parseStrings(data.text); } catch (e) { lines = []; parseProblem = e.message; }
  setDocData({ text: data.text, lines, path });
  if (doc) doc.selection = null;

  let saveTimer = null;
  let saved = data.text;

  const save = async () => {
    const text = serializeStrings(lines).replace(/\n/g, eol);
    if (text === saved) return;
    saving.classList.add('on');
    try {
      const result = await api('/api/project/file/save', { name: path, text });
      if (!result.ok) throw new Error(result.error);
      saved = text;
      if (doc && doc.data) doc.data.text = text;
      say(`saved ${shortName(path)}`, 'good');
      if (typeof itemsCountChanged === 'function') itemsCountChanged();
      if (state.browse === 'strings') loadList().catch(() => {});
    } catch (e) {
      say(e.message, 'bad');
    } finally {
      saving.classList.remove('on');
    }
  };
  const changed = () => {
    if (doc && doc.data) doc.data.lines = lines;
    if (document.activeElement !== json) json.value = serializeStrings(lines);
    problems();
    clearTimeout(saveTimer);
    saveTimer = setTimeout(() => save(), 500);
    if (activeDoc === doc) { drawHierarchy(); drawInspector(); }
  };

  const problems = () => {
    let count = 0;
    body.querySelectorAll('tr').forEach((row, i) => {
      const why = stringsIdProblem(lines[i].id, lines, i);
      row.classList.toggle('bad', Boolean(why));
      $('input.id', row).title = why || 'the id the mod says this line by';
      if (why) count++;
    });
    problemBadge.textContent = parseProblem ? parseProblem : count ? `${count} id problem${count === 1 ? '' : 's'}` : '';
    problemBadge.classList.toggle('on', Boolean(parseProblem || count));
  };

  const grow = area => {
    area.style.height = 'auto';
    area.style.height = Math.max(28, area.scrollHeight) + 'px';
  };

  const draw = () => {
    body.textContent = '';
    lines.forEach((line, index) => {
      const row = document.createElement('tr');
      row.dataset.index = index;
      if (doc && doc.selection === line.id) row.classList.add('on');

      const idCell = document.createElement('td');
      idCell.className = 'id';
      const id = document.createElement('input');
      id.className = 'id';
      id.value = line.id;
      id.spellcheck = false;
      id.oninput = () => { line.id = id.value.trim(); changed(); };
      id.onfocus = () => { if (doc) doc.selection = line.id; };
      idCell.append(id);

      const copyCell = document.createElement('td');
      copyCell.className = 'copy';
      const copy = document.createElement('button');
      copy.className = 'small';
      copy.textContent = 'Copy';
      copy.title = 'Copy the id - paste it after @ in a Chest or Talk field, or into startMessage2(0, id, 0, 0)';
      copy.onclick = async () => {
        try {
          await navigator.clipboard.writeText(line.id);
          say(`copied ${line.id}`, 'good');
        } catch (e) {
          // No clipboard (an http page on another machine): select the id so Ctrl+C takes it.
          id.focus();
          id.select();
        }
      };
      copyCell.append(copy);

      const textCell = document.createElement('td');
      const text = document.createElement('textarea');
      text.className = 'line';
      text.value = line.text;
      text.rows = 1;
      text.spellcheck = true;
      text.oninput = () => { line.text = text.value; grow(text); changed(); };
      text.onfocus = () => { if (doc) doc.selection = line.id; };
      text.onkeydown = event => {
        // Ctrl+Enter: a new line after this one, as Add does.
        if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) { event.preventDefault(); add(index + 1); }
      };
      textCell.append(text);
      if (line.lang) {
        const langs = document.createElement('i');
        langs.className = 'langs';
        const others = Object.keys(line.others || {});
        langs.textContent = `${line.lang} shown` + (others.length ? `; also ${others.join(', ')} - in the JSON below` : '');
        textCell.append(langs);
      }

      const removeCell = document.createElement('td');
      removeCell.className = 'remove';
      const remove = document.createElement('button');
      remove.className = 'small shut';
      remove.textContent = '×';
      remove.title = 'Remove this line';
      remove.onclick = () => {
        lines.splice(index, 1);
        draw();
        changed();
      };
      removeCell.append(remove);

      row.append(idCell, copyCell, textCell, removeCell);
      body.append(row);
      grow(text);
    });
    if (!lines.length) {
      const row = document.createElement('tr');
      const cell = document.createElement('td');
      cell.colSpan = 4;
      cell.className = 'empty';
      cell.textContent = parseProblem ? `The file does not read as JSON (${parseProblem}) - fix it in the pane below, or Add line starts over.` : 'No lines yet. Add line (above) makes the first at the next free id.';
      row.append(cell);
      body.append(row);
    }
    problems();
  };

  /// The next id no line of the file - or of the project - uses.
  const nextId = async () => {
    let next = STRINGS_FIRST_ID;
    for (const l of lines) if (/^\d+$/.test(l.id)) next = Math.max(next, Number(l.id) + 1);
    if (lines.length) return String(next);
    try {
      const r = await api('/api/project/text');
      if (r.ok && r.next) next = Math.max(next, Number(r.next));
    } catch (e) { /* the file's own count will do */ }
    return String(next);
  };

  const add = async at => {
    const id = await nextId();
    const line = { id, text: '' };
    if (at == null || at >= lines.length) lines.push(line); else lines.splice(at, 0, line);
    if (parseProblem) parseProblem = '';
    draw();
    changed();
    const row = body.querySelector(`tr[data-index="${lines.indexOf(line)}"]`);
    if (row) $('textarea', row).focus();
  };

  $('.add', node).onclick = () => add();
  $('.as-code', node).onclick = () => openDoc('code', path);

  // The JSON pane: the file as it will be saved; Apply reads it back into the table.
  json.value = data.text;
  const applyJsonHeight = () => {
    const split = $('.xml-split', node);
    split.classList.toggle('shut', !stringsUi.jsonOpen);
    split.style.height = stringsUi.jsonOpen ? `${stringsUi.jsonHeight}px` : '';
    const button = $('.xml-collapse', node);
    button.textContent = stringsUi.jsonOpen ? '\u25be' : '\u25b4';
    button.title = stringsUi.jsonOpen ? 'collapse' : 'expand';
  };
  applyJsonHeight();
  $('.xml-collapse', node).onclick = () => { stringsUi.jsonOpen = !stringsUi.jsonOpen; applyJsonHeight(); };
  $('.xml-grip', node).onpointerdown = event => {
    if (!stringsUi.jsonOpen) return;
    event.preventDefault();
    const startY = event.clientY;
    const startHeight = stringsUi.jsonHeight;
    const move = e => { stringsUi.jsonHeight = Math.max(90, Math.min(760, startHeight + (startY - e.clientY))); applyJsonHeight(); };
    const up = () => { window.removeEventListener('pointermove', move); window.removeEventListener('pointerup', up); };
    window.addEventListener('pointermove', move);
    window.addEventListener('pointerup', up);
  };
  $('.apply', node).onclick = () => {
    try {
      lines = parseStrings(json.value);
      parseProblem = '';
      draw();
      changed();
      say('the table follows the JSON', 'good');
    } catch (e) {
      say('not JSON: ' + e.message, 'bad');
    }
  };
  json.addEventListener('keydown', event => {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') { event.preventDefault(); $('.apply', node).click(); }
  });

  draw();
  // A row asked for from the hierarchy.
  if (doc) doc.revealLine = id => {
    const index = lines.findIndex(l => l.id === id);
    const row = body.querySelector(`tr[data-index="${index}"]`);
    if (!row) return;
    body.querySelectorAll('tr.on').forEach(r => r.classList.remove('on'));
    row.classList.add('on');
    row.scrollIntoView({ block: 'nearest' });
    $('textarea', row).focus();
  };
}

/// The hierarchy for a text file: its lines, "@id" with the words beside.
function outlineForStrings(doc) {
  const lines = (doc.data && doc.data.lines) || [];
  if (!lines.length) return [];
  return [{
    label: 'Lines',
    children: lines.map(l => ({
      ref: l.id,
      label: '@' + l.id,
      icon: 'text',
      note: (l.text || '').replace(/\s+/g, ' ').trim().slice(0, 40),
      reveal: () => { if (doc.revealLine) doc.revealLine(l.id); }
    }))
  }];
}

/// The inspector's facts for the file.
function stringsFacts(data) {
  const lines = (data && data.lines) || [];
  const ids = lines.map(l => Number(l.id)).filter(n => Number.isFinite(n));
  const facts = [['kind', 'the mod\'s text'], ['file', data && data.path || ''], ['lines', lines.length]];
  if (ids.length) facts.push(['ids', `${Math.min(...ids)} .. ${Math.max(...ids)}`]);
  return facts;
}
