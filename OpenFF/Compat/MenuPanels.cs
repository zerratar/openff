// Panels drawn with the game's sprites: a frame's background (MenuBackground - a colour, a picture
// laid out as quads) and its painted box (MenuPaint - a gradient, a border, round corners, shadows),
// each a MenuPanelSprite in the depth-sorted pass with the windows (GlobalScope.MenuPanel.cs). A set
// keeps its sprites and their textures to release together: a menu screen's (ModMenus), the field's
// HUD's (FieldHud).

using System;
using System.Collections.Generic;
using System.IO;
using OpenFF.Content;

namespace OpenFF.Client
{
	internal sealed class MenuPanels
	{
		private readonly List<GlobalScope.MenuPanelSprite> _sprites = new List<GlobalScope.MenuPanelSprite>();
		// Pictures by kind, path and filter (shared by the frames that use the same one), and painted boxes (each its own).
		private readonly Dictionary<string, (uint Id, int W, int H)> _pictures = new Dictionary<string, (uint, int, int)>(StringComparer.OrdinalIgnoreCase);
		private readonly List<uint> _painted = new List<uint>();

		/// <summary>How many sprites the set shows.</summary>
		public int Count => _sprites.Count;

		/// <summary>Pixels a menu unit for a painted box: the window's, so its edges are as sharp as the text's.</summary>
		public static float PaintScale()
		{
			try { return Math.Clamp((float)Math.Ceiling(GlobalScope.m_Graphics.GetGraphicsDeviceManager().GraphicsDevice.Viewport.Height / (float)Math.Max(1, GlobalScope.LCD_HEIGHT)), 1f, 4f); }
			catch (Exception) { return 2f; }
		}

		/// <summary>
		/// A background over a rectangle of the screen, at a depth in the windows' stack: url() pictures read from the directory
		/// given (a layout's). Its sprites and painted textures are noted in 'sprites' and 'textures' when given (a frame's own, to
		/// take away alone). A frame's panel stands between a window's fill (its depth and 16) and its frame (its depth): the
		/// window's depth and 8; its outer shadows a sprite of their own 16 further back, behind the fill.
		/// </summary>
		public void Add(string directory, string what, int x, int y, int width, int height, string declarations, double alpha, int depth,
			List<GlobalScope.MenuPanelSprite> sprites = null, List<uint> textures = null)
		{
			MenuBackground bg = MenuBackground.Parse(declarations);
			if (bg == null) return;
			// A named sprite of the sheet (sprites.json beside the layouts): its part and its borders.
			if (bg.Sprite != null)
			{
				MenuSprites.Sprite named = MenuSprites.Find(directory, bg.ImageKind, bg.ImagePath, bg.Sprite);
				if (named != null) bg.Use(named);
				else Log.Write(LogChannel.General, "menus: " + what + ": no sprite '" + bg.Sprite + "' on " + bg.ImagePath + " in " + MenuSprites.FileName);
			}
			// The colours whole; the frame's opacity the sprite's own, over them as it is drawn (changed in place as it fades).
			float opacity = (float)Math.Clamp(alpha, 0, 1);
			GlobalScope.MenuPanelSprite sprite = new GlobalScope.MenuPanelSprite { Width = width, Height = height, Opacity = opacity };
			byte[] Bytes(uint rgba) => new[] { (byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8), (byte)(rgba & 0xFF) };
			if (bg.Colour.HasValue) sprite.Fill = Bytes(bg.Colour.Value);
			sprite.Tint = Bytes(bg.Tint);
			if (bg.ImagePath != null)
			{
				(uint Id, int W, int H) texture = Picture(directory, what, bg);
				if (texture.Id != 0)
				{
					sprite.Texture = texture.Id;
					sprite.TextureWidth = texture.W;
					sprite.TextureHeight = texture.H;
					sprite.Quads = bg.Layout(width, height, texture.W, texture.H);
				}
			}
			// The box painted: the colour and the gradient under the picture, the inset shadows and the border over it, the
			// outer shadows a sprite of their own behind the frame's window.
			if (bg.Decorated)
			{
				sprite.Fill = null;
				float scale = PaintScale();
				try
				{
					if (Painted(Paint(declarations, bg, width, height, scale, MenuPaint.Layer.Fill), textures) is (uint, int, int, MenuBackground.Quad) fill) sprite.Before.Add(fill);
					if (Painted(Paint(declarations, bg, width, height, scale, MenuPaint.Layer.Over), textures) is (uint, int, int, MenuBackground.Quad) over) sprite.After.Add(over);
					if (Painted(Paint(declarations, bg, width, height, scale, MenuPaint.Layer.Shadow), textures) is (uint, int, int, MenuBackground.Quad) shade)
					{
						GlobalScope.MenuPanelSprite shadow = new GlobalScope.MenuPanelSprite { Width = width, Height = height, Opacity = opacity };
						shadow.Before.Add(shade);
						Show(shadow, x, y, depth + GlobalScope.ds.S32toFX32(16), sprites);
					}
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + what + ": the box could not be painted: " + ex.Message); }
			}
			Show(sprite, x, y, depth, sprites);
		}

		// Painted boxes kept by what they are - the declarations, the size, the scale and the layer -, the least recently used let go
		// first: the rows of a list paint one box between them, and a screen opened again (a menu come back to) paints nothing. The
		// pixels are kept, not the textures (a GL slot each while shown); a box with nothing on a layer is kept too (null).
		private static readonly Dictionary<string, LinkedListNode<(string Key, MenuPaint.Raster Raster)>> _rasters = new Dictionary<string, LinkedListNode<(string, MenuPaint.Raster)>>(StringComparer.Ordinal);
		private static readonly LinkedList<(string Key, MenuPaint.Raster Raster)> _rasterOrder = new LinkedList<(string, MenuPaint.Raster)>();
		private static long _rasterBytes;
		private const long RasterBudget = 96L * 1024 * 1024;

		private static MenuPaint.Raster Paint(string declarations, MenuBackground bg, int width, int height, float scale, MenuPaint.Layer layer)
		{
			string key = declarations + "|" + width + "x" + height + "@" + scale.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + layer;
			if (_rasters.TryGetValue(key, out LinkedListNode<(string Key, MenuPaint.Raster Raster)> known))
			{
				_rasterOrder.Remove(known);
				_rasterOrder.AddFirst(known);
				return known.Value.Raster;
			}
			MenuPaint.Raster raster = MenuPaint.Paint(bg, width, height, scale, layer);
			_rasters[key] = _rasterOrder.AddFirst((key, raster));
			_rasterBytes += raster?.Pixels.Length ?? 0;
			while (_rasterBytes > RasterBudget && _rasterOrder.Last != null && _rasterOrder.Last != _rasterOrder.First)
			{
				(string oldKey, MenuPaint.Raster old) = _rasterOrder.Last.Value;
				_rasterOrder.RemoveLast();
				_rasters.Remove(oldKey);
				_rasterBytes -= old?.Pixels.Length ?? 0;
			}
			return raster;
		}

		/// <summary>A picture's texture, loaded once for the set: one of the game's (resource) or a file beside the layout (url).</summary>
		private (uint, int, int) Picture(string directory, string what, MenuBackground bg)
		{
			string path = null;
			if (bg.ImageKind != "resource" && directory != null) path = Path.GetFullPath(Path.Combine(directory, bg.ImagePath));
			// A file's time in the key: a picture edited while the client runs is read afresh.
			string stamp = path != null && File.Exists(path) ? File.GetLastWriteTimeUtc(path).Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
			string key = bg.ImageKind + ":" + (path ?? bg.ImagePath) + (bg.Linear ? "" : "#point") + "@" + stamp;
			if (_pictures.TryGetValue(key, out (uint Id, int W, int H) texture)) return texture;
			if (_shared.TryGetValue(key, out Shared shared))
			{
				shared.Users++;
				shared.Used = ++_useClock;
				texture = (shared.Id, shared.W, shared.H);
			}
			else
			{
				byte[] data = null;
				texture = (0u, 0, 0);
				try
				{
					if (bg.ImageKind == "resource") data = GameArchive.Read(bg.ImagePath);
					else if (path != null && File.Exists(path)) data = File.ReadAllBytes(path);
					int tw = 0, th = 0;
					uint id = data == null ? 0 : GlobalScope.MenuPanelTexture(data, bg.Linear, out tw, out th);
					texture = id == 0 ? (0u, 0, 0) : (id, tw, th);
					if (id == 0 && data == null) Log.Write(LogChannel.General, "menus: " + what + ": no picture " + bg.ImageKind + "(\"" + bg.ImagePath + "\")");
				}
				catch (Exception ex) { Log.Write(LogChannel.General, "menus: " + what + ": picture " + bg.ImagePath + ": " + ex.Message); }
				if (texture.Id != 0) _shared[key] = new Shared { Id = texture.Id, W = texture.W, H = texture.H, Users = 1, Used = ++_useClock };
			}
			_pictures[key] = texture;
			return texture;
		}

		// Pictures shared by every set that shows them, and kept a while after the last lets go: a menu come back to (the main
		// menu after Item) finds its backdrop and its sheets decoded. The least recently used past Kept are let go.
		private sealed class Shared { public uint Id; public int W, H, Users; public long Used; }
		private static readonly Dictionary<string, Shared> _shared = new Dictionary<string, Shared>(StringComparer.OrdinalIgnoreCase);
		private static long _useClock;
		private const int Kept = 16;

		private static void LetGo(string key)
		{
			if (!_shared.TryGetValue(key, out Shared shared)) return;
			shared.Users = Math.Max(0, shared.Users - 1);
			List<KeyValuePair<string, Shared>> idle = new List<KeyValuePair<string, Shared>>();
			foreach (KeyValuePair<string, Shared> kv in _shared) if (kv.Value.Users == 0) idle.Add(kv);
			if (idle.Count <= Kept) return;
			idle.Sort((a, b) => a.Value.Used.CompareTo(b.Value.Used));
			for (int i = 0; i < idle.Count - Kept; i++)
			{
				_shared.Remove(idle[i].Key);
				try { GlobalScope.MenuPanelRelease(idle[i].Value.Id); } catch (Exception) { }
			}
		}

		/// <summary>A painted layer as a texture and its place over the frame; null for nothing.</summary>
		private (uint, int, int, MenuBackground.Quad)? Painted(MenuPaint.Raster r, List<uint> textures)
		{
			if (r == null) return null;
			uint id = GlobalScope.MenuPanelRaster(r.Pixels, r.Width, r.Height);
			if (id == 0) return null;
			_painted.Add(id);
			textures?.Add(id);
			return (id, r.Width, r.Height, new MenuBackground.Quad(r.X, r.Y, r.W, r.H, 0, 0, r.Width, r.Height));
		}

		private void Show(GlobalScope.MenuPanelSprite sprite, int x, int y, int depth, List<GlobalScope.MenuPanelSprite> sprites)
		{
			sprite.SetPlane(GlobalScope.sys2d.DS2D_OBJ_PLANE.DS2D_OBJ_PLANE_MAIN3D);
			sprite.SetPriority(3);
			sprite.SetDepth(depth);
			sprite.SetPositionI(x, y);
			sprite.BaseX = x;
			sprite.BaseY = y;
			sprite.SetShow(show: true);
			GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dAddSprite(sprite);
			_sprites.Add(sprite);
			sprites?.Add(sprite);
		}

		// Sprites taken away wait a frame: a sprite added to the game's list draws from the next frame on, so the one it takes the
		// place of stays until then (a panel made again every frame - an animated colour - would otherwise never be seen).
		private readonly List<GlobalScope.MenuPanelSprite> _doomedSprites = new List<GlobalScope.MenuPanelSprite>();
		private readonly List<uint> _doomedTextures = new List<uint>();

		/// <summary>Some of the set's sprites taken away, and their painted textures (a frame's panel made again) - at the next Flush.</summary>
		public void Remove(IEnumerable<GlobalScope.MenuPanelSprite> sprites, IEnumerable<uint> textures)
		{
			foreach (GlobalScope.MenuPanelSprite sp in sprites) if (_sprites.Remove(sp)) _doomedSprites.Add(sp);
			foreach (uint t in textures) if (_painted.Remove(t)) _doomedTextures.Add(t);
		}

		/// <summary>What was taken away a frame ago let go: once a frame (the screen's tick).</summary>
		public void Flush()
		{
			foreach (GlobalScope.MenuPanelSprite sp in _doomedSprites) { try { GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(sp); } catch (Exception) { } }
			_doomedSprites.Clear();
			foreach (uint t in _doomedTextures) { try { GlobalScope.MenuPanelRelease(t); } catch (Exception) { } }
			_doomedTextures.Clear();
		}

		/// <summary>A frame's sprites moved from their places (a translate), shown or not, in place.</summary>
		public static void Move(IEnumerable<GlobalScope.MenuPanelSprite> sprites, int dx, int dy, bool show = true)
		{
			foreach (GlobalScope.MenuPanelSprite sp in sprites)
			{
				sp.SetPositionI(sp.BaseX + dx, sp.BaseY + dy);
				sp.SetShow(show);
			}
		}

		/// <summary>A frame's sprites at an opacity, in place.</summary>
		public static void Fade(IEnumerable<GlobalScope.MenuPanelSprite> sprites, double opacity)
		{
			foreach (GlobalScope.MenuPanelSprite sp in sprites) sp.Opacity = (float)Math.Clamp(opacity, 0, 1);
		}

		/// <summary>Every sprite and texture of the set released.</summary>
		public void Clear()
		{
			Flush();
			foreach (GlobalScope.MenuPanelSprite p in _sprites) { try { GlobalScope.sys2d.DS2DManager.d2dGetInstance().d2dDeleteSprite(p); } catch (Exception) { } }
			_sprites.Clear();
			foreach (string key in _pictures.Keys) LetGo(key);
			_pictures.Clear();
			foreach (uint t in _painted) { try { GlobalScope.MenuPanelRelease(t); } catch (Exception) { } }
			_painted.Clear();
		}
	}
}
