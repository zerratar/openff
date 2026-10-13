// FF4's field effect commands that place an effect and keep it placed: playEffectEx (an effect at a point, turned),
// effectFollowCamera (eff::CEffectFollow's camera kind: held in front of the camera) and
// createCharacterFollowEffectTask (world::WTFollowCharacterEffect: kept on a cast's feet, offset).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4FieldEffects
	{
		private sealed class Follow
		{
			public int Effect;
			public int Ctrl = -1;              // a character to follow; -1 the camera
			public int X, Y, Z;                // the offset (fx32): the cast's world axes, or the camera's right, up and forward
		}

		private static readonly List<Follow> _follows = new List<Follow>();

		private static GlobalScope.eff.CEffectMng Effects => GlobalScope.eff.CEffectMng.instance();

		/// <summary>playEffectEx(category, number, index, x, y, z, rx, ry, rz, ?): the effect made, placed and turned. The turns
		/// go through libff4's sum as it is (FX_Div by 1.0, times 0x10000 in 32 bits, over 360).</summary>
		public static void PlayEffectEx(GlobalScope.ScriptEngine engine)
		{
			int category = (short)engine.getWord(), number = (short)engine.getWord(), index = (int)engine.getWord();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			int rx = (int)engine.getDword(), ry = (int)engine.getDword(), rz = (int)engine.getDword();
			engine.getDword();
			Ff4Cutscene.Guard("effect ex", () =>
			{
				int effect = Effects.create(category, number);
				if (effect == -1) return;
				Ff4EffectIndex.Set(index, effect);
				GlobalScope.VecFx32 at = new GlobalScope.VecFx32();
				at.set(x, y, z);
				Effects.setPosition(effect, at);
				Effects.setRotation(effect, Turn(rx), Turn(ry), unchecked(rz * 0x10000) / 0x168 & 0xffff);
				Effects.enableBoxCulling(effect, false);
			});
		}

		private static int Turn(int degrees) => unchecked(degrees * 0x10000) / 360 & 0xffff;

		/// <summary>effectFollowCamera(category, number, index, x, y, z): the effect made and held at the camera, offset along
		/// its right, up and forward.</summary>
		public static void EffectFollowCamera(GlobalScope.ScriptEngine engine)
		{
			int category = (short)engine.getWord(), number = (short)engine.getWord(), index = (sbyte)engine.getWord();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			Ff4Cutscene.Guard("effect at camera", () =>
			{
				int effect = Effects.create(category, number);
				if (effect == -1) return;
				Ff4EffectIndex.Set(index, effect);
				Effects.enableBoxCulling(effect, false);
				_follows.Add(new Follow { Effect = effect, X = x, Y = y, Z = z });
				TickOne(_follows[_follows.Count - 1]);
			});
		}

		/// <summary>createCharacterFollowEffectTask(cast, index, x, y, z, ?, ?): a made effect kept at the cast's position plus
		/// the offset while it plays.</summary>
		public static void CharacterFollowEffect(GlobalScope.ScriptEngine engine)
		{
			uint cast = engine.getWord();
			int index = (int)engine.getDword();
			int x = (int)engine.getDword(), y = (int)engine.getDword(), z = (int)engine.getDword();
			engine.getDword();
			engine.getDword();
			int slot = GlobalScope.CCastCommandTransit.getInstance().changeHichNumber(cast & 0xffff);
			if (slot == -1) return;
			int ctrl = EngineApi.Players.Player(slot)?.getCharacterId() ?? -1;
			int effect = Ff4EffectIndex.Object(index);
			if (ctrl < 0 || effect < 0) return;
			_follows.Add(new Follow { Effect = effect, Ctrl = ctrl, X = x, Y = y, Z = z });
		}

		/// <summary>Each frame: the followers placed, those whose effect has ended let go.</summary>
		public static void Tick()
		{
			for (int i = _follows.Count - 1; i >= 0; i--)
			{
				Follow f = _follows[i];
				bool live;
				try { live = Effects.isEffectObject(f.Effect) && (f.Ctrl < 0 || Effects.isPlay(f.Effect)); }
				catch (Exception) { live = false; }
				if (!live) { _follows.RemoveAt(i); continue; }
				try { TickOne(f); } catch (Exception) { }
			}
		}

		public static void Clear() => _follows.Clear();

		private static void TickOne(Follow f)
		{
			GlobalScope.VecFx32 at = new GlobalScope.VecFx32();
			if (f.Ctrl >= 0)
			{
				GlobalScope.characterMng.getPosition(f.Ctrl, at);
				at.set(at.x + f.X, at.y + f.Y, at.z + f.Z);
			}
			else
			{
				// executeCameraFollowEffect: the camera's position plus forward * z, up * y and (forward x up) * x, each made
				// unit length - read here from the camera matrix NNS_G3dGlbLookAt built (its x, y and -z axes).
				GlobalScope.VecFx32 position = GlobalScope.NNS_G3dGlbGetCameraPos();
				int[] m = GlobalScope.NNS_G3dGlbGetCameraMtx().a;
				double rx = m[0], ry = m[3], rz = m[6], ux = m[1], uy = m[4], uz = m[7], fx = -m[2], fy = -m[5], fz = -m[8];
				Unit(ref fx, ref fy, ref fz); Unit(ref ux, ref uy, ref uz); Unit(ref rx, ref ry, ref rz);
				at.set(position.x + (int)(fx * f.Z + ux * f.Y + rx * f.X),
					position.y + (int)(fy * f.Z + uy * f.Y + ry * f.X),
					position.z + (int)(fz * f.Z + uz * f.Y + rz * f.X));
			}
			Effects.setPosition(f.Effect, at);
		}

		private static void Unit(ref double x, ref double y, ref double z)
		{
			double length = Math.Sqrt(x * x + y * y + z * z);
			if (length <= 0) return;
			x /= length; y /= length; z /= length;
		}
	}
}
