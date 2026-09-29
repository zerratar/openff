// The game's effects as the DS effect library ("eld", OpenFF/GlobalScope/eld) stores them: an effect
// pack (e###.efp) holds templates - particle emitters, models, and sequences that boot the others
// along paths - with their sprite animations and textures; effect.efi maps a category and a member
// to a template's id. Read here for Crystal and the importer (EffectImport), never written back.
//
// Every number is little-endian; positions, speeds and sizes are fx32 (4096 = 1 world unit), angles
// the DS's (65536 a turn), colours 0-31. Offsets inside a pack are from the start of the file.

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenFF.Effects
{
	public enum EffectKind { Unknown, Particle, ParticleLarge, ParticleGather, Model, Sequence }

	/// <summary>effect.efi: a category's members, each a template id (member 0 is none).</summary>
	public sealed class EfiIndex
	{
		public readonly uint[][] Categories;

		private EfiIndex(uint[][] categories) { Categories = categories; }

		public static EfiIndex Read(byte[] data)
		{
			int count = (int)U32(data, 0);
			if (count < 0 || 16 + 8 * count > data.Length) throw new InvalidOperationException("not an effect index");
			uint[][] categories = new uint[count][];
			for (int c = 0; c < count; c++)
			{
				int at = (int)U32(data, 16 + 8 * c), members = (int)U32(data, 20 + 8 * c);
				uint[] ids = new uint[members > 0 && at + 4 * members <= data.Length ? members : 0];
				for (int m = 0; m < ids.Length; m++) ids[m] = U32(data, at + 4 * m);
				categories[c] = ids;
			}
			return new EfiIndex(categories);
		}

		/// <summary>The template id of a category's member; 0 for none.</summary>
		public uint TemplateId(int category, int member)
		{
			if (category < 0 || category >= Categories.Length || member < 0 || member >= Categories[category].Length) return 0;
			return Categories[category][member];
		}

		/// <summary>Which category and member name a template id first (-1, -1 when none does).</summary>
		public (int Category, int Member) Find(uint id, int preferCategory = -1)
		{
			if (preferCategory >= 0 && preferCategory < Categories.Length)
				for (int m = 1; m < Categories[preferCategory].Length; m++) if (Categories[preferCategory][m] == id) return (preferCategory, m);
			for (int c = 0; c < Categories.Length; c++)
				for (int m = 1; m < Categories[c].Length; m++) if (Categories[c][m] == id) return (c, m);
			return (-1, -1);
		}

		internal static uint U32(byte[] d, int at) => at + 4 <= d.Length ? BitConverter.ToUInt32(d, at) : 0;
	}

	/// <summary>An effect pack: its templates in file order.</summary>
	public sealed class EfpPack
	{
		public string Name;
		public uint Version;
		public readonly List<EffectTemplate> Templates = new List<EffectTemplate>();
		/// <summary>The pack's textures by offset (templates share them).</summary>
		public readonly Dictionary<int, NtpkTexture> Textures = new Dictionary<int, NtpkTexture>();

		private static readonly (uint A, ushort B, ushort C, ulong D, EffectKind Kind)[] Factories =
		{
			(0x91917E57, 0x9AF8, 0x4DA3, 0x28FA27B6BD839889UL, EffectKind.Particle),
			(0x605FD8B8, 0x992E, 0x41FF, 0xB03918B085F56B95UL, EffectKind.ParticleLarge),
			(0xCC29F917, 0x5900, 0x4E67, 0xB5CCD0BA0CD12396UL, EffectKind.ParticleGather),
			(0xA36D0390, 0xF0AB, 0x417A, 0x3E1FF5EDB9BC2FA1UL, EffectKind.Model),
			(0x63511AB8, 0x3F85, 0x4156, 0xF39D5A8E918AC4BCUL, EffectKind.Sequence),
		};

		public static bool IsPack(byte[] data) => data != null && data.Length >= 16 && data[0] == 'E' && data[1] == 'F' && data[2] == 'C' && data[3] == 'T';

		public static EfpPack Read(byte[] data, string name = null)
		{
			if (!IsPack(data)) throw new InvalidOperationException("not an effect pack");
			Reader r = new Reader(data, 4);
			int count = r.U16();
			r.U16();
			int textures = (int)r.U32();
			EfpPack pack = new EfpPack { Name = name, Version = r.U32() };
			int[] index = new int[count];
			for (int i = 0; i < count; i++) index[i] = (int)r.U32();
			for (int i = 0; i < count; i++)
			{
				int at = index[i];
				int end = i + 1 < count ? index[i + 1] : (textures > at ? textures : data.Length);
				pack.Templates.Add(ReadTemplate(pack, data, at, end));
			}
			return pack;
		}

		public EffectTemplate Template(uint id)
		{
			foreach (EffectTemplate t in Templates) if (t.Id == id) return t;
			return null;
		}

		private static EffectTemplate ReadTemplate(EfpPack pack, byte[] d, int at, int end)
		{
			Reader r = new Reader(d, at);
			uint a = r.U32(); ushort b = r.U16(), c = r.U16(); ulong g = BitConverter.ToUInt64(d, r.At); r.At += 8;
			EffectKind kind = EffectKind.Unknown;
			foreach (var f in Factories) if (f.A == a && f.B == b && f.C == c && f.D == g) kind = f.Kind;
			uint id = r.U32(), version = r.U32();
			int anim = (int)r.U32(), texture = (int)r.U32();
			int param = r.At;
			EffectTemplate t;
			switch (kind)
			{
				case EffectKind.Particle:
				case EffectKind.ParticleLarge:
				case EffectKind.ParticleGather:
					t = ParticleTemplate.Read(new Reader(d, param), kind);
					ParticleTemplate p = (ParticleTemplate)t;
					if (anim > 0 && anim < d.Length) p.Animation = SpriteAnimation.Read(d, anim);
					if (texture > 0 && texture < d.Length)
					{
						if (!pack.Textures.TryGetValue(texture, out NtpkTexture tex)) pack.Textures[texture] = tex = NtpkTexture.Read(d, texture);
						p.Texture = tex;
					}
					break;
				case EffectKind.Model:
					t = ModelTemplate.Read(new Reader(d, param));
					break;
				case EffectKind.Sequence:
					t = SequenceTemplate.Read(d, param);
					break;
				default:
					t = new EffectTemplate();
					break;
			}
			t.Kind = kind; t.Offset = at; t.Size = end - at; t.Id = id; t.TemplateVersion = version;
			return t;
		}
	}

	public class EffectTemplate
	{
		public EffectKind Kind;
		public int Offset, Size;
		public uint Id, TemplateVersion;
	}

	/// <summary>The DS flags of a particle template (enFLAG_PDS).</summary>
	[Flags]
	public enum ParticleFlags : uint { Loop = 1, AfterImage = 2, Fade = 4, MoveOffset = 8, TranslucentDepth = 16 }

	/// <summary>A particle emitter: ParticleDS, ParticleLargeDS (no circle), or ParticleGatherDS (gather, no speed).</summary>
	public sealed class ParticleTemplate : EffectTemplate
	{
		public ParticleFlags Flags;
		public int[] Range = new int[3];
		public int SizeBase, SizeRand;
		public int TimePlay, GroupLife, Interval, Childs, Groups;
		public int[] SpeedDir = new int[3]; public int SpeedPow, SpeedRand;
		public int[] GravityDir = new int[3]; public int GravityPow, GravityRand;
		public int[] EmitAngle = new int[3];
		public int AfterCount; public short[] AfterColour = new short[4];
		public int FadeStart, FadeTime; public short[] FadeColour = new short[4];
		public int CircleRadius, CircleRadiusAdd, CircleAngleAdd;
		public int GatherSpeed, GatherSpeedAdd; public int[] GatherRotate = new int[3];
		public SpriteAnimation Animation;
		public NtpkTexture Texture;

		internal static ParticleTemplate Read(Reader r, EffectKind kind)
		{
			ParticleTemplate p = new ParticleTemplate();
			r.U32();
			p.Flags = (ParticleFlags)r.U32();
			for (int i = 0; i < 3; i++) p.Range[i] = r.S32();
			p.SizeBase = r.S32(); p.SizeRand = r.S32();
			p.TimePlay = r.U16(); p.GroupLife = r.U16(); p.Interval = r.U16(); p.Childs = r.U16(); p.Groups = r.U16(); r.U16();
			if (kind != EffectKind.ParticleGather)
			{
				for (int i = 0; i < 3; i++) p.SpeedDir[i] = r.S32();
				p.SpeedPow = r.S32(); p.SpeedRand = r.S32();
				for (int i = 0; i < 3; i++) p.GravityDir[i] = r.S32();
				p.GravityPow = r.S32(); p.GravityRand = r.S32();
				for (int i = 0; i < 3; i++) p.EmitAngle[i] = r.S32();
			}
			p.AfterCount = r.U16();
			for (int i = 0; i < 4; i++) p.AfterColour[i] = r.S16();
			p.FadeStart = r.U16(); p.FadeTime = r.U16();
			for (int i = 0; i < 4; i++) p.FadeColour[i] = r.S16();
			r.U16();
			if (kind == EffectKind.ParticleGather)
			{
				p.GatherSpeed = r.S32(); p.GatherSpeedAdd = r.S32();
				for (int i = 0; i < 3; i++) p.GatherRotate[i] = r.S32();
			}
			else
			{
				p.CircleRadius = r.S32(); p.CircleRadiusAdd = r.S32(); p.CircleAngleAdd = r.S32();
				if (kind == EffectKind.ParticleLarge) p.CircleRadius = p.CircleRadiusAdd = p.CircleAngleAdd = 0;   // read, never used
			}
			return p;
		}
	}

	/// <summary>A particle group's sprite animation (eld.spr): a UV table, a scale table and a colour table, stepped once a game step.</summary>
	public sealed class SpriteAnimation
	{
		public const uint Loop = 0x80000000, Interpolate = 0x40000000;

		public int CellWidth, CellHeight, StartU, StartV, SheetWidth;
		public uint UvFlags, ScaleFlags, ColourFlags;
		/// <summary>(steps, pattern) - the pattern a cell of the grid, row by row.</summary>
		public readonly List<(int Time, int Pattern)> Uv = new List<(int, int)>();
		/// <summary>(steps, x, y) in fx32.</summary>
		public readonly List<(int Time, int X, int Y)> Scale = new List<(int, int, int)>();
		/// <summary>(steps, red, green, blue, alpha) 0-31 - stored red, blue, green, alpha in the file.</summary>
		public readonly List<(int Time, int R, int G, int B, int A)> Colour = new List<(int, int, int, int, int)>();

		internal static SpriteAnimation Read(byte[] d, int at)
		{
			Reader r = new Reader(d, at);
			int nUv = (int)r.U32(), nScale = (int)r.U32(), nColour = (int)r.U32(); r.U32();
			int oUv = r.S32(), oScale = r.S32(), oColour = r.S32();
			SpriteAnimation a = new SpriteAnimation();
			r.At = at + oUv;
			a.CellWidth = r.U16(); a.CellHeight = r.U16(); a.StartU = r.U16(); a.StartV = r.U16(); a.SheetWidth = r.U16();
			r.U16(); r.U16(); r.U16();
			a.UvFlags = r.U32(); r.At += 12;
			for (int i = 0; i < nUv; i++) a.Uv.Add((r.U16(), r.U16()));
			r.At = at + oScale;
			a.ScaleFlags = r.U32(); r.At += 12;
			for (int i = 0; i < nScale; i++) { int t = (int)r.U32(), x = r.S32(), y = r.S32(); r.U32(); a.Scale.Add((t, x, y)); }
			r.At = at + oColour;
			a.ColourFlags = r.U32(); r.At += 12;
			for (int i = 0; i < nColour; i++)
			{
				int t = (int)r.U32(); r.At += 12;
				int red = r.S32(), blue = r.S32(), green = r.S32(), alpha = r.S32();
				a.Colour.Add((t, red, green, blue, alpha));
			}
			return a;
		}
	}

	/// <summary>A DS texture as the packs carry it (NTPK): A3I5 or 256-colour in the game's data, the other formats read too.</summary>
	public sealed class NtpkTexture
	{
		public int Offset;
		public string Name;
		public int Format, Width, Height;
		public bool Colour0Transparent;
		public byte[] Texels;
		public ushort[] Palette;

		internal static NtpkTexture Read(byte[] d, int at)
		{
			if (at + 100 > d.Length || d[at] != 'N' || d[at + 1] != 'T' || d[at + 2] != 'P' || d[at + 3] != 'K') return null;
			NtpkTexture t = new NtpkTexture { Offset = at };
			int nameEnd = Array.IndexOf(d, (byte)0, at + 12, 32);
			t.Name = Encoding.ASCII.GetString(d, at + 12, (nameEnd < 0 ? at + 44 : nameEnd) - (at + 12));
			int texels = (int)EfiIndex.U32(d, at + 0x2C), palette = (int)EfiIndex.U32(d, at + 0x30);
			int texelsAt = (int)EfiIndex.U32(d, at + 0x38), paletteAt = (int)EfiIndex.U32(d, at + 0x3C);
			t.Format = d[at + 0x5C];
			t.Width = 8 << d[at + 0x5E];
			t.Height = 8 << d[at + 0x5F];
			t.Colour0Transparent = d[at + 0x62] != 0;
			t.Texels = new byte[Math.Max(0, Math.Min(texels, d.Length - (at + texelsAt)))];
			Array.Copy(d, at + texelsAt, t.Texels, 0, t.Texels.Length);
			int colours = paletteAt > 0 ? Math.Max(0, Math.Min(palette, d.Length - (at + paletteAt))) / 2 : 0;
			t.Palette = new ushort[colours];
			for (int i = 0; i < colours; i++) t.Palette[i] = BitConverter.ToUInt16(d, at + paletteAt + 2 * i);
			return t;
		}

		/// <summary>Red, green, blue, alpha, a byte each, top row first - as the port's LoadTexture decodes them.</summary>
		public byte[] DecodeRgba()
		{
			byte[] o = new byte[Width * Height * 4];
			for (int i = 0; i < Width * Height; i++)
			{
				int r = 0, g = 0, b = 0, a = 0;
				void Pal(int index, bool exact)
				{
					int c = index < Palette.Length ? Palette[index] : 0;
					if (exact) { r = (c & 31) * 255 / 31; g = ((c >> 5) & 31) * 255 / 31; b = ((c >> 10) & 31) * 255 / 31; }
					else { r = (c & 31) << 3; g = ((c >> 5) & 31) << 3; b = ((c >> 10) & 31) << 3; }
				}
				int Byte(int n) => n < Texels.Length ? Texels[n] : 0;
				switch (Format)
				{
					case 1: { int v = Byte(i); Pal(v & 31, false); a = (v >> 5) * 255 / 7; break; }
					case 6: { int v = Byte(i); Pal(v & 7, false); a = (v >> 3) * 255 / 31; break; }
					case 2: { int v = (Byte(i >> 2) >> ((i & 3) * 2)) & 3; Pal(v, false); a = v != 0 || !Colour0Transparent ? 255 : 0; break; }
					case 3: { int v = (Byte(i >> 1) >> ((i & 1) * 4)) & 15; Pal(v, false); a = v != 0 || !Colour0Transparent ? 255 : 0; break; }
					case 4: { int v = Byte(i); Pal(v, true); a = v != 0 || !Colour0Transparent ? 255 : 0; break; }
					case 7: { int c = Byte(2 * i) | (Byte(2 * i + 1) << 8); r = (c & 31) << 3; g = ((c >> 5) & 31) << 3; b = ((c >> 10) & 31) << 3; a = (c & 0x8000) != 0 ? 255 : 0; break; }
				}
				o[4 * i] = (byte)r; o[4 * i + 1] = (byte)g; o[4 * i + 2] = (byte)b; o[4 * i + 3] = (byte)a;
			}
			return o;
		}
	}

	/// <summary>A model the effect shows (ModelDS): its scale and the names of its animations; the model itself is not read yet.</summary>
	public sealed class ModelTemplate : EffectTemplate
	{
		public bool Loop;
		public int[] Scale = new int[3];
		public string Material, TextureSrt, TexturePattern, Visibility;

		internal static ModelTemplate Read(Reader r)
		{
			ModelTemplate m = new ModelTemplate();
			r.U32();
			m.Loop = (r.U32() & 1) != 0;
			for (int i = 0; i < 3; i++) m.Scale[i] = r.S32();
			r.At += 48;
			m.Material = r.Text(48); m.TextureSrt = r.Text(48); m.TexturePattern = r.Text(48); m.Visibility = r.Text(48);
			return m;
		}
	}

	/// <summary>A sequence (SequenceDS): commands run a game step at a time - wait, boot another template on a path, halt one, end.</summary>
	public sealed class SequenceTemplate : EffectTemplate
	{
		public bool Loop;
		public readonly List<SequenceCommand> Commands = new List<SequenceCommand>();
		public readonly List<SequencePath> Paths = new List<SequencePath>();

		internal static SequenceTemplate Read(byte[] d, int param)
		{
			SequenceTemplate s = new SequenceTemplate();
			Reader r = new Reader(d, param);
			int pathOffset = (int)r.U32();
			r.U32();
			s.Loop = (r.U32() & 1) != 0;
			r.At = param + 32;
			int end = param + pathOffset;
			while (r.At + 4 <= end)
			{
				uint op = r.U32();
				if (op == 0) { s.Commands.Add(new SequenceCommand { Op = SequenceOp.Wait, Time = (int)r.U32() }); r.At += 8; }
				else if (op == 1) { r.At += 12; }   // POSITION: never in the data (and misread by the port)
				else if (op == 2)
				{
					SequenceCommand c = new SequenceCommand { Op = SequenceOp.Boot, Category = (int)r.U32(), Member = (int)r.U32() };
					c.Position = new[] { r.F32(), r.F32(), r.F32() };
					r.At += 12;
					c.BootId = (int)r.U32(); r.U32();
					c.Path = r.U16(); r.At += 2;
					s.Commands.Add(c);
				}
				else if (op == 3) { r.U32(); s.Commands.Add(new SequenceCommand { Op = SequenceOp.Halt, BootId = (int)r.U32() }); r.U32(); }
				else if (op == 4) { r.At += 12; }
				else { s.Commands.Add(new SequenceCommand { Op = SequenceOp.End }); break; }
			}
			if (end + 16 <= d.Length && d[end] == 'P' && d[end + 1] == 'A' && d[end + 2] == 'T' && d[end + 3] == 'H')
			{
				int n = (int)EfiIndex.U32(d, end + 8);
				for (int i = 0; i < n; i++) s.Paths.Add(SequencePath.Read(d, end + (int)EfiIndex.U32(d, end + 16 + 4 * i)));
			}
			return s;
		}
	}

	public enum SequenceOp { Wait, Boot, Halt, End }

	public sealed class SequenceCommand
	{
		public SequenceOp Op;
		public int Time;
		public int Category, Member, BootId, Path;
		public float[] Position;
	}

	/// <summary>A path a booted template rides: one point, or segments of a line or a Hermite curve (P0, P1, T0, T1) timed in steps.</summary>
	public sealed class SequencePath
	{
		public const int Curve = 1, Hold = 2, PingPong = 4, Repeat = 8, Local = 0x10, World = 0x20;

		public int Flags, FrameTime;
		/// <summary>fx32 x, y, z per entry: one point, or four an segment.</summary>
		public readonly List<int[]> Points = new List<int[]>();
		public readonly List<int> Timing = new List<int>();

		internal static SequencePath Read(byte[] d, int at)
		{
			Reader r = new Reader(d, at);
			SequencePath p = new SequencePath();
			int count = (int)r.U32();
			p.FrameTime = (int)r.U32();
			p.Flags = (int)r.U32();
			r.U32();
			for (int i = 0; i < count; i++) { p.Points.Add(new[] { r.S32(), r.S32(), r.S32() }); r.S32(); }
			int points = (count >> 2) + 1;
			r.At += 16 * points;
			for (int i = 0; i < points; i++) p.Timing.Add((int)r.U32());
			return p;
		}
	}

	internal sealed class Reader
	{
		private readonly byte[] _d;
		public int At;
		public Reader(byte[] d, int at) { _d = d; At = at; }
		private bool Has(int n) => At >= 0 && At + n <= _d.Length;
		public uint U32() { uint v = Has(4) ? BitConverter.ToUInt32(_d, At) : 0; At += 4; return v; }
		public int S32() { int v = Has(4) ? BitConverter.ToInt32(_d, At) : 0; At += 4; return v; }
		public ushort U16() { ushort v = Has(2) ? BitConverter.ToUInt16(_d, At) : (ushort)0; At += 2; return v; }
		public short S16() { short v = Has(2) ? BitConverter.ToInt16(_d, At) : (short)0; At += 2; return v; }
		public float F32() { float v = Has(4) ? BitConverter.ToSingle(_d, At) : 0f; At += 4; return v; }
		public string Text(int n)
		{
			if (!Has(n)) { At += n; return ""; }
			int end = Array.IndexOf(_d, (byte)0, At, n);
			string s = Encoding.ASCII.GetString(_d, At, (end < 0 ? At + n : end) - At);
			At += n;
			return s;
		}
	}
}
