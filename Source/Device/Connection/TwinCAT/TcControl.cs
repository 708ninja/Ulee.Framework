using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

using Yila.Trw15.Tc;
using Yila.Trw15.Util;

namespace Op150
{
    public enum ETcRealTags
    {
        AI_00_AuditTorque,
        AI_01_AuditCurrent,
        AI_02_InputTorque,
        AI_03_OutputTorque,
        AI_04_PsuVoltage,
        AI_05_PsuCurrent,
        AI_06_DarkCurrent,
        AI_07_NvhForce,
        AX_08_InputSP,
        AX_09_OutputSP,
        AX_10_InputState,
        AX_11_OutputState,
        AO_12_InputDemand,
        AO_13_OutputDemand,
        AO_14_None,
        AO_15_None,
        AO_16_PsuDemand,
        AO_17_None,
        AO_18_None,
        AO_19_None,
        ENC_20_InputAngle,
        ENC_21_InputSpeed,
        ENC_22_OutputAngle,
        ENC_23_OutputSpeed,
        DI_24_None,
        DO_25_None,
        ID_26_TickCount,
        ID_27_ErrorCode,
        ID_28_Profile,
        AI_29_FilteredCurrent
    }

    public enum ETcDITags
    {
        DI_00_B_InputAlarm,
        DI_01_A_InputReady,
        DI_02_None,
        DI_03_None,
        DI_04_B_OutputAlarm,
        DI_05_A_OutputReady,
        DI_06_None,
        DI_07_None,
        DI_08_None,
        DI_09_None,
        DI_10_None,
        DI_11_None,
        DI_12_None,
        DI_13_None,
        DI_14_None,
        DI_15_B_EStop
    }

    public enum ETcDOTags
    {
        DO_00_A_InputMode1,
        DO_01_A_InputMode2,
        DO_02_A_InputActive,
        DO_03_A_InputReset,
        DO_04_B_OutputMode1,
        DO_05_A_OutputMode2,
        DO_06_A_OutputActive,
        DO_07_A_OutputReset,
        DO_08_A_DarkCurrent1,
        DO_09_A_DarkCurrent2,
        DO_10_None,
        DO_11_Ignition,
        DO_12_None,
        DO_13_None,
        DO_14_None,
        DO_15_None
    }

    public enum TcErrorCodes
    {
        E_ERR_NONE						= 0,
	    E_ERR_ESTOP						= -1000,
	    E_ERR_WATCHDOGTIMEOUT			= -1001,
        E_ERR_RESETCANMODULE            = -1002,
        E_ERR_INVALIDCOMMAND            = -1100,
	    E_ERR_INVALIDARGUMENT			= -1101,
        E_ERR_OVERPSUVOLTAGE			= -1200,
	    E_ERR_OVERPSUCURRENT			= -1201,
        E_ERR_AUDITOVERTORQUE           = -1202,
        E_ERR_AUDITOVERCURRENT          = -1203,
        E_ERR_SERVOALARM                = -1300,
	    E_ERR_SERVONOTREADY				= -1301,
	    E_ERR_NOSERVOPROFILE			= -1302,
	    E_ERR_INPUTALARM				= -1310,
	    E_ERR_INPUTOVERTORQUE			= -1311,
	    E_ERR_INPUTOVERPOSITION			= -1312,
	    E_ERR_INPUTTORQUEFOLLOW			= -1313,
	    E_ERR_INPUTSPEEDFOLLOW			= -1314,
	    E_ERR_INPUTPOSITIONFOLLOW		= -1315,
        E_ERR_INPUTAIRBEARING           = -1316,
        E_ERR_INPUTCOLLET               = -1317,
        E_ERR_OUTPUTALARM               = -1320,
	    E_ERR_OUTPUTOVERTORQUE			= -1321,
	    E_ERR_OUTPUTOVERPOSITION		= -1322,
	    E_ERR_OUTPUTTORQUEFOLLOW		= -1323,
	    E_ERR_OUTPUTSPEEDFOLLOW			= -1324,
	    E_ERR_OUTPUTPOSITIONFOLLOW		= -1325,
        E_ERR_OUTPUTAIRBEARING          = -1326,
        E_ERR_OUTPUTCOLLET              = -1327,
        E_ERR_NVHOVERFORCE              = -1330
    }

    public enum ETcServoState
    {
        None,
        Accelerating,
        Reached,
        NoControl
    }

    public enum ETcServoMode
    {
        Torque,
        Speed,
        Position
    }

    public enum ETcServoAxis
    {
        Input,
        Output
    }

    public class TcDaqStorage : IDisposable
    {
        #region Events

        /// <summary>
        /// Daq event data wrapper. Holds vector of fresh data from TcDaqStorage whenever read from TwinCAT
        /// </summary>
        public class DaqEventArgs : EventArgs
        {
            public DaqEventArgs(float[] values)
            {
                this.values = values;
            }

            float[] values;
            public float[] Values
            {
                get { return values; }
            }
        }

        /// <summary>
        /// Event  called whenever fresh data is available from TcDaqStorage
        /// </summary>
        public delegate void DaqEventHandler(object sender, DaqEventArgs e);

        public event DaqEventHandler DaqAvailable;
        protected virtual void OnDaqAvailable(DaqEventArgs e)
        {
            if (DaqAvailable != null)
                DaqAvailable(this, e);
        }

        #endregion

        private int index = 0;
        private int start = 0;
        private int stop = 0;
        private int length = 0;
        private const int recLength = (int)ETcRealTags.AI_29_FilteredCurrent + 1;

        private UInt16 diState = 0;
        private UInt16 doState = 0;

        private int errorCode = 0;
        public int ErrorCode
        {
            get { return errorCode; }
        }


        private float[] values;
        private List<float> fValueList;
        private List<double> dValueList;

        private bool disposed = false;
        private readonly object dataLock = new object();

        private const string tagDaqIndex  = ".gv.DaqStorage.index";
        private const string tagDaqActive = ".gv.DaqStorage.active";
        private const string tagDaqLength = ".gv.DaqStorage.length";
        private const string tagDaqValues = ".gv.DaqStorage.values";
 
        //private BackgroundWorker bgThread = null;
        private Thread gatheringThread = null;
        private volatile bool threadTerminated = false;

        private bool active = false;
        public bool Active
        {
            get { return active; }
            set 
            {
                lock (dataLock)
                {
                    if (active != value)
                    {
                        if (value == true)
                        {
                            start = index;
                            stop = index;
                        }
                        else
                        {
                            stop = index;
                        }

                        active = value;
                    }
                }
            }
        }

        public TcDaqStorage()
        {
            // Storage length = 10 minutes DAQ gathering length
            length = (int)TcComm.ReadInt(tagDaqLength, ETcRunTime.RT1) / 2 * 5 * 60 * 10;
            values = new float[length];
            fValueList = new List<float>(length / 2 / 30);
            dValueList = new List<double>(length / 2 / 30);
        }

        ~TcDaqStorage()
        {
            Dispose(false);
        }

        public void Open()
        {
            if (gatheringThread != null) return;

            gatheringThread = new Thread(DoThreadWork);
            gatheringThread.Priority = ThreadPriority.Highest;
            gatheringThread.Start();
        }

        public void Close()
        {
            if (gatheringThread != null)
            {
                threadTerminated = true;
                gatheringThread.Join();
                gatheringThread = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed == false)
            {
                if (disposing == true) Close();
                disposed = true;
            }
        }

        private void DoThreadWork()
        {
            float[] tcValues = null;
            float[] tcNewValues = null;
            int tcLength = TcComm.ReadInt(tagDaqLength, ETcRunTime.RT1) / 2;

            while (threadTerminated == false)
            {
                // Is a half of DAQ storage full?
                if (TcComm.ReadBool(tagDaqActive, ETcRunTime.RT1) == true)
                {
                    int tcIndex = TcComm.ReadInt(tagDaqIndex, ETcRunTime.RT1);
                    int tcStart = (tcIndex < tcLength) ? tcLength : 0;

                    // Read DAQ storage values from TwinCAT
                    tcValues = TcComm.ReadRealArray(tagDaqValues, tcLength * 2, ETcRunTime.RT1);

                    //Copy into newValues array
                    tcNewValues = new float[tcLength];
                    Array.Copy(tcValues, tcStart, tcNewValues, 0, tcLength);

                    // Reset DAQ half full flag
                    TcComm.WriteBool(tagDaqActive, false, ETcRunTime.RT1);

                    lock (dataLock)
                    {
                        Array.Copy(tcValues, tcStart, values, index, tcLength);
                        index = (index + tcLength) % length;
                    }

                    diState = (UInt16)GetFloatValue(ETcRealTags.DI_24_None);
                    doState = (UInt16)GetFloatValue(ETcRealTags.DO_25_None);
                    errorCode = (int)GetFloatValue(ETcRealTags.ID_27_ErrorCode);

                    try
                    {
                        OnDaqAvailable(new DaqEventArgs(tcNewValues));
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.ToString());
                    }
                }

                Thread.Sleep(1);
            }
        }
        
        public float GetFloatValue(ETcRealTags tag)
        {
            float value = 0;

            lock (dataLock)
            {
                if (index == 0)
                    value = values[(int)tag + length - recLength];
                else
                    value = values[(int)tag + index - recLength];
            }

            return value;
        }

        public float[] GetFloatValues(ETcRealTags tag)
        {
            lock (dataLock)
            {
                fValueList.Clear();

                if ((active == false) && (start != stop))
                {
                    if (start < stop)
                    {
                        for (int i = start; i < stop; i += recLength)
                        {
                            fValueList.Add(values[i + (int)tag]);
                        }
                    }
                    else
                    {
                        for (int i = start; i < length; i += recLength)
                        {
                            fValueList.Add(values[i + (int)tag]);
                        }
                        for (int i = 0; i < stop; i += recLength)
                        {
                            fValueList.Add(values[i + (int)tag]);
                        }
                    }
                }
            }

            return fValueList.ToArray();
        }

        public double GetDoubleValue(ETcRealTags tag)
        {
            double value;

            lock (dataLock)
            {
                if (index == 0)
                    value = (double)values[(int)tag + length - recLength];
                else
                    value = (double)values[(int)tag + index - recLength];
            }

            return value;
        }

        public double[] GetDoubleValues(ETcRealTags tag)
        {
            lock (dataLock)
            {
                dValueList.Clear();

                if ((active == false) && (start != stop))
                {
                    if (start < stop)
                    {
                        for (int i = start; i < stop; i += recLength)
                        {
                            dValueList.Add((double)values[i + (int)tag]);
                        }
                    }
                    else
                    {
                        for (int i = start; i < length; i += recLength)
                        {
                            dValueList.Add((double)values[i + (int)tag]);
                        }
                        for (int i = 0; i < stop; i += recLength)
                        {
                            dValueList.Add((double)values[i + (int)tag]);
                        }
                    }
                }
            }

            return dValueList.ToArray();
        }

        /// <summary>
        /// Extract channel values from value array provided by TcDaqStorage
        /// </summary>
        /// <param name="daqValues">Multi-channel vector of sampled values</param>
        /// <param name="tag">Channel number to extract</param>
        /// <returns>Vector of extracted data</returns>
        static public double[] GetDoubleValues(float[] daqValues, ETcRealTags tag)
        {
            double[] da = new double[daqValues.Length / recLength];
            List<double> lst = new List<double>();
            for(int i = 0, j = 0; i < daqValues.Length; i += recLength, ++j)
                da[j] = (double)daqValues[i + (int)tag];
            return da;
        }
        
        public ETcServoState GetServoState(ETcServoAxis axis)
        {
            return (ETcServoState)GetFloatValue((ETcRealTags)((int)ETcRealTags.AX_10_InputState+(int)axis));
        }

        public bool DI(ETcDITags tag)
        {
            bool value;

            lock (dataLock)
            {
                value = Convert.ToBoolean((diState >> (int)tag) & 0x0001);
            }

            return value;
        }

        public bool DO(ETcDOTags tag)
        {
            bool value;

            lock (dataLock)
            {
                value = Convert.ToBoolean((doState >> (int)tag) & 0x0001);
            }

            return value;
        }
    }

    public enum ETcMainCmd
    {
        Clear = 0,
        Cancel = 2000,

        ResetServo = 2100,
        SetServoActive = 2101,
        SetServoDemand = 2102,
        StopServo = 2105,
        StopAllServo = 2106,
        SearchHome = 2107,
        StartServoTorque = 2110,
        StartServoTorqueProfile = 2111,
        StartServoSpeed = 2120,
        StartServoSpeedProfile = 2121,
        StartServoPosition = 2130,
        StartServoPositionProfile = 2131,
        StartServoProfile = 2135,
        GetServoState = 2140,
        ResetServoEncoder = 2150
    }

    public class TcMainCmd : IDisposable
    {
        #region IDisposable implementation

        // Track whether Dispose has been called.
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    Close();
                disposed = true;
            }
        }

        ~TcMainCmd()
        {
            Dispose(false);
        }

        #endregion

        #region enums and variables

        TcCommand cmd = new TcCommand();

        private readonly object cmdLock = new object();
        private string name;
        private ETcRunTime runTime;

        #endregion

        public TcMainCmd(string name, ETcRunTime runTime=ETcRunTime.RT1)
        {
            this.name = name;
            this.runTime = runTime;
        }

        public void Open()
        {
            cmd.Open(name);
        }

        public void Close()
        {
            cmd.Close();
        }

        #region Main Commands

        public bool IsDone()
        {
            try
            {
                lock (cmdLock)
                {
                    return cmd.IsDone();
                }
            }
            catch (TcCommandException ex)
            {
                throw new Exception(string.Format("Tc reported error {0} : {1}", ex.ErrorCode, ((TcErrorCodes)ex.ErrorCode).ToString()));
            }
        }

        public bool TryIsDone(out int doneCode)
        {
            lock (cmdLock)
            {
                return cmd.TryIsDone(out doneCode);
            }
        }

        public bool IsBusy()
        {
            try { 
            lock (cmdLock)
            {
                return cmd.IsBusy();
            }
            }
            catch (TcCommandException ex)
            {
                throw new Exception(string.Format("Tc reported error {0} : {1}", ex.ErrorCode, ((TcErrorCodes)ex.ErrorCode).ToString()));
            }

        }

        void SendBlocking(int timeout)
        {
            try
            {
                cmd.SendBlocking(timeout);
            }
            catch(TcCommandException ex)
            {
                throw new Exception(string.Format("Tc reported error {0} : {1}", ex.ErrorCode, ((TcErrorCodes)ex.ErrorCode).ToString()));
            }
        }

        void Send()
        {
            try
            {
                cmd.Send();
            }
            catch (TcCommandException ex)
            {
                throw new Exception(string.Format("Tc reported error {0} : {1}", ex.ErrorCode, ((TcErrorCodes)ex.ErrorCode).ToString()));
            }
        }
        
        // Clear all system errors //
        public void Clear()
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.Clear;
                SendBlocking(1000);
            }
        }

        // Cancel running command //
        public void Cancel()
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.Cancel;
                SendBlocking(1000);
            }
        }

        // Reset servo command //
        public void ResetServo(ETcServoAxis axis)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.ResetServo;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }
        }

        // Set servo active command //
        public void SetServoActive(ETcServoAxis axis, bool active)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.SetServoActive;
                cmd.CData.Iarg1 = (int)axis;
                cmd.CData.Iarg2 = (active == false) ? 0 : 1;
                SendBlocking(1000);
            }
        }

        // Stop servo command //
        public void StopServo(ETcServoAxis axis)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StopServo;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }
        }

        // Stop all servo command //
        public void StopAllServo()
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StopAllServo;
                SendBlocking(1000);
            }
        }

        // Search home command //
        public void SearchHome(ETcServoAxis axis, double offset)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.SearchHome;
                cmd.CData.Iarg1 = (int)axis;
                cmd.CData.Farg1 = offset;
                SendBlocking(30000);
            }
        }

        // Set servo torque command //
        public void StartServoTorque(ETcServoAxis axis, double value)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoTorque;
                cmd.CData.Iarg1 = (int)axis;
                cmd.CData.Farg1 = value;
                SendBlocking(1000);
            }
        }

        // Set servo torque profile command //
        public void StartServoTorqueProfile(ETcServoAxis axis, PointF[] point)
        {
            SetServoProfile(axis, point);

            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoTorqueProfile;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }
        }

        // Set servo speed command //
        public void StartServoSpeed(ETcServoAxis axis, double value)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoSpeed;
                cmd.CData.Iarg1 = (int)axis;
                cmd.CData.Farg1 = value;
                SendBlocking(1000);
            }
        }

        // Set servo speed profile command //
        public void StartServoSpeedProfile(ETcServoAxis axis, PointF[] point)
        {
            SetServoProfile(axis, point);

            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoSpeedProfile;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }
        }

        // Set servo position command //
        public void StartServoPosition(ETcServoAxis axis, double value, double maxSpeed, double accel)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoPosition;
                cmd.CData.Iarg1 = (int)axis;
                cmd.CData.Farg1 = value;
                cmd.CData.Farg2 = maxSpeed;
                cmd.CData.Farg3 = accel;
                SendBlocking(1000);
            }
        }

        // Set servo position profile command //
        public void StartServoPositionProfile(ETcServoAxis axis, PointF[] point)
        {
            SetServoProfile(axis, point);

            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoPositionProfile;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }
        }

        public void StartServoProfile(ETcServoMode inputMode, ETcServoMode outputMode)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.StartServoProfile;
                cmd.CData.Iarg1 = (int)inputMode;
                cmd.CData.Iarg2 = (int)outputMode;
                SendBlocking(1000);
            }
        }

        // Get servo state command //
        public ETcServoState GetServoState(ETcServoAxis axis)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.GetServoState;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }

            return (ETcServoState)cmd.RData.Iarg2;
        }

        // Reset servo encoder command //
        public void ResetServoEncoder(ETcServoAxis axis)
        {
            lock (cmdLock)
            {
                cmd.CData.Command = (int)ETcMainCmd.ResetServoEncoder;
                cmd.CData.Iarg1 = (int)axis;
                SendBlocking(1000);
            }
        }

        // Set servo running profile //
        public void SetServoProfile(ETcServoAxis axis, PointF[] point)
        {
            for (int i = 0; i < point.Length; i++)
            {
                string sPointX = string.Format(".gv.Axis[{0}].profile.point[{1}].X", (int)axis, i);
                string sPointY = string.Format(".gv.Axis[{0}].profile.point[{1}].Y", (int)axis, i);

                TcComm.WriteReal(sPointX, point[i].X, ETcRunTime.RT1);
                TcComm.WriteReal(sPointY, point[i].Y, ETcRunTime.RT1);
            }

            string sLength = string.Format(".gv.Axis[{0}].profile.length", (int)axis);
            TcComm.WriteInt(sLength, (short)point.Length, ETcRunTime.RT1);
        }

        public void SetServoFriction(ETcServoAxis axis, int dir, float kf, float kv)
        {
            string sPid = string.Format(".gv.Axis[{0}].friction[{1}].", (int)axis, dir);

            TcComm.WriteReal(sPid + "Kf", (float)kf, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Kv", (float)kv, ETcRunTime.RT1);
        }

        // Set PID parameter
        // axis : Servo axis number
        // mode  : Servo PID control mode 0-Torque 1-Speed 2-Position
        // kp    : Propotional factor(%)
        // ki    : Integral factor(msec)
        // kd    : Differencial factor(msec)
        // tv    : Velocity compensation
        // tf    : Friction
        // ti    : Rotor inertia
        // min   : Minimum output value
        // max   : Maximum output value
        public void SetParamServoPID(ETcServoAxis axis, ETcServoMode mode, bool closeLoop, double kp, double ki, 
            double kd, double ka, double tv, double tf, double ti, double min, double max)
        {
            string sPid = string.Format(".gv.Axis[{0}].pidParam[{1}].", (int)axis, (int)mode);

            TcComm.WriteBool(sPid + "closeLoop", closeLoop, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Kp", (float)kp, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Ki", (float)ki, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Kd", (float)kd, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Ka", (float)ka, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Tv", (float)tv, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Tf", (float)tf, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "Ti", (float)ti, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "value.minValue", (float)min, ETcRunTime.RT1);
            TcComm.WriteReal(sPid + "value.maxValue", (float)max, ETcRunTime.RT1);
        }

        public void SetScaleAI(int index, int type, double scale, double offset)
        {
            string sAI = string.Format(".gv.Daq.AIN[{0}].", index);

            TcComm.WriteInt(sAI + "mode", (short)type, ETcRunTime.RT1);
            TcComm.WriteReal(sAI + "calScale", (float)scale, ETcRunTime.RT1);
            TcComm.WriteReal(sAI + "calOffset", (float)offset, ETcRunTime.RT1);
        }

        public void SetTripAI(int index, int errorCode, int maxCount, double min, double max)
        {
            string sTrip = string.Format(".gv.Daq.AIN[{0}].trip.", index);

            TcComm.WriteInt(sTrip + "code", (short)errorCode, ETcRunTime.RT1);
            TcComm.WriteInt(sTrip + "maxCount", (short)maxCount, ETcRunTime.RT1);
            TcComm.WriteReal(sTrip + "minValue", (float)min, ETcRunTime.RT1);
            TcComm.WriteReal(sTrip + "maxValue", (float)max, ETcRunTime.RT1);
        }

        public void SetScaleAO(int index, int type, double scale, double offset)
        {
            string sAO = string.Format(".gv.Daq.AOUT[{0}].", index);

            TcComm.WriteInt(sAO + "mode", (short)type, ETcRunTime.RT1);
            TcComm.WriteReal(sAO + "scale", (float)scale, ETcRunTime.RT1);
            TcComm.WriteReal(sAO + "offset", (float)offset, ETcRunTime.RT1);
        }

        public void SetTripAO(int index, int errorCode, int maxCount, double min, double max)
        {
            string sTrip = string.Format(".gv.Daq.AOUT[{0}].trip.", index);

            TcComm.WriteInt(sTrip + "code", (short)errorCode, ETcRunTime.RT1);
            TcComm.WriteInt(sTrip + "maxCount", (short)maxCount, ETcRunTime.RT1);
            TcComm.WriteReal(sTrip + "minValue", (float)min, ETcRunTime.RT1);
            TcComm.WriteReal(sTrip + "maxValue", (float)max, ETcRunTime.RT1);
        }

        public void SetTripDI(int index, int errorCode, bool state)
        {
            string sTrip = string.Format(".gv.Daq.DIN[{0}].trip.", index);

            TcComm.WriteInt(sTrip + "code", (short)errorCode, ETcRunTime.RT1);
            TcComm.WriteBool(sTrip + "state", state, ETcRunTime.RT1);
        }

        public void SetTripDO(int index, int errorCode, bool state)
        {
            string sTrip = string.Format(".gv.Daq.DOUT[{0}].trip.", index);

            TcComm.WriteInt(sTrip + "code", (short)errorCode, ETcRunTime.RT1);
            TcComm.WriteBool(sTrip + "state", state, ETcRunTime.RT1);
        }

        #endregion
    }

}
