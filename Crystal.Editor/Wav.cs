// Wraps the raw format + data blocks from a SoundEffect XNB back into a RIFF/WAVE file.
//
// The XNB stores a WAVEFORMATEX structure verbatim followed by the sample data, so
// no transcoding is needed - including for the ADPCM tracks, which keep their
// format-specific tail and get the "fact" chunk decoders expect.

using System;
using System.IO;
using System.Text;

namespace FF3.ContentTool
{
	internal static class Wav
	{
		private const int FormatPcm = 1;

		public static void Write(string path, byte[] format, byte[] data)
		{
			using FileStream file = File.Create(path);
			Write(file, format, data);
		}

		/// <summary>The same, to any stream - the editor serves these rather than
		/// writing them out.</summary>
		public static void Write(Stream output, byte[] format, byte[] data)
		{
			if (format == null || format.Length < 16)
			{
				throw new InvalidDataException("sound has no usable WAVEFORMATEX block");
			}

			int formatTag = BitConverter.ToInt16(format, 0);
			int channels = BitConverter.ToInt16(format, 2);
			int samplesPerSecond = BitConverter.ToInt32(format, 4);
			int bitsPerSample = BitConverter.ToInt16(format, 14);

			bool needsFact = formatTag != FormatPcm;
			int factSize = needsFact ? 12 : 0;

			// RIFF size covers everything after the size field itself.
			int riffSize = 4                          // "WAVE"
				+ 8 + Align(format.Length)            // fmt  chunk
				+ factSize                            // fact chunk
				+ 8 + Align(data.Length);             // data chunk

			using BinaryWriter writer = new BinaryWriter(output, Encoding.ASCII, leaveOpen: true);

			writer.Write(Encoding.ASCII.GetBytes("RIFF"));
			writer.Write(riffSize);
			writer.Write(Encoding.ASCII.GetBytes("WAVE"));

			writer.Write(Encoding.ASCII.GetBytes("fmt "));
			writer.Write(format.Length);
			writer.Write(format);
			if ((format.Length & 1) != 0)
			{
				writer.Write((byte)0);
			}

			if (needsFact)
			{
				// Number of decoded samples; decoders use it for duration.
				int blockAlign = BitConverter.ToInt16(format, 12);
				int samples = (blockAlign > 0 && channels > 0)
					? (data.Length / blockAlign) * SamplesPerBlock(format, channels, bitsPerSample)
					: 0;
				writer.Write(Encoding.ASCII.GetBytes("fact"));
				writer.Write(4);
				writer.Write(samples);
			}

			writer.Write(Encoding.ASCII.GetBytes("data"));
			writer.Write(data.Length);
			writer.Write(data);
			if ((data.Length & 1) != 0)
			{
				writer.Write((byte)0);
			}

			_ = samplesPerSecond;
		}

		private static int Align(int size) => size + (size & 1);

		/// <summary>MS-ADPCM stores its samples-per-block in the format tail.</summary>
		private static int SamplesPerBlock(byte[] format, int channels, int bitsPerSample)
		{
			if (format.Length >= 20)
			{
				int value = BitConverter.ToInt16(format, 18);
				if (value > 0)
				{
					return value;
				}
			}
			return channels > 0 && bitsPerSample > 0 ? 1 : 1;
		}
	}
}
