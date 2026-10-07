// The drive's "draws <name>": every 2D draw command of the next finished step, written out in 1920 x 1080 pixels -
// the same space Tools/ff4hook's draws dump gives the Steam FF4.exe's, so the two HUDs can be set side by side by
// number, not by eye. A row: kind, x, y, w, h, colour (r g b a), the text and its em in pixels, the texture's source
// rectangle.

using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OpenFF.Client
{
	internal static class DrawsDump
	{
		/// <summary>The file the next step's draws go to, or null.</summary>
		public static string Pending;

		public static void Write(IReadOnlyList<DrawCommand> commands)
		{
			string path = Pending;
			if (path == null) return;
			Pending = null;
			float sx = 1920f / DrawList.ScreenWidth, sy = 1080f / DrawList.ScreenHeight;
			CultureInfo c = CultureInfo.InvariantCulture;
			using StreamWriter w = new StreamWriter(path, false);
			w.WriteLine("kind\tx\ty\tw\th\tr\tg\tb\ta\tem\ttext\tsrc");
			foreach (DrawCommand d in commands)
			{
				float width = d.W, height = d.H, em = 0f;
				if (d.Kind == DrawKind.Text)
				{
					width = TrueTypeText.Width(d.Text, d.Size);
					em = d.Size * TrueTypeText.SizeFactor * sy;
					height = d.Size * TrueTypeText.SizeFactor;
				}
				if (d.Kind == DrawKind.Line) { width = d.X2 - d.X; height = d.Y2 - d.Y; }
				w.WriteLine(string.Join("\t",
					d.Kind.ToString(),
					(d.X * sx).ToString("0.0", c), (d.Y * sy).ToString("0.0", c), (width * sx).ToString("0.0", c), (height * sy).ToString("0.0", c),
					d.Color.R.ToString(c), d.Color.G.ToString(c), d.Color.B.ToString(c), d.Color.A.ToString(c),
					em.ToString("0.0", c), (d.Text ?? "").Replace('\t', ' ').Replace('\n', ' '),
					d.Kind == DrawKind.Sprite ? string.Format(c, "{0:0} {1:0} {2:0} {3:0}", d.SrcX, d.SrcY, d.SrcW, d.SrcH) : ""));
			}
			Log.Write(LogChannel.General, "drive: draws written to " + path + " (" + commands.Count + ")");
		}
	}
}
