// FF4's field commands for sound and loading (babilCommand_*, read from libff4.so). The sounds: one of the field's four
// SE slots played (SlotSEPlay: WSCSound's handles, a pan 0..255 about 0x80) and its volume moved over frames
// (SlotSESetVolume), a sound faded in from silence (MoveVolumeSE: SEHandle::PlayFadeIn). The loading: what the DS waited
// for or made room for - sound banks loaded in pieces, the texture VRAM packed, the file table, the language folders -
// OpenFF loads as it needs, so each is read and nothing waits.

using System;

namespace OpenFF.Client
{
	internal static class Ff4SoundCommands
	{
		private static readonly GlobalScope.MatrixSound.MtxSEHandle[] _slots = new GlobalScope.MatrixSound.MtxSEHandle[4];

		/// <summary>A sound played as every FF4 sound is (MtxSENDS_Play: the bank and number, a pan 0..255 about 128).</summary>
		private static GlobalScope.MatrixSound.MtxSEHandle Play(int bank, int number, int volume, int pan) =>
			GlobalScope.MatrixSound.MtxSENDS_Play(bank, number, volume, Math.Clamp(pan, 0, 255));

		private static void Move(GlobalScope.MatrixSound.MtxSEHandle handle, int volume, int frames) =>
			(handle as GlobalScope.MatrixSound.MtxSEHandleNDS)?._pSEObject?.MoveVolume(GlobalScope.MatrixSound.computeSEVolume() * volume / 127, frames);

		private static int Volume(int v) => Math.Clamp(v, 0, 127);   // dssndVolumeClamp

		/// <summary>SlotSEPlay(slot, bank, number, volume, pan): the sound on slot 0..3, its pan the script's less 0x80.</summary>
		public static void SlotSEPlay(GlobalScope.ScriptEngine engine)
		{
			uint slot = engine.getDword();
			int bank = (int)engine.getDword(), number = (int)engine.getDword(), volume = (int)engine.getDword(), pan = (int)engine.getDword();
			if (slot > 3) return;
			try { _slots[slot] = Play(bank, number, Volume(volume), pan); }
			catch (Exception ex) { Log.Write(LogChannel.General, "script: slotSEPlay: " + ex.Message); }
		}

		/// <summary>SlotSESetVolume(slot, frames, volume): the slot's sound to the volume over the frames.</summary>
		public static void SlotSESetVolume(GlobalScope.ScriptEngine engine)
		{
			uint slot = engine.getDword();
			int frames = (int)engine.getDword(), volume = (int)engine.getDword();
			if (slot > 3) return;
			try { Move(_slots[slot], Volume(volume), frames); } catch (Exception) { }
		}

		/// <summary>MoveVolumeSE(bank, number, pan, volume, frames): the sound up from silence to the volume over the frames.</summary>
		public static void MoveVolumeSE(GlobalScope.ScriptEngine engine)
		{
			int bank = engine.getWord(), number = engine.getWord(), pan = engine.getWord(), volume = engine.getWord(), frames = engine.getWord();
			try
			{
				Move(Play(bank, number, 0, pan), Volume(volume), frames);
			}
			catch (Exception ex) { Log.Write(LogChannel.General, "script: moveVolumeSE: " + ex.Message); }
		}

		/// <summary>The loading commands: their operands read (by FF4's table), nothing to wait for or free.</summary>
		public static GlobalScope.SCRIPT_COMMAND Loads(string name)
		{
			OpenFF.Script.ScriptOp op = null;
			OpenFF.Script.ScriptOpTable table = OpenFF.Script.ScriptOpTable.Ff4;
			for (int i = 0; i < table.Count && op == null; i++)
				if (table.Get(i) is OpenFF.Script.ScriptOp candidate && OpenFF.Script.ScriptOpTable.Simplify(candidate.Name) == name) op = candidate;
			OpenFF.Script.Operand[] operands = op?.Operands ?? Array.Empty<OpenFF.Script.Operand>();
			return engine =>
			{
				foreach (OpenFF.Script.Operand operand in operands)
				{
					if (operand == OpenFF.Script.Operand.Byte) engine.getByte();
					else if (operand == OpenFF.Script.Operand.Word) engine.getWord();
					else if (operand == OpenFF.Script.Operand.Dword) engine.getDword();
					else engine.getString();
				}
			};
		}
	}
}
