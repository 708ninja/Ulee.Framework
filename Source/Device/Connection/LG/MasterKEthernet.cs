using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ulee.Device.Connection.LG
{
    public enum EMasterKException
    {
        Default = 0,
        Timeout = -100,
        Stream = -101,
        Send = -102,
        Receive = -103,
        UnknownHead = -104,
        Connect = -105,
        InvalidAddress = -106,
        InvalidIPandPort = -107
    }

    public class MasterKException : EthernetConnectionException
    {
        public MasterKException(
            string msg = "Occurred LG Master-K ethernet exception!",
            EMasterKException code = EMasterKException.Default)
            : base(msg, (int)code)
        {
        }
    }

    public enum EMasterKDevice : byte
    {
        P = (byte)'P',
        M = (byte)'M',
        K = (byte)'K',
        L = (byte)'L',
        F = (byte)'F'
    }

    public class UlMasterKEthernetClient : UlEthernetClient
    {
        private UlMasterKEthernetClient(
            string name, string block, int length, int scanTime, 
            EEthernetProtocol protocol, int timeout)
            : base(name, scanTime, protocol, timeout)
        {
            this.block = block;
            this.length = length;

            valuePacket = " ";
            recvPacket = new StringBuilder(1024);
            Values = new UInt16[length];
        }

        public UlMasterKEthernetClient(
            string name, IPEndPoint ipPoint, string block, int length, int scanTime,
            EEthernetProtocol protocol = EEthernetProtocol.Tcp, int timeout = 1000)
            : this(name, block, length, scanTime, protocol, timeout)
        {
            IpPoint = ipPoint;
        }

        public UlMasterKEthernetClient(
            string name, string ip, int port, string block, int length, int scanTime,
            EEthernetProtocol protocol = EEthernetProtocol.Tcp, int timeout = 1000)
            : this(name, block, length, scanTime, protocol, timeout)
        {
            try
            {
                IpPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);

                throw new MasterKException(
                    "Occurred invalid IP and Port in UlMasterKEthernetClient.Creater",
                    EMasterKException.InvalidIPandPort);
            }
        }

        private string valuePacket;

        private StringBuilder recvPacket;

        private string block;

        private int length;

        public void Lock()
        {
            Monitor.Enter(Values);
        }

        public void Unlock()
        {
            Monitor.Exit(Values);
        }

        public UInt16[] Values { get; private set; }

        public bool GetBit(EMasterKDevice device, int address, int bitNo, int plcNo=0, bool checkBcc=true)
        {
            string cmd = (checkBcc == true) ? "r" : "R";
            string packet = $"{(char)EAscii.ENQ}{plcNo:X2}{cmd}SS0107%{device}X{address:D3}{bitNo:X1}{(char)EAscii.EOT}";

            if (checkBcc == true)
            {
                packet += CalcBcc(packet);
            }

            Send(packet, checkBcc);
            packet = packet.Substring(10, 2);

            return (packet == "00") ? false : true;
        }

        public void SetBit(EMasterKDevice device, int address, int bitNo, bool active, int plcNo = 0, bool checkBcc = true)
        {
            string cmd = (checkBcc == true) ? "w" : "W";
            string boolean = (active == false) ? "00" : "01";
            string packet = $"{(char)EAscii.ENQ}{plcNo:X2}{cmd}SS0107%{device}X{address:D3}{bitNo:X1}{boolean}{(char)EAscii.EOT}";

            if (checkBcc == true)
            {
                packet += CalcBcc(packet);
            }

            Send(packet, checkBcc);
        }

        public override void Read()
        {
            switch (Mode)
            {
                case EEthernetMode.Real:
                    if (Connected == true)
                    {
                        SetRealValues();
                    }
                    break;

                case EEthernetMode.Virtual:
                    SetVirtualValues();
                    break;
            }
        }

        private void SetRealValues()
        {
            Read(block, length, 3);

            lock (Values)
            {
                lock (valuePacket)
                {
                    for (int i = 0; i < length; i++)
                    {
                        try
                        {
                            Values[i] = UInt16.Parse(valuePacket.Substring(i * 4, 4), NumberStyles.HexNumber);
                        }
                        catch
                        {
                            Values[i] = 0;
                        }
                    }
                }
            }
        }

        private void SetVirtualValues()
        {
            lock (Values)
            {
                //for (int i = 0; i < length; i++)
                //{
                //    Values[i] = (UInt16)rand.Next(2);
                //}

                #region VirtualValues
                ushort[] values = new ushort[]
                {
                    1, 1, 1, 1,  // ID1 #1
                    1, 1, 1, 1,  // ID1 #2
                    1, 1, 1, 1,  // ID2 #1
                    1, 1, 1, 1   // ID2 #2
                };

                Array.Copy(values, Values, values.Length);
                #endregion
            }
        }

        public void Read(string device, int length, int plcNo = 0, bool checkBcc = true)
        {
            string cmd = (checkBcc == true) ? "r" : "R";
            string packet = $"{(char)EAscii.ENQ}{plcNo:X2}{cmd}SS{length:X2}{device}{(char)EAscii.EOT}";

            if (checkBcc == true)
            {
                packet += CalcBcc(packet);
            }

            Send(packet, checkBcc);

            lock (valuePacket)
            {
                lock (recvPacket)
                {
                    packet = recvPacket.ToString();
                }

                valuePacket = "";
                for (int i = 0; i < length; i++)
                {
                    valuePacket += packet.Substring(10 + i * 6, 4);
                }
            }
        }

        public void Read(EMasterKDevice device, int address, int length, int plcNo = 0, bool checkBcc = true)
        {
            if ((length < 1) || (length > 120))
            {
                Log(EEthernetLogItem.Exception, "Occurred out of reading length({0})", length);
                throw new MasterKException(
                    "Occurred out of reading length error in UlMasterKEthernetClient.Read",
                    EMasterKException.InvalidAddress);
            }

            string cmd = (checkBcc == true) ? "r" : "R";
            string packet = $"{(char)EAscii.ENQ}{plcNo:X2}{cmd}SB07%{device}W{address:D4}{length:X2}{(char)EAscii.EOT}";

            if (checkBcc == true)
            {
                packet += CalcBcc(packet);
            }

            Send(packet, checkBcc);

            lock (valuePacket)
            {
                lock (recvPacket)
                {
                    packet = recvPacket.ToString();
                }

                valuePacket = packet.Substring(8, length * 4);
            }
        }

        public void Write(EMasterKDevice device, int address, UInt16 value, int plcNo = 0, bool checkBcc = true)
        {
            string cmd = (checkBcc == true) ? "w" : "W";
            string packet = $"{(char)EAscii.ENQ}{plcNo:X2}{cmd}SS0107%{device}W{address:X4}{value:X4}{(char)EAscii.EOT}";

            if (checkBcc == true)
            {
                packet += CalcBcc(packet);
            }

            Send(packet, checkBcc);
        }

        public void Write(EMasterKDevice device, int address, UInt16[] values, int plcNo = 0, bool checkBcc = true)
        {
            if ((values == null) || (values.Length > 120))
            {
                Log(EEthernetLogItem.Exception, "Occurred out of writing length({0})", values.Length);
                throw new MasterKException(
                    "Occurred out of writinging length error in UlMasterKEthernetClient.Write",
                    EMasterKException.InvalidAddress);
            }

            string cmd = (checkBcc == true) ? "w" : "W";
            string packet = $"{(char)EAscii.ENQ}{plcNo:X2}{cmd}SB07%{device}W{address:X4}{values.Length:X2}";

            for (int i = 0; i < values.Length; i++)
            {
                packet += $"{values[i]:X4}";
            }
            packet += $"{(char)EAscii.EOT}";

            if (checkBcc == true)
            {
                packet += CalcBcc(packet);
            }

            Send(packet, checkBcc);
        }

        public UInt16[] ToWord()
        {
            UInt16 value;
            UInt16[] values = new UInt16[valuePacket.Length / 4];

            lock (valuePacket)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (UInt16.TryParse(valuePacket.Substring(i * 4, 4), out value) == true)
                    {
                        values[i] = value;
                    }
                    else
                    {
                        values[i] = 0;
                    }
                }
            }

            return values;
        }

        private void Send(string packet, bool checkBcc)
        {
            if (Connected == true)
            {
                lock (tcpClient)
                {
                    SendTcp(packet);
                    ReceiveTcp(checkBcc);
                }
            }
        }

        private void SendTcp(string message)
        {
            OnSending(true);

            try
            {
                try
                {
                    writer = new StreamWriter(tcpClient.GetStream(), Encoding.ASCII);
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    throw new MasterKException(
                        "Occurred TCP stream creating error in UlMasterKEthernetClient.SendTcp",
                        EMasterKException.Stream);
                }

                try
                {
                    writer.Write(message);
                    writer.Flush();
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    throw new MasterKException(
                        "Occurred TCP stream sending error in UlMasterKEthernetClient.SendTcp",
                        EMasterKException.Send);
                }

                Log(EEthernetLogItem.Send, message);
            }
            finally
            {
                OnSending(false);
            }
        }

        private void ReceiveTcp(bool checkBcc)
        {
            stopWatch.Restart();
            OnReceiving(true);

            try
            {
                recvPacket.Clear();
                NetworkStream stream;

                try
                {
                    stream = tcpClient.GetStream();
                    reader = new StreamReader(stream, Encoding.ASCII);
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    throw new MasterKException(
                        "Occurred TCP stream creating error in UlMasterKEthernetClient.ReceiveTcp",
                        EMasterKException.Stream);
                }

                // 지정된 시간동안 수신을 대기
                while (stream.DataAvailable == false)
                {
                    // 수신 대기시간이 초과 되었나?
                    if (stopWatch.ElapsedMilliseconds > Timeout)
                    {
                        string str = "Occurred TCP recieve timeout error in UlMasterKEthernetClient.ReceiveTcp";

                        Log(EEthernetLogItem.Exception, str);
                        throw new MasterKException(str, EMasterKException.Timeout);
                    }

                    Thread.Sleep(1);
                }

                try
                {
                    char c = (char)EAscii.NUL;

                    while (c != (char)EAscii.ETX)
                    {
                        // 수신 대기시간이 초과 되었나?
                        if (stopWatch.ElapsedMilliseconds > Timeout)
                        {
                            string str = "Occurred TCP recieve timeout error in UlMasterKEthernetClient.ReceiveTcp";

                            Log(EEthernetLogItem.Exception, str);
                            throw new MasterKException(str, EMasterKException.Timeout);
                        }

                        c = (char)reader.Read();

                        lock (recvPacket)
                        {
                            recvPacket.Append(c);
                        }
                    }

                    if (checkBcc == true)
                    {
                        lock (recvPacket)
                        {
                            recvPacket.Append((char)reader.Read());
                            recvPacket.Append((char)reader.Read());
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    throw new MasterKException(
                        "Occurred TCP stream receiving error in UlMasterKEthernetClient.ReceiveTcp",
                        EMasterKException.Receive);
                }

                try
                {
                    Validate(checkBcc);
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    Log(EEthernetLogItem.Exception, recvPacket.ToString());

                    throw new MasterKException(
                        "Occurred validation error in UlMasterKEthernetClient.ReceiveTcp",
                        EMasterKException.Receive);
                }
            }
            finally
            {
                OnReceiving(false);
            }
        }

        private void Validate(bool checkBcc)
        {
            string packet = recvPacket.ToString();

            Log(EEthernetLogItem.Receive, packet);

            if (((char)packet[0] != (char)EAscii.ACK) && ((char)packet[0] != (char)EAscii.NAK))
            {
                throw new MasterKException(
                    "Occurred unkown header error in UlMasterKEthernetClient.Validate",
                    EMasterKException.UnknownHead);
            }

            if ((char)packet[0] == (char)EAscii.NAK)
            {
                Log(EEthernetLogItem.Exception, "Master-K Error Code : {0}", packet.Substring(5, 2));
                throw new MasterKException(
                    "Occurred received error code in UlMasterKEthernetClient.Validate",
                    EMasterKException.Receive);
            }

            if (checkBcc == true)
            {
                string bcc1 = CalcBcc(packet.Substring(0, packet.Length - 2));
                string bcc2 = packet.Substring(packet.Length - 2, 2);

                if (bcc1 != bcc2)
                {
                    Log(EEthernetLogItem.Exception, "BCC error - Calcated : {0}, Received : {1}", bcc1, bcc2);
                    throw new MasterKException(
                        "Occurred BCC error in UlMasterKEthernetClient.Validate",
                        EMasterKException.Receive);
                }
            }
        }

        private string CalcBcc(string packet)
        {
            byte bcc = 0;

            foreach (var c in packet)
            {
                bcc += (byte)c;
            }

            return $"{bcc:X2}";
        }
    }
}
