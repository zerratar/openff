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

// Each vertex names which of its group's matrices moved it - the node's own, or a
// stack slot its display list restored - and the palette holds those matrices for the
// group being drawn, multiplied by the current pose. With no pose every entry is the
// billboard matrix (or identity), which is exactly what it drew before.
const PALETTE = 24;
const MODEL_VERTEX = `
attribute vec3 position;
attribute vec2 coord;
attribute vec3 colour;
attribute float mindex;
uniform mat4 camera;
uniform mat4 palette[${PALETTE}];
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  mat4 model = palette[int(mindex + 0.5)];
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
    colour: gl.getAttribLocation(program, 'colour'),
    mindex: gl.getAttribLocation(program, 'mindex')
  };
  const uniform = {
    camera: gl.getUniformLocation(program, 'camera'),
    palette: gl.getUniformLocation(program, 'palette'),
    picture: gl.getUniformLocation(program, 'picture'),
    textured: gl.getUniformLocation(program, 'textured'),
    tint: gl.getUniformLocation(program, 'tint'),
    alpha: gl.getUniformLocation(program, 'alpha')
  };

  const vertexBuffer = gl.createBuffer();
  const indexBuffer = gl.createBuffer();
  const indexAttrBuffer = gl.createBuffer();

  // The pose being shown: {counts, matrices, frames} from /api/model/pose, and the
  // frame of it. Null draws the bind pose.
  let pose = null;
  let poseFrame = 0;
  let poseOffsets = [];
  const paletteData = new Float32Array(16 * PALETTE);
  const blank = solidTexture(gl, [255, 255, 255, 255]);

  // Models can hold more than 65535 vertices, so 32-bit indices are needed where the
  // browser has them. Every current browser does; the fallback keeps old ones honest.
  const bigIndices = gl.getExtension('OES_element_index_uint');
  const indexType = bigIndices ? gl.UNSIGNED_INT : gl.UNSIGNED_SHORT;
  const indexSize = bigIndices ? 4 : 2;

  let bundle = null;
  let textures = new Map();
  let showHidden = false;

  // A second model under the first - a character the viewed weapon sits on - with its own
  // buffers, textures and pose; and the matrix that puts the viewed model where it goes
  // (the hand joint, the grip's turn and offset, the item's own fit). Null for neither.
  let companion = null;
  let attach = null;

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

    // The character first, then the weapon on it: both go through the same depth buffer,
    // so a blade behind the arm is hidden by the arm as in the game.
    if (companion && companion.bundle && companion.bundle.groups) {
      drawModel(companion.bundle, companion.vertexBuffer, companion.indexBuffer, companion.indexAttrBuffer, companion.textures,
        companion.pose, companion.poseFrame, companion.poseOffsets, null);
    }
    drawModel(bundle, vertexBuffer, indexBuffer, indexAttrBuffer, textures, pose, poseFrame, poseOffsets, attach);
  }

  function drawModel(model, vertices, indices, mindices, pictures, thePose, theFrame, theOffsets, world) {
    gl.bindBuffer(gl.ARRAY_BUFFER, vertices);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indices);
    const stride = 8 * 4;
    bind(attribute.position, 3, 0);
    bind(attribute.coord, 2, 3 * 4);
    bind(attribute.colour, 3, 5 * 4);
    if (attribute.mindex >= 0) {
      gl.bindBuffer(gl.ARRAY_BUFFER, mindices);
      gl.enableVertexAttribArray(attribute.mindex);
      gl.vertexAttribPointer(attribute.mindex, 1, gl.FLOAT, false, 4, 0);
      gl.bindBuffer(gl.ARRAY_BUFFER, vertices);
    }

    // The game draws the model twice - everything opaque, then everything
    // translucent - and that ordering is not cosmetic. Drawn in one pass, a
    // half-transparent quad writes depth and hides whatever is behind it, which is
    // where the holes in the terrain came from.
    for (let pass = 0; pass < 2; pass++) {
    gl.depthMask(pass === 0);
    for (const group of model.groups) {
      if (!group.count) continue;
      if (group.hidden && !showHidden) continue;
      if (Boolean(group.translucent) !== (pass === 1)) continue;

      const texture = group.texture ? pictures.get(group.texture) : null;
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, texture || blank);
      if (texture) applyWrap(gl, group);
      gl.uniform1i(uniform.picture, 0);
      gl.uniform1i(uniform.textured, texture ? 1 : 0);

      const tint = group.hidden
        ? [1, 0.4, 0.4]
        : (texture ? [1, 1, 1] : rgb(group.colour));
      gl.uniform3fv(uniform.tint, tint);
      gl.uniform1f(uniform.alpha, group.hidden ? 0.5 : (group.alpha ?? 1));
      gl.uniformMatrix4fv(uniform.palette, false, paletteFor(group, thePose, theFrame, theOffsets, world));

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

  /// The group's matrices for this frame, each times the billboard turn, in the
  /// order the vertices index them. Entries the group does not use stay whatever
  /// they were; nothing reads them.
  function paletteFor(group, thePose, theFrame, theOffsets, world) {
    const board = billboardMatrix(group);
    const count = Math.min(PALETTE, (group.matrices && group.matrices.length) || 1);
    for (let i = 0; i < count; i++) {
      let m = board;
      if (thePose && group.piece !== undefined && theOffsets[group.piece] !== undefined) {
        const stride = theOffsets[theOffsets.length - 1];
        const at = (theFrame * stride + theOffsets[group.piece] + i) * 12;
        const d = thePose.matrices;
        if (at + 12 <= d.length) {
          const delta = [
            d[at], d[at + 1], d[at + 2], 0,
            d[at + 3], d[at + 4], d[at + 5], 0,
            d[at + 6], d[at + 7], d[at + 8], 0,
            d[at + 9], d[at + 10], d[at + 11], 1
          ];
          m = board === IDENTITY ? delta : multiply(board, delta);
        }
      }
      // Into the hand: the attach matrix moves the whole model after its own pose.
      if (world) m = multiply(world, m);
      paletteData.set(m, i * 16);
    }
    return paletteData;
  }

  /// Vertex, index and matrix-index buffers for a bundle, uploaded.
  function uploadModel(model, vertices, indices, mindices) {
    gl.bindBuffer(gl.ARRAY_BUFFER, vertices);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(model.buffer), gl.STATIC_DRAW);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indices);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,
      bigIndices ? new Uint32Array(model.indices) : new Uint16Array(model.indices),
      gl.STATIC_DRAW);
    const vertexCount = model.buffer.length / 8;
    const slots = new Float32Array(vertexCount);
    if (model.matrixIndex && model.matrixIndex.length === vertexCount) slots.set(model.matrixIndex);
    gl.bindBuffer(gl.ARRAY_BUFFER, mindices);
    gl.bufferData(gl.ARRAY_BUFFER, slots, gl.STATIC_DRAW);
  }

  /// The textures a bundle names, fetched one at a time with a redraw each.
  async function loadTextures(model, packageName, into) {
    const wanted = [...new Set(model.groups.map(g => g.texture).filter(Boolean))];
    await Promise.all(wanted.map(name => new Promise(done => {
      const image = new Image();
      image.onload = () => {
        into.set(name, upload(gl, image));
        draw();
        done();
      };
      image.onerror = () => done();
      image.src = wsUrl(`/api/model/texture?name=${encodeURIComponent(packageName)}`
        + `&texture=${encodeURIComponent(name)}`);
    })));
    draw();
  }

  function offsetsOf(next) {
    const offsets = [];
    if (next && next.counts) {
      let sum = 0;
      for (const count of next.counts) { offsets.push(sum); sum += count; }
      offsets.push(sum);            // the total: one frame's stride in matrices
    }
    return offsets;
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

      uploadModel(model, vertexBuffer, indexBuffer, indexAttrBuffer);
      pose = null;
      poseFrame = 0;

      centre = model.centre || [0, 0, 0];
      distance = (model.radius || 1) * 3;
      draw();

      // Textures arrive one at a time and each one redraws, so the model appears
      // straight away and fills in rather than waiting on the slowest decode.
      await loadTextures(model, packageName, textures);
    },

    /// A character under the viewed model (null takes it away): its bundle, drawn with its
    /// own pose (setCompanionPose / setCompanionFrame). The view frames the character.
    async setCompanion(model, packageName) {
      if (!model) {
        companion = null;
        if (bundle) { centre = bundle.centre || [0, 0, 0]; distance = (bundle.radius || 1) * 3; }
        draw();
        return;
      }
      companion = {
        bundle: model, textures: new Map(),
        vertexBuffer: gl.createBuffer(), indexBuffer: gl.createBuffer(), indexAttrBuffer: gl.createBuffer(),
        pose: null, poseFrame: 0, poseOffsets: []
      };
      uploadModel(model, companion.vertexBuffer, companion.indexBuffer, companion.indexAttrBuffer);
      centre = model.centre || [0, 0, 0];
      distance = (model.radius || 1) * 3;
      draw();
      await loadTextures(model, packageName, companion.textures);
    },

    setCompanionPose(next) {
      if (!companion) return;
      companion.pose = next;
      companion.poseFrame = 0;
      companion.poseOffsets = offsetsOf(next);
      draw();
    },

    setCompanionFrame(frame) {
      if (!companion || !companion.pose) return;
      companion.poseFrame = Math.max(0, Math.min(companion.pose.frames - 1, Math.floor(frame)));
      draw();
    },

    /// Where the viewed model goes, as a column-major 4x4 (null: where it is).
    setAttach(matrix) { attach = matrix || null; draw(); },

    hasCompanion() { return Boolean(companion); },
    attach() { return attach; },

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
      const framed = companion ? companion.bundle : bundle;
      if (framed) {
        centre = framed.centre || [0, 0, 0];
        distance = (framed.radius || 1) * 3;
      }
      draw();
    },

    setShowHidden(on) { showHidden = on; draw(); },

    /// Shows a motion: `next` is the /api/model/pose answer, or null for the bind pose.
    setPose(next) {
      pose = next;
      poseFrame = 0;
      poseOffsets = offsetsOf(next);
      draw();
    },

    setFrame(frame) {
      if (!pose) return;
      poseFrame = Math.max(0, Math.min(pose.frames - 1, Math.floor(frame)));
      draw();
    },

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

/// The wrap a material asks for, set on the bound texture before its group draws. The
/// DS has repeat, clamp and "flip" - repeat with every other copy mirrored - and FF4's
/// world map tiles lean on flip to hide their seams; with plain repeat every other tile
/// shows the wrong half of its texture. Power-of-two sizes only, which NDS textures are.
const WRAP = { repeat: 0x2901, mirror: 0x8370, clamp: 0x812F };
function applyWrap(gl, group) {
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, WRAP[group.wrapS] || gl.REPEAT);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, WRAP[group.wrapT] || gl.REPEAT);
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
