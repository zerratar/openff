// --say=<message id>: open the field message window with one of the current map's
// messages a few seconds after the map is up. For checking text and window art without
// finding an NPC who will stand still - and for a modder to preview a line.

using System;
using System.Globalization;

namespace FF3
{
	internal static class DevSay
	{
		private static readonly int _id = int.TryParse(Options.Get("say"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : -1;
		private static int _frames;
		private static bool _done;

		public static void Tick()
		{
			if (_id < 0 || _done)
			{
				return;
			}
			_frames++;
			if (_frames < 240)
			{
				return;
			}
			_done = true;
			try
			{
				GlobalScope.wld.CMessageWindow window = GlobalScope.CCastCommandTransit.getInstance().cast_Field2D()?.MessageWindow();
				if (window == null)
				{
					Log.Write(LogChannel.General, "say: no field 2D yet");
					return;
				}
				window.createMessageWindow(0, _id, -1);
				window.createMessage(_id, 0, 0);
				Log.Write(LogChannel.General, "say: message " + _id + " opened");
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "say: " + ex.GetType().Name + " " + ex.Message);
			}
		}
	}
}
