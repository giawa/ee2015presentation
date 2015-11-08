using System;

using OpenGL;

namespace Presentation.Slides
{
    public abstract class Slide
    {
        public Action CustomDraw { get; set; }

        public virtual void Draw()
        {
            if (CustomDraw != null) CustomDraw();
        }
    }

    public static class Common
    {
        public static Vector3 TitleColor = new Vector3(95 / 255f, 203 / 255f, 239 / 255f);
        public static Vector3 SubtitleColor = new Vector3(0.5f, 0.5f, 0.5f);
        public static Vector3 TextColor = new Vector3(0.25f, 0.25f, 0.25f);

        public static VAO BackgroundQuad;
        public static Texture BackgroundTexture;
        public static Texture BulletTexture;
        public static VAO BulletQuad;

        public static VAO BoxQuad;
        public static VAO CrosshairVAO;

        public static void Init()
        {
            BackgroundQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader);
            BackgroundTexture = new Texture("media/background.png");
            BulletTexture = new Texture("media/bullet.png");
            BulletQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(24, 24));

            VBO<Vector3> vertices = new VBO<Vector3>(new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) });
            VBO<int> indices = new VBO<int>(new int[] { 0, 1, 2, 3, 0 }, BufferTarget.ElementArrayBuffer);
            BoxQuad = new VAO(Shaders.SimpleColoredShader, vertices, indices);
            BoxQuad.DrawMode = BeginMode.LineStrip;

            vertices = new VBO<Vector3>(new Vector3[] { new Vector3(0.5, -0.01, 0), new Vector3(0.5, 1.01, 0), new Vector3(-0.01, 0.5, 0), new Vector3(1.01, 0.5, 0) });
            indices = new VBO<int>(new int[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            CrosshairVAO = new VAO(Shaders.SimpleColoredShader, vertices, indices);
            CrosshairVAO.DrawMode = BeginMode.Lines;

            Shaders.FontShader.Use();
            Shaders.FontShader["viewMatrix"].SetValue(Matrix4.Identity);

            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["viewMatrix"].SetValue(Matrix4.Identity);
        }

        public static void DrawBackground()
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(Matrix4.Identity);
            Gl.BindTexture(BackgroundTexture);
            BackgroundQuad.Draw();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
        }

        public static void DrawBullet(Matrix4 modelMatrix)
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(modelMatrix);
            Gl.BindTexture(BulletTexture);
            BulletQuad.Draw();
        }

        public static void DrawBox(Matrix4 modelMatrix, Vector3 color)
        {
            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["modelMatrix"].SetValue(modelMatrix);
            Shaders.SimpleColoredShader["color"].SetValue(color);
            BoxQuad.Draw();
        }

        public static void DrawPlotter(Matrix4 modelMatrix)
        {
            DrawBox(modelMatrix, SubtitleColor);
            Shaders.SimpleColoredShader["color"].SetValue(TextColor);
            CrosshairVAO.Draw();
        }

        private static VAO<Vector3> sineLeft;

        public static void DrawSineLeft(float f, float a = 100f, float t = 0f)
        {
            if (sineLeft == null)
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
                sineLeft = new VAO<Vector3>(Shaders.SineShader, sineListVBO, "in_position", elementListVBO);
                sineLeft.DrawMode = BeginMode.LineStrip;
            }

            Shaders.SineShader.Use();
            Shaders.SineShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
            Shaders.SineShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SineShader["modelMatrix"].SetValue(Matrix4.CreateTranslation(new Vector3(72 + 441 / 2f, 0, 0)));
            Shaders.SineShader["color"].SetValue(Slides.Common.TitleColor);
            Shaders.SineShader["timeAmplitudeFrequency"].SetValue(new Vector3(t, a, f));
            Gl.LineWidth(2f);
            sineLeft.Draw();
        }
    }
}
