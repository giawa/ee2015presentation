using System;
using System.Collections.Generic;

using OpenGL;

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

            slideList[3].CustomDraw = () =>
                {
                    Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));
                    Slides.Common.DrawSineLeft((float)Math.Pow(1.1f, t * 0.5 + 1) - 1.1f);
                };
        }

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
