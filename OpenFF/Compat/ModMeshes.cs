// The mod's own models on the map - glTF files drawn by the client with the field's camera.
//
// Drawn from WorldPart.onDrawPart right after the scene (terrain and characters), while the
// game's depth buffer holds the frame: NNS_G3dGlbFlushP loads the field's camera into the
// emulation's model-view (and its projection is the frame's), each mesh's pose goes on top
// as an XNA matrix, and the triangles go to NativeRenderer.Draw - the same call every game
// polygon takes, so depth, blending and the depth-range fix are the game's. Units: the
// engine's world units are the GL floats the emulation works in (1.0 = 4096 fixed), so a
// scene object's position is the mesh's position with no conversion.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	// The engine's vectors cross this file: MeshHandle speaks OpenFF.Vector3, the renderer XNA's.
	using XnaVector3 = Microsoft.Xna.Framework.Vector3;

	internal sealed class ModMeshes : GameService, IMeshes
	{
		private static ModMeshes _instance;
		private readonly Dictionary<string, GltfModel> _models = new Dictionary<string, GltfModel>(StringComparer.OrdinalIgnoreCase);
		private readonly List<Handle> _handles = new List<Handle>();

		public ModMeshes() { _instance = this; }

		public IReadOnlyList<MeshHandle> All => _handles;

		private sealed class Handle : MeshHandle
		{
			public GltfModel Model;
			public bool Removed;
			private OpenFF.Vector3 _position;
			private float _yaw, _scale = 1f;
			public override string Path => Model?.Path;
			public override OpenFF.Vector3 Position { get => _position; set { _position = value; Resolidify(); } }
			public override float Yaw { get => _yaw; set { _yaw = value; Resolidify(); } }
			public override float Scale { get => _scale; set { _scale = value; Resolidify(); } }
			public override bool Hidden { get; set; }

			private bool _solid;
			public override bool Solid
			{
				get => _solid;
				set { if (_solid == value) return; _solid = value; if (value) Resolidify(); else ModCollision.Remove(this); }
			}

			/// <summary>The pose matrix the mesh is drawn with, as the collision needs it: the bind pose's triangles through it.</summary>
			public Matrix Pose => Matrix.CreateScale(_scale) * Matrix.CreateRotationY(MathHelper.ToRadians(_yaw)) * Matrix.CreateTranslation(new XnaVector3(_position.X, _position.Y, _position.Z));

			private void Resolidify()
			{
				if (!_solid || Removed || Model == null) return;
				Matrix pose = Pose;
				List<XnaVector3> world = new List<XnaVector3>();
				foreach (GltfPrimitive p in Model.Primitives)
					foreach (VertexPositionColorTexture v in p.Vertices) world.Add(XnaVector3.Transform(v.Position, pose));
				ModCollision.Set(this, world);
			}
			public override OpenFF.Vector3 Min => Model == null ? default : new OpenFF.Vector3(Model.Min.X, Model.Min.Y, Model.Min.Z);
			public override OpenFF.Vector3 Max => Model == null ? default : new OpenFF.Vector3(Model.Max.X, Model.Max.Y, Model.Max.Z);
			public override int Triangles => Model?.Triangles ?? 0;
			public override string Problem => Model?.Problem;
			public override void Remove() { Removed = true; ModCollision.Remove(this); _instance?._handles.Remove(this); }

			// The clip playing: its index in the file, the time along it, the frame's vertices.
			public OpenFF.Graphics.GltfAnimation Playing;
			public bool Loop;
			public float Speed = 1f, Time;
			public long LastTick;
			public VertexPositionColorTexture[][] Posed;
			public override IReadOnlyList<string> Clips => Model?.File == null ? Array.Empty<string>() : Model.File.Animations.ConvertAll(a => a.Name);
			public override string Clip => Playing?.Name;
			public override bool Play(string clip, bool loop = true, float speed = 1f)
			{
				if (Model?.File == null) return false;
				OpenFF.Graphics.GltfAnimation found = Model.File.Animations.Find(a => string.Equals(a.Name, clip, StringComparison.OrdinalIgnoreCase));
				if (found == null) return false;
				Playing = found; Loop = loop; Speed = speed <= 0 ? 1f : speed; Time = 0; LastTick = Environment.TickCount64; Posed = null;
				return true;
			}
			public override void Stop() { Playing = null; Posed = null; }

			/// <summary>The clip a step further and the vertices for it; the file's meshes are posed in place, so one handle at a time.</summary>
			public void Advance()
			{
				if (Playing == null || Model?.File == null) return;
				long now = Environment.TickCount64;
				float dt = LastTick == 0 ? 0 : Math.Min(0.25f, (now - LastTick) / 1000f);
				LastTick = now;
				Time += dt * Speed;
				if (Playing.Duration > 0)
				{
					if (Loop) Time %= Playing.Duration;
					else if (Time > Playing.Duration) Time = Playing.Duration;
				}
				OpenFF.Graphics.GltfFile file = Model.File;
				file.Pose(file.WorldMatrices(Playing, Time));
				if (Posed == null || Posed.Length != file.Meshes.Count) Posed = new VertexPositionColorTexture[file.Meshes.Count][];
				for (int i = 0; i < file.Meshes.Count; i++)
				{
					OpenFF.Graphics.GltfMesh mesh = file.Meshes[i];
					OpenFF.Graphics.GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new OpenFF.Graphics.GltfMaterial();
					Posed[i] = GltfModel.Vertices(mesh, material);
				}
			}
		}

		public MeshHandle Spawn(string path, OpenFF.Vector3 position, float yaw = 0f, float scale = 1f)
		{
			if (string.IsNullOrWhiteSpace(path)) return null;
			string full;
			try { full = System.IO.Path.GetFullPath(path); } catch (Exception) { return null; }
			if (!_models.TryGetValue(full, out GltfModel model))
			{
				try
				{
					GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
					model = File.Exists(full) ? GltfModel.Load(full, device) : new GltfModel { Path = full, Problem = "no file " + full };
					Log.Write(LogChannel.General, "meshes: " + System.IO.Path.GetFileName(full) + ": " + (model.Problem ?? model.Triangles + " triangles, " + model.Primitives.Count + " part(s), " + Extent(model)));
				}
				catch (Exception ex)
				{
					model = new GltfModel { Path = full, Problem = ex.Message };
					Log.Write(LogChannel.General, "meshes: " + System.IO.Path.GetFileName(full) + ": " + ex.Message);
				}
				_models[full] = model;
			}
			Handle handle = new Handle { Model = model, Position = position, Yaw = yaw, Scale = scale };
			_handles.Add(handle);
			return handle;
		}

		private static string Extent(GltfModel m)
		{
			if (m.Primitives.Count == 0) return "empty";
			XnaVector3 size = m.Max - m.Min;
			return size.X.ToString("0.#") + " x " + size.Y.ToString("0.#") + " x " + size.Z.ToString("0.#") + " units";
		}

		/// <summary>The map changed: every mesh standing is the old map's.</summary>
		public static void Clear()
		{
			if (_instance == null) return;
			foreach (Handle h in _instance._handles) h.Removed = true;
			_instance._handles.Clear();
			ModCollision.Clear();
		}

		/// <summary>Called by the world part after its scene has drawn: the meshes with the field's camera.</summary>
		public static void DrawWorld()
		{
			ModMeshes me = _instance;
			if (me == null || me._handles.Count == 0 || !NativeRenderer.Enabled) return;
			try
			{
				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				BasicEffect effect = GlobalScope.m_Graphics.getBasicEffect();
				// The field's camera: the DS-side matrix the game's own vertices go through on the CPU
				// (the GL model-view stays identity), as GL floats the way glLoadMatrixf reads them;
				// the projection is the frame's, already in the effect.
				float[] a = new float[16];
				GlobalScope.MTX_Copy43ToGLfloat(GlobalScope.NNS_G3dGlb.cameraMtx, a);
				Matrix camera = new Matrix(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
				Matrix projection = effect.Projection;
				foreach (Handle h in me._handles.ToArray())
				{
					if (h.Hidden || h.Removed || h.Model == null || h.Model.Primitives.Count == 0) continue;
					if (h.Playing != null) h.Advance();
					Matrix world = h.Pose * camera;
					for (int i = 0; i < h.Model.Primitives.Count; i++)
					{
						GltfPrimitive p = h.Model.Primitives[i];
						VertexPositionColorTexture[] vertices = h.Posed != null && i < h.Posed.Length && h.Posed[i] != null ? h.Posed[i] : p.Vertices;
						NativeRenderer.Draw(device, 4u, vertices, 0, vertices.Length, world, projection, p.Texture,
							TextureFilter.Linear, TextureAddressMode.Wrap, TextureAddressMode.Wrap,
							alphaTest: !p.Translucent, alphaReference: 0.5f, alphaFunction: CompareFunction.Greater,
							depthTest: true, depthWrite: !p.Translucent, depthFunction: CompareFunction.LessEqual,
							cull: false, cullMode: CullMode.None, destinationBlend: Blend.InverseSourceAlpha);
					}
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "meshes-draw", 3, () => "meshes: draw: " + ex.Message);
			}
		}
	}
}
