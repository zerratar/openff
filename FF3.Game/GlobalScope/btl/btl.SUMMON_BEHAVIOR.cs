using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using android.text;
using android.widget;
using java.io;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public enum SUMMON_BEHAVIOR
		{
			SET_DARK_SCREEN,
			IS_DARK_SCREEN,
			SET_NORMAL_SCREEN,
			IS_NORMAL_SCREEN,
			SET_FADE_OUT,
			IS_FADE_OUT,
			SET_FADE_IN,
			IS_FADE_IN,
			SET_MODEL,
			IS_MODEL,
			SET_MOTION,
			IS_MOTION,
			START_MOTION,
			IS_MOTION_FRAME,
			DEL_SUMMON,
			REGISTER_PLAYERS,
			SET_EFFECT,
			CLEAR_EFFECT,
			IS_EFFECT,
			DRAW_SUMMON_EFFECT,
			DRAW_SUMMON_EFFECT_TARGET_ALL,
			IS_END_SUMMON_EFFECT,
			CHANGE_CAMERA,
			SET_CAMERA_POSITION,
			SET_CAMERA_TARGET,
			SET_MOVE_CAMERA_FRAME,
			MOVING_CAMERA,
			SET_BATTLE_CAMERA,
			SET_SUMMON_PARAMETER,
			SET_POSITION_AND_ROTATION,
			SHOW_MONSTERS,
			HIDE_MONSTERS,
			SHOW_SUMMON,
			HIDE_SUMMON,
			SUMMON_ALPHA_RATE,
			SET_PLAYERS_ALPHA,
			APPEAR_PLAYERS,
			SET_TURN_FLAG,
			IS_TURN_FLAG,
			MOVE_CAMERA_AND_SUMMON_ALPHA,
			FRAME_COUNT,
			SET_2D,
			IS_2D,
			DEAD_CHARACTERS,
			SUMMON_BEHAVIOR_END,
			SHAKE_CAMERA,
			ROTATE_CHARACTER,
			READY_MOVE_CHARACTER,
			MOVE_CHARACTER,
			SET_FLASH,
			READY_AUTO_CAMERA,
			IS_AUTO_CAMERA,
			DRAW_TARGET_EFFECT,
			IS_SHAKE_CAMERA,
			SET_SHOW_PLAYER_WINDOW,
			CRAETE_EFFECT_AND_SET_POSITION,
			IS_END_MONSTER_EFFECT,
			IS_END_PLAYER_EFFECT,
			SET_MONSTERS_ALPHA,
			APPEAR_MONSTERS,
			DISAPPEAR_MONSTERS,
			SET_SE,
			PLAY_SE,
			CLEAR_SE,
			SUMMON_BEHAVIOR_MAX
		}
	}
}
