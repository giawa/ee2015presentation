using System;

using OpenGL;

namespace Presentation.Slides
{
    class TitleAndBullets : Slide
    {
        private Text title;
        private Text[] bullets;

        public TitleAndBullets(string titleText, string[] bulletText)
        {
            title = new Text(Text.FontSize._72pt, titleText, Common.TitleColor);

            bullets = new Text[bulletText.Length];
            for (int i = 0; i < bulletText.Length; i++)
            {
                bullets[i] = new Text(Text.FontSize._32pt, bulletText[i], Common.TextColor);
                bullets[i].ModelMatrix = Matrix4.CreateTranslation(new Vector3(115, 720 - 255 - i * 60, 0));
            }

            title.ModelMatrix = Matrix4.CreateTranslation(new Vector3(80, 720 - 120, 0));
        }

        public override void Draw()
        {
            Common.DrawBackground();

            title.Draw();
            for (int i = 0; i < bullets.Length; i++)
            {
                Common.DrawBullet(Matrix4.CreateTranslation(new Vector3(85, 720 - 260 - i * 60, 0)));
                bullets[i].Draw();
            }

            base.Draw();
        }
    }
}
