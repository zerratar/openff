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
	public static partial class mognet
	{
		public class MNMemento
		{
			public enum ADD_FRIEND_ERROR
			{
				AFE_SUCCESS,
				AFE_LIST_FULL,
				AFE_ALREADY_REGISTER
			}

			public const ADD_FRIEND_ERROR AFE_SUCCESS = ADD_FRIEND_ERROR.AFE_SUCCESS;

			public const ADD_FRIEND_ERROR AFE_LIST_FULL = ADD_FRIEND_ERROR.AFE_LIST_FULL;

			public const ADD_FRIEND_ERROR AFE_ALREADY_REGISTER = ADD_FRIEND_ERROR.AFE_ALREADY_REGISTER;

			public static MNMemento instance_ = new MNMemento();

			protected MNMementoData data_ = new MNMementoData();

			protected ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>> mails = new ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>>(MAILS_LIST_LENGTH);

			protected long npce_last_send_time;

			protected MNMail temporary_;

			protected MNFriendDataBackup friendDataBackup_ = new MNFriendDataBackup();

			public MNMemento()
			{
				MATH_CRC32InitTable(crc32table);
				mnmClearBackup();
			}

			public bool mnmAddMail(MNMail m, bool filter)
			{
				int num = -1;
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					if (SWC_IsValidFriendData(data_.friends[i]))
					{
						int num2 = DWC_GetGsProfileId(data_.user, data_.friends[i]);
						if (num2 > 0 && num2 == m.profileID())
						{
							num = i;
							break;
						}
					}
				}
				if (filter && num < 0)
				{
					return false;
				}
				if (num >= 0 && strcmp(m.name(), data_.fnames[num]) != 0)
				{
					m.name_set(data_.fnames[num]);
				}
				if (mails.size() >= MAILS_LIST_LENGTH)
				{
					mails.erase(0);
				}
				mails.push_back(m);
				if (m.appendage() > 0)
				{
					FlagManager.singleton().set(0u, (uint)m.appendage());
				}
				if (m.appendage2() > 0)
				{
					for (int j = 0; j < 10; j++)
					{
						FlagManager.singleton().reset(0u, (uint)(480 + j));
					}
					FlagManager.singleton().set(0u, 479u);
					FlagManager.singleton().set(0u, (uint)m.appendage2());
				}
				return true;
			}

			public ADD_FRIEND_ERROR mnmAddFriend(ref int fd, ref string name, ushort locale)
			{
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					if (SWC_IsEqualFriendData(mnmGetFriendList(i), fd))
					{
						if (strcmp(name, mnmMementoData().fnames[i]) != 0)
						{
							name = mnmMementoData().fnames[i];
							mnmSetFriendListDirty(m: true);
						}
						return ADD_FRIEND_ERROR.AFE_ALREADY_REGISTER;
					}
				}
				for (int j = 0; j < FRIENDS_LIST_LENGTH; j++)
				{
					if (!SWC_IsValidFriendData(mnmGetFriendList(j)))
					{
						fd = mnmMementoData().friends[j];
						name = mnmMementoData().fnames[j];
						mnmSetFriendListDirty(m: true);
						return ADD_FRIEND_ERROR.AFE_SUCCESS;
					}
				}
				return ADD_FRIEND_ERROR.AFE_LIST_FULL;
			}

			public bool mnmCanAddFriend()
			{
				bool result = false;
				for (int i = 0; FRIENDS_LIST_LENGTH > i; i++)
				{
					if (!SWC_IsValidFriendData(mnmGetFriendList(i)))
					{
						result = true;
						break;
					}
				}
				return result;
			}

			public bool mnmCheckFriendAlreadyRegister(int fd)
			{
				bool result = false;
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					if (SWC_IsEqualFriendData(mnmGetFriendList(i), fd))
					{
						result = true;
						break;
					}
				}
				return result;
			}

			public bool mnmWiFiAccessed()
			{
				return data_.user.gs_profile_id != 0;
			}

			public void mnmClearAllBuddies()
			{
				for (int i = 0; i < FRIENDS_LIST_LENGTH; i++)
				{
					int num = mnmGetFriendList(i);
					if (SWC_IsValidFriendData(num) && SWC_IsBuddyFriendData(num))
					{
						SWC_ClearBuddyFlagFriendData(num);
						mnmSetFriendListDirty(m: true);
					}
				}
			}

			public bool mnmLoadBackup()
			{
				byte[] array = new byte[1188];
				card.Manager.GetInstance().LoadData(array, 1188u, 55504u);
				data_ = (MNMementoData)array;
				if (!mnmUserCRCheck())
				{
					data_.user.setDefault();
				}
				if (!mnmFriendCRCheck())
				{
					MI_CpuClear8(data_.friends, 0);
				}
				if (!mnmDateTimeCRCheck())
				{
					data_.last_send_date.setDefault();
				}
				return true;
			}

			public bool mnmSaveBackup()
			{
				mnmSaveBackupUserDataOnly();
				mnmSaveFriendList();
				mnmSaveDateTime();
				return true;
			}

			public void mnmClearBackup()
			{
				data_.setDefault();
			}

			public bool mnmUserCRCheck()
			{
				return true;
			}

			public bool mnmFriendCRCheck()
			{
				return data_.friend_crc == MATH_CalcCRC32(crc32table, data_.friends, 0u);
			}

			public bool mnmDateTimeCRCheck()
			{
				return true;
			}

			public void mnmSaveBackupUserDataOnly()
			{
				card.Manager.GetInstance().WriteData(data_.user.toByteArray(), 44u, 55504u);
			}

			public bool mnmInitialize()
			{
				mnmLoadBackup();
				friendDataBackup_.copy(data_);
				return false;
			}

			public bool mnmCheckUpdateUserData()
			{
				return false;
			}

			public bool mnmIsBuddyFriend(int N)
			{
				return SWC_IsBuddyFriendData(mnmGetFriendList(N));
			}

			public void mnmSaveFriendList()
			{
				mnmSetFriendListDirty(m: false);
				card.Manager.GetInstance().WriteData(BitReader.convertByteArray(data_.friends, 0, FRIENDS_LIST_LENGTH), 0u, 55504u);
				data_.friend_crc = MATH_CalcCRC32(crc32table, data_.friends, 0u);
			}

			public void mnmClearDateTime()
			{
				data_.last_send_date.setDefault();
				npce_last_send_time = 0L;
			}

			public void mnmSaveDateTime()
			{
				RTCDate rTCDate = new RTCDate();
				RTCTime rTCTime = new RTCTime();
				RTC_GetDateTime(rTCDate, rTCTime);
				data_.last_send_date = rTCDate;
				data_.last_send_time = rTCTime;
				card.Manager.GetInstance().WriteData(data_.last_send_date.toByteArray(), 36u, 55504u);
			}

			public bool mnmCheckDateTimeForEvent()
			{
				RTCDate date = new RTCDate();
				RTCTime time = new RTCTime();
				RTC_GetDateTime(date, time);
				long num = RTC_ConvertDateTimeToSecond(date, time);
				num -= RTC_ConvertDateTimeToSecond(data_.last_send_date, data_.last_send_time);
				if (num > RESTRICT_DURATION)
				{
					return true;
				}
				return false;
			}

			public void mnmSaveDateTimeNPC()
			{
				RTCDate date = new RTCDate();
				RTCTime time = new RTCTime();
				RTC_GetDateTime(date, time);
				npce_last_send_time = RTC_ConvertDateTimeToSecond(date, time);
			}

			public bool mnmCheckDateTimeForNPC()
			{
				RTCDate date = new RTCDate();
				RTCTime time = new RTCTime();
				RTC_GetDateTime(date, time);
				long num = RTC_ConvertDateTimeToSecond(date, time);
				num -= npce_last_send_time;
				if (num > RESTRICT_DURATION)
				{
					return true;
				}
				return false;
			}

			public void mnmSetFriendName(int idx, string name)
			{
				data_.fnames[idx] = name;
				mnmSetFriendListDirty(m: true);
			}

			public void mnmUpdateFriendListBackup()
			{
				if (friendDataBackup_.compare(data_))
				{
					friendDataBackup_.copy(data_);
					mnmSaveFriendList();
				}
			}

			public void mnmUpdateUserDataBackup()
			{
				if (mnmCheckUpdateUserData())
				{
					mnmSaveBackupUserDataOnly();
				}
			}

			public static MNMemento getSingleton()
			{
				return instance_;
			}

			public SWCUserData mnmGetUserData()
			{
				return data_.user;
			}

			public long mnmGetDateTimeNPC()
			{
				return npce_last_send_time;
			}

			public void mnmSetDateTimeNPC(long t)
			{
				npce_last_send_time = t;
			}

			public int mnmGetFriendList(int idx)
			{
				return data_.friends[idx];
			}

			public string mnmGetFriendName(int idx)
			{
				return data_.fnames[idx];
			}

			public MNMail mnmGetMailTemporary()
			{
				return temporary_;
			}

			public void mnmGetMails(ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>> ma)
			{
				mails.copy(ma);
			}

			public void mnmSetMails(ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>> ma)
			{
				ma.copy(mails);
			}

			public void mnmClearMail()
			{
				mails.clear();
			}

			public ds.Vector<MNMail, ds.OrderSavedErasePolicy<MNMail>> mnmMailArray()
			{
				return mails;
			}

			public MNMementoData mnmMementoData()
			{
				return data_;
			}

			public void mnmSetFriendListDirty(bool m)
			{
			}
		}
	}
}
