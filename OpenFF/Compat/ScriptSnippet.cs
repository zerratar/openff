// One map-script command, run as the map's boot would have run it.
//
// The mod engine's stand-ins for the game's characters (GameCast) replay the boot cast's
// setup commands on themselves - setTreasureItem, bindMotion, setCharacterDetectionRadius,
// setSignEffect, whatever the boot did to the original - so that nothing the boot does is
// approximated. A line as Crystal's disassembly writes it ("setSignEffect(15, 1, 22, 0,
// 0x5000, 0, 0, 0)") is encoded against the FF3 command table (Shared/Script: the operand
// kinds per command) into the bytecode the game's ScriptEngine reads, and run on a spare
// engine (ScriptEngine.runSnippet). The command's cast operands resolve as the script's do -
// through the hich table - which is why RunCast comes first.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OpenFF.Script;

namespace OpenFF.Client
{
	internal static class ScriptSnippet
	{
		private static GlobalScope.ScriptEngine _engine;
		private static Dictionary<string, int> _opcodes;

		/// <summary>Runs one command line; false with the reason when it does not parse or the game is not on a map.</summary>
		public static bool Run(string line, out string error)
		{
			error = null;
			if (GameProfile.IsFf4) { error = "FF3's script table only"; return false; }
			byte[] code = Encode(line, out error);
			if (code == null) return false;
			if (!EngineApi.InWorld) { error = "not on a map"; return false; }
			try
			{
				(_engine ??= new GlobalScope.ScriptEngine()).runSnippet(code);
				return true;
			}
			catch (Exception ex)
			{
				error = ex.GetType().Name + ": " + ex.Message;
				return false;
			}
		}

		/// <summary>The bytecode for one command line, or null with the reason.</summary>
		internal static byte[] Encode(string line, out string error)
		{
			error = null;
			if (string.IsNullOrWhiteSpace(line)) { error = "empty"; return null; }
			string text = line.Trim().TrimEnd(';').Trim();
			int open = text.IndexOf('(');
			int close = text.LastIndexOf(')');
			if (open <= 0 || close < open) { error = "not a command: " + line; return null; }
			string name = text.Substring(0, open).Trim();
			List<string> args = SplitArgs(text.Substring(open + 1, close - open - 1));
			if (!Opcodes().TryGetValue(name, out int opcode)) { error = "no such command: " + name; return null; }
			ScriptOp op = ScriptOpTable.Ff3.Get(opcode);
			Operand[] kinds = op.Operands ?? Array.Empty<Operand>();
			if (args.Count != kinds.Length) { error = name + " takes " + kinds.Length + " operand(s), " + args.Count + " given"; return null; }
			using (MemoryStream bytes = new MemoryStream())
			{
				Word(bytes, (ushort)opcode);
				for (int i = 0; i < kinds.Length; i++)
				{
					string arg = args[i];
					switch (kinds[i])
					{
						case Operand.String:
							if (arg.Length >= 2 && arg[0] == '"' && arg[arg.Length - 1] == '"') arg = arg.Substring(1, arg.Length - 2);
							byte[] utf8 = Encoding.UTF8.GetBytes(arg.Replace("\\\"", "\"").Replace("\\n", "\n"));
							bytes.Write(utf8, 0, utf8.Length);
							bytes.WriteByte(0);
							break;
						default:
							if (!Number(arg, out long value)) { error = name + ": operand " + (i + 1) + " is not a number: " + arg; return null; }
							if (kinds[i] == Operand.Byte) bytes.WriteByte((byte)value);
							else if (kinds[i] == Operand.Word) Word(bytes, (ushort)value);
							else Dword(bytes, (uint)value);
							break;
					}
				}
				return bytes.ToArray();
			}
		}

		private static Dictionary<string, int> Opcodes()
		{
			if (_opcodes != null) return _opcodes;
			Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.Ordinal);
			for (int i = 0; i < ScriptOpTable.Ff3.Count; i++)
			{
				ScriptOp op = ScriptOpTable.Ff3.Get(i);
				if (op?.Name == null) continue;
				string mnemonic = ScriptOpTable.Simplify(op.Name);
				if (!map.ContainsKey(mnemonic)) map[mnemonic] = i;
			}
			return _opcodes = map;
		}

		private static List<string> SplitArgs(string inside)
		{
			List<string> args = new List<string>();
			StringBuilder current = new StringBuilder();
			bool quoted = false;
			for (int i = 0; i < inside.Length; i++)
			{
				char c = inside[i];
				if (c == '"' && (i == 0 || inside[i - 1] != '\\')) quoted = !quoted;
				if (c == ',' && !quoted)
				{
					args.Add(current.ToString().Trim());
					current.Clear();
					continue;
				}
				current.Append(c);
			}
			string last = current.ToString().Trim();
			if (last.Length > 0 || args.Count > 0) args.Add(last);
			// A trailing comment the disassembly writes ("// x 0  y 5  z 0") never reaches here: the caller trims at the ')'.
			return args;
		}

		private static bool Number(string text, out long value)
		{
			text = text.Trim();
			if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				bool ok = long.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
				return ok;
			}
			if (text.StartsWith("-0x", StringComparison.OrdinalIgnoreCase))
			{
				bool ok = long.TryParse(text.Substring(3), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
				value = -value;
				return ok;
			}
			return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
		}

		private static void Word(Stream s, ushort v) { s.WriteByte((byte)(v & 0xFF)); s.WriteByte((byte)(v >> 8)); }

		private static void Dword(Stream s, uint v) { Word(s, (ushort)(v & 0xFFFF)); Word(s, (ushort)(v >> 16)); }
	}
}
