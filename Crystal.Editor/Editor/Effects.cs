// The game's effects in Crystal's Effects library: the packs (e###.efp and the field's), each
// named after what plays it - the spells whose record points at the pack (player.chaindata chain 12),
// the schools' casts - with its members as effect.efi numbers them; one member imported into the
// new format for the Stage to play (Shared/Effects/EffectImport.cs), and the pack's textures as PNG.
//
// The project's own effects (defs/effects/<id>.json) are listed first and open on the same Stage,
// their PNGs read from beside them (/api/effect/own-texture?effect=&name=).
//
// Server: /api/effects (list), /api/effect?name= (a pack), /api/effect/import?category=&member=,
// /api/effect/texture?pack=&name=, and an effect's model: /api/effect/model?pack=&id= (its buffers,
// as the model viewer has them, and its motion posed frame by frame), /api/effect/model/texture.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using OpenFF.Data;
using OpenFF.Effects;

namespace Crystal.Editor
{
	internal static class Effects
	{
		private sealed class Cache
		{
			public int Version;
			public EfiIndex Index;
			public readonly Dictionary<string, EfpPack> Packs = new Dictionary<string, EfpPack>(StringComparer.OrdinalIgnoreCase);
			public Dictionary<uint, string> PackOfId;
			public Dictionary<(int Category, int Member), List<string>> Uses;
			/// <summary>The workspace's names by file name: files/e331.efp as e331.efp.</summary>
			public Dictionary<string, string> Names;
		}

		private static readonly Dictionary<Workspace, Cache> _caches = new Dictionary<Workspace, Cache>();

		/// <summary>What the game names by category: the schools' casts, the summons' opening.</summary>
		private static readonly Dictionary<int, string> Known = new Dictionary<int, string>
		{
			[406] = "monsters' cast", [407] = "black magic cast", [408] = "white magic cast", [243] = "summon cast",
			[201] = "the battle's common", [435] = "the battle's common",
		};

		private static Cache For(Workspace workspace)
		{
			lock (_caches)
			{
				if (_caches.TryGetValue(workspace, out Cache c) && c.Version == workspace.Version) return c;
				c = new Cache { Version = workspace.Version, Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) };
				foreach (WorkspaceEntry entry in workspace.List(".efp", ".efi", ".chaindata", ".lz"))
				{
					string file = System.IO.Path.GetFileName(entry.Name);
					if (!c.Names.ContainsKey(file)) c.Names[file] = entry.Name;
				}
				try { c.Index = EfiIndex.Read(workspace.Read(Named(c, "effect.efi"))); } catch (Exception) { c.Index = null; }
				_caches[workspace] = c;
				return c;
			}
		}

		/// <summary>A file's name in the workspace from its file name (the name itself when the workspace has no such file).</summary>
		private static string Named(Cache c, string file) => c.Names.TryGetValue(System.IO.Path.GetFileName(file ?? ""), out string name) ? name : file;

		private static EfpPack Pack(Workspace workspace, Cache c, string name)
		{
			name = Named(c, name);
			lock (c)
			{
				if (c.Packs.TryGetValue(name, out EfpPack p)) return p;
				try { p = workspace.Exists(name) ? EfpPack.Read(workspace.Read(name), System.IO.Path.GetFileName(name)) : null; } catch (Exception) { p = null; }
				c.Packs[name] = p;
				return p;
			}
		}

		public const string OwnFolder = "defs/effects";

		/// <summary>The project's effect definitions, by name (defs/effects/x.json).</summary>
		private static IEnumerable<string> Own(Project project)
		{
			string dir = project == null ? null : System.IO.Path.Combine(project.Directory, "defs", "effects");
			if (dir == null || !System.IO.Directory.Exists(dir)) return Enumerable.Empty<string>();
			return System.IO.Directory.EnumerateFiles(dir, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).Select(f => OwnFolder + "/" + System.IO.Path.GetFileName(f));
		}

		public static bool IsOwn(string name) => name != null && name.StartsWith(OwnFolder + "/", StringComparison.OrdinalIgnoreCase);

		private static string OwnPath(Project project, string name)
		{
			if (project == null || !IsOwn(name)) throw new InvalidOperationException("no project effect " + name);
			string file = System.IO.Path.GetFileName(name.Substring(OwnFolder.Length + 1));
			return System.IO.Path.Combine(project.Directory, "defs", "effects", file);
		}

		public static object List(Workspace workspace, Project project = null)
		{
			Cache c = For(workspace);
			Dictionary<(int, int), List<string>> uses = Uses(workspace, c);
			List<object> list = new List<object>();
			foreach (string own in Own(project)) list.Add(new { name = own, category = -1, templates = 0, note = "the mod's effect (defs/effects)", own = true });
			foreach (WorkspaceEntry entry in workspace.List(".efp").OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
			{
				int category = Category(entry.Name);
				EfpPack pack = Pack(workspace, c, entry.Name);
				list.Add(new
				{
					name = entry.Name,
					category,
					templates = pack?.Templates.Count ?? 0,
					note = Describe(category, uses),
				});
			}
			return list;
		}

		public static object Read(Workspace workspace, string name, Project project = null)
		{
			if (IsOwn(name))
			{
				// One of the project's: its definition as it is, played as written.
				JsonNode effect = JsonNode.Parse(System.IO.File.ReadAllText(OwnPath(project, name)));
				return new { name, category = -1, own = true, note = "the mod's effect (defs/effects)", effect, templates = new object[0], textures = new object[0] };
			}
			Cache c = For(workspace);
			EfpPack pack = Pack(workspace, c, name) ?? throw new InvalidOperationException("not an effect pack: " + name);
			int category = Category(name);
			Dictionary<(int, int), List<string>> uses = Uses(workspace, c);
			List<object> templates = new List<object>();
			foreach (EffectTemplate t in pack.Templates)
			{
				(int cat, int member) = c.Index != null ? c.Index.Find(t.Id, category) : (-1, -1);
				uses.TryGetValue((cat, member), out List<string> by);
				templates.Add(new
				{
					id = "0x" + t.Id.ToString("x"),
					kind = t.Kind.ToString(),
					category = cat,
					member,
					texture = (t as ParticleTemplate)?.Texture?.Name,
					used = by,
					summary = Summary(t),
				});
			}
			return new
			{
				name,
				category,
				note = Describe(category, uses),
				templates,
				textures = pack.Textures.Values.Where(x => x != null).Select(x => new { name = x.Name, width = x.Width, height = x.Height, format = Format(x.Format) }).ToList(),
			};
		}

		public static object Import(Workspace workspace, int category, int member)
		{
			List<string> notes = new List<string>();
			JsonObject effect = ImportJson(workspace, category, member, notes);
			return new { effect, notes };
		}

		/// <summary>One of the game's effects as a definition of the new format.</summary>
		public static JsonObject ImportJson(Workspace workspace, int category, int member, List<string> notes = null)
		{
			Cache c = For(workspace);
			if (c.Index == null) throw new InvalidOperationException("no effect.efi in this game");
			return EffectImport.Import(category, member, (cat, mem) => Resolve(workspace, c, cat, mem), notes);
		}

		/// <summary>Every picture of the game's effect packs, as a track's texture names it (game:pack:name), for the picker.</summary>
		public static object Textures(Workspace workspace)
		{
			Cache c = For(workspace);
			List<object> list = new List<object>();
			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (WorkspaceEntry entry in workspace.List(".efp").OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
			{
				EfpPack pack = Pack(workspace, c, entry.Name);
				if (pack == null) continue;
				string file = System.IO.Path.GetFileName(entry.Name);
				foreach (NtpkTexture t in pack.Textures.Values)
				{
					if (t == null || !seen.Add(file + ":" + t.Name)) continue;
					list.Add(new { image = "game:" + file + ":" + t.Name, pack = file, name = t.Name, width = t.Width, height = t.Height });
				}
			}
			return list;
		}

		/// <summary>A PNG beside one of the project's effect definitions.</summary>
		public static byte[] OwnTexture(Project project, string effect, string name)
		{
			string dir = System.IO.Path.GetDirectoryName(OwnPath(project, effect));
			string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(dir, name ?? ""));
			if (!path.StartsWith(System.IO.Path.GetFullPath(project.Directory), StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("outside the project: " + name);
			return System.IO.File.ReadAllBytes(path);
		}

		public static byte[] TexturePng(Workspace workspace, string pack, string name)
		{
			Cache c = For(workspace);
			EfpPack p = Pack(workspace, c, pack) ?? throw new InvalidOperationException("not an effect pack: " + pack);
			NtpkTexture t = p.Textures.Values.FirstOrDefault(x => x != null && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
				?? throw new InvalidOperationException("no texture " + name + " in " + pack);
			return Png.Encode(t.Width, t.Height, t.DecodeRgba());
		}

		/// <summary>
		/// What the Stage stands at the hit point: a monster's battle model (its family's), its scale, and
		/// where the battle plays an effect on it (monster.chaindata chain 4's hit offset: so far toward the
		/// camera, so far up - btl.TurnSystem.setHitEffectPosition) - or, for -1, the party's first, whose
		/// hit point is 9 toward the camera and 5 up.
		/// </summary>
		public static object Target(Workspace workspace, int monster)
		{
			Cache c = For(workspace);
			if (monster < 0) return new { name = "Luneth", model = Named(c, "j101.nmdp.lz"), scale = 1.0, toward = 9.0, up = 5.0, turn = -90, party = true, at = (double[])null };
			GameTables tables = GameData.Tables(workspace);
			MonsterDefinition m = tables.Monsters.FirstOrDefault(x => x.Id == monster) ?? throw new InvalidOperationException("no monster " + monster);
			double scale = 1, toward = 9, up = 5;
			int turn = 90;
			double[] at = null;
			try
			{
				ChainPack chain = ChainPack.Read(workspace.Read(Named(c, "monster.chaindata")));
				int records = chain.Size(4) / 160;
				for (int i = 0; i < records; i++)
				{
					byte[] r = chain.Record(4, 160, i);
					if (BitConverter.ToInt32(r, 0) != monster) continue;
					toward = BitConverter.ToInt32(r, 4);
					up = BitConverter.ToInt32(r, 8);
					turn = BitConverter.ToInt32(r, 96);
					scale = BitConverter.ToInt32(r, 100) / 4096.0;
					// A place of its own (a boss's: initializePosition @64, raised by its height @92), else a party's place.
					int x = BitConverter.ToInt32(r, 64), y = BitConverter.ToInt32(r, 68), z = BitConverter.ToInt32(r, 72);
					if (x != 0 || y != 0 || z != 0) at = new[] { x / 4096.0, y / 4096.0 + BitConverter.ToInt32(r, 92), z / 4096.0 };
					break;
				}
			}
			catch (Exception) { }
			string model = new[] { m.ModelId, m.Family }.Select(n => Named(c, "f" + n.ToString("000", CultureInfo.InvariantCulture) + ".nmdp.lz")).FirstOrDefault(workspace.Exists);
			return new { name = m.Name, model, scale = scale > 0 ? scale : 1, toward, up, turn, party = false, at };
		}

		/// <summary>A model an effect shows (ModelDS): its geometry as the viewers draw it, and its motion as a pose a frame.</summary>
		public static object Model(Workspace workspace, string pack, string id)
		{
			(ModelTemplate m, EfpPack p) = ModelOf(workspace, pack, id);
			byte[] geometry = m.ModelBytes(p), textures = m.TextureBytes(p), motion = m.MotionBytes(p);
			if (geometry == null) throw new InvalidOperationException("no geometry for model " + id + " in " + pack);
			ModelBundle bundle = Models.ReadData(geometry, m.Material, Models.FormatsOf(textures));
			Models.Pose pose = null;
			string problem = null;
			if (motion != null)
			{
				try
				{
					NcapFile ncap = NcapFile.Read(motion);
					if (ncap.Motions.Count > 0) pose = Models.PoseFrom(geometry, ncap, motion, 0, pack);
				}
				catch (Exception ex) { problem = "its motion: " + ex.Message; }
			}
			return new { model = bundle, pose, loop = m.Loop, scale = m.Scale.Select(v => v / 4096.0).ToArray(), problem };
		}

		public static byte[] ModelTexturePng(Workspace workspace, string pack, string id, string texture)
		{
			(ModelTemplate m, EfpPack p) = ModelOf(workspace, pack, id);
			return Models.TexturePng(m.TextureBytes(p), texture) ?? throw new InvalidOperationException("no texture " + texture + " for model " + id);
		}

		private static (ModelTemplate, EfpPack) ModelOf(Workspace workspace, string pack, string id)
		{
			Cache c = For(workspace);
			EfpPack p = Pack(workspace, c, pack) ?? throw new InvalidOperationException("not an effect pack: " + pack);
			uint n = uint.Parse((id ?? "").Replace("0x", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			return (p.Template(n) as ModelTemplate ?? throw new InvalidOperationException("no model " + id + " in " + pack), p);
		}

		/// <summary>A category's member as the game finds it: the id effect.efi gives, in the category's own pack, else in whichever pack has it.</summary>
		private static (EffectTemplate, EfpPack)? Resolve(Workspace workspace, Cache c, int category, int member)
		{
			uint id = c.Index.TemplateId(category, member);
			if (id == 0) return null;
			string own = "e" + category.ToString("000", CultureInfo.InvariantCulture) + ".efp";
			EfpPack pack = Pack(workspace, c, own);
			EffectTemplate t = pack?.Template(id);
			if (t != null) return (t, pack);
			lock (c)
			{
				if (c.PackOfId == null)
				{
					c.PackOfId = new Dictionary<uint, string>();
					foreach (WorkspaceEntry entry in workspace.List(".efp"))
					{
						EfpPack p = Pack(workspace, c, entry.Name);
						if (p == null) continue;
						foreach (EffectTemplate x in p.Templates) if (!c.PackOfId.ContainsKey(x.Id)) c.PackOfId[x.Id] = entry.Name;
					}
				}
			}
			if (!c.PackOfId.TryGetValue(id, out string where)) return null;
			pack = Pack(workspace, c, where);
			t = pack?.Template(id);
			return t != null ? (t, pack) : null;
		}

		/// <summary>The spells whose effect record names each category and member (player.chaindata chain 12).</summary>
		private static Dictionary<(int, int), List<string>> Uses(Workspace workspace, Cache c)
		{
			lock (c)
			{
				if (c.Uses != null) return c.Uses;
				Dictionary<(int, int), List<string>> uses = new Dictionary<(int, int), List<string>>();
				try
				{
					GameTables tables = GameData.Tables(workspace);
					ChainPack chain = ChainPack.Read(workspace.Read(Named(c, "player.chaindata")));
					int records = chain.Records(ModSpells.Chain, ModSpells.Stride);
					for (int i = 0; i < records; i++)
					{
						byte[] r = chain.Record(ModSpells.Chain, ModSpells.Stride, i);
						int spell = ChainPack.S16(r, 0), category = ChainPack.S16(r, 10), member = ChainPack.S16(r, 12);
						if (category <= 0) continue;
						string label = tables.Spell(spell)?.Name ?? tables.AbilityName(spell) ?? ("spell " + spell);
						if (!uses.TryGetValue((category, member), out List<string> list)) uses[(category, member)] = list = new List<string>();
						if (!list.Contains(label)) list.Add(label);
					}
				}
				catch (Exception) { }
				c.Uses = uses;
				return uses;
			}
		}

		/// <summary>Each spell's effect as its record names it (player.chaindata's effect table): spell id -> category, member.</summary>
		internal static Dictionary<int, (int, int)> SpellEffects(Workspace workspace)
		{
			Dictionary<int, (int, int)> found = new Dictionary<int, (int, int)>();
			try
			{
				Cache c = For(workspace);
				ChainPack chain = ChainPack.Read(workspace.Read(Named(c, "player.chaindata")));
				int records = chain.Records(ModSpells.Chain, ModSpells.Stride);
				for (int i = 0; i < records; i++)
				{
					byte[] r = chain.Record(ModSpells.Chain, ModSpells.Stride, i);
					int spell = ChainPack.S16(r, 0), category = ChainPack.S16(r, 10), member = ChainPack.S16(r, 12);
					if (category > 0 && !found.ContainsKey(spell)) found[spell] = (category, member);
				}
			}
			catch (Exception) { }
			return found;
		}

		private static string Describe(int category, Dictionary<(int, int), List<string>> uses)
		{
			if (category < 0) return null;
			List<string> names = uses.Where(u => u.Key.Item1 == category).OrderBy(u => u.Key.Item2).SelectMany(u => u.Value).Distinct().ToList();
			if (Known.TryGetValue(category, out string known)) names.Insert(0, known);
			return names.Count > 0 ? string.Join(", ", names) : null;
		}

		private static int Category(string name)
		{
			string stem = System.IO.Path.GetFileNameWithoutExtension(name ?? "");
			return stem.Length == 4 && (stem[0] == 'e' || stem[0] == 'E') && int.TryParse(stem.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : -1;
		}

		private static string Summary(EffectTemplate t)
		{
			switch (t)
			{
				case ParticleTemplate p:
					return p.Childs + " x " + p.Groups + " particle(s), life " + p.GroupLife + (p.Flags.HasFlag(ParticleFlags.Loop) ? ", looping" : "")
						+ (p.Texture != null ? ", " + p.Texture.Name + " " + p.Texture.Width + "x" + p.Texture.Height : "");
				case SequenceTemplate s:
					return s.Commands.Count(x => x.Op == SequenceOp.Boot) + " boot(s), " + s.Paths.Count + " path(s)" + (s.Loop ? ", looping" : "");
				case ModelTemplate m:
					return "model " + m.Material;
				default:
					return null;
			}
		}

		private static string Format(int f) => f switch { 1 => "A3I5", 2 => "4-colour", 3 => "16-colour", 4 => "256-colour", 5 => "4x4", 6 => "A5I3", 7 => "direct", _ => "?" };
	}
}
