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
	public static partial class mognet
	{
		public enum MailNPC
		{
			NPC_MAIL_TP,
			NPC_MAIL_TJ,
			NPC_MAIL_SA,
			NPC_MAIL_CD,
			NPC_MAIL_FJ,
			NPC_MAIL_AR,
			NPC_MAIL_MAX
		}
	}
}
