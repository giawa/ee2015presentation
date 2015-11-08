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
            //slideList.Add(new Slides.TitleSlide("2015 Presentation", "Exporation of cool electrical engineering topics."));
            slideList.Add(new Slides.TitleAndBullets("Sample Bullet Point", new string[] { "Point 1", "More information about something.", "And some more stuff!" }));
            slideList.Add(new Slides.TitleAndImage("Semiconductor Image", "media/slide3.jpg"));
            slideList.Add(new Slides.ImageAndText("Bullets on Right", new string[] { "Bullet 1", "Bullet 2", "Bullet 3" }));
            slideList.Add(new Slides.ImageAndText("Image and Bullets", "media/slide5.jpg", new string[] { "Bullet 1", "Bullet 2", "Bullet 3" }));

            /*slideList[3].CustomDraw = () =>
                {
                    Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
                    Slides.Common.DrawSineLeft((float)Math.Pow(1.1f, t * 0.5 + 1) - 1.1f);
                };*/
            slideList[3].CustomDraw = () =>
                {
                    Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
                    //Slides.Common.DrawSineLeft(0.1f * t);

                    for (int i = 0; i < 882; i++)
                        sine[i] = 100 * Math.Sin(i * 0.1f * t);// +50 * Math.Sin(i * 0.4f);//new Vector3(i - 441 / 2f, 100 * Math.Sin(i * 0.1f + t * 10), 0);

                    double[] fft = FilterTools.fftw.FFTW(FilterTools.WindowFunction.ApplyWindowFunction(sine, sine.Length, FilterTools.WindowFunction.WindowType.BlackmanHarris));
                    double max = 0;
                    for (int i = 0; i < fft.Length; i++)
                        if (fft[i] > max) max = fft[i];
                    for (int i = 0; i < fft.Length; i++)
                        fft[i] /= max;

                    for (int i = 0; i < 441; i++)
                    {
                        sineWaveData[i] = new Vector3(i - 441 / 2f, Math.Max(-200, 30 * Math.Log10(fft[i])), 0);
                    }

                    if (sineWave == null)
                    {
                        int[] array = new int[441];
                        for (int i = 0; i < array.Length; i++) array[i] = i;

                        sineWaveHandle = GCHandle.Alloc(sineWaveData, GCHandleType.Pinned);
                        sineWaveVBO = Slides.Common.BufferData(sineWaveVBO, sineWaveData, sineWaveHandle);
                        sineWave = new VAO<Vector3>(Shaders.SimpleColoredShader, sineWaveVBO, "in_position", new VBO<int>(array, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw));
                        sineWave.DrawMode = BeginMode.LineStrip;
                    }
                    else sineWaveVBO = Slides.Common.BufferData(sineWaveVBO, sineWaveData, sineWaveHandle);

                    Shaders.SimpleColoredShader.Use();
                    Shaders.SimpleColoredShader["projectionMatrix"].SetValue(uiProjectionMatrix);
                    Shaders.SimpleColoredShader["viewMatrix"].SetValue(Matrix4.Identity);
                    Shaders.SimpleColoredShader["modelMatrix"].SetValue(Matrix4.CreateTranslation(new Vector3(72 + 441 / 2f, 288, 0)));
                    Shaders.SimpleColoredShader["color"].SetValue(new Vector3(1, 0, 0));//Slides.Common.TitleColor);
                    sineWave.Draw();
                };
        }

        private static double[] sine = new double[882];

        private static VAO<Vector3> sineWave;
        private static VBO<Vector3> sineWaveVBO;
        private static Vector3[] sineWaveData = new Vector3[441];
        private static GCHandle sineWaveHandle;

        public static void SetSlide(int slide)
        {
            slideNumber = slide;
            t = 0;

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
    }
}
