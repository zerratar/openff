// FF4's Settings, as the Steam game's menu has them (Music, Sound Effects, Voices, Battle Mode, Battle Speed, Subtitles,
// Window Design): %LocalAppData%\OpenFF\ff4-settings.json, read once, written when the menu's Settings screen closes.
//
//   { "music": 10, "soundEffects": 10, "voices": 10, "battleMode": 0, "battleSpeed": 3, "subtitles": true, "windowDesign": 1 }
//
// Music and sound effects go to the game's own volumes (opt.SoundOption, 0..127); Battle Mode (0 Active, 1 Wait) and Battle
// Speed (1 the fastest .. 6) to the battle - the command line's --ff4-battle-wait and --ff4-battle-speed still win for a run;
// Window Design (1..6) picks MENU_Common's frame, scroll bar, button and wallpaper set (the menu layout's design-N class).

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFF.Client
{
	internal sealed class Ff4Settings
	{
		[JsonPropertyName("music")] public int Music { get; set; } = 10;
		[JsonPropertyName("soundEffects")] public int SoundEffects { get; set; } = 10;
		[JsonPropertyName("voices")] public int Voices { get; set; } = 10;
		[JsonPropertyName("battleMode")] public int BattleMode { get; set; }
		[JsonPropertyName("battleSpeed")] public int BattleSpeed { get; set; } = 3;
		[JsonPropertyName("subtitles")] public bool Subtitles { get; set; } = true;
		[JsonPropertyName("windowDesign")] public int WindowDesign { get; set; } = 1;

		private static Ff4Settings _current;

		public static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenFF", "ff4-settings.json");

		public static Ff4Settings Current
		{
			get
			{
				if (_current != null) return _current;
				try { if (File.Exists(FilePath)) _current = JsonSerializer.Deserialize<Ff4Settings>(File.ReadAllText(FilePath)); }
				catch (Exception ex) { Log.Write(LogChannel.General, "ff4 settings: " + FilePath + ": " + ex.Message); }
				_current ??= new Ff4Settings();
				_current.Clamp();
				return _current;
			}
		}

		private void Clamp()
		{
			Music = Math.Clamp(Music, 0, 10);
			SoundEffects = Math.Clamp(SoundEffects, 0, 10);
			Voices = Math.Clamp(Voices, 0, 10);
			BattleMode = Math.Clamp(BattleMode, 0, 1);
			BattleSpeed = Math.Clamp(BattleSpeed, 1, 6);
			WindowDesign = Math.Clamp(WindowDesign, 1, 6);
		}

		public void Save()
		{
			Clamp();
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
				File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "ff4 settings: not written: " + ex.Message); }
		}

		/// <summary>The volumes into the game's sound (opt.SoundOption: 0..127).</summary>
		public void Apply()
		{
			Clamp();
			try
			{
				GlobalScope.opt.COptionManager.getSingleton().soundOption().setBgmVolume(Music * 127 / 10);
				GlobalScope.opt.COptionManager.getSingleton().soundOption().setSeVolume(SoundEffects * 127 / 10);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "ff4 settings: volumes: " + ex.Message); }
		}

		/// <summary>Battle Speed for the battle: the command line's for a run, else the setting.</summary>
		public static int BattleSpeedNow => int.TryParse(Options.Get("ff4-battle-speed"), out int s) ? Math.Clamp(s, 1, 6) : Current.BattleSpeed;

		/// <summary>Wait mode for the battle: the command line's for a run, else the setting.</summary>
		public static bool WaitNow => Options.Get("ff4-battle-wait") != null || Current.BattleMode == 1;
	}
}
