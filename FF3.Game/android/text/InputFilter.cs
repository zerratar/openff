namespace android.text;

public abstract class InputFilter
{
	public class LengthFilter : InputFilter
	{
		private int m_iMax;

		public LengthFilter(int max)
		{
			m_iMax = max;
		}

		public override string filter(string source, int start, int end, string dest, int dstart, int dend)
		{
			string text = source.Substring(start, end - start);
			if (text.Length > m_iMax)
			{
				text = text.Substring(0, m_iMax);
			}
			return text;
		}
	}

	public abstract string filter(string source, int start, int end, string dest, int dstart, int dend);
}
