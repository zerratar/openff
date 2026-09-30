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
// the install) or a PNG beside the definition. A model (a mesh track) is the game's own object for
// one of its (game:<pack>:0x<id> - eld draws it and plays its motion) or a glTF of the mod's (a path
// from the mod's folder, posed by its clip), placed where the track is. The caster anchor is the
// acting character's hit point in battle, the hero on the field; the sound, flash and shake tracks
// are the battle's own there, the field's API's on the field.
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
			EffectPlayer player = new EffectPlayer(def, _seed++);
			double[] caster = Caster();
			if (caster != null) player.Anchors["caster"] = caster;
			ModEffectObject o = new ModEffectObject(e.Id, System.IO.Path.GetDirectoryName(e.Path), player);
			o.setNumberInformation(category, member);
			PreparePacks(def);
			lock (_live) _live.Add(o);
			return o;
		}

		/// <summary>Where the caster is: the acting character's hit point in battle (the turn system's), the hero on the field; null when neither.</summary>
		private static double[] Caster()
		{
			try
			{
				if (InBattle)
				{
					GlobalScope.btl.TurnSystem turn = GlobalScope.btl.TurnSystem.Current;
					GlobalScope.btl.BaseBattleCharacter who = turn?.nowCharacter();
					if (who == null) return null;
					GlobalScope.VecFx32 at = turn.HitPoint(who);
					return new[] { at.x / 4096.0, at.y / 4096.0, at.z / 4096.0 };
				}
				if (OpenFF.Game.Hero.Present)
				{
					OpenFF.Vector3 hero = OpenFF.Game.Hero.Position;
					return new double[] { hero.X, hero.Y + 6, hero.Z };
				}
			}
			catch (Exception) { }
			return null;
		}

		internal static bool Battle => InBattle;

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

		private static readonly Dictionary<string, GltfModel> _gltf = new Dictionary<string, GltfModel>(StringComparer.OrdinalIgnoreCase);

		/// <summary>A mesh track's glTF: a path from the mod's folder (the one defs/ is in), loaded once.</summary>
		internal static GltfModel Gltf(GraphicsDevice device, string model, string folder)
		{
			if (string.IsNullOrEmpty(model) || model.StartsWith("game:", StringComparison.OrdinalIgnoreCase)) return null;
			string root = System.IO.Path.GetFullPath(System.IO.Path.Combine(folder ?? "", "..", ".."));
			string full = System.IO.Path.IsPathRooted(model) ? model : System.IO.Path.GetFullPath(System.IO.Path.Combine(root, model));
			lock (_gltf)
			{
				if (_gltf.TryGetValue(full, out GltfModel known)) return known;
				GltfModel m = null;
				try { m = File.Exists(full) ? GltfModel.Load(full, device) : null; } catch (Exception ex) { Log.First(LogChannel.General, "effect-gltf-" + full, 1, () => "effects: " + model + ": " + ex.Message); }
				if (m == null) Log.First(LogChannel.General, "effect-gltf-" + full, 1, () => "effects: no model " + model + " (" + full + ")");
				else if (m.Problem != null) Log.First(LogChannel.General, "effect-gltf-" + full, 1, () => "effects: " + model + ": " + m.Problem);
				_gltf[full] = m;
				return m;
			}
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
				int frames = r["frames"] != null ? EffectDefinition.Int(r["frames"]) : 8;
				if (t.Type == "sound")
				{
					// A sound of the game's, by its archive and number (as a cutscene's se clip has them); the archive loaded first when asked.
					int archive = EffectDefinition.Int(r["archive"] ?? r["group"]), number = EffectDefinition.Int(r["number"]);
					int volume = r["volume"] != null ? EffectDefinition.Int(r["volume"]) : 127;
					if (EffectDefinition.Bool(r["load"])) GlobalScope.MatrixSound.MtxSENDS_Load(archive);
					GlobalScope.MatrixSound.MtxSENDS_Play(archive, number, volume, 64);
				}
				else if (t.Type == "shake")
				{
					double power = r["power"] != null ? EffectDefinition.Num(r["power"]) : 0.25;
					if (ModEffects.Battle)
					{
						GlobalScope.VecFx32 strength = new GlobalScope.VecFx32();
						strength.x = strength.y = strength.z = (int)Math.Round(power * 4096);
						GlobalScope.btl.battleDisplay.readyShakeCamera(frames, strength);
					}
					else OpenFF.Game.Camera.Shake(frames, (float)power, r["speed"] != null ? EffectDefinition.Int(r["speed"]) : 2);
				}
				else if (t.Type == "flash")
				{
					// The screen flashed a colour (white when none): count times, frames on and interval off.
					double[] c = EffectDefinition.Numbers(r["colour"]) ?? new double[] { 255, 255, 255 };
					int red = (int)Math.Clamp(c.Length > 0 ? c[0] : 255, 0, 255), green = (int)Math.Clamp(c.Length > 1 ? c[1] : 255, 0, 255), blue = (int)Math.Clamp(c.Length > 2 ? c[2] : 255, 0, 255);
					int interval = r["interval"] != null ? EffectDefinition.Int(r["interval"]) : 2, count = r["count"] != null ? EffectDefinition.Int(r["count"]) : 1;
					if (ModEffects.Battle && GlobalScope.btl.TurnSystem.Current != null)
						GlobalScope.btl.TurnSystem.Current.flash_.setFlashEx((byte)Math.Clamp(count, 1, 255), (short)Math.Max(1, frames), (short)Math.Max(0, interval), GlobalScope.GX_RGB(red >> 3, green >> 3, blue >> 3));
					else OpenFF.Game.Screen.Flash(new OpenFF.Color((byte)red, (byte)green, (byte)blue, 255), Math.Max(1, frames * Math.Max(1, count)), Math.Max(1, interval));
				}
			}
			catch (Exception ex) { Log.First(LogChannel.General, "effect-track-" + _id + t.Type, 2, () => "effects: " + _id + ": " + t.Type + ": " + ex.Message); }
		}

		/// <summary>The models the player has going: the game's object for each, made as its track starts and moved as it rides.</summary>
		private void PlaceModels()
		{
			foreach (EffectModel m in _player.Models())
			{
				if (m.Track.Model == null || !m.Track.Model.StartsWith("game:", StringComparison.OrdinalIgnoreCase)) continue;   // a glTF: drawn here
				if (!_models.TryGetValue(m.Instance, out GlobalScope.eld.IObject o))
				{
					o = ModEffects.CreateModel(m.Track);
					_models[m.Instance] = o;
				}
				o?.SetPosition((int)Math.Round(m.X * 4096), (int)Math.Round(m.Y * 4096), (int)Math.Round(m.Z * 4096));
				// The track's scale (the model's own times it) and yaw, when it gives them - a curve's at this step.
				JsonObject r = m.Track.Raw;
				if (o != null && r?["scale"] != null) o.SetScale((int)Math.Round(m.Scale[0] * 4096), (int)Math.Round(m.Scale[1] * 4096), (int)Math.Round(m.Scale[2] * 4096));
				if (o != null && r?["yaw"] != null) o.SetRotationXYZ(0, (int)Math.Round(m.Yaw / 360.0 * 65536) & 0xFFFF, 0);
			}
		}

		/// <summary>The mesh tracks that are the mod's glTFs: posed by their clip at the track's time, drawn as the mod's meshes are.</summary>
		private void DrawGltf(GraphicsDevice device, Matrix camera, Matrix projection)
		{
			foreach (EffectModel m in _player.Models())
			{
				GltfModel model = ModEffects.Gltf(device, m.Track.Model, _folder);
				if (model == null || model.Primitives.Count == 0) continue;
				JsonObject r = m.Track.Raw;
				VertexPositionColorTexture[][] posed = null;
				string clip = EffectDefinition.Text(r["clip"]);
				if (clip != null && model.File != null)
				{
					OpenFF.Graphics.GltfAnimation anim = model.File.Animations.Find(a => string.Equals(a.Name, clip, StringComparison.OrdinalIgnoreCase));
					if (anim != null)
					{
						double speed = r["speed"] != null ? EffectDefinition.Num(r["speed"]) : 1;
						float time = (float)(m.Frame / 30.0 * speed);
						if (anim.Duration > 0) time = EffectDefinition.Bool(r["loop"]) ? time % anim.Duration : Math.Min(time, anim.Duration);
						OpenFF.Graphics.GltfFile file = model.File;
						file.Pose(file.WorldMatrices(anim, time));
						posed = new VertexPositionColorTexture[file.Meshes.Count][];
						for (int i = 0; i < file.Meshes.Count; i++)
						{
							OpenFF.Graphics.GltfMesh mesh = file.Meshes[i];
							OpenFF.Graphics.GltfMaterial material = mesh.Material >= 0 && mesh.Material < file.Materials.Count ? file.Materials[mesh.Material] : new OpenFF.Graphics.GltfMaterial();
							posed[i] = GltfModel.Vertices(mesh, material);
						}
					}
				}
				double[] s = m.Scale ?? new double[] { 1, 1, 1 };
				XnaVector3 scale = new XnaVector3((float)s[0], (float)s[1], (float)s[2]);
				float yaw = (float)m.Yaw;
				Matrix world = Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.ToRadians(yaw)) * Matrix.CreateTranslation((float)m.X, (float)m.Y, (float)m.Z) * camera;
				for (int i = 0; i < model.Primitives.Count; i++)
				{
					GltfPrimitive p = model.Primitives[i];
					VertexPositionColorTexture[] vertices = posed != null && i < posed.Length && posed[i] != null ? posed[i] : p.Vertices;
					using (FrameCapture.Own(m.Instance, i + 1))
					{
						NativeRenderer.Draw(device, 4u, vertices, 0, vertices.Length, world, projection, p.Texture,
							TextureFilter.Linear, TextureAddressMode.Wrap, TextureAddressMode.Wrap,
							alphaTest: !p.Translucent, alphaReference: 0.5f, alphaFunction: CompareFunction.Greater,
							depthTest: true, depthWrite: !p.Translucent, depthFunction: CompareFunction.LessEqual,
							cull: false, cullMode: CullMode.None, destinationBlend: Blend.InverseSourceAlpha);
					}
				}
			}
		}

		public void Draw(GraphicsDevice device, Matrix camera, Matrix projection, XnaVector3 right, XnaVector3 up)
		{
			if (_dead) return;
			DrawGltf(device, camera, projection);
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
				if (q.Roll != 0)
				{
					// The spin: the quad turned in the screen's plane (w and h its half-width and half-height, turned).
					float cos = (float)Math.Cos(q.Roll * Math.PI / 180), sin = (float)Math.Sin(q.Roll * Math.PI / 180);
					XnaVector3 wr = right * (float)(q.HalfWidth * cos) + up * (float)(q.HalfWidth * sin);
					XnaVector3 hr = up * (float)(q.HalfHeight * cos) - right * (float)(q.HalfHeight * sin);
					w = wr; h = hr;
				}
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
