// New Game's difficulty question as Steam's DifficultyPart: over black, the question window - "Select a difficulty level."
// (babil_menu.msd 50765), Normal and Hard (50766, 50767) with the hand starting on Normal, the note under them (50768) -
// Left / Right between the two, Enter picks one (the preferences' bit 12: Hard), Back goes back to the title. The title
// opens it (Ff4Menu.OpenDifficulty) and takes the answer; the damage formulas read it (Ff4Battle.NormalScale).

namespace OpenFF.Client
{
	internal sealed partial class Ff4Menu
	{
		private bool _hardLit;

		/// <summary>What the difficulty question came to: 0 while it is up, 1 Normal, 2 Hard, -1 back to the title.</summary>
		internal int DifficultyChosen { get; private set; }

		/// <summary>The title's NEW GAME: the difficulty question over the title, the hand on Normal (setCursor 0x10000).</summary>
		internal void OpenDifficulty()
		{
			_open = true;
			_mode = Mode.Browse;
			_question = false;
			_screen = Screen.Difficulty;
			_hardLit = false;
			DifficultyChosen = 0;
			_fromTitle = _loadOpening = true;
			Game.Input.Capture = true;
			Log.Write(LogChannel.File, "menu: Difficulty");
		}

		private void UpdateDifficulty(InputState input)
		{
			if (_loadOpening) { _loadOpening = false; return; }
			if (input.Pressed(Pad.Left) || input.Pressed(Pad.Right)) _hardLit = !_hardLit;
			if (input.Pressed(Pad.B) || input.KeyPressed("Escape")) { DifficultyChosen = -1; Close(); return; }
			if (!input.Pressed(Pad.A)) return;
			DifficultyChosen = _hardLit ? 2 : 1;
			Close();
		}

		private void DrawDifficulty()
		{
			Ff4MenuHud.Data h = _hud;
			h.Root = h.Picking = h.Bubble = h.Question = h.Full = false;
			h.Inventory = h.Magic = h.Status = h.Equipment = h.Abilities = h.Gambits = h.Party = h.Save = h.Settings = false;
			h.Difficulty = true;
			h.DifficultyText = T(50765, "Select a difficulty level.");
			h.NormalLabel = T(50766, "Normal");
			h.HardLabel = T(50767, "Hard");
			string[] note = Ff4Layouts.MenuPage(50768, "Game content is the same with Normal and Hard difficulty,\nexcept with Hard, enemies are more formidable.").Split('\n');
			h.DifficultyNote1 = note[0].Trim();
			h.DifficultyNote2 = note.Length > 1 ? note[1].Trim() : "";
			h.NormalLit = !_hardLit;
			h.HardLit = _hardLit;
			Ff4MenuHud.Draw(h);
			h.Difficulty = false;
		}
	}
}
