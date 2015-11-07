using System;

using OpenGL;

namespace Presentation.Slides
{
    public class TitleSlide : ISlide
    {
        private Text title;
        private Text subtitle;

        public TitleSlide(string titleText, string subtitleText)
        {
            title = new Text(Text.FontSize._72pt, titleText, Common.TitleColor, BMFont.Justification.Right);
            subtitle = new Text(Text.FontSize._32pt, subtitleText, Common.SubtitleColor, BMFont.Justification.Right);

            title.ModelMatrix = Matrix4.CreateTranslation(new Vector3(980, 360, 0));
            subtitle.ModelMatrix = Matrix4.CreateTranslation(new Vector3(980, 320, 0));
        }

        public void Draw()
        {
            Common.DrawBackground();

            title.Draw();
            subtitle.Draw();
        }
    }
}
