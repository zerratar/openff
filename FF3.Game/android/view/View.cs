using android.graphics;
using android.os;

namespace android.view;

public class View
{
	public IBinder getWindowToken()
	{
		return null;
	}

	public void __onDraw()
	{
		onDraw(null);
	}

	protected virtual void onDraw(Canvas canvas)
	{
	}
}
