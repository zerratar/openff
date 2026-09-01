using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using android.content;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
						// PORT: the text-entry half of this went through an Android EditText and the
// Guide soft keyboard. MainActivity.createEditText now drives FF3.TextEntry
// directly, so only the message-box path remains.
public static class Dialog
						{
							private static DialogInterface.OnClickListener m_PositiveListener;

							private static DialogInterface.OnClickListener m_NegativeListener;

							public static bool showAskDialog(string strTitle, string strDescription, string strPositiveText, string strNegativeText, DialogInterface.OnClickListener positiveListener, DialogInterface.OnClickListener negativeListener)
							{
								if (Guide.IsVisible)
								{
									return false;
								}
								m_PositiveListener = positiveListener;
								m_NegativeListener = negativeListener;
								Guide.BeginShowMessageBox(PlayerIndex.One, strTitle, strDescription, (IEnumerable<string>)new string[2] { strPositiveText, strNegativeText }, 1, (MessageBoxIcon)0, (AsyncCallback)askDialogCallback, (object)null);
								return true;
							}

							public static void showMarket()
							{
								try
								{
									Guide.ShowMarketplace(PlayerIndex.One);
								}
								catch (Exception)
								{
								}
								UserInfo.recheckTrial();
							}

							private static void askDialogCallback(IAsyncResult asyncResult)
							{
								int? num = Guide.EndShowMessageBox(asyncResult);
								if (num.GetValueOrDefault() == 0 && num.HasValue)
								{
									if (m_PositiveListener != null)
									{
										m_PositiveListener.onClick(null, 0);
									}
								}
								else if (m_NegativeListener != null)
								{
									m_NegativeListener.onClick(null, 1);
								}
							}
						}
}
