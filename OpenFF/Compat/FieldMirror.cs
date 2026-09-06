// FF4's overworld is FF3's chip layout mirrored along z.
//
// Both games build a field from a grid of chips (f00_XY.flsc, X and Z in hex), each chip's
// model and collision centred on its own origin, and the stage manager places chip (X, Z)
// at size * (spot - centre). The FF3 logic ports that placement as written. FF4's engine
// places the same chips with z negated: measured 2026-09-04 over the 33 world-map arrivals
// FF4's scripts declare (setInside/OutsideMapJump ... "f00" ...), 31 land on grass or
// ground and none on walls or sea when z is negated, against 19 on sea with FF3's placement.
// So on an FF4 field stage the terrain is mirrored, and every position the game holds -
// arrivals, casts, exits, scripts - stays exactly as FF4 wrote it. The mirror lives in
// four places: the chip's world position (stg.getMidChipData / getChipData), the chip's
// geometry (MirrorModel: the built vertices and node offsets, done in the data so every
// matrix stays a rotation; the cull face is swapped for a mirrored model because a mirror
// reverses winding), the stage world matrix that collision queries go through
// (stg.getWldMtx), and the spot lookups that stream chips under the leader (stg.getSpot /
// getRelativeSpot, JumpPart.WithChip). The world's edges swap sides with it
// (getEdgeMin/Max) so the loop wraps at the right z.
//
// The camera goes with it (wld.CBaseSystem.setupCamera): FF4's overworld camera stands on
// the far side of the party, looking towards +z. That is not a matter of taste: the chips'
// mountains and forests are quads baked with a 45-degree tilt towards that camera, so from
// FF3's side of the party they are seen edge-on - the relief "flattens" into snow-coloured
// slivers in the grass, which is what a mirrored chip looked like until the camera moved.
//
// --fieldmirror=off turns it off for FF4 (positions then land on FF3's layout);
// --fieldmirror=force turns it on for FF3, which renders FF3's world mirrored - the check
// that mirrored chips draw right independently of FF4's data.

namespace OpenFF.Client
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
		public static uint CullFor(GlobalScope.MtxFx43 m, bool mirroredModel = false)
		{
			bool mirroredMatrix = false;
			if (m != null)
			{
				long a = m._00, b = m._01, c = m._02;
				long d = m._10, e = m._11, f = m._12;
				long g = m._20, h = m._21, i = m._22;
				long det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
				mirroredMatrix = det < 0;
			}
			// A mirrored model under a mirroring matrix winds the right way again.
			_drawMirrored = mirroredMatrix != mirroredModel;
			return _drawMirrored ? 1028u : 1029u;
		}

		private static bool _pivotWarned;

		/// <summary>
		/// Mirrors a loaded model's geometry along z: every shape's built vertex list, and
		/// every node's translation and rotation (a rotation R becomes M R M for the mirror
		/// M, which negates the four elements with one z index). Vertices are what the
		/// display lists were pre-built into at load, so this runs once per loaded chip.
		/// Rotations stored in the DS's pivot form are left alone with a note; FF4's chips
		/// carry none (their nodes are translations only).
		/// </summary>
		public static void MirrorModel(GlobalScope.NNSG3dResMdl mdl, string name)
		{
			if (mdl == null || mdl.mirroredZ)
			{
				return;
			}
			mdl.mirroredZ = true;
			if (mdl.shp?.shp != null)
			{
				foreach (GlobalScope.NNSG3dResShpData shape in mdl.shp.shp)
				{
					float[] z = shape?.cmd_vertex_z;
					if (z == null)
					{
						continue;
					}
					for (int i = 0; i < z.Length; i++)
					{
						z[i] = -z[i];
					}
				}
			}
			if (mdl.nodeInfo?.p == null)
			{
				return;
			}
			foreach (uint[] p in mdl.nodeInfo.p)
			{
				if (p == null || p.Length < 16)
				{
					continue;
				}
				uint flag = p[0] & 0xFFFF;
				int at = 1;
				if ((flag & 1) == 0)
				{
					p[3] = (uint)(-(int)p[3]);
					at += 3;
				}
				if ((flag & 2) == 0)
				{
					if ((flag & 8) == 0)
					{
						// m01,m02 | m10,m11 | m12,m20 | m21,m22 as fx16 pairs; m00 rides in p[0].
						p[at] = (p[at] & 0xFFFFu) | ((uint)(ushort)(-(short)(p[at] >> 16)) << 16);          // m02
						p[at + 2] = ((uint)(ushort)(-(short)(p[at + 2] & 0xFFFF))) | ((uint)(ushort)(-(short)(p[at + 2] >> 16)) << 16); // m12, m20
						p[at + 3] = (p[at + 3] & 0xFFFF0000u) | (uint)(ushort)(-(short)(p[at + 3] & 0xFFFF));   // m21
					}
					else if (!_pivotWarned)
					{
						_pivotWarned = true;
						Log.Write(LogChannel.General, "field mirror: " + name + " has a node with a pivot-form rotation, left unmirrored");
					}
				}
			}
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
