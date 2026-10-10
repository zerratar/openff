// The field camera while FF4's menu is open, as Steam's: it closes in on the lead - the eye 8 right, 32 down and 36
// nearer, the point it looks at 9 right, 5 down and 10 nearer (world units, traced with ff4hook at the Underground
// Waterway: eye (0,104,67) -> (8,72,31), target (0,51,-13) -> (9,46,-23)) - going half the way left each step, and back
// the same way when the menu shuts. The game's camera is left as it is; NNS_G3dGlbLookAt takes the moved one.

using System;

namespace OpenFF.Client
{
	internal static class Ff4MenuCamera
	{
		private static float _near;   // 0 the field's own view .. 1 the menu's
		private static readonly GlobalScope.VecFx32 _eye = new GlobalScope.VecFx32();
		private static readonly GlobalScope.VecFx32 _at = new GlobalScope.VecFx32();

		/// <summary>One step toward the menu's view (open) or the field's: half the way left.</summary>
		public static void Step(bool open)
		{
			float to = open ? 1f : 0f;
			_near += (to - _near) * 0.5f;
			if (Math.Abs(to - _near) < 1f / 4096f) _near = to;
		}

		/// <summary>The eye and target moved toward the menu's view (the field only), for NNS_G3dGlbLookAt.</summary>
		public static void Take(ref GlobalScope.VecFx32 camPos, ref GlobalScope.VecFx32 target)
		{
			if (_near <= 0f || !GameProfile.IsFf4 || camPos == null || target == null || Ff4Battle.Active) return;
			_eye.x = camPos.x + (int)(8 * 4096 * _near);
			_eye.y = camPos.y - (int)(32 * 4096 * _near);
			_eye.z = camPos.z - (int)(36 * 4096 * _near);
			_at.x = target.x + (int)(9 * 4096 * _near);
			_at.y = target.y - (int)(5 * 4096 * _near);
			_at.z = target.z - (int)(10 * 4096 * _near);
			camPos = _eye;
			target = _at;
		}
	}
}
