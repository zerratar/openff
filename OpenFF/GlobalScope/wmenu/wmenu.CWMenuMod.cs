// PORT: the screen that plays a mod's menu (OpenFF.Client.ModMenus): one WMENU_KIND (15, past
// the game's fifteen) for every screen the mods define. The layout came in with MenuDefine.xbn
// (ModMenus merges the mods' <menu>s as the file loads), so buildMenu finds it by name like
// the game's own; the game's focus rules, cursor, windows and font do the rest. What the screen
// does - the texts it shows, what a press means - is the mod's MenuBehaviours, reached through
// ModMenus: opened, ticked, told of focus, presses, cancel and keys, closed.

using System;

internal static partial class GlobalScope
{
	public static partial class wmenu
	{
		public class CWMenuMod : CWMenuMemberBase
		{
			/// <summary>The kind the manager registers this screen under: one past the game's own.</summary>
			public const int KIND = (int)WMENU_KIND.WMENU_KIND_MAX;

			private string _focused;

			public override bool cSelectInitialize()
			{
				// A character pick first when the screen asks for one (as Status and Equip do).
				return OpenFF.Client.ModMenus.CurrentWantsCharacterSelect();
			}

			public override void initialize()
			{
				setFinalize(val: false);
				string screen = OpenFF.Client.ModMenus.CurrentScreenName();
				if (screen == null)
				{
					CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
					CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
					return;
				}
				CWMenuManager.Instance().SetPrimaryBG(OpenFF.Client.ModMenus.CurrentBackground());
				// The party's faces off (the main menu's panel), unless a hero was picked - then that one's, as Status shows it.
				for (int i = 0; i < 4; i++) CWMenuManager.Instance().SetShowPcFace(i, show: false);
				if (OpenFF.Client.ModMenus.CurrentWantsCharacterSelect())
				{
					try { CWMenuManager.Instance().SetShowPcFace(pl.PlayerParty.instance().player((byte)menu.MenuManager.getSingleton().GetTargetCharNo()).playerId(), show: true); } catch (Exception) { }
				}
				menu.MenuManager.getSingleton().ClearBehaviorButton();
				menu.MenuManager.getSingleton().buildMenu(screen);
				menu.MenuManager.getSingleton().initFocus(0);
				menu.MenuManager.getSingleton().GetCursor2d().SetShow(show: true);
				_focused = null;
				OpenFF.Client.ModMenus.ScreenOpened(this);
				CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_RUN);
			}

			public override void run()
			{
				menu.MenuManager mgr = menu.MenuManager.getSingleton();
				mgr.execute();
				// Focus moved: blur the old frame, focus the new.
				string now = mgr.getFocuseMedget()?._id();
				if (now != _focused)
				{
					OpenFF.Client.ModMenus.FocusChanged(_focused, now);
					_focused = now;
				}
				if (mgr.GetActivateButtonState() != 0)
				{
					if (mgr.GetDecideButtonState() == 0)
					{
						OpenFF.Client.ModMenus.Pressed(now);
					}
					else if (mgr.GetCancelButtonState() == 0 || CWMenuManager.Instance().GetMenuButton().TouchButtonB())
					{
						OpenFF.Client.ModMenus.Cancelled();
					}
				}
				int edge = ds.g_Pad.edge();
				if ((edge & 0x200) != 0) OpenFF.Client.ModMenus.Key(OpenFF.MenuKey.L);
				if ((edge & 0x100) != 0) OpenFF.Client.ModMenus.Key(OpenFF.MenuKey.R);
				if ((edge & TAB_PREV_BUTTON) != 0) OpenFF.Client.ModMenus.Key(OpenFF.MenuKey.X);
				if ((edge & TAB_NEXT_BUTTON) != 0) OpenFF.Client.ModMenus.Key(OpenFF.MenuKey.Y);
				OpenFF.Client.ModMenus.Tick();
				mgr.ClearBehaviorButton();
			}

			public override void terminate()
			{
				if (!isFinalize())
				{
					OpenFF.Client.ModMenus.ScreenClosed();
					menu.MenuManager.getSingleton().releaseWindowAll();
					menu.MenuManager.getSingleton().release();
					setFinalize(val: true);
				}
			}

			/// <summary>Leaves the screen: to the main menu, or out of the menus when it was opened from the field.</summary>
			public void Leave(bool toMainMenu)
			{
				if (toMainMenu)
				{
					CWMenuManager.Instance().SetNextKind(WMENU_KIND.WMENU_KIND_MAIN_MENU);
					CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
				}
				else
				{
					CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_END);
				}
			}

			/// <summary>Leaves for another screen of the mods' (ModMenus has set which).</summary>
			public void LeaveFor()
			{
				CWMenuManager.Instance().SetNextKind((WMENU_KIND)KIND);
				CWMenuManager.Instance().SetProcState(WMENU_PROCESS.WMENU_PROCESS_TERMINATE);
			}
		}
	}
}
