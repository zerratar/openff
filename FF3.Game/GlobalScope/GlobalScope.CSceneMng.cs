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
	public class CSceneMng
	{
		private string StageName;

		private string PreStageName;

		private string StageMdlName;

		private string CharacterName;

		private sbyte commonMdlNo_;

		public void initialize()
		{
			StageName = "\0";
			PreStageName = "\0";
			StageMdlName = "\0";
			CharacterName = "\0";
			commonMdlNo_ = -1;
		}

		public void gotoStage(string name)
		{
			PreStageName = StageName;
			StageName = name;
		}

		public string getStage()
		{
			return StageName;
		}

		public void getStage_set(string arg0)
		{
			StageName = arg0;
		}

		public void setStage(string name)
		{
			StageName = name;
		}

		public string getPreStage()
		{
			return PreStageName;
		}

		public void gotoBattle(string arg0)
		{
		}

		public void gotoField()
		{
		}

		public void setCharacter(string name)
		{
			sprintf(out CharacterName, "%s", name);
		}

		public string getCharacter()
		{
			return CharacterName;
		}

		public sbyte getFieldNo()
		{
			if (StageName[0] != 'f')
			{
				return -1;
			}
			return (sbyte)(StageName[2] - 48);
		}

		public sbyte getPreFieldNo()
		{
			if (PreStageName[0] != 'f')
			{
				return -1;
			}
			return (sbyte)(PreStageName[2] - 48);
		}

		public void setCommonMdlNo(sbyte no)
		{
			commonMdlNo_ = no;
		}

		public string getCommonMdl()
		{
			return commonMdlNo_ switch
			{
				1 => "s02_01", 
				2 => "s02_02", 
				3 => "s02_03", 
				4 => "s02_04", 
				5 => "s02_05", 
				30 => "t30_01", 
				99 => "prev", 
				_ => "\0", 
			};
		}

		public CSceneMng()
		{
			commonMdlNo_ = -1;
		}
	}
}
