// Choosing a model, by looking at it.
//
// A dropdown of 145 names tells you nothing about which one is a chest and which is a
// shopkeeper. This is the same list as a wrapped grid of pictures with a filter over
// it, which is the only way to pick something whose name is `o043`.
//
// The pictures come from the same cache the project grid fills, so a model you have
// already seen appears instantly and the rest arrive as they are drawn.

'use strict';

/// Opens the picker. Calls back with the chosen model name, or not at all if it is
/// closed without choosing.
///
/// `options.only` narrows what is offered - a chest can only be an object model, and
/// showing 145 people it cannot be is worse than showing nothing.
function pickModel(current, onChosen, options = {}) {
  const veil = document.createElement('div');
  veil.className = 'picker-veil';

  const box = document.createElement('div');
  box.className = 'picker';

  const head = document.createElement('div');
  head.className = 'picker-head';
  const title = document.createElement('strong');
  title.textContent = options.title || 'Choose a model';
  const filter = document.createElement('input');
  filter.type = 'search';
  filter.placeholder = 'filter';
  filter.autocomplete = 'off';
  const shut = document.createElement('button');
  shut.className = 'shut';
  shut.textContent = '×';
  shut.title = 'close';
  head.append(title, filter, shut);

  const grid = document.createElement('div');
  grid.className = 'picker-grid';

  const foot = document.createElement('p');
  foot.className = 'picker-foot';

  box.append(head, grid, foot);
  veil.append(box);
  document.body.append(veil);

  // The mod's own model files first (an OpenFF project's assets/*.glb - the client draws
  // them directly), then the game's models.
  // `options.assetsOnly` offers the mod's own files alone - a weapon's look on the OpenFF
  // target is a glTF, never one of the game's models.
  const own = (!options.only || options.assetsOnly) && Array.isArray(state.assets)
    ? state.assets.map(a => ({ model: a.name, characterId: 0, from: 'the mod\'s own model file', uses: 0, asset: true }))
    : [];
  let models = own.concat(options.assetsOnly ? [] : (state.placeable || []).filter(
    entry => !options.only || options.only(entry.model)));
  let watcher = null;

  // A model file in: .glb (or a .gltf with its .bin and pictures, picked together) into assets/.
  if ((!options.only || options.assetsOnly) && Array.isArray(state.assets)) {
    const imp = document.createElement('button');
    imp.type = 'button';
    imp.textContent = 'Import a model…';
    imp.title = 'A glTF file (.glb, or .gltf with its .bin and pictures) into the project\'s assets folder; the client draws it directly';
    const chooser = document.createElement('input');
    chooser.type = 'file';
    chooser.multiple = true;
    chooser.accept = '.glb,.gltf,.bin,.png,.jpg,.jpeg';
    chooser.hidden = true;
    imp.onclick = () => chooser.click();
    chooser.onchange = async () => {
      const files = [...(chooser.files || [])];
      if (!files.length) return;
      try {
        const payload = [];
        for (const f of files) {
          const bytes = new Uint8Array(await f.arrayBuffer());
          let bin = '';
          for (let i = 0; i < bytes.length; i += 0x8000) bin += String.fromCharCode.apply(null, bytes.subarray(i, i + 0x8000));
          payload.push({ name: f.name, bytes: btoa(bin) });
        }
        const r = await api('/api/project/assets/import', { files: payload });
        if (!r.ok) throw new Error(r.error);
        state.assets = r.models || [];
        models = state.assets.map(a => ({ model: a.name, characterId: 0, from: 'the mod\'s own model file', uses: 0, asset: true })).concat(options.assetsOnly ? [] : (state.placeable || []));
        say(`${r.files.join(', ')} imported`, 'good');
        draw();
      } catch (e) { say('import: ' + e.message, 'bad'); }
      chooser.value = '';
    };
    head.insertBefore(imp, shut);
    head.append(chooser);
  }

  const draw = () => {
    const wanted = filter.value.trim().toLowerCase();
    grid.textContent = '';
    if (watcher) watcher.disconnect();
    watcher = watchThumbnails(grid, 'model');

    let shown = 0;
    for (const entry of models) {
      if (wanted && !entry.model.toLowerCase().includes(wanted)) continue;
      shown++;

      const cell = document.createElement('button');
      cell.type = 'button';
      cell.className = 'picker-cell' + (entry.model === current ? ' on' : '');
      cell.title = entry.asset ? `${entry.model} · ${entry.from}` : `${entry.model} · id ${entry.characterId} · ${entry.from}`;

      // The package is what the thumbnail maker needs; the model name is what a row holds.
      // A model file of the mod's own is its own package.
      const pkg = entry.asset ? entry.model : `files/${entry.model}.nmdp.lz`;
      cell.dataset.thumbFor = pkg;
      cell.append(icon('model'));

      const label = document.createElement('span');
      label.textContent = entry.asset ? entry.model.replace(/^assets\//, '') : entry.model;
      const note = document.createElement('i');
      note.textContent = entry.asset ? 'the mod\'s' : entry.uses ? `${entry.uses}×` : 'unused';
      cell.append(label, note);

      cell.onclick = () => {
        close();
        onChosen(entry.model);
      };
      grid.append(cell);
      if (watcher) watcher.observe(cell);
    }

    foot.textContent = shown === models.length
      ? `${models.length} ${options.what || 'models can go in a map row'}`
      : `${shown} of ${models.length}`;
  };

  const onKey = (event) => {
    if (event.key === 'Escape') close();
  };

  function close() {
    if (watcher) watcher.disconnect();
    window.removeEventListener('keydown', onKey);
    veil.remove();
  }

  shut.onclick = close;
  veil.onclick = (event) => {
    if (event.target === veil) close();
  };
  filter.oninput = draw;
  window.addEventListener('keydown', onKey);

  draw();
  filter.focus();
}
