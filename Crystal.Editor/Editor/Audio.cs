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

		/// <summary>The mod's own: sound/&lt;name&gt;_n.ogg (or .wav) in the project's files, nothing of the game's behind it.</summary>
		public bool Own { get; set; }
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

			AddOwn(workspace, assets);

			foreach (AudioAsset asset in assets.Values)
			{
				asset.Parts.Sort();
			}
			return assets.Values
				.OrderBy(a => a.Kind, StringComparer.Ordinal)
				.ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>The folder the mod's own sounds live in, under the project's files: sound/BGM30_1.ogg and the like.</summary>
		public const string OwnFolder = "sound";

		/// <summary>The BGM numbers the game ships no tune for: a mod's own go there. The game's run 0..58; the client takes any number whose parts the content chain holds, so a mod has 59..199.</summary>
		public const int FirstFreeBgm = 59, LastFreeBgm = 199;

		/// <summary>
		/// The mod's own sounds - sound/&lt;name&gt;_&lt;part&gt;.ogg or .wav in the project's files - added
		/// to (or laid over) the game's list. The client plays a name the game's table lacks
		/// from whatever parts the content chain holds, so a file here under a free BGM number
		/// is a tune of the mod's; under a game's name it replaces that part.
		/// </summary>
		private static void AddOwn(Workspace workspace, Dictionary<string, AudioAsset> assets)
		{
			string folder = workspace.OverrideDirectory == null ? null : Path.Combine(workspace.OverrideDirectory, OwnFolder);
			if (folder == null || !Directory.Exists(folder)) return;
			foreach (string file in Directory.EnumerateFiles(folder))
			{
				string extension = Path.GetExtension(file);
				if (!string.Equals(extension, ".ogg", StringComparison.OrdinalIgnoreCase) && !string.Equals(extension, ".wav", StringComparison.OrdinalIgnoreCase)) continue;
				Match match = Part.Match(Path.GetFileNameWithoutExtension(file));
				if (!match.Success) continue;
				string name = match.Groups["name"].Value;
				int part = int.Parse(match.Groups["part"].Value, CultureInfo.InvariantCulture);
				if (!assets.TryGetValue(name, out AudioAsset asset))
				{
					assets[name] = asset = new AudioAsset
					{
						Name = name,
						Kind = name.StartsWith("BGM", StringComparison.OrdinalIgnoreCase) ? "bgm" : "se",
						Call = CallFor(name),
						LoopAt = LoopPoint(workspace, name),
						Own = true
					};
				}
				if (!asset.Parts.Contains(part)) asset.Parts.Add(part);
				if (asset.Own && (asset.Milliseconds == 0 || part == 1))   // the loop is the tune's length; the intro only when there is nothing else
				{
					try
					{
						byte[] data = File.ReadAllBytes(file);
						(int ms, int rate, int channels) = Describe(data);
						asset.Milliseconds = ms; asset.SampleRate = rate; asset.Channels = channels;
					}
					catch (Exception) { }
				}
			}
		}

		/// <summary>Length, rate and channels of an Ogg Vorbis or a WAV, from its headers.</summary>
		private static (int ms, int rate, int channels) Describe(byte[] data)
		{
			if (data.Length > 12 && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F')
			{
				// fmt chunk: channels at 22, rate at 24, byte rate at 28; data chunk's length gives the time.
				int channels = data[22] | (data[23] << 8);
				int rate = (int)ReadUInt32(data, 24);
				int byteRate = (int)ReadUInt32(data, 28);
				int at = 12, dataLength = 0;
				while (at + 8 <= data.Length)
				{
					int size = (int)ReadUInt32(data, at + 4);
					if (data[at] == 'd' && data[at + 1] == 'a' && data[at + 2] == 't' && data[at + 3] == 'a') { dataLength = size; break; }
					at += 8 + size + (size & 1);
				}
				return (byteRate > 0 ? (int)((long)dataLength * 1000 / byteRate) : 0, rate, channels);
			}
			if (data.Length > 40 && data[0] == 'O' && data[1] == 'g' && data[2] == 'g' && data[3] == 'S')
			{
				// The identification header (the first packet): channels at +11, rate at +12 from "\x01vorbis";
				// the last page's granule position is the sample count.
				int id = IndexOf(data, new[] { (byte)1, (byte)'v', (byte)'o', (byte)'r', (byte)'b', (byte)'i', (byte)'s' }, 0);
				if (id < 0) return (0, 0, 0);
				int channels = data[id + 11];
				int rate = (int)ReadUInt32(data, id + 12);
				long granule = 0;
				for (int at = data.Length - 27; at >= 0; at--)
				{
					if (data[at] == 'O' && data[at + 1] == 'g' && data[at + 2] == 'g' && data[at + 3] == 'S')
					{
						granule = (long)ReadUInt32(data, at + 6) | ((long)ReadUInt32(data, at + 10) << 32);
						break;
					}
				}
				return (rate > 0 ? (int)(granule * 1000 / rate) : 0, rate, channels);
			}
			return (0, 0, 0);
		}

		/// <summary>The first BGM number the game ships no tune for and the project has not taken.</summary>
		public static int FreeBgm(Workspace workspace, List<AudioAsset> assets)
		{
			HashSet<string> taken = new HashSet<string>(assets.Select(a => a.Name), StringComparer.OrdinalIgnoreCase);
			for (int n = FirstFreeBgm; n <= LastFreeBgm; n++)
			{
				if (!taken.Contains("BGM" + n.ToString("00", CultureInfo.InvariantCulture))) return n;
			}
			return -1;
		}

		/// <summary>The sound-effect archive numbers a mod's own effects go under: the game's run to 277; there is no table to add to, a name's file is the effect.</summary>
		public const int FirstFreeSe = 300, LastFreeSe = 999;

		/// <summary>The first effect archive number (SEnnn) with no effect of the game's or the project's under it.</summary>
		public static int FreeSe(Workspace workspace, List<AudioAsset> assets)
		{
			HashSet<int> taken = new HashSet<int>();
			foreach (AudioAsset a in assets)
			{
				Match m = Regex.Match(a.Name, @"^SE(\d+)_", RegexOptions.IgnoreCase);
				if (m.Success) taken.Add(int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture));
			}
			for (int n = FirstFreeSe; n <= LastFreeSe; n++)
			{
				if (!taken.Contains(n)) return n;
			}
			return -1;
		}

		/// <summary>
		/// Writes a sound of the mod's own into the project's files: sound/&lt;name&gt;_&lt;part&gt;.ogg (or .wav
		/// by the bytes), and sound/&lt;name&gt;.dat with the loop point when one is given. The name is
		/// a BGMnn or an SEnnn_nn; a part is 0 (the intro) or 1 (the loop).
		/// </summary>
		public static string Import(Workspace workspace, string name, int part, byte[] bytes, int? loopMs)
		{
			if (string.IsNullOrWhiteSpace(name) || !Regex.IsMatch(name, @"^(BGM\d{2,3}|SE\d{3}_\d{2})$", RegexOptions.IgnoreCase)) throw new ArgumentException("a name like BGM30 or SE300_00");
			if (part != 0 && part != 1) throw new ArgumentException("part 0 (the intro) or 1 (the loop)");
			if (bytes == null || bytes.Length < 12) throw new ArgumentException("an Ogg Vorbis or WAV file");
			bool ogg = bytes[0] == 'O' && bytes[1] == 'g' && bytes[2] == 'g' && bytes[3] == 'S';
			bool wav = bytes[0] == 'R' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == 'F';
			if (!ogg && !wav) throw new ArgumentException("not an Ogg Vorbis (OggS) or a WAV (RIFF) file");
			name = name.ToUpperInvariant();
			string entry = OwnFolder + "/" + name + "_" + part + (ogg ? ".ogg" : ".wav");
			// One format per part: a .wav going in takes an .ogg of the same part out, and the other way.
			string other = Path.Combine(workspace.OverrideDirectory, OwnFolder, name + "_" + part + (ogg ? ".wav" : ".ogg"));
			if (File.Exists(other)) File.Delete(other);
			workspace.Write(entry, bytes);
			if (loopMs.HasValue && loopMs.Value >= 0)
			{
				workspace.Write(OwnFolder + "/" + name + ".dat", BitConverter.GetBytes(loopMs.Value));
			}
			return entry;
		}

		/// <summary>The bytes of one part of a sound of the mod's own, with its content type; null when the part is not the mod's.</summary>
		public static byte[] Own(Workspace workspace, string name, int part, out string contentType)
		{
			contentType = null;
			if (workspace.OverrideDirectory == null) return null;
			foreach ((string ext, string type) in new[] { (".ogg", "audio/ogg"), (".wav", "audio/wav") })
			{
				string path = Path.Combine(workspace.OverrideDirectory, OwnFolder, name + "_" + part + ext);
				if (File.Exists(path)) { contentType = type; return File.ReadAllBytes(path); }
			}
			return null;
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
			byte[] own = Own(workspace, name, part, out contentType);
			if (own != null) return own;
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
