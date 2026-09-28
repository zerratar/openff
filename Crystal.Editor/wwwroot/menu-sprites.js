// Named sprites on a sheet, each with its own 9-slice borders - Unity's Sprite Editor in Multiple
// mode. A folder of screens keeps them in sprites.json beside its layouts (Shared/Text/MenuSprites.cs
// reads it in the client); a frame uses one by -ff-sprite with the sheet as its background-image, and
// the sprite gives it the part of the picture and its borders. Here: the file's data for the open
// screen (menu.sprites), and the editor - the sheet with its sprites on it, a sprite's borders dragged
// on its own large view, and sprites made by hand, from the game's own cells, by a grid, or found by
// their see-through gaps.

/// The sheet's key in sprites.json: "url:images/ui.png", "resource:files/m000_window.NCGR".
function spriteSheetKey(image) {
  return image ? `${image.kind}:${image.path}` : null;
}

function spritesOf(image) {
  const data = (typeof menu !== 'undefined' && menu.sprites) || null;
  const sheet = data && data.sheets && data.sheets[spriteSheetKey(image)];
  return sheet && Array.isArray(sheet.sprites) ? sheet.sprites : [];
}

function findSprite(image, name) {
  if (!name) return null;
  const n = String(name).trim().toLowerCase();
  return spritesOf(image).find(s => String(s.name).toLowerCase() === n) || null;
}

/// The open screen's sprites.json, loaded with it (a screen of the mod's or the client's).
async function loadMenuSprites() {
  menu.sprites = { sheets: {} };
  menu.spritesDirty = false;
  if (!menu.project) return;
  try {
    const r = await api(menu.project.client ? '/api/client/menu/sprites' : '/api/project/menu/sprites');
    if (r.ok && r.json) {
      const parsed = JSON.parse(r.json);
      if (parsed && typeof parsed === 'object') menu.sprites = { sheets: parsed.sheets || {} };
    }
  } catch (error) { /* none yet */ }
}

async function saveMenuSprites() {
  if (!menu.project || !menu.spritesDirty) return true;
  const json = JSON.stringify({ sheets: menu.sprites.sheets }, null, 2);
  const r = await api(menu.project.client ? '/api/client/menu/sprites/save' : '/api/project/menu/sprites/save', { json });
  if (!r.ok) { say(r.error, 'bad'); return false; }
  menu.spritesDirty = false;
  say(`saved ${r.savedTo && r.savedTo.length ? 'sprites.json in ' + r.savedTo.join(' and ') : 'sprites.json'}`, 'good');
  return true;
}

/// A sprite drawn into a small canvas, for the picker and the editor's list.
function spriteThumbnail(img, s, size = 64) {
  const c = document.createElement('canvas');
  const k = Math.min(size / s.w, size / s.h, 8);
  c.width = Math.max(1, Math.round(s.w * k));
  c.height = Math.max(1, Math.round(s.h * k));
  const g = c.getContext('2d');
  g.imageSmoothingEnabled = false;
  g.drawImage(img, s.x, s.y, s.w, s.h, 0, 0, c.width, c.height);
  return c;
}

// ------------------------------------------------------------------ finding sprites on a sheet

/// The game's own cells for a sheet of its (a bank with the same name, .NCER): each cell's parts as sprites.
async function spritesFromCells(image) {
  if (!image || image.kind !== 'resource') return [];
  const bankName = image.path.replace(/\.[^./]+$/, '') + '.NCER';
  let bank;
  try { bank = await api(`/api/cell?name=${encodeURIComponent(bankName)}`); } catch (error) { return []; }
  if (!bank || !bank.cells) return [];
  const found = [], seen = new Set();
  bank.cells.forEach((cell, i) => {
    const parts = cell.parts || [];
    parts.forEach((p, k) => {
      const w = Math.abs(p.width), h = Math.abs(p.height);
      const key = `${p.sourceX},${p.sourceY},${w},${h}`;
      if (!w || !h || seen.has(key)) return;
      seen.add(key);
      found.push({ name: parts.length === 1 ? `cell${i}` : `cell${i}_${k}`, x: p.sourceX, y: p.sourceY, w, h });
    });
  });
  return found;
}

/// The sheet cut into cells of a size, from an offset, with a gap between them; empty cells left out.
function spritesByGrid(img, cw, ch, ox = 0, oy = 0, gap = 0) {
  const data = pixelsOf(img);
  const found = [];
  let n = 0;
  for (let y = oy; y + ch <= img.height; y += ch + gap) {
    for (let x = ox; x + cw <= img.width; x += cw + gap) {
      if (!anyOpaque(data, img.width, x, y, cw, ch)) continue;
      found.push({ name: `sprite${n++}`, x, y, w: cw, h: ch });
    }
  }
  return found;
}

/// Sprites found by the see-through between them: every island of pixels that are not clear, as its box.
function spritesByAlpha(img, minSize = 4) {
  const data = pixelsOf(img);
  const w = img.width, h = img.height;
  const seen = new Uint8Array(w * h);
  const found = [];
  const stack = [];
  let n = 0;
  for (let start = 0; start < w * h; start++) {
    if (seen[start] || data[start * 4 + 3] < 8) continue;
    let x0 = w, y0 = h, x1 = 0, y1 = 0;
    stack.push(start);
    seen[start] = 1;
    while (stack.length) {
      const i = stack.pop();
      const x = i % w, y = (i - x) / w;
      if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y;
      const near = [x > 0 ? i - 1 : -1, x < w - 1 ? i + 1 : -1, y > 0 ? i - w : -1, y < h - 1 ? i + w : -1];
      for (const j of near) if (j >= 0 && !seen[j] && data[j * 4 + 3] >= 8) { seen[j] = 1; stack.push(j); }
    }
    if (x1 - x0 + 1 >= minSize && y1 - y0 + 1 >= minSize) found.push({ name: `sprite${n++}`, x: x0, y: y0, w: x1 - x0 + 1, h: y1 - y0 + 1 });
  }
  return found.sort((a, b) => a.y - b.y || a.x - b.x).map((s, i) => ({ ...s, name: `sprite${i}` }));
}

function pixelsOf(img) {
  const c = document.createElement('canvas');
  c.width = img.width;
  c.height = img.height;
  const g = c.getContext('2d', { willReadFrequently: true });
  g.drawImage(img, 0, 0);
  return g.getImageData(0, 0, img.width, img.height).data;
}

function anyOpaque(data, width, x, y, w, h) {
  for (let j = y; j < y + h; j++) for (let i = x; i < x + w; i++) if (data[(j * width + i) * 4 + 3] >= 8) return true;
  return false;
}

// ------------------------------------------------------------------ the editor

/// The Sprite editor for a sheet. `current`: the sprite to start on (by name); onUse(name) when one is chosen for the frame.
async function editSprites(image, current, onUse) {
  const img = await loadBackgroundPicture(image);
  if (!img) return say('the picture did not load', 'bad');
  const key = spriteSheetKey(image);
  menu.sprites = menu.sprites || { sheets: {} };
  // A working copy: Cancel leaves the file as it was.
  let list = spritesOf(image).map(s => ({ ...s }));
  let selected = Math.max(0, list.findIndex(s => current && String(s.name).toLowerCase() === String(current).toLowerCase()));
  if (!list.length) selected = -1;

  const veil = document.createElement('div');
  veil.className = 'picker-veil';
  const box = document.createElement('div');
  box.className = 'picker sprite-editor';
  const head = document.createElement('div');
  head.className = 'picker-head';
  const title = document.createElement('strong');
  title.textContent = `Sprites of ${image.path.split('/').pop()} (${img.width} × ${img.height})`;
  const tools = document.createElement('div');
  tools.className = 'sprite-tools';
  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  head.append(title, tools, shut);

  const body = document.createElement('div');
  body.className = 'sprite-body';
  const sheetWrap = document.createElement('div');
  sheetWrap.className = 'sprite-sheet';
  const sheet = document.createElement('canvas');
  sheetWrap.append(sheet);
  const side = document.createElement('div');
  side.className = 'sprite-side';
  body.append(sheetWrap, side);

  const listBox = document.createElement('div');
  listBox.className = 'sprite-list';
  const fields = document.createElement('div');
  fields.className = 'sprite-fields';
  const detail = document.createElement('canvas');
  detail.className = 'sprite-detail';
  side.append(listBox, fields, detail);

  const foot = document.createElement('div');
  foot.className = 'slice-buttons';
  const hint = document.createElement('p');
  hint.className = 'none';
  hint.textContent = 'Drag on the sheet to make a sprite, drag one to move it; its borders are the green lines on its own view (they keep their size as a frame grows). Saved to sprites.json beside the screens.';
  const cancel = document.createElement('button');
  cancel.textContent = 'Cancel';
  const save = document.createElement('button');
  save.textContent = 'Save';
  const use = document.createElement('button');
  use.className = 'primary';
  use.textContent = 'Use this sprite';
  foot.append(hint, cancel, save, use);
  box.append(head, body, foot);
  veil.append(box);
  document.body.append(veil);

  const zoom = Math.max(0.25, Math.min(8, Math.min(640 / img.width, 440 / img.height)));
  sheet.width = Math.round(img.width * zoom);
  sheet.height = Math.round(img.height * zoom);
  const sg = sheet.getContext('2d');

  const tool = (label, tip, run) => { const b = document.createElement('button'); b.textContent = label; b.title = tip; b.onclick = run; tools.append(b); return b; };
  const replaceAll = (found, what) => {
    if (!found.length) return say(`no sprites found ${what}`, 'bad');
    if (list.length && !confirm(`Replace the ${list.length} sprite${list.length === 1 ? '' : 's'} with the ${found.length} found ${what}?`)) return;
    list = found;
    selected = 0;
    draw();
  };
  if (image.kind === 'resource') tool('From the game\'s cells', 'The pieces the game itself cuts this sheet into (its .NCER bank)', async () => replaceAll(await spritesFromCells(image), 'in the game\'s cells'));
  tool('Auto', 'Every island of pixels that are not see-through, as a sprite', () => replaceAll(spritesByAlpha(img), 'by transparency'));
  const gridW = document.createElement('input');
  gridW.type = 'number'; gridW.value = '16'; gridW.min = '1'; gridW.title = 'cell width';
  const gridH = document.createElement('input');
  gridH.type = 'number'; gridH.value = '16'; gridH.min = '1'; gridH.title = 'cell height';
  tool('Grid', 'Cut the sheet into cells of the size beside (empty cells left out)', () => replaceAll(spritesByGrid(img, Math.max(1, +gridW.value || 16), Math.max(1, +gridH.value || 16)), 'by the grid'));
  tools.append(gridW, gridH);
  tool('Delete', 'Delete the selected sprite', () => { if (selected < 0) return; list.splice(selected, 1); selected = Math.min(selected, list.length - 1); draw(); });

  const s = () => (selected >= 0 ? list[selected] : null);
  const inputs = {};
  const numberField = (label, key2, min = 0) => {
    const f = scrubNumber(label, 0, v => { const sp = s(); if (!sp) return; sp[key2] = Math.max(min, v); clampSprite(sp); draw(); }, { min });
    inputs[key2] = f;
    return f;
  };
  const nameInput = document.createElement('input');
  nameInput.type = 'text';
  nameInput.placeholder = 'name';
  nameInput.oninput = () => { const sp = s(); if (!sp) return; sp.name = nameInput.value.trim().replace(/[\s;:"']/g, '_') || sp.name; drawList(); };
  const rectRow = document.createElement('div');
  rectRow.className = 'sprite-grid4';
  rectRow.append(numberField('X', 'x'), numberField('Y', 'y'), numberField('W', 'w', 1), numberField('H', 'h', 1));
  const borderRow = document.createElement('div');
  borderRow.className = 'sprite-grid4';
  borderRow.append(numberField('L', 'left'), numberField('T', 'top'), numberField('R', 'right'), numberField('B', 'bottom'));
  const label = (text) => { const l = document.createElement('span'); l.className = 'sprite-label'; l.textContent = text; return l; };
  fields.append(label('Name'), nameInput, label('Rect'), rectRow, label('Borders (9-slice)'), borderRow);

  const clampSprite = sp => {
    sp.x = Math.max(0, Math.min(img.width - 1, Math.round(sp.x)));
    sp.y = Math.max(0, Math.min(img.height - 1, Math.round(sp.y)));
    sp.w = Math.max(1, Math.min(img.width - sp.x, Math.round(sp.w)));
    sp.h = Math.max(1, Math.min(img.height - sp.y, Math.round(sp.h)));
    for (const k of ['left', 'top', 'right', 'bottom']) sp[k] = Math.max(0, Math.round(sp[k] || 0));
    sp.left = Math.min(sp.left, sp.w); sp.right = Math.min(sp.right, sp.w - sp.left);
    sp.top = Math.min(sp.top, sp.h); sp.bottom = Math.min(sp.bottom, sp.h - sp.top);
  };

  const drawList = () => {
    listBox.textContent = '';
    list.forEach((sp, i) => {
      const row = document.createElement('button');
      row.className = 'sprite-row' + (i === selected ? ' on' : '');
      row.append(spriteThumbnail(img, sp, 28));
      const n = document.createElement('span');
      n.textContent = sp.name;
      const d = document.createElement('i');
      d.textContent = `${sp.w}×${sp.h}${sp.left || sp.top || sp.right || sp.bottom ? ' · sliced' : ''}`;
      row.append(n, d);
      row.onclick = () => { selected = i; draw(); };
      listBox.append(row);
    });
    if (!list.length) {
      const none = document.createElement('p');
      none.className = 'none';
      none.textContent = 'No sprites yet: drag a rectangle on the sheet, or use a tool above.';
      listBox.append(none);
    }
  };

  let detailZoom = 1;
  const drawDetail = () => {
    const sp = s();
    const g = detail.getContext('2d');
    if (!sp) { detail.width = 1; detail.height = 1; return; }
    detailZoom = Math.max(1, Math.min(12, Math.floor(Math.min(260 / sp.w, 200 / sp.h))));
    detail.width = sp.w * detailZoom;
    detail.height = sp.h * detailZoom;
    g.imageSmoothingEnabled = false;
    for (let y = 0; y < detail.height; y += 8) for (let x = 0; x < detail.width; x += 8) { g.fillStyle = ((x + y) / 8) % 2 ? '#2a2f37' : '#343a44'; g.fillRect(x, y, 8, 8); }
    g.drawImage(img, sp.x, sp.y, sp.w, sp.h, 0, 0, detail.width, detail.height);
    g.strokeStyle = '#4ade80';
    g.setLineDash([4, 3]);
    const line = (x1, y1, x2, y2) => { g.beginPath(); g.moveTo(Math.round(x1) + 0.5, Math.round(y1) + 0.5); g.lineTo(Math.round(x2) + 0.5, Math.round(y2) + 0.5); g.stroke(); };
    line(sp.left * detailZoom, 0, sp.left * detailZoom, detail.height);
    line((sp.w - sp.right) * detailZoom, 0, (sp.w - sp.right) * detailZoom, detail.height);
    line(0, sp.top * detailZoom, detail.width, sp.top * detailZoom);
    line(0, (sp.h - sp.bottom) * detailZoom, detail.width, (sp.h - sp.bottom) * detailZoom);
    g.setLineDash([]);
  };

  const drawSheet = () => {
    sg.imageSmoothingEnabled = zoom < 1;
    sg.clearRect(0, 0, sheet.width, sheet.height);
    for (let y = 0; y < sheet.height; y += 10) for (let x = 0; x < sheet.width; x += 10) { sg.fillStyle = ((x + y) / 10) % 2 ? '#2a2f37' : '#343a44'; sg.fillRect(x, y, 10, 10); }
    sg.drawImage(img, 0, 0, sheet.width, sheet.height);
    list.forEach((sp, i) => {
      sg.strokeStyle = i === selected ? '#60a5fa' : 'rgba(250, 204, 21, .75)';
      sg.lineWidth = i === selected ? 2 : 1;
      sg.strokeRect(sp.x * zoom + 0.5, sp.y * zoom + 0.5, sp.w * zoom - 1, sp.h * zoom - 1);
    });
    if (dragRect) {
      sg.strokeStyle = '#60a5fa';
      sg.setLineDash([3, 3]);
      sg.strokeRect(dragRect.x * zoom + 0.5, dragRect.y * zoom + 0.5, dragRect.w * zoom, dragRect.h * zoom);
      sg.setLineDash([]);
    }
  };

  const draw = () => {
    drawSheet();
    drawList();
    const sp = s();
    fields.classList.toggle('off', !sp);
    nameInput.value = sp ? sp.name : '';
    for (const [k, f] of Object.entries(inputs)) f.set(sp ? sp[k] || 0 : 0);
    drawDetail();
    use.disabled = !sp;
  };

  // The sheet: drag out a new sprite on a clear spot, or drag one to move it.
  let dragRect = null, from = null, moving = null;
  const at = (e, c, k) => { const r = c.getBoundingClientRect(); return [(e.clientX - r.left) * c.width / r.width / k, (e.clientY - r.top) * c.height / r.height / k]; };
  sheet.onpointerdown = e => {
    const [x, y] = at(e, sheet, zoom);
    const hit = list.findIndex((sp, i) => i === selected && x >= sp.x && x < sp.x + sp.w && y >= sp.y && y < sp.y + sp.h);
    const any = hit >= 0 ? hit : list.findIndex(sp => x >= sp.x && x < sp.x + sp.w && y >= sp.y && y < sp.y + sp.h);
    sheet.setPointerCapture(e.pointerId);
    if (any >= 0) {
      selected = any;
      moving = { dx: x - list[any].x, dy: y - list[any].y };
      draw();
      return;
    }
    from = [Math.floor(x), Math.floor(y)];
    dragRect = { x: from[0], y: from[1], w: 0, h: 0 };
  };
  sheet.onpointermove = e => {
    const [x, y] = at(e, sheet, zoom);
    if (moving && s()) {
      const sp = s();
      sp.x = Math.round(x - moving.dx);
      sp.y = Math.round(y - moving.dy);
      clampSprite(sp);
      draw();
      return;
    }
    if (!dragRect) return;
    const x2 = Math.max(0, Math.min(img.width, Math.ceil(x))), y2 = Math.max(0, Math.min(img.height, Math.ceil(y)));
    dragRect = { x: Math.min(from[0], x2), y: Math.min(from[1], y2), w: Math.abs(x2 - from[0]), h: Math.abs(y2 - from[1]) };
    drawSheet();
  };
  sheet.onpointerup = () => {
    if (dragRect && dragRect.w >= 2 && dragRect.h >= 2) {
      let n = list.length, name;
      do { name = `sprite${n++}`; } while (list.some(sp => sp.name === name));
      list.push({ name, x: dragRect.x, y: dragRect.y, w: dragRect.w, h: dragRect.h, left: 0, top: 0, right: 0, bottom: 0 });
      selected = list.length - 1;
    }
    dragRect = null;
    moving = null;
    draw();
  };

  // The sprite's own view: its borders as lines to drag.
  let border = null;
  detail.onpointerdown = e => {
    const sp = s();
    if (!sp) return;
    const [x, y] = at(e, detail, detailZoom);
    const near = [['left', Math.abs(x - sp.left)], ['right', Math.abs(x - (sp.w - sp.right))], ['top', Math.abs(y - sp.top)], ['bottom', Math.abs(y - (sp.h - sp.bottom))]].sort((p, q) => p[1] - q[1])[0];
    if (near[1] * detailZoom > 8) return;
    border = near[0];
    detail.setPointerCapture(e.pointerId);
  };
  detail.onpointermove = e => {
    const sp = s();
    if (!sp) return;
    const [x, y] = at(e, detail, detailZoom);
    if (!border) {
      const nx = Math.min(Math.abs(x - sp.left), Math.abs(x - (sp.w - sp.right))) * detailZoom <= 8, ny = Math.min(Math.abs(y - sp.top), Math.abs(y - (sp.h - sp.bottom))) * detailZoom <= 8;
      detail.style.cursor = nx ? 'ew-resize' : ny ? 'ns-resize' : 'default';
      return;
    }
    if (border === 'left') sp.left = x; else if (border === 'right') sp.right = sp.w - x; else if (border === 'top') sp.top = y; else sp.bottom = sp.h - y;
    clampSprite(sp);
    for (const [k, f] of Object.entries(inputs)) f.set(sp[k] || 0);
    drawDetail();
  };
  detail.onpointerup = () => { if (border) drawList(); border = null; };

  const commit = () => {
    const names = new Set();
    for (const sp of list) {
      let n = sp.name, i = 2;
      while (names.has(n.toLowerCase())) n = `${sp.name}_${i++}`;
      sp.name = n;
      names.add(n.toLowerCase());
    }
    menu.sprites.sheets = menu.sprites.sheets || {};
    if (list.length) menu.sprites.sheets[key] = { sprites: list.map(sp => ({ name: sp.name, x: sp.x, y: sp.y, w: sp.w, h: sp.h, left: sp.left || 0, top: sp.top || 0, right: sp.right || 0, bottom: sp.bottom || 0 })) };
    else delete menu.sprites.sheets[key];
    menu.spritesDirty = true;
    if (typeof menuSheets !== 'undefined' && typeof menu !== 'undefined') menu.sheetsVersion = (menu.sheetsVersion || 0) + 1;
  };
  const close = () => { veil.remove(); document.removeEventListener('keydown', keys, true); };
  const keys = e => {
    if (e.key === 'Escape') { e.preventDefault(); close(); }
    else if (e.key === 'Delete' && document.activeElement && !document.activeElement.matches('input')) { e.preventDefault(); if (selected >= 0) { list.splice(selected, 1); selected = Math.min(selected, list.length - 1); draw(); } }
  };
  document.addEventListener('keydown', keys, true);
  shut.onclick = close;
  cancel.onclick = close;
  save.onclick = async () => { commit(); if (await saveMenuSprites()) { close(); if (menu.node) redraw(menu.node); } };
  use.onclick = async () => {
    const sp = s();
    if (!sp) return;
    commit();
    await saveMenuSprites();
    close();
    if (onUse) onUse(sp.name);
  };
  draw();
}
