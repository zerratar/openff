// A diagnostic layer drawn over the finished frame.
//
// F1 toggles it. While it is up, more keys switch its parts on and off, and the layer's
// last lines list those keys with each part's state, so nobody has to remember them:
//
//   F2  boxes    the menu's frames (Medgets) as rectangles, the focused one in yellow,
//                with a cross where the hand cursor is told to go
//   F3  labels   the frames' ids and sizes written on the boxes
//   F4  sprites  every 2D sprite the DS2D manager is showing, as the rectangle its
//                OAMs are drawn at (green on the sub plane, magenta on the 3D plane)
//   F5  world    which part of the game is running, the loaded stage and its type,
//                the hero's position and chip spot, whether the field is mirrored
//   F6  stats    frame time, draw calls and vertices this frame, memory, viewport
//
// It is a DrawableGameComponent drawn after the game and before the screenshot
// component, so screenshots taken with --screenshot-every include it: a headless run
// can be read from the pictures alone. --debug=all (or a list such as
// --debug=boxes,labels) starts with it on.
//
// Everything here reads the game's state and draws on top; nothing here changes what
// the game does. Rectangles go through a SpriteBatch of the component's own in
// viewport pixels; text goes through the game's own font, in its 800x480 text space.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FF3
{
    internal sealed class DebugOverlay : DrawableGameComponent
    {
        [Flags]
        private enum Layer
        {
            None = 0,
            Boxes = 1,
            Labels = 2,
            Sprites = 4,
            World = 8,
            Stats = 16,
            All = Boxes | Labels | Sprites | World | Stats,
        }

        private const float TextSpaceWidth = 800f;
        private const float TextSpaceHeight = 480f;
        private const int FontSize = 12;

        /// <summary>Draw calls and vertices the native renderer has issued so far this frame; the overlay resets them when it draws.</summary>
        public static int DrawCalls;
        public static int Vertices;

        public static bool Active { get; private set; }

        private Layer _layers = Layer.Boxes | Layer.World | Layer.Stats;
        private KeyboardState _previous;
        private SpriteBatch _batch;
        private Texture2D _pixel;
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private double _lastFrameAt;
        private double _frameMs;
        private double _fpsAccumulated;
        private int _fpsFrames;
        private double _fps;
        private int _lastDrawCalls;
        private int _lastVertices;
        private readonly StringBuilder _text = new StringBuilder();
        private readonly List<(string text, float x, float y, Color colour)> _labels = new List<(string, float, float, Color)>();

        private DebugOverlay(Game game)
            : base(game)
        {
            // After the game, before the screenshot capture (int.MaxValue) and the
            // text-entry panel (int.MaxValue - 1).
            DrawOrder = int.MaxValue - 3;
            UpdateOrder = int.MaxValue - 3;
        }

        /// <summary>Adds the component to the game. Always available via F1; --debug=all|list starts with it on.</summary>
        public static void Attach(Game game)
        {
            DebugOverlay overlay = new DebugOverlay(game);
            string start = Options.Get("debug");
            if (!string.IsNullOrEmpty(start))
            {
                Active = true;
                if (!string.Equals(start, "all", StringComparison.OrdinalIgnoreCase) && !string.Equals(start, "on", StringComparison.OrdinalIgnoreCase) && !string.Equals(start, "1", StringComparison.OrdinalIgnoreCase))
                {
                    Layer layers = Layer.None;
                    foreach (string part in start.Split(',', ' ', ';'))
                    {
                        if (Enum.TryParse(part.Trim(), true, out Layer one))
                        {
                            layers |= one;
                        }
                    }
                    overlay._layers = layers == Layer.None ? overlay._layers : layers;
                }
                else
                {
                    overlay._layers = Layer.All;
                }
            }
            game.Components.Add(overlay);
            Log.Write(LogChannel.General, "debug overlay: F1 toggles; F2 boxes, F3 labels, F4 sprites, F5 world, F6 stats" + (Active ? (" (on: " + overlay._layers + ")") : ""));
        }

        public override void Update(GameTime gameTime)
        {
            if (!Game.IsActive)
            {
                return;
            }
            KeyboardState now = Keyboard.GetState();
            if (Pressed(now, Keys.F1))
            {
                Active = !Active;
                Log.Write(LogChannel.General, "debug overlay: " + (Active ? ("on, " + _layers) : "off"));
            }
            if (Active)
            {
                Toggle(now, Keys.F2, Layer.Boxes);
                Toggle(now, Keys.F3, Layer.Labels);
                Toggle(now, Keys.F4, Layer.Sprites);
                Toggle(now, Keys.F5, Layer.World);
                Toggle(now, Keys.F6, Layer.Stats);
            }
            _previous = now;
        }

        private bool Pressed(KeyboardState now, Keys key) => now.IsKeyDown(key) && !_previous.IsKeyDown(key);

        private void Toggle(KeyboardState now, Keys key, Layer layer)
        {
            if (Pressed(now, key))
            {
                _layers ^= layer;
                Log.Write(LogChannel.General, "debug overlay: " + layer + " " + ((_layers & layer) != 0 ? "on" : "off"));
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // Per-frame counters, whether or not anything is shown.
            double now = _clock.Elapsed.TotalMilliseconds;
            _frameMs = now - _lastFrameAt;
            _lastFrameAt = now;
            _fpsAccumulated += _frameMs;
            _fpsFrames++;
            if (_fpsAccumulated >= 500)
            {
                _fps = _fpsFrames * 1000.0 / _fpsAccumulated;
                _fpsAccumulated = 0;
                _fpsFrames = 0;
            }
            _lastDrawCalls = DrawCalls;
            _lastVertices = Vertices;
            DrawCalls = 0;
            Vertices = 0;

            if (!Active || RenderTest.Active)
            {
                return;
            }
            try
            {
                DrawLayers();
            }
            catch (Exception ex)
            {
                Log.First(LogChannel.General, "debug-overlay-error", 3, () => "debug overlay: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private void DrawLayers()
        {
            EnsureResources();
            Viewport view = GraphicsDevice.Viewport;
            _labels.Clear();

            _batch.Begin();
            if ((_layers & Layer.Boxes) != 0)
            {
                DrawMenuBoxes(view);
            }
            if ((_layers & Layer.Sprites) != 0)
            {
                DrawSpriteBoxes(view);
            }
            _text.Clear();
            if ((_layers & Layer.World) != 0)
            {
                AppendWorld();
            }
            if ((_layers & Layer.Stats) != 0)
            {
                AppendStats(view);
            }
            AppendLegend();
            int lines = 0;
            for (int i = 0; i < _text.Length; i++)
            {
                if (_text[i] == '\n')
                {
                    lines++;
                }
            }
            if (lines > 0)
            {
                // A dark backing, as wide as the longest line, so the text reads over anything.
                float lineHeight = FontSize * TrueTypeText.SizeFactor;//1.25f;
                float widest = 120;
                foreach (string line in _text.ToString().Split('\n'))
                {
                    widest = Math.Max(widest, TrueTypeText.Width(line, FontSize) + 12);
                }
                Rectangle back = TextToPixels(view, 4, 4, widest, lines * lineHeight + 6);
                _batch.Draw(_pixel, back, new Color(0, 0, 0, 150));
            }
            _batch.End();

            GlobalScope.Graphics graphics = GlobalScope.m_Graphics;
            if (graphics == null)
            {
                return;
            }
            graphics.SetImageOrigin(0f, 0f);
            graphics.SetImageRotation(0f);
            graphics.SetImageScale(1f, 1f);
            graphics.DrawStringStart();
            graphics.SetColor(255, 255, 255, 255);
            if (lines > 0)
            {
                float y = 6;
                foreach (string line in _text.ToString().Split('\n'))
                {
                    if (line.Length > 0)
                    {
                        graphics.DrawString(line, 8, y, FontSize);
                    }
                    y += FontSize * TrueTypeText.SizeFactor;// * 1.25f;
                }
            }
            if ((_layers & Layer.Labels) != 0)
            {
                foreach ((string text, float x, float y, Color colour) label in _labels)
                {
                    graphics.SetColor(label.colour.R, label.colour.G, label.colour.B, 255);
                    graphics.DrawString(label.text, label.x, label.y, FontSize);
                }
            }
            graphics.DrawStringEnd();
        }

        // ---- the menu's frames ----

        private void DrawMenuBoxes(Viewport view)
        {
            GlobalScope.menu.MenuManager manager = GlobalScope.menu.MenuManager.getSingleton();
            GlobalScope.menu.Medget root = manager?.root();
            if (root == null)
            {
                return;
            }
            GlobalScope.menu.Medget focused = manager.getFocuseMedget();
            Walk(root, view, focused, 0);
            if (focused != null)
            {
                // Where the hand is told to go.
                Rectangle cross = LcdToPixels(view, focused.cursorX() - 3, focused.cursorY() - 3, 6, 6);
                _batch.Draw(_pixel, new Rectangle(cross.X, cross.Y + cross.Height / 2, cross.Width, 1), Color.Yellow);
                _batch.Draw(_pixel, new Rectangle(cross.X + cross.Width / 2, cross.Y, 1, cross.Height), Color.Yellow);
            }
        }

        private void Walk(GlobalScope.menu.Medget medget, Viewport view, GlobalScope.menu.Medget focused, int depth)
        {
            int guard = 0;
            for (GlobalScope.menu.Medget m = medget; m != null && guard < 4096; m = m.nextSibling(), guard++)
            {
                if (m.width() > 0 && m.height() > 0)
                {
                    bool isFocused = m == focused;
                    Color colour = isFocused ? Color.Yellow : (m.behavior() != null ? new Color(80, 200, 255) : new Color(120, 120, 200));
                    Rectangle r = LcdToPixels(view, m.x(), m.y(), m.width(), m.height());
                    Outline(r, colour, isFocused ? 2 : 1);
                    if ((_layers & Layer.Labels) != 0 && (isFocused || depth <= 3))
                    {
                        string id = m._id();
                        if (!string.IsNullOrEmpty(id))
                        {
                            PixelsToText(view, r.X + 2, r.Y + 1, out float tx, out float ty);
                            _labels.Add((id + " " + m.x() + "," + m.y() + " " + m.width() + "x" + m.height(), tx, ty, colour));
                        }
                    }
                }
                if (m.childNode() != null)
                {
                    Walk(m.childNode(), view, focused, depth + 1);
                }
            }
        }

        // ---- the 2D sprites ----

        private void DrawSpriteBoxes(Viewport view)
        {
            GlobalScope.sys2d.DS2DManager d2d = GlobalScope.sys2d.DS2DManager.d2dGetInstance();
            if (d2d == null)
            {
                return;
            }
            int count = 0;
            foreach (GlobalScope.sys2d.Sprite sprite in d2d.d2dSprites())
            {
                if (sprite == null || !sprite.IsShow())
                {
                    continue;
                }
                GlobalScope.NNSG2dCellData cell = sprite.GetCellData();
                if (cell?.pOamAttrArray == null)
                {
                    continue;
                }
                GlobalScope.NNSG2dSVec2 pos = sprite.GetPositionI();
                bool sub = sprite.GetPlane() == GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_SUB2D;
                Color colour = sub ? new Color(80, 255, 120) : new Color(255, 100, 255);
                int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
                foreach (GlobalScope.NNSG2dCellOAMAttrData oam in cell.pOamAttrArray)
                {
                    SteamCells.DrawnRect(oam, out int x, out int y, out int w, out int h);
                    int ox = pos.x + x - GlobalScope.screenOffset[0];
                    int oy = pos.y + y - GlobalScope.screenOffset[1];
                    Outline(LcdToPixels(view, ox, oy, w, h), colour, 1);
                    left = Math.Min(left, ox); top = Math.Min(top, oy);
                    right = Math.Max(right, ox + w); bottom = Math.Max(bottom, oy + h);
                }
                if ((_layers & Layer.Labels) != 0 && right > left && count < 64)
                {
                    Rectangle r = LcdToPixels(view, left, top, right - left, bottom - top);
                    PixelsToText(view, r.X + 2, r.Bottom + 1, out float tx, out float ty);
                    _labels.Add(("sprite " + pos.x + "," + pos.y + " " + (right - left) + "x" + (bottom - top) + " pri " + sprite.GetPriority(), tx, ty, colour));
                }
                count++;
            }
        }

        // ---- text ----

        private void AppendWorld()
        {
            string part = "?";
            try
            {
                part = ((GlobalScope.GAMEPART)GlobalScope.sys.FF3PartSys.getCurrentPart()).ToString().Replace("GAMEPART_", "");
            }
            catch (Exception) { }
            _text.Append("part ").Append(part).Append("   ").Append(Launch.Game).Append('/').Append(Launch.Source).Append('\n');

            GlobalScope.stg.CStageMng stage = GlobalScope.stageMng;
            string stageName = GlobalScope.stg.CStageMng.CurrentName ?? "-";
            string type = "-";
            bool mirrored = false;
            try
            {
                if (stage != null)
                {
                    GlobalScope.stg.STAGE_TYPE t = stage.getStageType();
                    type = t.ToString().Replace("STAGE_TYPE_", "");
                    mirrored = FieldMirror.Active(t);
                }
            }
            catch (Exception) { }

            if (string.IsNullOrEmpty(type) || type.Contains("error", StringComparison.OrdinalIgnoreCase))
                type = "Intro";

            _text.Append("stage ").Append(stageName).Append(" (").Append(type).Append(')').Append(mirrored ? "  mirrored z" : "").Append('\n');

            // The hero: the character the camera looks at, the way CStateFieldStart finds it.
            // The stage's foot position is only kept up to date on chip fields.
            GlobalScope.VecFx32 hero = null;
            try
            {
                hero = GlobalScope.wld.CBaseSystem.Current?.PlayerMng()?.Player(GlobalScope.chr.CBaseCharacter.getLookIndex())?.getPosition();
            }
            catch (Exception) { }
            if (hero != null)
            {
                _text.Append("hero ").Append((hero.x / 4096.0).ToString("0.0")).Append(", ").Append((hero.y / 4096.0).ToString("0.0")).Append(", ").Append((hero.z / 4096.0).ToString("0.0"));
                if (stage != null && type.StartsWith("FIELD"))
                {
                    try
                    {
                        GlobalScope.stg.CStageProfile profile = stage.StageProfile();
                        if (profile != null)
                        {
                            profile.getSpot(hero, out sbyte sx, out sbyte sz);
                            _text.Append("   spot ").Append(sx).Append(',').Append(sz);
                        }
                    }
                    catch (Exception)
                    {
                        _text.Append("   spot -");
                    }
                }
                _text.Append('\n');
            }

            GlobalScope.menu.Medget focused = GlobalScope.menu.MenuManager.getSingleton()?.getFocuseMedget();
            if (focused != null)
            {
                _text.Append("focus ").Append(focused._id()).Append(" at ").Append(focused.x()).Append(',').Append(focused.y()).Append(' ').Append(focused.width()).Append('x').Append(focused.height())
                    .Append("  cursor ").Append(focused.cursorX()).Append(',').Append(focused.cursorY()).Append('\n');
            }
        }

        /// <summary>The keys and what each currently does, always shown while the overlay is up.</summary>
        private void AppendLegend()
        {
            _text.Append("F2 boxes ").Append(State(Layer.Boxes))
                .Append("   F3 labels ").Append(State(Layer.Labels))
                .Append("   F4 sprites ").Append(State(Layer.Sprites))
                .Append("   F5 world ").Append(State(Layer.World))
                .Append("   F6 stats ").Append(State(Layer.Stats)).Append('\n');
        }

        private string State(Layer layer) => (_layers & layer) != 0 ? "[on]" : "[off]";

        private void AppendStats(Viewport view)
        {
            _text.Append(_fps.ToString("0")).Append(" fps  ").Append(_frameMs.ToString("0.0")).Append(" ms   draws ").Append(_lastDrawCalls)
                .Append("  verts ").Append(_lastVertices).Append("  polys ").Append(GlobalScope.polyCount).Append('\n');
            _text.Append("view ").Append(view.Width).Append('x').Append(view.Height).Append("  lcd ").Append(GlobalScope.LCD_WIDTH).Append('x').Append(GlobalScope.LCD_HEIGHT)
                .Append("  mem ").Append((GC.GetTotalMemory(false) / (1024 * 1024)).ToString()).Append(" MB\n");
        }

        // ---- coordinate spaces ----

        /// <summary>The game's LCD units (the ortho projection NNS_G2dResetMatrix sets) to viewport pixels.</summary>
        private static Rectangle LcdToPixels(Viewport view, float x, float y, float w, float h)
        {
            float ox = (480 - GlobalScope.LCD_WIDTH) / 2f;
            float oy = (320 - GlobalScope.LCD_HEIGHT) / 2f;
            float sx = view.Width / (float)GlobalScope.LCD_WIDTH;
            float sy = view.Height / (float)GlobalScope.LCD_HEIGHT;
            return new Rectangle((int)Math.Round((x - ox) * sx), (int)Math.Round((y - oy) * sy), Math.Max(1, (int)Math.Round(w * sx)), Math.Max(1, (int)Math.Round(h * sy)));
        }

        private static Rectangle TextToPixels(Viewport view, float x, float y, float w, float h)
        {
            float sx = view.Width / TextSpaceWidth;
            float sy = view.Height / TextSpaceHeight;
            return new Rectangle((int)(x * sx), (int)(y * sy), (int)(w * sx), (int)(h * sy));
        }

        private static void PixelsToText(Viewport view, float px, float py, out float tx, out float ty)
        {
            tx = px / view.Width * TextSpaceWidth;
            ty = py / view.Height * TextSpaceHeight;
        }

        private void Outline(Rectangle r, Color colour, int thickness)
        {
            _batch.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), colour);
            _batch.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), colour);
            _batch.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), colour);
            _batch.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), colour);
        }

        private void EnsureResources()
        {
            if (_batch == null)
            {
                _batch = new SpriteBatch(GraphicsDevice);
            }
            if (_pixel == null || _pixel.IsDisposed)
            {
                _pixel = new Texture2D(GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
        }
    }
}
