// Batch-level geometry diagnostics for the GL emulation.
//
// Logging one vertex tells you nothing about a 4887-vertex mesh: the first vertex of
// a map is usually a corner that is legitimately off screen. This projects the whole
// batch and reports how much of it actually lands inside the clip volume, which
// separates "the transform is wrong" from "the transform is fine, something else is
// eating the pixels".

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenFF.Client
{
	internal static class GlDiag
	{
		private static int _burst;

		/// <summary>Arms a full (unsampled) dump of the next <paramref name="draws"/> draw calls.</summary>
		public static void ArmBurst(int draws)
		{
			_burst = draws;
			Log.Write(LogChannel.General, "--- draw burst armed: next " + draws + " draws ---");
		}

		/// <summary>True while a burst is running; consumes one draw.</summary>
		public static bool Burst()
		{
			if (_burst <= 0)
			{
				return false;
			}
			_burst--;
			return true;
		}

		/// <summary>NDC bounds and in-frustum vertex count for one draw batch.</summary>
		public static string Describe(VertexPositionColorTexture[] v, int first, int count,
			Matrix world, Matrix view, Matrix projection)
		{
			if (v == null || count <= 0 || first < 0 || first + count > v.Length)
			{
				return "batch=<invalid>";
			}

			Matrix wvp = Matrix.Multiply(Matrix.Multiply(world, view), projection);

			float minX = float.MaxValue, maxX = float.MinValue;
			float minY = float.MaxValue, maxY = float.MinValue;
			float minZ = float.MaxValue, maxZ = float.MinValue;
			int inside = 0, behind = 0;

			for (int i = first; i < first + count; i++)
			{
				Vector4 clip = Vector4.Transform(new Vector4(v[i].Position, 1f), wvp);
				if (clip.W <= 0f)
				{
					behind++;
					continue;
				}
				float x = clip.X / clip.W;
				float y = clip.Y / clip.W;
				float z = clip.Z / clip.W;

				if (x < minX) minX = x;
				if (x > maxX) maxX = x;
				if (y < minY) minY = y;
				if (y > maxY) maxY = y;
				if (z < minZ) minZ = z;
				if (z > maxZ) maxZ = z;

				if (x >= -1f && x <= 1f && y >= -1f && y <= 1f && z >= -1f && z <= 1f)
				{
					inside++;
				}
			}

			if (inside == 0 && behind == count)
			{
				return string.Format(CultureInfo.InvariantCulture,
					"ndc=<all {0} verts behind camera>", behind);
			}

			return string.Format(CultureInfo.InvariantCulture,
				"ndc x[{0:F2}..{1:F2}] y[{2:F2}..{3:F2}] z[{4:F2}..{5:F2}] inside={6}/{7} behind={8}",
				minX, maxX, minY, maxY, minZ, maxZ, inside, count, behind);
		}
	}
}
