// MSD - every line of text in the game. 1890 files, 55431 messages: dialogue,
// menu labels, item and spell names and descriptions, battle chatter.
//
//   ff3content msd        <file.msd | directory> [out]
//   ff3content msd-build  <file.json>            [out.msd]
//
// Menus reference these by number: MenuDefine.xbn gives the main menu's Item
// command <parameter>50003</parameter>, and message 50003 in eureka_menu.msd is
// "Item". Changing the text is an edit here; see Docs/Text.md.
//
// Layout (little endian)
//   +0  "MSDA"
//   +4  version                 0x00010000
//   +8  message count
//   +12 reserved
//   +16 messages, 12 bytes each, ordered by number:
//         +0  number            the id menus and scripts refer to
//         +4  page count
//         +5  font              1 in every shipped file
//         +6  padding           CCCC in every shipped file, uninitialised memory
//         +8  offset of the text, from the start of the file
//   then the text: per message, one NUL terminated UTF-8 string per page,
//   followed by one more NUL.
//
// The text is UTF-8, not Shift-JIS - the phone build converted it. Line breaks are
// real newlines, and %name% placeholders are substituted at runtime.
//
// Not all of it is valid UTF-8 though. The European scripts hold windows-1252 bytes
// ("Altarhoehle" spelt with a single 0xF6), and a few English messages carry raw
// Shift-JIS. Those messages are kept byte for byte, marked with "encoding": "latin1",
// which maps every byte to the code point of the same value and so loses nothing.
// See Docs/Text.md - the game decodes all of it as UTF-8, so those messages do not
// currently display correctly.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FF3.Content
{
	internal sealed class MsdMessage
	{
		[JsonPropertyName("id")]
		public uint Id { get; set; }

		/// <summary>One string per page; the game shows them one at a time.</summary>
		[JsonPropertyName("pages")]
		public List<string> Pages { get; set; } = new List<string>();

		/// <summary>Written only when it is not the usual 1.</summary>
		[JsonPropertyName("font")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public byte? Font { get; set; }

		/// <summary>Written only when it is not the usual CCCC.</summary>
		[JsonPropertyName("padding")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public ushort? Padding { get; set; }

		/// <summary>
		/// "latin1" for text that is not valid UTF-8, where each byte is held as the
		/// code point of the same value so nothing is lost. Absent means UTF-8.
		/// </summary>
		[JsonPropertyName("encoding")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string TextEncoding { get; set; }
	}

	internal sealed class MsdFile
	{
		[JsonPropertyName("format")]
		public string Format { get; set; } = "MSDA";

		[JsonPropertyName("version")]
		public uint Version { get; set; } = 0x00010000;

		[JsonPropertyName("reserved")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		public uint Reserved { get; set; }

		[JsonPropertyName("messages")]
		public List<MsdMessage> Messages { get; set; } = new List<MsdMessage>();
	}

	internal static class Msd
	{
		private const int HeaderSize = 16;
		private const int EntrySize = 12;
		private const byte DefaultFont = 1;
		private const ushort DefaultPadding = 0xCCCC;

		public static readonly JsonSerializerOptions Json = new JsonSerializerOptions
		{
			WriteIndented = true,
			// Text stays legible in the file rather than turning into \uXXXX escapes.
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		public static MsdFile Read(byte[] data)
		{
			if (data == null || data.Length < HeaderSize
				|| data[0] != (byte)'M' || data[1] != (byte)'S'
				|| data[2] != (byte)'D' || data[3] != (byte)'A')
			{
				throw new InvalidDataException("not an MSD file");
			}

			MsdFile file = new MsdFile
			{
				Version = ReadUInt32(data, 4),
				Reserved = ReadUInt32(data, 12)
			};
			uint count = ReadUInt32(data, 8);
			int textBase = HeaderSize + (int)count * EntrySize;
			if (count > int.MaxValue / EntrySize || textBase > data.Length)
			{
				throw new InvalidDataException("message count is not plausible: " + count);
			}

			// FF4 writes the same container with UTF-16LE text, terminated by a 16 bit
			// zero; FF3 writes UTF-8. A whole file is one or the other, so this is
			// decided once - and from how a message ends rather than what it says.
			// Looking at the text was wrong twice: a one-letter UTF-8 page, "A" 00, is
			// exactly a one-letter UTF-16 page; and FF4's item names open with a
			// private-use icon glyph, U+E040, whose high byte is not zero. The ending
			// is unambiguous. UTF-8 finishes text 00 00 - a nonzero byte, the page's
			// terminator, the message's. UTF-16 finishes 00 00 00 00.
			bool wide = SniffWide(data, (int)count);

			for (int i = 0; i < count; i++)
			{
				int at = HeaderSize + i * EntrySize;
				byte pages = data[at + 4];
				byte font = data[at + 5];
				ushort padding = (ushort)(data[at + 6] | (data[at + 7] << 8));
				int offset = (int)ReadUInt32(data, at + 8);

				MsdMessage message = new MsdMessage
				{
					Id = ReadUInt32(data, at),
					Font = font == DefaultFont ? (byte?)null : font,
					Padding = padding == DefaultPadding ? (ushort?)null : padding
				};

				// Spans first, then one decode for the whole message: a message that is
				// not valid UTF-8 is held byte for byte, and that is an all-or-nothing
				// decision so a single page cannot disagree with its neighbours.
				List<(int At, int Length)> spans = new List<(int, int)>(pages);
				int p = offset;
				for (int page = 0; page < pages; page++)
				{
					int end = wide ? IndexOfWideZero(data, p) : Array.IndexOf(data, (byte)0, p);
					if (end < 0)
					{
						throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
							"message {0} page {1} is not terminated", message.Id, page));
					}
					spans.Add((p, end - p));
					p = end + (wide ? 2 : 1);
				}

				Encoding encoding;
				if (wide)
				{
					encoding = Encoding.Unicode;
					message.TextEncoding = "utf-16";
				}
				else
				{
					bool utf8 = spans.All(span => IsUtf8(data, span.At, span.Length));
					encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
					message.TextEncoding = utf8 ? null : "latin1";
				}
				foreach ((int at2, int length) in spans)
				{
					message.Pages.Add(encoding.GetString(data, at2, length));
				}
				file.Messages.Add(message);
			}
			return file;
		}

		public static byte[] Write(MsdFile file)
		{
			int count = file.Messages.Count;
			int textBase = HeaderSize + count * EntrySize;

			List<byte[]> blobs = new List<byte[]>(count);
			int textSize = 0;
			foreach (MsdMessage message in file.Messages)
			{
				if (message.Pages.Count > byte.MaxValue)
				{
					throw new InvalidDataException("message " + message.Id
						+ " has more pages than the format allows: " + message.Pages.Count);
				}
				bool wide = message.TextEncoding == "utf-16";
				Encoding encoding = wide ? Encoding.Unicode
					: message.TextEncoding == "latin1" ? Encoding.Latin1 : Encoding.UTF8;
				using MemoryStream blob = new MemoryStream();
				foreach (string page in message.Pages)
				{
					byte[] text = encoding.GetBytes(page ?? string.Empty);
					if (!wide && Array.IndexOf(text, (byte)0) >= 0)
					{
						throw new InvalidDataException("message " + message.Id
							+ " contains a NUL, which separates pages");
					}
					blob.Write(text, 0, text.Length);
					blob.WriteByte(0);
					if (wide) blob.WriteByte(0);
				}
				blob.WriteByte(0);          // end of message
				if (wide) blob.WriteByte(0);
				byte[] bytes = blob.ToArray();
				blobs.Add(bytes);
				textSize += bytes.Length;
			}

			byte[] data = new byte[textBase + textSize];
			data[0] = (byte)'M';
			data[1] = (byte)'S';
			data[2] = (byte)'D';
			data[3] = (byte)'A';
			WriteUInt32(data, 4, file.Version);
			WriteUInt32(data, 8, (uint)count);
			WriteUInt32(data, 12, file.Reserved);

			int write = textBase;
			for (int i = 0; i < count; i++)
			{
				MsdMessage message = file.Messages[i];
				int at = HeaderSize + i * EntrySize;
				WriteUInt32(data, at, message.Id);
				data[at + 4] = (byte)message.Pages.Count;
				data[at + 5] = message.Font ?? DefaultFont;
				ushort padding = message.Padding ?? DefaultPadding;
				data[at + 6] = (byte)padding;
				data[at + 7] = (byte)(padding >> 8);
				WriteUInt32(data, at + 8, (uint)write);

				Buffer.BlockCopy(blobs[i], 0, data, write, blobs[i].Length);
				write += blobs[i].Length;
			}
			return data;
		}

		/// <summary>Whether a file's text is UTF-16LE, from how its messages end.</summary>
		private static bool SniffWide(byte[] data, int count)
		{
			for (int i = 0; i < count; i++)
			{
				int start = (int)ReadUInt32(data, HeaderSize + i * EntrySize + 8);
				int end = i + 1 < count
					? (int)ReadUInt32(data, HeaderSize + (i + 1) * EntrySize + 8)
					: data.Length;
				int length = end - start;
				// Four bytes is the least a message with any text can be in either
				// encoding; an empty page is shorter and says nothing.
				if (start < 0 || end > data.Length || length < 4)
				{
					continue;
				}
				// The last byte of text sits three from the end in UTF-8 and is never
				// zero there; in UTF-16 the two terminators are four zero bytes.
				return data[end - 3] == 0 && data[end - 4] == 0;
			}
			return false;
		}

		/// <summary>The first 16 bit zero on an even boundary from <paramref name="from"/>.</summary>
		private static int IndexOfWideZero(byte[] data, int from)
		{
			for (int i = from; i + 1 < data.Length; i += 2)
			{
				if (data[i] == 0 && data[i + 1] == 0) return i;
			}
			return -1;
		}

		/// <summary>Whether a span decodes as UTF-8 without substitutions.</summary>
		private static bool IsUtf8(byte[] data, int at, int length)
		{
			try
			{
				Strict.GetString(data, at, length);
				return true;
			}
			catch (DecoderFallbackException)
			{
				return false;
			}
		}

		private static readonly UTF8Encoding Strict = new UTF8Encoding(false, true);

		/// <summary>Message count and page count, for the one-line summary.</summary>
		public static string Describe(MsdFile file)
		{
			int pages = file.Messages.Sum(m => m.Pages.Count);
			return string.Format(CultureInfo.InvariantCulture,
				"{0} messages, {1} pages", file.Messages.Count, pages);
		}

		private static uint ReadUInt32(byte[] data, int at)
		{
			return (uint)(data[at] | (data[at + 1] << 8)
				| (data[at + 2] << 16) | (data[at + 3] << 24));
		}

		private static void WriteUInt32(byte[] data, int at, uint value)
		{
			data[at] = (byte)value;
			data[at + 1] = (byte)(value >> 8);
			data[at + 2] = (byte)(value >> 16);
			data[at + 3] = (byte)(value >> 24);
		}
	}
}
