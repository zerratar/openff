// Data bindings in the menu editor: the paths a frame's data-source and bind-* attributes may use,
// for the inspector's pickers, and a sample of the game's data so Preview shows what a bound text
// will read (Luneth, Lv 12...) rather than its placeholder. A port of OpenFF.Engine/MenuBindings.cs
// (the paths, templates and conditions), kept to the same rules; see that file for them.

const SAMPLE_MEMBER = (id, name, job, level, hp, maxHp) => ({
  id, slot: id, name, level, experience: level * 312, hp, maxHp, mp: 4, charges: [4, 3, 2, 0, 0, 0, 0, 0], maxCharges: [4, 3, 2, 0, 0, 0, 0, 0],
  job: 0, jobName: job, jobTitle: job, jobWord: job.toLowerCase(), jobSkill: 5, alive: hp > 0, conditions: 0, progression: 'jobs',
  stats: { strength: 18, vitality: 15, agility: 12, intellect: 9, mind: 8 }
});

/// What a bound frame reads in Preview: the game's data, made up.
const BINDING_SAMPLE = {
  hero: SAMPLE_MEMBER(0, 'Luneth', 'Warrior', 12, 230, 260),
  party: [SAMPLE_MEMBER(0, 'Luneth', 'Warrior', 12, 230, 260), SAMPLE_MEMBER(1, 'Arc', 'White Mage', 11, 150, 180), SAMPLE_MEMBER(2, 'Refia', 'Red Mage', 11, 0, 190), SAMPLE_MEMBER(3, 'Ingus', 'Monk', 12, 280, 300)],
  gil: 12345,
  items: [{ id: 1, name: 'Potion', count: 5 }, { id: 2, name: 'Phoenix Down', count: 2 }],
  menu: { id: 'menu', hero: 0, focused: null },
  // The field's HUD (field_hud): who speaks in the dialogue window, and the map-name banner's text.
  dialogue: { number: 1000142, text: 'The wind crystal\'s light has faded... We should see the elder.', speaker: 'Luneth', avatar: 'resource("files/pc1_01.NCGR")', map: 't01_01' },
  banner: { number: -1, text: 'Ur' }
};

/// The paths the pickers offer: the roots and what the game's data has under them.
const BINDING_PATHS = (() => {
  const member = ['name', 'level', 'experience', 'hp', 'maxHp', 'mp', 'jobTitle', 'jobWord', 'jobSkill', 'alive', 'slot', 'conditions', 'progression',
    'stats.strength', 'stats.vitality', 'stats.agility', 'stats.intellect', 'stats.mind', 'charges[0]', 'maxCharges[0]'];
  return [
    ...member.map(m => 'hero.' + m),
    'party', 'party.count', ...['name', 'level', 'hp', 'maxHp', 'jobTitle', 'alive'].map(m => 'party[0].' + m),
    'gil', 'items', 'items.count', 'items[0].name', 'items[0].count',
    'menu.hero', 'menu.focused', 'menu.id', 'this',
    'dialogue.speaker', 'dialogue.avatar', 'dialogue.text', 'dialogue.number', 'dialogue.map', 'banner.text', 'banner.number'
  ];
})();

// ------------------------------------------------------------------ the port

function bindingMember(on, name) {
  if (on === null || on === undefined || typeof on !== 'object') {
    if (typeof on === 'string' && name.toLowerCase() === 'length') return { found: true, value: on.length };
    return { found: false };
  }
  if (Array.isArray(on) && (name.toLowerCase() === 'count' || name.toLowerCase() === 'length')) return { found: true, value: on.length };
  const key = Object.keys(on).find(k => k.toLowerCase() === name.toLowerCase());
  return key === undefined ? { found: false } : { found: true, value: on[key] };
}

function bindingSteps(path) {
  const steps = [];
  let name = '';
  for (let i = 0; i < path.length; i++) {
    const c = path[i];
    if (c === '.') { if (name) steps.push(name); name = ''; }
    else if (c === '[') {
      if (name) steps.push(name);
      name = '';
      let depth = 0, j = i;
      for (; j < path.length; j++) { if (path[j] === '[') depth++; else if (path[j] === ']' && --depth === 0) break; }
      steps.push(path.slice(i, Math.min(j, path.length - 1) + 1));
      i = j;
    } else if (!/\s/.test(c)) name += c;
  }
  if (name) steps.push(name);
  return steps;
}

function bindingResolve(path, scope) {
  if (!path || !path.trim()) return scope.source;
  const steps = bindingSteps(path.trim());
  if (!steps.length) return null;
  let at;
  const first = steps[0];
  if (first.startsWith('[')) at = bindingIndex(scope.source, first, scope);
  else if (first.toLowerCase() === 'this') at = scope.source;
  else {
    const own = bindingMember(scope.source, first);
    if (own.found) at = own.value;
    else {
      const root = bindingMember(scope.roots, first);
      if (!root.found) return null;
      at = root.value;
    }
  }
  for (let i = 1; i < steps.length && at !== null && at !== undefined; i++) {
    const step = steps[i];
    if (step.startsWith('[')) at = bindingIndex(at, step, scope);
    else {
      const m = bindingMember(at, step);
      if (!m.found) return null;
      at = m.value;
    }
  }
  return at === undefined ? null : at;
}

function bindingIndex(on, step, scope) {
  if (on === null || on === undefined) return null;
  const key = bindingValue(step.slice(1, -1), scope);
  if (Array.isArray(on) && typeof key === 'number') return on[key] ?? null;
  if (typeof on === 'object' && key !== null) { const m = bindingMember(on, String(key)); return m.found ? m.value : null; }
  return null;
}

function bindingIndexOutside(text, what, fromEnd = false) {
  let found = -1, quote = '', depth = 0;
  for (let i = 0; i + what.length <= text.length; i++) {
    const c = text[i];
    if (quote) { if (c === quote) quote = ''; continue; }
    if (c === '\'' || c === '"') { quote = c; continue; }
    if (c === '[' || c === '(') depth++;
    else if (c === ']' || c === ')') depth--;
    if (depth === 0 && text.startsWith(what, i)) {
      if (what === '-' && (i === 0 || '+-*/<>=!&|'.includes(text[i - 1]) || (/\s/.test(text[i - 1]) && i >= 2 && '+-*/<>=!&|'.includes(text[i - 2])))) continue;
      if ((what === '<' || what === '>') && text[i + 1] === '=') continue;
      if ((what === '<' || what === '>' || what === '=') && i > 0 && '<>!='.includes(text[i - 1]) && what !== '==') continue;
      found = i;
      if (!fromEnd) return found;
    }
  }
  return found;
}

function bindingValue(text, scope) {
  text = String(text).trim();
  if (!text) return null;
  for (const op of ['+', '-', '*', '/']) {
    const at = bindingIndexOutside(text, op, true);
    if (at > 0 && at < text.length - 1) {
      const l = bindingValue(text.slice(0, at), scope), r = bindingValue(text.slice(at + 1), scope);
      if (typeof l === 'number' && typeof r === 'number') {
        const v = op === '+' ? l + r : op === '-' ? l - r : op === '*' ? l * r : (r === 0 ? 0 : l / r);
        return Number.isInteger(v) ? v : v;
      }
      return op === '+' ? bindingText(l) + bindingText(r) : null;
    }
  }
  if (text.length >= 2 && ((text[0] === '\'' && text.endsWith('\'')) || (text[0] === '"' && text.endsWith('"')))) return text.slice(1, -1);
  if (/^-?\d+(\.\d+)?$/.test(text)) return Number(text);
  const lower = text.toLowerCase();
  if (lower === 'true') return true;
  if (lower === 'false') return false;
  if (lower === 'null') return null;
  return bindingResolve(text, scope);
}

function bindingText(value, format) {
  if (value === null || value === undefined) return '';
  if (typeof value === 'number' && format) {
    const m = /^([NnDdFfPp])(\d*)$/.exec(format);
    if (m) {
      const digits = m[2] === '' ? null : Number(m[2]);
      const kind = m[1].toUpperCase();
      if (kind === 'N') return value.toLocaleString('en-US', { minimumFractionDigits: digits ?? 2, maximumFractionDigits: digits ?? 2 });
      if (kind === 'D') return String(Math.trunc(value)).padStart(digits ?? 0, '0');
      if (kind === 'F') return value.toFixed(digits ?? 2);
      if (kind === 'P') return (value * 100).toFixed(digits ?? 2) + ' %';
    }
    if (/^0+$/.test(format)) return String(Math.trunc(value)).padStart(format.length, '0');
  }
  if (typeof value === 'boolean') return value ? 'True' : 'False';
  return String(value);
}

function bindingTruthy(v) {
  if (v === null || v === undefined) return false;
  if (typeof v === 'boolean') return v;
  if (typeof v === 'number') return v !== 0;
  if (typeof v === 'string') return v.length > 0;
  if (Array.isArray(v)) return v.length > 0;
  return true;
}

/// A template's text: {path} and {path:format} replaced, {{ and }} for braces.
function bindingFormat(template, scope) {
  let out = '';
  for (let i = 0; i < template.length; i++) {
    const c = template[i];
    if (c === '{' && template[i + 1] === '{') { out += '{'; i++; continue; }
    if (c === '}' && template[i + 1] === '}') { out += '}'; i++; continue; }
    if (c !== '{') { out += c; continue; }
    const close = template.indexOf('}', i + 1);
    if (close < 0) { out += template.slice(i); break; }
    const inner = template.slice(i + 1, close);
    i = close;
    const colon = inner.indexOf(':');
    out += bindingText(bindingValue(colon >= 0 ? inner.slice(0, colon) : inner, scope), colon >= 0 ? inner.slice(colon + 1) : null);
  }
  return out;
}

/// An expression's truth: alive, !alive, hp < maxHp / 4, gil >= 1000 && menu.hero == 0.
function bindingTest(expression, scope) {
  if (!expression || !expression.trim()) return false;
  return expression.split('||').some(any => any.split('&&').every(part => {
    let text = part.trim(), not = false;
    while (text.startsWith('!') && !text.startsWith('!=')) { not = !not; text = text.slice(1).trim(); }
    for (const op of ['==', '!=', '<=', '>=', '<', '>']) {
      const at = bindingIndexOutside(text, op);
      if (at <= 0) continue;
      const a = bindingValue(text.slice(0, at), scope), b = bindingValue(text.slice(at + op.length), scope);
      let r;
      if (typeof a === 'number' && typeof b === 'number') r = { '==': a === b, '!=': a !== b, '<': a < b, '<=': a <= b, '>': a > b, '>=': a >= b }[op];
      else {
        const c = String(a ?? '').localeCompare(String(b ?? ''), undefined, { sensitivity: 'accent' });
        r = { '==': c === 0, '!=': c !== 0, '<': c < 0, '<=': c <= 0, '>': c > 0, '>=': c >= 0 }[op];
      }
      return r !== not;
    }
    return bindingTruthy(bindingValue(text, scope)) !== not;
  }));
}

/// The scope a frame of the file binds in, with the sample data: its and its parents' data-source, down from the screen's.
function bindingScopeOf(element) {
  const chain = [];
  for (let e = element; e && (e.tagName === 'frame' || e.tagName === 'menu' || e.tagName === 'unit'); e = e.parentElement) chain.unshift(e);
  let scope = { roots: BINDING_SAMPLE, source: null };
  for (const e of chain) {
    const source = e.getAttribute('data-source');
    if (source) scope = { roots: BINDING_SAMPLE, source: bindingResolve(source, scope) };
  }
  return scope;
}

/// A frame's bind-class as [name, expression] pairs.
function boundClasses(element) {
  return styleDeclarations(element.getAttribute('bind-class'));
}

function setBindAttribute(element, name, value) {
  if (value === null || value === undefined || !String(value).trim()) element.removeAttribute(name);
  else element.setAttribute(name, String(value));
}

// ------------------------------------------------------------------ the inspector's Bindings

/// The inspector's Bindings section: the frame's data source and what it binds, each with the paths the game's data has, and what Preview's sample makes of it.
function buildBindingsSection(panel, element, screen, edit, rebuild, sync) {
  const has = ['data-source', 'bind-text', 'bind-visible', 'bind-display', 'bind-class', 'bind-style'].some(a => element.hasAttribute(a));
  const body = propSection(panel, 'Bindings', 'logic', 'Data bindings: the frame\'s text, visibility and classes from the game\'s data as the screen runs - UI Toolkit\'s data source and binding paths', has);
  const listId = 'menu-binding-paths';
  let list = document.getElementById(listId);
  if (!list) {
    list = document.createElement('datalist');
    list.id = listId;
    for (const path of BINDING_PATHS) { const o = document.createElement('option'); o.value = path; list.append(o); }
    document.body.append(list);
  }
  const field = (label, attribute, placeholder, tip, paths) => {
    const input = propText(element.getAttribute(attribute) || '', v => edit('change ' + attribute, () => setBindAttribute(element, attribute, v)), placeholder);
    if (paths) input.setAttribute('list', listId);
    propRow(body, label, input, tip);
    return input;
  };
  field('Data source', 'data-source', 'hero, party[1], rules …', 'data-source: what this frame\'s paths (and its frames\') start from', true);
  const text = field('Text', 'bind-text', 'Lv {level}   {hp}/{maxHp}', 'bind-text: a template - {path}, or {path:format} ({gil:N0}, {hp:D3}); the layout\'s own text shows until the screen runs', false);
  // A path picked from the list goes into the template at the cursor, in braces.
  const insert = document.createElement('select');
  insert.className = 'binding-insert';
  const first = document.createElement('option');
  first.value = '';
  first.textContent = '+ path';
  insert.append(first);
  for (const path of BINDING_PATHS) { const o = document.createElement('option'); o.value = path; o.textContent = path; insert.append(o); }
  insert.onchange = () => {
    if (!insert.value) return;
    const at = text.selectionStart ?? text.value.length;
    text.value = text.value.slice(0, at) + `{${insert.value}}` + text.value.slice(text.selectionEnd ?? at);
    insert.value = '';
    text.dispatchEvent(new Event('input'));
    text.focus();
  };
  text.parentElement.append(insert);
  field('Visible', 'bind-visible', 'alive, hp > 0, gil >= 100', 'bind-visible: shown while the expression is true', true);
  field('In the layout', 'bind-display', 'dialogue.avatar', 'bind-display: in its row or column while the expression is true - out of it (display: none) otherwise, the others closing up over its place', true);
  field('Bind classes', 'bind-class', 'ko: !alive; low: hp < maxHp / 4', 'bind-class: each class on while its expression is true - the sheets\' rules for it follow', false);
  field('Bind style', 'bind-style', 'background-image: {dialogue.avatar}', 'bind-style: declarations with {paths} in them, over the frame\'s own style as the data says (a speaker\'s picture, a colour from the data)', false);

  const sample = document.createElement('p');
  sample.className = 'none binding-sample';
  const showSample = () => {
    const scope = bindingScopeOf(element);
    const lines = [];
    const template = element.getAttribute('bind-text');
    if (template) lines.push(`Text: “${bindingFormat(template, scope)}”`);
    const visible = element.getAttribute('bind-visible');
    if (visible) lines.push(`Visible: ${bindingTest(visible, scope) ? 'yes' : 'no'}`);
    const display = element.getAttribute('bind-display');
    if (display) lines.push(`In the layout: ${bindingTest(display, scope) ? 'yes' : 'no'}`);
    const classes = boundClasses(element);
    if (classes.length) lines.push('Classes: ' + classes.map(([k, v]) => `${k} ${bindingTest(v, scope) ? 'on' : 'off'}`).join(', '));
    const style = element.getAttribute('bind-style');
    if (style) lines.push(`Style: ${bindingFormat(style, scope)}`);
    sample.textContent = lines.length ? `With the sample (Luneth, Lv 12, 12,345 G): ${lines.join(' · ')}` : 'Nothing bound. Code can bind too: widget.Bind("text", "Lv {level}").';
  };
  showSample();
  sync.push(showSample);
  body.append(sample);
}
