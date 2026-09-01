using android.text;
using android.view;

namespace android.widget;

public class TextView : View
{
	public enum BufferType
	{
		EDITABLE,
		NORMAL,
		SPANNABLE
	}

	private InputFilter[] m_Filters;

	public InputFilter[] getFilters()
	{
		return m_Filters;
	}

	public void setFilters(InputFilter[] filters)
	{
		m_Filters = filters;
	}

	public void setInputType(int type)
	{
	}

	public void setWidth(int pixels)
	{
	}
}
