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
	public static partial class eld
	{
		public class ImpParticleLargeDSFactory : Factory
		{
			private static SGuid guid_data = new SGuid(1616894136u, 39214, 16895, new byte[8] { 149, 107, 245, 133, 176, 24, 57, 176 });

			public ImpParticleLargeDSFactory()
			{
				m_GUID.Set(guid_data);
			}

			public override IObject createObj(Template pTemplate)
			{
				EffAllocator<ImpParticleLargeDS> effAllocator = new EffAllocator<ImpParticleLargeDS>();
				ImpParticleLargeDS impParticleLargeDS = effAllocator.allocate(1u);
				if (impParticleLargeDS != null)
				{
					impParticleLargeDS.SetTemplate(pTemplate);
					if (!impParticleLargeDS.prepare())
					{
						effAllocator.deallocate(impParticleLargeDS);
						impParticleLargeDS = null;
					}
				}
				return impParticleLargeDS;
			}

			public override void initTemplate(Template pTemplate)
			{
				IServer.Instance().getVramManager().registerTexture(pTemplate.GetTextureAddr<ds.Texture>());
				spr.Eff_AnimationHeader animAddr = pTemplate.GetAnimAddr<spr.Eff_AnimationHeader>();
				spr.EffSprAnim.ToAbsoluteAddress(animAddr);
			}

			public override void disposeTemplate(Template pTemplate)
			{
				if (pTemplate.GetTextureAddr() != 0)
				{
					IServer.Instance().getVramManager().deregisterTexture(pTemplate.GetTextureAddr<ds.Texture>());
				}
			}

			~ImpParticleLargeDSFactory()
			{
			}
		}
	}
}
