// The editor's back end: a small local HTTP server over the codecs.
//
//   ff3content editor [--content=<dir>] [--override=<dir>] [--port=5050]
//
// It binds to localhost only, has no authentication, and is meant to be run by the
// person editing their own copy of the game. It is not a service.
//
// The browser does the editing; this side only decodes, compiles and saves. Which
// means the rules that matter - what compiles, what a menu's coordinates mean - stay
// in one place and are the same ones the command line uses.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Xml.Linq;

namespace FF3.ContentTool.Editor
{
	internal sealed class EditorServer
	{
		private static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = false,
			// The browser side reads camelCase, so the wire format says camelCase.
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		private readonly Workspace _workspace;
		private readonly string _webRoot;
		private readonly Func<uint, string> _lookupMessage;

		public EditorServer(Workspace workspace, string webRoot, Func<uint, string> lookupMessage)
		{
			_workspace = workspace;
			_webRoot = webRoot;
			_lookupMessage = lookupMessage;
		}

		public void Run(int port)
		{
			using HttpListener listener = new HttpListener();
			listener.Prefixes.Add(string.Format(CultureInfo.InvariantCulture,
				"http://localhost:{0}/", port));
			listener.Start();

			Console.WriteLine("FF3 content editor");
			Console.WriteLine("  content   {0} files", _workspace.FileCount);
			Console.WriteLine("  overrides {0}", _workspace.OverrideDirectory);
			Console.WriteLine();
			Console.WriteLine("  http://localhost:{0}/", port);
			Console.WriteLine();
			Console.WriteLine("Ctrl+C to stop.");

			using ManualResetEventSlim stopping = new ManualResetEventSlim(false);
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				stopping.Set();
				listener.Stop();
			};

			while (!stopping.IsSet)
			{
				HttpListenerContext context;
				try
				{
					context = listener.GetContext();
				}
				catch (HttpListenerException)
				{
					break;                                   // stopped
				}
				catch (InvalidOperationException)
				{
					break;
				}

				try
				{
					Handle(context);
				}
				catch (Exception ex)
				{
					Send(context, 500, "application/json",
						Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
							new { error = ex.Message }, Json)));
				}
			}
		}

		private void Handle(HttpListenerContext context)
		{
			string path = context.Request.Url.AbsolutePath;

			if (!path.StartsWith("/api/", StringComparison.Ordinal))
			{
				ServeStatic(context, path);
				return;
			}

			switch (path)
			{
				case "/api/status":
					SendJson(context, new
					{
						files = _workspace.FileCount,
						overrides = _workspace.OverrideDirectory
					});
					return;

				case "/api/list":
					SendJson(context, _workspace.List(Extensions(Query(context, "kind"))));
					return;

				case "/api/script":
					GetScript(context);
					return;

				case "/api/script/save":
					SaveScript(context);
					return;

				case "/api/text":
					GetText(context);
					return;

				case "/api/text/save":
					SaveText(context);
					return;

				case "/api/menu":
					GetMenu(context);
					return;

				case "/api/menu/save":
					SaveMenu(context);
					return;

				case "/api/table":
					GetTable(context);
					return;

				case "/api/table/save":
					SaveTable(context);
					return;

				case "/api/messages":
					GetMessages(context);
					return;

				case "/api/ops":
					GetOps(context);
					return;

				case "/api/revert":
					Revert(context);
					return;

				default:
					Send(context, 404, "application/json",
						Encoding.UTF8.GetBytes("{\"error\":\"no such endpoint\"}"));
					return;
			}
		}

		// ------------------------------------------------------------------ scripts

		private void GetScript(HttpListenerContext context)
		{
			string name = Query(context, "name");
			ScriptFile script = ScriptFile.Read(_workspace.Read(name));

			using StringWriter text = new StringWriter();
			Ffs.SourceWriter.Write(text, script, Path.GetFileName(name), _lookupMessage);

			SendJson(context, new
			{
				name,
				overridden = _workspace.IsOverridden(name),
				source = text.ToString()
			});
		}

		private void SaveScript(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			string source = (string)body["source"];
			bool save = body["save"] == null || (bool)body["save"];

			try
			{
				byte[] data = Ffs.Compiler.Compile(Ffs.Parser.Parse(source));
				if (save)
				{
					_workspace.Write(name, data);
				}
				SendJson(context, new
				{
					ok = true,
					saved = save,
					bytes = data.Length,
					overridden = _workspace.IsOverridden(name),
					problems = Array.Empty<object>()
				});
			}
			catch (Ffs.ScriptSyntaxException ex)
			{
				SendJson(context, new
				{
					ok = false,
					saved = false,
					problems = new[] { new { line = ex.Line, column = ex.Column, message = ex.Message } }
				});
			}
			catch (Ffs.ScriptCompileException ex)
			{
				SendJson(context, new
				{
					ok = false,
					saved = false,
					problems = ex.Diagnostics
						.Select(d => new { line = d.Line, column = d.Column, message = d.Message })
						.ToArray()
				});
			}
		}

		// --------------------------------------------------------------------- text

		private void GetText(HttpListenerContext context)
		{
			string name = Query(context, "name");
			MsdFile decoded = Msd.Read(_workspace.Read(name));
			SendJson(context, new
			{
				name,
				overridden = _workspace.IsOverridden(name),
				messages = decoded.Messages.Select(m => new
				{
					id = m.Id,
					pages = m.Pages,
					encoding = m.TextEncoding
				})
			});
		}

		private void SaveText(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];

			MsdFile file = new MsdFile();
			foreach (JsonNode message in body["messages"].AsArray())
			{
				MsdMessage entry = new MsdMessage
				{
					Id = (uint)message["id"],
					TextEncoding = (string)message["encoding"]
				};
				foreach (JsonNode page in message["pages"].AsArray())
				{
					entry.Pages.Add((string)page);
				}
				file.Messages.Add(entry);
			}

			byte[] data = Msd.Write(file);
			_workspace.Write(name, data);
			SendJson(context, new { ok = true, bytes = data.Length, overridden = true });
		}

		// -------------------------------------------------------------------- menus

		private void GetMenu(HttpListenerContext context)
		{
			string name = Query(context, "name");
			XDocument document = MenuXbn.ToXml(_workspace.Read(name));
			SendJson(context, new
			{
				name,
				overridden = _workspace.IsOverridden(name),
				xml = document.ToString(SaveOptions.None)
			});
		}

		private void SaveMenu(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			string xml = (string)body["xml"];

			try
			{
				byte[] data = MenuXbn.FromXml(XDocument.Parse(xml));
				_workspace.Write(name, data);
				SendJson(context, new { ok = true, bytes = data.Length, overridden = true });
			}
			catch (Exception ex)
			{
				SendJson(context, new { ok = false, error = ex.Message });
			}
		}

		/// <summary>
		/// The whole instruction set, with operand names and types. The editor needs
		/// it to highlight, to complete and to say what an argument is for - none of
		/// which should mean a round trip per keystroke, so it is sent once.
		/// </summary>
		private void GetOps(HttpListenerContext context)
		{
			List<object> ops = new List<object>();
			foreach (KeyValuePair<string, int> entry in Ffs.Mnemonics.All)
			{
				ScriptOp op = ScriptOps.Get(entry.Value);
				Operand[] operands = op.Operands ?? Array.Empty<Operand>();
				ops.Add(new
				{
					name = entry.Key,
					opcode = entry.Value,
					handler = op.Name,
					operands = operands.Select((operand, i) => new
					{
						type = operand.ToString().ToLowerInvariant(),
						name = ScriptOperands.Name(entry.Value, i),
						@fixed = ScriptOperands.IsFixed(entry.Value, i)
					}).ToArray()
				});
			}
			SendJson(context, ops);
		}

		/// <summary>
		/// Message ids to their text, for the menu preview. A widget's label is a
		/// message id, so this is what turns a box marked com_item into one that says
		/// "Item". Ids with no message are left out rather than guessed at.
		/// </summary>
		private void GetMessages(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			Dictionary<string, string> found = new Dictionary<string, string>();

			if (_lookupMessage != null && body["ids"] != null)
			{
				foreach (JsonNode id in body["ids"].AsArray())
				{
					// Menu parameters are not all message ids, and some are negative
					// or larger than a message id can be. Skip those rather than
					// letting one odd widget take the whole preview down.
					if (!long.TryParse(id?.ToJsonString(), NumberStyles.Integer,
							CultureInfo.InvariantCulture, out long value)
						|| value < 0 || value > uint.MaxValue)
					{
						continue;
					}
					string text = _lookupMessage((uint)value);
					if (text != null)
					{
						found[value.ToString(CultureInfo.InvariantCulture)] = text;
					}
				}
			}

			SendJson(context, new { available = _lookupMessage != null, messages = found });
		}

		// ------------------------------------------------------------------- tables

		private void GetTable(HttpListenerContext context)
		{
			string name = Query(context, "name");
			byte[] data = _workspace.Read(name);
			int chains = data.Length >= 4 ? BitConverter.ToInt32(data, 0) : 0;
			string family = Pak.FamilyOf(name, chains);
			PakFile decoded = Pak.Read(data, family, _lookupMessage);

			SendJson(context, new
			{
				name,
				family,
				overridden = _workspace.IsOverridden(name),
				// The decoded file goes across as it stands. Chains with no known
				// layout keep their bytes, so a table with one unmodelled chain can
				// still be edited and saved without losing it.
				file = JsonSerializer.SerializeToNode(decoded, Pak.Json)
			});
		}

		private void SaveTable(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			PakFile file = body["file"].Deserialize<PakFile>(Pak.Json);

			byte[] data = Pak.Write(file);
			_workspace.Write(name, data);
			SendJson(context, new { ok = true, bytes = data.Length, overridden = true });
		}

		// ------------------------------------------------------------------- revert

		private void Revert(HttpListenerContext context)
		{
			JsonNode body = ReadBody(context);
			string name = (string)body["name"];
			bool removed = _workspace.Revert(name);
			SendJson(context, new { ok = true, reverted = removed, overridden = false });
		}

		// ------------------------------------------------------------------ plumbing

		private static string[] Extensions(string kind)
		{
			switch (kind)
			{
				case "script": return new[] { ".script" };
				case "text": return new[] { ".msd" };
				case "menu": return new[] { ".xbn" };
				case "table": return new[] { ".pak", ".chaindata" };
				default: return Array.Empty<string>();
			}
		}

		private static string Query(HttpListenerContext context, string key)
		{
			return context.Request.QueryString[key];
		}

		private static JsonNode ReadBody(HttpListenerContext context)
		{
			using StreamReader reader = new StreamReader(
				context.Request.InputStream, Encoding.UTF8);
			return JsonNode.Parse(reader.ReadToEnd());
		}

		private static void SendJson(HttpListenerContext context, object value)
		{
			Send(context, 200, "application/json; charset=utf-8",
				Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, Json)));
		}

		private void ServeStatic(HttpListenerContext context, string path)
		{
			string relative = path == "/" ? "index.html" : path.TrimStart('/');
			string full = Path.GetFullPath(Path.Combine(_webRoot, relative));

			if (!full.StartsWith(_webRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)
				|| !File.Exists(full))
			{
				Send(context, 404, "text/plain", Encoding.UTF8.GetBytes("not found"));
				return;
			}

			Send(context, 200, ContentType(full), File.ReadAllBytes(full));
		}

		private static string ContentType(string path)
		{
			switch (Path.GetExtension(path).ToLowerInvariant())
			{
				case ".html": return "text/html; charset=utf-8";
				case ".js": return "text/javascript; charset=utf-8";
				case ".css": return "text/css; charset=utf-8";
				case ".svg": return "image/svg+xml";
				default: return "application/octet-stream";
			}
		}

		private static void Send(HttpListenerContext context, int status, string type, byte[] body)
		{
			context.Response.StatusCode = status;
			context.Response.ContentType = type;
			context.Response.ContentLength64 = body.Length;
			context.Response.OutputStream.Write(body, 0, body.Length);
			context.Response.OutputStream.Close();
		}
	}
}
