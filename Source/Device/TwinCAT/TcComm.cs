using System;
using System.Collections.Generic;
using TwinCAT.Ads;
using System.IO;
using System.Diagnostics;

namespace OxLib.Device.Plc
{
    public enum ETcRunTime
    {
        RT1 = 801,
        RT2 = 811,
        RT3 = 821,
        RT4 = 831
    }

    public class TcComm : IDisposable
    {
        class TcConnection
        {
            public TcAdsClient client;
            public int handle;
        }

        private  ETcRunTime runTime = ETcRunTime.RT1;
        private int hblock = 0;
        private readonly object padlock = new object();
        private TcAdsClient client = null;

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

        ~TcComm()
        {
            Dispose(false);
        }

        #endregion

        private static volatile Dictionary<string, TcConnection> cache = new Dictionary<string, TcConnection>();
        private static readonly object cacheLock = new object();

        public delegate void TcWriteFunc(BinaryWriter wr);
        public delegate void TcReadFunc(BinaryReader rd);

        public TcComm()
        {
        }

        public TcComm(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            this.runTime = runTime;
            Open(name);
        }

        public void Close()
        {
            //if (client != null)
            //{
            //    client.DeleteVariableHandle(hblock);
            //    hblock = 0;

            //    client.Dispose();
            //    client = null;
            //}
        }

        public static void CloseCache()
        {
            lock (cacheLock)
            {
                foreach (TcConnection tc in cache.Values)
                {
                    tc.client.DeleteVariableHandle(tc.handle);
                    tc.client.Dispose();
                }

                cache.Clear();
            }
        }

        public void Open(string name)
        {
            lock (padlock)
            {
                lock (cacheLock)
                {
                    //If a connection to this variable does not exist, create it
                    TcConnection tc;
                    if (!cache.TryGetValue(name, out tc) || !tc.client.IsConnected )
                    {
                        tc = new TcConnection();
                        tc.client = new TcAdsClient();
                        tc.client.Connect((int)runTime);
                        tc.handle = tc.client.CreateVariableHandle(name);
                        cache[name] = tc;
                    }
                    //Use the cached connection
                    client = tc.client;
                    hblock = tc.handle;
                }
            }
        }

        public void Write(TcWriteFunc writeFunc)
        {
            lock (padlock)
            {
                if (hblock == 0)
                    throw new Exception("Ads client is not open");
                using (AdsStream str = new AdsStream())
                {
                    using (BinaryWriter wr = new BinaryWriter(str))
                    {
                        try
                        {
                            writeFunc(wr);
                            client.Write(hblock, str);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            throw;
                        }
                    }
                }
            }
        }

        public void Read(TcReadFunc readFunc, int byteSize)
        {
            lock (padlock)
            {
                if (hblock == 0)
                    throw new Exception("Ads client is not open");

                using (AdsStream str = new AdsStream(byteSize))
                {
                    using (BinaryReader rd = new BinaryReader(str))
                    {
                        try
                        {
                            //Read the whole parameter block into our stream
                            client.Read(hblock, str);

                            //Read data from buffer
                            readFunc(rd);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            throw;
                        }
                    }
                }
            }
        }

        public void WriteBool(bool b)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write((byte)(b ? 1 : 0));
            };
            Write(del);
        }

        public void WriteWord(UInt16 w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write(w);
            };
            Write(del);
        }

        public void WriteInt(Int16 w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write(w);
            };
            Write(del);
        }

        public void WriteDWord(UInt32 w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write(w);
            };
            Write(del);
        }

        public void WriteDInt(Int32 w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write(w);
            };
            Write(del);
        }

        public void WriteReal(float w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write(w);
            };
            Write(del);
        }

        public void WriteReal(double w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                wr.Write((float)w);
            };
            Write(del);
        }

        public void WriteRealArray(float[] w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                for (int i = 0; i < w.Length; ++i)
                    wr.Write(w[i]);
            };
            Write(del);
        }

        public void WriteRealArray(float[,] w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                for (int i = 0; i < w.GetLength(0); ++i)
                    for (int j = 0; j < w.GetLength(1); ++j)
                        wr.Write(w[i, j]);
            };
            Write(del);
        }

        public void WriteRealArray(double[] w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                for (int i = 0; i < w.Length; ++i)
                    wr.Write((float)w[i]);
            };
            Write(del);
        }

        public void WriteRealArray(double[,] w)
        {
            TcWriteFunc del = delegate(BinaryWriter wr)
            {
                for (int i = 0; i < w.GetLength(0); ++i)
                    for (int j = 0; j < w.GetLength(1); ++j)
                        wr.Write((float)w[i, j]);
            };
            Write(del);
        }

        public bool ReadBool()
        {
            bool b = false;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                b = (rd.ReadByte() != 0);
            };
            Read(del, 1);
            return b;
        }

        public UInt16 ReadWord()
        {
            UInt16 w = 0;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = rd.ReadUInt16();
            };
            Read(del, 2);
            return w;
        }

        public Int16 ReadInt()
        {
            Int16 w = 0;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = rd.ReadInt16();
            };
            Read(del, 2);
            return w;
        }

        public UInt32 ReadDWord()
        {
            UInt32 w = 0;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = rd.ReadUInt32();
            };
            Read(del, 4);
            return w;
        }

        public Int32 ReadDInt()
        {
            Int32 w = 0;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = rd.ReadInt32();
            };
            Read(del, 4);
            return w;
        }

        public float ReadReal()
        {
            float w = 0.0F;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = rd.ReadSingle();
            };
            Read(del, 4);
            return w;
        }

        public static int[] ReadUInt16ArrayAsInt(BinaryReader rd, int d1)
        {
            int[] ia = new int[d1];
            byte[] ba = rd.ReadBytes(d1 * 2);
            for (int i = 0; i < d1; ++i)
                ia[i] = BitConverter.ToUInt16(ba, i * 2);
            return ia;
        }

        public static int[] ReadInt16ArrayAsInt(BinaryReader rd, int d1)
        {
            int[] ia = new int[d1];
            byte[] ba = rd.ReadBytes(d1 * 2);
            for (int i = 0; i < d1; ++i)
                ia[i] = BitConverter.ToInt16(ba, i * 2);
            return ia;
        }

        public static float[] ReadRealArray(BinaryReader rd, int d1)
        {
            float[] da = new float[d1];
            byte[] ba = rd.ReadBytes(d1 * 4);
            for (int i = 0; i < d1; ++i)
                da[i] = BitConverter.ToSingle(ba, i * 4);
            return da;
        }

        public static float[,] ReadRealArray(BinaryReader rd, int d1, int d2)
        {
            float[,] da = new float[d1, d2];
            byte[] ba = rd.ReadBytes(d1 * d2 * 4);
            for (int i = 0; i < d1; ++i)
                for (int j = 0; j < d2; ++j)
                    da[i, j] = BitConverter.ToSingle(ba, d2 * i * 4 + j * 4);
            return da;
        }

        public static double[] ReadRealArrayAsDouble(BinaryReader rd, int d1)
        {
            double[] da = new double[d1];
            byte[] ba = rd.ReadBytes(d1 * 4);
            for (int i = 0; i < d1; ++i)
                da[i] = BitConverter.ToSingle(ba, i * 4);
            return da;
        }

        public static double[,] ReadRealArrayAsDouble(BinaryReader rd, int d1, int d2)
        {
            double[,] da = new double[d1,d2];
            byte[] ba = rd.ReadBytes(d1 * d2 * 4);
            for (int i = 0; i < d1; ++i)
                for (int j = 0; j < d2; ++j)
                da[i, j] = BitConverter.ToSingle(ba, d2 * i * 4 + j * 4 );
            return da;
        }

        public float[] ReadRealArray(int d1)
        {
            float[] w = null;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = TcComm.ReadRealArray(rd, d1);
            };
            Read(del, 4 * d1);
            return w;
        }

        public float[,] ReadRealArray(int d1, int d2)
        {
            float[,] w = null;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = TcComm.ReadRealArray(rd, d1, d2);
            };
            Read(del, 4 * d1 * d2);
            return w;
        }

        public double[] ReadRealArrayAsDouble(int d1)
        {
            double[] w = null;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = TcComm.ReadRealArrayAsDouble(rd, d1);
            };
            Read(del, 4 * d1);
            return w;
        }

        public double[,] ReadRealArrayAsDouble(int d1, int d2)
        {
            double[,] w = null;
            TcReadFunc del = delegate(BinaryReader rd)
            {
                w = TcComm.ReadRealArrayAsDouble(rd, d1, d2);
            };
            Read(del, 4 * d1 * d2);
            return w;
        }

        public static void WriteBool(string name, bool b, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteBool(b);
            }
        }

        public static void WriteWord(string name, UInt16 w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteWord(w);
            }
        }

        public static void WriteInt(string name, Int16 w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteInt(w);
            }
        }

        public static void WriteDWord(string name, UInt32 w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteDWord(w);
            }
        }

        public static void WriteDInt(string name, Int32 w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteDInt(w);
            }
        }

        public static void WriteReal(string name, float w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteReal(w);
            }
        }

        public static void WriteRealArray(string name, float[] w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteRealArray(w);
            }
        }

        public static void WriteRealArray(string name, float[,] w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteRealArray(w);
            }
        }

        public static void WriteRealArray(string name, double[] w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteRealArray(w);
            }
        }

        public static void WriteRealArray(string name, double[,] w, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                tc.WriteRealArray(w);
            }
        }

        public static bool ReadBool(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadBool();
            }
        }

        public static UInt16 ReadWord(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadWord();
            }
        }

        public static Int16 ReadInt(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadInt();
            }
        }

        public static UInt32 ReadDWord(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadDWord();
            }
        }

        public static Int32 ReadDInt(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadDInt();
            }
        }

        public static float ReadReal(string name, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadReal();
            }
        }

        public static float[,] ReadRealArray(string name, int d1, int d2, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadRealArray(d1, d2);
            }
        }

        public static float[] ReadRealArray(string name, int d1, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadRealArray(d1);
            }
        }

        public static double[,] ReadRealArrayAsDouble(string name, int d1, int d2, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadRealArrayAsDouble(d1, d2);
            }
        }

        public static double[] ReadRealArrayAsDouble(string name, int d1, ETcRunTime runTime = ETcRunTime.RT1)
        {
            using (TcComm tc = new TcComm(name, runTime))
            {
                return tc.ReadRealArrayAsDouble(d1);
            }
        }
    }
}
