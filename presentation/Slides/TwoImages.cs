using System;

using OpenGL;

namespace Presentation.Slides
{
    public class TwoImages : Slide
    {
        private Text title;
        private Texture image1, image2;
        private VAO image1VAO, image2VAO;
        private Matrix4 image1ModelMatrix, image2ModelMatrix;

        public TwoImages(string titleText, string media1, string media2)
        {
            title = new Text(Text.FontSize._72pt, titleText, Common.TitleColor);
            title.ModelMatrix = Matrix4.CreateTranslation(new Vector3(80, 720 - 120, 0));

            image1 = new Texture(media1);
            image1VAO = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(441, 410));
            image1ModelMatrix = Matrix4.CreateTranslation(new Vector3(72, 720 - 227 - 410, 0));

            image2 = new Texture(media2);
            image2VAO = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(441, 410));
            image2ModelMatrix = Matrix4.CreateTranslation(new Vector3(535, 720 - 227 - 410, 0));
        }

        public override void Draw()
        {
            Common.DrawBackground();

            title.Draw();

            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);

            if (image1 != null)
            {
                Shaders.SimpleTexturedShader["modelMatrix"].SetValue(image1ModelMatrix);
                Gl.BindTexture(image1);
                image1VAO.Draw();
            }

            if (image2 != null)
            {
                Shaders.SimpleTexturedShader["modelMatrix"].SetValue(image2ModelMatrix);
                Gl.BindTexture(image2);
                image2VAO.Draw();
            }

            base.Draw();
        }
    }
}
