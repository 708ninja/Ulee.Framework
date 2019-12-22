using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Device
{
    public enum ERelayLogic
    {
        A_NO,                           // A접점(Normal open)
        B_NC                            // B접점(Normal close)
    }

    public class UlRelayTimer
    {
        private Stopwatch sw;

        private bool oldState;

        private bool nowState;

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }

            set
            {
                if (value == enabled) return;

                sw.Reset();
                Q = !LogicQ;
                oldState = false;
                nowState = false;
                enabled = value;
            }
        }

        // Output coil logic
        public bool LogicQ;

        // Input realy logic
        public ERelayLogic LogicIN { get; private set; }

        // Input relay
        public bool IN
        {
            set
            {
                // Timer 동작 중인가?
                if (enabled == true)
                {
                    // 입력로직에 맞게 신호 변환 - A접점 : Normal open, B접점 : Normal close
                    bool state = (LogicIN == ERelayLogic.A_NO) ? value : !value;

                    // Input relay가 On 인가?
                    if (state == true)
                    {
                        // 이전 Input relay가 Off 이면 stopwatch 동작시작
                        if (oldState == false) sw.Start();

                        // 현재출력이 출력로직과 다른가?
                        if (Q != LogicQ)
                        {
                            // 타이머 지연시간을 초과 했는가?
                            if (sw.ElapsedMilliseconds >= PT)
                            {
                                sw.Stop();
                                Q = LogicQ;
                            }
                        }
                    }
                    // Input relay Off 상태
                    else
                    {
                        // 현재출력이 출력로직과 같은가?
                        if (Q == LogicQ)
                        {
                            Q = !LogicQ;
                            sw.Reset();
                        }
                    }

                    oldState = nowState;
                    nowState = state;
                }
                else
                {
                    // 현재출력이 출력로직과 같은가?
                    if (Q == LogicQ) Q = !LogicQ;
                }
            }
        }

        // Present time(msec)
        public long PT { get; set; }

        // Output coil
        public bool Q { get; private set; }

        // Elapsed time(msec)
        public long ET
        {
            get
            {
                if (enabled == false) return 0;
                if (sw.ElapsedMilliseconds >= PT) return PT;

                return sw.ElapsedMilliseconds;
            }
        }

        public UlRelayTimer(long pt=0, bool logicQ=true, ERelayLogic logicIN=ERelayLogic.A_NO, bool enabled=false)
        {
            sw = new Stopwatch();

            LogicIN = logicIN;
            LogicQ = logicQ;
            PT = pt;
            this.enabled = enabled;

            Q = !LogicQ;
            oldState = false;
            nowState = false;
        }
    }
}
