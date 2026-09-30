// A stage's animation bundle (.namp, beside a battle map's bNN.nmdp): read, and played a frame at a
// time as the port plays it, for Crystal's effect Stage (wwwroot/effects.js) to draw the map moving.
//
// The file (OpenFF/GlobalScope/ds/sys3d/sys3d.namp.cs): "NAMP", its version, four counts and sizes,
// and the offsets of up to four NNS G3D animation files - material colour (BMA0), texture SRT (BTA0),
// texture pattern (BTP0), visibility (BVA0), in that order. The stage plays animation 0 of each, one
// frame a game step, each slot looping on its own length and never showing its last frame
// (sys3d.CAnimation.next). What the port applies, this gives: the texture SRT (the whole texture
// matrix, rebuilt each frame about the texture's centre) and the material's alpha (its diffuse and
// the rest are evaluated by the game and dropped). The pattern the port reads as something else and
// never plays; the visibility it plays is its own addition, and FF3's stages carry none - both are
// only reported here.
//
// Server: /api/model/animation?name=files/bNN.nmdp.lz.

using System;
using System.Collections.Generic;
using System.Linq;
using Crystal;

namespace Crystal.Editor
{
	internal static class Namp
	{
		/// <summary>The animation of a model's bundle, or null when it has none: per material, a frame's texture-coordinate transform and alpha.</summary>
		public static object Read(Workspace workspace, string model)
		{
			if (string.IsNullOrEmpty(model) || !model.EndsWith(".nmdp.lz", StringComparison.OrdinalIgnoreCase)) return new { none = true };
			string name = model.Substring(0, model.Length - ".nmdp.lz".Length) + ".namp.lz";
			if (!workspace.Exists(name)) return new { none = true };
			byte[] raw = workspace.Read(name);
			byte[] data = Lz.IsCompressed(raw) ? Lz.Decompress(raw) : raw;
			if (data.Length < 64 || data[0] != 'N' || data[1] != 'A' || data[2] != 'M' || data[3] != 'P') return new { none = true, problem = name + " is not a NAMP" };

			List<Mdl0Material> materials = Mdl0.Read(Models.Unpack(Lz.Decompress(workspace.Read(model)))).SelectMany(m => m.Materials).ToList();
			int ima = I32(data, 40), ita = I32(data, 44), itp = I32(data, 48), iva = I32(data, 52);

			object srt = null, alpha = null;
			if (ita > 0 && First(data, ita) is int a && U16(data, a + 2) == 0x5441)
			{
				// Texture SRT: numFrame, flag, mode, then a dictionary by material of five (info, ex) pairs -
				// scale S, scale T, rotation, translation S, translation T.
				int frames = U16(data, a + 4), shown = Shown(frames);
				Dictionary<string, float[][]> byMaterial = new Dictionary<string, float[][]>();
				foreach ((string mat, int entry) in Dict(data, a + 8))
				{
					Mdl0Material m = materials.FirstOrDefault(x => x.Name == mat);
					float[][] list = new float[shown][];
					for (int f = 0; f < shown; f++)
					{
						double Value(int j) => SrtValue(data, a, I32(data, entry + j * 8), I32(data, entry + j * 8 + 4), f);
						int rot = (int)Value(2);
						float cos = (short)(rot >> 16) / 4096f, sin = (short)(rot & 0xFFFF) / 4096f;
						list[f] = Relative(m, (float)(Value(0) / 4096), (float)(Value(1) / 4096), sin, cos, (float)(Value(3) / 4096), (float)(Value(4) / 4096));
					}
					byMaterial[mat] = list;
				}
				srt = new { length = shown, materials = byMaterial };
			}
			if (ima > 0 && First(data, ima) is int b && U16(data, b + 2) != 0x5441 && U16(data, b + 2) != 0x5641 && U16(data, b + 2) != 0x5450)
			{
				// Material colour: numFrame, flag, then a dictionary by material of five tags - diffuse, ambient,
				// emission, specular, polygon alpha. The port applies the alpha alone.
				int frames = U16(data, b + 4), shown = Shown(frames);
				Dictionary<string, float[]> byMaterial = new Dictionary<string, float[]>();
				foreach ((string mat, int entry) in Dict(data, b + 8))
				{
					uint tag = (uint)I32(data, entry + 16);
					float[] list = new float[shown];
					for (int f = 0; f < shown; f++)
					{
						int v;
						if ((tag & 0x20000000) != 0) v = (ushort)tag;
						else v = data[b + (int)(tag & 0xFFFF) + (f >> (int)(tag >> 30))];
						list[f] = Math.Min(31, v & 31) / 31f;
					}
					byMaterial[mat] = list;
				}
				alpha = new { length = shown, materials = byMaterial };
			}
			return new
			{
				name,
				srt,
				alpha,
				pattern = itp > 0,       // there, but the port never plays it
				visibility = iva > 0,    // the port's own addition; FF3's stages have none
			};
		}

		/// <summary>The frames a looping slot shows: 0 to numFrame - 2 (the last is never drawn), one at least.</summary>
		private static int Shown(int frames) => Math.Max(1, frames - 1);

		/// <summary>An SRT component at a frame: a constant, or a table of fx16 / fx32 stepped by the info's shift - no interpolation, as the port samples it.</summary>
		private static double SrtValue(byte[] data, int anim, int info, int ex, int frame)
		{
			uint i = (uint)info;
			if ((i & 0x20000000) != 0) return ex;
			int at = anim + ex, idx = frame >> (int)(i >> 30);
			if ((i & 0x10000000) != 0) return (short)U16(data, at + 2 * idx);
			return I32(data, at + 4 * idx);
		}

		/// <summary>
		/// The texture matrix the port builds from a frame's SRT (Members.cs DrawModel: scale and rotation
		/// about the texture's centre, S's translation negated), as a transform of the coordinates the model
		/// was baked with (its static matrix, Models.ReadData) - [t11, t12, t21, t22, t41, t42]:
		/// u' = u t11 + v t21 + t41, v' = u t12 + v t22 + t42.
		/// </summary>
		private static float[] Relative(Mdl0Material m, float scaleS, float scaleT, float sin, float cos, float transS, float transT)
		{
			float w = m != null && m.Width > 0 ? m.Width : 1, h = m != null && m.Height > 0 ? m.Height : 1;
			float[] now = Matrix(w, h, scaleS, scaleT, sin, cos, transS, transT);
			float[] baked = m != null && m.HasTexMatrix ? Matrix(w, h, m.ScaleS, m.ScaleT, m.RotSin, m.RotCos, m.TransS, m.TransT) : new[] { 1 / w, 0, 0, 1 / h, 0f, 0f };
			// [u v] = [s t] L0 + o0, so [s t] = ([u v] - o0) L0^-1, and [u' v'] = [s t] L1 + o1.
			float det = baked[0] * baked[3] - baked[1] * baked[2];
			if (Math.Abs(det) < 1e-12f) return new float[] { 1, 0, 0, 1, 0, 0 };
			float i11 = baked[3] / det, i12 = -baked[1] / det, i21 = -baked[2] / det, i22 = baked[0] / det;
			float t11 = i11 * now[0] + i12 * now[2], t12 = i11 * now[1] + i12 * now[3];
			float t21 = i21 * now[0] + i22 * now[2], t22 = i21 * now[1] + i22 * now[3];
			float t41 = now[4] - (baked[4] * t11 + baked[5] * t21), t42 = now[5] - (baked[4] * t12 + baked[5] * t22);
			return new[] { t11, t12, t21, t22, t41, t42 };
		}

		/// <summary>[m11, m12, m21, m22, m41, m42] from texels to 0..1, as the port and Models.ReadData compose it.</summary>
		private static float[] Matrix(float w, float h, float scaleS, float scaleT, float sin, float cos, float transS, float transT)
		{
			float sx = scaleS / w, sy = scaleT / h;
			float m11 = cos * sx, m12 = sin * sx, m21 = -sin * sy, m22 = cos * sy;
			float m41 = -transS - (m11 * w + m21 * h) * 0.5f + 0.5f;
			float m42 = transT - (m12 * w + m22 * h) * 0.5f + 0.5f;
			return new[] { m11, m12, m21, m22, m41, m42 };
		}

		/// <summary>Animation 0 of the NNS file at `file`: its first block's dictionary, whose entries are offsets from the block.</summary>
		private static int? First(byte[] data, int file)
		{
			if (file + 20 > data.Length) return null;
			int blocks = U16(data, file + 14);
			if (blocks < 1) return null;
			int block = file + I32(data, file + 16);
			List<(string, int)> anims = Dict(data, block + 8);
			if (anims.Count == 0) return null;
			return block + I32(data, anims[0].Item2);
		}

		/// <summary>An NNS dictionary: each entry's name and where its data is.</summary>
		private static List<(string, int)> Dict(byte[] data, int at)
		{
			int count = data[at + 1];
			int entries = at + U16(data, at + 6);
			int unit = U16(data, entries);
			int names = entries + U16(data, entries + 2);
			List<(string, int)> list = new List<(string, int)>(count);
			for (int i = 0; i < count; i++)
			{
				int n = names + 16 * i, len = 0;
				while (len < 16 && n + len < data.Length && data[n + len] != 0) len++;
				list.Add((System.Text.Encoding.ASCII.GetString(data, n, len), entries + 4 + unit * i));
			}
			return list;
		}

		private static int U16(byte[] d, int at) => d[at] | (d[at + 1] << 8);
		private static int I32(byte[] d, int at) => d[at] | (d[at + 1] << 8) | (d[at + 2] << 16) | (d[at + 3] << 24);
	}
}
