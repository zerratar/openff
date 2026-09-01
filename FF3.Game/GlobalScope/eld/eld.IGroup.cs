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
	public static partial class eld
	{
		public class IGroup
		{
			public IParticle[] _pParticles;

			public ushort _nbParticles;

			public ushort _usTimeLife;

			public bool _bLife;

			public spr.EffSprAnim _sprite = new spr.EffSprAnim();

			public spr.EffSprForm _form = new spr.EffSprForm();

			~IGroup()
			{
			}

			public virtual void create(ImpBaseParticle parent)
			{
				setSprite(parent.getSpriteData());
			}

			public virtual void update(ImpBaseParticle parent)
			{
				_sprite.Update();
				_form.copy(_sprite.GetSprFormData());
			}

			public IGroup()
			{
				_pParticles = null;
				_nbParticles = 0;
				_usTimeLife = 0;
				_bLife = false;
			}

			public bool isPlay()
			{
				return _bLife;
			}

			public void setParticle(IParticle[] pParticle, ushort nbParticles)
			{
				_pParticles = pParticle;
				_nbParticles = nbParticles;
			}

			public virtual IParticle getParticle(uint index)
			{
				return _pParticles[index];
			}

			public void setSprite(spr.Eff_AnimationHeader pSprite)
			{
				_sprite.SetData(pSprite);
				_sprite.Initialize();
			}

			public spr.EffSprForm getAnimation()
			{
				return _form;
			}
		}
	}
}
