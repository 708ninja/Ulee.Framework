using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Intel.Ipp
{
    [SuppressUnmanagedCodeSecurity]
    internal static class Ipps
    {
        public enum IppFftFlag
        {
            IPP_FFT_DIV_FWD_BY_N = 1,
            IPP_FFT_DIV_INV_BY_N = 2,
            IPP_FFT_DIV_BY_SQRTN = 4,
            IPP_FFT_NODIV_BY_ANY = 8
        }

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIR_64f([In] double[] pSrc, [Out] double[] pDst, [In] int numIters, [In] IntPtr pState);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIROne_64f([In] double src, ref double pDstVal, IntPtr pState);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsFIRInit_64f(ref IntPtr ppState, [In] double[] pTaps, [In] int tapsLen, [In] double[] pDlyLine, [In] byte* pBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIRGetStateSize_64f([In] int tapsLen, ref int pBufferSize);
        
        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsIIR_64f([In] double[] pSrc, [Out] double[] pDst, [In] int len, [In] IntPtr pState);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsIIROne_64f([In] double src, ref double pDstVal, IntPtr pState);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsIIRInit_64f(ref IntPtr ppState, [In] double[] pTaps, [In] int order, [In] double[] pDlyLine, [In] byte* pBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsIIRGetStateSize_64f([In] int order, ref int pBufferSize);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsIIRGenGetBufferSize([In] int order, ref int pBufferSize);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsIIRGenLowpass_64f([In] double rFreq, [In] double ripple, [In]int order, [Out] double[] pTaps, [In] IppsIIRFilterType filterType, [In] byte* pBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsIIRGenHighpass_64f([In] double rFreq, [In] double ripple, [In]int order, [Out] double[] pTaps, [In] IppsIIRFilterType filterType, [In] byte* pBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIRGenLowpass_64f([In] double rFreq, [Out] double[] pTaps, [In]int tapsLen, [In]IppWinType winType, [In]IppBool doNormal);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIRGenHighpass_64f([In] double rFreq, [Out] double[] pTaps, [In]int tapsLen, [In]IppWinType winType, [In]IppBool doNormal);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIRGenBandpass_64f([In]double rLowFreq, [In] double rHighFreq, [Out]double[] pTaps, [In]int tapsLen, [In]IppWinType winType, [In]IppBool doNormal);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFIRGenBandstop_64f([In]double rLowFreq, [In] double rHighFreq, [Out]double[] pTaps, [In]int tapsLen, [In]IppWinType winType, [In]IppBool doNormal);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsPowerSpectr_64f([In]double[] pSrcRe, [In] double[] pSrcIm, [Out] double[] pDst, int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFFTFwd_CToC_64f([In] double[] pSrcRe, [In] double[] pSrcIm, [Out] double[] pDstRe, [Out] double[] pDstIm, IntPtr pFFTSpec, byte[] pBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFFTInv_CToC_64f([In] double[] pSrcRe, [In] double[] pSrcIm, [Out] double[] pDstRe, [Out] double[] pDstIm, IntPtr pFFTSpec, byte[] pBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public unsafe static extern IppStatus ippsFFTInit_C_64f(ref IntPtr ppFFTSpec, [In]int order, [In]IppFftFlag flag, [In]IppHintAlgorithm hint, [In] byte* pSpec, [In] byte[] pSpecBuffer);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFFTGetSize_C_64f([In]int order, [In]IppFftFlag flag, [In]IppHintAlgorithm hint, ref int pSpecSize, ref int pSpecBufferSize, ref int pBufferSize);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsWinHann_64f([In] double[] pSrc, [Out] double[] pDst, [In] int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsMul_64f([In]double[] pSrc1, [In]double[] pSrc2, [Out]double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsAdd_64f([In]double[] pSrc1, [In]double[] pSrc2, [Out]double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsMulC_64f([In]double[] pSrc1, [In]double src2, [Out]double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsMulC_64f_I([In]double src1, [In, Out]double[] pSrcDst1, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsAddC_64f([In]double[] pSrc1, [In]double src2, [Out]double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsAddC_64f_I([In]double src1, [In, Out]double[] pSrcDst1, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsSet_64f([In]double val, [Out] double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsZero_64f([Out] double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsFlip_64f([In]double[] pSrc, [Out] double[] pDst, [In] int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsMagnitude_64f([In]double[] pSrcRe, [In] double[] pSrcIm, [Out]double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsPhase_64f([In]double[] pSrcRe, [In] double[] pSrcIm, [Out]double[] pDst, [In]int len);

        [DllImport("ipps.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = false)]
        public static extern IppStatus ippsMean_64f([In] double[] pSrc, [In] int len, [Out] out double pMean);
    }
}
