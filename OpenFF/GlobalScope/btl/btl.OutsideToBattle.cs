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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
	public static partial class btl
	{
		public class OutsideToBattle
		{
			public static OutsideToBattle instance_ = new OutsideToBattle();

			private BATTLE_OPENING_TYPE openingType_;

			private BATTLE_CAMERA battleCamera_;

			private BATTLE_TYPE battleType_;

			private InitializeBattleMap map_ = new InitializeBattleMap();

			private InitializeMonster monster_ = new InitializeMonster();

			private InitializePlayer player_ = new InitializePlayer();

			private InitializeNPC npc_ = new InitializeNPC();

			private bool escape_;

			private bool condition_;

			private bool restart_;

			private bool transfix_;

			private bool magicDefenseInvalidation_;

			public void initialize()
			{
				setBattleOpeningType(BATTLE_OPENING_TYPE.CALC_BATTLE_OPENING_TYPE);
				setBattleType(BATTLE_TYPE.NORMAL_BATTLE);
				map_.initialize();
				monster_.initialize();
				player_.initialize();
				npc_.initialize();
				offEscape();
				offCondition();
				offRestart();
				offTransfix();
				offMagicDefenseInvalidation();
			}

			public OutsideToBattle()
			{
				battleCamera_ = BATTLE_CAMERA.OPENING_CAMERA;
				escape_ = false;
				condition_ = false;
				restart_ = false;
				transfix_ = false;
			}

			public static OutsideToBattle getInstance()
			{
				return instance_;
			}

			public void initializeBattleCamera()
			{
				battleCamera_ = BATTLE_CAMERA.OPENING_CAMERA;
			}

			public void setBattleCamera(BATTLE_CAMERA camera)
			{
				battleCamera_ = camera;
			}

			public BATTLE_CAMERA battleCamera()
			{
				return battleCamera_;
			}

			public bool isFreeMode()
			{
				if (battleCamera_ != BATTLE_CAMERA.FREE_CAMERA)
				{
					return false;
				}
				return true;
			}

			public void setBattleType(BATTLE_TYPE type)
			{
				battleType_ = type;
			}

			public BATTLE_TYPE battleType()
			{
				return battleType_;
			}

			public void setBattleOpeningType(BATTLE_OPENING_TYPE type)
			{
				openingType_ = type;
			}

			public BATTLE_OPENING_TYPE battleOpeningType()
			{
				return openingType_;
			}

			public void setInitializeBattleMap(InitializeBattleMap map)
			{
				map_ = map;
			}

			public InitializeBattleMap initializeBattleMap()
			{
				return map_;
			}

			public void setInitializeMonster(InitializeMonster monster)
			{
				monster_ = monster;
			}

			public InitializeMonster initializeMonster()
			{
				return monster_;
			}

			public void setInitializePlayer(InitializePlayer player)
			{
				player_ = player;
			}

			public InitializePlayer initializePlayer()
			{
				return player_;
			}

			public void setInitializeNPC(InitializeNPC npc)
			{
				npc_ = npc;
			}

			public InitializeNPC initializeNPC()
			{
				return npc_;
			}

			public void onEscape()
			{
				escape_ = true;
			}

			public void offEscape()
			{
				escape_ = false;
			}

			public bool escape()
			{
				return escape_;
			}

			public void onCondition()
			{
				condition_ = true;
			}

			public void offCondition()
			{
				condition_ = false;
			}

			public bool condition()
			{
				return condition_;
			}

			public void onRestart()
			{
				restart_ = true;
			}

			public void offRestart()
			{
				restart_ = false;
			}

			public bool restart()
			{
				return restart_;
			}

			public void onTransfix()
			{
				transfix_ = true;
			}

			public void offTransfix()
			{
				transfix_ = false;
			}

			public bool transfix()
			{
				return transfix_;
			}

			public void onMagicDefenseInvalidation()
			{
				magicDefenseInvalidation_ = true;
			}

			public void offMagicDefenseInvalidation()
			{
				magicDefenseInvalidation_ = false;
			}

			public bool magicDefenseInvalidation()
			{
				return magicDefenseInvalidation_;
			}
		}
	}
}
