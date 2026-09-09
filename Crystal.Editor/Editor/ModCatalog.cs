// What a project's built code offers the editor: its Behaviour and GameService types,
// with the public fields an inspector can edit and their defaults.
//
// The assemblies are read in a collectible load context from their bytes, so the files
// stay free for the next build, and the engine assembly they reference is loaded the same
// way from the client's copy. Defaults come from making an instance (a behaviour's
// constructor and field initialisers; nothing else runs), so a field the code sets to 12
// shows 12 in the inspector. Summaries come from the code's XML documentation file when
// the csproj produces one (Crystal's does).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace Crystal.Editor
{
	internal sealed class CatalogField
	{
		public string Name { get; set; }
		/// <summary>int, float, bool, string, enum, vector3, vector2, color, strings (a list), item (an item id), or other.</summary>
		public string Type { get; set; }
		public string TypeName { get; set; }
		public object Default { get; set; }
		public List<string> Options { get; set; }
		public string Summary { get; set; }
		/// <summary>[Header]: a heading above this field.</summary>
		public string Header { get; set; }
		/// <summary>[Tooltip], else the XML summary.</summary>
		public string Tooltip { get; set; }
		/// <summary>[Range]: a slider between the two.</summary>
		public float? Min { get; set; }
		public float? Max { get; set; }
	}

	internal sealed class CatalogType
	{
		public string Name { get; set; }
		public string FullName { get; set; }
		/// <summary>behaviour or service.</summary>
		public string Kind { get; set; }
		public string Summary { get; set; }
		public string Assembly { get; set; }
		public List<CatalogField> Fields { get; set; } = new List<CatalogField>();
	}

	internal sealed class ModCatalogResult
	{
		public List<CatalogType> Behaviours { get; set; } = new List<CatalogType>();
		public List<CatalogType> Services { get; set; } = new List<CatalogType>();
		public List<string> Assemblies { get; set; } = new List<string>();
		public List<string> Problems { get; set; } = new List<string>();
	}

	internal static class ModCatalog
	{
		private static string _cacheKey;
		private static ModCatalogResult _cache;

		public static ModCatalogResult Read(Project project)
		{
			List<string> assemblies = ModCode.Assemblies(project);
			string engine = OpenFFClient.EngineAssembly();
			string key = string.Join("|", assemblies.Select(a => a + "@" + File.GetLastWriteTimeUtc(a).Ticks))
				+ "|" + engine + "@" + (engine != null && File.Exists(engine) ? File.GetLastWriteTimeUtc(engine).Ticks : 0);
			if (key == _cacheKey && _cache != null)
			{
				return _cache;
			}
			ModCatalogResult result = new ModCatalogResult { Assemblies = assemblies.Select(Path.GetFileName).ToList() };
			CatalogContext context = new CatalogContext(engine);
			try
			{
				// The engine's own behaviours (Trigger) first: a scene may place them whether
				// or not the mod has code of its own, so they are always on offer.
				if (engine != null && File.Exists(engine))
				{
					try
					{
						Assembly engineAssembly = context.LoadFromAssemblyName(new AssemblyName("OpenFF.Engine"));
						Dictionary<string, string> engineDocs = ReadDocs(Path.ChangeExtension(engine, ".xml"));
						foreach (Type type in engineAssembly.GetTypes())
						{
							if (type.IsAbstract || type.IsGenericTypeDefinition || !type.IsPublic) continue;
							if (!Derives(type, "OpenFF.Behaviour") || type.GetConstructor(Type.EmptyTypes) == null) continue;
							result.Behaviours.Add(Describe(type, "behaviour", "OpenFF.Engine.dll", engineDocs, result.Problems));
						}
					}
					catch (Exception ex)
					{
						result.Problems.Add("OpenFF.Engine.dll: " + ex.Message);
					}
				}
				foreach (string path in assemblies)
				{
					Assembly assembly;
					try
					{
						using FileStream stream = File.OpenRead(path);
						string pdb = Path.ChangeExtension(path, ".pdb");
						assembly = File.Exists(pdb) ? context.LoadFromStream(stream, File.OpenRead(pdb)) : context.LoadFromStream(stream);
					}
					catch (Exception ex)
					{
						result.Problems.Add(Path.GetFileName(path) + ": " + ex.Message);
						continue;
					}
					Dictionary<string, string> docs = ReadDocs(Path.ChangeExtension(path, ".xml"));
					Type[] types;
					try
					{
						types = assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						types = ex.Types.Where(t => t != null).ToArray();
						foreach (Exception inner in ex.LoaderExceptions.Where(e => e != null).Take(3))
						{
							result.Problems.Add(Path.GetFileName(path) + ": " + inner.Message);
						}
					}
					foreach (Type type in types)
					{
						if (type.IsAbstract || type.IsGenericTypeDefinition || !type.IsPublic && !type.IsNestedPublic) continue;
						string kind = Derives(type, "OpenFF.Behaviour") ? "behaviour" : Derives(type, "OpenFF.GameService") ? "service" : null;
						if (kind == null) continue;
						CatalogType entry = Describe(type, kind, Path.GetFileName(path), docs, result.Problems);
						(kind == "behaviour" ? result.Behaviours : result.Services).Add(entry);
					}
				}
			}
			finally
			{
				try { context.Unload(); } catch (Exception) { }
			}
			result.Behaviours.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
			result.Services.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
			_cacheKey = key;
			_cache = result;
			return result;
		}

		private static bool Derives(Type type, string baseFullName)
		{
			for (Type t = type.BaseType; t != null; t = t.BaseType)
			{
				if (t.FullName == baseFullName) return true;
			}
			return false;
		}

		private static CatalogType Describe(Type type, string kind, string assembly, Dictionary<string, string> docs, List<string> problems)
		{
			CatalogType entry = new CatalogType { Name = type.Name, FullName = type.FullName, Kind = kind, Assembly = assembly };
			docs.TryGetValue("T:" + type.FullName, out string summary);
			entry.Summary = summary;
			object instance = null;
			if (type.GetConstructor(Type.EmptyTypes) != null)
			{
				try { instance = Activator.CreateInstance(type); }
				catch (Exception ex) { problems.Add(type.Name + ": its constructor threw (" + (ex.InnerException ?? ex).Message + "); defaults are blank"); }
			}
			// Fields in declaration order, base class first: a derived component's own fields
			// come after the ones it inherits, as Unity shows them.
			foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => Depth(f.DeclaringType)).ThenBy(f => f.MetadataToken))
			{
				if (field.IsInitOnly || Has(field, "OpenFF.HideInInspectorAttribute")) continue;
				string fieldType = Classify(field.FieldType);
				if (fieldType == null) continue;
				CatalogField f = new CatalogField { Name = field.Name, Type = fieldType, TypeName = field.FieldType.Name };
				docs.TryGetValue("F:" + field.DeclaringType.FullName + "." + field.Name, out string fieldSummary);
				f.Summary = fieldSummary;
				Decorate(f, field);
				if (field.FieldType.IsEnum)
				{
					f.Options = Enum.GetNames(field.FieldType).ToList();
				}
				if (instance != null)
				{
					try { f.Default = Plain(field.GetValue(instance)); } catch (Exception) { }
				}
				entry.Fields.Add(f);
			}
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (!property.CanWrite || !property.CanRead || property.GetIndexParameters().Length > 0) continue;
				if (property.GetSetMethod(false) == null || Has(property, "OpenFF.HideInInspectorAttribute")) continue;
				string propertyType = Classify(property.PropertyType);
				if (propertyType == null) continue;
				CatalogField f = new CatalogField { Name = property.Name, Type = propertyType, TypeName = property.PropertyType.Name };
				docs.TryGetValue("P:" + type.FullName + "." + property.Name, out string propertySummary);
				f.Summary = propertySummary;
				Decorate(f, property);
				if (property.PropertyType.IsEnum)
				{
					f.Options = Enum.GetNames(property.PropertyType).ToList();
				}
				if (instance != null)
				{
					try { f.Default = Plain(property.GetValue(instance)); } catch (Exception) { }
				}
				entry.Fields.Add(f);
			}
			return entry;
		}

		private static int Depth(Type type)
		{
			int depth = 0;
			for (Type t = type; t != null; t = t.BaseType) depth++;
			return depth;
		}

		/// <summary>Whether a member carries an attribute, by the attribute type's full name (the types live in another load context).</summary>
		private static bool Has(MemberInfo member, string attribute)
		{
			return member.GetCustomAttributesData().Any(a => a.AttributeType.FullName == attribute);
		}

		/// <summary>[Header], [Tooltip], [Range], [ItemField] onto the field's description.</summary>
		private static void Decorate(CatalogField f, MemberInfo member)
		{
			foreach (CustomAttributeData a in member.GetCustomAttributesData())
			{
				switch (a.AttributeType.FullName)
				{
					case "OpenFF.HeaderAttribute":
						f.Header = a.ConstructorArguments.Count > 0 ? a.ConstructorArguments[0].Value as string : null;
						break;
					case "OpenFF.TooltipAttribute":
						f.Tooltip = a.ConstructorArguments.Count > 0 ? a.ConstructorArguments[0].Value as string : null;
						break;
					case "OpenFF.RangeAttribute":
						if (a.ConstructorArguments.Count >= 2)
						{
							f.Min = System.Convert.ToSingle(a.ConstructorArguments[0].Value);
							f.Max = System.Convert.ToSingle(a.ConstructorArguments[1].Value);
						}
						break;
					case "OpenFF.ItemFieldAttribute":
						if (f.Type == "int") f.Type = "item";
						break;
					case "OpenFF.FlagFieldAttribute":
						if (f.Type == "string") f.Type = "flags";
						break;
					case "OpenFF.FormationFieldAttribute":
						if (f.Type == "int") f.Type = "formation";
						break;
					case "OpenFF.MapFieldAttribute":
						if (f.Type == "string") f.Type = "map";
						break;
					case "OpenFF.BgmFieldAttribute":
						if (f.Type == "int") f.Type = "bgm";
						break;
				}
			}
			if (f.Tooltip == null) f.Tooltip = f.Summary;
		}

		private static string Classify(Type type)
		{
			if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)) return "int";
			if (type == typeof(float) || type == typeof(double)) return "float";
			if (type == typeof(bool)) return "bool";
			if (type == typeof(string)) return "string";
			if (type == typeof(string[]) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && type.GetGenericArguments()[0] == typeof(string))) return "strings";
			if (type.IsEnum) return "enum";
			switch (type.FullName)
			{
				case "OpenFF.Vector3": return "vector3";
				case "OpenFF.Vector2": return "vector2";
				case "OpenFF.Color": return "color";
				case "OpenFF.ObjectRef": return "object";
				case "OpenFF.Timeline": return "timeline";
			}
			return null;
		}

		/// <summary>A value as JSON-friendly data: numbers and strings as they are, enums by name, vectors and colours as objects.</summary>
		private static object Plain(object value)
		{
			if (value == null) return null;
			Type type = value.GetType();
			if (type.IsEnum) return value.ToString();
			if (type.IsPrimitive || value is string) return value;
			if (value is string[] strings) return strings;
			if (value is List<string> list) return list.ToArray();
			switch (type.FullName)
			{
				case "OpenFF.Vector3":
					return new { x = Field(value, "X"), y = Field(value, "Y"), z = Field(value, "Z") };
				case "OpenFF.Vector2":
					return new { x = Field(value, "X"), y = Field(value, "Y") };
				case "OpenFF.Color":
					return new { r = Field(value, "R"), g = Field(value, "G"), b = Field(value, "B"), a = Field(value, "A") };
				case "OpenFF.ObjectRef":
					return type.GetProperty("Path")?.GetValue(value) as string ?? "";
				case "OpenFF.Timeline":
					return new { tracks = new object[0] };
			}
			return value.ToString();
		}

		private static object Field(object value, string name)
		{
			FieldInfo f = value.GetType().GetField(name);
			return f != null ? f.GetValue(value) : null;
		}

		/// <summary>member name -> summary text, from a compiler XML documentation file; empty when there is none.</summary>
		public static Dictionary<string, string> ReadDocs(string xmlPath)
		{
			Dictionary<string, string> docs = new Dictionary<string, string>(StringComparer.Ordinal);
			if (!File.Exists(xmlPath)) return docs;
			try
			{
				XDocument document = XDocument.Load(xmlPath);
				foreach (XElement member in document.Descendants("member"))
				{
					string name = member.Attribute("name")?.Value;
					XElement summary = member.Element("summary");
					if (name == null || summary == null) continue;
					string text = string.Join(" ", summary.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim())).Trim();
					if (text.Length > 0) docs[name] = text;
				}
			}
			catch (Exception)
			{
			}
			return docs;
		}

		private sealed class CatalogContext : AssemblyLoadContext
		{
			private readonly string _engine;

			public CatalogContext(string engine) : base("crystal-catalog", isCollectible: true)
			{
				_engine = engine;
			}

			protected override Assembly Load(AssemblyName name)
			{
				if (string.Equals(name.Name, "OpenFF.Engine", StringComparison.OrdinalIgnoreCase) && _engine != null && File.Exists(_engine))
				{
					using FileStream stream = File.OpenRead(_engine);
					return LoadFromStream(stream);
				}
				return null;
			}
		}
	}
}
