using System;

using OpenGL;

namespace Presentation
{
    class Program
    {
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

            // set up the blending mode for ground clutter
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // initialize the shaders that we require
            Shaders.InitShaders(Shaders.ShaderVersion.GLSL120);

            // initialize commonly used object in slides such as fonts, background textures, etc
            Slides.Common.Init();

            // force a reshape to generate the UI projection matrix
            Window.OnReshape(Program.Width, Program.Height);

            // set to slide 0
            SetSlide(0);

            // the main game loop
            while (RunPresentation)
            {
                OnRenderFrame();
                Window.HandleEvents();
            }
        }

        public static void OnReshape()
        {
            Matrix4 uiProjectionMatrix = Matrix4.CreateTranslation(new Vector3(-Program.Width / 2, -Program.Height / 2, 0)) * Matrix4.CreateOrthographic(Program.Width, Program.Height, 0, 1000);

            // the uiProjectMatrix need only be set once (unless we reshape)
            Shaders.FontShader.Use();
            Shaders.FontShader["uiProjectionMatrix"].SetValue(uiProjectionMatrix);
        }

        private static void OnRenderFrame()
        {
            // clear the screen
            Gl.Viewport(0, 0, Program.Width, Program.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render the current slide if it exists
            if (currentSlide != null) currentSlide.Draw();

            // swap the buffers
            Window.SwapBuffers();
        }

        private static int slideNumber = 0;
        private static Slides.ISlide currentSlide;

        public static void SetSlide(int slide)
        {
            if (slideNumber == 0)
            {
                currentSlide = new Slides.TitleSlide("2015 Presentation", "Exporation of cool electrical engineering topics.");
            }
            else if (slideNumber == 1)
            {
                currentSlide = new Slides.TitleSlide("Slide 2", "Testing navigation.");
            }
        }

        public static void NextSlide()
        {
            slideNumber++;
            SetSlide(slideNumber);
        }

        public static void PrevSlide()
        {
            slideNumber = Math.Max(0, slideNumber - 1);
            SetSlide(slideNumber);
        }
    }
}
