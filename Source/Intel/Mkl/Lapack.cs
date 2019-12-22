using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ulee.Intel.Mkl
{
    // LAPACK native declarations 
    [SuppressUnmanagedCodeSecurity]
    public static class Lapack
    {
        public static readonly int LAPACK_ROW_MAJOR = 101;
        public static readonly int LAPACK_COL_MAJOR = 102;

        [DllImport("mkl_rt.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = false)]
        public static extern int LAPACKE_dgels([In] int matrix_order, [In] char trans, [In]int m, [In]int n, [In]int nrhs, [In, Out] double[] a, [In] int lda, [In, Out] double[] b, [In]int ldb);

    }
}
