using System;
using android.app;
using android.content;
using System.Threading;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using android.view;

// The game host. Was an Android Activity implementing a GLSurfaceView renderer;
// Game1 now drives these callbacks directly, so the lifecycle indirection is gone
// and the per-frame path is one hop instead of four.
public class MainActivity
{
	/// <summary>Resource strings are plain constants; this was a pass-through.</summary>
	public string getString(string resId) => resId;

	private class ExitDialogPositiveButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
			activity.appEnd();
		}
	}

	private class ExitDialogNegativeButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
		}
	}




	private class ConfirmDialogPositiveButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
			GlobalScope.Dialog.showMarket();
		}
	}

	private class ConfirmDialogNegativeButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
			GlobalScope.UserInfo.confirm_state = 3;
		}
	}

	private class UpdateDialogPositiveButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
			GlobalScope.Dialog.showMarket();
		}
	}

	private class UpdateDialogNegativeButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
		}
	}

	public static MainActivity activity;

	private string editString;

	private float[] touchX = new float[2];

	private float[] touchY = new float[2];

	private int touchCount;

	private int touchPeak;

	private int keyEvent;

	private bool keyAssign;

	private int viewX;

	private int viewY;

	private int viewW;

	private int viewH;

	private int language;

	private bool end;

	private bool suspend;

	private static SoundManager sound = new SoundManager();



	private static GlobalScope.JNIEnv m_Env = new GlobalScope.JNIEnv();

	public int init()
	{
		return GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_init(m_Env, this);
	}

	public void quit()
	{
		GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_quit(m_Env, this);
	}

	public void resume()
	{
		GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_resume(m_Env, this);
	}

	public void pause()
	{
		GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_pause(m_Env, this);
	}

	public void render()
	{
		GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_render(m_Env, this);
	}

	public void touch(int count, int peak, float x0, float y0, float x1, float y1)
	{
		GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_touch(m_Env, this, count, peak, x0, y0, x1, y1);
	}

	public static void encode(byte[] data, int mask)
	{
		GlobalScope.Java_com_square_1enix_FFIII_1J_MainActivity_encode(m_Env, null, data, mask);
	}

	public void onCreate()
	{
		// PORT: the selected language is a single byte in an embedded resource. This
		// used to be read through an Android raw-resource InputStream.
		try
		{
			byte[] selected = syrcusW.res.raw.language.language_dat;
			if (selected != null && selected.Length > 0)
			{
				language = selected[0];
			}
		}
		catch (Exception ex)
		{
			FF3.Log.Write(FF3.LogChannel.General, "language resource unreadable: " + ex.Message);
		}
		// The surface is the game window; report its size so the touch mapping and
		// the renderer agree on the 800x480 view the game was authored against.
		onSurfaceCreated();
		onSurfaceChanged(800, 480);
		// PORT: an entitlement check lived here and quit the game outright if the
		// Square Enix auth handshake had not succeeded. There is no such handshake
		// on Windows, so the check and the account layer behind it are gone.
		init();
	}

	public void onDestroy()
	{
		quit();
		sound.stopSoundAll();
	}

	public void onPause()
	{
		// PORT: dismissed the Android soft keyboard here. FF3.TextEntry is drawn by
		// the game itself and needs no dismissing when the window loses focus.
		pause();
		sound.pauseSoundAll(pause: true);
	}

	public void onResume()
	{
		if (MediaPlayer.State == MediaState.Playing)
		{
			sound.muteSound(bMute: true);
		}
		else
		{
			sound.muteSound(bMute: false);
		}
		suspend = true;
	}

	/// <summary>Asks whether to quit. The confirm path calls appEnd().</summary>
	public void finish()
	{
		onCreateDialog(0);
	}

	public void appEnd()
	{
		end = true;
		sound.stopSoundAll();
		JavaSystem.exit(0);
	}

	private object onCreateDialog(int id)
	{
		if (id == 0)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.setTitle(getString(R.@string.CLOSE_APP_TITLE));
			builder.setMessage(getString(R.@string.CLOSE_APP));
			builder.setPositiveButton(getString(R.@string.YES), new ExitDialogPositiveButtonListener());
			builder.setNegativeButton(getString(R.@string.NO), new ExitDialogNegativeButtonListener());
			return builder.create();
		}
		_ = 1;
		_ = 2;
		return null;
	}

	public static void startDownload()
	{
	}

	private void startDownloadActivity()
	{
		// PORT: this kicked off the resource download when content was missing.
		// On Windows the data ships with the game, so there is nothing to fetch.
		end = true;
		quit();
		sound.stopSoundAll();
	}

	public void onSurfaceCreated()
	{
		activity = this;
	}

	public void onSurfaceChanged(int width, int height)
	{
		if (width * 480 >= height * 800)
		{
			viewH = height;
			viewW = height * 800 / 480;
		}
		else
		{
			viewW = width;
			viewH = width * 480 / 800;
		}
		viewX = (width - viewW) / 2;
		viewY = (height - viewH) / 2;
		// PORT: the viewport is owned by the renderer; these values remain
		// because onTouchEvent normalises against them.
	}

	public bool onTouchEvent(MotionEvent e)
	{
		switch (e.getAction())
		{
		case 0:
		case 2:
		case 261:
		{
			touchCount = 0;
			for (int i = 0; i < e.getPointerCount(); i++)
			{
				if (touchCount >= 2)
				{
					break;
				}
				touchX[touchCount] = (e.getX(i) - (float)viewX) / (float)viewW;
				touchY[touchCount] = (e.getY(i) - (float)viewY) / (float)viewH;
				if (touchX[touchCount] >= 0f && touchX[touchCount] < 1f && touchY[touchCount] >= 0f && touchY[touchCount] < 1f)
				{
					touchCount++;
				}
			}
			touchPeak = Math.Max(touchCount, touchPeak);
			break;
		}
		case 262:
			touchCount = 0;
			touchX[touchCount] = (e.getX() - (float)viewX) / (float)viewW;
			touchY[touchCount] = (e.getY() - (float)viewY) / (float)viewH;
			if (touchX[touchCount] >= 0f && touchX[touchCount] < 1f && touchY[touchCount] >= 0f && touchY[touchCount] < 1f)
			{
				touchCount++;
			}
			touchPeak = Math.Max(touchCount, touchPeak);
			break;
		case 1:
			touchCount = 0;
			break;
		}
		if (touchPeak == 2)
		{
			touchPeak = touchCount;
		}
		return true;
	}

	public bool onKeyDown(int keyCode, KeyEvent @event)
	{
		if (keyCode == 4 && keyAssign)
		{
			keyEvent |= 2;
			return false;
		}
		return true;
	}

	public void onDrawFrame()
	{
		if (end)
		{
			return;
		}
		if (suspend)
		{
			try
			{
				Thread.Sleep(50);
			}
			catch (Exception)
			{
			}
			// PORT: waited here until the phone's lock screen was dismissed.
			touchPeak = (touchCount = 0);
			suspend = false;
			sound.pauseSoundAll(pause: false);
			resume();
		}
		touch(touchCount, touchPeak, touchX[0], touchY[0], touchX[1], touchY[1]);
		touchPeak = touchCount;
		render();
		sound.updateSound();
		// PORT: threw the frame away while the soft keyboard was up, to leave the
		// device some slack. Text entry is in-game now, so the frame keeps running.
	}

	public static byte[] loadFileEntry(string filename)
	{
		// PORT: content could also be served from a downloaded data.zip. A Windows
		// build always ships the archives, so only that path remains.
		return FF3.GameArchive.Read(filename);
	}

	public static byte[] loadFile(string filename)
	{
		string[] array = new string[9] { "ja", "en", "fr", "de", "it", "es", "zh_CN", "zh_TW", "ko" };
		byte[] array2 = null;
		int num = filename.LastIndexOf('.');
		if (num == -1)
		{
			num = filename.Length;
		}
		string text = filename.Substring(num);
		if (array2 == null)
		{
			array2 = loadFileEntry(array[activity.language] + ".lproj/" + filename);
		}
		if (array2 == null)
		{
			array2 = loadFileEntry("files/" + filename);
		}
		if (array2 == null)
		{
			return null;
		}
		if (text.Equals(".msd"))
		{
			if (FF3.Ff4Text.IsWide(array2))
			{
				// PORT: FF4's text is UTF-16LE; the message code reads UTF-8 (see Ff4Text).
				array2 = FF3.Ff4Text.DecodeWide(array2);
			}
			else if (filename[0] != 'e')
			{
				array2 = decodeString(array2);
			}
		}
		return array2;
	}

	public static byte[] decodeString(byte[] file)
	{
		if (activity.language >= 6)
		{
			return file;
		}
		string charsetName = ((activity.language == 0) ? "SJIS" : "windows-1252");
		byte[] array = new byte[file.Length * 2];
		int num = (file[8] & 0xFF) | ((file[9] & 0xFF) << 8) | ((file[10] & 0xFF) << 16) | ((file[11] & 0xFF) << 24);
		JavaSystem.arraycopy(file, 0, array, 0, 16 + 12 * num);
		int num2 = 16;
		int num3 = 16;
		int num4 = 16 + 12 * num;
		for (int i = 0; i < num; i++)
		{
			array[num3 + 8] = (byte)num4;
			array[num3 + 8 + 1] = (byte)(num4 >> 8);
			array[num3 + 8 + 2] = (byte)(num4 >> 16);
			array[num3 + 8 + 3] = (byte)(num4 >> 24);
			int num5 = file[num2 + 4];
			int num6 = (file[num2 + 8] & 0xFF) | ((file[num2 + 8 + 1] & 0xFF) << 8) | ((file[num2 + 8 + 2] & 0xFF) << 16) | ((file[num2 + 8 + 3] & 0xFF) << 24);
			for (int j = 0; j < num5; j++)
			{
				int k;
				for (k = 0; file[num6 + k] != 0; k++)
				{
				}
				byte[] array2;
				try
				{
					array2 = StringUtil.getBytes(StringUtil.createString(file, num6, k, charsetName), "UTF-8");
				}
				catch (Exception)
				{
					array2 = new byte[1];
				}
				JavaSystem.arraycopy(array2, 0, array, num4, array2.Length);
				array[num4 + array2.Length] = 0;
				num6 += k + 1;
				num4 += array2.Length + 1;
			}
			array[num4++] = 0;
			num2 += 12;
			num3 += 12;
		}
		trace("decodeString " + num);
		byte[] array3 = new byte[num4];
		JavaSystem.arraycopy(array, 0, array3, 0, num4);
		return array3;
	}

	public static void createSaveFile(int size)
	{
		// PORT: was Android per-app private storage, mapped onto IsolatedStorage.
		FF3.SaveFiles.Create("save.bin", size);
	}

	public static void trace(string text)
	{
	}

	public static long getCurrentFrame(long prevFrame)
	{
		long num;
		for (num = JavaSystem.currentTimeMillis() * 3 / 100; num == prevFrame; num = JavaSystem.currentTimeMillis() * 3 / 100)
		{
			try
			{
				Thread.Sleep(1);
			}
			catch (Exception)
			{
			}
		}
		return num;
	}

	public static int getLanguage()
	{
		return activity.language;
	}

	public static void setLanguage(int language)
	{
		activity.language = language;
		GlobalScope.setResourceCulture(language);
	}

	public static void assignBackButton(int assign)
	{
		activity.keyAssign = assign != 0;
		if (assign == 0)
		{
			activity.keyEvent = 0;
		}
	}

	public static int getKeyEvent()
	{
		int result = activity.keyEvent;
		activity.keyEvent = 0;
		// PORT: fold in the Windows keyboard. This is the game's own NDS pad register,
		// so keys drive every menu and field control the DS original supported; the
		// phone build only ever set the B bit here, from the hardware Back button.
		return result | FF3.DesktopInput.PadBits;
	}

	public static void webTo()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		WebBrowserTask val = new WebBrowserTask();
		val.Uri = new Uri("http://support.jp.square-enix.com/j/FFIII_wp", UriKind.Absolute);
		val.Show();
	}

	public static int[] loadTexture(byte[] data)
	{
		// PORT: went through android.graphics.BitmapFactory -> Bitmap -> ByteBuffer,
		// all of which only wrapped Texture2D.FromStream.
		return FF3.ImageDecoder.Decode(data);
	}

	// PORT: drawFont() rendered text into a bitmap via android.graphics.Canvas
	// and Paint. Nothing calls it - the game draws text with SpriteFont pages
	// through GlobalScope.Graphics.DrawString - and the Canvas/Paint shims were
	// stubs that would have thrown on the first call anyway.

	public static void playSound(int channel, string filename)
	{
		sound.playSound(channel, filename);
	}

	public static void stopSound(int channel)
	{
		sound.stopSound(channel);
	}

	public static void pauseSound(int channel, int pause)
	{
		sound.pauseSound(channel, pause);
	}

	public static void setSoundVolume(int channel, float volume)
	{
		sound.setSoundVolume(channel, volume);
	}

	public static int getSoundState(int channel)
	{
		return sound.getSoundState(channel);
	}

	public static void createEditText(string text)
	{
		// PORT: built an Android AlertDialog wrapping an EditText, which the shim
		// forwarded to GlobalScope.Dialog and on to the Guide keyboard. On Windows
		// you type into the game itself; FF3.TextEntry is the field.
		activity.editString = null;
		FF3.TextEntry entry = FF3.TextEntry.Instance;
		if (entry == null)
		{
			return;
		}
		entry.Show(activity.getString(R.@string.CHANGE_NAME), string.Empty, text, 6,
			delegate(string entered)
			{
				// null means cancelled, and the game reads that as "leave the name alone".
				activity.editString = entered;
			});
	}

	public static string getEditText()
	{
		string result = activity.editString;
		activity.editString = null;
		return result;
	}

	public static void confirmApp()
	{
		AlertDialog.Builder builder = new AlertDialog.Builder(activity);
		builder.setTitle(activity.getString(R.@string.app_name));
		builder.setMessage(activity.getString(R.@string.iap_confirm_purchase));
		builder.setPositiveButton(activity.getString(R.@string.iap_purchase), new ConfirmDialogPositiveButtonListener());
		builder.setNegativeButton(activity.getString(R.@string.iap_goto_title), new ConfirmDialogNegativeButtonListener());
		builder.create();
	}

	public static void purchaseApp()
	{
		GlobalScope.Dialog.showMarket();
	}

	public static void updateApp()
	{
		AlertDialog.Builder builder = new AlertDialog.Builder(activity);
		builder.setTitle(activity.getString(R.@string.UPDATE_APP_TITLE));
		builder.setMessage(activity.getString(R.@string.UPDATE_APP));
		builder.setPositiveButton(activity.getString(R.@string.YES), new UpdateDialogPositiveButtonListener());
		builder.setNegativeButton(activity.getString(R.@string.NO), new UpdateDialogNegativeButtonListener());
		builder.create();
	}
}
