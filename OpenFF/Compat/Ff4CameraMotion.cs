// FF4's camera motions: the .dsc sets in EVT_CAMERA.dat, played on the field camera.
//
// Read out of the FF4 binary (ds::sys3d::CameraMotionSet, CameraMotion, CameraHandle and
// ds::AbstractKeyDecoder, Tools/ff4_disasm.py):
//
//   set   "CMS2" | u32 version (0x30000) | u16 0x2c | char name[..] | @0x28 u32 count |
//         @0x2c count x { u32 id, u32 offset (from the file start) }
//   motion "CM4\0" | ... | @0x10 u32 frames | @0x14 u32 channel offset[8] (from the motion start)
//   channel u16 keys | u16 key type | keys...
//     type 0: (u8 frames, s8 delta)      type 1: (u16 frames, s16 delta)   type 2: (u32 frames, s32 delta)
//     type 3: float per frame, absolute  type 4: one s32, constant
//   A delta key adds its delta to the value once per frame for its frames; the value starts
//   at zero. The eight channels are the rotation quaternion x, y, z, w (fx12), the position
//   x, y, z (fx32) and a field of view as a 16-bit angle index. Each frame
//   CameraHandle::calculatePosition builds rotation x translation, puts the camera at the
//   translation, looks along (0,0,-1) x R and takes (0,1,0) x R as up - row vectors, NNS
//   style. The FOV channel it applies only when the handle's flag at +0x81 is set, and
//   nothing in the game sets it (the constructor and clear() zero it): a scene's FOV is
//   evt::EventCamera::initializeDefaultParameter's setFOV(0x424, 0xf74) - 30 degrees - at
//   the scene part's start and whatever eventCameraSetFovyMove set since. The scripts rely
//   on that: e01_00 sets 30 for shots whose channel says 43, and eight of its nineteen shots
//   set nothing and keep the previous value. (Read out with the channel constants of
//   e01_00.dsc against the script's eventCameraSetFovyMove lines.)
//   ce_PlayCameraMotion(slot, id, blend, loop): when a motion is already up, CameraHandle::start
//   saves the displayed pose (saveOldPosition) and calculatePosition slides position, rotation
//   (Quaternion::leap) and FOV to the new motion over `blend` frames. No shipped script passes
//   a blend or a loop (627 calls, all 0), so the slide is not built; a nonzero value is logged.
//   The CM4 header also carries a loop count (+4), a start wait (+8) and a loop wait (+0xc):
//   zero in every e01_00 motion, logged when not.

using System;
using System.Collections.Generic;
using System.IO;

namespace OpenFF.Client
{
	internal static class Ff4CameraMotion
	{
		private sealed class Motion
		{
			public uint Id;
			public int Frames;
			public int[][] Channels = new int[8][];   // value per frame, Frames + 1 entries
		}

		private sealed class MotionSet
		{
			public string Name;
			public Dictionary<uint, Motion> Motions = new Dictionary<uint, Motion>();
		}

		private static readonly Dictionary<int, MotionSet> _slots = new Dictionary<int, MotionSet>();
		private static Motion _playing;
		private static int _frame;
		private static bool _loop;

		// The scene camera's field of view as a 16-bit half-angle index: the part's default at a
		// scene's start, then eventCameraSetFovyMove's value, moved over frames. Motions never
		// change it (see the header). -1 until a scene sets it on this map.
		private static int _fovCurrent = -1;
		private static int _fovTarget = -1;
		private static int _fovFrom;
		private static int _fovFrames;
		private static int _fovTick;

		/// <summary>The event camera's default: setFOV(0x424, 0xf74), a 15-degree half angle - index 2731 gives exactly those words.</summary>
		public const int DefaultFovIndex = 2731;

		public static bool Playing => _playing != null && (_loop || _frame < _playing.Frames);
		public static bool Looping => _playing != null && _loop;

		// ---- loading ----

		public static void Setup(int slot, string name)
		{
			try
			{
				byte[] data = Read(name + ".dsc") ?? Read(name + ".dsc.lz");
				if (data == null)
				{
					Log.Write(LogChannel.General, "script: FF4 camera set " + name + " was not found");
					return;
				}
				if (data.Length > 4 && (data[0] == 0x10 || data[0] == 0x11) && !(data[0] == 'C' && data[1] == 'M'))
				{
					data = OpenFF.Content.Lz.Decompress(data);
				}
				MotionSet set = Parse(data, name);
				_slots[slot] = set;
				Log.Write(LogChannel.General, "script: FF4 camera set " + name + " in slot " + slot + ": " + set.Motions.Count + " motion(s)");
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "script: FF4 camera set " + name + ": " + ex.Message);
			}
		}

		public static void Cleanup(int slot)
		{
			_slots.Remove(slot);
		}

		private static byte[] Read(string name)
		{
			try
			{
				uint size = GlobalScope.ds.g_File.getSize(name);
				if (size == 0) return null;
				Array array = GlobalScope.ds.CHeap.alloc_app(size);
				if (array == null || !GlobalScope.ds.g_File.load(array, name)) return null;
				if (array is byte[] bytes) return bytes;
				byte[] copy = new byte[array.Length];
				Buffer.BlockCopy(array, 0, copy, 0, Math.Min(copy.Length, Buffer.ByteLength(array)));
				return copy;
			}
			catch (Exception) { return null; }
		}

		private static MotionSet Parse(byte[] data, string name)
		{
			if (data.Length < 0x30 || data[0] != 'C' || data[1] != 'M' || data[2] != 'S' || data[3] != '2')
			{
				throw new InvalidDataException("not a CMS2 camera set");
			}
			MotionSet set = new MotionSet { Name = name };
			int count = BitConverter.ToInt32(data, 0x28);
			for (int i = 0; i < count; i++)
			{
				uint id = BitConverter.ToUInt32(data, 0x2c + 8 * i);
				int offset = BitConverter.ToInt32(data, 0x30 + 8 * i);
				if (offset <= 0 || offset + 0x34 > data.Length) continue;
				if (data[offset] != 'C' || data[offset + 1] != 'M' || data[offset + 2] != '4') continue;
				Motion motion = new Motion { Id = id, Frames = BitConverter.ToInt32(data, offset + 0x10) };
				int loops = BitConverter.ToInt32(data, offset + 4), wait = BitConverter.ToInt32(data, offset + 8), loopWait = BitConverter.ToInt32(data, offset + 0xc);
				if (loops != 0 || wait != 0 || loopWait != 0)
				{
					Log.Write(LogChannel.General, "script: FF4 camera motion " + id + " in " + name + " has loop count " + loops + ", start wait " + wait + ", loop wait " + loopWait + " (not played)");
				}
				for (int channel = 0; channel < 8; channel++)
				{
					int at = offset + BitConverter.ToInt32(data, offset + 0x14 + 4 * channel);
					motion.Channels[channel] = Decode(data, at, motion.Frames);
				}
				set.Motions[id] = motion;
			}
			return set;
		}

		/// <summary>A channel's value at every frame, the key decoders run forward.</summary>
		private static int[] Decode(byte[] data, int at, int frames)
		{
			int[] values = new int[frames + 1];
			if (at <= 0 || at + 4 > data.Length) return values;
			int keys = BitConverter.ToUInt16(data, at);
			int type = BitConverter.ToUInt16(data, at + 2);
			int start = at + 4;
			switch (type)
			{
				case 4:
				{
					int constant = start + 4 <= data.Length ? BitConverter.ToInt32(data, start) : 0;
					for (int f = 0; f <= frames; f++) values[f] = constant;
					return values;
				}
				case 3:
				{
					int index = 0, value = 0;
					for (int f = 0; f <= frames; f++)
					{
						if (index < keys && start + 4 * index + 4 <= data.Length)
						{
							value = (int)BitConverter.ToSingle(data, start + 4 * index);
							index++;
						}
						values[f] = value;
					}
					return values;
				}
				case 0:
				case 1:
				case 2:
				{
					int stride = type == 0 ? 2 : type == 1 ? 4 : 8;
					Func<int, int> framesOf = k => type == 0 ? data[start + 2 * k]
						: type == 1 ? BitConverter.ToUInt16(data, start + 4 * k)
						: (int)(BitConverter.ToUInt32(data, start + 8 * k) & 0xffff);
					Func<int, int> deltaOf = k => type == 0 ? (sbyte)data[start + 2 * k + 1]
						: type == 1 ? BitConverter.ToInt16(data, start + 4 * k + 2)
						: BitConverter.ToInt32(data, start + 8 * k + 4);
					if (keys == 0 || start + stride > data.Length) return values;
					int value = 0, key = 0, remaining = framesOf(0);
					values[0] = 0;
					for (int f = 1; f <= frames; f++)
					{
						if (remaining > 0 && key < keys && start + stride * (key + 1) <= data.Length)
						{
							remaining--;
							value += deltaOf(key);
							if (remaining == 0)
							{
								key++;
								if (key < keys && start + stride * (key + 1) <= data.Length) remaining = framesOf(key);
							}
						}
						values[f] = value;
					}
					if (frames >= 1) values[0] = values[1];
					return values;
				}
				default:
					return values;
			}
		}

		// ---- playing ----

		/// <summary>A scene starts on this map (ContEventPart::initialize): the event camera's default FOV, unless a scene already set one here.</summary>
		public static void SceneStarted()
		{
			if (_fovCurrent >= 0) return;
			_fovCurrent = DefaultFovIndex;
			_fovTarget = DefaultFovIndex;
			_fovFrames = 0;
			ApplyFov(DefaultFovIndex);
		}

		/// <summary>The script's FOV move: degrees of full vertical FOV, over frames (0 = at once).</summary>
		public static void SetFovy(int degrees, int frames)
		{
			int index = (int)(degrees * 0.5 / 360.0 * 65536.0) & 0xffff;
			if (frames <= 0 || _fovCurrent < 0)
			{
				_fovCurrent = index;
				_fovTarget = index;
				_fovFrames = 0;
				ApplyFov(index);
				return;
			}
			_fovFrom = _fovCurrent;
			_fovTarget = index;
			_fovFrames = frames;
			_fovTick = 0;
		}

		private static void ApplyFov(int index)
		{
			try
			{
				GlobalScope.cmr.CWorldCamera camera = GlobalScope.CCastCommandTransit.getInstance().cast_FieldCamera();
				if (camera == null) return;
				double radians = index / 65536.0 * 2 * Math.PI;
				camera.setFOV((int)(Math.Sin(radians) * 4096), (int)(Math.Cos(radians) * 4096));
			}
			catch (Exception) { }
		}

		public static bool Play(int slot, uint id, int blend, bool loop)
		{
			if (OpenFF.Client.Options.Get("ff4cam") == "off")
			{
				Log.Write(LogChannel.File, "script: FF4 camera motion " + id + " skipped (--ff4cam=off)");
				return false;
			}
			if (!_slots.TryGetValue(slot, out MotionSet set))
			{
				Log.Write(LogChannel.General, "script: FF4 camera motion " + id + ": no set in slot " + slot);
				return false;
			}
			if (!set.Motions.TryGetValue(id, out Motion motion))
			{
				Log.Write(LogChannel.General, "script: FF4 camera motion " + id + " is not in set " + set.Name);
				return false;
			}
			if (blend != 0 && _playing != null)
			{
				Log.Write(LogChannel.General, "script: FF4 camera motion " + id + " asks a " + blend + "-frame slide from the last shot (not built; the shipped scripts never do)");
			}
			_playing = motion;
			_frame = 1;
			_loop = loop;
			GlobalScope.cmr.CWorldCamera.ExternalDrive = Drive;
			Log.Write(LogChannel.File, "script: FF4 camera motion " + id + " from " + set.Name + ", " + motion.Frames + " frames" + (loop ? ", looping" : ""));
			return true;
		}

		/// <summary>Lets the field camera go; a scene's end or a map change.</summary>
		public static void Stop()
		{
			_playing = null;
			if (GlobalScope.cmr.CWorldCamera.ExternalDrive == (Action<GlobalScope.cmr.CWorldCamera>)Drive)
			{
				GlobalScope.cmr.CWorldCamera.ExternalDrive = null;
			}
		}

		/// <summary>Each frame: on to the next frame; the camera picks the pose up in its own update.</summary>
		public static void Tick()
		{
			if (_fovFrames > 0)
			{
				_fovTick++;
				if (_fovTick >= _fovFrames)
				{
					_fovCurrent = _fovTarget;
					_fovFrames = 0;
				}
				else
				{
					_fovCurrent = _fovFrom + (int)((long)(_fovTarget - _fovFrom) * _fovTick / _fovFrames);
				}
				if (_playing == null) ApplyFov(_fovCurrent);
			}
			if (_playing == null) return;
			if (_frame < _playing.Frames)
			{
				_frame++;
			}
			else if (_loop)
			{
				_frame = 1;
			}
		}

		/// <summary>The camera's hook: the pose at the current frame, every frame the camera runs.</summary>
		private static void Drive(GlobalScope.cmr.CWorldCamera camera)
		{
			Motion m = _playing;
			if (m == null) return;
			int f = Math.Min(_frame, m.Frames);
			double qx = m.Channels[0][f] / 4096.0, qy = m.Channels[1][f] / 4096.0, qz = m.Channels[2][f] / 4096.0, qw = m.Channels[3][f] / 4096.0;
			double length = Math.Sqrt(qx * qx + qy * qy + qz * qz + qw * qw);
			if (length < 1e-6) { qx = qy = qz = 0; qw = 1; } else { qx /= length; qy /= length; qz /= length; qw /= length; }
			// ds::Quaternion::getRotateMatrix fills the standard matrix (_01 = 2(xy - wz), _10 = 2(xy + wz)
			// ...) and NNS multiplies ROW vectors (x' = x._00 + y._10 + z._20), so the camera's up is the
			// matrix's second row and its forward the negated third row.
			double r10 = 2 * (qx * qy + qz * qw), r11 = 1 - 2 * (qx * qx + qz * qz), r12 = 2 * (qy * qz - qx * qw);
			double r20 = 2 * (qx * qz - qy * qw), r21 = 2 * (qy * qz + qx * qw), r22 = 1 - 2 * (qx * qx + qy * qy);
			int tx = m.Channels[4][f], ty = m.Channels[5][f], tz = m.Channels[6][f];
			double fx = -r20, fy = -r21, fz = -r22;
			double ux = r10, uy = r11, uz = r12;
			try
			{
				GlobalScope.VecFx32 pos = new GlobalScope.VecFx32(tx, ty, tz);
				GlobalScope.VecFx32 trg = new GlobalScope.VecFx32(tx + (int)(fx * 4096 * 16), ty + (int)(fy * 4096 * 16), tz + (int)(fz * 4096 * 16));
				camera.Pos_set(pos);
				camera.setPrePos(pos);
				camera.Trg_set(trg);
				camera.setPreTrg(trg);
				camera.setPosition(pos);
				camera.setTarget(trg);
				camera.setCamUp((int)(ux * 4096), (int)(uy * 4096), (int)(uz * 4096));
				// The motion's FOV channel (m.Channels[7]) is not applied - FF4 does not (see the header);
				// the scene's FOV is held here against the camera's own updates.
				if (_fovCurrent >= 0)
				{
					double radians = _fovCurrent / 65536.0 * 2 * Math.PI;
					camera.setFOV((int)(Math.Sin(radians) * 4096), (int)(Math.Cos(radians) * 4096));
				}
			}
			catch (Exception ex)
			{
				Log.Write(LogChannel.General, "script: FF4 camera motion: " + ex.Message);
				Stop();
			}
		}

		public static void MapLeft()
		{
			Stop();
			_slots.Clear();
			_fovFrames = 0;
			_fovCurrent = -1;
		}
	}
}
