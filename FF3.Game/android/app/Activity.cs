using android.content;
using android.content.res;
using android.os;
using android.view;

namespace android.app;

public class Activity : Context
{
	private class NullSharedPreferences : SharedPreferences
	{
		private class NullEditor : Editor
		{
			public void apply()
			{
			}

			public Editor clear()
			{
				return this;
			}

			public bool commit()
			{
				return true;
			}

			public Editor putBoolean(string key, bool value)
			{
				return this;
			}

			public Editor putFloat(string key, float value)
			{
				return this;
			}

			public Editor putInt(string key, int value)
			{
				return this;
			}

			public Editor putLong(string key, long value)
			{
				return this;
			}

			public Editor putString(string key, string value)
			{
				return this;
			}

			public Editor remove(string key)
			{
				return this;
			}
		}

		public override Editor edit()
		{
			return new NullEditor();
		}

		public override string getString(string key, string defValue)
		{
			return "";
		}
	}

	private static KeyguardManager m_KeyguardManager = new KeyguardManager();

	private static Resources m_Resources = new Resources();

	private View m_View;

	public virtual void finish()
	{
		onPause();
		onDestroy();
		Android.removeActivity(this);
	}

	// PORT: getApplication() returned the Square Enix account application object.
	// Its only caller was the entitlement check in MainActivity, which is gone.

	public Context getApplicationContext()
	{
		return this;
	}

	public SharedPreferences getPreferences(int mode)
	{
		return new NullSharedPreferences();
	}

	public string getString(string resId)
	{
		return resId;
	}

	public object getSystemService(string name)
	{
		return name switch
		{
			"keyguard" => m_KeyguardManager, 
			_ => null, 
		};
	}

	public virtual bool onKeyDown(int keyCode, KeyEvent @event)
	{
		return true;
	}

	public virtual bool onTouchEvent(MotionEvent @event)
	{
		return true;
	}

	public Resources getResources()
	{
		return m_Resources;
	}

	// PORT: openFileOutput() mapped Android's private per-app storage onto
	// IsolatedStorage. Saves now go through FF3.SaveFiles, which writes to
	// %APPDATA%\FF3 - somewhere a Windows player can find and back up.

	public void setContentView(View view)
	{
		m_View = view;
	}

	public void setVolumeControlStream(int streamType)
	{
	}

	public void showDialog(int id)
	{
		onCreateDialog(id);
	}

	public void startActivity(Intent intent)
	{
		Activity activity = (Activity)intent.__getClassInstance();
		Android.addActivity(activity);
		activity.onCreate(null);
		activity.onStart();
		activity.onResume();
	}

	public void __onCreate()
	{
		onCreate(null);
	}

	public void __onDestroy()
	{
		onDestroy();
	}

	public void __onStart()
	{
		onStart();
	}

	public void __onStop()
	{
		onStop();
	}

	public void __onResume()
	{
		onResume();
	}

	public void __onPause()
	{
		onPause();
	}

	public void __onUpdate()
	{
	}

	public void __onDraw()
	{
		m_View.__onDraw();
	}

	protected virtual void onCreate(Bundle savedInstanceState)
	{
	}

	protected virtual Dialog onCreateDialog(int id)
	{
		return onCreateDialog(id);
	}

	protected virtual void onDestroy()
	{
	}

	protected virtual void onPause()
	{
	}

	protected virtual void onResume()
	{
	}

	protected virtual void onStart()
	{
	}

	protected virtual void onStop()
	{
	}
}
