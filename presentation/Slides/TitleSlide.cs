using System;

using OpenGL;

namespace Presentation.Slides
{
    public class TitleSlide : ISlide
    {
        private VAO<Vector3, Vector2> title;
        private VAO<Vector3, Vector2> subtitle;

        public TitleSlide(string titleText, string subtitleText)
        {
            title = Common.Font72.CreateString(Shaders.FontShader, titleText, BMFont.Justification.Right);
            subtitle = Common.Font32.CreateString(Shaders.FontShader, subtitleText, BMFont.Justification.Right);
        }

        public void Draw()
        {
            Common.DrawBackground();

            Common.DrawString(Common.Font72, title, new Vector2(980, 360), Common.TitleColor);
            Common.DrawString(Common.Font32, subtitle, new Vector2(980, 320), Common.SubtitleColor);
        }
    }
}
