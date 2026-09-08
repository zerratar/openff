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
			public override OpenFF.Vector3 Position { get => _position; set => _position = value; }
			public override float Yaw { get => _yaw; set => _yaw = value; }
			public override float Scale { get => _scale; set => _scale = value; }
			public override bool Hidden { get; set; }
			public override OpenFF.Vector3 Min => Model == null ? default : new OpenFF.Vector3(Model.Min.X, Model.Min.Y, Model.Min.Z);
			public override OpenFF.Vector3 Max => Model == null ? default : new OpenFF.Vector3(Model.Max.X, Model.Max.Y, Model.Max.Z);
			public override int Triangles => Model?.Triangles ?? 0;
			public override string Problem => Model?.Problem;
			public override void Remove() { Removed = true; _instance?._handles.Remove(this); }
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
					Matrix world = Matrix.CreateScale(h.Scale) * Matrix.CreateRotationY(MathHelper.ToRadians(h.Yaw)) * Matrix.CreateTranslation(new XnaVector3(h.Position.X, h.Position.Y, h.Position.Z)) * camera;
					foreach (GltfPrimitive p in h.Model.Primitives)
					{
						NativeRenderer.Draw(device, 4u, p.Vertices, 0, p.Vertices.Length, world, projection, p.Texture,
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
