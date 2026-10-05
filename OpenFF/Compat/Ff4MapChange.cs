// How FF4 leaves and enters a map. Its world has no area-change shutter (FF3's AreaChange, the black bars that close
// on the middle of the screen and open from it): WSFadeOutProcess fades both screens out over 15 frames (black, unless
// CustomFadeSetting said otherwise - the end states' custom branch already does that), and WSPrepare enters a map with
// "common fadein process" (WSFadeInProcess, both screens in over 15 frames) - unless the map's own script started an
// event on arrival (CEventManager's event flag, set by EventStart), when it goes straight to "field event" and the
// event fades in itself. A story scene called from an event (conteEventJumpAndReturnMapJamp, WSFieldEvent ->
// WSCallConteEvent) changes part at once, no fade at all: a new game passes Baron's throne room (t00_00), whose
// script calls the opening's first scene, still white from the title, and the scene's FadeIn brings it in from white.

namespace OpenFF.Client
{
	internal static class Ff4MapChange
	{
		private static bool _conte;
		private static bool _entering;

		/// <summary>The map jump about to be made calls a story scene (Ff4Cutscene's conteEventJumpAndReturnMapJamp).</summary>
		public static void ConteJump() => _conte = true;

		/// <summary>A field or town is left by a map jump (CStateFieldEnd / CStateTownEnd), in place of closing the shutter.</summary>
		public static void Leave()
		{
			if (_conte) return;
			GlobalScope.dgs.CFade.Main().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
			GlobalScope.dgs.CFade.Sub().fadeOut(15, GlobalScope.dgs.CFade.FADE_TYPE.FADE_TYPE_BLACK);
		}

		/// <summary>Whether the map is left: at once for a story scene, else when both screens have faded.</summary>
		public static bool Left => _conte || (GlobalScope.dgs.CFade.Main().isFaded() && GlobalScope.dgs.CFade.Sub().isFaded());

		/// <summary>A field or town is entered (CStateFieldStart / CStateTownStart's start), in place of opening the shutter.</summary>
		public static void Enter()
		{
			_conte = false;
			_entering = true;
			GlobalScope.wld.AreaChange.getInstance().setOpenStrong();
		}

		/// <summary>The start state's update, the map's scripts having run once: whether the entry is over.</summary>
		public static bool Entered()
		{
			if (_entering)
			{
				_entering = false;
				if (GlobalScope.evt.CEventManager.getInstance().isEvent() || Ff4Cutscene.Active) return true;
				GlobalScope.dgs.CFade.Main().fadeIn(15);
				GlobalScope.dgs.CFade.Sub().fadeIn(15);
			}
			return GlobalScope.dgs.CFade.Main().isCleared() && GlobalScope.dgs.CFade.Sub().isCleared();
		}
	}
}
