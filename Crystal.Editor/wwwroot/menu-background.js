// A menu frame's background in the editor: the colour and picture behind it (background-color,
// background-image, the 9-slice...), laid out as the client lays it out - a port of
// Shared/Text/MenuBackground.cs, the same quads - and drawn on the canvas; the inspector's Background
// section, the picture picker, and the slice editor that sets a picture's borders by dragging them.

// BACKGROUND_PROPS: menu-styles.js's, which bakes them.

// ------------------------------------------------------------------ the port

function bgNumber(text) {
  let t = String(text || '').trim().toLowerCase();
  if (t.endsWith('px')) t = t.slice(0, -2);
  const v = parseFloat(t);
  return Number.isFinite(v) ? v : 0;
}

/// #rgb, #rgba, #rrggbb, #rrggbbaa, rgb()/rgba(), transparent - as [r, g, b, a] 0..255, or null.
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
  const m = /^rgba?\(([^)]*)\)$/.exec(v);
  if (!m) return null;
  const parts = m[1].split(',').map(p => p.trim());
  if (parts.length < 3) return null;
  const c = p => Math.max(0, Math.min(255, Math.round(bgNumber(p))));
  const a = parts.length > 3 ? Math.max(0, Math.min(255, Math.round((parts[3].endsWith('%') ? bgNumber(parts[3]) / 100 : bgNumber(parts[3])) * 255))) : 255;
  return [c(parts[0]), c(parts[1]), c(parts[2]), a];
}

function bgImage(value) {
  const v = String(value || '').trim();
  for (const kind of ['url', 'resource']) {
    if (!v.toLowerCase().startsWith(kind + '(') || !v.endsWith(')')) continue;
    const inner = v.slice(kind.length + 1, -1).trim().replace(/^["']|["']$/g, '');
    return inner ? { kind, path: inner.replace(/\\/g, '/') } : null;
  }
  return null;
}

/// A background from its declarations, or null when it draws nothing.
function parseBackground(declarations) {
  if (!declarations || !declarations.trim()) return null;
  const b = { image: null, colour: null, tint: [255, 255, 255, 255], rect: null, scaleMode: null, size: null, position: null, repeat: null,
    left: 0, top: 0, right: 0, bottom: 0, sliceScale: 1, tiled: false, linear: true, sprite: null, sliceGiven: false };
  for (const [k, raw] of styleDeclarations(declarations)) {
    const v = raw.trim();
    switch (k) {
      case 'background-color': b.colour = bgColour(v); break;
      case 'background-image': b.image = bgImage(v); break;
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
  // A named sprite of the sheet (sprites.json): its part and its borders, where the frame gave none of its own.
  if (b.sprite && b.image && typeof findSprite === 'function') {
    const sp = findSprite(b.image, b.sprite);
    if (sp) {
      if (!b.rect && sp.w > 0 && sp.h > 0) b.rect = [sp.x, sp.y, sp.w, sp.h];
      if (!b.sliceGiven) { b.left = sp.left || 0; b.top = sp.top || 0; b.right = sp.right || 0; b.bottom = sp.bottom || 0; }
    }
  }
  const empty = !b.image && (!b.colour || b.colour[3] === 0);
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

/// The frame's background drawn at `scale` pixels a unit: its colour, then the picture's quads, tinted.
async function drawFrameBackground(declarations, width, height, scale, opacity = 1) {
  const b = parseBackground(declarations);
  if (!b || width <= 0 || height <= 0) return null;
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.ceil(width * scale));
  canvas.height = Math.max(1, Math.ceil(height * scale));
  canvas.style.width = `${width}px`;
  canvas.style.height = `${height}px`;
  canvas.className = 'frame-background';
  const gc = canvas.getContext('2d');
  gc.scale(scale, scale);
  if (b.colour && b.colour[3] > 0) {
    gc.fillStyle = `rgba(${b.colour[0]}, ${b.colour[1]}, ${b.colour[2]}, ${b.colour[3] / 255})`;
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
    gc.globalAlpha = ta / 255;
    gc.imageSmoothingEnabled = b.linear;
    for (const quad of layoutBackground(b, width, height, img.width, img.height)) {
      gc.drawImage(source, quad.u, quad.v, quad.uw, quad.vh, quad.x, quad.y, quad.w, quad.h);
    }
  }
  canvas.style.opacity = String(opacity);
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
  const row = (label, control, tip, props) => {
    const r = propRow(body, label, control, tip);
    if (!props) return r;
    const name = r.querySelector('.prop-label');
    const show = () => {
      const from = props.map(p => propertySource(element, p)).find(Boolean);
      const own = props.some(p => frameStyle(element).has(p));
      name.classList.toggle('own', own);
      name.classList.toggle('sheet', !own && !!from);
      name.title = own ? `Set on ${isScreen ? 'the screen' : 'this frame'} - click to take it off (back to ${props.map(p => computedFromSheets(p)).find(Boolean) || 'nothing'})` : from ? `From ${from.text}` : 'Not set';
    };
    name.onclick = () => {
      if (!props.some(p => frameStyle(element).has(p))) return;
      setNow('revert ' + label.toLowerCase(), Object.fromEntries(props.map(p => [p, null])));
    };
    show();
    sync.push(show);
    return r;
  };
  const computedFromSheets = prop => {
    const rules = isScreen ? screenRules(element) : frameRules(element);
    for (let i = rules.length - 1; i >= 0; i--) {
      const d = rules[i].declarations.find(([k]) => k === prop);
      if (d) return `${d[1]} (${rules[i].sheet})`;
    }
    return null;
  };

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
}
