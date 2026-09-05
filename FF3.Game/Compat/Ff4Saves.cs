// FF4 saving on the unified layer.
//
// FF4 kept its saves through the phone port's own save part, which did not come across;
// the OpenFF engine's save chunks (OpenFF.Engine/Saving.cs) hold FF4's game instead. A slot
// is one entry in saves/ff4.json: the party chunk (Ff4PartyService - roster, line-up, bag,
// gil) and the field chunk here (map, position, facing, the script flags, and a summary
// line for the menu). The menu's Save page writes a slot anywhere for now (FF4 allows it
// on the overworld and at save points; the points' flag is not read yet); its Load page
// and --load=<slot> at boot read one back: the party is rebuilt, the flags restored, and
// the party lands on the saved map through the jump part (at boot) or a map jump (in
// play). A mod's ISaveable services ride along in the same slot, as they do for FF3.

using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenFF;
using OpenFF.Data;

namespace FF3
{
	/// <summary>Where the party stands and what the scripts have done: the field chunk of an FF4 save.</summary>
	internal sealed class Ff4FieldState : ISaveable
	{
		public sealed class Data
		{
			public string Map;
			public float X, Y, Z;
			/// <summary>The leader's rotation about Y in the engine's 16-bit units (8192 per eighth of a turn).</summary>
			public int Rotation;
			/// <summary>Every set script flag as "group:index".</summary>
			public List<string> Flags = new List<string>();
			/// <summary>One line for the menu's slot list.</summary>
			public string Summary;
		}

		public string ChunkId => "ff4/field";
		public int ChunkVersion => 1;

		public object Save()
		{
			if (!EngineApi.InWorld || !Game.Hero.Present) return null;
			Data d = new Data { Map = Game.Field.Map };
			Vector3 at = Game.Hero.Position;
			d.X = at.X; d.Y = at.Y; d.Z = at.Z;
			try { d.Rotation = EngineApi.HeroPlayer.getRotation().y; } catch (Exception) { }
			byte[,] flags = GlobalScope.flags;
			if (flags != null)
			{
				for (int g = 0; g < flags.GetLength(0); g++)
				{
					for (int i = 0; i < flags.GetLength(1); i++)
					{
						if (flags[g, i] != 0) d.Flags.Add(g + ":" + i);
					}
				}
			}
			Character leader = Ff4Party.Party.Leader;
			d.Summary = (leader != null ? leader.Name + " L" + leader.Level : "nobody") + " - " + (d.Map ?? "?") + ", " + Ff4Party.Party.Gil + " gil";
			Log.Write(LogChannel.General, "save: field - " + d.Map + " at " + d.X.ToString("0") + "," + d.Y.ToString("0") + "," + d.Z.ToString("0") + " rot " + d.Rotation + ", " + d.Flags.Count + " flag(s)");
			return d;
		}

		public void Load(int version, JsonElement data)
		{
			Data d = JsonSerializer.Deserialize<Data>(data.GetRawText(), Ff4Saves.Json);
			if (d == null) return;
			byte[,] flags = GlobalScope.flags;
			if (flags != null)
			{
				Array.Clear(flags, 0, flags.Length);
				foreach (string f in d.Flags)
				{
					int colon = f.IndexOf(':');
					if (colon > 0 && int.TryParse(f.Substring(0, colon), out int g) && int.TryParse(f.Substring(colon + 1), out int i)
						&& g >= 0 && g < flags.GetLength(0) && i >= 0 && i < flags.GetLength(1))
					{
						flags[g, i] = 1;
					}
				}
			}
			Ff4Saves.Pending = d;
			Log.Write(LogChannel.General, "load: field - " + (d.Map ?? "?") + " at " + d.X.ToString("0") + "," + d.Y.ToString("0") + "," + d.Z.ToString("0") + ", " + d.Flags.Count + " flag(s)");
		}
	}

	internal static class Ff4Saves
	{
		public static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };

		public const int SlotCount = 3;

		/// <summary>The field state a load brought back, until the party has landed there (the jump part reads it at boot).</summary>
		public static Ff4FieldState.Data Pending;

		/// <summary>A line the menu shows for a moment after saving.</summary>
		public static string Notice;
		public static int NoticeFrames;

		public static bool Exists(int slot) => Game.Saves.WrittenAt(slot) != null;

		/// <summary>What a slot holds, for the menu: the field chunk's summary and when it was written.</summary>
		public static string Describe(int slot)
		{
			string when = Game.Saves.WrittenAt(slot);
			if (when == null) return "- empty -";
			JsonElement? field = Game.Saves.Peek(slot, "ff4/field");
			string summary = field.HasValue && field.Value.ValueKind == JsonValueKind.Object && field.Value.TryGetProperty("Summary", out JsonElement s) ? s.GetString() : null;
			return (summary ?? "a saved game") + "   " + when;
		}

		public static bool Save(int slot)
		{
			if (!EngineApi.InWorld || slot < 1 || slot > SlotCount) return false;
			Game.Saves.WriteSlot(slot, 0);
			Log.Write(LogChannel.General, "save: FF4 slot " + slot + " - " + Describe(slot));
			return true;
		}

		/// <summary>Reads a slot back. At boot the jump part lands the party; in play a map jump does.</summary>
		public static bool Load(int slot, bool atBoot)
		{
			Pending = null;
			if (slot < 1 || slot > SlotCount || !Game.Saves.ReadSlot(slot, 0))
			{
				Log.Write(LogChannel.General, "load: FF4 slot " + slot + " is empty");
				return false;
			}
			if (Pending == null)
			{
				Log.Write(LogChannel.General, "load: FF4 slot " + slot + " has no field chunk; the party alone came back");
				return false;
			}
			Log.Write(LogChannel.General, "load: FF4 slot " + slot + " - " + Describe(slot));
			if (!atBoot)
			{
				Ff4FieldState.Data d = Pending;
				Pending = null;
				GlobalScope.VecFx32 fx = new GlobalScope.VecFx32((int)Math.Round(d.X * 4096), (int)Math.Round(d.Y * 4096), (int)Math.Round(d.Z * 4096));
				int facing = (((int)Math.Round(d.Rotation / 8192.0)) % 8 + 8) % 8;
				Game.Field.Warp(JumpPart.WithChip(d.Map, fx), new Vector3(d.X, d.Y, d.Z), facing);
			}
			return true;
		}

		/// <summary>Once the party has landed after a load, say where; the pending state is done with then.</summary>
		public static void Tick()
		{
			if (Pending == null || !EngineApi.InWorld || !Game.Hero.Present) return;
			Vector3 at = Game.Hero.Position;
			Log.Write(LogChannel.General, "load: landed on " + (Game.Field.Map ?? "?") + " at " + at.X.ToString("0") + "," + at.Y.ToString("0") + "," + at.Z.ToString("0")
				+ " (saved " + Pending.X.ToString("0") + "," + Pending.Y.ToString("0") + "," + Pending.Z.ToString("0") + ")");
			Pending = null;
		}

		/// <summary>--load=&lt;slot&gt;: an FF4 game starts from a slot instead of the new game.</summary>
		public static void LoadAtBoot()
		{
			string arg = Options.Get("load");
			if (!GameProfile.IsFf4 || string.IsNullOrEmpty(arg) || !int.TryParse(arg, out int slot)) return;
			if (Load(slot, true)) Log.Write(LogChannel.General, "load: starting from slot " + slot + " on " + Pending.Map);
		}
	}
}
