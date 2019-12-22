using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Device
{
    public class UlTripState
    {
        public int Code { get; private set; }

        private bool state { get; set; }

        public bool Tripped(bool state)
        {
            if (Code == 0) return false;

            return (this.state == state) ? true : false;
        }

        public UlTripState(int code=0, bool state=false)
        {
            Code = code;
            this.state = state;
        }
    }

    public class UlTripRange
    {
        public int Code { get; private set; }

        private int count { get; set; }

        private int maxCount { get; set; }

        private double minValue { get; set; }

        private double maxValue { get; set; }

        public bool Tripped(double value)
        {
            if (Code == 0)
            {
                count = 0;
                return false;
            }

            bool decision = false;

            if ((value < minValue) || (value > maxValue)) count++;
            else count = 0;

            if (count >= maxValue)
            {
                count = 0;
                decision = true;
            }

            return decision;
        }

        public UlTripRange(int code=0, double min=0, double max=0)
        {
            Code = code;
            count = 0;
            minValue = min;
            maxValue = max;
        }
    }
}
