using System;

using OpenGL;

namespace Presentation.Slides
{
    public class ImageWithCaption : Slide
    {
        private Text title;
        private Text caption;
        private Texture image;
        private VAO imageVAO;
        private Matrix4 imageModelMatrix;

        public ImageWithCaption(string titleText, string captionText)
        {
            title = new Text(Text.FontSize._54pt, titleText, Common.TitleColor);
            caption = new Text(Text.FontSize._24pt, captionText, Common.SubtitleColor);
            title.ModelMatrix = Matrix4.CreateTranslation(new Vector3(80, 720 - 550, 0));
            caption.ModelMatrix = Matrix4.CreateTranslation(new Vector3(80, 720 - 590, 0));
        }

        public ImageWithCaption(string titleText, string captionText, string media)
            : this(titleText, captionText)
        {
            image = new Texture(media);
            imageVAO = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(904, 410));
            imageModelMatrix = Matrix4.CreateTranslation(new Vector3(72, 720 - 65 - 410, 0));
        }

        public override void Draw()
        {
            Common.DrawBackground();

            title.Draw();
            caption.Draw();

            if (image != null)
            {
                Shaders.SimpleTexturedShader.Use();
                Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
                Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
                Shaders.SimpleTexturedShader["modelMatrix"].SetValue(imageModelMatrix);
                Gl.BindTexture(image);
                imageVAO.Draw();
            }

            base.Draw();
        }
    }
}
