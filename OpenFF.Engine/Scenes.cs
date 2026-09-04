// Scene files: behaviours attached to a map's objects from the editor, not from code.
//
// Crystal lets a modder pick a map object (a character, an exit, the map itself) and
// attach one of the mod's Behaviour types to it, filling in its public fields - the way
// Unity's inspector does. That is saved as scenes/<map>.json in the mod:
//
//   { "map": "d01_05", "attachments": [
//       { "target": "object:3", "behaviour": "Greeter", "fields": { "Text": "Hello", "Radius": 12 } },
//       { "target": "map", "behaviour": "Welcome" } ] }
//
// When the engine enters that map it makes a GameObject per target in the legacy scene,
// gives it a MapObject component saying what it stands for (and, for a character, the Npc
// handle to move and talk through), and adds the behaviours with their fields set. The
// objects belong to the mod, so a hot reload of its code destroys and remakes them.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace OpenFF
{
	/// <summary>What a scene-file object stands for on the legacy map.</summary>
	public sealed class MapObject : Component
	{
		/// <summary>"object" (a character by its index in the map's cast list), "exit" (a slot), or "map".</summary>
		public string Kind { get; internal set; }
		public int Index { get; internal set; } = -1;
		/// <summary>For a character: the handle to move, turn and talk through; null for exits and the map.</summary>
		public Npc Npc { get; internal set; }
		/// <summary>The map the object is on.</summary>
		public string Map { get; internal set; }
		public override string ToString() => Kind + (Index >= 0 ? ":" + Index : "") + " on " + Map;
	}

	/// <summary>One attachment in a scene file.</summary>
	public sealed class SceneAttachment
	{
		public string Target { get; set; }
		public string Behaviour { get; set; }
		public Dictionary<string, JsonElement> Fields { get; set; }
	}

	public sealed class SceneFile
	{
		public string Map { get; set; }
		public List<SceneAttachment> Attachments { get; set; } = new List<SceneAttachment>();

		public static SceneFile Read(string path)
		{
			JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
			return JsonSerializer.Deserialize<SceneFile>(File.ReadAllText(path), options);
		}
	}

	/// <summary>Applies the mods' scene files to the map the game is on.</summary>
	public static class SceneLoader
	{
		/// <summary>The host's way from a target to a character handle: kind and index in, an Npc (or null) out.</summary>
		public static Func<string, int, Npc> ResolveNpc;

		/// <summary>Every loaded mod's scene file for a map.</summary>
		public static int ApplyAll(string map)
		{
			int made = 0;
			foreach (Modding.LoadedMod mod in Game.Mods.ToArray())
			{
				made += Apply(mod, map);
			}
			return made;
		}

		/// <summary>One mod's scene file for a map; the number of objects made.</summary>
		public static int Apply(Modding.LoadedMod mod, string map)
		{
			if (mod?.Definition?.Scenes == null || string.IsNullOrEmpty(map)) return 0;
			string path = Path.Combine(mod.Definition.Scenes, map + ".json");
			if (!File.Exists(path)) return 0;
			SceneFile file;
			try
			{
				file = SceneFile.Read(path);
			}
			catch (Exception ex)
			{
				Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json does not read: " + ex.Message);
				return 0;
			}
			if (file?.Attachments == null) return 0;
			Scene scene = Game.World.Legacy;
			Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
			int made = 0;
			foreach (SceneAttachment attachment in file.Attachments)
			{
				if (attachment == null || string.IsNullOrEmpty(attachment.Behaviour) || string.IsNullOrEmpty(attachment.Target)) continue;
				if (!mod.BehaviourTypes.TryGetValue(attachment.Behaviour, out Type type))
				{
					Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json names behaviour " + attachment.Behaviour + ", which the mod's code does not have");
					continue;
				}
				string key = attachment.Target.Trim().ToLowerInvariant();
				if (!objects.TryGetValue(key, out GameObject target))
				{
					target = MakeTarget(mod, map, key);
					if (target == null) continue;
					objects[key] = target;
					made++;
				}
				Behaviour behaviour = null;
				Game.Guard("new " + type.Name, () => behaviour = (Behaviour)Activator.CreateInstance(type));
				if (behaviour == null) continue;
				SetFields(behaviour, attachment.Fields, mod.Id);
				// Fields first, then the component: Awake sees them.
				target.AddComponent(behaviour);
			}
			if (made > 0)
			{
				Game.Log("engine: mod " + mod.Id + ": " + made + " object(s) from scenes/" + map + ".json");
			}
			return made;
		}

		private static GameObject MakeTarget(Modding.LoadedMod mod, string map, string key)
		{
			string kind = key;
			int index = -1;
			int colon = key.IndexOf(':');
			if (colon > 0)
			{
				kind = key.Substring(0, colon);
				if (!int.TryParse(key.Substring(colon + 1), out index)) index = -1;
			}
			if (kind != "map" && kind != "object" && kind != "exit")
			{
				Game.Warn("mod " + mod.Id + ": scenes/" + map + ".json has a target '" + key + "' (use map, object:<index> or exit:<slot>)");
				return null;
			}
			GameObject o = Game.World.Legacy.Add(map + "/" + key);
			o.Owner = mod;
			o.Tags.Add("scene");
			o.Tags.Add(kind);
			MapObject link = new MapObject { Kind = kind, Index = index, Map = map };
			if (kind == "object" && index >= 0 && ResolveNpc != null)
			{
				Game.Guard("scene npc " + key, () => link.Npc = ResolveNpc(kind, index));
				if (link.Npc != null)
				{
					o.Transform.Position = link.Npc.Position;
				}
			}
			o.AddComponent(link);
			return o;
		}

		/// <summary>Public fields (and settable properties) by name, from JSON: numbers, booleans, strings, enums, Vector3 ({x,y,z} or [x,y,z]), Color ({r,g,b,a} or "#rrggbb").</summary>
		public static void SetFields(object target, Dictionary<string, JsonElement> fields, string modId)
		{
			if (target == null || fields == null) return;
			Type type = target.GetType();
			foreach (KeyValuePair<string, JsonElement> pair in fields)
			{
				try
				{
					FieldInfo field = type.GetField(pair.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if (field != null && !field.IsInitOnly)
					{
						field.SetValue(target, Convert(pair.Value, field.FieldType));
						continue;
					}
					PropertyInfo property = type.GetProperty(pair.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
					if (property != null && property.CanWrite)
					{
						property.SetValue(target, Convert(pair.Value, property.PropertyType));
						continue;
					}
					Game.Warn("mod " + modId + ": " + type.Name + " has no field " + pair.Key);
				}
				catch (Exception ex)
				{
					Game.Warn("mod " + modId + ": " + type.Name + "." + pair.Key + ": " + ex.Message);
				}
			}
		}

		private static object Convert(JsonElement value, Type type)
		{
			if (type == typeof(string)) return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
			if (type == typeof(bool)) return value.ValueKind == JsonValueKind.True || (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out bool b) && b) || (value.ValueKind == JsonValueKind.Number && value.GetDouble() != 0);
			if (type.IsEnum)
			{
				if (value.ValueKind == JsonValueKind.Number) return Enum.ToObject(type, value.GetInt64());
				return Enum.Parse(type, value.GetString(), ignoreCase: true);
			}
			if (type == typeof(int)) return value.ValueKind == JsonValueKind.String ? int.Parse(value.GetString()) : (int)value.GetDouble();
			if (type == typeof(long)) return value.ValueKind == JsonValueKind.String ? long.Parse(value.GetString()) : (long)value.GetDouble();
			if (type == typeof(float)) return value.ValueKind == JsonValueKind.String ? float.Parse(value.GetString(), System.Globalization.CultureInfo.InvariantCulture) : (float)value.GetDouble();
			if (type == typeof(double)) return value.ValueKind == JsonValueKind.String ? double.Parse(value.GetString(), System.Globalization.CultureInfo.InvariantCulture) : value.GetDouble();
			if (type == typeof(Vector3))
			{
				if (value.ValueKind == JsonValueKind.Array)
				{
					float[] a = value.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
					return new Vector3(a.Length > 0 ? a[0] : 0, a.Length > 1 ? a[1] : 0, a.Length > 2 ? a[2] : 0);
				}
				return new Vector3(Number(value, "x"), Number(value, "y"), Number(value, "z"));
			}
			if (type == typeof(Vector2))
			{
				if (value.ValueKind == JsonValueKind.Array)
				{
					float[] a = value.EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
					return new Vector2(a.Length > 0 ? a[0] : 0, a.Length > 1 ? a[1] : 0);
				}
				return new Vector2(Number(value, "x"), Number(value, "y"));
			}
			if (type == typeof(Color))
			{
				if (value.ValueKind == JsonValueKind.String)
				{
					string s = value.GetString().TrimStart('#');
					if (s.Length >= 6)
					{
						byte r = System.Convert.ToByte(s.Substring(0, 2), 16), g = System.Convert.ToByte(s.Substring(2, 2), 16), bl = System.Convert.ToByte(s.Substring(4, 2), 16);
						byte a = s.Length >= 8 ? System.Convert.ToByte(s.Substring(6, 2), 16) : (byte)255;
						return new Color(r, g, bl, a);
					}
				}
				return new Color((byte)Number(value, "r"), (byte)Number(value, "g"), (byte)Number(value, "b"), value.TryGetProperty("a", out JsonElement alpha) ? (byte)alpha.GetDouble() : (byte)255);
			}
			if (type == typeof(string[]) || type == typeof(List<string>))
			{
				List<string> list = value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(e => e.ToString()).ToList() : new List<string> { value.ToString() };
				return type == typeof(string[]) ? (object)list.ToArray() : list;
			}
			return JsonSerializer.Deserialize(value.GetRawText(), type);
		}

		private static float Number(JsonElement o, string name)
		{
			return o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out JsonElement e) && e.ValueKind == JsonValueKind.Number ? (float)e.GetDouble() : 0f;
		}
	}
}
