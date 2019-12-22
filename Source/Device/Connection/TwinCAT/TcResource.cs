//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : TwinCAT PLC Resource Class
//------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Collections.Generic;

using TwinCAT.Ads;

using Ulee.Utils;
using Ulee.Threading;

namespace Ulee.Device.Connection.TwinCAT
{
    public class UlTcResource : UlThread, IDisposable
    {
        private int start;
        private int stop;
        private int index;
        private bool marking;
        public bool IsMarking
        {
            get { return marking; }
        }

        private int tcLength;
        private int tcHalfLength;
        private int valueLength;

        private int errorCode;
        public int ErrorCode
        {
            get { return errorCode; }
        }

        private UlTcResourceValue[] values;
        private UlTcAdsClient client;

        private bool disposed = false;
        private readonly object dataLock = new object();

        private const string tagErrorCode = "gv.sysParam.errorCode";
        private const string tagIndex = "gv.sysParam.storage.index";
        private const string tagActive = "gv.sysParam.storage.active";
        private const string tagLength = "gv.sysParam.storage.length";
        private const string tagValues = "gv.sysParam.storage.values";

        public UlTcResource(UlTcAdsClient client, int length, 
            int aiLen, int aoLen, int encLen, int diLen, int doLen)
            : base(false, false)
        {
            start = 0;
            stop = 0;
            index = 0;
            valueLength = 12 + aiLen * 4 + aoLen * 4 + encLen * 4 + diLen + doLen;

            this.client = client;
            Priority = ThreadPriority.Highest;

            values = new UlTcResourceValue[length];
            for (int i=0; i<length; i++)
            {
                values[i] = new UlTcResourceValue(aiLen, aoLen, encLen, diLen, doLen);
            }
        }

        ~UlTcResource()
        {
            Dispose(false);
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

        public void Open()
        {
            Start();
        }

        public void Close()
        {
            if (IsAlive == true)
            {
                Terminate();
            }
        }

        public void SetMark(bool enabled)
        {
            if (enabled == true)
            {
                if (marking == false)
                {
                    start = index;
                }
                else
                {
                    throw new Exception("Invalid marking state error in UlTcResource::SetMark");
                }
            }
            else
            {
                if (marking == true)
                {
                    stop = index;
                }
                else
                {
                    throw new Exception("Invalid marking state error in UlTcResource::SetMark");
                }
            }

            marking = enabled;
        }

        protected override void Execute()
        {
            int position;

            tcLength = client.ReadInt16(tagLength);
            tcHalfLength = tcLength / 2;

            while (Terminated == false)
            {
                // Is a half of DAQ storage full?
                if (client.ReadBoolean(tagActive) == true)
                {
                    int tcIndex = client.ReadInt16(tagIndex);

                    lock (dataLock)
                    {
                        errorCode = client.ReadInt16(tagErrorCode);

                        if (tcIndex < tcHalfLength) position = valueLength * tcHalfLength;
                        else position = 0;

                        client.Read(tagValues, ReadValues, valueLength * tcLength, position);

                        index = (index + tcHalfLength) % values.Length;
                    }

                    client.WriteBoolean(tagActive, false);
                }

                Thread.Sleep(1);
            }
        }

        private void ReadValues(AdsBinaryReader reader)
        {
            int i, j, k;

            for (i = 0; i < tcHalfLength; i++)
            {
                k = index + i;

                values[k].Tick = reader.ReadUInt32();
                values[k].SetTime(reader.ReadUInt32(), reader.ReadUInt32());

                for (j = 0; j < values[k].AI.Length; j++)
                {
                    values[k].AI[j] = reader.ReadSingle();
                }

                for (j = 0; j < values[k].AO.Length; j++)
                {
                    values[k].AO[j] = reader.ReadSingle();
                }

                for (j = 0; j < values[k].ENC.Length; j++)
                {
                    values[k].ENC[j] = reader.ReadSingle();
                }

                for (j = 0; j < values[k].DI.Count; j++)
                {
                    values[k].DI.Bytes[j] = reader.ReadByte();
                }

                for (j = 0; j < values[k].DO.Count; j++)
                {
                    values[k].DO.Bytes[j] = reader.ReadByte();
                }
            }
        }

        public UInt32 GetTickCount()
        {
            UInt32 tick;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                tick = values[j].Tick;
            }

            return tick;
        }

        public UInt32[] GetTickCount(int count, int step = 1)
        {
            UInt32[] tick = new UInt32[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    tick[i] = values[index - i * step - 1].Tick;
                }
            }

            return tick;
        }

        public UInt32[] GetMarkedTickCount()
        {
            int count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedTickCount");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            UInt32[] tick = new UInt32[count];

            lock (dataLock)
            {
                if (start < stop)
                {
                    for (int i = start; i < stop; i++)
                    {
                        tick[i] = values[i].Tick;
                    }
                }
                else
                {
                    for (int i = start; i < values.Length; i++)
                    {
                        tick[i] = values[i].Tick;
                    }
                    for (int i = 0; i < stop; i++)
                    {
                        tick[i] = values[i].Tick;
                    }
                }
            }

            return tick;
        }

        public DateTime GetDateTime()
        {
            DateTime time;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                time = values[j].Time;
            }

            return time;
        }

        public DateTime[] GetDateTime(int count, int step = 1)
        {
            DateTime[] time = new DateTime[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    int j = index - i * step - 1;
                    if (j < 0) j = values.Length + j;

                    time[i] = values[j].Time;
                }
            }

            return time;
        }

        public DateTime[] GetMarkedDateTime()
        {
            int count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedDateTime");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            DateTime[] time = new DateTime[count];

            lock (dataLock)
            {
                if (start < stop)
                {
                    for (int i = start; i < stop; i++)
                    {
                        time[i] = values[i].Time;
                    }
                }
                else
                {
                    for (int i = start; i < values.Length; i++)
                    {
                        time[i] = values[i].Time;
                    }
                    for (int i = 0; i < stop; i++)
                    {
                        time[i] = values[i].Time;
                    }
                }
            }

            return time;
        }

        public float GetAI(int channel)
        {
            float fValue;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                fValue = values[j].AI[channel];
            }

            return fValue;
        }

        public float[] GetAI(int channel, int count, int step = 1)
        {
            float[] fValues = new float[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    int j = index - i * step - 1;
                    if (j < 0) j = values.Length + j;

                    fValues[i] = values[j].AI[channel];
                }
            }

            return fValues;
        }

        public float[] GetMarkedAI(int channel)
        {
            int count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedAI");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            float[] AI = new float[count];

            lock (dataLock)
            {
                if (start < stop)
                {
                    for (int i = start; i < stop; i++)
                    {
                        AI[i] = values[i].AI[channel];
                    }
                }
                else
                {
                    for (int i = start; i < values.Length; i++)
                    {
                        AI[i] = values[i].AI[channel];
                    }
                    for (int i = 0; i < stop; i++)
                    {
                        AI[i] = values[i].AI[channel];
                    }
                }
            }

            return AI;
        }

        public float GetAO(int channel)
        {
            float fValue;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                fValue = values[j].AO[channel];
            }

            return fValue;
        }

        public float[] GetAO(int channel, int count, int step = 1)
        {
            float[] fValues = new float[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    int j = index - i * step - 1;
                    if (j < 0) j = values.Length + j;

                    fValues[i] = values[j].AO[channel];
                }
            }

            return fValues;
        }

        public float[] GetMarkedAO(int channel)
        {
            int i, j, count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedAO");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            float[] AO = new float[count];

            lock (dataLock)
            {
                j = 0;

                if (start < stop)
                {
                    for (i = start; i < stop; i++)
                    {
                        AO[j++] = values[i].AO[channel];
                    }
                }
                else
                {
                    for (i = start; i < values.Length; i++)
                    {
                        AO[j++] = values[i].AO[channel];
                    }
                    for (i = 0; i < stop; i++)
                    {
                        AO[j++] = values[i].AO[channel];
                    }
                }
            }

            return AO;
        }

        public float GetENC(int channel)
        {
            float fValue;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                fValue = values[j].ENC[channel];
            }

            return fValue;
        }

        public float[] GetENC(int channel, int count, int step = 1)
        {
            float[] fValues = new float[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    int j = index - i * step - 1;
                    if (j < 0) j = values.Length + j;

                    fValues[i] = values[j].ENC[channel];
                }
            }

            return fValues;
        }

        public float[] GetMarkedENC(int channel)
        {
            int count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedENC");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            float[] ENC = new float[count];

            lock (dataLock)
            {
                if (start < stop)
                {
                    for (int i = start; i < stop; i++)
                    {
                        ENC[i] = values[i].ENC[channel];
                    }
                }
                else
                {
                    for (int i = start; i < values.Length; i++)
                    {
                        ENC[i] = values[i].ENC[channel];
                    }
                    for (int i = 0; i < stop; i++)
                    {
                        ENC[i] = values[i].ENC[channel];
                    }
                }
            }

            return ENC;
        }

        public bool GetDI(int channel)
        {
            bool value;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                value = values[j].DI[channel];
            }

            return value;
        }

        public bool[] GetDI(int channel, int count, int step = 1)
        {
            bool[] bValues = new bool[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    int j = index - i * step - 1;
                    if (j < 0) j = values.Length + j;

                    bValues[i] = values[j].DI[channel];
                }
            }

            return bValues;
        }

        public bool[] GetMarkedDI(int channel)
        {
            int count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedDI");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            bool[] DI = new bool[count];

            lock (dataLock)
            {
                if (start < stop)
                {
                    for (int i = start; i < stop; i++)
                    {
                        DI[i] = values[i].DI[channel];
                    }
                }
                else
                {
                    for (int i = start; i < values.Length; i++)
                    {
                        DI[i] = values[i].DI[channel];
                    }
                    for (int i = 0; i < stop; i++)
                    {
                        DI[i] = values[i].DI[channel];
                    }
                }
            }

            return DI;
        }

        public bool GetDO(int channel)
        {
            bool value;

            lock (dataLock)
            {
                int j = index - 1;
                if (j < 0) j = values.Length + j;

                value = values[j].DO[channel];
            }

            return value;
        }

        public bool[] GetDO(int channel, int count, int step = 1)
        {
            bool[] bValues = new bool[count];

            lock (dataLock)
            {
                for (int i = 0; i < count; i++)
                {
                    int j = index - i * step - 1;
                    if (j < 0) j = values.Length + j;

                    bValues[i] = values[j].DO[channel];
                }
            }

            return bValues;
        }

        public bool[] GetMarkedDO(int channel)
        {
            int count;

            if (marking == true)
            {
                throw new Exception("Marking is enabled error in UlTcResource::GetMarkedDO");
            }

            if (start < stop) count = stop - start;
            else count = values.Length - start + stop;

            bool[] DO = new bool[count];

            lock (dataLock)
            {
                if (start < stop)
                {
                    for (int i = start; i < stop; i++)
                    {
                        DO[i] = values[i].DO[channel];
                    }
                }
                else
                {
                    for (int i = start; i < values.Length; i++)
                    {
                        DO[i] = values[i].DO[channel];
                    }
                    for (int i = 0; i < stop; i++)
                    {
                        DO[i] = values[i].DO[channel];
                    }
                }
            }

            return DO;
        }
    }

    public class UlTcResourceValue
    {
        public UInt32 Tick;
        public DateTime Time;

        public float[] AI;
        public float[] AO;
        public float[] ENC;
        public UlBinSets DI;
        public UlBinSets DO;

        public UlTcResourceValue(int aiLen, int aoLen, int encLen, int diLen, int doLen)
        {
            Tick = 0;
            Time = DateTime.Today;
            AI = new float[aiLen];
            AO = new float[aoLen];
            ENC = new float[encLen];
            DI = new UlBinSets(diLen);
            DO = new UlBinSets(doLen);
        }

        public void SetTime(UInt32 lo, UInt32 hi)
        {
            Time = DateTime.FromFileTime((long)((UInt64)lo + ((UInt64)hi << 32)));
        }
    }
}
