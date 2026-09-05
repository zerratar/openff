// PORT: visibility animations (NNS "BVA0" / category 'V' 'AV'). The phone port kept the
// header class for the result and dropped the resource, so node visibility never animated;
// FF3's stages have none, FF4's scene stages (the Red Wings deck) hide and show their
// props with them. Layout, read from FF4's e01_00.namp: the 4-byte animation header,
// u16 frames, u16 nodes, u32 size of the whole animation, then the bits frame-major -
// bit (frame * nodes + node) is that node's visibility on that frame.

internal static partial class GlobalScope
{
	public class NNSG3dResVisAnm
	{
		public NNSG3dResAnmHeader anmHeader;

		public ushort numFrame;

		public ushort numNode;

		public uint size;

		public uint[] visData;

		public bool IsVisible(int node, int frame)
		{
			if (visData == null || numNode == 0 || node < 0 || node >= numNode)
			{
				return true;
			}
			if (frame < 0) frame = 0;
			if (frame >= numFrame) frame = numFrame - 1;
			int bit = frame * numNode + node;
			int word = bit >> 5;
			if (word >= visData.Length)
			{
				return true;
			}
			return ((visData[word] >> (bit & 31)) & 1) != 0;
		}

		public static explicit operator NNSG3dResVisAnm(ArrayReader src)
		{
			NNSG3dResVisAnm anm = new NNSG3dResVisAnm();
			anm.anmHeader = (NNSG3dResAnmHeader)src;
			anm.numFrame = src.readUInt16();
			anm.numNode = src.readUInt16();
			anm.size = src.readUInt32();
			int words = (anm.numFrame * anm.numNode + 31) / 32;
			anm.visData = new uint[words];
			src.read(anm.visData, 0, words);
			return anm;
		}
	}
}
