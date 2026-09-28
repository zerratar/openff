// Drawing text the way the game draws it.
//
// A menu is mostly words - 348 of the 403 widgets across the eight .xbn files are Text -
// so a preview in the browser's font is showing the layout of something else. The game's
// font is Font12.glp and 57 Font12_*.xnb SpriteFont pages beside the game; the server
// works out where each character comes from and this blits them.
//
// The game draws one character at a time, each from whichever page holds it, advancing by
// that character's own width - so a line of English and Japanese is several atlases and
// there is no kerning between neighbours. The layout the server hands back is per
// character for exactly that reason.

'use strict';

const fontPages = new Map();     // "size/page" -> HTMLImageElement, once loaded
const fontLayouts = new Map();   // "size/text" -> the layout, because screens repeat

/// One atlas page, fetched once and kept.
function fontPage(size, page) {
  const key = `${size}/${page}`;
  if (fontPages.has(key)) return fontPages.get(key);

  const image = new Image();
  const ready = new Promise((resolve) => {
    image.onload = () => resolve(image);
    // A page that will not load leaves the text undrawn rather than the panel broken.
    image.onerror = () => resolve(null);
  });
  image.src = wsUrl(`/api/font/page?size=${size}&page=${page}`);
  const entry = { image, ready };
  fontPages.set(key, entry);
  return entry;
}

async function fontLayout(size, text) {
  const key = `${size}/${text}`;
  if (fontLayouts.has(key)) return fontLayouts.get(key);
  const layout = await api(
    `/api/font/layout?size=${size}&text=${encodeURIComponent(text)}`);
  fontLayouts.set(key, layout);
  return layout;
}

/// Draws a string into a canvas, in the game's font, and hands the canvas back.
///
/// The canvas is sized to the text rather than the widget, because a widget's declared
/// width is often 0 - the game measures the string and the box grows to it.
///
/// `lettering`: the text's own (a <textstyle>'s declarations, Crystal Style Sheets) - drawn in the
/// TrueType faces as the client draws a styled text (drawStyledText), whichever font the install has.
async function drawFontText(text, size, colour, lettering) {
  const style = lettering ? parseTextStyle(lettering) : null;
  if (style) {
    const styled = await drawStyledText(text, size, colour, style);
    if (styled) return styled;
  }
  const layout = await fontLayout(size, text);
  // No atlases (a Steam install: the game's text is TrueType there) - the install's faces, as the client draws them.
  if (layout.error || !layout.glyphs.length) return drawTrueTypeText(text, size, colour);

  // The layout arrives in menu units, which is what the preview is laid out in. Only
  // the source rectangles are still atlas pixels, so only those get scaled.
  const [sx, sy] = layout.scale;
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.ceil(layout.width) + 2);
  canvas.height = Math.ceil(size + 4);
  canvas.className = 'font-text';

  // Every page this string needs, before anything is drawn.
  await Promise.all(layout.pages.map(page => fontPage(size, page).ready));

  const gc = canvas.getContext('2d');
  gc.imageSmoothingEnabled = false;

  let pen = 0;
  for (const glyph of layout.glyphs) {
    const entry = fontPages.get(`${size}/${glyph.page}`);
    const image = entry && entry.image;
    if (image && image.width) {
      const [gx, gy, gw, gh] = glyph.source;
      gc.drawImage(image, gx, gy, gw, gh,
        pen + glyph.offset[0] * sx,
        glyph.offset[1] * sy - glyph.shiftY,
        gw * sx, gh * sy);
    }
    pen += glyph.advance;
  }

  // The atlas is white; the game tints it. Multiplying keeps the antialiasing.
  if (colour) {
    gc.globalCompositeOperation = 'source-in';
    gc.fillStyle = colour;
    gc.fillRect(0, 0, canvas.width, canvas.height);
  }

  return canvas;
}


// ---- TrueType: the game's text on a Steam install ----
//
// The Steam build (and the client on it) draws text from TrueType, not the phone's atlases: the install's
// faces, arial.ttf first with arialuni, TBUDRGothic and unifont behind it for what it lacks
// (OpenFF/Compat/TrueTypeText.cs). The client lays text out in an 800 x 480 space - a size-12 line is a
// 24 px face there (SizeFactor 2), raised a tenth of the size - and that space sits over the menu's 480 x
// 320 at 0.6 across and 2/3 down; a shadowed text is first drawn black one unit down and right
// (NNS_G3dGlbFlushP's text pass). This does the same, drawn at `scale` pixels a menu unit.

const GAME_FACES = [['OpenFF Arial', 'arial.ttf'], ['OpenFF Arial Unicode', 'arialuni.ttf'], ['OpenFF TBUDRGothic', 'TBUDRGoStd-Bold.otf'], ['OpenFF Unifont', 'unifont.ttf']];
let gameFaces = null;

function loadGameFaces() {
  if (!gameFaces) {
    gameFaces = Promise.all(GAME_FACES.map(async ([family, file]) => {
      try {
        const face = new FontFace(family, `url(${wsUrl(`/api/typeface?name=${encodeURIComponent(file)}`)})`);
        await face.load();
        document.fonts.add(face);
        return family;
      } catch (error) {
        return null;
      }
    })).then(list => list.filter(Boolean));
  }
  return gameFaces;
}

async function drawTrueTypeText(text, size, colour, scale = 4, shadow = true) {
  const families = await loadGameFaces();
  if (!families.length) return null;
  const px = size * 2;
  const font = `${px}px ${families.map(f => `"${f}"`).join(', ')}, sans-serif`;
  const probe = document.createElement('canvas').getContext('2d');
  probe.font = font;
  const metrics = probe.measureText(text);
  const ascent = metrics.fontBoundingBoxAscent || px * 0.9;
  const descent = metrics.fontBoundingBoxDescent || px * 0.25;
  // In menu units: the text space's 0.6 across and 2/3 down.
  const width = (metrics.width + 2) * 0.6;
  const height = (ascent + descent + 2) * (2 / 3);
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.ceil(width * scale));
  canvas.height = Math.max(1, Math.ceil(height * scale));
  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;
  canvas.className = 'font-text tt';
  canvas.menuWidth = metrics.width * 0.6;
  const gc = canvas.getContext('2d');
  gc.scale(scale * 0.6, scale * (2 / 3));
  gc.font = font;
  gc.textBaseline = 'alphabetic';
  const y = ascent - size * 0.1;
  if (shadow) {
    gc.fillStyle = '#000';
    gc.fillText(text, 1, y + 1);
  }
  gc.fillStyle = colour || '#fff';
  gc.fillText(text, 0, y);
  return canvas;
}


// ---- A menu text's own lettering (Crystal Style Sheets: text-shadow, font-family and the rest) ----
//
// A port of Shared/Text/MenuText.cs, and drawn as the client's text pass draws a styled text
// (GlobalScope.DrawStyledText): its shadows first, the last underneath, each blurred; its outline
// as the text drawn round about in the stroke's colour; then the text in its face, weight, slant
// and spacing. Every length is in menu units. A face beside the layout (url(...)) is loaded from
// the file the screen's folder serves; serif is Times New Roman, monospace Consolas, and any other
// name is Windows' face of that name - each with the game's faces behind it for what it lacks.

/// The lettering its declarations make, or null when they say nothing it draws (MenuText.Parse).
function parseTextStyle(declarations) {
  if (!declarations || !String(declarations).trim()) return null;
  const t = { key: String(declarations).trim(), shadows: null, families: [], bold: false, italic: false, letterSpacing: 0, lineHeight: null, lineFactor: false,
    underline: false, lineThrough: false, transform: null, strokeWidth: 0, strokeColour: [0, 0, 0, 255] };
  for (const [k, raw] of styleDeclarations(declarations)) {
    const v = raw.trim();
    const low = v.toLowerCase();
    switch (k) {
      case 'text-shadow':
        t.shadows = [];
        if (low === 'none') break;
        for (const item of cssCommaList(v)) {
          const numbers = [];
          let colour = null;
          for (const word of cssWords(item)) {
            const n = bgLength(word);
            if (n !== null) numbers.push(n);
            else colour = bgTextColour(word) || colour;
          }
          if (numbers.length < 2) continue;
          t.shadows.push({ x: numbers[0], y: numbers[1], blur: numbers.length > 2 ? Math.max(0, numbers[2]) : 0, colour: colour || [0, 0, 0, 255] });
        }
        break;
      case 'font-family':
        t.families = [];
        for (const item of cssCommaList(v)) {
          const image = bgImage(item);
          if (image && image.kind === 'url') t.families.push('url:' + image.path);
          else {
            const name = item.trim().replace(/^["']+|["']+$/g, '').trim();
            if (name) t.families.push(name.toLowerCase());
          }
        }
        break;
      case 'font-weight': t.bold = low === 'bold' || low === 'bolder' || (/^\d+$/.test(low) && parseInt(low, 10) >= 600); break;
      case 'font-style': t.italic = low.startsWith('italic') || low.startsWith('oblique'); break;
      case 'letter-spacing': t.letterSpacing = low === 'normal' ? 0 : bgLength(low) ?? 0; break;
      case 'line-height':
        if (low === 'normal') { t.lineHeight = null; break; }
        if (low !== '' && Number.isFinite(Number(low))) { t.lineHeight = Number(low); t.lineFactor = true; }
        else if (low.endsWith('%') && Number.isFinite(Number(low.slice(0, -1)))) { t.lineHeight = Number(low.slice(0, -1)) / 100; t.lineFactor = true; }
        else if (bgLength(low) !== null) { t.lineHeight = bgLength(low); t.lineFactor = false; }
        break;
      case 'text-decoration':
      case 'text-decoration-line':
        t.underline = low.includes('underline');
        t.lineThrough = low.includes('line-through');
        break;
      case 'text-transform': t.transform = ['uppercase', 'lowercase', 'capitalize'].includes(low) ? low : null; break;
      case '-ff-text-stroke':
      case '-webkit-text-stroke':
        t.strokeWidth = 0;
        for (const word of cssWords(v)) {
          const n = bgLength(word);
          if (n !== null) t.strokeWidth = Math.max(0, n);
          else { const c = bgTextColour(word); if (c) t.strokeColour = c; }
        }
        break;
    }
  }
  const plain = t.shadows === null && !t.families.length && !t.bold && !t.italic && t.letterSpacing === 0 && t.lineHeight === null
    && !t.underline && !t.lineThrough && t.transform === null && t.strokeWidth <= 0;
  return plain ? null : t;
}

/// The text as the style's case has it.
function textCased(style, text) {
  if (!text || !style.transform) return text;
  if (style.transform === 'uppercase') return text.toUpperCase();
  if (style.transform === 'lowercase') return text.toLowerCase();
  return text.replace(/(^|[\s\-("'])(\p{Ll})/gu, (m, a, b) => a + b.toUpperCase());
}

const letteringFaces = new Map();   // "url:path" -> a promise of its family's name, or of null when it will not load

/// A face beside the layout (url("fonts/x.ttf")), loaded from the file the screen's folder serves.
function loadLetteringFace(path) {
  const key = 'url:' + path;
  if (!letteringFaces.has(key)) {
    const family = 'OpenFF Menu ' + letteringFaces.size;
    letteringFaces.set(key, (async () => {
      try {
        const url = backgroundPictureUrl({ kind: 'url', path });
        const bytes = await fetch(url).then(r => r.ok ? r.arrayBuffer() : null);
        if (!bytes) return null;
        const face = new FontFace(family, bytes);
        await face.load();
        document.fonts.add(face);
        return family;
      } catch (error) {
        return null;
      }
    })());
  }
  return letteringFaces.get(key);
}

/// The CSS font a style's families come to: its faces in order, the first found drawing, the game's behind them.
async function letteringFont(style, px, gameFamilies) {
  const list = [];
  for (const family of style.families) {
    if (family.startsWith('url:')) { const f = await loadLetteringFace(family.slice(4)); if (f) list.push(`"${f}"`); continue; }
    if (family === 'game' || family === 'sans-serif' || family === 'system-ui') break;
    list.push(`"${family === 'serif' ? 'Times New Roman' : family === 'monospace' ? 'Consolas' : family.replace(/"/g, '')}"`);
  }
  const game = gameFamilies.map(f => `"${f}"`).join(', ');
  return `${style.italic ? 'italic ' : ''}${style.bold ? 'bold ' : ''}${px}px ${[...list, game].filter(Boolean).join(', ')}, sans-serif`;
}

/// A styled text drawn as the client draws it (TrueType, the 800 x 480 text space over the menu's 480 x 320), at
/// `scale` pixels a menu unit; its canvas wider than the text by what its shadows and outline reach, and set back by it.
async function drawStyledText(text, size, colour, style, scale = 4) {
  const families = await loadGameFaces();
  if (!families.length) return null;
  const px = size * 2;
  const font = await letteringFont(style, px, families);
  const lines = textCased(style, String(text)).split('\n');
  const probe = document.createElement('canvas').getContext('2d');
  probe.font = font;
  // The spacing in the text space's pixels (the style's is in menu units, 0.6 of one across).
  const spacing = style.letterSpacing / 0.6;
  if ('letterSpacing' in probe) probe.letterSpacing = `${spacing}px`;
  const metrics = lines.map(line => probe.measureText(line));
  const ascent = metrics[0].fontBoundingBoxAscent || px * 0.9;
  const descent = metrics[0].fontBoundingBoxDescent || px * 0.25;
  const textWidth = Math.max(...metrics.map(m => m.width));
  // From one line to the next (menu units): the style's, or the size itself as the game steps its lines.
  const step = style.lineHeight === null ? size : style.lineFactor ? size * style.lineHeight : style.lineHeight;
  const width = (textWidth + 2) * 0.6;
  const height = (ascent + descent + 2) * (2 / 3) + step * (lines.length - 1);
  // No text-shadow said: the game's own drop shadow, as the plain text has it; none: not even that.
  const shadows = style.shadows === null ? [{ x: 0.6, y: 2 / 3, blur: 0, colour: [0, 0, 0, 255] }] : style.shadows;
  // Room round it for the shadows and the outline.
  let pad = style.strokeWidth;
  for (const s of shadows) pad = Math.max(pad, Math.abs(s.x) + s.blur * 1.5, Math.abs(s.y) + s.blur * 1.5);
  pad = Math.ceil(pad);
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.ceil((width + pad * 2) * scale));
  canvas.height = Math.max(1, Math.ceil((height + pad * 2) * scale));
  canvas.style.width = `${width + pad * 2}px`;
  canvas.style.height = `${height + pad * 2}px`;
  canvas.style.position = 'relative';
  canvas.style.left = `${-pad}px`;
  canvas.style.top = `${-pad}px`;
  canvas.className = 'font-text tt';
  canvas.menuWidth = textWidth * 0.6;
  canvas.menuHeight = size + step * (lines.length - 1);
  const gc = canvas.getContext('2d');
  const baseline = ascent - size * 0.1;
  const far = 20000;
  // One draw of the text: moved (dx, dy) menu units, in a colour, blurred by a shadow's blur (sigma half of it, as CSS's).
  const run = (dx, dy, c, blur) => {
    gc.save();
    const x = (pad + dx) * scale, y = (pad + dy) * scale;
    if (blur > 0) {
      // Drawn far off the canvas, so only its shadow lands.
      gc.setTransform(scale * 0.6, 0, 0, scale * (2 / 3), x - far, y);
      gc.shadowColor = c;
      gc.shadowBlur = blur * scale;
      gc.shadowOffsetX = far;
      gc.fillStyle = '#000';
    } else {
      gc.setTransform(scale * 0.6, 0, 0, scale * (2 / 3), x, y);
      gc.fillStyle = c;
    }
    gc.font = font;
    gc.textBaseline = 'alphabetic';
    if ('letterSpacing' in gc) gc.letterSpacing = `${spacing}px`;
    lines.forEach((line, i) => {
      const at = baseline + step * i * 1.5;
      gc.fillText(line, 0, at);
      // The lines under or through it, as FontStashSharp's decoration draws them.
      if (style.underline || style.lineThrough) {
        const w = metrics[i].width, thick = Math.max(1, px / 12);
        if (style.underline) gc.fillRect(0, at + px * 0.1, w, thick);
        if (style.lineThrough) gc.fillRect(0, at - px * 0.3, w, thick);
      }
    });
    gc.restore();
  };
  const css = c => `rgba(${c[0]}, ${c[1]}, ${c[2]}, ${c[3] / 255})`;
  for (let i = shadows.length - 1; i >= 0; i--) run(shadows[i].x, shadows[i].y, css(shadows[i].colour), shadows[i].blur);
  if (style.strokeWidth > 0) {
    // The outline: the text round about at the stroke's width (and half of it, for a wide one), under the text.
    const w = style.strokeWidth, steps = w > 1.5 ? 16 : 8;
    for (let ring = w > 1.5 ? 2 : 1; ring >= 1; ring--) {
      const r = w * ring / (w > 1.5 ? 2 : 1);
      for (let k = 0; k < steps; k++) { const a = k * Math.PI * 2 / steps; run(Math.cos(a) * r, Math.sin(a) * r, css(style.strokeColour), 0); }
    }
  }
  run(0, 0, colour || '#fff', 0);
  return canvas;
}
