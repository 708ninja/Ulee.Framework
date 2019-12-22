//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : TEMP2500 Temp & Humidity Controller Connection Class
//------------------------------------------------------------------------------
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Ulee.Utils;

namespace Ulee.Device.Connection.Temi
{
    public class UlTemi2500Exception : ApplicationException
    {
        public UlTemi2500Exception()
            : base("Occurred Temi2500 PLC Exception!")
        {
        }

        public UlTemi2500Exception(string msg)
            : base(msg)
        {
        }
    }

    public class UlTemi2500TimeoutException : UlTemi2500Exception
    {
        public UlTemi2500TimeoutException()
            : base("Occurred Temi2500 communication time out Exception!")
        {
        }

        public UlTemi2500TimeoutException(string msg)
            : base(msg)
        {
        }
    }

    public enum ETemiLogging
    { None, Event, All }

    public enum ETemiMode
    { Run, Hold, Step, Stop }

    public enum ETemiValue
    { TempPV, TempSV, WetTempPV, WetTempSV, HumiPV, HumiSV, TempVOut, HumiVOut }

    public class UlTemi2500Tcp
    {
        private string str;
        private object criticalLock;
        private Stopwatch stopWatch;
        private NetworkStream netStream;

        private UlLogger logger;

        // Log 파일기록여부
        private ETemiLogging logging;
        public ETemiLogging Logging
        {
            get { return logging; }
            set { logging = value; }
        }

        // Log 파일경로
        public string LogPath
        {
            get { return logger.Path; }
            set { logger.Path = value; }
        }
        
        // Log 파일명
        public string LogFName
        {
            get { return logger.FName; }
            set { logger.FName = value; }
        }
        
        // Log 파일명
        public string LogExt
        {
            get { return logger.Ext; }
            set { logger.Ext = value; }
        }
        
        // Log 파일 Encoding 
        public Encoding LogFEncoding
        {
            get { return logger.FEncoding; }
            set { logger.FEncoding = value; }
        }
        
        // Log 파일 분리 기준 - Min, Hour, Day
        public EUlLogFileSeperation LogFSeperation
        {
            get { return logger.FSeperation; }
            set { logger.FSeperation = value; }
        }

        // Logger 속성 구분자
        private enum ETemiLogItems
        { Connect, Disconnect, Send, Recieve, Note, Exception }

        private TcpClient client;

        private bool connected;
        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        private string ip;
        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }

        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        private string recvPacket;
        public string RecvPacket
        {
            get { return recvPacket; }
            set { recvPacket = value; }
        }

        private const int csTimeout = 250;
        private const int csRertyCount = 3;

        public UlTemi2500Tcp(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            logging = ETemiLogging.Event;

            logger = new UlLogger();
            logger.Active = true;
            logger.Path = "";
            logger.FName = "Temi2500Log";
            logger.AddHead("CONNECT");
            logger.AddHead("DISCONNECT");
            logger.AddHead("PC->PLC");
            logger.AddHead("PC<-PLC");
            logger.AddHead("COMMENT");
            logger.AddHead("EXCEPTION");

            connected = false;
            client = new TcpClient();

            stopWatch = new Stopwatch();
            criticalLock = new object();
        }

        public void Open()
        {
            if (client.Connected == true) return;

            try
            {
                str = string.Format("TCP IP : {0}, Port : {1}", ip, port);
                LogEvent(ETemiLogItems.Connect, str);

                client.Connect(ip, port);
                connected = true;
            }
            catch (Exception e)
            {
                str = string.Format("Failed conection TCP IP : {0}, Port : {1}", ip, port);
                LogEvent(ETemiLogItems.Exception, str);
                LogEvent(ETemiLogItems.Exception, e.ToString());
                throw e;
            }
        }

        public void Close()
        {
            if (client.Connected == true)
            {
                str = string.Format("TCP IP : {0}, Port : {1}", ip, port);
                LogEvent(ETemiLogItems.Disconnect, str);

                client.Close();
                connected = false;
            }
        }

        public void Send(string packet)
        {
            byte[] bytes = new byte[packet.Length];

            LogPacket(ETemiLogItems.Send, packet);
            Buffer.BlockCopy(bytes, 0, packet.ToCharArray(), 0, packet.Length);

            for (int i = 0; i < csRertyCount; i++)
            {
                try
                {
                    lock (criticalLock)
                    {
                        SendBytes(bytes);
                        Recieve();
                    }
                }
                catch (UlTemi2500Exception e)
                {
                    LogEvent(ETemiLogItems.Exception, e.ToString());
                }

                Thread.Sleep(250);
            }
        }

        private void SendBytes(byte[] bytes)
        {
            try
            {
                netStream = client.GetStream();
                netStream.Write(bytes, 0, bytes.Length);
                netStream.Flush();
            }
            catch (Exception e)
            {
                LogEvent(ETemiLogItems.Exception, "Occurred TCP sending error");
                LogEvent(ETemiLogItems.Exception, e.ToString());

                throw new UlTemi2500Exception();
            }
        }

        private void Recieve()
        {
            try
            {
                netStream = client.GetStream();
            }
            catch (Exception e)
            {
                LogEvent(ETemiLogItems.Exception, "Occurred TCP stream creation error");
                LogEvent(ETemiLogItems.Exception, e.ToString());

                throw new UlTemi2500Exception();
            }

            lbRetry:
            stopWatch.Restart();

            // 지정된 시간동안 수신을 대기
            while (netStream.DataAvailable == false)
            {
                // 수신 대기시간이 초과 되었나?
                if (stopWatch.ElapsedMilliseconds > csTimeout)
                {
                    LogEvent(ETemiLogItems.Exception, "Occurred TCP recieve time out error");
                    throw new UlTemi2500TimeoutException();
                }

                Thread.Sleep(10);
            }

            // 수신 데이터의 byte 개수
            int length = client.Available;

            try
            {
                byte[] bytes = new byte[length];

                // 수신 데이터를 읽는다
                netStream.Read(bytes, 0, length);
                recvPacket = bytes.ToString(); ;

                LogPacket(ETemiLogItems.Recieve, recvPacket);
            }
            catch (Exception e)
            {
                LogEvent(ETemiLogItems.Exception, "Occurred TCP stream reading error");
                LogEvent(ETemiLogItems.Exception, e.ToString());

                throw new UlTemi2500Exception();
            }

            try
            {
                // 올바른 수신 데이터 인지 확인
                ValidatePacket(recvPacket);
            }
            catch (UlTemi2500Exception)
            {
                goto lbRetry;
            }
        }

        private void ValidatePacket(string packet)
        {
            if (recvPacket.Substring(3, 2) == "NG")
            {
                string msg = string.Format("returned Temi2500 NG code : {0}", recvPacket.Substring(5, 2));
                LogEvent(ETemiLogItems.Exception, msg);
                throw new UlTemi2500Exception();
            }

            if (packet.Substring(0, 6) != recvPacket.Substring(0, 6))
            {
                LogEvent(ETemiLogItems.Exception, "Invalid recieve packet error");
                throw new UlTemi2500Exception();
            }

            if (CalcChecksum(packet) != recvPacket.Substring(recvPacket.Length - 4, 2))
            {
                string msg = string.Format("Invalid checksum error : {0}", recvPacket.Substring(recvPacket.Length - 4, 2));
                LogEvent(ETemiLogItems.Exception, msg);
                throw new UlTemi2500Exception();
            }
        }

        public string CalcChecksum(string packet)
        {
            byte value = 0;

            for (int i=0; i<packet.Length-3; i++)
            {
                value += (byte)packet[i + 1];
            }

            return value.ToString("X02");
        }

        private void LogEvent(ETemiLogItems index, string str)
        {
            if (logging != ETemiLogging.None)
            {
                logger.Log((int)index, str);
            }
        }

        private void LogPacket(ETemiLogItems index, string packet)
        {
            if (logging == ETemiLogging.All)
            {
                LogEvent(index, packet);
            }
        }
    }

    public class UlTemi2500Client
    {
        private const string cmdReadDReg        = "RSD";
        private const string cmdReadRandDReg    = "RRD";
        private const string cmdWriteDReg       = "WSD";
        private const string cmdWriteRandDReg   = "WRD";
        private const string cmdSetRandDReg     = "STD";
        private const string cmdCallDReg        = "CLD";

        private UlTemi2500Tcp tcp;
        public UlTemi2500Tcp Tcp
        {
            get { return tcp; }
            set { tcp = value; }
        }

        private int addr;
        public int Addr
        {
            get { return addr; }
            set { addr = value; }
        }

        // 0-Temp PV, 1-Temp SV, 2-Wet Temp PV, 3-Wet Temp SV, 
        // 4-Humi PV, 5-Humi SV, 6-Temp VOut, 7-HumiVOut
        private double[] values;
        public double[] Values
        {
            get
            {
                string packet = string.Format("\x02{0:X02}{1},08,0001  \r\n", addr, cmdReadDReg);
                string checkSum = tcp.CalcChecksum(packet);

                packet = packet.Replace("  ", checkSum);
                UInt16[] nValues = Read(packet);

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (double)nValues[i] * 0.1;
                }

                return values;
            }
        }

        public ETemiMode Mode
        {
            get
            {
                string packet = string.Format("\x02{0:X02}{1},01,0101  \r\n", addr, cmdReadDReg);
                string checkSum = tcp.CalcChecksum(packet);

                packet = packet.Replace("  ", checkSum);
                UInt16[] values = Read(packet);

                return (ETemiMode)values[0];
            }
        }

        public UlTemi2500Client(UlTemi2500Tcp tcp, int addr = 0)
        {
            this.Tcp = tcp;
            this.Addr = addr;

            values = new double[8];
        }

        public UInt16[] Read(string packet)
        {
            int length = int.Parse(packet.Substring(6, 2));
            UInt16[] values = new UInt16[length];

            tcp.Send(packet);
            string str = tcp.RecvPacket.Substring(9, length * 5 - 1);
            string[] datas = str.Split(',');

            if (datas.Length != length) return null;

            for (int i = 0; i < datas.Length; i++)
            {
                try
                {
                    values[i] = Convert.ToUInt16(datas[i], 16);
                }
                catch
                {
                    values[i] = 0;
                }
            }

            return values;
        }
    }
}
