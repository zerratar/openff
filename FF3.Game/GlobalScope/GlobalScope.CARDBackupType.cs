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
	public enum CARDBackupType
	{
		CARD_BACKUP_TYPE_NOT_USE,
		CARD_BACKUP_TYPE_EEPROM_4KBITS,
		CARD_BACKUP_TYPE_EEPROM_64KBITS,
		CARD_BACKUP_TYPE_EEPROM_512KBITS,
		CARD_BACKUP_TYPE_FLASH_2MBITS,
		CARD_BACKUP_TYPE_FLASH_4MBITS,
		CARD_BACKUP_TYPE_FRAM_256KBITS
	}
}
