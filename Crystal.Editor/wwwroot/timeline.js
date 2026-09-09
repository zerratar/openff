// The timeline dock: a Cutscene component's Timeline laid out as Unity's Timeline window
// lays one out - tracks down the left, clips as blocks on a time ruler, a playhead to scrub,
// the picked clip's arguments in a column on the right. Everything edits the attachment's
// Timeline field in place and goes through sceneChanged(), so autosave and undo (Ctrl+Z)
// apply as to any other change on the map.
//
// Scrubbing previews in the 3D view: object tracks move, turn, scale, fade and hide their
// objects as the clips say, a camera clip moves the view. Dialogue, screen, audio and game
// clips show as a caption. Play in OpenFF exports the mod and starts the client on this map
// with the cutscene playing (--cutscene=<object path>).
//
// The clip types and their arguments mirror OpenFF.Engine/Timeline.cs (Cutscene.Begin);
// the two lists are kept in step by hand.

const TIMELINE_TRACKS = {
  object: { label: 'Object', hint: 'one of the scene\'s objects - a character walks and turns, a model moves' },
  hero: { label: 'Hero', hint: 'the player\'s character' },
  camera: { label: 'Camera', hint: 'where the camera stands and looks' },
  dialogue: { label: 'Dialogue', hint: 'lines said; the playhead waits for each to be dismissed' },
  screen: { label: 'Screen', hint: 'fades and flashes' },
  audio: { label: 'Audio', hint: 'sounds and music' },
  game: { label: 'Game', hint: 'flags, warps, battles, items, signals to code' },
};

const TIMELINE_EASES = ['smooth', 'linear', 'in', 'out'];

// kind: number | int | bool | text | multiline | point | lookat | ease | flags | item | bgm | object
const TIMELINE_CLIPS = {
  move: { label: 'Move', tracks: ['object', 'hero'], colour: '#3d7bd9', length: 1.5, hint: 'Walk (a character, with its walking animation) or glide (a model) to a point',
    args: [{ name: 'to', kind: 'point', label: 'To' }, { name: 'walk', kind: 'bool', label: 'Walk there (a character\'s own walk)', def: true }, { name: 'face', kind: 'bool', label: 'Face the way (a glide)', def: true }, { name: 'ease', kind: 'ease' }] },
  turn: { label: 'Turn', tracks: ['object', 'hero'], colour: '#5b8fd6', length: 0.4, hint: 'Turn to a yaw, or toward the hero, an object or a point',
    args: [{ name: 'yaw', kind: 'number', label: 'Yaw (0 = +Z, 90 = +X)' }, { name: 'at', kind: 'lookat', label: 'Toward (instead of the yaw)' }, { name: 'ease', kind: 'ease' }] },
  motion: { label: 'Motion', tracks: ['object', 'hero'], colour: '#7f6fd8', length: 0, hint: 'A motion by its index in the character\'s set (1001 is the idle)',
    args: [{ name: 'index', kind: 'int', label: 'Motion index', def: 1001 }, { name: 'loop', kind: 'bool', label: 'Loop' }, { name: 'set', kind: 'text', label: 'Bind a motion set first (b_b01, w_light_man…)' }] },
  show: { label: 'Show / hide', tracks: ['object'], colour: '#8a8f9c', length: 0, hint: 'Shown or hidden from here on',
    args: [{ name: 'visible', kind: 'bool', label: 'Visible', def: true }] },
  fade: { label: 'Fade', tracks: ['object'], colour: '#9a9fb0', length: 1, hint: 'A character\'s opacity to a value (0 gone, 100 solid)',
    args: [{ name: 'alpha', kind: 'int', label: 'To alpha (0-100)', def: 0, min: 0, max: 100 }, { name: 'ease', kind: 'ease' }] },
  scale: { label: 'Scale', tracks: ['object'], colour: '#6e9bd1', length: 1, hint: 'Size to a value (1 = as modelled)',
    args: [{ name: 'scale', kind: 'number', label: 'To scale', def: 1 }, { name: 'ease', kind: 'ease' }] },
  clip: { label: 'glTF clip', tracks: ['object'], colour: '#5f8ac9', length: 0, hint: 'An animation of the object\'s glTF (a Mesh) to loop from here; empty stops it',
    args: [{ name: 'name', kind: 'text', label: 'Clip name' }, { name: 'speed', kind: 'number', label: 'Speed', def: 1 }] },
  camera: { label: 'Camera', tracks: ['camera'], colour: '#2f9e8f', length: 2, hint: 'Glide to a position looking at a target; from where it stands, or from a given start',
    args: [{ name: 'position', kind: 'point', label: 'Position' }, { name: 'target', kind: 'point', label: 'Looks at' }, { name: 'from', kind: 'point', label: 'Start position (else where it is)', optional: true }, { name: 'fromTarget', kind: 'point', label: 'Start looks at', optional: true }, { name: 'ease', kind: 'ease' }] },
  follow: { label: 'Follow hero', tracks: ['camera'], colour: '#3fae9f', length: 0, hint: 'The camera goes back on the hero', args: [] },
  shake: { label: 'Shake', tracks: ['camera'], colour: '#39a190', length: 0.5, hint: 'A shake for the clip\'s length',
    args: [{ name: 'strength', kind: 'number', label: 'Strength', def: 1 }, { name: 'speed', kind: 'int', label: 'Speed', def: 2 }] },
  zoom: { label: 'Zoom', tracks: ['camera'], colour: '#48b3a3', length: 0, hint: 'The camera\'s zoom in degrees (0 is the map\'s)',
    args: [{ name: 'degrees', kind: 'int', label: 'Degrees', def: 0 }] },
  say: { label: 'Say', tracks: ['dialogue'], colour: '#d08a2f', length: 1, hint: 'A line in the message window; the playhead waits until it is dismissed',
    args: [{ name: 'text', kind: 'multiline', label: 'Text' }, { name: 'speaker', kind: 'text', label: 'Speaker (a name over the window)' }, { name: 'hold', kind: 'bool', label: 'Wait for the player to dismiss it', def: true }] },
  ask: { label: 'Ask', tracks: ['dialogue'], colour: '#c97c2a', length: 1, hint: 'A yes/no question; flags set either way',
    args: [{ name: 'question', kind: 'multiline', label: 'Question' }, { name: 'yes', kind: 'flags', label: 'Flags on yes' }, { name: 'no', kind: 'flags', label: 'Flags on no' }] },
  fadeOut: { label: 'Fade out', tracks: ['screen'], colour: '#444a57', length: 0.5, hint: 'The screen fades to black (or white) over the clip',
    args: [{ name: 'white', kind: 'bool', label: 'To white' }] },
  fadeIn: { label: 'Fade in', tracks: ['screen'], colour: '#6a7080', length: 0.5, hint: 'The screen comes back over the clip', args: [] },
  flash: { label: 'Flash', tracks: ['screen'], colour: '#c9c9c9', length: 0.3, hint: 'A flash of white (or red)',
    args: [{ name: 'white', kind: 'bool', label: 'White (else red)', def: true }, { name: 'interval', kind: 'int', label: 'Interval (frames)', def: 2 }] },
  se: { label: 'Sound', tracks: ['audio'], colour: '#b3548a', length: 0, hint: 'A sound effect by archive and number',
    args: [{ name: 'archive', kind: 'int', label: 'Archive', def: 1 }, { name: 'number', kind: 'int', label: 'Number', def: 0 }, { name: 'volume', kind: 'int', label: 'Volume (0-127)', def: 127 }] },
  bgm: { label: 'Music', tracks: ['audio'], colour: '#a24b7e', length: 0, hint: 'Music by number, fading in over frames',
    args: [{ name: 'number', kind: 'bgm', label: 'Music' }, { name: 'volume', kind: 'int', label: 'Volume (0-127)', def: 127 }, { name: 'fade', kind: 'int', label: 'Fade in (frames)', def: 0 }] },
  stopBgm: { label: 'Stop music', tracks: ['audio'], colour: '#8e3f6d', length: 0, hint: 'The music fades out',
    args: [{ name: 'fade', kind: 'int', label: 'Fade out (frames)', def: 15 }] },
  flags: { label: 'Flags', tracks: ['game'], colour: '#5f9e4a', length: 0, hint: 'Flags set or cleared ("0:14 !0:11")',
    args: [{ name: 'set', kind: 'flags', label: 'Set' }] },
  wait: { label: 'Wait', tracks: ['game'], colour: '#7a7f8a', length: 1, hint: 'Nothing happens - a pause to space the rest out', args: [] },
  warp: { label: 'Warp', tracks: ['game'], colour: '#4a8f9e', length: 0, hint: 'To another map; the cutscene ends here',
    args: [{ name: 'map', kind: 'text', label: 'Map (t01_01…)' }, { name: 'position', kind: 'point', label: 'Arrive at' }, { name: 'facing', kind: 'int', label: 'Facing', def: 0 }] },
  battle: { label: 'Battle', tracks: ['game'], colour: '#b04a4a', length: 0, hint: 'A battle against a formation; the cutscene ends here',
    args: [{ name: 'formation', kind: 'int', label: 'Formation (monster party)', def: 0 }, { name: 'map', kind: 'int', label: 'Battle map', def: 0 }] },
  item: { label: 'Give item', tracks: ['game'], colour: '#c2a33a', length: 0, hint: 'An item into the party\'s inventory',
    args: [{ name: 'item', kind: 'item', label: 'Item' }, { name: 'count', kind: 'int', label: 'Count', def: 1 }] },
  signal: { label: 'Signal', tracks: ['game'], colour: '#6b8f3a', length: 0, hint: 'A CutsceneSignal by name, for a mod\'s code to hear (Game.Events.Subscribe<CutsceneSignal>)',
    args: [{ name: 'name', kind: 'text', label: 'Name' }] },
};

// The one open panel: which attachment (by target and index among the target's Cutscenes),
// the playhead, the selection, the view's zoom - kept across inspector redraws.
let timelinePanel = null;

/// Opens (or refreshes) the dock for a Cutscene attachment on the map document.
function openTimeline(state, attachment, target) {
  const doc = typeof activeDoc !== 'undefined' ? activeDoc : null;
  const dock = doc && doc.pane ? doc.pane.querySelector('.timeline-dock') : null;
  if (!dock) { say('the timeline needs the map open', 'bad'); return; }
  if (!timelinePanel || timelinePanel.dock !== dock) {
    if (timelinePanel) timelinePanel.close();
    timelinePanel = makeTimelinePanel(dock, doc, state);
  }
  timelinePanel.show(attachment, target);
}

/// Closes the dock if it shows this attachment (its component was removed).
function closeTimelineFor(attachment) {
  if (timelinePanel && timelinePanel.attachment() === attachment) timelinePanel.close();
}

function timelineSummary(timeline) {
  const tracks = (timeline && timeline.tracks) || [];
  const clips = tracks.reduce((n, t) => n + ((t.clips || []).length), 0);
  const length = tracks.reduce((m, t) => Math.max(m, ...(t.clips || []).map(c => (c.start || 0) + (c.length || 0)), 0), 0);
  return `${tracks.length} track${tracks.length === 1 ? '' : 's'}, ${clips} clip${clips === 1 ? '' : 's'}, ${length.toFixed(1)} s`;
}

function makeTimelinePanel(dock, doc, state) {
  let attachment = null;     // the Cutscene attachment being edited
  let target = '';           // its object's path
  let time = 0;              // the playhead, seconds
  let pxPerSecond = 80;
  let selected = null;       // { track, clip }
  let playing = null;        // requestAnimationFrame handle
  let previewing = false;    // the 3D view shows the state at `time`
  let viewWas = null;        // the 3D view before a camera clip moved it
  let drag = null;           // a clip drag in progress
  const SNAP = 0.05;

  dock.hidden = false;
  dock.textContent = '';
  dock.className = 'timeline-dock';
  dock.tabIndex = 0;

  // ---- the bar
  const bar = el('div', 'tl-bar');
  const title = el('b', 'tl-title');
  const playBtn = button('▶', 'Play the preview from the playhead (Space)', togglePlay);
  const stopBtn = button('■', 'Stop and go back to the start', () => { stopPlay(); setTime(0); });
  const clock = el('span', 'tl-clock');
  const zoom = document.createElement('input');
  zoom.type = 'range'; zoom.min = '20'; zoom.max = '400'; zoom.step = '5'; zoom.value = String(pxPerSecond);
  zoom.title = 'Zoom the time axis';
  zoom.oninput = () => { pxPerSecond = Number(zoom.value); drawLanes(); };
  const addTrack = document.createElement('select');
  addTrack.className = 'tl-add-track';
  addTrack.title = 'Add a track';
  const first = document.createElement('option'); first.value = ''; first.textContent = '+ Track…'; addTrack.append(first);
  for (const [kind, meta] of Object.entries(TIMELINE_TRACKS)) {
    const o = document.createElement('option'); o.value = kind; o.textContent = meta.label; o.title = meta.hint; addTrack.append(o);
  }
  addTrack.onchange = () => {
    if (!addTrack.value) return;
    const kind = addTrack.value;
    addTrack.value = '';
    const t = timeline();
    const track = { name: TIMELINE_TRACKS[kind].label + (t.tracks.filter(x => x.kind === kind).length ? ' ' + (t.tracks.filter(x => x.kind === kind).length + 1) : ''), kind, target: kind === 'object' ? '' : undefined, muted: false, clips: [] };
    if (kind !== 'object') delete track.target;
    t.tracks.push(track);
    changed('add track');
    draw();
  };
  const run = button('Play in OpenFF', 'Export the mod and start OpenFF on this map with this cutscene playing', playInOpenFF);
  run.className = 'primary';
  run.hidden = !(typeof openFFProject === 'function' && openFFProject());
  const close = button('×', 'Close the timeline', () => panel.close());
  close.className = 'tl-close';
  const grow = el('span', 'grow');
  bar.append(title, playBtn, stopBtn, clock, zoom, addTrack, grow, run, close);

  // ---- the body: tracks | lanes | properties
  const body = el('div', 'tl-body');
  const tracksCol = el('div', 'tl-tracks');
  const lanesWrap = el('div', 'tl-lanes-wrap');
  const lanes = el('div', 'tl-lanes');
  lanesWrap.append(lanes);
  const props = el('div', 'tl-props');
  body.append(tracksCol, lanesWrap, props);

  // A caption for what the playhead is on that the 3D view cannot show (a line, a fade, a sound).
  const caption = el('div', 'tl-caption');

  // The dock's height, dragged at its top edge.
  const handle = el('div', 'tl-handle');
  handle.title = 'Drag to resize';
  handle.onmousedown = (e) => {
    e.preventDefault();
    const startY = e.clientY, startH = dock.getBoundingClientRect().height;
    const move = (ev) => { dock.style.height = Math.max(160, Math.min(window.innerHeight * 0.7, startH + (startY - ev.clientY))) + 'px'; if (doc.scene3d) doc.scene3d.redraw(); };
    const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
    window.addEventListener('mousemove', move); window.addEventListener('mouseup', up);
  };
  dock.append(handle, bar, body, caption);

  dock.onkeydown = (e) => {
    if (e.target && /^(INPUT|TEXTAREA|SELECT)$/.test(e.target.tagName)) return;
    if (e.key === ' ') { e.preventDefault(); togglePlay(); }
    else if (e.key === 'Delete' || e.key === 'Backspace') { if (selected) { e.preventDefault(); removeClip(selected.track, selected.clip); } }
    else if ((e.key === 'd' || e.key === 'D') && (e.ctrlKey || e.metaKey)) { if (selected) { e.preventDefault(); duplicateClip(selected.track, selected.clip); } }
    else if (e.key === 'Home') { setTime(0); }
    else if (e.key === 'End') { setTime(length()); }
    else if (e.key === 'ArrowLeft') { setTime(Math.max(0, time - (e.shiftKey ? 1 : 1 / 30))); }
    else if (e.key === 'ArrowRight') { setTime(time + (e.shiftKey ? 1 : 1 / 30)); }
  };

  // ------------------------------------------------------------------ the data

  function timeline() {
    if (!attachment) return { tracks: [] };
    attachment.fields = attachment.fields || {};
    let t = attachment.fields.Timeline;
    if (!t || typeof t !== 'object' || Array.isArray(t)) { t = { tracks: [] }; attachment.fields.Timeline = t; }
    if (!Array.isArray(t.tracks)) t.tracks = [];
    for (const track of t.tracks) { if (!Array.isArray(track.clips)) track.clips = []; for (const c of track.clips) { c.args = c.args || {}; c.start = Number(c.start) || 0; c.length = Math.max(0, Number(c.length) || 0); } }
    return t;
  }

  function length() { return Math.max(0, ...timeline().tracks.flatMap(t => t.clips.map(c => c.start + c.length))); }

  function changed(label) {
    if (typeof sceneChanged === 'function') sceneChanged(label);
    // The inspector's summary of the field, if it shows.
    for (const s of document.querySelectorAll('.tl-summary')) s.textContent = timelineSummary(timeline());
  }

  /// After an undo the attachment objects are new: find ours again by target and place.
  function reattach() {
    if (!attachment || typeof sceneState === 'undefined' || !sceneState || !sceneState.attachments) return;
    if (sceneState.attachments.includes(attachment)) return;
    const mine = sceneState.attachments.filter(a => a.behaviour === 'Cutscene' && (a.target || '').toLowerCase() === (target || '').toLowerCase());
    attachment = mine[attachment.__tlIndex || 0] || mine[0] || null;
    if (attachment) attachment.__tlIndex = mine.indexOf(attachment);
    if (!attachment) panel.close();
  }

  // ------------------------------------------------------------------ drawing

  function draw() {
    reattach();
    if (!attachment) return;
    // A selection from before an undo or a rewrite of the field points at clips that are gone.
    if (selected && !timeline().tracks.some(t => t === selected.track && (!selected.clip || t.clips.includes(selected.clip)))) selected = null;
    title.textContent = `Timeline · ${target || 'map'} / Cutscene`;
    drawTracks();
    drawLanes();
    drawProps();
    updateClock();
  }

  function drawTracks() {
    tracksCol.textContent = '';
    const head = el('div', 'tl-track-head');
    head.textContent = 'Tracks';
    tracksCol.append(head);
    const t = timeline();
    if (!t.tracks.length) {
      const none = el('p', 'none');
      none.textContent = 'No tracks yet - add one with + Track… above. An object track walks and turns one of the scene\'s objects; a dialogue track says lines.';
      tracksCol.append(none);
    }
    t.tracks.forEach((track, index) => {
      const row = el('div', 'tl-track' + (selected && selected.track === track ? ' on' : ''));
      const name = document.createElement('input');
      name.className = 'tl-track-name';
      name.value = track.name || TIMELINE_TRACKS[track.kind]?.label || track.kind;
      name.title = TIMELINE_TRACKS[track.kind]?.hint || '';
      name.onchange = () => { track.name = name.value.trim(); changed('rename track'); };
      const kind = el('span', 'tl-kind tl-kind-' + track.kind);
      kind.textContent = TIMELINE_TRACKS[track.kind]?.label || track.kind;
      row.append(name, kind);
      if (track.kind === 'object') {
        const sel = document.createElement('select');
        sel.className = 'tl-target';
        sel.title = 'The object this track acts on';
        const self = document.createElement('option'); self.value = ''; self.textContent = '(this object)'; sel.append(self);
        for (const item of (typeof flattenSceneObjects === 'function' ? flattenSceneObjects(sceneState) : [])) {
          const o = document.createElement('option'); o.value = item.path; o.textContent = ' '.repeat(item.depth * 2) + item.path + (item.model ? '  (' + item.model + ')' : ''); sel.append(o);
        }
        if (track.target && ![...sel.options].some(o => o.value === track.target)) { const gone = document.createElement('option'); gone.value = track.target; gone.textContent = track.target + '  (not on this map)'; sel.append(gone); }
        sel.value = track.target || '';
        sel.onchange = () => { track.target = sel.value; changed('track target'); preview(); };
        row.append(sel);
      }
      const tools = el('span', 'tl-track-tools');
      const mute = button(track.muted ? '🔇' : '🔊', track.muted ? 'Muted - plays nothing; click to unmute' : 'Mute this track', () => { track.muted = !track.muted; changed('mute'); draw(); });
      mute.className = 'icon-button' + (track.muted ? ' off' : '');
      const up = button('↑', 'Move up', () => { if (index > 0) { t.tracks.splice(index, 1); t.tracks.splice(index - 1, 0, track); changed('reorder'); draw(); } });
      const down = button('↓', 'Move down', () => { if (index < t.tracks.length - 1) { t.tracks.splice(index, 1); t.tracks.splice(index + 1, 0, track); changed('reorder'); draw(); } });
      const del = button('×', 'Remove this track and its clips', () => { t.tracks.splice(index, 1); if (selected && selected.track === track) selected = null; changed('remove track'); draw(); preview(); });
      up.className = down.className = del.className = 'icon-button';
      tools.append(mute, up, down, del);
      row.append(tools);
      row.onclick = (e) => { if (e.target === row || e.target === kind) { selected = null; draw(); } };
      tracksCol.append(row);
    });
  }

  function timeToX(s) { return Math.round(s * pxPerSecond); }
  function xToTime(x) { return Math.max(0, x / pxPerSecond); }
  function snap(s) { return Math.round(s / SNAP) * SNAP; }

  function drawLanes() {
    lanes.textContent = '';
    const t = timeline();
    const total = Math.max(length() + 4, 10);
    const width = timeToX(total) + 40;
    lanes.style.width = width + 'px';

    // The ruler.
    const ruler = el('div', 'tl-ruler');
    ruler.style.width = width + 'px';
    const step = pxPerSecond >= 160 ? 0.25 : pxPerSecond >= 60 ? 0.5 : 1;
    for (let s = 0; s <= total + 0.001; s += step) {
      const tick = el('div', 'tl-tick' + (Math.abs(s - Math.round(s)) < 0.001 ? ' major' : ''));
      tick.style.left = timeToX(s) + 'px';
      if (Math.abs(s - Math.round(s)) < 0.001) { const label = el('span'); label.textContent = Math.round(s) + 's'; tick.append(label); }
      ruler.append(tick);
    }
    ruler.onmousedown = (e) => {
      e.preventDefault();
      stopPlay();
      const rect = lanes.getBoundingClientRect();
      const at = (ev) => setTime(xToTime(ev.clientX - rect.left));
      at(e);
      const move = (ev) => at(ev);
      const up = () => { window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up); };
      window.addEventListener('mousemove', move); window.addEventListener('mouseup', up);
    };
    lanes.append(ruler);

    // One lane per track.
    t.tracks.forEach((track) => {
      const lane = el('div', 'tl-lane tl-lane-' + track.kind + (track.muted ? ' muted' : ''));
      lane.style.width = width + 'px';
      lane.ondblclick = (e) => {
        if (e.target !== lane) return;
        const rect = lane.getBoundingClientRect();
        addClip(track, snap(xToTime(e.clientX - rect.left)));
      };
      lane.oncontextmenu = (e) => {
        e.preventDefault();
        const rect = lane.getBoundingClientRect();
        clipMenu(e.clientX, e.clientY, track, snap(xToTime(e.clientX - rect.left)));
      };
      for (const clip of track.clips) {
        const meta = TIMELINE_CLIPS[clip.type] || { label: clip.type, colour: '#666' };
        const block = el('div', 'tl-clip' + (selected && selected.clip === clip ? ' on' : '') + (clip.length <= 0 ? ' instant' : ''));
        block.style.left = timeToX(clip.start) + 'px';
        block.style.width = Math.max(clip.length <= 0 ? 14 : 24, timeToX(clip.length)) + 'px';
        block.style.background = meta.colour;
        block.title = `${meta.label} · ${clip.start.toFixed(2)} s${clip.length > 0 ? ' for ' + clip.length.toFixed(2) + ' s' : ''}\n${clipCaption(track, clip)}`;
        const label = el('span', 'tl-clip-label');
        label.textContent = clipLabel(clip);
        block.append(label);
        if (clip.length > 0) block.append(el('div', 'tl-clip-grip'));
        block.onmousedown = (e) => {
          if (e.button !== 0) return;
          e.preventDefault(); e.stopPropagation();
          selected = { track, clip };
          drawTracks(); drawProps();
          for (const b of lanes.querySelectorAll('.tl-clip.on')) b.classList.remove('on');
          block.classList.add('on');
          const resizing = e.target.classList.contains('tl-clip-grip');
          drag = { clip, track, resizing, startX: e.clientX, start: clip.start, length: clip.length, moved: false };
          const move = (ev) => {
            const dx = ev.clientX - drag.startX;
            if (Math.abs(dx) > 2) drag.moved = true;
            if (drag.resizing) clip.length = Math.max(0, snap(drag.length + dx / pxPerSecond));
            else clip.start = Math.max(0, snap(drag.start + dx / pxPerSecond));
            block.style.left = timeToX(clip.start) + 'px';
            block.style.width = Math.max(clip.length <= 0 ? 14 : 24, timeToX(clip.length)) + 'px';
            block.classList.toggle('instant', clip.length <= 0);
            updateClock();
            drawProps();
          };
          const up = () => {
            window.removeEventListener('mousemove', move); window.removeEventListener('mouseup', up);
            if (drag && drag.moved) { changed(drag.resizing ? 'clip length' : 'clip time'); drawLanes(); preview(); }
            drag = null;
          };
          window.addEventListener('mousemove', move); window.addEventListener('mouseup', up);
        };
        block.oncontextmenu = (e) => { e.preventDefault(); e.stopPropagation(); selected = { track, clip }; draw(); clipMenu(e.clientX, e.clientY, track, clip.start, clip); };
        lane.append(block);
      }
      lanes.append(lane);
    });

    // The playhead.
    const head = el('div', 'tl-playhead');
    head.style.left = timeToX(time) + 'px';
    lanes.append(head);
    const endMark = el('div', 'tl-end');
    endMark.style.left = timeToX(length()) + 'px';
    endMark.title = 'The end: ' + length().toFixed(2) + ' s';
    lanes.append(endMark);
  }

  function clipLabel(clip) {
    const meta = TIMELINE_CLIPS[clip.type];
    const a = clip.args || {};
    switch (clip.type) {
      case 'say': return 'Say: ' + String(a.text || '').split('\n')[0].slice(0, 40);
      case 'ask': return 'Ask: ' + String(a.question || '').split('\n')[0].slice(0, 40);
      case 'motion': return 'Motion ' + (a.index ?? 1001);
      case 'show': return a.visible === false ? 'Hide' : 'Show';
      case 'fade': return 'Fade → ' + (a.alpha ?? 0);
      case 'scale': return 'Scale → ' + (a.scale ?? 1);
      case 'turn': return a.at ? 'Turn → ' + a.at : 'Turn → ' + (a.yaw ?? 0) + '°';
      case 'flags': return 'Flags ' + (a.set || '');
      case 'signal': return 'Signal ' + (a.name || '');
      case 'clip': return 'Clip ' + (a.name || '(stop)');
      case 'se': return `SE ${a.archive ?? 1}:${a.number ?? 0}`;
      case 'bgm': return 'BGM ' + (a.number ?? '');
      case 'warp': return 'Warp → ' + (a.map || '?');
      default: return meta ? meta.label : clip.type;
    }
  }

  function clipCaption(track, clip) {
    const a = clip.args || {};
    const point = p => p && typeof p === 'object' ? `${fmt(p.x)}, ${fmt(p.y)}, ${fmt(p.z)}` : '?';
    switch (clip.type) {
      case 'move': return (track.kind === 'hero' ? 'the hero' : (track.target || 'this object')) + (a.walk === false ? ' glides to ' : ' walks to ') + point(a.to);
      case 'camera': return `camera to ${point(a.position)} looking at ${point(a.target)}`;
      case 'say': return (a.speaker ? a.speaker + ': ' : '') + '"' + (a.text || '') + '"';
      case 'ask': return '"' + (a.question || '') + '"' + (a.yes ? ' yes → ' + a.yes : '') + (a.no ? ' no → ' + a.no : '');
      case 'fadeOut': return 'the screen fades to ' + (a.white ? 'white' : 'black');
      case 'fadeIn': return 'the screen comes back';
      default: return clipLabel(clip);
    }
  }

  function fmt(n) { return Number(n || 0).toFixed(1).replace(/\.0$/, ''); }

  function updateClock() {
    clock.textContent = `${time.toFixed(2)} s / ${length().toFixed(2)} s`;
    const head = lanes.querySelector('.tl-playhead');
    if (head) head.style.left = timeToX(time) + 'px';
    // Keep the playhead in view while playing.
    const x = timeToX(time);
    if (playing && (x < lanesWrap.scrollLeft || x > lanesWrap.scrollLeft + lanesWrap.clientWidth - 40)) lanesWrap.scrollLeft = Math.max(0, x - 60);
  }

  // ------------------------------------------------------------------ clips

  function defaultArgs(type, track, at) {
    const meta = TIMELINE_CLIPS[type];
    const args = {};
    for (const a of meta.args || []) {
      if (a.optional) continue;
      if (a.kind === 'point') {
        if (type === 'camera' && doc.scene3d && doc.scene3d.view) {
          const v = doc.scene3d.view();
          args[a.name] = a.name === 'position' ? { x: r(v.eye[0]), y: r(v.eye[1]), z: r(v.eye[2]) } : { x: r(v.target[0]), y: r(v.target[1]), z: r(v.target[2]) };
        } else {
          const where = trackObject(track);
          args[a.name] = where ? { x: r(where.x), y: r(where.y), z: r(where.z) } : { x: 0, y: 0, z: 0 };
        }
      } else if (a.kind === 'ease') args[a.name] = 'smooth';
      else if (a.def !== undefined) args[a.name] = a.def;
      else if (a.kind === 'bool') args[a.name] = false;
      else if (a.kind === 'int' || a.kind === 'number') args[a.name] = 0;
      else args[a.name] = '';
    }
    return args;
  }

  function r(n) { return Math.round(Number(n || 0) * 10) / 10; }

  /// The object an object track acts on, as the 3D view has it (world numbers); null for the hero or a missing object.
  function trackObject(track) {
    if (!track || track.kind === 'hero') return null;
    const path = track.kind === 'object' ? (track.target || target) : null;
    if (!path) return null;
    return typeof findSceneObject === 'function' ? findSceneObject(path) : null;
  }

  function addClip(track, at, type) {
    const types = Object.keys(TIMELINE_CLIPS).filter(k => TIMELINE_CLIPS[k].tracks.includes(track.kind));
    type = type || types[0];
    if (!type) return;
    const meta = TIMELINE_CLIPS[type];
    const clip = { type, start: Math.max(0, at), length: meta.length, args: defaultArgs(type, track, at) };
    track.clips.push(clip);
    track.clips.sort((a, b) => a.start - b.start);
    selected = { track, clip };
    changed('add ' + type);
    draw();
    preview();
  }

  function removeClip(track, clip) {
    const i = track.clips.indexOf(clip);
    if (i >= 0) track.clips.splice(i, 1);
    if (selected && selected.clip === clip) selected = null;
    changed('remove clip');
    draw();
    preview();
  }

  function duplicateClip(track, clip) {
    const copy = JSON.parse(JSON.stringify(clip));
    copy.start = clip.start + Math.max(clip.length, 0.5);
    track.clips.push(copy);
    track.clips.sort((a, b) => a.start - b.start);
    selected = { track, clip: copy };
    changed('duplicate clip');
    draw();
  }

  /// A right-click menu on a lane (add a clip of each type the track takes) or a clip (edit, duplicate, remove).
  function clipMenu(x, y, track, at, clip) {
    const items = [];
    if (clip) {
      items.push({ label: 'Duplicate (Ctrl+D)', run: () => duplicateClip(track, clip) });
      items.push({ label: 'Set playhead here', run: () => setTime(clip.start) });
      items.push({ label: 'Remove (Delete)', run: () => removeClip(track, clip) });
    } else {
      for (const [type, meta] of Object.entries(TIMELINE_CLIPS)) {
        if (!meta.tracks.includes(track.kind)) continue;
        items.push({ label: '+ ' + meta.label, hint: meta.hint, run: () => addClip(track, at, type) });
      }
    }
    if (typeof showContextMenu === 'function') showContextMenu({ clientX: x, clientY: y }, items);
  }

  // ------------------------------------------------------------------ properties

  function drawProps() {
    props.textContent = '';
    if (!selected || !selected.clip) {
      const head = el('div', 'tl-props-head'); head.textContent = 'Clip';
      const none = el('p', 'none');
      none.textContent = timeline().tracks.length
        ? 'Pick a clip to edit it. Double-click a lane to add one there, right-click for the kinds a track takes; drag a clip to move it, its right edge to change its length. The ruler scrubs; Space plays.'
        : 'Add a track, then clips on it.';
      props.append(head, none);
      return;
    }
    const { track, clip } = selected;
    const meta = TIMELINE_CLIPS[clip.type] || { label: clip.type, args: [] };
    const head = el('div', 'tl-props-head');
    head.textContent = meta.label;
    head.title = meta.hint || '';
    props.append(head);
    if (meta.hint) { const hint = el('p', 'none'); hint.textContent = meta.hint; props.append(hint); }

    const type = document.createElement('select');
    for (const [k, m] of Object.entries(TIMELINE_CLIPS)) { if (!m.tracks.includes(track.kind)) continue; const o = document.createElement('option'); o.value = k; o.textContent = m.label; type.append(o); }
    type.value = clip.type;
    type.onchange = () => { clip.type = type.value; const fresh = defaultArgs(clip.type, track, clip.start); clip.args = Object.assign(fresh, Object.fromEntries(Object.entries(clip.args || {}).filter(([k]) => k in fresh))); if (clip.length === 0 && TIMELINE_CLIPS[clip.type].length > 0) clip.length = TIMELINE_CLIPS[clip.type].length; changed('clip type'); draw(); preview(); };
    row('Type', type);

    const start = number(clip.start, 0.05, v => { clip.start = Math.max(0, v); changed('clip time'); drawLanes(); preview(); });
    row('Start (s)', start);
    const len = number(clip.length, 0.05, v => { clip.length = Math.max(0, v); changed('clip length'); drawLanes(); preview(); });
    row('Length (s)', len, clip.type === 'say' || clip.type === 'ask' ? 'How long the block shows; the playhead waits for the player regardless' : '');

    for (const a of meta.args || []) {
      const value = clip.args[a.name];
      const set = v => { if (v === undefined) delete clip.args[a.name]; else clip.args[a.name] = v; changed('clip ' + a.name); drawLanes(); preview(); };
      let input;
      switch (a.kind) {
        case 'bool': {
          input = document.createElement('input'); input.type = 'checkbox';
          input.checked = value === undefined ? !!a.def : !!value;
          input.onchange = () => set(input.checked);
          break;
        }
        case 'int':
        case 'number': {
          input = number(value === undefined ? (a.def ?? 0) : value, a.kind === 'int' ? 1 : 0.1, v => set(a.kind === 'int' ? Math.round(v) : v));
          if (a.min != null) input.min = a.min;
          if (a.max != null) input.max = a.max;
          break;
        }
        case 'multiline': {
          input = document.createElement('textarea'); input.rows = 3; input.value = value || '';
          input.oninput = () => set(input.value);
          break;
        }
        case 'ease': {
          input = document.createElement('select');
          for (const e of TIMELINE_EASES) { const o = document.createElement('option'); o.value = e; o.textContent = e; input.append(o); }
          input.value = value || 'smooth';
          input.onchange = () => set(input.value);
          break;
        }
        case 'point': {
          input = pointField(value, a, track, clip, set);
          break;
        }
        case 'lookat': {
          input = document.createElement('select');
          const none = document.createElement('option'); none.value = ''; none.textContent = '(use the yaw)'; input.append(none);
          const hero = document.createElement('option'); hero.value = '@hero'; hero.textContent = '@hero - the player'; input.append(hero);
          for (const item of (typeof flattenSceneObjects === 'function' ? flattenSceneObjects(sceneState) : [])) { const o = document.createElement('option'); o.value = item.path; o.textContent = item.path; input.append(o); }
          if (value && ![...input.options].some(o => o.value === value)) { const o = document.createElement('option'); o.value = value; o.textContent = value; input.append(o); }
          input.value = value || '';
          input.onchange = () => set(input.value || undefined);
          break;
        }
        case 'item': {
          input = document.createElement('select');
          const none = document.createElement('option'); none.value = '0'; none.textContent = '(none)'; input.append(none);
          const fill = () => { for (const entry of state.items || []) { const o = document.createElement('option'); o.value = entry.id; o.textContent = `${entry.name} · ${entry.category}`; input.append(o); } input.value = String(value || 0); };
          if (state.items) fill(); else api('/api/items').then(items => { state.items = items; fill(); }).catch(() => {});
          input.onchange = () => set(parseInt(input.value, 10) || 0);
          break;
        }
        case 'bgm': {
          input = document.createElement('select');
          const none = document.createElement('option'); none.value = '0'; none.textContent = '(none)'; input.append(none);
          const fill = () => {
            for (const sound of (state.audio || []).filter(s => s.kind === 'bgm')) {
              const n = parseInt(String(sound.name).replace(/^BGM/i, ''), 10);
              if (isNaN(n)) continue;
              const o = document.createElement('option'); o.value = String(n); o.textContent = `${n} · ${sound.name}`; input.append(o);
            }
            if (value && ![...input.options].some(o => o.value === String(value))) { const o = document.createElement('option'); o.value = String(value); o.textContent = String(value); input.append(o); }
            input.value = String(value || 0);
          };
          if (state.audio) fill(); else api('/api/audio').then(list => { state.audio = list; fill(); }).catch(() => { fill(); });
          input.onchange = () => set(parseInt(input.value, 10) || 0);
          break;
        }
        default: {
          input = document.createElement('input'); input.type = 'text'; input.value = value == null ? '' : value;
          if (a.kind === 'flags') input.placeholder = 'e.g. 0:14 !0:11';
          input.oninput = () => set(input.value);
        }
      }
      row(a.label || a.name, input);
    }
    const tools = el('div', 'tl-props-tools');
    const dup = button('Duplicate', 'A copy after this one (Ctrl+D)', () => duplicateClip(track, clip));
    const del = button('Remove', 'Remove this clip (Delete)', () => removeClip(track, clip));
    tools.append(dup, del);
    props.append(tools);

    function row(labelText, input, hint) {
      const label = el('label', 'tl-prop');
      const span = el('span'); span.textContent = labelText; if (hint) label.title = hint;
      label.append(span, input);
      props.append(label);
    }
  }

  function number(value, step, onChange) {
    const input = document.createElement('input');
    input.type = 'number'; input.step = String(step); input.value = value == null ? '' : String(Math.round(Number(value) * 1000) / 1000);
    input.oninput = () => { const v = Number(input.value); if (!isNaN(v)) onChange(v); };
    return input;
  }

  /// x, y, z with "from the selection", "from the view" and "pick on the ground".
  function pointField(value, a, track, clip, set) {
    const wrap = el('div', 'tl-point');
    const current = Object.assign({ x: 0, y: 0, z: 0 }, value || {});
    const parts = el('div', 'tl-point-parts');
    for (const part of ['x', 'y', 'z']) {
      const n = number(current[part], 0.5, v => { current[part] = v; set(Object.assign({}, current)); });
      n.placeholder = part; n.title = part;
      parts.append(n);
    }
    const tools = el('div', 'tl-point-tools');
    const fromSel = button('← selection', 'The selected object\'s spot', () => {
      const sel = doc.selection || '';
      let at = null;
      if (sel.startsWith('scene:')) { const f = findSceneObject(sel.slice(6)); if (f) at = [f.x, f.y, f.z]; }
      else if (sel.startsWith('object:') && typeof mapState !== 'undefined') { const c = mapState.data.characters.find(ch => ch.index === Number(sel.slice(7))); if (c) at = [c.x, c.y, c.z]; }
      if (!at) { say('select an object in the hierarchy or the 3D view first', 'bad'); return; }
      set({ x: r(at[0]), y: r(at[1]), z: r(at[2]) }); drawProps();
    });
    const fromView = button('← view', clip.type === 'camera' ? (a.name === 'position' || a.name === 'from' ? 'Where the 3D view stands' : 'What the 3D view looks at') : 'The point the 3D view looks at', () => {
      if (!doc.scene3d || !doc.scene3d.view) return;
      const v = doc.scene3d.view();
      const p = clip.type === 'camera' && (a.name === 'position' || a.name === 'from') ? v.eye : v.target;
      set({ x: r(p[0]), y: r(p[1]), z: r(p[2]) }); drawProps();
    });
    const pick = button('pick…', 'Click a spot on the ground in the 3D view', () => {
      if (typeof placeOnMap !== 'function') return;
      const node = doc.pane.querySelector('.view');
      placeOnMap(node, a.label || a.name, (x, y, z) => { set({ x: r(x), y: r(y), z: r(z) }); drawProps(); });
    });
    tools.append(fromSel, fromView, pick);
    if (a.optional) {
      const clear = button('none', 'Leave it out (where it already is)', () => { set(undefined); drawProps(); });
      tools.append(clear);
      if (value === undefined) { parts.classList.add('unset'); for (const n of parts.querySelectorAll('input')) n.value = ''; }
    }
    wrap.append(parts, tools);
    return wrap;
  }

  // ------------------------------------------------------------------ the playhead and the preview

  function setTime(t) {
    time = Math.max(0, t);
    updateClock();
    preview();
  }

  function togglePlay() {
    if (playing) { stopPlay(); return; }
    if (time >= length()) time = 0;
    let last = performance.now();
    playBtn.textContent = '❚❚';
    const tick = (now) => {
      const dt = Math.min(0.1, (now - last) / 1000);
      last = now;
      time += dt;
      if (time >= length()) { time = length(); updateClock(); preview(); stopPlay(); return; }
      updateClock();
      preview();
      playing = requestAnimationFrame(tick);
    };
    playing = requestAnimationFrame(tick);
  }

  function stopPlay() {
    if (playing) cancelAnimationFrame(playing);
    playing = null;
    playBtn.textContent = '▶';
  }

  function ease(kind, p) {
    p = Math.max(0, Math.min(1, p));
    switch (kind) { case 'linear': return p; case 'in': return p * p; case 'out': return 1 - (1 - p) * (1 - p); default: return p * p * (3 - 2 * p); }
  }

  function lerpYaw(a, b, p) { const d = ((b - a) % 360 + 540) % 360 - 180; return a + d * p; }

  /// The scene as the timeline has it at `time`, into the 3D view.
  function preview() {
    if (!doc.scene3d || typeof flattenSceneObjects !== 'function') return;
    const list = flattenSceneObjects(sceneState);
    const byPath = new Map(list.map(i => [i.path.toLowerCase(), i]));
    const t = timeline();
    const captions = [];
    let camera = null;
    let touched = false;
    for (const track of t.tracks) {
      if (track.muted) continue;
      const clips = [...track.clips].sort((a, b) => a.start - b.start);
      if (track.kind === 'object') {
        const path = (track.target || target || '').toLowerCase();
        const item = byPath.get(path);
        if (!item) continue;
        for (const clip of clips) {
          if (clip.start > time) break;
          const p = clip.length > 0 ? Math.min(1, (time - clip.start) / clip.length) : 1;
          const e = ease(clip.args.ease, p);
          switch (clip.type) {
            case 'move': {
              const to = clip.args.to || {};
              const from = { x: item.x, y: item.y, z: item.z };
              item.x = from.x + ((to.x || 0) - from.x) * e; item.y = from.y + ((to.y || 0) - from.y) * e; item.z = from.z + ((to.z || 0) - from.z) * e;
              const dx = (to.x || 0) - from.x, dz = (to.z || 0) - from.z;
              if ((clip.args.face !== false || clip.args.walk !== false) && Math.hypot(dx, dz) > 0.01) item.rotationY = Math.atan2(dx, dz) * 180 / Math.PI;
              touched = true;
              break;
            }
            case 'turn': {
              let toYaw = Number(clip.args.yaw) || 0;
              if (clip.args.at) {
                const other = clip.args.at === '@hero' ? null : byPath.get(String(clip.args.at).toLowerCase());
                if (other) toYaw = Math.atan2(other.x - item.x, other.z - item.z) * 180 / Math.PI;
                else if (/^-?[\d.]+,-?[\d.]+,-?[\d.]+$/.test(clip.args.at)) { const [ax, , az] = clip.args.at.split(',').map(Number); toYaw = Math.atan2(ax - item.x, az - item.z) * 180 / Math.PI; }
                else toYaw = item.rotationY;
              }
              item.rotationY = lerpYaw(item.rotationY || 0, toYaw, e);
              touched = true;
              break;
            }
            case 'scale': { const from = item.scale || 1; item.scale = from + ((Number(clip.args.scale) || 1) - from) * e; touched = true; break; }
            case 'fade': { const from = item.alpha == null ? 1 : item.alpha; item.alpha = from + ((Number(clip.args.alpha ?? 0) / 100) - from) * e; touched = true; break; }
            case 'show': { item.hidden = clip.args.visible === false; touched = true; break; }
          }
        }
      } else if (track.kind === 'camera') {
        for (const clip of clips) {
          if (clip.start > time) break;
          const p = clip.length > 0 ? Math.min(1, (time - clip.start) / clip.length) : 1;
          const e = ease(clip.args.ease, p);
          if (clip.type === 'camera') {
            const to = clip.args.position || { x: 0, y: 0, z: 0 }, at = clip.args.target || { x: 0, y: 0, z: 0 };
            const from = clip.args.from || (camera ? camera.eye : to), fromAt = clip.args.fromTarget || (camera ? camera.target : at);
            camera = {
              eye: { x: from.x + (to.x - from.x) * e, y: from.y + (to.y - from.y) * e, z: from.z + (to.z - from.z) * e },
              target: { x: fromAt.x + (at.x - fromAt.x) * e, y: fromAt.y + (at.y - fromAt.y) * e, z: fromAt.z + (at.z - fromAt.z) * e }
            };
          } else if (clip.type === 'follow') camera = null;
        }
      } else {
        for (const clip of clips) {
          if (clip.start > time || time > Math.max(clip.start + clip.length, clip.start + 0.75)) continue;
          captions.push(clipCaption(track, clip));
        }
      }
    }
    doc.scene3d.setPoints(list);
    previewing = touched || camera !== null || previewing;
    if (camera) {
      const was = doc.scene3d.lookFrom([camera.eye.x, camera.eye.y, camera.eye.z], [camera.target.x, camera.target.y, camera.target.z]);
      if (!viewWas) viewWas = was;
    } else if (viewWas) { doc.scene3d.lookBack(viewWas); viewWas = null; }
    caption.textContent = captions.join('  ·  ');
    caption.hidden = captions.length === 0;
  }

  /// The 3D view back to the scene as saved.
  function endPreview() {
    if (viewWas && doc.scene3d) { doc.scene3d.lookBack(viewWas); viewWas = null; }
    if (typeof syncSceneObjects === 'function') syncSceneObjects(doc);
    previewing = false;
    caption.hidden = true;
  }

  async function playInOpenFF() {
    if (typeof runInOpenFF !== 'function' || typeof mapState === 'undefined') return;
    const where = findSceneObject(target);
    await runInOpenFF({ map: mapState.name, pos: where ? [where.x, where.y, where.z] : null, cutscene: target });
  }

  // ------------------------------------------------------------------ helpers

  function el(tag, className) { const n = document.createElement(tag); if (className) n.className = className; return n; }
  function button(text, titleText, onClick) { const b = document.createElement('button'); b.textContent = text; b.title = titleText; b.onclick = onClick; return b; }

  const panel = {
    dock,
    attachment: () => attachment,
    show(a, t) {
      attachment = a; target = t || '';
      const mine = (sceneState.attachments || []).filter(x => x.behaviour === 'Cutscene' && (x.target || '').toLowerCase() === target.toLowerCase());
      attachment.__tlIndex = Math.max(0, mine.indexOf(attachment));
      selected = null;
      dock.hidden = false;
      draw();
      preview();
      if (doc.scene3d) doc.scene3d.redraw();
    },
    refresh() { if (!dock.hidden) { draw(); preview(); } },
    close() {
      stopPlay();
      endPreview();
      dock.hidden = true;
      dock.textContent = '';
      attachment = null;
      if (timelinePanel === panel) timelinePanel = null;
      if (doc.scene3d) doc.scene3d.redraw();
    }
  };
  return panel;
}
