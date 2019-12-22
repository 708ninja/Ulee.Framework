//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : TwinCAT PLC ADS Connection Class
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;

using TwinCAT.Ads;

namespace Ulee.Device.Connection.TwinCAT
{
    enum ETc2RunTime
    {
        RT1 = 801,
        RT2 = 811,
        RT3 = 821,
        RT4 = 831
    }

    enum ETc3RunTime
    {
        RT1 = 851,
        RT2 = 852,
        RT3 = 853,
        RT4 = 854,
        RT5 = 855,
        RT6 = 856,
        RT7 = 857,
        RT8 = 858
    }

    public delegate void TcReadHandler(AdsBinaryReader reader);
    public delegate void TcWriteHandler(AdsBinaryWriter writer);

    public class UlTcAdsClient : IDisposable
    {
        private string netId;
        private int port;

        private TcAdsClient client;
        private Dictionary<string, int> handles;

        private object criticalLock;

        public UlTcAdsClient(string netId="", int port = (int)ETc3RunTime.RT1)
        {
            if (netId.ToUpper() == "LOCALHOST") netId = "";

            this.netId = netId;
            this.port = port;

            client = new TcAdsClient();
            handles = new Dictionary<string, int>();

            criticalLock = new object();
        }

        ~UlTcAdsClient()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue == false)
            {
                if (disposing == true)
                {
                    client.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public void Connect()
        {
            lock (criticalLock)
            {
                try
                {
                    if (netId == "")
                    {
                        client.Connect(port);
                    }
                    else
                    {
                        client.Connect(netId, port);
                    }
                }
                catch (Exception e)
                {
                    string str = string.Format("Occurred exception({0}) in UlTcAdsClient::Connect", e.ToString());
                    throw new Exception(str);
                }
            }
        }

        public void Disconnect()
        {
            lock (criticalLock)
            {
                try
                {
                    foreach (KeyValuePair<string, int> dic in handles)
                    {
                        client.DeleteVariableHandle(dic.Value);
                    }

                    handles.Clear();
                }
                catch (Exception e)
                {
                    string str = string.Format("Occurred exception({0}) in UlTcAdsClient::Disconnect", e.ToString());
                    throw new Exception(str);
                }
            }
        }

        public bool IsConnected()
        {
            return client.IsConnected;
        }

        public int GetHandle(string name)
        {
            if (IsConnected() == false)
            {
                throw new Exception("ADS connection is not established in UlTcAdsClient::GetHandle");
            }

            lock (criticalLock)
            {
                if (handles.ContainsKey(name) == false)
                {
                    try
                    {
                        int handle = client.CreateVariableHandle(name);
                        handles.Add(name, handle);
                    }
                    catch (Exception e)
                    {
                        string str = string.Format("Occurred exception({0}) in UlTcAdsClient::GetHandle", e.ToString());
                        throw new Exception(str);
                    }
                }
            }

            return handles[name];
        }

        public void Read(string name, TcReadHandler handler, int length, int position=0)
        {
            int handle = GetHandle(name);

            if (length < 1)
            {
                throw new Exception("Reading length must be over 0 in UlTcAdsClient::Read");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        try
                        {
                            client.Read(handle, stream);
                            stream.Position = position;

                            handler(reader);
                        }
                        catch (Exception e)
                        {
                            string str = string.Format("Occurred exception({0}) in UlTcAdsClient::Read", e.ToString());
                            throw new Exception(str);
                        }
                    }
                }
            }
        }

        public void Write(string name, TcWriteHandler handler)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        try
                        {
                            handler(writer);
                            client.Write(handle, stream);
                        }
                        catch (Exception e)
                        {
                            string str = string.Format("Occurred exception({0}) in UlTcAdsClient::Write", e.ToString());
                            throw new Exception(str);
                        }
                    }
                }
            }
        }

        public bool ReadBoolean(string name)
        {
            bool value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(bool)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadBoolean();
                    }
                }
            }

            return value;
        }

        public bool[] ReadBoolean(string name, int length)
        {
            bool[] value = new bool[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadBoolean");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(bool) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadBoolean();
                        }
                    }
                }
            }

            return value;
        }

        public byte ReadByte(string name)
        {
            byte value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(byte)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadByte();
                    }
                }
            }

            return value;
        }

        public byte[] ReadByte(string name, int length)
        {
            byte[] value;
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadByte");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(byte) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadBytes(length);
                    }
                }
            }

            return value;
        }

        public Int16 ReadInt16(string name)
        {
            Int16 value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(Int16)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadInt16();
                    }
                }
            }

            return value;
        }

        public Int16[] ReadInt16(string name, int length)
        {
            Int16[] value = new Int16[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadInt16");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(Int16) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadInt16();
                        }
                    }
                }
            }

            return value;
        }

        public Int32 ReadInt32(string name)
        {
            Int32 value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(Int32)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadInt32();
                    }
                }
            }

            return value;
        }

        public Int32[] ReadInt32(string name, int length)
        {
            Int32[] value = new Int32[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadInt32");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(Int32) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadInt32();
                        }
                    }
                }
            }

            return value;
        }

        public Int64 ReadInt64(string name)
        {
            Int64 value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(Int64)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadInt64();
                    }
                }
            }

            return value;
        }

        public Int64[] ReadInt64(string name, int length)
        {
            Int64[] value = new Int64[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadInt64");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(Int64) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadInt64();
                        }
                    }
                }
            }

            return value;
        }

        public UInt16 ReadUInt16(string name)
        {
            UInt16 value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(UInt16)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadUInt16();
                    }
                }
            }

            return value;
        }

        public UInt16[] ReadUInt16(string name, int length)
        {
            UInt16[] value = new UInt16[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadUInt16");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(UInt16) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadUInt16();
                        }
                    }
                }
            }

            return value;
        }

        public UInt32 ReadUInt32(string name)
        {
            UInt32 value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(UInt32)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadUInt32();
                    }
                }
            }

            return value;
        }

        public UInt32[] ReadUInt32(string name, int length)
        {
            UInt32[] value = new UInt32[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadUInt32");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(UInt32) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadUInt32();
                        }
                    }
                }
            }

            return value;
        }

        public UInt64 ReadUInt64(string name)
        {
            UInt64 value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(UInt64)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadUInt64();
                    }
                }
            }

            return value;
        }

        public UInt64[] ReadUInt64(string name, int length)
        {
            UInt64[] value = new UInt64[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::ReadUInt64");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(UInt64) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadUInt64();
                        }
                    }
                }
            }

            return value;
        }

        public float ReadFloat(string name)
        {
            float value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(float)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadSingle();
                    }
                }
            }

            return value;
        }

        public float[] ReadFloat(string name, int length)
        {
            float[] value = new float[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::GetFloat");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(float) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadSingle();
                        }
                    }
                }
            }

            return value;
        }

        public double ReadDouble(string name)
        {
            double value;
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(double)))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);
                        value = reader.ReadDouble();
                    }
                }
            }

            return value;
        }

        public double[] ReadDouble(string name, int length)
        {
            double[] value = new double[length];
            int handle = GetHandle(name);

            if (length < 2)
            {
                throw new Exception("Reading length must be over 1 in UlTcAdsClient::GetDouble");
            }

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream(sizeof(double) * length))
                {
                    using (AdsBinaryReader reader = new AdsBinaryReader(stream))
                    {
                        client.Read(handle, stream);

                        for (int i = 0; i < length; i++)
                        {
                            value[i] = reader.ReadDouble();
                        }
                    }
                }
            }

            return value;
        }

        public void WriteBoolean(string name, bool value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteBoolean(string name, bool[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteByte(string name, byte value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteByte(string name, byte[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteInt16(string name, Int16 value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteInt16(string name, Int16[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteInt32(string name, Int32 value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteInt32(string name, Int32[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteInt64(string name, Int64 value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteInt64(string name, Int64[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteUInt16(string name, UInt16 value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteUInt16(string name, UInt16[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteUInt32(string name, UInt32 value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteUInt32(string name, UInt32[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteUInt64(string name, UInt64 value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteUInt64(string name, UInt64[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteFloat(string name, float value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                    }

                    client.Write(handle, stream);
                }
            }
        }

        public void WriteFloat(string name, float[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }

        public void WriteDouble(string name, double value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        writer.Write(value);
                    }

                    client.Write(handle, stream);
                }
            }
        }

        public void WriteDouble(string name, double[] value)
        {
            int handle = GetHandle(name);

            lock (criticalLock)
            {
                using (AdsStream stream = new AdsStream())
                {
                    using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                    {
                        for (int i = 0; i < value.Length; i++)
                        {
                            writer.Write(value[i]);
                        }

                        client.Write(handle, stream);
                    }
                }
            }
        }
    }
}
