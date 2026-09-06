namespace OpenFF.Platform;

public class KeyEvent
{
	public const int ACTION_DOWN = 0;

	public const int KEYCODE_BUTTON_A = 96;

	public const int KEYCODE_BUTTON_B = 97;

	public const int KEYCODE_BUTTON_SELECT = 109;

	public const int KEYCODE_BUTTON_START = 108;

	public const int KEYCODE_DPAD_RIGHT = 22;

	public const int KEYCODE_DPAD_LEFT = 21;

	public const int KEYCODE_DPAD_UP = 19;

	public const int KEYCODE_DPAD_DOWN = 20;

	public const int KEYCODE_BUTTON_R1 = 103;

	public const int KEYCODE_BUTTON_L1 = 102;

	public const int KEYCODE_BUTTON_X = 99;

	public const int KEYCODE_BUTTON_Y = 100;

	public const int KEYCODE_BUTTON_MODE = 110;

	public const int KEYCODE_BUTTON_R2 = 105;

	public const int KEYCODE_BUTTON_L2 = 104;

	public const int KEYCODE_BACK = 4;

	private int m_iAction;

	private int m_iCode;

	public KeyEvent(int action, int code)
	{
		m_iAction = action;
		m_iCode = code;
	}
}
