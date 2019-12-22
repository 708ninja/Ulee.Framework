//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Binary Set Class
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
namespace Ulee.Utils
{
	//--------------------------------------------------------------------------
	public enum EUlEndian { Little, Big }

    //--------------------------------------------------------------------------
    public class UlBinaryException : ApplicationException
    {
        public int ErrorCode { get; }

        public UlBinaryException()
            : base("Occurred Terminated Exception!")
        {
            ErrorCode = 0;
        }

        public UlBinaryException(string aMsg)
            : base(aMsg)
        {
            ErrorCode = 0;
        }

        public UlBinaryException(string aMsg, int aErrorCode)
            : base(aMsg)
        {
            ErrorCode = aErrorCode;
        }
    }

    //--------------------------------------------------------------------------
    public static class UlBits
	{
		//----------------------------------------------------------------------
		private static void CheckRange(int AIndex)
		{
			if (AIndex >= 8)
			{
				throw new UlBinaryException("Occurred Over range Exception in UlBinary", -1);
			}
		}

		//----------------------------------------------------------------------
		public static bool Get(byte AValue, int AIndex)
		{
			CheckRange(AIndex);

			return Convert.ToBoolean((AValue >> AIndex) & 0x01);
		}

		//----------------------------------------------------------------------
		public static byte Set(byte AValue, int AIndex, bool AActive)
		{
			CheckRange(AIndex);

			return Convert.ToByte(
				(AValue & (~(0x01 << AIndex))) | (Convert.ToByte(AActive) << AIndex));
		}

		//----------------------------------------------------------------------
		public static byte On(byte AValue, int AIndex)
		{
			return Set(AValue, AIndex, true);
		}

		//----------------------------------------------------------------------
		public static byte Off(byte AValue, int AIndex)
		{
			return Set(AValue, AIndex, false);
		}
	}

	//--------------------------------------------------------------------------
	public static class UlBinTrim
	{
		//----------------------------------------------------------------------
		public static byte LoNibble(Object AValue)
		{
			return Convert.ToByte(Convert.ToByte(AValue) & 0x0F);
		}

		//----------------------------------------------------------------------
		public static byte HiNibble(Object AValue)
		{
			return Convert.ToByte(Convert.ToByte(AValue) >> 4);
		}

		//----------------------------------------------------------------------
		public static byte LoByte(Object AValue)
		{
			return Convert.ToByte(Convert.ToUInt16(AValue) & 0xFF);
		}

		//----------------------------------------------------------------------
		public static byte HiByte(Object AValue)
		{
			return Convert.ToByte(Convert.ToUInt16(AValue) >> 8);
		}

		//----------------------------------------------------------------------
		public static UInt16 LoWord(Object AValue)
		{
			return Convert.ToUInt16(Convert.ToUInt32(AValue) & 0xFFFF);
		}

		//----------------------------------------------------------------------
		public static UInt16 HiWord(Object AValue)
		{
			return Convert.ToUInt16(Convert.ToUInt32(AValue) >> 16);
		}

		//----------------------------------------------------------------------
		public static UInt32 LoDWord(Object AValue)
		{
			return Convert.ToUInt32(Convert.ToUInt64(AValue) & 0xFFFFFFFF);
		}

		//----------------------------------------------------------------------
		public static UInt32 HiDWord(Object AValue)
		{
			return Convert.ToUInt32(Convert.ToUInt64(AValue) >> 32);
		}
	}

	//--------------------------------------------------------------------------
	public class UlBinSets
	{
		private byte[] bytes;
		private List<string> tags;

		//----------------------------------------------------------------------
		public UlBinSets(int ACount, string FName = "")
		{
			bytes = new byte[ACount];
			tags = new List<string>();

			Clear();
			if (FName.Trim() != "")
			{
				LoadFromFile(FName);
			}
		}

		//----------------------------------------------------------------------
		public byte[] Bytes
		{ get { return bytes; } }

		//----------------------------------------------------------------------
		public bool this[int AIndex]
		{
			get
			{
				int i = AIndex / 8;
				int j = AIndex % 8;

				CheckRange(i);

				return UlBits.Get(bytes[i], j);
			}
			set
			{
				int i = AIndex / 8;
				int j = AIndex % 8;

				CheckRange(i);

				bytes[i] = UlBits.Set(bytes[i], j, value);
			}
		}

		//----------------------------------------------------------------------
		public bool this[string ATag]
		{
			get
			{
				int AIndex = tags.IndexOf(ATag);

				CheckRange(AIndex);

				return this[AIndex];
			}
			set
			{
				int AIndex = tags.IndexOf(ATag);

				CheckRange(AIndex);

				this[AIndex] = value;
			}
		}

		//----------------------------------------------------------------------
		public int Count
		{
			get
			{
				return bytes.Length;
			}
		}

		//----------------------------------------------------------------------
		public void Clear()
		{
			tags.Clear();
			Array.Clear(bytes, 0, bytes.Length);
		}

		//----------------------------------------------------------------------
		public void LoadFromFile(string AFName)
		{
		}

        //----------------------------------------------------------------------
        public void AddTag(string ATag)
		{
			tags.Add(ATag);

        }

        //----------------------------------------------------------------------
        public void ClearTags()
        {
            tags.Clear();
        }

        //----------------------------------------------------------------------
        public int TagIndex(string ATag)
        {
            return tags.IndexOf(ATag);
        }

        //----------------------------------------------------------------------
        public byte Byte(int AIndex)
		{
			CheckRange(AIndex);

			return bytes[AIndex];
		}

		//----------------------------------------------------------------------
		public void Byte(int AIndex, byte AValue)
		{
			CheckRange(AIndex);

			bytes[AIndex] = AValue;
		}

		//----------------------------------------------------------------------
		public UInt16 Word(int AIndex, EUlEndian AEndian = EUlEndian.Little)
		{
			UInt16 nRet = 0;

			for (int i = 0; i < 2; i++)
			{
				if (AEndian == EUlEndian.Little)
				{
					nRet |= Convert.ToUInt16(Byte(AIndex + i) << (i * 8));
				}
				else
				{
					nRet |= Convert.ToUInt16(Byte(AIndex + i) << (8 - i * 8));
				}
			}

			return nRet;
		}

		//----------------------------------------------------------------------
		public void Word(int AIndex, UInt16 AValue, EUlEndian AEndian = EUlEndian.Little)
		{
			if (AEndian == EUlEndian.Little)
			{
				Byte(AIndex + 0, UlBinTrim.LoByte(AValue));
				Byte(AIndex + 1, UlBinTrim.HiByte(AValue));
			}
			else
			{
				Byte(AIndex + 0, UlBinTrim.HiByte(AValue));
				Byte(AIndex + 1, UlBinTrim.LoByte(AValue));
			}
		}

		//----------------------------------------------------------------------
		public UInt32 DWord(int AIndex, EUlEndian AEndian = EUlEndian.Little)
		{
			UInt32 nRet = 0;

			for (int i = 0; i < 4; i++)
			{
				if (AEndian == EUlEndian.Little)
				{
					nRet |= Convert.ToUInt32((Convert.ToUInt32(Byte(AIndex + i)) << (i * 8)));
				}
				else
				{
					nRet |= Convert.ToUInt32((Convert.ToUInt32((Byte(AIndex + i)) << (24 - i * 8))));
				}
			}

			return nRet;
		}

		//----------------------------------------------------------------------
		public void DWord(int AIndex, UInt32 AValue, EUlEndian AEndian = EUlEndian.Little)
		{
			if (AEndian == EUlEndian.Little)
			{
				Byte(AIndex + 0, UlBinTrim.LoByte(UlBinTrim.LoWord(AValue)));
				Byte(AIndex + 1, UlBinTrim.HiByte(UlBinTrim.LoWord(AValue)));
				Byte(AIndex + 2, UlBinTrim.LoByte(UlBinTrim.HiWord(AValue)));
				Byte(AIndex + 3, UlBinTrim.HiByte(UlBinTrim.HiWord(AValue)));
			}
			else
			{
				Byte(AIndex + 0, UlBinTrim.HiByte(UlBinTrim.HiWord(AValue)));
				Byte(AIndex + 1, UlBinTrim.LoByte(UlBinTrim.HiWord(AValue)));
				Byte(AIndex + 2, UlBinTrim.HiByte(UlBinTrim.LoWord(AValue)));
				Byte(AIndex + 3, UlBinTrim.LoByte(UlBinTrim.LoWord(AValue)));
			}
		}

		//----------------------------------------------------------------------
		public UInt64 DDWord(int AIndex, EUlEndian AEndian = EUlEndian.Little)
		{
			UInt64 nRet = 0;

			for (int i = 0; i < 8; i++)
			{
				if (AEndian == EUlEndian.Little)
				{
					nRet |= Convert.ToUInt64((Convert.ToUInt64(Byte(AIndex + i)) << (i * 8)));
				}
				else
				{
					nRet |= Convert.ToUInt64((Convert.ToUInt64(Byte(AIndex + i)) << (56 - i * 8)));
				}
			}

			return nRet;
		}

		//----------------------------------------------------------------------
		public void DDWord(int AIndex, UInt64 AValue, EUlEndian AEndian = EUlEndian.Little)
		{
			if (AEndian == EUlEndian.Little)
			{
				Byte(AIndex + 0, UlBinTrim.LoByte(UlBinTrim.LoWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 1, UlBinTrim.HiByte(UlBinTrim.LoWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 2, UlBinTrim.LoByte(UlBinTrim.HiWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 3, UlBinTrim.HiByte(UlBinTrim.HiWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 4, UlBinTrim.LoByte(UlBinTrim.LoWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 5, UlBinTrim.HiByte(UlBinTrim.LoWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 6, UlBinTrim.LoByte(UlBinTrim.HiWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 7, UlBinTrim.HiByte(UlBinTrim.HiWord(UlBinTrim.HiDWord(AValue))));
			}
			else
			{
				Byte(AIndex + 0, UlBinTrim.HiByte(UlBinTrim.HiWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 1, UlBinTrim.LoByte(UlBinTrim.HiWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 2, UlBinTrim.HiByte(UlBinTrim.LoWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 3, UlBinTrim.LoByte(UlBinTrim.LoWord(UlBinTrim.HiDWord(AValue))));
				Byte(AIndex + 4, UlBinTrim.HiByte(UlBinTrim.HiWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 5, UlBinTrim.LoByte(UlBinTrim.HiWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 6, UlBinTrim.HiByte(UlBinTrim.LoWord(UlBinTrim.LoDWord(AValue))));
				Byte(AIndex + 7, UlBinTrim.LoByte(UlBinTrim.LoWord(UlBinTrim.LoDWord(AValue))));
			}
		}

		//----------------------------------------------------------------------
		private void CheckRange(int AIndex)
		{
			if (AIndex == -1)
			{
                throw new UlBinaryException("Occurred invalid tag name Exception in UlBinSets", -3);
			}
			else if (AIndex >= bytes.Length)
			{
                throw new UlBinaryException("Occurred over range Exception in UlBinSets", -2);
			}
		}
	}
}
//------------------------------------------------------------------------------
