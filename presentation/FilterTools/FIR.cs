using System;
using System.Collections.Generic;

namespace FilterTools
{
    public class FIR : IFilter
    {
        #region Fields
        private double[] coeff;
        private double[] ring;
        private int offset;
        #endregion

        #region Constructor
        public FIR(double[] firCoefficients)
        {
            coeff = new double[firCoefficients.Length];
            for (int i = 0; i < coeff.Length; i++) coeff[i] = firCoefficients[coeff.Length - i - 1];
            ring = new double[firCoefficients.Length];
        }
        #endregion

        #region IFilter
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

        public void Load(double value)
        {
            for (int i = 0; i < ring.Length; i++) ring[i] = value;
            offset = ring.Length - 1;
        }

        public int Length { get { return coeff.Length; } }

        public object Clone()
        {
            return (object)new FIR(coeff);
        }
        #endregion

        #region Custom Methods
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
