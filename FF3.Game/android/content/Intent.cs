using System;

namespace android.content;

public class Intent
{
	private Type m_Cls;

	public Intent(Context packageContext, Type cls)
	{
		m_Cls = cls;
	}

	public Intent putExtra(string name, string value)
	{
		return this;
	}

	public object __getClassInstance()
	{
		return m_Cls.GetConstructor(new Type[0]).Invoke(new object[0]);
	}
}
