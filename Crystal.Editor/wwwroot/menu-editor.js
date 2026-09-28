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
  if (['data-source', 'bind-text', 'bind-visible', 'bind-class'].some(a => element.hasAttribute(a))) words.push('bound');
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
function menuSnapshot() {
  const picker = menu.node && $('.screens', menu.node);
  let path = null;
  if (menu.selected) {
    path = [];
    for (let e = menu.selected; isFrame(e); e = e.parentElement) path.unshift(frameChildren(e.parentElement).indexOf(e));
  }
  return { xml: new XMLSerializer().serializeToString(menu.doc), screen: picker ? picker.value : 0, path };
}

function menuRestore(snapshot) {
  menu.doc = new DOMParser().parseFromString(snapshot.xml, 'application/xml');
  const found = [...menu.doc.documentElement.children].filter(e => e.tagName === 'menu' || e.tagName === 'unit');
  menu.screens = found.length ? found : [menu.doc.documentElement];
  const picker = $('.screens', menu.node);
  if (picker) picker.value = snapshot.screen;
  let at = menu.screens[snapshot.screen || 0] || null;
  for (const index of snapshot.path || []) at = at ? frameChildren(at)[index] || null : null;
  menu.selected = snapshot.path && isFrame(at) ? at : null;
  redraw(menu.node);
}

/// Records the file as it is before a change. Changes of one kind that follow each other
/// closely (one drag, one field being typed in) share a step, given the same `run`.
function menuRemember(label, run) {
  const doc = activeDoc;
  if (!doc || !menu.doc) return;
  const now = Date.now();
  const last = doc.undo && doc.undo[doc.undo.length - 1];
  if (run && last && last === menu.lastStep && menu.lastRun === run && now - menu.lastAt < 1200) {
    menu.lastAt = now;
    return;
  }
  const before = menuSnapshot();
  let after = null;
  pushUndo(doc, label, () => { after = menuSnapshot(); menuRestore(before); }, () => { if (after) menuRestore(after); });
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
  add('id', uniqueFrameId(screen, kind === 'text' ? 'text1' : kind === 'window' ? 'w_panel1' : 'frame1'));
  add('x', '8');
  add('y', '8');
  add('width', kind === 'text' ? '120' : '160');
  add('height', kind === 'text' ? '24' : '64');
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
  buildBindingsSection(panel, element, screen, edit, rebuild, sync);

  // ---- Frame
  const frameBody = propSection(panel, 'Frame', 'menu', 'What kind of frame it is');
  propToggle(frameBody, 'Window', hasTag(element, 'window'), on => rebuild(on ? 'make a window' : 'not a window', () => {
    if (on) element.prepend(element.ownerDocument.createElement('window'));
    else removeTag(element, 'window');
  }), 'the game\'s window art (Background: a picture of your own)');
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

// ------------------------------------------------------------------ keys

document.addEventListener('keydown', event => {
  if (typeof state === 'undefined' || state.kind !== 'menu' || !menu.node || !menu.selected) return;
  if (event.target.matches && event.target.matches('input, textarea, select, [contenteditable]')) return;
  if (event.target.closest && event.target.closest('#project')) return;
  const mod = event.ctrlKey || event.metaKey;
  if (mod && event.key.toLowerCase() === 'd') {
    event.preventDefault();
    duplicateMenuFrame(menu.selected);
  } else if (!mod && event.key === 'Delete') {
    event.preventDefault();
    deleteMenuFrame(menu.selected);
  }
});
