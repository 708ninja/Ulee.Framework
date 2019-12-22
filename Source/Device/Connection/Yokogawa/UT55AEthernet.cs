using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Modbus.Device;

namespace Ulee.Device.Connection.Yokogawa
{
    public enum EUT55AException
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

    public enum EUT55ARegisterAddress
    {
        PV = 0x0002,                    // 현재값(Process value)
        SV = 0x012C,                    // 설정값(Setpoint value)
        OUT = 0x0004,                   // 출력값
        Mode = 0x00C8,                  // 동작모드(Auto / Manual)
        SPN = 0x00CE,                   // PID group
        P = 0x0131,                     // 비례값(P)
        I = 0x0132,                     // 적분값(I)
        D = 0x0133,                     // 미분값(D)
        DEV = 0x0019,                   // 편차(설정값 - 현재값)
        BS = 0x00F2,                    // 현재값 보정
        FL = 0x00F3,
        MOUT = 0x00D8
    }

    public enum EUT55ARegisterSeries
    { PV, SV, OUT, Mode, SPN, P, I, D, DEV, BS, FL, MOUT }

    public class UT55AException : EthernetConnectionException
    {
        public UT55AException(
            string msg = "Occurred Yokogawa UT55A ethernet exception!",
            EUT55AException code = EUT55AException.Default)
            : base(msg, (int)code)
        {
        }
    }

    public class UlUT55AEthernetClient : UlEthernetClient
    {
        private UlUT55AEthernetClient(
            string name, int slaveAddr, int SlaveCount, int scanTime, EEthernetProtocol protocol, int timeout)
            : base(name, scanTime, EEthernetProtocol.Tcp, timeout)
        {
            this.SlaveAddr = slaveAddr;
            this.SlaveCount = SlaveCount;

            tcpClient.ReceiveTimeout = timeout;

            master = null;
            Values = new float[SlaveCount][];
            logStr = new StringBuilder(128);
            dicRegRatio = new Dictionary<int, Dictionary<EUT55ARegisterAddress, float>>();

            for (int i=0; i<SlaveCount; i++)
            {
                Dictionary<EUT55ARegisterAddress, float> dic = new Dictionary<EUT55ARegisterAddress, float>();

                foreach (EUT55ARegisterAddress reg in Enum.GetValues(typeof(EUT55ARegisterAddress)))
                {
                    dic.Add(reg, csDefaultRatio);
                }

                dicRegRatio.Add(SlaveAddr + i, dic);

                Values[i] = new float[csRegisterCount];
            }
        }

        public UlUT55AEthernetClient(
            string name, IPEndPoint ipPoint, int SlaveAddr,  int SlaveCount,
            int scanTime = 0, int timeout = 1000)
            : this(name, SlaveAddr, SlaveCount, scanTime, EEthernetProtocol.Tcp, timeout)
        {
            IpPoint = ipPoint;
        }

        public UlUT55AEthernetClient(
            string name, string ip, int port, int SlaveAddr, int SlaveCount,
            int scanTime = 0, int timeout = 1000)
            : this(name, SlaveAddr, SlaveCount, scanTime, EEthernetProtocol.Tcp, timeout)
        {
            try
            {
                IpPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);

                throw new UT55AException(
                    "Occurred invalid IP and Port in UlUT55AEthernetClient.Creater",
                    EUT55AException.InvalidIPandPort);
            }
        }

        private const int csRegisterCount = 12;

        private const float csDefaultRatio = 100.0f;

        public int SlaveAddr { get; private set; }

        public int SlaveCount { get; private set; }

        private ModbusSerialMaster master;

        private StringBuilder logStr;

        private Dictionary<int, Dictionary<EUT55ARegisterAddress, float>> dicRegRatio;

        public float[][] Values { get; private set; }

        public void Lock()
        {
            Monitor.Enter(Values);
        }

        public void Unlock()
        {
            Monitor.Exit(Values);
        }

        public override void Connect()
        {
            base.Connect();
            master = ModbusSerialMaster.CreateRtu(tcpClient);
        }

        public override void Close()
        {
            base.Close();
            if (master != null)
            {
                master.Dispose();
                master = null;
            }
        }

        public override void Read()
        {
            switch (Mode)
            {
                case EEthernetMode.Real:
                    if (Connected == true)
                    {
                        RealRead();
                    }
                    break;

                case EEthernetMode.Virtual:
                    VirtualRead();
                    break;
            }
        }

        private void RealRead()
        {
            if (master != null)
            {
                try
                {
                    for (int i = 0; i < SlaveCount; i++)
                    {
                        Values[i][(int)EUT55ARegisterSeries.PV] = Read(SlaveAddr + i, EUT55ARegisterAddress.PV);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.SV] = Read(SlaveAddr + i, EUT55ARegisterAddress.SV);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.OUT] = Read(SlaveAddr + i, EUT55ARegisterAddress.OUT);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.Mode] = Read(SlaveAddr + i, EUT55ARegisterAddress.Mode);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.SPN] = Read(SlaveAddr + i, EUT55ARegisterAddress.SPN);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.P] = Read(SlaveAddr + i, EUT55ARegisterAddress.P);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.I] = Read(SlaveAddr + i, EUT55ARegisterAddress.I);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.D] = Read(SlaveAddr + i, EUT55ARegisterAddress.D);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.DEV] = Read(SlaveAddr + i, EUT55ARegisterAddress.DEV);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.BS] = Read(SlaveAddr + i, EUT55ARegisterAddress.BS);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.FL] = Read(SlaveAddr + i, EUT55ARegisterAddress.FL);
                        Thread.SpinWait(1);
                        Values[i][(int)EUT55ARegisterSeries.MOUT] = Read(SlaveAddr + i, EUT55ARegisterAddress.MOUT);
                        Thread.SpinWait(1);
                    }
                }
                catch (Exception e)
                {
                    Log(EEthernetLogItem.Exception, e.Message);
                    throw new UT55AException(
                        "Occurred TCP receiving error in UlUT55AEthernetClient.Read",
                        EUT55AException.Receive);
                }
            }
        }

        private void VirtualRead()
        {
            lock (Values)
            {
                for (int i = 0; i < SlaveCount; i++)
                {
                    foreach (EUT55ARegisterSeries series in Enum.GetValues(typeof(EUT55ARegisterSeries)))
                    {
                        Values[i][(int)series] = (float)rand.NextDouble() * 10.0f;
                    }
                }
            }
        }

        public void SetFixedDecimal(int addr, EUT55ARegisterAddress register, float value)
        {
            dicRegRatio[addr][register] = value;
        }

        public void SetFixedDecimal(int addr, float value)
        {
            SetFixedDecimal(addr, EUT55ARegisterAddress.PV, value);
            SetFixedDecimal(addr, EUT55ARegisterAddress.SV, value);
            SetFixedDecimal(addr, EUT55ARegisterAddress.BS, value);

            SetFixedDecimal(addr, EUT55ARegisterAddress.Mode, 1);
            SetFixedDecimal(addr, EUT55ARegisterAddress.SPN, 1);
            SetFixedDecimal(addr, EUT55ARegisterAddress.I, 1);
            SetFixedDecimal(addr, EUT55ARegisterAddress.D, 1);
            SetFixedDecimal(addr, EUT55ARegisterAddress.DEV, 1);
            SetFixedDecimal(addr, EUT55ARegisterAddress.FL, 1);

            SetFixedDecimal(addr, EUT55ARegisterAddress.OUT, 10);
            SetFixedDecimal(addr, EUT55ARegisterAddress.P, 10);
            SetFixedDecimal(addr, EUT55ARegisterAddress.MOUT, 10);
        }

        private float Read(int addr, EUT55ARegisterAddress register)
        {
            return FixedDecimalAfterRead(addr, register, (float)((short)Read(addr, register, 1)[0]));
        }

        private ushort[] Read(int addr, EUT55ARegisterAddress register, int count)
        {
            ushort[] nValues;

            lock (master)
            {
                nValues = master.ReadHoldingRegisters((byte)addr, (ushort)register, (ushort)count);

                if (Logging == EEthernetLogging.All)
                {
                    logStr.Clear();
                    logStr.Append($"Address : {addr:X2}, Register : {register} -");

                    for (int i = 0; i < nValues.Length; i++)
                    {
                        if (i == 0)
                        {
                            logStr.Append($" {nValues[i]:X4}");
                        }
                        else
                        {
                            logStr.Append($", {nValues[i]:X4}");
                        }
                    }

                    Log(EEthernetLogItem.Receive, logStr.ToString());
                }
            }

            return nValues;
        }

        public void Write(int addr, EUT55ARegisterAddress register, float value)
        {
            if (Mode == EEthernetMode.Virtual) return;

            if ((addr < SlaveAddr) || (addr >= (SlaveAddr + SlaveCount)))
            {
                Log(EEthernetLogItem.Exception, 
                    "Occurred invalid address error in UlUT55AEthernetClient.Write");
                throw new UT55AException(
                    "Occurred invalid address error in UlUT55AEthernetClient.Write",
                    EUT55AException.InvalidAddress);
            }

            try
            {
                lock (master)
                {
                    ushort nValue = FixedDecimalBeforeWrite(addr, register, value);
                    master.WriteSingleRegister((byte)addr, (ushort)register, nValue);
                }

                Log(EEthernetLogItem.Send, $"Address : {addr:X2}, Register : {register} - {value:F1}");
            }
            catch (Exception e)
            {
                Log(EEthernetLogItem.Exception, e.Message);
                throw new UT55AException(
                    "Occurred TCP stream sending error in UlUT55AEthernetClient.Write",
                    EUT55AException.Send);
            }
        }

        public float[][] ToFloat()
        {
            float[][] fValues = new float[SlaveCount][];

            lock (Values)
            {
                for (int i = 0; i < SlaveCount; i++)
                {
                    fValues[i] = new float[csRegisterCount];
                }

                Array.Copy(Values, fValues, SlaveCount * csRegisterCount);
            }

            return fValues;
        }

        public float[] ToFloat(int addr)
        {
            if ((addr < SlaveAddr) || (addr >= (SlaveAddr + SlaveCount)))
            {
                Log(EEthernetLogItem.Exception,
                    "Occurred invalid address error in UlUT55AEthernetClient.ToFloat");
                throw new UT55AException(
                    "Occurred invalid address error in UlUT55AEthernetClient.ToFloat",
                    EUT55AException.InvalidAddress);
            }

            float[] fValues = new float[csRegisterCount];

            lock (Values)
            {
                Array.Copy(Values[addr - SlaveAddr], fValues, csRegisterCount);
            }

            return fValues;
        }

        public float ToFloat(int addr, EUT55ARegisterSeries register)
        {
            return ToFloat(addr)[(int)register];
        }

        private float FixedDecimalAfterRead(int addr, EUT55ARegisterAddress register, float value)
        {
            try
            {
                value /= dicRegRatio[addr][register];
            }
            catch (Exception e)
            {
                value = 0;
                Log(EEthernetLogItem.Exception, e.Message);
            }

            return value;
        }

        private ushort FixedDecimalBeforeWrite(int addr, EUT55ARegisterAddress register, float value)
        {
            return (ushort)(value *= dicRegRatio[addr][register]);
        }
    }
}
