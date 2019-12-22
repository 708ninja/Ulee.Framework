using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Intel.Mkl
{
    public static class DFTI
    {
        /** Constants for DFTI, file "mkl_dfti.h" */
        /** DFTI configuration parameters */
        public static readonly int PRECISION = 3;
        public static readonly int FORWARD_DOMAIN = 0;
        public static readonly int DIMENSION = 1;
        public static readonly int LENGTHS = 2;
        public static readonly int NUMBER_OF_TRANSFORMS = 7;
        public static readonly int FORWARD_SCALE = 4;
        public static readonly int BACKWARD_SCALE = 5;
        public static readonly int PLACEMENT = 11;
        public static readonly int COMPLEX_STORAGE = 8;
        public static readonly int REAL_STORAGE = 9;
        public static readonly int CONJUGATE_EVEN_STORAGE = 10;
        public static readonly int DESCRIPTOR_NAME = 20;
        public static readonly int PACKED_FORMAT = 21;
        public static readonly int NUMBER_OF_USER_THREADS = 26;
        public static readonly int INPUT_DISTANCE = 14;
        public static readonly int OUTPUT_DISTANCE = 15;
        public static readonly int INPUT_STRIDES = 12;
        public static readonly int OUTPUT_STRIDES = 13;
        public static readonly int ORDERING = 18;
        public static readonly int TRANSPOSE = 19;
        public static readonly int COMMIT_STATUS = 22;
        public static readonly int VERSION = 23;
        /** DFTI configuration values */
        public static readonly int SINGLE = 35;
        public static readonly int DOUBLE = 36;
        public static readonly int COMPLEX = 32;
        public static readonly int REAL = 33;
        public static readonly int INPLACE = 43;
        public static readonly int NOT_INPLACE = 44;
        public static readonly int COMPLEX_COMPLEX = 39;
        public static readonly int REAL_REAL = 42;
        public static readonly int COMPLEX_REAL = 40;
        public static readonly int REAL_COMPLEX = 41;
        public static readonly int COMMITTED = 30;
        public static readonly int UNCOMMITTED = 31;
        public static readonly int ORDERED = 48;
        public static readonly int BACKWARD_SCRAMBLED = 49;
        public static readonly int NONE = 53;
        public static readonly int CCS_FORMAT = 54;
        public static readonly int PACK_FORMAT = 55;
        public static readonly int PERM_FORMAT = 56;
        public static readonly int CCE_FORMAT = 57;
        public static readonly int VERSION_LENGTH = 198;
        public static readonly int MAX_NAME_LENGTH = 10;
        public static readonly int MAX_MESSAGE_LENGTH = 40;
        /** DFTI predefined error classes */
        public static readonly int NO_ERROR = 0;
        public static readonly int MEMORY_ERROR = 1;
        public static readonly int INVALID_CONFIGURATION = 2;
        public static readonly int INCONSISTENT_CONFIGURATION = 3;
        public static readonly int NUMBER_OF_THREADS_ERROR = 8;
        public static readonly int MULTITHREADED_ERROR = 4;
        public static readonly int BAD_DESCRIPTOR = 5;
        public static readonly int UNIMPLEMENTED = 6;
        public static readonly int MKL_INTERNAL_ERROR = 7;
        public static readonly int LENGTH_EXCEEDS_INT32 = 9;

        /** DFTI DftiCreateDescriptor wrapper */
        public static int DftiCreateDescriptor(ref IntPtr desc,
            int precision, int domain, int dimension, int length)
        {
            return DFTINative.DftiCreateDescriptor(ref desc,
                precision, domain, dimension, length);
        }
        /** DFTI DftiFreeDescriptor wrapper */
        public static int DftiFreeDescriptor(ref IntPtr desc)
        {
            return DFTINative.DftiFreeDescriptor(ref desc);
        }
        /** DFTI DftiSetValue wrapper */
        public static int DftiSetValue(IntPtr desc,
            int config_param, int config_val)
        {
            return DFTINative.DftiSetValue(desc,
                config_param, config_val);
        }
        /** DFTI DftiSetValue wrapper */
        public static int DftiSetValue(IntPtr desc,
            int config_param, double config_val)
        {
            return DFTINative.DftiSetValue(desc,
                config_param, config_val);
        }
        /** DFTI DftiGetValue wrapper */
        public static int DftiGetValue(IntPtr desc,
            int config_param, ref double config_val)
        {
            return DFTINative.DftiGetValue(desc,
                config_param, ref config_val);
        }
        /** DFTI DftiCommitDescriptor wrapper */
        public static int DftiCommitDescriptor(IntPtr desc)
        {
            return DFTINative.DftiCommitDescriptor(desc);
        }
        /** DFTI DftiComputeForward wrapper */
        public static int DftiComputeForward(IntPtr desc,
            [In] double[] x_in, [Out] double[] x_ref)
        {
            return DFTINative.DftiComputeForward(desc, x_in, x_ref);
        }
        /** DFTI DftiComputeBackward wrapper */
        public static int DftiComputeBackward(IntPtr desc,
            [In] double[] x_in, [Out] double[] x_ref)
        {
            return DFTINative.DftiComputeBackward(desc, x_in, x_ref);
        }
    }

    /** DFTI native declarations */
    [SuppressUnmanagedCodeSecurity]
    public static class DFTINative
    {
        /** DFTI native DftiCreateDescriptor declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiCreateDescriptor(ref IntPtr desc, int precision, int domain, int dimention, int length);
 
        /** DFTI native DftiCommitDescriptor declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiCommitDescriptor(IntPtr desc);
        
        /** DFTI native DftiFreeDescriptor declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiFreeDescriptor(ref IntPtr desc);
        
        /** DFTI native DftiSetValue declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiSetValue(IntPtr desc, int config_param, int config_val);
        
        /** DFTI native DftiSetValue declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiSetValue(IntPtr desc, int config_param, double config_val);
        
        /** DFTI native DftiGetValue declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiGetValue(IntPtr desc, int config_param, ref double config_val);
        
        /** DFTI native DftiComputeForward declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiComputeForward(IntPtr desc, [In] double[] x_in, [Out] double[] x_ref);
        
        /** DFTI native DftiComputeBackward declaration */
        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int DftiComputeBackward(IntPtr desc, [In] double[] x_in, [Out] double[] x_ref);
    }
}
