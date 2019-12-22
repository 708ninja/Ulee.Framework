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

using Ulee.Intel.Mkl;

namespace Ulee.Intel
{
    public static class Fitting
    {
        public static double[] LsFit(double[] x, double[] y, int order)
        {
            //Least squares fit to x, y data 
            int m = Math.Min(x.Length, y.Length);
            int n = order + 1;
            int i, j, r;

            if (m < 1)
                throw new Exception("Invalid number of elements for Ls");

            double[] a = new double[m * n];
            double[] b = new double[Math.Max(m, n)];

            for (i = 0; i < m; ++i)
            {
                r = n * i;
                a[r] = 1.0;
                b[i] = y[i];
                for (j = 1; j < n; ++j)
                    a[r + j] = x[i] * a[r + j - 1];
            }

            int info = Lapack.LAPACKE_dgels(Lapack.LAPACK_ROW_MAJOR, 'N', m, n, 1, a, n, b, 1);
            if (info != 0)
                throw new Exception(string.Format("LAPACKE_dgels returned {0}", info));
            double[] c = new double[n];
            for (i = 0; i < n; ++i)
                c[i] = b[i];

            return c;
        }

        public static double[] LsSolve(double[] a, int rows, int cols, double[] b)
        {
            //Least squares solution of X * A = B, X returned. a is row * col matrix, b is column vector  
            int i;
            double[] b1 = new double[b.Length];
            Array.Copy(b, b1, b.Length);

            int info = Lapack.LAPACKE_dgels(Lapack.LAPACK_ROW_MAJOR, 'N', rows, cols, 1, a, cols, b1, 1);
            if (info != 0)
                throw new Exception(string.Format("LAPACKE_dgels returned {0}", info));
            double[] c = new double[cols];
            for (i = 0; i < cols; ++i)
                c[i] = b1[i];

            return c;
        }

        public unsafe static double[] DfSpline(double[] x, double[] y, double[] interpX)
        {
            int status;          /* Status of a Data Fitting operation */
            IntPtr task = IntPtr.Zero;      /* Data Fitting operations are task based */

            try
            {
                /* Parameters describing the partition */
                int nx = x.Length;          /* The size of partition x */

                if (x.Length != y.Length)
                    throw new Exception("GenerateSpline input array dimensions do not match");

                /* Set values of partition x */
                int xhint = Df.DF_NON_UNIFORM_PARTITION;  /* The partition is non-uniform. */

                /* Parameters describing the function */
                int ny = 1;          /* Function dimension */

                /* Parameters describing the spline */

                double[] scoeff = new double[(x.Length - 1) * Df.DF_PP_CUBIC];   /* Array of spline coefficients */

                /* Parameters describing interpolation computations */
                int nsite = interpX.Length;        /* Number of interpolation sites */
                double[] site = interpX;   /* Array of interpolation sites */

                double[] r = new double[site.Length];    /* Array of interpolation results */

                /* Set function values */
                int yhint = Df.DF_NO_HINT;    /* No additional information about the function is provided. */

                /* Create a Data Fitting task */

                fixed (double* px = &x[0])
                {
                    fixed (double* py = &y[0])
                    {
                        status = Df.dfdNewTask1D(out task, nx, px, xhint, ny, py, yhint);
                        if (status != 0)
                            throw new Exception("dfdNewTask1D returned error code " + status.ToString());

                        /* Initialize spline parameters */
                        int s_order = Df.DF_PP_CUBIC;     /* Spline is of the fourth order (cubic spline). */
                        int s_type = Df.DF_PP_BESSEL;     /* Spline is of the Bessel cubic type. */

                        /* Define internal conditions for cubic spline construction (none in this example) */
                        int ic_type = Df.DF_NO_IC;

                        /* Use not-a-knot boundary conditions. In this case, the is first and the last 
                         interior breakpoints are inactive, no additional values are provided. */
                        int bc_type = Df.DF_BC_NOT_A_KNOT;

                        int scoeffhint = Df.DF_NO_HINT;    /* No additional information about the spline. */

                        /* Set spline parameters  in the Data Fitting task */
                        fixed (double* pcoeff = &scoeff[0])
                        {
                            status = Df.dfdEditPPSpline1D(task, s_order, s_type, bc_type, null, ic_type, null, pcoeff, scoeffhint);
                            if (status != 0)
                                throw new Exception("dfdEditPPSpline1D returned error code " + status.ToString());

                            /* Check the Data Fitting operation status */

                            /* Use a standard method to construct a cubic Bessel spline: */
                            /* Pi(x) = ci,0 + ci,1(x - xi) + ci,2(x - xi)2 + ci,3(x - xi)3, */
                            /* The library packs spline coefficients to array scoeff: */
                            /* scoeff[4*i+0] = ci,0, scoef[4*i+1] = ci,1,         */
                            /* scoeff[4*i+2] = ci,2, scoef[4*i+1] = ci,3,         */
                            /* i=0,...,N-2  */
                            status = Df.dfdConstruct1D(task, Df.DF_PP_SPLINE, Df.DF_METHOD_STD);
                            if (status != 0)
                                throw new Exception("dfdConstruct1D returned error code " + status.ToString());

                            /* Check the Data Fitting operation status */

                            /* Initialize interpolation parameters */
                            int sitehint = Df.DF_NON_UNIFORM_PARTITION; /* Partition of sites is non-uniform */

                            /* Request to compute spline values */
                            int ndorder = 1;
                            int[] dorder = new int[] { 1 };
                            int rhint = Df.DF_MATRIX_STORAGE_ROWS; /* The library packs interpolation results 
                                       in row-major format. */

                            /* Solve interpolation problem using the default method: compute the spline values
                               at the points site(i), i=0,..., nsite-1 and place the results to array r */
                            status = Df.dfdInterpolate1D(task, Df.DF_INTERP, Df.DF_METHOD_PP, nsite, site, sitehint, ndorder, dorder, null, r, rhint, null);
                            if (status != 0)
                                throw new Exception("dfdInterpolate1D returned error code " + status.ToString());
                        }
                    }
                }

                return r;
            }
            finally
            {
                if (task != IntPtr.Zero)
                {
                    status = Df.dfDeleteTask(ref task);
                    if (status != 0)
                        throw new Exception("dfDeleteTask returned error code " + status.ToString());
                }
            }
        }

    }
}