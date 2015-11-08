using System;

using OpenGL;

namespace Presentation.Slides
{
    public class ImageAndText : Slide
    {
        private Text title;
        private Texture image;
        private VAO imageVAO;
        private Matrix4 imageModelMatrix;
        private Text[] bullets;

        public ImageAndText(string titleText, string media, string[] bulletText)
            : this(titleText, bulletText)
        {
            image = new Texture(media);
            imageVAO = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(441, 410));
            imageModelMatrix = Matrix4.CreateTranslation(new Vector3(72, 720 - 227 - 410, 0));
        }

        public ImageAndText(string titleText, string[] bulletText)
        {
            title = new Text(Text.FontSize._72pt, titleText, Common.TitleColor);
            title.ModelMatrix = Matrix4.CreateTranslation(new Vector3(80, 720 - 120, 0));

            bullets = new Text[bulletText.Length];
            for (int i = 0; i < bulletText.Length; i++)
            {
                bullets[i] = new Text(Text.FontSize._32pt, bulletText[i], Common.TextColor);
                bullets[i].ModelMatrix = Matrix4.CreateTranslation(new Vector3(580, 720 - 255 - i * 60, 0));
            }
        }

        public override void Draw()
        {
            Common.DrawBackground();

            title.Draw();

            if (image != null)
            {
                Shaders.SimpleTexturedShader.Use();
                Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
                Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
                Shaders.SimpleTexturedShader["modelMatrix"].SetValue(imageModelMatrix);
                Gl.BindTexture(image);
                imageVAO.Draw();
            }

            for (int i = 0; i < bullets.Length; i++)
            {
                Common.DrawBullet(Matrix4.CreateTranslation(new Vector3(580 - 30, 720 - 260 - i * 60, 0)));
                bullets[i].Draw();
            }

            base.Draw();
        }
    }
}
