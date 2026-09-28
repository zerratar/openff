// The window an ALIGN_BUTTON widget draws for itself.
//
// Most of a menu's boxes are painted into the screen background and cannot move: resize
// com_save and the panel behind it stays exactly where the art puts it. Alignment 4 is
// the exception. MBText builds a window from the widget's own rectangle:
//
//   m_ButtonWindow.bwCreateUL(plane, Vector2(x, y), Vector2(width, height), 0);
//
// so that one does follow a resize, which is the way to lay a menu out for a desktop
// rather than a phone.
//
// It is a nine slice from m014_button, and the rule is ButtonWindow.SetSize and
// SetPositionCC between them:
//
//   frame 0 top left      at (x+4,     y+4)
//   frame 6 top right     at (x+w-4,   y+4)
//   frame 2 bottom left   at (x+4,     y+h-4)
//   frame 4 bottom right  at (x+w-4,   y+h-4)
//   frame 7 top edge      centred across, scaled x by (w-16)/16
//   frame 3 bottom edge   the same, at y+h-4
//   frame 1 left edge     centred down, scaled y by (h-16)/16
//   frame 5 right edge    the same, at x+w-4
//   cell = (state is off ? 0 : 8) + frame
//
// Those +4s look wrong until you notice every part carries the half flag: a 16 by 16
// corner draws 8 by 8, offset by -4, so it lands centred on the point and the corner
// sits exactly in the widget's corner. The edges then span x+8 to x+w-8, which is
// precisely the gap the corners leave.
//
// Under 12 wide or tall the game hides the window rather than drawing a squashed one.

'use strict';

const BUTTON_BANK = 'files/m014_button.NCER';
let buttonWindow = null;      // { bank, sheet }, once loaded

async function loadButtonWindow() {
  if (buttonWindow) return buttonWindow;
  try {
    const bank = await api(`/api/cell?name=${encodeURIComponent(BUTTON_BANK)}`);
    const sheet = new Image();
    await new Promise((resolve) => {
      sheet.onload = resolve;
      sheet.onerror = resolve;
      sheet.src = wsUrl(`/api/image?name=${encodeURIComponent(bank.sheet)}`);
    });
    buttonWindow = sheet.width ? { bank, sheet } : null;
  } catch (error) {
    buttonWindow = null;
  }
  return buttonWindow;
}

/// One part of one cell, blitted at a given place and size.
function blitCell(gc, loaded, cellIndex, dx, dy, dw, dh) {
  const cell = loaded.bank.cells[cellIndex];
  if (!cell || !cell.parts.length) return;
  const part = cell.parts[0];
  gc.drawImage(loaded.sheet,
    part.sourceX, part.sourceY, part.width, part.height,
    dx, dy, dw, dh);
}

/// Draws the button window for a widget, as a canvas the size of that widget.
async function drawButtonWindow(width, height, pressed) {
  // The game hides it rather than drawing a squashed one.
  if (width < 12 || height < 12) return null;

  const loaded = await loadButtonWindow();
  if (!loaded) return null;

  const canvas = document.createElement('canvas');
  canvas.width = width;
  canvas.height = height;
  canvas.className = 'button-window';
  const gc = canvas.getContext('2d');
  gc.imageSmoothingEnabled = false;

  const base = pressed ? 8 : 0;
  const w = width;
  const h = height;

  // Corners, 8 by 8 after the half flag.
  blitCell(gc, loaded, base + 0, 0, 0, 8, 8);
  blitCell(gc, loaded, base + 6, w - 8, 0, 8, 8);
  blitCell(gc, loaded, base + 2, 0, h - 8, 8, 8);
  blitCell(gc, loaded, base + 4, w - 8, h - 8, 8, 8);

  // Edges, filling exactly the gap the corners leave.
  const across = Math.max(0, w - 16);
  const down = Math.max(0, h - 16);
  if (across > 0) {
    blitCell(gc, loaded, base + 7, 8, 0, across, 8);
    blitCell(gc, loaded, base + 3, 8, h - 8, across, 8);
  }
  if (down > 0) {
    blitCell(gc, loaded, base + 1, 0, 8, 8, down);
    blitCell(gc, loaded, base + 5, w - 8, 8, 8, down);
  }

  return canvas;
}


// ---- The game's window: what a <window/> frame is drawn with ----
//
// menu.BasicWindow, off state, over m000_window: the fill (cell 0, 256 square at half size) stretched
// to w-4 by h-4 two units in; the four 16x16 corners (cells 1, 7, 4, 9); along the top a fixed 64-wide
// piece (cell 6) ending at the top-right corner and a run (cell 5) stretched over what is left; the
// bottom (cell 10) and the right (cell 8) runs stretched corner to corner; down the left a 16-tall piece
// (cell 3) above the bottom-left corner and a run (cell 2) over the rest. Every part is centred on its
// position (the cells' OAMs are), so the rectangles below are where the game's own positions and scales
// put them (BasicWindow.bwSetSize / SetPositionCC; BW_FRAME_ANIM_NO, BW_FRAME_SIZE).

const WINDOW_BANK = 'files/m000_window.NCER';
let gameWindow = null;

async function loadGameWindow() {
  if (gameWindow) return gameWindow;
  try {
    const bank = await api(`/api/cell?name=${encodeURIComponent(WINDOW_BANK)}`);
    const sheet = new Image();
    await new Promise((resolve) => {
      sheet.onload = resolve;
      sheet.onerror = resolve;
      sheet.src = wsUrl(`/api/image?name=${encodeURIComponent(bank.sheet)}`);
    });
    gameWindow = sheet.width && bank.cells && bank.cells.length > 10 ? { bank, sheet } : null;
  } catch (error) {
    gameWindow = null;
  }
  return gameWindow;
}

/// A window the size of a frame, as the game draws it; drawn at `scale` pixels a unit so it stays crisp when zoomed.
async function drawGameWindow(width, height, scale = 4, tint = null) {
  if (width <= 0 || height <= 0) return null;
  const loaded = await loadGameWindow();
  if (!loaded) return null;
  const canvas = document.createElement('canvas');
  canvas.width = Math.ceil(width * scale);
  canvas.height = Math.ceil(height * scale);
  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;
  canvas.className = 'game-window';
  const gc = canvas.getContext('2d');
  gc.imageSmoothingEnabled = true;
  gc.scale(scale, scale);
  const w = width, h = height;
  const part = (cell, dx, dy, dw, dh) => {
    if (dw <= 0 || dh <= 0) return;
    const c = loaded.bank.cells[cell];
    if (!c || !c.parts.length) return;
    const p = c.parts[0];
    gc.drawImage(loaded.sheet, p.sourceX, p.sourceY, p.width, p.height, dx, dy, dw, dh);
  };
  part(0, 2, 2, w - 4, h - 4);
  part(1, 0, 0, 16, 16);
  part(7, w - 16, 0, 16, 16);
  part(4, 0, h - 16, 16, 16);
  part(9, w - 16, h - 16, 16, 16);
  if (w > 32) {
    const run = w - 32;
    const s = run > 64 ? 1 : run / 64;
    const x = run > 64 ? w - 48 : w / 2;
    part(6, x - 32 * s, 0, 64 * s, 16);
    if (run - 64 > 0) part(5, 16, 0, run - 64, 16);
    part(10, 16, h - 16, w - 32, 16);
  }
  if (h > 32) {
    const run = h - 32;
    const s = run > 16 ? 1 : run / 16;
    const y = run > 16 ? h - 24 : h / 2;
    part(3, 0, y - 8 * s, 16, 16 * s);
    if (run - 16 > 0) part(2, 0, 16, 16, run - 16);
    part(8, w - 16, 16, 16, h - 32);
  }
  // -ff-tint: the art multiplied by the colour, as the client's sprites modulate it; the art's own alpha kept.
  if (tint) {
    const mask = document.createElement('canvas');
    mask.width = canvas.width;
    mask.height = canvas.height;
    mask.getContext('2d').drawImage(canvas, 0, 0);
    gc.setTransform(1, 0, 0, 1, 0, 0);
    gc.globalCompositeOperation = 'multiply';
    gc.fillStyle = tint;
    gc.fillRect(0, 0, canvas.width, canvas.height);
    gc.globalCompositeOperation = 'destination-in';
    gc.drawImage(mask, 0, 0);
    gc.globalCompositeOperation = 'source-over';
  }
  return canvas;
}
