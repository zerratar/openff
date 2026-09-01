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
    public static class part
    {
        public class CPartRegister
        {
            private GAMEPART m_GamePart;

            public void initialize()
            {
                ds.g_TouchPanel.initialize();
                ds.CFile.initialize();
                ds.g_Pad.setDebug(flag: false);
                GAMEPART gAMEPART = GAMEPART.GAMEPART_CAMPANY_LOGO;
                // PORT: --start=<part> jumps past the logo and prologue, which otherwise cost
                // about a minute before any 3D reaches the screen.
                gAMEPART = FF3.RenderOverrides.StartPart(gAMEPART); /*FF3LOG*/
                sys.GGlobal.setPartAfterSoftReset(gAMEPART);
                m_GamePart = gAMEPART;
                ds.CDevice.singleton().setFPS(ds.CDevice.enFPS.enFPS_30);
                ds.CDevice.singleton().setDepthBufferMode(0);
                logo.CampanyLogoPart.registerPart();
                ttl.TitlePart.registerPart();
                ttl.LinkPart.registerPart();
                btl.BattlePart.registerPart();
                wld.WorldPart.registerPart();
                MogNetPart.registerPart();
                spl.SpecialPart.registerPart();
                load.LoadPart.registerPart();
                load.SuspendLoadPart.registerPart();
                movie.MoviePart.registerPart();
                ds.init_rand(OS_GetVBlankCount());
                btl.OutsideToBattle.getInstance().initialize();
                wld.CWorldOutSideData.getInstance().initialize();
                evt.CEventManager.getInstance().initialize();
                itm.ItemManager.instance().load();
            }

            public GAMEPART getGamePart()
            {
                return m_GamePart;
            }
        }

        public static CPartRegister partRegister = new CPartRegister();
    }
}
