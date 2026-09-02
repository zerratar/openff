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
    colour: gl.getAttribLocation(program, 'colour')
  };
  const uniform = {
    camera: gl.getUniformLocation(program, 'camera'),
    model: gl.getUniformLocation(program, 'model'),
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

  let gizmoOn = true;
  let gizmoScale = 1;            // how big the arrows are, as a multiplier
  let onFrame = () => {};
  let gizmoMode = 'move';        // 'move' or 'rotate'
  let gizmoAxis = null;          // the axis being dragged, if any
  let gizmoFrom = null;          // where the drag started
  let onMoved = () => {};

  let yaw = 0.7;
  let pitch = 0.9;
  let distance = 400;
  let centre = [0, 0, 0];

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

  /// Place, face and size one instance. Posture is in degrees about the vertical.
  function placement(item) {
    const a = (item.rotationY || 0) * Math.PI / 180;
    const c = Math.cos(a), s = Math.sin(a);
    const k = item.scale || 1;
    return new Float32Array([
      c * k, 0, -s * k, 0,
      0, k, 0, 0,
      s * k, 0, c * k, 0,
      item.x, item.y, item.z, 1
    ]);
  }

  function drawBundle(entry, matrix, tintWith) {
    gl.bindBuffer(gl.ARRAY_BUFFER, entry.vertexBuffer);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, entry.indexBuffer);
    const stride = 8 * 4;
    bind(attribute.position, 3, 0);
    bind(attribute.coord, 2, 3 * 4);
    bind(attribute.colour, 3, 5 * 4);
    gl.uniformMatrix4fv(uniform.model, false, matrix);

    for (let pass = 0; pass < 2; pass++) {
      gl.depthMask(pass === 0);
      for (const group of entry.groups) {
        if (!group.count || group.hidden) continue;
        if (Boolean(group.translucent) !== (pass === 1)) continue;

        const texture = group.texture ? entry.textures.get(group.texture) : null;
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture || blank);
        gl.uniform1i(uniform.picture, 0);
        gl.uniform1i(uniform.textured, texture ? 1 : 0);
        gl.uniform3fv(uniform.tint, tintWith || (texture ? [1, 1, 1] : rgb(group.colour)));
        gl.uniform1f(uniform.alpha, group.alpha ?? 1);
        gl.drawElements(gl.TRIANGLES, group.count, indexType, group.start * indexSize);
      }
    }
    gl.depthMask(true);

    function bind(where, size, offset) {
      if (where < 0) return;
      gl.enableVertexAttribArray(where);
      gl.vertexAttribPointer(where, size, gl.FLOAT, false, stride, offset);
    }
  }

  function draw() {
    resize();
    gl.clearColor(0.09, 0.10, 0.12, 1);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    gl.disable(gl.CULL_FACE);
    if (!scene) return;

    gl.useProgram(program);
    gl.uniformMatrix4fv(uniform.camera, false, cameraMatrix());

    const terrain = scene.terrain && loaded.get(scene.terrain);
    if (terrain) drawBundle(terrain, IDENTITY, null);

    for (const item of instances) {
      const entry = loaded.get(item.package);
      if (!entry) continue;
      // The selected one is washed blue rather than outlined - an outline needs a second
      // pass over the same geometry, and this reads just as clearly against the terrain.
      const tint = selected === item.index ? [0.55, 0.75, 1.35] : null;
      drawBundle(entry, placement(item), tint);
    }

    drawGizmo();
    onFrame();
  }

  function selectedItem() {
    return instances.find(o => o.index === selected) || null;
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
      const a = (item.rotationY || 0) * Math.PI / 180;
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
    }
    gl.enable(gl.DEPTH_TEST);
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
      // atan2(x, z) matches the convention the placement matrix uses for facing.
      angle: Math.atan2(x, z) * 180 / Math.PI,
      away: Math.hypot(x, z)
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
      textures: new Map(), centre: bundle.centre, radius: bundle.radius
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
      image.src = `/api/model/texture?name=${encodeURIComponent(name)}`
        + `&texture=${encodeURIComponent(texture)}`;
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
    const a = (item.rotationY || 0) * Math.PI / 180;
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
      draw();

      await ensure(data.terrain);
      const terrain = data.terrain && loaded.get(data.terrain);
      if (terrain && terrain.centre) {
        centre = terrain.centre.slice();
        distance = Math.max(80, terrain.radius * 2.6);
      }
      draw();

      // One fetch per distinct model, not per instance - a town of twenty villagers
      // usually wears three or four models between them.
      for (const name of new Set(instances.map(o => o.package))) {
        await ensure(name);
      }
      draw();
    },

    select(index) { selected = index; selectedExit = null; draw(); },

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

    selectExit(index) { selectedExit = index; selected = null; draw(); },

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

    setGizmoScale(scale) { gizmoScale = scale; draw(); },

    /// Called after every frame, so the page can put its tags where things are now.
    onFrame(callback) { onFrame = callback || (() => {}); },

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
      if (axis.name === 'turn') {
        const ground = onGround(px, py, [item.x, item.y, item.z]);
        if (!ground) {
          gizmoAxis = null;
          gizmoFrom = null;
          return false;
        }
        gizmoFrom.angle = ground.angle;
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
        let turned = Math.round(gizmoFrom.rotationY + (ground.angle - gizmoFrom.angle));
        turned = ((turned % 360) + 360) % 360;
        item.rotationY = turned;
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

    /// Whether an arrow is under the cursor, for the pointer shape.
    hovering(px, py) { return Boolean(axisAt(px, py)); },

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
        draw();
        onPick(best, bestKind);
      }
      return best;
    },

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
      draw();
    },

    redraw: draw
  };
}
