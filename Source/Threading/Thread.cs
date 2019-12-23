//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Thread Class
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

using Ulee.DllImport.Win32;

//------------------------------------------------------------------------------
namespace Ulee.Threading
{
    //--------------------------------------------------------------------------
    public enum EUlYieldType
    {
        Sleep,
        SpinWait,
        SwitchToThread
    }

    //--------------------------------------------------------------------------
    public class UlThreadTerminatedException : ApplicationException
    {
        public int Code { get; private set; }

        public UlThreadTerminatedException(int code=0)
            : base("Occurred Terminated Exception!")
        {
            Code = code;
        }

        public UlThreadTerminatedException(string aMsg, int code=0)
            : base(aMsg)
        {
            Code = code;
        }
    }

    //--------------------------------------------------------------------------
    public abstract class UlThread
    {
        private EUlYieldType yieldType;
        private bool terminateException;
        private int terminateCode;

        protected volatile bool suspended;
        protected volatile bool terminated;

        protected Thread thread;
        protected Stopwatch Watch { get; private set; }

        protected abstract void Execute();

        //----------------------------------------------------------------------
        protected UlThread(bool start=true, bool terminateException=false)
        {
            this.thread = null;
            this.suspended = false;
            this.terminated = false;
            this.yieldType = EUlYieldType.Sleep;
            this.terminateException = terminateException;
            this.terminateCode = 0;

            Watch = new Stopwatch();
            thread = new Thread(Execute);

            if (start == true) Start();
        }

        //----------------------------------------------------------------------
        public bool IsAlive
        {
            get { return thread.IsAlive; }
        }

        //----------------------------------------------------------------------
        public bool Suspended
        {
            get { return suspended; }
        }

        //----------------------------------------------------------------------
        protected bool Terminated
        {
            get { return terminated; }
        }

        //----------------------------------------------------------------------
        public EUlYieldType YieldType
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
            get { return Watch.Elapsed; }
        }

        //----------------------------------------------------------------------
        public long ElapsedTicks
        {
            get { return Watch.ElapsedTicks; }
        }

        //----------------------------------------------------------------------
        public long ElapsedMilliseconds
        {
            get { return Watch.ElapsedMilliseconds; }
        }

        //----------------------------------------------------------------------
        public bool IsTimeoutTicks(long ABeginTicks, long ADelayTicks)
        {
            if ((Watch.ElapsedTicks - ABeginTicks) > ADelayTicks)
            {
                return true;
            }

            return false;
        }

        //----------------------------------------------------------------------
        public bool IsTimeoutMilliseconds(long ABeginTime, long ADelayTime)
        {
            if ((Watch.ElapsedMilliseconds - ABeginTime) > ADelayTime)
            {
                return true;
            }

            return false;
        }

        //----------------------------------------------------------------------
        public void Start()
        {
            if (IsAlive == false)
            {
                Watch.Restart();
                thread.Start();

                while (thread.IsAlive == false)
                {
                    Yield(1);
                }

                suspended = false;
            }
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
            if (IsAlive == false) Start();
            else
            {
                if (suspended == true)
                {
                    suspended = false;
                }
            }
        }

        //----------------------------------------------------------------------
        public void Terminate(int code=0, bool aWaitTermination=true)
        {
            Resume();

            terminateCode = code;
            terminated = true;

            if (aWaitTermination == true)
            {
                WaitFor();
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
                case EUlYieldType.Sleep:
                    Thread.Sleep(AValue);
                    break;

                case EUlYieldType.SpinWait:
                    Thread.SpinWait(AValue);
                    break;

                case EUlYieldType.SwitchToThread:
                    Win32.SwitchToThread();
                    break;
            }
        }

        //----------------------------------------------------------------------
        protected virtual void Yield(int AValue = 1)
        {
            long beginTime = ElapsedMilliseconds;

            if (suspended == true)
            {
                Watch.Stop();

                while (suspended == true)
                {
                    Thread.Sleep(1);
                }

                Watch.Start();
            }
            else
            {
                do
                {
                    Sleep(1);
                    DoTerminatedException();
                } while (IsTimeoutMilliseconds(beginTime, AValue) == false);
            }
        }

        protected void DoTerminatedException()
        {
            if (terminated == true)
            {
                if (terminateException == true)
                {
                    if (terminateCode != 0) terminated = false;

                    throw new UlThreadTerminatedException(terminateCode);
                }
            }
        }
    }

    //--------------------------------------------------------------------------
    public abstract class UlPlcMasterThread : UlThread
    {
        private List<UlPlcSlaveThread> slaves;

        private Stopwatch scanWatch;

        public long ScanTime
        {
            get
            {
                long msec = 0;

                lock (scanWatch)
                {
                    msec = scanWatch.ElapsedMilliseconds;
                }

                return msec;
            }
        }

        protected UlPlcMasterThread(bool start = false, bool terminateException = true)
            : base(start, terminateException)
        {
            scanWatch = new Stopwatch();
            slaves = new List<UlPlcSlaveThread>();
        }

        protected void StartWatch()
        {
            lock (scanWatch)
            {
                scanWatch.Restart();
            }
        }

        protected void StopWatch()
        {
            lock (scanWatch)
            {
                scanWatch.Stop();
            }
        }

        public void AddSlave(UlPlcSlaveThread slave)
        {
            slaves.Add(slave);
        }

        //----------------------------------------------------------------------
        public void ResumeSlaves()
        {
            foreach (UlPlcSlaveThread slave in slaves)
            {
                slave.Resume();
            }
        }

        //----------------------------------------------------------------------
        public void SuspendSlaves()
        {
            foreach (UlPlcSlaveThread slave in slaves)
            {
                slave.Suspend();
            }
        }

        //----------------------------------------------------------------------
        public void TerminateSlaves()
        {
            foreach (UlPlcSlaveThread slave in slaves)
            {
                slave.Terminate();
            }
        }

        //----------------------------------------------------------------------
        protected void ExecuteSlaves()
        {
            foreach (UlPlcSlaveThread slave in slaves)
            {
                slave.Set();
            }
        }

        //----------------------------------------------------------------------
        protected void WaitOneSlaves()
        {
            foreach (UlPlcSlaveThread slave in slaves)
            {
                slave.WaitOne(false);
            }
        }
    }

    //--------------------------------------------------------------------------
    public abstract class UlPlcSlaveThread : UlThread
    {
        private ManualResetEvent notificationEvent;

        public bool Notified
        { get { return notificationEvent.WaitOne(0); } }

        public void Set()
        {
            notificationEvent.Set();
        }

        public void Reset()
        {
            notificationEvent.Reset();
        }

        public void WaitOne(bool active)
        {
            while (Notified != active)
            {
                DoTerminatedException();
                Sleep(1);
            }
        }

        //----------------------------------------------------------------------
        protected UlPlcSlaveThread(bool start = false, bool terminateException = true)
            : base(start, terminateException)
        {
            notificationEvent = new ManualResetEvent(false);
        }

        //----------------------------------------------------------------------
        protected override void Yield(int AValue = 1)
        {
            long beginTime = ElapsedMilliseconds;

            DoTerminatedException();

            if (suspended == true)
            {
                Watch.Stop();

                while (suspended == true)
                {
                    WaitOne(true);
                    Reset();
                    Thread.Sleep(1);
                }

                Watch.Start();
            }
            else
            {
                do
                {
                    WaitOne(true);
                    Reset();
                    Sleep(1);
                } while (IsTimeoutMilliseconds(beginTime, AValue) == false);
            }
        }
    }
}
//------------------------------------------------------------------------------
