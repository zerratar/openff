// FF4's menu layouts and menu texts, for the OpenFF menu on FF4.
//
// MENU_LAYOUT.dat holds MenuLayout_<name>.xbn.lz (Root, Status, Item, Equipment, Magic, ...;
// FF3.Content.Xbn reads them; Reference/libff4/menu-layouts.txt lists their frames); the
// texts are babil_menu.msd (50002 Inventory .. 50011 Abilities, 50204.. Right/Left/Head/Body/
// Arms, 50401 Lv, 50410 HP, 50411 MP, 50420.. the attributes, 50446 Gil, 50451 EXP, 51000..
// the class names). Both are read through the game's file system on first use.

using System;
using System.Collections.Generic;
using FF3.Content;
using OpenFF.Data;

namespace FF3
{
	internal static class Ff4Layouts
	{
		private static readonly Dictionary<string, Layout> _layouts = new Dictionary<string, Layout>(StringComparer.OrdinalIgnoreCase);
		private static Dictionary<uint, string> _texts;
		private static bool _textsLooked;

		/// <summary>MenuLayout_&lt;name&gt;.xbn, or null when the content lacks it.</summary>
		public static Layout Get(string name)
		{
			if (_layouts.TryGetValue(name, out Layout l)) return l;
			Layout layout = null;
			try
			{
				byte[] data = Ff4Ui.ReadFile("MenuLayout_" + name + ".xbn.lz") ?? Ff4Ui.ReadFile("MenuLayout_" + name + ".xbn");
				if (data != null && Lz.IsCompressed(data)) data = Lz.Decompress(data);
				if (data != null) layout = Xbn.ReadLayout(data);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "ff4 layouts: " + name + ": " + ex.Message); }
			if (layout == null) Log.Write(LogChannel.General, "ff4 layouts: MenuLayout_" + name + " not read");
			_layouts[name] = layout;
			return layout;
		}

		/// <summary>The y (DS units) of a frame of a layout, or the fallback.</summary>
		public static int FrameY(string layout, int id, int fallback)
		{
			LayoutFrame f = Get(layout)?.Frame(id);
			return f != null ? f.Y : fallback;
		}

		/// <summary>A menu text by its babil_menu.msd id; the fallback when unknown.</summary>
		public static string Text(uint id, string fallback = null)
		{
			if (!_textsLooked)
			{
				_textsLooked = true;
				try { _texts = TableFiles.ReadNames(GameArchive.Chain, "babil_menu.msd", new GameTables()); }
				catch (Exception ex) { Log.Write(LogChannel.General, "ff4 layouts: babil_menu.msd: " + ex.Message); }
			}
			if (_texts != null && _texts.TryGetValue(id, out string s) && !string.IsNullOrEmpty(s)) return s.Replace("\n", " ").Trim();
			return fallback ?? ("#" + id);
		}
	}
}
