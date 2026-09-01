using android.content;

namespace android.widget;

public class EditText : TextView
{
	private string m_strText;

	public EditText(Context context)
	{
	}

	public string getText()
	{
		return m_strText;
	}

	public void setText(string text, BufferType type)
	{
		m_strText = text;
	}
}
