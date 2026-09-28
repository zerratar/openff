// Crystal Style Sheets in the editor: the screen's stylesheets and its frames' style attributes
// cascaded, so the canvas shows the look (and the layout rules a sheet gives) as the client will.
// A port of Shared/Text/MenuStyles.cs, kept to the same rules; see that file for the language.
//
// The cascade is worked out on a copy of the screen (menuStyled), never on the file: the copy's
// frames carry the look as the client reads it (<colour>, <font>, <align>, <opacity>, <hidden/>,
// <window/>, <panel>, <tint>) and their layout rules in their style, and each is paired with its
// frame in the file, which is what is edited.

/// The sheets of the open screen's folder ({ name, css, dirty }): menu.sheets, loaded with the screen (and swapped
/// with it as the tabs change); menu.sheetsVersion climbs whenever one is edited, so the cascade is worked out again.
const menuSheets = {
  get list() { return (typeof menu !== 'undefined' && menu.sheets) || []; },
  get version() { return (typeof menu !== 'undefined' && menu.sheetsVersion) || 0; }
};

const STYLE_INHERITED = new Set(['color', 'font-size', 'text-align', 'visibility']);
const STYLE_LAYOUT = ['position', 'left', 'top', 'right', 'bottom', 'width', 'height', 'min-width', 'min-height', 'max-width', 'max-height', 'translate',
  'margin', 'margin-left', 'margin-top', 'margin-right', 'margin-bottom', 'padding', 'padding-left', 'padding-top', 'padding-right', 'padding-bottom',
  'flex-direction', 'gap', 'justify-content', 'align-items', 'align-self', 'flex-grow'];
/// The properties the language knows (a style or a sheet naming anything else is told so in the inspector).
const STYLE_KNOWN = new Set([...STYLE_LAYOUT, 'display', 'color', 'font-size', 'text-align', 'opacity', 'visibility', '-ff-panel', '-ff-tint']);

function styleDeclarations(text) {
  const out = [];
  for (const part of String(text || '').split(';')) {
    const colon = part.indexOf(':');
    if (colon <= 0) continue;
    const name = part.slice(0, colon).trim().toLowerCase();
    const value = part.slice(colon + 1).replace('!important', '').trim();
    if (name && value) out.push([name, value]);
  }
  return out;
}

function parseCompound(token) {
  const m = /^(\*|[A-Za-z][\w-]*)?((?:[#.][\w-]+)*)$/.exec(token);
  if (!m) return null;
  const c = { type: m[1] ? m[1].toLowerCase() : null, id: null, classes: [] };
  for (const part of m[2].match(/[#.][\w-]+/g) || []) {
    if (part[0] === '#') c.id = part.slice(1);
    else c.classes.push(part.slice(1));
  }
  return c;
}

function parseSelector(text) {
  if (!text || /[:\[+~]/.test(text)) return null;
  const selector = { parts: [], joins: [], specificity: 0, text };
  let join = ' ';
  for (const token of text.replace(/>/g, ' > ').split(/\s+/).filter(Boolean)) {
    if (token === '>') { join = '>'; continue; }
    const c = parseCompound(token);
    if (!c) return null;
    if (selector.parts.length) selector.joins.push(join);
    selector.parts.push(c);
    join = ' ';
    selector.specificity += (c.id ? 10000 : 0) + c.classes.length * 100 + (c.type && c.type !== '*' ? 1 : 0);
  }
  return selector.parts.length ? selector : null;
}

/// A sheet's rules: { selectors, declarations, order, sheet, selectorText }; rules this does not read are passed over.
function parseSheet(css, sheet, into, counter) {
  const text = String(css || '').replace(/\/\*[\s\S]*?\*\//g, ' ');
  let at = 0;
  while (at < text.length) {
    const open = text.indexOf('{', at);
    if (open < 0) break;
    const close = text.indexOf('}', open);
    if (close < 0) break;
    const head = text.slice(at, open).trim();
    const body = text.slice(open + 1, close);
    at = close + 1;
    if (head.startsWith('@')) continue;
    const selectors = head.split(',').map(s => parseSelector(s.trim())).filter(Boolean);
    if (!selectors.length) continue;
    into.push({ selectors, declarations: styleDeclarations(body), order: counter.n++, sheet, selectorText: head });
  }
}

function styleIdOf(e) {
  const tag = e.tagName === 'frame' ? 'id' : 'name';
  const child = [...e.children].find(c => c.tagName === tag);
  return child ? (child.getAttribute('value') ?? child.textContent).trim() : null;
}

function compoundMatches(c, e) {
  const name = e.tagName;
  if (name !== 'frame' && name !== 'menu' && name !== 'unit') return false;
  if (c.type && c.type !== '*') {
    const behaviour = [...e.children].find(x => x.tagName === 'behavior');
    const ok = c.type === 'menu' ? (name === 'menu' || name === 'unit')
      : c.type === 'frame' ? name === 'frame'
      : c.type === 'text' ? name === 'frame' && !!behaviour && behaviour.getAttribute('value') === 'Text'
      : c.type === 'window' ? name === 'frame' && [...e.children].some(x => x.tagName === 'window')
      : false;
    if (!ok) return false;
  }
  if (c.id && styleIdOf(e) !== c.id) return false;
  if (c.classes.length) {
    const has = new Set((e.getAttribute('class') || '').split(/\s+/).filter(Boolean));
    if (!c.classes.every(k => has.has(k))) return false;
  }
  return true;
}

function selectorMatches(selector, e) {
  const from = (index, el) => {
    if (!compoundMatches(selector.parts[index], el)) return false;
    if (index === 0) return true;
    const join = selector.joins[index - 1];
    for (let up = el.parentElement; up; up = up.parentElement) {
      if (from(index - 1, up)) return true;
      if (join === '>') return false;
    }
    return false;
  };
  return from(selector.parts.length - 1, e);
}

/// The rules that match an element, in cascade order, with the specificity each matched by.
function matchingRules(rules, e) {
  const found = [];
  for (const rule of rules) {
    let best = -1;
    for (const s of rule.selectors) if (selectorMatches(s, e)) best = Math.max(best, s.specificity);
    if (best >= 0) found.push({ rule, specificity: best });
  }
  return found.sort((a, b) => a.specificity - b.specificity || a.rule.order - b.rule.order);
}

function cascadeOf(e, rules, inherited) {
  const values = new Map();
  if (inherited) for (const [k, v] of inherited) if (STYLE_INHERITED.has(k)) values.set(k, v);
  for (const { rule } of matchingRules(rules, e)) for (const [k, v] of rule.declarations) values.set(k, v);
  for (const [k, v] of styleDeclarations(e.getAttribute('style'))) values.set(k, v);
  return values;
}

function styleOpacity(values, parent) {
  if (!values.has('opacity')) return parent;
  const t = values.get('opacity').trim();
  const o = t.endsWith('%') ? parseFloat(t) / 100 : Number(t);
  return Number.isFinite(o) ? parent * Math.min(1, Math.max(0, o)) : parent;
}

function styleHex(value) {
  const v = String(value || '').trim().toLowerCase();
  if (!/^#([0-9a-f]{3}|[0-9a-f]{6})$/.test(v)) return null;
  return v.length === 4 ? '#' + v[1] + v[1] + v[2] + v[2] + v[3] + v[3] : v;
}

function setLook(frame, tag, value) {
  let child = [...frame.children].find(c => c.tagName === tag);
  if (value === null) { if (child) child.remove(); return; }
  if (!child) { child = frame.ownerDocument.createElement(tag); frame.append(child); }
  child.textContent = value;
}

function bakeLook(frame, values, opacity) {
  const layout = STYLE_LAYOUT.filter(k => values.has(k)).map(k => [k, values.get(k)]);
  const display = (values.get('display') || '').trim().toLowerCase();
  if (display === 'none') layout.push(['display', 'none']);
  if (layout.length) frame.setAttribute('style', layout.map(([k, v]) => `${k}: ${v}`).join('; '));
  else frame.removeAttribute('style');

  if (values.has('color')) {
    const v = values.get('color');
    const colour = styleHex(v) || (/^[a-z][a-z0-9 -]*$/i.test(v.trim()) ? v.trim().toLowerCase() : null);
    if (colour) setLook(frame, 'colour', colour);
  }
  if (values.has('font-size')) {
    let word = values.get('font-size').trim().toLowerCase().replace(/px$/, '').trim();
    const n = parseInt(word, 10);
    if (word === 'large' || word === 'normal' || (String(n) === word && n >= 6 && n <= 31)) setLook(frame, 'font', word === 'normal' ? '12' : word);
  }
  if (values.has('text-align')) {
    let word = values.get('text-align').trim().toLowerCase();
    if (word === 'centre') word = 'center';
    if (['left', 'right', 'center', 'menu', 'button'].includes(word)) setLook(frame, 'align', word);
  }
  if (opacity < 0.999) setLook(frame, 'opacity', String(Math.round(opacity * 1000) / 1000));
  const hidden = (values.get('visibility') || '').trim().toLowerCase() === 'hidden' || display === 'none';
  if (hidden && ![...frame.children].some(c => c.tagName === 'hidden')) frame.append(frame.ownerDocument.createElement('hidden'));
  if (values.has('-ff-panel')) {
    const word = values.get('-ff-panel').trim().toLowerCase();
    if (word === 'none') { [...frame.children].filter(c => c.tagName === 'window' || c.tagName === 'panel').forEach(c => c.remove()); }
    else if (word === 'window' || word === 'bar') {
      if (![...frame.children].some(c => c.tagName === 'window')) frame.prepend(frame.ownerDocument.createElement('window'));
      setLook(frame, 'panel', word === 'bar' ? 'bar' : null);
    }
  }
  if (values.has('-ff-tint') && styleHex(values.get('-ff-tint'))) setLook(frame, 'tint', styleHex(values.get('-ff-tint')));
}

/// The rules of the open screen: its folder's sheets, then the file's own <style>s.
function menuRules(screen) {
  const rules = [];
  const counter = { n: 0 };
  for (const sheet of menuSheets.list) parseSheet(sheet.css, sheet.name, rules, counter);
  const doc = screen.ownerDocument;
  for (const style of doc.getElementsByTagName('style')) parseSheet(style.textContent, '<style>', rules, counter);
  return rules;
}

// The last cascade, kept until the file or the sheets change: a MutationObserver's pending records
// (taken synchronously) say whether anything in the file moved since.
let styledCache = null;
let styledObserver = null;
let styledDoc = null;

/// The screen with its styles cascaded: { copy, look (a Map of each frame in the file to its styled copy), computed (each frame's properties), rules }.
function menuStyled(screen) {
  const doc = screen.ownerDocument;
  if (styledDoc !== doc) {
    if (styledObserver) styledObserver.disconnect();
    styledObserver = new MutationObserver(() => { styledCache = null; });
    styledObserver.observe(doc, { subtree: true, childList: true, attributes: true, characterData: true });
    styledDoc = doc;
    styledCache = null;
  }
  if (styledObserver.takeRecords().length) styledCache = null;
  const preview = typeof menu !== 'undefined' && !!menu.preview;
  if (styledCache && styledCache.screen === screen && styledCache.version === menuSheets.version && styledCache.preview === preview) return styledCache;

  const rules = menuRules(screen);
  const copy = screen.cloneNode(true);
  [...copy.getElementsByTagName('style')].forEach(s => s.remove());
  const look = new Map();
  const computed = new Map();
  // Matched on the copy as it is baked, parent before child, as MenuStyles matches: a frame a sheet
  // gives a panel is a window to the selectors of the frames inside it.
  const screenValues = cascadeOf(copy, rules, null);
  const walk = (source, target, inherited, opacity) => {
    const sources = [...source.children].filter(e => e.tagName === 'frame');
    const targets = [...target.children].filter(e => e.tagName === 'frame');
    sources.forEach((frame, i) => {
      const t = targets[i];
      // In Preview, a class the frame binds (bind-class) is on or off as the sample data says.
      if (preview && typeof boundClasses === 'function' && frame.hasAttribute('bind-class')) {
        const scope = bindingScopeOf(frame);
        const classes = new Set((t.getAttribute('class') || '').split(/\s+/).filter(Boolean));
        for (const [name, expression] of boundClasses(frame)) { if (bindingTest(expression, scope)) classes.add(name); else classes.delete(name); }
        if (classes.size) t.setAttribute('class', [...classes].join(' ')); else t.removeAttribute('class');
      }
      const values = cascadeOf(t, rules, inherited);
      const own = styleOpacity(values, opacity);
      look.set(frame, t);
      computed.set(frame, values);
      bakeLook(t, values, own);
      walk(frame, t, values, own);
    });
  };
  walk(screen, copy, screenValues, styleOpacity(screenValues, 1));
  styledCache = { screen, version: menuSheets.version, preview, copy, look, computed, rules };
  return styledCache;
}

/// A frame's look as the client will read it: its styled copy (the frame itself when there is none).
function frameLook(element) {
  const screen = frameScreen(element);
  return (screen && menuStyled(screen).look.get(element)) || element;
}

/// The sheet rules that reach a frame, for the inspector: [{ selector, sheet, declarations, specificity }], the winning last.
function frameRules(element) {
  const screen = frameScreen(element);
  if (!screen) return [];
  const styled = menuStyled(screen);
  const copy = styled.look.get(element);
  if (!copy) return [];
  return matchingRules(styled.rules, copy).map(({ rule, specificity }) => ({
    selector: rule.selectors.filter(s => selectorMatches(s, copy)).map(s => s.text).join(', '),
    sheet: rule.sheet, declarations: rule.declarations, specificity
  }));
}
