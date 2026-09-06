using OpenFF.Platform;

namespace OpenFF.Platform;

// PORT: was an Android Dialog. Only the yes/no prompt survives, and it goes
// through GlobalScope.Dialog to MonoGame's MessageBox.
public class AlertDialog
{
	public class Builder(object owner)
	{
		private string m_strTitle = "";

		private string m_strMessage = "";

		private DialogInterface.OnClickListener m_NegativeButtonListener;

		private DialogInterface.OnCancelListener m_CancelListener;

		private DialogInterface.OnClickListener m_PositiveButtonListener;

		private string m_strNegativeButtonText = "";

		private string m_strPositiveButtonText = "";

		public AlertDialog create()
		{
			GlobalScope.Dialog.showAskDialog(m_strTitle, m_strMessage, m_strPositiveButtonText, m_strNegativeButtonText, m_PositiveButtonListener, m_NegativeButtonListener);
			return new AlertDialog();
		}

		public Builder setNegativeButton(string text, DialogInterface.OnClickListener listener)
		{
			m_strNegativeButtonText = text;
			m_NegativeButtonListener = listener;
			return this;
		}

		public Builder setOnCancelListener(DialogInterface.OnCancelListener onCancelListener)
		{
			m_CancelListener = onCancelListener;
			return this;
		}

		public Builder setPositiveButton(string text, DialogInterface.OnClickListener listener)
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

	}
}
