// The mods' own effects (defs/effects/<id>.json, Docs/Effects-Plan.md's format) played in the game.
//
// Each definition gets a category of its own, from 1000 up - numbers effect.efi never uses - so the
// game asks for one the way it asks for its own: a spell's record naming it (defs/spells "effect",
// "cast"), IEffects.Spawn, anything that calls eld's createObject. eld.Manager.createObject hands
// such a category here and gets back a ModEffectObject, an eld object like the game's: the battle
// places it (TurnSystem.setHitEffectPosition), steps it (Calculate, a game step), asks whether it
// still plays and deletes it, none the wiser. BattleEffect.addEfp has no pack to load for one.
//
// The object steps the shared player (Shared/Effects/EffectPlayer.cs - Crystal's Stage plays the
// same one's twin) and Scene.draw draws its quads right after the game's own effects: camera-facing,
// in world units, through NativeRenderer.Draw with the depth test and without writing depth, alpha or
// additive; each particle's draws its own (FrameCapture.Own), so at 60 frames a second a particle is
// drawn between its two steps. A texture is the game's (game:<pack>:<name>, decoded from the pack in
// the install) or a PNG beside the definition. A model (a mesh track: game:<pack>:0x<id>) is the
// game's own object for it - eld draws it and plays its motion - placed where the track is.
//
// A definition changed while the game runs plays as saved the next time it starts.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenFF.Effects;

namespace OpenFF.Client
{
	using Color = Microsoft.Xna.Framework.Color;
	using XnaVector3 = Microsoft.Xna.Framework.Vector3;
	using Vector2 = Microsoft.Xna.Framework.Vector2;

	internal static class ModEffects
	{
		public const string Folder = "defs/effects";
		public const int FirstCategory = 1000;

		private sealed class Entry
		{
			public string Id, Path;
			public int Category;
			public EffectDefinition Definition;
			public DateTime Stamp;
		}

		private static readonly Dictionary<string, Entry> _byId = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<int, Entry> _byCategory = new Dictionary<int, Entry>();
		private static readonly List<ModEffectObject> _live = new List<ModEffectObject>();
		private static readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
		private static readonly Dictionary<string, EfpPack> _packs = new Dictionary<string, EfpPack>(StringComparer.OrdinalIgnoreCase);
		private static EfiIndex _index;
		private static uint _seed = 1;

		public static IReadOnlyCollection<string> Ids => _byId.Keys;

		/// <summary>The definitions of the mods (the project's first): their ids by file name, a category each in the order of their ids.</summary>
		public static void Register(IEnumerable<string> roots)
		{
			_byId.Clear();
			_byCategory.Clear();
			_index = null;
			foreach (string root in roots ?? Enumerable.Empty<string>())
			{
				string dir = System.IO.Path.Combine(root, Folder.Replace('/', System.IO.Path.DirectorySeparatorChar));
				if (!Directory.Exists(dir)) continue;
				foreach (string file in Directory.EnumerateFiles(dir, "*.json"))
				{
					string id = System.IO.Path.GetFileNameWithoutExtension(file);
					if (_byId.ContainsKey(id)) { Log.Write(LogChannel.General, "effects: " + id + " (" + file + ") - an earlier mod has one of that name"); continue; }
					_byId[id] = new Entry { Id = id, Path = file };
				}
			}
			int next = FirstCategory;
			foreach (Entry e in _byId.Values.OrderBy(e => e.Id, StringComparer.OrdinalIgnoreCase))
			{
				e.Category = next++;
				_byCategory[e.Category] = e;
			}
			if (_byId.Count > 0) Log.Write(LogChannel.General, "effects: " + _byId.Count + " of the mods': " + string.Join(", ", _byId.Values.OrderBy(e => e.Category).Select(e => e.Id + " (" + e.Category + ")")));
		}

		/// <summary>The category a mod's effect plays as, by its id; null when no mod has one of that name.</summary>
		public static int? Category(string id) => id != null && _byId.TryGetValue(id.Trim(), out Entry e) ? e.Category : (int?)null;

		/// <summary>Whether a category is one of the mods' effects.</summary>
		public static bool Is(int category) => category >= FirstCategory && _byCategory.ContainsKey(category);

		/// <summary>A category's definition, read again when its file has changed since.</summary>
		private static EffectDefinition Definition(Entry e)
		{
			try
			{
				DateTime stamp = File.GetLastWriteTimeUtc(e.Path);
				if (e.Definition == null || stamp != e.Stamp)
				{
					e.Definition = EffectDefinition.Read(JsonNode.Parse(File.ReadAllText(e.Path)).AsObject());
					e.Stamp = stamp;
				}
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "effect-read-" + e.Id, 3, () => "effects: " + e.Id + ": " + ex.Message);
			}
			return e.Definition;
		}

		/// <summary>eld.Manager.createObject for one of the mods' categories: the object to run, or null.</summary>
		public static GlobalScope.eld.IObject Create(int category, int member)
		{
			if (!_byCategory.TryGetValue(category, out Entry e)) return null;
			EffectDefinition def = Definition(e);
			if (def == null) return null;
			ModEffectObject o = new ModEffectObject(e.Id, System.IO.Path.GetDirectoryName(e.Path), new EffectPlayer(def, _seed++));
			o.setNumberInformation(category, member);
			PreparePacks(def);
			lock (_live) _live.Add(o);
			return o;
		}

		internal static void Forget(ModEffectObject o)
		{
			lock (_live) _live.Remove(o);
		}

		// ------------------------------------------------------------------ the game's models

		/// <summary>A mesh track's model is the game's template: its pack loaded now, so it is there when the track starts.</summary>
		private static void PreparePacks(EffectDefinition def)
		{
			foreach (EffectTrack t in def.Tracks)
			{
				if (t.Type != "mesh" || !Game(t.Model, out string pack, out _)) continue;
				int category = PackCategory(pack);
				if (category < 0 || GlobalScope.eld.g_elsvr == null) continue;
				try
				{
					if (InBattle) GlobalScope.btl.BattleEffect.instance().addEfp(category);
				}
				catch (Exception ex) { Log.First(LogChannel.General, "effect-pack-" + pack, 2, () => "effects: " + pack + ": " + ex.Message); }
			}
		}

		private static bool InBattle
		{
			get
			{
				try { return (GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart() == GlobalScope.GAMEPART.GAMEPART_BATTLE; }
				catch (Exception) { return false; }
			}
		}

		/// <summary>The game's object for a mesh track's model: the category and member effect.efi gives its template.</summary>
		internal static GlobalScope.eld.IObject CreateModel(EffectTrack t)
		{
			if (!Game(t.Model, out string pack, out string what) || !what.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return null;
			if (!uint.TryParse(what.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint id)) return null;
			EfiIndex index = Index();
			if (index == null) return null;
			(int category, int member) = index.Find(id, PackCategory(pack));
			if (category < 0) return null;
			try { return GlobalScope.eld.g_elsvr.createObject((uint)category, (uint)member); }
			catch (Exception ex) { Log.First(LogChannel.General, "effect-model-" + t.Model, 2, () => "effects: " + t.Model + ": " + ex.Message); return null; }
		}

		private static EfiIndex Index()
		{
			if (_index == null)
			{
				byte[] data = Read("effect.efi");
				if (data != null) try { _index = EfiIndex.Read(data); } catch (Exception) { }
			}
			return _index;
		}

		private static int PackCategory(string pack)
		{
			string stem = System.IO.Path.GetFileNameWithoutExtension(pack ?? "");
			return stem.Length == 4 && (stem[0] == 'e' || stem[0] == 'E') && int.TryParse(stem.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1;
		}

		/// <summary>"game:e332.efp:0x107e" as its pack and what of it.</summary>
		private static bool Game(string reference, out string pack, out string what)
		{
			pack = what = null;
			if (reference == null || !reference.StartsWith("game:", StringComparison.OrdinalIgnoreCase)) return false;
			string[] parts = reference.Substring(5).Split(new[] { ':' }, 2);
			if (parts.Length != 2) return false;
			pack = parts[0]; what = parts[1];
			return true;
		}

		private static byte[] Read(string name)
		{
			ContentChain chain = GameArchive.Chain;
			if (chain == null) return null;
			return chain.Read(name) ?? chain.Read("files/" + name);
		}

		// ------------------------------------------------------------------ drawing

		/// <summary>From Scene.draw, after the game's own effects: every mod effect's quads, with the scene's camera.</summary>
		public static void DrawScene()
		{
			if (!NativeRenderer.Enabled) return;
			ModEffectObject[] live;
			lock (_live) { if (_live.Count == 0) return; live = _live.ToArray(); }
			try
			{
				GraphicsDevice device = GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
				BasicEffect effect = GlobalScope.m_Graphics.getBasicEffect();
				float[] a = new float[16];
				GlobalScope.MTX_Copy43ToGLfloat(GlobalScope.NNS_G3dGlb.cameraMtx, a);
				Matrix camera = new Matrix(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
				Matrix projection = effect.Projection;
				// The camera's right and up in the world: a quad faces the camera, as eld's are.
				XnaVector3 right = XnaVector3.Normalize(new XnaVector3(camera.M11, camera.M21, camera.M31));
				XnaVector3 up = XnaVector3.Normalize(new XnaVector3(camera.M12, camera.M22, camera.M32));
				foreach (ModEffectObject o in live) o.Draw(device, camera, projection, right, up);
			}
			catch (Exception ex)
			{
				Log.First(LogChannel.General, "effects-draw", 3, () => "effects: draw: " + ex.Message);
			}
		}

		/// <summary>A track's texture: the game's, decoded from its pack, or a PNG beside the definition; null draws white.</summary>
		internal static Texture2D Texture(GraphicsDevice device, string image, string folder)
		{
			if (string.IsNullOrEmpty(image)) return null;
			string key = image.StartsWith("game:", StringComparison.OrdinalIgnoreCase) ? image : System.IO.Path.Combine(folder ?? "", image);
			lock (_textures)
			{
				if (_textures.TryGetValue(key, out Texture2D known)) return known;
				Texture2D made = null;
				try
				{
					if (Game(image, out string pack, out string name))
					{
						if (!_packs.TryGetValue(pack, out EfpPack p))
						{
							byte[] data = Read(pack);
							p = data != null && EfpPack.IsPack(data) ? EfpPack.Read(data, pack) : null;
							_packs[pack] = p;
						}
						NtpkTexture t = p?.Textures.Values.FirstOrDefault(x => x != null && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
						if (t != null)
						{
							made = new Texture2D(device, t.Width, t.Height, false, SurfaceFormat.Color);
							made.SetData(t.DecodeRgba());
						}
					}
					else if (File.Exists(key))
					{
						using FileStream stream = File.OpenRead(key);
						made = Texture2D.FromStream(device, stream);
					}
					if (made == null) Log.First(LogChannel.General, "effect-texture-" + key, 1, () => "effects: no texture " + image);
				}
				catch (Exception ex) { Log.First(LogChannel.General, "effect-texture-" + key, 1, () => "effects: texture " + image + ": " + ex.Message); }
				_textures[key] = made;
				return made;
			}
		}
	}

	/// <summary>A mod's effect as an eld object: the game places, steps and deletes it as it does its own.</summary>
	internal sealed class ModEffectObject : GlobalScope.eld.IObject
	{
		private readonly string _id, _folder;
		private readonly EffectPlayer _player;
		private readonly List<EffectQuad> _quads = new List<EffectQuad>();
		private readonly Dictionary<object, GlobalScope.eld.IObject> _models = new Dictionary<object, GlobalScope.eld.IObject>();
		private bool _dead;
		private int _x, _y, _z;
		private VertexPositionColorTexture[] _vertices = new VertexPositionColorTexture[6];

		public ModEffectObject(string id, string folder, EffectPlayer player)
		{
			_id = id;
			_folder = folder;
			_player = player;
		}

		public override bool Initialize(GlobalScope.eld.IGL rGL) => true;

		public override bool Calculate()
		{
			if (_dead) return true;
			// STOP (eld's Stop): a loop finishes its cycle, as the game's loops do.
			if ((GetStatus() & 8u) != 0) _player.Stop();
			_player.Step();
			foreach (EffectTrack t in _player.Started) Play(t);
			PlaceModels();
			return true;
		}

		public override bool isPlay() => !_dead && !_player.Finished;
		public override bool isLoop() => _player.Definition.Loop;

		public override void SetPosition(int x, int y, int z)
		{
			_x = x; _y = y; _z = z;
			_player.Anchor = new[] { x / 4096.0, y / 4096.0, z / 4096.0 };
		}

		public override void SetPosition(GlobalScope.ds.Vector3<int> pos) => SetPosition(pos.vx, pos.vy, pos.vz);

		public override void GetPosition(out int x, out int y, out int z) { x = _x; y = _y; z = _z; }

		public override bool Terminate()
		{
			End();
			return true;
		}

		public override void DeleteObject()
		{
			base.DeleteObject();
			End();
		}

		private void End()
		{
			if (_dead) return;
			_dead = true;
			foreach (GlobalScope.eld.IObject m in _models.Values) try { m?.DeleteObject(); } catch (Exception) { }
			_models.Clear();
			ModEffects.Forget(this);
		}

		/// <summary>The tracks that are not particles, as they start: a sound of the game's, a shake of the battle's camera.</summary>
		private void Play(EffectTrack t)
		{
			try
			{
				JsonObject r = t.Raw;
				if (t.Type == "sound")
				{
					int group = EffectDefinition.Int(r["group"]), number = EffectDefinition.Int(r["number"]);
					GlobalScope.btl.BattleSE.instance().load(group);
					GlobalScope.btl.BattleSE.instance().play(group, number);
				}
				else if (t.Type == "shake" && GlobalScope.btl.BattleEffect.instance() != null)
				{
					double power = EffectDefinition.Num(r["power"]);
					int frames = r["frames"] != null ? EffectDefinition.Int(r["frames"]) : 10;
					GlobalScope.VecFx32 strength = new GlobalScope.VecFx32();
					strength.x = strength.y = strength.z = (int)Math.Round((power != 0 ? power : 0.25) * 4096);
					GlobalScope.btl.battleDisplay.readyShakeCamera(frames, strength);
				}
			}
			catch (Exception ex) { Log.First(LogChannel.General, "effect-track-" + _id + t.Type, 2, () => "effects: " + _id + ": " + t.Type + ": " + ex.Message); }
		}

		/// <summary>The models the player has going: the game's object for each, made as its track starts and moved as it rides.</summary>
		private void PlaceModels()
		{
			foreach (EffectModel m in _player.Models())
			{
				if (!_models.TryGetValue(m.Instance, out GlobalScope.eld.IObject o))
				{
					o = ModEffects.CreateModel(m.Track);
					_models[m.Instance] = o;
				}
				o?.SetPosition((int)Math.Round(m.X * 4096), (int)Math.Round(m.Y * 4096), (int)Math.Round(m.Z * 4096));
			}
		}

		public void Draw(GraphicsDevice device, Matrix camera, Matrix projection, XnaVector3 right, XnaVector3 up)
		{
			if (_dead) return;
			_player.Quads(_quads);
			foreach (EffectQuad q in _quads)
			{
				EffectTexture tex = q.Track.Texture;
				Texture2D picture = ModEffects.Texture(device, tex?.Image, _folder);
				float u0 = 0, v0 = 0, u1 = 1, v1 = 1;
				if (tex != null)
				{
					int[] cell = tex.Cell ?? new[] { 0, 0, tex.Width, tex.Height };
					int gx = tex.Columns > 0 ? q.Cell % tex.Columns : 0, gy = tex.Columns > 0 ? q.Cell / tex.Columns : 0;
					u0 = (cell[0] + cell[2] * gx) / (float)tex.Width; v0 = (cell[1] + cell[3] * gy) / (float)tex.Height;
					u1 = u0 + cell[2] / (float)tex.Width; v1 = v0 + cell[3] / (float)tex.Height;
				}
				XnaVector3 c = new XnaVector3((float)q.X, (float)q.Y, (float)q.Z);
				XnaVector3 w = right * (float)q.HalfWidth, h = up * (float)q.HalfHeight;
				Color colour = new Color((int)Math.Clamp(q.R, 0, 255), (int)Math.Clamp(q.G, 0, 255), (int)Math.Clamp(q.B, 0, 255), (int)Math.Clamp(q.A, 0, 255));
				_vertices[0] = new VertexPositionColorTexture(c - w + h, colour, new Vector2(u0, v0));
				_vertices[1] = new VertexPositionColorTexture(c - w - h, colour, new Vector2(u0, v1));
				_vertices[2] = new VertexPositionColorTexture(c + w - h, colour, new Vector2(u1, v1));
				_vertices[3] = _vertices[0];
				_vertices[4] = _vertices[2];
				_vertices[5] = new VertexPositionColorTexture(c + w + h, colour, new Vector2(u1, v0));
				bool additive = string.Equals(q.Track.Blend, "additive", StringComparison.OrdinalIgnoreCase);
				using (FrameCapture.Own(q.Particle, q.Trail + 1))
				{
					NativeRenderer.Draw(device, 4u, _vertices, 0, 6, camera, projection, picture,
						TextureFilter.Point, TextureAddressMode.Clamp, TextureAddressMode.Clamp,
						alphaTest: false, alphaReference: 0f, alphaFunction: CompareFunction.Greater,
						depthTest: true, depthWrite: false, depthFunction: CompareFunction.LessEqual,
						cull: false, cullMode: CullMode.None, destinationBlend: additive ? Blend.One : Blend.InverseSourceAlpha);
				}
			}
		}
	}
}
