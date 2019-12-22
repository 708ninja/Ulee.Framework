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
using System.Diagnostics;

using Ulee.Intel.Mkl;

namespace Ulee.Intel
{
    public static class MathTransforms
    {
        public static void Test()
        {
            IntPtr desc = new IntPtr();
            int precision = DFTI.DOUBLE;
            int forward_domain = DFTI.REAL;
            int dimension = 1, length = 6;

            /* The data to be transformed */
            double[] x_normal = new double[length];
            double[] x_transformed = new double[length];

            /* Create new DFTI descriptor */
            int ret = DFTI.DftiCreateDescriptor(ref desc,
                precision, forward_domain, dimension, length);

            Debug.WriteLine("ret = " + ret);

            /* Setup the scale factor */
            long transform_size = length;
            double scale_factor = 1.0 / transform_size;
            ret = DFTI.DftiSetValue(desc, DFTI.BACKWARD_SCALE, scale_factor);
            Debug.WriteLine("ret = " + ret);

            /* Try floating-point and GetValue function */
            double backward_scale = 0;
            ret = DFTI.DftiGetValue(desc, DFTI.BACKWARD_SCALE, ref backward_scale);
            Debug.WriteLine("ret = " + ret);
            Debug.WriteLine("Backward transform scale: " + backward_scale);

            /* Setup the transform parameters */
            ret = DFTI.DftiSetValue(desc, DFTI.PLACEMENT, DFTI.NOT_INPLACE);
            Debug.WriteLine("ret = " + ret);
            ret = DFTI.DftiSetValue(desc, DFTI.PACKED_FORMAT, DFTI.PACK_FORMAT);
            Debug.WriteLine("ret = " + ret);

            /* Commit the descriptor */
            ret = DFTI.DftiCommitDescriptor(desc);
            Debug.WriteLine("ret = " + ret);

            /* Initialize the data array */
            Debug.WriteLine("Initial data:");
            for (int i = 0; i < length; i++)
            {
                x_normal[i] = i;
                Debug.Write("\t" + i);
            }
            Debug.WriteLine("");

            /* Forward, then backward transform */
            ret = DFTI.DftiComputeForward(desc, x_normal, x_transformed);
            Debug.WriteLine("ret = " + ret);

            ret = DFTI.DftiComputeBackward(desc, x_transformed, x_normal);
            Debug.WriteLine("ret = " + ret);

            DFTI.DftiFreeDescriptor(ref desc);

            /* Check the data array */
            Debug.WriteLine("Resulting data:");
            for (int i = 0; i < length; i++)
            {
                Debug.Write("\t" + x_normal[i]);
            }
            Debug.WriteLine("");
            Debug.WriteLine("TEST PASSED");
            Debug.WriteLine("");
        }
    }
}
