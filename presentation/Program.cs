using System;
using System.Collections.Generic;

using OpenGL;
using System.Runtime.InteropServices;

namespace Presentation
{
    class Program
    {
        public static Matrix4 uiProjectionMatrix = Matrix4.Identity;
        private static System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
        private static List<Slides.Slide> slideList = new List<Slides.Slide>();
        private static float dt = 0;
        private static float t = 0;
        private static int slideNumber = 0;
        private static Slides.Slide currentSlide;

        public static int Width = 1280, Height = 720;

        public static bool Fullscreen = false;

        public static bool RunPresentation = true;

        static void Main(string[] args)
        {
            // create a window using SDL and create a valid OpenGL context
            Window.CreateWindow("2015 Presentation");
            Window.OnReshapeCallbacks.Add(OnReshape);

            // set antialiasing mode, depth test and cull face
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.Blend);
            Gl.Enable(EnableCap.Multisample);

            // set up the blending mode for ground clutter
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the shaders that we require
            Shaders.InitShaders(Shaders.ShaderVersion.GLSL120);

            // initialize commonly used object in slides such as fonts, background textures, etc
            Slides.Common.Init();

            // force a reshape to generate the UI projection matrix
            Window.OnReshape(Program.Width, Program.Height);

            // build slide 1 first
            slideList.Add(new Slides.TitleSlide("2015 Presentation", "Exporation of cool electrical engineering topics."));

            // set to slide 0
            SetSlide(30);

            // render frame 0 so that the program appears responsive while it builds the other slides
            OnRenderFrame();
            BuildSlides();

            // the main game loop
            while (RunPresentation)
            {
                OnRenderFrame();
                Window.HandleEvents();
            }
        }

        public static void OnReshape()
        {
            uiProjectionMatrix = Matrix4.CreateTranslation(new Vector3(-Program.Width / 2, -Program.Height / 2, 0)) * Matrix4.CreateOrthographic(Program.Width, Program.Height, -500, 1000);

            // the uiProjectMatrix need only be set once (unless we reshape)
            Shaders.FontShader.Use();
            Shaders.FontShader["projectionMatrix"].SetValue(uiProjectionMatrix);

            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["projectionMatrix"].SetValue(uiProjectionMatrix);
        }

        private static void OnRenderFrame()
        {
            dt = watch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();
            t += dt;

            // clear the screen
            Gl.Viewport(0, 0, Program.Width, Program.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render the current slide if it exists
            if (currentSlide != null) currentSlide.Draw();

            // swap the buffers
            Window.SwapBuffers();
        }

        private static void BuildSlides()
        {
            // check if this method has already been called
            if (slideList.Count != 1) return;

            // first slide is already built

            // introduction to presentation
            slideList.Add(new Slides.TitleAndBullets("About Me", new string[] { "Graduated 2010 in Electrical Engineering", "Working (since 2006) in the semiconductor industry", "Love hacking in C#, C and OpenGL", "GitHub: https://www.github.com/giawa", "Twitter: @giawadev" }));
            slideList.Add(new Slides.TwoImages("Exploration using Senses", "media/slide2_1.jpg", "media/slide2_2.jpg"));
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", "media/slide3.jpg", new string[] { "Microphone/ADC for Analog->Digital" }));
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", "media/slide4.jpg", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog" }));
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog", "How is audio stored in a computer?" }));
            slideList[slideList.Count - 1].CustomDraw = Slide5Render;
            slideList[slideList.Count - 1].ResetTime = false;
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog", "How is audio stored in a computer?", "As individual samples!" }));
            slideList[slideList.Count - 1].CustomDraw = Slide6Render;
            slideList[slideList.Count - 1].ResetTime = false;
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog", "How is audio stored in a computer?", "As individual samples!", "How do we sample?" }));
            slideList[slideList.Count - 1].CustomDraw = Slide6Render;
            slideList[slideList.Count - 1].ResetTime = false;

            // nyquist sampling criteria (slide 8)
            slideList.Add(new Slides.ImageAndText("Sampling Example", new string[] { "Take continuous input data" }));
            slideList[slideList.Count - 1].CustomDraw = () => Slide8Render(true);
            slideList.Add(new Slides.ImageAndText("Sampling Example", new string[] { "Take continuous input data", "Sample it" }));
            slideList[slideList.Count - 1].CustomDraw = () => Slide8Render(true, true);
            slideList[slideList.Count - 1].ResetTime = false;
            slideList.Add(new Slides.ImageAndText("Sampling Example", new string[] { "Take continuous input data", "Sample it" }));
            slideList[slideList.Count - 1].CustomDraw = () => Slide8Render(false, true);
            slideList[slideList.Count - 1].ResetTime = false;
            slideList.Add(new Slides.ImageAndText("Sampling Example", new string[] { "Take continuous input data", "Sample it", "Filter it" }));
            slideList[slideList.Count - 1].CustomDraw = () => Slide8Render(false, true, true);
            slideList[slideList.Count - 1].ResetTime = false;
            slideList.Add(new Slides.ImageAndText("Sampling Example", new string[] { "Take continuous input data", "Sample it", "Filter it" }));
            slideList[slideList.Count - 1].CustomDraw = () => Slide8Render(false, false, true);
            slideList[slideList.Count - 1].ResetTime = false;
            slideList.Add(new Slides.ImageAndText("Sampling Example", new string[] { "Take continuous input data", "Sample it", "Filter it", "Where do things go wrong?" }));
            slideList[slideList.Count - 1].CustomDraw = () => Slide8Render(false, false, true);
            slideList[slideList.Count - 1].ResetTime = false;

            slideList.Add(new Slides.TitleAndBullets("Shannon Sampling Theorem", new string[] { "Must sample at least twice our frequency of interest" }));
            slideList.Add(new Slides.TitleAndBullets("Shannon Sampling Theorem", new string[] { "Must sample at least twice our frequency of interest", "For humans we can hear up to around 20kHz or so" }));
            slideList.Add(new Slides.TitleAndBullets("Shannon Sampling Theorem", new string[] { "Must sample at least twice our frequency of interest", "For humans we can hear up to around 20kHz or so", "Audio CDs sample at 44.1kS/s, giving 22.05kHz as the highest frequency" }));
            slideList.Add(new Slides.TitleAndBullets("Shannon Sampling Theorem", new string[] { "Must sample at least twice our frequency of interest", "For humans we can hear up to around 20kHz or so", "Audio CDs sample at 44.1kS/s, giving 22.05kHz as the highest frequency", "This fits nicely with our understanding of the human ear" }));

            // fourier series (slide 18)
            slideList.Add(new Slides.ImageWithCaption("Fourier Series", "What was Jean-Baptiste Joseph Fourier trying to solve?", "media/slide18.jpg"));
            slideList.Add(new Slides.ImageWithCaption("Fourier Series", "Heat equation is difficult to solve, but sin and cos solutions are well understood!", "media/slide18.jpg"));
            slideList.Add(new Slides.ImageWithCaption("Fourier Series", "Would it be possible to approximate an arbitrary signal with sin and cos?", "media/slide18.jpg"));

            slideList.Add(new Slides.ImageAndText("Fourier Example #1", new string[] { "Let's approximate a sine wave" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries1();
            slideList.Add(new Slides.ImageAndText("Fourier Example #1", new string[] { "Let's approximate a sine wave", "Ok - that's easy" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries1(true);

            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2();
            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave", "First sine wave (fundamental)" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2(1);
            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave", "First sine wave (fundamental)", "Next sine wave (harmonic)" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2(2);
            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "3 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2(3);
            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "5 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2(5);
            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "10 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2(10);
            slideList.Add(new Slides.ImageAndText("Fourier Example #2", new string[] { "Let's approximate a square wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "50 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries2(50);

            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3();
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(1);
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)", "Next sine wave (harmonic)" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(2);
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "3 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(3);
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "5 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(5);
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "10 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(10);
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "50 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(50);
            slideList.Add(new Slides.ImageAndText("Fourier Example #3", new string[] { "Let's approximate a sawtooth wave", "First sine wave (fundamental)", "Next sine wave (harmonic)", "100 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierSeries3(100);

            // fourier transform (slide 38)
            slideList.Add(new Slides.ImageAndText("Fourier Transform Example", new string[] { "Let's go back to the square wave", "50 sine waves" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierTransformExample(0f, true);
            slideList.Add(new Slides.ImageAndText("Fourier Transform Example", new string[] { "Let's go back to the square wave", "50 sine waves", "Add depth to each sine wave" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierTransformExample(0f, false);
            slideList.Add(new Slides.ImageAndText("Fourier Transform Example", new string[] { "Let's go back to the square wave", "50 sine waves", "Add depth to each sine wave" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierTransformExample(t);
            slideList.Add(new Slides.ImageAndText("Fourier Transform Example", new string[] { "Let's go back to the square wave", "50 sine waves", "Add depth to each sine wave" }));
            slideList[slideList.Count - 1].CustomDraw = () => FourierTransformExample(10f, false, true);
        }

        public static void SetSlide(int slide)
        {
            if (slide > 0 && slideList.Count == 1) BuildSlides();

            slideNumber = slide;

            if (slideList[slideNumber] != null) currentSlide = slideList[slideNumber];
            else currentSlide = null;

            if (currentSlide != null && currentSlide.ResetTime) t = 0;
        }

        public static void NextSlide()
        {
            SetSlide(Math.Min(slideList.Count - 1, slideNumber + 1));
        }

        public static void PrevSlide()
        {
            SetSlide(Math.Max(0, slideNumber - 1));
        }

        #region Slide Rendering
        private static void Slide5Render()
        {
            float f;
            float time = (t % 10);
            if (time < 5) f = 0.02f + 0.1f * CubicEaseInOut(time, 0, 1, 5);
            else f = 0.12f - 0.1f * CubicEaseInOut(time - 5, 0, 1, 5);

            float[] sine = new float[441];
            for (int i = 0; i < sine.Length; i++)
                sine[i] = 0.5f * (float)Math.Sin((i - 441 / 2) * f);

            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            Slides.Common.DrawPlotLeft(sine, Slides.Common.TitleColor);
        }

        private static void Slide6Render()
        {
            float f;
            float time = (t % 10);
            if (time < 5) f = 0.02f + 0.1f * CubicEaseInOut(time, 0, 1, 5);
            else f = 0.12f - 0.1f * CubicEaseInOut(time - 5, 0, 1, 5);

            float[] sine = new float[441];
            for (int i = 0; i < sine.Length; i++)
                sine[i] = 0.5f * (float)Math.Sin((i - 441 / 2) * f);

            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            Slides.Common.DrawPlotLeft(sine, Slides.Common.TitleColor);

            float[] samples = new float[441];
            for (int i = 0; i < sine.Length; i++)
            {
                if ((i % 10) == 0) samples[i] = sine[i];
            }

            Slides.Common.DrawPlotLeft(samples, new Vector3(1, 0, 0));
        }

        private static Text frequencyText;

        private static void Slide8Render(bool drawOriginal = false, bool drawSamples = false, bool drawFiltered = false)
        {
            if (frequencyText == null) frequencyText = new Text(Text.FontSize._24pt, "0Hz @ 1000Hz", Slides.Common.TextColor);

            float f = t * 0.05f;
            if (f * 5000 > Math.PI * 1000) t = 0;
            frequencyText.String = string.Format("{0:0.0}Hz @ 1000Hz", f / Math.PI * 5000);

            float[] sine = new float[441];
            for (int i = 0; i < sine.Length; i++)
                sine[i] = 0.5f * (float)Math.Sin((i - 441 / 2) * f);

            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            if (drawOriginal) Slides.Common.DrawPlotLeft(sine, Slides.Common.TitleColor);

            float[] samples = new float[441];
            for (int i = 0; i < sine.Length; i++)
            {
                samples[i] = sine[(int)(i / 10) * 10];
            }

            FilterTools.BiQuad lpf = new FilterTools.BiQuad(2205, 0.707, 44100, 0, FilterTools.BiQuad.Type.LPF);
            lpf.Child = (FilterTools.BiQuad)lpf.Clone();

            float[] filter = new float[441];
            lpf.Load(samples[0]);
            for (int i = 0; i < filter.Length; i++) filter[i] = (float)lpf.GetOutput(samples[i]);

            if (drawSamples) Slides.Common.DrawPlotLeft(samples, new Vector3(1, 0, 0));
            if (drawFiltered) Slides.Common.DrawPlotLeft(filter, new Vector3(0, 1, 0));

            if (drawSamples || drawFiltered)
            {
                frequencyText.ModelMatrix = Matrix4.CreateTranslation(new Vector3(70, 500, 0));
                frequencyText.Draw();
            }
        }

        private static void FourierSeries1(bool drawFourier = false)
        {
            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            Slides.Common.DrawSineLeft((drawFourier ? new Vector3(1, 0, 0) : Slides.Common.TitleColor), 0.1f);
        }

        private static void FourierSeries2(int sineWaves = 0)
        {
            Gl.Disable(EnableCap.DepthTest);
            Gl.LineWidth(2f);
            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));

            float[] squareWave = new float[441];
            for (int i = 0; i < squareWave.Length; i++) squareWave[i] = ((i % 100) < 50) ? 0.5f : -0.5f;

            Slides.Common.DrawPlotLeft(squareWave, Slides.Common.TitleColor);

            if (sineWaves > 0)
            {
                float[] fourier = new float[441];

                for (int i = sineWaves - 1; i >= 0; i--)
                {
                    float[] sine = new float[441];

                    for (int j = 0; j < 441; j++)
                    {
                        sine[j] = 0.63662f * (float)Math.Sin(Math.PI * 2 / 100 * j * (1 + i * 2)) / (1 + i * 2);
                        fourier[j] += sine[j];
                    }

                    if (sineWaves != 1 && i < 10) Slides.Common.DrawPlotLeft(sine, new Vector3(0.2 + i * 0.06f, 0.2 + i * 0.06f, 1));
                }

                Slides.Common.DrawPlotLeft(fourier, new Vector3(1, 0, 0));
            }
            Gl.Enable(EnableCap.DepthTest);
        }

        private static void FourierSeries3(int sineWaves = 0)
        {
            Gl.Disable(EnableCap.DepthTest);
            Gl.LineWidth(2f);
            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));

            float[] squareWave = new float[441];
            for (int i = 0; i < squareWave.Length; i++) squareWave[i] = -(((i + 29) % 100) - 50) / 100f;//((i % 100) < 50) ? 0.5f : -0.5f;

            Slides.Common.DrawPlotLeft(squareWave, Slides.Common.TitleColor);

            if (sineWaves > 0)
            {
                float[] fourier = new float[441];

                for (int i = sineWaves - 1; i >= 0; i--)
                {
                    float[] sine = new float[441];

                    for (int j = 0; j < 441; j++)
                    {
                        sine[j] = 0.3183f * (float)Math.Sin(Math.PI * 2 / 100 * (j - 21) * (1 + i)) / (i % 2 == 0 ? -(i + 1) : (i + 1));
                        fourier[j] += sine[j];
                    }

                    if (sineWaves != 1 && i < 10) Slides.Common.DrawPlotLeft(sine, new Vector3(0.2 + i * 0.06f, 0.2 + i * 0.06f, 1));
                }

                Slides.Common.DrawPlotLeft(fourier, new Vector3(1, 0, 0));
            }
            Gl.Enable(EnableCap.DepthTest);
        }

        private static Text fourierText;

        private static void FourierTransformExample(float transformation, bool drawSquare = false, bool drawAbs = false)
        {
            if (transformation >= 9.99f) transformation = 9.99f;
            transformation /= 10;

            float t2 = (transformation > 0.5 ? 1 - transformation : transformation);
            float t3 = CubicEaseInOut(t2 * 2, 0, 0.2f, 1);
            float t4 = CubicEaseInOut(transformation, 0, 1, 1);

            // calculate a cool camera transformation
            Matrix4 rotateY = Quaternion.Slerp(Quaternion.Identity, Quaternion.FromRotationMatrix(Matrix4.CreateRotationY((float)Math.PI / 2)), t4).Matrix4;
            Matrix4 rotateX = Quaternion.Slerp(Quaternion.Identity, Quaternion.FromRotationMatrix(Matrix4.CreateRotationX((float)Math.PI / -2)), t3).Matrix4 * Matrix4.CreateTranslation(new Vector3(0, t3 * 120, 0));
            Matrix4 viewMatrix = rotateY * Matrix4.CreateTranslation(new Vector3(t4 * 100, 0, 0)) * rotateX;

            // draw the plotter frame
            Gl.Disable(EnableCap.DepthTest);
            Gl.LineWidth(2f);

            if (transformation == 0) Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            else if (transformation >= 0.95)
            {
                Slides.Common.DrawXPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));

                if (drawAbs && fourierText == null)
                {
                    fourierText = new Text(Text.FontSize._24pt, "f  3f  5f  7f 9f  11f ....", Slides.Common.SubtitleColor);
                    fourierText.ModelMatrix = Matrix4.CreateTranslation(new Vector3(125, 265, 0));
                }
                if (drawAbs) fourierText.Draw();
            }
            else Slides.Common.DrawBox(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)), Slides.Common.SubtitleColor);

            // enable stencil testing to ensure that the plots are clipped to inside the plotter
            Slides.Common.EnableStencil(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            Gl.StencilFunc(StencilFunction.Equal, 1, 0xFFFF);

            // create the square wave
            if (drawSquare)
            {
                float[] squareWave = new float[441];
                for (int i = 0; i < squareWave.Length; i++) squareWave[i] = ((i % 100) < 50) ? 0.5f : -0.5f;

                Slides.Common.Draw3DPlotLeft(squareWave, 0, Slides.Common.TitleColor, viewMatrix);
            }

            float[] fourier = new float[441];

            for (int i = 49; i >= 0; i--)
            {
                float[] sine = new float[441];

                for (int j = 0; j < 441; j++)
                {
                    sine[j] = 0.63662f * (float)Math.Sin(Math.PI * 2 / 100 * j * (1 + i * 2)) / (1 + i * 2);
                    if (drawAbs) sine[j] = Math.Abs(sine[j]);
                    fourier[j] += sine[j];
                }

                if (i < 13)
                    Slides.Common.Draw3DPlotLeft(sine, (i + 1) * 30, new Vector3(0.2 + i * 0.06f, 0.2 + i * 0.06f, 1), viewMatrix);
            }

            if (drawSquare) Slides.Common.Draw3DPlotLeft(fourier, 0, new Vector3(1, 0, 0), viewMatrix);
            Gl.Disable(EnableCap.StencilTest);
            Gl.Enable(EnableCap.DepthTest);
        }

        private static float CubicEaseInOut(float t, float b, float c, float d)
        {
            if ((t /= d / 2) < 1)
                return c / 2 * t * t * t + b;

            return c / 2 * ((t -= 2) * t * t + 2) + b;
        }
        #endregion
    }
}
