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
	public static partial class ds
	{
		public class BasicTextureObject : TextureObject
		{
			private Texture _texture;

			public BasicTextureObject()
			{
				_texture = null;
			}

			public BasicTextureObject(Texture pTexture)
			{
				setTexture(pTexture);
			}

			public void setTexture(Texture pTexture)
			{
				_texture = pTexture;
			}

			public Texture getTexture()
			{
				return _texture;
			}

			public override void sendTexture()
			{
				_texture.send();
			}

			public override Texture getTextureBody()
			{
				return _texture;
			}
		}
	}
}
