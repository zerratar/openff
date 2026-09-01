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
		public class ImpParticleGatherDSFactory : Factory
		{
			private static SGuid guid_data = new SGuid(3425302807u, 22784, 20071, new byte[8] { 150, 35, 209, 12, 186, 208, 204, 181 });

			public ImpParticleGatherDSFactory()
			{
				m_GUID.Set(guid_data);
			}

			public override IObject createObj(Template pTemplate)
			{
				EffAllocator<ImpParticleGatherDS> effAllocator = new EffAllocator<ImpParticleGatherDS>();
				ImpParticleGatherDS impParticleGatherDS = effAllocator.allocate(1u);
				if (impParticleGatherDS != null)
				{
					impParticleGatherDS.SetTemplate(pTemplate);
					if (!impParticleGatherDS.prepare())
					{
						effAllocator.deallocate(impParticleGatherDS);
						impParticleGatherDS = null;
					}
				}
				return impParticleGatherDS;
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

			~ImpParticleGatherDSFactory()
			{
			}
		}
	}
}
