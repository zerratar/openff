// The hero the story treats as its lead (Game.Party.Protagonist).
//
// FF3's scripts name the four heroes by their cast numbers - PC casts 5..8 are heroes 0..3,
// %shuyaku1 in a line is hero 0's name - and the opening is written for hero 0: Luneth falls
// into the Altar Cave, Luneth's figure is booted for the scene, Luneth joins the party. A
// game played by several people, one hero each, wants the opening for whoever was picked.
// So the story's references go through here: with a protagonist P set, the script's hero 0
// is P and its hero P is 0 - a swap, so every hero still has one story role - and the rest
// stay as they are. Null is the game as written.

namespace OpenFF.Client
{
	internal static class StoryCast
	{
		private static int _protagonist = -1;

		/// <summary>The hero (0..3) the story's hero 0 stands for, or -1 for the game's own.</summary>
		public static int Protagonist
		{
			get => _protagonist;
			set
			{
				_protagonist = value >= 0 && value < 4 ? value : -1;
				Log.Write(LogChannel.General, "story: the lead is hero " + (_protagonist < 0 ? "0, as written" : _protagonist.ToString()));
			}
		}

		/// <summary>A script's hero number as the hero record it means now.</summary>
		public static byte Hero(byte scriptHero)
		{
			if (_protagonist <= 0) return scriptHero;
			if (scriptHero == 0) return (byte)_protagonist;
			if (scriptHero == _protagonist) return 0;
			return scriptHero;
		}

		public static uint Hero(uint scriptHero) => scriptHero < 4 ? Hero((byte)scriptHero) : scriptHero;
		public static int Hero(int scriptHero) => scriptHero >= 0 && scriptHero < 4 ? Hero((byte)scriptHero) : scriptHero;
	}
}
