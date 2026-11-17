// The field's footstep effects (createEffectTaskWait / Walk / Run: world::WTAlarm{Waiting,Walking,Running}AttrEffect,
// world tasks named "attr se"): every so many frames, while the cast stands on ground of the kind given (landFormParameter(0)
// of its land form) and is waiting, walking or running (CharacterObject +0x34c: bit 1 moving, bit 0 dashing - WSMove's), the
// effect is made at its feet plus the offset. The global script sets them up as Baron, Kaipo, Mysidia, Troia, the Mist cave
// and the others it names are entered (e803's splashes: kind 65, 30 / 15 / 5 frames apart); WSMapJump deletes them with the
// map (WTAttrEffectManager::deleteAttrEffectTask).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4FootEffects
	{
		private sealed class Task
		{
			public int Mode;          // 0 waiting, 2 walking, 3 running
			public uint Cast;
			public int Kind, Dx, Dy, Dz, Effect, Member, Every, Count;
		}

		private static readonly List<Task> _tasks = new List<Task>();
		private static GlobalScope.VecFx32 _last;

		public static GlobalScope.SCRIPT_COMMAND Command(int mode) => engine =>
		{
			Task t = new Task { Mode = mode, Cast = engine.getWord() };
			t.Kind = (int)engine.getDword();
			t.Dx = (int)engine.getDword(); t.Dy = (int)engine.getDword(); t.Dz = (int)engine.getDword();
			t.Effect = (int)engine.getDword(); t.Member = (int)engine.getDword(); t.Every = (int)engine.getDword();
			_tasks.Add(t);
		};

		/// <summary>A map set up (the map jump deletes the tasks).</summary>
		public static void Clear() { _tasks.Clear(); _last = null; }

		/// <summary>Each game step (LegacyStep).</summary>
		public static void Tick()
		{
			if (_tasks.Count == 0) return;
			string map = GlobalScope.stg.CStageMng.CurrentName;
			GlobalScope.pl.CBasePlayer hero = EngineApi.HeroPlayer;
			if (hero == null) return;
			GlobalScope.VecFx32 at = hero.getPosition();
			bool moving = _last != null && (at.x != _last.x || at.z != _last.z);
			_last = new GlobalScope.VecFx32(at.x, at.y, at.z);
			int mode = !moving ? 0 : GlobalScope.pl.isRun() ? 3 : 2;
			int kind = -2;
			foreach (Task t in _tasks)
			{
				if (t.Count-- >= 1) continue;
				t.Count = t.Every;
				if (t.Cast != 0 || t.Mode != mode || Ff4Cutscene.Active) continue;
				if (kind == -2) kind = KindUnderHero(map);
				if (kind != t.Kind) continue;
				try
				{
					int effect = GlobalScope.eff.CEffectMng.instance().create(t.Effect, t.Member);
					if (effect != -1) GlobalScope.eff.CEffectMng.instance().setPosition(effect, new GlobalScope.VecFx32(at.x + t.Dx, at.y + t.Dy, at.z + t.Dz));
				}
				catch (Exception ex) { Log.First(LogChannel.General, "foot-effect", 3, () => "foot effect: " + ex.Message); }
			}
		}

		private static int KindUnderHero(string map)
		{
			int land = Ff4Encounters.LandUnderHero();
			Ff4Encounters.Table table = Ff4Encounters.For(map);
			return land >= 0 && land < 12 && table != null ? table.RawRates[land] : -1;
		}
	}
}
