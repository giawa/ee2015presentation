using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
//using System.Numerics;

namespace FilterTools
{
    public class fftw
    {
        #region Enums
        /// <summary>
        /// FFTW planner flags
        /// </summary>
        [Flags]
        public enum fftw_flags : uint
        {
            /// <summary>
            /// Tells FFTW to find an optimized plan by actually computing several FFTs and measuring their execution time. 
            /// Depending on your machine, this can take some time (often a few seconds). Default (0x0). 
            /// </summary>
            Measure = 0,
            /// <summary>
            /// Specifies that an out-of-place transform is allowed to overwrite its 
            /// input array with arbitrary data; this can sometimes allow more efficient algorithms to be employed.
            /// </summary>
            DestroyInput = 1,
            /// <summary>
            /// Rarely used. Specifies that the algorithm may not impose any unusual alignment requirements on the input/output 
            /// arrays (i.e. no SIMD). This flag is normally not necessary, since the planner automatically detects 
            /// misaligned arrays. The only use for this flag is if you want to use the guru interface to execute a given 
            /// plan on a different array that may not be aligned like the original. 
            /// </summary>
            Unaligned = 2,
            /// <summary>
            /// Not used.
            /// </summary>
            ConserveMemory = 4,
            /// <summary>
            /// Like Patient, but considers an even wider range of algorithms, including many that we think are 
            /// unlikely to be fast, to produce the most optimal plan but with a substantially increased planning time. 
            /// </summary>
            Exhaustive = 8,
            /// <summary>
            /// Specifies that an out-of-place transform must not change its input array. 
            /// </summary>
            /// <remarks>
            /// This is ordinarily the default, 
            /// except for c2r and hc2r (i.e. complex-to-real) transforms for which DestroyInput is the default. 
            /// In the latter cases, passing PreserveInput will attempt to use algorithms that do not destroy the 
            /// input, at the expense of worse performance; for multi-dimensional c2r transforms, however, no 
            /// input-preserving algorithms are implemented and the planner will return null if one is requested.
            /// </remarks>
            PreserveInput = 16,
            /// <summary>
            /// Like Measure, but considers a wider range of algorithms and often produces a “more optimal” plan 
            /// (especially for large transforms), but at the expense of several times longer planning time 
            /// (especially for large transforms).
            /// </summary>
            Patient = 32,
            /// <summary>
            /// Specifies that, instead of actual measurements of different algorithms, a simple heuristic is 
            /// used to pick a (probably sub-optimal) plan quickly. With this flag, the input/output arrays 
            /// are not overwritten during planning. 
            /// </summary>
            Estimate = 64
        }

        /// <summary>
        /// Defines direction of operation
        /// </summary>
        public enum fftw_direction : int
        {
            /// <summary>
            /// Computes a regular DFT
            /// </summary>
            Forward = -1,
            /// <summary>
            /// Computes the inverse DFT
            /// </summary>
            Backward = 1
        }

        /// <summary>
        /// Kinds of real-to-real transforms
        /// </summary>
        public enum fftw_kind : uint
        {
            R2HC = 0,
            HC2R = 1,
            DHT = 2,
            REDFT00 = 3,
            REDFT01 = 4,
            REDFT10 = 5,
            REDFT11 = 6,
            RODFT00 = 7,
            RODFT01 = 8,
            RODFT10 = 9,
            RODFT11 = 10
        }
        #endregion

        #region FFTW DllImport
        /// <summary>
        /// Allocates FFTW-optimized unmanaged memory
        /// </summary>
        /// <param name="length">Amount to allocate, in bytes</param>
        /// <returns>Pointer to allocated memory</returns>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_malloc",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr malloc(int length);

        /// <summary>
        /// Deallocates memory allocated by FFTW malloc
        /// </summary>
        /// <param name="mem">Pointer to memory to release</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_free",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void free(IntPtr mem);

        /// <summary>
        /// Deallocates an FFTW plan and all associated resources
        /// </summary>
        /// <param name="plan">Pointer to the plan to release</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_destroy_plan",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy_plan(IntPtr plan);

        /// <summary>
        /// Clears all memory used by FFTW, resets it to initial state. Does not replace destroy_plan and free
        /// </summary>
        /// <remarks>After calling fftw_cleanup, all existing plans become undefined, and you should not 
        /// attempt to execute them nor to destroy them. You can however create and execute/destroy new plans, 
        /// in which case FFTW starts accumulating wisdom information again. 
        /// fftw_cleanup does not deallocate your plans; you should still call fftw_destroy_plan for this purpose.</remarks>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_cleanup",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void cleanup();

        /// <summary>
        /// Sets the maximum time that can be used by the planner.
        /// </summary>
        /// <param name="seconds">Maximum time, in seconds.</param>
        /// <remarks>This function instructs FFTW to spend at most seconds seconds (approximately) in the planner. 
        /// If seconds == -1.0 (the default value), then planning time is unbounded. 
        /// Otherwise, FFTW plans with a progressively wider range of algorithms until the the given time limit is 
        /// reached or the given range of algorithms is explored, returning the best available plan. For example, 
        /// specifying fftw_flags.Patient first plans in Estimate mode, then in Measure mode, then finally (time 
        /// permitting) in Patient. If fftw_flags.Exhaustive is specified instead, the planner will further progress to 
        /// Exhaustive mode. 
        /// </remarks>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_set_timelimit",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_timelimit(double seconds);

        /// <summary>
        /// Executes an FFTW plan, provided that the input and output arrays still exist
        /// </summary>
        /// <param name="plan">Pointer to the plan to execute</param>
        /// <remarks>execute (and equivalents) is the only function in FFTW guaranteed to be thread-safe.</remarks>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_execute",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void execute(IntPtr plan);

        /// <summary>
        /// Creates a plan for a 1-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="n">The logical size of the transform</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_1d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_1d(int n, IntPtr input, IntPtr output,
            fftw_direction direction, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="nx">The logical size of the transform along the first dimension</param>
        /// <param name="ny">The logical size of the transform along the second dimension</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_2d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_2d(int nx, int ny, IntPtr input, IntPtr output,
            fftw_direction direction, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="nx">The logical size of the transform along the first dimension</param>
        /// <param name="ny">The logical size of the transform along the second dimension</param>
        /// <param name="nz">The logical size of the transform along the third dimension</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_3d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_3d(int nx, int ny, int nz, IntPtr input, IntPtr output,
            fftw_direction direction, fftw_flags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional complex-to-complex DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the logical size along each dimension</param>
        /// <param name="direction">Specifies the direction of the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft(int rank, int[] n, IntPtr input, IntPtr output,
            fftw_direction direction, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 1-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="n">Number of REAL (input) elements in the transform</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_r2c_1d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c_1d(int n, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="nx">Number of REAL (input) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (input) elements in the transform along the second dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_r2c_2d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c_2d(int nx, int ny, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="nx">Number of REAL (input) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (input) elements in the transform along the second dimension</param>
        /// <param name="nz">Number of REAL (input) elements in the transform along the third dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_r2c_3d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c_3d(int nx, int ny, int nz, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional real-to-complex DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the number of REAL (input) elements along each dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_r2c",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_r2c(int rank, int[] n, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 1-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="n">Number of REAL (output) elements in the transform</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_c2r_1d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r_1d(int n, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="nx">Number of REAL (output) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (output) elements in the transform along the second dimension</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_c2r_2d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r_2d(int nx, int ny, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="nx">Number of REAL (output) elements in the transform along the first dimension</param>
        /// <param name="ny">Number of REAL (output) elements in the transform along the second dimension</param>
        /// <param name="nz">Number of REAL (output) elements in the transform along the third dimension</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_c2r_3d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r_3d(int nx, int ny, int nz, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional complex-to-real DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the number of REAL (output) elements along each dimension</param>
        /// <param name="input">Pointer to an array of 16-byte complex numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_dft_c2r",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dft_c2r(int rank, int[] n, IntPtr input, IntPtr output, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 1-dimensional real-to-real DFT
        /// </summary>
        /// <param name="n">Number of elements in the transform</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kind">The kind of real-to-real transform to compute</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_r2r_1d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r_1d(int n, IntPtr input, IntPtr output, fftw_kind kind, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 2-dimensional real-to-real DFT
        /// </summary>
        /// <param name="nx">Number of elements in the transform along the first dimension</param>
        /// <param name="ny">Number of elements in the transform along the second dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kindx">The kind of real-to-real transform to compute along the first dimension</param>
        /// <param name="kindy">The kind of real-to-real transform to compute along the second dimension</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_r2r_2d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r_2d(int nx, int ny, IntPtr input, IntPtr output,
            fftw_kind kindx, fftw_kind kindy, fftw_flags flags);

        /// <summary>
        /// Creates a plan for a 3-dimensional real-to-real DFT
        /// </summary>
        /// <param name="nx">Number of elements in the transform along the first dimension</param>
        /// <param name="ny">Number of elements in the transform along the second dimension</param>
        /// <param name="nz">Number of elements in the transform along the third dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kindx">The kind of real-to-real transform to compute along the first dimension</param>
        /// <param name="kindy">The kind of real-to-real transform to compute along the second dimension</param>
        /// <param name="kindz">The kind of real-to-real transform to compute along the third dimension</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_r2r_3d",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r_3d(int nx, int ny, int nz, IntPtr input, IntPtr output,
            fftw_kind kindx, fftw_kind kindy, fftw_kind kindz, fftw_flags flags);

        /// <summary>
        /// Creates a plan for an n-dimensional real-to-real DFT
        /// </summary>
        /// <param name="rank">Number of dimensions</param>
        /// <param name="n">Array containing the number of elements in the transform along each dimension</param>
        /// <param name="input">Pointer to an array of 8-byte real numbers</param>
        /// <param name="output">Pointer to an array of 8-byte real numbers</param>
        /// <param name="kind">An array containing the kind of real-to-real transform to compute along each dimension</param>
        /// <param name="flags">Flags that specify the behavior of the planner</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_plan_r2r",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr r2r(int rank, int[] n, IntPtr input, IntPtr output,
            fftw_kind[] kind, fftw_flags flags);

        /// <summary>
        /// Returns (approximately) the number of flops used by a certain plan
        /// </summary>
        /// <param name="plan">The plan to measure</param>
        /// <param name="add">Reference to double to hold number of adds</param>
        /// <param name="mul">Reference to double to hold number of muls</param>
        /// <param name="fma">Reference to double to hold number of fmas (fused multiply-add)</param>
        /// <remarks>Total flops ~= add+mul+2*fma or add+mul+fma if fma is supported</remarks>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_flops",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void flops(IntPtr plan, ref double add, ref double mul, ref double fma);

        /// <summary>
        /// Outputs a "nerd-readable" version of the specified plan to stdout
        /// </summary>
        /// <param name="plan">The plan to output</param>
        [DllImport("libfftw3-3.dll",
             EntryPoint = "fftw_print_plan",
             ExactSpelling = true,
             CallingConvention = CallingConvention.Cdecl)]
        public static extern void print_plan(IntPtr plan);
        #endregion

        #region FFTW Methods
        public static double[] FFTW(double[] inArray)
        {
            // add some random noise to the signal to avoid FFTs with NaN returned
            Random gen = new Random(Environment.TickCount);

            double[] fin = new double[inArray.Length * 2];
            double[] fout = new double[inArray.Length * 2];

            GCHandle gc1 = GCHandle.Alloc(fin, GCHandleType.Pinned);
            GCHandle gc2 = GCHandle.Alloc(fout, GCHandleType.Pinned);

            for (int i = 0; i < fin.Length; i++)
                fin[i] = (i % 2 == 1) ? inArray[i >> 1] + (gen.NextDouble() - 0.5) / 1e12 : 0;

            IntPtr plan1 = fftw.dft_1d(inArray.Length, gc1.AddrOfPinnedObject(), gc2.AddrOfPinnedObject(), fftw_direction.Forward, fftw_flags.Estimate);
            fftw.execute(plan1);

            gc1.Free();
            gc2.Free();
            fftw.destroy_plan(plan1);

            double[] outArray = new double[inArray.Length];
            for (int i = 0; i < outArray.Length; i++)
                outArray[i] = Math.Sqrt(fout[i * 2] * fout[i * 2] + fout[i * 2 + 1] * fout[i * 2 + 1]);

            return outArray;
        }

        public static double[,] FFTW(double[] y, double samplerate, out double max)
        {
            double[] ffty = FFTW(y);

            double[,] fft = new double[2, y.Length / 2 - 1];
            max = 0;
            for (int i = 0; i < fft.Length / 2; i++)
            {
                fft[0, i] = (double)(i + 1) / y.Length * samplerate;
                fft[1, i] = ffty[i + 1];
                if (ffty[i] > max) max = ffty[i + 1];
            }
            for (int i = 0; i < fft.Length / 2; i++) fft[1, i] /= max;

            return fft;
        }

        public static double[] FFTWFrequencies(int length, double samplerate)
        {
            double[] freqs = new double[length / 2];
            for (int i = 0; i < freqs.Length; i++) freqs[i] = (double)(i + 1) / length * samplerate;
            return freqs;
        }

        public static double[] FFTWAmplitude(double[] y)
        {
            double[] ffty = FFTW(y);

            double[] ampl = new double[y.Length / 2];
            for (int i = 0; i < ampl.Length; i++) ampl[i] = ffty[i + 1];

            return ampl;
        }

        public static double[,] FFTW(double[] y, double samplerate, double max = 0)
        {
            double[] ffty = FFTW(y);

            double[,] fft = new double[2, y.Length / 2 - 1];
            bool findmax = (max == 0);
            for (int i = 0; i < fft.Length / 2; i++)
            {
                fft[0, i] = (double)(i + 1) / y.Length * samplerate;
                fft[1, i] = ffty[i + 1];
                if (findmax & ffty[i] > max) max = ffty[i + 1];
            }
            for (int i = 0; i < fft.Length / 2; i++) fft[1, i] /= max;

            return fft;
        }

        public static double[,] FFTW(double[] y, double samplerate, WindowFunction.WindowType window)
        {
            double[] w = new double[y.Length];
            for (int i = 0; i < w.Length; i++) w[i] = 1;
            w = WindowFunction.ApplyWindowFunction(w, w.Length, window);
            double mean = Mean(w) / 2 * y.Length;

            y = WindowFunction.ApplyWindowFunction(y, y.Length, window);
            var fft = FFTW(y, samplerate, 1);
            for (int i = 0; i < fft.Length / 2; i++)
                fft[1, i] /= mean;

            return fft;
        }

        public static double Mean(IEnumerable<double> list)
        {
            Decimal sum = 0;
            long i = 0;

            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                sum += (Decimal)enumerator.Current;
                i++;
            }

            if (i == 0) return 1;
            return (double)(sum / (Decimal)i);
        }
        #endregion
    }
}