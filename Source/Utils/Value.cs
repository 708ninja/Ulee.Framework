//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Value Class
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Utils
{
    public enum EUlDecision
    {
        None = 0, 
        Ok = 1,
        Ng = -1,
        Fail = -2
    }

    public enum EUlValueComparison
    { None, Equal, GreatEqual, LessEqual, GreatEqualAndLessEqual }

    public class UlDoubleValue
    {
        private EUlDecision decision;
        public EUlDecision Decision
        {
            get { return decision; }
            set { decision = value; }
        }

        private EUlValueComparison comparison;
        public EUlValueComparison Comparison
        {
            get { return comparison; }
            set { comparison = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private double max;
        public double Max
        {
            get { return max; }
            set { max = value; }
        }

        private double min;
        public double Min
        {
            get { return min; }
            set { min = value; }
        }

        private string unit;
        public string Unit
        {
            get { return unit; }
            set { unit = value; }
        }

        private string format;
        public string Format
        {
            get { return format; }
            set { format = value; }
        }

        private double value;
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public UlDoubleValue(
            EUlDecision decision = EUlDecision.None, 
            EUlValueComparison comparison = EUlValueComparison.None, 
            double max = 0, 
            double min = 0, 
            string unit = "", 
            string format = "0.0", 
            double value = 0)
        {
            this.decision = decision;
            this.comparison = comparison;
            this.max = max;
            this.min = min;
            this.unit = unit;
            this.format = format;
            this.value = value;
        }

        public void Decide()
        {
            switch (comparison)
            {
                case EUlValueComparison.None:
                    decision = EUlDecision.None;
                    break;

                case EUlValueComparison.Equal:
                    decision = (value == max) ? EUlDecision.Ok : EUlDecision.Ng;
                    break;

                case EUlValueComparison.GreatEqual:
                    decision = (value >= min) ? EUlDecision.Ok : EUlDecision.Ng;
                    break;

                case EUlValueComparison.LessEqual:
                    decision = (value <= max) ? EUlDecision.Ok : EUlDecision.Ng;
                    break;

                case EUlValueComparison.GreatEqualAndLessEqual:
                    decision = ((value >= min) && (value <= max)) ? EUlDecision.Ok : EUlDecision.Ng;
                    break;
            }
        }
    }
}
