//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : MELSEC-Q PLC Ethernet 4E Protocol Connection Class
//------------------------------------------------------------------------------
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Generic;

using Ulee.Utils;
using Ulee.Threading;

//------------------------------------------------------------------------------
namespace Ulee.Device.Connection.Melsec
{
	public enum EUlProtocol { Tcp, Udp }

	//--------------------------------------------------------------------------
	public class UlMelsecQException : ApplicationException
	{
		public UlMelsecQException()
			: base("Occurred Melsec-Q PLC Exception!")
		{
		}

		public UlMelsecQException(string aMsg)
			: base(aMsg)
		{
		}
	}

	//--------------------------------------------------------------------------
	public class UlMelsecQSerialNoException : UlMelsecQException
	{
		public UlMelsecQSerialNoException()
			: base("Occurred invalid serial no Exception!")
		{
		}

		public UlMelsecQSerialNoException(string aMsg)
			: base(aMsg)
		{
		}
	}

	//--------------------------------------------------------------------------
	public class UlMelsecQTerminalCodeException : UlMelsecQException
	{
		public UlMelsecQTerminalCodeException()
			: base("Occurred terminal code Exception!")
		{
		}

		public UlMelsecQTerminalCodeException(string aMsg)
			: base(aMsg)
		{
		}
	}

	//--------------------------------------------------------------------------
	public class UlMelsecQTimeoutException : UlMelsecQException
	{
		public UlMelsecQTimeoutException()
			: base("Occurred Melsec-Q PLC communication time out Exception!")
		{
		}

		public UlMelsecQTimeoutException(string aMsg)
			: base(aMsg)
		{
		}
	}

	//--------------------------------------------------------------------------
	public abstract class UlQnA4EPacket
	{
		protected UlBinSets packet;

		// 고정값 - 0x0054 고정
		public UInt16 Head1
		{ get { return packet.Word(0); } set { packet.Word(0, value); } }

		// 시리얼번호 - 송신측에서 Packet 구분을 위해서 지정하는 임의의 번호
		public UInt16 Serial
		{ get { return packet.Word(2); } set { packet.Word(2, value); } }

		// 고정값 - 0x0000 고정
		public UInt16 Head2
		{ get { return packet.Word(4); } set { packet.Word(4, value); } }

		// Network No
		public byte Network
		{ get { return packet.Byte(6); } set { packet.Byte(6, value); } }

		// PLC No - 0xFF 고정
		public byte Plc
		{ get { return packet.Byte(7); } set { packet.Byte(7, value); } }

		// 요구 상대 모듈 I/O번호
		public UInt16 IO
		{ get { return packet.Word(8); } set { packet.Word(8, value); } }

		// 요구 상대 모듈 국번호
		public byte Station
		{ get { return packet.Byte(10); } set { packet.Byte(10, value); } }

		// 데이터 길이
		public UInt16 Length
		{ get { return packet.Word(11); } set { packet.Word(11, value); } }

		public byte[] Bytes
		{ get { return packet.Bytes; } }

		public int Count
		{ get { return packet.Count; } }

		public UlQnA4EPacket(int aCount)
		{
			packet = new UlBinSets(aCount);
		}
	}

	//--------------------------------------------------------------------------
	public class UlQnA4ESendPacket : UlQnA4EPacket
	{
		// Cpu 감시 타이머
		public UInt16 CpuWatchDog
		{ get { return packet.Word(13); } set { packet.Word(13, value); } }

		// Main Command
		public UInt16 MainCmd
		{ get { return packet.Word(15); } set { packet.Word(15, value); } }

		// Sub Command
		public UInt16 SubCmd
		{ get { return packet.Word(17); } set { packet.Word(17, value); } }

		public void Byte(int aIndex, byte aValue)
		{
			packet.Byte(aIndex + 19, aValue);
		}

		public void Word(int aIndex, UInt16 aValue)
		{
			packet.Word(aIndex + 19, aValue);
		}

		public void DWord(int aIndex, UInt32 aValue)
		{
			packet.DWord(aIndex + 19, aValue);
		}

		public UlQnA4ESendPacket(int aCount)
			: base(aCount)
		{
			Clear();
		}

		public void Clear()
		{
			packet.Clear();

			Head1 = 0x0054;
			Serial = 0x0000;
			Head2 = 0x0000;
			Network = 0x00;
			Plc = 0xFF;
			IO = 0x03FF;
			Station = 0x00;
			Length = 0x00;
			CpuWatchDog = 0x0010;
			MainCmd = 0x0000;
			SubCmd = 0x0000;
		}
	}

	//--------------------------------------------------------------------------
	public class UlQnA4ERecvPacket : UlQnA4EPacket
	{
		// 종료 코드
		public UInt16 Return
		{ get { return packet.Word(13); } set { packet.Word(13, value); } }

		//----------------------------------------------------------------------
		public UlQnA4ERecvPacket(int aCount)
			: base(aCount)
		{
			Clear();
		}

		//----------------------------------------------------------------------
		public void Clear()
		{
			packet.Clear();
		}

		//----------------------------------------------------------------------
		public bool Bit(int aIndex)
		{
			byte byValue = packet.Byte(aIndex / 2 + 15);

			if ((aIndex % 2) == 0)
			{
				byValue = UlBinTrim.HiNibble(byValue);
			}
			else
			{
				byValue = UlBinTrim.LoNibble(byValue);
			}

			return (byValue == 0) ? false : true;
		}

		//----------------------------------------------------------------------
		public byte Byte(int aIndex)
		{
			return packet.Byte(aIndex + 15);
		}

		//----------------------------------------------------------------------
		public UInt16 Word(int aIndex)
		{
			return packet.Word(aIndex + 15);
		}

		//----------------------------------------------------------------------
		public UInt32 DWord(int aIndex)
		{
			return packet.DWord(aIndex + 15);
		}
	}

	//--------------------------------------------------------------------------
	public enum EMQLog
	{ None, Event, All }

	//--------------------------------------------------------------------------
	// Define PLC Device Code
	public enum EMQDevice : byte
	{
		X = 0x9C,
		Y = 0x9D,
		M = 0x90,
		L = 0x92,
		S = 0x98,
		B = 0xA0,
		F = 0x93,
		TS = 0xC1,
		TC = 0xC0,
		TN = 0xC2,
		CS = 0xC4,
		CC = 0xC3,
		CN = 0xC5,
		D = 0xA8,
		W = 0xB4,
		R = 0xAF,
		ZR = 0xB0
	}

	//--------------------------------------------------------------------------
	public class UlMelsecQClient
	{
		protected const int csPortCount = 1;
		protected const int csRetryCount = 3;
		protected const int csPacketSize = 4096;
		protected const int csSendHeadSize = 19;
		protected const int csTimeout = 1000;

		// Main Command
		private enum EMQMainCmd : ushort
		{
			 BatchRead = 0x0401,
			 BatchWrite = 0x1401,
			 RandomRead = 0x0403,
			 RandomWrite = 0x1402,
			 MonitorRead = 0x0802,
			 MonitorWrite = 0x0801,
			 BlockRead = 0x0406,
			 BlockWrite = 0x1406,
			 Run = 1001,
			 Stop = 1002,
			 Pause = 1003,
			 LatchClear = 1005,
			 Reset = 1006,
			 CpuNoRead = 0101
		}

		// Sub Command
		private enum EMQSubCmd : ushort
		{
			Bit = 0x0001,
			Word = 0x0000,
			Normal = 0x0000,
			Monitor = 0x0040,
			NormalBit = Normal | Bit,
			NormalWord = Normal | Word,
			MonitorBit = Monitor | Bit,
			MonitorWord = Monitor | Word
		}

        // Logger 속성 구분자
		private enum EMQLogID
		{ Connect, Disconnect, Send, Recieve, Note, Exception }

		private string str;

		private bool connected;
		// 송수신 Protocol - Tcp, Udp
		private EUlProtocol protocol;
		// 송수신 IP
		private string ip;
		// 송수신 Port
		private int port;
		// 송수신 Port수
		private int portCount;
		// Tcp 사용시 현재 사용중인 Tcp 객체번호
		private int index;
		// 송수신 Packet 크기
		private int packetSize;

		private Random rand;
		private Stopwatch stopWatch;
		private NetworkStream netStream;
		private UlLogger logger;
		private EMQLog logging;

		private UdpClient udp;
		private List<TcpClient> tcpList;

		// Melsec-Q PLC QnA 호환 4E Frame 송신Packet
		private UlQnA4ESendPacket sPacket;
		// Melsec-Q PLC QnA 호환 4E Frame 수신Packet
		private UlQnA4ERecvPacket rPacket;

		private UInt16 serial;
		private byte network;
		private byte plc;
		private byte station;

		public delegate void MelsecQClientEHandler(bool aActive);
		
		// 송신 Event Handler
		public event MelsecQClientEHandler SendingEvent;
		protected void OnSendingEvent(bool aActive)
		{
			if (SendingEvent != null)
			{
				SendingEvent(aActive);
			}
		}
		
		// 수신 Event Handler
        public event MelsecQClientEHandler ReceivingEvent;
		protected void OnReceivingEvent(bool aActive)
		{
			if (ReceivingEvent != null)
			{
				ReceivingEvent(aActive);
			}
		}

		public bool Connected
		{ get { return connected; } }

		// Log 파일기록여부
		public EMQLog Logging
		{ get { return logging;  } set { logging = value; } }
		// Log 파일경로
		public string LogPath
		{ get { return logger.Path; } set { logger.Path = value; } }
		// Log 파일명
		public string LogFName
		{ get { return logger.FName; } set { logger.FName = value; } }
		// Log 파일명
		public string LogExt
		{ get { return logger.Ext; } set { logger.Ext = value; } }
		// Log 파일 Encoding 
		public Encoding LogFEncoding
		{ get { return logger.FEncoding; } set { logger.FEncoding = value; } }
		// Log 파일 분리 기준 - Min, Hour, Day
		public EUlLogFileSeperation LogFSeperation
		{ get { return logger.FSeperation; } set { logger.FSeperation = value; } }

		// Melsec Network 번호 - 0x00
		public byte Network
		{ get { return network; } set { network = value; } }
		// Melsec Plc 번호 - 0xFF
		public byte Plc
		{ get { return plc; } set { plc = value; } }
		// Melsec Ethernet Unit 국번호 - 0x00
		public byte Station
		{ get { return station; } set { station = value; } }
        
        // 송신 Data Setting
        public void Byte(int aIndex, byte aValue)
        { sPacket.Byte(aIndex, aValue); }
        public void Word(int aIndex, UInt16 aValue)
        { sPacket.Word(aIndex, aValue); }
        public void DWord(int aIndex, UInt32 aValue)
        { sPacket.DWord(aIndex, aValue); }

        // 수신 Data 확인
		public bool Bit(int aIndex)
		{ return rPacket.Bit(aIndex); }
		public byte Byte(int aIndex)
		{ return rPacket.Byte(aIndex); }
		public UInt16 Word(int aIndex)
		{ return rPacket.Word(aIndex); }
		public UInt32 DWord(int aIndex)
		{ return rPacket.DWord(aIndex); }

		//----------------------------------------------------------------------
		public UlMelsecQClient(
			string aIP, 
			int aPort,
			EUlProtocol aProtocol=EUlProtocol.Tcp, 
			int aPortCount=csPortCount, 
			int aPacketSize=csPacketSize)
		{
			ip = aIP;
			port = aPort;
			protocol = aProtocol;
			portCount = aPortCount;
			packetSize = aPacketSize;

			index = 0;
			connected = false;

			SendingEvent = null;
			ReceivingEvent = null;

			network = 0x00;
			plc = 0xFF;
			station = 0x00;

			rand = new Random();
			stopWatch = new Stopwatch();

			logging = EMQLog.Event;
			logger = new UlLogger();
			logger.Active = true;
			logger.Path = "";
			logger.FName = "MelsecQLog";
			logger.AddHead("CONNECT");
			logger.AddHead("DISCONNECT");
			logger.AddHead("PC->PLC");
			logger.AddHead("PC<-PLC");
			logger.AddHead("COMMENT");
			logger.AddHead("EXCEPTION");

			if (protocol == EUlProtocol.Tcp)
			{
				tcpList = new List<TcpClient>();
			
				for (int i = 0; i < portCount; i++)
				{
					tcpList.Add(new TcpClient());
				}
			}
			else
			{
				udp = new UdpClient();
			}

			// 송수신 Packet 생성
			sPacket = new UlQnA4ESendPacket(packetSize);
			rPacket = new UlQnA4ERecvPacket(packetSize);
		}

		//----------------------------------------------------------------------
		public void Open()
		{
            int nErrCount = 0;

            if (protocol == EUlProtocol.Tcp)
            {
                for (int i = 0; i < tcpList.Count; i++)
                {
                    try
                    {
                        str = string.Format("TCP IP : {0}, Port : {1}", ip, port + i);
                        LogEvent(EMQLogID.Connect, str);

                        tcpList[i].Connect(ip, port + i);
                    }
                    catch (Exception)
                    {
                        str = string.Format("Failed conection TCP IP : {0}, Port : {1}", ip, port + i);
						LogEvent(EMQLogID.Note, str);
                        nErrCount++;
                    }
                }

                for (int i = 0; i < tcpList.Count; i++)
                {
                    if (tcpList[i].Connected == true)
                    {
						connected = true;
                        break;
                    }
                    else
                    {
                        index++;
                    }
                }

                if (nErrCount == tcpList.Count)
                {
                    str = "Occurred All TCP connection fail exception in UlMelsecQ.Open";
					LogEvent(EMQLogID.Exception, str);
                    throw new Exception(str);
                }
            }
            else
            {
				try
				{
					str = string.Format("UDP IP : {0}, Port : {1}", ip, port);
					LogEvent(EMQLogID.Connect, str);

					udp.Connect(ip, port);
					connected = true;
				}
				catch (Exception)
				{
					str = string.Format("Failed conection UDP IP : {0}, Port : {1}", ip, port);
					LogEvent(EMQLogID.Note, str);
					throw new Exception(str);
				}
            }
		}

		//----------------------------------------------------------------------
		public void Close()
		{
            if (protocol == EUlProtocol.Tcp)
            {
                for (int i = 0; i < tcpList.Count; i++)
                {
					if (tcpList[i].Connected == true)
					{
						str = string.Format("TCP IP : {0}, Port : {1}", ip, port + i);
						LogEvent(EMQLogID.Disconnect, str);

						tcpList[i].Close();
					}
                }
            }
            else
            {
                udp.Close();
            }

			connected = false;
		}

		//----------------------------------------------------------------------
		private UInt16 GenSerial()
		{
			return (UInt16) rand.Next(0xFFFF);
		}

		//----------------------------------------------------------------------
		private void LogEvent(EMQLogID aIndex, string aStr)
		{
			if (logging != EMQLog.None)
			{
				logger.Log((int)aIndex, aStr);
			}
		}

		//----------------------------------------------------------------------
		private void LogPacket(EMQLogID aIndex, int aCount)
		{
			byte[] thePacket;

			if (logging == EMQLog.All)
			{
				if (aIndex == EMQLogID.Send)
				{
					thePacket = sPacket.Bytes;
				}
				else
				{
					thePacket = rPacket.Bytes;
				}

				str = "";
				for (int i = 0; i < aCount; i++)
				{
					str += string.Format("{0:X02} ", thePacket[i]);
				}

				LogEvent(aIndex, str);
			}
		}

		//----------------------------------------------------------------------
		private void Send(int aCount)
		{
			switch (protocol)
			{
				case EUlProtocol.Tcp:
					SendTcp(aCount);
					break;

				case EUlProtocol.Udp:
					SendUdp(aCount);
					break;
			}
		}

		//----------------------------------------------------------------------
		private void SendTcp(int aCount)
		{
			for (int i = 0; i < tcpList.Count; i++)
			{
				for (int j = 0; j < csRetryCount; j++)
				{
					OnSendingEvent(true);

					try
					{
						SendTcpPacket(aCount);
						RecieveTcp();
						return;
					}
					catch (UlMelsecQTimeoutException)
					{
					}
					catch (UlMelsecQException)
					{
						break;
					}
					finally
					{
						OnSendingEvent(false);
					}
				}

				Thread.Sleep(1000);
				index++;

				if (index < tcpList.Count)
				{
					str = string.Format(
						"Changed TCP Port {0} -> {1} in UlMelsecQ.SendTcp",
						port + index - 1, port + index);
					LogEvent(EMQLogID.Note, str);
					LogEvent(EMQLogID.Note, "Retry sending packet in UlMelsecQ.SendTcp");
				}
			}

			LogEvent(EMQLogID.Exception, 
				"Occurred TCP communication error in UlMelsecQ.SendTcp");

			throw new UlMelsecQException(str);
		}

		//----------------------------------------------------------------------
		private void SendTcpPacket(int aCount)
		{
			try
			{
				LogPacket(EMQLogID.Send, aCount);

				netStream = tcpList[index].GetStream();
				netStream.Write(sPacket.Bytes, 0, aCount);
				netStream.Flush();
			}
			catch (Exception)
			{
				str = "Occurred TCP sending error in UlMelsecQ.SendTcpPacket";
				LogEvent(EMQLogID.Exception, str);

				throw new UlMelsecQException(str);
			}
		}

		//----------------------------------------------------------------------
		private void SendUdp(int aCount)
		{
			for (int i = 0; i < csRetryCount; i++)
			{
				OnSendingEvent(true);

				try
				{
					SendUdpPacket(aCount);
					RecieveUdp();
					return;
				}
				catch (UlMelsecQTimeoutException)
				{
				}
				catch (UlMelsecQException)
				{
					break;
				}
				finally
				{
					OnSendingEvent(false);
				}
			}

			LogEvent(EMQLogID.Exception,
				"Occurred UDP communication error in UlMelsecQ.SendUdp");
			throw new UlMelsecQException(str);
		}

        //----------------------------------------------------------------------
        private void SendUdpPacket(int aCount)
        {
            try
            {
                LogPacket(EMQLogID.Send, aCount);
                udp.Send(sPacket.Bytes, aCount);
            }
            catch (Exception)
            {
                str = "Occurred UDP sending error in UlMelsecQ.SendUdpPacket";
				LogEvent(EMQLogID.Exception, str);

                throw new UlMelsecQException(str);
            }
        }

        //----------------------------------------------------------------------
		private void RecieveTcp()
		{
			lbRetry:
			try
			{
				netStream = tcpList[index].GetStream();
			}
			catch (Exception)
			{
				LogEvent(EMQLogID.Exception, 
					"Occurred TCP stream creation error in UlMelsecQ.RecieveTcp");
				throw new UlMelsecQException();
			}

			stopWatch.Restart();
			OnReceivingEvent(true);

			try
			{
				// 지정된 시간동안 수신을 대기
				while (netStream.DataAvailable == false)
				{
					// 수신 대기시간이 초과 되었나?
					if (stopWatch.ElapsedMilliseconds > csTimeout)
					{
						LogEvent(EMQLogID.Exception,
							"Occurred TCP recieve time out error in UlMelsecQ.RecieveTcp");
						throw new UlMelsecQTimeoutException();
					}

					Thread.Sleep(10);
				}

				// 수신 데이터의 byte 개수
				int nLen = tcpList[index].Available;

				try
				{
					// 수신 Packet을 최기화
					rPacket.Clear();

					// 수신 데이터를 읽는다
					netStream.Read(rPacket.Bytes, 0, nLen);

					LogPacket(EMQLogID.Recieve, nLen);
				}
				catch (Exception)
				{
					LogEvent(EMQLogID.Exception,
						"Occurred TCP stream reading error in UlMelsecQ.RecieveTcp");
					throw new UlMelsecQException();
				}

				try
				{
					// 올바른 수신 데이터 인지 확인
					Validate();
				}
				catch (UlMelsecQException)
				{
					goto lbRetry;
				}
			}
			finally
			{
				OnReceivingEvent(false);
			}
		}

		//----------------------------------------------------------------------
		private void RecieveUdp()
		{
			byte[] theBytes;
			IPEndPoint theIpEnd = new IPEndPoint(IPAddress.Any, 0);

			stopWatch.Restart();

			lbRetry:
			OnReceivingEvent(true);

			try
			{
				// 지정된 시간동안 수신을 대기
				while (udp.Available == 0)
				{
					// 수신 대기시간이 초과 되었나?
					if (stopWatch.ElapsedMilliseconds > csTimeout)
					{
						LogEvent(EMQLogID.Exception,
							"Occurred UDP recieve time out error in UlMelsecQ.RecieveUdp");
						throw new UlMelsecQTimeoutException();
					}

					Thread.Sleep(10);
				}

				try
				{
					theBytes = udp.Receive(ref theIpEnd);
				}
				catch (Exception)
				{
					LogEvent(EMQLogID.Exception,
						"Occurred UDP reading error in UlMelsecQ.RecieveUdp");
					throw new UlMelsecQException();
				}

				// 수신 Packet을 최기화
				rPacket.Clear();
				// 수신 Packet으로 수신 Data 복사
				Array.Copy(theBytes, 0, rPacket.Bytes, 0, theBytes.Length);

				LogPacket(EMQLogID.Recieve, theBytes.Length);

				try
				{
					// 올바른 수신 데이터 인지 확인
					Validate();
				}
				catch (UlMelsecQException)
				{
					stopWatch.Restart();
					goto lbRetry;
				}
			}
			finally
			{
				OnReceivingEvent(false);
			}
        }

		//----------------------------------------------------------------------
		private void Validate()
		{
			// 송수신 Serial No가 같지 않은가?
			if (rPacket.Serial != sPacket.Serial)
			{
				LogEvent(EMQLogID.Exception, 
					"Occurred mismatch serial no error in UlMelsecQ.Validate");
	
				throw new UlMelsecQSerialNoException();
			}

			// 종료코드가 정상이 아닌가?
			if (rPacket.Return != 0)
			{
				str = string.Format(
					"Occurred abnormal terminal code #{0:X04} in UlMelsecQ.Validate",
					rPacket.Return);
				LogEvent(EMQLogID.Exception, str);

				throw new UlMelsecQTerminalCodeException(str);
			}
		}

		//----------------------------------------------------------------------
		public void SetDevice(int aIndex, EMQDevice aDevice, UInt32 aAddr)
		{
			sPacket.DWord(aIndex, aAddr); 
			sPacket.Byte(aIndex + 3, (byte)aDevice);
		}

		//----------------------------------------------------------------------
		public void SetDevice(int aIndex, EMQDevice aDevice, UInt32 aAddr, bool aActive)
		{
			byte byActive = (byte) ((aActive == false) ? 0x00 : 0x01);

			SetDevice(aIndex, aDevice, aAddr);
			sPacket.Byte(aIndex + 4, byActive);
		}

        //----------------------------------------------------------------------
        public void Run()
        {
            serial = GenSerial();

            lock (sPacket)
            {
                sPacket.Clear();

                // Header부 Setting
                sPacket.Head1 = 0x0054;
                sPacket.Serial = serial;
                sPacket.Head2 = 0x0000;
                sPacket.Network = network;
                sPacket.IO = 0x03FF;
                sPacket.Plc = plc;
                sPacket.Station = station;
                sPacket.Length = 10;
                sPacket.CpuWatchDog = 0x0010;

                // 주명령 PLC Run
                sPacket.MainCmd = (UInt16) EMQMainCmd.Run;
                // 부명령
                sPacket.SubCmd = 0x0000;
                // 강제명령실행 - false
                sPacket.Word(0, 0x0001);
                // Clear Mode 설정 - 모든 메모리 선택
                sPacket.Word(0, 0x0002);

                // 명령 Packet 전송
                Send(csSendHeadSize + 4);
            }
        }

        //----------------------------------------------------------------------
        public void Stop()
        {
            serial = GenSerial();

            lock (sPacket)
            {
                sPacket.Clear();

                // Header부 Setting
                sPacket.Head1 = 0x0054;
                sPacket.Serial = serial;
                sPacket.Head2 = 0x0000;
                sPacket.Network = network;
                sPacket.IO = 0x03FF;
                sPacket.Plc = plc;
                sPacket.Station = station;
                sPacket.Length = 8;
                sPacket.CpuWatchDog = 0x0010;

                // 주명령 PLC Stop
                sPacket.MainCmd = (UInt16) EMQMainCmd.Stop;
                // 부명령
                sPacket.SubCmd = 0x0000;
                // 강제명령실행 - false
                sPacket.Word(0, 0x0001);

                // 명령 Packet 전송
                Send(csSendHeadSize + 2);
            }
        }

        //----------------------------------------------------------------------
        public void Pause()
        {
            serial = GenSerial();

            lock (sPacket)
            {
                sPacket.Clear();

                // Header부 Setting
                sPacket.Head1 = 0x0054;
                sPacket.Serial = serial;
                sPacket.Head2 = 0x0000;
                sPacket.Network = network;
                sPacket.IO = 0x03FF;
                sPacket.Plc = plc;
                sPacket.Station = station;
                sPacket.Length = 8;
                sPacket.CpuWatchDog = 0x0010;

                // 주명령 PLC Pause
                sPacket.MainCmd = (UInt16) EMQMainCmd.Pause;
                // 부명령
                sPacket.SubCmd = 0x0000;
                // 강제명령실행 - false
                sPacket.Word(0, 0x0001);

                // 명령 Packet 전송
                Send(csSendHeadSize + 2);
            }
        }

        //----------------------------------------------------------------------
        public void Reset()
        {
            serial = GenSerial();

            lock (sPacket)
            {
                sPacket.Clear();

                // Header부 Setting
                sPacket.Head1 = 0x0054;
                sPacket.Serial = serial;
                sPacket.Head2 = 0x0000;
                sPacket.Network = network;
                sPacket.IO = 0x03FF;
                sPacket.Plc = plc;
                sPacket.Station = station;
                sPacket.Length = 8;
                sPacket.CpuWatchDog = 0x0010;

                // 주명령 PLC Reset
                sPacket.MainCmd = (UInt16) EMQMainCmd.Reset;
                // 부명령
                sPacket.SubCmd = 0x0000;
                // 강제명령실행 - false
                sPacket.Word(0, 0x0001);

                // 명령 Packet 전송
                Send(csSendHeadSize + 2);
            }
        }

        //----------------------------------------------------------------------
        public string GetCpuNo()
        {
            serial = GenSerial();

            lock (sPacket)
            {
                sPacket.Clear();

                // Header부 Setting
                sPacket.Head1 = 0x0054;
                sPacket.Serial = serial;
                sPacket.Head2 = 0x0000;
                sPacket.Network = network;
                sPacket.IO = 0x03FF;
                sPacket.Plc = plc;
                sPacket.Station = station;
                sPacket.Length = 8;
                sPacket.CpuWatchDog = 0x0010;

                // 주명령 CPU 번호 읽기
                sPacket.MainCmd = (UInt16) EMQMainCmd.CpuNoRead;
                // 부명령
                sPacket.SubCmd = 0x0000;
                // 강제명령실행 - false
                sPacket.Word(0, 0x0001);

                // 명령 Packet 전송
                Send(csSendHeadSize + 2);

                // 수신 Data String 변환
                str = Encoding.ASCII.GetString(rPacket.Bytes, 15, 16).Trim();
            }

            return str;
        }

        //----------------------------------------------------------------------
		public void GetBit(EMQDevice aDevice, UInt32 aAddr, UInt16 aCount=1)
		{
            if (aCount < 1)
			{
				str = "Occurred 'aCount' is low error in UlMelsecQ.GetBit";
				LogEvent(EMQLogID.Exception, str);
				throw new UlMelsecQException(str);
			}

			serial = GenSerial();

			lock (sPacket)
			{
				sPacket.Clear();

				// Header부 Setting
				sPacket.Head1 = 0x0054;
				sPacket.Serial = serial;
				sPacket.Head2 = 0x0000;
				sPacket.Network = network;
				sPacket.IO = 0x03FF;
				sPacket.Plc = plc;
				sPacket.Station = station;
				sPacket.Length = 12;
				sPacket.CpuWatchDog = 0x0010;

				// 주명령 일괄읽기
				sPacket.MainCmd = (UInt16) EMQMainCmd.BatchRead;
				// 부명령 일반Bit단위
				sPacket.SubCmd = (UInt16) EMQSubCmd.NormalBit;
				// 시작 Address
				sPacket.DWord(0, aAddr);
				// PLC 내부 접점 Device - X, Y, D, M, ...
				sPacket.Byte(3, (byte) aDevice);
				// 읽을 개수
				sPacket.Word(4, aCount);

				// 명령 Packet 전송
				Send(csSendHeadSize + 6);
			}
		}

		//----------------------------------------------------------------------
		public void SetBit(EMQDevice aDevice, UInt32 aAddr, bool aActive)
		{
			byte byActive = (byte) ((aActive == false) ? 0x00 : 0x10);

			serial = GenSerial();

			lock (sPacket)
			{
				sPacket.Clear();

				// Header부 Setting
				sPacket.Head1 = 0x0054;
				sPacket.Serial = serial;
				sPacket.Head2 = 0x0000;
				sPacket.Network = network;
				sPacket.IO = 0x03FF;
				sPacket.Plc = plc;
				sPacket.Station = station;
				sPacket.Length = 13;
				sPacket.CpuWatchDog = 0x0010;

				// 주명령 일괄쓰기
				sPacket.MainCmd = (UInt16) EMQMainCmd.BatchWrite;
				// 부명령 일반Bit단위
				sPacket.SubCmd = (UInt16) EMQSubCmd.NormalBit;
				// 시작 Address
				sPacket.DWord(0, aAddr);
				// PLC 내부 접점 Device - X, Y, D, M, ...
				sPacket.Byte(3, (byte) aDevice);
				// 쓰기 개수
				sPacket.Word(4, 1);
				// 쓰기 Data
				sPacket.Byte(6, byActive);

				// 명령 Packet 전송
				Send(csSendHeadSize + 7);
			}
		}

		//----------------------------------------------------------------------
		public void GetWord(EMQDevice aDevice, UInt32 aAddr, UInt16 aCount=1)
		{
			if (aCount < 1)
			{
				str = "Occurred 'aCount' is low error in UlMelsecQ.GetWord";
				LogEvent(EMQLogID.Exception, str);
				throw new UlMelsecQException(str);
			}

			serial = GenSerial();

			lock (sPacket)
			{
				sPacket.Clear();

				// Header부 Setting
				sPacket.Head1 = 0x0054;
				sPacket.Serial = serial;
				sPacket.Head2 = 0x0000;
				sPacket.Network = network;
				sPacket.IO = 0x03FF;
				sPacket.Plc = plc;
				sPacket.Station = station;
				sPacket.Length = 12;
				sPacket.CpuWatchDog = 0x0010;

				// 주명령 일괄읽기
				sPacket.MainCmd = (UInt16) EMQMainCmd.BatchRead;
				// 부명령 일반Word단위
				sPacket.SubCmd = (UInt16) EMQSubCmd.NormalWord;
				// 시작 Address
				sPacket.DWord(0, aAddr);
				// PLC 내부 접점 Device - X, Y, D, M, ...
				sPacket.Byte(3, (byte) aDevice);
				// 읽기 개수
				sPacket.Word(4, aCount);

				// 명령 Packet 전송
				Send(csSendHeadSize + 6);
			}
		}

		//----------------------------------------------------------------------
		public void SetWord(EMQDevice aDevice, UInt32 aAddr, UInt16 aValue)
		{
			serial = GenSerial();

			lock (sPacket)
			{
				sPacket.Clear();

				// Header부 Setting
				sPacket.Head1 = 0x0054;
				sPacket.Serial = serial;
				sPacket.Head2 = 0x0000;
				sPacket.Network = network;
				sPacket.IO = 0x03FF;
				sPacket.Plc = plc;
				sPacket.Station = station;
				sPacket.Length = 14;
				sPacket.CpuWatchDog = 0x0010;

				// 주명령 일괄쓰기
				sPacket.MainCmd = (UInt16) EMQMainCmd.BatchWrite;
				// 부명령 일반Word단위
				sPacket.SubCmd = (UInt16) EMQSubCmd.NormalWord;
				// 시작 Address
				sPacket.DWord(0, aAddr);
				// PLC 내부 접점 Device - X, Y, D, M, ...
				sPacket.Byte(3, (byte) aDevice);
				// 쓰기 개수
				sPacket.Word(4, 1);
				// 쓰기 Data
				sPacket.Word(6, aValue);

				// 명령 Packet 전송
				Send(csSendHeadSize + 8);
			}
		}

		//----------------------------------------------------------------------
		public void SetWord(EMQDevice aDevice, UInt32 aAddr, UInt16[] aValue)
		{
			UInt16 nCount = (UInt16) aValue.Length;

			serial = GenSerial();

			lock (sPacket)
			{
				sPacket.Clear();

				// Header부 Setting
				sPacket.Head1 = 0x0054;
				sPacket.Serial = serial;
				sPacket.Head2 = 0x0000;
				sPacket.Network = network;
				sPacket.IO = 0x03FF;
				sPacket.Plc = plc;
				sPacket.Station = station;
				sPacket.Length = Convert.ToUInt16(nCount * 2 + 12);
				sPacket.CpuWatchDog = 0x0010;

				// 주명령 일괄쓰기
				sPacket.MainCmd = (UInt16) EMQMainCmd.BatchWrite;
				// 부명령 일반Word단위
				sPacket.SubCmd = (UInt16) EMQSubCmd.NormalWord;
				// 시작 Address
				sPacket.DWord(0, aAddr);
				// PLC 내부 접점 Device - X, Y, D, M, ...
				sPacket.Byte(3, (byte) aDevice);
				// 쓰기 개수
				sPacket.Word(4, nCount);

				// 쓰기 Data
				for (int i = 0; i < nCount; i++)
				{
					sPacket.Word(i * 2 + 6, aValue[i]);
				}

				// 명령 Packet 전송
				Send(csSendHeadSize + 6 + nCount * 2);
			}
		}
	}

	//--------------------------------------------------------------------------
	public class UlMelsecQScaner
	{
		// 이전 PLC Device Data 값
		protected UlBinSets oldSets;
		// 현재 PLC Device Data 값
		protected UlBinSets newSets;
		// Melsec-Q PLC Ethernet Client
		private UlMelsecQClient client;
		// 주기적으로 PLC Data 읽는 Thread
		protected UlMelsecQScanThread thread;

		public UlMelsecQClient Client
		{ get { return client; } }

		public UlMelsecQScanThread.MelsecQScanHandler ScanningEvent
		{ set { thread.ScanningEvent += value; } }

		public UlMelsecQScanThread.MelsecQScanHandler AfterScanEvent
		{ set { thread.AfterScanEvent += value; } }

		//----------------------------------------------------------------------
        public UlMelsecQScaner(
			UlMelsecQClient aClient, int aCount, long aCycleTime)
        {
			client = aClient;

			// PLC Device 접점 저장소 할당
			oldSets = new UlBinSets(aCount);
			newSets = new UlBinSets(aCount);

			// 주기적으로 PLC Data 읽는 Thread 생성
			thread = new UlMelsecQScanThread(this, aCycleTime);
			thread.Resume();
		}

		//----------------------------------------------------------------------
		public void Close()
		{
			thread.Terminate();
		}

		//----------------------------------------------------------------------
		public void Resume()
		{
			thread.Resume();
		}

		//----------------------------------------------------------------------
		public void Suspend()
		{
			thread.Suspend();
		}

		//----------------------------------------------------------------------
        public void Byte(int aIndex, byte aValue)
        {
            lock (this)
            {
                oldSets.Byte(aIndex, newSets.Byte(aIndex));
                newSets.Byte(aIndex, aValue);
            }
        }

		//----------------------------------------------------------------------
		public void Byte(byte[] aValue)
		{
			if (aValue.Length != newSets.Count)
			{
				throw new UlMelsecQException(
					"Occurred array length mismatch error in UlMelsecQDevice.Copy");
			}

			lock (this)
			{
				Array.Copy(newSets.Bytes, 0, oldSets.Bytes, 0, newSets.Count);
				Array.Copy(aValue, 0, newSets.Bytes, 0, aValue.Length);
			}
		}

        //----------------------------------------------------------------------
        public void Word(int aIndex, UInt16 aValue)
        {
            lock (this)
            {
                oldSets.Word(aIndex, newSets.Word(aIndex));
                newSets.Word(aIndex, aValue);
            }
        }

        //----------------------------------------------------------------------
        public void DWord(int aIndex, UInt32 aValue)
        {
            lock (this)
            {
                oldSets.DWord(aIndex, newSets.DWord(aIndex));
                newSets.DWord(aIndex, aValue);
            }
        }

		//----------------------------------------------------------------------
		public bool Bit(int aIndex)
		{
			bool bRet;

			lock (this)
			{
				bRet = newSets[aIndex];
			}

			return bRet;
		}

		//----------------------------------------------------------------------
		public bool Bit(string aTag)
		{
			bool bRet;

			lock (this)
			{
				bRet = newSets[aTag];
			}

			return bRet;
		}

        //----------------------------------------------------------------------
        public bool IsBitChanged(int aIndex)
        {
            bool bRet = false;

            lock (this)
            {
                if (oldSets[aIndex] != newSets[aIndex])
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        //----------------------------------------------------------------------
        public bool IsBitChanged(string aTag)
        {
            bool bRet = false;

            lock (this)
            {
                if (oldSets[aTag] != newSets[aTag])
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        //----------------------------------------------------------------------
        public bool IsBitRising(int aIndex)
        {
            bool bRet = false;

            lock (this)
            {
                if ((oldSets[aIndex] == false) && (newSets[aIndex] == true))
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        //----------------------------------------------------------------------
        public bool IsBitRising(string aTag)
        {
            bool bRet = false;

            lock (this)
            {
                if ((oldSets[aTag] == false) && (newSets[aTag] == true))
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        //----------------------------------------------------------------------
        public bool IsBitFalling(int aIndex)
        {
            bool bRet = false;

            lock (this)
            {
                if ((oldSets[aIndex] == true) && (newSets[aIndex] == false))
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        //----------------------------------------------------------------------
        public bool IsBitFalling(string aTag)
        {
            bool bRet = false;

            lock (this)
            {
                if ((oldSets[aTag] == true) && (newSets[aTag] == false))
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        //----------------------------------------------------------------------
		public byte Byte(int aIndex)
		{
			byte byRet;

			lock (this)
			{
				byRet = newSets.Byte(aIndex);
			}

			return byRet;
		}

		//----------------------------------------------------------------------
		public bool IsByteChanged(int aIndex)
		{
			bool bRet = false;

			lock (this)
			{
				if (oldSets.Byte(aIndex) != newSets.Byte(aIndex))
				{
					bRet = true;
				}
			}

			return bRet;
		}

		//----------------------------------------------------------------------
		public UInt16 Word(int aIndex)
		{
			UInt16 nRet;

			lock (this)
			{
				nRet = newSets.Word(aIndex);			
			}

			return nRet;
		}

		//----------------------------------------------------------------------
		public bool IsWordChanged(int aIndex)
		{
			if (oldSets.Word(aIndex) != newSets.Word(aIndex))
			{
				return true;
			}

			return false;
		}

		//----------------------------------------------------------------------
		public UInt32 DWord(int aIndex)
		{
			UInt32 nRet;

			lock (this)
			{
				nRet = newSets.DWord(aIndex);
			}

			return nRet;
		}

		//----------------------------------------------------------------------
		public bool IsDWordChanged(int aIndex)
		{
			bool bRet = false;

			lock (this)
			{
				if (oldSets.DWord(aIndex) != newSets.DWord(aIndex))
				{
					bRet = true;
				}
			}

			return bRet;
		}
	}

    //--------------------------------------------------------------------------
    public sealed class UlMelsecQScanThread : UlThread
    {
		private long cycleTime;
		private UlMelsecQScaner scaner;

		public delegate void MelsecQScanHandler(UlMelsecQScaner aScaner);

		public event MelsecQScanHandler ScanningEvent;
		public event MelsecQScanHandler AfterScanEvent;

		//----------------------------------------------------------------------
		public UlMelsecQScanThread(UlMelsecQScaner aScaner, long aCycleTime)
        {
            scaner = aScaner;
			cycleTime = aCycleTime;

            ScanningEvent = null;
			AfterScanEvent = null;
        }

		//----------------------------------------------------------------------
		private void OnScanningEvent()
		{
			// Event Handler가 지정되어 있는가?
			if (ScanningEvent != null)
			{
				ScanningEvent(scaner);
			}
		}

		//----------------------------------------------------------------------
		private void OnAfterScanEvent()
		{
			// Event Handler가 지정되어 있는가?
			if (AfterScanEvent != null)
			{
				AfterScanEvent(scaner);
			}
		}

		//----------------------------------------------------------------------
        protected override void Execute()
        {
            long nBeginTime = ElapsedMilliseconds;

			// Thread 종료 신호가 들어올때 까지 루프
            while (Terminated == false)
            {
				// 지정된 시간이 초과 되었는가?
                if (IsTimeoutMilliseconds(nBeginTime, cycleTime) == true)
                {
					// Timer 초기화
                    nBeginTime += cycleTime;
					
					// 통신포트가 연결된 상태인가?
					if (scaner.Client.Connected == true)
					{
						// PLC Data를 읽어온다
						OnScanningEvent();

						// PLC Data 읽기 이후 처리 Event Call
						OnAfterScanEvent();
					}
                }

				// 제어권 양보
                Yield();
            }
        }
	}
}
//------------------------------------------------------------------------------
