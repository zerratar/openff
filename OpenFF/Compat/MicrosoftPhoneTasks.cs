// Windows Phone launcher compatibility layer.
//
// MainActivity.webTo() opens the Square Enix support page through the phone's
// WebBrowserTask. On desktop that is just "open this URL in the default browser".

using System;
using System.Diagnostics;

namespace Microsoft.Phone.Tasks
{
	public sealed class WebBrowserTask
	{
		public Uri Uri { get; set; }

		public string URL
		{
			get => Uri?.ToString();
			set => Uri = new Uri(value, UriKind.Absolute);
		}

		public void Show()
		{
			if (Uri == null)
			{
				return;
			}
			try
			{
				Process.Start(new ProcessStartInfo(Uri.ToString()) { UseShellExecute = true });
			}
			catch (Exception ex)
			{
				Debug.WriteLine("WebBrowserTask.Show failed: " + ex.Message);
			}
		}
	}
}
