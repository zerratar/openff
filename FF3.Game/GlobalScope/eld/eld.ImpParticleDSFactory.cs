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
	public static partial class eld
	{
		public class ImpParticleDSFactory : Factory
		{
			private static SGuid guid_data = new SGuid(2442231383u, 39672, 19875, new byte[8] { 137, 152, 131, 189, 182, 39, 250, 40 });

			public ImpParticleDSFactory()
			{
				m_GUID.Set(guid_data);
			}

			public override IObject createObj(Template pTemplate)
			{
				EffAllocator<ImpParticleDS> effAllocator = new EffAllocator<ImpParticleDS>();
				ImpParticleDS impParticleDS = effAllocator.allocate(1u);
				if (impParticleDS != null)
				{
					impParticleDS.SetTemplate(pTemplate);
					if (!impParticleDS.prepare())
					{
						effAllocator.deallocate(impParticleDS);
						impParticleDS = null;
					}
				}
				return impParticleDS;
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

			~ImpParticleDSFactory()
			{
			}
		}
	}
}
