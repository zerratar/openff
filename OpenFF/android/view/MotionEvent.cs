namespace android.view;

public sealed class MotionEvent
{
	public const int ACTION_DOWN = 0;

	public const int ACTION_MOVE = 2;

	public const int ACTION_POINTER_2_DOWN = 261;

	public const int ACTION_POINTER_2_UP = 262;

	public const int ACTION_UP = 1;

	private int m_iAction;

	private int m_iPointerCount;

	private float[] m_afX;

	private float[] m_afY;

	public MotionEvent()
	{
		m_iAction = 0;
		m_iPointerCount = 1;
		m_afX = new float[m_iPointerCount];
		m_afY = new float[m_iPointerCount];
		m_afX[0] = -1f;
		m_afY[0] = -1f;
	}

	public MotionEvent(int iAction, int iPointerCount, float[] afX, float[] afY)
	{
		FF3.Log.First(FF3.LogChannel.Input, "MotionEvent", 40, () => $"action={iAction} n={iPointerCount} x={(afX != null && afX.Length > 0 ? afX[0] : -1)} y={(afY != null && afY.Length > 0 ? afY[0] : -1)}"); /*FF3LOG*/
		m_iAction = iAction;
		m_iPointerCount = iPointerCount;
		m_afX = new float[m_iPointerCount];
		m_afY = new float[m_iPointerCount];
		for (int i = 0; i < m_iPointerCount; i++)
		{
			m_afX[i] = afX[i];
			m_afY[i] = afY[i];
		}
	}

	public int getAction()
	{
		return m_iAction;
	}

	public int getPointerCount()
	{
		return m_iPointerCount;
	}

	public float getX(int pointerIndex)
	{
		return m_afX[pointerIndex];
	}

	public float getX()
	{
		return getX(0);
	}

	public float getY(int pointerIndex)
	{
		return m_afY[pointerIndex];
	}

	public float getY()
	{
		return getY(0);
	}
}
