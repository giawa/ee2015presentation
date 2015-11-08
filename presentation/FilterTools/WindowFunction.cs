using System;

namespace FilterTools
{
    public class WindowFunction
    {
        #region Enumerations
        public enum WindowType
        {
            Rectangular = 1,
            Hamming = 2,
            Hann = 3,
            Cosine = 4,
            Lanczos = 5,
            Bartlett = 6,
            Triangular = 7,
            Gauss = 8,
            BartlettHann = 9,
            Blackman = 10,
            Nuttall = 11,
            BlackmanHarris = 12,
            BlackmanNuttall = 13,
            FlatTop = 14
        }
        #endregion

        #region Variables
        protected delegate double dFunction(int N, int n);
        protected dFunction evalFunction;
        protected WindowType window;
        #endregion

        #region Constructors
        public WindowFunction(WindowType type)
        {
            Window = type;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Takes the input array and shortens it to the correct value by passing it through a window function.
        /// </summary>
        /// <param name="input">The input array that is oversized.</param>
        /// <param name="reqLength">The required length of the input array.</param>
        /// <param name="type">The type of window you would like to apply.</param>
        /// <returns>The output array which is properly sized.</returns>
        public static double[] ApplyWindowFunction(double[] input, int reqLength, WindowType type)
        {
            WindowFunction function = new WindowFunction(type);
            return function.ApplyWindowFunction(input, reqLength);
        }

        /// <summary>
        /// Takes the input array and shortens it to the correct value by passing it through a window function.
        /// </summary>
        /// <param name="input">The input array that is oversized.</param>
        /// <param name="reqLength">The required length of the input array.</param>
        /// <returns>The output array which is properly sized.</returns>
        public double[] ApplyWindowFunction(double[] input, int reqLength)
        {
            double[] output = new double[reqLength];
            int offset = (int)Math.Round((input.Length - reqLength) / 2.0);
            for (int i = 0; i < reqLength; i++)
                output[i] = input[i + offset] * evalFunction(reqLength, i);
            return output;
        }

        /// <summary>
        /// Sets the window function type by using a giant switch statement with all the window formulas.
        /// </summary>
        public WindowType Window
        {
            get { return window; }
            set
            {
                window = value;
                double a0, a1, a2, a3, a4, alpha, phi;
                switch (window)
                {
                    case WindowType.Rectangular:
                        evalFunction = delegate(int N, int n)
                        { return 1.0; };
                        break;
                    case WindowType.Hamming:
                        evalFunction = delegate(int N, int n)
                        { return 0.53836 - 0.46164 * Math.Cos(2.0 * Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.Hann:
                        evalFunction = delegate(int N, int n)
                        { return 0.5 * (1 - Math.Cos(2.0 * Math.PI * n / (N - 1))); };
                        break;
                    case WindowType.Cosine:
                        evalFunction = delegate(int N, int n)
                        { return Math.Sin(Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.Lanczos:
                        evalFunction = delegate(int N, int n)
                        { return Math.Sin(2.0 * n / (N - 1) - 1) / (2.0 * n / (n - 1) - 1); };
                        break;
                    case WindowType.Bartlett:
                        evalFunction = delegate(int N, int n)
                        { return 2.0 / (N - 1) * ((N - 1) / 2.0 - Math.Abs(n - (N - 1) / 2.0)); };
                        break;
                    case WindowType.Triangular:
                        evalFunction = delegate(int N, int n)
                        { return 2.0 / N * (N / 2.0 - Math.Abs(n - (N - 1) / 2.0)); };
                        break;
                    case WindowType.Gauss:
                        phi = 0.4;
                        evalFunction = delegate(int N, int n)
                        { return Math.Exp(-0.5 * Math.Pow(((n - (N - 1) / 2.0) / (phi * (N - 1) / 2.0)), 2)); };
                        break;
                    case WindowType.BartlettHann:
                        evalFunction = delegate(int N, int n)
                        { return 0.62 - 0.48 * Math.Abs(n / (N - 1) - 0.5) - 0.38 * Math.Cos(2.0 * Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.Blackman:
                        alpha = 0.16;
                        a0 = (1 - alpha) / 2.0;
                        a1 = 0.5;
                        a2 = alpha / 2.0;
                        evalFunction = delegate(int N, int n)
                        { return a0 - a1 * Math.Cos(2 * Math.PI * n / (N - 1)) + a2 * Math.Cos(4 * Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.Nuttall:
                        a0 = 0.355768;
                        a1 = 0.487396;
                        a2 = 0.144232;
                        a3 = 0.012604;
                        evalFunction = delegate(int N, int n)
                        { return a0 - a1 * Math.Cos(2 * Math.PI * n / (N - 1)) + a2 * Math.Cos(4 * Math.PI * n / (N - 1)) - a3 * Math.Cos(6 * Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.BlackmanHarris:
                        a0 = 0.35875;
                        a1 = 0.48829;
                        a2 = 0.14128;
                        a3 = 0.01168;
                        evalFunction = delegate(int N, int n)
                        { return a0 - a1 * Math.Cos(2 * Math.PI * n / (N - 1)) + a2 * Math.Cos(4 * Math.PI * n / (N - 1)) - a3 * Math.Cos(6 * Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.BlackmanNuttall:
                        a0 = 0.3635819;
                        a1 = 0.4891775;
                        a2 = 0.1365995;
                        a3 = 0.0106411;
                        evalFunction = delegate(int N, int n)
                        { return a0 - a1 * Math.Cos(2 * Math.PI * n / (N - 1)) + a2 * Math.Cos(4 * Math.PI * n / (N - 1)) - a3 * Math.Cos(6 * Math.PI * n / (N - 1)); };
                        break;
                    case WindowType.FlatTop:
                        a0 = 1;
                        a1 = 1.93;
                        a2 = 1.29;
                        a3 = 0.388;
                        a4 = 0.032;
                        evalFunction = delegate(int N, int n)
                        { return a0 - a1 * Math.Cos(2 * Math.PI * n / (N - 1)) + a2 * Math.Cos(4 * Math.PI * n / (N - 1)) - a3 * Math.Cos(6 * Math.PI * n / (N - 1)) + a4 * Math.Cos(8 * Math.PI * n / (N - 1)); };
                        break;
                }
            }
        }
        #endregion
    }
}
