using System;

using OpenGL;

namespace Presentation.Slides
{
    public class TitleAndImage : Slide
    {
        private Text title;
        private Texture image;
        private VAO imageVAO;
        private Matrix4 imageModelMatrix;

        public TitleAndImage(string titleText)
        {
            title = new Text(Text.FontSize._72pt, titleText, Common.TitleColor);
            title.ModelMatrix = Matrix4.CreateTranslation(new Vector3(80, 720 - 120, 0));
        }

        public TitleAndImage(string titleText, string media)
            : this(titleText)
        {
            image = new Texture(media);
            imageVAO = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(904, 410));
            imageModelMatrix = Matrix4.CreateTranslation(new Vector3(72, 720 - 227 - 410, 0));
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

            base.Draw();
        }
    }
}
