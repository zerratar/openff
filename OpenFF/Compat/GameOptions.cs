// The game's own settings (the Config screen's), for the layouts' bindings: the "config" root the menus reach.
//
// A layout of the Config screen that draws its own sliders binds them here: the music and sound volumes as the
// game keeps them (0..127), as the screen shows them (0..10), and as percents; the other rows' settings as their
// choice's index (a choice itself is :checked while it is the one set).
//
//   <frame bind-style="width: {config.musicPercent}%"> ...                 <!-- a volume bar's fill -->
//   <frame bind-style="translate: {config.music * 14}px 0"> ...            <!-- its knob, 0..10 along 140 units -->

namespace OpenFF.Client
{
	internal sealed class GameOptions
	{
		public static readonly GameOptions Current = new GameOptions();

		private static GlobalScope.opt.COptionManager Options => GlobalScope.opt.COptionManager.getSingleton();

		/// <summary>The volumes as the game keeps them, 0..127.</summary>
		public int MusicLevel => Options.soundOption().bgmVolume();
		public int SoundLevel => Options.soundOption().seVolume();

		/// <summary>The volumes as the Config screen shows them, 0..10.</summary>
		public int Music => MusicLevel * 10 / 127;
		public int Sound => SoundLevel * 10 / 127;

		public int MusicPercent => MusicLevel * 100 / 127;
		public int SoundPercent => SoundLevel * 100 / 127;

		/// <summary>The other rows' settings, as the index of the choice set.</summary>
		public int TextSpeed => (int)Options.messageOption().messageSpeed();
		public int Cursor => (int)Options.cursorOption().cursorPosition();
		public int Move => (int)Options.gameOption().worldMoveType();
		public int BattleMenu => (int)Options.gameOption().menuZoomSetting();
	}
}
