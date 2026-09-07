// The mod's own files: C# under code/, scene files under scenes/, project.json.
//
// The project tree lists them under an "OpenFF mod" folder at the bottom, beside the
// games' libraries, and this is the view that opens one: a plain editor with the same
// highlighted <pre> behind a transparent <textarea> the script editor uses, Save
// (Ctrl+S), Build, and Open in IDE for whoever would rather have Visual Studio, Rider
// or VS Code - the file is theirs either way; this is only the shorter way to a small
// change. A build's errors land under the editor, and clicking one puts the caret on
// the line.

'use strict';

// ------------------------------------------------------------- highlighting

const CS_KEYWORDS = new Set(('abstract as base bool break byte case catch char checked class '
  + 'const continue decimal default delegate do double else enum event explicit extern false '
  + 'finally fixed float for foreach goto if implicit in int interface internal is lock long '
  + 'namespace new null object operator out override params private protected public readonly '
  + 'ref return sbyte sealed short sizeof stackalloc static string struct switch this throw '
  + 'true try typeof uint ulong unchecked unsafe ushort using var virtual void volatile while '
  + 'yield async await get set init record when where nameof partial').split(' '));

const JSON_WORDS = new Set(['true', 'false', 'null']);

/// Which highlighter a file wants, from its name.
function languageOf(name) {
  if (/\.cs$/i.test(name)) return 'cs';
  if (/\.json$/i.test(name)) return 'json';
  if (/\.(csproj|props|targets|xml|config)$/i.test(name)) return 'xml';
  return 'text';
}

/// One line as HTML. `block` is the comment state carried from the line before
/// (/* */ in C#, <!-- --> in XML), and the state after this line comes back with it.
function highlightCodeLine(line, lang, block) {
  let html = '';
  let i = 0;
  const n = line.length;
  const push = (cls, text) => {
    html += cls ? '<i class="' + cls + '">' + escapeHtml(text) + '</i>' : escapeHtml(text);
  };

  while (i < n) {
    if (block === 'comment') {
      const closer = lang === 'xml' ? '-->' : '*/';
      const end = line.indexOf(closer, i);
      if (end < 0) { push('t-comment', line.slice(i)); i = n; break; }
      push('t-comment', line.slice(i, end + closer.length));
      i = end + closer.length;
      block = null;
      continue;
    }

    const rest = line.slice(i);
    let m;

    if (lang === 'cs' && rest.startsWith('//')) { push('t-comment', rest); i = n; break; }
    if (lang === 'cs' && rest.startsWith('/*')) { push('t-comment', '/*'); i += 2; block = 'comment'; continue; }
    if (lang === 'xml' && rest.startsWith('<!--')) { push('t-comment', '<!--'); i += 4; block = 'comment'; continue; }

    // Strings: "..." with escapes; C# also @"..." (doubled quotes) and '...'.
    if (lang === 'cs' && (m = rest.match(/^\$?@\$?"(?:[^"]|"")*"?/))) {
      push('t-string', m[0]); i += m[0].length; continue;
    }
    if ((m = rest.match(/^\$?"(?:[^"\\]|\\.)*"?/))) {
      // A JSON key is a string with a colon after it.
      const key = lang === 'json' && /^\s*:/.test(rest.slice(m[0].length));
      push(key ? 't-label' : 't-string', m[0]); i += m[0].length; continue;
    }
    if (lang === 'cs' && (m = rest.match(/^'(?:[^'\\]|\\.)*'?/))) {
      push('t-string', m[0]); i += m[0].length; continue;
    }

    // XML: the tag names and the brackets.
    if (lang === 'xml' && (m = rest.match(/^<\/?[A-Za-z_][\w.:-]*|^\/?>/))) {
      push('t-key', m[0]); i += m[0].length; continue;
    }

    // Numbers, when not the tail of a name.
    if ((m = rest.match(/^-?(?:0[xX][0-9a-fA-F_]+|\d[\d_]*(?:\.\d+)?(?:[eE][-+]?\d+)?[fFdDmMuUlL]*)/))
      && !/[A-Za-z_\d]/.test(i > 0 ? line[i - 1] : '')) {
      push('t-number', m[0]); i += m[0].length; continue;
    }

    // Words.
    if ((m = rest.match(/^@?[A-Za-z_][\w]*/))) {
      const word = m[0];
      let cls = 't-name';
      if (lang === 'cs') {
        cls = CS_KEYWORDS.has(word) ? 't-key' : /^[A-Z]/.test(word) ? 't-type' : 't-name';
      } else if (lang === 'json') {
        cls = JSON_WORDS.has(word) ? 't-key' : 't-name';
      } else if (lang === 'xml') {
        cls = 't-name';
      }
      push(cls, word); i += word.length; continue;
    }

    push(null, line[i]);
    i++;
  }
  return { html, block };
}

function highlightCode(source, lang) {
  if (lang === 'text') return escapeHtml(source) + '\n';
  let block = null;
  const out = [];
  for (const line of source.split('\n')) {
    const result = highlightCodeLine(line, lang, block);
    block = result.block;
    out.push(result.html);
  }
  return out.join('\n') + '\n';
}

// ------------------------------------------------------------------ the view

/// The last build's problems, so a file opened after the build shows its share.
let lastBuildProblems = null;

/// The view for one of the mod's files. (openCode, in project.js, is the csproj in the IDE.)
async function openCodeFile(name) {
  const data = await api(`/api/project/file?name=${encodeURIComponent(name)}`);
  if (data.ok === false) throw new Error(data.error);
  const doc = activeDoc;
  setDocData(data);

  const node = view('code', name, false);
  const text = $('textarea', node);
  const gutter = $('.gutter', node);
  const highlight = $('.highlight', node);
  const problems = $('.problems', node);
  const dirty = $('.badge.dirty', node);
  const lang = languageOf(name);

  text.value = data.text;
  text.readOnly = Boolean(data.readOnly);
  $('.badge.readonly', node).classList.toggle('on', Boolean(data.readOnly));
  $('.save', node).disabled = Boolean(data.readOnly);
  // A textarea holds LF whatever the file had; the file's own line ending goes back
  // on save, so a CRLF file written by Visual Studio stays one.
  const eol = data.text.includes('\r\n') ? '\r\n' : '\n';
  let saved = text.value;

  let outlineTimer = null;
  const repaint = () => {
    const lines = text.value.split('\n').length;
    gutter.textContent = Array.from({ length: lines }, (_, i) => i + 1).join('\n');
    highlight.innerHTML = highlightCode(text.value, lang);
    highlight.scrollTop = text.scrollTop;
    highlight.scrollLeft = text.scrollLeft;
    gutter.scrollTop = text.scrollTop;
    dirty.classList.toggle('on', text.value !== saved);
    // The hierarchy reads the declarations off the text; not on every keystroke.
    if (doc && doc.data) doc.data.text = text.value;
    clearTimeout(outlineTimer);
    outlineTimer = setTimeout(() => { if (activeDoc === doc) drawHierarchy(); }, 600);
  };
  repaint();
  clearTimeout(outlineTimer);

  text.addEventListener('input', repaint);
  text.addEventListener('scroll', () => {
    highlight.scrollTop = text.scrollTop;
    highlight.scrollLeft = text.scrollLeft;
    gutter.scrollTop = text.scrollTop;
  });

  // Tab indents rather than leaving the editor; with lines selected it indents them
  // all, and shift+tab takes one level off.
  text.addEventListener('keydown', event => {
    if (event.key === 'Tab' && !text.readOnly) {
      event.preventDefault();
      indentSelection(text, event.shiftKey);
      repaint();
    } else if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') {
      event.preventDefault();
      save().catch(e => say(e.message, 'bad'));
    }
  });

  async function save() {
    if (text.readOnly) return;
    say('saving…');
    const result = await api('/api/project/file/save', { name, text: text.value.replace(/\n/g, eol) });
    if (!result.ok) throw new Error(result.error);
    saved = text.value;
    repaint();
    say(`saved ${shortName(name)} - Build to compile it`, 'good');
    // The list marks files changed since the last build.
    if (state.browse === 'code') loadList().catch(() => {});
  }

  $('.save', node).onclick = () => save().catch(e => say(e.message, 'bad'));
  $('.ide', node).onclick = async () => {
    try {
      const result = await api('/api/project/file/open', { name });
      if (!result.ok) throw new Error(result.error);
      say(`opened ${shortName(name)} in your editor`, 'good');
    } catch (error) {
      say(error.message, 'bad');
    }
  };
  $('.build', node).onclick = async () => {
    if (text.value !== saved && !text.readOnly) {
      try { await save(); } catch (error) { say(error.message, 'bad'); return; }
    }
    if (typeof buildCode === 'function') buildCode();
  };

  // The build's problems for this file, with a click that goes to the line.
  const showProblems = list => {
    problems.textContent = '';
    for (const problem of (list || []).filter(p => sameFile(p.file, name))) {
      const item = document.createElement('li');
      if (problem.kind === 'warning') item.className = 'warning';
      const where = document.createElement('b');
      where.textContent = `line ${problem.line}`;
      item.append(where, document.createTextNode(`${problem.kind} ${problem.code}: ${problem.message}`));
      item.onclick = () => goToLine(text, problem.line, problem.column);
      problems.append(item);
    }
  };
  if (doc) doc.showBuild = showProblems;
  if (lastBuildProblems) showProblems(lastBuildProblems);
}

function sameFile(a, b) {
  return (a || '').replace(/\\/g, '/').toLowerCase() === (b || '').replace(/\\/g, '/').toLowerCase();
}

/// Tab and shift+tab in the editor. One caret gets a tab; a selection spanning lines
/// gets every one of them indented or outdented and stays selected.
function indentSelection(text, outdent) {
  const start = text.selectionStart;
  const end = text.selectionEnd;
  const value = text.value;
  const lineStart = value.lastIndexOf('\n', start - 1) + 1;
  const multi = value.slice(start, end).includes('\n');
  if (!multi && !outdent) {
    text.value = value.slice(0, start) + '\t' + value.slice(end);
    text.setSelectionRange(start + 1, start + 1);
    return;
  }
  let lineEnd = value.indexOf('\n', end);
  if (lineEnd < 0) lineEnd = value.length;
  const block = value.slice(lineStart, lineEnd);
  const lines = block.split('\n');
  const changed = lines.map(line => outdent ? line.replace(/^(\t| {1,4})/, '') : '\t' + line);
  const replaced = changed.join('\n');
  text.value = value.slice(0, lineStart) + replaced + value.slice(lineEnd);
  if (multi) {
    text.setSelectionRange(lineStart, lineStart + replaced.length);
  } else {
    const shift = changed[0].length - lines[0].length;
    const at = Math.max(lineStart, start + shift);
    text.setSelectionRange(at, at);
  }
}

/// After a build: every open code document shows its share of the problems, and the
/// first error's file is opened if nothing shows it yet - what an IDE does.
async function showBuildProblems(list) {
  lastBuildProblems = list || [];
  for (const doc of docs.values()) {
    if (doc.kind === 'code' && doc.showBuild) doc.showBuild(lastBuildProblems);
  }
  const first = lastBuildProblems.find(p => p.kind === 'error');
  if (!first) return;
  const shown = [...docs.values()].some(d => d.kind === 'code' && sameFile(d.name, first.file));
  if (!shown) {
    const opened = await openDoc('code', first.file, { preview: true }).catch(() => null);
    if (opened && opened.showBuild) opened.showBuild(lastBuildProblems);
  }
}

// ------------------------------------------------------------ the outline

/// The declarations of a C# file, or the contents of a scene file, for the hierarchy.
function outlineForCode(doc) {
  const source = doc.data && doc.data.text;
  if (typeof source !== 'string') return [];
  const lang = languageOf(doc.name);

  if (lang === 'cs') {
    const lineOf = index => source.slice(0, index).split('\n').length;
    const groups = [];
    const types = [];
    const typePattern = /^[ \t]*(?:(?:public|private|protected|internal|static|abstract|sealed|partial)\s+)*(class|struct|interface|enum|record)\s+([A-Za-z_]\w*)(?:\s*:\s*([A-Za-z_][\w.<>]*))?/gm;
    let match;
    while ((match = typePattern.exec(source))) {
      const line = lineOf(match.index);
      types.push({
        label: match[2],
        note: match[3] ? `${match[1]} : ${match[3]}` : match[1],
        ref: `line:${line}`,
        icon: 'code',
        reveal: () => revealLine(doc, line)
      });
    }
    if (types.length) groups.push({ label: 'Types', children: types });

    const members = [];
    const memberPattern = /^[ \t]*(?:(?:public|private|protected|internal|static|override|virtual|abstract|async|sealed|new|partial|extern|readonly|const)\s+)+(?!class\b|struct\b|interface\b|enum\b|record\b|return\b|new\b|throw\b)([\w<>\[\],.?]+)\s+([A-Za-z_]\w*)\s*(\(|=|;|\{)/gm;
    while ((match = memberPattern.exec(source))) {
      const line = lineOf(match.index);
      const kind = match[3] === '(' ? 'method' : match[3] === '{' ? 'property' : 'field';
      members.push({
        label: match[2],
        note: kind === 'method' ? `${match[1]}()` : match[1],
        ref: `line:${line}`,
        icon: kind === 'method' ? 'logic' : 'file',
        reveal: () => revealLine(doc, line)
      });
    }
    if (members.length) groups.push({ label: 'Members', children: members });
    return groups;
  }

  if (lang === 'json') {
    let parsed;
    try { parsed = JSON.parse(source); } catch (error) { return [{ label: 'JSON', children: [{ label: 'does not parse: ' + error.message, ref: 'bad', icon: 'file' }] }]; }
    const groups = [];
    if (parsed && Array.isArray(parsed.attachments)) {
      groups.push({
        label: 'Behaviours',
        children: parsed.attachments.map((a, i) => ({
          label: `${a.target || '?'} → ${a.behaviour || '?'}`,
          note: Object.keys(a.fields || {}).length ? `${Object.keys(a.fields).length} fields` : '',
          ref: `attachment:${i}`,
          icon: 'logic'
        }))
      });
    }
    if (parsed && Array.isArray(parsed.points)) {
      groups.push({
        label: 'Points',
        children: parsed.points.map((p, i) => ({
          label: p.name || `point ${i}`,
          note: (p.tags || []).join(' '),
          ref: `point:${i}`,
          icon: 'exit'
        }))
      });
    }
    if (!groups.length && parsed && typeof parsed === 'object') {
      groups.push({
        label: 'Keys',
        children: Object.keys(parsed).map(key => ({
          label: key,
          note: Array.isArray(parsed[key]) ? `${parsed[key].length} items` : typeof parsed[key] === 'object' && parsed[key] ? 'object' : String(parsed[key]),
          ref: `key:${key}`,
          icon: 'file'
        }))
      });
    }
    return groups;
  }
  return [];
}

/// The facts the inspector shows for one of the mod's files.
function codeFacts(data) {
  const facts = [];
  if (!data) return facts;
  const kinds = { cs: 'C# source', csproj: 'C# project', json: 'JSON' };
  facts.push(['kind', kinds[data.kind] || 'text']);
  if (typeof data.text === 'string') facts.push(['lines', data.text.split('\n').length]);
  if (data.bytes !== undefined) facts.push(['bytes', data.bytes]);
  if (data.modified) facts.push(['modified', new Date(data.modified).toLocaleString()]);
  if (data.readOnly) facts.push(['edited through', 'File ▸ Project settings…']);
  return facts;
}

/// The first lines of a file, highlighted, for the inspector's preview.
function codePeek(name, data) {
  if (!data || typeof data.text !== 'string') return null;
  const pre = document.createElement('pre');
  pre.className = 'code-peek';
  const lines = data.text.split('\n');
  pre.innerHTML = highlightCode(lines.slice(0, 18).join('\n'), languageOf(name))
    + (lines.length > 18 ? '<i class="t-comment">… ' + (lines.length - 18) + ' more lines</i>\n' : '');
  return pre;
}
