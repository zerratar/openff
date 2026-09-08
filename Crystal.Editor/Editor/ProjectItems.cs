// A project's item definitions: defs/items/<id>.json, the mod's own items (Shared/Data/
// ModItems.cs says what one is and how the client composes them into the game's tables).
// The editor lists them, shows one beside the record it starts from - the base item's
// fields by the game's names, so a definition changes only what it names - and writes it
// back; a new one gets the next free number, which is then its id in the game for good.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenFF.Data;

namespace Crystal.Editor
{
	internal static class ProjectItems
	{
		public static string Directory(Project project) => Path.Combine(project.Directory, "defs", "items");

		/// <summary>Every definition in the project, in file order.</summary>
		public static List<ModItem> All(Project project, List<string> notes = null)
		{
			return project == null ? new List<ModItem>() : ModItems.Load(new[] { project.Directory }, notes);
		}

		/// <summary>The base item's record and chain for a definition, from the workspace's item_parameter.pak; null when the id is no item.</summary>
		public static byte[] BaseRecord(Workspace workspace, int baseId, out int chain)
		{
			chain = -1;
			byte[] pak = workspace?.Read("files/item_parameter.pak");
			if (pak == null) return null;
			try { return ModItems.RecordOf(ChainPack.Read(pak), baseId, out chain); }
			catch (Exception) { return null; }
		}

		/// <summary>What the editor shows for one definition: the definition, its base's name and chain, and every field the chain has with the base's value.</summary>
		public static object Describe(Workspace workspace, ModItem item, Func<uint, string> lookup)
		{
			byte[] record = BaseRecord(workspace, item.Base, out int chain);
			List<object> fields = new List<object>();
			if (record != null)
			{
				foreach (string name in ModItems.FieldNames(chain))
				{
					int? value = ModItems.Get(record, chain, name);
					fields.Add(new { name, baseValue = value, value = item.Fields.TryGetValue(name, out int set) ? (int?)set : null });
				}
			}
			string baseName = record != null ? Text(record, 4, lookup) : null;
			string baseCaption = record != null ? Text(record, 6, lookup) : null;
			return new
			{
				id = item.Id, number = item.Number, @base = item.Base, baseName, baseCaption,
				chain = chain >= 0 ? ModItems.ChainNames[chain] : null,
				name = item.Name, caption = item.Caption, buy = item.Buy, sell = item.Sell,
				baseBuy = record != null && chain != 4 ? ModItems.Get(record, chain, "buy") : null,
				baseSell = record != null && chain != 4 ? ModItems.Get(record, chain, "price") : null,
				fields,
				file = "defs/items/" + item.Id + ".json"
			};
		}

		private static string Text(byte[] record, int offset, Func<uint, string> lookup)
		{
			int id = ChainPack.S16(record, offset);
			if (id <= 0 || lookup == null) return null;
			try { return lookup((uint)id); } catch (Exception) { return null; }
		}

		/// <summary>Writes a definition; the file is named by its id.</summary>
		public static string Save(Project project, ModItem item)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			if (string.IsNullOrWhiteSpace(item.Id)) throw new ArgumentException("an item needs an id");
			if (item.Number <= 0) throw new ArgumentException("an item needs a number");
			if (item.Base <= 0) throw new ArgumentException("an item needs a base item");
			string directory = Directory(project);
			System.IO.Directory.CreateDirectory(directory);
			string path = Path.Combine(directory, item.Id + ".json");
			File.WriteAllText(path, item.ToJson(), new UTF8Encoding(false));
			return path;
		}

		/// <summary>A new definition from a name and a base: the id from the name, the next number free.</summary>
		public static ModItem New(Project project, string name, int baseId)
		{
			if (project == null) throw new InvalidOperationException("no project is open");
			List<ModItem> all = All(project);
			string id = Slug(name);
			if (string.IsNullOrEmpty(id)) id = "item";
			string unique = id;
			for (int n = 2; all.Any(i => string.Equals(i.Id, unique, StringComparison.OrdinalIgnoreCase)) || File.Exists(Path.Combine(Directory(project), unique + ".json")); n++) unique = id + "-" + n;
			ModItem item = new ModItem { Id = unique, Number = ModItems.NextNumber(all), Base = baseId, Name = name?.Trim() ?? "", Caption = "" };
			Save(project, item);
			return item;
		}

		public static bool Delete(Project project, string id)
		{
			if (project == null || string.IsNullOrWhiteSpace(id) || id.IndexOfAny(new[] { '/', '\\', '.' }) >= 0) return false;
			string path = Path.Combine(Directory(project), id + ".json");
			if (!File.Exists(path)) return false;
			File.Delete(path);
			return true;
		}

		/// <summary>
		/// The Steam side: a native mod has no client to compose the tables as they are read, so
		/// the two files are composed here - the shipped item_parameter.pak and eureka_item.msd
		/// with the project's definitions appended - and written into the target's files/, where
		/// Install copies them into the game and the zip carries them. Run before either. With no
		/// definitions the two files are left as the project has them (an edit of the tables by
		/// hand stays); returns what was written.
		/// </summary>
		public static List<string> WriteTables(Project project, Workspace workspace, string target)
		{
			List<string> written = new List<string>();
			if (project == null || workspace == null || !string.Equals(Targets.GameOf(target), "ff3", StringComparison.OrdinalIgnoreCase)) return written;
			List<ModItem> items = All(project);
			Dictionary<uint, string> text = ProjectText.Lines(project);
			if (items.Count == 0 && text.Count == 0) return written;
			string files = project.FilesFor(target);
			foreach ((string name, Func<byte[], byte[]> compose) in new (string, Func<byte[], byte[]>)[]
			{
				("files/item_parameter.pak", data => items.Count > 0 ? ModItems.ComposePak(data, items) : data),
				("files/eureka_item.msd", data => items.Count > 0 ? ModItems.ComposeMsd(data, items) : data),
				// The mod's own lines, into the file every map falls back to.
				("files/eureka_permanent.msd", data => ModText.Compose(data, text)),
			})
			{
				// From the shipped file, not the project's copy: the definitions are the source, and
				// composing over an earlier composition would only skip what is there already.
				byte[] shipped = workspace.ReadShipped(name);
				if (shipped == null) continue;
				byte[] composed = compose(shipped);
				if (composed == null || ReferenceEquals(composed, shipped)) continue;
				string path = Path.Combine(files, name.Replace('/', Path.DirectorySeparatorChar));
				System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, composed);
				written.Add(name);
			}
			return written;
		}

		/// <summary>"Hi-Potion+" -> "hi-potion-plus": a file name and a word a mod can use.</summary>
		public static string Slug(string name)
		{
			string s = (name ?? "").Trim().ToLowerInvariant().Replace("+", "-plus").Replace("&", "-and-");
			s = Regex.Replace(s, @"[^a-z0-9]+", "-").Trim('-');
			return s;
		}
	}
}
