using System;

using OpenGL;

namespace Presentation.Slides
{
    public interface ISlide
    {
        void Draw();
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

        public static void Init()
        {
            BackgroundQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader);
            BackgroundTexture = new Texture("media/background.png");
            BulletTexture = new Texture("media/bullet.png");
            BulletQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(24, 24));

            Shaders.FontShader.Use();
            Shaders.FontShader["viewMatrix"].SetValue(Matrix4.Identity);
        }

        public static void DrawBackground()
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(Matrix4.Identity);
            Gl.BindTexture(BackgroundTexture);
            BackgroundQuad.Draw();
        }

        public static void DrawBullet(Matrix4 modelMatrix)
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(modelMatrix);
            Gl.BindTexture(BulletTexture);
            BulletQuad.Draw();
        }
    }
}
