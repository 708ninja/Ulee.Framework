//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
namespace OxLib.Utils
{
	//--------------------------------------------------------------------------
	public enum EOxEndian { Little, Big }

	//--------------------------------------------------------------------------
	public static class OxBits
	{
		//----------------------------------------------------------------------
		private static void CheckRange(int AIndex)
		{
			if (AIndex >= 8)
			{
				throw new Exception("Occurred Over range Exception in OxBinary");
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
	public static class OxBinTrim
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
	public class OxBinSets
	{
		private byte[] bytes;
		private List<string> tags;

		//----------------------------------------------------------------------
		public OxBinSets(int ACount, string FName = "")
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

				return OxBits.Get(bytes[i], j);
			}
			set
			{
				int i = AIndex / 8;
				int j = AIndex % 8;

				CheckRange(i);

				bytes[i] = OxBits.Set(bytes[i], j, value);
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
		public UInt16 Word(int AIndex, EOxEndian AEndian = EOxEndian.Little)
		{
			UInt16 nRet = 0;

			for (int i = 0; i < 2; i++)
			{
				if (AEndian == EOxEndian.Little)
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
		public void Word(int AIndex, UInt16 AValue, EOxEndian AEndian = EOxEndian.Little)
		{
			if (AEndian == EOxEndian.Little)
			{
				Byte(AIndex + 0, OxBinTrim.LoByte(AValue));
				Byte(AIndex + 1, OxBinTrim.HiByte(AValue));
			}
			else
			{
				Byte(AIndex + 0, OxBinTrim.HiByte(AValue));
				Byte(AIndex + 1, OxBinTrim.LoByte(AValue));
			}
		}

		//----------------------------------------------------------------------
		public UInt32 DWord(int AIndex, EOxEndian AEndian = EOxEndian.Little)
		{
			UInt32 nRet = 0;

			for (int i = 0; i < 4; i++)
			{
				if (AEndian == EOxEndian.Little)
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
		public void DWord(int AIndex, UInt32 AValue, EOxEndian AEndian = EOxEndian.Little)
		{
			if (AEndian == EOxEndian.Little)
			{
				Byte(AIndex + 0, OxBinTrim.LoByte(OxBinTrim.LoWord(AValue)));
				Byte(AIndex + 1, OxBinTrim.HiByte(OxBinTrim.LoWord(AValue)));
				Byte(AIndex + 2, OxBinTrim.LoByte(OxBinTrim.HiWord(AValue)));
				Byte(AIndex + 3, OxBinTrim.HiByte(OxBinTrim.HiWord(AValue)));
			}
			else
			{
				Byte(AIndex + 0, OxBinTrim.HiByte(OxBinTrim.HiWord(AValue)));
				Byte(AIndex + 1, OxBinTrim.LoByte(OxBinTrim.HiWord(AValue)));
				Byte(AIndex + 2, OxBinTrim.HiByte(OxBinTrim.LoWord(AValue)));
				Byte(AIndex + 3, OxBinTrim.LoByte(OxBinTrim.LoWord(AValue)));
			}
		}

		//----------------------------------------------------------------------
		public UInt64 DDWord(int AIndex, EOxEndian AEndian = EOxEndian.Little)
		{
			UInt64 nRet = 0;

			for (int i = 0; i < 8; i++)
			{
				if (AEndian == EOxEndian.Little)
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
		public void DDWord(int AIndex, UInt64 AValue, EOxEndian AEndian = EOxEndian.Little)
		{
			if (AEndian == EOxEndian.Little)
			{
				Byte(AIndex + 0, OxBinTrim.LoByte(OxBinTrim.LoWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 1, OxBinTrim.HiByte(OxBinTrim.LoWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 2, OxBinTrim.LoByte(OxBinTrim.HiWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 3, OxBinTrim.HiByte(OxBinTrim.HiWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 4, OxBinTrim.LoByte(OxBinTrim.LoWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 5, OxBinTrim.HiByte(OxBinTrim.LoWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 6, OxBinTrim.LoByte(OxBinTrim.HiWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 7, OxBinTrim.HiByte(OxBinTrim.HiWord(OxBinTrim.HiDWord(AValue))));
			}
			else
			{
				Byte(AIndex + 0, OxBinTrim.HiByte(OxBinTrim.HiWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 1, OxBinTrim.LoByte(OxBinTrim.HiWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 2, OxBinTrim.HiByte(OxBinTrim.LoWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 3, OxBinTrim.LoByte(OxBinTrim.LoWord(OxBinTrim.HiDWord(AValue))));
				Byte(AIndex + 4, OxBinTrim.HiByte(OxBinTrim.HiWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 5, OxBinTrim.LoByte(OxBinTrim.HiWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 6, OxBinTrim.HiByte(OxBinTrim.LoWord(OxBinTrim.LoDWord(AValue))));
				Byte(AIndex + 7, OxBinTrim.LoByte(OxBinTrim.LoWord(OxBinTrim.LoDWord(AValue))));
			}
		}

		//----------------------------------------------------------------------
		private void CheckRange(int AIndex)
		{
			if (AIndex == -1)
			{
				throw new Exception("Occurred invalid tag name Exception in OxBinSets");
			}
			else if (AIndex >= bytes.Length)
			{
				throw new Exception("Occurred over range Exception in OxBinSets");
			}
		}
	}
}
//------------------------------------------------------------------------------
