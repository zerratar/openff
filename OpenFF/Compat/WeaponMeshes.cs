// A mod weapon's own look in battle: a glTF drawn in the hand in place of the game's w###.
//
// The game gives every weapon a model by its record's graphId (w005 for a sword's shape), loads
// it as a second-priority character, and each frame poses it from the wielder's hand joint
// (BattlePlayer.haveWeapon: R_te / L_te, turned and offset into the grip). A mod item's
// "model" (Shared/Data/ModItems.cs) keeps all of that - the w### is loaded, posed, hidden and
// faded as the game does - and only the draw is taken over: the w###'s render object gets a
// stand-in (CRenderObject.StandIn) that draws the glTF at the same pose matrix, through
// NativeRenderer with the battle's camera and projection, as ModMeshes draws a scene's models
// on the field. So a sheathed, hidden, fading or frog-shrunk weapon behaves as before, and the
// author's file sits exactly where a w### would.
//
// The hand's frame is the w###'s: the grip at the origin, the blade along +z, the guard across
// y (a sword is about 7 units long, w005 runs z -0.9..6.2). "modelScale" scales the file into
// it; "modelClip" loops one of the file's animations (a rune glowing, a gem turning) through
// the same clip player the scene meshes use.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenFF.Data;

namespace OpenFF.Client
{
	// MonoGame's Color by name: the engine's OpenFF.Color sits a namespace up.
	using Color = Microsoft.Xna.Framework.Color;

	internal static class WeaponMeshes
	{
		private sealed class Look
		{
			public string Path;
			public float Scale = 1f;
			public string Clip;
			public GltfModel Model;
			public OpenFF.Graphics.GltfAnimation Playing;
			public float Time;
			public long LastTick;
			public VertexPositionColorTexture[][] Posed;
		}

		/// <summary>Item number -> its look, from the definitions in play.</summary>
		private static readonly Dictionary<int, Look> _looks = new Dictionary<int, Look>();

		/// <summary>The definitions' looks, when the mods are registered (ModItemsLayer.Register). Items without a model are left to the game.</summary>
		public static void Register(IEnumerable<ModItem> items)
		{
			_looks.Clear();
			foreach (ModItem item in items ?? Array.Empty<ModItem>())
			{
				string path = item.ModelPath;
				if (path == null) continue;
				_looks[item.Number] = new Look { Path = path, Scale = item.ModelScale <= 0 ? 1f : item.ModelScale, Clip = item.ModelClip };
				Log.Write(LogChannel.File, "weapons: item " + item.Number + " (" + (item.Name ?? item.Id) + ") looks like " + item.Model + (File.Exists(path) ? "" : " - no such file"));
			}
		}

		/// <summary>Whether an item has a look of its own.</summary>
		public static bool Has(int itemNumber) => _looks.ContainsKey(itemNumber);

		/// <summary>
		/// The game made a weapon's w### character for an item: from now on its draws are the
		/// glTF's. Called right after CCharacterMng.setCharacter in BattlePlayer; the model may
		/// still be loading, the stand-in waits on the render object for it.
		/// </summary>
		public static void Attach(int characterMngId, int itemNumber)
		{
			if (characterMngId < 0 || !_looks.TryGetValue(itemNumber, out Look look)) return;
			if (look.Model == null) look.Model = LoadModel(look);
			if (look.Model == null || look.Model.Primitives.Count == 0) return;
			if (look.Clip != null && look.Playing == null && look.Model.File != null)
			{
				look.Playing = look.Model.File.Animations.Find(a => string.Equals(a.Name, look.Clip, StringComparison.OrdinalIgnoreCase));
				if (look.Playing == null) Log.Write(LogChannel.General, "weapons: " + Path.GetFileName(look.Path) + " has no clip \"" + look.Clip + "\"");
			}
			GlobalScope.characterMng.setStandIn(characterMngId, ro => Draw(look, ro));
			Log.Write(LogChannel.File, "weapons: item " + itemNumber + " drawn as " + Path.GetFileName(look.Path) + " (character " + characterMngId + ")");
		}

		private static GltfModel LoadModel(Look look)
		{
			try
			{
				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				if (!File.Exists(look.Path))
				{
					Log.Write(LogChannel.General, "weapons: " + look.Path + ": no such file - the game's model stays");
					return null;
				}
				GltfModel model = GltfModel.Load(look.Path, device);
				if (model.Problem != null) Log.Write(LogChannel.General, "weapons: " + Path.GetFileName(look.Path) + ": " + model.Problem);
				else Log.Write(LogChannel.General, "weapons: " + Path.GetFileName(look.Path) + ": " + model.Triangles + " triangles, " + model.Primitives.Count + " part(s)" + (model.File != null ? ", " + model.File.Animations.Count + " clip(s)" : ""));
				return model;
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "weapons: " + Path.GetFileName(look.Path) + ": " + ex.Message);
				return null;
			}
		}

		private static readonly GlobalScope.MtxFx43 _pose = new GlobalScope.MtxFx43();
		private static readonly float[] _floats = new float[16];

		private static Matrix ToMatrix(GlobalScope.MtxFx43 m)
		{
			float[] a = _floats;
			GlobalScope.MTX_Copy43ToGLfloat(m, a);
			return new Matrix(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
		}

		/// <summary>The stand-in's draw: the glTF at the render object's pose, with the scene's camera, faded as the object is.</summary>
		private static void Draw(Look look, GlobalScope.ds.sys3d.CRenderObject ro)
		{
			if (!NativeRenderer.Enabled || look.Model == null) return;
			try
			{
				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				BasicEffect effect = GlobalScope.m_Graphics.getBasicEffect();
				ro.getPoseMtx(_pose);
				Matrix pose = ToMatrix(_pose);
				Matrix camera = ToMatrix(GlobalScope.NNS_G3dGlb.cameraMtx);
				Matrix world = Matrix.CreateScale(look.Scale) * pose * camera;
				Matrix projection = effect.Projection;
				if (look.Playing != null) Advance(look);
				int alphaRate = Math.Max(0, Math.Min(100, ro.getAlphaRate()));
				if (alphaRate == 0) return;
				Log.First(LogChannel.File, "weapons-drawn-" + look.Path, 1, () => "weapons: " + Path.GetFileName(look.Path) + " drawn in the hand at " + pose.Translation.X.ToString("0.#") + ", " + pose.Translation.Y.ToString("0.#") + ", " + pose.Translation.Z.ToString("0.#"));
				for (int i = 0; i < look.Model.Primitives.Count; i++)
				{
					GltfPrimitive p = look.Model.Primitives[i];
					VertexPositionColorTexture[] vertices = look.Posed != null && i < look.Posed.Length && look.Posed[i] != null ? look.Posed[i] : p.Vertices;
					bool translucent = p.Translucent;
					if (alphaRate < 100)
					{
						vertices = Faded(vertices, alphaRate / 100f);
						translucent = true;
					}
					NativeRenderer.Draw(device, 4u, vertices, 0, vertices.Length, world, projection, p.Texture,
						TextureFilter.Linear, TextureAddressMode.Wrap, TextureAddressMode.Wrap,
						alphaTest: !translucent, alphaReference: 0.5f, alphaFunction: CompareFunction.Greater,
						depthTest: true, depthWrite: !translucent, depthFunction: CompareFunction.LessEqual,
						cull: false, cullMode: CullMode.None, destinationBlend: Blend.InverseSourceAlpha);
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "weapons-draw", 3, () => "weapons: draw: " + ex.Message);
			}
		}

		private static VertexPositionColorTexture[] Faded(VertexPositionColorTexture[] vertices, float alpha)
		{
			VertexPositionColorTexture[] faded = new VertexPositionColorTexture[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				VertexPositionColorTexture v = vertices[i];
				Color c = v.Color;
				faded[i] = new VertexPositionColorTexture(v.Position, new Color(c.R, c.G, c.B, (byte)Math.Round(c.A * alpha)), v.TextureCoordinate);
			}
			return faded;
		}

		/// <summary>The clip a step further, looping, and the frame's vertices - as ModMeshes.Handle.Advance.</summary>
		private static void Advance(Look look)
		{
			OpenFF.Graphics.GltfFile file = look.Model.File;
			if (file == null || look.Playing == null) return;
			long now = Environment.TickCount64;
			float dt = look.LastTick == 0 ? 0 : Math.Min(0.25f, (now - look.LastTick) / 1000f);
			look.LastTick = now;
			look.Time += dt;
			if (look.Playing.Duration > 0) look.Time %= look.Playing.Duration;
			file.Pose(file.WorldMatrices(look.Playing, look.Time));
			if (look.Posed == null || look.Posed.Length != file.Meshes.Count) look.Posed = new VertexPositionColorTexture[file.Meshes.Count][];
			for (int i = 0; i < file.Meshes.Count; i++)
			{
				OpenFF.Graphics.GltfMesh mesh = file.Meshes[i];
				OpenFF.Graphics.GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new OpenFF.Graphics.GltfMaterial();
				look.Posed[i] = GltfModel.Vertices(mesh, material);
			}
		}
	}
}
