using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Ulee.Device.Connection.Yokogawa
{
    public enum EWT330Exception
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

    public class WT330Exception : EthernetConnectionException
    {
        public WT330Exception(
            string msg = "Occurred Yokogawa WT330 ethernet exception!",
            EWT330Exception code = EWT330Exception.Default)
            : base(msg, (int)code)
        {
        }
    }

    public enum EWT330Phase
    {
        [Description("1-Phase")]
        P1,
        [Description("3-Phase")]
        P3
    }

    public enum EWT330Wiring
    {
        [Description("1-Phase")]
        P1W3,
        [Description("3-Phase 3-Wire")]
        P3W3,
        [Description("3-Phase 4-Wire")]
        P3W4
    }

    public enum EWT330State { Start, Stop, Error, Reset }

    //public enum EWT330P1Series
    //{
    //    W,
    //    V,
    //    A,
    //    Hz,
    //    PF,
    //    Wh,
    //    Time
    //}

    public enum EWT330P3Series
    {
        R_W,
        R_V,
        R_A,
        R_Hz,
        R_PF,
        R_Wh,
        S_W,
        S_V,
        S_A,
        S_Hz,
        S_PF,
        S_Wh,
        T_W,
        T_V,
        T_A,
        T_Hz,
        T_PF,
        T_Wh,
        Sigma_W,
        Sigma_V,
        Sigma_A,
        Sigma_Hz,
        Sigma_PF,
        Sigma_Wh,
        Time
    }

    public class UlWT330EthernetClient : UlEthernetClient
    {
        private UlWT330EthernetClient(
            string name, EWT330Phase phase, int scanTime, EEthernetProtocol protocol, int timeout)
            : base(name, scanTime, protocol, timeout)
        {
            this.Phase = phase;
            this.packet = " ";
            Mode = EEthernetMode.Real;
            Values = new float[ItemLength];
        }

        public UlWT330EthernetClient(
            string name, EWT330Phase phase, IPEndPoint ipPoint,
            int scanTime = 250, 
            EEthernetProtocol protocol = EEthernetProtocol.Tcp,
            int timeout = 1000)
            : this(name, phase, scanTime, protocol, timeout)
        {
            IpPoint = ipPoint;
        }

        public UlWT330EthernetClient(
            string name, EWT330Phase phase, string ip, int port,
            int scanTime = 250,
            EEthernetProtocol protocol = EEthernetProtocol.Tcp,
            int timeout = 1000)
            : this(name, phase, scanTime, protocol, timeout)
        {
            try
            {
                IpPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);

                throw new WT330Exception(
                    "Occurred invalid IP and Port in UlWT330EthernetClient.Creater",
                    EWT330Exception.InvalidIPandPort);
            }
        }

        private const string csCmdGetValue = "NUM:NORM:VAL?";

        private const string csCmdStartIntegration = "INTEG:STAR";
        private const string csCmdStopIntegration = "INTEG:STOP";
        private const string csCmdResetIntegration = "INTEG:RES";

        private const string csCmdSetRemoteOn = ":COMM:REM ON";
        private const string csCmdSetRemoteOff = ":COMM:REM OFF";
        private const string csCmdSetLockOn = ":COMM:LOCK ON";
        private const string csCmdSetLockOff = ":COMM:LOCK OFF";
        private const string csCmdSetHeadOn = ":COMM:HEAD ON";
        private const string csCmdSetHeadOff = ":COMM:HEAD OFF";
        private const string csCmdSetWiring1PW3 = "INP:WIR P1W3\nDISP:NORM:ITEM1 U,1;ITEM2 I,1;ITEM3 P,1;ITEM4 FU,1";
        private const string csCmdSetWiring3PW3 = "INP:WIR P3W3\nDISP:NORM:ITEM1 U,SIGMA;ITEM2 I,SIGMA;ITEM3 P,SIGMA;ITEM4 FU,1";
        private const string csCmdSetWiring3PW4 = "INP:WIR P3W4\nDISP:NORM:ITEM1 U,SIGMA;ITEM2 I,SIGMA;ITEM3 P,SIGMA;ITEM4 FU,1";
        private const string csCmdSetAscii = "NUM:FORM ASC";
        private const string csCmdSetValueBlockP1 = "NUM:NORM:NUM 7;ITEM1 P,1;ITEM2 U,1;ITEM3 I,1;ITEM4 FU,1;ITEM5 LAMB,1;ITEM6 WH,1;ITEM7 TIME";
        private const string csCmdSetValueBlockP3 = "NUM:NORM:NUM 25;" +
                                                  "ITEM1 P,1;ITEM2 U,1;ITEM3 I,1;ITEM4 FU,1;ITEM5 LAMB,1;ITEM6 WH,1;" +
                                                  "ITEM7 P,2;ITEM8 U,2;ITEM9 I,2;ITEM10 FU,2;ITEM11 LAMB,2;ITEM12 WH,2;" +
                                                  "ITEM13 P,3;ITEM14 U,3;ITEM15 I,3;ITEM16 FU,3;ITEM17 LAMB,3;ITEM18 WH,3;" +
                                                  "ITEM19 P,SIGMA;ITEM20 U,SIGMA;ITEM21 I,SIGMA;ITEM22 FU,SIGMA;ITEM23 LAMB,SIGMA;ITEM24 WH,SIGMA;" +
                                                  "ITEM25 TIME";

        private string packet;

        public EWT330Phase Phase { get; private set; }

        public int ItemLength { get { return 25; } }

        //public int ItemLength { get { return (Phase == EWT330Phase.P1) ? 7 : 25; } }

        public override void Read()
        {
            switch (Mode)
            {
                case EEthernetMode.Real:
                    if (Connected == true)
                    {
                        SendTcp(csCmdGetValue);
                        ReceiveTcp();
                        StrToFloat();
                    }
                    break;

                case EEthernetMode.Virtual:
                    SetVirtualValues();
                    break;
            }
        }

        public float[] ToFloat()
        {
            string[] str;

            lock (packet)
            {
                str = packet.Split(new[] { ',' }, StringSplitOptions.None);
            }

            float[] values = new float[str.Length];

            for (int i = 0; i < str.Length; i++)
            {
                if (float.TryParse(str[i], out values[i]) == false)
                {
                    values[i] = InvalidFloatValue;
                }
            }

            return values;
        }

        private void StrToFloat()
        {
            string[] str;

            lock (packet)
            {
                str = packet.Split(new[] { ',' }, StringSplitOptions.None);
            }

            if (str.Length != Values.Length)
            {
                throw new WT330Exception(
                    "Occurred TCP stream creating error in UlWT330EthernetClient.StrToFloat",
                    EWT330Exception.Convert);
            }

            lock (Values)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (float.TryParse(str[i], out Values[i]) == false)
                    {
                        Values[i] = InvalidFloatValue;
                    }
                }
            }
        }

        private void SetVirtualValues()
        {
            lock (Values)
            {
                //for (int i = 0; i < ItemLength; i++)
                //{
                //    Values[i] = (float)(rand.NextDouble() * 100.0);
                //}

                #region VirtualValues
                float[] values = new float[]
                {
                    // Power Meter #1
                    2751.1f, 231.1f, 11.001f, 61.01f, 0.9101f, 41.01f,  // W-R, V-R, A-R, Hz-R, PF-R, WH-R,
                    2751.2f, 231.2f, 11.002f, 61.02f, 0.9102f, 41.02f,  // W-S, V-S, A-S, Hz-S, PF-S, WH-S,
                    2751.3f, 231.3f, 11.003f, 61.03f, 0.9103f, 41.03f,  // W-T, V-T, A-T, Hz-T, PF-T, WH-T,
                    2751.4f, 231.4f, 11.004f, 61.04f, 0.9104f, 41.04f,  // W-Sigma, V-Sigma, A-Sigma, Hz-Sigma, PF-Sigma, WH-Sigma,
                    181f                                                // WH Time

                    //// Power Meter #2
                    //2752.1f, 232.1f, 12.001f, 62.01f, 0.9201f, 42.01f,  // W-R, V-R, A-R, Hz-R, PF-R, WH-R,
                    //2752.2f, 232.2f, 12.002f, 62.02f, 0.9202f, 42.02f,  // W-S, V-S, A-S, Hz-S, PF-S, WH-S,
                    //2752.3f, 232.3f, 12.003f, 62.03f, 0.9203f, 42.03f,  // W-T, V-T, A-T, Hz-T, PF-T, WH-T,
                    //2752.4f, 232.4f, 12.004f, 62.04f, 0.9204f, 42.04f,  // W-Sigma, V-Sigma, A-Sigma, Hz-Sigma, PF-Sigma, WH-Sigma,
                    //182f,                                               // WH Time

                    //// Power Meter #3
                    //2753.1f, 233.1f, 13.001f, 63.01f, 0.9301f, 43.01f,  // W-R, V-R, A-R, Hz-R, PF-R, WH-R,
                    //2753.2f, 233.2f, 13.002f, 63.02f, 0.9302f, 43.02f,  // W-S, V-S, A-S, Hz-S, PF-S, WH-S,
                    //2753.3f, 233.3f, 13.003f, 63.03f, 0.9303f, 43.03f,  // W-T, V-T, A-T, Hz-T, PF-T, WH-T,
                    //2753.4f, 233.4f, 13.004f, 63.04f, 0.9304f, 43.04f,  // W-Sigma, V-Sigma, A-Sigma, Hz-Sigma, PF-Sigma, WH-Sigma,
                    //183f,                                               // WH Time

                    //// Power Meter #4
                    //2754.1f, 234.1f, 14.001f, 64.01f, 0.9401f, 44.01f,  // W-R, V-R, A-R, Hz-R, PF-R, WH-R,
                    //2754.2f, 234.2f, 14.002f, 64.02f, 0.9402f, 44.02f,  // W-S, V-S, A-S, Hz-S, PF-S, WH-S,
                    //2754.3f, 234.3f, 14.003f, 64.03f, 0.9403f, 44.03f,  // W-T, V-T, A-T, Hz-T, PF-T, WH-T,
                    //2754.4f, 234.4f, 14.004f, 64.04f, 0.9404f, 44.04f,  // W-Sigma, V-Sigma, A-Sigma, Hz-Sigma, PF-Sigma, WH-Sigma,
                    //184f,                                               // WH Time
                };

                Array.Copy(values, Values, values.Length);
                #endregion
            }
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

        public void SetRemote(bool active)
        {
            if (Mode == EEthernetMode.Virtual) return;

            if (active == true)
            {
                SendTcp(csCmdSetRemoteOn);
            }
            else
            {
                SendTcp(csCmdSetRemoteOff);
            }
        }

        public void SetLock(bool active)
        {
            if (Mode == EEthernetMode.Virtual) return;

            if (active == true)
            {
                SendTcp(csCmdSetLockOn);
            }
            else
            {
                SendTcp(csCmdSetLockOff);
            }
        }

        public void SetHead(bool active)
        {
            if (Mode == EEthernetMode.Virtual) return;

            if (active == true)
            {
                SendTcp(csCmdSetHeadOn);
            }
            else
            {
                SendTcp(csCmdSetHeadOff);
            }
        }

        public void SetWiring(EWT330Wiring wiring)
        {
            if (Mode == EEthernetMode.Virtual) return;

            //if (Phase == EWT330Phase.P1) return;

            switch (wiring)
            {
                case EWT330Wiring.P1W3:
                    SendTcp(csCmdSetWiring1PW3);
                    break;

                case EWT330Wiring.P3W3:
                    SendTcp(csCmdSetWiring3PW3);
                    break;

                case EWT330Wiring.P3W4:
                    SendTcp(csCmdSetWiring3PW4);
                    break;
            }
        }

        public void SetAscii()
        {
            if (Mode == EEthernetMode.Virtual) return;

            SendTcp(csCmdSetAscii);
        }

        public void SetValueBlock()
        {
            if (Mode == EEthernetMode.Virtual) return;

            //switch (Phase)
            //{
            //    case EWT330Phase.P1:
            //        SendTcp(csCmdSetValueBlockP1);
            //        break;

            //    case EWT330Phase.P3:
            //        SendTcp(csCmdSetValueBlockP3);
            //        break;
            //}

            SendTcp(csCmdSetValueBlockP3);
        }

        public void StartIntegration()
        {
            if (Mode == EEthernetMode.Virtual) return;

            SendTcp(csCmdStartIntegration);
        }

        public void StopIntegration()
        {
            if (Mode == EEthernetMode.Virtual) return;

            SendTcp(csCmdStopIntegration);
        }

        public void ResetIntegration()
        {
            if (Mode == EEthernetMode.Virtual) return;

            SendTcp(csCmdResetIntegration);
        }

        public void Initialize()
        {
            SetHead(false);
            SetRemote(true);

            StopIntegration();
            ResetIntegration();

            SetWiring(EWT330Wiring.P3W3);
            SetAscii();
            SetValueBlock();
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
                        throw new WT330Exception(
                            "Occurred TCP stream creating error in UlWT330EthernetClient.SendTcp",
                            EWT330Exception.Stream);
                    }

                    try
                    {
                        writer.WriteLine(message);
                        writer.Flush();
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new WT330Exception(
                            "Occurred TCP stream sending error in UlWT330EthernetClient.SendTcp",
                            EWT330Exception.Send);
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
                        throw new WT330Exception(
                            "Occurred TCP stream creating error in UlWT330EthernetClient.ReceiveTcp",
                            EWT330Exception.Stream);
                    }

                    // 지정된 시간동안 수신을 대기
                    while (stream.DataAvailable == false)
                    {
                        // 수신 대기시간이 초과 되었나?
                        if (stopWatch.ElapsedMilliseconds > Timeout)
                        {
                            string str = "Occurred TCP recieve timeout error in UlWT330EthernetClient.ReceiveTcp";

                            Log(EEthernetLogItem.Exception, str);
                            throw new WT330Exception(str, EWT330Exception.Timeout);
                        }

                        Thread.Sleep(1);
                    }

                    try
                    {
                        reader = new StreamReader(stream);

                        lock (packet)
                        {
                            packet = reader.ReadLine();
                        }

                        Log(EEthernetLogItem.Receive, packet);
                    }
                    catch (Exception e)
                    {
                        Log(EEthernetLogItem.Exception, e.Message);
                        throw new WT330Exception(
                            "Occurred TCP stream receiving error in UlWT330EthernetClient.ReceiveTcp",
                            EWT330Exception.Receive);
                    }
                }
                finally
                {
                    OnReceiving(false);
                }
            }
        }
    }
}
