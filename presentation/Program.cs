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
            SetSlide(0);

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
            uiProjectionMatrix = Matrix4.CreateTranslation(new Vector3(-Program.Width / 2, -Program.Height / 2, 0)) * Matrix4.CreateOrthographic(Program.Width, Program.Height, 0, 1000);

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
            // first slide is already built

            // introduction to presentation
            slideList.Add(new Slides.TitleAndBullets("About Me", new string[] { "Graduated 2010 in Electrical Engineering", "Working (since 2006) in the semiconductor industry", "Love hacking in C#, C and OpenGL", "GitHub: https://www.github.com/giawa", "Twitter: @giawadev" }));
            slideList.Add(new Slides.TwoImages("Exploration using Senses", "media/slide2_1.jpg", "media/slide2_2.jpg"));
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", "media/slide3.jpg", new string[] { "Microphone/ADC for Analog->Digital" }));
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", "media/slide4.jpg", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog" }));
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog", "How is audio stored in a computer?" }));
            slideList[slideList.Count - 1].CustomDraw = Slide5Render;
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog", "How is audio stored in a computer?", "As individual samples!" }));
            slideList[slideList.Count - 1].CustomDraw = Slide6Render;
            slideList.Add(new Slides.ImageAndText("Exploration using Senses", new string[] { "Microphone/ADC for Analog->Digital", "DAC/Speaker for Digital->Analog", "How is audio stored in a computer?", "As individual samples!", "How do we sample?" }));
            slideList[slideList.Count - 1].CustomDraw = Slide6Render;

            // nyquist sampling criteria
            slideList.Add(new Slides.TitleAndImage("Sampling Theorem"));
            slideList[slideList.Count - 1].CustomDraw = Slide8Render;
        }

        public static void SetSlide(int slide)
        {
            if ((slide == 5 && slideNumber == 6) || (slide == 7 && slideNumber == 6) || slide == 6) ;
            else t = 0;

            slideNumber = slide;

            if (slideList[slideNumber] != null) currentSlide = slideList[slideNumber];
            else currentSlide = null;
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

        private static void Slide8Render()
        {
            if (frequencyText == null) frequencyText = new Text(Text.FontSize._24pt, "0Hz @ 1000Hz", Slides.Common.TextColor);

            float f = t * 0.05f;
            frequencyText.String = string.Format("{0:0.0}Hz @ 1000Hz", f / Math.PI * 5000);

            float[] sine = new float[441];
            for (int i = 0; i < sine.Length; i++)
                sine[i] = 0.5f * (float)Math.Sin((i - 441 / 2) * f);

            Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
            Slides.Common.DrawPlotLeft(sine, Slides.Common.TitleColor);

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

            Slides.Common.DrawPlotLeft(samples, new Vector3(1, 0, 0));
            Slides.Common.DrawPlotLeft(filter, new Vector3(0, 1, 0));

            frequencyText.ModelMatrix = Matrix4.CreateTranslation(new Vector3(70, 500, 0));
            frequencyText.Draw();
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
