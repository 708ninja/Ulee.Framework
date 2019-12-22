//------------------------------------------------------------------------------
// Copyright (C) 2018 by Seong-Ho, Lee All Rights Reserved.
//------------------------------------------------------------------------------
// Author      : Seong-Ho, Lee
// E-Mail      : 708ninja@naver.com
// Tab Size    : 4 Column
// Date        : 2018/03/28
// Language    : Visual Studio 2017 C# for .NET 4.6.1
// Description : Intel Math Class
//------------------------------------------------------------------------------
using System;

using Ulee.Intel.Ipp;

namespace Ulee.Intel
{
    public static class Filters
    {
        public static unsafe double[] Fir(double[] src, double[] taps, bool shift)
        {
            double[] dst = new double[src.Length];
            double[] delLine = new double[taps.Length];
            IppStatus st = 0;

            if (src.Length < 5)
                throw new Exception("Invalid FIR src length");

            if (taps.Length < 1)
                throw new Exception("Invalid FIR win length");

            //Initialize delay line from initial sample and sample gradient
            double dd = src[1] - src[0];
            delLine[0] = src[0] - dd;
            for (int i = 1; i < delLine.Length; ++i)
                delLine[i] = delLine[i - 1] - dd;

            //Get buffer size for filter
            int sz = 0;
            st = Ipps.ippsFIRGetStateSize_64f(taps.Length, ref sz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FIRGetStateSize error {0}", st));

            //Buffer for FIR state information. 
            //This is used between calls to ipp and so must be fixed
            byte[] buf = new byte[sz];
            fixed (byte* pbuf = &buf[0])
            {
                IntPtr pstate = IntPtr.Zero;
                //pstate is initialized to point to somewhere in pbuf

                st = Ipps.ippsFIRInit_64f(ref pstate, taps, taps.Length, delLine, pbuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("FIRInit error {0}", st));

                st = Ipps.ippsFIR_64f(src, dst, src.Length, pstate);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("FIR error {0}", st));

                if (shift)
                {
                    //Move the samples forward in the dst vector by half the filter length
                    //to compensate for filter phase delay and pad at final sample value and gradient 
                    int w2 = taps.Length / 2;
                    Array.Copy(dst, w2, dst, 0, dst.Length - w2);
                    dd = src[src.Length - 1] - src[src.Length - 2];
                    double g = src[src.Length - 1] + dd;
                    double d = 0;
                    for (int i = dst.Length - w2; i < dst.Length; ++i)
                    {
                        st = Ipps.ippsFIROne_64f(g, ref d, pstate);
                        if (st != IppStatus.ippStsNoErr)
                            throw new Exception(string.Format("FIROne error {0}", st));
                        dst[i] = d;
                        g += dd;
                    }
                }
            }
            return dst;
        }

        public static double[] Smooth(double[] x, int len)
        {
            //Rolling average of len samples
            double[] taps = new double[len];
            double d = 1.0 / len;
            for (int i = 0; i < len; ++i)
                taps[i] = d;
            return Fir(x, taps, true);
        }

        public static double[] Der5(double[] x, double interval)
        {
            //Differentiate using 5 point stencil
            double[] taps = new double[5];
            taps[0] = -1.0 / (12 * interval);
            taps[1] = 8.0 / (12 * interval);
            taps[2] = 0;
            taps[3] = -8.0 / (12 * interval);
            taps[4] = 1.0 / (12 * interval);
            return Fir(x, taps, true);
        }

        public static double[] Der3(double[] x, double interval)
        {
            //Differentiate using 3 point stencil
            double[] taps = new double[3];
            taps[0] = 1.0 / (2 * interval);
            taps[1] = 0;
            taps[2] = -1.0 / (2 * interval);
            return Fir(x, taps, true);
        }

        public static unsafe double[] Iir(double[] src, double[] coeff, int initCount)
        {
            int order = coeff.Length / 2 - 1;

            if (src.Length < 5)
                throw new Exception("Invalid IIR src length");

            if (order < 1)
                throw new Exception("Invalid IIR coeff length");

            double[] dst = new double[src.Length];
            double[] delLine = new double[order];
            IppStatus st = 0;

            //Get buffer size for filter
            int sz = 0;
            st = Ipps.ippsIIRGetStateSize_64f(order, ref sz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("IIRGetStateSize error {0}", st));

            //Buffer for IIR state information. 
            //This is used between calls to ipp and so must be fixed
            byte[] buf = new byte[sz];
            fixed (byte* pbuf = &buf[0])
            {
                IntPtr pstate = IntPtr.Zero;

                st = Ipps.ippsIIRInit_64f(ref pstate, coeff, order, delLine, pbuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("IIRInit error {0}", st));

                //Initialize the filter to minimize settling
                for (int i = 0; i < initCount; ++i)
                {
                    st = Ipps.ippsIIR_64f(src, dst, 1, pstate);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("IIR error {0}", st));
                }
                //Filter all the data
                st = Ipps.ippsIIR_64f(src, dst, src.Length, pstate);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("IIR error {0}", st));
            }

            return dst;
        }

        public static double[] IirFwdRev(double[] src, double[] coeff, int initCount)
        {
            //Filter
            double[] dst = Iir(src, coeff, initCount);
            double[] dst1 = new double[dst.Length];
            IppStatus st;

            //Reverse
            st = Ipps.ippsFlip_64f(dst, dst1, dst.Length);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("Flip error {0}", st));

            //Filter again
            dst = Iir(dst1, coeff, initCount);

            //Reverse again
            st = Ipps.ippsFlip_64f(dst, dst1, dst.Length);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("Flip error {0}", st));

            //Final
            return dst1;
        }

        public static unsafe double[] IirCreateLpButter(double cutoff, int order)
        {
            IppStatus st;
            double[] coeff = new double[2 * (order + 1)];

            int sz = 0;
            st = Ipps.ippsIIRGenGetBufferSize(order, ref sz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("ippsIIRGenGetBufferSize error {0}", st));

            byte[] buf = new byte[sz];
            fixed (byte* pbuf = &buf[0])
            {
                st = Ipps.ippsIIRGenLowpass_64f(cutoff, 0, order, coeff, IppsIIRFilterType.ippButterworth, pbuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("IIRGenLowpass error {0}", st));
            }

            return coeff;
        }

        public static unsafe double[] IirCreateHpButter(double cutoff, int order)
        {
            IppStatus st;
            double[] coeff = new double[2 * (order + 1)];

            int sz = 0;
            st = Ipps.ippsIIRGenGetBufferSize(order, ref sz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("ippsIIRGenGetBufferSize error {0}", st));

            byte[] buf = new byte[sz];
            fixed (byte* pbuf = &buf[0])
            {
                st = Ipps.ippsIIRGenHighpass_64f(cutoff, 0, order, coeff, IppsIIRFilterType.ippButterworth, pbuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("IIRGenHighpass error {0}", st));
            }

            return coeff;
        }

        public static unsafe double[] IirCreateLpCheby(double cutoff, int order, double ripple)
        {
            IppStatus st;
            double[] coeff = new double[2 * (order + 1)];

            int sz = 0;
            st = Ipps.ippsIIRGenGetBufferSize(order, ref sz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("ippsIIRGenGetBufferSize error {0}", st));

            byte[] buf = new byte[sz];
            fixed (byte* pbuf = &buf[0])
            {
                st = Ipps.ippsIIRGenLowpass_64f(cutoff, ripple, order, coeff, IppsIIRFilterType.ippChebyshev1, pbuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("IIRGenLowpass error {0}", st));
            }

            return coeff;
        }

        public static unsafe double[] IirCreateHpCheby(double cutoff, int order, double ripple)
        {
            IppStatus st;
            double[] coeff = new double[2 * (order + 1)];

            int sz = 0;
            st = Ipps.ippsIIRGenGetBufferSize(order, ref sz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("ippsIIRGenGetBufferSize error {0}", st));

            byte[] buf = new byte[sz];
            fixed (byte* pbuf = &buf[0])
            {
                st = Ipps.ippsIIRGenHighpass_64f(cutoff, ripple, order, coeff, IppsIIRFilterType.ippChebyshev1, pbuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("IIRGenHighpass error {0}", st));
            }

            return coeff;
        }

        public static double[] FirCreateHpHamming(double cutoff, int numTaps)
        {
            double[] taps = new double[numTaps];

            IppStatus st = Ipps.ippsFIRGenHighpass_64f(cutoff, taps, numTaps, IppWinType.ippWinHamming, IppBool.ippTrue);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FIRGenHighpass error {0}", st));

            return taps;
        }

        public static double[] FirCreateLpHamming(double cutoff, int numTaps)
        {
            double[] taps = new double[numTaps];

            IppStatus st = Ipps.ippsFIRGenLowpass_64f(cutoff, taps, numTaps, IppWinType.ippWinHamming, IppBool.ippTrue);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FIRGenLowpass error {0}", st));

            return taps;
        }

        public static double[] FirCreateBpHamming(double lowCutoff, double highCutoff, int numTaps)
        {
            double[] taps = new double[numTaps];

            IppStatus st = Ipps.ippsFIRGenBandpass_64f(lowCutoff, highCutoff, taps, numTaps, IppWinType.ippWinHamming, IppBool.ippTrue);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FIRGenBandpass error {0}", st));

            return taps;
        }

        public static double[] FirCreateBsHamming(double lowCutoff, double highCutoff, int numTaps)
        {
            double[] taps = new double[numTaps];

            IppStatus st = Ipps.ippsFIRGenBandstop_64f(lowCutoff, highCutoff, taps, numTaps, IppWinType.ippWinHamming, IppBool.ippTrue);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FIRGenBandstop error {0}", st));

            return taps;
        }
    }
}


