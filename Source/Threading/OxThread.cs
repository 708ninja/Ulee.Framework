//------------------------------------------------------------------------------
using System; 
using System.Diagnostics; 
using System.Threading;

using OxLib.DllImport;

namespace OxLib.Threading
{
    //--------------------------------------------------------------------------
    public enum EOxYieldType
    {
        Sleep,
        SpinWait,
        SwitchToThread
    }

    //--------------------------------------------------------------------------
    public abstract class OxThread
    {
        private volatile bool suspended;
        private volatile bool terminated;
        private EOxYieldType yieldType;

        protected Thread thread;
        protected Stopwatch watch;

        protected abstract void Execute();

        //----------------------------------------------------------------------
        protected OxThread()
        {
            thread = null;
            suspended = true;
            terminated = false;
            yieldType = EOxYieldType.Sleep;

            watch = new Stopwatch();
            watch.Start();
            
            thread = new Thread(Execute);
            thread.Start();
        }

        //----------------------------------------------------------------------
        protected bool Terminated
        {
            get { return terminated; }
        }

        //----------------------------------------------------------------------
        public EOxYieldType YieldType
        {
            get { return yieldType; }
            set { yieldType = value; }
        }

        //----------------------------------------------------------------------
        public ThreadPriority Priority
        {
            get { return thread.Priority; }
            set { thread.Priority = value; }
        }

        //----------------------------------------------------------------------
        public long Frequency
        {
            get { return Stopwatch.Frequency; }
        }

        //----------------------------------------------------------------------
        public long MillisecondTicks
        {
            get { return Stopwatch.Frequency / 1000; }
        }

        //----------------------------------------------------------------------
        public long MicrosecondTicks
        {
            get { return Stopwatch.Frequency / 1000000; }
        }

        //----------------------------------------------------------------------
        public long NanosecondTicks
        {
            get { return Stopwatch.Frequency / 1000000000; }
        }

        //----------------------------------------------------------------------
        public TimeSpan Elapsed
        {
            get { return watch.Elapsed; }
        }

        //----------------------------------------------------------------------
        public long ElapsedTicks
        {
            get { return watch.ElapsedTicks; }
        }

        //----------------------------------------------------------------------
        public long ElapsedMilliseconds
        {
            get { return watch.ElapsedMilliseconds; }
        }

        //----------------------------------------------------------------------
        public bool IsTimeoutTicks(long ABeginTicks, long ADelayTicks)
        {
            if ((watch.ElapsedTicks - ABeginTicks) > ADelayTicks)
            {
                return true;
            }

            return false;
        }

        //----------------------------------------------------------------------
        public bool IsTimeoutMilliseconds(long ABeginTime, long ADelayTime)
        {
            if ((watch.ElapsedMilliseconds - ABeginTime) > ADelayTime)
            {
                return true;
            }

            return false;
        }

        //----------------------------------------------------------------------
        public void Suspend()
        {
            if (suspended == false)
            {
                suspended = true;
            }
        }

        //----------------------------------------------------------------------
        public void Resume()
        {
            if (suspended == true)
            {
                suspended = false;
            }
        }

		//----------------------------------------------------------------------
		public void Terminate(bool aWaitTermination=true)
		{
			if (aWaitTermination == true)
			{
				Resume();
				terminated = true;
				WaitFor();
			}
			else
			{
				terminated = true;
			}
		}

        //----------------------------------------------------------------------
        public void WaitFor()
        {
            thread.Join();
        }

        //----------------------------------------------------------------------
        protected void Sleep(int AValue)
        {
            switch (yieldType)
            {
                case EOxYieldType.Sleep:
                    Thread.Sleep(AValue);
                    break;

                case EOxYieldType.SpinWait:
                    Thread.SpinWait(AValue);
                    break;

                case EOxYieldType.SwitchToThread:
                    Win32.SwitchToThread();
                    break;
            }
        }

        //----------------------------------------------------------------------
        protected void Yield(int AValue=1)
        {
            if (suspended == true)
            {
                while (suspended == true)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                Sleep(AValue);
            }
        }
    }
}
