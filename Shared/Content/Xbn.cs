// XBN, Square's binary XML: the menu layouts of FF3 (MenuDefine.xbn) and FF4 (MENU_LAYOUT.dat's
// MenuLayout_*.xbn). Read here for drawing menus from the layouts; Tools/xbn_dump.py prints one.
//
// Header: "XBN " u32 version, u32 node count, u32 reserved; then one record of five int32 per
// node - the offset of its name from the end of the record table, the data type (0 void,
// 1 string, 2 decimal), the data (an int, or a string's offset likewise), the child count,
// the family size (the node and everything under it) - and the Shift-JIS strings after.
// Children follow their parent in order, so the family sizes rebuild the tree.
//
// A layout's schema (FF4): layout > unit {name, display} > frame {id, group, x, y, width,
// height, choices, link, top, behavior "FBText" | "FBTextSCC" | "FBSprite" {parameter...}},
// nested frames relative to their parent, in the DS's 256 x 192 units. layout::Frame::setup
// in the FF4 binary makes x and y absolute by adding the parent's and inherits the group.

using System;
using System.Collections.Generic;
using System.Text;

namespace FF3.Content
{
	public sealed class XbnNode
	{
		public string Name;
		public int Type;
		public int Int;
		public string String;
		public List<XbnNode> Children = new List<XbnNode>();

		public XbnNode Child(string name)
		{
			foreach (XbnNode c in Children) if (c.Name == name) return c;
			return null;
		}

		public int? IntOf(string name)
		{
			XbnNode c = Child(name);
			return c != null && c.Type == 2 ? c.Int : (int?)null;
		}

		public string StringOf(string name) => Child(name)?.String;
	}

	/// <summary>A frame of a layout, with absolute position (DS units) and its behaviour.</summary>
	public sealed class LayoutFrame
	{
		public int Id = -1, Group = -1;
		public int X, Y, Width, Height;
		public bool Choices;
		public string Behavior;
		public List<int> Parameters = new List<int>();
		public LayoutFrame Parent;
		public List<LayoutFrame> Children = new List<LayoutFrame>();
		/// <summary>FBText's first parameter: the message id of the text shown.</summary>
		public int MessageId => Behavior == "FBText" && Parameters.Count > 0 ? Parameters[0] : -1;
	}

	public sealed class Layout
	{
		public string UnitName;
		public List<LayoutFrame> Frames = new List<LayoutFrame>();   // every frame, in file order

		public LayoutFrame Frame(int id)
		{
			foreach (LayoutFrame f in Frames) if (f.Id == id) return f;
			return null;
		}
	}

	public static class Xbn
	{
		/// <summary>The tree of an XBN file (decompressed already); null when it is not one.</summary>
		public static XbnNode Read(byte[] data)
		{
			if (data == null || data.Length < 16 || data[0] != 'X' || data[1] != 'B' || data[2] != 'N') return null;
			int count = BitConverter.ToInt32(data, 8);
			if (count <= 0 || 16 + 20 * count > data.Length) return null;
			int strings = 16 + 20 * count;
			XbnNode[] nodes = new XbnNode[count];
			int[] family = new int[count];
			for (int i = 0; i < count; i++)
			{
				int at = 16 + 20 * i;
				int nameOffset = BitConverter.ToInt32(data, at);
				int type = BitConverter.ToInt32(data, at + 4);
				int value = BitConverter.ToInt32(data, at + 8);
				family[i] = BitConverter.ToInt32(data, at + 16);
				XbnNode n = new XbnNode { Name = CString(data, strings + nameOffset), Type = type };
				if (type == 1) n.String = CString(data, strings + value);
				else if (type == 2) n.Int = value;
				nodes[i] = n;
			}
			// Children follow their parent; a family's end is the parent's index plus its size.
			Stack<(int End, XbnNode Node)> open = new Stack<(int, XbnNode)>();
			for (int i = 0; i < count; i++)
			{
				while (open.Count > 0 && i >= open.Peek().End) open.Pop();
				if (open.Count > 0) open.Peek().Node.Children.Add(nodes[i]);
				open.Push((i + Math.Max(1, family[i]), nodes[i]));
			}
			return nodes[0];
		}

		/// <summary>The frames of a layout file with absolute DS positions, as layout::Frame::setup computes them.</summary>
		public static Layout ReadLayout(byte[] data)
		{
			XbnNode root = Read(data);
			if (root == null) return null;
			Layout layout = new Layout();
			XbnNode unit = root.Name == "unit" ? root : root.Child("unit") ?? root;
			layout.UnitName = unit.StringOf("name");
			foreach (XbnNode c in unit.Children)
			{
				if (c.Name == "frame") AddFrame(layout, c, null, 0, 0, -1);
			}
			return layout;
		}

		private static void AddFrame(Layout layout, XbnNode node, LayoutFrame parent, int px, int py, int group)
		{
			LayoutFrame f = new LayoutFrame
			{
				Parent = parent,
				Id = node.IntOf("id") ?? -1,
				Group = node.IntOf("group") ?? group,
				X = (node.IntOf("x") ?? 0) + px,
				Y = (node.IntOf("y") ?? 0) + py,
				Width = node.IntOf("width") ?? 0,
				Height = node.IntOf("height") ?? 0,
				Choices = node.Child("choices") != null,
			};
			XbnNode behavior = node.Child("behavior");
			if (behavior != null)
			{
				f.Behavior = behavior.String;
				foreach (XbnNode p in behavior.Children) if (p.Name == "parameter" && p.Type == 2) f.Parameters.Add(p.Int);
			}
			layout.Frames.Add(f);
			parent?.Children.Add(f);
			foreach (XbnNode c in node.Children)
			{
				if (c.Name == "frame") AddFrame(layout, c, f, f.X, f.Y, f.Group);
			}
		}

		private static string CString(byte[] data, int at)
		{
			if (at < 0 || at >= data.Length) return "";
			int end = Array.IndexOf(data, (byte)0, at);
			if (end < 0) end = data.Length;
			return Encoding.Latin1.GetString(data, at, end - at);   // the names are ASCII; Shift-JIS text would need the code-pages provider
		}
	}
}
