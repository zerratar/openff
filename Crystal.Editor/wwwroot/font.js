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
async function drawFontText(text, size, colour) {
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
