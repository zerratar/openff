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
	public static partial class btl
	{
		public class BattleEffect
		{
			public const int EFFECT_MAX = 32;

			public const int EFFECT_MNG_MAX = 16;

			public const int ERR_ID = -1;

			private const int EFPID_MAX = 24;

			public static BattleEffect instance_ = new BattleEffect();

			private EffectObject[] effect_ = new EffectObject[32];

			private eld.SFileHeader[] efpId_ = new eld.SFileHeader[24];

			private int nowEfpIdMax_;

			private Array linkTexel_;

			private Array linkPalette_;

			public void setup(ds.sys3d.Scene scene)
			{
				eld.g_elsvr.setup(eld.g_elgl, eld.g_elaloc, eld.g_elvmng, scene);
				eld.g_elsvr.loadID("/EFFECT/effect.efi");
				eld.g_elsvr.eraseObjects();
				eld.g_elsvr.destroyEfp();
				eld.g_elsvr.loadEfp("/EFFECT/e201.efp");
				eld.g_elsvr.loadEfp("/EFFECT/e435.efp");
				clear();
			}

			public void cleanup()
			{
				endEfp();
				clear();
				eld.g_elsvr.cleanup();
			}

			public void execute()
			{
				eld.g_elsvr.doExecute();
				for (int i = 0; i < 32; i++)
				{
					if (effectObject(i).object_ != null && !effectObject(i).object_.isPlay())
					{
						deleteEffect(i);
					}
				}
			}

			public void deleteAll()
			{
				for (int i = 0; i < 32; i++)
				{
					if (effectObject(i).object_ != null)
					{
						deleteEffect(i);
					}
				}
			}

			public void draw()
			{
				eld.g_elsvr.doDraw();
			}

			public void update()
			{
				eld.g_elsvr.doUpdate();
			}

			public eld.SFileHeader addEfp(int categoryId)
			{
				string arg = "";
				sprintf(out arg, "/EFFECT/e%03d.efp", categoryId);
				efpId_[checkEfpId()] = eld.g_elsvr.loadEfp(arg);
				return efpId_[checkEfpId()];
			}

			public void subEfp(eld.SFileHeader efpId)
			{
				eld.g_elsvr.unloadEfp(efpId);
			}

			public void startEfp()
			{
				endEfp();
				nowEfpIdMax_ = 0;
				for (int i = 0; i < 24; i++)
				{
					efpId_[i] = null;
				}
			}

			public void endEfp()
			{
				for (int i = 0; i < 24; i++)
				{
					if (efpId_[i] != null)
					{
						subEfp(efpId_[i]);
						efpId_[i] = null;
					}
				}
				nowEfpIdMax_ = 0;
				clear();
			}

			public int checkEfpId()
			{
				for (int i = 0; i < 24; i++)
				{
					if (efpId_[i] == null)
					{
						nowEfpIdMax_ = i;
						return nowEfpIdMax_;
					}
				}
				nowEfpIdMax_ = 0;
				return nowEfpIdMax_;
			}

			public int create(int category, int member)
			{
				int num = serchEffectObject();
				if (num == -1)
				{
					return -1;
				}
				eld.IObject obj = eld.g_elsvr.createObject((uint)category, (uint)member);
				if (obj == null)
				{
					return -1;
				}
				registerEffectObject(obj, num);
				return num;
			}

			public int release(int _id)
			{
				if (effectObject(_id).object_ != null)
				{
					eld.g_elsvr.deleteObject(effectObject(_id).object_);
					effectObject(_id).object_ = null;
				}
				effectObject(_id).category_ = -1;
				effectObject(_id).member_ = -1;
				return _id;
			}

			public int deleteEffect(int _id)
			{
				if (_id < 0)
				{
					return _id;
				}
				if (effectObject(_id).object_ != null)
				{
					effectObject(_id).object_.DeleteObject();
					effectObject(_id).object_ = null;
				}
				effectObject(_id).category_ = -1;
				effectObject(_id).member_ = -1;
				return _id;
			}

			public void registerEffectObject(eld.IObject obj, int _id)
			{
				effectObject(_id).object_ = obj;
				effectObject(_id).category_ = obj.getCategoryNo();
				effectObject(_id).member_ = obj.getMemberNo();
			}

			public int serchEffectObject()
			{
				for (int i = 0; i < 32; i++)
				{
					if (effectObject(i).object_ == null)
					{
						return i;
					}
				}
				return -1;
			}

			public bool isEffectObject(int _id)
			{
				if (effectObject(_id).object_ == null)
				{
					return false;
				}
				return true;
			}

			public void clear()
			{
				for (int i = 0; i < 32; i++)
				{
					release(i);
				}
			}

			public void setPosition(int _id, VecFx32 pos)
			{
				int x = pos.x;
				int y = pos.y;
				int z = pos.z;
				effectObject(_id).object_.SetPosition(x, y, z);
			}

			public BattleEffect()
			{
				for (int i = 0; i < effect_.Length; i++)
				{
					effect_[i] = new EffectObject();
				}
			}

			public static BattleEffect instance()
			{
				return instance_;
			}

			public EffectObject effectObject(int _id)
			{
				return effect_[_id];
			}
		}
	}
}
