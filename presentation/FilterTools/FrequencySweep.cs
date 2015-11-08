using System;

namespace FilterTools
{
    /// <summary>
    /// Interface for a standard filter (single input single output).
    /// This interface is used by filter analysis algorithms, such as
    /// plotting frequency response, etc.
    /// </summary>
    public interface IFilter : ICloneable
    {
        /// <summary>
        /// The total group delay length of the filter, including any children.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Gets a floating-point output from the filter.
        /// </summary>
        /// <param name="Input">Floating-point input.</param>
        /// <returns>Floating point output.</returns>
        double GetOutput(double Input);

        /// <summary>
        /// Loads all value in to the delay line.
        /// </summary>
        /// <param name="Value">The value to load in to all z of the filter.</param>
        void Load(double Value);
    }

    public class FrequencySweep
    {
        #region Variables
        private double phase, incStart, incStop, inc2, delT;
        private delegate double dFunction(double in2, double inStart);
        private dFunction incFunction;
        private bool log = false;
        #endregion

        #region Constructors
        /// <summary>Sets up a frequency sweep with default non-log and 2000 points.</summary>
        public FrequencySweep(double startFreq, double stopFreq, double deltaT)
        {
            setup(startFreq, stopFreq, deltaT, 2000, false);
        }

        /// <summary>Sets up a frequency sweep with default non-log.</summary>
        public FrequencySweep(double startFreq, double stopFreq, double deltaT, int numPoints)
        {
            setup(startFreq, stopFreq, deltaT, numPoints, false);
        }

        /// <summary>Sets up a frequency sweep with no defaults.</summary>
        public FrequencySweep(double startFreq, double stopFreq, double deltaT, int numPoints, bool geometric)
        {
            setup(startFreq, stopFreq, deltaT, numPoints, geometric);
        }
        #endregion

        #region Methods
        protected void setup(double startFreq, double stopFreq, double deltaT, int numPoints, bool geometric)
        {
            delT = deltaT;
            log = geometric;
            // Lisp translation of frequency-sweep constructor
            phase = 0;
            incStart = 2 * Math.PI * deltaT * startFreq;
            incStop = 2 * Math.PI * deltaT * stopFreq;
            if (log) inc2 = Math.Pow(startFreq / stopFreq, 1 / numPoints);
            else inc2 = (incStop - incStart) / numPoints;
            if (log)
                incFunction = delegate(double a, double b) { return a * b; };
            else
                incFunction = delegate(double a, double b) { return a + b; };
        }

        /// <summary>
        /// Get the next cosine value from the frequency sweep
        /// </summary>
        /// <returns>GetCosine[0] = Cos(phase), GetCosine[1] = phase, GetCosine[2] = frequency</returns>
        public double GetCosine(out double _phase, out double _freq)
        {
            double _output = Math.Cos(phase);
            _phase = phase;
            _freq = incStart / (2 * Math.PI * delT);
            phase += incStart;
            incStart = incFunction(inc2, incStart);
            return _output;
        }

        public double GetCosine(out double _freq)
        {
            double p;
            return GetCosine(out p, out _freq);
        }

        public double GetCosine()
        {
            double p, f;
            return GetCosine(out p, out f);
        }

        /// <summary>
        /// Get the next sine value from the frequency sweep
        /// </summary>
        /// <returns>GetSine[0] = Sin(phase), GetSine[1] = phase, GetSine[2] = frequency</returns>
        public double GetSine(out double _phase, out double _freq)
        {
            double _output = Math.Sin(phase);
            _phase = phase;
            phase += incStart;
            incStart = incFunction(inc2, incStart);
            _freq = incStart / (2 * Math.PI * delT);
            return _output;
        }

        public double GetQuadrature(out double sine, out double cosine)
        {
            sine = Math.Sin(phase);
            cosine = Math.Cos(phase);
            phase += incStart;
            incStart = incFunction(inc2, incStart);
            return incStart / (2 * Math.PI * delT);
        }

        public double GetSine(out double _freq)
        {
            double p;
            return GetSine(out p, out _freq);
        }

        public double GetSine()
        {
            double p, f;
            return GetSine(out p, out f);
        }
        #endregion
    }

    public class FrequencyResponseFromQuadratureTimeDomain
    {
        #region Variables
        private FrequencySweep sweep;
        private IFilter iFilter;
        private IFilter qFilter;
        private double opFreq;
        private int numPoints, samplerate;
        #endregion

        #region Constructor
        /// <summary>
        /// Sweeps across a filter in quadrature domains (sine and cosine) to find the
        /// frequency response.  Is interesting because this is a simulation of a real filter,
        /// which may be the only way to analyze a black box object that implements IFilter.
        /// </summary>
        public FrequencyResponseFromQuadratureTimeDomain(IFilter Filter,
            double startFreq, double stopFreq, double operateFreq, double runTime, int sampleRate)
        {
            samplerate = (int)sampleRate;
            numPoints = (int)(runTime * operateFreq);
            opFreq = operateFreq;
            sweep = new FrequencySweep(startFreq, stopFreq, 1 / operateFreq, numPoints);
            iFilter = (IFilter)Filter.Clone();
            qFilter = (IFilter)Filter.Clone();
            iFilter.Load(0.0);
            qFilter.Load(1.0);
        }
        #endregion

        #region Methods
        /// <summary>
        /// The number of points going through the filter.
        /// </summary>
        public int Size
        {
            get { return numPoints; }
        }

        private double GetResp(out double i, out double q, out double f)
        {
            f = sweep.GetQuadrature(out i, out q);
            i = iFilter.GetOutput(i);
            q = qFilter.GetOutput(q);
            return Math.Sqrt(i * i + q * q);
        }

        private double GetResp(out double f)
        {
            double i, q;
            f = sweep.GetQuadrature(out i, out q);
            i = iFilter.GetOutput(i);
            q = qFilter.GetOutput(q);
            return Math.Sqrt(i * i + q * q);
        }

        /// <summary>
        /// Plots the magnitude response of the IFilter object.
        /// </summary>
        /// <param name="targetPlot">The plot control to draw the frequency response</param>
        /// <returns>The frequency and magnitude in a two dimensional array.</returns>
        public double[,] GetResponse()
        {
            double[,] output = new double[2, numPoints];
            //double[,] sine = new double[2, numPoints];
            //double[,] cosine = new double[2, numPoints];
            //double sine, cosine;

            for (int i = 0; i < numPoints; i++)
            {
                //output[1, i] = GetResp(out sine, out cosine, out output[0, i]);//out sine[1, i], out cosine[1, i], out output[0, i]);
                output[1, i] = GetResp(out output[0, i]);
                if (double.IsNaN(output[1, i]))
                    return null;
                //sine[0, i] = cosine[0, i] = output[0, i];
            }

            //if (drawSines) targetSinePlot.AddPlot("sine", sine, Color.Red/*, fLength, true*/);
            //if (drawSines) targetSinePlot.AddPlot("cosine", cosine, Color.Blue/*, fLength, true*/);
            return output;
        }

        public static double[,] GetResponse(IFilter filter, double start, double stop, double samplerate, double time)
        {
            FrequencyResponseFromQuadratureTimeDomain resp = new FrequencyResponseFromQuadratureTimeDomain(filter, start, stop, samplerate, time, (int)samplerate);
            return resp.GetResponse();
        }

        /// <summary>
        /// Returns the i and q filters (in that order) as a Tuple.
        /// </summary>
        /// <returns>The iFilter and qFilter objects (in that order).</returns>
        public Tuple<IFilter, IFilter> GetFilters()
        {
            return new Tuple<IFilter, IFilter>(iFilter, qFilter);
        }
        #endregion
    }

    public static class ImpulseResponse
    {
        #region Methods
        /// <summary>
        /// Calculate the impulse response of an IFilter object.
        /// </summary>
        /// <param name="filter">The filter to use when calculating the impulse response.</param>
        /// <returns>The impulse response with length filter.Length.</returns>
        public static double[] GetResponse(IFilter filter)
        {
            return GetResponse(filter, filter.Length);
        }

        /// <summary>
        /// Calculate the impulse response of an IFilter object using an arbitrary length.
        /// </summary>
        /// <param name="filter">The filter to use when calculating the impulse response.</param>
        /// <param name="length">The length of the impulse response to calculate.</param>
        /// <returns>The impulse response with the provided length.</returns>
        public static double[] GetResponse(IFilter filter, int length)
        {
            double[] response = new double[length];
            filter.Load(0);
            response[0] = filter.GetOutput(1);
            for (int i = 1; i < length; i++)
                response[i] = filter.GetOutput(0);
            return response;
        }
        #endregion
    }

    public static class StepResponse
    {
        #region Methods
        /// <summary>
        /// Calculate the step response of an IFilter object.
        /// </summary>
        /// <param name="filter">The filter to use when calculating the step response.</param>
        /// <returns>The step response with length filter.Length.</returns>
        public static double[] GetResponse(IFilter filter)
        {
            return GetResponse(filter, filter.Length);
        }

        /// <summary>
        /// Calculate the step response of an IFilter object using an arbitrary length.
        /// </summary>
        /// <param name="filter">The filter to use when calculating the step response.</param>
        /// <param name="length">The length of the impulse response to calculate.</param>
        /// <returns>The step response with the provided length.</returns>
        public static double[] GetResponse(IFilter filter, int length)
        {
            double[] response = new double[length];
            filter.Load(0);
            response[0] = filter.GetOutput(1);
            for (int i = 1; i < length; i++)
                response[i] = filter.GetOutput(1);
            return response;
        }
        #endregion
    }
}
