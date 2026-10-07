// Skipping one of FF4's story scenes, as Steam's FF4.exe offers it: while a scene allows it (ce_SetSkip 1 or 2), Esc, a
// click or the L button asks "Skip this scene?" (EventConteManager::execute opens its widget 0x1a on a tap or L). Yes
// fades both screens out over 15 frames (state 3); once black the scene runs through, the commands that look at the
// skip flag passing over their work and waits (Ff4Cutscene.PassedWhileSkipping), to its end or to a ce_StopSkip in
// state 1 - then the field, a battle or the next map comes in as the scene would have left it. No, Backspace or Esc
// again closes the question and the scene goes on; it ran on beneath it, as it does in libff4.
//
// The question is drawn the client's way - FF4's window and glove - not as Steam's widget.

using System;
using Microsoft.Xna.Framework.Input;

namespace OpenFF.Client
{
	internal sealed class SceneSkip : GameService
	{
		/// <summary>Whether the question is up: the game's input is withheld meanwhile (DesktopInput.IsTyping).</summary>
		public static bool PromptOpen { get; private set; }

		/// <summary>A scene that may be skipped is running: its Esc asks to skip, not to open the pause menu.</summary>
		public static bool Skippable =>
			GameProfile.IsFf4 && Ff4Cutscene.Active && (Ff4Cutscene.EventSkip == 1 || Ff4Cutscene.EventSkip == 2) && !Ff4Cutscene.EventSkipping;

		private const int PadA = 1, PadB = 2, PadRight = 16, PadLeft = 32, PadUp = 64, PadDown = 128, PadL = 512;

		private bool _yes = true;
		private int _padWas;
		private bool _escWas, _mouseWas;

		// The question's window and its two answers, in the 800 x 480 draw space.
		private const float BoxW = 300f, BoxH = 116f;
		private static float BoxX => (DrawList.ScreenWidth - BoxW) / 2f;
		private static float BoxY => (DrawList.ScreenHeight - BoxH) / 2f;
		private static float YesX => BoxX + 92f;
		private static float NoX => BoxX + 192f;
		private static float AnswerY => BoxY + 74f;

		public override bool WantsUpdate => true;

		public override void OnUpdate()
		{
			if (!GameProfile.IsFf4) return;

			// The input, read here whatever the game is withholding: the keys as the pad's bits, Esc, the mouse.
			Microsoft.Xna.Framework.Game game = null;
			try { game = GlobalScope.m_Graphics.getGame(); } catch (Exception) { }
			bool active = game != null && game.IsActive;
			int pad = DesktopInput.RawPadBits(defaultPad: true);
			bool esc = (active && Keyboard.GetState().IsKeyDown(Keys.Escape)) || DesktopInput.Injected.Contains(Keys.Escape);
			MouseState mouse = Mouse.GetState();
			bool inWindow = false;
			float mx = 0, my = 0;
			if (game != null)
			{
				Microsoft.Xna.Framework.Rectangle client = game.Window.ClientBounds;
				inWindow = active && mouse.X >= 0 && mouse.Y >= 0 && mouse.X < client.Width && mouse.Y < client.Height;
				if (client.Width > 0 && client.Height > 0)
				{
					mx = mouse.X * DrawList.ScreenWidth / client.Width;
					my = mouse.Y * DrawList.ScreenHeight / client.Height;
				}
			}
			bool click = inWindow && mouse.LeftButton == ButtonState.Pressed;
			int edge = pad & ~_padWas;
			bool escEdge = esc && !_escWas, clickEdge = click && !_mouseWas;
			_padWas = pad;
			_escWas = esc;
			_mouseWas = click;

			// A skip asked: once both screens are black the scene runs through.
			if (Ff4Cutscene.EventSkip == 3 && !Ff4Cutscene.EventSkipping
				&& GlobalScope.dgs.CFade.Main().isFaded() && GlobalScope.dgs.CFade.Sub().isFaded())
			{
				Ff4Cutscene.EventSkipping = true;
				Log.Write(LogChannel.General, "scene skip: running through " + (Ff4Cutscene.SceneStage ?? "the scene"));
			}

			if (!PromptOpen)
			{
				if (Skippable && (escEdge || clickEdge || (edge & PadL) != 0))
				{
					PromptOpen = true;
					_yes = true;
					Se(3);
				}
				return;
			}
			if (!Skippable)
			{
				PromptOpen = false;   // the scene ended, or stopped allowing it, under the question
				return;
			}

			bool overYes = Over(mx, my, YesX), overNo = Over(mx, my, NoX);
			if ((edge & (PadLeft | PadRight | PadUp | PadDown)) != 0) { _yes = !_yes; Se(3); }
			else if (click && (overYes || overNo) && overYes != _yes) _yes = overYes;
			if ((edge & PadA) != 0 || (clickEdge && (overYes || overNo)))
			{
				if (_yes) Skip();
				else Se(2);
				PromptOpen = false;
				return;
			}
			if ((edge & PadB) != 0 || escEdge || (clickEdge && !InBox(mx, my)))
			{
				Se(2);
				PromptOpen = false;
				return;
			}
			Draw(Game.Draw);
		}

		private static bool Over(float x, float y, float answerX) => x >= answerX - 34f && x <= answerX + 60f && y >= AnswerY - 6f && y <= AnswerY + 30f;
		private static bool InBox(float x, float y) => x >= BoxX && x <= BoxX + BoxW && y >= BoxY && y <= BoxY + BoxH;

		/// <summary>Yes: both screens out to black over 15 frames (state 3) and the scene's voice cut; the run-through
		/// starts once they are black.</summary>
		private static void Skip()
		{
			Se(1);
			Ff4Cutscene.EventSkip = 3;
			GlobalScope.dgs.CFade.Main().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			Ff4Cutscene.StopSceneVoice();
			Log.Write(LogChannel.General, "scene skip: asked on " + (Ff4Cutscene.SceneStage ?? "a scene"));
		}

		private void Draw(DrawList d)
		{
			const int size = 9;
			d.Rect(0, 0, DrawList.ScreenWidth, DrawList.ScreenHeight, new Color(0, 0, 0, 90));
			if (!Ff4Ui.Window(d, BoxX, BoxY, BoxW, BoxH)) d.Rect(BoxX, BoxY, BoxW, BoxH, new Color(20, 34, 74, 230));
			string question = "Skip this scene?";
			Shadowed(d, question, BoxX + (BoxW - d.MeasureText(question, size)) / 2f, BoxY + 22f, size);
			Shadowed(d, "Yes", YesX, AnswerY, size);
			Shadowed(d, "No", NoX, AnswerY, size);
			Ff4Ui.Glove(d, (_yes ? YesX : NoX) - 10f, AnswerY + size * 1.05f);
		}

		private static void Shadowed(DrawList d, string text, float x, float y, int size)
		{
			d.Text(text, x + 1.5f, y + 1.5f, new Color(0, 0, 0, 255), size);
			d.Text(text, x, y, Color.White, size);
		}

		private static void Se(int number)
		{
			try { GlobalScope.MatrixSound.MtxSENDS_Play(0, number, 127, 64); } catch (Exception) { }
		}
	}
}
