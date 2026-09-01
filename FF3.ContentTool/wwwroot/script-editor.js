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

const ops = { list: null, byName: new Map() };

async function loadOps() {
  if (ops.list) return ops.list;
  ops.list = await api('/api/ops');
  for (const op of ops.list) ops.byName.set(op.name, op);
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

const escapeHtml = text => text
  .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

/// One line of source as highlighted HTML.
function highlightLine(line) {
  const comment = line.indexOf('//');
  const code = comment >= 0 ? line.slice(0, comment) : line;
  const trailing = comment >= 0 ? line.slice(comment) : '';

  let html = '';
  let rest = code;

  // A label definition owns its whole line up to the colon.
  const label = rest.match(/^(\s*)([A-Za-z_][\w.]*)(:)/);
  if (label) {
    html += label[1] + `<i class="t-label">${escapeHtml(label[2])}</i>:`;
    rest = rest.slice(label[0].length);
  } else {
    const first = rest.match(/^(\s*)([A-Za-z_][\w.]*)/);
    if (first) {
      const word = first[2];
      const known = ops.byName.has(word);
      const kind = ['map', 'cast', 'function', 'data'].includes(word) ? 't-key'
        : known ? 't-op' : 't-unknown';
      html += first[1] + `<i class="${kind}">${escapeHtml(word)}</i>`;
      rest = rest.slice(first[0].length);
    }
  }

  // Then numbers, strings, keywords and bare words - the arguments.
  const token = /("(?:[^"\\]|\\.)*")|(-?0[xX][0-9a-fA-F]+|-?\d+)|\b(none|init|main|exit)\b|([A-Za-z_][\w.]*)/g;
  let at = 0;
  let match;
  while ((match = token.exec(rest)) !== null) {
    html += escapeHtml(rest.slice(at, match.index));
    const text = escapeHtml(match[0]);
    if (match[1]) html += `<i class="t-string">${text}</i>`;
    else if (match[2]) html += `<i class="t-number">${text}</i>`;
    else if (match[3]) html += `<i class="t-key">${text}</i>`;
    else html += `<i class="t-name">${text}</i>`;
    at = match.index + match[0].length;
  }
  html += escapeHtml(rest.slice(at));

  if (trailing) html += `<i class="t-comment">${escapeHtml(trailing)}</i>`;
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

  const args = withoutComment.slice(words[0].length);
  const argument = args.trim().length === 0 && !args.includes(',')
    ? 0
    : args.split(',').length - 1;
  return { op, argument };
}

function setupSignature(text, bar) {
  const update = () => {
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
    const suffix = op.operands.length ? ' ' : '';
    text.value = text.value.slice(0, from) + op.name + suffix + text.value.slice(start);
    const caret = from + op.name.length + suffix.length;
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
  text.addEventListener('input', () => repaint(text, highlight));
  text.addEventListener('scroll', () => {
    highlight.scrollTop = text.scrollTop;
    highlight.scrollLeft = text.scrollLeft;
  });

  setupSignature(text, bar);
  setupCompletion(text, list);
}
