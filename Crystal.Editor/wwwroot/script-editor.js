// Guidance for the script editor: highlighting, completion, and telling you what an
// argument is for.
//
// The instruction set comes from the server once and is cached, because it is derived
// from the game's own handlers rather than written down anywhere the browser could
// read. See Docs/Script-Language.md.
//
// Highlighting is a <pre> behind a transparent <textarea>, both with the same metrics
// and scroll position. It is the oldest trick there is and it avoids pulling in an
// editor library for a tool that should stay a single folder of files.

'use strict';

const ops = { list: null, byName: new Map(), conditions: new Set() };

/// Forget the cached set: the workspace can change games under us (File > Open a
/// project that targets FF4), and FF4 numbers 500 instructions to FF3's 298.
function resetOps() {
  ops.list = null;
  ops.byName.clear();
  ops.conditions.clear();
}

async function loadOps() {
  if (ops.list) return ops.list;
  const data = await api('/api/ops');
  ops.list = data.ops;
  for (const op of ops.list) ops.byName.set(op.name, op);
  // Derived from the opcode table on the server; keeping a copy here would only
  // give the two something to disagree about.
  for (const condition of data.conditions) ops.conditions.add(condition.name);
  ops.conditions.add('value');
  return ops.list;
}

/// "wait frame:word" - what an instruction takes, in words rather than widths.
function signatureOf(op) {
  if (!op.operands.length) return op.name;
  const parts = op.operands.map(operand => {
    const type = operand.fixed ? operand.type + ' fixed' : operand.type;
    return operand.name ? `${operand.name}:${type}` : type;
  });
  return `${op.name} ${parts.join(', ')}`;
}

// escapeHtml lives in app.js, which loads before this file. It was declared in both
// for a while, which is a redeclaration error - and that quietly disabled everything
// below, leaving the editor showing blank lines.

/// One line of source as highlighted HTML.
const KEYWORDS = new Set(['map', 'cast', 'function', 'func', 'extern', 'data',
  'if', 'else', 'while', 'do', 'for', 'goto', 'break', 'continue',
  'init', 'main', 'exit', 'none']);

const LABEL = new RegExp('^(\\s*)([A-Za-z_][\\w.]*)(\\s*:)');
const TOKEN = new RegExp('("(?:[^"' + String.fromCharCode(92, 92) + ']|'
  + String.fromCharCode(92, 92) + '.)*")'
  + '|(-?0[xX][0-9a-fA-F]+|-?[0-9]+)'
  + '|([A-Za-z_][A-Za-z0-9_.]*)', 'g');
const CALLED = /^\s*\(/;

function highlightLine(line) {
  const comment = line.indexOf('//');
  const code = comment >= 0 ? line.slice(0, comment) : line;
  const trailing = comment >= 0 ? line.slice(comment) : '';

  let html = '';
  let at = 0;

  // A label definition owns the line up to its colon.
  const label = code.match(LABEL);
  if (label && !KEYWORDS.has(label[2])) {
    html += label[1] + '<i class="t-label">' + escapeHtml(label[2]) + '</i>' + label[3];
    at = label[0].length;
  }

  TOKEN.lastIndex = at;
  let match;
  while ((match = TOKEN.exec(code)) !== null) {
    html += escapeHtml(code.slice(at, match.index));
    const text = escapeHtml(match[0]);

    if (match[1]) {
      html += '<i class="t-string">' + text + '</i>';
    } else if (match[2]) {
      html += '<i class="t-number">' + text + '</i>';
    } else {
      const word = match[3];
      // A name followed by ( is being called; anything else is a label or a value.
      const called = CALLED.test(code.slice(match.index + word.length));
      const kind = KEYWORDS.has(word) ? 't-key'
        : ops.conditions.has(word) ? 't-cond'
        : ops.byName.has(word) ? 't-op'
        : called ? 't-unknown'
        : 't-name';
      html += '<i class="' + kind + '">' + text + '</i>';
    }
    at = match.index + match[0].length;
  }
  html += escapeHtml(code.slice(at));

  if (trailing) html += '<i class="t-comment">' + escapeHtml(trailing) + '</i>';
  return html;
}

function repaint(text, highlight) {
  highlight.innerHTML = text.value.split('\n').map(highlightLine).join('\n') + '\n';
  highlight.scrollTop = text.scrollTop;
  highlight.scrollLeft = text.scrollLeft;
}

/// The instruction the caret is inside, and which argument it is on.
function caretContext(text) {
  const upto = text.value.slice(0, text.selectionStart);
  const lineStart = upto.lastIndexOf('\n') + 1;
  const line = upto.slice(lineStart);
  const withoutComment = line.split('//')[0];
  const words = withoutComment.match(/^\s*([A-Za-z_][\w.]*)/);
  if (!words) return null;

  const op = ops.byName.get(words[1]);
  if (!op) return null;

  // Count the commas inside the brackets, so the strip follows the caret along.
  let args = withoutComment.slice(words[0].length).trimStart();
  if (args.startsWith('(')) args = args.slice(1);
  const argument = args.split(',').length - 1;
  return { op, argument };
}

function setupSignature(text, bar) {
  const update = () => {
    if (!ops.list) { loadOps().then(update).catch(() => {}); return; }
    const context = caretContext(text);
    if (!context) {
      bar.textContent = '';
      bar.classList.remove('on');
      return;
    }
    bar.classList.add('on');
    bar.textContent = '';

    const name = document.createElement('b');
    name.textContent = context.op.name;
    bar.append(name);

    context.op.operands.forEach((operand, i) => {
      const part = document.createElement('span');
      const type = operand.fixed ? `${operand.type} fixed` : operand.type;
      part.textContent = operand.name ? `${operand.name}:${type}` : type;
      if (i === context.argument) part.className = 'here';
      bar.append(part);
    });

    if (!context.op.operands.length) {
      const none = document.createElement('span');
      none.textContent = 'no arguments';
      bar.append(none);
    }
  };

  ['keyup', 'click', 'input', 'select'].forEach(event =>
    text.addEventListener(event, update));
  update();
}

function setupCompletion(text, list) {
  let matches = [];
  let index = 0;

  const wordBefore = () => {
    const upto = text.value.slice(0, text.selectionStart);
    const line = upto.slice(upto.lastIndexOf('\n') + 1);
    // Only the first word on a line is an instruction.
    const match = line.match(/^(\s*)([A-Za-z_][\w.]*)$/);
    return match ? match[2] : null;
  };

  const hide = () => { list.hidden = true; matches = []; };

  const show = () => {
    // The set is dropped when the project changes under us (a scene saved, a file
    // installed): fetch it again and come back, rather than fail on every keystroke.
    if (!ops.list) { loadOps().then(show).catch(() => {}); return; }
    const word = wordBefore();
    if (!word || word.length < 2) return hide();

    const lower = word.toLowerCase();
    matches = ops.list
      .filter(op => op.name.toLowerCase().includes(lower))
      .sort((a, b) => {
        const aStarts = a.name.toLowerCase().startsWith(lower) ? 0 : 1;
        const bStarts = b.name.toLowerCase().startsWith(lower) ? 0 : 1;
        return aStarts - bStarts || a.name.localeCompare(b.name);
      })
      .slice(0, 12);

    if (!matches.length) return hide();
    index = 0;
    draw();
    place();
    list.hidden = false;
  };

  const draw = () => {
    list.textContent = '';
    matches.forEach((op, i) => {
      const item = document.createElement('li');
      if (i === index) item.className = 'on';
      const name = document.createElement('b');
      name.textContent = op.name;
      const args = document.createElement('span');
      args.textContent = signatureOf(op).slice(op.name.length);
      item.append(name, args);
      item.onmousedown = event => { event.preventDefault(); accept(op); };
      list.append(item);
    });
  };

  /// Put the list under the caret, roughly - a line height is close enough.
  const place = () => {
    const before = text.value.slice(0, text.selectionStart);
    const line = before.split('\n').length;
    const column = before.length - before.lastIndexOf('\n') - 1;
    const style = getComputedStyle(text);
    const lineHeight = parseFloat(style.lineHeight) || 18;
    const charWidth = 7.2;
    list.style.top = `${(line - 1) * lineHeight - text.scrollTop + lineHeight + 10}px`;
    list.style.left = `${column * charWidth - text.scrollLeft + 12}px`;
  };

  const accept = op => {
    const start = text.selectionStart;
    const upto = text.value.slice(0, start);
    const lineStart = upto.lastIndexOf('\n') + 1;
    const typed = upto.slice(lineStart).match(/([A-Za-z_][\w.]*)$/);
    if (!typed) return hide();
    const from = lineStart + typed.index;
    // Insert the call form, caret between the brackets where there are arguments.
    const insert = op.operands.length ? op.name + '()' : op.name + '();';
    text.value = text.value.slice(0, from) + insert + text.value.slice(start);
    const caret = from + op.name.length + (op.operands.length ? 1 : insert.length);
    text.setSelectionRange(caret, caret);
    hide();
    text.dispatchEvent(new Event('input', { bubbles: true }));
  };

  text.addEventListener('input', show);
  text.addEventListener('blur', hide);
  text.addEventListener('scroll', () => { if (!list.hidden) place(); });

  text.addEventListener('keydown', event => {
    if (list.hidden) {
      // Ctrl+Space asks for the list even mid-word.
      if (event.ctrlKey && event.key === ' ') { event.preventDefault(); show(); }
      return;
    }
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      index = (index + 1) % matches.length;
      draw();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      index = (index - 1 + matches.length) % matches.length;
      draw();
    } else if (event.key === 'Enter' || event.key === 'Tab') {
      event.preventDefault();
      accept(matches[index]);
    } else if (event.key === 'Escape') {
      event.preventDefault();
      hide();
    }
  });
}

/// Turns the plain textarea into something that helps.
async function enhanceScriptEditor(node) {
  await loadOps();

  const text = $('textarea', node);
  const highlight = $('.highlight', node);
  const list = $('.completions', node);
  const bar = $('.signature', node);

  repaint(text, highlight);
  text.addEventListener('input', () => {
    if (!ops.list) loadOps().then(() => repaint(text, highlight)).catch(() => {});
    repaint(text, highlight);
  });
  text.addEventListener('scroll', () => {
    highlight.scrollTop = text.scrollTop;
    highlight.scrollLeft = text.scrollLeft;
  });

  setupSignature(text, bar);
  setupCompletion(text, list);
}
