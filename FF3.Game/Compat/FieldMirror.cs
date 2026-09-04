// FF4's overworld is FF3's chip layout mirrored along z.
//
// Both games build a field from a grid of chips (f00_XY.flsc, X and Z in hex), each chip's
// model and collision centred on its own origin, and the stage manager places chip (X, Z)
// at size * (spot - centre). The FF3 logic ports that placement as written. FF4's engine
// places the same chips with z negated: measured 2026-09-04 over the 33 world-map arrivals
// FF4's scripts declare (setInside/OutsideMapJump ... "f00" ...), 31 land on grass or
// ground and none on walls or sea when z is negated, against 19 on sea with FF3's placement.
// So on an FF4 field stage the terrain is mirrored, and every position the game holds -
// arrivals, casts, exits, scripts, the camera - stays exactly as FF4 wrote it. The mirror
// lives in four places: the chip's world position (stg.getMidChipData / getChipData), the
// chip's render scale (z = -1, with the cull face swapped because a mirror reverses winding),
// the stage world matrix that collision queries go through (stg.getWldMtx), and the spot
// lookups that stream chips under the leader (stg.getSpot / getRelativeSpot, JumpPart.WithChip).
// The world's edges swap sides with it (getEdgeMin/Max) so the loop wraps at the right z.
//
// --fieldmirror=off turns it off for FF4 (positions then land on FF3's layout);
// --fieldmirror=force turns it on for FF3, which renders FF3's world mirrored - the check
// that mirrored chips draw right independently of FF4's data.

namespace FF3
{
	internal static class FieldMirror
	{
		/// <summary>Whether field stages of the running game are to be mirrored.</summary>
		public static bool WantsFieldMirror
		{
			get
			{
				string option = Options.Get("fieldmirror");
				if (option == "off")
				{
					return false;
				}
				return GameProfile.IsFf4 || option == "force";
			}
		}

		/// <summary>True when a stage of this type is a field (overworld) stage to mirror.</summary>
		public static bool Active(GlobalScope.stg.STAGE_TYPE type)
		{
			// A field stage's type is its number (f01 -> FIELD01); FF4's overworld is f00, type 0,
			// which stg promotes to FIELD01 so the field code runs for it.
			int t = (int)type;
			return WantsFieldMirror && t >= 0 && t <= (int)GlobalScope.stg.STAGE_TYPE.STAGE_TYPE_FIELD04;
		}

		private static bool _drawMirrored;

		/// <summary>
		/// The GL cull face for a model drawn under this matrix, called once at the start of
		/// its draw: a mirroring matrix (negative determinant) reverses triangle winding, so
		/// the faces the game's default cull removes are the front ones. 1029 = GL_BACK (the
		/// game's default), 1028 = GL_FRONT.
		/// </summary>
		public static uint CullFor(GlobalScope.MtxFx43 m)
		{
			if (m == null)
			{
				_drawMirrored = false;
				return 1029u;
			}
			long a = m._00, b = m._01, c = m._02;
			long d = m._10, e = m._11, f = m._12;
			long g = m._20, h = m._21, i = m._22;
			long det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
			_drawMirrored = det < 0;
			return _drawMirrored ? 1028u : 1029u;
		}

		/// <summary>
		/// A material's own cull face for the model being drawn, swapped while that model's
		/// matrix mirrors.
		/// </summary>
		public static uint ForDraw(uint materialCull)
		{
			if (!_drawMirrored)
			{
				return materialCull;
			}
			return materialCull == 1028u ? 1029u : (materialCull == 1029u ? 1028u : materialCull);
		}
	}
}
