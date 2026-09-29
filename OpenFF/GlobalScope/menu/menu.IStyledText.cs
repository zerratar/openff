internal static partial class GlobalScope
{
	public static partial class menu
	{
		/// <summary>
		/// PORT: a behaviour drawing a text of its own that is not an MBText (a config screen's choice, its explanation): the
		/// message a layout's styles put their font, colour and visibility on, and its place worked out again once they changed
		/// its size (OpenFF.Client.ModMenus).
		/// </summary>
		public interface IStyledText
		{
			dgs.DGSMessage StyledMessage { get; }
			void StyledPlace(Medget M);
		}
	}
}
