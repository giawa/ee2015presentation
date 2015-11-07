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
        public static Vector3 TextColor = new Vector3(0, 0, 0);

        private static VAO backgroundQuad;
        private static Texture backgroundTexture;

        public static BMFont Font24, Font32, Font48, Font54, Font72;

        public static void Init()
        {
            Font24 = BMFont.LoadFont("media/font24.fnt");
            Font32 = BMFont.LoadFont("media/font32.fnt");
            Font48 = BMFont.LoadFont("media/font48.fnt");
            Font54 = BMFont.LoadFont("media/font54.fnt");
            Font72 = BMFont.LoadFont("media/font72.fnt");

            backgroundQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader);
            backgroundTexture = new Texture("media/background.png");
        }

        public static void DrawBackground()
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(Matrix4.Identity);
            Gl.BindTexture(backgroundTexture);
            backgroundQuad.Draw();
        }

        public static void DrawString(BMFont font, VAO<Vector3, Vector2> text, Vector2 position, Vector3 color)
        {
            Shaders.FontShader.Use();
            Shaders.FontShader["position"].SetValue(position);
            Shaders.FontShader["color"].SetValue(color);
            Gl.BindTexture(font.FontTexture);
            text.Draw();
        }
    }
}
