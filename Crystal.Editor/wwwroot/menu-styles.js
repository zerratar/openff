// Crystal Style Sheets in the editor: the screen's stylesheets and its frames' style attributes
// cascaded, so the canvas shows the look (and the layout rules a sheet gives) as the client will.
// A port of Shared/Text/MenuStyles.cs, kept to the same rules; see that file for the language.
//
// The cascade is worked out on a copy of the screen (menuStyled), never on the file: the copy's
// frames carry the look as the client reads it (<colour>, <font>, <align>, <opacity>, <hidden/>,
// <window/>, <panel>, <tint>, <background>, <textstyle>, <transition>, <animation>) and their layout
// rules in their style, and each is paired with its frame in the file, which is what is edited.
//
// The pseudo-classes' states (:focus, :disabled) are the preview's (menu.previewState), put on the
// copy as data-ff-state before it is cascaded - as the client puts them on the screen it restyles.
// The file never has them, so a save never writes them.

/// The sheets of the open screen's folder ({ name, css, dirty }): menu.sheets, loaded with the screen (and swapped
/// with it as the tabs change); menu.sheetsVersion climbs whenever one is edited, so the cascade is worked out again.
const menuSheets = {
  get list() { return (typeof menu !== 'undefined' && menu.sheets) || []; },
  get version() { return (typeof menu !== 'undefined' && menu.sheetsVersion) || 0; }
};

/// The lettering (Shared/Text/MenuText.cs): inherited, and baked into <textstyle>.
const TEXT_STYLE_PROPS = ['text-shadow', 'font-family', 'font-weight', 'font-style', 'letter-spacing', 'line-height',
  'text-decoration', 'text-decoration-line', 'text-transform', '-ff-text-stroke', '-webkit-text-stroke'];
/// How the look moves (Shared/Text/MenuAnimation.cs): baked into <transition> and <animation>.
const TRANSITION_PROPS = ['transition', 'transition-property', 'transition-duration', 'transition-timing-function', 'transition-delay'];
const ANIMATION_PROPS = ['animation', 'animation-name', 'animation-duration', 'animation-timing-function', 'animation-delay',
  'animation-iteration-count', 'animation-direction', 'animation-fill-mode', 'animation-play-state'];
const STYLE_INHERITED = new Set(['color', 'font-size', 'text-align', 'visibility', ...TEXT_STYLE_PROPS]);
const STYLE_LAYOUT = ['position', 'left', 'top', 'right', 'bottom', 'width', 'height', 'min-width', 'min-height', 'max-width', 'max-height', 'translate',
  'margin', 'margin-left', 'margin-top', 'margin-right', 'margin-bottom', 'padding', 'padding-left', 'padding-top', 'padding-right', 'padding-bottom',
  'flex-direction', 'gap', 'justify-content', 'align-items', 'align-self', 'flex-grow'];
/// A frame's background and the box round it (menu-background.js; Shared/Text/MenuBackground.cs), in the order the bake writes them.
const BACKGROUND_PROPS = ['background-color', 'background-image', '-ff-background-rect', '-ff-background-tint', '-ff-background-scale-mode',
  'background-size', 'background-position', 'background-repeat',
  '-ff-slice', '-ff-slice-left', '-ff-slice-top', '-ff-slice-right', '-ff-slice-bottom', '-ff-slice-scale', '-ff-slice-type', '-ff-background-filter', '-ff-sprite',
  'border', 'border-width', 'border-color', 'border-style',
  'border-top', 'border-right', 'border-bottom', 'border-left',
  'border-top-width', 'border-right-width', 'border-bottom-width', 'border-left-width',
  'border-top-color', 'border-right-color', 'border-bottom-color', 'border-left-color',
  'border-radius', 'border-top-left-radius', 'border-top-right-radius', 'border-bottom-right-radius', 'border-bottom-left-radius',
  'box-shadow'];
/// The properties the language knows (a style or a sheet naming anything else is told so in the inspector).
const STYLE_KNOWN = new Set([...STYLE_LAYOUT, 'display', 'color', 'font-size', 'text-align', 'opacity', 'visibility', '-ff-panel', '-ff-tint',
  ...BACKGROUND_PROPS, ...TEXT_STYLE_PROPS, ...TRANSITION_PROPS, ...ANIMATION_PROPS]);
/// The attribute that carries a frame's states for the pseudo-classes ("focus disabled"), on the copy the cascade runs on.
const STYLE_STATE = 'data-ff-state';

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

/// A comma list split at its top level (not inside brackets: cubic-bezier(a, b, c, d) stays one) - MenuAnimation.CommaList.
function cssCommaList(text) {
  const items = [];
  const t = String(text || '');
  if (!t.trim()) return items;
  let depth = 0, start = 0;
  for (let i = 0; i < t.length; i++) {
    const c = t[i];
    if (c === '(') depth++;
    else if (c === ')') depth = Math.max(0, depth - 1);
    else if (c === ',' && depth === 0) { items.push(t.slice(start, i).trim()); start = i + 1; }
  }
  items.push(t.slice(start).trim());
  return items.filter(s => s.length > 0);
}

/// Words split at spaces at the top level (not inside brackets) - MenuAnimation.Words.
function cssWords(text) {
  const words = [];
  const t = String(text || '');
  let depth = 0, w = '';
  for (const c of t) {
    if (c === '(') depth++;
    else if (c === ')') depth = Math.max(0, depth - 1);
    if (/\s/.test(c) && depth === 0) { if (w) { words.push(w); w = ''; } continue; }
    w += c;
  }
  if (w) words.push(w);
  return words;
}

const STYLE_PSEUDOS = new Set(['focus', 'focus-within', 'disabled', 'enabled', 'first-child', 'last-child', 'only-child', 'nth-child', 'nth-last-child', 'not']);

/// As CSS counts it: an id, then classes and pseudo-classes (a :not() by what it holds), then a type.
function compoundSpecificity(c) {
  return (c.id ? 10000 : 0) + c.classes.length * 100 + c.pseudos.reduce((s, p) => s + (p.name === 'not' ? (p.not ? compoundSpecificity(p.not) : 0) : 100), 0) + (c.type && c.type !== '*' ? 1 : 0);
}

function parseCompound(token) {
  const m = /^(\*|[A-Za-z][\w-]*)?((?:[#.][\w-]+|:[\w-]+(?:\([^)]*\))?)*)$/.exec(token);
  if (!m) return null;
  const c = { type: m[1] ? m[1].toLowerCase() : null, id: null, classes: [], pseudos: [] };
  for (const part of m[2].matchAll(/[#.][\w-]+|:([\w-]+)(?:\(([^)]*)\))?/g)) {
    if (part[0][0] === '#') c.id = part[0].slice(1);
    else if (part[0][0] === '.') c.classes.push(part[0].slice(1));
    else {
      const name = part[1].toLowerCase();
      if (!STYLE_PSEUDOS.has(name)) return null;   // :hover and the rest: the rule is passed over
      const argument = part[2] !== undefined ? part[2] : null;
      let not = null;
      if (name === 'not') {
        not = argument === null ? null : parseCompound(argument);
        if (!not) return null;
      } else if ((name === 'nth-child' || name === 'nth-last-child') && !(argument || '').trim()) return null;
      c.pseudos.push({ name, argument, not });
    }
  }
  return c;
}

/// A selector, or null for one this does not read (an attribute, a pseudo-element, a pseudo-class not known) - its rule is passed over.
function parseSelector(text) {
  // A + inside brackets (:nth-child(2n+1)) is no combinator.
  if (!text || /[\[+~]/.test(text.replace(/\([^)]*\)/g, '()')) || text.includes('::')) return null;
  // Spaces inside brackets (":nth-child(2n + 1)", ":not(.a)") are no combinators.
  const tight = text.replace(/\([^)]*\)/g, m => m.replace(/ /g, ''));
  const selector = { parts: [], joins: [], specificity: 0, text };
  let join = ' ';
  for (const token of tight.replace(/>/g, ' > ').split(/\s+/).filter(Boolean)) {
    if (token === '>') { join = '>'; continue; }
    const c = parseCompound(token);
    if (!c) return null;
    if (selector.parts.length) selector.joins.push(join);
    selector.parts.push(c);
    join = ' ';
    selector.specificity += compoundSpecificity(c);
  }
  return selector.parts.length ? selector : null;
}

/// The top-level blocks of a sheet: each one's head and what is between its braces (a nested block kept whole, as @keyframes has them).
function styleBlocks(text) {
  const blocks = [];
  let at = 0;
  while (at < text.length) {
    const open = text.indexOf('{', at);
    if (open < 0) break;
    let depth = 1, close = open + 1;
    for (; close < text.length && depth > 0; close++) {
      if (text[close] === '{') depth++;
      else if (text[close] === '}') depth--;
    }
    if (depth > 0) break;
    let head = text.slice(at, open).trim();
    // A declaration left before a block ("a: b; x { }") is no part of its head.
    const semi = head.lastIndexOf(';');
    if (semi >= 0) head = head.slice(semi + 1).trim();
    blocks.push([head, text.slice(open + 1, close - 1)]);
    at = close;
  }
  return blocks;
}

/// A sheet's rules: { selectors, declarations, order, sheet, selectorText }; rules this does not read are passed over.
/// Its @keyframes go into `keyframes` (name -> [{ offset, values: Map }], the last of a name winning).
function parseSheet(css, sheet, into, counter, keyframes) {
  const text = String(css || '').replace(/\/\*[\s\S]*?\*\//g, ' ');
  for (const [head, body] of styleBlocks(text)) {
    const low = head.toLowerCase();
    if (low.startsWith('@keyframes') || low.startsWith('@-webkit-keyframes')) {
      const name = head.slice(low.indexOf('keyframes') + 9).trim().replace(/^["']+|["']+$/g, '');
      if (!name || !keyframes) continue;
      const stops = [];
      for (const [at, declarations] of styleBlocks(body)) {
        for (const one of at.split(',')) {
          const o = one.trim().toLowerCase();
          const p = parseFloat(o);
          const offset = o === 'from' ? 0 : o === 'to' ? 1 : o.endsWith('%') && Number.isFinite(p) ? Math.min(1, Math.max(0, p / 100)) : null;
          if (offset === null) continue;
          const values = new Map();
          for (const [k, v] of styleDeclarations(declarations)) values.set(k, v);
          stops.push({ offset, values });
        }
      }
      keyframes.set(name, stops.sort((a, b) => a.offset - b.offset));
      continue;
    }
    if (head.startsWith('@')) continue;   // @media and the rest: not for a menu
    const selectors = cssCommaList(head).map(s => parseSelector(s.trim())).filter(Boolean);
    if (!selectors.length) continue;
    into.push({ selectors, declarations: styleDeclarations(body), order: counter.n++, sheet, selectorText: head });
  }
}

function styleIdOf(e) {
  const tag = e.tagName === 'frame' ? 'id' : 'name';
  const child = [...e.children].find(c => c.tagName === tag);
  return child ? (child.getAttribute('value') ?? child.textContent).trim() : null;
}

function styleHasState(e, state) {
  return (e.getAttribute(STYLE_STATE) || '').split(' ').includes(state);
}

/// Whether the n-th (from 1) is picked by An+B, odd or even.
function styleNth(formula, n) {
  let f = String(formula || '').replace(/ /g, '').toLowerCase();
  if (f === 'odd') f = '2n+1';
  else if (f === 'even') f = '2n';
  const m = /^(?:([+-]?\d*)n)?([+-]?\d+)?$/.exec(f);
  if (!m || !f.length) return false;
  const hasN = f.includes('n');
  const g1 = m[1] || '';
  const a = !hasN ? 0 : g1 === '' || g1 === '+' ? 1 : g1 === '-' ? -1 : parseInt(g1, 10);
  const b = m[2] ? parseInt(m[2], 10) : 0;
  if (a === 0) return n === b;
  const k = n - b;
  return k % a === 0 && k / a >= 0;
}

function stylePseudo(e, p) {
  switch (p.name) {
    case 'focus': return styleHasState(e, 'focus');
    case 'focus-within': return styleHasState(e, 'focus') || [...e.getElementsByTagName('frame')].some(d => styleHasState(d, 'focus'));
    case 'disabled': return styleHasState(e, 'disabled');
    case 'enabled': return !styleHasState(e, 'disabled');
    case 'not': return !!p.not && !compoundMatches(p.not, e);
  }
  const siblings = e.parentElement ? [...e.parentElement.children].filter(x => x.tagName === e.tagName) : [e];
  const index = siblings.indexOf(e);
  switch (p.name) {
    case 'first-child': return index === 0;
    case 'last-child': return index === siblings.length - 1;
    case 'only-child': return siblings.length === 1;
    case 'nth-child': return styleNth(p.argument, index + 1);
    case 'nth-last-child': return styleNth(p.argument, siblings.length - index);
  }
  return false;
}

function compoundMatches(c, e) {
  const name = e.tagName;
  if (name !== 'frame' && name !== 'menu' && name !== 'unit') return false;
  for (const p of c.pseudos || []) if (!stylePseudo(e, p)) return false;
  if (c.type && c.type !== '*') {
    const behaviour = [...e.children].find(x => x.tagName === 'behavior');
    const ok = c.type === 'menu' ? (name === 'menu' || name === 'unit')
      : c.type === 'frame' ? name === 'frame'
      : c.type === 'text' ? name === 'frame' && !!behaviour && behaviour.getAttribute('value') === 'Text'
      : c.type === 'window' ? name === 'frame' && [...e.children].some(x => x.tagName === 'window')
      : c.type === 'portrait' ? name === 'frame' && [...e.children].some(x => x.tagName === 'portrait')
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

/// Declarations of a group of properties as one element (<textstyle>, <transition>, <animation>), as MenuStyles.Element writes them.
function bakeGroup(frame, tag, props, values, draws) {
  const said = props.filter(k => values.has(k)).map(k => `${k}: ${values.get(k)}`);
  if (!said.length) return;
  const declarations = said.join('; ');
  setLook(frame, tag, draws(declarations) ? declarations : null);
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
    // No window - and said so, for a frame whose window the game makes whatever the layout says (field_hud's dialogue and map name).
    if (word === 'none') { [...frame.children].filter(c => c.tagName === 'window').forEach(c => c.remove()); setLook(frame, 'panel', 'none'); }
    else if (word === 'window' || word === 'bar') {
      if (![...frame.children].some(c => c.tagName === 'window')) frame.prepend(frame.ownerDocument.createElement('window'));
      setLook(frame, 'panel', word === 'bar' ? 'bar' : null);
    }
  }
  if (values.has('-ff-tint') && styleHex(values.get('-ff-tint'))) setLook(frame, 'tint', styleHex(values.get('-ff-tint')));
  // The background: its declarations as one element, as MenuStyles bakes them.
  const background = BACKGROUND_PROPS.filter(k => values.has(k)).map(k => `${k}: ${values.get(k)}`);
  if (background.length) setLook(frame, 'background', typeof parseBackground !== 'function' || parseBackground(background.join('; ')) ? background.join('; ') : null);
  // The lettering, and how the look moves: their declarations as one element each.
  bakeGroup(frame, 'textstyle', TEXT_STYLE_PROPS, values, d => typeof parseTextStyle !== 'function' || !!parseTextStyle(d));
  bakeGroup(frame, 'transition', TRANSITION_PROPS, values, () => true);
  bakeGroup(frame, 'animation', ANIMATION_PROPS, values, () => true);
}

/// One frame's look from its properties (the cascade's, or what the animator made of them): baked onto a copy of
/// it (its frames left out) and handed back, as MenuStyles.LookOf does.
function frameLookOf(frame, values, opacity) {
  const copy = frame.cloneNode(false);
  for (const child of frame.children) if (child.tagName !== 'frame') copy.append(child.cloneNode(true));
  bakeLook(copy, values, opacity);
  return copy;
}

/// The rules of the open screen: its folder's sheets, then the file's own <style>s; and their @keyframes.
function menuRules(screen, keyframes) {
  const rules = [];
  const counter = { n: 0 };
  for (const sheet of menuSheets.list) parseSheet(sheet.css, sheet.name, rules, counter, keyframes);
  const doc = screen.ownerDocument;
  for (const style of doc.getElementsByTagName('style')) parseSheet(style.textContent, '<style>', rules, counter, keyframes);
  return rules;
}

/// The preview's states (menu.previewState): the frame the cursor is on, the frames taken as disabled. The editor's alone - never in the file.
function menuPreviewState() {
  if (typeof menu === 'undefined') return null;
  if (!menu.previewState) menu.previewState = { focus: null, disabled: new Set(), version: 0 };
  return menu.previewState;
}

// The last cascade, kept until the file or the sheets change: a MutationObserver's pending records
// (taken synchronously) say whether anything in the file moved since.
let styledCache = null;
let styledObserver = null;
let styledDoc = null;

/// The screen with its styles cascaded: { copy, look (a Map of each frame in the file to its styled copy), computed (each frame's properties), rules, keyframes }.
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
  const state = menuPreviewState();
  const stateVersion = state ? state.version : 0;
  if (styledCache && styledCache.screen === screen && styledCache.version === menuSheets.version && styledCache.preview === preview && styledCache.stateVersion === stateVersion) return styledCache;

  const keyframes = new Map();
  const rules = menuRules(screen, keyframes);
  const copy = screen.cloneNode(true);
  [...copy.getElementsByTagName('style')].forEach(s => s.remove());
  // The previewed states onto the copy first: :focus-within on a frame asks after the frames inside it.
  if (state && (state.focus || state.disabled.size)) {
    const mark = (source, target) => {
      const sources = [...source.children].filter(e => e.tagName === 'frame');
      const targets = [...target.children].filter(e => e.tagName === 'frame');
      sources.forEach((frame, i) => {
        const words = [];
        if (state.focus === frame) words.push('focus');
        if (state.disabled.has(frame)) words.push('disabled');
        if (words.length) targets[i].setAttribute(STYLE_STATE, words.join(' '));
        mark(frame, targets[i]);
      });
    };
    mark(screen, copy);
  }
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
      // And a style it binds (bind-style), over its own, as the sample fills it.
      if (preview && typeof bindingFormat === 'function' && frame.hasAttribute('bind-style')) {
        const bound = bindingFormat(frame.getAttribute('bind-style'), bindingScopeOf(frame));
        t.setAttribute('style', [t.getAttribute('style'), bound].filter(s => s && s.trim()).join('; '));
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
  styledCache = { screen, version: menuSheets.version, preview, stateVersion, copy, look, computed, rules, keyframes, screenValues };
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
