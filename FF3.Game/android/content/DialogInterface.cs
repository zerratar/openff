namespace android.content;

public abstract class DialogInterface
{
	public interface OnCancelListener
	{
		void onCancel(DialogInterface dialog);
	}

	public interface OnClickListener
	{
		void onClick(DialogInterface dialog, int which);
	}
}
