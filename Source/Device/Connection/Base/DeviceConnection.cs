using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

using Ulee.Device;
using Ulee.Utils;
using Ulee.Threading;

namespace Ulee.Device.Connection
{
    public class EthernetConnectionException : DeviceException
    {
        public EthernetConnectionException(string msg="Occurred Ethernet Connection Exception!", int code=0)
            : base(msg, code)
        {
        }
    }

    public enum EEthernetConnectionException
    {
        Default = 0
    }

    public enum EEthernetLink
    { Disconnect, Connect }

    public enum EEthernetMode
    { Real, Virtual }

    public enum EEthernetLogging
    { None, Event, All }

    public enum EEthernetLogItem
    { Connect, Disconnect, Send, Receive, Note, Exception }

    public enum EEthernetProtocol
    { Tcp, Udp }

    public enum EAscii : byte
    {
        NUL = 0x00,
        SOH = 0x01,
        STX = 0x02,
        ETX = 0x03,
        EOT = 0x04,
        ENQ = 0x05,
        ACK = 0x06,
        BEL = 0x07,
        BS = 0x08,
        TAB = 0x09,
        LF = 0x0A,
        VT = 0x0B,
        FF = 0x0C,
        CR = 0x0D,
        SO = 0x0E,
        SI = 0x0F,
        DLE = 0x10,
        DC1 = 0x11,
        DC2 = 0x12,
        DC3 = 0x13,
        DC4 = 0x14,
        NAK = 0x15,
        SYN = 0x16,
        ETB = 0x17,
        CAN = 0x18,
        EM = 0x19,
        SUB = 0x1A,
        ESC = 0x1B,
        FS = 0x1C,
        GS = 0x1D,
        RS = 0x1E,
        US = 0x1F
    }

    public class UlEthernetRealValue
    {
        public UlEthernetRealValue(int index, float value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; private set; }

        public float Value { get; set; }
    }

    public class UlEthernetShortValue
    {
        public UlEthernetShortValue(int index, UInt16 value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; private set; }

        public UInt16 Value { get; set; }
    }

    public delegate void ConnectionEventHandler(bool active);

    public interface IConnectionClient
    {
        string Name { get; }

        int Timeout { get; }

        event ConnectionEventHandler Sending;
        void OnSending(bool active);

        event ConnectionEventHandler Receiving;
        void OnReceiving(bool active);

        bool Connected { get; }

        void Connect();

        void Close();
    }

    public interface IEthernetClient : IConnectionClient
    {
        EEthernetProtocol Protocol { get; }

        IPEndPoint IpPoint { get; }

        string Ip { get; }

        int Port { get; }

        int ScanTime { get; }
    }

    public abstract class UlEthernetClient : IEthernetClient
    {
        public UlEthernetClient(
            string name, int scanTime, EEthernetProtocol protocol, int timeout)
        {
            InvalidFloatValue = float.NaN;
            ScanTime = scanTime;
            Protocol = protocol;
            Timeout = timeout;
            Name = name;

            Mode = EEthernetMode.Real;

            rand = new Random();

            stopWatch = new Stopwatch();

            CreateClient();

            if (ScanTime > 0)
            {
                scaner = new UlEthernetScanerThread(this, ScanTime);
            }
            else
            {
                scaner = null;
            }

            Logging = EEthernetLogging.None;

            logger = new UlLogger();
            logger.Active = true;
            logger.Path = "";
            logger.FName = "EthernetClientLog";

            logger.Clear();
            logger.AddTag("CONNECT");
            logger.AddTag("DISCONNECT");
            logger.AddTag("PC->Device");
            logger.AddTag("PC<-Device");
            logger.AddTag("NOTE");
            logger.AddTag("EXCEPTION");
        }

        ~UlEthernetClient()
        {
            if (tcpClient != null) tcpClient.Close();
            if (udpClient != null) udpClient.Close();
        }

        public string Name { get; protected set; }

        public EEthernetMode Mode { get; set; }

        public IPEndPoint IpPoint { get; protected set; }

        public string Ip { get { return IpPoint.Address.ToString(); } }

        public int Port { get { return IpPoint.Port; } }

        public int ScanTime { get; protected set; }

        public EEthernetProtocol Protocol { get; protected set; }

        public int Timeout { get; protected set; }

        public float InvalidFloatValue { get; set; }

        public bool Connected
        {
            get
            {
                if (Protocol == EEthernetProtocol.Udp) return false;

                bool active;

                if (tcpClient == null)
                {
                    active = false;
                }
                else
                {
                    lock (tcpClient)
                    {
                        try
                        {
                            active = tcpClient.Connected;
                        }
                        catch (Exception e)
                        {
                            Log(EEthernetLogItem.Exception, e.ToString());
                            active = false;
                        }
                    }
                }

                return active;
            }
        }

        public virtual void Connect()
        {
            if (Protocol == EEthernetProtocol.Udp) return;

            try
            {
                if (tcpClient == null) CreateClient();

                lock (tcpClient)
                {
                    tcpClient.Connect(IpPoint);
                }
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.ToString());
                throw new DeviceException(
                    $"Can't establish TCP connection({Ip}:{Port}) in UlEthernetClient.Connect", 0);
            }

            Log(EEthernetLogItem.Connect, "Ethernet client connection({0}:{1})", Ip, Port);
        }

        public virtual void Close()
        {
            if (Protocol == EEthernetProtocol.Udp) return;

            if (tcpClient != null)
            {
                lock (tcpClient)
                {
                    tcpClient.Close();
                    Log(EEthernetLogItem.Disconnect, "Ethernet client connection({0}:{1})", Ip, Port);
                }

                tcpClient = null;

                Mode = EEthernetMode.Virtual;
                Log(EEthernetLogItem.Note, "Changed mode of client connection({0}:{1}) to Virtual", Ip, Port);
            }
        }

        private void CreateClient()
        {
            switch (Protocol)
            {
                case EEthernetProtocol.Tcp:
                    tcpClient = new TcpClient();
                    udpClient = null;
                    break;

                case EEthernetProtocol.Udp:
                    tcpClient = null;
                    udpClient = new UdpClient();
                    break;
            }
        }

        public abstract void Read();

        // 송신 Event Handler
        public event ConnectionEventHandler Sending;
        public void OnSending(bool active)
        {
            Sending?.Invoke(active);
        }

        // 수신 Event Handler
        public event ConnectionEventHandler Receiving;
        public void OnReceiving(bool active)
        {
            Receiving?.Invoke(active);
        }

        // Log 파일기록여부
        public EEthernetLogging Logging;

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

        protected UlEthernetScanerThread scaner;

        public void AddScanningHandler(EventHandler handler)
        {
            scaner.Scanning += handler;
        }

        public void DeleteScanningHandler(EventHandler handler)
        {
            scaner.Scanning -= handler;
        }

        public void AddAfterExceptionHandler(EventHandler handler)
        {
            scaner.AfterException += handler;
        }

        public void DeleteAfterExceptionHandler(EventHandler handler)
        {
            scaner.AfterException -= handler;
        }

        public void Resume()
        {
            if (scaner != null)
            {
                if (scaner.IsAlive == false)
                    scaner.Start();
                else
                    scaner.Resume();
            }
        }

        public void Suspend()
        {
            if (scaner != null)
            {
                scaner.Suspend();
            }
        }

        public void Terminate()
        {
            if (scaner != null)
            {
                scaner.Terminate();
            }
        }

        protected UlLogger logger;

        protected Stopwatch stopWatch;

        protected TcpClient tcpClient;

        protected UdpClient udpClient;

        protected StreamReader reader;

        protected StreamWriter writer;

        protected Random rand;

        public void Log(EEthernetLogItem item, string str)
        {
            switch (Logging)
            {
                case EEthernetLogging.None:
                    break;

                case EEthernetLogging.Event:
                    if ((item != EEthernetLogItem.Send) && (item != EEthernetLogItem.Receive))
                    {
                        logger.Log((int)item, str);
                    }
                    break;

                case EEthernetLogging.All:
                    logger.Log((int)item, str);
                    break;
            }
        }

        public void Log(EEthernetLogItem item, string fmt, params object[] args)
        {
            string str = string.Format(fmt, args);

            Log(item, str);
        }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; } 

        public ExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public class UlEthernetScanerThread : UlThread
    {
        public UlEthernetScanerThread(UlEthernetClient client, long time) : base(false)
        {
            Client = client;
            scanTime = time;
            Scanning = null;
        }

        private long scanTime;

        public UlEthernetClient Client { get; private set; }

        public event EventHandler Scanning;

        private void OnScanning(object sender, EventArgs args)
        {
            try
            {
                if (Scanning == null)
                {
                    Client.Read();
                }
                else
                {
                    Scanning(sender, args);
                }
            }
            catch (Exception e)
            {
                Client.Close();
                OnAfterException(sender, new ExceptionEventArgs(e));
            }
        }

        public event EventHandler AfterException;

        private void OnAfterException(object sender, ExceptionEventArgs args)
        {
            AfterException?.Invoke(sender, args);
        }

        protected override void Execute()
        {
            long beginTime = ElapsedMilliseconds;

            // Thread 종료 신호가 들어올때 까지 루프
            while (Terminated == false)
            {
                // 지정된 시간이 초과 되었는가?
                if (IsTimeoutMilliseconds(beginTime, scanTime) == true)
                {
                    // Timer 초기화
                    beginTime += scanTime;

                    OnScanning(Client, null);
                }

                // 제어권 양보
                Yield();
            }
        }
    }
}
