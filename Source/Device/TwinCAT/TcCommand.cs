using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace OxLib.Device.Plc
{
    public class TcCommand : IDisposable
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

        ~TcCommand()
        {
            Dispose(false);
        }

        #endregion

        TcComm tcTx;
        TcComm tcRx;

        string reqName;
        string respName;
        UInt16 counter = 0;
        
        CommandData cData = new CommandData();
        public CommandData CData
        {
            get { return cData; }
            set { cData = value; }
        }

        CommandData rData = new CommandData();
        public CommandData RData
        {
            get { return rData; }
            set { rData = value; }
        }

        public class CommandData
        {
            int command;
            public int Command
            {
                get { return command; }
                set { command = value; }
            }

            UInt16 counter;
            public UInt16 Counter
            {
                get { return counter; }
                set { counter = value; }
            }

            double farg1 = 0;
            public double Farg1
            {
                get { return farg1; }
                set { farg1 = value; }
            }

            double farg2 = 0;
            public double Farg2
            {
                get { return farg2; }
                set { farg2 = value; }
            }

            double farg3 = 0;
            public double Farg3
            {
                get { return farg3; }
                set { farg3 = value; }
            }

            double farg4 = 0;
            public double Farg4
            {
                get { return farg4; }
                set { farg4 = value; }
            }

            int iarg1 = 0;
            public int Iarg1
            {
                get { return iarg1; }
                set { iarg1 = value; }
            }

            int iarg2 = 0;
            public int Iarg2
            {
                get { return iarg2; }
                set { iarg2 = value; }
            }

            int iarg3 = 0;
            public int Iarg3
            {
                get { return iarg3; }
                set { iarg3 = value; }
            }

            int iarg4 = 0;
            public int Iarg4
            {
                get { return iarg4; }
                set { iarg4 = value; }
            }

            int done = 0;
            public int Done
            {
                get { return done; }
                set { done = value; }
            }

            int working = 0;
            public int Working
            {
                get { return working; }
                set { working = value; }
            }

            public void Write(BinaryWriter wr)
            {
                wr.Write((UInt16)(command & 0xffff));
                wr.Write(counter);
                wr.Write((float)farg1);
                wr.Write((float)farg2);
                wr.Write((float)farg3);
                wr.Write((float)farg4);
                wr.Write((Int16)iarg1);
                wr.Write((Int16)iarg2);
                wr.Write((Int16)iarg3);
                wr.Write((Int16)iarg4);
                wr.Write((Int16)(done & 0xffff));
                wr.Write((UInt16)(working & 0xffff));
            }

            public void Read(BinaryReader rd)
            {
                command = rd.ReadUInt16();
                counter = rd.ReadUInt16();
                farg1 = rd.ReadSingle();
                farg2 = rd.ReadSingle();
                farg3 = rd.ReadSingle();
                farg4 = rd.ReadSingle();
                iarg1 = rd.ReadInt16();
                iarg2 = rd.ReadInt16();
                iarg3 = rd.ReadInt16();
                iarg4 = rd.ReadInt16();
                done = rd.ReadInt16();
                working = rd.ReadUInt16();
            }

            static public int ByteSize
            {
                get { return 32; }
            }
        }

        public TcCommand()
        {
        }

        public void Open(string iAppInterface, ETcRunTime runTime=ETcRunTime.RT1)
        {
            Open(iAppInterface + ".cData", iAppInterface + ".rData", runTime);
        }

        public void Open(string requestName, string responseName, ETcRunTime runTime=ETcRunTime.RT1)
        {
            this.reqName = requestName;
            this.respName = responseName;


            tcTx = new TcComm(reqName, runTime);
            tcRx = new TcComm(respName, runTime);

            //Initialize counter to PLC value
            Receive();
            counter = rData.Counter;
            cData.Counter = counter;
            cData.Command = rData.Command;
        }

        public void Close()
        {
            tcTx.Close();
            tcRx.Close();
        }

        public void Send()
        {
            cData.Counter = ++counter;
            tcTx.Write(cData.Write);
        }

        public void Receive()
        {
            rData = new CommandData();
            tcRx.Read(rData.Read, CommandData.ByteSize);
        }

        public void WaitCompletion(int timeOut)
        {
            //int interval = 0;
            Stopwatch sw = Stopwatch.StartNew();

            for (;;)
            {
                if (IsDone())
                    return;

                if (sw.ElapsedMilliseconds > timeOut)
                    throw new TcCommandException(-1, string.Format("Command {0} timed out with command {1}", respName, cData.Command));

                Thread.Sleep(1);

                //interval += 20;
                //if (interval > 250) interval = 250;

                //Thread.Sleep(interval);
            }
        }

        public bool IsDone()
        {
            Receive();

            if (rData.Command == cData.Command && rData.Counter == cData.Counter)
            {
                if (rData.Done < 0)
                {
                    throw new TcCommandException(
                            rData.Done, string.Format("Command {0} reported error: {1}", 
                            respName, rData.Done));
                }
                else if (rData.Done == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBusy()
        {
            Receive();

            if (rData.Command == cData.Command && rData.Counter == cData.Counter)
                return rData.Done == 0; //Active
            
            return true; //Pending
        }

        public bool TryIsDone(out int doneCode)
        {
            Receive();
            if (rData.Command == cData.Command && rData.Counter == cData.Counter)
            {
                doneCode = rData.Done;
                return rData.Done != 0; //Active
            }
            doneCode = 0;
            return false; //Pending
        }

        public void SendBlocking(int timeout)
        {
            Send();
            WaitCompletion(timeout);
        }

        public void Cancel()
        {
            cData = new CommandData();
            SendBlocking(1000);
        }
    }

    public class TcCommandException : Exception
    {
        public TcCommandException(int errorCode, string message)
            : base(message)
        {
            this.errorCode = errorCode;
        }

        int errorCode = 0;
        public int ErrorCode
        {
            get { return errorCode; }
        }
    }
}
