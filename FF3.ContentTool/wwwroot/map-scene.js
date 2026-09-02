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
  let onPick = () => {};

  const arrowBuffer = gl.createBuffer();
  const arrow = arrowGeometry();
  gl.bindBuffer(gl.ARRAY_BUFFER, arrowBuffer);
  gl.bufferData(gl.ARRAY_BUFFER, arrow, gl.STATIC_DRAW);
  const arrowVertices = arrow.length / 8;

  let gizmoOn = true;
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
  }

  function selectedItem() {
    return instances.find(o => o.index === selected) || null;
  }

  /// How big an arrow has to be to stay about the same size on screen.
  function gizmoSize() {
    return distance * 0.13;
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
    for (const axis of AXES) {
      const lit = gizmoAxis === axis.name;
      gl.uniform3fv(uniform.tint, lit ? [1, 0.95, 0.5] : axis.colour);
      gl.uniformMatrix4fv(uniform.model, false, axisMatrix(axis.name, at, gizmoSize()));
      gl.drawArrays(gl.TRIANGLES, 0, arrowVertices);
    }
    gl.enable(gl.DEPTH_TEST);
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

  /// Which arrow is under the cursor, if any.
  function axisAt(px, py) {
    const item = gizmoOn && selectedItem();
    if (!item) return null;

    const rect = canvas.getBoundingClientRect();
    const mouse = [px - rect.left, py - rect.top];
    const at = [item.x, item.y, item.z];
    const origin = toScreen(at);
    if (!origin) return null;

    let best = null;
    let bestAway = 11;                    // pixels; a fat enough target to hit
    for (const axis of AXES) {
      const tip = toScreen([
        at[0] + axis.dir[0] * gizmoSize(),
        at[1] + axis.dir[1] * gizmoSize(),
        at[2] + axis.dir[2] * gizmoSize()
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
    const m = cameraMatrix();
    const x = item.x, y = item.y, z = item.z;
    const w = m[3] * x + m[7] * y + m[11] * z + m[15];
    if (w <= 0) return null;
    return {
      x: (m[0] * x + m[4] * y + m[8] * z + m[12]) / w,
      y: (m[1] * x + m[5] * y + m[9] * z + m[13]) / w
    };
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

    select(index) { selected = index; draw(); },

    setGizmo(on) { gizmoOn = on; if (!on) gizmoAxis = null; draw(); },

    /// True if the press landed on an arrow, in which case the camera stays put.
    beginDrag(px, py) {
      const axis = axisAt(px, py);
      if (!axis) return false;
      const item = selectedItem();
      gizmoAxis = axis.name;
      gizmoFrom = { px, py, x: item.x, y: item.y, z: item.z };
      draw();
      return true;
    },

    dragTo(px, py) {
      if (!gizmoAxis || !gizmoFrom) return null;
      const item = selectedItem();
      if (!item) return null;

      const axis = AXES.find(a => a.name === gizmoAxis);
      const at = [gizmoFrom.x, gizmoFrom.y, gizmoFrom.z];
      const origin = toScreen(at);
      const tip = toScreen([
        at[0] + axis.dir[0] * gizmoSize(),
        at[1] + axis.dir[1] * gizmoSize(),
        at[2] + axis.dir[2] * gizmoSize()
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
      const howFar = ((mx * ax + my * ay) / along) * gizmoSize();

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

    focus(item) {
      selected = item.index;
      centre = [item.x, item.y, item.z];
      distance = 90;
      draw();
    },

    /// The instance nearest the click, in screen space.
    pickAt(px, py) {
      const rect = canvas.getBoundingClientRect();
      const nx = ((px - rect.left) / rect.width) * 2 - 1;
      const ny = 1 - ((py - rect.top) / rect.height) * 2;

      let best = null;
      let bestDistance = 0.06;             // a click has to land reasonably close
      for (const item of instances) {
        const at = project(item);
        if (!at) continue;
        const away = Math.hypot(at.x - nx, at.y - ny);
        if (away < bestDistance) {
          bestDistance = away;
          best = item;
        }
      }
      if (best) {
        selected = best.index;
        draw();
        onPick(best);
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
