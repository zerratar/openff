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
		onDraw();
	}

	// PORT: took an android.graphics.Canvas that was only ever passed as null -
	// the surface view forwards straight to the renderer.
	protected virtual void onDraw()
	{
	}
}
