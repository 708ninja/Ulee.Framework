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

using Ulee.Intel.Ipp;

namespace Ulee.Intel
{
    public static class DspTransforms
    {
        public static double[] HanningWindow(int length)
        {
            double[] src = new double[length];
            double[] dst = new double[length];
            IppStatus st;

            st = Ipps.ippsSet_64f(1.0, src, src.Length);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("Set error {0}", st));

            st = Ipps.ippsWinHann_64f(src, dst, length);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("WinHann error {0}", st));
            return dst;
        }

        public unsafe static void FftFwd(double[] x, int order, out double[] re, out double[] im)
        {
            IppStatus st;
            int sz = 1 << order;
            if (sz > 1 << 23 || sz < 4)
                throw new Exception("Invalid FftFwd order");
            double[] rw = new double[sz];
            double[] iw = new double[sz];

            double[] frw = new double[sz];
            double[] fiw = new double[sz];

            Array.Copy(x, rw, Math.Min(x.Length, rw.Length));

            int specSz = 0;
            int specBufSz = 0;
            int bufSz = 0;

            st = Ipps.ippsFFTGetSize_C_64f(order, Ipps.IppFftFlag.IPP_FFT_DIV_FWD_BY_N, IppHintAlgorithm.ippAlgHintNone, ref specSz, ref specBufSz, ref bufSz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FFTGetSize returned error {0}", st));

            byte[] spec = new byte[specSz];
            byte[] specBuf = new byte[specBufSz];
            byte[] buf = new byte[bufSz];

            IntPtr pspec = IntPtr.Zero;
            fixed (byte* ps = &spec[0])
            {
                //Spec needs to be fixed as used between ipp calls
                st = Ipps.ippsFFTInit_C_64f(ref pspec, order, Ipps.IppFftFlag.IPP_FFT_DIV_FWD_BY_N, IppHintAlgorithm.ippAlgHintNone, ps, specBuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("FFTInit returned error {0}", st));

                st = Ipps.ippsFFTFwd_CToC_64f(rw, iw, frw, fiw, pspec, buf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("FFTFwd returned error {0}", st));

                re = frw;
                im = fiw;
            }
        }

        public unsafe static double[] SpectrumBlockAverage(double[] x, int order, double percOverlap, bool power, bool zeroMean, double windowCorrection)
        {
            IppStatus st;
            int sz = 1 << order;
            if (sz > 1 << 23 || sz < 4)
                throw new Exception("Invalid SpectrumBlockAverage order");

            if (percOverlap < 0 || percOverlap >= 100)
                throw new Exception("Invalid SpectrumBlockAverage percOverlap");

            if (x == null || x.Length < 5)
                throw new Exception("Invalid SpectrumBlockAverage data length");

            double[] rw = new double[sz];
            double[] iw = new double[sz];

            double[] frw = new double[sz];
            double[] fiw = new double[sz];

            double[] psp = new double[sz];
            double[] sum = new double[sz];
            double[] res = new double[sz / 2];

            int specSz = 0;
            int specBufSz = 0;
            int bufSz = 0;

            st = Ipps.ippsFFTGetSize_C_64f(order, Ipps.IppFftFlag.IPP_FFT_DIV_FWD_BY_N, IppHintAlgorithm.ippAlgHintNone, ref specSz, ref specBufSz, ref bufSz);
            if (st != IppStatus.ippStsNoErr)
                throw new Exception(string.Format("FFTGetSize returned error {0}", st));

            byte[] spec = new byte[specSz];
            byte[] specBuf = new byte[specBufSz];
            byte[] buf = new byte[bufSz];


            IntPtr pspec = IntPtr.Zero;
            fixed (byte* ps = &spec[0])
            {
                //Spec needs to be fixed as used between ipp calls
                st = Ipps.ippsFFTInit_C_64f(ref pspec, order, Ipps.IppFftFlag.IPP_FFT_DIV_FWD_BY_N, IppHintAlgorithm.ippAlgHintNone, ps, specBuf);
                if (st != IppStatus.ippStsNoErr)
                    throw new Exception(string.Format("FFTInit returned error {0}", st));

                int count = 0;
                int step = (int)(0.01 * (100.0 - percOverlap) * sz);
                double[] win = HanningWindow(sz);

                for (int ix = 0; ix < x.Length - sz; ix += step, ++count)
                {
                    //Prepare input block
                    st = Ipps.ippsZero_64f(iw, iw.Length);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("Zero returned error {0}", st));

                    //Window block
                    Array.Copy(x, ix, rw, 0, sz);

                    if (zeroMean)
                    {
                        //Subtract mean from block
                        double m;
                        st = Ipps.ippsMean_64f(rw, rw.Length, out m);
                        if (st != IppStatus.ippStsNoErr)
                            throw new Exception(string.Format("Mean returned error {0}", st));

                        st = Ipps.ippsAddC_64f_I(-m, rw, rw.Length);
                        if (st != IppStatus.ippStsNoErr)
                            throw new Exception(string.Format("Mean returned error {0}", st));
                    }

                    st = Ipps.ippsMul_64f(rw, win, rw, sz);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("Mul returned error {0}", st));

                    //Correct for window
                    st = Ipps.ippsMulC_64f_I(windowCorrection, rw, sz);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("MulC returned error {0}", st));

                    //Calculate FFT
                    st = Ipps.ippsFFTFwd_CToC_64f(rw, iw, frw, fiw, pspec, buf);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("FFTFwd returned error {0}", st));

                    if (power)
                    {
                        //Calc power spectrum
                        st = Ipps.ippsPowerSpectr_64f(frw, fiw, psp, sz);
                        if (st != IppStatus.ippStsNoErr)
                            throw new Exception(string.Format("PowerSpectr returned error {0}", st));
                    }
                    else
                    {
                        //Calculate magnitude
                        st = Ipps.ippsMagnitude_64f(frw, fiw, psp, psp.Length);
                        if (st != IppStatus.ippStsNoErr)
                            throw new Exception(string.Format("Mag returned error {0}", st));
                    }
                    //Add result
                    st = Ipps.ippsAdd_64f(psp, sum, sum, psp.Length);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("Add returned error {0}", st));
                }
                if (count > 0)
                {
                    //Scale average
                    st = Ipps.ippsMulC_64f_I(1.0 / count, sum, sum.Length);
                    if (st != IppStatus.ippStsNoErr)
                        throw new Exception(string.Format("MulC returned error {0}", st));

                    //Calc result discarding upper half samples 
                    res[0] = sum[0];
                    for (int i = 1; i < sum.Length / 2; ++i)
                        res[i] = 2 * sum[i];
                }

                return res;
            }
        }
    }
}
