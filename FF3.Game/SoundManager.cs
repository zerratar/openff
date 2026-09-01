using System;
using System.Collections.Generic;
using android.media;

internal class SoundManager : MediaPlayer.OnCompletionListener
{
	private readonly Dictionary<string, int> SoundAssignTable = new Dictionary<string, int>
	{
		{ "BGM00", 2 },
		{ "BGM01", 3 },
		{ "BGM02", 3 },
		{ "BGM03", 3 },
		{ "BGM04", 2 },
		{ "BGM05", 3 },
		{ "BGM06", 3 },
		{ "BGM07", 2 },
		{ "BGM08", 2 },
		{ "BGM09", 3 },
		{ "BGM10", 2 },
		{ "BGM11", 3 },
		{ "BGM12", 3 },
		{ "BGM13", 3 },
		{ "BGM14", 2 },
		{ "BGM15", 3 },
		{ "BGM16", 3 },
		{ "BGM17", 3 },
		{ "BGM18", 3 },
		{ "BGM19", 2 },
		{ "BGM20", 3 },
		{ "BGM21", 3 },
		{ "BGM22", 3 },
		{ "BGM23", 3 },
		{ "BGM24", 2 },
		{ "BGM25", 3 },
		{ "BGM26", 3 },
		{ "BGM27", 2 },
		{ "BGM28", 2 },
		{ "BGM29", 2 },
		{ "BGM30", 2 },
		{ "BGM31", 2 },
		{ "BGM32", 3 },
		{ "BGM33", 2 },
		{ "BGM34", 3 },
		{ "BGM35", 1 },
		{ "BGM36", 1 },
		{ "BGM37", 1 },
		{ "BGM38", 2 },
		{ "BGM39", 3 },
		{ "BGM40", 1 },
		{ "BGM41", 1 },
		{ "BGM42", 1 },
		{ "BGM43", 1 },
		{ "BGM44", 1 },
		{ "BGM45", 1 },
		{ "BGM46", 1 },
		{ "BGM47", 3 },
		{ "BGM48", 3 },
		{ "BGM49", 3 },
		{ "BGM50", 3 },
		{ "BGM51", 3 },
		{ "BGM52", 1 },
		{ "BGM53", 3 },
		{ "BGM54", 1 },
		{ "BGM55", 3 },
		{ "BGM56", 2 },
		{ "BGM57", 2 },
		{ "BGM58", 3 },
		{ "SE000_00", 1 },
		{ "SE000_01", 1 },
		{ "SE000_02", 1 },
		{ "SE000_03", 1 },
		{ "SE000_06", 1 },
		{ "SE000_07", 1 },
		{ "SE000_09", 2 },
		{ "SE000_10", 2 },
		{ "SE000_11", 2 },
		{ "SE000_22", 2 },
		{ "SE000_31", 1 },
		{ "SE000_40", 1 },
		{ "SE001_00", 1 },
		{ "SE001_01", 1 },
		{ "SE001_02", 1 },
		{ "SE001_03", 1 },
		{ "SE001_06", 1 },
		{ "SE001_07", 1 },
		{ "SE001_08", 1 },
		{ "SE001_12", 1 },
		{ "SE001_13", 1 },
		{ "SE001_14", 1 },
		{ "SE001_15", 1 },
		{ "SE001_16", 1 },
		{ "SE001_17", 1 },
		{ "SE001_18", 1 },
		{ "SE001_24", 1 },
		{ "SE001_25", 1 },
		{ "SE001_26", 1 },
		{ "SE001_28", 1 },
		{ "SE001_29", 1 },
		{ "SE001_32", 1 },
		{ "SE001_33", 1 },
		{ "SE001_34", 1 },
		{ "SE001_35", 1 },
		{ "SE001_36", 1 },
		{ "SE001_37", 1 },
		{ "SE001_38", 1 },
		{ "SE001_39", 1 },
		{ "SE001_40", 1 },
		{ "SE001_41", 1 },
		{ "SE001_42", 1 },
		{ "SE001_43", 1 },
		{ "SE001_44", 1 },
		{ "SE001_45", 1 },
		{ "SE001_46", 1 },
		{ "SE001_47", 1 },
		{ "SE001_48", 1 },
		{ "SE001_49", 1 },
		{ "SE001_50", 1 },
		{ "SE001_51", 1 },
		{ "SE001_52", 1 },
		{ "SE001_53", 1 },
		{ "SE001_54", 1 },
		{ "SE002_00", 1 },
		{ "SE002_01", 1 },
		{ "SE002_02", 1 },
		{ "SE002_03", 1 },
		{ "SE002_04", 1 },
		{ "SE002_05", 1 },
		{ "SE002_06", 1 },
		{ "SE002_08", 1 },
		{ "SE002_09", 1 },
		{ "SE002_10", 1 },
		{ "SE002_12", 1 },
		{ "SE003_00", 1 },
		{ "SE003_01", 1 },
		{ "SE004_00", 1 },
		{ "SE004_01", 1 },
		{ "SE004_02", 1 },
		{ "SE005_00", 1 },
		{ "SE005_01", 1 },
		{ "SE005_03", 1 },
		{ "SE005_05", 1 },
		{ "SE005_06", 1 },
		{ "SE006_00", 1 },
		{ "SE006_01", 1 },
		{ "SE007_00", 1 },
		{ "SE007_01", 1 },
		{ "SE007_02", 1 },
		{ "SE007_03", 1 },
		{ "SE008_02", 1 },
		{ "SE008_04", 1 },
		{ "SE008_05", 1 },
		{ "SE009_00", 1 },
		{ "SE009_01", 1 },
		{ "SE009_03", 1 },
		{ "SE009_06", 1 },
		{ "SE009_07", 1 },
		{ "SE009_08", 1 },
		{ "SE009_09", 1 },
		{ "SE009_10", 1 },
		{ "SE010_00", 1 },
		{ "SE011_00", 1 },
		{ "SE011_01", 1 },
		{ "SE011_02", 1 },
		{ "SE011_03", 1 },
		{ "SE011_04", 1 },
		{ "SE011_06", 1 },
		{ "SE011_07", 2 },
		{ "SE012_01", 2 },
		{ "SE012_03", 1 },
		{ "SE012_04", 1 },
		{ "SE012_05", 1 },
		{ "SE012_06", 1 },
		{ "SE012_07", 1 },
		{ "SE012_08", 1 },
		{ "SE012_09", 1 },
		{ "SE013_00", 1 },
		{ "SE013_04", 1 },
		{ "SE013_05", 1 },
		{ "SE013_06", 1 },
		{ "SE013_07", 1 },
		{ "SE013_08", 1 },
		{ "SE014_00", 1 },
		{ "SE014_01", 1 },
		{ "SE014_03", 1 },
		{ "SE014_04", 1 },
		{ "SE014_05", 1 },
		{ "SE014_06", 1 },
		{ "SE014_07", 1 },
		{ "SE015_00", 1 },
		{ "SE015_01", 1 },
		{ "SE015_02", 1 },
		{ "SE015_03", 1 },
		{ "SE016_00", 2 },
		{ "SE016_02", 1 },
		{ "SE016_04", 2 },
		{ "SE016_05", 1 },
		{ "SE017_00", 1 },
		{ "SE017_01", 1 },
		{ "SE017_02", 1 },
		{ "SE018_01", 1 },
		{ "SE018_02", 1 },
		{ "SE018_03", 1 },
		{ "SE018_04", 1 },
		{ "SE018_05", 1 },
		{ "SE018_06", 1 },
		{ "SE018_07", 1 },
		{ "SE018_08", 1 },
		{ "SE018_09", 1 },
		{ "SE018_10", 1 },
		{ "SE018_11", 1 },
		{ "SE018_15", 2 },
		{ "SE018_16", 1 },
		{ "SE018_17", 1 },
		{ "SE018_18", 1 },
		{ "SE018_20", 1 },
		{ "SE018_21", 1 },
		{ "SE019_00", 1 },
		{ "SE019_01", 1 },
		{ "SE019_02", 1 },
		{ "SE019_03", 1 },
		{ "SE019_04", 1 },
		{ "SE020_00", 1 },
		{ "SE020_01", 2 },
		{ "SE020_02", 1 },
		{ "SE021_00", 2 },
		{ "SE021_01", 1 },
		{ "SE021_02", 1 },
		{ "SE021_03", 2 },
		{ "SE021_04", 1 },
		{ "SE022_00", 1 },
		{ "SE022_01", 1 },
		{ "SE023_12", 1 },
		{ "SE023_13", 1 },
		{ "SE023_14", 1 },
		{ "SE023_19", 1 },
		{ "SE023_22", 2 },
		{ "SE024_19", 1 },
		{ "SE025_00", 1 },
		{ "SE027_01", 1 },
		{ "SE030_00", 1 },
		{ "SE030_01", 1 },
		{ "SE031_00", 1 },
		{ "SE031_01", 1 },
		{ "SE031_02", 1 },
		{ "SE031_03", 1 },
		{ "SE031_04", 1 },
		{ "SE031_05", 1 },
		{ "SE031_06", 1 },
		{ "SE032_00", 2 },
		{ "SE032_01", 1 },
		{ "SE032_02", 2 },
		{ "SE033_00", 1 },
		{ "SE033_01", 2 },
		{ "SE034_02", 1 },
		{ "SE035_00", 1 },
		{ "SE096_00", 1 },
		{ "SE096_01", 2 },
		{ "SE096_02", 1 },
		{ "SE096_03", 1 },
		{ "SE096_04", 1 },
		{ "SE098_00", 1 },
		{ "SE098_01", 1 },
		{ "SE098_02", 1 },
		{ "SE098_03", 1 },
		{ "SE098_04", 1 },
		{ "SE098_05", 1 },
		{ "SE098_06", 1 },
		{ "SE098_10", 1 },
		{ "SE098_11", 1 },
		{ "SE098_12", 1 },
		{ "SE098_13", 1 },
		{ "SE098_14", 1 },
		{ "SE098_15", 1 },
		{ "SE098_16", 1 },
		{ "SE098_17", 1 },
		{ "SE098_19", 1 },
		{ "SE098_20", 1 },
		{ "SE098_30", 1 },
		{ "SE098_31", 1 },
		{ "SE098_33", 1 },
		{ "SE098_34", 1 },
		{ "SE098_35", 1 },
		{ "SE098_37", 1 },
		{ "SE098_39", 1 },
		{ "SE098_40", 1 },
		{ "SE098_41", 1 },
		{ "SE098_42", 1 },
		{ "SE098_43", 1 },
		{ "SE098_44", 1 },
		{ "SE099_00", 1 },
		{ "SE099_01", 1 },
		{ "SE099_02", 1 },
		{ "SE200_00", 1 },
		{ "SE200_01", 1 },
		{ "SE200_02", 1 },
		{ "SE200_03", 1 },
		{ "SE200_04", 1 },
		{ "SE200_05", 1 },
		{ "SE200_06", 1 },
		{ "SE200_07", 2 },
		{ "SE200_08", 1 },
		{ "SE200_09", 1 },
		{ "SE200_10", 1 },
		{ "SE200_11", 1 },
		{ "SE200_12", 1 },
		{ "SE200_13", 1 },
		{ "SE200_14", 1 },
		{ "SE200_15", 1 },
		{ "SE200_16", 1 },
		{ "SE200_17", 1 },
		{ "SE201_00", 1 },
		{ "SE201_01", 1 },
		{ "SE201_02", 1 },
		{ "SE201_03", 1 },
		{ "SE201_04", 1 },
		{ "SE201_05", 1 },
		{ "SE201_06", 1 },
		{ "SE201_07", 1 },
		{ "SE201_08", 1 },
		{ "SE201_09", 1 },
		{ "SE201_10", 1 },
		{ "SE201_11", 1 },
		{ "SE201_12", 1 },
		{ "SE201_13", 1 },
		{ "SE201_14", 1 },
		{ "SE201_15", 1 },
		{ "SE201_16", 1 },
		{ "SE201_17", 1 },
		{ "SE201_18", 1 },
		{ "SE201_19", 1 },
		{ "SE201_20", 1 },
		{ "SE201_21", 1 },
		{ "SE201_22", 1 },
		{ "SE202_00", 1 },
		{ "SE202_01", 1 },
		{ "SE202_02", 1 },
		{ "SE202_05", 1 },
		{ "SE202_06", 1 },
		{ "SE202_07", 1 },
		{ "SE202_08", 1 },
		{ "SE202_09", 1 },
		{ "SE202_10", 1 },
		{ "SE202_12", 1 },
		{ "SE202_14", 1 },
		{ "SE202_15", 1 },
		{ "SE202_16", 1 },
		{ "SE203_01", 1 },
		{ "SE203_02", 1 },
		{ "SE203_03", 1 },
		{ "SE203_05", 1 },
		{ "SE203_06", 1 },
		{ "SE203_07", 1 },
		{ "SE203_08", 1 },
		{ "SE203_09", 1 },
		{ "SE203_10", 1 },
		{ "SE203_11", 1 },
		{ "SE203_12", 1 },
		{ "SE203_13", 1 },
		{ "SE203_14", 1 },
		{ "SE204_00", 1 },
		{ "SE204_01", 1 },
		{ "SE204_02", 1 },
		{ "SE204_03", 1 },
		{ "SE204_08", 1 },
		{ "SE204_09", 1 },
		{ "SE204_10", 1 },
		{ "SE204_11", 1 },
		{ "SE204_12", 1 },
		{ "SE204_13", 1 },
		{ "SE204_14", 1 },
		{ "SE204_15", 1 },
		{ "SE205_00", 1 },
		{ "SE206_00", 1 },
		{ "SE210_00", 1 },
		{ "SE210_01", 1 },
		{ "SE210_02", 1 },
		{ "SE210_03", 1 },
		{ "SE210_04", 1 },
		{ "SE210_05", 1 },
		{ "SE210_06", 1 },
		{ "SE210_07", 1 },
		{ "SE210_08", 1 },
		{ "SE210_09", 1 },
		{ "SE250_00", 1 },
		{ "SE250_02", 1 },
		{ "SE251_00", 1 },
		{ "SE251_01", 1 },
		{ "SE251_02", 1 },
		{ "SE252_00", 1 },
		{ "SE252_01", 1 },
		{ "SE252_02", 1 },
		{ "SE253_00", 1 },
		{ "SE253_01", 1 },
		{ "SE253_02", 1 },
		{ "SE254_00", 1 },
		{ "SE254_01", 1 },
		{ "SE254_02", 1 },
		{ "SE255_00", 1 },
		{ "SE255_01", 1 },
		{ "SE255_02", 1 },
		{ "SE256_00", 1 },
		{ "SE256_01", 1 },
		{ "SE256_02", 1 },
		{ "SE257_00", 1 },
		{ "SE257_01", 1 },
		{ "SE257_02", 1 },
		{ "SE260_00", 1 },
		{ "SE260_01", 1 },
		{ "SE260_02", 1 },
		{ "SE261_00", 1 },
		{ "SE261_01", 1 },
		{ "SE261_02", 1 },
		{ "SE262_00", 1 },
		{ "SE262_01", 1 },
		{ "SE262_02", 1 },
		{ "SE263_00", 1 },
		{ "SE263_01", 1 },
		{ "SE263_02", 1 },
		{ "SE264_00", 1 },
		{ "SE264_01", 1 },
		{ "SE264_02", 1 },
		{ "SE265_00", 1 },
		{ "SE265_01", 1 },
		{ "SE265_02", 1 },
		{ "SE266_00", 1 },
		{ "SE266_01", 1 },
		{ "SE266_02", 1 },
		{ "SE267_00", 1 },
		{ "SE267_01", 1 },
		{ "SE267_02", 1 },
		{ "SE270_00", 1 },
		{ "SE270_01", 1 },
		{ "SE270_03", 1 },
		{ "SE270_05", 1 },
		{ "SE271_00", 1 },
		{ "SE271_03", 1 },
		{ "SE272_00", 1 },
		{ "SE272_03", 1 },
		{ "SE273_00", 1 },
		{ "SE273_03", 1 },
		{ "SE274_00", 1 },
		{ "SE274_01", 1 },
		{ "SE274_02", 1 },
		{ "SE274_03", 1 },
		{ "SE275_00", 1 },
		{ "SE275_02", 1 },
		{ "SE275_03", 1 },
		{ "SE276_00", 1 },
		{ "SE276_03", 1 },
		{ "SE277_00", 1 },
		{ "SE277_01", 1 },
		{ "SE277_02", 1 },
		{ "SE277_03", 1 }
	};

	private MediaPlayer[,] sound = new MediaPlayer[32, 2];

	private float[] soundVolume = new float[32];

	private int[] soundState = new int[32];

	private int[] soundLoop = new int[32];

	private bool[] soundMute = new bool[32];

	private string[] soundFilename = new string[32];

	private bool[] updateVolume = new bool[32];

	private long soundTime;

	public void onCompletion(MediaPlayer mediaPlayer)
	{
		for (int i = 0; i < sound.GetLength(0); i++)
		{
			MediaPlayer[,] array = sound;
			if (mediaPlayer == array[i, 0])
			{
				array[i, 0].setOnCompletionListener(null);
				soundState[i] = 0;
			}
		}
	}

	public void updateSound()
	{
		long num = JavaSystem.currentTimeMillis();
		int num2 = Math.Max((int)(num - soundTime), 0);
		soundTime = num;
		for (int i = 0; i < sound.GetLength(0); i++)
		{
			MediaPlayer[,] array = sound;
			if (updateVolume[i])
			{
				updateVolume[i] = false;
				for (int j = 0; j < 2; j++)
				{
					if (array[i, j] != null)
					{
						array[i, j].setVolume(soundVolume[i], soundVolume[i], soundMute[i]);
						break;
					}
				}
			}
			if (soundState[i] != 1 || array[i, 0] == null || array[i, 1] == null)
			{
				continue;
			}
			soundLoop[i] -= num2;
			if (soundLoop[i] <= -50)
			{
				float num3 = soundVolume[i];
				if (soundLoop[i] > -150)
				{
					float num4 = (float)(-(soundLoop[i] + 50)) * 0.01f;
					float num5 = 1f - num4;
					array[i, 0].setVolume(num3 * num5, num3 * num5, soundMute[i]);
					array[i, 1].setVolume(num3 * num4, num3 * num4, soundMute[i]);
				}
				else
				{
					array[i, 1].setVolume(num3, num3, soundMute[i]);
					array[i, 0].stop();
					array[i, 0].release();
					array[i, 0] = null;
				}
			}
		}
	}

	public void playSound(int channel, string filename)
	{
		stopSound(channel);
		soundFilename[channel] = filename;
		MediaPlayer[,] array = sound;
		float num = soundVolume[channel];
		for (int i = 0; i < 2; i++)
		{
			if (SoundAssignTable.ContainsKey(filename) && (SoundAssignTable[filename] & (1 << i)) != 0)
			{
				array[channel, i] = new MediaPlayer();
				try
				{
					array[channel, i].setDataSource(filename + "_" + i);
					array[channel, i].prepare();
					array[channel, i].setVolume(num, num, soundMute[channel]);
					array[channel, i].setLooping(i == 1);
					num = 0f;
				}
				catch (Exception)
				{
					array[channel, i].release();
					array[channel, i] = null;
				}
			}
		}
		if (array[channel, 0] != null && array[channel, 1] != null)
		{
			byte[] array2 = MainActivity.loadFileEntry("sound/" + filename + ".dat");
			soundLoop[channel] = (array2[0] & 0xFF) | ((array2[1] & 0xFF) << 8) | ((array2[2] & 0xFF) << 16) | (array2[3] << 24);
			soundLoop[channel] += Math.Max((int)(JavaSystem.currentTimeMillis() - soundTime), 0);
			soundState[channel] = 1;
			array[channel, 0].start();
			array[channel, 1].start();
		}
		else if (array[channel, 0] != null)
		{
			soundState[channel] = 1;
			array[channel, 0].setOnCompletionListener(this);
			array[channel, 0].start();
		}
		else if (array[channel, 1] != null)
		{
			soundState[channel] = 1;
			array[channel, 1].start();
		}
	}

	public void stopSound(int channel)
	{
		MediaPlayer[,] array = sound;
		soundState[channel] = 0;
		soundLoop[channel] = 0;
		for (int i = 0; i < 2; i++)
		{
			if (array[channel, i] != null)
			{
				array[channel, i].stop();
				array[channel, i].setOnCompletionListener(null);
				array[channel, i].release();
				array[channel, i] = null;
			}
		}
		soundFilename[channel] = null;
	}

	public void pauseSound(int channel, int pause)
	{
		MediaPlayer[,] array = sound;
		if (soundState[channel] == 1 && pause == 0)
		{
			playSound(channel, soundFilename[channel]);
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (array[channel, i] != null)
			{
				if (pause != 0)
				{
					array[channel, i].pause();
					soundState[channel] = 2;
				}
				else
				{
					array[channel, i].start();
					soundState[channel] = 1;
				}
			}
		}
	}

	public void pauseSoundAll(bool pause)
	{
		soundTime = JavaSystem.currentTimeMillis();
		for (int i = 0; i < sound.GetLength(0); i++)
		{
			if (!pause && soundState[i] != 1)
			{
				continue;
			}
			for (int j = 0; j < 2; j++)
			{
				MediaPlayer mediaPlayer = sound[i, j];
				if (mediaPlayer != null)
				{
					if (pause)
					{
						mediaPlayer.pause();
					}
					else
					{
						mediaPlayer.start();
					}
				}
			}
		}
	}

	public void stopSoundAll()
	{
		for (int i = 0; i < sound.GetLength(0); i++)
		{
			stopSound(i);
		}
	}

	public void setSoundVolume(int channel, float volume)
	{
		soundVolume[channel] = volume;
		updateVolume[channel] = true;
	}

	public int getSoundState(int channel)
	{
		return soundState[channel];
	}

	public void muteSound(bool bMute)
	{
		for (int i = 0; i < sound.GetLength(0); i++)
		{
			MediaPlayer[,] array = sound;
			soundMute[i] = bMute;
			for (int j = 0; j < 2; j++)
			{
				if (array[i, j] != null)
				{
					array[i, j].setVolume(soundVolume[i], soundVolume[i], soundMute[i]);
					break;
				}
			}
		}
	}
}
