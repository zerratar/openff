// A VP8 video decoder: the reference decoder of RFC 6386 ("dixie", the RFC's attachment one) carried over to C#, function
// for function, so a frame comes out as libvpx makes it. FF4's opening.mkv is VP8 (with Vorbis sound, NVorbis's): Steam's
// FF4.exe plays it through the libavcodec beside it; this is the client's own way to the same pictures.
//
// The shape is dixie's: the first partition's frame header, the modes and motion vectors of a row (modemv), its tokens
// from the row's token partition (tokens), the prediction and the inverse transforms into the frame (predict), the row
// before it loop-filtered (dixie_loopfilter) - so intra prediction reads the unfiltered row above, as it must - and the
// reference frames (last, golden, altref) swapped by reference count at the end. Frames carry a 16-pixel border that the
// intra prediction's edges are written into; motion compensation near an edge reads through an emulated border (the
// edge pixels repeated).
//
// Decode takes a frame's bytes; Frame is then the picture (I420, Y / U / V with their strides) to show while Shown.
//
// ---- The reference decoder's licence ----
// Copyright (c) 2010, 2011, Google Inc.  All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
// following conditions are met:
//
// o  Redistributions of source code must retain the above copyright notice, this list of conditions and the following
//    disclaimer.
// o  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following
//    disclaimer in the documentation and/or other materials provided with the distribution.
// o  Neither the name of Google nor the names of its contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// Google's grant of patent rights for VP8 (the RFC's PATENTS) covers implementations of it such as this one.

using System;
using System.Runtime.InteropServices;

namespace OpenFF.Client
{
	internal sealed unsafe class Vp8Decoder : IDisposable
	{
		// ---- the boolean decoder (bool_decoder.h) ----

		private struct BoolDecoder
		{
			public byte* Input;
			public int Length;
			public uint Range, Value;
			public int BitCount;

			public void Init(byte* start, int size)
			{
				if (size >= 2)
				{
					Value = (uint)((start[0] << 8) | start[1]);
					Input = start + 2;
					Length = size - 2;
				}
				else
				{
					Value = 0;
					Input = null;
					Length = 0;
				}
				Range = 255;
				BitCount = 0;
			}

			public int Get(int probability)
			{
				uint split = 1 + (uint)(((Range - 1) * probability) >> 8);
				uint bigSplit = split << 8;
				int bit;
				if (Value >= bigSplit)
				{
					bit = 1;
					Range -= split;
					Value -= bigSplit;
				}
				else
				{
					bit = 0;
					Range = split;
				}
				while (Range < 128)
				{
					Value <<= 1;
					Range <<= 1;
					if (++BitCount == 8)
					{
						BitCount = 0;
						if (Length > 0)
						{
							Value |= *Input++;
							Length--;
						}
					}
				}
				return bit;
			}

			public int Bit() => Get(128);

			public int Uint(int bits)
			{
				int z = 0;
				for (int bit = bits - 1; bit >= 0; bit--) z |= Bit() << bit;
				return z;
			}

			public int Int(int bits)
			{
				int z = Uint(bits);
				return Bit() != 0 ? -z : z;
			}

			public int MaybeInt(int bits) => Bit() != 0 ? Int(bits) : 0;

			public int Tree(int[] tree, byte* probs)
			{
				int i = 0;
				while ((i = tree[i + Get(probs[i >> 1])]) > 0) { }
				return -i;
			}
		}

		// ---- modes, frames, macroblocks (dixie.h) ----

		private const int DcPred = 0, VPred = 1, HPred = 2, TmPred = 3, BPred = 4;
		private const int NearestMv = 5, NearMv = 6, ZeroMv = 7, NewMv = 8, SplitMv = 9;
		private const int BDcPred = 0, BTmPred = 1, BVePred = 2, BHePred = 3, BLdPred = 4, BRdPred = 5, BVrPred = 6, BVlPred = 7, BHdPred = 8, BHuPred = 9;
		private const int Left4x4 = 10, Above4x4 = 11, Zero4x4 = 12, New4x4 = 13;
		private const int CurrentFrame = 0, LastFrame = 1, GoldenFrame = 2, AltrefFrame = 3, NumRefFrames = 4;
		private const int BorderPixels = 16;
		private const int TokenBlockY1 = 0, TokenBlockUv = 1, TokenBlockY2 = 2;
		private const int MvProbCount = 19;

		[StructLayout(LayoutKind.Sequential)]
		private struct Mv
		{
			public short X, Y;
			public uint Raw => (ushort)X | ((uint)(ushort)Y << 16);
			public static Mv FromRaw(uint raw) => new Mv { X = (short)(raw & 0xFFFF), Y = (short)(raw >> 16) };
		}

		private struct MbInfo
		{
			public byte YMode, UvMode, SegmentId, RefFrame, SkipCoeff, NeedMcBorder, Partitioning;
			public Mv Mv;
			public int EobMask;
			public fixed short MvX[16];
			public fixed short MvY[16];
			public fixed byte Modes[16];

			public Mv SplitMv(int b) => new Mv { X = MvX[b], Y = MvY[b] };
			public void SetSplitMv(int b, Mv mv) { MvX[b] = mv.X; MvY[b] = mv.Y; }
		}

		private sealed class Image
		{
			public byte* Data;
			public byte* Y, U, V;
			public int Stride, UvStride;
			public int RefCount;
		}

		private sealed class TokenPartition
		{
			public BoolDecoder Bool;
			public readonly int[] Left = new int[9];
			public short* Coeffs;
		}

		// The frame header (vp8_frame_hdr)
		private bool _keyframe, _shown;
		private int _version, _part0Size, _width, _height, _scaleW, _scaleH;
		private bool _sizeUpdated;

		// The segment header
		private bool _segEnabled, _segUpdateData, _segUpdateMap, _segAbs;
		private readonly byte[] _segTreeProbs = { 255, 255, 255 };
		private readonly int[] _segLfLevel = new int[4], _segQuantIdx = new int[4];

		// The loop filter header
		private bool _lfSimple, _lfDeltaEnabled;
		private int _lfLevel, _lfSharpness;
		private readonly int[] _lfRefDelta = new int[4], _lfModeDelta = new int[4];

		// The token partitions
		private int _partitions;
		private readonly TokenPartition[] _tokens = new TokenPartition[8];

		// The quantizer header
		private int _qIndex, _y1DcDelta, _y2DcDelta, _y2AcDelta, _uvDcDelta, _uvAcDelta;
		private bool _qDeltaUpdate;

		// The reference header
		private bool _refreshLast, _refreshGf, _refreshArf, _refreshEntropy;
		private int _copyGf, _copyArf;
		private readonly int[] _signBias = new int[4];

		// The entropy header (and the copy kept over a frame that does not refresh it)
		private sealed class Entropy
		{
			public readonly byte[] CoeffProbs = new byte[4 * 8 * 3 * 11];
			public readonly byte[] MvProbs = new byte[2 * MvProbCount];
			public bool CoeffSkipEnabled;
			public byte CoeffSkipProb;
			public readonly byte[] YModeProbs = new byte[4];
			public readonly byte[] UvModeProbs = new byte[3];
			public byte ProbInter, ProbLast, ProbGf;

			public void CopyFrom(Entropy o)
			{
				Buffer.BlockCopy(o.CoeffProbs, 0, CoeffProbs, 0, CoeffProbs.Length);
				Buffer.BlockCopy(o.MvProbs, 0, MvProbs, 0, MvProbs.Length);
				CoeffSkipEnabled = o.CoeffSkipEnabled;
				CoeffSkipProb = o.CoeffSkipProb;
				Buffer.BlockCopy(o.YModeProbs, 0, YModeProbs, 0, 4);
				Buffer.BlockCopy(o.UvModeProbs, 0, UvModeProbs, 0, 3);
				ProbInter = o.ProbInter;
				ProbLast = o.ProbLast;
				ProbGf = o.ProbGf;
			}
		}
		private readonly Entropy _entropy = new Entropy(), _savedEntropy = new Entropy();

		private int _mbRows, _mbCols;
		private MbInfo* _mbInfoStorage;
		private int _mbInfoWidth;
		private int* _aboveTokenContext;

		// Dequantization factors per segment: [segment][block type][DC, AC]
		private readonly int[] _dqQuantIdx = { -1, -1, -1, -1 };
		private readonly short[,,] _dqFactor = new short[4, 3, 2];

		private readonly Image[] _storage = new Image[NumRefFrames];
		private readonly Image[] _refFrames = new Image[NumRefFrames];
		private readonly long[] _refOffsets = new long[NumRefFrames];
		private short[][] _subpixelFilters;
		private byte* _emulBlock;

		/// <summary>The picture's size (the frame's own, not rounded to whole macroblocks).</summary>
		public int Width => _width;
		public int Height => _height;
		/// <summary>Whether the frame decoded last is one to show.</summary>
		public bool Shown => _shown;

		// ---- trees and small tables (modemv_data.h, tokens.c, predict.c) ----

		private static readonly byte[] KfYModeProbs = { 145, 156, 163, 128 };
		private static readonly byte[] KfUvModeProbs = { 142, 114, 183 };
		private static readonly int[] KfYModeTree = { -BPred, 2, 4, 6, -DcPred, -VPred, -HPred, -TmPred };
		private static readonly int[] YModeTree = { -DcPred, 2, 4, 6, -VPred, -HPred, -TmPred, -BPred };
		private static readonly int[] UvModeTree = { -DcPred, 2, -VPred, 4, -HPred, -TmPred };
		private static readonly int[] BModeTree = { -BDcPred, 2, -BTmPred, 4, -BVePred, 6, 8, 12, -BHePred, 10, -BRdPred, -BVrPred, -BLdPred, 14, -BVlPred, 16, -BHdPred, -BHuPred };
		private static readonly int[] SmallMvTree = { 2, 8, 4, 6, -0, -1, -2, -3, 10, 12, -4, -5, -6, -7 };
		private static readonly int[] MvRefTree = { -ZeroMv, 2, -NearestMv, 4, -NearMv, 6, -NewMv, -SplitMv };
		private static readonly int[] SubmvRefTree = { -Left4x4, 2, -Above4x4, 4, -Zero4x4, -New4x4 };
		private static readonly int[] SplitMvTree = { -3, 2, -2, 4, -0, -1 };
		private static readonly byte[] DefaultBModeProbs = { 120, 90, 79, 133, 87, 85, 80, 111, 151 };
		private static readonly byte[,] MvCountsToProbs = { { 7, 1, 1, 143 }, { 14, 18, 14, 107 }, { 135, 64, 57, 68 }, { 60, 56, 128, 65 }, { 159, 134, 128, 34 }, { 234, 188, 128, 28 } };
		private static readonly byte[] SplitMvProbs = { 110, 111, 150 };
		private static readonly byte[] SubmvRefProbs2 = { 147, 136, 18, 106, 145, 1, 179, 121, 1, 223, 1, 34, 208, 1, 1 };
		private static readonly int[,] MvPartitions =
		{
			{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 },
			{ 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 },
			{ 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 3, 3, 2, 2, 3, 3 },
			{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }
		};

		private static readonly short[][] SixtapFilters =
		{
			new short[] { 0, 0, 128, 0, 0, 0 }, new short[] { 0, -6, 123, 12, -1, 0 }, new short[] { 2, -11, 108, 36, -8, 1 }, new short[] { 0, -9, 93, 50, -6, 0 },
			new short[] { 3, -16, 77, 77, -16, 3 }, new short[] { 0, -6, 50, 93, -9, 0 }, new short[] { 1, -8, 36, 108, -11, 2 }, new short[] { 0, -1, 12, 123, -6, 0 }
		};
		private static readonly short[][] BilinearFilters =
		{
			new short[] { 0, 0, 128, 0, 0, 0 }, new short[] { 0, 0, 112, 16, 0, 0 }, new short[] { 0, 0, 96, 32, 0, 0 }, new short[] { 0, 0, 80, 48, 0, 0 },
			new short[] { 0, 0, 64, 64, 0, 0 }, new short[] { 0, 0, 48, 80, 0, 0 }, new short[] { 0, 0, 32, 96, 0, 0 }, new short[] { 0, 0, 16, 112, 0, 0 }
		};

		public Vp8Decoder()
		{
			for (int i = 0; i < 8; i++) _tokens[i] = new TokenPartition();
		}

		// ---- the frame (dixie.c) ----

		/// <summary>Decodes one frame; false (and the frame left as it was) when the data is not a frame this can decode.</summary>
		public bool Decode(byte[] data, int size)
		{
			fixed (byte* p = data)
			{
				try { return DecodeFrame(p, size); }
				catch (Exception) { return false; }
			}
		}

		private bool ParseFrameHeader(byte* data, int size)
		{
			if (size < 3) return false;
			uint raw = (uint)(data[0] | (data[1] << 8) | (data[2] << 16));
			_keyframe = (raw & 1) == 0;
			_version = (int)((raw >> 1) & 3);
			bool experimental = ((raw >> 3) & 1) != 0;
			_shown = ((raw >> 4) & 1) != 0;
			_part0Size = (int)((raw >> 5) & 0x7FFFF);
			if (experimental) return false;
			if (size < _part0Size + (_keyframe ? 10 : 3)) return false;
			_sizeUpdated = false;
			if (_keyframe)
			{
				if (data[3] != 0x9d || data[4] != 0x01 || data[5] != 0x2a) return false;
				raw = (uint)(data[6] | (data[7] << 8) | (data[8] << 16) | (data[9] << 24));
				int w = (int)(raw & 0x3FFF), sw = (int)((raw >> 14) & 3), h = (int)((raw >> 16) & 0x3FFF), sh = (int)((raw >> 30) & 3);
				_sizeUpdated = w != _width || h != _height || sw != _scaleW || sh != _scaleH;
				_width = w;
				_height = h;
				_scaleW = sw;
				_scaleH = sh;
				if (w == 0 || h == 0) return false;
			}
			return true;
		}

		private bool DecodeFrame(byte* data, int size)
		{
			if (!ParseFrameHeader(data, size)) return false;
			if (!_keyframe && _storage[0] == null) return false;   // nothing to predict from yet
			data += 3;
			size -= 3;
			if (_keyframe)
			{
				data += 7;
				size -= 7;
				_mbCols = (_width + 15) / 16;
				_mbRows = (_height + 15) / 16;
			}
			BoolDecoder header = default;
			header.Init(data, _part0Size);
			if (_keyframe) header.Uint(2);   // the colour space and the clamping type (the pixels are clamped anyway)
			DecodeSegmentationHeader(ref header);
			DecodeLoopFilterHeader(ref header);
			if (!DecodeTokenPartitions(ref header, data + _part0Size, size - _part0Size)) return false;
			DecodeQuantizerHeader(ref header);
			DecodeReferenceHeader(ref header);
			if (_keyframe)
			{
				Buffer.BlockCopy(Vp8Tables.DefaultCoeffProbs, 0, _entropy.CoeffProbs, 0, _entropy.CoeffProbs.Length);
				Buffer.BlockCopy(Vp8Tables.DefaultMvProbs, 0, _entropy.MvProbs, 0, _entropy.MvProbs.Length);
				Buffer.BlockCopy(Vp8Tables.DefaultYModeProbs, 0, _entropy.YModeProbs, 0, 4);
				Buffer.BlockCopy(Vp8Tables.DefaultUvModeProbs, 0, _entropy.UvModeProbs, 0, 3);
			}
			if (!_refreshEntropy) _savedEntropy.CopyFrom(_entropy);
			DecodeEntropyHeader(ref header);

			ModeMvInit();
			TokensInit();
			PredictInit();
			DequantInit();

			for (int row = 0, partition = 0; row < _mbRows; row++)
			{
				ModeMvProcessRow(ref header, row);
				TokensProcessRow(partition, row);
				PredictProcessRow(row);
				if (_lfLevel != 0 && row != 0) LoopFilterProcessRow(row - 1);
				if (++partition == _partitions) partition = 0;
			}
			if (_lfLevel != 0) LoopFilterProcessRow(_mbRows - 1);

			if (!_refreshEntropy) _entropy.CopyFrom(_savedEntropy);

			// The reference frames
			if (_copyArf == 1) Swap(AltrefFrame, _refFrames[LastFrame]);
			else if (_copyArf == 2) Swap(AltrefFrame, _refFrames[GoldenFrame]);
			if (_copyGf == 1) Swap(GoldenFrame, _refFrames[LastFrame]);
			else if (_copyGf == 2) Swap(GoldenFrame, _refFrames[AltrefFrame]);
			if (_refreshGf) Swap(GoldenFrame, _refFrames[CurrentFrame]);
			if (_refreshArf) Swap(AltrefFrame, _refFrames[CurrentFrame]);
			if (_refreshLast) Swap(LastFrame, _refFrames[CurrentFrame]);
			return true;
		}

		private void Swap(int slot, Image to)
		{
			if (_refFrames[slot] != null) _refFrames[slot].RefCount--;
			_refFrames[slot] = to;
			if (to != null) to.RefCount++;
		}

		private void DecodeEntropyHeader(ref BoolDecoder b)
		{
			byte[] coeff = _entropy.CoeffProbs, update = Vp8Tables.CoeffUpdateProbs;
			for (int i = 0; i < coeff.Length; i++)
				if (b.Get(update[i]) != 0) coeff[i] = (byte)b.Uint(8);
			_entropy.CoeffSkipEnabled = b.Bit() != 0;
			if (_entropy.CoeffSkipEnabled) _entropy.CoeffSkipProb = (byte)b.Uint(8);
			if (!_keyframe)
			{
				_entropy.ProbInter = (byte)b.Uint(8);
				_entropy.ProbLast = (byte)b.Uint(8);
				_entropy.ProbGf = (byte)b.Uint(8);
				if (b.Bit() != 0) for (int i = 0; i < 4; i++) _entropy.YModeProbs[i] = (byte)b.Uint(8);
				if (b.Bit() != 0) for (int i = 0; i < 3; i++) _entropy.UvModeProbs[i] = (byte)b.Uint(8);
				for (int i = 0; i < 2 * MvProbCount; i++)
					if (b.Get(Vp8Tables.MvUpdateProbs[i]) != 0)
					{
						int x = b.Uint(7);
						_entropy.MvProbs[i] = (byte)(x != 0 ? x << 1 : 1);
					}
			}
		}

		private void DecodeReferenceHeader(ref BoolDecoder b)
		{
			bool key = _keyframe;
			_refreshGf = key || b.Bit() != 0;
			_refreshArf = key || b.Bit() != 0;
			_copyGf = key ? 0 : !_refreshGf ? b.Uint(2) : 0;
			_copyArf = key ? 0 : !_refreshArf ? b.Uint(2) : 0;
			_signBias[GoldenFrame] = key ? 0 : b.Bit();
			_signBias[AltrefFrame] = key ? 0 : b.Bit();
			_refreshEntropy = b.Bit() != 0;
			_refreshLast = key || b.Bit() != 0;
		}

		private void DecodeQuantizerHeader(ref BoolDecoder b)
		{
			int lastQ = _qIndex;
			_qIndex = b.Uint(7);
			bool update = lastQ != _qIndex;
			update |= (_y1DcDelta = b.MaybeInt(4)) != 0;
			update |= (_y2DcDelta = b.MaybeInt(4)) != 0;
			update |= (_y2AcDelta = b.MaybeInt(4)) != 0;
			update |= (_uvDcDelta = b.MaybeInt(4)) != 0;
			update |= (_uvAcDelta = b.MaybeInt(4)) != 0;
			_qDeltaUpdate = update;
		}

		private bool DecodeTokenPartitions(ref BoolDecoder b, byte* data, int size)
		{
			_partitions = 1 << b.Uint(2);
			if (size < 3 * (_partitions - 1)) return false;
			byte* sizes = data;
			data += 3 * (_partitions - 1);
			size -= 3 * (_partitions - 1);
			for (int i = 0; i < _partitions; i++)
			{
				int partSize = i < _partitions - 1 ? (sizes[i * 3 + 2] << 16) | (sizes[i * 3 + 1] << 8) | sizes[i * 3] : size;
				if (size < partSize) return false;
				_tokens[i].Bool.Init(data, partSize);
				data += partSize;
				size -= partSize;
			}
			return true;
		}

		private void DecodeLoopFilterHeader(ref BoolDecoder b)
		{
			if (_keyframe)
			{
				Array.Clear(_lfRefDelta);
				Array.Clear(_lfModeDelta);
			}
			_lfSimple = b.Bit() != 0;
			_lfLevel = b.Uint(6);
			_lfSharpness = b.Uint(3);
			_lfDeltaEnabled = b.Bit() != 0;
			if (_lfDeltaEnabled && b.Bit() != 0)
			{
				for (int i = 0; i < 4; i++) _lfRefDelta[i] = b.MaybeInt(6);
				for (int i = 0; i < 4; i++) _lfModeDelta[i] = b.MaybeInt(6);
			}
		}

		private void DecodeSegmentationHeader(ref BoolDecoder b)
		{
			if (_keyframe)
			{
				_segEnabled = _segUpdateData = _segUpdateMap = _segAbs = false;
				_segTreeProbs[0] = _segTreeProbs[1] = _segTreeProbs[2] = 0;
				Array.Clear(_segLfLevel);
				Array.Clear(_segQuantIdx);
			}
			_segEnabled = b.Bit() != 0;
			if (_segEnabled)
			{
				_segUpdateMap = b.Bit() != 0;
				_segUpdateData = b.Bit() != 0;
				if (_segUpdateData)
				{
					_segAbs = b.Bit() != 0;
					for (int i = 0; i < 4; i++) _segQuantIdx[i] = b.MaybeInt(7);
					for (int i = 0; i < 4; i++) _segLfLevel[i] = b.MaybeInt(6);
				}
				if (_segUpdateMap)
					for (int i = 0; i < 3; i++) _segTreeProbs[i] = (byte)(b.Bit() != 0 ? b.Uint(8) : 255);
			}
			else
			{
				_segUpdateMap = false;
				_segUpdateData = false;
			}
		}

		private static int ClampQ(int q) => q < 0 ? 0 : q > 127 ? 127 : q;

		private void DequantInit()
		{
			for (int i = 0; i < (_segEnabled ? 4 : 1); i++)
			{
				int q = _qIndex;
				if (_segEnabled) q = !_segAbs ? q + _segQuantIdx[i] : _segQuantIdx[i];
				if (_dqQuantIdx[i] != q || _qDeltaUpdate)
				{
					_dqFactor[i, TokenBlockY1, 0] = Vp8Tables.DcQ[ClampQ(q + _y1DcDelta)];
					_dqFactor[i, TokenBlockY1, 1] = Vp8Tables.AcQ[ClampQ(q)];
					_dqFactor[i, TokenBlockUv, 0] = Vp8Tables.DcQ[ClampQ(q + _uvDcDelta)];
					_dqFactor[i, TokenBlockUv, 1] = Vp8Tables.AcQ[ClampQ(q + _uvAcDelta)];
					_dqFactor[i, TokenBlockY2, 0] = (short)(Vp8Tables.DcQ[ClampQ(q + _y2DcDelta)] * 2);
					_dqFactor[i, TokenBlockY2, 1] = (short)(Vp8Tables.AcQ[ClampQ(q + _y2AcDelta)] * 155 / 100);
					if (_dqFactor[i, TokenBlockY2, 1] < 8) _dqFactor[i, TokenBlockY2, 1] = 8;
					if (_dqFactor[i, TokenBlockUv, 0] > 132) _dqFactor[i, TokenBlockUv, 0] = 132;
					_dqQuantIdx[i] = q;
				}
			}
		}

		// ---- modes and motion vectors (modemv.c) ----

		private MbInfo* Row(int row) => _mbInfoStorage + 1 + (row + 1) * _mbInfoWidth;

		private void ModeMvInit()
		{
			int w = _mbCols + 1, h = _mbRows + 1;
			if (_sizeUpdated && _mbInfoStorage != null)
			{
				NativeMemory.Free(_mbInfoStorage);
				_mbInfoStorage = null;
			}
			if (_mbInfoStorage == null) _mbInfoStorage = (MbInfo*)NativeMemory.AllocZeroed((nuint)(w * h), (nuint)sizeof(MbInfo));
			_mbInfoWidth = w;
		}

		private struct MvBounds { public int ToLeft, ToRight, ToTop, ToBottom; }

		private static Mv ClampMv(Mv raw, in MvBounds b)
		{
			Mv m;
			m.X = raw.X < b.ToLeft ? (short)b.ToLeft : raw.X;
			m.X = raw.X > b.ToRight ? (short)b.ToRight : m.X;
			m.Y = raw.Y < b.ToTop ? (short)b.ToTop : raw.Y;
			m.Y = raw.Y > b.ToBottom ? (short)b.ToBottom : m.Y;
			return m;
		}

		private int ReadSegmentId(ref BoolDecoder b) =>
			b.Get(_segTreeProbs[0]) != 0 ? 2 + b.Get(_segTreeProbs[2]) : b.Get(_segTreeProbs[1]);

		private static int AboveBlockMode(MbInfo* self, MbInfo* above, int b)
		{
			if (b < 4)
			{
				switch (above->YMode)
				{
					case DcPred: return BDcPred;
					case VPred: return BVePred;
					case HPred: return BHePred;
					case TmPred: return BTmPred;
					case BPred: return above->Modes[b + 12];
					default: return BDcPred;
				}
			}
			return self->Modes[b - 4];
		}

		private static int LeftBlockMode(MbInfo* self, MbInfo* left, int b)
		{
			if ((b & 3) == 0)
			{
				switch (left->YMode)
				{
					case DcPred: return BDcPred;
					case VPred: return BVePred;
					case HPred: return BHePred;
					case TmPred: return BTmPred;
					case BPred: return left->Modes[b + 3];
					default: return BDcPred;
				}
			}
			return self->Modes[b - 1];
		}

		private static void DecodeKfMbMode(MbInfo* self, MbInfo* left, MbInfo* above, ref BoolDecoder b)
		{
			fixed (byte* yProbs = KfYModeProbs, uvProbs = KfUvModeProbs, bProbs = Vp8Tables.KfBModeProbs)
			{
				int yMode = b.Tree(KfYModeTree, yProbs);
				if (yMode == BPred)
				{
					for (int i = 0; i < 16; i++)
					{
						int a = AboveBlockMode(self, above, i), l = LeftBlockMode(self, left, i);
						self->Modes[i] = (byte)b.Tree(BModeTree, bProbs + a * 90 + l * 9);
					}
				}
				int uvMode = b.Tree(UvModeTree, uvProbs);
				self->YMode = (byte)yMode;
				self->UvMode = (byte)uvMode;
				self->Mv = default;
				self->RefFrame = 0;
			}
		}

		private void DecodeIntraMbMode(MbInfo* self, ref BoolDecoder b)
		{
			fixed (byte* yProbs = _entropy.YModeProbs, uvProbs = _entropy.UvModeProbs, bProbs = DefaultBModeProbs)
			{
				int yMode = b.Tree(YModeTree, yProbs);
				if (yMode == BPred)
					for (int i = 0; i < 16; i++) self->Modes[i] = (byte)b.Tree(BModeTree, bProbs);
				int uvMode = b.Tree(UvModeTree, uvProbs);
				self->YMode = (byte)yMode;
				self->UvMode = (byte)uvMode;
				self->Mv = default;
				self->RefFrame = CurrentFrame;
			}
		}

		private static int ReadMvComponent(ref BoolDecoder b, byte* mvc)
		{
			const int IsShort = 0, Sign = 1, Short = 2, Bits = Short + 8 - 1, LongWidth = 10;
			int x = 0;
			if (b.Get(mvc[IsShort]) != 0)
			{
				for (int i = 0; i < 3; i++) x += b.Get(mvc[Bits + i]) << i;
				for (int i = LongWidth - 1; i > 3; i--) x += b.Get(mvc[Bits + i]) << i;
				if ((x & 0xFFF0) == 0 || b.Get(mvc[Bits + 3]) != 0) x += 8;
			}
			else x = b.Tree(SmallMvTree, mvc + Short);
			if (x != 0 && b.Get(mvc[Sign]) != 0) x = -x;
			return x << 1;
		}

		private void ReadMv(ref BoolDecoder b, ref Mv mv)
		{
			fixed (byte* probs = _entropy.MvProbs)
			{
				mv.Y = (short)ReadMvComponent(ref b, probs);
				mv.X = (short)ReadMvComponent(ref b, probs + MvProbCount);
			}
		}

		private static Mv AboveBlockMv(MbInfo* self, MbInfo* above, int b)
		{
			if (b < 4)
			{
				if (above->YMode == SplitMv) return above->SplitMv(b + 12);
				return above->Mv;
			}
			return self->SplitMv(b - 4);
		}

		private static Mv LeftBlockMv(MbInfo* self, MbInfo* left, int b)
		{
			if ((b & 3) == 0)
			{
				if (left->YMode == SplitMv) return left->SplitMv(b + 3);
				return left->Mv;
			}
			return self->SplitMv(b - 1);
		}

		private static int SubmvRef(ref BoolDecoder b, Mv l, Mv a)
		{
			bool lez = l.Raw == 0, aez = a.Raw == 0, lea = l.Raw == a.Raw;
			int ctx = 0;
			if (lea && lez) ctx = 4;
			else if (lea) ctx = 3;
			else if (aez) ctx = 2;
			else if (lez) ctx = 1;
			fixed (byte* probs = SubmvRefProbs2) return b.Tree(SubmvRefTree, probs + ctx * 3);
		}

		private void MvBias(MbInfo* mb, int refFrame, ref Mv mv)
		{
			if ((_signBias[mb->RefFrame] ^ _signBias[refFrame]) != 0)
			{
				mv.X = (short)-mv.X;
				mv.Y = (short)-mv.Y;
			}
		}

		private const int CntBest = 0, CntZeroZero = 0, CntNearest = 1, CntNear = 2, CntSplitMv = 3;

		private void FindNearMvs(MbInfo* self, MbInfo* left, MbInfo* above, Mv* nearMvs, int* cnt)
		{
			MbInfo* aboveLeft = above - 1;
			int mv = 0;   // index into nearMvs, as dixie's pointer
			int cntx = 0;
			nearMvs[0] = nearMvs[1] = nearMvs[2] = default;
			nearMvs[3] = default;
			cnt[0] = cnt[1] = cnt[2] = cnt[3] = 0;
			if (above->RefFrame != CurrentFrame)
			{
				if (above->Mv.Raw != 0)
				{
					nearMvs[++mv] = above->Mv;
					MvBias(above, self->RefFrame, ref nearMvs[mv]);
					++cntx;
				}
				cnt[cntx] += 2;
			}
			if (left->RefFrame != CurrentFrame)
			{
				if (left->Mv.Raw != 0)
				{
					Mv thisMv = left->Mv;
					MvBias(left, self->RefFrame, ref thisMv);
					if (thisMv.Raw != nearMvs[mv].Raw)
					{
						nearMvs[++mv] = thisMv;
						++cntx;
					}
					cnt[cntx] += 2;
				}
				else cnt[CntZeroZero] += 2;
			}
			if (aboveLeft->RefFrame != CurrentFrame)
			{
				if (aboveLeft->Mv.Raw != 0)
				{
					Mv thisMv = aboveLeft->Mv;
					MvBias(aboveLeft, self->RefFrame, ref thisMv);
					if (thisMv.Raw != nearMvs[mv].Raw)
					{
						nearMvs[++mv] = thisMv;
						++cntx;
					}
					cnt[cntx] += 1;
				}
				else cnt[CntZeroZero] += 1;
			}
			if (cnt[CntSplitMv] != 0)
			{
				if (nearMvs[mv].Raw == nearMvs[CntNearest].Raw) cnt[CntNearest] += 1;
			}
			cnt[CntSplitMv] = ((above->YMode == SplitMv ? 1 : 0) + (left->YMode == SplitMv ? 1 : 0)) * 2 + (aboveLeft->YMode == SplitMv ? 1 : 0);
			if (cnt[CntNear] > cnt[CntNearest])
			{
				int tmp = cnt[CntNearest];
				cnt[CntNearest] = cnt[CntNear];
				cnt[CntNear] = tmp;
				Mv t = nearMvs[CntNearest];
				nearMvs[CntNearest] = nearMvs[CntNear];
				nearMvs[CntNear] = t;
			}
			if (cnt[CntNearest] >= cnt[CntBest]) nearMvs[CntBest] = nearMvs[CntNearest];
		}

		private void DecodeSplitMv(MbInfo* self, MbInfo* left, MbInfo* above, Mv bestMv, ref BoolDecoder b)
		{
			int partitionId;
			fixed (byte* probs = SplitMvProbs) partitionId = b.Tree(SplitMvTree, probs);
			self->Partitioning = (byte)partitionId;
			for (int j = 0, mask = 0; mask < 65535; j++)
			{
				int k = 0;
				while (j != MvPartitions[partitionId, k]) k++;
				Mv leftMv = LeftBlockMv(self, left, k), aboveMv = AboveBlockMv(self, above, k);
				Mv mv = default;
				switch (SubmvRef(ref b, leftMv, aboveMv))
				{
					case Left4x4: mv = leftMv; break;
					case Above4x4: mv = aboveMv; break;
					case Zero4x4: mv = default; break;
					case New4x4:
						ReadMv(ref b, ref mv);
						mv.X += bestMv.X;
						mv.Y += bestMv.Y;
						break;
				}
				for (; k < 16; k++)
					if (j == MvPartitions[partitionId, k])
					{
						self->SetSplitMv(k, mv);
						mask |= 1 << k;
					}
			}
		}

		private static bool NeedMcBorder(Mv mv, int l, int t, int bw, int w, int h)
		{
			l += mv.X >> 3;
			t += mv.Y >> 3;
			int r = w - (l + bw), bb = h - (t + bw);
			return (l >> 1) < 2 || (r >> 1) < 3 || (t >> 1) < 2 || (bb >> 1) < 3;
		}

		private void DecodeMvs(MbInfo* self, MbInfo* left, MbInfo* above, in MvBounds bounds, ref BoolDecoder b)
		{
			Mv* nearMvs = stackalloc Mv[4];
			int* mvCnts = stackalloc int[4];
			byte* probs = stackalloc byte[4];
			const int Best = 0, Nearest = 1, Near = 2;
			self->RefFrame = (byte)(b.Get(_entropy.ProbLast) != 0 ? 2 + b.Get(_entropy.ProbGf) : 1);
			FindNearMvs(self, self - 1, above, nearMvs, mvCnts);
			probs[0] = MvCountsToProbs[mvCnts[0], 0];
			probs[1] = MvCountsToProbs[mvCnts[1], 1];
			probs[2] = MvCountsToProbs[mvCnts[2], 2];
			probs[3] = MvCountsToProbs[mvCnts[3], 3];
			self->YMode = (byte)b.Tree(MvRefTree, probs);
			self->UvMode = self->YMode;
			self->NeedMcBorder = 0;
			int x = (-bounds.ToLeft - 128) >> 3, y = (-bounds.ToTop - 128) >> 3, w = _mbCols * 16, h = _mbRows * 16;
			switch (self->YMode)
			{
				case NearestMv:
					self->Mv = ClampMv(nearMvs[Nearest], bounds);
					break;
				case NearMv:
					self->Mv = ClampMv(nearMvs[Near], bounds);
					break;
				case ZeroMv:
					self->Mv = default;
					return;
				case NewMv:
				{
					Mv clampedBest = ClampMv(nearMvs[Best], bounds);
					Mv mv = default;
					ReadMv(ref b, ref mv);
					mv.X += clampedBest.X;
					mv.Y += clampedBest.Y;
					self->Mv = mv;
					break;
				}
				case SplitMv:
				{
					Mv* chromaMv = stackalloc Mv[4];
					int* cx = stackalloc int[4];
					int* cy = stackalloc int[4];
					Mv clampedBest = ClampMv(nearMvs[Best], bounds);
					DecodeSplitMv(self, left, above, clampedBest, ref b);
					self->Mv = self->SplitMv(15);
					for (int bl = 0; bl < 16; bl++)
					{
						int c = ((bl >> 1) & 1) + ((bl >> 2) & 2);
						cx[c] += self->MvX[bl];
						cy[c] += self->MvY[bl];
						if (NeedMcBorder(self->SplitMv(bl), x + (bl & 3) * 4, y + (bl & ~3), 4, w, h))
						{
							self->NeedMcBorder = 1;
							break;
						}
					}
					for (int bl = 0; bl < 4; bl++)
					{
						// dixie keeps the sums in 16 bits as it goes
						short sx = (short)cx[bl], sy = (short)cy[bl];
						int tx = sx + 4 + 8 * (sx >> 31), ty = sy + 4 + 8 * (sy >> 31);
						chromaMv[bl].X = (short)(tx / 4);
						chromaMv[bl].Y = (short)(ty / 4);
						if (NeedMcBorder(chromaMv[bl], x + (bl & 1) * 8, y + (bl >> 1) * 8, 16, w, h))
						{
							self->NeedMcBorder = 1;
							break;
						}
					}
					return;
				}
			}
			if (NeedMcBorder(self->Mv, x, y, 16, w, h)) self->NeedMcBorder = 1;
		}

		private void ModeMvProcessRow(ref BoolDecoder b, int row)
		{
			MbInfo* self = Row(row), above = Row(row - 1);
			MvBounds bounds;
			bounds.ToLeft = -(1 << 7);
			bounds.ToRight = _mbCols << 7;
			bounds.ToTop = -((row + 1) << 7);
			bounds.ToBottom = (_mbRows - row) << 7;
			for (int col = 0; col < _mbCols; col++)
			{
				if (_segUpdateMap) self->SegmentId = (byte)ReadSegmentId(ref b);
				// libvpx: no skip flag without the skip probability (dixie would keep the frame before's)
				self->SkipCoeff = _entropy.CoeffSkipEnabled ? (byte)b.Get(_entropy.CoeffSkipProb) : (byte)0;
				if (_keyframe)
				{
					if (!_segUpdateMap) self->SegmentId = 0;
					DecodeKfMbMode(self, self - 1, above, ref b);
				}
				else
				{
					if (b.Get(_entropy.ProbInter) != 0) DecodeMvs(self, self - 1, above, bounds, ref b);
					else DecodeIntraMbMode(self, ref b);
					bounds.ToLeft -= 16 << 3;
					bounds.ToRight -= 16 << 3;
				}
				self++;
				above++;
			}
		}

		// ---- tokens (tokens.c) ----

		private static readonly int[] LeftContextIndex = { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8 };
		private static readonly int[] AboveContextIndex = { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7, 8 };
		private static readonly int[] BandsX = { 0 * 33, 1 * 33, 2 * 33, 3 * 33, 6 * 33, 4 * 33, 5 * 33, 6 * 33, 6 * 33, 6 * 33, 6 * 33, 6 * 33, 6 * 33, 6 * 33, 6 * 33, 7 * 33 };
		private static readonly int[] Zigzag = { 0, 1, 4, 8, 5, 2, 3, 6, 9, 12, 13, 10, 7, 11, 14, 15 };
		private static readonly byte[] Cat1 = { 159 }, Cat2 = { 145, 165 }, Cat3 = { 140, 148, 173 }, Cat4 = { 135, 140, 155, 176 }, Cat5 = { 130, 134, 141, 157, 180 };
		private static readonly byte[] Cat6 = { 129, 130, 133, 140, 153, 177, 196, 230, 243, 254, 254 };

		private const int EobNode = 0, ZeroNode = 1, OneNode = 2, LowValNode = 3, TwoNode = 4, ThreeNode = 5, HighLowNode = 6, CatOneNode = 7, CatThreeFourNode = 8, CatThreeNode = 9, CatFiveNode = 10;

		private static int ExtraBits(ref BoolDecoder b, byte[] probs, int min)
		{
			int val = min;
			for (int bits = probs.Length - 1; bits >= 0; bits--) val += b.Get(probs[bits]) << bits;
			return val;
		}

		/// <summary>decode_mb_tokens, its gotos as a loop: the coefficients of the macroblock's 25 blocks (Y2 first when
		/// there is one), dequantized into place; the eob mask (bit 31 set when anything is non-zero).</summary>
		private int DecodeMbTokens(ref BoolDecoder b, int[] left, int* above, short* tokens, int mode, byte* probs, int segment)
		{
			int i, stop, type;
			int eobMask = 0;
			short* bTokens;
			int dqType;
			if (mode != BPred && mode != SplitMv)
			{
				i = 24;
				stop = 24;
				type = 1;
				bTokens = tokens + 24 * 16;
				dqType = TokenBlockY2;
			}
			else
			{
				i = 0;
				stop = 16;
				type = 3;
				bTokens = tokens;
				dqType = TokenBlockY1;
			}
			while (true)
			{
				byte* typeProbs = probs + type * 264;
				int t = left[LeftContextIndex[i]] + above[AboveContextIndex[i]];
				int c = type == 0 ? 1 : 0;
				byte* prob = typeProbs + t * 11 + BandsX[c];
				int dc = _dqFactor[segment, dqType, 0], ac = _dqFactor[segment, dqType, 1];
				bool checkEob = true;
				while (true)
				{
					if (checkEob && b.Get(prob[EobNode]) == 0) break;
					if (b.Get(prob[ZeroNode]) == 0)
					{
						// a zero: the next coefficient (no end of block straight after a zero)
						if (c >= 15) { c = 16; break; }
						++c;
						prob = typeProbs + BandsX[c];
						checkEob = false;
						continue;
					}
					int v, nextCtx;
					if (b.Get(prob[OneNode]) == 0)
					{
						v = 1;
						nextCtx = 1;
					}
					else
					{
						nextCtx = 2;
						if (b.Get(prob[LowValNode]) == 0)
						{
							if (b.Get(prob[TwoNode]) == 0) v = 2;
							else v = b.Get(prob[ThreeNode]) == 0 ? 3 : 4;
						}
						else if (b.Get(prob[HighLowNode]) == 0)
							v = b.Get(prob[CatOneNode]) == 0 ? ExtraBits(ref b, Cat1, 5) : ExtraBits(ref b, Cat2, 7);
						else if (b.Get(prob[CatThreeFourNode]) == 0)
							v = b.Get(prob[CatThreeNode]) == 0 ? ExtraBits(ref b, Cat3, 11) : ExtraBits(ref b, Cat4, 19);
						else
							v = b.Get(prob[CatFiveNode]) == 0 ? ExtraBits(ref b, Cat5, 35) : ExtraBits(ref b, Cat6, 67);
					}
					v = (b.Bit() != 0 ? -v : v) * (c != 0 ? ac : dc);
					if (c < 15)
					{
						bTokens[Zigzag[c]] = (short)v;
						++c;
						prob = typeProbs + nextCtx * 11 + BandsX[c];
						checkEob = true;
						continue;
					}
					bTokens[Zigzag[15]] = (short)v;
					c = 16;
					break;
				}
				// BLOCK_FINISHED
				eobMask |= (c > 1 ? 1 : 0) << i;
				int nonzero = c != (type == 0 ? 1 : 0) ? 1 : 0;
				eobMask |= nonzero << 31;
				left[LeftContextIndex[i]] = above[AboveContextIndex[i]] = nonzero;
				bTokens += 16;
				i++;
				if (i < stop) continue;
				if (i == 25)
				{
					type = 0;
					i = 0;
					stop = 16;
					bTokens = tokens;
					dqType = TokenBlockY1;
					continue;
				}
				if (i == 16)
				{
					type = 2;
					stop = 24;
					dqType = TokenBlockUv;
					continue;
				}
				return eobMask;
			}
		}

		private void TokensInit()
		{
			if (_sizeUpdated || _aboveTokenContext == null)
			{
				for (int i = 0; i < 8; i++)
				{
					if (_tokens[i].Coeffs != null) NativeMemory.AlignedFree(_tokens[i].Coeffs);
					_tokens[i].Coeffs = (short*)NativeMemory.AlignedAlloc((nuint)(_mbCols * 25 * 16 * sizeof(short)), 16);
				}
				if (_aboveTokenContext != null) NativeMemory.Free(_aboveTokenContext);
				_aboveTokenContext = (int*)NativeMemory.AllocZeroed((nuint)(_mbCols * 9), sizeof(int));
			}
		}

		private void TokensProcessRow(int partition, int row)
		{
			TokenPartition tp = _tokens[partition];
			short* coeffs = tp.Coeffs;
			int* above = _aboveTokenContext;
			int[] left = tp.Left;
			MbInfo* mbi = Row(row);
			if (row == 0) NativeMemory.Clear(above, (nuint)(_mbCols * 9 * sizeof(int)));
			Array.Clear(left);
			fixed (byte* probs = _entropy.CoeffProbs)
			{
				for (int col = 0; col < _mbCols; col++)
				{
					NativeMemory.Clear(coeffs, 25 * 16 * sizeof(short));
					if (mbi->SkipCoeff != 0)
					{
						// reset_mb_context: Y, U, V, and Y2 only where the mode has one
						for (int k = 0; k < 8; k++) { left[k] = 0; above[k] = 0; }
						if (mbi->YMode != BPred && mbi->YMode != SplitMv) { left[8] = 0; above[8] = 0; }
						mbi->EobMask = 0;
					}
					else mbi->EobMask = DecodeMbTokens(ref tp.Bool, left, above, coeffs, mbi->YMode, probs, _segEnabled ? mbi->SegmentId : 0);
					above += 9;
					mbi++;
					coeffs += 25 * 16;
				}
			}
		}

		// ---- the inverse transforms (idct_add.c) ----

		private static void Walsh(short* input, short* output)
		{
			short* ip = input, op = output;
			for (int i = 0; i < 4; i++)
			{
				int a1 = ip[0] + ip[12], b1 = ip[4] + ip[8], c1 = ip[4] - ip[8], d1 = ip[0] - ip[12];
				op[0] = (short)(a1 + b1);
				op[4] = (short)(c1 + d1);
				op[8] = (short)(a1 - b1);
				op[12] = (short)(d1 - c1);
				ip++;
				op++;
			}
			ip = output;
			op = output;
			for (int i = 0; i < 4; i++)
			{
				int a1 = ip[0] + ip[3], b1 = ip[1] + ip[2], c1 = ip[1] - ip[2], d1 = ip[0] - ip[3];
				int a2 = a1 + b1, b2 = c1 + d1, c2 = a1 - b1, d2 = d1 - c1;
				op[0] = (short)((a2 + 3) >> 3);
				op[1] = (short)((b2 + 3) >> 3);
				op[2] = (short)((c2 + 3) >> 3);
				op[3] = (short)((d2 + 3) >> 3);
				ip += 4;
				op += 4;
			}
		}

		private const int CosPi8Sqrt2Minus1 = 20091, SinPi8Sqrt2 = 35468;

		private static byte Clamp255(int x) => (byte)(x < 0 ? 0 : x > 255 ? 255 : x);

		private static void IdctAdd(byte* recon, byte* predict, int stride, short* coeffs)
		{
			short* tmp = stackalloc short[16];
			short* ip = coeffs, op = tmp;
			for (int i = 0; i < 4; i++)
			{
				int a1 = ip[0] + ip[8], b1 = ip[0] - ip[8];
				int temp1 = (ip[4] * SinPi8Sqrt2) >> 16;
				int temp2 = ip[12] + ((ip[12] * CosPi8Sqrt2Minus1) >> 16);
				int c1 = temp1 - temp2;
				temp1 = ip[4] + ((ip[4] * CosPi8Sqrt2Minus1) >> 16);
				temp2 = (ip[12] * SinPi8Sqrt2) >> 16;
				int d1 = temp1 + temp2;
				op[0] = (short)(a1 + d1);
				op[12] = (short)(a1 - d1);
				op[4] = (short)(b1 + c1);
				op[8] = (short)(b1 - c1);
				ip++;
				op++;
			}
			short* cf = tmp;
			for (int i = 0; i < 4; i++)
			{
				int a1 = cf[0] + cf[2], b1 = cf[0] - cf[2];
				int temp1 = (cf[1] * SinPi8Sqrt2) >> 16;
				int temp2 = cf[3] + ((cf[3] * CosPi8Sqrt2Minus1) >> 16);
				int c1 = temp1 - temp2;
				temp1 = cf[1] + ((cf[1] * CosPi8Sqrt2Minus1) >> 16);
				temp2 = (cf[3] * SinPi8Sqrt2) >> 16;
				int d1 = temp1 + temp2;
				recon[0] = Clamp255(predict[0] + ((a1 + d1 + 4) >> 3));
				recon[3] = Clamp255(predict[3] + ((a1 - d1 + 4) >> 3));
				recon[1] = Clamp255(predict[1] + ((b1 + c1 + 4) >> 3));
				recon[2] = Clamp255(predict[2] + ((b1 - c1 + 4) >> 3));
				cf += 4;
				recon += stride;
				predict += stride;
			}
		}

		// ---- prediction (predict.c) ----

		private static void PredictHNxN(byte* predict, int stride, int n)
		{
			byte* left = predict - 1;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++) predict[i * stride + j] = left[i * stride];
		}

		private static void PredictVNxN(byte* predict, int stride, int n)
		{
			byte* above = predict - stride;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++) predict[i * stride + j] = above[j];
		}

		private static void PredictTmNxN(byte* predict, int stride, int n)
		{
			byte* left = predict - 1;
			byte* above = predict - stride;
			int p = above[-1];
			for (int j = 0; j < n; j++)
			{
				for (int i = 0; i < n; i++) predict[i] = Clamp255(*left + above[i] - p);
				predict += stride;
				left += stride;
			}
		}

		private static void PredictDcNxN(byte* predict, int stride, int n)
		{
			byte* left = predict - 1;
			byte* above = predict - stride;
			int dc = 0;
			for (int i = 0; i < n; i++)
			{
				dc += *left + above[i];
				left += stride;
			}
			dc = n == 16 ? (dc + 16) >> 5 : n == 8 ? (dc + 8) >> 4 : (dc + 4) >> 3;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++) predict[i * stride + j] = (byte)dc;
		}

		private static void PredictVe4x4(byte* p, int s)
		{
			byte* a = p - s;
			p[0] = (byte)((a[-1] + 2 * a[0] + a[1] + 2) >> 2);
			p[1] = (byte)((a[0] + 2 * a[1] + a[2] + 2) >> 2);
			p[2] = (byte)((a[1] + 2 * a[2] + a[3] + 2) >> 2);
			p[3] = (byte)((a[2] + 2 * a[3] + a[4] + 2) >> 2);
			for (int i = 1; i < 4; i++)
				for (int j = 0; j < 4; j++) p[i * s + j] = p[j];
		}

		private static void PredictHe4x4(byte* p, int s)
		{
			byte* l = p - 1;
			byte v = (byte)((l[-s] + 2 * l[0] + l[s] + 2) >> 2);
			p[0] = p[1] = p[2] = p[3] = v;
			p += s; l += s;
			v = (byte)((l[-s] + 2 * l[0] + l[s] + 2) >> 2);
			p[0] = p[1] = p[2] = p[3] = v;
			p += s; l += s;
			v = (byte)((l[-s] + 2 * l[0] + l[s] + 2) >> 2);
			p[0] = p[1] = p[2] = p[3] = v;
			p += s; l += s;
			v = (byte)((l[-s] + 2 * l[0] + l[0] + 2) >> 2);
			p[0] = p[1] = p[2] = p[3] = v;
		}

		private static void PredictLd4x4(byte* p, int s)
		{
			byte* a = p - s;
			byte p0 = (byte)((a[0] + 2 * a[1] + a[2] + 2) >> 2), p1 = (byte)((a[1] + 2 * a[2] + a[3] + 2) >> 2);
			byte p2 = (byte)((a[2] + 2 * a[3] + a[4] + 2) >> 2), p3 = (byte)((a[3] + 2 * a[4] + a[5] + 2) >> 2);
			byte p4 = (byte)((a[4] + 2 * a[5] + a[6] + 2) >> 2), p5 = (byte)((a[5] + 2 * a[6] + a[7] + 2) >> 2);
			byte p6 = (byte)((a[6] + 2 * a[7] + a[7] + 2) >> 2);
			p[0] = p0; p[1] = p1; p[2] = p2; p[3] = p3; p += s;
			p[0] = p1; p[1] = p2; p[2] = p3; p[3] = p4; p += s;
			p[0] = p2; p[1] = p3; p[2] = p4; p[3] = p5; p += s;
			p[0] = p3; p[1] = p4; p[2] = p5; p[3] = p6;
		}

		private static void PredictRd4x4(byte* p, int s)
		{
			byte* l = p - 1;
			byte* a = p - s;
			byte p0 = (byte)((l[0] + 2 * a[-1] + a[0] + 2) >> 2), p1 = (byte)((a[-1] + 2 * a[0] + a[1] + 2) >> 2);
			byte p2 = (byte)((a[0] + 2 * a[1] + a[2] + 2) >> 2), p3 = (byte)((a[1] + 2 * a[2] + a[3] + 2) >> 2);
			byte p4 = (byte)((l[s] + 2 * l[0] + a[-1] + 2) >> 2), p5 = (byte)((l[s * 2] + 2 * l[s] + l[0] + 2) >> 2);
			byte p6 = (byte)((l[s * 3] + 2 * l[s * 2] + l[s] + 2) >> 2);
			p[0] = p0; p[1] = p1; p[2] = p2; p[3] = p3; p += s;
			p[0] = p4; p[1] = p0; p[2] = p1; p[3] = p2; p += s;
			p[0] = p5; p[1] = p4; p[2] = p0; p[3] = p1; p += s;
			p[0] = p6; p[1] = p5; p[2] = p4; p[3] = p0;
		}

		private static void PredictVr4x4(byte* p, int s)
		{
			byte* l = p - 1;
			byte* a = p - s;
			byte p0 = (byte)((a[-1] + a[0] + 1) >> 1), p1 = (byte)((a[0] + a[1] + 1) >> 1), p2 = (byte)((a[1] + a[2] + 1) >> 1), p3 = (byte)((a[2] + a[3] + 1) >> 1);
			byte p4 = (byte)((l[0] + 2 * a[-1] + a[0] + 2) >> 2), p5 = (byte)((a[-1] + 2 * a[0] + a[1] + 2) >> 2);
			byte p6 = (byte)((a[0] + 2 * a[1] + a[2] + 2) >> 2), p7 = (byte)((a[1] + 2 * a[2] + a[3] + 2) >> 2);
			byte p8 = (byte)((l[s] + 2 * l[0] + a[-1] + 2) >> 2), p9 = (byte)((l[s * 2] + 2 * l[s] + l[0] + 2) >> 2);
			p[0] = p0; p[1] = p1; p[2] = p2; p[3] = p3; p += s;
			p[0] = p4; p[1] = p5; p[2] = p6; p[3] = p7; p += s;
			p[0] = p8; p[1] = p0; p[2] = p1; p[3] = p2; p += s;
			p[0] = p9; p[1] = p4; p[2] = p5; p[3] = p6;
		}

		private static void PredictVl4x4(byte* p, int s)
		{
			byte* a = p - s;
			byte p0 = (byte)((a[0] + a[1] + 1) >> 1), p1 = (byte)((a[1] + a[2] + 1) >> 1), p2 = (byte)((a[2] + a[3] + 1) >> 1), p3 = (byte)((a[3] + a[4] + 1) >> 1);
			byte p4 = (byte)((a[0] + 2 * a[1] + a[2] + 2) >> 2), p5 = (byte)((a[1] + 2 * a[2] + a[3] + 2) >> 2);
			byte p6 = (byte)((a[2] + 2 * a[3] + a[4] + 2) >> 2), p7 = (byte)((a[3] + 2 * a[4] + a[5] + 2) >> 2);
			byte p8 = (byte)((a[4] + 2 * a[5] + a[6] + 2) >> 2), p9 = (byte)((a[5] + 2 * a[6] + a[7] + 2) >> 2);
			p[0] = p0; p[1] = p1; p[2] = p2; p[3] = p3; p += s;
			p[0] = p4; p[1] = p5; p[2] = p6; p[3] = p7; p += s;
			p[0] = p1; p[1] = p2; p[2] = p3; p[3] = p8; p += s;
			p[0] = p5; p[1] = p6; p[2] = p7; p[3] = p9;
		}

		private static void PredictHd4x4(byte* p, int s)
		{
			byte* l = p - 1;
			byte* a = p - s;
			byte p0 = (byte)((l[0] + a[-1] + 1) >> 1), p1 = (byte)((l[0] + 2 * a[-1] + a[0] + 2) >> 2);
			byte p2 = (byte)((a[-1] + 2 * a[0] + a[1] + 2) >> 2), p3 = (byte)((a[0] + 2 * a[1] + a[2] + 2) >> 2);
			byte p4 = (byte)((l[s] + l[0] + 1) >> 1), p5 = (byte)((l[s] + 2 * l[0] + a[-1] + 2) >> 2);
			byte p6 = (byte)((l[s * 2] + l[s] + 1) >> 1), p7 = (byte)((l[s * 2] + 2 * l[s] + l[0] + 2) >> 2);
			byte p8 = (byte)((l[s * 3] + l[s * 2] + 1) >> 1), p9 = (byte)((l[s * 3] + 2 * l[s * 2] + l[s] + 2) >> 2);
			p[0] = p0; p[1] = p1; p[2] = p2; p[3] = p3; p += s;
			p[0] = p4; p[1] = p5; p[2] = p0; p[3] = p1; p += s;
			p[0] = p6; p[1] = p7; p[2] = p4; p[3] = p5; p += s;
			p[0] = p8; p[1] = p9; p[2] = p6; p[3] = p7;
		}

		private static void PredictHu4x4(byte* p, int s)
		{
			byte* l = p - 1;
			byte p0 = (byte)((l[0] + l[s] + 1) >> 1), p1 = (byte)((l[0] + 2 * l[s] + l[s * 2] + 2) >> 2);
			byte p2 = (byte)((l[s] + l[s * 2] + 1) >> 1), p3 = (byte)((l[s] + 2 * l[s * 2] + l[s * 3] + 2) >> 2);
			byte p4 = (byte)((l[s * 2] + l[s * 3] + 1) >> 1), p5 = (byte)((l[s * 2] + 2 * l[s * 3] + l[s * 3] + 2) >> 2);
			byte p6 = l[s * 3];
			p[0] = p0; p[1] = p1; p[2] = p2; p[3] = p3; p += s;
			p[0] = p2; p[1] = p3; p[2] = p4; p[3] = p5; p += s;
			p[0] = p4; p[1] = p5; p[2] = p6; p[3] = p6; p += s;
			p[0] = p6; p[1] = p6; p[2] = p6; p[3] = p6;
		}

		/// <summary>The four pixels above-right of subblock 3 copied above-right of subblocks 7, 11 and 15.</summary>
		private static void CopyDown(byte* recon, int stride)
		{
			uint* copy = (uint*)(recon + 16 - stride);
			uint tmp = *copy;
			*(uint*)((byte*)copy + stride * 4) = tmp;
			*(uint*)((byte*)copy + stride * 8) = tmp;
			*(uint*)((byte*)copy + stride * 12) = tmp;
		}

		private static void BPredict(byte* predict, int stride, MbInfo* mbi, short* coeffs)
		{
			CopyDown(predict, stride);
			for (int i = 0; i < 16; i++)
			{
				byte* bp = predict + (i & 3) * 4;
				switch (mbi->Modes[i])
				{
					case BDcPred: PredictDcNxN(bp, stride, 4); break;
					case BTmPred: PredictTmNxN(bp, stride, 4); break;
					case BVePred: PredictVe4x4(bp, stride); break;
					case BHePred: PredictHe4x4(bp, stride); break;
					case BLdPred: PredictLd4x4(bp, stride); break;
					case BRdPred: PredictRd4x4(bp, stride); break;
					case BVrPred: PredictVr4x4(bp, stride); break;
					case BVlPred: PredictVl4x4(bp, stride); break;
					case BHdPred: PredictHd4x4(bp, stride); break;
					case BHuPred: PredictHu4x4(bp, stride); break;
				}
				IdctAdd(bp, bp, stride, coeffs);
				coeffs += 16;
				if ((i & 3) == 3) predict += stride * 4;
			}
		}

		private static void FixupDcCoeffs(short* coeffs)
		{
			short* y2 = stackalloc short[16];
			Walsh(coeffs + 24 * 16, y2);
			for (int i = 0; i < 16; i++) coeffs[i * 16] = y2[i];
		}

		private static void PredictIntraLuma(byte* predict, int stride, MbInfo* mbi, short* coeffs)
		{
			if (mbi->YMode == BPred)
			{
				BPredict(predict, stride, mbi, coeffs);
				return;
			}
			switch (mbi->YMode)
			{
				case DcPred: PredictDcNxN(predict, stride, 16); break;
				case VPred: PredictVNxN(predict, stride, 16); break;
				case HPred: PredictHNxN(predict, stride, 16); break;
				case TmPred: PredictTmNxN(predict, stride, 16); break;
			}
			FixupDcCoeffs(coeffs);
			for (int i = 0; i < 16; i++)
			{
				IdctAdd(predict, predict, stride, coeffs);
				coeffs += 16;
				predict += 4;
				if ((i & 3) == 3) predict += stride * 4 - 16;
			}
		}

		private static void PredictIntraChroma(byte* u, byte* v, int stride, MbInfo* mbi, short* coeffs)
		{
			switch (mbi->UvMode)
			{
				case DcPred: PredictDcNxN(u, stride, 8); PredictDcNxN(v, stride, 8); break;
				case VPred: PredictVNxN(u, stride, 8); PredictVNxN(v, stride, 8); break;
				case HPred: PredictHNxN(u, stride, 8); PredictHNxN(v, stride, 8); break;
				case TmPred: PredictTmNxN(u, stride, 8); PredictTmNxN(v, stride, 8); break;
			}
			coeffs += 16 * 16;
			for (int i = 16; i < 20; i++)
			{
				IdctAdd(u, u, stride, coeffs);
				coeffs += 16;
				u += 4;
				if ((i & 1) != 0) u += stride * 4 - 8;
			}
			for (int i = 20; i < 24; i++)
			{
				IdctAdd(v, v, stride, coeffs);
				coeffs += 16;
				v += 4;
				if ((i & 1) != 0) v += stride * 4 - 8;
			}
		}

		private static void SixtapHoriz(byte* output, int outStride, byte* reference, int refStride, int cols, int rows, short[] f)
		{
			for (int r = 0; r < rows; r++)
			{
				for (int c = 0; c < cols; c++)
				{
					int temp = reference[-2] * f[0] + reference[-1] * f[1] + reference[0] * f[2] + reference[1] * f[3] + reference[2] * f[4] + reference[3] * f[5] + 64;
					output[c] = Clamp255(temp >> 7);
					reference++;
				}
				reference += refStride - cols;
				output += outStride;
			}
		}

		private static void SixtapVert(byte* output, int outStride, byte* reference, int refStride, int cols, int rows, short[] f)
		{
			for (int r = 0; r < rows; r++)
			{
				for (int c = 0; c < cols; c++)
				{
					int temp = reference[-2 * refStride] * f[0] + reference[-1 * refStride] * f[1] + reference[0] * f[2] + reference[refStride] * f[3] + reference[2 * refStride] * f[4] + reference[3 * refStride] * f[5] + 64;
					output[c] = Clamp255(temp >> 7);
					reference++;
				}
				reference += refStride - cols;
				output += outStride;
			}
		}

		private static void Sixtap2D(byte* output, int outStride, byte* reference, int refStride, int cols, int rows, int mx, int my, short[][] filters)
		{
			byte* temp = stackalloc byte[16 * (16 + 5)];
			SixtapHoriz(temp, 16, reference - 2 * refStride, refStride, cols, rows + 5, filters[mx]);
			SixtapVert(output, outStride, temp + 2 * 16, 16, cols, rows, filters[my]);
		}

		private static byte* FilterBlock(byte* output, byte* reference, int stride, Mv mv, short[][] filters)
		{
			if (mv.Raw == 0) return reference;
			int mx = mv.X & 7, my = mv.Y & 7;
			reference += (mv.Y >> 3) * stride + (mv.X >> 3);
			if ((mx | my) != 0)
			{
				Sixtap2D(output, stride, reference, stride, 4, 4, mx, my, filters);
				reference = output;
			}
			return reference;
		}

		private static void Recon1Block(byte* output, byte* reference, int stride, Mv mv, short[][] filters, short* coeffs, int b)
		{
			byte* predict = FilterBlock(output, reference, stride, mv, filters);
			IdctAdd(output, predict, stride, coeffs + 16 * b);
		}

		private static Mv ChromaSplitMv(MbInfo* mbi, int b, bool fullPixel)
		{
			Mv mv;
			int temp = mbi->MvX[b] + mbi->MvX[b + 1] + mbi->MvX[b + 4] + mbi->MvX[b + 5];
			temp += temp < 0 ? -4 : 4;
			mv.X = (short)(temp / 8);
			temp = mbi->MvY[b] + mbi->MvY[b + 1] + mbi->MvY[b + 4] + mbi->MvY[b + 5];
			temp += temp < 0 ? -4 : 4;
			mv.Y = (short)(temp / 8);
			if (fullPixel)
			{
				mv.X = (short)(mv.X & ~7);
				mv.Y = (short)(mv.Y & ~7);
			}
			return mv;
		}

		private static void BuildMcBorder(byte* dst, byte* src, int stride, int x, int y, int bw, int bh, int w, int h)
		{
			byte* refRow = src - x - y * stride;
			if (y >= h) refRow += (h - 1) * stride;
			else if (y > 0) refRow += y * stride;
			do
			{
				int left = x < 0 ? -x : 0, right = 0;
				if (left > bw) left = bw;
				if (x + bw > w) right = x + bw - w;
				if (right > bw) right = bw;
				int copy = bw - left - right;
				if (left != 0) NativeMemory.Fill(dst, (nuint)left, refRow[0]);
				if (copy != 0) Buffer.MemoryCopy(refRow + x + left, dst + left, copy, copy);
				if (right != 0) NativeMemory.Fill(dst + left + copy, (nuint)right, refRow[w - 1]);
				dst += stride;
				y++;
				if (y < h && y > 0) refRow += stride;
			}
			while (--bh != 0);
		}

		private static void Recon1EdgeBlock(byte* output, byte* emul, byte* reference, int stride, Mv mv, short[][] filters, short* coeffs, int x, int y, int w, int h, int b)
		{
			const int bw = 4, bh = 4;
			x += mv.X >> 3;
			y += mv.Y >> 3;
			if (x < 2 || x + bw - 1 + 3 >= w || y < 2 || y + bh - 1 + 3 >= h)
			{
				reference += (mv.X >> 3) + (mv.Y >> 3) * stride;
				BuildMcBorder(emul, reference - 2 - 2 * stride, stride, x - 2, y - 2, bw + 5, bh + 5, w, h);
				reference = emul + 2 * stride + 2;
				reference -= (mv.X >> 3) + (mv.Y >> 3) * stride;
			}
			byte* predict = FilterBlock(output, reference, stride, mv, filters);
			IdctAdd(output, predict, stride, coeffs + 16 * b);
		}

		private Mv ChromaMv(MbInfo* mbi, bool fullPixel)
		{
			Mv uvmv = mbi->Mv;
			uvmv.X = (short)((uvmv.X + 1 + (uvmv.X >> 31) * 2) / 2);
			uvmv.Y = (short)((uvmv.Y + 1 + (uvmv.Y >> 31) * 2) / 2);
			if (fullPixel)
			{
				uvmv.X = (short)(uvmv.X & ~7);
				uvmv.Y = (short)(uvmv.Y & ~7);
			}
			return uvmv;
		}

		private void PredictInter(byte* y, byte* u, byte* v, int stride, int uvStride, short* coeffs, MbInfo* mbi, int mbCol, int mbRow)
		{
			bool fullPixel = _version == 3;
			Mv* chromaMv = stackalloc Mv[4];
			if (mbi->YMode != SplitMv)
			{
				Mv uvmv = ChromaMv(mbi, fullPixel);
				chromaMv[0] = chromaMv[1] = chromaMv[2] = chromaMv[3] = uvmv;
			}
			else
			{
				chromaMv[0] = ChromaSplitMv(mbi, 0, fullPixel);
				chromaMv[1] = ChromaSplitMv(mbi, 2, fullPixel);
				chromaMv[2] = ChromaSplitMv(mbi, 8, fullPixel);
				chromaMv[3] = ChromaSplitMv(mbi, 10, fullPixel);
			}
			long offset = _refOffsets[mbi->RefFrame];
			short[][] filters = _subpixelFilters;
			if (mbi->NeedMcBorder == 0)
			{
				for (int b = 0; b < 16; b++)
				{
					Mv ymv = mbi->YMode != SplitMv ? mbi->Mv : mbi->SplitMv(b);
					Recon1Block(y, y + offset, stride, ymv, filters, coeffs, b);
					y += 4;
					if ((b & 3) == 3) y += 4 * stride - 16;
				}
				for (int b = 0; b < 4; b++)
				{
					Recon1Block(u, u + offset, uvStride, chromaMv[b], filters, coeffs, b + 16);
					Recon1Block(v, v + offset, uvStride, chromaMv[b], filters, coeffs, b + 20);
					u += 4;
					v += 4;
					if ((b & 1) != 0)
					{
						u += 4 * uvStride - 8;
						v += 4 * uvStride - 8;
					}
				}
				return;
			}
			// predict_inter_emulated_edge
			int x = mbCol * 16, yy = mbRow * 16, w = _mbCols * 16, h = _mbRows * 16;
			byte* output = y;
			byte* reference = output + offset;
			for (int b = 0; b < 16; b++)
			{
				Mv ymv = mbi->YMode != SplitMv ? mbi->Mv : mbi->SplitMv(b);
				Recon1EdgeBlock(output, _emulBlock, reference, stride, ymv, filters, coeffs, x, yy, w, h, b);
				x += 4;
				output += 4;
				reference += 4;
				if ((b & 3) == 3)
				{
					x -= 16;
					yy += 4;
					output += 4 * stride - 16;
					reference += 4 * stride - 16;
				}
			}
			x = mbCol * 8;
			yy = mbRow * 8;
			w >>= 1;
			h >>= 1;
			for (int b = 0; b < 4; b++)
			{
				Recon1EdgeBlock(u, _emulBlock, u + offset, uvStride, chromaMv[b], filters, coeffs, x, yy, w, h, b + 16);
				Recon1EdgeBlock(v, _emulBlock, v + offset, uvStride, chromaMv[b], filters, coeffs, x, yy, w, h, b + 20);
				u += 4;
				v += 4;
				x += 4;
				if ((b & 1) != 0)
				{
					x -= 8;
					yy += 4;
					u += 4 * uvStride - 8;
					v += 4 * uvStride - 8;
				}
			}
		}

		private static void FixupLeft(byte* predict, int width, int stride, int row, int mode)
		{
			byte* left = predict - 1;
			if (mode == DcPred && row != 0)
			{
				byte* above = predict - stride;
				for (int i = 0; i < width; i++)
				{
					*left = above[i];
					left += stride;
				}
			}
			else
			{
				left -= stride;
				for (int i = -1; i < width; i++)
				{
					*left = 129;
					left += stride;
				}
			}
		}

		private static void FixupAbove(byte* predict, int width, int stride, int col, int mode)
		{
			byte* above = predict - stride;
			if (mode == DcPred && col != 0)
			{
				byte* left = predict - 1;
				for (int i = 0; i < width; i++)
				{
					above[i] = *left;
					left += stride;
				}
			}
			else NativeMemory.Fill(above - 1, (nuint)(width + 1), 127);
			NativeMemory.Fill(above + width, 4, 127);
		}

		private Image NewImage()
		{
			int w = _mbCols * 16 + BorderPixels * 2, h = _mbRows * 16 + BorderPixels * 2;
			Image img = new Image { Stride = w, UvStride = w / 2 };
			img.Data = (byte*)NativeMemory.AllocZeroed((nuint)(w * h + 2 * (w / 2) * (h / 2)));
			img.Y = img.Data + BorderPixels * w + BorderPixels;
			img.U = img.Data + w * h + (BorderPixels / 2) * (w / 2) + BorderPixels / 2;
			img.V = img.Data + w * h + (w / 2) * (h / 2) + (BorderPixels / 2) * (w / 2) + BorderPixels / 2;
			return img;
		}

		private void PredictInit()
		{
			if (_sizeUpdated || _storage[0] == null)
			{
				for (int i = 0; i < NumRefFrames; i++)
				{
					if (_storage[i] != null) NativeMemory.Free(_storage[i].Data);
					_storage[i] = NewImage();
					_refFrames[i] = null;
				}
				if (_emulBlock != null) NativeMemory.Free(_emulBlock);
				_emulBlock = (byte*)NativeMemory.AllocZeroed((nuint)(_storage[0].Stride * 10));
				_subpixelFilters = _version != 0 ? BilinearFilters : SixtapFilters;
			}
			if (_refFrames[CurrentFrame] != null) _refFrames[CurrentFrame].RefCount--;
			Image free = null;
			foreach (Image img in _storage)
				if (img.RefCount == 0) { free = img; break; }
			free ??= _storage[0];
			free.RefCount = 1;
			_refFrames[CurrentFrame] = free;
			for (int i = 0; i < NumRefFrames; i++)
				_refOffsets[i] = _refFrames[i] != null ? _refFrames[i].Data - free.Data : 0;
		}

		private void PredictProcessRow(int row)
		{
			Image img = _refFrames[CurrentFrame];
			int stride = img.Stride, uvStride = img.UvStride;
			byte* y = img.Y + stride * row * 16, u = img.U + uvStride * row * 8, v = img.V + uvStride * row * 8;
			MbInfo* mbi = Row(row);
			short* coeffs = _tokens[row & (_partitions - 1)].Coeffs;
			FixupLeft(y, 16, stride, row, mbi->YMode);
			FixupLeft(u, 8, uvStride, row, mbi->UvMode);
			FixupLeft(v, 8, uvStride, row, mbi->UvMode);
			if (row == 0) *(y - stride - 1) = 127;
			for (int col = 0; col < _mbCols; col++)
			{
				if (row == 0)
				{
					FixupAbove(y, 16, stride, col, mbi->YMode);
					FixupAbove(u, 8, uvStride, col, mbi->UvMode);
					FixupAbove(v, 8, uvStride, col, mbi->UvMode);
				}
				if (mbi->YMode <= BPred)
				{
					PredictIntraLuma(y, stride, mbi, coeffs);
					PredictIntraChroma(u, v, uvStride, mbi, coeffs);
				}
				else
				{
					if (mbi->YMode != SplitMv) FixupDcCoeffs(coeffs);
					PredictInter(y, u, v, stride, uvStride, coeffs, mbi, col, row);
				}
				mbi++;
				y += 16;
				u += 8;
				v += 8;
				coeffs += 25 * 16;
			}
			// The last row of the macroblock row extended four pixels for the intra prediction of the row below's last block
			byte last = y[-1 + 15 * stride];
			*(uint*)(y + 15 * stride) = 0x01010101u * last;
		}

		// ---- the loop filter (dixie_loopfilter.c) ----

		private static int Abs(int x) => x >= 0 ? x : -x;
		private static int SaturateInt8(int x) => x < -128 ? -128 : x > 127 ? 127 : x;
		private static byte SaturateUint8(int x) => (byte)(x < 0 ? 0 : x > 255 ? 255 : x);

		private static bool HighEdgeVariance(byte* p, int s, int hev) => Abs(p[-2 * s] - p[-s]) > hev || Abs(p[s] - p[0]) > hev;

		private static bool SimpleThreshold(byte* p, int s, int limit) => Abs(p[-s] - p[0]) * 2 + (Abs(p[-2 * s] - p[s]) >> 1) <= limit;

		private static bool NormalThreshold(byte* p, int s, int e, int i)
		{
			int p3 = p[-4 * s], p2 = p[-3 * s], p1 = p[-2 * s], p0 = p[-s], q0 = p[0], q1 = p[s], q2 = p[2 * s], q3 = p[3 * s];
			return SimpleThreshold(p, s, 2 * e + i) && Abs(p3 - p2) <= i && Abs(p2 - p1) <= i && Abs(p1 - p0) <= i
				&& Abs(q3 - q2) <= i && Abs(q2 - q1) <= i && Abs(q1 - q0) <= i;
		}

		private static void FilterCommon(byte* p, int s, bool useOuterTaps)
		{
			int p1 = p[-2 * s], p0 = p[-s], q0 = p[0], q1 = p[s];
			int a = 3 * (q0 - p0);
			if (useOuterTaps) a += SaturateInt8(p1 - q1);
			a = SaturateInt8(a);
			int f1 = (a + 4 > 127 ? 127 : a + 4) >> 3;
			int f2 = (a + 3 > 127 ? 127 : a + 3) >> 3;
			p[-s] = SaturateUint8(p0 + f2);
			p[0] = SaturateUint8(q0 - f1);
			if (!useOuterTaps)
			{
				a = (f1 + 1) >> 1;
				p[-2 * s] = SaturateUint8(p1 + a);
				p[s] = SaturateUint8(q1 - a);
			}
		}

		private static void FilterMbEdge(byte* p, int s)
		{
			int p2 = p[-3 * s], p1 = p[-2 * s], p0 = p[-s], q0 = p[0], q1 = p[s], q2 = p[2 * s];
			int w = SaturateInt8(SaturateInt8(p1 - q1) + 3 * (q0 - p0));
			int a = (27 * w + 63) >> 7;
			p[-s] = SaturateUint8(p0 + a);
			p[0] = SaturateUint8(q0 - a);
			a = (18 * w + 63) >> 7;
			p[-2 * s] = SaturateUint8(p1 + a);
			p[s] = SaturateUint8(q1 - a);
			a = (9 * w + 63) >> 7;
			p[-3 * s] = SaturateUint8(p2 + a);
			p[2 * s] = SaturateUint8(q2 - a);
		}

		private static void FilterMbVEdge(byte* src, int stride, int e, int i, int hev, int size)
		{
			for (int n = 0; n < 8 * size; n++)
			{
				if (NormalThreshold(src, 1, e, i))
				{
					if (HighEdgeVariance(src, 1, hev)) FilterCommon(src, 1, true);
					else FilterMbEdge(src, 1);
				}
				src += stride;
			}
		}

		private static void FilterSubblockVEdge(byte* src, int stride, int e, int i, int hev, int size)
		{
			for (int n = 0; n < 8 * size; n++)
			{
				if (NormalThreshold(src, 1, e, i)) FilterCommon(src, 1, HighEdgeVariance(src, 1, hev));
				src += stride;
			}
		}

		private static void FilterMbHEdge(byte* src, int stride, int e, int i, int hev, int size)
		{
			for (int n = 0; n < 8 * size; n++)
			{
				if (NormalThreshold(src, stride, e, i))
				{
					if (HighEdgeVariance(src, stride, hev)) FilterCommon(src, stride, true);
					else FilterMbEdge(src, stride);
				}
				src += 1;
			}
		}

		private static void FilterSubblockHEdge(byte* src, int stride, int e, int i, int hev, int size)
		{
			for (int n = 0; n < 8 * size; n++)
			{
				if (NormalThreshold(src, stride, e, i)) FilterCommon(src, stride, HighEdgeVariance(src, stride, hev));
				src += 1;
			}
		}

		private static void FilterVEdgeSimple(byte* src, int stride, int limit)
		{
			for (int n = 0; n < 16; n++)
			{
				if (SimpleThreshold(src, 1, limit)) FilterCommon(src, 1, true);
				src += stride;
			}
		}

		private static void FilterHEdgeSimple(byte* src, int stride, int limit)
		{
			for (int n = 0; n < 16; n++)
			{
				if (SimpleThreshold(src, stride, limit)) FilterCommon(src, stride, true);
				src += 1;
			}
		}

		private void FilterParameters(MbInfo* mbi, out int edgeLimit, out int interiorLimit, out int hevThreshold)
		{
			int level = _lfLevel;
			if (_segEnabled)
			{
				if (!_segAbs) level += _segLfLevel[mbi->SegmentId];
				else level = _segLfLevel[mbi->SegmentId];
			}
			level = level > 63 ? 63 : level < 0 ? 0 : level;
			if (_lfDeltaEnabled)
			{
				level += _lfRefDelta[mbi->RefFrame];
				if (mbi->RefFrame == CurrentFrame)
				{
					if (mbi->YMode == BPred) level += _lfModeDelta[0];
				}
				else if (mbi->YMode == ZeroMv) level += _lfModeDelta[1];
				else if (mbi->YMode == SplitMv) level += _lfModeDelta[3];
				else level += _lfModeDelta[2];
			}
			level = level > 63 ? 63 : level < 0 ? 0 : level;
			int interior = level;
			if (_lfSharpness != 0)
			{
				interior >>= _lfSharpness > 4 ? 2 : 1;
				if (interior > 9 - _lfSharpness) interior = 9 - _lfSharpness;
			}
			if (interior < 1) interior = 1;
			int hev = level >= 15 ? 1 : 0;
			if (level >= 40) hev++;
			if (level >= 20 && !_keyframe) hev++;
			edgeLimit = level;
			interiorLimit = interior;
			hevThreshold = hev;
		}

		private void LoopFilterProcessRow(int row)
		{
			Image img = _refFrames[CurrentFrame];
			int stride = img.Stride, uvStride = img.UvStride;
			byte* y = img.Y + stride * row * 16, u = img.U + uvStride * row * 8, v = img.V + uvStride * row * 8;
			MbInfo* mbi = Row(row);
			for (int col = 0; col < _mbCols; col++)
			{
				FilterParameters(mbi, out int e, out int i, out int hev);
				if (e != 0)
				{
					bool sub = mbi->EobMask != 0 || mbi->YMode == SplitMv || mbi->YMode == BPred;
					if (_lfSimple)
					{
						int mbLimit = (e + 2) * 2 + i, bLimit = e * 2 + i;
						if (col != 0) FilterVEdgeSimple(y, stride, mbLimit);
						if (sub)
						{
							FilterVEdgeSimple(y + 4, stride, bLimit);
							FilterVEdgeSimple(y + 8, stride, bLimit);
							FilterVEdgeSimple(y + 12, stride, bLimit);
						}
						if (row != 0) FilterHEdgeSimple(y, stride, mbLimit);
						if (sub)
						{
							FilterHEdgeSimple(y + 4 * stride, stride, bLimit);
							FilterHEdgeSimple(y + 8 * stride, stride, bLimit);
							FilterHEdgeSimple(y + 12 * stride, stride, bLimit);
						}
					}
					else
					{
						if (col != 0)
						{
							FilterMbVEdge(y, stride, e + 2, i, hev, 2);
							FilterMbVEdge(u, uvStride, e + 2, i, hev, 1);
							FilterMbVEdge(v, uvStride, e + 2, i, hev, 1);
						}
						if (sub)
						{
							FilterSubblockVEdge(y + 4, stride, e, i, hev, 2);
							FilterSubblockVEdge(y + 8, stride, e, i, hev, 2);
							FilterSubblockVEdge(y + 12, stride, e, i, hev, 2);
							FilterSubblockVEdge(u + 4, uvStride, e, i, hev, 1);
							FilterSubblockVEdge(v + 4, uvStride, e, i, hev, 1);
						}
						if (row != 0)
						{
							FilterMbHEdge(y, stride, e + 2, i, hev, 2);
							FilterMbHEdge(u, uvStride, e + 2, i, hev, 1);
							FilterMbHEdge(v, uvStride, e + 2, i, hev, 1);
						}
						if (sub)
						{
							FilterSubblockHEdge(y + 4 * stride, stride, e, i, hev, 2);
							FilterSubblockHEdge(y + 8 * stride, stride, e, i, hev, 2);
							FilterSubblockHEdge(y + 12 * stride, stride, e, i, hev, 2);
							FilterSubblockHEdge(u + 4 * uvStride, uvStride, e, i, hev, 1);
							FilterSubblockHEdge(v + 4 * uvStride, uvStride, e, i, hev, 1);
						}
					}
				}
				y += 16;
				u += 8;
				v += 8;
				mbi++;
			}
		}

		// ---- the picture ----

		/// <summary>The decoded frame as RGBA (BT.601, limited range; each chroma sample for its 2 x 2 pixels), Width x Height,
		/// into <paramref name="rgba"/> (Width * Height * 4 bytes).</summary>
		public void ToRgba(byte[] rgba)
		{
			Image img = _refFrames[CurrentFrame];
			if (img == null) return;
			int w = _width, h = _height;
			fixed (byte* outp = rgba)
			{
				for (int yy = 0; yy < h; yy++)
				{
					byte* yRow = img.Y + yy * img.Stride, uRow = img.U + (yy >> 1) * img.UvStride, vRow = img.V + (yy >> 1) * img.UvStride;
					uint* o = (uint*)(outp + yy * w * 4);
					for (int xx = 0; xx < w; xx++)
					{
						int c = 298 * (yRow[xx] - 16), d = uRow[xx >> 1] - 128, e = vRow[xx >> 1] - 128;
						int r = (c + 409 * e + 128) >> 8, g = (c - 100 * d - 208 * e + 128) >> 8, b = (c + 516 * d + 128) >> 8;
						o[xx] = Clamp255(r) | ((uint)Clamp255(g) << 8) | ((uint)Clamp255(b) << 16) | 0xFF000000u;
					}
				}
			}
		}

		public void Dispose()
		{
			for (int i = 0; i < NumRefFrames; i++)
			{
				if (_storage[i] != null) NativeMemory.Free(_storage[i].Data);
				_storage[i] = null;
				_refFrames[i] = null;
			}
			for (int i = 0; i < 8; i++)
			{
				if (_tokens[i].Coeffs != null) NativeMemory.AlignedFree(_tokens[i].Coeffs);
				_tokens[i].Coeffs = null;
			}
			if (_mbInfoStorage != null) NativeMemory.Free(_mbInfoStorage);
			_mbInfoStorage = null;
			if (_aboveTokenContext != null) NativeMemory.Free(_aboveTokenContext);
			_aboveTokenContext = null;
			if (_emulBlock != null) NativeMemory.Free(_emulBlock);
			_emulBlock = null;
		}
	}
}
