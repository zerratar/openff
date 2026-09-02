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
