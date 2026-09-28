// The menus' layout rules in the editor: a frame's style="left: 0; right: 0; ..." worked out into
// the rect the game will draw it at, so the canvas shows what the client and a baked .xbn show.
// A port of Shared/Text/MenuLayout.cs (what the game actually gets) - the same rules, kept in step;
// see that file for what each property does. Nothing here writes to the file: the rects are
// worked out as the canvas is drawn, and the style is the only thing saved.

const LAYOUT_SCREEN = { w: 480, h: 288 };

/// A style attribute as an ordered map of its properties (the last of a name wins).
function parseStyle(text) {
  const values = new Map();
  for (const part of String(text || '').split(';')) {
    const colon = part.indexOf(':');
    if (colon <= 0) continue;
    const name = part.slice(0, colon).trim().toLowerCase();
    const value = part.slice(colon + 1).trim();
    if (name && value) {
      values.delete(name);
      values.set(name, value);
    }
  }
  return values;
}

function formatStyle(values) {
  return [...values].map(([k, v]) => `${k}: ${v}`).join('; ');
}

function frameStyle(element) {
  return parseStyle(element.getAttribute('style'));
}

/// Sets (or with null, removes) properties of a frame's style; an empty style goes altogether.
function setFrameStyle(element, changes) {
  const values = frameStyle(element);
  for (const [name, value] of Object.entries(changes)) {
    if (value === null || value === undefined || value === '') values.delete(name);
    else values.set(name, String(value));
  }
  if (values.size) element.setAttribute('style', formatStyle(values));
  else element.removeAttribute('style');
}

/// A length: units (px optional) or a percent of `whole`; null for none or auto.
function parseLength(text, whole) {
  if (text === undefined || text === null) return null;
  let v = String(text).trim().toLowerCase();
  if (!v || v === 'auto') return null;
  if (v.endsWith('%')) {
    const p = parseFloat(v.slice(0, -1));
    return Number.isFinite(p) ? whole * p / 100 : null;
  }
  if (v.endsWith('px')) v = v.slice(0, -2);
  const n = Number(v);
  return v !== '' && Number.isFinite(n) ? n : null;
}

/// A length as it would be written: whole units in px, or a percent kept a percent.
function formatLength(value, asPercentOf) {
  if (asPercentOf) return `${Math.round(value / asPercentOf * 1000) / 10}%`;
  return `${Math.round(value)}px`;
}

const isPercent = text => typeof text === 'string' && text.trim().endsWith('%');

class LayoutStyle {
  constructor(values) { this.values = values; }
  /// A frame's layout rules: for a frame of the file, with its stylesheets' (menu-styles.js); a styled copy's are already in its style.
  static of(element) {
    if (element && element.tagName === 'frame' && typeof menuStyled === 'function' && element.ownerDocument.documentElement.contains(element)) {
      const screen = frameScreen(element);
      const copy = screen && menuStyled(screen).look.get(element);
      if (copy) return new LayoutStyle(frameStyle(copy));
    }
    return new LayoutStyle(frameStyle(element));
  }
  get empty() { return this.values.size === 0; }
  has(name) { return this.values.has(name); }
  word(name) { const v = this.values.get(name); return v === undefined ? null : v.trim().toLowerCase(); }
  number(name) { const v = this.values.get(name); const n = v === undefined ? NaN : Number(v.trim()); return Number.isFinite(n) ? n : null; }
  length(name, whole) { return this.values.has(name) ? parseLength(this.values.get(name), whole) : null; }
  box(name, whole) {
    let t = 0, r = 0, b = 0, l = 0;
    const all = this.values.get(name);
    if (all !== undefined) {
      const parts = all.split(/\s+/).filter(Boolean).map(p => parseLength(p, whole) ?? 0);
      if (parts.length === 1) t = r = b = l = parts[0];
      else if (parts.length === 2) { t = b = parts[0]; r = l = parts[1]; }
      else if (parts.length === 3) { t = parts[0]; r = l = parts[1]; b = parts[2]; }
      else if (parts.length >= 4) [t, r, b, l] = parts;
    }
    return {
      left: this.length(name + '-left', whole) ?? l,
      top: this.length(name + '-top', whole) ?? t,
      right: this.length(name + '-right', whole) ?? r,
      bottom: this.length(name + '-bottom', whole) ?? b
    };
  }
  translate(width, height) {
    const v = this.values.get('translate');
    if (v === undefined) return { x: 0, y: 0 };
    const parts = v.split(/\s+/).filter(Boolean);
    return { x: parts.length > 0 ? parseLength(parts[0], width) ?? 0 : 0, y: parts.length > 1 ? parseLength(parts[1], height) ?? 0 : 0 };
  }
}

const layoutFrames = element => [...element.children].filter(e => e.tagName === 'frame');
const layoutNumber = (element, name) => {
  const child = [...element.children].find(e => e.tagName === name);
  const n = child ? parseFloat(child.getAttribute('value') ?? child.textContent) : NaN;
  return Number.isFinite(n) ? n : 0;
};
const layoutRound = v => Math.sign(v) * Math.round(Math.abs(v));

function layoutClamp(value, style, min, max, parent) {
  const hi = style.length(max, parent);
  const lo = style.length(min, parent);
  if (hi !== null) value = Math.min(value, hi);
  if (lo !== null) value = Math.max(value, lo);
  return value;
}

function layoutSize(element, style, across, parentW, parentH) {
  const name = across ? 'width' : 'height';
  if (style.word(name) === 'auto') return layoutIntrinsic(element, style, across);
  return style.length(name, across ? parentW : parentH) ?? layoutNumber(element, name);
}

function layoutIntrinsic(element, style, across) {
  const direction = style.word('flex-direction');
  const pad = style.box('padding', 0);
  const padding = across ? pad.left + pad.right : pad.top + pad.bottom;
  const kids = layoutFrames(element).filter(f => LayoutStyle.of(f).word('position') !== 'absolute' && LayoutStyle.of(f).word('display') !== 'none');
  if ((direction !== 'row' && direction !== 'column') || !kids.length) return padding + layoutNumber(element, across ? 'width' : 'height');
  const alongMain = (direction === 'row') === across;
  const name = across ? 'width' : 'height';
  let total = 0;
  for (const kid of kids) {
    const s = LayoutStyle.of(kid);
    const m = s.box('margin', 0);
    const own = s.word(name) === 'auto' ? layoutIntrinsic(kid, s, across)
      : s.has(name) ? (s.length(name, 0) ?? 0) : layoutNumber(kid, name);
    const withMargins = own + (across ? m.left + m.right : m.top + m.bottom);
    total = alongMain ? total + withMargins : Math.max(total, withMargins);
  }
  if (alongMain) total += (style.length('gap', 0) ?? 0) * (kids.length - 1);
  return padding + total;
}

function layoutAxis(style, start, end, size, min, max, marginStart, marginEnd, fallbackAt, fallbackLength, parent) {
  const from = style.length(start, parent);
  const to = style.length(end, parent);
  const length = style.has(size) && style.word(size) !== 'auto' ? style.length(size, parent) : null;
  const margins = style.box('margin', parent);
  const ms = margins[marginStart], me = margins[marginEnd];
  if (from !== null && to !== null && length === null) {
    return { at: from + ms, length: layoutClamp(parent - from - to - ms - me, style, min, max, parent) };
  }
  const l = layoutClamp(length ?? fallbackLength, style, min, max, parent);
  return { at: from !== null ? from + ms : to !== null ? parent - to - me - l : fallbackAt, length: l };
}

function layoutAbsolute(element, style, parentW, parentH) {
  const h = layoutAxis(style, 'left', 'right', 'width', 'min-width', 'max-width', 'left', 'right', layoutNumber(element, 'x'), layoutSize(element, style, true, parentW, parentH), parentW);
  const v = layoutAxis(style, 'top', 'bottom', 'height', 'min-height', 'max-height', 'top', 'bottom', layoutNumber(element, 'y'), layoutSize(element, style, false, parentW, parentH), parentH);
  const t = style.translate(h.length, v.length);
  return { x: h.at + t.x, y: v.at + t.y, w: h.length, h: v.length };
}

function layoutFlow(frames, parentStyle, row, width, height, rects) {
  const pad = parentStyle.box('padding', width);
  const gap = parentStyle.length('gap', row ? width : height) ?? 0;
  const innerMain = row ? width - pad.left - pad.right : height - pad.top - pad.bottom;
  const innerCross = row ? height - pad.top - pad.bottom : width - pad.left - pad.right;
  const justify = parentStyle.word('justify-content') || 'flex-start';
  const alignItems = parentStyle.word('align-items') || 'stretch';

  const n = frames.length;
  const styles = frames.map(f => LayoutStyle.of(f));
  const main = frames.map((f, i) => layoutSize(f, styles[i], row, width, height));
  const margins = styles.map(s => s.box('margin', width));
  const before = margins.map(m => row ? m.left : m.top);
  const after = margins.map(m => row ? m.right : m.bottom);
  const grow = styles.map(s => Math.max(0, s.number('flex-grow') ?? 0));
  const sum = list => list.reduce((a, b) => a + b, 0);
  let free = innerMain - (sum(main) + sum(before) + sum(after) + gap * (n - 1));
  const growing = sum(grow);
  if (free > 0 && growing > 0) {
    for (let i = 0; i < n; i++) main[i] = layoutClamp(main[i] + free * grow[i] / growing, styles[i], row ? 'min-width' : 'min-height', row ? 'max-width' : 'max-height', row ? width : height);
    free = 0;
  }
  let start = 0, spacing = 0;
  if (free > 0) {
    if (justify === 'center') start = free / 2;
    else if (justify === 'flex-end') start = free;
    else if (justify === 'space-between' && n > 1) spacing = free / (n - 1);
  }
  let cursor = (row ? pad.left : pad.top) + start;
  for (let i = 0; i < n; i++) {
    const s = styles[i];
    cursor += before[i];
    const align = s.word('align-self') || alignItems;
    const crossName = row ? 'height' : 'width';
    const m = margins[i];
    const crossBefore = row ? m.top : m.left, crossAfter = row ? m.bottom : m.right;
    const own = s.has(crossName) && s.word(crossName) !== 'auto';
    let cross = align === 'stretch' && !own ? innerCross - crossBefore - crossAfter : layoutSize(frames[i], s, !row, width, height);
    cross = layoutClamp(cross, s, row ? 'min-height' : 'min-width', row ? 'max-height' : 'max-width', row ? height : width);
    const edge = row ? pad.top : pad.left;
    let crossAt = edge + crossBefore;
    if (align === 'center') crossAt = edge + (innerCross - cross) / 2;
    else if (align === 'flex-end') crossAt = edge + innerCross - cross - crossAfter;
    const t = s.translate(row ? main[i] : cross, row ? cross : main[i]);
    rects.set(frames[i], row
      ? { x: cursor + t.x, y: crossAt + t.y, w: main[i], h: cross, flow: true }
      : { x: crossAt + t.x, y: cursor + t.y, w: cross, h: main[i], flow: true });
    cursor += main[i] + after[i] + gap + spacing;
  }
}

/// Every frame of a screen's rect relative to its parent, rounded as the bake rounds it: a Map of
/// element to { x, y, w, h, driven (the style or the parent's flow places it), flow (in a column or row) }.
/// Worked out on the screen with its stylesheets cascaded (menuStyled), kept with that cascade.
function menuLayoutRects(screen) {
  if (!screen || typeof menuStyled !== 'function' || !screen.ownerDocument.documentElement.contains(screen)) return layoutRectsOf(screen);
  const styled = menuStyled(screen);
  if (styled.rects) return styled.rects;
  const inner = layoutRectsOf(styled.copy);
  const rects = new Map();
  for (const [frame, copy] of styled.look) rects.set(frame, inner.get(copy));
  styled.rects = rects;
  return rects;
}

function layoutRectsOf(screen) {
  const rects = new Map();
  if (!screen) return rects;
  const screenStyle = LayoutStyle.of(screen);
  const walk = (parent, style, width, height) => {
    const frames = layoutFrames(parent);
    if (!frames.length) return;
    const direction = style.word('flex-direction');
    const flows = direction === 'row' || direction === 'column';
    const worked = new Map();
    const flowing = flows ? frames.filter(f => LayoutStyle.of(f).word('position') !== 'absolute' && LayoutStyle.of(f).word('display') !== 'none') : [];
    if (flowing.length) layoutFlow(flowing, style, direction === 'row', width, height, worked);
    for (const frame of frames) {
      if (worked.has(frame)) continue;
      const s = LayoutStyle.of(frame);
      if (s.empty) continue;
      worked.set(frame, layoutAbsolute(frame, s, width, height));
    }
    for (const frame of frames) {
      const r = worked.get(frame);
      const rect = r
        ? { x: layoutRound(r.x), y: layoutRound(r.y), w: Math.max(0, layoutRound(r.w)), h: Math.max(0, layoutRound(r.h)), driven: true, flow: !!r.flow }
        : { x: layoutNumber(frame, 'x'), y: layoutNumber(frame, 'y'), w: layoutNumber(frame, 'width'), h: layoutNumber(frame, 'height'), driven: false, flow: false };
      rects.set(frame, rect);
      walk(frame, LayoutStyle.of(frame), r ? r.w : rect.w, r ? r.h : rect.h);
    }
  };
  const width = screenStyle.length('width', LAYOUT_SCREEN.w) ?? LAYOUT_SCREEN.w;
  const height = screenStyle.length('height', LAYOUT_SCREEN.h) ?? LAYOUT_SCREEN.h;
  walk(screen, screenStyle, width, height);
  return rects;
}

/// Whether a frame's parent lays its frames out in a column or row (and this one is in that flow).
function frameInFlow(element) {
  const parent = element.parentElement;
  if (!parent) return false;
  const direction = LayoutStyle.of(parent).word('flex-direction');
  return (direction === 'row' || direction === 'column') && LayoutStyle.of(element).word('position') !== 'absolute' && LayoutStyle.of(element).word('display') !== 'none';
}

/// The size a frame's parent gives it to be placed in (the screen's for one at the top level), as the layout works it out.
function frameParentSize(element, rects) {
  const parent = element.parentElement;
  if (parent && parent.tagName === 'frame') {
    const r = rects.get(parent);
    return r ? { w: r.w, h: r.h } : { w: layoutNumber(parent, 'width'), h: layoutNumber(parent, 'height') };
  }
  const s = parent ? LayoutStyle.of(parent) : new LayoutStyle(new Map());
  return { w: s.length('width', LAYOUT_SCREEN.w) ?? LAYOUT_SCREEN.w, h: s.length('height', LAYOUT_SCREEN.h) ?? LAYOUT_SCREEN.h };
}

/// Moves a frame by dx, dy through whatever places it: the edge its style holds it to (a percent
/// kept a percent), or its own x and y. A frame in a column or row is placed by its parent and does not move.
function moveFrameBy(element, dx, dy, rects) {
  if (frameInFlow(element)) return false;
  const style = frameStyle(element);
  const size = frameParentSize(element, rects);
  const shift = (start, end, tag, delta, whole) => {
    if (!delta) return;
    if (style.has(start)) {
      const v = style.get(start);
      const now = parseLength(v, whole) ?? 0;
      setFrameStyle(element, { [start]: formatLength(now + delta, isPercent(v) ? whole : 0) });
      // Held to both edges: the far one moves too, so the frame keeps its size.
      if (style.has(end)) {
        const e = style.get(end);
        setFrameStyle(element, { [end]: formatLength((parseLength(e, whole) ?? 0) - delta, isPercent(e) ? whole : 0) });
      }
    } else if (style.has(end)) {
      const e = style.get(end);
      setFrameStyle(element, { [end]: formatLength((parseLength(e, whole) ?? 0) - delta, isPercent(e) ? whole : 0) });
    } else {
      setChildText(element, tag, String(layoutNumber(element, tag) + delta));
    }
  };
  shift('left', 'right', 'x', dx, size.w);
  shift('top', 'bottom', 'y', dy, size.h);
  return true;
}
