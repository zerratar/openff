// Saving, with mods: chunks per owner beside the legacy save.
//
// The legacy game writes its 64 KB save file in slots (an offset and a length). Whenever
// it does, the engine writes one chunk per registered ISaveable for that slot into its
// own store, and records which mods were loaded; whenever the legacy game reads a slot
// back that it once wrote, the chunks come back too. A chunk whose owner is not loaded
// is kept as it is, so disabling a mod does not destroy its data; a chunk's version is
// passed to Load so an owner can migrate what an older build wrote.
//
// The store is one JSON file the host names (Game.Saves.StorePath). Moving the chunks
// into the save file itself, and importing the phone's and Steam's save.bin into engine
// chunks, is the later step described in Docs/OpenFF-Engine.md.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenFF
{
	/// <summary>Something with state worth saving: a service or a behaviour.</summary>
	public interface ISaveable
	{
		/// <summary>The chunk's key in the save; unique per owner. A mod's service uses "modid/TypeName" unless it says otherwise.</summary>
		string ChunkId { get; }

		/// <summary>The version of what Save writes; Load receives the version it reads.</summary>
		int ChunkVersion { get; }

		/// <summary>What to keep: anything System.Text.Json can serialise. Null saves nothing.</summary>
		object Save();

		/// <summary>What was kept, as JSON, with the version it was written under.</summary>
		void Load(int version, JsonElement data);
	}

	public sealed class SaveChunks
	{
		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = true,
			IncludeFields = true,
		};

		private readonly List<ISaveable> _saveables = new List<ISaveable>();
		private JsonObject _store;

		/// <summary>The JSON file the chunks live in; set by the host before the first save.</summary>
		public string StorePath { get; set; }

		public IReadOnlyList<ISaveable> Registered => _saveables;

		public void Register(ISaveable saveable)
		{
			if (saveable != null && !_saveables.Contains(saveable))
			{
				_saveables.Add(saveable);
			}
		}

		public void Unregister(ISaveable saveable)
		{
			_saveables.Remove(saveable);
		}

		/// <summary>The legacy game wrote a slot: write every registered owner's chunk for it.</summary>
		public void WriteSlot(int offset, int length)
		{
			if (string.IsNullOrEmpty(StorePath))
			{
				return;
			}
			JsonObject store = Store();
			JsonObject slots = store["slots"] as JsonObject ?? (JsonObject)(store["slots"] = new JsonObject());
			string key = offset.ToString();
			JsonObject slot = slots[key] as JsonObject ?? (JsonObject)(slots[key] = new JsonObject());
			slot["length"] = length;
			slot["written"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			slot["mods"] = new JsonArray(Game.Mods.Select(m => (JsonNode)new JsonObject { ["id"] = m.Id, ["version"] = m.Version }).ToArray());
			JsonObject chunks = slot["chunks"] as JsonObject ?? (JsonObject)(slot["chunks"] = new JsonObject());
			int written = 0;
			foreach (ISaveable saveable in _saveables.ToArray())
			{
				Game.Guard("save " + saveable.ChunkId, () =>
				{
					object data = saveable.Save();
					if (data == null)
					{
						chunks.Remove(saveable.ChunkId);
						return;
					}
					chunks[saveable.ChunkId] = new JsonObject
					{
						["version"] = saveable.ChunkVersion,
						["data"] = JsonSerializer.SerializeToNode(data, data.GetType(), Json),
					};
					written++;
				});
			}
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(StorePath));
				File.WriteAllText(StorePath, store.ToJsonString(Json));
				Game.Log("save: slot " + offset + " (" + length + " bytes) - " + written + " chunk(s) to " + StorePath);
			}
			catch (Exception ex)
			{
				Game.Warn("save: " + StorePath + " not written: " + ex.Message);
			}
		}

		/// <summary>The legacy game read a slot it once wrote: hand every owner its chunk back. False when the slot is unknown.</summary>
		public bool ReadSlot(int offset, int length)
		{
			if (string.IsNullOrEmpty(StorePath) || !(Store()["slots"] is JsonObject slots) || !(slots[offset.ToString()] is JsonObject slot))
			{
				return false;
			}
			if (slot["length"]?.GetValue<int>() != length)
			{
				return false;
			}
			int loaded = 0;
			if (slot["chunks"] is JsonObject chunks)
			{
				foreach (ISaveable saveable in _saveables.ToArray())
				{
					if (!(chunks[saveable.ChunkId] is JsonObject chunk))
					{
						continue;
					}
					Game.Guard("load " + saveable.ChunkId, () =>
					{
						int version = chunk["version"]?.GetValue<int>() ?? 0;
						JsonElement data = JsonSerializer.Deserialize<JsonElement>(chunk["data"]?.ToJsonString() ?? "null");
						saveable.Load(version, data);
						loaded++;
					});
				}
				// Chunks nobody claims: the mod that wrote them is not loaded. Say so, keep them.
				foreach (KeyValuePair<string, JsonNode> pair in chunks)
				{
					if (!_saveables.Any(s => s.ChunkId == pair.Key))
					{
						Game.Log("load: slot " + offset + " has a chunk from '" + pair.Key + "', which is not loaded; kept");
					}
				}
			}
			Game.Log("load: slot " + offset + " - " + loaded + " chunk(s)");
			return true;
		}

		/// <summary>The mods a slot was written under, or null when the slot is unknown.</summary>
		public IReadOnlyList<(string id, string version)> ModsOf(int offset)
		{
			if (string.IsNullOrEmpty(StorePath) || !(Store()["slots"] is JsonObject slots) || !(slots[offset.ToString()] is JsonObject slot) || !(slot["mods"] is JsonArray mods))
			{
				return null;
			}
			return mods.OfType<JsonObject>().Select(m => (m["id"]?.GetValue<string>(), m["version"]?.GetValue<string>())).ToList();
		}

		private JsonObject Store()
		{
			if (_store != null)
			{
				return _store;
			}
			try
			{
				if (File.Exists(StorePath))
				{
					_store = JsonNode.Parse(File.ReadAllText(StorePath)) as JsonObject;
				}
			}
			catch (Exception ex)
			{
				Game.Warn("save: " + StorePath + " unreadable (" + ex.Message + "), starting a new store");
			}
			return _store ??= new JsonObject();
		}
	}
}
