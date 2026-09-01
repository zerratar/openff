// .pak - the parameter tables. Items, weapons, armour, spells, monsters, and the
// per map data behind exits, encounters and cameras.
//
//   ff3content pak       <file.pak | directory> [out] [--text=<dir>]
//   ff3content pak-build <file.json> [out.pak]
//
// A .pak is a container of chains; a chain is an array of fixed size records. The
// record layouts are generated from the game's own parse methods into PakRecords.cs,
// so the field names here are the names the game uses.
//
// Layout (little endian)
//   +0  chain count
//   +4  reserved
//   +8  relocated flag - non zero once the game has fixed up the pointers in memory,
//       always zero on disk
//   +12 reserved
//   +16 chains, 8 bytes each: offset, then size in bytes
//   chain data follows, each chain aligned to 16 bytes
//
// Chains are 16 byte aligned, and 147 of the gaps that creates hold leftover bytes
// rather than zeros, so the padding is recorded where it is not zero. Otherwise a
// rebuild would be a byte or two different from the file it came from and there
// would be no way to tell that from a real mistake.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FF3.ContentTool
{
	internal sealed class PakChainData
	{
		[JsonPropertyName("index")]
		public int Index { get; set; }

		[JsonPropertyName("label")]
		public string Label { get; set; }

		/// <summary>Kept so a rebuild lands the chain exactly where it was.</summary>
		[JsonPropertyName("offset")]
		public int Offset { get; set; }

		/// <summary>Alignment bytes before this chain, only when they are not zero.</summary>
		[JsonPropertyName("padBefore")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string PadBefore { get; set; }

		/// <summary>Records, when the chain has a known layout.</summary>
		[JsonPropertyName("records")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<JsonObject> Records { get; set; }

		/// <summary>The bytes, when it does not.</summary>
		[JsonPropertyName("raw")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string Raw { get; set; }
	}

	internal sealed class PakFile
	{
		[JsonPropertyName("family")]
		public string Family { get; set; }

		/// <summary>
		/// The file's length. Kept because a pak can end with alignment padding that
		/// carries no data - item_parameter.pak has four zero bytes after its last
		/// chain - and losing them would make every rebuild differ by a few bytes.
		/// </summary>
		[JsonPropertyName("size")]
		public int Size { get; set; }

		[JsonPropertyName("chains")]
		public List<PakChainData> Chains { get; set; } = new List<PakChainData>();

		/// <summary>Bytes after the last chain, only when they are not zero.</summary>
		[JsonPropertyName("tail")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string Tail { get; set; }
	}

	internal static class Pak
	{
		private const int HeaderSize = 16;
		private const int Alignment = 16;

		public static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = true
		};

		/// <summary>
		/// Which set of record layouts applies. The name settles it for the two
		/// singular files; everything else with the seven chain shape is a map.
		/// </summary>
		public static string FamilyOf(string fileName, int chainCount)
		{
			string name = Path.GetFileName(fileName);
			if (string.Equals(name, "item_parameter.pak", StringComparison.OrdinalIgnoreCase))
			{
				return "Item";
			}
			if (string.Equals(name, "monster.chaindata", StringComparison.OrdinalIgnoreCase))
			{
				return "Monster";
			}
			if (string.Equals(name, "player.chaindata", StringComparison.OrdinalIgnoreCase))
			{
				return "Player";
			}
			return chainCount == 7 ? "Map" : null;
		}

		/// <summary>
		/// Fields holding a message id, and the annotation written beside each. These
		/// are read only: a rebuild takes the id, never the text, so editing the
		/// annotation changes nothing. Renaming an item means editing the message it
		/// points at - see Docs/Text.md.
		/// </summary>
		private static readonly (string Id, string Text)[] Annotated =
		{
			("nameId", "nameText"),
			("captionId", "captionText")
		};

		public static PakFile Read(byte[] data, string family)
		{
			return Read(data, family, null);
		}

		public static PakFile Read(byte[] data, string family, Func<uint, string> lookupMessage)
		{
			if (data == null || data.Length < HeaderSize)
			{
				throw new InvalidDataException("too small to be a pak");
			}
			if (ReadInt32(data, 8) != 0)
			{
				throw new InvalidDataException("this pak has its pointers relocated, "
					+ "which only happens in memory");
			}

			int count = ReadInt32(data, 0);
			if (count <= 0 || count > 64 || HeaderSize + count * 8 > data.Length)
			{
				throw new InvalidDataException("chain count is not plausible: " + count);
			}

			PakFile file = new PakFile { Family = family, Size = data.Length };
			int previous = HeaderSize + count * 8;

			for (int i = 0; i < count; i++)
			{
				int offset = ReadInt32(data, HeaderSize + i * 8);
				int size = ReadInt32(data, HeaderSize + i * 8 + 4);
				if (offset < 0 || size < 0 || offset + size > data.Length)
				{
					throw new InvalidDataException("chain " + i + " runs outside the file");
				}

				PakChain layout = family == null ? null : PakRecords.Find(family, i);
				PakChainData chain = new PakChainData
				{
					Index = i,
					Label = layout?.Label ?? "chain" + i.ToString(CultureInfo.InvariantCulture),
					Offset = offset,
					PadBefore = Hex(data, previous, offset - previous)
				};

				if (layout != null && size > 0)
				{
					chain.Records = ReadRecords(data, offset, size, layout);
					if (lookupMessage != null)
					{
						Annotate(chain.Records, lookupMessage);
					}
				}
				else
				{
					chain.Raw = ToHex(data, offset, size);
				}

				file.Chains.Add(chain);
				previous = offset + size;
			}

			file.Tail = Hex(data, previous, data.Length - previous);
			return file;
		}

		private static List<JsonObject> ReadRecords(byte[] data, int offset, int size,
			PakChain layout)
		{
			List<JsonObject> records = new List<JsonObject>();
			bool fills = layout.Fields.Any(f => f.Count < 0);
			int stride = Math.Max(layout.Stride, 1);
			int count = fills ? 1 : size / stride;

			int at = offset;
			for (int i = 0; i < count; i++)
			{
				JsonObject record = new JsonObject();
				foreach (PakField field in layout.Fields)
				{
					int elements = field.Count < 0
						? Remaining(offset + size - at, field.Type)
						: field.Count;
					if (elements == 1 && field.Count >= 0)
					{
						record[field.Name] = Value(data, ref at, field.Type);
					}
					else
					{
						JsonArray values = new JsonArray();
						for (int e = 0; e < elements; e++)
						{
							values.Add(Value(data, ref at, field.Type));
						}
						record[field.Name] = values;
					}
				}
				records.Add(record);
			}
			return records;
		}

		private static void Annotate(List<JsonObject> records, Func<uint, string> lookupMessage)
		{
			foreach (JsonObject record in records)
			{
				foreach ((string id, string text) in Annotated)
				{
					// A JsonValue made from a short does not hand back an int, so the
					// number is read back through its JSON text rather than by type.
					if (record[id] is JsonValue value
						&& int.TryParse(value.ToJsonString(), NumberStyles.Integer,
							CultureInfo.InvariantCulture, out int number)
						&& number > 0)
					{
						string message = lookupMessage((uint)number);
						if (message != null)
						{
							record[text] = message;
						}
					}
				}
			}
		}

		private static int Remaining(int bytes, FieldType type)
		{
			int width = Width(type);
			return width == 0 ? 0 : Math.Max(0, bytes / width);
		}

		public static byte[] Write(PakFile file)
		{
			int count = file.Chains.Count;
			List<byte[]> blobs = new List<byte[]>(count);
			foreach (PakChainData chain in file.Chains)
			{
				PakChain layout = file.Family == null
					? null : PakRecords.Find(file.Family, chain.Index);
				blobs.Add(chain.Records != null && layout != null
					? WriteRecords(chain.Records, layout)
					: FromHex(chain.Raw));
			}

			int end = file.Chains.Count == 0
				? HeaderSize
				: file.Chains[count - 1].Offset + blobs[count - 1].Length;
			byte[] tail = FromHex(file.Tail);
			byte[] data = new byte[Math.Max(file.Size, end + tail.Length)];

			WriteInt32(data, 0, count);
			for (int i = 0; i < count; i++)
			{
				PakChainData chain = file.Chains[i];
				WriteInt32(data, HeaderSize + i * 8, chain.Offset);
				WriteInt32(data, HeaderSize + i * 8 + 4, blobs[i].Length);
				Buffer.BlockCopy(blobs[i], 0, data, chain.Offset, blobs[i].Length);

				byte[] pad = FromHex(chain.PadBefore);
				if (pad.Length > 0 && chain.Offset - pad.Length >= 0)
				{
					Buffer.BlockCopy(pad, 0, data, chain.Offset - pad.Length, pad.Length);
				}
			}
			Buffer.BlockCopy(tail, 0, data, end, tail.Length);
			return data;
		}

		private static byte[] WriteRecords(List<JsonObject> records, PakChain layout)
		{
			using MemoryStream stream = new MemoryStream();
			foreach (JsonObject record in records)
			{
				foreach (PakField field in layout.Fields)
				{
					// Only layout fields are written, so the text annotations are
					// carried along harmlessly and cannot change what is built.
					JsonNode node = record[field.Name];
					if (node is JsonArray array)
					{
						foreach (JsonNode element in array)
						{
							Put(stream, element, field.Type);
						}
					}
					else
					{
						Put(stream, node, field.Type);
					}
				}
			}
			return stream.ToArray();
		}

		/// <summary>Records and chain count, for the one-line summary.</summary>
		public static string Describe(PakFile file)
		{
			int records = file.Chains.Sum(c => c.Records?.Count ?? 0);
			int raw = file.Chains.Count(c => c.Records == null);
			return string.Format(CultureInfo.InvariantCulture,
				"{0} chains, {1} records{2}", file.Chains.Count, records,
				raw > 0 ? ", " + raw + " not decoded" : string.Empty);
		}

		// ---------------------------------------------------------------- values

		private static int Width(FieldType type)
		{
			switch (type)
			{
				case FieldType.U8:
				case FieldType.S8:
					return 1;
				case FieldType.U16:
				case FieldType.S16:
					return 2;
				default:
					return 4;
			}
		}

		private static JsonNode Value(byte[] data, ref int at, FieldType type)
		{
			switch (type)
			{
				case FieldType.U8:
					return JsonValue.Create(data[at++]);
				case FieldType.S8:
					return JsonValue.Create((sbyte)data[at++]);
				case FieldType.U16:
					at += 2;
					return JsonValue.Create((ushort)(data[at - 2] | (data[at - 1] << 8)));
				case FieldType.S16:
					at += 2;
					return JsonValue.Create((short)(data[at - 2] | (data[at - 1] << 8)));
				case FieldType.F32:
					at += 4;
					return JsonValue.Create(BitConverter.ToSingle(data, at - 4));
				case FieldType.U32:
					at += 4;
					return JsonValue.Create(BitConverter.ToUInt32(data, at - 4));
				default:
					at += 4;
					return JsonValue.Create(BitConverter.ToInt32(data, at - 4));
			}
		}

		private static void Put(Stream stream, JsonNode node, FieldType type)
		{
			switch (type)
			{
				case FieldType.U8:
					stream.WriteByte(node == null ? (byte)0 : node.GetValue<byte>());
					break;
				case FieldType.S8:
					stream.WriteByte(node == null ? (byte)0 : (byte)node.GetValue<sbyte>());
					break;
				case FieldType.U16:
					Put16(stream, node == null ? (ushort)0 : node.GetValue<ushort>());
					break;
				case FieldType.S16:
					Put16(stream, node == null ? (ushort)0 : (ushort)node.GetValue<short>());
					break;
				case FieldType.F32:
					Put32(stream, BitConverter.GetBytes(
						node == null ? 0f : node.GetValue<float>()));
					break;
				case FieldType.U32:
					Put32(stream, BitConverter.GetBytes(
						node == null ? 0u : node.GetValue<uint>()));
					break;
				default:
					Put32(stream, BitConverter.GetBytes(
						node == null ? 0 : node.GetValue<int>()));
					break;
			}
		}

		private static void Put16(Stream stream, ushort value)
		{
			stream.WriteByte((byte)value);
			stream.WriteByte((byte)(value >> 8));
		}

		private static void Put32(Stream stream, byte[] four)
		{
			stream.Write(four, 0, 4);
		}

		// ---------------------------------------------------------------- bytes

		/// <summary>Hex for a span, or null when it is empty or all zeros.</summary>
		private static string Hex(byte[] data, int at, int length)
		{
			if (length <= 0)
			{
				return null;
			}
			for (int i = at; i < at + length; i++)
			{
				if (data[i] != 0)
				{
					return ToHex(data, at, length);
				}
			}
			return null;
		}

		private static string ToHex(byte[] data, int at, int length)
		{
			StringBuilder text = new StringBuilder(length * 2);
			for (int i = at; i < at + length; i++)
			{
				text.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
			}
			return text.ToString();
		}

		private static byte[] FromHex(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return Array.Empty<byte>();
			}
			byte[] data = new byte[text.Length / 2];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = byte.Parse(text.Substring(i * 2, 2),
					NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return data;
		}

		private static int ReadInt32(byte[] data, int at)
		{
			return data[at] | (data[at + 1] << 8) | (data[at + 2] << 16) | (data[at + 3] << 24);
		}

		private static void WriteInt32(byte[] data, int at, int value)
		{
			data[at] = (byte)value;
			data[at + 1] = (byte)(value >> 8);
			data[at + 2] = (byte)(value >> 16);
			data[at + 3] = (byte)(value >> 24);
		}
	}
}
