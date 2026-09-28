using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

// PORT: a menu frame's background (Crystal Style Sheets: background-color, background-image, the
// 9-slice and the rest - OpenFF.Content.MenuBackground) drawn as a sprite of the game's own: added to
// the DS2D list with the windows, sorted by depth with them, and drawn the way NNS_G2dDrawCell draws a
// cell - quads through drawImage and glDrawArrays - from a picture of its own in one of the GL texture
// slots. So it sits under the cursor and the portrait and over the backdrop, as a window does.
internal static partial class GlobalScope
{
	internal sealed class MenuPanelSprite : sys2d.Sprite
	{
		/// <summary>The picture's quads (the frame's units, the picture's pixels), and the colour's under them.</summary>
		public List<OpenFF.Content.MenuBackground.Quad> Quads = new List<OpenFF.Content.MenuBackground.Quad>();
		public float Width, Height;
		public uint Texture;
		public int TextureWidth, TextureHeight;
		/// <summary>RGBA 0..255: the picture's tint and the colour under it, the frame's opacity in their alpha.</summary>
		public byte[] Tint = { 255, 255, 255, 255 };
		public byte[] Fill;
		/// <summary>Painted layers (MenuPaint: a gradient, a border, round corners, shadows), each a picture of its own over the frame - those Before under the picture, those After over it - drawn in Paint (white, the frame's opacity in its alpha).</summary>
		public List<(uint Texture, int Width, int Height, OpenFF.Content.MenuBackground.Quad Place)> Before = new List<(uint, int, int, OpenFF.Content.MenuBackground.Quad)>();
		public List<(uint Texture, int Width, int Height, OpenFF.Content.MenuBackground.Quad Place)> After = new List<(uint, int, int, OpenFF.Content.MenuBackground.Quad)>();
		public byte[] Paint = { 255, 255, 255, 255 };
	}

	private static uint _menuPanelWhite;

	/// <summary>A picture (PNG, JPG, BMP bytes) into a GL texture slot of its own; 0 when it will not load.</summary>
	internal static uint MenuPanelTexture(byte[] data, bool linear, out int width, out int height)
	{
		width = height = 0;
		if (data == null || data.Length == 0) return 0;
		GraphicsDevice device = m_Graphics.GetGraphicsDeviceManager().GraphicsDevice;
		Texture2D texture;
		using (MemoryStream stream = new MemoryStream(data)) texture = Texture2D.FromStream(device, stream);
		return MenuPanelSlot(texture, linear, out width, out height);
	}

	private static uint MenuPanelSlot(Texture2D texture, bool linear, out int width, out int height)
	{
		width = texture.Width;
		height = texture.Height;
		uint[] id = new uint[1];
		glGenTextures(1, id);
		if (id[0] == 0 || m_aGlTexture[id[0]] == null) { texture.Dispose(); return 0; }
		GlTexture slot = m_aGlTexture[id[0]];
		slot.m_Texture2D = texture;
		slot.m_TextureFilter = linear ? TextureFilter.Linear : TextureFilter.Point;
		slot.m_TextureAddressModeS = TextureAddressMode.Clamp;
		slot.m_TextureAddressModeT = TextureAddressMode.Clamp;
		return id[0];
	}

	/// <summary>A painted picture (RGBA, not premultiplied, as a PNG comes) into a GL texture slot of its own; 0 when it will not go.</summary>
	internal static uint MenuPanelRaster(byte[] rgba, int width, int height)
	{
		if (rgba == null || width <= 0 || height <= 0 || rgba.Length < width * height * 4) return 0;
		Texture2D texture = new Texture2D(m_Graphics.GetGraphicsDeviceManager().GraphicsDevice, width, height);
		texture.SetData(rgba);
		return MenuPanelSlot(texture, true, out _, out _);
	}

	internal static void MenuPanelRelease(uint id)
	{
		if (id == 0 || id == _menuPanelWhite) return;
		glDeleteTextures(1, new[] { id });
	}

	/// <summary>A white texel, for the background's colour.</summary>
	private static uint MenuPanelWhite()
	{
		if (_menuPanelWhite != 0 && m_aGlTexture[_menuPanelWhite] != null) return _menuPanelWhite;
		Texture2D white = new Texture2D(m_Graphics.GetGraphicsDeviceManager().GraphicsDevice, 1, 1);
		white.SetData(new[] { 0xFFFFFFFFu });
		_menuPanelWhite = MenuPanelSlot(white, false, out _, out _);
		return _menuPanelWhite;
	}

	/// <summary>DS2DManager.d2dRegisterSprite's for a panel: the sprite's place, then its colour and its picture's quads.</summary>
	internal static bool DrawMenuPanel(MenuPanelSprite sp)
	{
		if (skipFrame != 0 || !sp.IsShow()) return false;
		MTX_Identity43(currentMtx);
		G3_PushMtx();
		G3_Translate(sp.GetPosition().x, sp.GetPosition().y, 0);
		G3_PolygonAttr(0, GXPolygonMode.GX_POLYGONMODE_MODULATE, GXCull.GX_CULL_NONE, sp.GetPolygonID(), 31, 0);
		using (OpenFF.Client.FrameCapture.Own(sp, sp.m_iRegistration))
		{
			if (sp.Fill != null && sp.Fill[3] > 0)
			{
				uint white = MenuPanelWhite();
				if (white != 0) DrawMenuPanelQuads(white, 1, 1, new List<OpenFF.Content.MenuBackground.Quad> { new OpenFF.Content.MenuBackground.Quad(0, 0, sp.Width, sp.Height, 0, 0, 1, 1) }, sp.Fill);
			}
			foreach ((uint texture, int w, int h, OpenFF.Content.MenuBackground.Quad place) in sp.Before) DrawMenuPanelQuads(texture, w, h, new List<OpenFF.Content.MenuBackground.Quad> { place }, sp.Paint);
			if (sp.Texture != 0 && sp.Quads.Count > 0) DrawMenuPanelQuads(sp.Texture, sp.TextureWidth, sp.TextureHeight, sp.Quads, sp.Tint);
			foreach ((uint texture, int w, int h, OpenFF.Content.MenuBackground.Quad place) in sp.After) DrawMenuPanelQuads(texture, w, h, new List<OpenFF.Content.MenuBackground.Quad> { place }, sp.Paint);
		}
		G3_PopMtx(1);
		return true;
	}

	private static void DrawMenuPanelQuads(uint texture, int tw, int th, List<OpenFF.Content.MenuBackground.Quad> quads, byte[] colour)
	{
		// drawImage takes the picture's part in whole units scaled by texScaleU/V and keeps half a unit in from
		// each edge. In halves of a pixel here (a tiled edge's cut part lands within one), with a quarter more
		// kept in on each side, so the half pixel the game keeps for its cells is kept: a smooth picture does
		// not take in the pixels beside its part (the seams between one slice or tile and the next).
		const int Sub = 2;
		texScaleU = 1f / (tw * Sub);
		texScaleV = 1f / (th * Sub);
		glPushMatrix();
		float[] array = fnd_reuse_f;
		MTX_Copy43ToGLfloat(currentMtx, array);
		glMultMatrixf(array);
		int max = Math.Min(quads.Count, vtc.Length / 6);
		for (int i = 0; i < max; i++)
		{
			OpenFF.Content.MenuBackground.Quad q = quads[i];
			drawImage(vtc, i * 6, q.X - screenOffset[0], q.Y - screenOffset[1], q.W, q.H,
				(int)Math.Round((q.U + 0.25f) * Sub), (int)Math.Round((q.V + 0.25f) * Sub), Math.Max(1, (int)Math.Round((q.UW - 0.5f) * Sub)), Math.Max(1, (int)Math.Round((q.VH - 0.5f) * Sub)), colour);
		}
		glEnable(3553u);
		glBindTexture(3553u, texture);
		glDrawArrays(4u, 0, max * 6, vtc);
		polyCount += max * 6;
		glDisableClientState(32888u);
		glDisable(3553u);
		glPopMatrix();
	}
}
