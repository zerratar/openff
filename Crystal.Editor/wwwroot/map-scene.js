// A map drawn as a scene: the terrain with everything standing on it.
//
// The single-model viewer draws one bundle; this draws a terrain plus one instance per
// .hich row, each with its own place and facing. The shader and the texture handling
// come from model-viewer.js, which loads first - there is only one way to draw an FF3
// model and it should only be written down once.
//
// The units line up without any conversion. A .hich position is in the same units as
// the geometry, and its posture is in degrees; both were measured against the terrain
// rather than assumed. See Editor/MapScene.cs.

'use strict';

// ------------------------------------------------------------------ the gizmo
//
// Three arrows at whatever is selected, one per axis, dragged to move it. The arrows
// are real geometry rather than lines: a line comes out one pixel wide however thick
// you ask for it, which is not something you can reliably grab.
//
// Dragging works in screen space. The axis is projected to the screen, the mouse
// movement is projected onto that, and the ratio says how far along the axis to go -
// which behaves sensibly whatever angle the axis is seen from, including nearly
// end-on, where the axis simply stops responding rather than flying off.

const AXES = [
  { name: 'x', colour: [0.92, 0.35, 0.38], dir: [1, 0, 0] },
  { name: 'y', colour: [0.45, 0.85, 0.45], dir: [0, 1, 0] },
  { name: 'z', colour: [0.38, 0.60, 0.95], dir: [0, 0, 1] }
];

/// An arrow along +X, one unit long: a shaft and a head, both eight sided.
/// A unit box from 0,0,0 to 1,1,1, as triangles. Used for the region that fires an
/// exit, which is a real box in the collision mesh rather than a marker: seeing where
/// its edges are is the difference between a door that works and one you walk past.
function boxGeometry() {
  const out = [];
  const push = (x, y, z) => out.push(x, y, z, 0, 0, 1, 1, 1);
  const quad = (a, b, c, d) => {
    push(...a); push(...b); push(...c);
    push(...a); push(...c); push(...d);
  };
  const p = [
    [0, 0, 0], [1, 0, 0], [1, 0, 1], [0, 0, 1],
    [0, 1, 0], [1, 1, 0], [1, 1, 1], [0, 1, 1]
  ];
  quad(p[0], p[1], p[5], p[4]);
  quad(p[1], p[2], p[6], p[5]);
  quad(p[2], p[3], p[7], p[6]);
  quad(p[3], p[0], p[4], p[7]);
  quad(p[4], p[5], p[6], p[7]);
  quad(p[3], p[2], p[1], p[0]);
  return new Float32Array(out);
}

function arrowGeometry() {
  const out = [];
  const push = (x, y, z) => out.push(x, y, z, 0, 0, 1, 1, 1);
  const shaft = 0.022;
  const head = 0.085;
  const neck = 0.74;
  const sides = 8;

  for (let i = 0; i < sides; i++) {
    const a0 = (i / sides) * Math.PI * 2;
    const a1 = ((i + 1) / sides) * Math.PI * 2;
    const y0 = Math.cos(a0), z0 = Math.sin(a0);
    const y1 = Math.cos(a1), z1 = Math.sin(a1);

    push(0, y0 * shaft, z0 * shaft);
    push(neck, y0 * shaft, z0 * shaft);
    push(neck, y1 * shaft, z1 * shaft);
    push(0, y0 * shaft, z0 * shaft);
    push(neck, y1 * shaft, z1 * shaft);
    push(0, y1 * shaft, z1 * shaft);

    push(neck, y0 * head, z0 * head);
    push(1, 0, 0);
    push(neck, y1 * head, z1 * head);

    push(neck, 0, 0);
    push(neck, y1 * head, z1 * head);
    push(neck, y0 * head, z0 * head);
  }
  return new Float32Array(out);
}

/// A ring lying in the ground plane, for turning something about its vertical axis.
/// One ring rather than three, because a .hich row holds one angle: its facing. There
/// is no scale in the format either, which is why there is no scale gizmo.
function ringGeometry() {
  const out = [];
  const push = (x, y, z) => out.push(x, y, z, 0, 0, 1, 1, 1);
  const ring = 1;
  const tube = 0.035;
  const around = 48;
  const through = 6;

  const at = (a, b) => [
    (ring + tube * Math.cos(b)) * Math.cos(a),
    tube * Math.sin(b),
    (ring + tube * Math.cos(b)) * Math.sin(a)
  ];

  for (let i = 0; i < around; i++) {
    const a0 = (i / around) * Math.PI * 2;
    const a1 = ((i + 1) / around) * Math.PI * 2;
    for (let j = 0; j < through; j++) {
      const b0 = (j / through) * Math.PI * 2;
      const b1 = ((j + 1) / through) * Math.PI * 2;
      const A = at(a0, b0), B = at(a1, b0), C = at(a1, b1), D = at(a0, b1);
      push(...A); push(...B); push(...C);
      push(...A); push(...C); push(...D);
    }
  }
  return new Float32Array(out);
}

/// Places an arrow: rotates local +X onto the axis, scales it, moves it into place.
function axisMatrix(axis, at, size) {
  const s = size;
  const rows = {
    x: [s, 0, 0, 0, s, 0, 0, 0, s],
    y: [0, s, 0, -s, 0, 0, 0, 0, s],
    z: [0, 0, s, 0, s, 0, -s, 0, 0]
  }[axis];
  return new Float32Array([
    rows[0], rows[1], rows[2], 0,
    rows[3], rows[4], rows[5], 0,
    rows[6], rows[7], rows[8], 0,
    at[0], at[1], at[2], 1
  ]);
}

function makeMapScene(canvas, status) {
  const gl = canvas.getContext('webgl', { antialias: true, alpha: true });
  if (!gl) {
    status('this browser has no WebGL, so the 3D view cannot be drawn', 'bad');
    return null;
  }

  const program = link(gl, MODEL_VERTEX, MODEL_FRAGMENT);
  const attribute = {
    position: gl.getAttribLocation(program, 'position'),
    coord: gl.getAttribLocation(program, 'coord'),
    colour: gl.getAttribLocation(program, 'colour'),
    mindex: gl.getAttribLocation(program, 'mindex')
  };
  const uniform = {
    camera: gl.getUniformLocation(program, 'camera'),
    // The shader is the viewer's, which skins each vertex through palette[mindex]. The
    // scene poses nothing: mindex is pinned to 0 in draw(), and palette[0] - the location
    // named 'palette' - is the instance matrix. (A uniform named 'model' no longer exists;
    // WebGL ignores a null location silently, and every mesh collapsed to the origin.)
    model: gl.getUniformLocation(program, 'palette'),
    picture: gl.getUniformLocation(program, 'picture'),
    textured: gl.getUniformLocation(program, 'textured'),
    tint: gl.getUniformLocation(program, 'tint'),
    alpha: gl.getUniformLocation(program, 'alpha')
  };

  const bigIndices = gl.getExtension('OES_element_index_uint');
  const indexType = bigIndices ? gl.UNSIGNED_INT : gl.UNSIGNED_SHORT;
  const indexSize = bigIndices ? 4 : 2;
  const blank = solidTexture(gl, [255, 255, 255, 255]);

  // package name -> { vertexBuffer, indexBuffer, groups, textures, centre, radius }
  const loaded = new Map();
  let scene = null;
  let instances = [];
  let selected = null;
  let selectedExit = null;
  // Points from an OpenFF project's scene file: spots a mod finds by name. Not in the
  // map's own files, so the page hands them in and hears about moves like the rest.
  let points = [];
  let selectedPoint = null;

  // An exit is two positioned things and they are not in the same place: the region is
  // the doorway you walk into, the arrival is where you come out on the other side. So
  // which half is being moved has to be part of the selection.
  let selectedPart = 'arrival';
  let onPick = () => {};

  const arrowBuffer = gl.createBuffer();
  const arrow = arrowGeometry();
  gl.bindBuffer(gl.ARRAY_BUFFER, arrowBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, arrow, gl.STATIC_DRAW);
  const arrowVertices = arrow.length / 8;

  const ringBuffer = gl.createBuffer();
  const ring = ringGeometry();
  gl.bindBuffer(gl.ARRAY_BUFFER, ringBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, ring, gl.STATIC_DRAW);
  const ringVertices = ring.length / 8;

  const boxBuffer = gl.createBuffer();
  const box = boxGeometry();
  gl.bindBuffer(gl.ARRAY_BUFFER, boxBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, box, gl.STATIC_DRAW);
  const boxVertices = box.length / 8;

  // The move gizmo's plane handle: a unit square in the ground plane (x 0..1, z 0..1), placed
  // between the x and z arrows; dragging it moves the object freely across the ground.
  const planeBuffer = gl.createBuffer();
  gl.bindBuffer(gl.ARRAY_BUFFER, planeBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
    0, 0, 0, 0, 0, 1, 1, 1,  1, 0, 0, 0, 0, 1, 1, 1,  1, 0, 1, 0, 0, 1, 1, 1,
    0, 0, 0, 0, 0, 1, 1, 1,  1, 0, 1, 0, 0, 1, 1, 1,  0, 0, 1, 0, 0, 1, 1, 1
  ]), gl.STATIC_DRAW);
  // Where the handle sits and how big, as fractions of the arrows' length.
  const PLANE_FROM = 0.28, PLANE_SIZE = 0.3;

  let gizmoOn = true;
  let gizmoScale = 1;            // how big the arrows are, as a multiplier
  let background = null;         // [r, g, b] 0-1 from the map's MapSettings, or null for the editor's own
  let onFrame = () => {};
  let gizmoMode = 'move';        // 'move' or 'rotate'
  let gizmoAxis = null;          // the axis being dragged, if any ('x', 'y', 'z', 'xz' the plane, 'turn')
  let gizmoHover = null;         // the handle under the cursor, for lighting it
  let gizmoFrom = null;          // where the drag started
  let onMoved = () => {};

  let yaw = 0.7;
  let pitch = 0.9;
  let distance = 400;
  let centre = [0, 0, 0];
  // A field's chips mirrored along z (FF4's layout of FF3's grid); the toolbar toggles it.
  let mirrorZ = false;

  /// Where a chip sits: FF3's placement, or the same grid with z negated and each chip
  /// mirrored about its own centre. A mirror reverses winding, which does not matter
  /// here because the scene draws both faces.
  function chipMatrix(chip) {
    const sz = mirrorZ ? -1 : 1;
    return new Float32Array([
      1, 0, 0, 0,
      0, 1, 0, 0,
      0, 0, sz, 0,
      chip.x, 0, sz * chip.z, 1
    ]);
  }

  /// Frames the whole field, or the one chip an FF3 chip map is about. A mirrored field
  /// is looked at from the far side, as FF4's camera does: its mountains and forests are
  /// quads tilted towards that camera, and from FF3's side they show their backs.
  function frameField() {
    const field = scene && scene.field;
    if (!field || !field.chips.length) return false;
    yaw = 0.7 + (mirrorZ ? Math.PI : 0);
    const focus = field.focus && field.chips.find(c => c.package.endsWith('/' + field.focus + '.flsc.lz'));
    if (focus) {
      centre = [focus.x, 0, (mirrorZ ? -1 : 1) * focus.z];
      distance = Math.max(field.sizeX, field.sizeZ) * 2.2;
      return true;
    }
    const xs = field.chips.map(c => c.x), zs = field.chips.map(c => c.z);
    centre = [(Math.min(...xs) + Math.max(...xs)) / 2, 0,
              (mirrorZ ? -1 : 1) * (Math.min(...zs) + Math.max(...zs)) / 2];
    distance = Math.max(Math.max(...xs) - Math.min(...xs) + field.sizeX,
                        Math.max(...zs) - Math.min(...zs) + field.sizeZ) * 1.1;
    return true;
  }

  function resize() {
    const scale = window.devicePixelRatio || 1;
    const width = Math.max(1, Math.round(canvas.clientWidth * scale));
    const height = Math.max(1, Math.round(canvas.clientHeight * scale));
    if (canvas.width !== width || canvas.height !== height) {
      canvas.width = width;
      canvas.height = height;
    }
    gl.viewport(0, 0, canvas.width, canvas.height);
  }

  function cameraMatrix() {
    const aspect = canvas.width / Math.max(1, canvas.height);
    const near = Math.max(0.5, distance * 0.005);
    const far = distance * 20 + 2000;
    const f = 1 / Math.tan(0.7 / 2);
    const projection = [
      f / aspect, 0, 0, 0,
      0, f, 0, 0,
      0, 0, (far + near) / (near - far), -1,
      0, 0, (2 * far * near) / (near - far), 0
    ];
    const cy = Math.cos(yaw), sy = Math.sin(yaw);
    const cp = Math.cos(pitch), sp = Math.sin(pitch);
    const eye = [
      centre[0] + distance * cp * sy,
      centre[1] + distance * sp,
      centre[2] + distance * cp * cy
    ];
    return multiply(projection, lookAt(eye, centre, [0, 1, 0]));
  }

  /// Which way round a facing turns, because the two kinds here disagree.
  ///
  /// A .hich row's posture is in degrees, and the game negates it before use:
  ///
  ///   vecFx2.set(..., 4096 * FX_DEG_TO_IDX(m_Posture[1]) * -1, ...);
  ///
  /// next to a position and a scale it passes straight through. An exit's arrival
  /// facing is a different field in a different format - a 16 bit angle where a whole
  /// turn is 65536 - and setupMapJumpPosition uses it as it stands, no negation. So
  /// the same number turns opposite ways depending on which of the two it came from,
  /// and drawing both the same way meant a placed character faced left in the editor
  /// and right in the game.
  function facingSign(item) {
    // Exits and points hold the facing the way the engine reads it; a .hich character's
    // is negated by the game.
    return item && (item.exit !== undefined || item.point !== undefined) ? 1 : -1;
  }

  /// An item's facing in radians, the way the game will apply it.
  function facingRadians(item) {
    return facingSign(item) * (item.rotationY || 0) * Math.PI / 180;
  }

  /// Place, face and size one instance. Posture is in degrees about the vertical.
  function placement(item) {
    const a = facingRadians(item);
    const c = Math.cos(a), s = Math.sin(a);
    const k = item.scale || 1;
    return new Float32Array([
      c * k, 0, -s * k, 0,
      0, k, 0, 0,
      s * k, 0, c * k, 0,
      item.x, item.y, item.z, 1
    ]);
  }

  function drawBundle(entry, matrix, tintWith, fade) {
    gl.bindBuffer(gl.ARRAY_BUFFER, entry.vertexBuffer);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, entry.indexBuffer);
    const stride = 8 * 4;
    bind(attribute.position, 3, 0);
    bind(attribute.coord, 2, 3 * 4);
    bind(attribute.colour, 3, 5 * 4);
    gl.uniformMatrix4fv(uniform.model, false, matrix);
    let current = matrix;

    for (let pass = 0; pass < 2; pass++) {
      gl.depthMask(pass === 0);
      for (const group of entry.groups) {
        if (!group.count || group.hidden) continue;
        if (Boolean(group.translucent) !== (pass === 1)) continue;

        // A billboard turns to face the camera when the game draws it, so a mirroring
        // matrix (a mirrored field chip) must move its pivot and leave its facing alone:
        // the same matrix with the reflection taken out and the pivot's image kept.
        let wanted = matrix;
        if (matrix[10] < 0 && group.billboard && group.pivot) {
          wanted = new Float32Array(matrix);
          wanted[10] = -matrix[10];
          wanted[14] = matrix[14] - 2 * group.pivot[2] * wanted[10];
        }
        if (wanted !== current) {
          gl.uniformMatrix4fv(uniform.model, false, wanted);
          current = wanted;
        }

        const texture = group.texture ? entry.textures.get(group.texture) : null;
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture || blank);
        if (texture) applyWrap(gl, group);
        gl.uniform1i(uniform.picture, 0);
        gl.uniform1i(uniform.textured, texture ? 1 : 0);
        gl.uniform3fv(uniform.tint, tintWith || (texture ? [1, 1, 1] : rgb(group.colour)));
        gl.uniform1f(uniform.alpha, (group.alpha ?? 1) * (fade == null ? 1 : fade));
        gl.drawElements(gl.TRIANGLES, group.count, indexType, group.start * indexSize);
      }
    }
    gl.depthMask(true);
    gl.uniform1f(uniform.alpha, 1);

    function bind(where, size, offset) {
      if (where < 0) return;
      gl.enableVertexAttribArray(where);
      gl.vertexAttribPointer(where, size, gl.FLOAT, false, stride, offset);
    }
  }

  function draw() {
    resize();
    // The editor's own dusk, or the map's sky when a MapSettings says one (a map of the mod's own).
    if (background) gl.clearColor(background[0], background[1], background[2], 1); else gl.clearColor(0.09, 0.10, 0.12, 1);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    gl.disable(gl.CULL_FACE);
    if (!scene) return;

    gl.useProgram(program);
    gl.uniformMatrix4fv(uniform.camera, false, cameraMatrix());
    if (attribute.mindex >= 0) {
      gl.disableVertexAttribArray(attribute.mindex);
      gl.vertexAttrib1f(attribute.mindex, 0);
    }

    const terrain = scene.terrain && loaded.get(scene.terrain);
    if (terrain) drawBundle(terrain, IDENTITY, null);
    if (scene.field) {
      for (const chip of scene.field.chips) {
        const entry = loaded.get(chip.package);
        if (entry) drawBundle(entry, chipMatrix(chip), null);
      }
    }

    for (const item of instances) {
      const entry = loaded.get(item.package);
      if (!entry) continue;
      // The selected one is washed blue rather than outlined - an outline needs a second
      // pass over the same geometry, and this reads just as clearly against the terrain.
      const tint = selected === item.index ? [0.55, 0.75, 1.35] : null;
      drawBundle(entry, placement(item), tint);
    }

    drawRegions();
    drawPoints();
    drawGhost();
    drawGizmo();
    onFrame();
  }

  // A model being dragged in from the library, riding the cursor over the scene until it is
  // dropped (or the drag is given up): { package, x, y, z }, drawn see-through and washed blue.
  let ghost = null;

  function drawGhost() {
    const entry = ghost && loaded.get(ghost.package);
    if (!entry) return;
    gl.enable(gl.BLEND); gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    drawBundle(entry, placement({ x: ghost.x, y: ghost.y, z: ghost.z, rotationY: 0, scale: 1 }), [0.7, 0.9, 1.4], 0.6);
  }

  /// Where a pixel's ray meets the scene's geometry - the terrain, the field's chips, the
  /// map's characters and the mod's objects (not the ghost itself) - nearest first; failing
  /// any, where it meets the plane y = 0, so a drop over nothing lands on the floor rather
  /// than far away. Null when the ray never comes down.
  function surfaceAt(px, py) {
    const { eye, dir } = rayAt(px, py);
    let best = Infinity;
    const test = (entry, matrix) => {
      if (!entry || !entry.buffer || !entry.indices) return;
      // The ray in the model's own space (the matrices are affine), so the triangles stay as stored.
      const inv = invertAffine(matrix);
      if (!inv) return;
      const o = transformPoint(inv, eye), d = transformDirection(inv, dir);
      // A cheap first look: the model's sphere.
      if (entry.centre && entry.radius) {
        const cx = entry.centre[0] - o[0], cy = entry.centre[1] - o[1], cz = entry.centre[2] - o[2];
        const along = cx * d[0] + cy * d[1] + cz * d[2];
        const dd = d[0] * d[0] + d[1] * d[1] + d[2] * d[2];
        const off2 = cx * cx + cy * cy + cz * cz - along * along / dd;
        if (off2 > entry.radius * entry.radius * 1.2) return;
      }
      const p = entry.buffer, idx = entry.indices;
      for (const group of entry.groups) {
        if (!group.count || group.hidden) continue;
        for (let i = group.start; i + 2 < group.start + group.count; i += 3) {
          const a = idx[i] * 8, b = idx[i + 1] * 8, c = idx[i + 2] * 8;
          const t = rayTriangle(o, d, p[a], p[a + 1], p[a + 2], p[b], p[b + 1], p[b + 2], p[c], p[c + 1], p[c + 2]);
          // t is in the model's space along d, which is dir transformed: the same parameter in the world.
          if (t !== null && t > 1e-4 && t < best) best = t;
        }
      }
    };
    const terrain = scene && scene.terrain && loaded.get(scene.terrain);
    if (terrain) test(terrain, IDENTITY);
    if (scene && scene.field) for (const chip of scene.field.chips) test(loaded.get(chip.package), chipMatrix(chip));
    for (const item of instances) test(loaded.get(item.package), placement(item));
    for (const point of points) if (!point.hidden && point.package) test(loaded.get(point.package), placement(point));
    if (best < Infinity) return [eye[0] + dir[0] * best, eye[1] + dir[1] * best, eye[2] + dir[2] * best];
    if (dir[1] < -1e-4) { const along = -eye[1] / dir[1]; return [eye[0] + dir[0] * along, 0, eye[2] + dir[2] * along]; }
    return null;
  }

  function transformPoint(m, v) {
    return [m[0] * v[0] + m[4] * v[1] + m[8] * v[2] + m[12], m[1] * v[0] + m[5] * v[1] + m[9] * v[2] + m[13], m[2] * v[0] + m[6] * v[1] + m[10] * v[2] + m[14]];
  }
  function transformDirection(m, v) {
    return [m[0] * v[0] + m[4] * v[1] + m[8] * v[2], m[1] * v[0] + m[5] * v[1] + m[9] * v[2], m[2] * v[0] + m[6] * v[1] + m[10] * v[2]];
  }
  /// The inverse of a column-major affine 4x4 (its last row 0 0 0 1), or null when it has none.
  function invertAffine(m) {
    const a = m[0], b = m[4], c = m[8], d = m[1], e = m[5], f = m[9], g = m[2], h = m[6], i = m[10];
    const det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
    if (Math.abs(det) < 1e-12) return null;
    const r = [
      (e * i - f * h) / det, (c * h - b * i) / det, (b * f - c * e) / det,
      (f * g - d * i) / det, (a * i - c * g) / det, (c * d - a * f) / det,
      (d * h - e * g) / det, (b * g - a * h) / det, (a * e - b * d) / det
    ];
    const tx = m[12], ty = m[13], tz = m[14];
    return new Float32Array([
      r[0], r[3], r[6], 0,
      r[1], r[4], r[7], 0,
      r[2], r[5], r[8], 0,
      -(r[0] * tx + r[1] * ty + r[2] * tz), -(r[3] * tx + r[4] * ty + r[5] * tz), -(r[6] * tx + r[7] * ty + r[8] * tz), 1
    ]);
  }
  function rayTriangle(o, d, ax, ay, az, bx, by, bz, cx, cy, cz) {
    const e1x = bx - ax, e1y = by - ay, e1z = bz - az, e2x = cx - ax, e2y = cy - ay, e2z = cz - az;
    const px = d[1] * e2z - d[2] * e2y, py = d[2] * e2x - d[0] * e2z, pz = d[0] * e2y - d[1] * e2x;
    const det = e1x * px + e1y * py + e1z * pz;
    if (Math.abs(det) < 1e-9) return null;
    const inv = 1 / det;
    const tx = o[0] - ax, ty = o[1] - ay, tz = o[2] - az;
    const u = (tx * px + ty * py + tz * pz) * inv;
    if (u < 0 || u > 1) return null;
    const qx = ty * e1z - tz * e1y, qy = tz * e1x - tx * e1z, qz = tx * e1y - ty * e1x;
    const v = (d[0] * qx + d[1] * qy + d[2] * qz) * inv;
    if (v < 0 || u + v > 1) return null;
    return (e2x * qx + e2y * qy + e2z * qz) * inv;
  }

  /// What the gizmo is acting on. Objects are their own row; an exit is either its
  /// arrival point or its region, and both are handed back in the shape the gizmo
  /// works in, so nothing below has to know the difference.
  function selectedItem() {
    if (selected !== null) {
      return instances.find(o => o.index === selected) || null;
    }
    if (selectedPoint !== null) {
      const point = points[selectedPoint];
      if (!point) return null;
      return {
        get x() { return point.x; },
        set x(v) { point.x = v; },
        get y() { return point.y; },
        set y(v) { point.y = v; },
        get z() { return point.z; },
        set z(v) { point.z = v; },
        get rotationY() { return point.rotationY || 0; },
        set rotationY(v) { point.rotationY = v; },
        point: selectedPoint,
        name: point.name,
        package: point.package,
        scale: point.scale || 1
      };
    }
    const exit = selectedExit !== null && scene && scene.exits
      && scene.exits[selectedExit];
    if (!exit) return null;

    if (selectedPart === 'region') {
      if (!exit.region) return null;
      // The region is centre x, floor y, centre z then its size. The gizmo moves the
      // first three; the box is drawn from them.
      return {
        get x() { return exit.region[0]; },
        set x(v) { exit.region[0] = v; },
        get y() { return exit.region[1]; },
        set y(v) { exit.region[1] = v; },
        get z() { return exit.region[2]; },
        set z(v) { exit.region[2] = v; },
        rotationY: 0,
        exit: selectedExit,
        part: 'region'
      };
    }

    return {
      get x() { return exit.x; },
      set x(v) { exit.x = v; },
      get y() { return exit.y; },
      set y(v) { exit.y = v; },
      get z() { return exit.z; },
      set z(v) { exit.z = v; },
      get rotationY() { return exit.rotationY || 0; },
      set rotationY(v) { exit.rotationY = v; },
      exit: selectedExit,
      part: 'arrival'
    };
  }

  /// How big an arrow has to be to stay the same size on screen.
  ///
  /// Measured from the eye to the thing itself, not from the camera's orbit distance.
  /// Those are only the same for whatever the camera happens to be focused on, which
  /// is why a gizmo out at the edge of a zoomed-out map used to come out as big as the
  /// room and then shrink the moment you focused it. The multiplier on the end is
  /// yours, from the slider.
  function gizmoSize(at) {
    const eye = eyePosition();
    const away = at
      ? Math.hypot(at[0] - eye[0], at[1] - eye[1], at[2] - eye[2])
      : distance;
    return Math.max(0.5, away) * 0.075 * gizmoScale;
  }

  /// The boxes that fire the exits. Drawn before the gizmo so the arrows sit over
  /// them, and lightly, because they are volumes you look through rather than at.
  function drawRegions() {
    const exits = (scene && scene.exits) || [];
    if (!exits.length) return;

    gl.bindBuffer(gl.ARRAY_BUFFER, boxBuffer);
    const stride = 8 * 4;
    if (attribute.position >= 0) {
      gl.enableVertexAttribArray(attribute.position);
      gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
    }
    if (attribute.coord >= 0) {
      gl.enableVertexAttribArray(attribute.coord);
      gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 3 * 4);
    }
    if (attribute.colour >= 0) {
      gl.enableVertexAttribArray(attribute.colour);
      gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 5 * 4);
    }
    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, blank);
    gl.uniform1i(uniform.picture, 0);
    gl.uniform1i(uniform.textured, 0);

    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    gl.depthMask(false);

    exits.forEach((exit, index) => {
      if (!exit.region) return;
      const [cx, fy, cz, w, h, d] = exit.region;
      const chosen = index === selectedExit;
      gl.uniform3fv(uniform.tint, chosen ? [1, 0.78, 0.35] : [0.55, 0.42, 0.25]);
      gl.uniform1f(uniform.alpha, chosen ? 0.34 : 0.16);
      gl.uniformMatrix4fv(uniform.model, false, new Float32Array([
        w, 0, 0, 0,
        0, h, 0, 0,
        0, 0, d, 0,
        cx - w / 2, fy, cz - d / 2, 1
      ]));
      gl.drawArrays(gl.TRIANGLES, 0, boxVertices);
    });

    gl.depthMask(true);
    gl.uniform1f(uniform.alpha, 1);
  }

  /// The mod's objects: one with a model is drawn as that model (washed blue when chosen,
  /// as the map's characters are); the rest, and one whose model has not arrived, as a
  /// small box standing on its spot with a sliver ahead for the facing.
  function drawPoints() {
    if (!points.length) return;
    for (const point of points) {
      if (point.hidden) continue;   // a timeline preview's "show: off"
      const entry = point.package && loaded.get(point.package);
      if (!entry) continue;
      const chosen = points.indexOf(point) === selectedPoint;
      if (point.alpha != null && point.alpha < 1) { gl.enable(gl.BLEND); gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA); }
      drawBundle(entry, placement(point), chosen ? [0.55, 0.75, 1.35] : null, point.alpha);
    }
    if (!points.some(p => !(p.package && loaded.get(p.package)))) return;
    gl.bindBuffer(gl.ARRAY_BUFFER, boxBuffer);
    const stride = 8 * 4;
    if (attribute.position >= 0) {
      gl.enableVertexAttribArray(attribute.position);
      gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
    }
    if (attribute.coord >= 0) {
      gl.enableVertexAttribArray(attribute.coord);
      gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 3 * 4);
    }
    if (attribute.colour >= 0) {
      gl.enableVertexAttribArray(attribute.colour);
      gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 5 * 4);
    }
    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, blank);
    gl.uniform1i(uniform.picture, 0);
    gl.uniform1i(uniform.textured, 0);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);

    points.forEach((point, index) => {
      if (point.package && loaded.get(point.package)) return;
      const chosen = index === selectedPoint;
      const s = 3;
      gl.uniform3fv(uniform.tint, chosen ? [0.5, 1.2, 1.3] : [0.3, 0.8, 0.95]);
      gl.uniform1f(uniform.alpha, chosen ? 0.85 : 0.6);
      gl.uniformMatrix4fv(uniform.model, false, new Float32Array([
        s, 0, 0, 0,
        0, s * 2, 0, 0,
        0, 0, s, 0,
        point.x - s / 2, point.y, point.z - s / 2, 1
      ]));
      gl.drawArrays(gl.TRIANGLES, 0, boxVertices);
      // The facing: a thin bar out of the box along the yaw (0 = +z, 90 = +x).
      const a = (point.rotationY || 0) * Math.PI / 180;
      const c = Math.cos(a), sn = Math.sin(a);
      gl.uniformMatrix4fv(uniform.model, false, new Float32Array([
        c, 0, -sn, 0,
        0, 0.6, 0, 0,
        sn * 5, 0, c * 5, 0,
        point.x - c * 0.4 + sn * 1.5, point.y + s, point.z + sn * 0.4 + c * 1.5, 1
      ]));
      gl.drawArrays(gl.TRIANGLES, 0, boxVertices);
    });
    gl.uniform1f(uniform.alpha, 1);
  }

  function drawGizmo() {
    const item = gizmoOn && selectedItem();
    if (!item) return;

    // Over the top of everything: a gizmo you cannot see is a gizmo you cannot use.
    gl.disable(gl.DEPTH_TEST);
    gl.bindBuffer(gl.ARRAY_BUFFER, arrowBuffer);
    const stride = 8 * 4;
    if (attribute.position >= 0) {
      gl.enableVertexAttribArray(attribute.position);
      gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
    }
    if (attribute.coord >= 0) {
      gl.enableVertexAttribArray(attribute.coord);
      gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 3 * 4);
    }
    if (attribute.colour >= 0) {
      gl.enableVertexAttribArray(attribute.colour);
      gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 5 * 4);
    }

    gl.activeTexture(gl.TEXTURE0);
    gl.bindTexture(gl.TEXTURE_2D, blank);
    gl.uniform1i(uniform.picture, 0);
    gl.uniform1i(uniform.textured, 0);
    gl.uniform1f(uniform.alpha, 1);

    const at = [item.x, item.y, item.z];
    if (gizmoMode === 'rotate') {
      const size = gizmoSize(at);
      gl.bindBuffer(gl.ARRAY_BUFFER, ringBuffer);
      if (attribute.position >= 0) {
        gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
      }
      if (attribute.coord >= 0) {
        gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 3 * 4);
      }
      if (attribute.colour >= 0) {
        gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 5 * 4);
      }
      gl.uniform3fv(uniform.tint, gizmoAxis === 'turn' ? [1, 0.95, 0.5] : [0.45, 0.85, 0.45]);
      gl.uniformMatrix4fv(uniform.model, false, new Float32Array([
        size, 0, 0, 0, 0, size, 0, 0, 0, 0, size, 0, at[0], at[1], at[2], 1
      ]));
      gl.drawArrays(gl.TRIANGLES, 0, ringVertices);

      // A short arrow showing which way it is facing, so the angle is readable
      // without going to the inspector for it.
      const a = facingRadians(item);
      gl.bindBuffer(gl.ARRAY_BUFFER, arrowBuffer);
      if (attribute.position >= 0) {
        gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
      }
      if (attribute.coord >= 0) {
        gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 3 * 4);
      }
      if (attribute.colour >= 0) {
        gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 5 * 4);
      }
      gl.uniform3fv(uniform.tint, [0.95, 0.85, 0.35]);
      // Local +X onto the facing direction, which is (sin a, 0, cos a).
      gl.uniformMatrix4fv(uniform.model, false, new Float32Array([
        Math.sin(a) * size, 0, Math.cos(a) * size, 0,
        0, size, 0, 0,
        Math.cos(a) * size, 0, -Math.sin(a) * size, 0,
        at[0], at[1], at[2], 1
      ]));
      gl.drawArrays(gl.TRIANGLES, 0, arrowVertices);
    } else {
      for (const axis of AXES) {
        const lit = gizmoAxis === axis.name;
        gl.uniform3fv(uniform.tint, lit ? [1, 0.95, 0.5] : axis.colour);
        gl.uniformMatrix4fv(uniform.model, false, axisMatrix(axis.name, at, gizmoSize(at)));
        gl.drawArrays(gl.TRIANGLES, 0, arrowVertices);
      }
      // The plane handle between the x and z arrows: yellow, see-through, solid while dragged.
      const size = gizmoSize(at);
      gl.bindBuffer(gl.ARRAY_BUFFER, planeBuffer);
      if (attribute.position >= 0) gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
      if (attribute.coord >= 0) gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 3 * 4);
      if (attribute.colour >= 0) gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 5 * 4);
      gl.enable(gl.BLEND); gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
      gl.uniform3fv(uniform.tint, [1, 0.9, 0.25]);
      gl.uniform1f(uniform.alpha, gizmoAxis === 'xz' || gizmoHover === 'xz' ? 0.9 : 0.5);
      const k = size * PLANE_SIZE, o = size * PLANE_FROM;
      gl.uniformMatrix4fv(uniform.model, false, new Float32Array([k, 0, 0, 0, 0, k, 0, 0, 0, 0, k, 0, at[0] + o, at[1], at[2] + o, 1]));
      gl.drawArrays(gl.TRIANGLES, 0, 6);
      gl.uniform1f(uniform.alpha, 1);
    }
    gl.enable(gl.DEPTH_TEST);
  }

  /// The plane handle's four corners in the world, for picking.
  function planeCorners(at) {
    const size = gizmoSize(at), k = size * PLANE_SIZE, o = size * PLANE_FROM;
    return [[at[0] + o, at[1], at[2] + o], [at[0] + o + k, at[1], at[2] + o], [at[0] + o + k, at[1], at[2] + o + k], [at[0] + o, at[1], at[2] + o + k]];
  }

  /// Whether a screen point lies inside a (convex) polygon of screen points.
  function insidePolygon(p, corners) {
    let sign = 0;
    for (let i = 0; i < corners.length; i++) {
      const a = corners[i], b = corners[(i + 1) % corners.length];
      const cross = (b[0] - a[0]) * (p[1] - a[1]) - (b[1] - a[1]) * (p[0] - a[0]);
      if (Math.abs(cross) < 1e-9) continue;
      const s = Math.sign(cross);
      if (sign === 0) sign = s; else if (s !== sign) return false;
    }
    return sign !== 0;
  }

  function eyePosition() {
    const cy = Math.cos(yaw), sy = Math.sin(yaw);
    const cp = Math.cos(pitch), sp = Math.sin(pitch);
    return [
      centre[0] + distance * cp * sy,
      centre[1] + distance * sp,
      centre[2] + distance * cp * cy
    ];
  }

  /// Moves the camera without turning it: the eye goes here, the target follows.
  function setEye(eye) {
    const cy = Math.cos(yaw), sy = Math.sin(yaw);
    const cp = Math.cos(pitch), sp = Math.sin(pitch);
    centre = [
      eye[0] - distance * cp * sy,
      eye[1] - distance * sp,
      eye[2] - distance * cp * cy
    ];
  }

  /// The camera's own axes, for flying along and for casting a ray.
  function basis() {
    const eye = eyePosition();
    const forward = normalise([
      centre[0] - eye[0], centre[1] - eye[1], centre[2] - eye[2]
    ]);
    const right = normalise(cross(forward, [0, 1, 0]));
    return { eye, forward, right, up: cross(right, forward) };
  }

  /// A ray from the eye through one pixel.
  function rayAt(px, py) {
    const rect = canvas.getBoundingClientRect();
    const nx = ((px - rect.left) / rect.width) * 2 - 1;
    const ny = 1 - ((py - rect.top) / rect.height) * 2;
    const aspect = canvas.width / Math.max(1, canvas.height);
    const t = Math.tan(0.7 / 2);
    const { eye, forward, right, up } = basis();
    const dir = normalise([
      forward[0] + right[0] * nx * t * aspect + up[0] * ny * t,
      forward[1] + right[1] * nx * t * aspect + up[1] * ny * t,
      forward[2] + right[2] * nx * t * aspect + up[2] * ny * t
    ]);
    return { eye, dir };
  }

  /// Where the cursor lands on the ground plane through a point, as an angle and a
  /// distance from it. This is what makes the ring turn with the mouse rather than
  /// with the screen, so it behaves the same from any camera angle.
  function onGround(px, py, at) {
    const { eye, dir } = rayAt(px, py);
    if (Math.abs(dir[1]) < 1e-4) return null;
    const along = (at[1] - eye[1]) / dir[1];
    if (along <= 0) return null;
    const x = eye[0] + dir[0] * along - at[0];
    const z = eye[2] + dir[2] * along - at[2];
    return {
      // Where the cursor is, as a screen-space bearing. Turning that back into a
      // stored facing goes through facingSign, since the two kinds store it opposite
      // ways round.
      angle: Math.atan2(x, z) * 180 / Math.PI,
      away: Math.hypot(x, z),
      x, z
    };
  }

  /// Where a world point lands on the canvas, in pixels.
  function toScreen(point) {
    const m = cameraMatrix();
    const [x, y, z] = point;
    const w = m[3] * x + m[7] * y + m[11] * z + m[15];
    if (w <= 0) return null;
    const nx = (m[0] * x + m[4] * y + m[8] * z + m[12]) / w;
    const ny = (m[1] * x + m[5] * y + m[9] * z + m[13]) / w;
    const rect = canvas.getBoundingClientRect();
    return [(nx + 1) / 2 * rect.width, (1 - ny) / 2 * rect.height];
  }

  /// Which arrow is under the cursor, if any. In rotate mode there is one target,
  /// the ring, and being near it means being near its radius on the ground.
  function axisAt(px, py) {
    const item = gizmoOn && selectedItem();
    if (!item) return null;

    if (gizmoMode === 'rotate') {
      const ground = onGround(px, py, [item.x, item.y, item.z]);
      if (!ground) return null;
      const size = gizmoSize([item.x, item.y, item.z]);
      return Math.abs(ground.away - size) < size * 0.22
        ? { name: 'turn' } : null;
    }

    const rect = canvas.getBoundingClientRect();
    const mouse = [px - rect.left, py - rect.top];
    const at = [item.x, item.y, item.z];
    const origin = toScreen(at);
    if (!origin) return null;

    // The plane handle first: it sits between the arrows, where a near-miss of either lands.
    const corners = planeCorners(at).map(toScreen);
    if (corners.every(Boolean) && insidePolygon(mouse, corners)) return { name: 'xz' };

    let best = null;
    let bestAway = 11;                    // pixels; a fat enough target to hit
    for (const axis of AXES) {
      const size = gizmoSize(at);
      const tip = toScreen([
        at[0] + axis.dir[0] * size,
        at[1] + axis.dir[1] * size,
        at[2] + axis.dir[2] * size
      ]);
      if (!tip) continue;
      const away = pointToSegment(mouse, origin, tip);
      if (away < bestAway) {
        bestAway = away;
        best = axis;
      }
    }
    return best;
  }

  function pointToSegment(p, a, b) {
    const vx = b[0] - a[0], vy = b[1] - a[1];
    const len = vx * vx + vy * vy;
    if (len < 1e-6) return Math.hypot(p[0] - a[0], p[1] - a[1]);
    let t = ((p[0] - a[0]) * vx + (p[1] - a[1]) * vy) / len;
    t = Math.max(0, Math.min(1, t));
    return Math.hypot(p[0] - (a[0] + vx * t), p[1] - (a[1] + vy * t));
  }

  /// Fetches one package's geometry and its textures, once.
  async function ensure(name) {
    if (!name || loaded.has(name)) return;
    loaded.set(name, null);                 // claim it, so two objects do not both fetch

    const bundle = await api(`/api/model?name=${encodeURIComponent(name)}`);
    if (bundle.error || bundle.problem || !bundle.buffer || !bundle.buffer.length) {
      loaded.delete(name);
      return;
    }

    const vertexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(bundle.buffer), gl.STATIC_DRAW);

    const indexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,
      bigIndices ? new Uint32Array(bundle.indices) : new Uint16Array(bundle.indices),
      gl.STATIC_DRAW);

    const entry = {
      vertexBuffer, indexBuffer, groups: bundle.groups,
      textures: new Map(), centre: bundle.centre, radius: bundle.radius,
      // The geometry kept on this side too, for the ray that finds where a dragged model lands.
      buffer: Float32Array.from(bundle.buffer), indices: bundle.indices
    };
    loaded.set(name, entry);
    draw();

    const wanted = [...new Set(bundle.groups.map(g => g.texture).filter(Boolean))];
    await Promise.all(wanted.map(texture => new Promise(done => {
      const image = new Image();
      image.onload = () => {
        entry.textures.set(texture, upload(gl, image));
        draw();
        done();
      };
      image.onerror = () => done();
      image.src = wsUrl(`/api/model/texture?name=${encodeURIComponent(name)}`
        + `&texture=${encodeURIComponent(texture)}`);
    })));
    draw();
  }

  /// Where an object is on screen right now, for picking and for labels.
  function project(item) {
    return projectPoint([item.x, item.y, item.z]);
  }

  function projectPoint(at) {
    const m = cameraMatrix();
    const [x, y, z] = at;
    const w = m[3] * x + m[7] * y + m[11] * z + m[15];
    if (w <= 0) return null;
    return {
      x: (m[0] * x + m[4] * y + m[8] * z + m[12]) / w,
      y: (m[1] * x + m[5] * y + m[9] * z + m[13]) / w
    };
  }

  /// Roughly where an instance is and how big, as a sphere in the world.
  ///
  /// A .hich position is where something stands, which for a character is its feet.
  /// Picking on that alone means aiming at the ground under somebody rather than at
  /// them, and how far off it looks depends entirely on the camera angle. This lifts
  /// the target onto the model and gives it the model's own size.
  function boundsOf(item) {
    const entry = item.package && loaded.get(item.package);
    const k = item.scale || 1;
    if (!entry || !entry.centre) {
      return { at: [item.x, item.y + 4 * k, item.z], radius: 5 * k };
    }

    // The model's own centre, turned by the instance's facing and moved into place -
    // the same transform placement() builds, applied to one point.
    const a = facingRadians(item);
    const c = Math.cos(a), sn = Math.sin(a);
    const cx = entry.centre[0] * k, cy = entry.centre[1] * k, cz = entry.centre[2] * k;
    return {
      at: [item.x + (c * cx + sn * cz), item.y + cy, item.z + (-sn * cx + c * cz)],
      radius: Math.max(0.5, entry.radius * k)
    };
  }

  /// How wide something of this size looks on screen, in the same units as project().
  function screenRadius(at, radius) {
    const { right } = basis();
    const middle = projectPoint(at);
    const edge = projectPoint([
      at[0] + right[0] * radius, at[1] + right[1] * radius, at[2] + right[2] * radius
    ]);
    if (!middle || !edge) return 0;
    return Math.hypot(edge.x - middle.x, edge.y - middle.y);
  }

  return {
    async load(data, pick) {
      scene = data;
      onPick = pick || (() => {});
      instances = data.objects.filter(o => o.package);
      selected = null;

      // Frame the map before anything has arrived, so it is not looking at nothing.
      const xs = instances.map(o => o.x);
      const zs = instances.map(o => o.z);
      if (xs.length) {
        centre = [(Math.min(...xs) + Math.max(...xs)) / 2, 0,
                  (Math.min(...zs) + Math.max(...zs)) / 2];
        distance = Math.max(120, Math.max(
          Math.max(...xs) - Math.min(...xs), Math.max(...zs) - Math.min(...zs)) * 1.6);
      }
      mirrorZ = !!(data.field && data.field.mirrorZ);
      if (frameField()) draw();
      draw();

      await ensure(data.terrain);
      const terrain = data.terrain && loaded.get(data.terrain);
      if (terrain && terrain.centre) {
        centre = terrain.centre.slice();
        distance = Math.max(80, terrain.radius * 2.6);
      }
      draw();

      // A field is a few hundred chips; fetch them a handful at a time, drawing as
      // they land, rather than all at once or one after another.
      if (data.field) {
        const queue = data.field.chips.map(c => c.package);
        const worker = async () => { while (queue.length) await ensure(queue.shift()); };
        await Promise.all(Array.from({ length: 6 }, worker));
        draw();
      }

      // One fetch per distinct model, not per instance - a town of twenty villagers
      // usually wears three or four models between them.
      for (const name of new Set(instances.map(o => o.package))) {
        await ensure(name);
      }
      draw();
    },

    select(index) { selected = index; selectedExit = null; selectedPoint = null; draw(); },

    /// Picks up a model that was changed under it, fetching the geometry if this is
    /// the first time the map has used it.
    async reload() {
      instances = scene.objects.filter(o => o.package);
      draw();
      for (const name of new Set(instances.map(o => o.package))) {
        await ensure(name);
      }
      draw();
    },

    selectExit(index, part) {
      selectedExit = index;
      selected = null;
      selectedPoint = null;
      if (part) selectedPart = part;
      draw();
    },

    /// Puts the camera on an exit, the way focus() does for a character.
    focusExit(index) {
      const exit = scene && scene.exits && scene.exits[index];
      if (!exit) return;
      selectedExit = index;
      selected = null;
      centre = [exit.x, exit.y + 6, exit.z];
      distance = 46;
      draw();
    },

    setGizmo(on) { gizmoOn = on; if (!on) gizmoAxis = null; draw(); },

    /// Which half of the selected exit the gizmo moves.
    selectPart(part) { selectedPart = part; draw(); },
    part() { return selectedPart; },

    setGizmoScale(scale) { gizmoScale = scale; draw(); },

    /// The colour behind the scene: [r, g, b] 0-1 from a MapSettings, or null for the editor's own.
    setBackground(rgb) { background = rgb || null; draw(); },

    /// Called after every frame, so the page can put its tags where things are now.
    onFrame(callback) { onFrame = callback || (() => {}); },

    /// The view from a point toward another - a camera clip's preview in the timeline. Returns
    /// what the view was (centre, distance, yaw, pitch), for lookBack.
    lookFrom(eye, target) {
      const was = { centre: centre.slice(), distance, yaw, pitch };
      const dx = eye[0] - target[0], dy = eye[1] - target[1], dz = eye[2] - target[2];
      const flat = Math.hypot(dx, dz);
      centre = target.slice();
      distance = Math.max(1, Math.hypot(dx, dy, dz));
      yaw = Math.atan2(dx, dz);
      pitch = Math.max(-1.5, Math.min(1.5, Math.atan2(dy, flat)));
      draw();
      return was;
    },

    /// Where the view stands and what it looks at, for a camera clip taken from the view.
    view() { return { eye: eyePosition(), target: centre.slice() }; },

    /// The view as lookFrom found it.
    lookBack(was) {
      if (!was) return;
      centre = was.centre.slice(); distance = was.distance; yaw = was.yaw; pitch = was.pitch;
      draw();
    },

    /// Where a world point is on the canvas, in CSS pixels, or null if it is behind.
    screenAt(x, y, z) {
      const rect = canvas.getBoundingClientRect();
      const m = cameraMatrix();
      const w = m[3] * x + m[7] * y + m[11] * z + m[15];
      if (w <= 0) return null;
      const nx = (m[0] * x + m[4] * y + m[8] * z + m[12]) / w;
      const ny = (m[1] * x + m[5] * y + m[9] * z + m[13]) / w;
      if (nx < -1.4 || nx > 1.4 || ny < -1.4 || ny > 1.4) return null;
      return [(nx + 1) / 2 * rect.width, (1 - ny) / 2 * rect.height];
    },

    /// Where a pixel lands on the ground, for dropping something onto the map. The
    /// plane is the height most things stand at, which is the height of whatever is
    /// already there rather than a guess at zero.
    groundAt(px, py) {
      const height = instances.length
        ? instances.map(o => o.y).sort((a, b) => a - b)[instances.length >> 1]
        : 0;
      const { eye, dir } = rayAt(px, py);
      if (Math.abs(dir[1]) < 1e-4) return null;
      const along = (height - eye[1]) / dir[1];
      if (along <= 0) return null;
      return [
        Math.round(eye[0] + dir[0] * along),
        height,
        Math.round(eye[2] + dir[2] * along)
      ];
    },

    /// Where a pixel's ray meets the scene - its geometry, else the floor (y = 0) - or null.
    surfaceAt(px, py) { return surfaceAt(px, py); },

    /// A model dragged in from the library, shown see-through where the cursor's ray meets the
    /// scene; its geometry fetched the first time. Returns where it stands, or null off the scene.
    showGhost(name, px, py) {
      const at = surfaceAt(px, py);
      if (!at) { if (ghost) { ghost = null; draw(); } return null; }
      ghost = { package: name, x: at[0], y: at[1], z: at[2] };
      if (!loaded.has(name)) ensure(name).then(() => { if (ghost && ghost.package === name) draw(); });
      draw();
      return at;
    },
    hideGhost() { if (ghost) { ghost = null; draw(); } },
    ghostAt() { return ghost ? [ghost.x, ghost.y, ghost.z] : null; },

    /// The spot on the ground the view looks at: where the centre pixel lands on the
    /// height things stand at. For "play here" and a new object at the view's centre -
    /// the camera's own target drifts up and down as it flies, and a hero put there would
    /// stand on the roofs.
    viewGround() {
      const rect = canvas.getBoundingClientRect();
      const hit = this.groundAt(rect.left + rect.width / 2, rect.top + rect.height / 2);
      if (hit) return hit;
      const height = instances.length ? instances.map(o => o.y).sort((a, b) => a - b)[instances.length >> 1] : 0;
      return [Math.round(centre[0]), height, Math.round(centre[2])];
    },

    setGizmoMode(mode) { gizmoMode = mode; gizmoAxis = null; draw(); },
    gizmoMode() { return gizmoMode; },

    /// Turning on the spot: the eye stays put and the view swings round it.
    look(dx, dy) {
      const eye = eyePosition();
      yaw -= dx * 0.005;
      pitch = Math.max(-1.5, Math.min(1.5, pitch + dy * 0.005));
      setEye(eye);
      draw();
    },

    /// One step of flying. The amounts are -1, 0 or 1 per axis.
    fly(ahead, sideways, upward, quick) {
      if (!ahead && !sideways && !upward) return;
      const { eye, forward, right } = basis();
      const step = Math.max(0.4, distance * 0.02) * (quick ? 4 : 1);
      setEye([
        eye[0] + (forward[0] * ahead + right[0] * sideways) * step,
        eye[1] + (forward[1] * ahead + right[1] * sideways) * step + upward * step,
        eye[2] + (forward[2] * ahead + right[2] * sideways) * step
      ]);
      draw();
    },

    /// True if the press landed on an arrow, in which case the camera stays put.
    beginDrag(px, py) {
      const axis = axisAt(px, py);
      if (!axis) return false;
      const item = selectedItem();
      gizmoAxis = axis.name;
      gizmoFrom = {
        px, py, x: item.x, y: item.y, z: item.z, rotationY: item.rotationY || 0,
        angle: 0
      };
      if (axis.name === 'turn' || axis.name === 'xz') {
        // Where on the object's own ground plane the press landed: the turn keeps its bearing,
        // the plane handle the offset from the object, so it moves without jumping under the cursor.
        const ground = onGround(px, py, [item.x, item.y, item.z]);
        if (!ground) {
          gizmoAxis = null;
          gizmoFrom = null;
          return false;
        }
        gizmoFrom.angle = ground.angle;
        gizmoFrom.gx = ground.x; gizmoFrom.gz = ground.z;
      }
      draw();
      return true;
    },

    dragTo(px, py) {
      if (!gizmoAxis || !gizmoFrom) return null;
      const item = selectedItem();
      if (!item) return null;

      if (gizmoAxis === 'turn') {
        const ground = onGround(px, py, [gizmoFrom.x, gizmoFrom.y, gizmoFrom.z]);
        if (!ground) return null;
        // Dragged towards a bearing, so the stored angle moves against it for a
        // character and with it for an exit - whichever way that one is applied.
        let turned = Math.round(gizmoFrom.rotationY
          + facingSign(item) * (ground.angle - gizmoFrom.angle));
        turned = ((turned % 360) + 360) % 360;
        item.rotationY = turned;
        draw();
        onMoved(item);
        return item;
      }

      if (gizmoAxis === 'xz') {
        // Freely across the ground: where the cursor's ray meets the plane the object stands
        // on, less the offset the press had from the object.
        const ground = onGround(px, py, [gizmoFrom.x, gizmoFrom.y, gizmoFrom.z]);
        if (!ground) return null;
        item.x = Math.round(gizmoFrom.x + ground.x - gizmoFrom.gx);
        item.z = Math.round(gizmoFrom.z + ground.z - gizmoFrom.gz);
        draw();
        onMoved(item);
        return item;
      }

      const axis = AXES.find(a => a.name === gizmoAxis);
      const at = [gizmoFrom.x, gizmoFrom.y, gizmoFrom.z];
      const origin = toScreen(at);
      const size = gizmoSize(at);
      const tip = toScreen([
        at[0] + axis.dir[0] * size,
        at[1] + axis.dir[1] * size,
        at[2] + axis.dir[2] * size
      ]);
      if (!origin || !tip) return null;

      const ax = tip[0] - origin[0];
      const ay = tip[1] - origin[1];
      const along = ax * ax + ay * ay;
      // Seen end on, an axis has almost no length on screen and dragging it would
      // send the object into the distance. Leave it alone instead.
      if (along < 30) return null;

      const mx = px - gizmoFrom.px;
      const my = py - gizmoFrom.py;
      const howFar = ((mx * ax + my * ay) / along) * size;

      // .hich positions are whole numbers, so this snaps rather than drifting.
      item.x = Math.round(at[0] + axis.dir[0] * howFar);
      item.y = Math.round(at[1] + axis.dir[1] * howFar);
      item.z = Math.round(at[2] + axis.dir[2] * howFar);
      draw();
      onMoved(item);
      return item;
    },

    endDrag() {
      gizmoAxis = null;
      gizmoFrom = null;
      draw();
    },

    /// True while an arrow is being dragged, so the caller leaves the camera alone.
    dragging() { return Boolean(gizmoAxis); },

    /// Whether a handle is under the cursor, for the pointer shape; the plane handle lights up.
    hovering(px, py) {
      const axis = axisAt(px, py);
      const over = axis ? axis.name : null;
      if ((over === 'xz') !== (gizmoHover === 'xz')) { gizmoHover = over; draw(); } else gizmoHover = over;
      return Boolean(axis);
    },

    onMove(callback) { onMoved = callback || (() => {}); },

    /// Frames something, the way F does in a scene view. How far back to stand comes
    /// from the model's own size where it is known, so a villager fills the view and a
    /// building does not fall out of it - a fixed distance did one or the other.
    focus(item) {
      if (item.index !== undefined && item.index >= 0) selected = item.index;
      const entry = item.package && loaded.get(item.package);
      const radius = entry && entry.radius ? entry.radius * (item.scale || 1) : 6;
      const lift = entry && entry.centre ? entry.centre[1] * (item.scale || 1) : radius;

      centre = [item.x, item.y + lift, item.z];
      distance = Math.max(9, radius * 3.2);
      draw();
    },

    /// The instance nearest the click, in screen space.
    pickAt(px, py) {
      const rect = canvas.getBoundingClientRect();
      const nx = ((px - rect.left) / rect.width) * 2 - 1;
      const ny = 1 - ((py - rect.top) / rect.height) * 2;

      // Anything the click actually lands on, nearest to the camera first - so
      // clicking where two characters overlap picks the one in front rather than
      // whichever happens to have its feet closer to the cursor.
      const eye = eyePosition();
      let best = null;
      let bestKind = null;
      let bestDepth = Infinity;
      let bestReach = Infinity;

      for (const item of instances) {
        const bounds = boundsOf(item);
        const middle = projectPoint(bounds.at);
        if (!middle) continue;

        const reach = Math.max(screenRadius(bounds.at, bounds.radius), 0.02);
        if (Math.hypot(middle.x - nx, middle.y - ny) > reach) continue;

        const depth = Math.hypot(
          bounds.at[0] - eye[0], bounds.at[1] - eye[1], bounds.at[2] - eye[2]);
        if (depth < bestDepth) {
          bestDepth = depth;
          best = item;
          bestKind = 'object';
        }
      }

      // The mod's objects: one with a model is as big as the model; the rest are small, so
      // their box is the target.
      points.forEach((point, index) => {
        const drawn = point.package && loaded.get(point.package);
        const bounds = drawn ? boundsOf(point) : { at: [point.x, point.y + 3, point.z], radius: 3.5 };
        const at = bounds.at;
        const middle = projectPoint(at);
        if (!middle) return;
        const reach = Math.max(screenRadius(at, bounds.radius), 0.02);
        if (Math.hypot(middle.x - nx, middle.y - ny) > reach) return;
        const depth = Math.hypot(at[0] - eye[0], at[1] - eye[1], at[2] - eye[2]);
        // Among the mod's objects the smallest target under the cursor wins: a crate standing
        // on a ground slab is what a click on the crate means, whatever their centres' depths.
        const smaller = bestKind === 'point' && reach < bestReach;
        if (smaller || (bestKind !== 'point' && depth < bestDepth)) {
          bestDepth = depth;
          bestReach = reach;
          best = { index, name: point.name, x: point.x, y: point.y, z: point.z };
          bestKind = 'point';
        }
      });

      // Nothing hit: fall back to whatever is nearest the cursor, so a click that
      // just misses something small still lands on it.
      if (!best) {
        let bestDistance = 0.06;
        for (const item of instances) {
          const at = projectPoint(boundsOf(item).at);
          if (!at) continue;
          const away = Math.hypot(at.x - nx, at.y - ny);
          if (away < bestDistance) {
            bestDistance = away;
            best = item;
            bestKind = 'object';
          }
        }
      }

      if (best) {
        selected = bestKind === 'object' ? best.index : null;
        selectedExit = bestKind === 'exit' ? best.index : null;
        selectedPoint = bestKind === 'point' ? best.index : null;
        draw();
        onPick(best, bestKind);
      } else if (selected !== null || selectedExit !== null || selectedPoint !== null) {
        // A click on nothing takes the selection off; the page hears it as a pick of nothing.
        selected = selectedExit = selectedPoint = null;
        draw();
        onPick(null, null);
      }
      return best;
    },

    /// The objects of an OpenFF project's scene file, in world terms, drawn and movable
    /// like the rest; one with a model (package) fetches it and is drawn as it.
    setPoints(list) {
      points = list || [];
      if (selectedPoint !== null && selectedPoint >= points.length) selectedPoint = null;
      for (const point of points) {
        if (point.package && !loaded.has(point.package)) ensure(point.package).catch(() => {});
      }
      draw();
    },

    selectPoint(index) {
      selectedPoint = index;
      selected = null;
      selectedExit = null;
      draw();
    },

    focusPoint(index) {
      const point = points[index];
      if (!point) return;
      selectedPoint = index;
      selected = null;
      selectedExit = null;
      const entry = point.package && loaded.get(point.package);
      const radius = entry && entry.radius ? entry.radius * (point.scale || 1) : 6;
      const lift = entry && entry.centre ? entry.centre[1] * (point.scale || 1) : 4;
      centre = [point.x, point.y + lift, point.z];
      distance = Math.max(30, radius * 3.2);
      draw();
    },

    /// Where the camera looks, for putting something new in view.
    viewCentre() { return centre.slice(); },

    orbit(dx, dy) {
      yaw -= dx * 0.008;
      pitch = Math.max(-1.4, Math.min(1.5, pitch + dy * 0.008));
      draw();
    },

    pan(dx, dy) {
      const cy = Math.cos(yaw), sy = Math.sin(yaw);
      const step = distance * 0.0016;
      centre[0] -= (cy * dx - sy * dy) * step;
      centre[2] -= (sy * dx + cy * dy) * step;
      draw();
    },

    zoom(amount) {
      distance = Math.max(10, Math.min(6000, distance * Math.pow(1.12, amount)));
      draw();
    },

    reset() {
      yaw = 0.7;
      pitch = 0.9;
      const terrain = scene && scene.terrain && loaded.get(scene.terrain);
      if (terrain && terrain.centre) {
        centre = terrain.centre.slice();
        distance = Math.max(80, terrain.radius * 2.6);
      }
      frameField();
      draw();
    },

    /// Flips a field between FF3's chip layout and the same grid mirrored along z.
    setMirrorZ(on) { mirrorZ = !!on; frameField(); draw(); },
    get mirrorZ() { return mirrorZ; },
    get isField() { return !!(scene && scene.field); },

    redraw: draw
  };
}
