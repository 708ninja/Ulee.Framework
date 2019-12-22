using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Intel.Mkl
{
    [SuppressUnmanagedCodeSecurity]
    public static class Df
    {
            public static readonly int DF_STATUS_OK = 0;

            /*
            // Common errors (-1..-999)
            */
            public static readonly int DF_ERROR_CPU_NOT_SUPPORTED = -1;


            /*
            //++
            // DATA FITTING ERROR/WARNING CODES
            //--
            */
            /*
            // Errors (-1000..-1999)
            */
            public static readonly int DF_ERROR_NULL_TASK_DESCRIPTOR = -1000;
            public static readonly int DF_ERROR_MEM_FAILURE = -1001;
            public static readonly int DF_ERROR_METHOD_NOT_SUPPORTED = -1002;
            public static readonly int DF_ERROR_COMP_TYPE_NOT_SUPPORTED = -1003;

            public static readonly int DF_ERROR_BAD_NX = -1004;
            public static readonly int DF_ERROR_BAD_X = -1005;
            public static readonly int DF_ERROR_BAD_X_HINT = -1006;
            public static readonly int DF_ERROR_BAD_NY = -1007;
            public static readonly int DF_ERROR_BAD_Y = -1008;
            public static readonly int DF_ERROR_BAD_Y_HINT = -1009;
            public static readonly int DF_ERROR_BAD_SPLINE_ORDER = -1010;
            public static readonly int DF_ERROR_BAD_SPLINE_TYPE = -1011;
            public static readonly int DF_ERROR_BAD_IC_TYPE = -1012;
            public static readonly int DF_ERROR_BAD_IC = -1013;
            public static readonly int DF_ERROR_BAD_BC_TYPE = -1014;
            public static readonly int DF_ERROR_BAD_BC = -1015;
            public static readonly int DF_ERROR_BAD_PP_COEFF = -1016;
            public static readonly int DF_ERROR_BAD_PP_COEFF_HINT = -1017;
            public static readonly int DF_ERROR_BAD_PERIODIC_VAL = -1018;
            public static readonly int DF_ERROR_BAD_DATA_ATTR = -1019;
            public static readonly int DF_ERROR_BAD_DATA_IDX = -1020;


            public static readonly int DF_ERROR_BAD_NSITE = -1021;
            public static readonly int DF_ERROR_BAD_SITE = -1022;
            public static readonly int DF_ERROR_BAD_SITE_HINT = -1023;
            public static readonly int DF_ERROR_BAD_NDORDER = -1024;
            public static readonly int DF_ERROR_BAD_DORDER = -1025;
            public static readonly int DF_ERROR_BAD_DATA_HINT = -1026;
            public static readonly int DF_ERROR_BAD_INTERP = -1027;
            public static readonly int DF_ERROR_BAD_INTERP_HINT = -1028;
            public static readonly int DF_ERROR_BAD_CELL_IDX = -1029;
            public static readonly int DF_ERROR_BAD_NLIM = -1030;
            public static readonly int DF_ERROR_BAD_LLIM = -1031;
            public static readonly int DF_ERROR_BAD_RLIM = -1032;
            public static readonly int DF_ERROR_BAD_INTEGR = -1033;
            public static readonly int DF_ERROR_BAD_INTEGR_HINT = -1034;
            public static readonly int DF_ERROR_BAD_LOOKUP_INTERP_SITE = -1035;



            /*
            // Internal errors caused by internal routines of the functions
            */
            public static readonly int VSL_DF_ERROR_INTERNAL_C1 = -1500;
            public static readonly int VSL_DF_ERROR_INTERNAL_C2 = -1501;

            /*
            // User-defined callback status
            */
            public static readonly int DF_STATUS_EXACT_RESULT = 1000;

            /*
            //++
            // MACROS USED IN DATAFITTING EDITORS AND COMPUTE ROUTINES
            //--
            */

            /*
            // Attributes of parameters that can be modified in Data Fitting task
            */
            public static readonly int DF_X = 1;
            public static readonly int DF_Y = 2;
            public static readonly int DF_IC = 3;
            public static readonly int DF_BC = 4;
            public static readonly int DF_PP_SCOEFF = 5;

            public static readonly int DF_NX = 14;
            public static readonly int DF_XHINT = 15;
            public static readonly int DF_NY = 16;
            public static readonly int DF_YHINT = 17;
            public static readonly int DF_SPLINE_ORDER = 18;
            public static readonly int DF_SPLINE_TYPE = 19;
            public static readonly int DF_IC_TYPE = 20;
            public static readonly int DF_BC_TYPE = 21;
            public static readonly int DF_PP_COEFF_HINT = 22;

            /*
            //++
            // SPLINE ORDERS SUPPORTED IN DATA FITTING ROUTINES
            //--
            */
            public static readonly int DF_PP_STD = 0;
            public static readonly int DF_PP_LINEAR = 2;
            public static readonly int DF_PP_QUADRATIC = 3;
            public static readonly int DF_PP_CUBIC = 4;

            /*
            //++
            // SPLINE TYPES SUPPORTED IN DATA FITTING ROUTINES
            //--
            */

            public static readonly int DF_PP_DEFAULT = 0;
            public static readonly int DF_PP_SUBBOTIN = 1;
            public static readonly int DF_PP_NATURAL = 2;
            public static readonly int DF_PP_HERMITE = 3;
            public static readonly int DF_PP_BESSEL = 4;
            public static readonly int DF_PP_AKIMA = 5;
            public static readonly int DF_LOOKUP_INTERPOLANT = 6;
            public static readonly int DF_CR_STEPWISE_CONST_INTERPOLANT = 7;
            public static readonly int DF_CL_STEPWISE_CONST_INTERPOLANT = 8;

            /*
            //++
            // TYPES OF BOUNDARY CONDITIONS USED IN SPLINE CONSTRUCTION
            //--
            */
            public static readonly int DF_NO_BC = 0;
            public static readonly int DF_BC_NOT_A_KNOT = 1;
            public static readonly int DF_BC_FREE_END = 2;
            public static readonly int DF_BC_1ST_LEFT_DER = 4;
            public static readonly int DF_BC_1ST_RIGHT_DER = 8;
            public static readonly int DF_BC_2ND_LEFT_DER = 16;
            public static readonly int DF_BC_2ND_RIGHT_DER = 32;
            public static readonly int DF_BC_PERIODIC = 64;
            public static readonly int DF_BC_Q_VAL = 128;

            /*
            //++
            // TYPES OF INTERNAL CONDITIONS USED IN SPLINE CONSTRUCTION
            //--
            */
            public static readonly int DF_NO_IC = 0;
            public static readonly int DF_IC_1ST_DER = 1;
            public static readonly int DF_IC_2ND_DER = 2;
            public static readonly int DF_IC_Q_KNOT = 8;



            /*
            //++
            // TYPES OF SUPPORTED HINTS
            //--
            */
            public static readonly int DF_NO_HINT = 0x00000000;
            public static readonly int DF_NON_UNIFORM_PARTITION = 0x00000001;
            public static readonly int DF_QUASI_UNIFORM_PARTITION = 0x00000002;
            public static readonly int DF_UNIFORM_PARTITION = 0x00000004;

            public static readonly int DF_MATRIX_STORAGE_ROWS = 0x00000010;
            public static readonly int DF_MATRIX_STORAGE_COLS = 0x00000020;

            public static readonly int DF_SORTED_DATA = 0x00000040;
            public static readonly int DF_1ST_COORDINATE = 0x00000080;

            /*
            //++
            // TYPES OF APRIORI INFORMATION
            // ABOUT DATA STRUCTURE
            //--
            */
            public static readonly int DF_NO_APRIORI_INFO = 0x00000000;
            public static readonly int DF_APRIORI_MOST_LIKELY_CELL = 0x00000001;



            /*
            //++
            // ESTIMATES TO BE COMUTED WITH DATA FITTING COMPUTE ROUTINE
            //--
            */
            public static readonly int DF_INTERP = 0x00000001;
            public static readonly int DF_CELL = 0x00000002;



            /*
            //++
            // METHODS TO BE USED FOR EVALUATION OF THE SPLINE RELATED ESTIMATES
            //--
            */
            public static readonly int DF_METHOD_STD = 0;
            public static readonly int DF_METHOD_PP = 1;


            /*
            //++
            // SPLINE FORMATS SUPPORTED IN SPLINE CONSTRUCTION ROUTINE
            //--
            */

            public static readonly int DF_PP_SPLINE = 0;

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static unsafe extern int dfdNewTask1D([Out]out IntPtr task, [In] int nx, [In]double* x, [In]int xHint, [In]int ny, [In]double* y, [In]int yHint);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static unsafe extern int dfdEditPPSpline1D([In] IntPtr task, [In]int s_order, [In]int s_type, [In]int bc_type, [In, Out]double* bc, [In]int ic_type, [In, Out]double* ic, [In, Out]double* scoeff, [In]int scoeffhint);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int dfdConstruct1D([In] IntPtr task, [In] int s_format, [In] int method);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int dfdInterpolate1D([In] IntPtr task, [In] int type, [In] int method, [In] int nsite, [In] double[] site, [In] int sitehint, [In] int ndorder, [In] int[] dorder, [In] double[] datahint, [Out] double[] r, [In] int rhint, [Out] int[] cell);

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int dfDeleteTask([In, Out] ref IntPtr task);
    }
}
