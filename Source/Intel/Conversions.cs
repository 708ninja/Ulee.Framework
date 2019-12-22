//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : InTel Math Class
//------------------------------------------------------------------------------
using System;

namespace Ulee.Intel
{
    public static class Conversions
    {
        public static double RmsTimeDomain(double[] x)
        {
            double sumsq = 0;

            for (int i = 0; i < x.Length; ++i)
                sumsq += x[i] * x[i];

            return Math.Sqrt(sumsq / x.Length);
        }

        public static double RmsPowerSpectrum(double[] x)
        {
            double sumsq = 0;

            for (int i = 0; i < x.Length; ++i)
                sumsq += x[i] * x[i];

            return Math.Sqrt(Sum(x));
        }

        public static double Sum(double[] x)
        {
            double sum = 0;

            for (int i = 0; i < x.Length; ++i)
                sum += x[i];

            return sum;
        }
    }
}
