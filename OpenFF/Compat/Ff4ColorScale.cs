// FF4's grey-scale fades (glayScaleMap, glayScaleCharacter, glayScaleCharaAll): libff4's evt::ColorScaleToward.
//
// libff4 greys a model's texture palette: each colour's 5-bit r, g and b go toward their average,
// c + (avg - c) * ratio / 100, the ratio stepping from one percentage to another over a number of frames (worked out
// again only when it moves a 31st of the way), and the palette goes back to VRAM. The port decodes each texture to RGBA
// once, so the same sum runs on the decoded texels instead, always from the texture as it was loaded: the palette
// formats only, as libff4 (a direct-colour texture has no palette to grey).

using System;
using System.Collections.Generic;

namespace OpenFF.Client
{
	internal static class Ff4ColorScale
	{
		private sealed class Entry
		{
			public GlobalScope.NNSG3dResMdl Model;
			public int Frame, Frames, From, To, Ratio;
		}

		private static readonly List<Entry> _entries = new List<Entry>();

		/// <summary>Each greyed texture as it was loaded (RGBA), by GL texture number.</summary>
		private static readonly Dictionary<uint, byte[]> _originals = new Dictionary<uint, byte[]>();

		/// <summary>ColorScaleToward::regist: the model's textures from one percentage of grey to another over that many frames.</summary>
		public static void Regist(GlobalScope.NNSG3dResMdl model, int frames, int from, int to)
		{
			if (model == null) return;
			_entries.RemoveAll(e => e.Model == model);
			_entries.Add(new Entry { Model = model, Frames = frames, From = from, To = to, Ratio = to });
		}

		/// <summary>glayScaleMap(frames, from, to): the stage's model.</summary>
		public static void Map(GlobalScope.ScriptEngine engine)
		{
			int frames = (short)engine.getWord(), from = (short)engine.getWord(), to = (short)engine.getWord();
			Regist(GlobalScope.stageMng?.ModelResource, frames, from, to);
		}

		/// <summary>glayScaleCharacter(cast, frames, from, to): one cast's model.</summary>
		public static void Character(GlobalScope.ScriptEngine engine)
		{
			uint hich = engine.getWord();
			int frames = (short)engine.getWord(), from = (short)engine.getWord(), to = (short)engine.getWord();
			int index = GlobalScope.CCastCommandTransit.getInstance().changeHichNumber(hich & 0xffff);
			if (index == -1) return;
			int ctrl = EngineApi.Players.Player(index)?.getCharacterId() ?? -1;
			if (ctrl >= 0) Regist(GlobalScope.characterMng.getMdlResource(ctrl), frames, from, to);
		}

		/// <summary>glayScaleCharaAll(frames, from, to): every character with a texture of its own.</summary>
		public static void CharaAll(GlobalScope.ScriptEngine engine)
		{
			int frames = (short)engine.getWord(), from = (short)engine.getWord(), to = (short)engine.getWord();
			GlobalScope.CCharacterMng characters = GlobalScope.characterMng;
			for (int ctrl = 0; ctrl < characters.Capacity; ctrl++)
			{
				if (characters.isValidCharacter(ctrl)) Regist(characters.getMdlResource(ctrl), frames, from, to);
			}
		}

		/// <summary>ColorScaleToward::execute, each frame.</summary>
		public static void Tick()
		{
			for (int i = _entries.Count - 1; i >= 0; i--)
			{
				Entry e = _entries[i];
				int was = e.Frame;
				e.Frame++;
				int ratio = e.To;
				bool apply = true;
				if (e.Frame < e.Frames)
				{
					ratio = e.From + (e.Frames != 0 ? (e.To - e.From) * e.Frame / e.Frames : 0);
					apply = was == 0 || e.Ratio * 31 / 200 != ratio * 31 / 200;
				}
				else
				{
					_entries.RemoveAt(i);
				}
				if (apply)
				{
					try { Apply(e.Model, ratio); } catch (Exception ex) { Log.First(LogChannel.General, "ff4-grey", 3, () => "script: grey scale: " + ex.Message); }
				}
				e.Ratio = ratio;
			}
		}

		/// <summary>The map is left: its models and textures go with it.</summary>
		public static void Clear()
		{
			_entries.Clear();
			_originals.Clear();
		}

		private static void Apply(GlobalScope.NNSG3dResMdl model, int ratio)
		{
			GlobalScope.NNSG3dResMat materials = GlobalScope.NNS_G3dGetMat(model);
			if (materials?.mat == null) return;
			HashSet<uint> done = new HashSet<uint>();
			foreach (GlobalScope.NNSG3dResMatData material in materials.mat)
			{
				GlobalScope.TexVramList texture = material?.texImageParam;
				if (texture == null || texture.tex == 0 || texture.fmt == 7 || !done.Add(texture.tex)) continue;
				Grey(texture.tex, ratio);
			}
		}

		private static void Grey(uint id, int ratio)
		{
			GlobalScope.GlTexture[] textures = GlobalScope.m_aGlTexture;
			if (textures == null || id >= textures.Length || textures[id]?.m_Texture2D == null || textures[id].m_Texture2D.IsDisposed) return;
			Microsoft.Xna.Framework.Graphics.Texture2D texture = textures[id].m_Texture2D;
			if (!_originals.TryGetValue(id, out byte[] original))
			{
				if (ratio == 0) return;
				original = new byte[texture.Width * texture.Height * 4];
				texture.GetData(original);
				_originals[id] = original;
			}
			if (ratio == 0)
			{
				texture.SetData(original);   // as loaded, to the last bit
				return;
			}
			byte[] grey = new byte[original.Length];
			for (int i = 0; i < original.Length; i += 4)
			{
				int r = original[i] >> 3, g = original[i + 1] >> 3, b = original[i + 2] >> 3;
				int average = (r + g + b) / 3;
				grey[i] = (byte)((r + (average - r) * ratio / 100) << 3);
				grey[i + 1] = (byte)((g + (average - g) * ratio / 100) << 3);
				grey[i + 2] = (byte)((b + (average - b) * ratio / 100) << 3);
				grey[i + 3] = original[i + 3];
			}
			texture.SetData(grey);
		}
	}
}
