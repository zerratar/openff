// A viewer for the MDL0 models.
//
// Plain WebGL, no library. The server has already done the hard part - display lists
// walked, strips expanded, node matrices applied - so what arrives is a vertex buffer,
// an index buffer, and groups saying which range uses which texture. All this has to do
// is put a camera in front of it.
//
// There is no lighting. The models carry no usable normals and the game does not light
// them either: what you see in FF3 is the texture and the material colour, so that is
// what this draws. A model that looks flat here looks flat in the game.

'use strict';

const MODEL_VERTEX = `
attribute vec3 position;
attribute vec2 coord;
attribute vec3 colour;
uniform mat4 camera;
uniform mat4 model;
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  gl_Position = camera * model * vec4(position, 1.0);
  vCoord = coord;
  vColour = colour;
}`;

const MODEL_FRAGMENT = `
precision mediump float;
uniform sampler2D picture;
uniform bool textured;
uniform vec3 tint;
uniform float alpha;
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  vec4 c = textured ? texture2D(picture, vCoord) : vec4(1.0);
  if (c.a < 0.05) discard;
  gl_FragColor = vec4(c.rgb * vColour * tint, c.a * alpha);
}`;

function makeModelViewer(canvas, status, options = {}) {
  // preserveDrawingBuffer is off by default because it costs something on every frame;
  // the thumbnail maker turns it on, since reading the canvas back without it gives an
  // empty picture - the browser is free to discard the buffer after each frame.
  const gl = canvas.getContext('webgl', {
    antialias: true,
    alpha: true,
    preserveDrawingBuffer: Boolean(options.preserveDrawingBuffer)
  });
  if (!gl) {
    status('this browser has no WebGL, so models cannot be drawn', 'bad');
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

  const vertexBuffer = gl.createBuffer();
  const indexBuffer = gl.createBuffer();
  const blank = solidTexture(gl, [255, 255, 255, 255]);

  // Models can hold more than 65535 vertices, so 32-bit indices are needed where the
  // browser has them. Every current browser does; the fallback keeps old ones honest.
  const bigIndices = gl.getExtension('OES_element_index_uint');
  const indexType = bigIndices ? gl.UNSIGNED_INT : gl.UNSIGNED_SHORT;
  const indexSize = bigIndices ? 4 : 2;

  let bundle = null;
  let textures = new Map();
  let showHidden = false;

  // Camera: an orbit, in radians and model units.
  let yaw = 0.6;
  let pitch = 0.5;
  let distance = 3;
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

  function draw() {
    resize();
    gl.clearColor(0.09, 0.10, 0.12, 1);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    // Both sides are drawn: the models are single-sided in places and winding is not
    // consistent enough across strips to cull on.
    gl.disable(gl.CULL_FACE);

    if (!bundle || !bundle.groups) return;

    gl.useProgram(program);
    gl.uniformMatrix4fv(uniform.camera, false, cameraMatrix());

    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
    const stride = 8 * 4;
    bind(attribute.position, 3, 0);
    bind(attribute.coord, 2, 3 * 4);
    bind(attribute.colour, 3, 5 * 4);

    // The game draws the model twice - everything opaque, then everything
    // translucent - and that ordering is not cosmetic. Drawn in one pass, a
    // half-transparent quad writes depth and hides whatever is behind it, which is
    // where the holes in the terrain came from.
    for (let pass = 0; pass < 2; pass++) {
    gl.depthMask(pass === 0);
    for (const group of bundle.groups) {
      if (!group.count) continue;
      if (group.hidden && !showHidden) continue;
      if (Boolean(group.translucent) !== (pass === 1)) continue;

      const texture = group.texture ? textures.get(group.texture) : null;
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, texture || blank);
      gl.uniform1i(uniform.picture, 0);
      gl.uniform1i(uniform.textured, texture ? 1 : 0);

      const tint = group.hidden
        ? [1, 0.4, 0.4]
        : (texture ? [1, 1, 1] : rgb(group.colour));
      gl.uniform3fv(uniform.tint, tint);
      gl.uniform1f(uniform.alpha, group.hidden ? 0.5 : (group.alpha ?? 1));
      gl.uniformMatrix4fv(uniform.model, false, billboardMatrix(group));

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

  // A billboard node has its rotation post-multiplied by the inverse camera, so it
  // ends up facing the viewer. The vertices arrive with that already done for a camera
  // at identity, so what is left is to turn the piece about its pivot by however far
  // the camera has moved from there. Kind 2 only turns about the vertical axis, which
  // is what keeps a tree upright rather than tipping over as you look down on it.
  function billboardMatrix(group) {
    if (!group.billboard || !group.pivot) return IDENTITY;

    const [px, py, pz] = group.pivot;
    const cy = Math.cos(yaw), sy = Math.sin(yaw);
    const cp = group.billboard === 2 ? 1 : Math.cos(pitch);
    const sp = group.billboard === 2 ? 0 : Math.sin(pitch);

    // Yaw about y, then pitch about the already-turned x axis.
    const r = [
      cy, 0, -sy,
      sy * sp, cp, cy * sp,
      sy * cp, -sp, cy * cp
    ];

    return new Float32Array([
      r[0], r[1], r[2], 0,
      r[3], r[4], r[5], 0,
      r[6], r[7], r[8], 0,
      px - (r[0] * px + r[3] * py + r[6] * pz),
      py - (r[1] * px + r[4] * py + r[7] * pz),
      pz - (r[2] * px + r[5] * py + r[8] * pz),
      1
    ]);
  }

  function cameraMatrix() {
    const aspect = canvas.width / Math.max(1, canvas.height);
    const near = Math.max(0.01, distance * 0.01);
    const far = distance * 20 + 100;
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
    const view = lookAt(eye, centre, [0, 1, 0]);
    return multiply(projection, view);
  }

  return {
    async show(model, packageName) {
      bundle = model;
      textures = new Map();

      if (!model || !model.buffer || !model.buffer.length) {
        draw();
        return;
      }

      gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(model.buffer), gl.STATIC_DRAW);
      gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
      gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,
        bigIndices ? new Uint32Array(model.indices) : new Uint16Array(model.indices),
        gl.STATIC_DRAW);

      centre = model.centre || [0, 0, 0];
      distance = (model.radius || 1) * 3;
      draw();

      // Textures arrive one at a time and each one redraws, so the model appears
      // straight away and fills in rather than waiting on the slowest decode.
      const wanted = [...new Set(model.groups.map(g => g.texture).filter(Boolean))];
      await Promise.all(wanted.map(name => new Promise(done => {
        const image = new Image();
        image.onload = () => {
          textures.set(name, upload(gl, image));
          draw();
          done();
        };
        image.onerror = () => done();
        image.src = `/api/model/texture?name=${encodeURIComponent(packageName)}`
          + `&texture=${encodeURIComponent(name)}`;
      })));
      draw();
    },

    orbit(dx, dy) {
      yaw -= dx * 0.01;
      pitch = Math.max(-1.5, Math.min(1.5, pitch + dy * 0.01));
      draw();
    },

    zoom(amount) {
      distance = Math.max(0.05, distance * Math.pow(1.1, amount));
      draw();
    },

    reset() {
      yaw = 0.6;
      pitch = 0.5;
      if (bundle) {
        centre = bundle.centre || [0, 0, 0];
        distance = (bundle.radius || 1) * 3;
      }
      draw();
    },

    setShowHidden(on) { showHidden = on; draw(); },
    redraw: draw
  };
}

// ------------------------------------------------------------------ plumbing

const IDENTITY = new Float32Array([1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]);

function link(gl, vertexSource, fragmentSource) {
  const program = gl.createProgram();
  gl.attachShader(program, compile(gl, gl.VERTEX_SHADER, vertexSource));
  gl.attachShader(program, compile(gl, gl.FRAGMENT_SHADER, fragmentSource));
  gl.linkProgram(program);
  if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
    throw new Error(gl.getProgramInfoLog(program));
  }
  return program;
}

function compile(gl, kind, source) {
  const shader = gl.createShader(kind);
  gl.shaderSource(shader, source);
  gl.compileShader(shader);
  if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
    throw new Error(gl.getShaderInfoLog(shader));
  }
  return shader;
}

function solidTexture(gl, rgba) {
  const texture = gl.createTexture();
  gl.bindTexture(gl.TEXTURE_2D, texture);
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 1, 1, 0, gl.RGBA, gl.UNSIGNED_BYTE,
    new Uint8Array(rgba));
  return texture;
}

function upload(gl, image) {
  const texture = gl.createTexture();
  gl.bindTexture(gl.TEXTURE_2D, texture);
  // No flip. The game decodes a texture top row first and uploads it as it is, so
  // v = 0 is the top of the picture - and the shapes pass v straight through. This
  // was flipping, which did far more than mirror each quad: most of these textures
  // are atlases whose halves hold different things, so a mirrored v made every quad
  // sample the wrong part of its own atlas.
  gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);

  // Textures repeat far more often than they clamp, and non-power-of-two sizes cannot
  // repeat in WebGL 1 - but every NDS texture is a power of two, so this is safe.
  const power = (n) => (n & (n - 1)) === 0;
  if (power(image.width) && power(image.height)) {
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.REPEAT);
  } else {
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  }

  // The game asks for 9728 - GL_NEAREST - on both filters, and repeat on both axes
  // whatever the material says, so that is what happens here. Nearest matters more
  // than it sounds: these textures are atlases, and a linear filter bleeds one cell
  // into the next along every seam.
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
  return texture;
}

function rgb(value) {
  return [((value >> 16) & 255) / 255, ((value >> 8) & 255) / 255, (value & 255) / 255];
}

function lookAt(eye, at, up) {
  const z = normalise([eye[0] - at[0], eye[1] - at[1], eye[2] - at[2]]);
  const x = normalise(cross(up, z));
  const y = cross(z, x);
  return [
    x[0], y[0], z[0], 0,
    x[1], y[1], z[1], 0,
    x[2], y[2], z[2], 0,
    -dot(x, eye), -dot(y, eye), -dot(z, eye), 1
  ];
}

function multiply(a, b) {
  const out = new Float32Array(16);
  for (let i = 0; i < 4; i++) {
    for (let j = 0; j < 4; j++) {
      out[i * 4 + j] = a[j] * b[i * 4] + a[4 + j] * b[i * 4 + 1]
        + a[8 + j] * b[i * 4 + 2] + a[12 + j] * b[i * 4 + 3];
    }
  }
  return out;
}

function cross(a, b) {
  return [a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0]];
}

function dot(a, b) {
  return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
}

function normalise(v) {
  const length = Math.hypot(v[0], v[1], v[2]) || 1;
  return [v[0] / length, v[1] / length, v[2] / length];
}
