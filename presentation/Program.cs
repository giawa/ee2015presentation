using System;
using System.Collections.Generic;

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
            Gl.Enable(EnableCap.Multisample);

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

        public static Matrix4 uiProjectionMatrix = Matrix4.Identity;

        public static void OnReshape()
        {
            uiProjectionMatrix = Matrix4.CreateTranslation(new Vector3(-Program.Width / 2, -Program.Height / 2, 0)) * Matrix4.CreateOrthographic(Program.Width, Program.Height, 0, 1000);

            // the uiProjectMatrix need only be set once (unless we reshape)
            Shaders.FontShader.Use();
            Shaders.FontShader["projectionMatrix"].SetValue(uiProjectionMatrix);

            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["projectionMatrix"].SetValue(uiProjectionMatrix);
        }

        private static float dt = 0;

        private static void OnRenderFrame()
        {
            dt = watch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            // clear the screen
            Gl.Viewport(0, 0, Program.Width, Program.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render the current slide if it exists
            if (currentSlide != null) currentSlide.Draw();
            RenderSlide();

            // swap the buffers
            Window.SwapBuffers();
        }

        private static int slideNumber = 0;
        private static Slides.ISlide currentSlide;

        private static Slides.ISlide[] slideList = new Slides.ISlide[256];

        public static void SetSlide(int slide)
        {
            slideNumber = slide;
            t = 0;

            if (slideList[slideNumber] != null)
            {
                currentSlide = slideList[slideNumber];
                return;
            }

            if (slideNumber == 0)
            {
                slideList[slideNumber] = new Slides.TitleSlide("2015 Presentation", "Exporation of cool electrical engineering topics.");
            }
            else if (slideNumber == 1)
            {
                slideList[slideNumber] = new Slides.TitleAndBullets("Sample Bullet Point", new string[] { "Point 1", "More information about something.", "And some more stuff!" });
            }
            else if (slideNumber == 2)
            {
                slideList[slideNumber] = new Slides.TitleAndImage("Semiconductor Image", "media/slide3.jpg");
            }
            else if (slideNumber == 3)
            {
                slideList[slideNumber] = new Slides.ImageAndText("Bullets on Right", new string[] { "Bullet 1", "Bullet 2", "Bullet 3" });
            }
            else if (slideNumber == 4)
            {
                slideList[slideNumber] = new Slides.ImageAndText("Image and Bullets", "media/slide5.jpg", new string[] { "Bullet 1", "Bullet 2", "Bullet 3" });
            }

            currentSlide = slideList[slideNumber];
        }

        private static VAO<Vector3> sineWave;
        private static float t = 0;
        private static System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

        public static void RenderSlide()
        {
            t += dt;

            if (slideNumber == 3)
            {
                Slides.Common.DrawPlotter(Utilities.FastMatrix4(new Vector3(72, 720 - 227 - 410, 0), new Vector3(441, 410, 1)));

                if (sineWave == null)
                {
                    Vector3[] sineArray = new Vector3[441];
                    int[] elementArray = new int[sineArray.Length];
                    for (int i = 0; i < sineArray.Length; i++)
                    {
                        sineArray[i] = new Vector3(i - 441 / 2f, 288, 0);
                        elementArray[i] = i;
                    }
                    VBO<Vector3> sineListVBO = new VBO<Vector3>(sineArray);
                    VBO<int> elementListVBO = new VBO<int>(elementArray, BufferTarget.ElementArrayBuffer);
                    sineWave = new VAO<Vector3>(Shaders.SineShader, sineListVBO, "in_position", elementListVBO);
                    sineWave.DrawMode = BeginMode.LineStrip;
                }

                Shaders.SineShader.Use();
                Shaders.SineShader["projectionMatrix"].SetValue(uiProjectionMatrix);
                Shaders.SineShader["viewMatrix"].SetValue(Matrix4.Identity);
                Shaders.SineShader["modelMatrix"].SetValue(Matrix4.CreateTranslation(new Vector3(72 + 441 / 2f, 0, 0)));
                Shaders.SineShader["color"].SetValue(Slides.Common.TitleColor);
                Shaders.SineShader["f"].SetValue((float)Math.Pow(1.1f, t * 0.5 + 1) - 1.1f);
                Shaders.SineShader["a"].SetValue(5f);
                Gl.LineWidth(2f);
                sineWave.Draw();
            }
        }

        public static void NextSlide()
        {
            SetSlide(Math.Min(slideList.Length - 1, slideNumber + 1));
        }

        public static void PrevSlide()
        {
            SetSlide(Math.Max(0, slideNumber - 1));
        }
    }
}
