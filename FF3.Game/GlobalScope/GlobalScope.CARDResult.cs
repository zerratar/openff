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
	public enum CARDResult
	{
		CARD_RESULT_SUCCESS,
		CARD_RESULT_FAILURE,
		CARD_RESULT_INVALID_PARAM,
		CARD_RESULT_UNSUPPORTED,
		CARD_RESULT_TIMEOUT,
		CARD_RESULT_ERROR,
		CARD_RESULT_NO_RESPONSE,
		CARD_RESULT_CANCELED
	}
}
