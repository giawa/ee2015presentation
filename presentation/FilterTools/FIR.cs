using System;
using System.Collections.Generic;

namespace FilterTools
{
    public class FIR : IFilter
    {
        #region Private Fields
        private double[] coeff;
        private double[] ring;
        private int offset;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs an FIR with the provided coefficients.
        /// </summary>
        /// <param name="firCoefficients">A list of coefficients that make up the FIR filter.</param>
        public FIR(double[] firCoefficients)
        {
            coeff = new double[firCoefficients.Length];
            for (int i = 0; i < coeff.Length; i++) coeff[i] = firCoefficients[coeff.Length - i - 1];
            ring = new double[firCoefficients.Length];
        }
        #endregion

        #region IFilter
        /// <summary>
        /// Applies the input to the first delay of the filter and
        /// calculates the output by taking the convolution of the 
        /// delay line with the coefficients.
        /// </summary>
        /// <param name="input">The input sample.</param>
        /// <returns>The output of the FIR filter.</returns>
        public double GetOutput(double input)
        {
            double sum = 0;

            ring[offset] = input;

            for (int i = 0; i < coeff.Length; i++)
                sum += coeff[i] * ring[(offset + i) % ring.Length];

            offset = (offset - 1);
            if (offset < 0) offset = ring.Length - 1;
            return sum;
        }

        /// <summary>
        /// Loads the FIR filter with a single value, which can
        /// be useful to remove transients when performing sweeps/etc.
        /// </summary>
        /// <param name="value">The value to stuff into the delay line.</param>
        public void Load(double value)
        {
            for (int i = 0; i < ring.Length; i++) ring[i] = value;
            offset = ring.Length - 1;
        }

        /// <summary>
        /// The length of the FIR filter.
        /// </summary>
        public int Length { get { return coeff.Length; } }

        /// <summary>
        /// Creates a copy of this FIR filter.
        /// </summary>
        public object Clone()
        {
            return (object)new FIR(coeff);
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Applies the FIR filter to an array of input data.
        /// This is equivalent to calling GetOutput(double) in
        /// a loop on the input data, but can be slightly higher
        /// performance.
        /// </summary>
        /// <param name="input">The input array to apply to this FIR filter.</param>
        /// <returns>An output array of the same length of 'input' that has been filtered.</returns>
        public double[] GetOutput(double[] input)
        {
            double[] output = new double[input.Length - this.Length];

            for (int i = 0; i < output.Length; i++)
            {
                double sum = 0;
                for (int j = 0; j < coeff.Length; j++)
                {
                    sum += coeff[j] * input[i + j];
                }
                output[i] = sum;
            }

            return output;
        }
        #endregion
    }
}
