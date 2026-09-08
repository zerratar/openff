// A CastScript's code in a tab of its own: the script editor - highlighting, completion
// on the command names, the signature strip that says what each argument is - writing
// back into the scene as you type, and a Check that compiles the lines exactly as the
// client will (Ffs.CastCode on both sides), so a wrong command or a missing bracket shows
// here with its line, not in the log at the next Play. The inspector's field stays as a
// short preview with the button that opens this.

'use strict';

/// The code documents open, by name: the scene state, the attachment and the object,
/// handed over by the inspector before openDoc. A document restored from a session with
/// nothing here asks for the map to be opened first.
const castCodeDocs = new Map();

/// The document's name: the map and the object's path, as the tab reads.
function castCodeName(map, target) {
  return `${map}: ${target}`;
}

/// From the CastScript card: opens (or focuses) the code editor for this attachment.
function openCastCode(state, attachment, target) {
  const name = castCodeName(state.map, target);
  castCodeDocs.set(name, { state, attachment, target });
  openDoc('castcode', name, { pin: true });
}

async function openCastCodeView(name) {
  const link = castCodeDocs.get(name);
  const doc = activeDoc;
  const node = view('castcode', name, false);
  const text = $('textarea', node);
  const gutter = $('.gutter', node);
  const problems = $('.problems', node);
  const saving = $('.badge.saving', node);
  const problemBadge = $('.badge.problem', node);
  const where = $('.cast-where', node);

  if (!link || !link.attachment) {
    text.value = '';
    text.readOnly = true;
    where.textContent = 'Open the map and the object, then Edit code… on its CastScript - the code lives in the scene file.';
    where.classList.add('on');
    setDocData({ lines: [], orphan: true });
    return;
  }
  const { state, attachment, target } = link;
  attachment.fields = attachment.fields || {};
  const cast = () => Number(attachment.fields.Cast) || 0;
  where.textContent = `cast ${cast() || '(own, @me)'} · ${target}`;
  where.classList.add('on');
  where.title = cast() ? 'The cast number the code runs as (the original\'s)' : 'An object of the mod\'s own: a number from 5000 is given in play; @me stands for it in the lines';

  const lines = Array.isArray(attachment.fields.Main) ? attachment.fields.Main : [];
  text.value = lines.join('\n');
  setDocData({ lines, target, map: state.map });

  const numberLines = () => {
    const count = text.value.split('\n').length;
    gutter.textContent = Array.from({ length: count }, (_, i) => i + 1).join('\n');
    gutter.scrollTop = text.scrollTop;
  };
  numberLines();
  text.addEventListener('scroll', () => { gutter.scrollTop = text.scrollTop; });

  const showProblems = list => {
    problems.textContent = '';
    for (const problem of list || []) {
      const item = document.createElement('li');
      const at = document.createElement('b');
      at.textContent = problem.line > 0 ? `line ${problem.line}` : 'the frame';
      item.append(at, document.createTextNode(problem.message));
      if (problem.line > 0) item.onclick = () => goToLine(text, problem.line, (problem.column || 0) + 1);
      problems.append(item);
    }
    problemBadge.textContent = list && list.length ? `${list.length} problem${list.length === 1 ? '' : 's'}` : '';
    problemBadge.classList.toggle('on', Boolean(list && list.length));
  };

  // Into the scene: the attachment's lines, then the scene's own autosave. The inspector's
  // preview follows when the map document is in front.
  let writeTimer = null;
  const write = () => {
    const now = text.value.split('\n');
    attachment.fields.Main = now;
    if (doc && doc.data) doc.data.lines = now;
    if (typeof sceneChanged === 'function' && sceneState === state) sceneChanged('code of ' + target);
    saving.classList.remove('on');
    if (typeof drawInspector === 'function') drawInspector();
  };
  let checkTimer = null;
  const check = async (loud) => {
    try {
      const result = await api('/api/scene/cast-check', { cast: cast(), lines: text.value.split('\n') });
      showProblems(result.problems || []);
      if (loud) say(result.ok ? `compiles cleanly - ${result.bytes} bytes` : `${result.problems.length} problem(s)`, result.ok ? 'good' : 'bad');
    } catch (e) {
      if (loud) say(e.message, 'bad');
    }
  };
  text.addEventListener('input', () => {
    numberLines();
    saving.classList.add('on');
    clearTimeout(writeTimer);
    writeTimer = setTimeout(write, 400);
    clearTimeout(checkTimer);
    checkTimer = setTimeout(() => check(false), 900);
  });
  text.addEventListener('keydown', event => {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') {
      event.preventDefault();
      clearTimeout(writeTimer);
      write();
      check(true);
    }
  });

  $('.check', node).onclick = () => { clearTimeout(writeTimer); write(); check(true); };
  $('.commands', node).onclick = () => commandPicker(text);

  if (typeof enhanceScriptEditor === 'function') await enhanceScriptEditor(node);
  check(false);
}

/// Every command with what it takes, filtered as you type; picks one into the code at
/// the caret, its brackets ready and the caret between them.
async function commandPicker(text) {
  if (typeof loadOps !== 'function') return;
  const list = await loadOps();
  const body = dialog('Commands of the script language', { wide: true });
  const filter = field(body, 'Filter', '', { placeholder: 'part of a name - message, motion, flag…' });
  const hint = document.createElement('p');
  hint.className = 'muted';
  hint.textContent = `${list.length} commands, as the game's own scripts use them. The strip under the editor says what each argument is while you type; @me is this object's cast number.`;
  body.append(hint);
  const rows = document.createElement('div');
  rows.className = 'command-list';
  body.append(rows);
  const draw = () => {
    const want = filter.value.trim().toLowerCase();
    rows.textContent = '';
    const shown = list
      .filter(op => !want || op.name.toLowerCase().includes(want) || (op.handler || '').toLowerCase().includes(want))
      .sort((a, b) => {
        const as = want && a.name.toLowerCase().startsWith(want) ? 0 : 1;
        const bs = want && b.name.toLowerCase().startsWith(want) ? 0 : 1;
        return as - bs || a.name.localeCompare(b.name);
      })
      .slice(0, 80);
    for (const op of shown) {
      const row = document.createElement('div');
      row.className = 'command-row';
      const name = document.createElement('b');
      name.textContent = op.name;
      const args = document.createElement('span');
      args.textContent = op.operands.length
        ? op.operands.map(o => (o.name ? `${o.name}:` : '') + o.type + (o.fixed ? ' fixed' : '')).join(', ')
        : 'no arguments';
      row.append(name, args);
      row.title = `opcode ${op.opcode}` + (op.handler ? ` · ${op.handler}` : '');
      row.onclick = () => {
        insertCommand(text, op);
        const shut = document.querySelector('.picker.dialog .shut');
        if (shut) shut.click();
      };
      rows.append(row);
    }
    if (!shown.length) {
      const none = document.createElement('p');
      none.className = 'none';
      none.textContent = 'No command has that in its name.';
      rows.append(none);
    }
  };
  filter.oninput = draw;
  filter.onkeydown = e => { if (e.key === 'Enter') { const first = rows.querySelector('.command-row'); if (first) first.click(); } };
  draw();
  setTimeout(() => filter.focus(), 0);
}

/// The call form of a command on its own line at the caret, the caret between the brackets.
function insertCommand(text, op) {
  const start = text.selectionStart;
  const before = text.value.slice(0, start);
  const after = text.value.slice(text.selectionEnd);
  const lineStart = before.lastIndexOf('\n') + 1;
  const onLine = before.slice(lineStart);
  const lead = onLine.trim().length ? '\n' : '';
  const tail = after.length && !after.startsWith('\n') ? '\n' : '';
  const insert = op.operands.length ? `${op.name}();` : `${op.name}();`;
  text.value = before + lead + insert + tail + after;
  const caret = start + lead.length + op.name.length + (op.operands.length ? 1 : insert.length);
  text.focus();
  text.setSelectionRange(caret, caret);
  text.dispatchEvent(new Event('input', { bubbles: true }));
}

/// The hierarchy for a code document: its labels, to jump to.
function outlineForCastCode(doc) {
  const lines = (doc.data && doc.data.lines) || [];
  const children = [];
  lines.forEach((line, i) => {
    const label = (line || '').match(/^\s*([A-Za-z_][\w.]*)\s*:\s*(\/\/.*)?$/);
    if (label) children.push({
      ref: 'line:' + i, label: label[1], icon: 'script', note: `line ${i + 1}`,
      reveal: () => { const text = $('textarea', doc.pane); if (text) goToLine(text, i + 1, 1); }
    });
  });
  return children.length ? [{ label: 'Labels', children }] : [];
}

function castCodeFacts(data) {
  const lines = (data && data.lines) || [];
  const commands = lines.filter(l => /^\s*[A-Za-z_][\w.]*\s*\(/.test(l || '')).length;
  const facts = [['kind', 'a cast\'s code']];
  if (data && data.map) facts.push(['map', data.map]);
  if (data && data.target) facts.push(['object', data.target]);
  facts.push(['lines', lines.length], ['commands', commands]);
  return facts;
}
