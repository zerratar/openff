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
using OpenFF.Platform;
using OpenFF.Resources;

internal static partial class GlobalScope
{
    public class Graphics
    {
        private Game game;

        private GraphicsDeviceManager gdm;

        private SpriteBatch spBatch;

        private BasicEffect effect;

        private Color color;

        private SpriteEffects flip;

        private float rotation;

        private Vector2 scale;

        private Vector2 origin;

        private Vector2 pos;

        private float depth;

        private float[][] fontShiftY;

        private float[] fontShiftYFlipAdjust;

        private byte[][] aaGlyph;

        private SpriteFont[][] aaSpriteFont;

        private string[] astrWord;

        private int fontCount;

        private int wordCount;

        private AlphaTestEffect m_AlphaTestEffect;

        private bool m_bPause;

        public Graphics(Game game)
        {
            this.game = game;
            gdm = new GraphicsDeviceManager(game);
            gdm.PreferredBackBufferWidth = 800;
            gdm.PreferredBackBufferHeight = 480;
            gdm.PreferMultiSampling = true;
            gdm.PreparingDeviceSettings += PreparingDeviceSettingsCallback;
            gdm.IsFullScreen = true;
            setPause(bPause: false);
        }

        public void LoadContent()
        {
            spBatch = new SpriteBatch(gdm.GraphicsDevice);
            // PORT: TrueType text when a face can be found; the atlases otherwise.
            OpenFF.Client.TrueTypeText.Initialise(gdm.GraphicsDevice);
            effect = new BasicEffect(gdm.GraphicsDevice);
            effect.VertexColorEnabled = true;
            color = Color.White;
            flip = SpriteEffects.None;
            rotation = 0f;
            scale = Vector2.One;
            origin = Vector2.Zero;
            pos = Vector2.Zero;
            depth = 0f;
            fontShiftY = new float[32][];
            fontShiftYFlipAdjust = new float[32];
            m_AlphaTestEffect = new AlphaTestEffect(gdm.GraphicsDevice);
            m_AlphaTestEffect.VertexColorEnabled = true;
            aaGlyph = new byte[32][];
            aaSpriteFont = new SpriteFont[32][];
            astrWord = new string[65536];
            fontCount = 0;
            wordCount = 0;
            // Glyph tables: character -> (atlas page, vertical shift class).
            // PORT: the atlas glyph tables ship with our Content only. Text is TrueType
            // from a Steam install, so a boot without them is fine - the atlas path just
            // has nothing to draw with.
            try
            {
                aaGlyph[12] = OpenFF.Client.GameFiles.ReadAllBytes("Content/Font12.glp");
                aaGlyph[16] = OpenFF.Client.GameFiles.ReadAllBytes("Content/Font16.glp");
            }
            catch (Exception)
            {
                aaGlyph[12] = null;
                aaGlyph[16] = null;
            }
            aaSpriteFont[12] = new SpriteFont[256];
            aaSpriteFont[16] = new SpriteFont[256];
            fontShiftY[12] = new float[6];
            fontShiftY[16] = new float[6];
            fontShiftY[12][0] = 2f;
            fontShiftY[12][1] = 2f;
            fontShiftY[12][2] = 2f;
            fontShiftY[12][3] = 2f;
            fontShiftY[12][4] = 3f;
            fontShiftY[12][5] = 2f;
            fontShiftY[16][0] = 3f;
            fontShiftY[16][1] = 3f;
            fontShiftY[16][2] = 3f;
            fontShiftY[16][3] = 3f;
            fontShiftY[16][4] = 4.5f;
            fontShiftY[16][5] = 3f;
            fontShiftYFlipAdjust[12] = 1f;
            fontShiftYFlipAdjust[16] = 1f;
        }

        public bool isFlipScreen()
        {
            return game.Window.CurrentOrientation != DisplayOrientation.LandscapeLeft;
        }

        public void setPause(bool bPause)
        {
            m_bPause = bPause;
        }

        public bool isPause()
        {
            return m_bPause;
        }

        private void PreparingDeviceSettingsCallback(object sender, PreparingDeviceSettingsEventArgs e)
        {
            _ = e.GraphicsDeviceInformation.PresentationParameters;
        }

        public Game getGame()
        {
            return game;
        }

        public GraphicsDeviceManager GetGraphicsDeviceManager()
        {
            return gdm;
        }

        public BasicEffect getBasicEffect()
        {
            return effect;
        }

        public AlphaTestEffect getAlphaTestEffect()
        {
            return m_AlphaTestEffect;
        }

        public void SetColor(int red, int green, int blue)
        {
            color.R = (byte)red;
            color.G = (byte)green;
            color.B = (byte)blue;
            color.A = byte.MaxValue;
        }

        public void SetColor(int red, int green, int blue, int alpha)
        {
            color.R = (byte)red;
            color.G = (byte)green;
            color.B = (byte)blue;
            color.A = (byte)alpha;
        }

        public float StringWidth(string text, int iSize)
        {
            if (OpenFF.Client.TrueTypeText.Enabled)
            {
                return OpenFF.Client.TrueTypeText.Width(text, iSize) * scale.X;
            }
            if (aaGlyph[iSize] == null)
            {
                return 0f;
            }
            int length = text.Length;
            float num = 0f;
            for (int i = 0; i < length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '\u007f':
                        c = ' ';
                        break;
                    case '\u00a0':
                        c = ' ';
                        break;
                    case '\u00ad':
                        c = ' ';
                        break;
                }
                byte b = aaGlyph[iSize][c * 2];
                if (b == byte.MaxValue)
                {
                    c = '?';
                    b = aaGlyph[iSize][c * 2];
                }
                if (astrWord[(uint)c] == null)
                {
                    astrWord[(uint)c] = c.ToString();
                    wordCount++;
                }
                if (aaSpriteFont[iSize][b] == null)
                {
                    aaSpriteFont[iSize][b] = loadAsset<SpriteFont>("Font" + iSize + "_" + b);
                    OpenFF.Client.FontDump.Dump("Font" + iSize + "_" + b, aaSpriteFont[iSize][b]); /*FF3LOG*/
                    fontCount++;
                }
                num += aaSpriteFont[iSize][b].MeasureString(astrWord[(uint)c]).X * scale.X;
            }
            return num;
        }

        public float StringHeight(string text, int iSize)
        {
            return iSize;
        }

        public void SetFlip(SpriteEffects flip)
        {
            this.flip = flip;
        }

        public void SetImageRotation(float rotation)
        {
            this.rotation = rotation;
        }

        public void SetImageScale(float x, float y)
        {
            scale.X = x;
            scale.Y = y;
        }

        public void SetImageOrigin(float x, float y)
        {
            origin.X = x;
            origin.Y = y;
        }

        public void clear()
        {
            gdm.GraphicsDevice.Clear(Color.Black);
        }

        /// <summary>
        /// The 800 by 480 the text is positioned in. drawString works every coordinate
        /// out as x * 800 / LCD_WIDTH, so that is the space it hands us.
        /// </summary>
        private const float TextSpaceWidth = 800f;
        private const float TextSpaceHeight = 480f;

        public void DrawStringStart()
        {
            // Text is the only thing that goes through SpriteBatch. Everything else -
            // sprites, portraits, the cursor - is drawn by NativeRenderer with a
            // projection matrix, so it fills whatever viewport the window has and scales
            // with it for free. SpriteBatch has no such matrix by default: its
            // coordinates are viewport pixels, so text laid out for 800 by 480 stayed
            // in the top left corner of a fullscreen window while everything around it
            // grew.
            //
            // Taken from the viewport rather than the back buffer, because the viewport
            // is what the 3D path is drawing into as well - so the two cannot disagree,
            // and if something has narrowed it this follows.
            Viewport view = gdm.GraphicsDevice.Viewport;
            OpenFF.Client.TrueTypeText.Initialise(gdm.GraphicsDevice);
            OpenFF.Client.TrueTypeText.SetViewportScale(view.Height / TextSpaceHeight);
            Matrix fit = Matrix.CreateScale(
                view.Width / TextSpaceWidth,
                view.Height / TextSpaceHeight,
                1f);

            spBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, fit);
            depth = 0f;
        }

        public void DrawStringEnd()
        {
            spBatch.End();
            depth = 0f;
        }

        public void DrawString(string text, float x, float y, int iSize)
        {
            if (text == null)
            {
                return;
            }
            if (OpenFF.Client.TrueTypeText.Enabled)
            {
                OpenFF.Client.TrueTypeText.Draw(spBatch, text, x, y, color, rotation, origin, scale, flip, depth, iSize);
                depth += 0.001f;
                return;
            }
            if (aaGlyph[iSize] == null)
            {
                return;
            }
            int length = text.Length;
            pos.X = x;
            for (int i = 0; i < length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '\u007f':
                        c = ' ';
                        break;
                    case '\u00a0':
                        c = ' ';
                        break;
                    case '\u00ad':
                        c = ' ';
                        break;
                }
                byte b = aaGlyph[iSize][c * 2];
                if (b == byte.MaxValue)
                {
                    c = '?';
                    b = aaGlyph[iSize][c * 2];
                }
                if (astrWord[(uint)c] == null)
                {
                    astrWord[(uint)c] = c.ToString();
                    wordCount++;
                }
                if (aaSpriteFont[iSize][b] == null)
                {
                    aaSpriteFont[iSize][b] = loadAsset<SpriteFont>("Font" + iSize + "_" + b);
                    OpenFF.Client.FontDump.Dump("Font" + iSize + "_" + b, aaSpriteFont[iSize][b]); /*FF3LOG*/
                    fontCount++;
                }
                if (GX_GetFlipScreen() == 1)
                {
                    pos.Y = y + fontShiftY[iSize][aaGlyph[iSize][c * 2 + 1]] + fontShiftYFlipAdjust[iSize];
                }
                else
                {
                    pos.Y = y - fontShiftY[iSize][aaGlyph[iSize][c * 2 + 1]];
                }
                spBatch.DrawString(aaSpriteFont[iSize][b], astrWord[(uint)c], pos, color, rotation, origin, scale, flip, depth);
                if (GX_GetFlipScreen() == 1)
                {
                    pos.X -= aaSpriteFont[iSize][b].MeasureString(astrWord[(uint)c]).X * scale.X;
                }
                else
                {
                    pos.X += aaSpriteFont[iSize][b].MeasureString(astrWord[(uint)c]).X * scale.X;
                }
            }
            depth += 0.001f;
        }

        public T loadAsset<T>(string strAssetName)
        {
            return game.Content.Load<T>(strAssetName);
        }

        public ContentManager CreateContentManager()
        {
            return new ContentManager(game.Services, game.Content.RootDirectory);
        }
    }
}
