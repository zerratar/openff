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
