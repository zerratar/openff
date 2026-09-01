using System;
using System.Text;

public static class StringUtil
{
	private class SJISEncoding : Encoding
	{
		public override string WebName => "shift_jis";

		public char FallbackChar { get; set; }

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			int num = 0;
			for (int i = 0; i < byteCount; i++)
			{
				byte b = bytes[byteIndex + i];
				if ((32 <= b && b <= 128) || (160 <= b && b <= 223))
				{
					if (Sjis.Jis0201Table.ContainsKey(b))
					{
						chars[charIndex + num] = Sjis.Jis0201Table[b];
					}
					else
					{
						chars[charIndex + num] = FallbackChar;
					}
					num++;
				}
				else if (((129 <= b && b <= 159) || (224 <= b && b <= byte.MaxValue)) && i + 1 < byteCount)
				{
					ushort key = (ushort)((b << 8) | bytes[byteIndex + i + 1]);
					i++;
					if (Sjis.Jis0208Table.ContainsKey(key))
					{
						chars[charIndex + num] = Sjis.Jis0208Table[key];
					}
					else
					{
						chars[charIndex + num] = FallbackChar;
					}
					num++;
				}
				else
				{
					chars[charIndex + num] = (char)b;
					num++;
				}
			}
			return num;
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			return 0;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return 0;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				byte b = bytes[index + i];
				if ((32 <= b && b <= 128) || (160 <= b && b <= 223))
				{
					num++;
				}
				else if (((129 <= b && b <= 159) || (224 <= b && b <= byte.MaxValue)) && i + 1 < count)
				{
					_ = bytes[index + i + 1];
					i++;
					num++;
				}
				else
				{
					num++;
				}
			}
			return num;
		}

		public override int GetMaxByteCount(int charCount)
		{
			return 0;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return 0;
		}
	}

	private class Windows1252Encoding : Encoding
	{
		public override string WebName => "Windows-1252";

		public char FallbackChar { get; set; }

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			int num = 0;
			for (int i = 0; i < byteCount; i++)
			{
				byte b = bytes[byteIndex + i];
				chars[charIndex + num] = Windows1252.Table[b];
				num++;
			}
			return num;
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			return 0;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return 0;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				_ = bytes[index + i];
				num++;
			}
			return num;
		}

		public override int GetMaxByteCount(int charCount)
		{
			return 0;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return 0;
		}
	}

	private static SJISEncoding m_SJISEncoding = new SJISEncoding();

	private static Windows1252Encoding m_Windows1252Encoding = new Windows1252Encoding();

	public static string createString(byte[] data)
	{
		return createString(data, 0, data.Length);
	}

	public static string createString(sbyte[] data)
	{
		return createString(data, 0, data.Length);
	}

	public static string createString(byte[] data, int offset)
	{
		int i;
		for (i = 0; data[offset + i] != 0; i++)
		{
		}
		return createString(data, offset, i, "UTF-8");
	}

	public static string createString(sbyte[] data, int offset)
	{
		int i;
		for (i = 0; data[offset + i] != 0; i++)
		{
		}
		return createString(data, offset, i);
	}

	public static string createString(byte[] data, int offset, int byteCount)
	{
		return createString(data, offset, byteCount, "UTF-8");
	}

	public static string createString(sbyte[] data, int offset, int byteCount)
	{
		byte[] array = new byte[byteCount];
		Buffer.BlockCopy(data, offset, array, 0, byteCount);
		return createString(array);
	}

	public static string createString(byte[] data, int offset, int byteCount, string charsetName)
	{
		Encoding encoding = getEncoding(charsetName);
		char[] array = new char[encoding.GetCharCount(data, offset, byteCount)];
		encoding.GetChars(data, offset, byteCount, array, 0);
		int i;
		for (i = 0; i < array.Length && array[i] != 0; i++)
		{
		}
		return new string(array, 0, i);
	}

	public static byte[] getBytes(string str)
	{
		return getBytes(str, "UTF-8");
	}

	public static byte[] getBytes(string str, string charsetName)
	{
		Encoding encoding = getEncoding(charsetName);
		return encoding.GetBytes(str);
	}

	public static sbyte[] getSBytes(string str)
	{
		return getSBytes(str, "UTF-8");
	}

	public static sbyte[] getSBytes(string str, string charsetName)
	{
		byte[] bytes = getBytes(str, charsetName);
		sbyte[] array = new sbyte[bytes.Length];
		Buffer.BlockCopy(bytes, 0, array, 0, array.Length);
		return array;
	}

	public static string format(string strFormat, params object[] aArg)
	{
		int length = strFormat.Length;
		string text = "";
		int num = 0;
		char c = ' ';
		bool flag = false;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < length; i++)
		{
			switch (num4)
			{
			case 0:
			{
				char c2 = strFormat[i];
				if (c2 == '%')
				{
					c = ' ';
					flag = false;
					num2 = 0;
					num3 = 0;
					num4 = 1;
				}
				else
				{
					text += strFormat[i];
				}
				break;
			}
			case 1:
				switch (strFormat[i])
				{
				case 'C':
				case 'c':
				{
					string text2 = (((object)aArg[num].GetType() != typeof(char)) ? ((char)int.Parse(aArg[num].ToString())) : ((char)aArg[num])).ToString();
					num2 -= text2.Length;
					for (int j = 0; j < num2; j++)
					{
						text += c;
					}
					text += text2;
					num++;
					num4 = 0;
					break;
				}
				case 'D':
				case 'X':
				case 'd':
				case 'x':
				{
					string text2 = (((object)aArg[num].GetType() != typeof(bool)) ? int.Parse(aArg[num].ToString()) : (bool.Parse(aArg[num].ToString()) ? 1 : 0)).ToString(strFormat[i].ToString() + num3);
					num2 -= text2.Length;
					for (int j = 0; j < num2; j++)
					{
						text += c;
					}
					text += text2;
					num++;
					num4 = 0;
					break;
				}
				case 'F':
				case 'f':
				{
					string text2 = float.Parse(aArg[num].ToString()).ToString(strFormat[i].ToString() + num3);
					num2 -= text2.Length;
					for (int j = 0; j < num2; j++)
					{
						text += c;
					}
					text += text2;
					num++;
					num4 = 0;
					break;
				}
				case 'S':
				case 's':
				{
					string text2 = aArg[num].ToString();
					num2 -= text2.Length;
					for (int j = 0; j < num2; j++)
					{
						text += c;
					}
					text += text2;
					num++;
					num4 = 0;
					break;
				}
				case '%':
					text += "%";
					num4 = 0;
					break;
				case '0':
					c = '0';
					break;
				case '.':
					flag = true;
					break;
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (flag)
					{
						num3 = strFormat[i] - 48;
					}
					else
					{
						num2 = strFormat[i] - 48;
					}
					break;
				}
				break;
			}
		}
		return text;
	}

	private static Encoding getEncoding(string name)
	{
		if (name == "SJIS")
		{
			return m_SJISEncoding;
		}
		if (name == "windows-1252")
		{
			return m_Windows1252Encoding;
		}
		return Encoding.GetEncoding(name);
	}
}
