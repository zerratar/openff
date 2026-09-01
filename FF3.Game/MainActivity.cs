using System;
using System.Threading;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using android.app;
using android.content;
using android.graphics;
using android.opengl;
using android.os;
using android.text;
using android.view;
using android.view.inputmethod;
using android.widget;
using java.io;
using javax.microedition.khronos.egl;
using javax.microedition.khronos.opengles;

public class MainActivity : Activity, GLSurfaceView.Renderer
{
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

	private class EditTextPositiveButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
			activity.editString = activity.editText.getText().ToString();
			activity.editText = null;
		}
	}

	private class EditTextNegativeButtonListener : DialogInterface.OnClickListener
	{
		public void onClick(DialogInterface dialog, int whichButton)
		{
			activity.editText = null;
		}
	}

	private class EditTextCancelListener : DialogInterface.OnCancelListener
	{
		public void onCancel(DialogInterface dialog)
		{
			activity.editText = null;
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

	private GLSurfaceView mGLSurfaceView;

	private EditText editText;

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

	protected override void onCreate(Bundle savedInstanceState)
	{
		base.onCreate(savedInstanceState);
		setVolumeControlStream(3);
		InputStream inputStream = null;
		try
		{
			inputStream = getResources().openRawResource(2130968576);
			byte[] array = new byte[1];
			inputStream.read(array);
			language = array[0];
		}
		catch (Exception)
		{
		}
		try
		{
			inputStream.close();
		}
		catch (Exception)
		{
		}
		mGLSurfaceView = new GLSurfaceView(this);
		mGLSurfaceView.setRenderer(this);
		setContentView(mGLSurfaceView);
		// PORT: an entitlement check lived here and quit the game outright if the
		// Square Enix auth handshake had not succeeded. There is no such handshake
		// on Windows, so the check and the account layer behind it are gone.
		init();
	}

	protected override void onDestroy()
	{
		quit();
		sound.stopSoundAll();
		base.onDestroy();
	}

	protected override void onPause()
	{
		mGLSurfaceView.onPause();
		base.onPause();
		if (editText != null)
		{
			InputMethodManager inputMethodManager = (InputMethodManager)getSystemService("input_method");
			inputMethodManager.hideSoftInputFromWindow(editText.getWindowToken(), 0);
		}
		pause();
		sound.pauseSoundAll(pause: true);
	}

	protected override void onResume()
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
		base.onResume();
		mGLSurfaceView.onResume();
	}

	public override void finish()
	{
		showDialog(0);
	}

	public void appEnd()
	{
		end = true;
		base.finish();
		sound.stopSoundAll();
		JavaSystem.exit(0);
	}

	protected override Dialog onCreateDialog(int id)
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
		base.finish();
	}

	public void onSurfaceCreated(GL10 gl, EGLConfig config)
	{
		activity = this;
	}

	public void onSurfaceChanged(GL10 gl, int width, int height)
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
		gl.glViewport(viewX, viewY, viewW, viewH);
	}

	public override bool onTouchEvent(MotionEvent e)
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

	public override bool onKeyDown(int keyCode, KeyEvent @event)
	{
		if (keyCode == 4 && keyAssign)
		{
			keyEvent |= 2;
			return false;
		}
		return base.onKeyDown(keyCode, @event);
	}

	public void onDrawFrame(GL10 gl)
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
			KeyguardManager keyguardManager = (KeyguardManager)getSystemService("keyguard");
			if (keyguardManager.inKeyguardRestrictedInputMode())
			{
				return;
			}
			touchPeak = (touchCount = 0);
			suspend = false;
			sound.pauseSoundAll(pause: false);
			resume();
		}
		touch(touchCount, touchPeak, touchX[0], touchY[0], touchX[1], touchY[1]);
		touchPeak = touchCount;
		render();
		sound.updateSound();
		if (editText == null)
		{
			return;
		}
		try
		{
			Thread.Sleep(50);
		}
		catch (Exception)
		{
		}
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
		if (text.Equals(".msd") && filename[0] != 'e')
		{
			array2 = decodeString(array2);
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
		FF3.Log.First(FF3.LogChannel.Texture, "loadTexture", 200, () => $"data={(data == null ? -1 : data.Length)} bytes"); /*FF3LOG*/
		BitmapFactory.Options options = new BitmapFactory.Options();
		options.inScaled = false;
		Bitmap bitmap = BitmapFactory.decodeByteArray(data, 0, data.Length, options);
		int width = bitmap.getWidth();
		int height = bitmap.getHeight();
		int[] array = new int[width * height + 2];
		array[0] = width;
		array[1] = height;
		bitmap.getPixels(array, 2, width, 0, 0, width, height);
		bitmap.recycle();
		return array;
	}

	public static int[] drawFont(string text, int size, int fontSize, int y)
	{
		Paint paint = new Paint();
		paint.setTextSize(fontSize);
		Paint.FontMetrics fontMetrics = paint.getFontMetrics();
		Bitmap bitmap = Bitmap.createBitmap(size, size, Bitmap.Config.ARGB_8888);
		Canvas canvas = new Canvas(bitmap);
		paint.setAntiAlias(aa: true);
		canvas.drawText(text, 0f, (float)y - (fontMetrics.top + fontMetrics.bottom) / 2f, paint);
		int[] array = new int[size * size + 1];
		array[0] = (int)paint.measureText(text);
		bitmap.getPixels(array, 1, size, 0, 0, size, size);
		bitmap.recycle();
		return array;
	}

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
		activity.editString = null;
		AlertDialog.Builder builder = new AlertDialog.Builder(activity);
		EditText editText = new EditText(activity);
		editText.setText(text, TextView.BufferType.NORMAL);
		editText.setInputType(1);
		editText.setFilters(new InputFilter[1]
		{
			new InputFilter.LengthFilter(6)
		});
		editText.setWidth(100);
		activity.editText = editText;
		builder.setTitle(activity.getString(R.@string.CHANGE_NAME));
		builder.setView(editText);
		builder.setPositiveButton(activity.getString(R.@string.OK), new EditTextPositiveButtonListener());
		builder.setNegativeButton(activity.getString(R.@string.CANCEL), new EditTextNegativeButtonListener());
		builder.setOnCancelListener(new EditTextCancelListener());
		builder.show();
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
