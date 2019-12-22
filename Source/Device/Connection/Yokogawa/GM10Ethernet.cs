using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;

namespace Ulee.Device.Connection.Yokogawa
{
    public enum EGM10Exception
    {
        Default = 0,
        Timeout = -100,
        Stream = -101,
        Send = -102,
        Receive = -103,
        UnknownHead = -104,
        Connect = -105,
        InvalidAddress = -106,
        InvalidIPandPort = -107,
        Convert = -108
    }

    public class GM10Exception : EthernetConnectionException
    {
        public GM10Exception(
            string msg = "Occurred Yokogawa GM10 ethernet exception!", 
            EGM10Exception code = EGM10Exception.Default)
            : base(msg, (int)code)
        {
        }
    }

    internal class UlGM10StringPacket
    {
        public UlGM10StringPacket(int count, float fValue)
        {
            Count = 0;
            invalidFloatValue = fValue;
            MaxCount = count;
            packet = new StringBuilder(33 + count * csValueTotalLength + 2);
            floatList = new List<float>(count);
        }

        const int csValueStart = 33;
        const int csValueTotalLength = 31;
        const int csValueFloatLength = 13;

        private StringBuilder packet;

        private List<float> floatList;

        private float invalidFloatValue;

        public void Clear()
        {
            lock (this)
            {
                Count = 0;
                packet.Clear();
            }
        }

        public string Head { get { return packet.ToString().Substring(0, 2); } }

        public DateTime DateTime
        {
            get
            {
                DateTime retDate;

                lock (this)
                {
                    int hour;

                    int.TryParse(packet.ToString().Substring(20, 2), out hour);

                    if (hour > 11) hour -= 12;

                    string date = "20" + packet.ToString().Substring(7, 8) +
                        " " + hour.ToString("00") + ":" + packet.ToString().Substring(23, 9);

                    if (DateTime.TryParse(date, out retDate) == false)
                    {
                        retDate = DateTime.Now;
                    }
                }

                return retDate;
            }
        }

        public string Tail { get { return packet.ToString().Substring(packet.Length - 2, 2); } }

        public int MaxCount { get; private set; }

        public int Count { get; private set; }

        public void Append(string str, bool reqLock = true)
        {
            if (reqLock == true)
            {
                lock (this)
                {
                    if (str[1] == ' ') Count++;
                    packet.Append(str);
                }
            }
            else
            {
                if (str[1] == ' ') Count++;
                packet.Append(str);
            }
        }

        public float ToFloat(int index, bool reqLock = true)
        {
            string str;
            float value;

            if (reqLock == true)
            {
                lock (this)
                {
                    str = packet.ToString().Substring(
                        csValueStart + index * csValueFloatLength, csValueFloatLength);
                }
            }
            else
            {
                str = packet.ToString().Substring(
                    csValueStart + index * csValueFloatLength, csValueFloatLength);
            }

            if (float.TryParse(str, out value) == false)
            {
                value = invalidFloatValue;
            }

            return value;
        }

        public float[] ToFloat()
        {
            lock (this)
            {
                floatList.Clear();

                for (int i = 0; i < Count; i++)
                {
                    floatList.Add(ToFloat(i, false));
                }
            }

            return floatList.ToArray();
        }

        public override string ToString()
        {
            return packet.ToString();
        }
    }

    public class UlGM10EthernetClient : UlEthernetClient
    {
        private UlGM10EthernetClient(
            string name, int length, int scanTime, EEthernetProtocol protocol, int timeout)
            : base(name, scanTime, protocol, timeout)
        {
            recvPacket = new UlGM10StringPacket(csMaxCount, InvalidFloatValue);
            Values = new float[length];
        }

        public UlGM10EthernetClient(
            string name,
            IPEndPoint ipPoint, 
            int length,
            int scanTime,
            EEthernetProtocol protocol = EEthernetProtocol.Tcp,
            int timeout = 1000)
            : this(name, length, scanTime, protocol, timeout)
        {
            IpPoint = ipPoint;
        }

        public UlGM10EthernetClient(
            string name,
            string ip, 
            int port,
            int length,
            int scanTime,
            EEthernetProtocol protocol = EEthernetProtocol.Tcp,
            int timeout = 1000)
            : this(name, length, scanTime, protocol, timeout)
        {
            try
            {
                IpPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);

                throw new GM10Exception(
                    "Occurred invalid IP and Port in UlGM10EthernetClient.Creater",
                    EGM10Exception.InvalidIPandPort);
            }
        }

        private const int csMaxCount = 512;

        private const string csCmdReqStringValue = "FData,0";

        private const string csReceiveHead = "EA";
        private const string csReceiveTail = "EN";
        private const string csConnectionHead = "E0";

        private UlGM10StringPacket recvPacket;

        public int Length { get { return Values.Length; } }

        public override void Connect()
        {
            if (Protocol == EEthernetProtocol.Udp) return;

            if (tcpClient == null)
            {
                tcpClient = new TcpClient();
            }

            try
            {
                base.Connect();
                reader = new StreamReader(tcpClient.GetStream());

                if (reader.ReadLine() != csConnectionHead)
                {
                    string str = "Occurred TCP connection failing error in UlGM10EthernetClient.Connect";

                    Log(EEthernetLogItem.Exception, str);
                    throw new GM10Exception(str, EGM10Exception.Connect);
                }
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);
                throw new GM10Exception(
                   "Occurred TCP connection failing error in UlGM10EthernetClient.Connect", 
                   EGM10Exception.Connect);
            }
        }

        public DateTime DateTime { get { return recvPacket.DateTime; } }

        public float ToFloat(int index)
        {
            return recvPacket.ToFloat(index);
        }

        public float[] ToFloat()
        {
            return recvPacket.ToFloat();
        }

        public void Lock()
        {
            Monitor.Enter(Values);
        }

        public void Unlock()
        {
            Monitor.Exit(Values);
        }

        public float[] Values { get; private set; }

        private void Send(string message)
        {
            if (Mode == EEthernetMode.Virtual)
            {
                SetVirtualValues();
            }
            else
            {
                switch (Protocol)
                {
                    case EEthernetProtocol.Tcp:
                        if (Connected == true)
                        {
                            SendTcp(message);
                            ReceiveTcp();
                        }
                        break;

                    case EEthernetProtocol.Udp:
                        SendUdp(message);
                        ReceiveUdp();
                        break;
                }

                StrToFloat();
            }
        }

        private void StrToFloat()
        {
            try
            {
                lock (Values)
                {
                    lock (recvPacket)
                    {
                        for (int i = 0; i < Values.Length; i++)
                        {
                            Values[i] = recvPacket.ToFloat(i, false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);
                throw new GM10Exception(
                    "Occurred ToFloat error in UlGM10EthernetClient.StrToFloat",
                    EGM10Exception.Convert);
            }
        }

        private void SetVirtualValues()
        {
            lock (Values)
            {
                //for (int i=0; i<Values.Length; i++)
                //{
                //    Values[i] = (float)(rand.NextDouble() * 100.0);
                //}

                #region VirtualValues
                float[] values = new float[]
                {
                    // Indoor Side No.1 - #1
                    27.11f, 19.11f, 11.11f, 10.11f,  // Entering DB, Entering WB, Leaving DB, Leaving WB,
                    25.11f, 0.11f, 12.11f,           // Nozzle Diff. Press, Chamber Diff. Press, Nozzle Inlet Temp,
                    
                    // Indoor Side No.1 - #2
                    27.12f, 19.12f, 11.12f, 10.12f,  // Entering DB, Entering WB, Leaving DB, Leaving WB,
                    25.12f, 0.12f, 12.12f,           // Nozzle Diff. Press, Chamber Diff. Press, Nozzle Inlet Temp,
                    761.1f,                          // Atmospheric Press,

                    // Indoor Side No.2 - #1
                    27.21f, 19.21f, 11.21f, 10.21f,  // Entering DB, Entering WB, Leaving DB, Leaving WB,
                    25.21f, 0.21f, 12.21f,           // Nozzle Diff. Press, Chamber Diff. Press, Nozzle Inlet Temp,

                    // Indoor Side No.2 - #2
                    27.22f, 19.22f, 11.22f, 10.22f,  // Entering DB, Entering WB, Leaving DB, Leaving WB,
                    25.22f, 0.22f, 12.22f,           // Nozzle Diff. Press, Chamber Diff. Press, Nozzle Inlet Temp,
                    762.2f,                          // Atmospheric Press,

                    // Outdoor Side
                    35.33f, 24.33f, 10.33f,          // Entering DB, Entering WB, Entering DP

                    // Pressure
                    33.51f, 10.21f, 33.52f, 10.22f, 33.53f, 10.23f, 33.54f, 10.24f,

                    // Skip
                    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,

                    // Indoor Side No.1 Thermocouple
                    101.0f, 102.0f, 103.0f, 104.0f, 105.0f, 106.0f, 107.0f, 108.0f, 109.0f, 110.0f, 111.0f, 112.0f, 113.0f, 114.0f, 115.0f, 116.0f, 117.0f, 118.0f, 119.0f, 120.0f, 121.0f, 122.0f, 123.0f, 124.0f, 125.0f, 126.0f, 127.0f, 128.0f, 129.0f, 130.0f,
                    131.0f, 132.0f, 133.0f, 134.0f, 135.0f, 136.0f, 137.0f, 138.0f, 139.0f, 140.0f, 141.0f, 142.0f, 143.0f, 144.0f, 145.0f, 146.0f, 147.0f, 148.0f, 149.0f, 150.0f, 151.0f, 152.0f, 153.0f, 154.0f, 155.0f, 156.0f, 157.0f, 158.0f, 159.0f, 160.0f,
                    // Indoor Side No.2 Thermocouple
                    201.0f, 202.0f, 203.0f, 204.0f, 205.0f, 206.0f, 207.0f, 208.0f, 209.0f, 210.0f, 211.0f, 212.0f, 213.0f, 214.0f, 215.0f, 216.0f, 217.0f, 218.0f, 219.0f, 220.0f, 221.0f, 222.0f, 223.0f, 224.0f, 225.0f, 226.0f, 227.0f, 228.0f, 229.0f, 230.0f,
                    231.0f, 232.0f, 233.0f, 234.0f, 235.0f, 236.0f, 237.0f, 238.0f, 239.0f, 240.0f, 241.0f, 242.0f, 243.0f, 244.0f, 245.0f, 246.0f, 247.0f, 248.0f, 249.0f, 250.0f, 251.0f, 252.0f, 253.0f, 254.0f, 255.0f, 256.0f, 257.0f, 258.0f, 259.0f, 260.0f,
                    // Outdoor Side Thermocouple
                    301.0f, 302.0f, 303.0f, 304.0f, 305.0f, 306.0f, 307.0f, 308.0f, 309.0f, 310.0f, 311.0f, 312.0f, 313.0f, 314.0f, 315.0f, 316.0f, 317.0f, 318.0f, 319.0f, 320.0f, 321.0f, 322.0f, 323.0f, 324.0f, 325.0f, 326.0f, 327.0f, 328.0f, 329.0f, 330.0f,
                    331.0f, 332.0f, 333.0f, 334.0f, 335.0f, 336.0f, 337.0f, 338.0f, 339.0f, 340.0f, 341.0f, 342.0f, 343.0f, 344.0f, 345.0f, 346.0f, 347.0f, 348.0f, 349.0f, 350.0f, 351.0f, 352.0f, 353.0f, 354.0f, 355.0f, 356.0f, 357.0f, 358.0f, 359.0f, 360.0f
                };

                Array.Copy(values, Values, values.Length);
                #endregion
            }
        }

        private void SendTcp(string message)
        {
            OnSending(true);

            lock (tcpClient)
            {
                try
                {
                    try
                    {
                        writer = new StreamWriter(tcpClient.GetStream());
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new GM10Exception(
                            "Occurred TCP stream creating error in UlGM10EthernetClient.SendTcp",
                            EGM10Exception.Stream);
                    }

                    try
                    {
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new GM10Exception(
                            "Occurred TCP stream sending error in UlGM10EthernetClient.SendTcp",
                            EGM10Exception.Send);
                    }

                    Log(EEthernetLogItem.Send, message);
                }
                finally
                {
                    OnSending(false);
                }
            }
        }

        private void ReceiveTcp()
        {
            stopWatch.Restart();
            OnReceiving(true);

            lock (tcpClient)
            {
                try
                {
                    NetworkStream stream;

                    try
                    {
                        stream = tcpClient.GetStream();
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new GM10Exception(
                            "Occurred TCP stream creating error in UlGM10EthernetClient.ReceiveTcp",
                            EGM10Exception.Stream);
                    }

                    // 지정된 시간동안 수신을 대기
                    while (stream.DataAvailable == false)
                    {
                        // 수신 대기시간이 초과 되었나?
                        if (stopWatch.ElapsedMilliseconds > Timeout)
                        {
                            string str = "Occurred TCP recieve timeout error in UlGM10EthernetClient.ReceiveTcp";

                            Log(EEthernetLogItem.Exception, str);
                            throw new GM10Exception(str, EGM10Exception.Timeout);
                        }

                        Thread.Sleep(1);
                    }

                    try
                    {
                        string str;
                        reader = new StreamReader(stream);

                        lock (recvPacket)
                        {
                            recvPacket.Clear();

                            for (int i = 0; i < recvPacket.MaxCount + 4; i++)
                            {
                                str = reader.ReadLine();

                                if (str == csReceiveTail) break;
                                if (i > 2) str = str.Substring(str.Length - 13);

                                recvPacket.Append(str, false);
                            }
                        }

                        Log(EEthernetLogItem.Receive, recvPacket.ToString());

                        if (recvPacket.Head != csReceiveHead)
                        {
                            str = $"Occurred unknown head({recvPacket.Head}) readed error in UlGM10EthernetClient.ReceiveTcp";

                            Log(EEthernetLogItem.Exception, str);
                            throw new GM10Exception(str, EGM10Exception.UnknownHead);
                        }
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new GM10Exception(
                            "Occurred TCP stream receiving error in UlGM10EthernetClient.ReceiveTcp",
                            EGM10Exception.Receive);
                    }
                }
                finally
                {
                    OnReceiving(false);
                }
            }
        }

        private void SendUdp(string message)
        {
            lock (udpClient)
            {
                try
                {
                    message += "\r\n";
                    byte[] bytes = Encoding.UTF8.GetBytes(message);

                    udpClient.Send(bytes, bytes.Length, IpPoint);
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    throw new GM10Exception(
                        "Occurred UDP stream creating error in UlGM10EthernetClient.SendUdp",
                        EGM10Exception.Stream);
                }
            }

            Log(EEthernetLogItem.Send, message);
        }

        private void ReceiveUdp()
        {
            byte[] bytes;

            stopWatch.Restart();
            OnReceiving(true);

            lock (udpClient)
            {
                try
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 0);
                    MemoryStream stream = new MemoryStream();

                lbRetry:
                    // 지정된 시간동안 수신을 대기
                    while (udpClient.Available == 0)
                    {
                        // 수신 대기시간이 초과 되었나?
                        if (stopWatch.ElapsedMilliseconds > Timeout)
                        {
                            string str = "Occurred UDP receive timeout error in UlGM10EthernetClient.ReceiveUdp";

                            Log(EEthernetLogItem.Exception, str);
                            throw new GM10Exception(str, EGM10Exception.Timeout);
                        }

                        Thread.Sleep(1);
                    }

                    bytes = udpClient.Receive(ref ipPoint);
                    if (ipPoint.Address.ToString() != IpPoint.Address.ToString())
                    {
                        goto lbRetry;
                    }

                    stream.Write(bytes, 0, bytes.Length);

                    try
                    {
                        reader = new StreamReader(stream);

                        lock (recvPacket)
                        {
                            recvPacket.Clear();

                            for (int i = 0; i < recvPacket.MaxCount + 4; i++)
                            {
                                recvPacket.Append(reader.ReadLine(), false);

                                if (recvPacket.Tail == csReceiveTail) break;
                            }
                        }

                        Log(EEthernetLogItem.Receive, recvPacket.ToString());

                        if (recvPacket.Head != csReceiveHead)
                        {
                            string str = $"Occurred unknown head({recvPacket.Head}) readed error in UlGM10EthernetClient.ReceiveUdp";

                            Log(EEthernetLogItem.Exception, str);
                            throw new GM10Exception(str, EGM10Exception.UnknownHead);
                        }
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new GM10Exception(
                            "Occurred UDP stream reading error in UlGM10EthernetClient.ReceiveUdp",
                            EGM10Exception.Receive);
                    }
                }
                finally
                {
                    OnReceiving(false);
                }
            }
        }

        public override void Read()
        {
            Send(csCmdReqStringValue);
        }

        public void Read(int address, int count)
        {
            if (address < 1)
            {
                string str = "Occurred invalid address in UlGM10EthernetClient.GetValues";

                Log(EEthernetLogItem.Exception, str);
                throw new GM10Exception(str, EGM10Exception.InvalidAddress);
            }

            Send($"{csCmdReqStringValue},{address:d4},{address + count - 1:d4}");
        }
    }
}
