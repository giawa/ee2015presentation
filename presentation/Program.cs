using System;

using OpenGL;

namespace Presentation
{
    class Program
    {
        public static int Width = 1280, Height = 720;

        public static bool Fullscreen = false;

        public static bool RunPresentation = true;

        public static BMFont Font24, Font32, Font48, Font54, Font72;

        private static VAO<Vector3, Vector2> title;
        private static VAO<Vector3, Vector2> subtitle;

        private static VAO backgroundQuad;
        private static Texture backgroundTexture;

        private static Vector3 titleColor = new Vector3(95 / 255f, 203 / 255f, 239 / 255f);
        private static Vector3 subtitleColor = new Vector3(0.5f, 0.5f, 0.5f);
        private static Vector3 textColor = new Vector3(0, 0, 0);

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

            Font24 = BMFont.LoadFont("media/font24.fnt");
            Font32 = BMFont.LoadFont("media/font32.fnt");
            Font48 = BMFont.LoadFont("media/font48.fnt");
            Font54 = BMFont.LoadFont("media/font54.fnt");
            Font72 = BMFont.LoadFont("media/font72.fnt");

            title = Font72.CreateString(Shaders.FontShader, "2015 Presentation", BMFont.Justification.Right);
            subtitle = Font32.CreateString(Shaders.FontShader, "Chatting about cool things in electrical engineering!", BMFont.Justification.Right);
            backgroundQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader);
            backgroundTexture = new Texture("media/background.png");

            Window.OnReshape(Program.Width, Program.Height);

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

            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(Matrix4.Identity);
            Gl.BindTexture(backgroundTexture);
            backgroundQuad.Draw();

            DrawString(Font72, title, new Vector2(980, 360), titleColor);
            DrawString(Font32, subtitle, new Vector2(980, 320), subtitleColor);

            // swap the buffers
            Window.SwapBuffers();
        }

        public static void DrawString(BMFont font, VAO<Vector3, Vector2> text, Vector2 position, Vector3 color)
        {
            Shaders.FontShader.Use();
            //Shaders.FontShader["uiProjectionMatrix"].SetValue(uiProjectionMatrix);
            Shaders.FontShader["position"].SetValue(position);
            Shaders.FontShader["color"].SetValue(color);
            Gl.BindTexture(font.FontTexture);
            text.Draw();
        }

        public static void NextSlide()
        {
            Console.WriteLine("Next slide!");
        }

        public static void PrevSlide()
        {
            Console.WriteLine("Prev slide!");
        }
    }
}
