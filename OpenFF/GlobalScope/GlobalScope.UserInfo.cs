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
    public static class UserInfo
    {
        public class AchievementInfo
        {
            public bool m_bShow;

            public string m_strName;

            public string m_strDesc;

            public uint m_uiIconSize;

            public Array m_abyIconData;
        }

        public const int ACHIEVEMENT_ID_A = 0;

        public const int ACHIEVEMENT_ID_B = 1;

        public const int ACHIEVEMENT_ID_C = 2;

        public const int ACHIEVEMENT_ID_D = 3;

        public const int ACHIEVEMENT_ID_E = 4;

        public const int ACHIEVEMENT_ID_F = 5;

        public const int ACHIEVEMENT_ID_G = 6;

        public const int ACHIEVEMENT_ID_H = 7;

        public const int ACHIEVEMENT_ID_I = 8;

        public const int ACHIEVEMENT_ID_J = 9;

        public const int ACHIEVEMENT_ID_K = 10;

        public const int ACHIEVEMENT_ID_L = 11;

        public const int ACHIEVEMENT_ID_M = 12;

        public const int ACHIEVEMENT_ID_N = 13;

        public const int ACHIEVEMENT_ID_O = 14;

        public const int ACHIEVEMENT_ID_P = 15;

        public const int ACHIEVEMENT_ID_Q = 16;

        public const int ACHIEVEMENT_ID_R = 17;

        public const int ACHIEVEMENT_ID_MAX = 18;

        private static readonly string[] ACHIEVEMENT_KEY_TABLE = new string[18]
        {
                                "KILL_ZIN", "KILL_SALAMANDER", "KILL_KRAKEN", "KILL_TITAN", "KILL_CLOUD_OF_DARKNESS", "GET_ALL_WEAPON", "MONEY_50000", "MONEY_500000", "GET_BEAST", "GET_ALL_BEAST",
                                "TREASURE_50", "TREASURE_100", "MONSTER_50", "MONSTER_100", "GET_ULTIMA_WEAPON", "KILL_IRON_GIANT", "ONE_JOB", "ALL_JOB"
        };

        private static object m_LockObject = new object();

        private static Achievement[] m_aAchievement = (Achievement[])(object)new Achievement[18];

        private static bool[] m_abAwardFlag = new bool[18];

        private static AchievementInfo[] m_aAchievementInfo = new AchievementInfo[18];

        private static bool m_bTrial;

        public static int confirm_state;

        private static string[] ACHIEVEMENT_NAME_TABLE => new string[18]
        {
                                R.@string.ACHIEVEMENT_TITLE_A,
                                R.@string.ACHIEVEMENT_TITLE_B,
                                R.@string.ACHIEVEMENT_TITLE_C,
                                R.@string.ACHIEVEMENT_TITLE_D,
                                R.@string.ACHIEVEMENT_TITLE_E,
                                R.@string.ACHIEVEMENT_TITLE_F,
                                R.@string.ACHIEVEMENT_TITLE_G,
                                R.@string.ACHIEVEMENT_TITLE_H,
                                R.@string.ACHIEVEMENT_TITLE_I,
                                R.@string.ACHIEVEMENT_TITLE_J,
                                R.@string.ACHIEVEMENT_TITLE_K,
                                R.@string.ACHIEVEMENT_TITLE_L,
                                R.@string.ACHIEVEMENT_TITLE_M,
                                R.@string.ACHIEVEMENT_TITLE_N,
                                R.@string.ACHIEVEMENT_TITLE_O,
                                R.@string.ACHIEVEMENT_TITLE_P,
                                R.@string.ACHIEVEMENT_TITLE_Q,
                                R.@string.ACHIEVEMENT_TITLE_R
        };

        private static string[] ACHIEVEMENT_DESC_TABLE => new string[18]
        {
                                R.@string.ACHIEVEMENT_MESSAGE_A,
                                R.@string.ACHIEVEMENT_MESSAGE_B,
                                R.@string.ACHIEVEMENT_MESSAGE_C,
                                R.@string.ACHIEVEMENT_MESSAGE_D,
                                R.@string.ACHIEVEMENT_MESSAGE_E,
                                R.@string.ACHIEVEMENT_MESSAGE_F,
                                R.@string.ACHIEVEMENT_MESSAGE_G,
                                R.@string.ACHIEVEMENT_MESSAGE_H,
                                R.@string.ACHIEVEMENT_MESSAGE_I,
                                R.@string.ACHIEVEMENT_MESSAGE_J,
                                R.@string.ACHIEVEMENT_MESSAGE_K,
                                R.@string.ACHIEVEMENT_MESSAGE_L,
                                R.@string.ACHIEVEMENT_MESSAGE_M,
                                R.@string.ACHIEVEMENT_MESSAGE_N,
                                R.@string.ACHIEVEMENT_MESSAGE_O,
                                R.@string.ACHIEVEMENT_MESSAGE_P,
                                R.@string.ACHIEVEMENT_MESSAGE_Q,
                                R.@string.ACHIEVEMENT_MESSAGE_R
        };

        public static void init()
        {
            SignedInGamer.SignedIn += GamerSignedInCallback;
            checkTrial();
        }

        public static void checkTrial()
        {
            m_bTrial = Guide.IsTrialMode;
            confirm_state = ((!m_bTrial) ? 2 : 0);
        }

        public static void recheckTrial()
        {
            m_bTrial = Guide.IsTrialMode;
            confirm_state = (m_bTrial ? 3 : 2);
        }

        public static bool isTrial()
        {
            return m_bTrial;
        }

        public static void AwardAchievement(int iId)
        {
            if (m_abAwardFlag[iId])
            {
                return;
            }
            m_abAwardFlag[iId] = true;
            if (m_bTrial)
            {
                return;
            }
            try
            {
                SignedInGamer val = Gamer.SignedInGamers[PlayerIndex.One];
                if (val == null)
                {
                    return;
                }
                lock (m_LockObject)
                {
                    val.BeginAwardAchievement(ACHIEVEMENT_KEY_TABLE[iId], (AsyncCallback)AwardAchievementCallback, (object)val);
                }
            }
            catch (Exception)
            {
            }
        }

        public static AchievementInfo GetAchievementInfo(int iId)
        {
            if (m_aAchievementInfo[iId] == null)
            {
                m_aAchievementInfo[iId] = new AchievementInfo();
            }
            int num = iId;
            if (!m_abAwardFlag[iId])
            {
                num += 20;
            }
            string filename = "link_icon_" + num / 10 + num % 10 + ".NCGR";
            m_aAchievementInfo[iId].m_bShow = true;
            m_aAchievementInfo[iId].m_strName = ACHIEVEMENT_NAME_TABLE[iId];
            m_aAchievementInfo[iId].m_strDesc = ACHIEVEMENT_DESC_TABLE[iId];
            m_aAchievementInfo[iId].m_uiIconSize = ds.g_File.getSize(filename);
            m_aAchievementInfo[iId].m_abyIconData = ds.CHeap.alloc_app(m_aAchievementInfo[iId].m_uiIconSize);
            ds.g_File.load(m_aAchievementInfo[iId].m_abyIconData, filename);
            return m_aAchievementInfo[iId];
        }

        private static void GamerSignedInCallback(object sender, SignedInEventArgs args)
        {
            try
            {
                SignedInGamer gamer = args.Gamer;
                if (gamer != null)
                {
                    gamer.BeginGetAchievements((AsyncCallback)GetAchievementsCallback, (object)gamer);
                }
            }
            catch (Exception)
            {
            }
        }

        private static void GetAchievementsCallback(IAsyncResult result)
        {
            try
            {
                object asyncState = result.AsyncState;
                SignedInGamer val = (SignedInGamer)((asyncState is SignedInGamer) ? asyncState : null);
                if (val == null)
                {
                    return;
                }
                lock (m_LockObject)
                {
                    AchievementCollection val2 = val.EndGetAchievements(result);
                    for (int i = 0; i < 18; i++)
                    {
                        m_abAwardFlag[i] = false;
                        m_aAchievement[i] = null;
                        m_aAchievementInfo[i] = null;
                        foreach (Achievement item in val2)
                        {
                            if (item.Key == ACHIEVEMENT_KEY_TABLE[i])
                            {
                                m_aAchievement[i] = item;
                                m_abAwardFlag[i] = item.IsEarned;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void AwardAchievementCallback(IAsyncResult result)
        {
            try
            {
                object asyncState = result.AsyncState;
                SignedInGamer val = (SignedInGamer)((asyncState is SignedInGamer) ? asyncState : null);
                if (val != null)
                {
                    val.EndAwardAchievement(result);
                    val.BeginGetAchievements((AsyncCallback)GetAchievementsCallback, (object)val);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
