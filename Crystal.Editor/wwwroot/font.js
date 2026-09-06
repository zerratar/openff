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
  if (layout.error || !layout.glyphs.length) return null;

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
