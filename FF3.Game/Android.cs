using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using android.app;
using android.view;

public static class Android
{
	private static List<Activity> m_ListActivity = new List<Activity>();

	public static void addActivity(Activity activity)
	{
		m_ListActivity.Add(activity);
	}

	public static void removeActivity(Activity activity)
	{
		m_ListActivity.Remove(activity);
	}

	public static void finish()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.finish();
		}
	}

	public static void onCreate()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onCreate();
		}
	}

	public static void onDestroy()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onDestroy();
		}
	}

	public static void onStart()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onStart();
		}
	}

	public static void onStop()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onStop();
		}
	}

	public static void onResume()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onResume();
		}
	}

	public static void onPause()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onPause();
		}
	}

	public static void onUpdate()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onUpdate();
		}
	}

	public static void onDraw()
	{
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			activity.__onDraw();
		}
	}

	public static bool onKeyDown(Keys key)
	{
		FF3.Log.Write(FF3.LogChannel.Input, $"onKeyDown {key}"); /*FF3LOG*/
		int num = convertKeyCode(key);
		if (num < 0)
		{
			return true;
		}
		Activity[] array = m_ListActivity.ToArray();
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onKeyDown(num, new KeyEvent(0, num)))
			{
				return false;
			}
		}
		if (num == 4)
		{
			finish();
			return false;
		}
		return true;
	}

	public static bool onTouchDown(int iX, int iY)
	{
		FF3.Log.Write(FF3.LogChannel.Input, $"onTouchDown {iX},{iY} activities={m_ListActivity.Count}"); /*FF3LOG*/
		Activity[] array = m_ListActivity.ToArray();
		MotionEvent motionEvent = new MotionEvent(0, 1, new float[1] { iX }, new float[1] { iY });
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onTouchEvent(motionEvent))
			{
				return false;
			}
		}
		return true;
	}

	public static bool onTouchUp(int iX, int iY)
	{
		FF3.Log.Write(FF3.LogChannel.Input, $"onTouchUp {iX},{iY}"); /*FF3LOG*/
		Activity[] array = m_ListActivity.ToArray();
		MotionEvent motionEvent = new MotionEvent(1, 1, new float[1] { iX }, new float[1] { iY });
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onTouchEvent(motionEvent))
			{
				return false;
			}
		}
		return true;
	}

	public static bool onTouchMove(int iX, int iY)
	{
		FF3.Log.Sample(FF3.LogChannel.Input, "onTouchMove", 20, () => $"{iX},{iY}"); /*FF3LOG*/
		Activity[] array = m_ListActivity.ToArray();
		MotionEvent motionEvent = new MotionEvent(2, 1, new float[1] { iX }, new float[1] { iY });
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onTouchEvent(motionEvent))
			{
				return false;
			}
		}
		return true;
	}

	public static bool onTouchDown(int iXA, int iYA, int iXB, int iYB)
	{
		Activity[] array = m_ListActivity.ToArray();
		MotionEvent motionEvent = new MotionEvent(0, 2, new float[2] { iXA, iXB }, new float[2] { iYA, iYB });
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onTouchEvent(motionEvent))
			{
				return false;
			}
		}
		return true;
	}

	public static bool onTouchUp(int iXA, int iYA, int iXB, int iYB)
	{
		Activity[] array = m_ListActivity.ToArray();
		MotionEvent motionEvent = new MotionEvent(1, 2, new float[2] { iXA, iXB }, new float[2] { iYA, iYB });
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onTouchEvent(motionEvent))
			{
				return false;
			}
		}
		return true;
	}

	public static bool onTouchMove(int iXA, int iYA, int iXB, int iYB)
	{
		Activity[] array = m_ListActivity.ToArray();
		MotionEvent motionEvent = new MotionEvent(2, 2, new float[2] { iXA, iXB }, new float[2] { iYA, iYB });
		Activity[] array2 = array;
		foreach (Activity activity in array2)
		{
			if (!activity.onTouchEvent(motionEvent))
			{
				return false;
			}
		}
		return true;
	}

	private static int convertKeyCode(Keys key)
	{
		return key switch
		{
			Keys.Z => 96, 
			Keys.X => 97, 
			Keys.N => 109, 
			Keys.M => 108, 
			Keys.Right => 22, 
			Keys.Left => 21, 
			Keys.Up => 19, 
			Keys.Down => 20, 
			Keys.S => 103, 
			Keys.A => 102, 
			Keys.V => 99, 
			Keys.C => 100, 
			Keys.B => 110, 
			Keys.F => 105, 
			Keys.D => 104, 
			Keys.Escape => 4, 
			_ => -1, 
		};
	}
}
