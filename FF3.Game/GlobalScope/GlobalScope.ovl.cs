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
    public static class ovl
    {
        public class COverlayRegister
        {
            private ds.COverlay _overlay = new ds.COverlay();

            private OVERLAYINDEX _index;

            public void initialize()
            {
                _overlay.RegisterOverlay(0, 0u);
                _overlay.RegisterOverlay(0, 1u);
                _overlay.RegisterOverlay(0, 2u);
                _overlay.RegisterOverlay(0, 3u);
                _overlay.RegisterOverlay(0, 4u);
                _overlay.RegisterOverlay(0, 5u);
                _overlay.RegisterOverlay(0, 6u);
                _overlay.Initialize();
                _overlay.ChangeOverlay(0u);
                _index = OVERLAYINDEX.PART_DEBUG;
            }

            public void ChangeOverlay(OVERLAYINDEX index)
            {
                _overlay.ChangeOverlay((uint)index);
                _index = index;
            }

            public OVERLAYINDEX GetOverlayIndex()
            {
                return _index;
            }
        }

        public enum OVERLAYINDEX
        {
            PART_DEBUG,
            PART_TITLE,
            PART_MOVIE,
            PART_BATTLE,
            PART_WORLD,
            PART_MOGNET,
            PART_SPECIAL,
            PART_MAX
        }

        public const OVERLAYINDEX PART_DEBUG = OVERLAYINDEX.PART_DEBUG;

        public const OVERLAYINDEX PART_TITLE = OVERLAYINDEX.PART_TITLE;

        public const OVERLAYINDEX PART_MOVIE = OVERLAYINDEX.PART_MOVIE;

        public const OVERLAYINDEX PART_BATTLE = OVERLAYINDEX.PART_BATTLE;

        public const OVERLAYINDEX PART_WORLD = OVERLAYINDEX.PART_WORLD;

        public const OVERLAYINDEX PART_MOGNET = OVERLAYINDEX.PART_MOGNET;

        public const OVERLAYINDEX PART_SPECIAL = OVERLAYINDEX.PART_SPECIAL;

        public const OVERLAYINDEX PART_MAX = OVERLAYINDEX.PART_MAX;

        public static COverlayRegister overlayRegister = new COverlayRegister();
    }
}
