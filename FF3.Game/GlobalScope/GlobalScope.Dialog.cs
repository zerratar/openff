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
using android.text;
using android.widget;
using syrcusW.res.raw;
using syrcusW.res.values;

internal static partial class GlobalScope
{
						public static class Dialog
						{
							private static EditText m_EditText;

							private static DialogInterface.OnClickListener m_PositiveListener;

							private static DialogInterface.OnClickListener m_NegativeListener;

							private static DialogInterface.OnCancelListener m_CancelListener;

							public static bool showInputDialog(string strTitle, string strDescription, EditText editText, DialogInterface.OnClickListener positiveListener, DialogInterface.OnCancelListener cancelListener)
							{
								if (Guide.IsVisible)
								{
									return false;
								}
								m_EditText = editText;
								m_PositiveListener = positiveListener;
								m_CancelListener = cancelListener;
								Guide.BeginShowKeyboardInput(PlayerIndex.One, strTitle, strDescription, m_EditText.getText(), (AsyncCallback)inputDialogCallback, (object)null);
								return true;
							}

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

							private static void inputDialogCallback(IAsyncResult asyncResult)
							{
								if (m_EditText == null)
								{
									return;
								}
								string text = Guide.EndShowKeyboardInput(asyncResult);
								if (text != null)
								{
									InputFilter[] filters = m_EditText.getFilters();
									InputFilter[] array = filters;
									foreach (InputFilter inputFilter in array)
									{
										text = inputFilter.filter(text, 0, text.Length, null, 0, 0);
									}
									m_EditText.setText(text, TextView.BufferType.NORMAL);
									if (m_PositiveListener != null)
									{
										m_PositiveListener.onClick(null, 0);
									}
								}
								else if (m_CancelListener != null)
								{
									m_CancelListener.onCancel(null);
								}
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
