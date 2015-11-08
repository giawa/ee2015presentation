using System;
using System.Runtime.Serialization;

namespace FilterTools
{
    [Serializable]
    public class BiQuad : IFilter
    {
        #region Enumerations
        public enum Type
        {
            Off = 0,
            LPF = 1,
            HPF = 2,
            BPF_Q_Peak = 3,
            BPF_0_Peak = 4,
            Notch = 5,
            APF = 6,
            PeakingEQ = 7,
            LowShelf = 8,
            HighShelf = 9,
            Custom = 10,
            ButterworthLP = 11,
            ButterworthHP = 12
        }
        #endregion

        #region Coefficient Generation
        public static void LPF(out double[] A, out double[] B, double omega, double alpha, double gain)
        {
            double b0 = (1 - Math.Cos(omega)) / 2;
            double b1 = 1 - Math.Cos(omega);
            double b2 = (1 - Math.Cos(omega)) / 2;
            double a0 = 1 + alpha;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha;

            double scale = Math.Pow(10, gain / 20);

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0 * scale, b1 / a0 * scale, b2 / a0 * scale };
        }

        public static void HPF(out double[] A, out double[] B, double omega, double alpha, double gain)
        {
            double b0 = (1 + Math.Cos(omega)) / 2;
            double b1 = -(1 + Math.Cos(omega));
            double b2 = (1 + Math.Cos(omega)) / 2;
            double a0 = 1 + alpha;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha;

            double scale = Math.Pow(10, gain / 20);//Math.Min(0, gain / 20));

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0 * scale, b1 / a0 * scale, b2 / a0 * scale };
        }

        public static void BPF_Q_Peak(out double[] A, out double[] B, double omega, double alpha, double q)
        {
            double b0 = q * alpha;
            double b1 = 0;
            double b2 = -q * alpha;
            double a0 = 1 + alpha;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha;

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0, b1 / a0, b2 / a0 };
        }

        public static void BPF_0_Peak(out double[] A, out double[] B, double omega, double alpha)
        {
            double b0 = alpha;
            double b1 = 0;
            double b2 = -alpha;
            double a0 = 1 + alpha;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha;

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0, b1 / a0, b2 / a0 };
        }

        public static void Notch(out double[] A, out double[] B, double omega, double alpha)
        {
            double b0 = 1;
            double b1 = -2 * Math.Cos(omega);
            double b2 = 1;
            double a0 = 1 + alpha;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha;

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0, b1 / a0, b2 / a0 };
        }

        public static void APF(out double[] A, out double[] B, double omega, double alpha, double gain)
        {
            double scale = Math.Pow(10, Math.Min(0, gain / 20));

            A = new double[] { 1, 0, 0 };
            B = new double[] { scale, 0, 0 };
        }

        public static void PeakingEQ(out double[] A, out double[] B, double omega, double alpha, double a)
        {
            double b0 = 1 + alpha * a;
            double b1 = -2 * Math.Cos(omega);
            double b2 = 1 - alpha * a;
            double a0 = 1 + alpha / a;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha / a;

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0, b1 / a0, b2 / a0 };
        }

        public static void LowShelf(out double[] A, out double[] B, double omega, double alpha, double a)
        {
            double b0 = a * ((a + 1) - (a - 1) * Math.Cos(omega) + 2 * Math.Sqrt(a) * alpha);
            double b1 = 2 * a * ((a - 1) - (a + 1) * Math.Cos(omega));
            double b2 = a * ((a + 1) - (a - 1) * Math.Cos(omega) - 2 * Math.Sqrt(a) * alpha);
            double a0 = (a + 1) + (a - 1) * Math.Cos(omega) + 2 * Math.Sqrt(a) * alpha;
            double a1 = -2 * ((a - 1) + (a + 1) * Math.Cos(omega));
            double a2 = (a + 1) + (a - 1) * Math.Cos(omega) - 2 * Math.Sqrt(a) * alpha;

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0, b1 / a0, b2 / a0 };
        }

        public static void HighShelf(out double[] A, out double[] B, double omega, double alpha, double a)
        {
            double b0 = a * ((a + 1) + (a - 1) * Math.Cos(omega) + 2 * Math.Sqrt(a) * alpha);
            double b1 = -2 * a * ((a - 1) + (a + 1) * Math.Cos(omega));
            double b2 = a * ((a + 1) + (a - 1) * Math.Cos(omega) - 2 * Math.Sqrt(a) * alpha);
            double a0 = (a + 1) - (a - 1) * Math.Cos(omega) + 2 * Math.Sqrt(a) * alpha;
            double a1 = 2 * ((a - 1) - (a + 1) * Math.Cos(omega));
            double a2 = (a + 1) - (a - 1) * Math.Cos(omega) - 2 * Math.Sqrt(a) * alpha;

            A = new double[] { 1.0, a1 / a0, a2 / a0 };
            B = new double[] { b0 / a0, b1 / a0, b2 / a0 };
        }

        public static void Butterworth(out double[] A, out double[] B, double fc, double q, double samplerate, bool highpass)
        {
            double k = Math.Tan(Math.PI * fc / samplerate);

            double a0 = k * k;
            double a1 = 2 * k * k;
            double a2 = k * k;
            double b0 = k * k + k / q + 1;
            double b1 = 2 * (k * k - 1);
            double b2 = k * k - k / q + 1;

            B = new double[] { a0 / b0, a1 / b0, a2 / b0 };
            A = new double[] { 1, b1 / b0, b2 / b0 };
        }
        #endregion

        #region Variables
        private double z1, z2, scale;
        private double[] A, B;
        public double Scale { get { return scale; } }
        public double[] ACoeff { get { return A.Clone() as double[]; } }
        public double[] BCoeff { get { return B.Clone() as double[]; } }
        public BiQuad Child { get; set; }
        public double CenterFrequency { get; private set; }
        public double Samplerate { get; set; }
        public double Q { get; private set; }
        public double dBgain { get; private set; }
        public double Omega { get { return 2 * Math.PI * CenterFrequency / Samplerate; } }
        public Type FilterType { get; private set; }

        public double Bandwidth
        {
            get
            {   // BW = asinh(1/(2*Q)) * 2 / ln(2) * sin(omega) / omega
                return asinh(1 / (2 * Q)) * 2.8854 * Math.Sin(Omega) / Omega;
            }
        }

        private double asinh(double x)
        {
            return x > 0 ? Math.Log(x + Math.Sqrt(x * x + 1)) : -Math.Log(Math.Sqrt(x * x + 1) - x);
        }

        public int Length
        {
            get { return 500 + (Child == null ? 0 : Child.Length); }
        }
        #endregion

        #region Constructor
        public BiQuad(double scale, double[] A, double[] B)
        {
            z1 = 0; z2 = 0;
            this.scale = scale;
            this.A = A;
            this.B = B;
            this.FilterType = Type.Custom;
            this.CenterFrequency = 0;
            Samplerate = CenterFrequency = Q = dBgain = 0;
        }

        public BiQuad(double[] A, double[] B, double center, double samplerate)
            : this(1.0, A, B)
        {
            this.CenterFrequency = center;
            this.Samplerate = samplerate;
            this.Q = 1;
        }

        public BiQuad(double center, double q, double samplerate, double dBgain, Type Type)
        {
            CreateBiquad(center, q, samplerate, dBgain, Type);
        }

        public BiQuad(double center, double samplerate)
        {
            CreateBiquad(center, 4, samplerate, 0, Type.PeakingEQ);
        }

        public BiQuad(double center)
        {
            CreateBiquad(center, 4, 48000, 0, Type.PeakingEQ);
        }
        #endregion

        #region IFilter
        public double[] ApplyFilter(double[] input)
        {
            double[] output = new double[input.Length];
            for (int i = 0; i < input.Length; i++) output[i] = this.GetOutput(input[i]);
            return output;
        }

        public double[,] ApplyFilter(double[,] input)
        {
            double[,] output = new double[2, input.Length / 2];
            for (int i = 0; i < input.Length / 2; i++)
            {
                output[0, i] = input[0, i];
                output[1, i] = this.GetOutput(input[1, i]);
            }
            return output;
        }

        public double GetOutput(double input)
        {
            double scaled = input * scale;
            double ret = scaled * B[0] + z2;
            z2 = z1 + B[1] * scaled - A[1] * ret;
            z1 = B[2] * scaled - A[2] * ret;
            if (Child != null) return Child.GetOutput(ret);
            else return ret;
        }

        public object Clone(bool Children)
        {
            BiQuad ret = this.MemberwiseClone() as BiQuad;
            if (ret.Child != null && Children) ret.Child = this.Child.Clone() as BiQuad;
            else ret.Child = null;
            return (object)ret;
        }

        public object Clone()
        {
            BiQuad ret = this.MemberwiseClone() as BiQuad;
            if (Child != null)
                ret.Child = (BiQuad)this.Child.Clone();
            return ret;
        }
        #endregion

        #region Methods
        public void SetBandwidth(double BW)
        {
            double invq = 2 * Math.Sinh(0.34657359 * BW * Omega / Math.Sin(Omega));
            CreateBiquad(CenterFrequency, 1 / invq, Samplerate, dBgain, FilterType);
        }

        public void SetGain(double dBGain)
        {
            CreateBiquad(CenterFrequency, Q, Samplerate, dBGain, FilterType);
        }

        public void SetFilterType(Type Type)
        {
            this.FilterType = Type;
            if (Type == BiQuad.Type.Off)
            {
                APF(out this.A, out this.B, 0, 0, 0);
            }
        }

        public void SetSamplerate(double samplerate)
        {
            this.Samplerate = samplerate;
            if (this.FilterType != Type.Custom) CreateBiquad(this.CenterFrequency, this.Q, samplerate, this.dBgain, this.FilterType);
        }

        public void CreateBiquad(double scale, double[] A, double[] B, double samplerate)
        {
            this.FilterType = Type.Custom;
            this.Samplerate = samplerate;

            this.scale = 1;// scale;
            this.A = A;
            this.B = new double[] { B[0] * scale, B[1] * scale, B[2] * scale };
        }

        public void CreateBiquad(double center, double q, double samplerate, double dBgain, Type type)
        {
            this.CenterFrequency = center;
            this.Samplerate = samplerate;
            if (this.FilterType == Type.Custom)
            {
                double w = center / samplerate * 2 * Math.PI;    // find omega in terms of radians
                this.dBgain = 20 * Math.Log10(this.Amplitude(w));
                this.Q = 0;
                return;
            }
            this.Q = q;
            this.dBgain = dBgain;
            this.FilterType = type;
            this.scale = 1.0;

            double a = Math.Pow(10, dBgain / 40);
            double omega = 2 * Math.PI * center / samplerate;
            double alpha = Math.Sin(omega) / (2 * q);

            // Note: No need to set scale in here - it is done in the methods themselves (incorporated into the B array usually)
            switch (type)
            {
                case Type.LPF: LPF(out this.A, out this.B, omega, alpha, dBgain);
                    break;
                case Type.HPF: HPF(out this.A, out this.B, omega, alpha, dBgain);
                    break;
                case Type.BPF_Q_Peak: BPF_Q_Peak(out this.A, out this.B, omega, alpha, q);
                    break;
                case Type.BPF_0_Peak: BPF_0_Peak(out this.A, out this.B, omega, alpha);
                    break;
                case Type.Notch: Notch(out this.A, out this.B, omega, alpha);
                    break;
                case Type.APF: APF(out this.A, out this.B, omega, alpha, dBgain);
                    break;
                case Type.PeakingEQ: PeakingEQ(out this.A, out this.B, omega, alpha, a);
                    break;
                case Type.LowShelf: LowShelf(out this.A, out this.B, omega, alpha, a);
                    break;
                case Type.HighShelf: HighShelf(out this.A, out this.B, omega, alpha, a);
                    break;
                case Type.ButterworthLP: Butterworth(out this.A, out this.B, center, q, samplerate, false);
                    break;
                case Type.ButterworthHP: Butterworth(out this.A, out this.B, center, q, samplerate, true);
                    break;
                default: this.A = new double[] { 1.0, 0.0, 0.0 }; this.B = new double[] { 0.0, 0.0, 0.0 };
                    break;
            }

            // Lets try to balance the DNR of the scale and b coefficients
            // The maximum scaling factor for B is 1.0 / max(B)
            double maxScale = 1.0 / Math.Max(B[0], Math.Max(B[1], B[2]));

            // The scale factor we want to apply is sqrt(scale * min(B))
            double reqScale = 1.0 / Math.Sqrt(Math.Abs(scale * Math.Min(B[0], Math.Min(B[1], B[2]))));

            // The actual scaling factor is the min of maxScale and reqScale
            double actScale = Math.Min(maxScale, reqScale);

            actScale = Math.Min(8, actScale);
            if (actScale < 1 && type == Type.HPF) actScale *= 2;

            B[0] *= actScale;
            B[1] *= actScale;
            B[2] *= actScale;
            scale /= actScale;
        }

        public double Amplitude(double w)
        {
            double[] B = new double[] { scale * this.B[0], scale * this.B[1], scale * this.B[2] };

            double y = Math.Sqrt(square(B[0] + B[1] + B[2]) - 4 * (B[0] * B[1] + 4 * B[0] * B[2] + B[1] * B[2]) * phi(w) + 16 * B[0] * B[2] * square(phi(w))) /
                    Math.Sqrt(square(A[0] + A[1] + A[2]) - 4 * (A[0] * A[1] + 4 * A[0] * A[2] + A[1] * A[2]) * phi(w) + 16 * A[0] * A[2] * square(phi(w)));
            if (double.IsNaN(y)) y = 0;
            return y;
        }

        public double[,] FrequencyResponse(double start, double stop, double samplerate, int points, double low)
        {
            double[,] childResp = null;
            if (Child != null) childResp = Child.FrequencyResponse(start, stop, samplerate, points, low);

            // sweep across in the log domain for equal distribution of points on the plot
            double deltaLog = (Math.Log10(stop) - Math.Log10(start)) / points;
            double logFreq = Math.Log10(start);
            double[] B = new double[] { scale * this.B[0], scale * this.B[1], scale * this.B[2] };

            double[,] response = new double[2, points];
            for (int i = 0; i < points; i++)
            {
                double w = Math.Pow(10, logFreq) / samplerate * 2 * Math.PI;    // find omega in terms of radians
                response[0, i] = Math.Pow(10, logFreq);                         // x axis is the frequency
                response[1, i] = Math.Sqrt(square(B[0] + B[1] + B[2]) - 4 * (B[0] * B[1] + 4 * B[0] * B[2] + B[1] * B[2]) * phi(w) + 16 * B[0] * B[2] * square(phi(w))) /
                    Math.Sqrt(square(A[0] + A[1] + A[2]) - 4 * (A[0] * A[1] + 4 * A[0] * A[2] + A[1] * A[2]) * phi(w) + 16 * A[0] * A[2] * square(phi(w)));
                if (double.IsNaN(response[1, i]) || response[1, i] == 0)
                    response[1, i] = (i > 0) ? response[1, i - 1] : 0;
                if (childResp != null) response[1, i] *= childResp[1, i];
                response[1, i] = Math.Max(low, response[1, i]);
                logFreq += deltaLog;
            }
            return response;
        }

        public double[,] FrequencyResponse(double start, double stop, double samplerate, int points)
        {
            double[,] childResp = null;
            if (Child != null) childResp = Child.FrequencyResponse(start, stop, samplerate, points);

            // sweep across in the log domain for equal distribution of points on the plot
            double deltaLog = (Math.Log10(stop) - Math.Log10(start)) / points;
            double logFreq = Math.Log10(start);
            double[] B = new double[] { scale * this.B[0], scale * this.B[1], scale * this.B[2] };

            double[,] response = new double[2, points];
            for (int i = 0; i < points; i++)
            {
                double w = Math.Pow(10, logFreq) / samplerate * 2 * Math.PI;    // find omega in terms of radians
                response[0, i] = Math.Pow(10, logFreq);                         // x axis is the frequency
                response[1, i] = Math.Sqrt(square(B[0] + B[1] + B[2]) - 4 * (B[0] * B[1] + 4 * B[0] * B[2] + B[1] * B[2]) * phi(w) + 16 * B[0] * B[2] * square(phi(w))) /
                    Math.Sqrt(square(A[0] + A[1] + A[2]) - 4 * (A[0] * A[1] + 4 * A[0] * A[2] + A[1] * A[2]) * phi(w) + 16 * A[0] * A[2] * square(phi(w)));
                if (double.IsNaN(response[1, i]) || response[1, i] == 0)
                    response[1, i] = (i > 0) ? response[1, i - 1] : 0;
                if (childResp != null) response[1, i] *= childResp[1, i];
                logFreq += deltaLog;
            }
            return response;
        }

        private double phi(double w) { return square(Math.Sin(w / 2)); }

        private double square(double x) { return x * x; }

        public override string ToString()
        {
            return string.Format("{0} biquad(s) @{1}Hz, {2}dB", FilterType.ToString(), CenterFrequency, dBgain);
        }

        public double GetMaxCoefficient()
        {
            double thisMax = Math.Max(Math.Abs(A[0]), Math.Max(Math.Abs(A[1]), Math.Max(Math.Abs(A[2]),
                Math.Max(Math.Abs(B[0]), Math.Max(Math.Abs(B[1]), Math.Abs(B[2]))))));
            return (Child != null) ? Math.Max(Child.GetMaxCoefficient(), thisMax) : thisMax;
        }

        /// <summary>
        /// Loads all value in to the delay line.
        /// </summary>
        /// <param name="Value">The value to load in to all z of the filter.</param>
        public void Load(double Value)
        {
            for (int i = 0; i < this.Length; i++) GetOutput(Value);
        }
        #endregion
    }
}
