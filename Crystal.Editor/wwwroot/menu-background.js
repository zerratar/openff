// A menu frame's background in the editor: the colour and picture behind it (background-color,
// background-image, the 9-slice...), laid out as the client lays it out - a port of
// Shared/Text/MenuBackground.cs, the same quads - and drawn on the canvas, with the box round it (a
// gradient, a border, round corners, shadows) painted as Shared/Text/MenuPaint.cs paints it; the
// inspector's Background and Box sections, the picture picker, and the slice editor that sets a
// picture's borders by dragging them.

// BACKGROUND_PROPS: menu-styles.js's, which bakes them.

// ------------------------------------------------------------------ the port

function bgNumber(text) {
  let t = String(text || '').trim().toLowerCase();
  if (t.endsWith('px')) t = t.slice(0, -2);
  const v = parseFloat(t);
  return Number.isFinite(v) ? v : 0;
}

/// CSS's colour names, the common ones - MenuBackground's list.
const BG_NAMED = {
  black: '000000', white: 'ffffff', red: 'ff0000', lime: '00ff00', green: '008000', blue: '0000ff', yellow: 'ffff00', cyan: '00ffff', aqua: '00ffff',
  magenta: 'ff00ff', fuchsia: 'ff00ff', gray: '808080', grey: '808080', silver: 'c0c0c0', maroon: '800000', olive: '808000', navy: '000080',
  purple: '800080', teal: '008080', orange: 'ffa500', gold: 'ffd700', pink: 'ffc0cb', brown: 'a52a2a', crimson: 'dc143c', coral: 'ff7f50',
  salmon: 'fa8072', tomato: 'ff6347', orchid: 'da70d6', violet: 'ee82ee', indigo: '4b0082', plum: 'dda0dd', khaki: 'f0e68c', beige: 'f5f5dc',
  ivory: 'fffff0', tan: 'd2b48c', chocolate: 'd2691e', skyblue: '87ceeb', steelblue: '4682b4', royalblue: '4169e1', midnightblue: '191970',
  darkblue: '00008b', darkred: '8b0000', darkgreen: '006400', darkgray: 'a9a9a9', darkgrey: 'a9a9a9', lightgray: 'd3d3d3', lightgrey: 'd3d3d3',
  lightblue: 'add8e6', slategray: '708090', dimgray: '696969', whitesmoke: 'f5f5f5', goldenrod: 'daa520', firebrick: 'b22222'
};

/// #rgb, #rgba, #rrggbb, #rrggbbaa, rgb()/rgba(), hsl()/hsla(), transparent, a CSS name - as [r, g, b, a] 0..255, or null.
function bgColour(value) {
  const v = String(value || '').trim().toLowerCase();
  if (!v) return null;
  if (v === 'transparent') return [0, 0, 0, 0];
  if (v.startsWith('#')) {
    let hex = v.slice(1);
    if (hex.length === 3 || hex.length === 4) hex = [...hex].map(c => c + c).join('');
    if (hex.length === 6) hex += 'ff';
    if (!/^[0-9a-f]{8}$/.test(hex)) return null;
    return [0, 2, 4, 6].map(i => parseInt(hex.slice(i, i + 2), 16));
  }
  const alphaOf = p => { const t = p.trim(); return Math.max(0, Math.min(255, Math.round((t.endsWith('%') ? bgNumber(t.slice(0, -1)) / 100 : bgNumber(t)) * 255))); };
  if (v.startsWith('rgb')) {
    const open = v.indexOf('('), close = v.lastIndexOf(')');
    if (open < 0 || close < open) return null;
    const parts = v.slice(open + 1, close).split(',');
    if (parts.length < 3) return null;
    const c = p => Math.max(0, Math.min(255, Math.round(bgNumber(p))));
    return [c(parts[0]), c(parts[1]), c(parts[2]), parts.length > 3 ? alphaOf(parts[3]) : 255];
  }
  if (v.startsWith('hsl')) {
    const open = v.indexOf('('), close = v.lastIndexOf(')');
    if (open < 0 || close < open) return null;
    const parts = v.slice(open + 1, close).replace(/\//g, ',').split(/[,\s]+/).filter(Boolean);
    if (parts.length < 3) return null;
    const hue = (((bgNumber(parts[0].replace('deg', '')) % 360) + 360) % 360) / 360;
    const sat = Math.min(1, Math.max(0, bgNumber(parts[1].replace(/%$/, '')) / 100)), light = Math.min(1, Math.max(0, bgNumber(parts[2].replace(/%$/, '')) / 100));
    const q = light < 0.5 ? light * (1 + sat) : light + sat - light * sat, pp = 2 * light - q;
    const h = t => { t = (t + 1) % 1; return t < 1 / 6 ? pp + (q - pp) * 6 * t : t < 0.5 ? q : t < 2 / 3 ? pp + (q - pp) * (2 / 3 - t) * 6 : pp; };
    const ch = x => Math.max(0, Math.min(255, Math.round(x * 255)));
    return [ch(h(hue + 1 / 3)), ch(h(hue)), ch(h(hue - 1 / 3)), parts.length > 3 ? alphaOf(parts[3]) : 255];
  }
  const named = BG_NAMED[v];
  return named ? [...[0, 2, 4].map(i => parseInt(named.slice(i, i + 2), 16)), 255] : null;
}

/// A colour as a text's or a box's is read (MenuText.Colour): a CSS colour, else one of the game's palette words.
function bgTextColour(value) {
  return bgColour(value) || (typeof menuPalette === 'function' ? menuPalette(String(value || '').trim().toLowerCase()) : null);
}

function bgCss(c) {
  return `rgba(${c[0]}, ${c[1]}, ${c[2]}, ${c[3] / 255})`;
}

/// 12px / 12 / .5em (of 12) as menu units; null for a word that is not a length (MenuText.Length).
function bgLength(text) {
  let t = String(text ?? '').trim().toLowerCase();
  if (!t) return null;
  let scale = 1;
  if (t.endsWith('px')) t = t.slice(0, -2);
  else if (t.endsWith('em')) { t = t.slice(0, -2); scale = 12; }
  const v = Number(t);
  return t.trim() !== '' && Number.isFinite(v) ? v * scale : null;
}

function bgImage(value) {
  const v = String(value || '').trim();
  for (const kind of ['url', 'resource']) {
    if (!v.toLowerCase().startsWith(kind + '(') || !v.endsWith(')')) continue;
    const inner = v.slice(kind.length + 1, -1).trim().replace(/^["']+|["']+$/g, '');
    return inner ? { kind, path: inner.replace(/\\/g, '/') } : null;
  }
  return null;
}

/// An angle in CSS degrees (deg, grad, rad, turn); null for none.
function bgDegrees(word) {
  const w = String(word).trim().toLowerCase();
  const num = t => (t.trim() !== '' && Number.isFinite(Number(t)) ? Number(t) : NaN);
  let v = NaN;
  if (w.endsWith('deg')) v = num(w.slice(0, -3));
  else if (w.endsWith('grad')) v = num(w.slice(0, -4)) * 0.9;
  else if (w.endsWith('rad')) v = num(w.slice(0, -3)) * 180 / Math.PI;
  else if (w.endsWith('turn')) v = num(w.slice(0, -4)) * 360;
  return Number.isNaN(v) ? null : v;
}

/// A gradient from background-image's value (MenuGradient.Parse); null for one that is not.
function parseGradient(value) {
  const v = String(value ?? '').trim();
  const m = /^(repeating-)?(linear|radial|conic)-gradient\(([\s\S]*)\)$/i.exec(v);
  if (!m) return null;
  const g = { kind: m[2].toLowerCase(), repeating: !!m[1], angle: 180, cornerX: 0, cornerY: 0, circle: false, sizeWord: 'farthest-corner', sizeX: null, sizeY: null, atX: '50%', atY: '50%', from: 0, stops: [] };
  const args = cssCommaList(m[3]);
  if (!args.length) return null;
  let first = 0;
  // The first argument says the direction or the shape when it holds no colour.
  if (!bgTextColour(cssWords(args[0])[0] || '')) {
    first = 1;
    const words = cssWords(args[0].toLowerCase());
    if (g.kind === 'linear') {
      if (words[0] === 'to') {
        for (const w of words.slice(1)) {
          if (w === 'left') g.cornerX = -1; else if (w === 'right') g.cornerX = 1;
          else if (w === 'top') g.cornerY = -1; else if (w === 'bottom') g.cornerY = 1;
        }
        if (g.cornerX === 0 || g.cornerY === 0) {
          g.angle = g.cornerX === 1 ? 90 : g.cornerX === -1 ? 270 : g.cornerY === -1 ? 0 : 180;
          g.cornerX = g.cornerY = 0;
        }
      } else if (words.length && bgDegrees(words[0]) !== null) g.angle = bgDegrees(words[0]);
    } else {
      const at = words.indexOf('at');
      const shape = at >= 0 ? words.slice(0, at) : words;
      if (at >= 0) {
        const pos = words.slice(at + 1);
        let x = '50%', y = '50%';
        for (const w of pos) {
          if (w === 'left') x = '0%'; else if (w === 'right') x = '100%';
          else if (w === 'top') y = '0%'; else if (w === 'bottom') y = '100%';
          else if (w === 'center') { /* the middle */ }
          else if (x === '50%' && pos.indexOf(w) === 0) x = w;
          else y = w;
        }
        g.atX = x; g.atY = y;
      }
      if (g.kind === 'conic') {
        const from = shape.indexOf('from');
        if (from >= 0 && from + 1 < shape.length && bgDegrees(shape[from + 1]) !== null) g.from = bgDegrees(shape[from + 1]);
      } else {
        const lengths = [];
        for (const w of shape) {
          if (w === 'circle') g.circle = true;
          else if (w === 'ellipse') g.circle = false;
          else if (['closest-side', 'farthest-side', 'closest-corner', 'farthest-corner'].includes(w)) g.sizeWord = w;
          else lengths.push(w);
        }
        if (lengths.length) { g.sizeWord = null; g.sizeX = lengths[0]; g.sizeY = lengths.length > 1 ? lengths[1] : lengths[0]; if (lengths.length === 1) g.circle = true; }
      }
    }
  }
  for (let i = first; i < args.length; i++) {
    let colour = null;
    const positions = [];
    for (const w of cssWords(args[i])) {
      const c = colour === null ? bgTextColour(w) : null;
      if (c) colour = c; else positions.push(w);
    }
    if (!colour) continue;   // a colour hint alone: passed over
    if (!positions.length) g.stops.push({ colour, position: null });
    for (const p of positions.slice(0, 2)) g.stops.push({ colour, position: p });
  }
  return g.stops.length >= 1 ? g : null;
}

/// One to four values (as margin: top, right, bottom, left) into four.
function bgFour(value, read, into) {
  const v = cssWords(value).map(read);
  if (!v.length) return;
  if (v.length === 1) into.fill(v[0]);
  else if (v.length === 2) { into[0] = into[2] = v[0]; into[1] = into[3] = v[1]; }
  else if (v.length === 3) { into[0] = v[0]; into[1] = into[3] = v[1]; into[2] = v[2]; }
  else { into[0] = v[0]; into[1] = v[1]; into[2] = v[2]; into[3] = v[3]; }
}

function bgBorderWidth(word) {
  const w = String(word).trim().toLowerCase();
  if (w === 'thin') return 1;
  if (w === 'medium') return 3;
  if (w === 'thick') return 5;
  return Math.max(0, bgLength(w) ?? 0);
}

/// "2px solid #fff" onto one side (0 top, 1 right, 2 bottom, 3 left); a style of none takes it away.
function bgBorderSide(b, side, value) {
  let width = 3;   // CSS's medium
  let none = false;
  for (const word of cssWords(value)) {
    const w = word.toLowerCase();
    if (w === 'none' || w === 'hidden') none = true;
    else if (w === 'thin' || w === 'medium' || w === 'thick' || bgLength(w) !== null) width = bgBorderWidth(w);
    else { const c = bgTextColour(w); if (c) b.borderColour[side] = c; }
  }
  b.borderWidth[side] = none ? 0 : width;
}

function bgCorner(b, i, word) {
  const w = String(word).trim();
  b.radiusPercent[i] = w.endsWith('%');
  b.radius[i] = Math.max(0, b.radiusPercent[i] ? (Number.isFinite(parseFloat(w)) ? parseFloat(w) : 0) : bgLength(w) ?? 0);
}

/// The box-shadow list: [{ x, y, blur, spread, colour, inset }], the first on top.
function parseBoxShadows(value) {
  const shadows = [];
  if (String(value).trim().toLowerCase() === 'none') return shadows;
  for (const item of cssCommaList(value)) {
    const numbers = [];
    const s = { x: 0, y: 0, blur: 0, spread: 0, colour: [0, 0, 0, 128], inset: false };
    for (const word of cssWords(item)) {
      if (word.toLowerCase() === 'inset') s.inset = true;
      else if (bgLength(word) !== null) numbers.push(bgLength(word));
      else { const c = bgTextColour(word); if (c) s.colour = c; }
    }
    if (numbers.length < 2) continue;
    s.x = numbers[0];
    s.y = numbers[1];
    s.blur = numbers.length > 2 ? Math.max(0, numbers[2]) : 0;
    s.spread = numbers.length > 3 ? numbers[3] : 0;
    shadows.push(s);
  }
  return shadows;
}

/// A background from its declarations, or null when it draws nothing.
function parseBackground(declarations) {
  if (!declarations || !declarations.trim()) return null;
  const b = { image: null, colour: null, tint: [255, 255, 255, 255], rect: null, scaleMode: null, size: null, position: null, repeat: null,
    left: 0, top: 0, right: 0, bottom: 0, sliceScale: 1, tiled: false, linear: true, sprite: null, sliceGiven: false,
    gradient: null, borderWidth: [0, 0, 0, 0], borderColour: [[255, 255, 255, 255], [255, 255, 255, 255], [255, 255, 255, 255], [255, 255, 255, 255]],
    radius: [0, 0, 0, 0], radiusPercent: [false, false, false, false], shadows: [] };
  let borderNone = false;
  for (const [k, raw] of styleDeclarations(declarations)) {
    const v = raw.trim();
    switch (k) {
      case 'background-color': b.colour = bgColour(v); break;
      case 'background-image':
        b.gradient = parseGradient(v);
        b.image = b.gradient ? null : bgImage(v);
        break;
      case 'border': for (let i = 0; i < 4; i++) bgBorderSide(b, i, v); break;
      case 'border-top': bgBorderSide(b, 0, v); break;
      case 'border-right': bgBorderSide(b, 1, v); break;
      case 'border-bottom': bgBorderSide(b, 2, v); break;
      case 'border-left': bgBorderSide(b, 3, v); break;
      case 'border-width': bgFour(v, bgBorderWidth, b.borderWidth); break;
      case 'border-top-width': b.borderWidth[0] = bgBorderWidth(v); break;
      case 'border-right-width': b.borderWidth[1] = bgBorderWidth(v); break;
      case 'border-bottom-width': b.borderWidth[2] = bgBorderWidth(v); break;
      case 'border-left-width': b.borderWidth[3] = bgBorderWidth(v); break;
      case 'border-color': bgFour(v, w => bgTextColour(w) || [255, 255, 255, 255], b.borderColour); break;
      case 'border-top-color': b.borderColour[0] = bgTextColour(v) || b.borderColour[0]; break;
      case 'border-right-color': b.borderColour[1] = bgTextColour(v) || b.borderColour[1]; break;
      case 'border-bottom-color': b.borderColour[2] = bgTextColour(v) || b.borderColour[2]; break;
      case 'border-left-color': b.borderColour[3] = bgTextColour(v) || b.borderColour[3]; break;
      case 'border-style': borderNone = ['none', 'hidden'].includes(v.toLowerCase()); break;
      case 'border-radius': {
        const parts = v.split('/')[0].split(' ').filter(Boolean);   // elliptical corners: their horizontal radii
        if (!parts.length) break;
        const four = parts.length === 1 ? [parts[0], parts[0], parts[0], parts[0]] : parts.length === 2 ? [parts[0], parts[1], parts[0], parts[1]]
          : parts.length === 3 ? [parts[0], parts[1], parts[2], parts[1]] : parts.slice(0, 4);
        four.forEach((w, i) => bgCorner(b, i, w));
        break;
      }
      case 'border-top-left-radius': bgCorner(b, 0, v.split(' ')[0]); break;
      case 'border-top-right-radius': bgCorner(b, 1, v.split(' ')[0]); break;
      case 'border-bottom-right-radius': bgCorner(b, 2, v.split(' ')[0]); break;
      case 'border-bottom-left-radius': bgCorner(b, 3, v.split(' ')[0]); break;
      case 'box-shadow': b.shadows = parseBoxShadows(v); break;
      case '-ff-background-rect': {
        const r = v.split(/[\s,]+/).filter(Boolean).map(p => parseInt(p.replace('px', ''), 10));
        b.rect = r.length === 4 && r.every(n => n >= 0) && r[2] > 0 && r[3] > 0 ? r : null;
        break;
      }
      case '-ff-background-tint': b.tint = bgColour(v) || [255, 255, 255, 255]; break;
      case '-ff-background-scale-mode': b.scaleMode = v.toLowerCase(); break;
      case 'background-size': b.size = v.toLowerCase(); break;
      case 'background-position': b.position = v.toLowerCase(); break;
      case 'background-repeat': b.repeat = v.toLowerCase(); break;
      case '-ff-sprite': b.sprite = v.replace(/^["']|["']$/g, '').trim() || null; if (b.sprite === 'none') b.sprite = null; break;
      case '-ff-slice': {
        b.sliceGiven = true;
        const s = v.split(/\s+/).filter(Boolean).map(bgNumber);
        if (s.length === 1) b.top = b.right = b.bottom = b.left = s[0];
        else if (s.length === 2) { b.top = b.bottom = s[0]; b.right = b.left = s[1]; }
        else if (s.length === 3) { b.top = s[0]; b.right = b.left = s[1]; b.bottom = s[2]; }
        else if (s.length >= 4) [b.top, b.right, b.bottom, b.left] = s;
        break;
      }
      case '-ff-slice-left': b.left = bgNumber(v); b.sliceGiven = true; break;
      case '-ff-slice-top': b.top = bgNumber(v); b.sliceGiven = true; break;
      case '-ff-slice-right': b.right = bgNumber(v); b.sliceGiven = true; break;
      case '-ff-slice-bottom': b.bottom = bgNumber(v); b.sliceGiven = true; break;
      case '-ff-slice-scale': { const f = bgNumber(v); b.sliceScale = f > 0 ? Math.max(0.01, f) : 1; break; }
      case '-ff-slice-type': b.tiled = v.toLowerCase() === 'tiled'; break;
      case '-ff-background-filter': b.linear = v.toLowerCase() !== 'point'; break;
    }
  }
  if (borderNone) b.borderWidth = [0, 0, 0, 0];
  // A named sprite of the sheet (sprites.json): its part and its borders, where the frame gave none of its own.
  if (b.sprite && b.image && typeof findSprite === 'function') {
    const sp = findSprite(b.image, b.sprite);
    if (sp) {
      if (!b.rect && sp.w > 0 && sp.h > 0) b.rect = [sp.x, sp.y, sp.w, sp.h];
      if (!b.sliceGiven) { b.left = sp.left || 0; b.top = sp.top || 0; b.right = sp.right || 0; b.bottom = sp.bottom || 0; }
    }
  }
  b.hasBorder = b.borderWidth.some(w => w > 0);
  b.hasRadius = b.radius.some(r => r > 0);
  // Painted (bgPaint) rather than a colour and a picture's quads: a gradient, a border, round corners or a shadow.
  b.decorated = !!b.gradient || b.hasBorder || b.hasRadius || b.shadows.length > 0;
  const empty = !b.image && !b.gradient && !b.hasBorder && !b.shadows.length && (!b.colour || b.colour[3] === 0);
  return empty ? null : b;
}

function bgPlace(word, room, start, end) {
  if (word === start) return 0;
  if (word === end) return room;
  if (word === 'center' || word === 'centre') return room / 2;
  if (word.endsWith('%')) return room * parseFloat(word) / 100;
  return bgNumber(word);
}

function bgTilePosition(b, w, h, tw, th) {
  const parts = (b.position || 'left top').split(/\s+/).filter(Boolean);
  let px = parts[0] || 'left', py = parts.length > 1 ? parts[1] : (px === 'top' || px === 'bottom' ? px : 'center');
  if (px === 'top' || px === 'bottom') { const t = px; px = py === 'top' || py === 'bottom' ? 'center' : py; py = t; }
  return [bgPlace(px, w - tw, 'left', 'right'), bgPlace(py, h - th, 'top', 'bottom')];
}

/// The quads: [{ x, y, w, h (frame units), u, v, uw, vh (picture pixels) }], as MenuBackground.Layout gives them.
function layoutBackground(b, w, h, iw, ih) {
  const quads = [];
  if (w <= 0 || h <= 0 || iw <= 0 || ih <= 0) return quads;
  let sx = 0, sy = 0, sw = iw, sh = ih;
  if (b.rect) { [sx, sy] = b.rect; sw = Math.min(b.rect[2], iw - sx); sh = Math.min(b.rect[3], ih - sy); }
  if (sw <= 0 || sh <= 0) return quads;
  const q = (x, y, qw, qh, u, v, uw, vh) => quads.push({ x, y, w: qw, h: qh, u, v, uw, vh });

  if (b.left > 0 || b.top > 0 || b.right > 0 || b.bottom > 0) {
    const l = Math.min(b.left, sw), r = Math.min(b.right, sw - l), t = Math.min(b.top, sh), bo = Math.min(b.bottom, sh - t);
    let L = l * b.sliceScale, R = r * b.sliceScale, T = t * b.sliceScale, B = bo * b.sliceScale;
    if (L + R > w && L + R > 0) { const k = w / (L + R); L *= k; R *= k; }
    if (T + B > h && T + B > 0) { const k = h / (T + B); T *= k; B *= k; }
    const dx = [0, L, w - R, w], dy = [0, T, h - B, h];
    const ux = [sx, sx + l, sx + sw - r, sx + sw], uy = [sy, sy + t, sy + sh - bo, sy + sh];
    for (let row = 0; row < 3; row++) {
      for (let col = 0; col < 3; col++) {
        const x = dx[col], y = dy[row], cw = dx[col + 1] - x, ch = dy[row + 1] - y;
        const u = ux[col], v = uy[row], uw = ux[col + 1] - u, vh = uy[row + 1] - v;
        if (cw <= 0 || ch <= 0 || uw <= 0 || vh <= 0) continue;
        const corner = col !== 1 && row !== 1;
        if (!b.tiled || corner) { q(x, y, cw, ch, u, v, uw, vh); continue; }
        const tw = col === 1 ? uw * b.sliceScale : cw, th = row === 1 ? vh * b.sliceScale : ch;
        if (tw <= 0.5 || th <= 0.5) { q(x, y, cw, ch, u, v, uw, vh); continue; }
        let guard = 0;
        for (let ty = y; ty < y + ch - 0.01 && guard < 4096; ty += th) {
          for (let tx = x; tx < x + cw - 0.01 && guard < 4096; tx += tw, guard++) {
            const qw = Math.min(tw, x + cw - tx), qh = Math.min(th, y + ch - ty);
            q(tx, ty, qw, qh, u, v, uw * qw / tw, vh * qh / th);
          }
        }
      }
    }
    return quads;
  }

  let css = b.size !== null || (b.repeat !== null && b.repeat !== 'no-repeat') || b.position !== null;
  let mode = css ? null : (b.scaleMode || 'stretch-to-fill');
  if (b.size === 'cover') { mode = 'scale-and-crop'; css = false; }
  else if (b.size === 'contain') { mode = 'scale-to-fit'; css = false; }
  const fraction = () => {
    if (b.position === null) return [0.5, 0.5];
    const [x, y] = bgTilePosition(b, 1, 1, 0, 0);
    return [Math.max(0, Math.min(1, x)), Math.max(0, Math.min(1, y))];
  };
  if (mode === 'scale-and-crop') {
    const s = Math.max(w / sw, h / sh), vw = w / s, vh = h / s;
    const [px, py] = fraction();
    q(0, 0, w, h, sx + (sw - vw) * px, sy + (sh - vh) * py, vw, vh);
    return quads;
  }
  if (mode === 'scale-to-fit') {
    const s = Math.min(w / sw, h / sh), dw = sw * s, dh = sh * s;
    const [px, py] = fraction();
    q((w - dw) * px, (h - dh) * py, dw, dh, sx, sy, sw, sh);
    return quads;
  }
  if (!css) { q(0, 0, w, h, sx, sy, sw, sh); return quads; }

  const length = (text, whole) => text === 'auto' ? null : text.endsWith('%') ? whole * parseFloat(text) / 100 : bgNumber(text);
  const parts = (b.size || 'auto').split(/\s+/).filter(Boolean);
  const a = length(parts[0] || 'auto', w), c = length(parts[1] || 'auto', h);
  const [tw, th] = a === null && c === null ? [sw, sh] : a === null ? [c * sw / sh, c] : c === null ? [a, a * sh / sw] : [Math.max(0.5, a), Math.max(0.5, c)];
  const [ox, oy] = bgTilePosition(b, w, h, tw, th);
  const rx = b.repeat === 'repeat' || b.repeat === 'repeat-x', ry = b.repeat === 'repeat' || b.repeat === 'repeat-y';
  const x0 = rx ? ox - Math.ceil(ox / tw) * tw : ox, y0 = ry ? oy - Math.ceil(oy / th) * th : oy;
  let guard = 0;
  for (let y = y0; y < h && guard < 4096; y += th) {
    for (let x = x0; x < w && guard < 4096; x += tw) {
      const x1 = Math.max(0, x), y1 = Math.max(0, y), x2 = Math.min(w, x + tw), y2 = Math.min(h, y + th);
      if (x2 > x1 && y2 > y1) q(x1, y1, x2 - x1, y2 - y1, sx + (x1 - x) * sw / tw, sy + (y1 - y) * sh / th, (x2 - x1) * sw / tw, (y2 - y1) * sh / th);
      guard++;
      if (!rx) break;
    }
    if (!ry) break;
  }
  return quads;
}

// ------------------------------------------------------------------ pictures

const bgPictures = new Map();

/// Where a background picture is served from: a file beside the screen, or one of the game's.
function backgroundPictureUrl(image) {
  if (!image) return null;
  if (image.kind === 'resource') return wsUrl(`/api/image?name=${encodeURIComponent(image.path)}`);
  const client = typeof menu !== 'undefined' && menu.project && menu.project.client;
  return `${client ? '/api/client/menu/file' : '/api/project/menu/file'}?path=${encodeURIComponent(image.path)}`;
}

function loadBackgroundPicture(image) {
  const url = backgroundPictureUrl(image);
  if (!url) return Promise.resolve(null);
  if (!bgPictures.has(url)) {
    bgPictures.set(url, new Promise(resolve => {
      const img = new Image();
      img.onload = () => resolve(img.width ? img : null);
      img.onerror = () => resolve(null);
      img.src = url;
    }));
  }
  return bgPictures.get(url);
}

// ------------------------------------------------------------------ the box painted (MenuPaint)
//
// The gradient, the border, the round corners and the shadows, painted with the canvas as
// Shared/Text/MenuPaint.cs paints them into pictures of their own: the outer shadows (the first on
// top, blurred with a Gaussian of sigma blur / 2, cut away under the box itself) apart, to go behind
// the game's window; the colour and the gradient clipped to the round box, under the picture; the
// inset shadows and the border ring over it.

/// The corners' radii on a frame w by h, shrunk together where two would overlap (MenuBackground.Radii) - a percent is of the shorter side.
function bgRadii(b, w, h) {
  const shorter = Math.min(w, h);
  const r = b.radius.map((v, i) => Math.max(0, b.radiusPercent[i] ? shorter * v / 100 : v));
  let f = 1;
  if (r[0] + r[1] > w) f = Math.min(f, w / (r[0] + r[1]));
  if (r[3] + r[2] > w) f = Math.min(f, w / (r[3] + r[2]));
  if (r[0] + r[3] > h) f = Math.min(f, h / (r[0] + r[3]));
  if (r[1] + r[2] > h) f = Math.min(f, h / (r[1] + r[2]));
  return f < 1 ? r.map(x => x * f) : r;
}

/// A box with round corners (top-left, top-right, bottom-right, bottom-left), and one grown or moved - as MenuPaint.Box.
const bgBox = (x0, y0, x1, y1, r) => ({ x0, y0, x1: Math.max(x0, x1), y1: Math.max(y0, y1), r });
const bgBoxOffset = (b, dx, dy) => bgBox(b.x0 + dx, b.y0 + dy, b.x1 + dx, b.y1 + dy, b.r);
const bgBoxGrown = (b, by) => bgBox(b.x0 - by, b.y0 - by, b.x1 + by, b.y1 + by, b.r.map(x => x > 0 ? Math.max(0, x + by) : 0));

/// The box's outline onto the path (a corner no bigger than half the shorter side, as the distance MenuPaint measures has it).
function bgBoxPath(gc, b, reverse) {
  const hw = (b.x1 - b.x0) / 2, hh = (b.y1 - b.y0) / 2;
  if (hw <= 0 || hh <= 0) return;
  const [r0, r1, r2, r3] = b.r.map(r => Math.min(r, hw, hh));
  const P = Math.PI;
  if (!reverse) {
    gc.moveTo(b.x0 + r0, b.y0);
    gc.lineTo(b.x1 - r1, b.y0);
    if (r1 > 0) gc.arc(b.x1 - r1, b.y0 + r1, r1, -P / 2, 0);
    gc.lineTo(b.x1, b.y1 - r2);
    if (r2 > 0) gc.arc(b.x1 - r2, b.y1 - r2, r2, 0, P / 2);
    gc.lineTo(b.x0 + r3, b.y1);
    if (r3 > 0) gc.arc(b.x0 + r3, b.y1 - r3, r3, P / 2, P);
    gc.lineTo(b.x0, b.y0 + r0);
    if (r0 > 0) gc.arc(b.x0 + r0, b.y0 + r0, r0, P, P * 1.5);
  } else {
    gc.moveTo(b.x0 + r0, b.y0);
    if (r0 > 0) gc.arc(b.x0 + r0, b.y0 + r0, r0, P * 1.5, P, true);
    gc.lineTo(b.x0, b.y1 - r3);
    if (r3 > 0) gc.arc(b.x0 + r3, b.y1 - r3, r3, P, P / 2, true);
    gc.lineTo(b.x1 - r2, b.y1);
    if (r2 > 0) gc.arc(b.x1 - r2, b.y1 - r2, r2, P / 2, 0, true);
    gc.lineTo(b.x1, b.y0 + r1);
    if (r1 > 0) gc.arc(b.x1 - r1, b.y0 + r1, r1, 0, -P / 2, true);
  }
  gc.closePath();
}

/// How far past the frame the outer shadows reach (menu units): the shadow picture's margin.
function bgShadowReach(b) {
  let m = 0;
  for (const s of b.shadows) if (!s.inset) m = Math.max(m, s.blur + Math.max(0, s.spread) + Math.max(Math.abs(s.x), Math.abs(s.y)));
  return Math.ceil(m);
}

function bgGradientLength(text, whole) {
  const t = String(text ?? '50%').trim().toLowerCase();
  if (t.endsWith('%') && Number.isFinite(parseFloat(t))) return whole * parseFloat(t) / 100;
  return bgLength(t) ?? whole / 2;
}

/// A radial gradient's radii: its lengths, or its size keyword from the centre to the frame's sides or corners.
function bgRadialSize(g, w, h, cx, cy) {
  if (g.sizeWord === null) {
    const sx = bgGradientLength(g.sizeX, w), sy = g.circle ? sx : bgGradientLength(g.sizeY, h);
    return [Math.max(0.01, sx), Math.max(0.01, sy)];
  }
  const nearX = Math.min(Math.abs(cx), Math.abs(w - cx)), farX = Math.max(Math.abs(cx), Math.abs(w - cx));
  const nearY = Math.min(Math.abs(cy), Math.abs(h - cy)), farY = Math.max(Math.abs(cy), Math.abs(h - cy));
  let rx, ry;
  switch (g.sizeWord) {
    case 'closest-side': rx = nearX; ry = nearY; if (g.circle) rx = ry = Math.min(nearX, nearY); break;
    case 'farthest-side': rx = farX; ry = farY; if (g.circle) rx = ry = Math.max(farX, farY); break;
    case 'closest-corner':
      if (g.circle) rx = ry = Math.hypot(nearX, nearY); else { rx = nearX * Math.SQRT2; ry = nearY * Math.SQRT2; }
      break;
    default:
      if (g.circle) rx = ry = Math.hypot(farX, farY); else { rx = farX * Math.SQRT2; ry = farY * Math.SQRT2; }
  }
  return [Math.max(0.01, rx), Math.max(0.01, ry)];
}

function bgLinearDirection(g, w, h) {
  let dx, dy;
  if (g.cornerX !== 0 && g.cornerY !== 0) {
    // To a corner: at right angles to the line between the two corners beside it.
    dx = h * g.cornerX; dy = w * g.cornerY;
    const n = Math.hypot(dx, dy); dx /= n; dy /= n;
  } else { const a = g.angle * Math.PI / 180; dx = Math.sin(a); dy = -Math.cos(a); }
  return [dx, dy, Math.abs(w * dx) + Math.abs(h * dy)];
}

/// The stops at their places (0..1 of the gradient's line, its radius or the turn), as MenuPaint.Stops places them: [{ at, c: [r, g, b, a] 0..1 }].
function bgGradientStops(g, w, h) {
  const length = g.kind === 'linear' ? bgLinearDirection(g, w, h)[2] : g.kind === 'radial' ? bgRadialSize(g, w, h, bgGradientLength(g.atX, w), bgGradientLength(g.atY, h))[0] : 360;
  const place = position => {
    if (position === null) return null;
    const p = String(position).trim().toLowerCase();
    if (p.endsWith('%') && Number.isFinite(parseFloat(p))) return parseFloat(p) / 100;
    if (g.kind === 'conic' && bgDegrees(p) !== null) return bgDegrees(p) / 360;
    const px = bgLength(p);
    if (px !== null) return length > 0 ? px / length : 0;
    return null;
  };
  const at = g.stops.map(s => place(s.position));
  if (at.length && at[0] === null) at[0] = 0;
  if (at.length > 1 && at[at.length - 1] === null) at[at.length - 1] = 1;
  for (let i = 0; i < at.length; i++) {
    if (at[i] !== null) continue;
    let next = i;
    while (next < at.length && at[next] === null) next++;
    const a = at[i - 1], b = at[next];
    for (let k = i; k < next; k++) at[k] = a + (b - a) * (k - i + 1) / (next - i + 1);
  }
  let max = -Infinity;
  const stops = at.map((p, i) => {
    max = Math.max(max, p ?? 0);
    return { at: max, c: g.stops[i].colour.map(n => n / 255) };
  });
  if (stops.length === 1) stops.push({ at: 1, c: stops[0].c });
  return stops;
}

/// The colour at a place along the stops (straight RGBA 0..1), mixed premultiplied between the two around it.
function bgGradientAt(stops, t) {
  if (t <= stops[0].at) return stops[0].c;
  for (let i = 1; i < stops.length; i++) {
    if (t > stops[i].at) continue;
    const a = stops[i - 1], b = stops[i];
    const f = b.at - a.at <= 0 ? 1 : (t - a.at) / (b.at - a.at);
    const alpha = a.c[3] + (b.c[3] - a.c[3]) * f;
    if (alpha <= 0) return [0, 0, 0, 0];
    const ch = k => (a.c[k] * a.c[3] + (b.c[k] * b.c[3] - a.c[k] * a.c[3]) * f) / alpha;
    return [ch(0), ch(1), ch(2), alpha];
  }
  return stops[stops.length - 1].c;
}

/// A canvas gradient standing for a CSS one over [t0, t1] of its line: the colours sampled from the stops as MenuPaint
/// mixes them (the canvas mixes straight, not premultiplied, so a stop going see-through is sampled finer), repeated when it repeats.
function bgCanvasStops(canvasGradient, g, stops, t0, t1) {
  const first = stops[0].at, last = stops[stops.length - 1].at, span = last - first;
  const repeating = g.repeating && span > 0.0001;
  const value = t => bgGradientAt(stops, repeating ? first + (((t - first) % span) + span) % span : t);
  const marks = new Set([t0, t1]);
  if (repeating) {
    for (let n = Math.floor((t0 - first) / span) - 1; first + n * span <= t1 + span && marks.size < 4000; n++) {
      for (const s of stops) { const p = s.at + n * span; if (p > t0 && p < t1) marks.add(p); }
    }
  } else for (const s of stops) if (s.at > t0 && s.at < t1) marks.add(s.at);
  const points = [...marks].sort((a, b) => a - b);
  const room = t1 - t0 || 1;
  const put = (t, c) => canvasGradient.addColorStop(Math.min(1, Math.max(0, (t - t0) / room)), `rgba(${Math.round(c[0] * 255)}, ${Math.round(c[1] * 255)}, ${Math.round(c[2] * 255)}, ${c[3]})`);
  for (let i = 0; i + 1 < points.length; i++) {
    const u = points[i], v = points[i + 1], e = (v - u) * 1e-4;
    const a = value(u + e), b = value(v - e);
    put(u, a);
    if (Math.abs(a[3] - b[3]) > 0.01) for (let k = 1; k < 8; k++) { const t = u + (v - u) * k / 8; put(t, value(t)); }
    put(v, b);
  }
}

/// The gradient over the frame (w by h), into the path the caller has clipped to.
function bgPaintGradient(gc, g, w, h) {
  const stops = bgGradientStops(g, w, h);
  if (!stops.length) return;
  const cx = bgGradientLength(g.atX, w), cy = bgGradientLength(g.atY, h);
  gc.save();
  let fill;
  if (g.kind === 'linear') {
    // The gradient line through the middle, as long as the frame's corners need.
    const [dx, dy, length] = bgLinearDirection(g, w, h);
    fill = gc.createLinearGradient(w / 2 - dx * length / 2, h / 2 - dy * length / 2, w / 2 + dx * length / 2, h / 2 + dy * length / 2);
    bgCanvasStops(fill, g, stops, 0, 1);
    gc.fillStyle = fill;
    gc.fillRect(0, 0, w, h);
  } else if (g.kind === 'radial') {
    const [rx, ry] = bgRadialSize(g, w, h, cx, cy);
    // As far out as the farthest corner, so the stops past the radius are the canvas's too.
    const reach = Math.max(1, ...[[0, 0], [w, 0], [0, h], [w, h]].map(([x, y]) => Math.hypot((x - cx) / rx, (y - cy) / ry)));
    gc.translate(cx, cy);
    gc.scale(1, ry / rx);
    fill = gc.createRadialGradient(0, 0, 0, 0, 0, rx * reach);
    bgCanvasStops(fill, g, stops, 0, reach);
    gc.fillStyle = fill;
    gc.fillRect(-cx, -cy * rx / ry, w, h * rx / ry);
  } else {
    // CSS's 0deg is up and goes round clockwise; the canvas's starts at the right.
    fill = gc.createConicGradient((g.from - 90) * Math.PI / 180, cx, cy);
    bgCanvasStops(fill, g, stops, 0, 1);
    gc.fillStyle = fill;
    gc.fillRect(0, 0, w, h);
  }
  gc.restore();
}

/// The part of the rectangle w by h nearest a side (in that side's widths), as MenuPaint.Sides colours the ring: a polygon.
function bgSidePolygon(side, widths, w, h) {
  // Each side's measure a x + b y + c: y / top, (w - x) / right, (h - y) / bottom, x / left.
  const d = [[0, 1 / widths[0], 0], [-1 / widths[1], 0, w / widths[1]], [0, -1 / widths[2], h / widths[2]], [1 / widths[3], 0, 0]];
  let poly = [[0, 0], [w, 0], [w, h], [0, h]];
  for (let j = 0; j < 4 && poly.length; j++) {
    if (j === side || !(widths[j] > 0)) continue;
    // Keep where this side's measure is no more than the other's.
    const a = d[j][0] - d[side][0], b = d[j][1] - d[side][1], c = d[j][2] - d[side][2];
    const out = [];
    for (let i = 0; i < poly.length; i++) {
      const p = poly[i], q = poly[(i + 1) % poly.length];
      const fp = a * p[0] + b * p[1] + c, fq = a * q[0] + b * q[1] + c;
      if (fp >= 0) out.push(p);
      if ((fp >= 0) !== (fq >= 0)) { const t = fp / (fp - fq); out.push([p[0] + (q[0] - p[0]) * t, p[1] + (q[1] - p[1]) * t]); }
    }
    poly = out;
  }
  return poly;
}

/// One layer of the box ('shadow', 'fill' or 'over') onto a context set to menu units (the frame's top left at 0, 0),
/// `scale` pixels a unit and `origin` its device offset: shadowBlur and shadowOffset are the canvas's own pixels.
function bgPaint(gc, b, w, h, scale, origin, layer) {
  const radii = bgRadii(b, w, h);
  const border = bgBox(0, 0, w, h, radii);
  const [bt, br, bb, bl] = b.borderWidth;
  const padding = bgBox(bl, bt, w - br, h - bb, [Math.max(0, radii[0] - Math.max(bl, bt)), Math.max(0, radii[1] - Math.max(br, bt)), Math.max(0, radii[2] - Math.max(br, bb)), Math.max(0, radii[3] - Math.max(bl, bb))]);
  // A shape drawn this far off the canvas throws only its shadow onto it.
  const far = 20000;
  const shadowed = (colour, blur, draw) => {
    gc.save();
    gc.setTransform(scale, 0, 0, scale, origin[0] - far, origin[1]);
    gc.shadowColor = bgCss(colour);
    gc.shadowBlur = blur * scale;
    gc.shadowOffsetX = far;
    gc.fillStyle = '#000';
    draw();
    gc.restore();
  };
  if (layer === 'shadow') {
    // The outer shadows, the last underneath; cut away where the frame is (CSS draws none under a see-through box).
    for (const s of [...b.shadows].reverse()) {
      if (s.inset || s.colour[3] === 0) continue;
      const shape = bgBoxGrown(bgBoxOffset(border, s.x, s.y), s.spread);
      shadowed(s.colour, s.blur, () => { gc.beginPath(); bgBoxPath(gc, shape); gc.fill(); });
    }
    gc.save();
    gc.globalCompositeOperation = 'destination-out';
    gc.beginPath();
    bgBoxPath(gc, border);
    gc.fill();
    gc.restore();
    return;
  }
  if (layer === 'fill') {
    gc.save();
    gc.beginPath();
    bgBoxPath(gc, border);
    gc.clip();
    if (b.colour && b.colour[3] > 0) { gc.fillStyle = bgCss(b.colour); gc.fillRect(0, 0, w, h); }
    if (b.gradient) bgPaintGradient(gc, b.gradient, w, h);
    gc.restore();
    return;
  }
  // Over: the inset shadows inside the padding box, then the border.
  const insets = [...b.shadows].reverse().filter(s => s.inset && s.colour[3] > 0);
  if (insets.length) {
    gc.save();
    gc.beginPath();
    bgBoxPath(gc, padding);
    gc.clip();
    for (const s of insets) {
      const hole = bgBoxGrown(bgBoxOffset(padding, s.x, s.y), -s.spread);
      const big = w + h + s.blur * 3 + Math.abs(s.x) + Math.abs(s.y) + 64;
      shadowed(s.colour, s.blur, () => {
        gc.beginPath();
        gc.rect(-big, -big, w + big * 2, h + big * 2);
        bgBoxPath(gc, hole, true);
        gc.fill('evenodd');
      });
    }
    gc.restore();
  }
  if (b.hasBorder) {
    gc.save();
    gc.beginPath();
    bgBoxPath(gc, border);
    bgBoxPath(gc, padding, true);
    const same = b.borderColour.every(c => c.join() === b.borderColour[0].join());
    if (same) { gc.fillStyle = bgCss(b.borderColour[0]); gc.fill('evenodd'); }
    else {
      gc.clip('evenodd');
      for (let side = 0; side < 4; side++) {
        if (!(b.borderWidth[side] > 0)) continue;
        const poly = bgSidePolygon(side, b.borderWidth, w, h);
        if (poly.length < 3) continue;
        gc.beginPath();
        poly.forEach(([x, y], i) => i ? gc.lineTo(x, y) : gc.moveTo(x, y));
        gc.closePath();
        gc.fillStyle = bgCss(b.borderColour[side]);
        gc.fill();
      }
    }
    gc.restore();
  }
}

/// The frame's background drawn at `scale` pixels a unit: its colour (or, painted, the colour and gradient in the
/// round box), the picture's quads tinted, then the inset shadows and the border. Its outer shadows, when it has
/// any, are a canvas of their own (the result's .shadow), reaching past the frame, for behind the game's window.
async function drawFrameBackground(declarations, width, height, scale, opacity = 1) {
  const b = parseBackground(declarations);
  if (!b || width <= 0 || height <= 0) return null;
  // A big frame is painted at less, as MenuPaint paints one at no more than about a million pixels (four here).
  scale = Math.max(0.5, Math.min(scale, Math.sqrt(4000000 / Math.max(1, width * height))));
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.ceil(width * scale));
  canvas.height = Math.max(1, Math.ceil(height * scale));
  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;
  canvas.className = 'frame-background';
  const gc = canvas.getContext('2d');
  gc.scale(scale, scale);
  if (b.decorated) bgPaint(gc, b, width, height, scale, [0, 0], 'fill');
  else if (b.colour && b.colour[3] > 0) {
    gc.fillStyle = bgCss(b.colour);
    gc.fillRect(0, 0, width, height);
  }
  const img = b.image ? await loadBackgroundPicture(b.image) : null;
  if (img) {
    // Tinted the way the client's sprites modulate: the picture multiplied by the tint, its own alpha times the tint's.
    let source = img;
    const [tr, tg, tb, ta] = b.tint;
    if (tr !== 255 || tg !== 255 || tb !== 255) {
      const t = document.createElement('canvas');
      t.width = img.width;
      t.height = img.height;
      const tc = t.getContext('2d');
      tc.drawImage(img, 0, 0);
      tc.globalCompositeOperation = 'multiply';
      tc.fillStyle = `rgb(${tr}, ${tg}, ${tb})`;
      tc.fillRect(0, 0, t.width, t.height);
      tc.globalCompositeOperation = 'destination-in';
      tc.drawImage(img, 0, 0);
      source = t;
    }
    gc.save();
    gc.globalAlpha = ta / 255;
    gc.imageSmoothingEnabled = b.linear;
    for (const quad of layoutBackground(b, width, height, img.width, img.height)) {
      gc.drawImage(source, quad.u, quad.v, quad.uw, quad.vh, quad.x, quad.y, quad.w, quad.h);
    }
    gc.restore();
  }
  if (b.decorated) bgPaint(gc, b, width, height, scale, [0, 0], 'over');
  canvas.style.opacity = String(opacity);
  if (b.shadows.some(s => !s.inset)) {
    const margin = bgShadowReach(b);
    const shadow = document.createElement('canvas');
    shadow.width = Math.max(1, Math.ceil((width + margin * 2) * scale));
    shadow.height = Math.max(1, Math.ceil((height + margin * 2) * scale));
    shadow.style.width = `${width + margin * 2}px`;
    shadow.style.height = `${height + margin * 2}px`;
    shadow.style.left = `${-margin}px`;
    shadow.style.top = `${-margin}px`;
    shadow.className = 'frame-shadow';
    const sc = shadow.getContext('2d');
    sc.setTransform(scale, 0, 0, scale, margin * scale, margin * scale);
    bgPaint(sc, b, width, height, scale, [margin * scale, margin * scale], 'shadow');
    shadow.style.opacity = String(opacity);
    canvas.shadow = shadow;
  }
  return canvas;
}

// ------------------------------------------------------------------ the picker

/// Choose a picture: the screen's own (images/ beside it), or one of the game's; import a PNG of your own. Calls back with the background-image value.
async function pickBackgroundPicture(current, onChosen) {
  const veil = document.createElement('div');
  veil.className = 'picker-veil';
  const box = document.createElement('div');
  box.className = 'picker bg-picker';
  const head = document.createElement('div');
  head.className = 'picker-head';
  const title = document.createElement('strong');
  title.textContent = 'Choose a picture';
  const tabs = document.createElement('div');
  tabs.className = 'bg-picker-tabs';
  const filter = document.createElement('input');
  filter.type = 'search';
  filter.placeholder = 'filter';
  const importButton = document.createElement('button');
  importButton.textContent = 'Import PNG…';
  importButton.title = 'Copy a picture of your own into images/ beside the screen';
  const file = document.createElement('input');
  file.type = 'file';
  file.accept = 'image/png,image/jpeg,image/bmp';
  file.hidden = true;
  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  head.append(title, tabs, filter, importButton, file, shut);
  const grid = document.createElement('div');
  grid.className = 'picker-grid bg-picker-grid';
  const note = document.createElement('p');
  note.className = 'none bg-picker-note';
  box.append(head, grid, note);
  veil.append(box);
  document.body.append(veil);
  const close = () => { veil.remove(); document.removeEventListener('keydown', key, true); };
  const key = e => { if (e.key === 'Escape') { e.preventDefault(); close(); } };
  document.addEventListener('keydown', key, true);
  shut.onclick = close;
  veil.onclick = e => { if (e.target === veil) close(); };

  const client = menu.project && menu.project.client;
  const own = !!menu.project;
  const chosen = bgImage(current);
  const chosenKind = () => chosen && chosen.kind;
  let lists = { own: [], game: [] };
  let tab = own && !(chosenKind() === 'resource') ? 'own' : 'game';
  const tabButton = (id, label) => {
    const b = document.createElement('button');
    b.textContent = label;
    b.onclick = () => { tab = id; draw(); };
    b.dataset.tab = id;
    tabs.append(b);
  };
  if (own) tabButton('own', 'Beside the screen');
  tabButton('game', 'The game\'s');
  importButton.hidden = !own;

  const load = async () => {
    if (own) {
      try { const r = await api(client ? '/api/client/menu/images' : '/api/project/menu/images'); lists.own = (r.pictures || []).map(p => ({ value: `url("${p.path}")`, name: p.path, url: backgroundPictureUrl({ kind: 'url', path: p.path }), size: p.width ? `${p.width} × ${p.height}` : '' })); } catch (e) { lists.own = []; }
    }
    try {
      const images = await api('/api/images');
      lists.game = (images || []).filter(i => !i.problem && i.width).map(i => ({ value: `resource("${i.name}")`, name: i.name, url: wsUrl(`/api/image?name=${encodeURIComponent(i.name)}`), size: `${i.width} × ${i.height}` }));
    } catch (e) { lists.game = []; }
  };

  const draw = () => {
    for (const b of tabs.children) b.classList.toggle('on', b.dataset.tab === tab);
    grid.textContent = '';
    const words = filter.value.trim().toLowerCase();
    const list = lists[tab].filter(p => !words || p.name.toLowerCase().includes(words));
    note.textContent = tab === 'own'
      ? `url(...): files in images/ beside the screen - they ship with it. ${list.length} picture${list.length === 1 ? '' : 's'}.`
      : `resource(...): the game's own pictures by name, read from the player's install - nothing copied into the mod. ${list.length} picture${list.length === 1 ? '' : 's'}.`;
    const none = document.createElement('button');
    none.className = 'picker-cell bg-none';
    none.textContent = 'None';
    none.onclick = () => { close(); onChosen(null); };
    grid.append(none);
    for (const p of list.slice(0, 400)) {
      const cell = document.createElement('button');
      cell.className = 'picker-cell' + (chosen && chosen.path === p.name ? ' on' : '');
      cell.title = `${p.name}  ${p.size}`;
      const img = document.createElement('img');
      img.loading = 'lazy';
      img.src = p.url;
      const label = document.createElement('span');
      label.textContent = p.name.split('/').pop();
      const size = document.createElement('i');
      size.textContent = p.size;
      cell.append(img, label, size);
      cell.onclick = () => sprites(p);
      grid.append(cell);
    }
  };
  // A sheet chosen: its sprites (sprites.json), the whole picture, or the Sprite editor to make some.
  const sprites = async p => {
    const image = bgImage(p.value);
    const list = spritesOf(image);
    const game = image.kind === 'resource' ? await spritesFromCells(image) : [];
    if (!list.length && !game.length) { close(); onChosen(p.value, null); return; }
    const img = await loadBackgroundPicture(image);
    grid.textContent = '';
    tabs.hidden = true;
    filter.hidden = true;
    title.textContent = `${p.name.split('/').pop()} - which part?`;
    const back = document.createElement('button');
    back.className = 'picker-cell bg-none';
    back.textContent = '\u2190 Back';
    back.onclick = () => { tabs.hidden = false; filter.hidden = false; title.textContent = 'Choose a picture'; draw(); };
    const whole = document.createElement('button');
    whole.className = 'picker-cell';
    whole.title = 'All of the picture';
    if (img) whole.append(spriteThumbnail(img, { x: 0, y: 0, w: img.width, h: img.height }, 96));
    const wl = document.createElement('span');
    wl.textContent = 'The whole picture';
    whole.append(wl);
    whole.onclick = () => { close(); onChosen(p.value, null); };
    const editor = document.createElement('button');
    editor.className = 'picker-cell bg-none';
    editor.textContent = list.length ? 'Edit sprites\u2026' : 'Make sprites\u2026';
    editor.title = list.length ? 'The Sprite editor for this sheet' : `The Sprite editor - ${game.length} piece${game.length === 1 ? '' : 's'} of the game's own cells to start from`;
    editor.disabled = !menu.project;
    editor.onclick = () => { close(); editSprites(image, null, n => onChosen(p.value, n)); };
    grid.append(back, whole, editor);
    for (const sp of list) {
      const cell = document.createElement('button');
      cell.className = 'picker-cell';
      cell.title = `${sp.name}  ${sp.x} ${sp.y} ${sp.w}\u00d7${sp.h}${sp.left || sp.top || sp.right || sp.bottom ? `  borders ${sp.left} ${sp.top} ${sp.right} ${sp.bottom}` : ''}`;
      if (img) cell.append(spriteThumbnail(img, sp, 96));
      const n = document.createElement('span');
      n.textContent = sp.name;
      const d = document.createElement('i');
      d.textContent = `${sp.w} \u00d7 ${sp.h}${sp.left || sp.top || sp.right || sp.bottom ? ' \u00b7 sliced' : ''}`;
      cell.append(n, d);
      cell.onclick = () => { close(); onChosen(p.value, sp.name); };
      grid.append(cell);
    }
    note.textContent = list.length ? `${list.length} sprite${list.length === 1 ? '' : 's'} of this sheet (sprites.json) - each with its own borders.` : `No sprites of this sheet yet - Make sprites starts from the game's own ${game.length} pieces.`;
  };
  filter.oninput = draw;
  importButton.onclick = () => file.click();
  file.onchange = async () => {
    const f = file.files && file.files[0];
    if (!f) return;
    const r = await fetch(`${client ? '/api/client/menu/image/import' : '/api/project/menu/image/import'}?name=${encodeURIComponent(f.name)}`, { method: 'POST', body: f }).then(x => x.json()).catch(e => ({ ok: false, error: e.message }));
    if (!r.ok) return say(r.error, 'bad');
    say(`imported ${r.path}`, 'good');
    close();
    onChosen(`url("${r.path}")`);
  };
  grid.textContent = 'Loading…';
  await load();
  if (tab === 'own' && !lists.own.length) tab = 'game';
  draw();
  filter.focus();
}

// ------------------------------------------------------------------ the slice editor

/// The picture large, as Unity's Sprite Editor: its four borders as lines to drag, and (Pick part) a
/// rectangle dragged on the whole sheet for the one sprite of it to use. Calls back with the slices
/// [left, top, right, bottom] and the part [x, y, w, h] (null: all of the picture), in its pixels.
async function editSlices(image, rect, slices, onDone) {
  const img = await loadBackgroundPicture(image);
  if (!img) return say('the picture did not load', 'bad');
  let part = rect ? rect.slice() : null;
  let [l, t, r, b] = slices.map(v => Math.max(0, Math.round(v)));
  let picking = !part && (img.width > 256 || img.height > 256);   // a big sheet: the part first
  const veil = document.createElement('div');
  veil.className = 'picker-veil';
  const box = document.createElement('div');
  box.className = 'picker slice-editor';
  const head = document.createElement('div');
  head.className = 'picker-head';
  const title = document.createElement('strong');
  const pickButton = document.createElement('button');
  pickButton.title = 'Drag a rectangle on the whole picture: the part of it (one sprite of a sheet) the frame uses';
  const wholeButton = document.createElement('button');
  wholeButton.textContent = 'Whole picture';
  wholeButton.title = 'Use all of the picture';
  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  head.append(title, pickButton, wholeButton, shut);
  const stage = document.createElement('div');
  stage.className = 'slice-stage';
  const canvas = document.createElement('canvas');
  const gc = canvas.getContext('2d');
  stage.append(canvas);
  const fields = document.createElement('div');
  fields.className = 'slice-fields';
  const partFields = document.createElement('div');
  partFields.className = 'slice-fields';
  const inputs = {};
  const partInputs = [];
  let zoom = 1, view = [0, 0, img.width, img.height];

  const layout = () => {
    view = picking || !part ? [0, 0, img.width, img.height] : part;
    zoom = Math.max(picking ? 0.25 : 1, Math.min(12, Math.min(620 / view[2], 400 / view[3])));
    if (!picking) zoom = Math.max(1, Math.floor(zoom));
    canvas.width = Math.round(view[2] * zoom);
    canvas.height = Math.round(view[3] * zoom);
    const [, , pw, ph] = part || [0, 0, img.width, img.height];
    title.textContent = picking ? `Pick the part of ${image.path.split('/').pop()} (${img.width} × ${img.height}) - drag a rectangle` : `Slices of ${image.path.split('/').pop()} - ${part ? `part ${part.join(' ')}` : 'the whole picture'} (${pw} × ${ph})`;
    pickButton.textContent = picking ? 'Done picking' : 'Pick part…';
    fields.hidden = picking;
    redraw();
  };
  const clampAll = () => {
    const [, , pw, ph] = part || [0, 0, img.width, img.height];
    l = Math.max(0, Math.min(pw, Math.round(l)));
    r = Math.max(0, Math.min(pw - l, Math.round(r)));
    t = Math.max(0, Math.min(ph, Math.round(t)));
    b = Math.max(0, Math.min(ph - t, Math.round(b)));
  };
  const redraw = () => {
    gc.imageSmoothingEnabled = false;
    gc.clearRect(0, 0, canvas.width, canvas.height);
    for (let y = 0; y < canvas.height; y += 8) for (let x = 0; x < canvas.width; x += 8) { gc.fillStyle = ((x + y) / 8) % 2 ? '#2a2f37' : '#343a44'; gc.fillRect(x, y, 8, 8); }
    gc.drawImage(img, view[0], view[1], view[2], view[3], 0, 0, canvas.width, canvas.height);
    const line = (x1, y1, x2, y2) => { gc.beginPath(); gc.moveTo(Math.round(x1) + 0.5, Math.round(y1) + 0.5); gc.lineTo(Math.round(x2) + 0.5, Math.round(y2) + 0.5); gc.stroke(); };
    if (picking) {
      if (part) {
        gc.fillStyle = 'rgba(0, 0, 0, .45)';
        gc.fillRect(0, 0, canvas.width, canvas.height);
        gc.drawImage(img, part[0], part[1], part[2], part[3], part[0] * zoom, part[1] * zoom, part[2] * zoom, part[3] * zoom);
        gc.strokeStyle = '#60a5fa';
        gc.lineWidth = 1;
        gc.strokeRect(part[0] * zoom + 0.5, part[1] * zoom + 0.5, part[2] * zoom - 1, part[3] * zoom - 1);
      }
    } else {
      const [, , pw, ph] = part || [0, 0, img.width, img.height];
      gc.strokeStyle = '#4ade80';
      gc.lineWidth = 1;
      gc.setLineDash([4, 3]);
      line(l * zoom, 0, l * zoom, canvas.height);
      line((pw - r) * zoom, 0, (pw - r) * zoom, canvas.height);
      line(0, t * zoom, canvas.width, t * zoom);
      line(0, (ph - b) * zoom, canvas.width, (ph - b) * zoom);
      gc.setLineDash([]);
    }
    for (const [k, v] of Object.entries({ l, t, r, b })) inputs[k].set(v);
    (part || [0, 0, 0, 0]).forEach((v, i) => partInputs[i].set(v));
  };
  for (const [k, label] of [['l', 'Left'], ['t', 'Top'], ['r', 'Right'], ['b', 'Bottom']]) {
    const f = scrubNumber(label, { l, t, r, b }[k], v => { if (k === 'l') l = v; else if (k === 't') t = v; else if (k === 'r') r = v; else b = v; clampAll(); redraw(); }, { min: 0 });
    inputs[k] = f;
    fields.append(f);
  }
  ['X', 'Y', 'W', 'H'].forEach((label, i) => {
    const f = scrubNumber('Part ' + label, 0, v => {
      const p = part ? part.slice() : [0, 0, img.width, img.height];
      p[i] = Math.max(0, v);
      p[2] = Math.max(1, Math.min(p[2], img.width - p[0]));
      p[3] = Math.max(1, Math.min(p[3], img.height - p[1]));
      part = p;
      clampAll();
      layout();
    }, { min: 0 });
    partInputs.push(f);
    partFields.append(f);
  });

  const at = e => {
    const box2 = canvas.getBoundingClientRect();
    return [(e.clientX - box2.left) * canvas.width / box2.width / zoom, (e.clientY - box2.top) * canvas.height / box2.height / zoom];
  };
  let dragging = null, from = null;
  canvas.onpointerdown = e => {
    const [x, y] = at(e);
    if (picking) {
      from = [Math.floor(x), Math.floor(y)];
      dragging = 'part';
      canvas.setPointerCapture(e.pointerId);
      return;
    }
    const [, , pw, ph] = part || [0, 0, img.width, img.height];
    const near = [['l', Math.abs(x - l)], ['r', Math.abs(x - (pw - r))], ['t', Math.abs(y - t)], ['b', Math.abs(y - (ph - b))]].sort((p, q) => p[1] - q[1])[0];
    if (near[1] * zoom > 8) return;
    dragging = near[0];
    canvas.setPointerCapture(e.pointerId);
  };
  canvas.onpointermove = e => {
    const [x, y] = at(e);
    if (dragging === 'part') {
      const x2 = Math.max(0, Math.min(img.width, Math.ceil(x))), y2 = Math.max(0, Math.min(img.height, Math.ceil(y)));
      const px = Math.max(0, Math.min(from[0], x2)), py = Math.max(0, Math.min(from[1], y2));
      part = [px, py, Math.max(1, Math.abs(x2 - from[0])), Math.max(1, Math.abs(y2 - from[1]))];
      redraw();
      return;
    }
    if (!dragging) {
      if (picking) { canvas.style.cursor = 'crosshair'; return; }
      const [, , pw, ph] = part || [0, 0, img.width, img.height];
      const nearX = Math.min(Math.abs(x - l), Math.abs(x - (pw - r))) * zoom <= 8, nearY = Math.min(Math.abs(y - t), Math.abs(y - (ph - b))) * zoom <= 8;
      canvas.style.cursor = nearX ? 'ew-resize' : nearY ? 'ns-resize' : 'default';
      return;
    }
    const [, , pw, ph] = part || [0, 0, img.width, img.height];
    if (dragging === 'l') l = x; else if (dragging === 'r') r = pw - x; else if (dragging === 't') t = y; else b = ph - y;
    clampAll();
    redraw();
  };
  canvas.onpointerup = () => {
    // A part picked: straight on to its slices.
    if (dragging === 'part' && part) { picking = false; clampAll(); layout(); }
    dragging = null;
  };
  pickButton.onclick = () => { picking = !picking; layout(); };
  wholeButton.onclick = () => { part = null; picking = false; clampAll(); layout(); };

  const buttons = document.createElement('div');
  buttons.className = 'slice-buttons';
  const clear = document.createElement('button');
  clear.textContent = 'No slices';
  clear.onclick = () => { l = t = r = b = 0; redraw(); };
  const apply = document.createElement('button');
  apply.className = 'primary';
  apply.textContent = 'Apply';
  const hint = document.createElement('p');
  hint.className = 'none';
  hint.textContent = 'Drag a green line, or type. The borders keep their size as the frame grows; the middle and the edges stretch (or tile, with Slice type: tiled). Pick part chooses one sprite of a sheet.';
  buttons.append(hint, clear, apply);
  box.append(head, stage, partFields, fields, buttons);
  veil.append(box);
  document.body.append(veil);
  const close = () => { veil.remove(); document.removeEventListener('keydown', key, true); };
  const key = e => { if (e.key === 'Escape') { e.preventDefault(); close(); } };
  document.addEventListener('keydown', key, true);
  shut.onclick = close;
  veil.onclick = e => { if (e.target === veil) close(); };
  apply.onclick = () => { close(); onDone([l, t, r, b], part && !(part[0] === 0 && part[1] === 0 && part[2] === img.width && part[3] === img.height) ? part : null); };
  layout();
}

// ------------------------------------------------------------------ the inspector's Background

/// Where a property of a frame's (or the screen's) style comes from: its own style, a sheet's rule, or nowhere.
function propertySource(element, prop) {
  if (frameStyle(element).has(prop)) return { own: true, text: element.tagName === 'frame' ? 'this frame\'s own style' : 'the screen\'s own style' };
  const rules = element.tagName === 'frame' ? frameRules(element) : screenRules(element);
  for (let i = rules.length - 1; i >= 0; i--) {
    if (rules[i].declarations.some(([k]) => k === prop)) return { own: false, text: `${rules[i].sheet} ${rules[i].selector}` };
  }
  return null;
}

/// The sheet rules that reach a screen (menu, #name), in cascade order.
function screenRules(screen) {
  const styled = menuStyled(screen);
  return matchingRules(styled.rules, styled.copy).map(({ rule, specificity }) => ({
    selector: rule.selectors.filter(s => selectorMatches(s, styled.copy)).map(s => s.text).join(', '),
    sheet: rule.sheet, declarations: rule.declarations, specificity
  }));
}

// ------------------------------------------------------------------ the inspector's style fields, shared

/// A property's value on a frame (or the screen): its own style's, else what the cascade gives it.
function styledValue(element, prop) {
  const own = frameStyle(element);
  if (own.has(prop)) return own.get(prop);
  const isScreen = element.tagName !== 'frame';
  const styled = menuStyled(isScreen ? element : frameScreen(element));
  const computed = isScreen ? styled.screenValues : styled.computed.get(element);
  return computed ? computed.get(prop) : undefined;
}

/// What a sheet's rule gives a property of a frame (or the screen), for "back to ..."; null for none.
function sheetValueText(element, prop) {
  const rules = element.tagName === 'frame' ? frameRules(element) : screenRules(element);
  for (let i = rules.length - 1; i >= 0; i--) {
    const d = rules[i].declarations.find(([k]) => k === prop);
    if (d) return `${d[1]} (${rules[i].sheet})`;
  }
  return null;
}

/// Rows whose label says where the value comes from - the frame's own style, a sheet's rule, or nowhere;
/// set on the frame, a click on the label takes it back off. row(label, control, tip, props) as propRow.
function sourcedRows(body, element, setNow, sync) {
  const isScreen = element.tagName !== 'frame';
  return (label, control, tip, props) => {
    const r = propRow(body, label, control, tip);
    if (!props) return r;
    const name = r.querySelector('.prop-label');
    const show = () => {
      const from = props.map(p => propertySource(element, p)).find(Boolean);
      const own = props.some(p => frameStyle(element).has(p));
      name.classList.toggle('own', own);
      name.classList.toggle('sheet', !own && !!from);
      name.title = own ? `Set on ${isScreen ? 'the screen' : 'this frame'} - click to take it off (back to ${props.map(p => sheetValueText(element, p)).find(Boolean) || 'nothing'})` : from ? `From ${from.text}` : 'Not set';
    };
    name.onclick = () => {
      if (!props.some(p => frameStyle(element).has(p))) return;
      setNow('revert ' + label.toLowerCase(), Object.fromEntries(props.map(p => [p, null])));
    };
    show();
    sync.push(show);
    return r;
  };
}

function hexOf(c) {
  return '#' + c.slice(0, 3).map(n => n.toString(16).padStart(2, '0')).join('');
}

/// A colour with its alpha: a colour well and an α scrub (%). onChange hears '#rrggbb' or '#rrggbbaa'; field.set(text) shows one.
function colourAlphaField(onChange, tip) {
  const wrap = document.createElement('div');
  wrap.className = 'bg-colour colour-field';
  const input = document.createElement('input');
  input.type = 'color';
  if (tip) input.title = tip;
  let alphaNow = 100;
  const write = () => {
    const aa = Math.round(Math.max(0, Math.min(100, alphaNow)) * 2.55).toString(16).padStart(2, '0');
    onChange(input.value + (aa === 'ff' ? '' : aa));
  };
  const alpha = scrubNumber('α', 100, v => { alphaNow = v; write(); }, { min: 0, max: 100, title: 'The colour\'s opacity, %' });
  input.oninput = write;
  wrap.append(input, alpha);
  wrap.set = text => {
    const c = bgTextColour(text);
    input.value = c ? hexOf(c) : '#000000';
    alphaNow = c ? Math.round(c[3] / 2.55) : 100;
    alpha.set(alphaNow);
  };
  wrap.disable = off => { input.disabled = !!off; alpha.disable(off); };
  return wrap;
}

/// A list edited in place (shadows, stops, transitions): a card an item, its fields and a ×, and + Add.
///   read()          -> { text, items }: the value now, and its items
///   write(items)    -> the text it wrote
///   fresh(items)    -> a new item
///   fields          -> [(item, put, i) => control]: put(key, value) changes the item and writes the list
/// list.sync() shows the value anew when it changed other than by the list itself.
function styleListEditor({ read, write, fresh, fields, addLabel, empty }) {
  const wrap = document.createElement('div');
  wrap.className = 'style-list';
  let now = read();
  let items = now.items;
  let written = now.text;
  const put = () => { written = write(items); };
  const render = () => {
    wrap.textContent = '';
    items.forEach((item, i) => {
      const card = document.createElement('div');
      card.className = 'style-list-item';
      for (const make of fields) card.append(make(item, (key, value) => { item[key] = value; put(); }, i));
      const remove = document.createElement('button');
      remove.className = 'mini';
      remove.textContent = '×';
      remove.title = 'Take this one out';
      remove.onclick = () => { items.splice(i, 1); put(); render(); };
      card.append(remove);
      wrap.append(card);
    });
    if (!items.length && empty) {
      const none = document.createElement('p');
      none.className = 'none';
      none.textContent = empty;
      wrap.append(none);
    }
    const add = document.createElement('button');
    add.className = 'link style-list-add';
    add.textContent = addLabel || '+ Add';
    add.onclick = () => { items.push(fresh(items)); put(); render(); };
    wrap.append(add);
  };
  wrap.sync = () => {
    now = read();
    if (now.text === written) return;
    written = now.text;
    items = now.items;
    render();
  };
  render();
  return wrap;
}

// Field makers for a list's cards.
const listScrub = (label, key, options) => (item, put) => { const f = scrubNumber(label, item[key], v => put(key, v), options); f.classList.add('list-field'); return f; };
const listColour = (key, tip) => (item, put) => { const f = colourAlphaField(v => put(key, v), tip); f.set(item[key]); f.classList.add('list-field', 'wide'); return f; };
const listSelect = (key, options, tip) => (item, put) => { const s = propSelect(options, item[key], v => put(key, v)); s.title = tip || ''; s.classList.add('list-field'); return s; };
const listText = (key, placeholder, tip, datalist) => (item, put) => {
  const t = propText(item[key], v => put(key, v.trim()), placeholder);
  t.title = tip || '';
  t.classList.add('list-field');
  if (datalist) t.setAttribute('list', datalist);
  return t;
};
const listCheck = (label, key, tip) => (item, put) => {
  const l = document.createElement('label');
  l.className = 'list-field list-check';
  l.title = tip || '';
  const c = document.createElement('input');
  c.type = 'checkbox';
  c.checked = !!item[key];
  c.onchange = () => put(key, c.checked);
  l.append(c, label);
  return l;
};

/// A datalist of the page's, filled anew (for a text field's suggestions).
function styleDatalist(id, values) {
  let list = document.getElementById(id);
  if (!list) { list = document.createElement('datalist'); list.id = id; document.body.append(list); }
  list.textContent = '';
  for (const v of values) { const o = document.createElement('option'); o.value = v; list.append(o); }
  return id;
}

const cssNumber = n => String(Math.round(n * 1000) / 1000);
const cssPx = n => n === 0 ? '0' : `${cssNumber(n)}px`;
const colourText = c => animColourText(c);

// ------------------------------------------------------------------ the gradient's editor

/// A gradient as the editor keeps it, from background-image's value: the kind, its direction or shape and centre, its stops as written.
function gradientModel(value) {
  const g = parseGradient(value) || parseGradient('linear-gradient(to bottom, #2a4a8a, #0e1a36)');
  const angle = g.cornerX && g.cornerY ? `to ${g.cornerY < 0 ? 'top' : 'bottom'} ${g.cornerX < 0 ? 'left' : 'right'}`
    : { 0: 'to top', 90: 'to right', 180: 'to bottom', 270: 'to left' }[g.angle] || `${cssNumber(g.angle)}deg`;
  return {
    kind: g.kind, repeating: g.repeating, direction: angle,
    shape: g.circle ? 'circle' : 'ellipse', size: g.sizeWord || [g.sizeX, g.circle ? null : g.sizeY].filter(Boolean).join(' '),
    at: `${g.atX} ${g.atY}`, from: g.from,
    stops: g.stops.map(s => ({ colour: colourText(s.colour), position: s.position || '' }))
  };
}

function gradientText(m) {
  const head = [];
  if (m.kind === 'linear') { if (m.direction && m.direction !== 'to bottom') head.push(m.direction); }
  else {
    if (m.kind === 'radial') {
      if (m.shape === 'circle') head.push('circle');
      if (m.size && m.size !== 'farthest-corner') head.push(m.size);
    } else if (m.from) head.push(`from ${cssNumber(m.from)}deg`);
    const at = (m.at || '').trim();
    if (at && at !== '50% 50%' && at !== 'center') head.push(`at ${at}`);
  }
  const stops = m.stops.map(s => `${s.colour}${s.position ? ' ' + s.position : ''}`);
  return `${m.repeating ? 'repeating-' : ''}${m.kind}-gradient(${[head.join(' '), ...stops].filter(Boolean).join(', ')})`;
}

/// The Background's gradient: its kind, repeating, direction (linear), shape, size and centre (radial), start
/// and centre (conic), and its stops - a colour and a place each - with a strip showing it on the frame's shape.
function gradientEditor(element, set, sync, size) {
  const wrap = document.createElement('div');
  wrap.className = 'gradient-editor';
  const value = () => styledValue(element, 'background-image') || '';
  let model = gradientModel(value());
  let written = gradientText(model);
  const write = what => { written = gradientText(model); set(what || 'change gradient', { 'background-image': written }); paintStrip(); };
  const body = document.createElement('div');
  body.className = 'prop-body';
  wrap.append(body);
  const strip = document.createElement('canvas');
  strip.className = 'gradient-strip';
  const paintStrip = () => {
    const [w, h] = size();
    const sw = 220, sh = Math.max(16, Math.min(80, Math.round(sw * h / Math.max(1, w))));
    strip.width = sw * 2;
    strip.height = sh * 2;
    strip.style.height = `${sh}px`;
    const gc = strip.getContext('2d');
    gc.clearRect(0, 0, strip.width, strip.height);
    const g = parseGradient(written);
    if (!g) return;
    // The frame's own shape, scaled down: a gradient's lengths are the frame's.
    gc.setTransform(strip.width / w, 0, 0, strip.height / h, 0, 0);
    bgPaintGradient(gc, g, w, h);
  };
  const build = () => {
    body.textContent = '';
    const kind = propSelect([['linear', 'linear'], ['radial', 'radial'], ['conic', 'conic']], model.kind, v => { model.kind = v; write('change gradient kind'); build(); });
    const repeat = document.createElement('label');
    repeat.className = 'list-check';
    const r = document.createElement('input');
    r.type = 'checkbox';
    r.checked = model.repeating;
    r.onchange = () => { model.repeating = r.checked; write('change gradient repeat'); };
    repeat.append(r, 'repeating');
    const head = document.createElement('div');
    head.className = 'style-pair';
    head.append(kind, repeat);
    propRow(body, 'Gradient', head, 'linear-gradient(), radial-gradient() or conic-gradient(), and repeating-: the stops over and over');
    if (model.kind === 'linear') {
      const words = ['to top', 'to right', 'to bottom', 'to left', 'to top right', 'to bottom right', 'to bottom left', 'to top left'];
      const isAngle = !words.includes(model.direction);
      const dir = propSelect([...words.map(w => [w, w]), ['angle', 'an angle…']], isAngle ? 'angle' : model.direction, v => {
        model.direction = v === 'angle' ? '135deg' : v;
        write('change gradient direction');
        build();
      });
      const row = document.createElement('div');
      row.className = 'style-pair';
      row.append(dir);
      if (isAngle) row.append(scrubNumber('°', bgDegrees(model.direction) ?? 180, v => { model.direction = `${v}deg`; write('change gradient angle'); }, { title: 'The direction: 0deg up, 90deg right (CSS\'s)' }));
      propRow(body, 'Direction', row, 'Where it goes: to a side or a corner (at right angles to the diagonal), or at an angle');
    } else {
      if (model.kind === 'radial') {
        const shape = propSelect([['ellipse', 'ellipse'], ['circle', 'circle']], model.shape, v => { model.shape = v; write('change gradient shape'); });
        const sized = propText(model.size === 'farthest-corner' ? '' : model.size, v => { model.size = v.trim() || 'farthest-corner'; write('change gradient size'); }, 'farthest-corner');
        sized.setAttribute('list', styleDatalist('gradient-sizes', ['closest-side', 'farthest-side', 'closest-corner', 'farthest-corner', '40px', '60% 40%']));
        sized.title = 'closest-side, farthest-side, closest-corner, farthest-corner, or a radius (40px) - two for an ellipse (60% 40%)';
        const row = document.createElement('div');
        row.className = 'style-pair';
        row.append(shape, sized);
        propRow(body, 'Shape', row, 'Its shape and how far it reaches');
      } else {
        propRow(body, 'From', scrubNumber('°', model.from, v => { model.from = v; write('change gradient start'); }, { title: 'The angle it starts from: 0deg up, round clockwise' }), 'from: where the turn starts');
      }
      const at = propText(model.at === '50% 50%' ? '' : model.at, v => { model.at = v.trim() || '50% 50%'; write('change gradient centre'); }, 'center (50% 50%)');
      at.title = 'at x y: left, center, right, top, bottom, px or %';
      propRow(body, 'At', at, 'Its centre on the frame');
    }
    const stops = styleListEditor({
      read: () => ({ text: written, items: model.stops }),
      write: items => { model.stops = items; write('change gradient stops'); return written; },
      fresh: items => ({ colour: items.length ? items[items.length - 1].colour : '#ffffff', position: '' }),
      fields: [listColour('colour', 'The stop\'s colour'), listText('position', 'auto', 'Where it is: 30%, 6px (or 90deg round a conic); empty, spread between its neighbours - two (0 6px) for a band')],
      addLabel: '+ Stop'
    });
    propRow(body, 'Stops', stops, 'The colours along it, in order');
    body.append(strip);
    paintStrip();
  };
  build();
  wrap.sync = () => {
    if ((styledValue(element, 'background-image') || '') === written) return;
    if (!parseGradient(value())) return;
    model = gradientModel(value());
    written = gradientText(model);
    build();
  };
  sync.push(wrap.sync);
  return wrap;
}

// ------------------------------------------------------------------ the inspector's Box

/// The box round a frame (MenuBackground, MenuPaint): its border - widths, colour, style -, its round corners, and its shadows.
function buildBoxSection(panel, element, screen, edit, rebuild, sync) {
  const value = prop => styledValue(element, prop);
  const box = () => parseBackground(BACKGROUND_PROPS.filter(p => value(p) !== undefined).map(p => `${p}: ${value(p)}`).join('; ') + '; background-color: #000');
  const has = ['border', 'border-width', 'border-color', 'border-style', 'border-radius', 'box-shadow', 'border-top', 'border-right', 'border-bottom', 'border-left'].some(p => value(p) !== undefined);
  const body = propSection(panel, 'Box', 'part', 'The box round the frame, as CSS draws one: a border, round corners and shadows - painted over and under its panel (Crystal Style Sheets)', has);
  const set = (what, changes) => edit(what, () => setFrameStyle(element, changes));
  const setNow = (what, changes) => rebuild(what, () => setFrameStyle(element, changes));
  const row = sourcedRows(body, element, setNow, sync);
  const sides = ['border-top', 'border-right', 'border-bottom', 'border-left'];
  const sideLonghands = ['width', 'color'].flatMap(k => sides.map(s => `${s}-${k}`));

  // ---- the border: written as border-width, -color and -style, the shorthands of its own taken off
  const writeBorder = (what, change) => {
    const b = box();
    const widths = b.borderWidth.slice(), colours = b.borderColour.map(c => colourText(c));
    let style = (value('border-style') || 'solid').trim().toLowerCase();
    if (b.hasBorder === false && style === 'none') style = 'solid';
    const next = change({ widths, colours, style });
    const four = a => a.every(x => x === a[0]) ? String(a[0]) : a.join(' ');
    set(what, {
      border: null, ...Object.fromEntries([...sides, ...sideLonghands].map(p => [p, null])),
      'border-width': next.widths.every(w => w === 0) ? null : four(next.widths.map(cssPx)),
      'border-color': four(next.colours),
      'border-style': next.style === 'solid' ? null : next.style
    });
  };
  const widthGrid = document.createElement('div');
  widthGrid.className = 'prop-grid four';
  ['T', 'R', 'B', 'L'].forEach((label, i) => {
    const f = scrubNumber(label, box().borderWidth[i], v => writeBorder('change border width', s => { s.widths[i] = Math.max(0, v); if (s.style === 'none') s.style = 'solid'; return s; }), { min: 0, step: 0.5, title: ['top', 'right', 'bottom', 'left'][i] + ': the border\'s width, menu units' });
    sync.push(() => f.set(box().borderWidth[i]));
    widthGrid.append(f);
  });
  const allWidth = scrubNumber('All', Math.max(...box().borderWidth), v => { writeBorder('change border width', s => { s.widths = [v, v, v, v].map(x => Math.max(0, x)); if (s.style === 'none') s.style = 'solid'; return s; }); }, { min: 0, step: 0.5, title: 'Every side at once' });
  sync.push(() => allWidth.set(Math.max(...box().borderWidth)));
  const widthRow = document.createElement('div');
  widthRow.className = 'box-width-row';
  widthRow.append(allWidth, widthGrid);
  row('Border', widthRow, 'border-width: one to four (top, right, bottom, left)', ['border', 'border-width', ...sides, ...sides.map(s => s + '-width')]);
  const colour = colourAlphaField(v => writeBorder('change border colour', s => { s.colours = [v, v, v, v]; return s; }), 'border-color');
  const showColour = () => colour.set(colourText(box().borderColour[0]));
  showColour();
  sync.push(showColour);
  row('Border Colour', colour, 'border-color (per side in a sheet: border-color with four, or border-top-color and the rest)', ['border-color', ...sides.map(s => s + '-color')]);
  const style = propSelect([['solid', 'solid'], ['none', 'none (no border)']], (value('border-style') || 'solid').trim().toLowerCase() === 'none' || (value('border-style') || '').trim().toLowerCase() === 'hidden' ? 'none' : 'solid',
    v => { writeBorder('change border style', s => { s.style = v; return s; }); });
  row('Border Style', style, 'border-style: solid is drawn (dashes and the rest are drawn solid); none takes the border away', ['border-style']);

  // ---- the corners: all, or one by one; px or a percent of the shorter side
  const corners = ['border-top-left-radius', 'border-top-right-radius', 'border-bottom-right-radius', 'border-bottom-left-radius'];
  const percent = () => box().radiusPercent.some(Boolean);
  const writeRadius = (what, radii, asPercent) => {
    const words = radii.map(r => asPercent ? `${cssNumber(r)}%` : cssPx(r));
    set(what, { ...Object.fromEntries(corners.map(c => [c, null])), 'border-radius': radii.every(r => r === 0) ? null : words.every(w => w === words[0]) ? words[0] : words.join(' ') });
  };
  const cornerGrid = document.createElement('div');
  cornerGrid.className = 'prop-grid four';
  const cornerFields = ['TL', 'TR', 'BR', 'BL'].map((label, i) => {
    const f = scrubNumber(label, box().radius[i], v => { const r = box().radius.slice(); r[i] = Math.max(0, v); writeRadius('change corner', r, percent()); }, { min: 0, title: ['top left', 'top right', 'bottom right', 'bottom left'][i] + ' corner\'s radius' });
    cornerGrid.append(f);
    return f;
  });
  const allRadius = scrubNumber('All', Math.max(...box().radius), v => writeRadius('change corners', [v, v, v, v].map(x => Math.max(0, x)), percent()), { min: 0, title: 'Every corner at once' });
  const unit = propSelect([['px', 'px'], ['%', '%']], percent() ? '%' : 'px', u => {
    // As big as they are now, in the other unit (a percent is of the shorter side, as the client takes it).
    const [w, h] = frameSizeOf(element);
    const shorter = Math.max(1, Math.min(w, h));
    const b = box();
    const now = b.radius.map((r, i) => b.radiusPercent[i] ? shorter * r / 100 : r);
    writeRadius('change corner unit', u === '%' ? now.map(r => Math.round(r / shorter * 1000) / 10) : now.map(Math.round), u === '%');
  });
  unit.className = 'rule-unit';
  sync.push(() => { const b = box(); cornerFields.forEach((f, i) => f.set(b.radius[i])); allRadius.set(Math.max(...b.radius)); unit.value = percent() ? '%' : 'px'; });
  const radiusRow = document.createElement('div');
  radiusRow.className = 'box-width-row';
  radiusRow.append(allRadius, unit, cornerGrid);
  row('Corners', radiusRow, 'border-radius: one for all, or top-left, top-right, bottom-right, bottom-left - px, or % of the shorter side', ['border-radius', ...corners]);

  // ---- the shadows: outer ones behind the frame (behind the game's window too), inset ones inside it
  const shadowList = styleListEditor({
    read: () => {
      const text = value('box-shadow') || '';
      return { text, items: parseBoxShadows(text).map(s => ({ inset: s.inset, x: s.x, y: s.y, blur: s.blur, spread: s.spread, colour: colourText(s.colour) })) };
    },
    write: items => {
      const text = items.map(s => `${s.inset ? 'inset ' : ''}${cssPx(s.x)} ${cssPx(s.y)} ${cssPx(s.blur)}${s.spread ? ' ' + cssPx(s.spread) : ''} ${s.colour}`).join(', ');
      set('change box shadow', { 'box-shadow': text || (sheetValueText(element, 'box-shadow') ? 'none' : null) });
      return text || (sheetValueText(element, 'box-shadow') ? 'none' : '');
    },
    fresh: () => ({ inset: false, x: 0, y: 4, blur: 10, spread: 0, colour: '#000000c0' }),
    fields: [listCheck('inset', 'inset', 'Inside the frame rather than behind it'), listScrub('X', 'x', { step: 1 }), listScrub('Y', 'y', { step: 1 }), listScrub('Blur', 'blur', { min: 0 }), listScrub('Spread', 'spread', {}), listColour('colour', 'The shadow\'s colour')],
    addLabel: '+ Shadow',
    empty: 'No shadow.'
  });
  sync.push(shadowList.sync);
  row('Shadows', shadowList, 'box-shadow: the first on top; an outer one behind the frame and its window, an inset one inside it', ['box-shadow']);
}

/// A frame's size as its layout rules make it now (the screen's for the screen).
function frameSizeOf(element) {
  if (element.tagName !== 'frame') return [480, 320];
  const found = collectFrames(frameScreen(element)).find(f => f.element === element);
  return found ? [Math.max(1, found.width), Math.max(1, found.height)] : [100, 40];
}

/// The inspector's Background section, as UI Toolkit's: colour, picture (and its sprite), tint, part, scale
/// mode, size, position, repeat, the slices and the filter - each showing where it comes from, and what is
/// drawn when no picture is: the game's window or bar, built in.
function buildBackgroundSection(panel, element, screen, edit, rebuild, sync, options = {}) {
  const isScreen = element.tagName !== 'frame';
  const styledNow = () => menuStyled(isScreen ? element : frameScreen(element));
  const computed = () => (isScreen ? styledNow().screenValues : styledNow().computed.get(element)) || new Map();
  const value = prop => { const own = frameStyle(element); return own.has(prop) ? own.get(prop) : computed().get(prop); };
  const has = BACKGROUND_PROPS.some(p => value(p) !== undefined);
  const body = propSection(panel, options.title || 'Background', 'image', 'What is drawn behind the frame: a colour and a picture - stretched, cropped, fitted, repeated or 9-sliced - or a sprite of a sheet (Crystal Style Sheets, the frame\'s own style)', has || options.open);
  const set = (what, changes) => edit(what, () => setFrameStyle(element, changes));
  const setNow = (what, changes) => rebuild(what, () => setFrameStyle(element, changes));
  const rectValue = () => { const r = String(value('-ff-background-rect') || '').split(/[\s,]+/).filter(Boolean).map(n => parseInt(n, 10)); return r.length === 4 ? r : null; };

  // A row's label says where its value comes from; set on the frame, a click on it takes it back off.
  const row = sourcedRows(body, element, setNow, sync);
  const computedFromSheets = prop => sheetValueText(element, prop);

  // ---- colour, with its alpha
  const colourRow = document.createElement('div');
  colourRow.className = 'bg-colour';
  const colour = document.createElement('input');
  colour.type = 'color';
  const alphaOf = () => { const c = bgColour(value('background-color')); return c ? Math.round(c[3] / 2.55) : 100; };
  const writeColour = a => {
    const aa = Math.round(Math.max(0, Math.min(100, a ?? alphaOf())) * 2.55).toString(16).padStart(2, '0');
    set('change background colour', { 'background-color': colour.value + (aa === 'ff' ? '' : aa) });
  };
  const alpha = scrubNumber('α', 100, v => writeColour(v), { min: 0, max: 100, title: 'The colour\'s opacity, %' });
  colour.oninput = () => writeColour();
  const noColour = document.createElement('button');
  noColour.className = 'mini';
  noColour.textContent = '×';
  colourRow.append(colour, alpha, noColour);
  row('Color', colourRow, 'background-color: under the picture (and alone, a plain panel)', ['background-color']);
  const showColour = () => {
    const c = bgColour(value('background-color'));
    colour.value = c ? '#' + c.slice(0, 3).map(n => n.toString(16).padStart(2, '0')).join('') : '#000000';
    alpha.set(c ? Math.round(c[3] / 2.55) : 100);
    colourRow.classList.toggle('unset', !c);
    const own = frameStyle(element).has('background-color'), sheet = !own && c;
    noColour.hidden = !c;
    noColour.title = own ? 'Take the colour off this frame' : sheet ? 'No colour here (over the sheet\'s)' : '';
  };
  noColour.onclick = () => setNow('clear background colour', { 'background-color': frameStyle(element).has('background-color') ? null : 'transparent' });
  showColour();
  sync.push(showColour);

  // ---- a picture or a gradient: what background-image is (MenuGradient) - the picture's rows, or the gradient's editor
  const isGradient = () => !!parseGradient(value('background-image') || '');
  const fillMode = document.createElement('div');
  fillMode.className = 'segmented';
  const modeButtons = [['picture', 'Picture', 'background-image: url(...) a picture beside the screen, resource(...) one of the game\'s'], ['gradient', 'Gradient', 'background-image: a linear, radial or conic gradient']].map(([v, label, tip]) => {
    const b = document.createElement('button');
    b.textContent = label;
    b.title = tip;
    b.onclick = () => {
      if (v === 'gradient' && !isGradient()) setNow('make a gradient', {
        'background-image': 'linear-gradient(to bottom, #2a4a8a, #0e1a36)', '-ff-sprite': null,
        // A gradient of its own in place of the game's window: the window goes, as for a picture.
        ...(!isScreen && hasTag(frameLook(element), 'window') && !frameStyle(element).has('-ff-panel') ? { '-ff-panel': 'none' } : {})
      });
      else if (v === 'picture' && isGradient()) setNow('no gradient', { 'background-image': frameStyle(element).has('background-image') && !computedFromSheets('background-image') ? null : 'none' });
    };
    fillMode.append(b);
    return [v, b];
  });
  row('Fill', fillMode, 'What background-image is: a picture, or a gradient', ['background-image']);
  const gradientBox = gradientEditor(element, set, sync, () => isScreen ? [480, 320] : frameSizeOf(element));
  body.append(gradientBox);

  // ---- the picture: what is drawn, and from where - a picture, a sprite of one, or the game's window built in
  const imageRow = document.createElement('div');
  imageRow.className = 'bg-image';
  const thumb = document.createElement('button');
  thumb.className = 'bg-thumb';
  const name = document.createElement('span');
  name.className = 'bg-image-name';
  const clearImage = document.createElement('button');
  clearImage.className = 'mini';
  clearImage.textContent = '×';
  imageRow.append(thumb, name, clearImage);
  row('Image', imageRow, 'background-image: url(...) a picture beside the screen, resource(...) one of the game\'s', ['background-image', '-ff-sprite']);
  const chooser = () => pickBackgroundPicture(value('background-image') || '', (chosen, sprite) => setNow('change background picture', {
    'background-image': chosen, '-ff-sprite': sprite || null,
    // A picture of its own in place of the game's window: the window goes.
    ...(chosen && !isScreen && hasTag(frameLook(element), 'window') && !frameStyle(element).has('-ff-panel') ? { '-ff-panel': 'none' } : {})
  }));
  thumb.onclick = chooser;
  name.onclick = chooser;
  const builtIn = document.createElement('p');
  builtIn.className = 'none bg-builtin';
  body.append(builtIn);
  const showImage = () => {
    const image = bgImage(value('background-image'));
    const sprite = value('-ff-sprite');
    const from = propertySource(element, 'background-image');
    thumb.textContent = '';
    builtIn.textContent = '';
    builtIn.hidden = true;
    if (image) {
      const sp = sprite ? findSprite(image, sprite) : null;
      loadBackgroundPicture(image).then(img => {
        thumb.textContent = '';
        if (!img) { thumb.textContent = '?'; return; }
        const part = sp || (() => { const r = rectValue(); return r ? { x: r[0], y: r[1], w: r[2], h: r[3] } : { x: 0, y: 0, w: img.width, h: img.height }; })();
        thumb.append(spriteThumbnail(img, part, 30));
      });
      name.textContent = `${image.path.split('/').pop()}${sprite ? ' › ' + sprite + (sp ? '' : ' (missing)') : ''}  · ${image.kind === 'resource' ? 'the game\'s' : 'beside the screen'}`;
      name.title = `${image.kind}("${image.path}")${sprite ? `, sprite ${sprite}` : ''}\nfrom ${from ? from.text : '?'}`;
      clearImage.hidden = false;
      clearImage.title = from && from.own ? 'Take the picture off this frame' + (computedFromSheets('background-image') ? ' (back to the sheet\'s)' : '') : 'No picture here (over the sheet\'s)';
    } else {
      thumb.textContent = 'None';
      name.textContent = 'Choose…';
      name.title = 'Choose a picture, or a sprite of a sheet';
      clearImage.hidden = !frameStyle(element).has('background-image');
      clearImage.title = 'Take "none" off this frame';
      // Not a picture - the game's own window, or its bar, drawn instead.
      if (!isScreen) {
        const look = frameLook(element);
        if (hasTag(look, 'window')) {
          const bar = (childText(look, 'panel') || '').trim() === 'bar';
          const panelFrom = propertySource(element, '-ff-panel');
          const layout = [...element.children].some(e => e.tagName === 'window');
          builtIn.hidden = false;
          builtIn.textContent = '';
          builtIn.append(`Drawn: the game's ${bar ? 'translucent bar' : 'window'} (built in - m000_window), from ${panelFrom ? panelFrom.text : layout ? 'the layout\'s <window/>' : 'the layout'}. `);
          const off = document.createElement('button');
          off.className = 'link';
          off.textContent = 'No window';
          off.title = 'Nothing drawn behind the frame (-ff-panel: none)';
          off.onclick = () => setNow('no window', { '-ff-panel': 'none' });
          const instead = document.createElement('button');
          instead.className = 'link';
          instead.textContent = 'A picture instead…';
          instead.onclick = chooser;
          builtIn.append(off, ' · ', instead);
        }
      }
    }
  };
  clearImage.onclick = () => {
    const own = frameStyle(element).has('background-image');
    setNow('clear background picture', own ? { 'background-image': null, '-ff-sprite': null } : { 'background-image': 'none', '-ff-sprite': null });
  };
  showImage();
  sync.push(showImage);

  // ---- the sprite of the sheet
  const spriteRow = document.createElement('div');
  spriteRow.className = 'bg-slice-row';
  const spritePick = document.createElement('select');
  const editSpritesButton = document.createElement('button');
  editSpritesButton.textContent = 'Sprites…';
  editSpritesButton.title = 'The Sprite editor: the sheet\'s named sprites and their borders (sprites.json)';
  spriteRow.append(spritePick, editSpritesButton);
  row('Sprite', spriteRow, '-ff-sprite: a named sprite of the sheet - its part and its borders (a Part or Slice set here wins over them)', ['-ff-sprite']);
  const showSprites = () => {
    const image = bgImage(value('background-image'));
    spritePick.textContent = '';
    const add = (v, t) => { const o = document.createElement('option'); o.value = v; o.textContent = t; spritePick.append(o); };
    add('', image ? 'the whole picture' : '(no picture)');
    for (const sp of spritesOf(image)) add(sp.name, `${sp.name}  ${sp.w}×${sp.h}${sp.left || sp.top || sp.right || sp.bottom ? ' sliced' : ''}`);
    const cur = value('-ff-sprite') || '';
    if (cur && !findSprite(image, cur)) add(cur, `${cur} (not on the sheet)`);
    spritePick.value = cur;
    spritePick.disabled = !image;
    editSpritesButton.disabled = !image || !menu.project;
  };
  spritePick.onchange = () => setNow('change sprite', { '-ff-sprite': spritePick.value || null });
  editSpritesButton.onclick = () => {
    const image = bgImage(value('background-image'));
    if (image) editSprites(image, value('-ff-sprite'), n => setNow('use sprite', { '-ff-sprite': n }));
  };
  showSprites();
  sync.push(showSprites);

  const tint = document.createElement('input');
  tint.type = 'color';
  tint.oninput = () => set('change image tint', { '-ff-background-tint': tint.value === '#ffffff' ? null : tint.value });
  const showTint = () => { const c = bgColour(value('-ff-background-tint')); tint.value = c ? '#' + c.slice(0, 3).map(n => n.toString(16).padStart(2, '0')).join('') : '#ffffff'; };
  showTint();
  sync.push(showTint);
  row('Image Tint', tint, '-ff-background-tint: the picture multiplied by it', ['-ff-background-tint']);

  const rectGrid = document.createElement('div');
  rectGrid.className = 'prop-grid four';
  ['X', 'Y', 'W', 'H'].forEach((label, i) => {
    const f = scrubNumber(label, 0, v => {
      const r = rectValue() || [0, 0, 0, 0];
      r[i] = Math.max(0, v);
      set('change picture part', { '-ff-background-rect': r[2] > 0 && r[3] > 0 ? r.join(' ') : null });
    }, { min: 0, title: '-ff-background-rect: the part of the picture used, in its pixels (0 width: the sprite\'s, or all of it)' });
    sync.push(() => f.set((rectValue() || [0, 0, 0, 0])[i]));
    f.set((rectValue() || [0, 0, 0, 0])[i]);
    rectGrid.append(f);
  });
  row('Part', rectGrid, 'A part of the picture of this frame\'s own - over the sprite\'s; the sprite, or all of it, when W or H is 0', ['-ff-background-rect']);

  const segmented = (options2, current, onPick) => {
    const wrap = document.createElement('div');
    wrap.className = 'segmented';
    const buttons = options2.map(([v, label, tip]) => {
      const b = document.createElement('button');
      b.textContent = label;
      b.title = tip;
      b.onclick = () => onPick(v);
      wrap.append(b);
      return [v, b];
    });
    wrap.mark = cur => buttons.forEach(([v, b]) => b.classList.toggle('on', v === cur));
    wrap.mark(current);
    return wrap;
  };
  const scale = segmented([['stretch-to-fill', 'Stretch', 'stretch-to-fill: the picture pulled to the frame'], ['scale-and-crop', 'Crop', 'scale-and-crop: as big as it must be to cover, the rest cut'], ['scale-to-fit', 'Fit', 'scale-to-fit: as big as fits, the aspect kept']],
    value('-ff-background-scale-mode') || 'stretch-to-fill', v => setNow('change scale mode', { '-ff-background-scale-mode': v === 'stretch-to-fill' ? null : v }));
  row('Scale Mode', scale, '-ff-background-scale-mode (with no size or repeat, and no slices)', ['-ff-background-scale-mode']);
  sync.push(() => scale.mark(value('-ff-background-scale-mode') || 'stretch-to-fill'));

  const note = document.createElement('p');
  note.className = 'none bg-note';
  const showNote = () => {
    const b = parseBackground(BACKGROUND_PROPS.filter(p => value(p) !== undefined).map(p => `${p}: ${value(p)}`).join('; ') + '; background-color: #000');
    const isSliced = b && (b.left > 0 || b.top > 0 || b.right > 0 || b.bottom > 0);
    note.textContent = isSliced ? `Sliced${b.sprite && !b.sliceGiven ? ' by the sprite\'s borders' : ''}: the slices decide it - scale mode, size and repeat are not used.` : (value('background-size') || (value('background-repeat') && value('background-repeat') !== 'no-repeat')) ? 'Size and repeat decide it - the scale mode is not used.' : '';
  };
  showNote();
  sync.push(showNote);
  body.append(note);

  row('Size', propText(value('background-size') || '', v => set('change background size', { 'background-size': v.trim() || null }), 'auto, cover, contain, 24px 24px, 50% auto'), 'background-size: a tile\'s size, for repeat and position', ['background-size']);
  row('Position', propText(value('background-position') || '', v => set('change background position', { 'background-position': v.trim() || null }), 'left top, center, right 8px, 50% 100%'), 'background-position: where the picture (or the first tile) sits', ['background-position']);
  row('Repeat', propSelect([['', 'no-repeat'], ['repeat', 'repeat'], ['repeat-x', 'repeat-x'], ['repeat-y', 'repeat-y']], (value('background-repeat') || '').replace('no-repeat', ''), v => setNow('change repeat', { 'background-repeat': v || null })), 'background-repeat: the picture repeated across and down', ['background-repeat']);

  // The slices: the frame's own (over a sprite's borders), four borders, a scale and a type.
  const slicesNow = () => {
    const b = parseBackground(BACKGROUND_PROPS.filter(p => value(p) !== undefined).map(p => `${p}: ${value(p)}`).join('; ') + '; background-color: #000');
    return b ? [b.left, b.top, b.right, b.bottom] : [0, 0, 0, 0];
  };
  const writeSlices = s => {
    const [l, t, r, b] = s.map(v => Math.max(0, Math.round(v)));
    const all = l === t && t === r && r === b;
    return { '-ff-slice': l + t + r + b === 0 && !value('-ff-sprite') ? null : all ? String(l) : `${t} ${r} ${b} ${l}`, '-ff-slice-left': null, '-ff-slice-top': null, '-ff-slice-right': null, '-ff-slice-bottom': null };
  };
  const sliceGrid = document.createElement('div');
  sliceGrid.className = 'prop-grid four';
  ['L', 'T', 'R', 'B'].forEach((label, i) => {
    const f = scrubNumber(label, slicesNow()[i], v => { const s = slicesNow(); s[i] = v; set('change slices', writeSlices(s)); }, { min: 0, title: ['left', 'top', 'right', 'bottom'][i] + ': the border kept at its size, in the picture\'s pixels' });
    sync.push(() => f.set(slicesNow()[i]));
    sliceGrid.append(f);
  });
  row('Slice', sliceGrid, '-ff-slice: the borders that keep their size as the frame grows (9-slice) - the frame\'s own, over its sprite\'s', ['-ff-slice', '-ff-slice-left', '-ff-slice-top', '-ff-slice-right', '-ff-slice-bottom']);
  const editButton = document.createElement('button');
  editButton.textContent = 'Edit slices…';
  editButton.title = 'The picture large, its borders as lines to drag (a sprite: its own borders, in the Sprite editor)';
  editButton.onclick = () => {
    const image = bgImage(value('background-image'));
    if (!image) return say('choose a picture first', 'bad');
    if (value('-ff-sprite') && findSprite(image, value('-ff-sprite')) && !frameStyle(element).has('-ff-slice')) {
      editSprites(image, value('-ff-sprite'), n => setNow('use sprite', { '-ff-sprite': n }));
      return;
    }
    editSlices(image, rectValue(), slicesNow(), (s, part) => setNow('change slices', { ...writeSlices(s), '-ff-background-rect': part ? part.join(' ') : null }));
  };
  const sliceScale = scrubNumber('×', bgNumber(value('-ff-slice-scale') || '1') || 1, v => set('change slice scale', { '-ff-slice-scale': v === 1 ? null : String(v) }), { min: 0.1, step: 0.1, title: '-ff-slice-scale: menu units per picture pixel for the borders' });
  sync.push(() => sliceScale.set(bgNumber(value('-ff-slice-scale') || '1') || 1));
  const sliceRow = document.createElement('div');
  sliceRow.className = 'bg-slice-row';
  sliceRow.append(editButton, sliceScale);
  row('Slice Scale', sliceRow, '-ff-slice-scale, and the slice editor', ['-ff-slice-scale']);
  const type = segmented([['sliced', 'Sliced', 'The edges and the middle stretched'], ['tiled', 'Tiled', 'The edges and the middle repeated']], (value('-ff-slice-type') || 'sliced').toLowerCase(), v => setNow('change slice type', { '-ff-slice-type': v === 'sliced' ? null : v }));
  sync.push(() => type.mark((value('-ff-slice-type') || 'sliced').toLowerCase()));
  row('Slice Type', type, '-ff-slice-type', ['-ff-slice-type']);
  const filter = segmented([['linear', 'Smooth', 'linear: smooth when scaled'], ['point', 'Pixels', 'point: the pixels kept sharp']], (value('-ff-background-filter') || 'linear').toLowerCase(), v => setNow('change filter', { '-ff-background-filter': v === 'linear' ? null : v }));
  sync.push(() => filter.mark((value('-ff-background-filter') || 'linear').toLowerCase()));
  row('Filter', filter, '-ff-background-filter', ['-ff-background-filter']);

  // A gradient in place of a picture: the picture's rows go, the gradient's editor comes.
  const pictureRows = [...body.children].slice([...body.children].indexOf(gradientBox) + 1);
  const showMode = () => {
    const g = isGradient();
    modeButtons.forEach(([v, b]) => b.classList.toggle('on', (v === 'gradient') === g));
    gradientBox.hidden = !g;
    for (const r of pictureRows) r.hidden = g || (r === builtIn && !builtIn.textContent);
  };
  showMode();
  sync.push(showMode);
}
