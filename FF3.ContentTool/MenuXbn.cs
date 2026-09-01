// XBN - the binary XML the menus, shops, battle screens and name entry are
// described in. Eight files, and between them they define nearly every screen in
// the game: layout, focus order, and which behaviour class drives each widget.
//
//   ff3content xbn        <file.xbn> [out.xml]     binary -> XML
//   ff3content xbn-build  <file.xml> [out.xbn]     XML -> binary
//
// With the override path, that is the whole loop for changing a menu: decode,
// edit, build, drop the .xbn in Content/Override/files/, restart.
//
// Layout (little endian, unlike the archives)
//   +0  "XBN "
//   +4  version                     0x00010000
//   +8  node count
//   +12 reserved
//   +16 nodes, 20 bytes each, in pre-order:
//         +0  name offset, from the end of the node table
//         +4  data type              0 void, 1 string, 2 decimal
//         +8  decimal value, or for a string its offset from the same base
//         +12 direct children
//         +16 descendants            nextSibling is at id + descendants + 1
//   then NUL-terminated strings, each padded to an even length.
//
// Strings are Shift-JIS, decoded and encoded through the game's own tables (see
// ShiftJis.cs) rather than through CP932, so what this writes is exactly what the
// game reads. Four of the eight files carry Japanese text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace FF3.ContentTool
{
	internal static class MenuXbn
	{
		private const int HeaderSize = 16;
		private const int NodeSize = 20;

		private enum DataType
		{
			Void = 0,
			String = 1,
			Decimal = 2
		}

		private sealed class Node
		{
			public string Name;
			public DataType Type;
			public int Value;          // decimal value, or string offset
			public string Text;
			public int Children;
			public int Descendants;
		}

		// ---------------------------------------------------------------- decode

		public static XDocument ToXml(byte[] data)
		{
			if (data == null || data.Length < HeaderSize
				|| data[0] != (byte)'X' || data[1] != (byte)'B'
				|| data[2] != (byte)'N' || data[3] != (byte)' ')
			{
				throw new InvalidDataException("not an XBN file");
			}

			int version = ReadInt32(data, 4);
			int count = ReadInt32(data, 8);
			int stringBase = HeaderSize + count * NodeSize;
			if (count < 1 || stringBase > data.Length)
			{
				throw new InvalidDataException("node count is not plausible: " + count);
			}

			Node[] nodes = new Node[count];
			for (int i = 0; i < count; i++)
			{
				int at = HeaderSize + i * NodeSize;
				Node node = new Node
				{
					Name = ReadString(data, stringBase + ReadInt32(data, at)),
					Type = (DataType)ReadInt32(data, at + 4),
					Value = ReadInt32(data, at + 8),
					Children = ReadInt32(data, at + 12),
					Descendants = ReadInt32(data, at + 16)
				};
				if (node.Type == DataType.String)
				{
					node.Text = ReadString(data, stringBase + node.Value);
				}
				nodes[i] = node;
			}

			int consumed = 0;
			XElement root = Build(nodes, 0, ref consumed);
			if (version != 0x00010000)
			{
				root.SetAttributeValue("xbnVersion",
					version.ToString(CultureInfo.InvariantCulture));
			}
			return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
		}

		/// <summary>Pre-order walk: a node's children are the nodes that follow it.</summary>
		private static XElement Build(Node[] nodes, int index, ref int consumed)
		{
			Node node = nodes[index];
			XElement element = new XElement(XmlName(node.Name));

			int at = index + 1;
			for (int i = 0; i < node.Children; i++)
			{
				int used = 0;
				element.Add(Build(nodes, at, ref used));
				at += used;
			}

			switch (node.Type)
			{
				case DataType.Decimal:
					if (node.Children > 0)
					{
						// Cannot be written as element text with children present.
						element.SetAttributeValue("value",
							node.Value.ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						element.Value = node.Value.ToString(CultureInfo.InvariantCulture);
					}
					break;

				case DataType.String:
					// "type" only where the text would otherwise read back as a number.
					if (IsInteger(node.Text))
					{
						element.SetAttributeValue("type", "string");
					}
					// Element text cannot carry leading, trailing or purely blank
					// content - a run of ideographic spaces is real data in these
					// files, and reading it back would trim it away.
					if (node.Children > 0 || !IsTextSafe(node.Text))
					{
						element.SetAttributeValue("value", node.Text);
					}
					else
					{
						element.Value = node.Text;
					}
					break;
			}

			consumed = at - index;
			if (consumed != node.Descendants + 1)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
					"node {0} ({1}) claims {2} descendants but the tree holds {3}",
					index, node.Name, node.Descendants, consumed - 1));
			}
			return element;
		}

		// ---------------------------------------------------------------- encode

		public static byte[] FromXml(XDocument document)
		{
			List<Node> nodes = new List<Node>();
			Flatten(document.Root, nodes);

			// One string table in first-use order, deduplicated - which is how the
			// shipped files are laid out, so a decode/build round trip is byte exact.
			List<string> strings = new List<string>();
			Dictionary<string, int> offsets = new Dictionary<string, int>(StringComparer.Ordinal);
			int blob = 0;

			int Intern(string text)
			{
				if (offsets.TryGetValue(text, out int at))
				{
					return at;
				}
				at = blob;
				offsets[text] = at;
				strings.Add(text);
				blob += Padded(text);
				return at;
			}

			int[] nameOffset = new int[nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				nameOffset[i] = Intern(nodes[i].Name);
				if (nodes[i].Type == DataType.String)
				{
					nodes[i].Value = Intern(nodes[i].Text);
				}
			}

			byte[] data = new byte[HeaderSize + nodes.Count * NodeSize + blob];
			data[0] = (byte)'X';
			data[1] = (byte)'B';
			data[2] = (byte)'N';
			data[3] = (byte)' ';
			WriteInt32(data, 4, Version(document.Root));
			WriteInt32(data, 8, nodes.Count);
			WriteInt32(data, 12, 0);

			for (int i = 0; i < nodes.Count; i++)
			{
				int at = HeaderSize + i * NodeSize;
				WriteInt32(data, at, nameOffset[i]);
				WriteInt32(data, at + 4, (int)nodes[i].Type);
				WriteInt32(data, at + 8, nodes[i].Value);
				WriteInt32(data, at + 12, nodes[i].Children);
				WriteInt32(data, at + 16, nodes[i].Descendants);
			}

			int write = HeaderSize + nodes.Count * NodeSize;
			foreach (string text in strings)
			{
				byte[] bytes = Encode(text);
				Buffer.BlockCopy(bytes, 0, data, write, bytes.Length);
				write += Padded(text);
			}
			return data;
		}

		private static int Version(XElement root)
		{
			XAttribute version = root.Attribute("xbnVersion");
			return version != null && int.TryParse(version.Value, NumberStyles.Integer,
				CultureInfo.InvariantCulture, out int value) ? value : 0x00010000;
		}

		/// <summary>Appends the subtree in pre-order and returns its size.</summary>
		private static int Flatten(XElement element, List<Node> into)
		{
			Node node = new Node { Name = element.Name.LocalName };
			int index = into.Count;
			into.Add(node);

			int children = 0;
			int descendants = 0;
			foreach (XElement child in element.Elements())
			{
				descendants += Flatten(child, into);
				children++;
			}

			node.Children = children;
			node.Descendants = descendants;

			XAttribute value = element.Attribute("value");
			string text = value != null
				? value.Value                       // including an empty one
				: (children > 0 ? null : ReadText(element));
			string declared = (string)element.Attribute("type");

			if (text == null)
			{
				node.Type = DataType.Void;
			}
			else if (declared == "string" || !IsInteger(text))
			{
				node.Type = DataType.String;
				node.Text = text;
			}
			else
			{
				node.Type = DataType.Decimal;
				node.Value = int.Parse(text, CultureInfo.InvariantCulture);
			}
			return descendants + 1;
		}

		/// <summary>Whether the text survives a trip through element text unchanged.</summary>
		private static bool IsTextSafe(string text)
		{
			return text.Length > 0 && text == text.Trim();
		}

		private static string ReadText(XElement element)
		{
			string text = element.Value;
			// An element written across several lines is still empty, not a string.
			return element.IsEmpty || text.Trim().Length == 0 ? null : text;
		}

		// ---------------------------------------------------------------- helpers

		private static bool IsInteger(string text)
		{
			return text != null && text.Length > 0
				&& int.TryParse(text, NumberStyles.AllowLeadingSign,
					CultureInfo.InvariantCulture, out _);
		}

		private static string XmlName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new InvalidDataException("node with an empty tag name");
			}
			try
			{
				XmlConvert.VerifyName(name);
			}
			catch (XmlException)
			{
				throw new InvalidDataException("tag name is not usable as XML: " + name);
			}
			return name;
		}

		/// <summary>Bytes one string occupies: NUL terminated, padded to even.</summary>
		private static int Padded(string text)
		{
			int length = Encode(text).Length + 1;
			return length + (length & 1);
		}

		private static byte[] Encode(string text)
		{
			return ShiftJis.Encode(text);
		}

		private static string ReadString(byte[] data, int at)
		{
			if (at < 0 || at >= data.Length)
			{
				throw new InvalidDataException("string offset outside the file: " + at);
			}
			return ShiftJis.Decode(data, at, out _);
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
