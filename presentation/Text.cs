using System;

using OpenGL;

namespace Presentation
{
    public class Text : IDisposable
    {
        #region Built-In Font Sizes
        public enum FontSize
        {
            _24pt = 0,
            _32pt = 1,
            _48pt = 2,
            _54pt = 3,
            _72pt = 4
        }

        public static BMFont FontFromSize(FontSize font)
        {
            switch (font)
            {
                case FontSize._24pt: return BMFont.LoadFont("media/font24.fnt");
                case FontSize._32pt: return BMFont.LoadFont("media/font32.fnt");
                case FontSize._48pt: return BMFont.LoadFont("media/font48.fnt");
                case FontSize._54pt: return BMFont.LoadFont("media/font54.fnt");
                case FontSize._72pt: return BMFont.LoadFont("media/font72.fnt");
                default: 
                    Console.WriteLine("Unknown font " + font + " requested.");
                    return BMFont.LoadFont("fonts/font12.fnt");
            }
        }
        #endregion

        #region Public Properties
        public Matrix4 ModelMatrix { get; set; }

        public Vector3 Color { get; set; }
        #endregion

        #region Private Fields
        private BMFont font;

        private VAO<Vector3, Vector2> VAO;
        #endregion

        #region Constructor
        public Text(FontSize size, string text, Vector3 color, BMFont.Justification justification = BMFont.Justification.Left)
        {
            this.font = FontFromSize(size);
            this.VAO = font.CreateString(Shaders.FontShader, text, justification);
            this.Color = color;
        }
        #endregion

        #region Public Methods
        public void Draw()
        {
            Shaders.FontShader.Use();
            Shaders.FontShader["modelMatrix"].SetValue(ModelMatrix);
            Shaders.FontShader["color"].SetValue(Color);
            Gl.BindTexture(font.FontTexture);
            VAO.Draw();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (VAO != null)
            {
                VAO.DisposeChildren = true;
                VAO.Dispose();
                VAO = null;
            }
        }
        #endregion
    }
}
