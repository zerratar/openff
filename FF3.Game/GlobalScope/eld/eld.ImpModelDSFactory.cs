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
		public class ImpModelDSFactory : Factory
		{
			private static SGuid guid_data = new SGuid(2741830544u, 61611, 16762, new byte[8] { 161, 47, 188, 185, 237, 245, 31, 62 });

			public ImpModelDSFactory()
			{
				m_GUID.Set(guid_data);
			}

			public override IObject createObj(Template pTemplate)
			{
				EffAllocator<ImpModelDS> effAllocator = new EffAllocator<ImpModelDS>();
				ImpModelDS impModelDS = effAllocator.allocate(1u);
				if (impModelDS != null)
				{
					impModelDS.SetTemplate(pTemplate);
					if (!impModelDS.prepare())
					{
						effAllocator.deallocate(impModelDS);
						impModelDS = null;
					}
				}
				return impModelDS;
			}

			public override void initTemplate(Template pTemplate)
			{
				ModelDSNmdpHeader textureAddr = pTemplate.GetTextureAddr<ModelDSNmdpHeader>();
				ModelDSNcapHeader animAddr = pTemplate.GetAnimAddr<ModelDSNcapHeader>();
				if (textureAddr.unInitialize == 0)
				{
					textureAddr.unInitialize = 1u;
				}
				if (animAddr.unInitialize == 0)
				{
					animAddr.unInitialize = 1u;
				}
				if (textureAddr.unModelTexturePointer == null)
				{
					textureAddr.unModelTexturePointer = IServer.Instance().getVramManager().registerModelTexture(textureAddr.unOffsetTexture);
				}
			}

			public override void disposeTemplate(Template pTemplate)
			{
				ModelDSNmdpHeader textureAddr = pTemplate.GetTextureAddr<ModelDSNmdpHeader>();
				if (textureAddr.unInitialize != 0)
				{
					IServer.Instance().getVramManager().deregisterModelTexture(textureAddr.unModelTexturePointer);
					textureAddr.unModelTexturePointer = null;
				}
			}

			~ImpModelDSFactory()
			{
			}
		}
	}
}
