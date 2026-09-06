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
		public class ModelDSSetup
		{
			public uint version;

			public uint flag;

			public ds.Vector3<int> vScale;

			public ds.Vector3<int> vDiffuse;

			public ds.Vector3<int> vAmbient;

			public ds.Vector3<int> vSpecular;

			public ds.Vector3<int> vEmission;

			public string nameIma;

			public string nameIta;

			public string nameItp;

			public string nameIva;

			public static explicit operator ModelDSSetup(ArrayReader src)
			{
				ModelDSSetup modelDSSetup = new ModelDSSetup();
				byte[] array = new byte[MDS_AMN_NAME_SIZE];
				modelDSSetup.vScale = new ds.Vector3<int>();
				modelDSSetup.vDiffuse = new ds.Vector3<int>();
				modelDSSetup.vAmbient = new ds.Vector3<int>();
				modelDSSetup.vSpecular = new ds.Vector3<int>();
				modelDSSetup.vEmission = new ds.Vector3<int>();
				modelDSSetup.version = src.readUInt32();
				modelDSSetup.flag = src.readUInt32();
				modelDSSetup.vScale.vx = src.readInt32();
				modelDSSetup.vScale.vy = src.readInt32();
				modelDSSetup.vScale.vz = src.readInt32();
				modelDSSetup.vDiffuse.vx = src.readInt32();
				modelDSSetup.vDiffuse.vy = src.readInt32();
				modelDSSetup.vDiffuse.vz = src.readInt32();
				modelDSSetup.vAmbient.vx = src.readInt32();
				modelDSSetup.vAmbient.vy = src.readInt32();
				modelDSSetup.vAmbient.vz = src.readInt32();
				modelDSSetup.vSpecular.vx = src.readInt32();
				modelDSSetup.vSpecular.vy = src.readInt32();
				modelDSSetup.vSpecular.vz = src.readInt32();
				modelDSSetup.vEmission.vx = src.readInt32();
				modelDSSetup.vEmission.vy = src.readInt32();
				modelDSSetup.vEmission.vz = src.readInt32();
				src.read(array, 0, (int)MDS_AMN_NAME_SIZE);
				modelDSSetup.nameIma = StringUtil.createString(array);
				src.read(array, 0, (int)MDS_AMN_NAME_SIZE);
				modelDSSetup.nameIta = StringUtil.createString(array);
				src.read(array, 0, (int)MDS_AMN_NAME_SIZE);
				modelDSSetup.nameItp = StringUtil.createString(array);
				src.read(array, 0, (int)MDS_AMN_NAME_SIZE);
				modelDSSetup.nameIva = StringUtil.createString(array);
				return modelDSSetup;
			}
		}
	}
}
