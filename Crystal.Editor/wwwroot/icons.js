// The icon set.
//
// Every asset kind gets a mark, and so does every kind of thing inside one. They are
// drawn rather than fetched: ten small shapes as inline SVG cost nothing next to a
// sprite sheet and a request for it, and because they are stroked in `currentColor`
// they take the colour of whatever row they are sitting in - dimmed in a list, bright
// when selected, without a second copy of each file.
//
// All of them are 16x16 on the same grid, stroked at 1.3 and rounded, so they sit
// together at the small sizes the tabs and the trees use them at.

'use strict';

const ICONS = {
  // A folded map, panels and creases.
  map: '<path d="M1.6 3.7 5.6 2.2l4.8 1.7 4-1.5v9.9l-4 1.5-4.8-1.7-4 1.5z"/>'
     + '<path d="M5.6 2.2v9.9M10.4 3.9v9.9"/>',

  // A page with code brackets on it.
  script: '<path d="M4 1.8h5l3 3v9.4H4z"/><path d="M9 1.8v3h3"/>'
        + '<path d="m7 8.3-1.3 1.5L7 11.3M9.4 8.3l1.3 1.5-1.3 1.5"/>',

  // A window: title bar, then rows.
  menu: '<rect x="1.8" y="2.6" width="12.4" height="10.8" rx="1.2"/>'
      + '<path d="M1.8 5.6h12.4M4.4 8.4h7.2M4.4 10.9h4.6"/>',

  // A speech bubble - this is the game's words, not a spreadsheet.
  text: '<path d="M2 3.4h12v7.6H7.4L4.2 14v-3H2z"/><path d="M4.6 6.2h6.8M4.6 8.5h4.4"/>',

  // A grid with a heading row.
  table: '<rect x="1.8" y="2.6" width="12.4" height="10.8" rx="1"/>'
       + '<path d="M1.8 6h12.4M1.8 9.7h12.4M6.6 6v7.4M10.6 6v7.4"/>',

  // A picture: horizon, sun, hill.
  image: '<rect x="1.8" y="2.8" width="12.4" height="10.4" rx="1.2"/>'
       + '<circle cx="5.6" cy="6.3" r="1.2"/><path d="m1.8 11.6 3.5-3 3 2.6 2.4-2 3.5 3.1"/>',

  // A swatch, half of it checked.
  texture: '<rect x="1.8" y="2.8" width="12.4" height="10.4" rx="1.2"/>'
         + '<path d="M1.8 8h6.2v5.2H1.8zM8 2.8h6.2V8H8z" fill="currentColor" stroke="none"'
         + ' opacity=".55"/>',

  // A box seen corner on.
  model: '<path d="M8 1.9l5.6 3v6.2L8 14.1 2.4 11.1V4.9z"/>'
       + '<path d="M2.4 4.9 8 7.9l5.6-3M8 7.9v6.2"/>',

  // Cut marks round a sprite.
  cell: '<path d="M1.9 5V2.9h2.2M11.9 2.9h2.2V5M14.1 11v2.1h-2.2M4.1 13.1H1.9V11"/>'
      + '<rect x="5.1" y="5.1" width="5.8" height="5.8" rx=".8"/>',

  // A speaker and one wave.
  audio: '<path d="M2.4 6.1h2.5l3.3-2.8v9.4L4.9 9.9H2.4z"/>'
       + '<path d="M10.9 6.2a3.2 3.2 0 0 1 0 3.6M12.9 4.4a5.8 5.8 0 0 1 0 7.2"/>',

  // ------------------------------------------------------------ the mod's own

  // A folder with a sharp on it: the OpenFF mod's code and data.
  mod: '<path d="M1.8 4.2h4.2l1.4 1.6h6.8v7.8H1.8z"/>'
     + '<path d="M7.6 9h4.2M7.6 11.2h4.2M8.9 7.8v4.8M10.5 7.8v4.8"/>',

  // A game pad: a Steam mod, the game's own files replaced.
  steam: '<path d="M4.6 4.6h6.8a3 3 0 0 1 3 3l-.6 4.2a1.6 1.6 0 0 1-2.9.6L9.9 11H6.1l-1 1.4a1.6 1.6 0 0 1-2.9-.6L1.6 7.6a3 3 0 0 1 3-3z"/>'
       + '<path d="M5.2 6.6v2.6M3.9 7.9h2.6"/><circle cx="11" cy="7.2" r=".7"/><circle cx="12.4" cy="8.6" r=".7"/>',

  // A page with a sharp: a C# source file.
  code: '<path d="M4 1.8h5l3 3v9.4H4z"/><path d="M9 1.8v3h3"/>'
      + '<path d="M6 9.2h4.2M6 11.4h4.2M7.3 8v4.6M8.9 8v4.6"/>',

  // A sharp in a rounded frame: a Behaviour class, something attached to a map object.
  behaviour: '<rect x="2" y="2" width="12" height="12" rx="2.4"/>'
           + '<path d="M5.2 6.6h5.6M5.2 9.4h5.6M6.8 4.6v6.8M9.2 4.6v6.8"/>',

  // A potion bottle: an item definition, the mod's own item.
  item: '<path d="M6.4 1.8h3.2v2.6l2.6 3.2v5.4a1.4 1.4 0 0 1-1.4 1.4H5.2a1.4 1.4 0 0 1-1.4-1.4V7.6l2.6-3.2z"/>'
      + '<path d="M4.2 9.4h7.6"/>',

  // A cog: the GameService, the mod's engine room.
  service: '<circle cx="8" cy="8" r="2.4"/>'
         + '<path d="M8 1.8v2.1M8 12.1v2.1M1.8 8h2.1M12.1 8h2.1M3.6 3.6l1.5 1.5M10.9 10.9l1.5 1.5M3.6 12.4l1.5-1.5M10.9 5.1l1.5-1.5"/>',

  // A map with a pin on it: a scene file, the mod's behaviours placed on a map.
  scene: '<path d="M1.6 4.7 5.6 3.2l4.8 1.7 4-1.5v8.9l-4 1.5-4.8-1.7-4 1.5z"/>'
       + '<path d="M5.6 3.2v8.9M10.4 4.9v8.9"/>'
       + '<circle cx="12.2" cy="3.6" r="1.9"/><path d="M12.2 5.5v3"/>',

  // ------------------------------------------------------------ inside a map

  // Layered ground.
  terrain: '<path d="m1.9 9.4 6.1-3 6.1 3-6.1 3z"/><path d="m1.9 12.2 6.1 3 6.1-3"/>'
         + '<path d="m4.6 5.2 3.4-1.7 3.4 1.7"/>',

  // Somebody standing there.
  character: '<circle cx="8" cy="4.6" r="2.4"/>'
           + '<path d="M3.4 14.1a4.6 4.6 0 0 1 9.2 0"/>',

  // Braces: this row is code, not scenery.
  logic: '<path d="M6.2 2.4c-1.8 0-1.8 4.6-3 4.6h-.9c1.2 0 1.2 6.6 3 6.6"/>'
       + '<path d="M9.8 2.4c1.8 0 1.8 4.6 3 4.6h.9c-1.2 0-1.2 6.6-3 6.6"/>',

  // A doorway with a way out of it.
  exit: '<path d="M9.6 2.2H3.4v11.6h6.2"/><path d="M7.4 8h6.4M11.4 5.6 13.8 8l-2.4 2.4"/>',

  // A triangle of a mesh.
  part: '<path d="M8 2.4 14 12.9H2z"/><path d="M8 2.4v10.5M5 7.6h6"/>',

  // ----------------------------------------------------------------- fallback

  file: '<path d="M4 1.8h5l3 3v9.4H4z"/><path d="M9 1.8v3h3"/>'
};
// The mod's text is text: the same bubble as the game's.
ICONS.strings = ICONS.text;
// A bone: a shaft with knobs at both ends.
ICONS.bone = '<path d="M5 5 11 11"/><circle cx="4" cy="4" r="1.7"/><circle cx="12" cy="12" r="1.7"/>';
// The model view's toolbox.
ICONS.orbit = '<circle cx="8" cy="8" r="5.2"/><path d="M2.8 8h10.4M8 2.8c-2.4 2.6-2.4 7.8 0 10.4M8 2.8c2.4 2.6 2.4 7.8 0 10.4"/>';
ICONS.wireframe = '<path d="M2.5 4.5 8 2l5.5 2.5v7L8 14l-5.5-2.5z"/><path d="M2.5 4.5 8 7l5.5-2.5M8 7v7M2.5 11.5 8 7l5.5 4.5"/>';
ICONS.heat = '<path d="M8 2.5c2 2.6 4.2 4.4 4.2 7a4.2 4.2 0 0 1-8.4 0c0-1.6.8-2.8 1.6-3.8.4 1.2 1 1.9 1.8 2.3C7.5 6.3 8.2 4.4 8 2.5z"/>';
ICONS.palette = '<circle cx="8" cy="8" r="5.5"/><circle cx="5.5" cy="6.5" r=".9"/><circle cx="8.5" cy="4.8" r=".9"/><circle cx="11" cy="7" r=".9"/><circle cx="9.5" cy="10.2" r=".9"/>';
ICONS.assign = '<path d="M3 13 9.5 6.5l2 2L5 15H3z"/><path d="M9.5 6.5 12 4l2 2-2.5 2.5"/><path d="M10 13.5h4"/>';
ICONS.add = '<circle cx="8" cy="8" r="5.5"/><path d="M8 5v6M5 8h6"/>';
ICONS.erase = '<circle cx="8" cy="8" r="5.5"/><path d="M5 8h6"/>';
ICONS.smooth = '<path d="M2 10c2-4 4-4 6 0s4 4 6 0"/><path d="M2 6.5c2-3 4-3 6 0s4 3 6 0" opacity=".5"/>';
ICONS.cuts = '<path d="M8 2v12"/><ellipse cx="8" cy="5" rx="4.5" ry="1.6"/><ellipse cx="8" cy="9.2" rx="5.2" ry="1.7"/><ellipse cx="8" cy="12.6" rx="3.6" ry="1.3"/>';
ICONS.markers = '<circle cx="8" cy="7" r="3"/><path d="M8 10v4M5.5 14h5"/><circle cx="8" cy="7" r=".9"/>';
ICONS.clips = '<path d="M3 3h10v10H3z"/><path d="M6.5 5.5v5l4-2.5z"/>';
ICONS.undo = '<path d="M5 6H2.5V3.5"/><path d="M2.8 6.2A5.5 5.5 0 1 1 3 10.5"/>';
ICONS.save = '<path d="M3 3h8l2 2v8H3z"/><path d="M5 3v3.5h5V3M5 13V9.5h6V13"/>';
// A monster: a horned head; a formation: three of them in a row.
ICONS.monster = '<path d="M3.2 8.6a4.8 4.8 0 0 1 9.6 0v3.6a1.4 1.4 0 0 1-1.4 1.4H4.6a1.4 1.4 0 0 1-1.4-1.4z"/><path d="M4.4 4.6 3 1.8M11.6 4.6 13 1.8"/><circle cx="6.2" cy="8.6" r=".8"/><circle cx="9.8" cy="8.6" r=".8"/>';
ICONS.formation = '<circle cx="3.4" cy="8" r="2"/><circle cx="8" cy="8" r="2"/><circle cx="12.6" cy="8" r="2"/><path d="M2 13.2h12"/>';

/// One icon, as an element ready to put in a row.
function icon(name, extra) {
  const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
  svg.setAttribute('viewBox', '0 0 16 16');
  svg.setAttribute('aria-hidden', 'true');
  svg.setAttribute('class', 'icon' + (name ? ' k-' + name : '') + (extra ? ' ' + extra : ''));
  svg.innerHTML = ICONS[name] || ICONS.file;
  return svg;
}
