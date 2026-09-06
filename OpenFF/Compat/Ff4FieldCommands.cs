// FF4 field commands with an answer of their own: the Yes/No confirm window, locale
// waits and jumps, the reward window, player levels by FF4's ids.
//
// Read from the FF4 binary (Tools/ff4_calls.py): confirm(a, b) opens menu::ConfirmWindow;
// confirmWait(yesText, noText, jumpYes, jumpNo) waits while it is open, then jumps by the
// result (0 = fall through) - the same texts and targets appear in every call of a script,
// which is what told the two pairs apart. setRewardMessage(textId, 0, icon, 0, 0, 0)
// registers a line for menu::RewardWindow and executeRewardMessageWindow shows them.
// waitByLocale(japanese, other) is a wait whose length depends on the language - the second
// count is the one for anything but Japanese. jumpByLocale(1, 0, label) jumps for one
// locale; the label follows at once in every script that uses it, so falling through is
// what the English game does.

using System;
using System.Collections.Generic;
using OpenFF;

namespace FF3
{
	internal static class Ff4FieldCommands
	{
		// ---- confirm: a Yes/No box drawn by the engine, over the message window ----

		private static bool _open;
		private static bool _yes;
		private static bool _decided;
		private static bool _result;
		private static long _openedFrame;
		private static string _yesText = "Yes", _noText = "No";

		public static bool IsOpen => _open;

		/// <summary>confirm(a, b): opens the box; the texts come with confirmWait.</summary>
		public static void Confirm(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			_open = true;
			_decided = false;
			_yes = true;
			_openedFrame = OpenFF.Game.Time.Frame;
		}

		// A wait that spans frames re-runs the command (suspendRedo), so what has been read is
		// remembered per engine until the answer comes.
		private static readonly Dictionary<GlobalScope.ScriptEngine, uint[]> _waiting = new Dictionary<GlobalScope.ScriptEngine, uint[]>();

		/// <summary>confirmWait(yesText, noText, jumpIfYes, jumpIfNo).</summary>
		public static void ConfirmWait(GlobalScope.ScriptEngine engine)
		{
			uint[] operands = new[] { engine.getDword(), engine.getDword(), engine.getDword(), engine.getDword() };
			if (!_waiting.ContainsKey(engine))
			{
				_waiting[engine] = operands;
				_yesText = Text(operands[0]) ?? "Yes";
				_noText = Text(operands[1]) ?? "No";
				if (!_open)
				{
					// confirmWait without confirm: open it here.
					_open = true;
					_decided = false;
					_yes = true;
					_openedFrame = OpenFF.Game.Time.Frame;
				}
			}
			if (_open && !_decided)
			{
				engine.suspendRedo();
				return;
			}
			_waiting.Remove(engine);
			_open = false;
			uint target = _result ? operands[2] : operands[3];
			if (target != 0)
			{
				engine.jump(target);
			}
		}

		// ---- a Yes/No asked by another command (the inn) ----

		private static readonly HashSet<GlobalScope.ScriptEngine> _asking = new HashSet<GlobalScope.ScriptEngine>();

		/// <summary>Opens the box for a command of its own and holds the engine until it is answered; true, with the answer, once it is.</summary>
		public static bool Ask(GlobalScope.ScriptEngine engine, string yes, string no, out bool answer)
		{
			answer = false;
			if (!_asking.Contains(engine))
			{
				_asking.Add(engine);
				_yesText = yes ?? "Yes";
				_noText = no ?? "No";
				_open = true;
				_decided = false;
				_yes = true;
				_openedFrame = OpenFF.Game.Time.Frame;
			}
			if (_open && !_decided)
			{
				engine.suspendRedo();
				return false;
			}
			_asking.Remove(engine);
			_open = false;
			answer = _result;
			return true;
		}

		private static string Text(uint id)
		{
			if (id == 0) return null;
			try
			{
				string text = GlobalScope.dgs.msg.CMessageSys.getInstance().Main().getMessage(id);
				return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
			}
			catch (Exception) { return null; }
		}

		private const float BoxX = 610f, BoxY = 262f, BoxW = 150f, BoxH = 74f, RowH = 30f;

		/// <summary>Each frame: the box and its answer, while a confirm is up.</summary>
		public static void Tick()
		{
			if (!_open || _decided) return;
			InputState input = OpenFF.Game.Input;
			if (OpenFF.Game.Time.Frame - _openedFrame < 2)
			{
				Draw();
				return;
			}
			if (input.Pressed(Pad.Up) || input.Pressed(Pad.Down)) _yes = !_yes;
			int decided = -1;
			if (input.Pressed(Pad.A)) decided = _yes ? 1 : 0;
			if (input.Pressed(Pad.B)) decided = 0;
			if (decided < 0 && input.PointerReleased)
			{
				float x = input.PointerX, y = input.PointerY;
				if (x >= BoxX && x <= BoxX + BoxW)
				{
					if (y >= BoxY + 6 && y < BoxY + 6 + RowH) decided = 1;
					else if (y >= BoxY + 6 + RowH && y < BoxY + BoxH) decided = 0;
				}
			}
			if (decided < 0)
			{
				Draw();
				return;
			}
			_decided = true;
			_result = decided == 1;
			Log.Write(LogChannel.General, "script: FF4 confirm answered " + (_result ? _yesText : _noText));
		}

		private static void Draw()
		{
			DrawList draw = OpenFF.Game.Draw;
			float w = Math.Max(BoxW, Math.Max(draw.MeasureText(_yesText, 16), draw.MeasureText(_noText, 16)) + 60);
			float x = BoxX + BoxW - w;
			draw.Rect(x, BoxY, w, BoxH, new Color(24, 40, 96, 235));
			draw.Rect(x, BoxY, w, BoxH, new Color(230, 230, 240), filled: false);
			draw.Rect(x + 1, BoxY + 1, w - 2, BoxH - 2, new Color(120, 130, 170), filled: false);
			float yesY = BoxY + 6, noY = BoxY + 6 + RowH;
			draw.Rect(x + 6, (_yes ? yesY : noY) + 2, w - 12, RowH - 4, new Color(255, 255, 255, 40));
			draw.Text(">", x + 14, (_yes ? yesY : noY) + 6, Color.Yellow, 16);
			draw.Text(_yesText, x + 40, yesY + 6, _yes ? Color.White : new Color(200, 200, 210), 16);
			draw.Text(_noText, x + 40, noY + 6, _yes ? new Color(200, 200, 210) : Color.White, 16);
		}

		// ---- locale ----

		private static readonly Dictionary<GlobalScope.ScriptEngine, int> _localeWaits = new Dictionary<GlobalScope.ScriptEngine, int>();

		/// <summary>waitByLocale(japanese, other): a wait of the second count.</summary>
		public static void WaitByLocale(GlobalScope.ScriptEngine engine)
		{
			uint japanese = engine.getWord();
			uint other = engine.getWord();
			if (!_localeWaits.TryGetValue(engine, out int left))
			{
				left = (int)other;
				_localeWaits[engine] = left;
			}
			if (left > 0)
			{
				_localeWaits[engine] = left - 1;
				engine.suspendRedo();
				return;
			}
			_localeWaits.Remove(engine);
		}

		/// <summary>jumpByLocale(locale, ?, label): the label follows at once in every use; falling through is the English game's path.</summary>
		public static void JumpByLocale(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
			engine.getDword();
			engine.getDword();
		}

		// ---- the reward window ----

		private static readonly List<string> _rewards = new List<string>();

		/// <summary>setRewardMessage(textId, 0, icon, 0, 0, 0).</summary>
		public static void SetRewardMessage(GlobalScope.ScriptEngine engine)
		{
			uint id = engine.getDword();
			for (int i = 0; i < 5; i++) engine.getDword();
			string text = Text(id);
			if (text != null) _rewards.Add(text);
		}

		/// <summary>setRewardMessageInterval(frames).</summary>
		public static void SetRewardMessageInterval(GlobalScope.ScriptEngine engine)
		{
			engine.getDword();
		}

		/// <summary>executeRewardMessageWindow(): the registered lines in the message window; the player taps past them.</summary>
		public static void ExecuteRewardMessageWindow(GlobalScope.ScriptEngine engine)
		{
			if (_rewards.Count == 0) return;
			string text = string.Join("\n", _rewards);
			_rewards.Clear();
			OpenFF.Game.Guard("FF4 reward window", () => EngineApi.Dialogue.Say(text));
		}

		// ---- the party ----

		/// <summary>setPlayerLevel(playerType, level): FF4's ids are the party's own (FF3's handler takes 5 off).</summary>
		public static void SetPlayerLevel(GlobalScope.ScriptEngine engine)
		{
			uint id = engine.getDword();
			uint level = engine.getWord();
			try
			{
				GlobalScope.pl.Player player = GlobalScope.pl.PlayerParty.instance().playerForId((byte)id);
				player?.growParameter((byte)Math.Clamp((int)level, 1, 99));
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "script: FF4 setPlayerLevel(" + id + ", " + level + "): " + ex.Message);
			}
		}
	}
}
