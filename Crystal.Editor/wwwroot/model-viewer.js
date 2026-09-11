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
// The DS keeps 31 matrix stack slots and the current matrix: 32. A model's vertices name any of
// them (j101 uses all 32), and a palette short of that reads past its end in the shader - the
// vertices vanished or flew off, depending on what lay beyond.
const PALETTE = 32;
const MODEL_VERTEX = `
attribute vec3 position;
attribute vec2 coord;
attribute vec3 colour;
attribute float mindex;
uniform mat4 camera;
uniform mat4 palette[${PALETTE}];
uniform float nearer;
varying vec2 vCoord;
varying vec3 vColour;
void main() {
  mat4 model = palette[int(mindex + 0.5)];
  gl_Position = camera * model * vec4(position, 1.0);
  // Lines drawn over the faces they belong to are pulled a little towards the eye, or
  // they would lose the depth test to those faces half the time and stipple.
  gl_Position.z -= nearer * gl_Position.w;
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
    alpha: gl.getUniformLocation(program, 'alpha'),
    nearer: gl.getUniformLocation(program, 'nearer')
  };

  const vertexBuffer = gl.createBuffer();
  const indexBuffer = gl.createBuffer();
  const indexAttrBuffer = gl.createBuffer();
  // The viewed model's edges, each once, for the wireframe.
  const edgeBuffer = gl.createBuffer();
  let edgeCount = 0;
  let edgeRanges = [];           // per group: { group, start, count } into edgeBuffer, in edges

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

  // A skinned glTF driven by a game model's motion (/api/model/rig-pose): the rig's node
  // matrices per frame, the joint -> node map by name, and a copy of the vertex buffer the
  // skinned positions are written into each frame. Null: the file as it is.
  let skinRig = null;
  let skinJointNode = null;
  let skinBuffer = null;
  let skinFrame = -1;
  let skinFitted = false;        // the file's joints sit where its own elbows and knees are, not the game's: retargeted

  // Weight painting on a skinned glTF: the bone shown as a heat map and painted, the brush,
  // and the strokes to undo. The weights live in bundle.skin.jointIndex and bundle.skin.weights
  // (four a vertex) and are what weightsData() hands back for saving. The heat map (heat), the
  // skeleton (bones) and the wireframe (wire) are each their own switch: the skeleton and the
  // wireframe show over the shaded model too, with weights mode off.
  const paint = { on: false, bone: -1, radius: 1, strength: 0.25, mode: 'add', mirror: true, heat: true, bones: false, wire: false };
  const undoStack = [];
  let adjacency = null;          // per vertex, the vertices sharing an edge (twins by position merged)
  let canonical = null;          // per vertex, the first vertex at its position
  const boneBuffer = gl.createBuffer();
  const boneIndexAttr = gl.createBuffer();

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
    gl.uniform1f(uniform.nearer, 0);

    // The character first, then the weapon on it: both go through the same depth buffer,
    // so a blade behind the arm is hidden by the arm as in the game.
    if (companion && companion.bundle && companion.bundle.groups) {
      drawModel(companion.bundle, companion.vertexBuffer, companion.indexBuffer, companion.indexAttrBuffer, companion.textures,
        companion.pose, companion.poseFrame, companion.poseOffsets, null);
    }
    drawModel(bundle, vertexBuffer, indexBuffer, indexAttrBuffer, textures, pose, poseFrame, poseOffsets, attach);
    if (paint.wire) drawWireframe();
    if (paint.bones) drawSkeleton();
    drawCuts();
    drawMarkers();
    drawBrush();
  }

  /// Whether the viewed model's colours are the heat map of the painted bone right now.
  function heatShown() { return paint.on && paint.heat && paint.bone >= 0 && Boolean(bundle && bundle.skin); }

  /// The viewed model's edges over its faces, dark, through the same palette as the faces.
  function drawWireframe() {
    if (!edgeCount) return;
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, edgeBuffer);
    const stride = 8 * 4;
    gl.enableVertexAttribArray(attribute.position); gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
    gl.enableVertexAttribArray(attribute.coord); gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 12);
    gl.enableVertexAttribArray(attribute.colour); gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 20);
    if (attribute.mindex >= 0) {
      gl.bindBuffer(gl.ARRAY_BUFFER, indexAttrBuffer);
      gl.enableVertexAttribArray(attribute.mindex); gl.vertexAttribPointer(attribute.mindex, 1, gl.FLOAT, false, 4, 0);
    }
    gl.bindTexture(gl.TEXTURE_2D, blank);
    gl.uniform1i(uniform.textured, 0);
    gl.uniform3fv(uniform.tint, [0, 0, 0]);
    gl.uniform1f(uniform.alpha, 0.45);
    gl.uniform1f(uniform.nearer, 0.0015);
    // The faces' own vertex colours would tint the lines: the colour attribute is fixed at white for the pass.
    gl.disableVertexAttribArray(attribute.colour);
    gl.vertexAttrib3f(attribute.colour, 1, 1, 1);
    gl.lineWidth(1);
    // Group by group, through each group's own palette, so a posed game model's wireframe poses with it.
    for (const range of edgeRanges) {
      const group = bundle.groups[range.group];
      if (group.hidden && !showHidden) continue;
      gl.uniformMatrix4fv(uniform.palette, false, paletteFor(group, pose, poseFrame, poseOffsets, attach));
      gl.drawElements(gl.LINES, range.count * 2, indexType, range.start * 2 * indexSize);
    }
    gl.uniform1f(uniform.nearer, 0);
    gl.enableVertexAttribArray(attribute.colour);
  }

  /// Each edge of the viewed model once per group (both directions folded together), uploaded.
  function uploadEdges(model) {
    edgeCount = 0;
    edgeRanges = [];
    if (!model || !model.indices || !model.indices.length || !model.groups) return;
    const edges = [];
    const tri = model.indices;
    model.groups.forEach((group, g) => {
      const seen = new Set();
      const from = edges.length / 2;
      for (let t = group.start; t + 2 < group.start + group.count; t += 3) {
        for (let k = 0; k < 3; k++) {
          const a = tri[t + k], b = tri[t + (k + 1) % 3];
          if (a === b) continue;
          const key = a < b ? a * 4294967296 + b : b * 4294967296 + a;
          if (seen.has(key)) continue;
          seen.add(key);
          edges.push(a, b);
        }
      }
      edgeRanges.push({ group: g, start: from, count: edges.length / 2 - from });
    });
    edgeCount = edges.length / 2;
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, edgeBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, bigIndices ? new Uint32Array(edges) : new Uint16Array(edges), gl.STATIC_DRAW);
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

      // In weights mode the heat map is the colour, plain: the texture would tint it.
      const heated = model === bundle && heatShown();
      const texture = !heated && group.texture ? pictures.get(group.texture) : null;
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, texture || blank);
      if (texture) applyWrap(gl, group);
      gl.uniform1i(uniform.picture, 0);
      gl.uniform1i(uniform.textured, texture ? 1 : 0);

      const tint = group.hidden
        ? [1, 0.4, 0.4]
        : (texture || heated ? [1, 1, 1] : rgb(group.colour));
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

  /// The skinned glTF at a frame of the rig's motion: every vertex through its four joints -
  /// the file's inverse bind, then the game node's matrix for the frame (the node the joint is
  /// named after) - written into the vertex buffer. Frame -1 puts the file back as it is.
  /// The joint matrices (file local -> model space) for a frame of the rig's motion, or null for the bind pose.
  function jointMatrices(frame) {
    const skin = bundle && bundle.skin;
    if (!skin || !skinRig || frame < 0) return null;
    return matricesFor(skinRig, skinJointNode, frame);
  }

  /// A 4x4 column-major matrix from 12 floats of a rig's row-vector 4x3 at an offset.
  function mat4At(w, o) { return [w[o], w[o + 1], w[o + 2], 0, w[o + 3], w[o + 4], w[o + 5], 0, w[o + 6], w[o + 7], w[o + 8], 0, w[o + 9], w[o + 10], w[o + 11], 1]; }

  /// Whether the file's joints sit where the game's bind pose has them (within a hundredth of
  /// the height). When they do not, the file has a fitted skeleton and is driven by retargeting.
  function jointsFitted(rig, jointNode) {
    const skin = bundle && bundle.skin;
    if (!skin || !rig || !rig.bind) return false;
    const h = bounds ? bounds.max[1] - bounds.min[1] : 1;
    for (let j = 0; j < skin.joints.length; j++) {
      const n = jointNode[j];
      if (n < 0) continue;
      const own = invert4(skin.inverseBind.slice(j * 16, j * 16 + 16));
      if (!own) continue;
      const g = rig.bind, o = n * 12;
      if (Math.hypot(own[12] - g[o + 9], own[13] - g[o + 10], own[14] - g[o + 11]) > 0.01 * h) return true;
    }
    return false;
  }

  /// The skinning matrix per joint at a frame of a rig. Joints where the game's are: the game's
  /// world matrix through the inverse bind. A fitted skeleton (joints moved to the file's own):
  /// retargeted - each joint's local rotation is the game's, changed from its bind as the game's
  /// is at this frame, about the file's own offsets, composed down the tree; a root moves as the
  /// game's does. The game's rotations, the file's proportions.
  function matricesFor(rig, jointNode, frame) {
    const skin = bundle.skin;
    const nodeCount = rig.nodes.length;
    const at = Math.max(0, Math.min(rig.frames - 1, frame)) * nodeCount * 12;
    const joints = skin.joints.length;
    const matrices = new Array(joints);
    if (!skinFitted) {
      for (let j = 0; j < joints; j++) {
        const n = jointNode[j];
        matrices[j] = n < 0 ? null : multiply(mat4At(rig.worlds, at + n * 12), skin.inverseBind.slice(j * 16, j * 16 + 16));
      }
      return matrices;
    }
    // Joint by node, the tree from the rig, the file's bind worlds from its inverse binds.
    const jointOfNode = new Map();
    jointNode.forEach((n, j) => { if (n >= 0 && !jointOfNode.has(n)) jointOfNode.set(n, j); });
    const ownBind = new Array(joints);
    for (let j = 0; j < joints; j++) ownBind[j] = invert4(skin.inverseBind.slice(j * 16, j * 16 + 16));
    const rot = (m) => [m[0], m[1], m[2], 0, m[4], m[5], m[6], 0, m[8], m[9], m[10], 0, 0, 0, 0, 1];
    const world = new Array(joints).fill(null);
    const build = (j) => {
      if (world[j]) return world[j];
      const n = jointNode[j];
      if (n < 0 || !ownBind[j]) return null;
      const gameNow = mat4At(rig.worlds, at + n * 12), gameBind = mat4At(rig.bind, n * 12);
      const parentNode = rig.parents[n];
      const pj = parentNode >= 0 && jointOfNode.has(parentNode) ? jointOfNode.get(parentNode) : -1;
      if (pj < 0) {
        // A root: the game's turn since its bind, and its move, about the file's own root.
        const m = multiply(multiply(rot(gameNow), invert4(rot(gameBind))), rot(ownBind[j]));
        m[12] = ownBind[j][12] + gameNow[12] - gameBind[12]; m[13] = ownBind[j][13] + gameNow[13] - gameBind[13]; m[14] = ownBind[j][14] + gameNow[14] - gameBind[14];
        return world[j] = m;
      }
      const parentWorld = build(pj);
      if (!parentWorld) return null;
      const gameParentNow = mat4At(rig.worlds, at + parentNode * 12), gameParentBind = mat4At(rig.bind, parentNode * 12);
      const localNow = multiply(invert4(gameParentNow), gameNow), localBind = multiply(invert4(gameParentBind), gameBind);
      const delta = multiply(rot(localNow), invert4(rot(localBind)));
      const ownLocal = multiply(invert4(ownBind[pj]), ownBind[j]);
      const local = multiply(delta, rot(ownLocal));
      local[12] = ownLocal[12] + localNow[12] - localBind[12]; local[13] = ownLocal[13] + localNow[13] - localBind[13]; local[14] = ownLocal[14] + localNow[14] - localBind[14];
      return world[j] = multiply(parentWorld, local);
    };
    for (let j = 0; j < joints; j++) { const w = build(j); matrices[j] = w ? multiply(w, skin.inverseBind.slice(j * 16, j * 16 + 16)) : null; }
    return matrices;
  }

  /// The heat of a weight: blue for none through green to red for all of it.
  /// A bone's own colour for the all-bones view: hues spread by the golden angle so neighbours in
  /// the list differ, mirrored bones (L_/R_) a shade apart. Bright and saturated, dimmed by weight.
  function boneColour(j) {
    const names = bundle && bundle.skin ? bundle.skin.joints : [];
    const name = names[j] || '';
    // The same hue for L_x and R_x, the right side a little lighter, so the two sides read as a pair.
    const key = name.startsWith('L_') || name.startsWith('R_') ? names.findIndex(n => n === 'L_' + name.slice(2)) : j;
    const hue = ((key < 0 ? j : key) * 137.508) % 360;
    const light = name.startsWith('R_') ? 0.7 : 0.55;
    return hsl(hue / 360, 0.85, light);
  }
  function hsl(h, s, l) {
    const q = l < 0.5 ? l * (1 + s) : l + s - l * s, p = 2 * l - q;
    const f = t => { t = (t + 1) % 1; if (t < 1 / 6) return p + (q - p) * 6 * t; if (t < 1 / 2) return q; if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6; return p; };
    return [f(h + 1 / 3), f(h), f(h - 1 / 3)];
  }

  function heat(w) {
    if (w <= 0) return [0.16, 0.18, 0.55];
    if (w < 0.5) { const t = w / 0.5; return [0.16 * (1 - t), 0.18 + 0.62 * t, 0.55 * (1 - t) + 0.2 * t]; }
    const t = (w - 0.5) / 0.5;
    return [t, 0.8 * (1 - t) + 0.15 * t, 0.2 * (1 - t)];
  }

  /// The skinned glTF's buffer for a frame (-1: the bind pose): positions through the joints, and
  /// in weights mode the colours as a heat map of the painted bone; uploaded.
  function skinTo(frame) {
    const skin = bundle && bundle.skin;
    if (!skin) return;
    skinFrame = frame;
    const matrices = jointMatrices(frame);
    const heated = heatShown();
    if (!matrices && !heated) {
      skinBuffer = null;
      gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(bundle.buffer), gl.STATIC_DRAW);
      return;
    }
    if (!skinBuffer) skinBuffer = new Float32Array(bundle.buffer);
    const count = bundle.buffer.length / 8;
    const local = skin.local, index = skin.jointIndex, weight = skin.weights, base = bundle.buffer;
    for (let v = 0; v < count; v++) {
      if (matrices) {
        const x = local[v * 3], y = local[v * 3 + 1], z = local[v * 3 + 2];
        let px = 0, py = 0, pz = 0, total = 0;
        for (let k = 0; k < 4; k++) {
          const wk = weight[v * 4 + k];
          const j = index[v * 4 + k];
          if (wk <= 0 || j < 0 || !matrices[j]) continue;
          const m = matrices[j];
          px += wk * (m[0] * x + m[4] * y + m[8] * z + m[12]);
          py += wk * (m[1] * x + m[5] * y + m[9] * z + m[13]);
          pz += wk * (m[2] * x + m[6] * y + m[10] * z + m[14]);
          total += wk;
        }
        if (total <= 0) { skinBuffer[v * 8] = base[v * 8]; skinBuffer[v * 8 + 1] = base[v * 8 + 1]; skinBuffer[v * 8 + 2] = base[v * 8 + 2]; }
        else { skinBuffer[v * 8] = px / total; skinBuffer[v * 8 + 1] = py / total; skinBuffer[v * 8 + 2] = pz / total; }
      } else {
        skinBuffer[v * 8] = base[v * 8]; skinBuffer[v * 8 + 1] = base[v * 8 + 1]; skinBuffer[v * 8 + 2] = base[v * 8 + 2];
      }
      if (heated && paint.heatAll) {
        // Every bone at once: the vertex in its heaviest bone's colour, dimmer the less it has of it.
        let best = -1, most = 0;
        for (let k = 0; k < 4; k++) if (weight[v * 4 + k] > most) { most = weight[v * 4 + k]; best = index[v * 4 + k]; }
        const c = best >= 0 ? boneColour(best) : [0.2, 0.2, 0.2];
        const dim = 0.45 + 0.55 * most;
        skinBuffer[v * 8 + 5] = c[0] * dim; skinBuffer[v * 8 + 6] = c[1] * dim; skinBuffer[v * 8 + 7] = c[2] * dim;
      } else if (heated) {
        let w = 0;
        for (let k = 0; k < 4; k++) if (index[v * 4 + k] === paint.bone) w += weight[v * 4 + k];
        const c = heat(w);
        skinBuffer[v * 8 + 5] = c[0]; skinBuffer[v * 8 + 6] = c[1]; skinBuffer[v * 8 + 7] = c[2];
      } else {
        skinBuffer[v * 8 + 5] = base[v * 8 + 5]; skinBuffer[v * 8 + 6] = base[v * 8 + 6]; skinBuffer[v * 8 + 7] = base[v * 8 + 7];
      }
    }
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, skinBuffer, gl.DYNAMIC_DRAW);
  }

  /// The vertex positions as drawn now: the skinned copy, or the file's.
  function currentPositions() { return skinBuffer || bundle.buffer; }

  /// The skeleton this frame: each joint's position (the game node's, or the bind's from the inverse bind), lines to the parents.
  function drawSkeleton() {
    const skin = bundle && bundle.skin;
    if (!skin) return;
    const joints = skin.joints.length;
    const positions = new Array(joints);
    if (skinRig && skinFrame >= 0 && skinFitted) {
      // The fitted skeleton as posed: each joint's world is its skinning matrix through its own bind.
      const matrices = jointMatrices(skinFrame);
      for (let j = 0; j < joints; j++) {
        const own = invert4(skin.inverseBind.slice(j * 16, j * 16 + 16));
        const w = matrices && matrices[j] && own ? multiply(matrices[j], own) : null;
        positions[j] = w ? [w[12], w[13], w[14]] : null;
      }
    } else if (skinRig && skinFrame >= 0) {
      const nodeCount = skinRig.nodes.length;
      const at = Math.max(0, Math.min(skinRig.frames - 1, skinFrame)) * nodeCount * 12;
      for (let j = 0; j < joints; j++) {
        const n = skinJointNode[j];
        positions[j] = n < 0 ? null : [skinRig.worlds[at + n * 12 + 9], skinRig.worlds[at + n * 12 + 10], skinRig.worlds[at + n * 12 + 11]];
      }
    } else {
      for (let j = 0; j < joints; j++) {
        const inv = invert4(skin.inverseBind.slice(j * 16, j * 16 + 16));
        positions[j] = inv ? [inv[12], inv[13], inv[14]] : null;
      }
    }
    // Parents by the rig's tree when there is one (joint -> node -> parent node -> joint), else none.
    const nodeToJoint = new Map();
    if (skinRig) skinJointNode.forEach((n, j) => { if (n >= 0 && !nodeToJoint.has(n)) nodeToJoint.set(n, j); });
    const lines = [];
    const push = (p, c) => lines.push(p[0], p[1], p[2], 0, 0, c[0], c[1], c[2]);
    const size = (bundle.radius || 1) * 0.03;
    for (let j = 0; j < joints; j++) {
      const p = positions[j];
      if (!p) continue;
      const colour = j === paint.bone ? [1, 0.9, 0.2] : [0.85, 0.85, 0.9];
      // A small cross at the joint.
      push([p[0] - size, p[1], p[2]], colour); push([p[0] + size, p[1], p[2]], colour);
      push([p[0], p[1] - size, p[2]], colour); push([p[0], p[1] + size, p[2]], colour);
      push([p[0], p[1], p[2] - size], colour); push([p[0], p[1], p[2] + size], colour);
      if (skinRig && skinRig.parents) {
        const n = skinJointNode[j];
        const parentNode = n >= 0 ? skinRig.parents[n] : -1;
        const pj = parentNode >= 0 && nodeToJoint.has(parentNode) ? nodeToJoint.get(parentNode) : -1;
        if (pj >= 0 && positions[pj]) { push(p, colour); push(positions[pj], j === paint.bone ? colour : [0.55, 0.6, 0.7]); }
      }
    }
    if (!lines.length) return;
    gl.bindBuffer(gl.ARRAY_BUFFER, boneBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lines), gl.DYNAMIC_DRAW);
    const stride = 8 * 4;
    gl.enableVertexAttribArray(attribute.position); gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
    gl.enableVertexAttribArray(attribute.coord); gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 12);
    gl.enableVertexAttribArray(attribute.colour); gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 20);
    if (attribute.mindex >= 0) {
      gl.bindBuffer(gl.ARRAY_BUFFER, boneIndexAttr);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lines.length / 8), gl.DYNAMIC_DRAW);
      gl.enableVertexAttribArray(attribute.mindex); gl.vertexAttribPointer(attribute.mindex, 1, gl.FLOAT, false, 4, 0);
    }
    gl.bindTexture(gl.TEXTURE_2D, blank);
    gl.uniform1i(uniform.textured, 0);
    gl.uniform3fv(uniform.tint, [1, 1, 1]);
    gl.uniform1f(uniform.alpha, 1);
    paletteData.set(IDENTITY, 0);
    gl.uniformMatrix4fv(uniform.palette, false, paletteData);
    gl.disable(gl.DEPTH_TEST);
    gl.lineWidth(2);
    gl.drawArrays(gl.LINES, 0, lines.length / 8);
    gl.enable(gl.DEPTH_TEST);
  }

  /// The point on the mesh under a canvas position, in model space (as drawn now), or null.
  function pick(clientX, clientY) {
    if (!bundle || !bundle.buffer) return null;
    const box = canvas.getBoundingClientRect();
    const nx = ((clientX - box.left) / Math.max(1, box.width)) * 2 - 1;
    const ny = 1 - ((clientY - box.top) / Math.max(1, box.height)) * 2;
    const inverse = invert4(cameraMatrix());
    if (!inverse) return null;
    const unproject = (z) => {
      const v = [nx, ny, z, 1];
      const out = [0, 0, 0, 0];
      for (let r = 0; r < 4; r++) out[r] = inverse[r] * v[0] + inverse[4 + r] * v[1] + inverse[8 + r] * v[2] + inverse[12 + r] * v[3];
      return [out[0] / out[3], out[1] / out[3], out[2] / out[3]];
    };
    const origin = unproject(-1), far = unproject(1);
    const dir = normalise([far[0] - origin[0], far[1] - origin[1], far[2] - origin[2]]);
    const p = currentPositions();
    const idx = bundle.indices;
    let best = Infinity, hit = null, tri = -1;
    for (const group of bundle.groups) {
      if (group.hidden && !showHidden) continue;
      for (let i = group.start; i + 2 < group.start + group.count; i += 3) {
        const a = idx[i] * 8, b = idx[i + 1] * 8, c = idx[i + 2] * 8;
        const t = rayTriangle(origin, dir, p[a], p[a + 1], p[a + 2], p[b], p[b + 1], p[b + 2], p[c], p[c + 1], p[c + 2]);
        if (t !== null && t < best) { best = t; hit = [origin[0] + dir[0] * t, origin[1] + dir[1] * t, origin[2] + dir[2] * t]; tri = i; }
      }
    }
    if (!hit) return null;
    // The triangle's normal, turned to face the viewer, and its corners: the brush's seat.
    const a = idx[tri] * 8, b = idx[tri + 1] * 8, c = idx[tri + 2] * 8;
    const e1 = [p[b] - p[a], p[b + 1] - p[a + 1], p[b + 2] - p[a + 2]], e2 = [p[c] - p[a], p[c + 1] - p[a + 1], p[c + 2] - p[a + 2]];
    let normal = normalise([e1[1] * e2[2] - e1[2] * e2[1], e1[2] * e2[0] - e1[0] * e2[2], e1[0] * e2[1] - e1[1] * e2[0]]);
    if (normal[0] * dir[0] + normal[1] * dir[1] + normal[2] * dir[2] > 0) normal = [-normal[0], -normal[1], -normal[2]];
    hit.normal = normal;
    hit.corners = [idx[tri], idx[tri + 1], idx[tri + 2]];
    return hit;
  }

  // The brush's seat under the cursor as it hovers: { point, normal } on the mesh, drawn as a ring.
  let brushSeat = null;

  // How the rigged file's geometry was carried into the bind pose (/api/model/carry): the
  // original's vertices as fitted, and the inverse of each joint's skinning matrix at the pose
  // they were carried out of. With it, a change of weights carries the geometry again - a part
  // painted onto another bone goes where that bone has it, and no weights at all shows the
  // file's own pose - instead of leaving the auto-rig's carry baked in. Null: no record.
  let carry = null;

  /// The bind-pose positions carried again through the weights as they are now, into skin.local
  /// and the drawn buffer.
  function recarry() {
    if (!carry || !bundle || !bundle.skin) return;
    const skin = bundle.skin, count = bundle.buffer.length / 8;
    const P = carry.positions, undo = carry.undo;
    for (let v = 0; v < count; v++) {
      const x = P[v * 3], y = P[v * 3 + 1], z = P[v * 3 + 2];
      let px = 0, py = 0, pz = 0, total = 0;
      for (let k = 0; k < 4; k++) {
        const w = skin.weights[v * 4 + k], j = skin.jointIndex[v * 4 + k];
        if (w <= 0 || j < 0) continue;
        const m = undo[j];
        if (!m) { px += w * x; py += w * y; pz += w * z; total += w; continue; }
        px += w * (m[0] * x + m[4] * y + m[8] * z + m[12]);
        py += w * (m[1] * x + m[5] * y + m[9] * z + m[13]);
        pz += w * (m[2] * x + m[6] * y + m[10] * z + m[14]);
        total += w;
      }
      if (total <= 0) { px = x; py = y; pz = z; total = 1; }
      skin.local[v * 3] = px / total; skin.local[v * 3 + 1] = py / total; skin.local[v * 3 + 2] = pz / total;
      bundle.buffer[v * 8] = skin.local[v * 3]; bundle.buffer[v * 8 + 1] = skin.local[v * 3 + 1]; bundle.buffer[v * 8 + 2] = skin.local[v * 3 + 2];
    }
  }

  /// After a change of weights: the geometry carried again (when there is a carry), skinned and drawn.
  function weightsChanged() {
    recarry();
    skinTo(skinFrame);
    draw();
  }

  // The auto-rig's cuts drawn on the (unrigged) model: { neck, hips, armFloor, torsoWidth } as
  // fractions of the model's height and width, or null for none.
  let cuts = null;
  let bounds = null;             // the viewed model's box: { min, max }

  function boundsOf(model) {
    const b = model.buffer;
    const min = [Infinity, Infinity, Infinity], max = [-Infinity, -Infinity, -Infinity];
    for (let v = 0; v + 2 < b.length; v += 8) for (let k = 0; k < 3; k++) { if (b[v + k] < min[k]) min[k] = b[v + k]; if (b[v + k] > max[k]) max[k] = b[v + k]; }
    return isFinite(min[0]) ? { min, max } : null;
  }

  /// The ring the mesh makes where a plane cuts it: of the vertices within a thin slab about
  /// <paramref name="point"/> across <paramref name="axis"/> (and within reach of it, for a limb rather
  /// than the whole body), the middle and the mean distance from it. Null when nothing is there.
  function limbRing(point, axis, reach) {
    if (!bundle || !bundle.buffer || !bounds) return null;
    const p = currentPositions();
    const h = bounds.max[1] - bounds.min[1];
    const n = normalise(axis), slab = h * 0.015, reach2 = reach ? reach * reach : Infinity;
    const inside = [];
    for (let v = 0; v + 2 < p.length; v += 8) {
      const dx = p[v] - point[0], dy = p[v + 1] - point[1], dz = p[v + 2] - point[2];
      if (Math.abs(dx * n[0] + dy * n[1] + dz * n[2]) > slab) continue;
      if (dx * dx + dy * dy + dz * dz > reach2) continue;
      inside.push(v);
    }
    if (inside.length < 4) return null;
    const centre = [0, 0, 0];
    for (const v of inside) { centre[0] += p[v]; centre[1] += p[v + 1]; centre[2] += p[v + 2]; }
    centre[0] /= inside.length; centre[1] /= inside.length; centre[2] /= inside.length;
    let radius = 0;
    for (const v of inside) {
      const dx = p[v] - centre[0], dy = p[v + 1] - centre[1], dz = p[v + 2] - centre[2];
      const along = dx * n[0] + dy * n[1] + dz * n[2];
      radius += Math.sqrt(Math.max(0, dx * dx + dy * dy + dz * dz - along * along));
    }
    return { centre, normal: n, radius: radius / inside.length * 1.12 };
  }

  /// The cuts as rings the model's own size at their heights - the way an IK rig draws its
  /// joints (neck violet, hips orange, arm floor green) - and two vertical lines at the torso's
  /// width between the arm floor and the neck (blue).
  function drawCuts() {
    if (!cuts || !bounds) return;
    const { min, max } = bounds;
    const h = max[1] - min[1], w = max[0] - min[0];
    const pad = Math.max(w, max[2] - min[2]) * 0.08;
    const lines = [];
    const push = (x, y, z, c) => lines.push(x, y, z, 0, 0, c[0], c[1], c[2]);
    const ring = (fraction, colour) => {
      if (fraction === null || fraction === undefined) return;
      const y = min[1] + fraction * h;
      const found = limbRing([(min[0] + max[0]) / 2, y, (min[2] + max[2]) / 2], [0, 1, 0], null);
      if (found) ringInto(lines, found.centre, found.normal, found.radius, colour, false);
      else { const x0 = min[0] - pad, x1 = max[0] + pad, z = (min[2] + max[2]) / 2; push(x0, y, z, colour); push(x1, y, z, colour); }
    };
    ring(cuts.neck, [0.75, 0.45, 1]);
    ring(cuts.hips, [1, 0.6, 0.2]);
    ring(cuts.armFloor, [0.4, 0.9, 0.5]);
    if (cuts.torsoWidth !== null && cuts.torsoWidth !== undefined) {
      const cx = (min[0] + max[0]) / 2, half = cuts.torsoWidth * w / 2;
      const y0 = min[1] + (cuts.armFloor ?? 0.3) * h, y1 = min[1] + (cuts.neck ?? 0.85) * h;
      const c = [0.4, 0.7, 1];
      for (const x of [cx - half, cx + half]) for (const z of [min[2] - pad, max[2] + pad]) { push(x, y0, z, c); push(x, y1, z, c); }
      for (const x of [cx - half, cx + half]) for (const y of [y0, y1]) { push(x, y, min[2] - pad, c); push(x, y, max[2] + pad, c); }
    }
    if (lines.length) drawLines(lines, 1.5);
  }

  /// A ring on a surface: at c, in the plane across the normal n, of radius r, into lines (8 floats
  /// a vertex), lifted a little off the surface; with a dot-sized inner ring and, when asked, a line up the normal.
  function ringInto(lines, c, n, r, colour, withNormal) {
    const [n0, n1, n2] = n;
    const ref = Math.abs(n1) < 0.9 ? [0, 1, 0] : [1, 0, 0];
    const u = normalise([n1 * ref[2] - n2 * ref[1], n2 * ref[0] - n0 * ref[2], n0 * ref[1] - n1 * ref[0]]);
    const w = [n1 * u[2] - n2 * u[1], n2 * u[0] - n0 * u[2], n0 * u[1] - n1 * u[0]];
    const lift = r * 0.02;
    const push = (x, y, z) => lines.push(x + n0 * lift, y + n1 * lift, z + n2 * lift, 0, 0, colour[0], colour[1], colour[2]);
    const circle = (radius, segments) => {
      for (let i = 0; i < segments; i++) {
        const a0 = (i / segments) * Math.PI * 2, a1 = ((i + 1) / segments) * Math.PI * 2;
        push(c[0] + (u[0] * Math.cos(a0) + w[0] * Math.sin(a0)) * radius, c[1] + (u[1] * Math.cos(a0) + w[1] * Math.sin(a0)) * radius, c[2] + (u[2] * Math.cos(a0) + w[2] * Math.sin(a0)) * radius);
        push(c[0] + (u[0] * Math.cos(a1) + w[0] * Math.sin(a1)) * radius, c[1] + (u[1] * Math.cos(a1) + w[1] * Math.sin(a1)) * radius, c[2] + (u[2] * Math.cos(a1) + w[2] * Math.sin(a1)) * radius);
      }
    };
    circle(r, 48);
    circle(r * 0.08, 16);
    if (withNormal) { push(c[0], c[1], c[2]); push(c[0] + n0 * r * 0.5, c[1] + n1 * r * 0.5, c[2] + n2 * r * 0.5); }
  }

  /// The ring where the brush would land: the radius, in the surface's plane at the cursor, with a
  /// short line up the normal; over everything, so it shows through the mesh's own faces.
  function drawBrush() {
    if (!brushSeat || !paint.on || paint.bone < 0) return;
    const colour = paint.mode === 'erase' ? [1, 0.45, 0.35] : paint.mode === 'smooth' ? [0.55, 0.85, 1] : [1, 0.95, 0.4];
    const lines = [];
    ringInto(lines, brushSeat.point, brushSeat.normal, paint.radius, colour, true);
    drawLines(lines, 2);
  }

  // Markers placed on the model (the auto-rig's chin, wrists, elbows, knees, groin): [{ point,
  // normal, colour }], drawn as rings; and the one under the cursor as it hovers, waiting to be placed.
  let markers = [];
  let markerSeat = null;

  function drawMarkers() {
    if (!markers.length && !markerSeat) return;
    const r = (bundle && bundle.radius || 1) * 0.045;
    const h = bounds ? bounds.max[1] - bounds.min[1] : 1;
    const lines = [];
    for (const m of markers) {
      // A ring around the limb through the marker (across the limb's axis), as an IK rig draws a
      // joint; the marker itself a small ring on the surface where it was placed.
      const around = m.axis ? limbRing(m.point, m.axis, h * 0.12) : null;
      if (around) ringInto(lines, around.centre, around.normal, around.radius, m.colour || [1, 1, 1], false);
      ringInto(lines, m.point, m.normal || [0, 0, 1], r * (around ? 0.4 : 1), m.colour || [1, 1, 1], false);
    }
    if (markerSeat) ringInto(lines, markerSeat.point, markerSeat.normal, r * 1.15, markerSeat.colour || [1, 1, 1], true);
    drawLines(lines, 2);
  }

  /// Lines (8 floats a vertex, as the model's buffer) drawn plain and over everything.
  function drawLines(lines, width) {
    gl.bindBuffer(gl.ARRAY_BUFFER, boneBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lines), gl.DYNAMIC_DRAW);
    const stride = 8 * 4;
    gl.enableVertexAttribArray(attribute.position); gl.vertexAttribPointer(attribute.position, 3, gl.FLOAT, false, stride, 0);
    gl.enableVertexAttribArray(attribute.coord); gl.vertexAttribPointer(attribute.coord, 2, gl.FLOAT, false, stride, 12);
    gl.enableVertexAttribArray(attribute.colour); gl.vertexAttribPointer(attribute.colour, 3, gl.FLOAT, false, stride, 20);
    if (attribute.mindex >= 0) {
      gl.bindBuffer(gl.ARRAY_BUFFER, boneIndexAttr);
      gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(lines.length / 8), gl.DYNAMIC_DRAW);
      gl.enableVertexAttribArray(attribute.mindex); gl.vertexAttribPointer(attribute.mindex, 1, gl.FLOAT, false, 4, 0);
    }
    gl.bindTexture(gl.TEXTURE_2D, blank);
    gl.uniform1i(uniform.textured, 0);
    gl.uniform3fv(uniform.tint, [1, 1, 1]);
    gl.uniform1f(uniform.alpha, 1);
    paletteData.set(IDENTITY, 0);
    gl.uniformMatrix4fv(uniform.palette, false, paletteData);
    gl.disable(gl.DEPTH_TEST);
    gl.lineWidth(width);
    gl.drawArrays(gl.LINES, 0, lines.length / 8);
    gl.enable(gl.DEPTH_TEST);
  }

  /// Neighbours by shared edge, twins by position merged, for the smooth brush.
  function buildAdjacency() {
    const count = bundle.buffer.length / 8;
    const cells = new Map();
    canonical = new Int32Array(count);
    for (let v = 0; v < count; v++) {
      const key = Math.round(bundle.buffer[v * 8] * 2000) + ',' + Math.round(bundle.buffer[v * 8 + 1] * 2000) + ',' + Math.round(bundle.buffer[v * 8 + 2] * 2000);
      if (!cells.has(key)) cells.set(key, v);
      canonical[v] = cells.get(key);
    }
    adjacency = new Array(count);
    const link = (a, b) => { if (a === b) return; (adjacency[a] ||= new Set()).add(b); (adjacency[b] ||= new Set()).add(a); };
    const idx = bundle.indices;
    for (let i = 0; i + 2 < idx.length; i += 3) {
      const a = canonical[idx[i]], b = canonical[idx[i + 1]], c = canonical[idx[i + 2]];
      link(a, b); link(b, c); link(a, c);
    }
  }

  /// A vertex's weights as a map joint -> weight, and back into the four slots (heaviest four, summing to one).
  function weightsOf(v) {
    const skin = bundle.skin;
    const m = new Map();
    for (let k = 0; k < 4; k++) {
      const j = skin.jointIndex[v * 4 + k], w = skin.weights[v * 4 + k];
      if (j >= 0 && w > 0) m.set(j, (m.get(j) || 0) + w);
    }
    return m;
  }
  /// How many steps apart two joints are in the skeleton's tree (through the rig's parents), or 99 without a rig.
  function jointDistance(a, b) {
    if (a === b) return 0;
    if (!skinRig || !skinRig.parents || !skinJointNode) return 99;
    const chain = (j) => { const out = new Map(); let node = skinJointNode[j], hops = 0; while (node >= 0 && hops < 64) { out.set(node, hops); node = skinRig.parents[node]; hops++; } return out; };
    const ca = chain(a), cb = chain(b);
    let best = 99;
    for (const [node, ha] of ca) if (cb.has(node)) best = Math.min(best, ha + cb.get(node));
    return best;
  }

  /// A vertex's bones kept to what can share a vertex: every bone within two steps of its
  /// heaviest in the skeleton's tree. A hand and a head, a left knee and a right heel, a stomach
  /// and a sleeve are never one vertex's; an elbow's upper arm, forearm and hand are. What is
  /// dropped goes to the bones kept, in proportion.
  function keepCompatible(m) {
    if (m.size <= 1) return m;
    const sorted = [...m.entries()].sort((a, b) => b[1] - a[1]);
    const heaviest = sorted[0][0];
    const kept = new Map();
    for (const [j, w] of sorted) if (jointDistance(heaviest, j) <= 2) kept.set(j, w);
    return kept;
  }

  function storeWeights(v, m) {
    const skin = bundle.skin;
    m = keepCompatible(m);
    const top = [...m.entries()].filter(([, w]) => w > 0.0005).sort((a, b) => b[1] - a[1]).slice(0, 4);
    const total = top.reduce((s, [, w]) => s + w, 0) || 1;
    for (let k = 0; k < 4; k++) {
      skin.jointIndex[v * 4 + k] = k < top.length ? top[k][0] : -1;
      skin.weights[v * 4 + k] = k < top.length ? top[k][1] / total : 0;
    }
  }

  /// The vertices the brush reaches from a seat on the mesh, with how far each is: not a ball about
  /// the point, which would take the chest under a sleeve along with the sleeve, but the surface
  /// itself - out from the seat's triangle along the mesh's edges (as drawn now), as far as the
  /// radius. What is not joined to the seat within the radius is not touched. Returns
  /// [canonical vertex, distance] pairs.
  function reach(seeds, radius) {
    const p = currentPositions();
    const dist = new Map();
    // A small binary heap of [distance, vertex].
    const heap = [];
    const up = (i) => { while (i > 0) { const j = (i - 1) >> 1; if (heap[j][0] <= heap[i][0]) break; [heap[i], heap[j]] = [heap[j], heap[i]]; i = j; } };
    const down = (i) => { for (;;) { let m = i; const l = 2 * i + 1, r = l + 1; if (l < heap.length && heap[l][0] < heap[m][0]) m = l; if (r < heap.length && heap[r][0] < heap[m][0]) m = r; if (m === i) break; [heap[i], heap[m]] = [heap[m], heap[i]]; i = m; } };
    const offer = (v, d) => { if (d > radius || (dist.has(v) && dist.get(v) <= d)) return; dist.set(v, d); heap.push([d, v]); up(heap.length - 1); };
    for (const [v, d] of seeds) offer(canonical ? canonical[v] : v, d);
    while (heap.length) {
      const [d, v] = heap[0];
      const last = heap.pop(); if (heap.length) { heap[0] = last; down(0); }
      if (dist.get(v) < d) continue;
      for (const other of adjacency[v] || []) {
        const dx = p[other * 8] - p[v * 8], dy = p[other * 8 + 1] - p[v * 8 + 1], dz = p[other * 8 + 2] - p[v * 8 + 2];
        offer(other, d + Math.sqrt(dx * dx + dy * dy + dz * dz));
      }
    }
    return [...dist.entries()];
  }

  /// The brush at a seat on the mesh (a point with the corners of its triangle): every vertex the
  /// surface joins to it within the radius has the bone's weight added, taken away or smoothed
  /// towards its neighbours, more at the centre; the other bones give way so the vertex still
  /// sums to one. Mirror paints the x-mirrored point with the mirrored bone (L_ <-> R_).
  function brush(hit, bone, sign) {
    const skin = bundle.skin;
    const count = bundle.buffer.length / 8;
    const p = currentPositions();
    if (!adjacency) buildAdjacency();
    // The seat's corners, each as far as it is from the point, start the flood.
    const seeds = [];
    for (const v of hit.corners || []) {
      const dx = p[v * 8] - hit[0], dy = p[v * 8 + 1] - hit[1], dz = p[v * 8 + 2] - hit[2];
      seeds.push([v, Math.sqrt(dx * dx + dy * dy + dz * dz)]);
    }
    if (!seeds.length) return 0;
    const touched = reach(seeds, paint.radius).map(([v, d]) => [v, 1 - d / paint.radius]);
    // Twins by position paint together, whichever the brush reached.
    const seen = new Set();
    for (const [v, falloff] of touched) {
      const c = canonical ? canonical[v] : v;
      if (seen.has(c)) continue;
      seen.add(c);
      const amount = paint.strength * (0.3 + 0.7 * falloff);
      const m = weightsOf(c);
      if (paint.mode === 'smooth') {
        const around = new Map();
        let n = 0;
        for (const other of adjacency[c] || []) { for (const [j, w] of weightsOf(other)) around.set(j, (around.get(j) || 0) + w); n++; }
        if (n === 0) continue;
        const blended = new Map();
        for (const j of new Set([...m.keys(), ...around.keys()])) blended.set(j, (1 - amount) * (m.get(j) || 0) + amount * (around.get(j) || 0) / n);
        storeWeights(c, blended);
      } else if (paint.mode === 'assign') {
        // Assign: the ring is this bone's, outright - a vertex inside it goes to the bone whole,
        // the outer third of the ring easing in - whatever it had before. The tool for "this part
        // belongs to that bone", which add's blending and erase's redistribution never quite say.
        const had = m.get(bone) || 0;
        const ease = Math.min(1, falloff / 0.3);
        const want = Math.max(had, ease);
        const others = [...m.entries()].filter(([j]) => j !== bone);
        const othersTotal = others.reduce((s, [, w]) => s + w, 0);
        const next = new Map();
        if (othersTotal > 0 && want < 1) for (const [j, w] of others) next.set(j, w / othersTotal * (1 - want));
        next.set(bone, want);
        storeWeights(c, next);
      } else if (sign > 0) {
        // Add: the bone takes the amount and the others give way in proportion.
        const had = m.get(bone) || 0;
        const want = Math.min(1, had + amount);
        const others = [...m.entries()].filter(([j]) => j !== bone);
        const othersTotal = others.reduce((s, [, w]) => s + w, 0);
        const next = new Map();
        if (othersTotal > 0) for (const [j, w] of others) next.set(j, w / othersTotal * (1 - want));
        next.set(bone, othersTotal > 0 ? want : 1);
        storeWeights(c, next);
      } else {
        storeWeights(c, takeOff(c, m, bone, amount));
      }
      // Every vertex at this position takes the same weights.
      if (canonical) for (let v2 = 0; v2 < count; v2++) if (canonical[v2] === c && v2 !== c) { for (let k = 0; k < 4; k++) { skin.jointIndex[v2 * 4 + k] = skin.jointIndex[c * 4 + k]; skin.weights[v2 * 4 + k] = skin.weights[c * 4 + k]; } }
    }
    return touched.length;
  }

  /// Erase: a vertex's weights with <paramref name="amount"/> taken off one bone, and what came off
  /// handed to where the surface around the vertex is bound - its neighbours' bones, this one left
  /// out - so a chest vertex losing its chest weight goes to the chest around it, not to a stray
  /// hand weight it happened to carry (which would fling it to the hand). Without neighbours that
  /// know better: its own other bones if they amount to something, else the bone's parent.
  function takeOff(c, m, bone, amount) {
    const had = m.get(bone) || 0;
    const want = Math.max(0, had - amount);
    const removed = had - want;
    const next = new Map(m);
    if (removed <= 0) return next;
    const pool = new Map();
    for (const other of adjacency[c] || []) for (const [j, w] of weightsOf(other)) if (j !== bone) pool.set(j, (pool.get(j) || 0) + w);
    let poolTotal = [...pool.values()].reduce((s, w) => s + w, 0);
    if (poolTotal <= 0) {
      for (const [j, w] of m) if (j !== bone) pool.set(j, w);
      poolTotal = [...pool.values()].reduce((s, w) => s + w, 0);
      if (poolTotal < 0.2) {
        pool.clear();
        const parent = parentJoint(bone);
        pool.set(parent >= 0 && parent !== bone ? parent : bone, 1);
        poolTotal = 1;
      }
    }
    for (const [j, w] of pool) next.set(j, (next.get(j) || 0) + removed * w / poolTotal);
    next.set(bone, want);
    return next;
  }

  /// Takes a bone off every vertex that carries it, the weight going where erase sends it. Returns how many.
  function clearBone(bone) {
    if (!adjacency) buildAdjacency();
    const count = bundle.buffer.length / 8;
    let cleared = 0;
    // Twice: a vertex whose neighbours all had only this bone gets a real pool the second time.
    for (let round = 0; round < 2; round++) {
      for (let c = 0; c < count; c++) {
        if (canonical && canonical[c] !== c) continue;
        const m = weightsOf(c);
        if (!(m.get(bone) > 0)) continue;
        storeWeights(c, takeOff(c, m, bone, 1));
        if (round === 0) cleared++;
      }
    }
    copyTwins();
    return cleared;
  }

  /// One smoothing pass over the whole mesh: each vertex half its own weights, half its neighbours' mean.
  function smoothAll() {
    if (!adjacency) buildAdjacency();
    const count = bundle.buffer.length / 8;
    const before = new Map();
    for (let c = 0; c < count; c++) if (!canonical || canonical[c] === c) before.set(c, weightsOf(c));
    for (const [c, mine] of before) {
      const around = new Map();
      let n = 0;
      for (const other of adjacency[c] || []) { const theirs = before.get(other); if (!theirs) continue; for (const [j, w] of theirs) around.set(j, (around.get(j) || 0) + w); n++; }
      if (n === 0) continue;
      const blended = new Map();
      for (const j of new Set([...mine.keys(), ...around.keys()])) blended.set(j, 0.5 * (mine.get(j) || 0) + 0.5 * (around.get(j) || 0) / n);
      storeWeights(c, blended);
    }
    copyTwins();
  }

  /// Every vertex at a position takes its canonical twin's weights.
  function copyTwins() {
    if (!canonical) return;
    const skin = bundle.skin, count = bundle.buffer.length / 8;
    for (let v = 0; v < count; v++) {
      const c = canonical[v];
      if (c === v) continue;
      for (let k = 0; k < 4; k++) { skin.jointIndex[v * 4 + k] = skin.jointIndex[c * 4 + k]; skin.weights[v * 4 + k] = skin.weights[c * 4 + k]; }
    }
  }

  /// The joint above a joint in the game's tree (through the rig's parents), or -1 without a rig.
  function parentJoint(bone) {
    if (!skinRig || !skinRig.parents || !skinJointNode) return -1;
    let node = skinJointNode[bone];
    for (let hops = 0; hops < 64 && node >= 0; hops++) {
      node = skinRig.parents[node];
      if (node < 0) return -1;
      const j = skinJointNode.indexOf(node);
      if (j >= 0) return j;
    }
    return -1;
  }

  function mirroredBone(bone) {
    const name = bundle.skin.joints[bone] || '';
    const other = name.startsWith('L_') ? 'R_' + name.slice(2) : name.startsWith('R_') ? 'L_' + name.slice(2) : null;
    if (other === null) return bone;
    const j = bundle.skin.joints.indexOf(other);
    return j >= 0 ? j : bone;
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
      uploadEdges(model);
      pose = null;
      poseFrame = 0;
      skinRig = null;
      skinJointNode = null;
      skinBuffer = null;
      skinFrame = -1;
      skinFitted = false;
      adjacency = null;
      canonical = null;
      undoStack.length = 0;
      paint.bone = -1;
      brushSeat = null;
      cuts = null;
      bounds = boundsOf(model);

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
      if (skinRig) { skinTo(Math.floor(frame)); draw(); return; }
      if (!pose) return;
      poseFrame = Math.max(0, Math.min(pose.frames - 1, Math.floor(frame)));
      draw();
    },

    /// A skinned glTF driven by a game model's motion: `rig` is the /api/model/rig-pose answer
    /// (nodes, frames, worlds), or null for the file as it is. Joints find their node by name.
    setSkinPose(rig) {
      skinRig = rig && rig.frames > 0 && bundle && bundle.skin ? rig : null;
      if (!skinRig) { skinTo(-1); draw(); return; }
      const byName = new Map(rig.nodes.map((n, i) => [String(n).toLowerCase(), i]));
      skinJointNode = bundle.skin.joints.map(j => byName.has(String(j).toLowerCase()) ? byName.get(String(j).toLowerCase()) : -1);
      skinFitted = jointsFitted(rig, skinJointNode);
      skinTo(0);
      draw();
    },

    /// Whether the shown file has a fitted skeleton (driven by retargeting the game's motions).
    skinIsFitted() { return skinFitted; },

    /// Whether the shown model is a skinned glTF, and which game model drives it (or null).
    skinModel() { return bundle && bundle.skin ? (bundle.skin.model || null) : null; },
    hasSkin() { return Boolean(bundle && bundle.skin); },
    skinJoints() { return bundle && bundle.skin ? bundle.skin.joints.slice() : []; },

    // ---- weight painting
    /// Weights mode and the brush: { on, bone, radius, strength, mode: add|erase|smooth, mirror, heat, bones, wire }.
    /// heat colours the model by the bone's weight (in weights mode); bones and wire are the skeleton
    /// and the wireframe over whatever is shown, weights mode or not.
    setPaint(options) {
      Object.assign(paint, options || {});
      if (!paint.on) brushSeat = null;
      if (bundle && bundle.skin) skinTo(skinFrame);
      draw();
    },
    paintOptions() { return { ...paint }; },

    /// A stroke's start: the weights as they are, for undo.
    beginStroke() {
      if (!bundle || !bundle.skin) return;
      undoStack.push({ joints: Int32Array.from(bundle.skin.jointIndex), weights: Float32Array.from(bundle.skin.weights) });
      if (undoStack.length > 30) undoStack.shift();
    },

    /// The brush at a canvas position; true when it touched the mesh.
    paintAt(clientX, clientY) {
      if (!bundle || !bundle.skin || paint.bone < 0) return false;
      const hit = pick(clientX, clientY);
      if (!hit) return false;
      const sign = paint.mode === 'erase' ? -1 : 1;
      brush(hit, paint.bone, sign);
      if (paint.mirror) {
        // The mirrored seat: the nearest vertex to the mirrored point, when the mesh has one there.
        const m = [-hit[0], hit[1], hit[2]];
        const p = currentPositions();
        let best = -1, bestD = paint.radius * paint.radius;
        for (let v = 0; v < bundle.buffer.length / 8; v++) {
          const dx = p[v * 8] - m[0], dy = p[v * 8 + 1] - m[1], dz = p[v * 8 + 2] - m[2];
          const d = dx * dx + dy * dy + dz * dz;
          if (d < bestD) { bestD = d; best = v; }
        }
        if (best >= 0) { m.corners = [best]; brush(m, mirroredBone(paint.bone), sign); }
      }
      brushSeat = { point: [hit[0], hit[1], hit[2]], normal: hit.normal };
      weightsChanged();
      return true;
    },

    /// The brush's ring follows the cursor over the mesh (null, or off the mesh, hides it). Returns
    /// the bones of the vertex under the cursor, heaviest first, as [{ bone, name, weight }], or null.
    hoverBrush(clientX, clientY) {
      const before = brushSeat;
      const hit = clientX === null || !bundle || !bundle.skin || !paint.on || paint.bone < 0 ? null : pick(clientX, clientY);
      brushSeat = hit ? { point: [hit[0], hit[1], hit[2]], normal: hit.normal } : null;
      if (before || brushSeat) draw();
      if (!hit) return null;
      // The nearest of the triangle's corners: its four bones.
      const p = currentPositions();
      let best = -1, bestD = Infinity;
      for (const v of hit.corners || []) {
        const dx = p[v * 8] - hit[0], dy = p[v * 8 + 1] - hit[1], dz = p[v * 8 + 2] - hit[2];
        const d = dx * dx + dy * dy + dz * dz;
        if (d < bestD) { bestD = d; best = v; }
      }
      if (best < 0) return null;
      const out = [];
      for (let k = 0; k < 4; k++) {
        const j = bundle.skin.jointIndex[best * 4 + k], w = bundle.skin.weights[best * 4 + k];
        if (j >= 0 && w > 0.005) out.push({ bone: j, name: bundle.skin.joints[j], weight: w });
      }
      return out.sort((a, b) => b.weight - a.weight);
    },

    /// The auto-rig's cuts to draw on the model ({ neck, hips, armFloor, torsoWidth } as fractions; null: none).
    setCuts(next) { cuts = next || null; draw(); },

    /// The point on the mesh under a canvas position, as drawn now: { point: [x, y, z], normal } or null.
    pointAt(clientX, clientY) {
      const hit = pick(clientX, clientY);
      return hit ? { point: [hit[0], hit[1], hit[2]], normal: hit.normal } : null;
    },
    /// Markers to draw on the model: [{ point, normal, colour }] (empty for none).
    setMarkers(list) { markers = Array.isArray(list) ? list : []; draw(); },
    /// The marker waiting to be placed, following the cursor ({ point, normal, colour } or null).
    setMarkerSeat(seat) { markerSeat = seat || null; draw(); },

    /// How the file's geometry was carried into the bind pose: the original's fitted positions (3 a
    /// vertex) and the rig pose (as /api/model/rig-pose gives it) with the frame they were carried out
    /// of; null for the bind pose itself (the file was fitted in it) or none. Repainting then carries
    /// the geometry again. Returns whether a carry is in force.
    setCarry(next) {
      carry = null;
      if (!next || !next.positions || !bundle || !bundle.skin || next.positions.length !== bundle.buffer.length / 8 * 3) { draw(); return false; }
      const skin = bundle.skin;
      const undo = new Array(skin.joints.length).fill(null);
      if (next.rig && next.rig.frames > 0) {
        // The skinning matrices at the pose the file was carried out of - through the same sums
        // the playback uses, fitted or not - and their inverses to carry with.
        const byName = new Map(next.rig.nodes.map((n, i) => [String(n).toLowerCase(), i]));
        const map = skin.joints.map(j => byName.has(String(j).toLowerCase()) ? byName.get(String(j).toLowerCase()) : -1);
        const was = skinFitted;
        skinFitted = jointsFitted(next.rig, map);
        const matrices = matricesFor(next.rig, map, next.frame || 0);
        skinFitted = was;
        for (let j = 0; j < skin.joints.length; j++) undo[j] = matrices[j] ? invert4(matrices[j]) : null;
      }
      carry = { positions: Float32Array.from(next.positions), undo };
      weightsChanged();
      return true;
    },
    hasCarry() { return Boolean(carry); },

    /// A bone's colour in the all-bones view, as a CSS rgb() string.
    boneColour(j) { const c = boneColour(j); return `rgb(${Math.round(c[0] * 255)}, ${Math.round(c[1] * 255)}, ${Math.round(c[2] * 255)})`; },

    /// The heaviest bone under a canvas position, or -1.
    boneAt(clientX, clientY) {
      if (!bundle || !bundle.skin) return -1;
      const hit = pick(clientX, clientY);
      if (!hit) return -1;
      const p = currentPositions();
      let best = -1, bestD = Infinity;
      for (let v = 0; v < bundle.buffer.length / 8; v++) {
        const dx = p[v * 8] - hit[0], dy = p[v * 8 + 1] - hit[1], dz = p[v * 8 + 2] - hit[2];
        const d = dx * dx + dy * dy + dz * dz;
        if (d < bestD) { bestD = d; best = v; }
      }
      if (best < 0) return -1;
      let bone = -1, w = 0;
      for (let k = 0; k < 4; k++) if (bundle.skin.weights[best * 4 + k] > w) { w = bundle.skin.weights[best * 4 + k]; bone = bundle.skin.jointIndex[best * 4 + k]; }
      return bone;
    },

    /// Takes the painted bone off the whole mesh (an undo step of its own). Returns how many vertices carried it.
    clearBone(bone) {
      if (!bundle || !bundle.skin || bone < 0) return 0;
      this.beginStroke();
      const n = clearBone(bone);
      weightsChanged();
      return n;
    },

    /// One smoothing pass over every vertex (an undo step of its own).
    smoothAll() {
      if (!bundle || !bundle.skin) return;
      this.beginStroke();
      smoothAll();
      weightsChanged();
    },

    undoPaint() {
      const last = undoStack.pop();
      if (!last || !bundle || !bundle.skin) return false;
      bundle.skin.jointIndex = Array.from(last.joints);
      bundle.skin.weights = Array.from(last.weights);
      weightsChanged();
      return true;
    },
    canUndo() { return undoStack.length > 0; },

    /// The weights as painted, for saving: { jointIndex, weights } four a vertex, joints as skinJoints().
    weightsData() { return bundle && bundle.skin ? { jointIndex: Array.from(bundle.skin.jointIndex), weights: Array.from(bundle.skin.weights), positions: carry ? Array.from(bundle.skin.local) : null } : null; },

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

/// The inverse of a column-major 4x4, or null when singular.
function invert4(m) {
  const inv = new Array(16);
  inv[0] = m[5] * m[10] * m[15] - m[5] * m[11] * m[14] - m[9] * m[6] * m[15] + m[9] * m[7] * m[14] + m[13] * m[6] * m[11] - m[13] * m[7] * m[10];
  inv[4] = -m[4] * m[10] * m[15] + m[4] * m[11] * m[14] + m[8] * m[6] * m[15] - m[8] * m[7] * m[14] - m[12] * m[6] * m[11] + m[12] * m[7] * m[10];
  inv[8] = m[4] * m[9] * m[15] - m[4] * m[11] * m[13] - m[8] * m[5] * m[15] + m[8] * m[7] * m[13] + m[12] * m[5] * m[11] - m[12] * m[7] * m[9];
  inv[12] = -m[4] * m[9] * m[14] + m[4] * m[10] * m[13] + m[8] * m[5] * m[14] - m[8] * m[6] * m[13] - m[12] * m[5] * m[10] + m[12] * m[6] * m[9];
  inv[1] = -m[1] * m[10] * m[15] + m[1] * m[11] * m[14] + m[9] * m[2] * m[15] - m[9] * m[3] * m[14] - m[13] * m[2] * m[11] + m[13] * m[3] * m[10];
  inv[5] = m[0] * m[10] * m[15] - m[0] * m[11] * m[14] - m[8] * m[2] * m[15] + m[8] * m[3] * m[14] + m[12] * m[2] * m[11] - m[12] * m[3] * m[10];
  inv[9] = -m[0] * m[9] * m[15] + m[0] * m[11] * m[13] + m[8] * m[1] * m[15] - m[8] * m[3] * m[13] - m[12] * m[1] * m[11] + m[12] * m[3] * m[9];
  inv[13] = m[0] * m[9] * m[14] - m[0] * m[10] * m[13] - m[8] * m[1] * m[14] + m[8] * m[2] * m[13] + m[12] * m[1] * m[10] - m[12] * m[2] * m[9];
  inv[2] = m[1] * m[6] * m[15] - m[1] * m[7] * m[14] - m[5] * m[2] * m[15] + m[5] * m[3] * m[14] + m[13] * m[2] * m[7] - m[13] * m[3] * m[6];
  inv[6] = -m[0] * m[6] * m[15] + m[0] * m[7] * m[14] + m[4] * m[2] * m[15] - m[4] * m[3] * m[14] - m[12] * m[2] * m[7] + m[12] * m[3] * m[6];
  inv[10] = m[0] * m[5] * m[15] - m[0] * m[7] * m[13] - m[4] * m[1] * m[15] + m[4] * m[3] * m[13] + m[12] * m[1] * m[7] - m[12] * m[3] * m[5];
  inv[14] = -m[0] * m[5] * m[14] + m[0] * m[6] * m[13] + m[4] * m[1] * m[14] - m[4] * m[2] * m[13] - m[12] * m[1] * m[6] + m[12] * m[2] * m[5];
  inv[3] = -m[1] * m[6] * m[11] + m[1] * m[7] * m[10] + m[5] * m[2] * m[11] - m[5] * m[3] * m[10] - m[9] * m[2] * m[7] + m[9] * m[3] * m[6];
  inv[7] = m[0] * m[6] * m[11] - m[0] * m[7] * m[10] - m[4] * m[2] * m[11] + m[4] * m[3] * m[10] + m[8] * m[2] * m[7] - m[8] * m[3] * m[6];
  inv[11] = -m[0] * m[5] * m[11] + m[0] * m[7] * m[9] + m[4] * m[1] * m[11] - m[4] * m[3] * m[9] - m[8] * m[1] * m[7] + m[8] * m[3] * m[5];
  inv[15] = m[0] * m[5] * m[10] - m[0] * m[6] * m[9] - m[4] * m[1] * m[10] + m[4] * m[2] * m[9] + m[8] * m[1] * m[6] - m[8] * m[2] * m[5];
  const det = m[0] * inv[0] + m[1] * inv[4] + m[2] * inv[8] + m[3] * inv[12];
  if (Math.abs(det) < 1e-12) return null;
  for (let i = 0; i < 16; i++) inv[i] /= det;
  return inv;
}

/// The distance along a ray to a triangle, or null (Moller-Trumbore, both faces).
function rayTriangle(o, d, ax, ay, az, bx, by, bz, cx, cy, cz) {
  const e1x = bx - ax, e1y = by - ay, e1z = bz - az;
  const e2x = cx - ax, e2y = cy - ay, e2z = cz - az;
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
  const t = (e2x * qx + e2y * qy + e2z * qz) * inv;
  return t > 1e-6 ? t : null;
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
