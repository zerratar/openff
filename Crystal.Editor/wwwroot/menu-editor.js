// The Menus editor's tools: the screen's frames as a tree in the Hierarchy, the widget
// inspector in sections (Rect, Frame, Navigation, Text, Game behaviour, Behaviours), number
// fields that scrub, the screen's budget against the game's limits, and undo for all of it.
//
// The canvas and the XML panel are app.js's (openMenu, drawScreen); this is everything that
// edits a frame from outside the canvas. A frame is its <frame> element in menu.doc, and every
// edit goes through that element, so the canvas, the XML panel and these panels never disagree.

// ------------------------------------------------------------ number fields that scrub
//
// A number with its label to the left, as Unity's: drag across the label to change it (Shift
// for big steps, Alt for fine ones), type into it (a sum works: 240-32), the arrow keys and the
// wheel step it while it has the focus, Escape puts back what it was. onChange hears every
// value as it changes; field.set(v) shows a value without telling onChange.

function scrubNumber(label, value, onChange, options = {}) {
  const { min = -Infinity, max = Infinity, step = 1, title = '' } = options;
  const wrap = document.createElement('div');
  wrap.className = 'scrub';
  const grip = document.createElement('span');
  grip.className = 'scrub-grip';
  grip.textContent = label;
  grip.title = (title ? title + '\n' : '') + 'Drag across to change it - Shift for big steps, Alt for fine ones';
  const input = document.createElement('input');
  input.type = 'text';
  input.className = 'scrub-input';
  input.spellcheck = false;
  input.inputMode = 'decimal';
  if (title) input.title = title;
  let current = Number(value) || 0;
  let before = current;
  input.value = String(current);
  wrap.append(grip, input);

  const clamp = v => Math.min(max, Math.max(min, v));
  const put = (v, typed) => {
    v = clamp(Math.round(v / step) * step);
    if (!typed) input.value = String(v);
    if (v === current) return;
    current = v;
    onChange(v);
  };

  grip.onpointerdown = event => {
    if (event.button !== 0 || input.disabled) return;
    event.preventDefault();
    try { grip.setPointerCapture(event.pointerId); } catch (error) { /* a pointer the page did not see go down */ }
    let exact = current;
    let lastX = event.clientX;
    wrap.classList.add('scrubbing');
    document.body.classList.add('scrubbing');
    const move = e => {
      const dx = e.clientX - lastX;
      lastX = e.clientX;
      // Two pixels a unit; Shift four units a pixel, Alt eight pixels a unit.
      const rate = e.shiftKey ? 4 : e.altKey ? 0.125 : 0.5;
      exact = clamp(exact + dx * rate * step);
      put(exact);
    };
    const up = () => {
      grip.removeEventListener('pointermove', move);
      grip.removeEventListener('pointerup', up);
      grip.removeEventListener('pointercancel', up);
      wrap.classList.remove('scrubbing');
      document.body.classList.remove('scrubbing');
    };
    grip.addEventListener('pointermove', move);
    grip.addEventListener('pointerup', up);
    grip.addEventListener('pointercancel', up);
  };

  input.onfocus = () => { before = current; setTimeout(() => input.select(), 0); };
  input.oninput = () => {
    const text = input.value.trim();
    const v = Number(text);
    if (text !== '' && Number.isFinite(v)) put(v, true);
  };
  input.onkeydown = event => {
    if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      event.preventDefault();
      const by = step * (event.shiftKey ? 8 : 1);
      put(current + (event.key === 'ArrowUp' ? by : -by));
      input.select();
    } else if (event.key === 'Enter') {
      event.preventDefault();
      const text = input.value.trim();
      // A sum as well as a number: 480-64, 288/2, (480-200)/2.
      if (/^[-+*/().\d\s]+$/.test(text)) {
        try {
          const v = Function(`"use strict"; return (${text});`)();
          if (Number.isFinite(v)) put(v);
        } catch (error) { /* not a sum after all */ }
      }
      input.value = String(current);
      input.select();
    } else if (event.key === 'Escape') {
      event.preventDefault();
      put(before);
      input.blur();
    }
  };
  input.onblur = () => { input.value = String(current); };
  input.addEventListener('wheel', event => {
    if (document.activeElement !== input) return;
    event.preventDefault();
    const by = step * (event.shiftKey ? 8 : 1);
    put(current + (event.deltaY < 0 ? by : -by));
  }, { passive: false });

  wrap.input = input;
  wrap.disable = (off, why) => {
    input.disabled = !!off;
    wrap.classList.toggle('off', !!off);
    grip.title = off && why ? why : (title ? title + '\n' : '') + 'Drag across to change it - Shift for big steps, Alt for fine ones';
  };
  wrap.set = v => {
    if (document.activeElement === input) return;
    current = Number(v) || 0;
    input.value = String(current);
  };
  return wrap;
}

// ------------------------------------------------------------------ frames

/// The game's screen area a top-level frame sits in: 480 across, 288 down above the bottom bar.
const MENU_SCREEN = { w: 480, h: 288 };
/// What one screen may hold (menu.MenuManager): 32 frames at its top level, 30 texts a layer.
const MENU_TOP_FRAMES = 32;
const MENU_TEXTS_PER_LAYER = 30;

const menuFrameKeys = new WeakMap();
let menuFrameNext = 1;

/// A frame's key for the Hierarchy and the selection: its element's, so two frames of one id stay two rows.
function menuFrameKey(element) {
  let key = menuFrameKeys.get(element);
  if (!key) {
    key = 'frame:' + menuFrameNext++;
    menuFrameKeys.set(element, key);
  }
  return key;
}

const frameChildren = element => [...element.children].filter(e => e.tagName === 'frame');
const hasTag = (element, tag) => [...element.children].some(e => e.tagName === tag);
const isFrame = element => element && element.tagName === 'frame';

function removeTag(element, tag) {
  const child = [...element.children].find(e => e.tagName === tag);
  if (child) child.remove();
}

/// The screen the picker is on.
function menuScreen() {
  if (!menu.screens || !menu.node) return null;
  const picker = $('.screens', menu.node);
  return menu.screens[(picker && picker.value) || 0] || null;
}

function menuFrameByKey(key) {
  const screen = menuScreen();
  if (!screen) return null;
  return collectFrames(screen).map(f => f.element).find(e => menuFrameKey(e) === key) || null;
}

/// The <menu> a frame is on.
function frameScreen(element) {
  let e = element;
  while (isFrame(e)) e = e.parentElement;
  return e;
}

/// Where a frame is drawn on the screen, as the layout rules (menu-layout.js) put it.
function frameAbsolute(element) {
  const found = collectFrames(frameScreen(element)).find(f => f.element === element);
  return found ? { x: found.x, y: found.y } : { x: 0, y: 0 };
}

/// The rectangle a frame is placed in: its parent frame's, or the screen's for one at the top level.
function frameParentRect(element) {
  const parent = element.parentElement;
  const size = frameParentSize(element, menuLayoutRects(frameScreen(element)));
  if (isFrame(parent)) return { ...frameAbsolute(parent), w: size.w, h: size.h, name: childText(parent, 'id') || 'its parent' };
  return { x: 0, y: 0, w: size.w, h: size.h, name: 'the screen' };
}

/// The frame's rect relative to its parent as the layout works it out now, and the size it is placed in.
function frameLayoutNow(element) {
  const rects = menuLayoutRects(frameScreen(element));
  const rect = rects.get(element) || { x: 0, y: 0, w: 0, h: 0, driven: false, flow: false };
  return { rect, parent: frameParentSize(element, rects), rects };
}

/// Writes the frame's rect as it is now into its own x, y, width and height - what its style falls
/// back on - so a rule taken away or swapped for another leaves the frame where it was.
function settleFrame(element) {
  const { rect } = frameLayoutNow(element);
  // Less its translate, which is put on after the rest and stays.
  const t = LayoutStyle.of(element).translate(rect.w, rect.h);
  setChildText(element, 'x', String(Math.round(rect.x - t.x)));
  setChildText(element, 'y', String(Math.round(rect.y - t.y)));
  setChildText(element, 'width', String(rect.w));
  setChildText(element, 'height', String(rect.h));
}

function frameBehaviourName(element) {
  const behavior = [...element.children].find(e => e.tagName === 'behavior');
  if (!behavior) return null;
  const named = behavior.getAttribute('value');
  if (named) return named;
  const text = [...behavior.childNodes].find(n => n.nodeType === 3 && n.textContent.trim());
  return text ? text.textContent.trim() : null;
}

function frameIsText(element) {
  return textMessageId(element) !== null;
}

function frameIcon(element) {
  if (hasTag(element, 'portrait')) return 'character';
  if (hasTag(frameLook(element), 'window')) return 'menu';
  if (frameIsText(element)) return 'text';
  if (frameBehaviourName(element)) return 'behaviour';
  return 'part';
}

/// What a frame is, in a few words, for its Hierarchy row.
function frameSummary(element) {
  const words = [];
  const look = frameLook(element);
  if (hasTag(look, 'window')) words.push((childText(look, 'panel') || '').trim() === 'bar' ? 'bar' : 'window');
  if (hasTag(look, 'hidden')) words.push('hidden');
  if (hasTag(element, 'portrait')) words.push('portrait');
  const id = textMessageId(element);
  if (id !== null) {
    const literal = id < 0 ? (childText(element, 'data') || '') : null;
    const shown = literal !== null ? literal : menu.messages && menu.messages[id];
    if (shown) words.push('“' + (shown.length > 18 ? shown.slice(0, 17) + '…' : shown) + '”');
    else words.push(id < 0 ? 'text' : `msg ${id}`);
  } else {
    const behaviour = frameBehaviourName(element);
    if (behaviour) words.push(behaviour);
  }
  if (hasTag(element, 'focus')) words.push('focus');
  if (['data-source', 'bind-text', 'bind-visible', 'bind-display', 'bind-class', 'bind-style'].some(a => element.hasAttribute(a))) words.push('bound');
  const direction = LayoutStyle.of(element).word('flex-direction');
  if (direction === 'row' || direction === 'column') words.push(direction);
  return words.join(' · ');
}

function frameHasOwnBehaviours(element) {
  const def = menu.project && menu.project.definition;
  const id = (childText(element, 'id') || '').toLowerCase();
  return !!(def && id && (def.attachments || []).some(a => (a.target || '').toLowerCase() === id));
}

function menuIds(screen) {
  return new Set(collectFrames(screen).map(f => f.id).filter(Boolean));
}

function uniqueFrameId(screen, base) {
  const taken = menuIds(screen);
  if (!taken.has(base)) return base;
  const stem = base.replace(/\d+$/, '');
  for (let n = 1; ; n++) if (!taken.has(stem + n)) return stem + n;
}

// ------------------------------------------------------------------ the budget

/// What the screen takes of the game's limits: frames at the top level, and texts on each of
/// the two text layers (<display>1</display> puts a text on the main one).
function menuBudget(screen) {
  const top = frameChildren(screen).length;
  let sub = 0, main = 0;
  for (const f of collectFrames(screen)) {
    if (!frameIsText(f.element)) continue;
    if ((childText(f.element, 'display') || '').trim() === '1') main++;
    else sub++;
  }
  // The hero's portrait on a screen that asks for one takes a text slot of its own.
  const portrait = !!(menu.project && menu.project.definition && menu.project.definition.characterSelect);
  if (portrait) sub++;
  return { top, sub, main, portrait, over: top > MENU_TOP_FRAMES || sub > MENU_TEXTS_PER_LAYER || main > MENU_TEXTS_PER_LAYER };
}

function menuBudgetNote(screen) {
  const b = menuBudget(screen);
  return {
    text: `${b.top}/${MENU_TOP_FRAMES} · texts ${b.sub}+${b.main}`,
    title: `${b.top} of the ${MENU_TOP_FRAMES} frames a screen may have at its top level (more are not built)\n`
      + `${b.sub} of ${MENU_TEXTS_PER_LAYER} texts on the sub layer${b.portrait ? ', the hero\'s portrait among them' : ''}\n`
      + `${b.main} of ${MENU_TEXTS_PER_LAYER} on the main layer (display 1)\nPast either, the rest are not drawn.`,
    warn: b.over
  };
}

/// The budget as bars, for the screen's card.
function menuBudgetCard(screen) {
  const b = menuBudget(screen);
  const card = document.createElement('div');
  card.className = 'component budget';
  const head = document.createElement('div');
  head.className = 'behaviour-header';
  head.textContent = 'Budget';
  card.append(head);
  const bar = (label, used, cap, tip) => {
    const row = document.createElement('div');
    row.className = 'budget-row' + (used > cap ? ' over' : used >= cap - 2 ? ' near' : '');
    row.title = tip;
    const name = document.createElement('span');
    name.textContent = label;
    const track = document.createElement('u');
    const fill = document.createElement('i');
    fill.style.width = Math.min(100, used / cap * 100) + '%';
    track.append(fill);
    const count = document.createElement('b');
    count.textContent = `${used} / ${cap}`;
    row.append(name, track, count);
    card.append(row);
  };
  bar('Top-level frames', b.top, MENU_TOP_FRAMES, 'menu.MenuManager builds 32 frames at a screen\'s top level; the rest are left out. Frames nested in a window do not count.');
  bar('Texts, sub layer', b.sub, MENU_TEXTS_PER_LAYER, 'MESSAGE_CONTROLL_MAX: 30 texts a layer.' + (b.portrait ? ' The hero\'s portrait takes one.' : ''));
  bar('Texts, main layer', b.main, MENU_TEXTS_PER_LAYER, 'Texts with <display>1</display> - the other layer, with its own 30.');
  return card;
}

// ------------------------------------------------------------------ undo

/// The file as it is, and which screen and frame are on - what an undo step puts back.
function menuSnapshot(withSheets) {
  const picker = menu.node && $('.screens', menu.node);
  let path = null;
  if (menu.selected) {
    path = [];
    for (let e = menu.selected; isFrame(e); e = e.parentElement) path.unshift(frameChildren(e.parentElement).indexOf(e));
  }
  // A step that writes a stylesheet (Make a class) keeps the sheets as they were too; the others leave them be.
  const sheets = withSheets ? (menu.sheets || []).map(s => ({ name: s.name, css: s.css, dirty: s.dirty })) : null;
  return { xml: new XMLSerializer().serializeToString(menu.doc), screen: picker ? picker.value : 0, path, sheets };
}

function menuRestore(snapshot) {
  // The previewed states (:focus, :disabled) are on frames of the file about to be parsed afresh: kept by where the frames are.
  const pathOf = el => { const p = []; for (let e = el; isFrame(e); e = e.parentElement) p.unshift(frameChildren(e.parentElement).indexOf(e)); return { screen: menu.screens.indexOf(frameScreen(el)), p }; };
  const state = menu.previewState;
  const kept = state && { focus: state.focus && state.focus.isConnected ? pathOf(state.focus) : null, disabled: [...state.disabled].filter(e => e.isConnected).map(pathOf) };
  menu.doc = new DOMParser().parseFromString(snapshot.xml, 'application/xml');
  const found = [...menu.doc.documentElement.children].filter(e => e.tagName === 'menu' || e.tagName === 'unit');
  menu.screens = found.length ? found : [menu.doc.documentElement];
  if (kept) {
    const atPath = ({ screen, p }) => { let at = menu.screens[screen] || null; for (const i of p) at = at ? frameChildren(at)[i] || null : null; return isFrame(at) ? at : null; };
    state.focus = kept.focus ? atPath(kept.focus) : null;
    state.disabled = new Set(kept.disabled.map(atPath).filter(Boolean));
    state.version++;
  }
  const picker = $('.screens', menu.node);
  if (picker) picker.value = snapshot.screen;
  let at = menu.screens[snapshot.screen || 0] || null;
  for (const index of snapshot.path || []) at = at ? frameChildren(at)[index] || null : null;
  menu.selected = snapshot.path && isFrame(at) ? at : null;
  if (snapshot.sheets) { menu.sheets = snapshot.sheets.map(s => ({ ...s })); menu.sheetsVersion = (menu.sheetsVersion || 0) + 1; menuSheetsShown(); }
  redraw(menu.node);
}

/// Records the file as it is before a change. Changes of one kind that follow each other
/// closely (one drag, one field being typed in) share a step, given the same `run`.
function menuRemember(label, run, { sheets = false } = {}) {
  const doc = activeDoc;
  if (!doc || !menu.doc) return;
  const now = Date.now();
  const last = doc.undo && doc.undo[doc.undo.length - 1];
  if (run && last && last === menu.lastStep && menu.lastRun === run && now - menu.lastAt < 1200) {
    menu.lastAt = now;
    return;
  }
  const before = menuSnapshot(sheets);
  let after = null;
  pushUndo(doc, label, () => { after = menuSnapshot(sheets); menuRestore(before); }, () => { if (after) menuRestore(after); });
  menu.lastStep = doc.undo[doc.undo.length - 1];
  menu.lastRun = run;
  menu.lastAt = now;
}

// ------------------------------------------------------------------ changing the tree

function selectMenuFrame(element) {
  if (!menu.node) return;
  menu.selected = element || null;
  redraw(menu.node);
  if (element) {
    const box = $('.widget.on', menu.node);
    if (box) box.scrollIntoView({ block: 'nearest', inline: 'nearest' });
  }
}

/// A new frame inside `parent` (a frame, or the screen for one at the top level).
function addMenuFrame(parent, kind) {
  const screen = menuScreen();
  if (!screen) return;
  const doc = menu.doc;
  const frame = doc.createElement('frame');
  const add = (tag, text) => {
    const child = doc.createElement(tag);
    if (text !== undefined) child.textContent = text;
    frame.append(child);
    return child;
  };
  if (kind === 'window') add('window');
  if (kind === 'portrait') add('portrait');
  add('id', uniqueFrameId(screen, kind === 'text' ? 'text1' : kind === 'window' ? 'w_panel1' : kind === 'portrait' ? 'portrait' : 'frame1'));
  // A portrait where the game puts its own, at its size; the rest a little in from the parent's corner.
  const place = kind === 'portrait' && !isFrame(parent) ? GAME_PORTRAIT : { x: 8, y: 8, w: kind === 'text' ? 120 : kind === 'portrait' ? 56 : 160, h: kind === 'text' ? 24 : kind === 'portrait' ? 56 : 64 };
  add('x', String(place.x));
  add('y', String(place.y));
  add('width', String(place.w));
  add('height', String(place.h));
  if (kind === 'text') {
    const behavior = add('behavior');
    behavior.setAttribute('value', 'Text');
    for (const v of ['-1', '8', '0']) {
      const p = doc.createElement('parameter');
      p.textContent = v;
      behavior.append(p);
    }
    add('data', 'Text');
  }
  menuRemember('new ' + kind);
  parent.append(frame);
  if (!isFrame(parent) && frameChildren(screen).length > MENU_TOP_FRAMES) say(`the screen has ${frameChildren(screen).length} frames at its top level - the game builds ${MENU_TOP_FRAMES}; put some inside a window`, 'bad');
  selectMenuFrame(frame);
}

function duplicateMenuFrame(element) {
  const screen = menuScreen();
  if (!screen || !isFrame(element)) return;
  menuRemember('duplicate');
  const copy = element.cloneNode(true);
  // The copy and every frame in it get ids of their own.
  for (const f of [copy, ...copy.querySelectorAll('frame')]) {
    const id = childText(f, 'id');
    if (!id) continue;
    const taken = menuIds(screen);
    let fresh = id + '_copy';
    for (let n = 2; taken.has(fresh); n++) fresh = `${id}_copy${n}`;
    setChildText(f, 'id', fresh);
  }
  setChildText(copy, 'y', String(number(childText(copy, 'y')) + 8));
  element.after(copy);
  selectMenuFrame(copy);
}

function deleteMenuFrame(element) {
  if (!isFrame(element)) return;
  menuRemember('delete ' + (childText(element, 'id') || 'frame'));
  const siblings = frameChildren(element.parentElement);
  const at = siblings.indexOf(element);
  const next = siblings[at + 1] || siblings[at - 1] || (isFrame(element.parentElement) ? element.parentElement : null);
  element.remove();
  selectMenuFrame(next);
  say('deleted - Ctrl+Z brings it back', 'good');
}

/// Moves a frame before, after or into another (or to the screen's top level), keeping where
/// it is drawn: its x and y become relative to the new parent, as Unity keeps a moved object's place.
function moveMenuFrame(element, target, where) {
  if (!isFrame(element) || !target) return;
  if (target === element || element.contains(target)) return say('a frame cannot go inside itself', 'bad');
  const at = frameAbsolute(element);
  menuRemember('move ' + (childText(element, 'id') || 'frame'));
  if (where === 'inside') target.append(element);
  else if (where === 'before') target.before(element);
  else target.after(element);
  const parent = element.parentElement;
  // Back to where it was drawn, through whatever places it now (its edges, or its x and y); in a
  // column or row its new parent places it.
  const now = frameAbsolute(element);
  moveFrameBy(element, at.x - now.x, at.y - now.y, menuLayoutRects(frameScreen(element)));
  const screen = menuScreen();
  if (screen && !isFrame(parent) && frameChildren(screen).length > MENU_TOP_FRAMES) say(`${frameChildren(screen).length} frames at the top level - the game builds ${MENU_TOP_FRAMES}`, 'bad');
  selectMenuFrame(element);
}

function previousFrame(element) {
  const list = frameChildren(element.parentElement);
  return list[list.indexOf(element) - 1] || null;
}

function nextFrame(element) {
  const list = frameChildren(element.parentElement);
  return list[list.indexOf(element) + 1] || null;
}

function addMenuItems(parent) {
  const where = isFrame(parent) ? ' inside' : '';
  return [
    { label: 'New window' + where, icon: 'menu', run: () => addMenuFrame(parent, 'window') },
    { label: 'New text' + where, icon: 'text', run: () => addMenuFrame(parent, 'text') },
    { label: 'New frame' + where, icon: 'part', run: () => addMenuFrame(parent, 'frame') },
    { label: 'New portrait' + where, icon: 'character', run: () => addMenuFrame(parent, 'portrait') },
  ];
}

function frameMenuItems(element) {
  const before = previousFrame(element);
  const after = nextFrame(element);
  const nested = isFrame(element.parentElement);
  return [
    ...addMenuItems(element),
    { sep: true },
    { label: 'Duplicate (Ctrl+D)', icon: 'file', run: () => duplicateMenuFrame(element) },
    { label: 'Move up', disabled: !before, run: () => moveMenuFrame(element, before, 'before') },
    { label: 'Move down', disabled: !after, run: () => moveMenuFrame(element, after, 'after') },
    { label: 'Out of ' + (nested ? (childText(element.parentElement, 'id') || 'its parent') : 'its parent'), disabled: !nested, run: () => moveMenuFrame(element, element.parentElement, 'after') },
    { sep: true },
    ...menuStyleItems(element),
    { sep: true },
    { label: 'Delete (Del)', icon: 'exit', run: () => deleteMenuFrame(element) },
  ];
}

// ------------------------------------------------------------------ the Hierarchy

/// The open screen's frames as a tree, for shell.js's Hierarchy: nested as the file nests them,
/// a row selecting its frame on the canvas, dragged onto another to go before it (the top of the
/// row), inside it (the middle) or after it (the bottom).
function menuOutline(doc) {
  if (menu.name !== doc.name) return [];
  const screen = menuScreen();
  if (!screen) return [];
  menu.folded = menu.folded || new Set();

  const dropOn = target => {
    const handler = (key, where) => {
      const element = menuFrameByKey(key);
      if (element) moveMenuFrame(element, target, isFrame(target) ? where : 'inside');
    };
    if (isFrame(target)) handler.zones = true;
    return handler;
  };

  const rows = [{
    label: childText(screen, 'name') || 'screen',
    ref: 'screen',
    icon: 'menu',
    note: menu.project ? (menu.project.client ? 'the client\'s screen' : 'the mod\'s screen') : 'the game\'s screen',
    reveal: () => selectMenuFrame(null),
    menu: () => addMenuItems(screen),
    drop: dropOn(screen)
  }];
  // The screen's backdrop: one of the game's pictures (or none) under every frame, with an eye of its own.
  const def = menu.project && menu.project.definition;
  const backdropName = def ? backdropLabel(def.background ?? 10) : 'the game\'s own';
  const screenBg = menuStyled(screen).screenValues;
  rows.push({
    label: 'backdrop',
    ref: 'backdrop',
    icon: 'image',
    depth: 1,
    note: backdropName + (BACKGROUND_PROPS.some(k => screenBg.has(k)) ? ' \u00b7 + a background' : ''),
    eye: {
      hidden: !!menu.hideBackdrop,
      inherited: false,
      title: menu.hideBackdrop ? 'the backdrop is hidden in the view - click to show it' : 'click to hide the backdrop in the view (the editor\'s only)',
      toggle: () => { menu.hideBackdrop = !menu.hideBackdrop; menu.hideScreenBackground = menu.hideBackdrop; if (menu.node) redraw(menu.node, true); }
    },
    reveal: () => {
      menu.selected = null;
      if (menu.node) { $$('.widget', menu.node).forEach(w => w.classList.remove('on')); refreshXmlPanel(menu.node); }
      activeDoc.selection = 'backdrop';
      activeDoc.inspect = () => buildBackdropCard(menu.node);
    },
    menu: () => backdropMenuItems(screen)
  });
  if (gamePortraitShown(screen)) {
    rows.push({
      label: 'portrait',
      ref: 'game-portrait',
      icon: 'character',
      depth: 1,
      dim: true,
      note: 'the game\'s own, fixed',
      eye: {
        hidden: !!menu.hideGamePortrait,
        inherited: false,
        title: menu.hideGamePortrait ? 'hidden in the view - click to show it' : 'click to hide the game\'s portrait in the view (the editor\'s only)',
        toggle: () => { menu.hideGamePortrait = !menu.hideGamePortrait; if (menu.node) redraw(menu.node, true); }
      },
      reveal: () => {
        menu.selected = null;
        if (menu.node) $$('.widget', menu.node).forEach(w => w.classList.remove('on'));
        activeDoc.selection = 'game-portrait';
        activeDoc.inspect = () => buildGamePortraitCard();
      },
      menu: () => [{ label: 'Make it a frame (move, size and style it)', icon: 'character', run: () => addMenuFrame(screen, 'portrait') }]
    });
  }
  const walk = (parent, depth) => {
    for (const element of frameChildren(parent)) {
      const key = menuFrameKey(element);
      const nested = frameChildren(element).length > 0;
      const folded = menu.folded.has(key);
      rows.push({
        label: childText(element, 'id') || '(no id)',
        ref: key,
        icon: frameIcon(element),
        eye: frameEye(element),
        depth,
        note: frameSummary(element),
        badge: frameHasOwnBehaviours(element),
        fold: nested ? { folded, toggle: () => { if (menu.folded.has(key)) menu.folded.delete(key); else menu.folded.add(key); } } : null,
        reveal: () => selectMenuFrame(element),
        menu: () => frameMenuItems(element),
        drag: key,
        drop: dropOn(element)
      });
      if (!folded) walk(element, depth + 1);
    }
  };
  walk(screen, 1);

  const budget = menuBudgetNote(screen);
  return [{ label: 'Frames', note: budget.text, noteTitle: budget.title, warn: budget.warn, children: rows, menu: () => addMenuItems(screen), drop: dropOn(screen) }];
}

// ------------------------------------------------------------------ seen and not seen
//
// The Hierarchy's eyes: frames left out of the canvas for a while (to look at w_back alone), the
// editor's only - nothing in the file changes and the game shows them as ever. A frame hidden hides
// what is in it; Alt+click on one hides every frame but it (and its parents and what is in it), and
// Alt+click again brings them all back. Kept per menu tab for as long as it is open.

function menuUnseen() {
  menu.unseen = menu.unseen || new Set();
  return menu.unseen;
}

/// Whether the canvas leaves a frame out: it, or a frame it is in, has its eye shut.
function frameUnseen(element) {
  const unseen = menu.unseen;
  if (!unseen || !unseen.size) return false;
  for (let e = element; isFrame(e); e = e.parentElement) if (unseen.has(menuFrameKey(e))) return true;
  return false;
}

function frameEye(element) {
  const key = menuFrameKey(element);
  const unseen = menuUnseen();
  let inherited = false;
  for (let e = element.parentElement; isFrame(e); e = e.parentElement) if (unseen.has(menuFrameKey(e))) inherited = true;
  return {
    hidden: unseen.has(key),
    inherited,
    title: inherited && !unseen.has(key) ? 'hidden with the frame it is in' : null,
    toggle: alone => {
      if (alone) {
        const screen = menuScreen();
        const keep = new Set();
        for (let e = element; isFrame(e); e = e.parentElement) keep.add(menuFrameKey(e));
        for (const f of element.querySelectorAll('frame')) keep.add(menuFrameKey(f));
        const all = collectFrames(screen).map(f => f.element);
        // Already alone: everything back.
        const already = all.every(e => keep.has(menuFrameKey(e)) || frameUnseen(e)) && !unseen.has(key);
        unseen.clear();
        if (!already) {
          // The frames outside its line, at the highest level each can be shut at.
          for (const e of all) {
            if (keep.has(menuFrameKey(e))) continue;
            if (isFrame(e.parentElement) && !keep.has(menuFrameKey(e.parentElement))) continue;
            unseen.add(menuFrameKey(e));
          }
        }
      } else if (unseen.has(key)) unseen.delete(key);
      else unseen.add(key);
      if (menu.node) redraw(menu.node, true);
    }
  };
}

// ------------------------------------------------------------------ the inspector

/// A section of the inspector that folds, remembered open or shut by its title.
function propSection(panel, title, iconName, tip, openByDefault = true) {
  const details = document.createElement('details');
  details.className = 'component prop-section';
  const key = 'menu.section.' + title;
  let open = openByDefault;
  try { const v = localStorage.getItem(key); if (v !== null) open = v === '1'; } catch (e) { /* no storage */ }
  details.open = open;
  details.ontoggle = () => { try { localStorage.setItem(key, details.open ? '1' : '0'); } catch (e) { /* no storage */ } };
  const summary = document.createElement('summary');
  summary.className = 'component-head';
  summary.append(icon(iconName));
  const b = document.createElement('b');
  b.textContent = title;
  summary.append(b);
  if (tip) summary.title = tip;
  details.append(summary);
  const body = document.createElement('div');
  body.className = 'prop-body';
  details.append(body);
  panel.append(details);
  return body;
}

/// A row of the inspector: a label on the left, the control taking the rest.
function propRow(body, label, control, tip) {
  const row = document.createElement('div');
  row.className = 'prop-row';
  const name = document.createElement('span');
  name.className = 'prop-label';
  name.textContent = label;
  row.append(name, control);
  if (tip) row.title = tip;
  body.append(row);
  return row;
}

/// A switch with its words beside it (not stacked over them).
function propToggle(body, label, checked, onChange, hint) {
  const row = document.createElement('label');
  row.className = 'prop-toggle';
  const box = document.createElement('input');
  box.type = 'checkbox';
  box.checked = checked;
  box.onchange = () => onChange(box.checked);
  const name = document.createElement('span');
  name.textContent = label;
  row.append(box, name);
  if (hint) {
    const i = document.createElement('i');
    i.textContent = hint;
    row.append(i);
  }
  body.append(row);
  return box;
}

function propSelect(options, value, onChange) {
  const select = document.createElement('select');
  for (const [v, text] of options) {
    const o = document.createElement('option');
    o.value = v;
    o.textContent = text;
    select.append(o);
  }
  select.value = value;
  if (select.value !== value && value !== '') {
    // A value the list does not have stays as it is rather than turning into the first choice.
    const o = document.createElement('option');
    o.value = value;
    o.textContent = value;
    select.append(o);
    select.value = value;
  }
  select.onchange = () => onChange(select.value);
  return select;
}

function propText(value, onInput, placeholder) {
  const input = document.createElement('input');
  input.type = 'text';
  input.value = value ?? '';
  input.placeholder = placeholder || '';
  input.spellcheck = false;
  input.oninput = () => onInput(input.value);
  return input;
}

/// The widget inspector: what app.js's buildWidget hands the shared panel.
function buildWidgetInspector(held) {
  const panel = document.createElement('div');
  panel.className = 'widget-inspector';
  if (!held) return panel;
  const { node, screen } = held;
  const element = held.frame.element;
  const key = menuFrameKey(element);
  const sync = [];
  held.sync = () => sync.forEach(f => f());

  // Every edit: the undo step first (one per run of the same edit), the change, then the
  // canvas and the Hierarchy drawn again - quietly, leaving this panel and its focus alone.
  const edit = (what, change) => {
    menuRemember(what, what + key);
    change();
    drawScreen(node, screen, element, true);
  };
  // A change to what the panel shows (a section appears or goes): the panel is built again.
  const rebuild = (what, change) => {
    menuRemember(what);
    change();
    drawScreen(node, screen, element);
  };
  const tag = name => childText(element, name) ?? '';
  const setTag = (name, value) => {
    if (value === '' || value === null || value === undefined) removeTag(element, name);
    else setChildText(element, name, String(value));
  };

  // ---- the head: the id, and where the frame is
  const head = document.createElement('div');
  head.className = 'widget-head';
  head.append(icon(frameIcon(element)));
  const id = propText(tag('id'), v => edit('rename', () => setChildText(element, 'id', v)), '(no id)');
  id.className = 'widget-id';
  id.title = 'The frame\'s id: what code finds it by, and what up/down/left/right name';
  head.append(id);
  panel.append(head);
  const where = document.createElement('p');
  where.className = 'sub';
  const showWhere = () => {
    const chain = [];
    for (let e = element.parentElement; isFrame(e); e = e.parentElement) chain.unshift(childText(e, 'id') || '?');
    const at = frameAbsolute(element);
    where.textContent = `${[childText(screen, 'name') || 'screen', ...chain].join(' / ')}  ·  at ${at.x}, ${at.y} on the screen`;
  };
  showWhere();
  sync.push(showWhere);
  panel.append(where);

  buildRectSections(panel, element, screen, edit, rebuild, sync);
  buildStyleSection(panel, element, screen, edit, rebuild, sync);
  buildBackgroundSection(panel, element, screen, edit, rebuild, sync);
  buildBoxSection(panel, element, screen, edit, rebuild, sync);
  // A frame that is no text: its lettering is for the texts inside it, which inherit it.
  if (textMessageId(element) === null) {
    const has = TEXT_STYLE_PROPS.some(p => frameStyle(element).has(p));
    buildLetteringRows(propSection(panel, 'Lettering', 'text', 'The lettering of the texts inside it (they inherit it, as CSS inherits it): shadows, face, weight, spacing, case, outline', has), element, edit, rebuild, sync);
  }
  buildMotionSection(panel, element, screen, edit, rebuild, sync);
  buildBindingsSection(panel, element, screen, edit, rebuild, sync);

  // ---- Frame
  const frameBody = propSection(panel, 'Frame', 'menu', 'What kind of frame it is');
  propToggle(frameBody, 'Window', hasTag(element, 'window'), on => rebuild(on ? 'make a window' : 'not a window', () => {
    if (on) element.prepend(element.ownerDocument.createElement('window'));
    else removeTag(element, 'window');
  }), 'the game\'s window art (Background: a picture of your own)');
  propToggle(frameBody, 'Portrait', hasTag(element, 'portrait'), on => rebuild(on ? 'make a portrait' : 'not a portrait', () => {
    if (on) element.prepend(element.ownerDocument.createElement('portrait'));
    else removeTag(element, 'portrait');
  }), 'the hero\'s face, in place of the game\'s own');
  propToggle(frameBody, 'Focus', hasTag(element, 'focus'), on => rebuild(on ? 'focus on' : 'focus off', () => {
    if (on) element.prepend(element.ownerDocument.createElement('focus'));
    else removeTag(element, 'focus');
  }), 'the cursor can land on it');

  // ---- Navigation: where the cursor goes from here
  const ids = [...menuIds(screen)].filter(i => i !== tag('id'));
  const listId = 'menu-nav-ids';
  const hasNav = ['up', 'down', 'left', 'right'].some(d => tag(d));
  if (hasTag(element, 'focus') || hasNav) {
    const nav = propSection(panel, 'Navigation', 'exit', 'The frame the cursor moves to for each direction ("dummy": nowhere)');
    let list = document.getElementById(listId);
    if (!list) {
      list = document.createElement('datalist');
      list.id = listId;
      document.body.append(list);
    }
    list.textContent = '';
    for (const v of ['dummy', ...ids]) {
      const o = document.createElement('option');
      o.value = v;
      list.append(o);
    }
    const navGrid = document.createElement('div');
    navGrid.className = 'nav-grid';
    for (const [dir, arrow] of [['up', '↑'], ['down', '↓'], ['left', '←'], ['right', '→']]) {
      const cell = document.createElement('div');
      cell.className = 'nav-cell';
      const go = document.createElement('button');
      go.className = 'mini';
      go.textContent = arrow;
      go.title = `${dir}: select the frame it names`;
      go.onclick = () => {
        const target = collectFrames(screen).find(f => f.id && f.id === tag(dir));
        if (target) selectMenuFrame(target.element);
        else say(tag(dir) ? `no frame called ${tag(dir)} on this screen` : `nothing set for ${dir}`, 'bad');
      };
      const input = propText(tag(dir), v => edit('change ' + dir, () => setTag(dir, v.trim())), dir);
      input.setAttribute('list', listId);
      const known = () => input.classList.toggle('unknown', !!input.value.trim() && input.value.trim() !== 'dummy' && !ids.includes(input.value.trim()));
      input.addEventListener('input', known);
      known();
      cell.append(go, input);
      navGrid.append(cell);
    }
    nav.append(navGrid);
  }

  // ---- Text
  const messageId = textMessageId(element);
  if (messageId !== null) {
    const text = propSection(panel, 'Text', 'text', 'What the frame writes and how');
    const behavior = [...element.children].find(e => e.tagName === 'behavior');
    const parameters = () => [...behavior.children].filter(e => e.tagName === 'parameter');
    const param = index => {
      const p = parameters()[index];
      return p ? (p.getAttribute('value') ?? p.textContent) : '';
    };
    const setParam = (index, value) => {
      let list = parameters();
      while (list.length <= index) {
        behavior.append(element.ownerDocument.createElement('parameter'));
        list = parameters();
      }
      if (list[index].hasAttribute('value')) list[index].setAttribute('value', value);
      else list[index].textContent = value;
    };
    if (messageId < 0) {
      propRow(text, 'Text', propText(tag('data'), v => edit('change text', () => setChildText(element, 'data', v)), 'written by a behaviour'), 'What the layout shows; a behaviour may write over it');
    } else {
      const message = document.createElement('p');
      message.className = 'none';
      const showMessage = () => {
        const id = textMessageId(element);
        const shown = menu.messages && menu.messages[id];
        message.textContent = shown != null ? `“${shown}”` : menu.preview ? 'not in this language\'s messages' : 'turn on Preview to see the message';
      };
      propRow(text, 'Message', scrubNumber('#', messageId, v => { edit('change message', () => setParam(0, String(v))); showMessage(); }, { title: 'The message the frame draws (the game\'s text, by number; -1: a text of the layout\'s own)' }));
      showMessage();
      text.append(message);
    }
    if (menu.project || messageId < 0) {
      // A layout of the mod's or the client's: the style words the client reads.
      propRow(text, 'Size', propSelect([['', 'normal (12)'], ['large', 'large (16)'], ['8', '8'], ['10', '10'], ['14', '14'], ['18', '18'], ['20', '20'], ['24', '24'], ['28', '28']], (tag('font') || '').trim().toLowerCase(), v => edit('change size', () => setTag('font', v))), 'The game\'s two sizes, or one of the text\'s own (6..31), drawn by the TrueType face');
      propRow(text, 'Align', propSelect([['', 'left'], ['menu', 'menu (the hand stands clear)'], ['center', 'centre'], ['right', 'right'], ['button', 'button (a button frame behind)']], (tag('align') || '').trim().toLowerCase(), v => edit('change alignment', () => setTag('align', v))), 'Where the text sits across the frame; down, it is centred in the height');
      propRow(text, 'Colour', propSelect([['', 'white'], ['pale-blue', 'pale blue (headings)'], ['yellow', 'yellow (chosen)'], ['disabled', 'grey (cannot be taken)'], ['red', 'red'], ['green', 'green'], ['blue', 'blue'], ['cyan', 'cyan'], ['magenta', 'magenta'], ['pale-yellow', 'pale yellow'], ['pale-red', 'pale red']], (tag('colour') || '').trim().toLowerCase(), v => edit('change colour', () => setTag('colour', v))), 'The game\'s text colours; a behaviour may change it as the screen runs');
    } else {
      // One of the game's own layouts: the Text behaviour's parameters, as MBText reads them.
      propRow(text, 'Size', propSelect([['8', 'small (12)'], ['16', 'large (16)']], number(param(1)) > 8 ? '16' : '8', v => edit('change size', () => setParam(1, v))), 'MBText: a second parameter over 8 takes the large face');
      propRow(text, 'Align', propSelect([['0', 'left'], ['1', 'right'], ['2', 'centre'], ['3', 'flexible'], ['4', 'button'], ['5', 'menu']], String(number(param(2))), v => edit('change alignment', () => setParam(2, v))), 'MBText\'s third parameter');
    }
    propRow(text, 'Layer', propSelect([['', 'sub (the usual)'], ['1', 'main (display 1)']], (tag('display') || '').trim() === '1' ? '1' : '', v => edit('change layer', () => setTag('display', v))), 'Which of the two text layers it is drawn on - 30 texts each (see the budget)');
    // Its lettering (Crystal Style Sheets, the frame's own style): drawn by the TrueType faces, as the client draws it.
    const heading = document.createElement('p');
    heading.className = 'prop-subhead';
    heading.textContent = 'Lettering';
    text.append(heading);
    buildLetteringRows(text, element, edit, rebuild, sync);
  }

  // ---- Game behaviour: a <behavior> other than Text, with its parameters
  const behaviourName = frameBehaviourName(element);
  if (behaviourName && messageId === null) {
    const game = propSection(panel, 'Game behaviour', 'behaviour', 'The game\'s own behaviour on the frame (menu.Medget) and the parameters it reads, in order', false);
    const behavior = [...element.children].find(e => e.tagName === 'behavior');
    propRow(game, 'Name', propText(behaviourName, v => edit('rename behaviour', () => behavior.setAttribute('value', v))));
    [...behavior.children].filter(e => e.tagName === 'parameter').forEach((p, index) => {
      const value = p.getAttribute('value') ?? p.textContent;
      propRow(game, `#${index}`, propText(value, v => edit('change parameter', () => { if (p.hasAttribute('value')) p.setAttribute('value', v); else p.textContent = v; })));
    });
  }

  // ---- Behaviours (OpenFF): a screen of the mod's or the client's
  if (menu.project) {
    const behaviours = propSection(panel, 'Behaviours (OpenFF)', 'behaviour', 'MenuBehaviours on this frame: what a press or the focus does, in C# or the engine\'s own');
    const box = document.createElement('div');
    behaviours.append(box);
    menuBehaviourState().then(state => { if (typeof drawBehaviours === 'function') drawBehaviours(box, state, tag('id'), 'frame'); }).catch(e => { box.textContent = e.message; });
  }

  return panel;
}

// ------------------------------------------------------------------ Rect and Layout

/// The inspector's Rect (where the frame is drawn, and presets that anchor it), Layout (the rules
/// that place it: its edges, its size, or its part in its parent's column or row) and Children
/// (how it lays out the frames in it). Every field edits the rule that places the frame, so what
/// is typed is where it goes; a rule taken away leaves the frame where it was (settleFrame).
function buildRectSections(panel, element, screen, edit, rebuild, sync) {
  const now = () => frameLayoutNow(element);
  const inFlow = frameInFlow(element);
  const parentName = isFrame(element.parentElement) ? (childText(element.parentElement, 'id') || 'its parent') : 'the screen';

  // ---- Rect: the frame as drawn, relative to its parent
  const rect = propSection(panel, 'Rect', 'part', 'Where the frame is drawn, relative to its parent (the screen for one at the top level), and its size - as its layout rules put it');
  const grid = document.createElement('div');
  grid.className = 'prop-grid';
  const setPlace = (across, v) => {
    const { rect: r, rects } = now();
    moveFrameBy(element, across ? v - r.x : 0, across ? 0 : v - r.y, rects);
  };
  const setSize = (across, v) => {
    const name = across ? 'width' : 'height', start = across ? 'left' : 'top', end = across ? 'right' : 'bottom';
    const style = frameStyle(element);
    const { rect: r, parent } = now();
    const whole = across ? parent.w : parent.h;
    const has = style.has(name) && style.get(name).trim().toLowerCase() !== 'auto';
    if (has) setFrameStyle(element, { [name]: formatLength(v, isPercent(style.get(name)) ? whole : 0) });
    else if (inFlow) setFrameStyle(element, { [name]: formatLength(v, 0) });
    else if (style.has(start) && style.has(end)) {
      const e = style.get(end);
      setFrameStyle(element, { [end]: formatLength((parseLength(e, whole) ?? 0) - (v - (across ? r.w : r.h)), isPercent(e) ? whole : 0) });
    } else setChildText(element, name, String(v));
  };
  const field = (label, get, set, tip, options = {}) => {
    const f = scrubNumber(label, get(now()), v => edit('change ' + label, () => set(v)), { title: tip, ...options });
    const refresh = () => {
      const state = now();
      f.set(get(state));
      if (options.off) f.disable(...options.off(state));
    };
    refresh();
    sync.push(refresh);
    return f;
  };
  const flowWhy = `Placed by ${parentName}'s ${LayoutStyle.of(element.parentElement).word('flex-direction')} - its Children layout, or Position: absolute in Layout`;
  const autoWhy = name => `${name} auto: what the frames in it take`;
  grid.append(
    field('X', s => s.rect.x, v => setPlace(true, v), 'Across from the parent\'s left edge', { off: () => [inFlow, flowWhy] }),
    field('Y', s => s.rect.y, v => setPlace(false, v), 'Down from the parent\'s top edge', { off: () => [inFlow, flowWhy] }),
    field('W', s => s.rect.w, v => setSize(true, v), 'Width (0: the text\'s own width)', { min: 0, off: () => [LayoutStyle.of(element).word('width') === 'auto', autoWhy('Width')] }),
    field('H', s => s.rect.h, v => setSize(false, v), 'Height (a text is centred down it)', { min: 0, off: () => [LayoutStyle.of(element).word('height') === 'auto', autoWhy('Height')] }));
  rect.append(grid);

  // Anchor presets, Unity's: they write the edges the frame is held to (left, right, 50% with a
  // translate for the centre, both for a stretch), so it keeps its place as the parent changes size.
  const alignBox = document.createElement('div');
  alignBox.className = 'align-box';
  const presets = document.createElement('div');
  presets.className = 'align-presets';
  let inset = 0;
  const px = v => `${Math.round(v)}px`;
  const place = (h, v) => edit('anchor', () => {
    settleFrame(element);
    const style = frameStyle(element);
    const t = (style.get('translate') || '0 0').split(/\s+/);
    let tx = t[0] || '0', ty = t[1] || '0';
    const s = {};
    if (h === 'left') { s.left = px(inset); s.right = null; tx = '0'; }
    else if (h === 'center') { s.left = '50%'; s.right = null; tx = '-50%'; }
    else if (h === 'right') { s.right = px(inset); s.left = null; tx = '0'; }
    else if (h === 'stretch') { s.left = px(inset); s.right = px(inset); s.width = null; tx = '0'; }
    if (v === 'top') { s.top = px(inset); s.bottom = null; ty = '0'; }
    else if (v === 'middle') { s.top = '50%'; s.bottom = null; ty = '-50%'; }
    else if (v === 'bottom') { s.bottom = px(inset); s.top = null; ty = '0'; }
    else if (v === 'stretch') { s.top = px(inset); s.bottom = px(inset); s.height = null; ty = '0'; }
    s.translate = tx === '0' && ty === '0' ? null : `${tx} ${ty}`;
    setFrameStyle(element, s);
  });
  const anchored = () => {
    const style = frameStyle(element);
    const t = (style.get('translate') || '').split(/\s+/);
    const h = style.has('left') && style.has('right') && !style.has('width') ? 'stretch'
      : (style.get('left') || '').trim() === '50%' && t[0] === '-50%' ? 'center'
      : style.has('left') ? 'left' : style.has('right') ? 'right' : null;
    const v = style.has('top') && style.has('bottom') && !style.has('height') ? 'stretch'
      : (style.get('top') || '').trim() === '50%' && t[1] === '-50%' ? 'middle'
      : style.has('top') ? 'top' : style.has('bottom') ? 'bottom' : null;
    return { h, v };
  };
  const buttons = [];
  for (const v of ['top', 'middle', 'bottom', 'stretch']) {
    for (const h of ['left', 'center', 'right', 'stretch']) {
      const button = document.createElement('button');
      button.className = `align-preset h-${h} v-${v}`;
      button.title = `Anchor: ${v === 'stretch' ? 'the whole height' : v}, ${h === 'stretch' ? 'the whole width' : h} of ${parentName}`;
      button.append(document.createElement('i'));
      button.disabled = inFlow;
      button.onclick = () => place(h, v);
      buttons.push({ button, h, v });
      presets.append(button);
    }
  }
  const markPreset = () => {
    const { h, v } = anchored();
    for (const b of buttons) b.button.classList.toggle('on', b.h === h && b.v === v);
  };
  markPreset();
  sync.push(markPreset);
  const side = document.createElement('div');
  side.className = 'align-side';
  const insetField = scrubNumber('Inset', 0, v => { inset = v; }, { min: 0, title: 'The gap the presets leave to the parent\'s edges' });
  const about = document.createElement('p');
  about.className = 'none';
  const showAbout = () => {
    const { parent } = now();
    const { h, v } = anchored();
    about.textContent = inFlow
      ? `In ${parentName}'s ${LayoutStyle.of(element.parentElement).word('flex-direction')}, which places it.`
      : `In ${parentName}, ${Math.round(parent.w)} \u00d7 ${Math.round(parent.h)}. ${h || v ? `Held to ${[h, v].filter(Boolean).join(', ')}: it stays there as the parent changes size.` : 'Placed by its numbers: an anchor keeps it to an edge.'}`;
  };
  showAbout();
  sync.push(showAbout);
  side.append(insetField, about);
  alignBox.append(presets, side);
  rect.append(alignBox);

  // ---- Layout: the rules that place this frame
  const layout = propSection(panel, 'Layout', 'scene', 'The frame\'s layout rules (its style attribute, Crystal Style Sheets): what holds it to its parent\'s edges, how big it is, its part in a column or row. OpenFF reads them; a Steam or GOG build gets them baked into plain numbers.', false);
  const parentFlows = (() => { const d = LayoutStyle.of(element.parentElement).word('flex-direction'); return d === 'row' || d === 'column'; })();
  if (parentFlows) {
    propRow(layout, 'Position', propSelect([['', `in ${parentName}'s ${LayoutStyle.of(element.parentElement).word('flex-direction')}`], ['absolute', 'absolute (held to its edges)']], frameStyle(element).get('position') === 'absolute' ? 'absolute' : '', v => rebuild('position', () => { settleFrame(element); setFrameStyle(element, { position: v || null }); })), 'A frame in a column or a row follows the others; absolute takes it out, placed by its own edges');
  }
  const whole = across => { const { parent } = now(); return across ? parent.w : parent.h; };
  // What each rule would be to keep the frame where it is (its translate is put on after the edges).
  const measure = prop => {
    const { rect: r, parent } = now();
    const t = LayoutStyle.of(element).translate(r.w, r.h);
    const x = r.x - t.x, y = r.y - t.y;
    return { left: x, top: y, right: parent.w - x - r.w, bottom: parent.h - y - r.h, width: r.w, height: r.h }[prop];
  };
  const isContainer = () => { const d = LayoutStyle.of(element).word('flex-direction'); return d === 'row' || d === 'column'; };
  const ruleRow = (prop, label, across, tip) => {
    const row = document.createElement('div');
    row.className = 'prop-row rule-row';
    const on = document.createElement('input');
    on.type = 'checkbox';
    const name = document.createElement('span');
    name.className = 'prop-label';
    name.textContent = label;
    const value = () => frameStyle(element).get(prop);
    const unitOf = () => { const v = (value() || '').trim().toLowerCase(); return v === 'auto' ? 'auto' : v.endsWith('%') ? '%' : 'px'; };
    const shown = () => {
      const v = value();
      if (v === undefined || unitOf() === 'auto') return Math.round(measure(prop));
      return unitOf() === '%' ? parseFloat(v) : Math.round(parseLength(v, whole(across)) ?? 0);
    };
    const scrub = scrubNumber('', shown(), n => edit('change ' + prop, () => setFrameStyle(element, { [prop]: unitOf() === '%' ? `${n}%` : `${n}px` })), { title: tip, step: 1 });
    const units = [['px', 'px'], ['%', '%']];
    if (prop === 'width' || prop === 'height') units.push(['auto', 'auto']);
    const unit = propSelect(units, unitOf(), u => rebuild('change ' + prop, () => {
      const m = measure(prop);
      setFrameStyle(element, { [prop]: u === 'auto' ? 'auto' : u === '%' ? formatLength(m, whole(across)) : formatLength(m, 0) });
    }));
    unit.className = 'rule-unit';
    on.onchange = () => rebuild((on.checked ? 'set ' : 'clear ') + prop, () => {
      settleFrame(element);
      setFrameStyle(element, { [prop]: on.checked ? `${Math.round(measure(prop))}px` : null });
    });
    const refresh = () => {
      const has = value() !== undefined;
      on.checked = has;
      scrub.set(shown());
      scrub.disable(!has || unitOf() === 'auto', has ? `${label}: auto` : `${label} is not a rule: tick it to hold the frame by it`);
      unit.disabled = !has;
      if (has) unit.value = unitOf();
      row.classList.toggle('unset', !has);
    };
    refresh();
    sync.push(refresh);
    row.title = tip;
    row.append(on, name, scrub, unit);
    layout.append(row);
  };
  if (!inFlow) {
    ruleRow('left', 'Left', true, 'Held this far from the parent\'s left edge');
    ruleRow('right', 'Right', true, 'Held this far from the parent\'s right edge (with Left too and no Width: stretched between them)');
    ruleRow('top', 'Top', false, 'Held this far from the parent\'s top edge');
    ruleRow('bottom', 'Bottom', false, 'Held this far from the parent\'s bottom edge (with Top too and no Height: stretched between them)');
  }
  ruleRow('width', 'Width', true, 'Its width as a rule: units, a percent of the parent\'s, or auto (a column or row: what its frames take)');
  ruleRow('height', 'Height', false, 'Its height as a rule: units, a percent of the parent\'s, or auto');
  if (!inFlow) {
    propRow(layout, 'Translate', propText(frameStyle(element).get('translate') || '', v => edit('change translate', () => setFrameStyle(element, { translate: v.trim() || null })), 'x y, e.g. -50% 0'), 'Moved by this after it is placed; a percent is of its own size (-50% centres it on its left)');
  }
  if (inFlow) {
    const sides = document.createElement('div');
    sides.className = 'prop-grid four';
    for (const [side, letter] of [['left', 'L'], ['top', 'T'], ['right', 'R'], ['bottom', 'B']]) {
      const prop = 'margin-' + side;
      const get = () => Math.round(LayoutStyle.of(element).box('margin', whole(true))[side]);
      const f = scrubNumber(letter, get(), v => edit('change ' + prop, () => setFrameStyle(element, { [prop]: v ? `${v}px` : null })), { title: `margin-${side}: space kept clear on its ${side}` });
      sync.push(() => f.set(get()));
      sides.append(f);
    }
    propRow(layout, 'Margin', sides, 'Space kept clear around it in the column or row');
    const grow = scrubNumber('', LayoutStyle.of(element).number('flex-grow') ?? 0, v => edit('change grow', () => setFrameStyle(element, { 'flex-grow': v ? String(v) : null })), { min: 0, title: 'flex-grow: its share of the space the column or row has left' });
    propRow(layout, 'Grow', grow, 'flex-grow: takes this share of the space left over');
    propRow(layout, 'Align self', propSelect([['', 'as the parent says'], ['stretch', 'stretch'], ['flex-start', 'start'], ['center', 'centre'], ['flex-end', 'end']], LayoutStyle.of(element).word('align-self') || '', v => edit('change align-self', () => setFrameStyle(element, { 'align-self': v || null }))), 'Across the column or row: this frame\'s own alignment');
  }

  // ---- Children: how this frame lays out the frames inside it
  const kids = frameChildren(element).length;
  const children = propSection(panel, 'Children layout', 'table', 'A column or a row: the frames in this one follow each other, gap apart, inside its padding - UI Toolkit\'s flex', isContainer());
  const direction = LayoutStyle.of(element).word('flex-direction') || '';
  propRow(children, 'Direction', propSelect([['', 'none - each by its own rules'], ['column', 'column (down)'], ['row', 'row (across)']], direction === 'row' || direction === 'column' ? direction : '', v => rebuild('change direction', () => {
    for (const kid of frameChildren(element)) settleFrame(kid);
    setFrameStyle(element, { 'flex-direction': v || null });
  })), kids ? `How the ${kids} frame${kids === 1 ? '' : 's'} in it are placed` : 'How frames put in it are placed');
  if (isContainer()) {
    const gap = scrubNumber('', Math.round(LayoutStyle.of(element).length('gap', whole(true)) ?? 0), v => edit('change gap', () => setFrameStyle(element, { gap: v ? `${v}px` : null })), { min: 0, title: 'gap: space between one frame and the next' });
    propRow(children, 'Gap', gap);
    const pads = document.createElement('div');
    pads.className = 'prop-grid four';
    for (const [side, letter] of [['left', 'L'], ['top', 'T'], ['right', 'R'], ['bottom', 'B']]) {
      const prop = 'padding-' + side;
      const get = () => { const r = now().rect; return Math.round(LayoutStyle.of(element).box('padding', r.w)[side]); };
      const f = scrubNumber(letter, get(), v => edit('change ' + prop, () => setFrameStyle(element, { [prop]: v ? `${v}px` : null })), { min: 0, title: `padding-${side}: space inside its ${side} edge` });
      sync.push(() => f.set(get()));
      pads.append(f);
    }
    propRow(children, 'Padding', pads, 'Space inside its edges that the frames keep clear of');
    propRow(children, 'Align items', propSelect([['', 'stretch (fill across)'], ['flex-start', 'start'], ['center', 'centre'], ['flex-end', 'end']], LayoutStyle.of(element).word('align-items') || '', v => edit('change align-items', () => setFrameStyle(element, { 'align-items': v || null }))), 'Across: where the frames sit (stretch: the whole width of a column, the whole height of a row, unless a frame gives its own)');
    propRow(children, 'Justify', propSelect([['', 'start'], ['center', 'centre'], ['flex-end', 'end'], ['space-between', 'space between']], LayoutStyle.of(element).word('justify-content') || '', v => edit('change justify', () => setFrameStyle(element, { 'justify-content': v || null }))), 'Along: where the run of frames sits when there is room left');
  }
}

// ------------------------------------------------------------------ Crystal Style Sheets

/// The panel under the canvas as the stylesheets' editor: its picker has the XML, each of the folder's
/// sheets (styles/*.css) and a new one; a sheet applies as it is typed, and Save writes it.
function wireStylesPanel(node, own) {
  const mode = $('.xml-mode', node);
  const area = $('.xml', node);
  const split = $('.xml-split', node);
  if (!mode || !area) return;
  const sheetOf = m => (menu.sheets || []).find(x => 'css:' + x.name === m);
  const fill = () => {
    mode.textContent = '';
    const add = (value, text) => { const o = document.createElement('option'); o.value = value; o.textContent = text; mode.append(o); };
    add('xml', 'XML');
    for (const sheet of menu.sheets || []) add('css:' + sheet.name, `styles/${sheet.name}${sheet.dirty ? ' •' : ''}`);
    if (own) add('new', '+ New stylesheet');
    mode.value = menu.xmlMode || 'xml';
  };
  const show = () => {
    const m = menu.xmlMode || 'xml';
    const sheet = sheetOf(m);
    split.classList.toggle('css-mode', !!sheet);
    $('.apply', node).hidden = !!sheet;
    const note = $('.xml-note', node);
    if (!sheet) {
      note.innerHTML = 'XML for <strong class="xml-what">the whole file</strong>';
      refreshXmlPanel(node);
      return;
    }
    note.textContent = `styles/${sheet.name} · Crystal Style Sheets, applied as you type; Save writes it`;
    area.value = sheet.css;
  };
  mode.onchange = () => {
    if (mode.value === 'new') {
      // Named after the screen (gambits.css), or numbered when that is taken.
      const stem = (menu.project && menu.project.id) || 'menu';
      let name = stem + '.css';
      for (let n = 2; (menu.sheets || []).some(x => x.name === name); n++) name = `${stem}-${n}.css`;
      const screenName = childText(menuScreen(), 'name') || stem;
      menu.sheets.push({ name, css: `/* Crystal Style Sheets for ${screenName}: #id, .class, text, window, frame - see Docs/Menus.md */\n\n#${screenName} window {\n}\n`, dirty: true });
      menu.sheetsVersion++;
      menu.xmlMode = 'css:' + name;
    } else menu.xmlMode = mode.value;
    if (!menu.xmlOpen) { menu.xmlOpen = true; applyXmlHeight(node); }
    fill();
    show();
    redraw(node, true);
  };
  let timer = null;
  area.addEventListener('input', () => {
    const sheet = sheetOf(menu.xmlMode);
    if (!sheet) return;
    sheet.css = area.value;
    const first = !sheet.dirty;
    sheet.dirty = true;
    menu.sheetsVersion++;
    if (first) fill();
    clearTimeout(timer);
    timer = setTimeout(() => redraw(node, true), 120);
  });
  fill();
  show();
}

/// The inspector's Style section: the frame's classes, its own look (its style attribute's panel, opacity,
/// tint, colour, hidden), and the sheet rules that reach it, the way a browser's devtools list them.
function buildStyleSection(panel, element, screen, edit, rebuild, sync) {
  const body = propSection(panel, 'Style', 'image', 'Crystal Style Sheets: the frame\'s classes, its own look, and the sheets\' rules that reach it', true);
  const computed = () => (menuStyled(frameScreen(element)).computed.get(element)) || new Map();
  const inline = () => frameStyle(element);

  // Its look, copied to other frames or made a class of: the buttons, as the frame's menu has them.
  const tools = document.createElement('div');
  tools.className = 'menu-style-tools';
  for (const item of menuStyleItems(element)) {
    const b = document.createElement('button');
    b.className = 'chip';
    b.textContent = item.label.replace(/\s*\(.*\)$/, '');
    b.title = item.title || item.label;
    b.disabled = !!item.disabled;
    b.onclick = item.run;
    tools.append(b);
  }
  body.append(tools);

  propRow(body, 'Classes', propText(element.getAttribute('class') || '', v => edit('change classes', () => {
    const words = v.trim().split(/\s+/).filter(Boolean).join(' ');
    if (words) element.setAttribute('class', words); else element.removeAttribute('class');
  }), 'row picked …'), 'The frame\'s classes: what .row, .picked … in a sheet pick it out by');

  propRow(body, 'Panel', propSelect([['', 'as its layout and sheets say'], ['window', 'the game\'s window'], ['bar', 'the translucent bar'], ['none', 'none (no panel)']], (inline().get('-ff-panel') || '').trim(), v => rebuild('change panel', () => setFrameStyle(element, { '-ff-panel': v || null }))), '-ff-panel: what is drawn behind the frame');

  // A rule of the frame's own with a switch, as Layout's: off, the sheets' (or nothing) have it.
  const ownRule = (label, prop, control, read, write, tip) => {
    const row = document.createElement('div');
    row.className = 'prop-row rule-row';
    row.title = tip;
    const on = document.createElement('input');
    on.type = 'checkbox';
    const name = document.createElement('span');
    name.className = 'prop-label';
    name.textContent = label;
    on.onchange = () => rebuild((on.checked ? 'set ' : 'clear ') + prop, () => setFrameStyle(element, { [prop]: on.checked ? write(read(computed().get(prop))) : null }));
    const refresh = () => {
      const has = inline().has(prop);
      on.checked = has;
      control.set(read(has ? inline().get(prop) : computed().get(prop)));
      control.disable(!has);
      row.classList.toggle('unset', !has);
    };
    row.append(on, name, control);
    body.append(row);
    refresh();
    sync.push(refresh);
  };
  const percent = v => {
    if (v === undefined) return 100;
    const t = String(v).trim();
    const n = t.endsWith('%') ? parseFloat(t) : Number(t) * 100;
    return Number.isFinite(n) ? Math.round(n) : 100;
  };
  const opacity = scrubNumber('%', 100, n => edit('change opacity', () => setFrameStyle(element, { opacity: `${n}%` })), { min: 0, max: 100, title: 'opacity: 0 (not seen) to 100 (as drawn); a parent\'s multiplies its frames\'' });
  ownRule('Opacity', 'opacity', opacity, percent, n => `${n}%`, 'opacity: the frame\'s panel and text drawn see-through');

  const hexRule = (label, prop, tip) => {
    const input = document.createElement('input');
    input.type = 'color';
    input.oninput = () => edit('change ' + prop, () => setFrameStyle(element, { [prop]: input.value }));
    const wrap = document.createElement('div');
    wrap.className = 'scrub';
    wrap.append(input);
    wrap.set = v => { input.value = styleHex(v) || '#ffffff'; };
    wrap.disable = off => { input.disabled = off; };
    ownRule(label, prop, wrap, v => v, v => styleHex(v) || '#ffffff', tip);
  };
  hexRule('Tint', '-ff-tint', '-ff-tint: the panel\'s art multiplied by this colour');
  if (textMessageId(element) !== null) hexRule('Text colour', 'color', 'color: the text in a colour of its own (a palette word such as pale-blue works in a sheet too)');

  propToggle(body, 'Hidden', (inline().get('visibility') || '').trim() === 'hidden', on => rebuild(on ? 'hide' : 'show', () => setFrameStyle(element, { visibility: on ? 'hidden' : null })), 'visibility: hidden - its text and panel not drawn');

  // The states a sheet's :focus, :focus-within and :disabled ask after - previewed, never saved (the copy the cascade runs on has them).
  const state = menuPreviewState();
  const stateRow = document.createElement('div');
  stateRow.className = 'style-pair preview-state';
  const toggle = (label, on, flip, tip) => {
    const l = document.createElement('label');
    l.className = 'list-check';
    l.title = tip;
    const c = document.createElement('input');
    c.type = 'checkbox';
    c.checked = on;
    c.onchange = () => { flip(c.checked); state.version++; redraw(menu.node, true); };
    l.append(c, label);
    stateRow.append(l);
    return c;
  };
  const focusBox = toggle(':focus', state.focus === element, on => { state.focus = on ? element : state.focus === element ? null : state.focus; }, 'Preview the cursor on this frame: :focus here, :focus-within on the frames round it (the editor\'s only - not saved)');
  const disabledBox = toggle(':disabled', state.disabled.has(element), on => { if (on) state.disabled.add(element); else state.disabled.delete(element); }, 'Preview this frame as one that cannot be taken: :disabled, not :enabled (the editor\'s only - not saved)');
  const clear = document.createElement('button');
  clear.className = 'link';
  clear.textContent = 'none';
  clear.title = 'No previewed states on the screen';
  clear.onclick = () => { state.focus = null; state.disabled.clear(); state.version++; redraw(menu.node, true); };
  stateRow.append(clear);
  sync.push(() => { focusBox.checked = state.focus === element; disabledBox.checked = state.disabled.has(element); });
  propRow(body, 'Preview State', stateRow, 'The pseudo-classes previewed on the canvas; with Play on, a change runs the transitions');

  // The rules that reach it: the sheets' in cascade order, then its own style; what a later one says over is struck through.
  const rules = document.createElement('div');
  rules.className = 'style-rules';
  const unknown = document.createElement('p');
  unknown.className = 'style-unknown';
  const showRules = () => {
    rules.textContent = '';
    const list = frameRules(element).map(r => ({ ...r, inline: false }));
    const own = [...inline()];
    if (own.length) list.push({ selector: 'style=""', sheet: 'the frame', declarations: own, inline: true });
    const winner = new Map();
    list.forEach((r, i) => r.declarations.forEach(([k]) => winner.set(k, i)));
    const odd = new Set();
    list.forEach((r, i) => {
      const card = document.createElement('div');
      card.className = 'style-rule' + (r.inline ? ' inline' : '');
      const head = document.createElement('b');
      head.textContent = r.selector;
      const from = document.createElement('i');
      from.textContent = r.sheet;
      card.append(from, head);
      for (const [k, v] of r.declarations) {
        const line = document.createElement('div');
        line.textContent = `${k}: ${v};`;
        if (winner.get(k) !== i) line.className = 'over';
        if (!STYLE_KNOWN.has(k)) odd.add(k);
        card.append(line);
      }
      rules.append(card);
    });
    if (!list.length) {
      const none = document.createElement('p');
      none.className = 'none';
      none.textContent = (menu.sheets || []).length || screen.ownerDocument.getElementsByTagName('style').length
        ? 'No rule of the sheets reaches this frame.' : 'No stylesheets yet: the panel under the canvas has + New stylesheet.';
      rules.append(none);
    }
    unknown.textContent = odd.size ? `Not Crystal Style Sheets (passed over): ${[...odd].join(', ')}` : '';
  };
  showRules();
  sync.push(showRules);
  body.append(rules, unknown);
}

// ------------------------------------------------------------------ Lettering and Motion

/// Faces to offer for font-family: the game's own, CSS's kinds the client knows, and Windows' common ones.
const LETTERING_FACES = [['', 'as its parents say'], ['game', 'the game\'s face'], ['serif', 'serif (Times New Roman)'], ['monospace', 'monospace (Consolas)'],
  ['"Georgia"', 'Georgia'], ['"Arial"', 'Arial'], ['"Verdana"', 'Verdana'], ['"Tahoma"', 'Tahoma'], ['"Segoe UI"', 'Segoe UI'], ['"Trebuchet MS"', 'Trebuchet MS'],
  ['"Palatino Linotype"', 'Palatino Linotype'], ['"Courier New"', 'Courier New'], ['"Comic Sans MS"', 'Comic Sans MS'], ['"Impact"', 'Impact']];

/// A text's lettering (MenuText): its shadows, face, weight, slant, spacing, line height, lines, case and outline -
/// each the frame's own style, inherited by the texts inside it as CSS inherits them.
function buildLetteringRows(body, element, edit, rebuild, sync) {
  const value = prop => styledValue(element, prop);
  const set = (what, changes) => edit(what, () => setFrameStyle(element, changes));
  const setNow = (what, changes) => rebuild(what, () => setFrameStyle(element, changes));
  const row = sourcedRows(body, element, setNow, sync);
  const word = prop => (value(prop) || '').trim().toLowerCase();

  // ---- the shadows: the game's own drop shadow (nothing said), none at all, or a list of one's own
  const shadowKind = () => value('text-shadow') === undefined ? 'game' : word('text-shadow') === 'none' ? 'none' : 'own';
  const kind = document.createElement('div');
  kind.className = 'segmented';
  const kinds = [['game', 'Game\'s', 'Nothing said: the game\'s own drop shadow, one unit down and right'], ['none', 'None', 'text-shadow: none - not even the game\'s'], ['own', 'Own', 'text-shadow: shadows of its own, the first on top']].map(([v, label, tip]) => {
    const b = document.createElement('button');
    b.textContent = label;
    b.title = tip;
    b.onclick = () => setNow('change text shadow', { 'text-shadow': v === 'game' ? null : v === 'none' ? 'none' : (shadowKind() === 'own' ? value('text-shadow') : '2px 2px 3px #000000') });
    kind.append(b);
    return [v, b];
  });
  const shadows = styleListEditor({
    read: () => {
      const text = value('text-shadow') || '';
      const style = parseTextStyle('text-shadow: ' + (text || 'none'));
      return { text, items: ((style && style.shadows) || []).map(s => ({ x: s.x, y: s.y, blur: s.blur, colour: colourText(s.colour) })) };
    },
    write: items => {
      const text = items.map(s => `${cssPx(s.x)} ${cssPx(s.y)} ${cssPx(s.blur)} ${s.colour}`).join(', ') || 'none';
      set('change text shadow', { 'text-shadow': text });
      return text;
    },
    fresh: () => ({ x: 1, y: 1, blur: 0, colour: '#000000' }),
    fields: [listScrub('X', 'x', { step: 0.5 }), listScrub('Y', 'y', { step: 0.5 }), listScrub('Blur', 'blur', { min: 0, step: 0.5 }), listColour('colour', 'The shadow\'s colour')],
    addLabel: '+ Shadow'
  });
  const shadowBox = document.createElement('div');
  shadowBox.className = 'lettering-shadows';
  shadowBox.append(kind, shadows);
  const showShadows = () => {
    const k = shadowKind();
    kinds.forEach(([v, b]) => b.classList.toggle('on', v === k));
    shadows.hidden = k !== 'own';
    shadows.sync();
  };
  showShadows();
  sync.push(showShadows);
  row('Shadow', shadowBox, 'text-shadow: x y blur colour (menu units), the first on top', ['text-shadow']);

  // ---- the face: a kind, one of Windows', or a face beside the layout (url("fonts/x.ttf"))
  const faceRow = document.createElement('div');
  faceRow.className = 'style-pair';
  const faceNow = () => (value('font-family') || '').trim();
  const face = propSelect(LETTERING_FACES, LETTERING_FACES.some(([v]) => v && v.toLowerCase() === faceNow().toLowerCase()) ? faceNow() : '', v => setNow('change face', { 'font-family': v || null }));
  const faceText = propText(faceNow(), v => set('change face', { 'font-family': v.trim() || null }), 'url("fonts/x.ttf"), "Georgia", serif');
  faceText.title = 'font-family: the faces in order, the first found drawing - url("...") a file beside the screen, serif, monospace, sans-serif or game, or a Windows face by name';
  faceRow.append(face, faceText);
  sync.push(() => { if (document.activeElement !== faceText) faceText.value = faceNow(); });
  row('Face', faceRow, 'font-family', ['font-family']);

  const weight = propSelect([['', 'normal'], ['bold', 'bold'], ['300', '300'], ['500', '500'], ['600', '600 (bold)'], ['700', '700 (bold)'], ['800', '800 (bold)'], ['900', '900 (bold)']], word('font-weight') === 'normal' ? '' : word('font-weight'),
    v => set('change weight', { 'font-weight': v || (sheetValueText(element, 'font-weight') ? 'normal' : null) }));
  row('Weight', weight, 'font-weight: bold, or 600 and over - the face\'s bold if Windows has one, else drawn heavier', ['font-weight']);
  const slant = propSelect([['', 'normal'], ['italic', 'italic'], ['oblique', 'oblique']], word('font-style') === 'normal' ? '' : word('font-style'),
    v => set('change slant', { 'font-style': v || (sheetValueText(element, 'font-style') ? 'normal' : null) }));
  row('Slant', slant, 'font-style: the face\'s italic if Windows has one, else slanted', ['font-style']);

  const spacing = scrubNumber('', bgLength(value('letter-spacing') || '0') ?? 0, v => set('change letter spacing', { 'letter-spacing': v ? cssPx(v) : (sheetValueText(element, 'letter-spacing') ? 'normal' : null) }), { step: 0.25, title: 'letter-spacing: menu units between the letters' });
  sync.push(() => spacing.set(bgLength(value('letter-spacing') || '0') ?? 0));
  row('Spacing', spacing, 'letter-spacing (menu units)', ['letter-spacing']);
  const lineHeight = propText(value('line-height') || '', v => set('change line height', { 'line-height': v.trim() || null }), 'normal, 1.2, 14px');
  lineHeight.title = 'line-height: a text of several lines, its lines this far apart - menu units (14px), or a number times the size (1.2)';
  row('Line Height', lineHeight, 'line-height', ['line-height']);
  const decoration = propSelect([['', 'none'], ['underline', 'underline'], ['line-through', 'line-through'], ['underline line-through', 'both']], word('text-decoration') === 'none' ? '' : word('text-decoration'),
    v => set('change decoration', { 'text-decoration': v || (sheetValueText(element, 'text-decoration') ? 'none' : null), 'text-decoration-line': null }));
  row('Lines', decoration, 'text-decoration: a line under it, or through it', ['text-decoration', 'text-decoration-line']);
  const casing = propSelect([['', 'as written'], ['uppercase', 'UPPERCASE'], ['lowercase', 'lowercase'], ['capitalize', 'Capitalize']], word('text-transform') === 'none' ? '' : word('text-transform'),
    v => set('change case', { 'text-transform': v || (sheetValueText(element, 'text-transform') ? 'none' : null) }));
  row('Case', casing, 'text-transform', ['text-transform']);

  // ---- the outline: width and colour
  const stroke = () => { const s = parseTextStyle('-ff-text-stroke: ' + (value('-ff-text-stroke') || value('-webkit-text-stroke') || '0')); return s ? [s.strokeWidth, colourText(s.strokeColour)] : [0, '#000000']; };
  const writeStroke = (w, c) => set('change outline', { '-ff-text-stroke': w > 0 ? `${cssPx(w)} ${c}` : (sheetValueText(element, '-ff-text-stroke') ? '0' : null), '-webkit-text-stroke': null });
  const strokeRow = document.createElement('div');
  strokeRow.className = 'style-pair';
  const strokeWidth = scrubNumber('W', stroke()[0], v => writeStroke(Math.max(0, v), stroke()[1]), { min: 0, step: 0.5, title: 'The outline\'s width, menu units (0: none)' });
  const strokeColour = colourAlphaField(c => writeStroke(stroke()[0] || 1, c), 'The outline\'s colour');
  strokeRow.append(strokeWidth, strokeColour);
  const showStroke = () => { const [w, c] = stroke(); strokeWidth.set(w); strokeColour.set(c); };
  showStroke();
  sync.push(showStroke);
  row('Outline', strokeRow, '-ff-text-stroke: width colour - the letters outlined (drawn round about, under the text)', ['-ff-text-stroke', '-webkit-text-stroke']);
}

/// The Motion section (MenuAnimation): the frame's transitions - what moves when a value changes, and how - and its
/// animations, each a sheet's @keyframes run over and over or once. Play on the canvas's bar shows them.
function buildMotionSection(panel, element, screen, edit, rebuild, sync) {
  const value = prop => styledValue(element, prop);
  const has = [...TRANSITION_PROPS, ...ANIMATION_PROPS].some(p => value(p) !== undefined);
  const body = propSection(panel, 'Motion', 'logic', 'How the look moves (Crystal Style Sheets): transitions as a value changes - a state previewed in the Style section - and @keyframes animations. Play on the canvas\'s bar runs them.', has);
  const set = (what, changes) => edit(what, () => setFrameStyle(element, changes));
  const setNow = (what, changes) => rebuild(what, () => setFrameStyle(element, changes));
  const row = sourcedRows(body, element, setNow, sync);
  const timings = [['ease', 'ease'], ['linear', 'linear'], ['ease-in', 'ease-in'], ['ease-out', 'ease-out'], ['ease-in-out', 'ease-in-out'], ['step-start', 'step-start'], ['step-end', 'step-end'], ['steps(4, end)', 'steps(4)']];
  const seconds = n => `${cssNumber(n)}s`;
  const props = styleDatalist('motion-properties', ['all', 'opacity', 'color', '-ff-tint', 'background-color', 'background-image', 'border-color', 'border-radius', 'box-shadow', 'text-shadow', 'letter-spacing', '-ff-text-stroke']);

  const transitions = styleListEditor({
    read: () => {
      const map = new Map([...TRANSITION_PROPS].filter(p => value(p) !== undefined).map(p => [p, value(p)]));
      return { text: [...map].map(([k, v]) => `${k}: ${v}`).join('; '), items: animTransitions(map).map(t => ({ property: t.property, duration: t.duration, timing: t.timing, delay: t.delay })) };
    },
    write: items => {
      const text = items.map(t => `${t.property || 'all'} ${seconds(t.duration)}${t.timing && t.timing !== 'ease' ? ' ' + t.timing : ''}${t.delay ? ' ' + seconds(t.delay) : ''}`).join(', ');
      const changes = Object.fromEntries(TRANSITION_PROPS.map(p => [p, null]));
      changes.transition = text || (sheetValueText(element, 'transition') ? 'none' : null);
      set('change transition', changes);
      return [...new Map(Object.entries(changes).filter(([, v]) => v))].map(([k, v]) => `${k}: ${v}`).join('; ');
    },
    fresh: () => ({ property: 'all', duration: 0.3, timing: 'ease', delay: 0 }),
    fields: [listText('property', 'all', 'The property that moves (all: every one; border: its longhands too)', props), listScrub('s', 'duration', { min: 0, step: 0.05, title: 'How long it takes, seconds' }), listSelect('timing', timings, 'How it eases'), listScrub('delay', 'delay', { step: 0.05, title: 'How long it waits first, seconds' })],
    addLabel: '+ Transition',
    empty: 'None: a change shows at once.'
  });
  sync.push(transitions.sync);
  row('Transitions', transitions, 'transition: property duration timing delay, ... - what moves as a value changes (:focus, :disabled) and how', TRANSITION_PROPS);

  // The @keyframes the sheets have, to pick an animation's name from.
  const names = [...menuStyled(frameScreen(element)).keyframes.keys()];
  const nameList = styleDatalist('motion-keyframes', names);
  const animations = styleListEditor({
    read: () => {
      const map = new Map([...ANIMATION_PROPS].filter(p => value(p) !== undefined).map(p => [p, value(p)]));
      return { text: [...map].map(([k, v]) => `${k}: ${v}`).join('; '), items: animAnimations(map).map(a => ({ name: a.name, duration: a.duration, timing: a.timing, delay: a.delay, count: Number.isFinite(a.count) ? String(a.count) : 'infinite', direction: a.direction, fill: a.fill })) };
    },
    write: items => {
      const text = items.filter(a => a.name).map(a => [a.name, seconds(a.duration), a.timing !== 'ease' ? a.timing : '', a.delay ? seconds(a.delay) : '', a.count && a.count !== '1' ? a.count : '', a.direction !== 'normal' ? a.direction : '', a.fill !== 'none' ? a.fill : ''].filter(Boolean).join(' ')).join(', ');
      const changes = Object.fromEntries(ANIMATION_PROPS.map(p => [p, null]));
      changes.animation = text || (sheetValueText(element, 'animation') ? 'none' : null);
      set('change animation', changes);
      return [...new Map(Object.entries(changes).filter(([, v]) => v))].map(([k, v]) => `${k}: ${v}`).join('; ');
    },
    fresh: () => ({ name: names[0] || '', duration: 1, timing: 'ease', delay: 0, count: 'infinite', direction: 'normal', fill: 'none' }),
    fields: [
      listText('name', names.length ? names[0] : '@keyframes name', 'The @keyframes it runs (a sheet\'s)', nameList),
      listScrub('s', 'duration', { min: 0, step: 0.05, title: 'One run, seconds' }),
      listSelect('timing', timings, 'How each run eases'),
      listScrub('delay', 'delay', { step: 0.05, title: 'How long it waits first, seconds' }),
      listText('count', '1', 'How many runs: a number, or infinite', styleDatalist('motion-counts', ['1', '2', '3', 'infinite'])),
      listSelect('direction', [['normal', 'normal'], ['reverse', 'reverse'], ['alternate', 'alternate'], ['alternate-reverse', 'alternate-reverse']], 'Which way each run goes'),
      listSelect('fill', [['none', 'fill: none'], ['forwards', 'forwards'], ['backwards', 'backwards'], ['both', 'both']], 'What it shows before it starts and after it ends')
    ],
    addLabel: '+ Animation',
    empty: names.length ? 'None.' : 'None - and no @keyframes in the sheets yet.'
  });
  sync.push(animations.sync);
  row('Animations', animations, 'animation: name duration timing delay count direction fill-mode, ... - a sheet\'s @keyframes', ANIMATION_PROPS);
  if (names.length) {
    const known = document.createElement('p');
    known.className = 'none';
    known.textContent = `@keyframes in the sheets: ${names.join(', ')}`;
    body.append(known);
  }
}

// ------------------------------------------------------------------ the portrait

/// Where the game draws a screen's portrait of the picked hero (its face, on a tile layer - fixed, its size its own).
const GAME_PORTRAIT = { x: 8, y: 8, w: 56, h: 56 };

/// Whether the game's own portrait shows on the screen: it asks for a hero and has no portrait frame of its own.
function gamePortraitShown(screen) {
  const def = menu.project && menu.project.definition;
  return !!(def && def.characterSelect && screen && !screen.querySelector('frame > portrait'));
}

/// The face Preview shows: the sample hero's (Luneth, the first job), or a party slot's.
function samplePortraitFace(portrait) {
  const slot = parseInt(portrait, 10);
  const hero = Number.isFinite(slot) && slot >= 0 && slot <= 3 ? slot : (BINDING_SAMPLE.hero.id || 0);
  return `files/pc${hero + 1}_01.NCGR`;
}

function buildGamePortraitCard() {
  const panel = document.createElement('div');
  const title = document.createElement('h2');
  title.textContent = 'Portrait';
  const sub = document.createElement('p');
  sub.className = 'sub';
  sub.textContent = 'The game\'s own portrait of the picked hero ("Ask for a hero"): its face, drawn where the game puts it (8, 8, 56 \u00d7 56) and at its own size. A portrait frame of the layout\'s takes its place - wherever you put it, as big as you make it, styled like any frame (opacity, a window or a background behind it).';
  const make = document.createElement('button');
  make.className = 'primary';
  make.textContent = 'Make it a frame';
  make.onclick = () => addMenuFrame(menuScreen(), 'portrait');
  panel.append(title, sub, make);
  return panel;
}

// ------------------------------------------------------------------ the backdrop

/// The backdrop row's right-click menu: seen or not in the view, which of the game's (or none), a picture of the screen's own, new frames.
function backdropMenuItems(screen) {
  const def = menu.project && menu.project.definition;
  const setBackdrop = n => { def.background = n; saveMenuDefinition(); if (menu.node) redraw(menu.node, true); drawHierarchy(); drawInspector(); };
  const items = [
    { label: menu.hideBackdrop ? 'Show in the view' : 'Hide in the view', icon: menu.hideBackdrop ? 'eye' : 'eye-off', run: () => { menu.hideBackdrop = !menu.hideBackdrop; menu.hideScreenBackground = menu.hideBackdrop; if (menu.node) redraw(menu.node, true); drawHierarchy(); } },
    { sep: true }
  ];
  if (def) {
    const now = def.background ?? 10;
    for (const [v, label] of BACKDROPS) {
      items.push({ label: (v === Number(now) ? '✓ ' : ' ') + (v < 0 ? 'No backdrop' : label + ' backdrop'), disabled: v === Number(now), run: () => setBackdrop(v) });
    }
    items.push({ sep: true });
    items.push({ label: 'Screen background picture…', icon: 'image', run: () => pickBackgroundPicture(frameStyle(screen).get('background-image') || '', (chosen, sprite) => {
      menuRemember('screen background');
      setFrameStyle(screen, { 'background-image': chosen, '-ff-sprite': sprite || null, '-ff-background-scale-mode': chosen && !frameStyle(screen).has('-ff-background-scale-mode') ? 'scale-and-crop' : frameStyle(screen).get('-ff-background-scale-mode') || null });
      if (menu.node) redraw(menu.node, true);
      drawHierarchy();
      drawInspector();
    }) });
    if (BACKGROUND_PROPS.some(k => frameStyle(screen).has(k))) items.push({ label: 'No screen background', run: () => {
      menuRemember('no screen background');
      setFrameStyle(screen, Object.fromEntries(BACKGROUND_PROPS.map(k => [k, null])));
      if (menu.node) redraw(menu.node, true);
      drawHierarchy();
      drawInspector();
    } });
    items.push({ sep: true });
  } else {
    items.push({ label: 'The game\'s own backdrop (take the screen into a mod to choose one)', disabled: true, run: () => {} }, { sep: true });
  }
  return items.concat(addMenuItems(screen).map(item => ({ ...item, label: item.label + ' (top level)' })));
}

const BACKDROPS = [[10, 'plain'], [0, 'Item\'s'], [1, 'Magic\'s'], [2, 'Equipment\'s'], [3, 'Status\'s'], [5, 'Job\'s'], [6, 'Config\'s'], [7, 'Quicksave\'s'], [8, 'Save\'s'], [9, 'the main menu\'s'], [13, 'tips, first page'], [14, 'tips, text page'], [-1, 'none']];

function backdropLabel(n) {
  const found = BACKDROPS.find(([v]) => v === Number(n));
  return found ? (found[0] < 0 ? 'no backdrop' : `${found[1]} backdrop`) : `backdrop ${n}`;
}

// The backdrop's row lines (every backdrop draws its rows' separating lines at the heights of the game's own layout):
// "game", "none", or "fit" - laid again at the list's rows as they are (the main menu's commands, with the mods'
// entries). A screen of the mod's or the client's says so in its definition; one of the game's screens in a definition
// of the open project's that reaches it (the game's own file is not the mod's to change).

/// The lines' mode for a screen, as the preview draws them; null for the screen's default.
function backdropLinesFor(screenName) {
  if (menu.project) return (menu.project.definition.backdropLines || '').trim().toLowerCase() || null;
  return gameScreenSetting(screenName, 'backdropLines');
}

/// A setting of the open project's for one of the game's screens (in a definition that reaches it), fetched once and kept; null for none.
function gameScreenSetting(screenName, key) {
  if (!screenName || !(typeof isOpenFFProject === 'function' && isOpenFFProject())) return null;
  menu.gameSettings = menu.gameSettings || {};
  const id = `${key}:${screenName}`;
  if (!(id in menu.gameSettings)) {
    menu.gameSettings[id] = null;
    api(`/api/project/menus/game-setting?file=${encodeURIComponent(shortName(menu.name))}&screen=${encodeURIComponent(screenName)}&key=${encodeURIComponent(key)}`)
      .then(r => { if (r.ok && r.value != null) { menu.gameSettings[id] = r.value; if (menu.node) redraw(menu.node, true); } }).catch(() => {});
  }
  return menu.gameSettings[id];
}

/// A setting of the mod's for one of the game's screens written (null takes it away), kept for the preview.
async function setGameScreenSetting(screenName, key, value) {
  const r = await api('/api/project/menus/game-setting', { file: shortName(menu.name), screen: screenName, key, value: value == null ? null : String(value) });
  if (r.ok) { menu.gameSettings = menu.gameSettings || {}; menu.gameSettings[`${key}:${screenName}`] = value == null ? null : String(value); }
  return r;
}

/// The row lines' switch, on the backdrop's card.
function backdropLinesRow(card, node, screenName) {
  const main = screenName === 'main_menu';
  const options = [['', main ? 'fitted to its rows (the client\'s: the mods\' entries in)' : 'as the game draws them'], ...(main ? [['game', 'as the game draws them']] : []), ['none', 'none']];
  const own = !!menu.project;
  const project = typeof isOpenFFProject === 'function' && isOpenFFProject();
  const pick = propSelect(options, backdropLinesFor(screenName) || '', async v => {
    if (own) {
      if (v) menu.project.definition.backdropLines = v; else delete menu.project.definition.backdropLines;
      saveMenuDefinition();
    } else {
      const r = await setGameScreenSetting(screenName, 'backdropLines', v || null);
      if (!r.ok) return say(r.error, 'bad');
      say(v ? `the mod's ${screenName}: row lines ${v} (menus/${r.file})` : `the mod's ${screenName}: row lines as the screen's own`, 'good');
    }
    redraw(node, true);
  });
  pick.disabled = !own && !project;
  const r = document.createElement('div');
  r.className = 'prop-row';
  const l = document.createElement('span');
  l.className = 'prop-label';
  l.textContent = 'Row lines';
  r.append(l, pick);
  r.title = own ? 'The backdrop\'s lines between its rows ("backdropLines" in the screen\'s definition)'
    : project ? `The backdrop's lines between its rows - a setting of the mod's for the game's ${screenName} (a definition in menus/ that reaches it)`
    : 'Open an OpenFF project: the setting is the mod\'s, kept in its menus/ (the game\'s own file is not the mod\'s to change)';
  card.append(r);
}

/// The backdrop's card: which of the game's (or none), its row lines, and a background of the screen's own over it.
function buildBackdropCard(node) {
  const panel = document.createElement('div');
  const screen = menuScreen();
  const title = document.createElement('h2');
  title.textContent = 'Backdrop';
  const sub = document.createElement('p');
  sub.className = 'sub';
  panel.append(title, sub);
  if (!menu.project) {
    const screenName = childText(screen, 'name');
    const project = typeof isOpenFFProject === 'function' && isOpenFFProject();
    sub.textContent = 'One of the game\'s screens: it picks its backdrop in code. The mod can put another in its place, and say its row lines - both kept in a definition of the mod\'s that reaches the screen (the game\'s own file is not the mod\'s to change).';
    const card = document.createElement('div');
    card.className = 'component';
    const h = document.createElement('div');
    h.className = 'behaviour-header';
    h.textContent = 'The game\'s backdrop';
    card.append(h);
    const said = gameScreenSetting(screenName, 'background');
    const options = [['', 'the screen\'s own'], ...BACKDROPS.map(([v, label]) => [String(v), v < 0 ? 'none (black, or the screen\'s own background)' : label])];
    const pick = propSelect(options, said != null ? String(said) : '', async v => {
      const r = await setGameScreenSetting(screenName, 'background', v === '' ? null : parseInt(v, 10));
      if (!r.ok) return say(r.error, 'bad');
      say(v === '' ? `the mod's ${screenName}: the screen's own backdrop` : `the mod's ${screenName}: backdrop ${v} (menus/${r.file})`, 'good');
      redraw(node, true);
      drawHierarchy();
    });
    pick.disabled = !project;
    const row = document.createElement('div');
    row.className = 'prop-row';
    const label = document.createElement('span');
    label.className = 'prop-label';
    label.textContent = 'Backdrop';
    row.append(label, pick);
    row.title = project ? `"background" in the mod's definition for the game's ${screenName}: the backdrop drawn in place of the one the screen picks`
      : 'Open an OpenFF project: the setting is the mod\'s, kept in its menus/';
    card.append(row);
    backdropLinesRow(card, node, screenName);
    panel.append(card);
    return panel;
  }
  const def = menu.project.definition;
  sub.textContent = 'Under every frame: one of the game\'s menu backdrops, or none - and over it, if you like, a background of the screen\'s own (a picture, a colour, a sprite).';
  const card = document.createElement('div');
  card.className = 'component';
  const h = document.createElement('div');
  h.className = 'behaviour-header';
  h.textContent = 'The game\'s backdrop';
  card.append(h);
  const pick = propSelect(BACKDROPS.map(([v, label]) => [String(v), v < 0 ? 'none (black, or the screen\'s own background)' : label]), String(def.background ?? 10), v => {
    def.background = parseInt(v, 10);
    saveMenuDefinition();
    redraw(node, true);
    drawHierarchy();
  });
  const r = document.createElement('div');
  r.className = 'prop-row';
  const l = document.createElement('span');
  l.className = 'prop-label';
  l.textContent = 'Backdrop';
  r.append(l, pick);
  r.title = 'The screen\'s definition: "background" - saved with it';
  card.append(r);
  backdropLinesRow(card, node, childText(screen, 'name'));
  const hint = document.createElement('p');
  hint.className = 'none';
  hint.textContent = 'These are the game\'s own backdrops, made of pieces of files/menu_bg_01.NCGR (a cell bank), so they are not whole pictures in the picker; its pieces are, as sprites of that sheet (Make sprites\u2026 from the game\'s cells).';
  card.append(hint);
  panel.append(card);
  // The screen's own background: the <menu>'s style.
  const sync = [];
  const edit = (what, change) => { menuRemember(what, what + 'screen'); change(); redraw(node, true); sync.forEach(f => f()); };
  const rebuild = (what, change) => { menuRemember(what); change(); redraw(node, true); drawInspector(); };
  buildBackgroundSection(panel, screen, screen, edit, rebuild, sync, { title: 'Screen background', open: true });
  return panel;
}

// ------------------------------------------------------------------ keys

document.addEventListener('keydown', event => {
  if (typeof state === 'undefined' || state.kind !== 'menu' || !menu.node || !menu.selected) return;
  if (event.target.matches && event.target.matches('input, textarea, select, [contenteditable]')) return;
  if (event.target.closest && event.target.closest('#project')) return;
  const mod = event.ctrlKey || event.metaKey;
  if (mod && event.altKey && event.key.toLowerCase() === 'c') {
    event.preventDefault();
    copyMenuStyle(menu.selected);
  } else if (mod && event.altKey && event.key.toLowerCase() === 'v') {
    event.preventDefault();
    pasteMenuStyle([menu.selected]);
  } else if (mod && event.key.toLowerCase() === 'd') {
    event.preventDefault();
    duplicateMenuFrame(menu.selected);
  } else if (!mod && event.key === 'Delete') {
    event.preventDefault();
    deleteMenuFrame(menu.selected);
  }
});

// ------------------------------------------------------------------ a frame's look: copied, pasted, made a class

// What of a frame's own style is its place and its size, not its look: kept where the look goes on another frame.
const MENU_PLACEMENT = new Set(['position', 'left', 'right', 'top', 'bottom', 'width', 'height', 'min-width', 'min-height', 'max-width', 'max-height',
  'translate', 'margin', 'margin-left', 'margin-top', 'margin-right', 'margin-bottom', 'flex-grow', 'align-self']);
// A text frame's size, alignment and colour, as the mod's and the client's layouts write them (tags beside the style).
const MENU_TEXT_TAGS = ['font', 'align', 'colour'];
let menuStyleClipboard = null;

const menuFrameName = element => childText(element, 'id') || (textMessageId(element) !== null ? 'a text' : 'a frame');
const menuIsText = element => textMessageId(element) !== null;

/// A frame's look: its own style but its place and size, and (a text of a mod's or the client's layout) its size, alignment and colour.
function menuLookOf(element) {
  const style = [...frameStyle(element)].filter(([k]) => !MENU_PLACEMENT.has(k));
  const tags = {};
  if (menu.project && menuIsText(element)) for (const t of MENU_TEXT_TAGS) { const v = childText(element, t); if (v !== null && v !== undefined && String(v).trim() !== '') tags[t] = String(v).trim(); }
  return { style, tags };
}

function menuStyleItems(element) {
  const has = !!menuStyleClipboard;
  return [
    { label: 'Copy style (Ctrl+Alt+C)', title: 'its look - border, background, gradient, shadow, lettering, panel, opacity, motion - to paste on others; not its place or size', run: () => copyMenuStyle(element) },
    { label: 'Paste style (Ctrl+Alt+V)', title: has ? 'the look copied from ' + menuStyleClipboard.from + ' in place of this one\'s (its place and size stay)' : 'copy a frame\'s style first', disabled: !has, run: () => pasteMenuStyle([element]) },
    { label: 'Paste style to…', title: 'the copied look on several frames at once', disabled: !has, run: () => pasteMenuStyleDialog() },
    { label: 'Make a class of its look…', title: 'its look into a stylesheet class: a rule shared by every frame that has the class, changed in one place', run: () => makeMenuClassDialog(element) },
  ];
}

function copyMenuStyle(element) {
  if (!element) return;
  const look = menuLookOf(element);
  menuStyleClipboard = { ...look, from: menuFrameName(element) };
  const n = look.style.length + Object.keys(look.tags).length;
  say(n ? `copied the look of ${menuStyleClipboard.from} (${n} propert${n === 1 ? 'y' : 'ies'})` : `${menuStyleClipboard.from} has no look of its own to copy (its sheets give it its look)`, n ? 'good' : 'warn');
  if (menu.node) redraw(menu.node);   // the Paste buttons come on
}

/// The copied look on frames: their own look replaced by it, their place and size kept.
function pasteMenuStyle(targets) {
  const clip = menuStyleClipboard;
  targets = (targets || []).filter(isFrame);
  if (!clip || !targets.length) return;
  menuRemember(targets.length > 1 ? 'paste the style on ' + targets.length + ' frames' : 'paste the style');
  for (const el of targets) {
    const clear = {};
    for (const [k] of frameStyle(el)) if (!MENU_PLACEMENT.has(k)) clear[k] = null;
    setFrameStyle(el, clear);
    if (clip.style.length) setFrameStyle(el, Object.fromEntries(clip.style));
    if (menu.project && menuIsText(el)) for (const t of MENU_TEXT_TAGS) { if (clip.tags[t] !== undefined) setChildText(el, t, clip.tags[t]); }
  }
  say(`pasted the look of ${clip.from} on ${targets.length === 1 ? menuFrameName(targets[0]) : targets.length + ' frames'}`, 'good');
  redraw(menu.node);
}

/// A checklist of the screen's frames, indented as they nest; `done(chosen)` with the ticked ones.
function menuFrameChecklist(body, { except = null, text = 'Frames' } = {}) {
  const box = document.createElement('div');
  box.className = 'menu-frame-checklist';
  const head = document.createElement('div');
  head.className = 'dialog-section';
  head.textContent = text;
  body.append(head, box);
  const boxes = [];
  const screen = menuScreen();
  const walk = (parent, depth) => {
    for (const el of frameChildren(parent)) {
      if (el !== except) {
        const row = document.createElement('label');
        row.className = 'menu-frame-check';
        row.style.paddingLeft = (6 + depth * 14) + 'px';
        const check = document.createElement('input');
        check.type = 'checkbox';
        const name = document.createElement('span');
        name.textContent = menuFrameName(el);
        const kind = document.createElement('i');
        kind.textContent = menuIsText(el) ? 'text' : (frameStyle(el).get('-ff-panel') || [...el.children].some(c => c.tagName === 'window') ? 'window' : 'frame');
        row.append(check, name, kind);
        box.append(row);
        boxes.push([check, el]);
      }
      walk(el, depth + 1);
    }
  };
  if (screen) walk(screen, 0);
  const all = document.createElement('button');
  all.className = 'mini';
  all.textContent = 'All';
  all.onclick = () => boxes.forEach(([c]) => { c.checked = true; });
  const none = document.createElement('button');
  none.className = 'mini';
  none.textContent = 'None';
  none.onclick = () => boxes.forEach(([c]) => { c.checked = false; });
  head.append(' ', all, none);
  return () => boxes.filter(([c]) => c.checked).map(([, el]) => el);
}

function pasteMenuStyleDialog() {
  if (!menuStyleClipboard) return;
  const body = dialog('Paste the style to…');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  note.textContent = `The look copied from ${menuStyleClipboard.from} goes on every frame ticked, in place of its own look; where each sits and its size stay.`;
  body.append(note);
  const chosen = menuFrameChecklist(body, { text: 'On' });
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Paste';
  go.onclick = () => { const list = chosen(); body.close(); pasteMenuStyle(list); };
  actions.append(go);
  body.append(actions);
}

/// A frame's look made a class: a rule `.name { ... }` in the screen's stylesheet (a layout of the game's: its own
/// <style>), the class given to it and to the frames ticked, and the rule's properties taken off their own style so
/// the class is what they show - one step to undo, the sheet with it.
function makeMenuClassDialog(element) {
  const look = menuLookOf(element);
  const body = dialog('Make a class of its look');
  const note = document.createElement('p');
  note.className = 'dialog-note';
  const where = menu.sheetsFolder ? 'the screens\' stylesheet (styles/)' : 'the layout\'s own <style>';
  note.textContent = look.style.length
    ? `The ${look.style.length} propert${look.style.length === 1 ? 'y' : 'ies'} of ${menuFrameName(element)}'s own look become a class in ${where}. Every frame with the class looks the same, and a change to the class changes all of them.` + (Object.keys(look.tags).length ? ' A text\'s size, alignment and colour stay on the text.' : '')
    : `${menuFrameName(element)} has no look of its own (its sheets give it its look) - give it one first, then make a class of it.`;
  body.append(note);
  if (!look.style.length) return;
  const base = String(childText(element, 'id') || 'look').toLowerCase().replace(/[^a-z0-9_-]+/g, '-').replace(/^-+|-+$/g, '') || 'look';
  const name = field(body, 'Class name', /^[a-z_]/.test(base) ? base : 'look-' + base);
  const others = menuFrameChecklist(body, { except: element, text: 'Give it to these too' });
  const problem = errorLine(body);
  const actions = document.createElement('div');
  actions.className = 'dialog-actions';
  const go = document.createElement('button');
  go.className = 'primary';
  go.textContent = 'Make the class';
  go.onclick = () => {
    const cls = name.value.trim();
    if (!/^[A-Za-z_][A-Za-z0-9_-]*$/.test(cls)) { problem.textContent = 'A class name is letters, digits, - and _, starting with a letter.'; return; }
    const list = [element, ...others()];
    body.close();
    makeMenuClass(cls, look.style, list);
  };
  actions.append(go);
  body.append(actions);
  name.focus();
  name.select();
}

function makeMenuClass(cls, style, frames) {
  menuRemember('make the class .' + cls, null, { sheets: true });
  const rule = `\n.${cls} {\n${style.map(([k, v]) => `  ${k}: ${v};`).join('\n')}\n}\n`;
  if (menu.sheetsFolder || (menu.sheets && menu.sheets.length)) {
    // The mod's or the client's screens: the screen's sheet, or one made for it as + New stylesheet makes it.
    menu.sheets = menu.sheets || [];
    const stem = (menu.project && menu.project.id) || 'menu';
    let sheet = menu.sheets.find(x => x.name === stem + '.css') || menu.sheets[menu.sheets.length - 1];
    if (!sheet) {
      sheet = { name: stem + '.css', css: `/* Crystal Style Sheets: #id, .class, text, window, frame - see Docs/Menus.md */\n`, dirty: true };
      menu.sheets.push(sheet);
    }
    // A class of that name already: the new rule after it, so it wins.
    sheet.css = sheet.css.replace(/\s*$/, '\n') + rule;
    sheet.dirty = true;
    menu.sheetsVersion = (menu.sheetsVersion || 0) + 1;
    menuSheetsShown();
  } else {
    // One of the game's layouts: its own <style>, which the client reads too.
    const doc = menu.doc;
    let style = [...doc.getElementsByTagName('style')][0];
    if (!style) { style = doc.createElement('style'); doc.documentElement.append(style); }
    style.textContent = (style.textContent || '').replace(/\s*$/, '\n') + rule;
  }
  const props = new Set(style.map(([k]) => k));
  for (const el of frames) {
    const words = (el.getAttribute('class') || '').split(/\s+/).filter(Boolean);
    if (!words.includes(cls)) words.push(cls);
    el.setAttribute('class', words.join(' '));
    const clear = {};
    for (const [k] of frameStyle(el)) if (props.has(k)) clear[k] = null;
    setFrameStyle(el, clear);
  }
  // An #id rule outranks a class: say so where one reaches a frame and sets the same.
  const keysOf = d => d instanceof Map ? [...d.keys()] : Array.isArray(d) ? d.map(x => Array.isArray(x) ? x[0] : (x && (x.prop || x.name))) : Object.keys(d || {});
  let beaten = [];
  try { beaten = frames.filter(el => (frameRules(el) || []).some(r => /#/.test(r.selector || '') && keysOf(r.declarations).some(k => props.has(k)))); } catch (e) { beaten = []; }
  say(`made .${cls} and gave it to ${frames.length === 1 ? menuFrameName(frames[0]) : frames.length + ' frames'}` + (menu.sheetsFolder ? ' - Save writes the stylesheet' : '') + (beaten.length ? ` (an #id rule still sets some of it on ${beaten.length})` : ''), 'good');
  redraw(menu.node);
}

/// The stylesheets as the panel under the canvas shows them: the one open there, and the • of one changed.
function menuSheetsShown() {
  if (!menu.node) return;
  const mode = $('.xml-mode', menu.node);
  if (mode) for (const o of mode.options) {
    const sheet = (menu.sheets || []).find(x => 'css:' + x.name === o.value);
    if (sheet) o.textContent = `styles/${sheet.name}${sheet.dirty ? ' •' : ''}`;
  }
  const sheet = (menu.sheets || []).find(x => 'css:' + x.name === menu.xmlMode);
  const text = $('.xml', menu.node);
  if (sheet && text) text.value = sheet.css;
}
