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
	public static class eff
	{
		public class CEffectMng
		{
			public const int EFFECT_MAX = 32;

			public const int EFP_ID_MAX = 5;

			public const int ERR_ID = -1;

			public static CEffectMng instance_ = new CEffectMng();

			private eld.SFileHeader[] m_EfpId = new eld.SFileHeader[5];

			private EffectObject[] effect_ = new EffectObject[32];

			public void initialize(ds.sys3d.Scene scene)
			{
				eld.g_elsvr.setup(eld.g_elgl, eld.g_elaloc, eld.g_elvmng, scene);
				eld.g_elsvr.eraseObjects();
				eld.g_elsvr.destroyEfp();
				clear();
				for (int i = 0; i < 5; i++)
				{
					m_EfpId[i] = null;
				}
			}

			public void loadEfi(string efi)
			{
				eld.g_elsvr.loadID(efi);
			}

			public void loadEfp(string efp)
			{
				for (int i = 0; i < 5; i++)
				{
					if (m_EfpId[i] == null)
					{
						eld.SFileHeader sFileHeader = static_cast<eld.SFileHeader>(eld.g_elsvr.loadEfp(efp));
						if (sFileHeader != null)
						{
							m_EfpId[i] = sFileHeader;
						}
						break;
					}
				}
			}

			// PORT (FF4): effectLoadAsync(name) / cleanUpEffectData2(name) address packs by name.
			private readonly string[] m_EfpName = new string[5];

			public bool loadEfpNamed(string name, string path)
			{
				for (int i = 0; i < 5; i++)
				{
					if (m_EfpId[i] != null && m_EfpName[i] == name)
					{
						return true;
					}
				}
				for (int i = 0; i < 5; i++)
				{
					if (m_EfpId[i] == null)
					{
						eld.SFileHeader header = static_cast<eld.SFileHeader>(eld.g_elsvr.loadEfp(path));
						if (header == null)
						{
							return false;
						}
						m_EfpId[i] = header;
						m_EfpName[i] = name;
						return true;
					}
				}
				return false;
			}

			public void unLoadEfpNamed(string name)
			{
				for (int i = 0; i < 5; i++)
				{
					if (m_EfpId[i] != null && m_EfpName[i] == name)
					{
						eld.g_elsvr.unloadEfp(m_EfpId[i]);
						m_EfpId[i] = null;
						m_EfpName[i] = null;
					}
				}
			}

			public void unLoadEfp(eld.SFileHeader _id)
			{
				if (_id == null)
				{
					return;
				}
				for (int i = 0; i < 5; i++)
				{
					if (m_EfpId[i] == _id)
					{
						eld.g_elsvr.unloadEfp(_id);
						m_EfpId[i] = null;
						break;
					}
				}
			}

			public void unLoadEfp2()
			{
				for (int num = 4; num >= 0; num--)
				{
					if (m_EfpId[num] != null)
					{
						eld.g_elsvr.unloadEfp(m_EfpId[num]);
						m_EfpId[num] = null;
						break;
					}
				}
			}

			public void allUnLoadEfp()
			{
				for (int i = 0; i < 5; i++)
				{
					if (m_EfpId[i] != null)
					{
						eld.g_elsvr.unloadEfp(m_EfpId[i]);
						m_EfpId[i] = null;
					}
				}
			}

			public void cleanup()
			{
				clear();
				eld.g_elsvr.eraseObjects();
				eld.g_elsvr.cleanup();
				NNS_GfdResetLnkTexVramState();
				NNS_GfdResetLnkPlttVramState();
				for (int i = 0; i < 5; i++)
				{
					m_EfpId[i] = null;
				}
			}

			public void execute()
			{
				eld.g_elsvr.doExecute();
				for (int i = 0; i < 32; i++)
				{
					if (effect_[i].isEnable_ && !effect_[i].object_.isPlay())
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
				if (effect_[_id].object_ != null)
				{
					eld.g_elsvr.deleteObject(effect_[_id].object_);
					effect_[_id].object_ = null;
				}
				effect_[_id].isEnable_ = false;
				effect_[_id].category_ = -1;
				effect_[_id].member_ = -1;
				return _id;
			}

			public int deleteEffect(int _id)
			{
				if (effect_[_id].object_ != null)
				{
					eld.g_elsvr.deleteObject(effect_[_id].object_);
					effect_[_id].object_ = null;
				}
				effect_[_id].isEnable_ = false;
				effect_[_id].category_ = -1;
				effect_[_id].member_ = -1;
				return _id;
			}

			public void registerEffectObject(eld.IObject obj, int _id)
			{
				effect_[_id].isEnable_ = true;
				effect_[_id].category_ = obj.getCategoryNo();
				effect_[_id].member_ = obj.getMemberNo();
				effect_[_id].object_ = obj;
			}

			public int serchEffectObject()
			{
				for (int i = 0; i < 32; i++)
				{
					if (!effect_[i].isEnable_)
					{
						return i;
					}
				}
				return -1;
			}

			public bool isEffectObject(int _id)
			{
				return effect_[_id].isEnable_;
			}

			public void clear()
			{
				for (int i = 0; i < 32; i++)
				{
					release(i);
				}
			}

			public void setPause(int _id, bool pause)
			{
				if (isEffectObject(_id))
				{
					if (pause)
					{
						effect_[_id].object_.Pause();
					}
					else
					{
						effect_[_id].object_.Restart();
					}
				}
			}

			public void setPosition(int _id, VecFx32 pos)
			{
				if (isEffectObject(_id))
				{
					int x = pos.x;
					int y = pos.y;
					int z = pos.z;
					effect_[_id].object_.SetPosition(x, y, z);
				}
			}

			public void setScale(int _id, VecFx32 scl)
			{
				if (isEffectObject(_id))
				{
					int x = scl.x;
					int y = scl.y;
					int z = scl.z;
					effect_[_id].object_.SetScale(x, y, z);
				}
			}

			public byte getLoadedEfpNum()
			{
				byte b = 0;
				for (byte b2 = 0; b2 < 5; b2++)
				{
					if (m_EfpId[b2] != null)
					{
						b++;
					}
				}
				return b;
			}

			public uint getEffectObjectNum()
			{
				return eld.g_elsvr.getNbObjects();
			}

			public CEffectMng()
			{
				for (int i = 0; i < effect_.Length; i++)
				{
					effect_[i] = new EffectObject();
				}
			}

			~CEffectMng()
			{
			}

			public static CEffectMng instance()
			{
				return instance_;
			}
		}

		public class EffectObject
		{
			public bool isEnable_;

			public int category_;

			public int member_;

			public eld.IObject object_;
		}
	}
}
