using android.content;
using android.view;
using android.widget;

namespace android.app;

public class AlertDialog : Dialog
{
	public class Builder(Context context)
	{
		private string m_strTitle = "";

		private string m_strMessage = "";

		private EditText m_EditText;

		private OnClickListener m_NegativeButtonListener;

		private OnCancelListener m_CancelListener;

		private OnClickListener m_PositiveButtonListener;

		private string m_strNegativeButtonText = "";

		private string m_strPositiveButtonText = "";

		public AlertDialog create()
		{
			GlobalScope.Dialog.showAskDialog(m_strTitle, m_strMessage, m_strPositiveButtonText, m_strNegativeButtonText, m_PositiveButtonListener, m_NegativeButtonListener);
			return new AlertDialog();
		}

		public Builder setNegativeButton(string text, OnClickListener listener)
		{
			m_strNegativeButtonText = text;
			m_NegativeButtonListener = listener;
			return this;
		}

		public Builder setOnCancelListener(OnCancelListener onCancelListener)
		{
			m_CancelListener = onCancelListener;
			return this;
		}

		public Builder setPositiveButton(string text, OnClickListener listener)
		{
			m_strPositiveButtonText = text;
			m_PositiveButtonListener = listener;
			return this;
		}

		public Builder setTitle(string title)
		{
			m_strTitle = title;
			return this;
		}

		public Builder setMessage(string message)
		{
			m_strMessage = message;
			m_strMessage = m_strMessage.Replace("\\n", "\n");
			return this;
		}

		public Builder setView(View view)
		{
			m_EditText = view as EditText;
			return this;
		}

		public AlertDialog show()
		{
			GlobalScope.Dialog.showInputDialog(m_strTitle, m_strMessage, m_EditText, m_PositiveButtonListener, m_CancelListener);
			return new AlertDialog();
		}
	}
}
