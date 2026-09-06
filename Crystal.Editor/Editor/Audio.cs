// Audio, and what plays it.
//
// The chain, end to end:
//
//   playBGM 1            -> "BGM01"      -> BGM01_0.xnb (intro), BGM01_1.xnb (loop)
//   playSE 270, 1        -> "SE270_01"   -> SE270_01_0.xnb
//
// NNS_SndMain builds the name - "BGM%.2d" for music, "SE%.3d_%.2d" for effects - and
// SoundManager.playSound appends _0 and _1 for the two parts a track can have. Where
// both exist the loop point comes from sound/<name>.dat in the archives, a single 32
// bit millisecond count, which is why there are exactly as many .dat files as there
// are looping tracks.
//
// So a script naming a number and a file on disk are two ends of the same thing, and
// this puts them back together.
//
// FF4 3D keeps its sound as files/SOUND/BGM|SE|VOICE/*.akb: an 'AKB ' header (channels,
// sample rate, sample count, loop start and end in samples) in front of an Ogg Vorbis
// stream at byte 204. Same names, one part each, and the browser plays Ogg as it is.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Crystal.Editor
{
	internal sealed class AudioAsset
	{
		public string Name { get; set; }             // BGM01, SE270_01, en_Ev01_000a
		public string Kind { get; set; }             // bgm, se or voice
		public List<int> Parts { get; set; } = new List<int>();
		public int Milliseconds { get; set; }
		public int SampleRate { get; set; }
		public int Channels { get; set; }

		/// <summary>What a script would write to play this.</summary>
		public string Call { get; set; }

		/// <summary>Loop point in milliseconds, from sound/&lt;name&gt;.dat, or -1.</summary>
		public int LoopAt { get; set; } = -1;
	}

	internal sealed class AudioUse
	{
		public string Script { get; set; }
		public int Count { get; set; }
	}

	internal static class Audio
	{
		private static readonly Regex Part = new Regex(@"^(?<name>.+)_(?<part>[01])$",
			RegexOptions.Compiled);

		/// <summary>Every sound in the content directory, grouped into its parts.</summary>
		public static List<AudioAsset> List(string contentDirectory, Workspace workspace)
		{
			if (workspace.Game == "ff4")
			{
				return ListAkb(workspace);
			}

			Dictionary<string, AudioAsset> assets =
				new Dictionary<string, AudioAsset>(StringComparer.OrdinalIgnoreCase);

			foreach (string file in Directory.EnumerateFiles(contentDirectory, "*.xnb"))
			{
				string stem = Path.GetFileNameWithoutExtension(file);
				Match match = Part.Match(stem);
				if (!match.Success)
				{
					continue;
				}

				string name = match.Groups["name"].Value;
				int part = int.Parse(match.Groups["part"].Value, CultureInfo.InvariantCulture);
				if (!name.StartsWith("BGM", StringComparison.OrdinalIgnoreCase)
					&& !name.StartsWith("SE", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!assets.TryGetValue(name, out AudioAsset asset))
				{
					assets[name] = asset = new AudioAsset
					{
						Name = name,
						Kind = name.StartsWith("BGM", StringComparison.OrdinalIgnoreCase)
							? "bgm" : "se",
						Call = CallFor(name),
						LoopAt = LoopPoint(workspace, name)
					};
				}
				asset.Parts.Add(part);

				if (asset.Milliseconds == 0)
				{
					try
					{
						Xnb xnb = Xnb.Open(file);
						try
						{
							XnbSoundEffect sound = xnb.ReadSoundEffect();
							asset.Milliseconds = sound.DurationMs;
							asset.SampleRate = sound.SampleRate;
							asset.Channels = sound.Channels;
						}
						finally
						{
							xnb.Close();
						}
					}
					catch (Exception)
					{
						// A sound that will not parse still belongs in the list; it
						// just has nothing to say about itself.
					}
				}
			}

			foreach (AudioAsset asset in assets.Values)
			{
				asset.Parts.Sort();
			}
			return assets.Values
				.OrderBy(a => a.Kind, StringComparer.Ordinal)
				.ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>The script line that plays this sound, worked back from its name.</summary>
		private static string CallFor(string name)
		{
			if (name.StartsWith("BGM", StringComparison.OrdinalIgnoreCase)
				&& int.TryParse(name.Substring(3), NumberStyles.Integer,
					CultureInfo.InvariantCulture, out int bgm))
			{
				return string.Format(CultureInfo.InvariantCulture,
					"playBGM {0}, <volume>, <fadeinFrame>", bgm);
			}

			Match match = Regex.Match(name, @"^SE(\d+)_(\d+)$", RegexOptions.IgnoreCase);
			return match.Success
				? string.Format(CultureInfo.InvariantCulture,
					"playSE {0}, {1}, <volume>, <pan>",
					int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
					int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture))
				: null;
		}

		private static int LoopPoint(Workspace workspace, string name)
		{
			string entry = "sound/" + name + ".dat";
			try
			{
				byte[] data = workspace.Exists(entry) ? workspace.Read(entry) : null;
				return data != null && data.Length >= 4
					? data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24)
					: -1;
			}
			catch (Exception)
			{
				return -1;
			}
		}

		/// <summary>One part of a sound, decoded to a playable wav.</summary>
		/// <summary>
		/// The sound as the browser can play it: a wav decoded from the XNB, or FF4's
		/// Ogg stream lifted out of its AKB header. contentType says which.
		/// </summary>
		public static byte[] Playable(Workspace workspace, string name, int part,
			out string contentType)
		{
			if (workspace.Game == "ff4")
			{
				contentType = "audio/ogg";
				return Ogg(workspace, name);
			}
			contentType = "audio/wav";
			return Wav(workspace.ContentDirectory, name, part);
		}

		public static byte[] Wav(string contentDirectory, string name, int part)
		{
			string file = Path.Combine(contentDirectory, string.Format(
				CultureInfo.InvariantCulture, "{0}_{1}.xnb", name, part));
			if (!File.Exists(file))
			{
				throw new FileNotFoundException("no such sound: " + name + "_" + part);
			}

			Xnb xnb = Xnb.Open(file);
			try
			{
				XnbSoundEffect sound = xnb.ReadSoundEffect();
				using MemoryStream stream = new MemoryStream();
				global::Crystal.Wav.Write(stream, sound.Format, sound.Data);
				return stream.ToArray();
			}
			finally
			{
				xnb.Close();
			}
		}

		// ------------------------------------------------------------ what plays it

		// ---- FF4: AKB ---------------------------------------------------------------------

		private const int AkbOggOffset = 204;
		private static readonly byte[] OggMagic = { (byte)'O', (byte)'g', (byte)'g', (byte)'S' };

		private static readonly Dictionary<Workspace, List<AudioAsset>> _akb =
			new Dictionary<Workspace, List<AudioAsset>>();

		/// <summary>
		/// Every .akb under files/SOUND, read once per workspace: the header carries what
		/// the list shows, and the folder says what kind of sound it is.
		/// </summary>
		private static List<AudioAsset> ListAkb(Workspace workspace)
		{
			lock (_akb)
			{
				if (_akb.TryGetValue(workspace, out List<AudioAsset> known))
				{
					return known;
				}
			}

			List<AudioAsset> assets = new List<AudioAsset>();
			foreach (WorkspaceEntry entry in workspace.List(".akb"))
			{
				string folder = Path.GetFileName(Path.GetDirectoryName(entry.Name) ?? string.Empty);
				string name = Path.GetFileNameWithoutExtension(entry.Name);
				AudioAsset asset = new AudioAsset
				{
					Name = name,
					Kind = folder.Equals("BGM", StringComparison.OrdinalIgnoreCase) ? "bgm"
						: folder.Equals("VOICE", StringComparison.OrdinalIgnoreCase) ? "voice" : "se",
					Call = CallFor(name)
				};
				asset.Parts.Add(0);
				try
				{
					byte[] data = workspace.Read(entry.Name);
					if (data.Length >= 32 && data[0] == 'A' && data[1] == 'K' && data[2] == 'B')
					{
						// 0x0C codec, 0x0D channels, 0x0E rate, 0x10 samples, 0x14 loop start,
						// 0x18 loop end - measured against the files; nothing names them.
						asset.Channels = data[0x0D];
						asset.SampleRate = data[0x0E] | (data[0x0F] << 8);
						uint samples = ReadUInt32(data, 0x10);
						uint loopStart = ReadUInt32(data, 0x14);
						if (asset.SampleRate > 0)
						{
							asset.Milliseconds = (int)(samples * 1000L / asset.SampleRate);
							asset.LoopAt = loopStart > 0 ? (int)(loopStart * 1000L / asset.SampleRate) : -1;
						}
					}
				}
				catch (Exception)
				{
					// Listed regardless; the player will say if it cannot play it.
				}
				assets.Add(asset);
			}

			assets = assets
				.OrderBy(a => a.Kind, StringComparer.Ordinal)
				.ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
			lock (_akb)
			{
				_akb[workspace] = assets;
			}
			return assets;
		}

		/// <summary>The Ogg Vorbis stream inside an AKB, by the sound's name.</summary>
		private static byte[] Ogg(Workspace workspace, string name)
		{
			foreach (string folder in new[] { "BGM", "SE", "VOICE" })
			{
				string entry = "files/SOUND/" + folder + "/" + name + ".akb";
				if (!workspace.Exists(entry))
				{
					continue;
				}
				byte[] data = workspace.Read(entry);
				int at = IndexOf(data, OggMagic, AkbOggOffset);
				if (at < 0)
				{
					at = IndexOf(data, OggMagic, 0);
				}
				if (at < 0)
				{
					throw new FileNotFoundException("no Ogg stream in " + entry);
				}
				byte[] ogg = new byte[data.Length - at];
				Buffer.BlockCopy(data, at, ogg, 0, ogg.Length);
				return ogg;
			}
			throw new FileNotFoundException("no such sound: " + name);
		}

		private static int IndexOf(byte[] data, byte[] needle, int from)
		{
			for (int i = Math.Max(0, from); i + needle.Length <= data.Length; i++)
			{
				int k = 0;
				while (k < needle.Length && data[i + k] == needle[k])
				{
					k++;
				}
				if (k == needle.Length)
				{
					return i;
				}
			}
			return -1;
		}

		private static uint ReadUInt32(byte[] d, int at)
		{
			return (uint)(d[at] | (d[at + 1] << 8) | (d[at + 2] << 16) | (d[at + 3] << 24));
		}

		private static Dictionary<string, List<AudioUse>> _uses;
		private static Workspace _usesFor;

		/// <summary>
		/// Which scripts play which sound, by walking every script once and reading the
		/// operands of playBGM and playSE. Built on the first question and kept, because
		/// it means decoding 356 scripts and that is not a per-click cost.
		/// </summary>
		public static List<AudioUse> Uses(Workspace workspace, string name)
		{
			// Per workspace: File > Open can swap the game under this, and FF3's
			// answers about FF4's sounds would be confidently wrong.
			if (_uses == null || _usesFor != workspace)
			{
				_uses = BuildUses(workspace);
				_usesFor = workspace;
			}
			return _uses.TryGetValue(name, out List<AudioUse> found)
				? found : new List<AudioUse>();
		}

		private static Dictionary<string, List<AudioUse>> BuildUses(Workspace workspace)
		{
			int playBgm = workspace.Ops.Names.Opcode("playBGM");
			int playSe = workspace.Ops.Names.Opcode("playSE");
			Dictionary<string, Dictionary<string, int>> counts =
				new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

			foreach (WorkspaceEntry entry in workspace.List(".script"))
			{
				ScriptFile script;
				try
				{
					script = ScriptFile.Read(workspace.Read(entry.Name), workspace.Ops);
				}
				catch (Exception)
				{
					continue;                        // s01_01 is version 1.0
				}

				foreach (ScriptInstruction instruction in
					ScriptDisassembler.Disassemble(script).Code)
				{
					string sound = null;
					if (instruction.Opcode == playBgm && instruction.Operands.Count >= 1)
					{
						sound = string.Format(CultureInfo.InvariantCulture,
							"BGM{0:D2}", (uint)instruction.Operands[0]);
					}
					else if (instruction.Opcode == playSe && instruction.Operands.Count >= 2)
					{
						sound = string.Format(CultureInfo.InvariantCulture,
							"SE{0:D3}_{1:D2}",
							(uint)instruction.Operands[0], (uint)instruction.Operands[1]);
					}
					if (sound == null)
					{
						continue;
					}

					if (!counts.TryGetValue(sound, out Dictionary<string, int> perScript))
					{
						counts[sound] = perScript = new Dictionary<string, int>(StringComparer.Ordinal);
					}
					perScript.TryGetValue(entry.Name, out int seen);
					perScript[entry.Name] = seen + 1;
				}
			}

			return counts.ToDictionary(
				pair => pair.Key,
				pair => pair.Value
					.OrderByDescending(s => s.Value)
					.ThenBy(s => s.Key, StringComparer.Ordinal)
					.Select(s => new AudioUse { Script = s.Key, Count = s.Value })
					.ToList(),
				StringComparer.OrdinalIgnoreCase);
		}
	}
}
