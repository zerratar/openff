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
    public enum OSLanguage
    {
        OS_LANGUAGE_JAPANESE,
        OS_LANGUAGE_ENGLISH,
        OS_LANGUAGE_FRENCH,
        OS_LANGUAGE_GERMAN,
        OS_LANGUAGE_ITALIAN,
        OS_LANGUAGE_SPANISH,
        OS_LANGUAGE_CHINESE_CN,
        OS_LANGUAGE_CHINESE_TW,
        OS_LANGUAGE_CODE_MAX
    }
}
