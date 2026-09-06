// The OpenFF engine's API as a reference page: read from the engine's own XML documentation
// (OpenFF.Engine.xml beside the client's OpenFF.Engine.dll), grouped by type, so a modder
// can look up Game.Hero.MoveTo without leaving Crystal.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Crystal.Editor
{
	internal sealed class ApiMember
	{
		/// <summary>method, property, field, event.</summary>
		public string Kind { get; set; }
		public string Name { get; set; }
		public string Signature { get; set; }
		public string Summary { get; set; }
	}

	internal sealed class ApiType
	{
		public string Name { get; set; }
		public string FullName { get; set; }
		public string Summary { get; set; }
		public List<ApiMember> Members { get; set; } = new List<ApiMember>();
	}

	internal static class ApiReference
	{
		private static string _cacheKey;
		private static List<ApiType> _cache;

		/// <summary>The engine's documented types, or null when no engine (or no XML) is found.</summary>
		public static List<ApiType> Read(out string source)
		{
			source = null;
			string engine = OpenFFClient.EngineAssembly();
			if (engine == null) return null;
			string xml = Path.ChangeExtension(engine, ".xml");
			if (!File.Exists(xml)) return null;
			source = xml;
			string key = xml + "@" + File.GetLastWriteTimeUtc(xml).Ticks;
			if (key == _cacheKey && _cache != null) return _cache;

			Dictionary<string, ApiType> types = new Dictionary<string, ApiType>(StringComparer.Ordinal);
			List<(string owner, ApiMember member)> pending = new List<(string, ApiMember)>();
			XDocument document = XDocument.Load(xml);
			foreach (XElement element in document.Descendants("member"))
			{
				string name = element.Attribute("name")?.Value;
				if (string.IsNullOrEmpty(name) || name.Length < 3) continue;
				string summary = Text(element.Element("summary"));
				char kind = name[0];
				string body = name.Substring(2);
				if (kind == 'T')
				{
					types[body] = new ApiType { FullName = body, Name = Short(body), Summary = summary };
					continue;
				}
				string owner, memberName, signature = null;
				int paren = body.IndexOf('(');
				string head = paren >= 0 ? body.Substring(0, paren) : body;
				int dot = head.LastIndexOf('.');
				if (dot < 0) continue;
				owner = head.Substring(0, dot);
				memberName = head.Substring(dot + 1);
				if (paren >= 0)
				{
					signature = memberName + "(" + Tidy(body.Substring(paren + 1).TrimEnd(')')) + ")";
				}
				string kindName = kind == 'M' ? "method" : kind == 'P' ? "property" : kind == 'F' ? "field" : kind == 'E' ? "event" : "member";
				if (kindName == "method" && memberName == "#ctor") { memberName = "new"; signature = "new " + Short(owner) + signature.Substring(5); }
				pending.Add((owner, new ApiMember { Kind = kindName, Name = memberName, Signature = signature ?? memberName, Summary = summary }));
			}
			foreach ((string owner, ApiMember member) in pending)
			{
				if (!types.TryGetValue(owner, out ApiType type))
				{
					type = new ApiType { FullName = owner, Name = Short(owner) };
					types[owner] = type;
				}
				type.Members.Add(member);
			}
			List<ApiType> list = types.Values
				.Where(t => t.Members.Count > 0 || !string.IsNullOrEmpty(t.Summary))
				.OrderBy(t => Rank(t.Name)).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
			foreach (ApiType type in list)
			{
				type.Members.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
			}
			_cacheKey = key;
			_cache = list;
			return list;
		}

		/// <summary>Game and its facades first, then the object model, then the rest.</summary>
		private static int Rank(string name)
		{
			if (name == "Game") return 0;
			if (name.StartsWith("I", StringComparison.Ordinal) && name.Length > 1 && char.IsUpper(name[1])) return 1;
			if (name == "Behaviour" || name == "GameService" || name == "GameObject" || name == "Scene" || name == "Component" || name == "Transform" || name == "World") return 2;
			if (name == "Npc" || name == "Spell" || name == "Monster" || name == "PartyMember" || name == "Stats") return 3;
			if (name.StartsWith("Events.", StringComparison.Ordinal)) return 5;
			return 4;
		}

		private static string Short(string fullName)
		{
			string s = fullName.StartsWith("OpenFF.", StringComparison.Ordinal) ? fullName.Substring(7) : fullName;
			return s.Replace('+', '.');
		}

		private static string Tidy(string parameters)
		{
			string s = Regex.Replace(parameters, @"\bSystem\.Collections\.Generic\.", "");
			s = Regex.Replace(s, @"\bSystem\.(String|Int32|Single|Boolean|Double|Object|UInt32|Int64|Byte)\b", m => m.Groups[1].Value switch
			{
				"String" => "string", "Int32" => "int", "Single" => "float", "Boolean" => "bool", "Double" => "double",
				"Object" => "object", "UInt32" => "uint", "Int64" => "long", "Byte" => "byte", _ => m.Value
			});
			s = s.Replace("System.", "").Replace("OpenFF.Events.", "").Replace("OpenFF.Modding.", "").Replace("OpenFF.", "");
			s = s.Replace("{", "<").Replace("}", ">").Replace("`1", "").Replace("@", " ref");
			return s.Replace(",", ", ");
		}

		private static string Text(XElement element)
		{
			if (element == null) return null;
			string raw = string.Concat(element.Nodes().Select(n => n is XElement e ? (e.Attribute("cref")?.Value?.Split('.').Last() ?? e.Value) : n.ToString()));
			string text = string.Join(" ", raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim())).Trim();
			return text.Length > 0 ? text : null;
		}
	}
}
