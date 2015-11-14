using System;

using OpenGL;
using System.Runtime.InteropServices;

namespace Presentation.Slides
{
    public abstract class Slide
    {
        public Action CustomDraw { get; set; }

        public bool ResetTime { get; set; }

        public Slide()
        {
            ResetTime = true;
        }

        public virtual void Draw()
        {
            if (CustomDraw != null) CustomDraw();
        }
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

        public static VAO BoxQuad;
        public static VAO CrosshairVAO;
        public static VAO StencilQuad;

        public static void Init()
        {
            BackgroundQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader);
            BackgroundTexture = new Texture("media/background.png");
            BulletTexture = new Texture("media/bullet.png");
            BulletQuad = Utilities.CreateQuad(Shaders.SimpleTexturedShader, Vector2.Zero, new Vector2(24, 24));

            VBO<Vector3> vertices = new VBO<Vector3>(new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) });
            VBO<int> indices = new VBO<int>(new int[] { 0, 1, 2, 3, 0 }, BufferTarget.ElementArrayBuffer);
            BoxQuad = new VAO(Shaders.SimpleColoredShader, vertices, indices);
            BoxQuad.DrawMode = BeginMode.LineStrip;

            indices = new VBO<int>(new int[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);
            StencilQuad = new VAO(Shaders.SimpleColoredShader, vertices, indices);
            StencilQuad.DrawMode = BeginMode.Quads;

            vertices = new VBO<Vector3>(new Vector3[] { new Vector3(0.5, -0.01, 0), new Vector3(0.5, 1.01, 0), new Vector3(-0.01, 0.5, 0), new Vector3(1.01, 0.5, 0) });
            indices = new VBO<int>(new int[] { 2, 3, 0, 1 }, BufferTarget.ElementArrayBuffer);
            CrosshairVAO = new VAO(Shaders.SimpleColoredShader, vertices, indices);
            CrosshairVAO.DrawMode = BeginMode.Lines;

            Shaders.FontShader.Use();
            Shaders.FontShader["viewMatrix"].SetValue(Matrix4.Identity);

            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["viewMatrix"].SetValue(Matrix4.Identity);
        }

        public static void DrawBackground()
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(Matrix4.Identity);
            Gl.BindTexture(BackgroundTexture);
            BackgroundQuad.Draw();
            Shaders.SimpleTexturedShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
        }

        public static void DrawBullet(Matrix4 modelMatrix)
        {
            Shaders.SimpleTexturedShader.Use();
            Shaders.SimpleTexturedShader["modelMatrix"].SetValue(modelMatrix);
            Gl.BindTexture(BulletTexture);
            BulletQuad.Draw();
        }

        public static void DrawBox(Matrix4 modelMatrix, Vector3 color)
        {
            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["modelMatrix"].SetValue(modelMatrix);
            Shaders.SimpleColoredShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SimpleColoredShader["color"].SetValue(color);
            BoxQuad.Draw();
        }

        public static void DrawPlotter(Matrix4 modelMatrix)
        {
            DrawBox(modelMatrix, SubtitleColor);
            Shaders.SimpleColoredShader["color"].SetValue(TextColor);
            CrosshairVAO.Draw();
        }

        public static void DrawXPlotter(Matrix4 modelMatrix)
        {
            DrawBox(modelMatrix, SubtitleColor);
            Shaders.SimpleColoredShader["color"].SetValue(TextColor);

            CrosshairVAO.VertexCount = 2;
            CrosshairVAO.Draw();
            CrosshairVAO.VertexCount = 4;
        }

        public static void EnableStencil(Matrix4 modelMatrix)
        {
            // draw an outline around the chunk
            Gl.ClearStencil(0);
            Gl.Clear(ClearBufferMask.StencilBufferBit);
            Gl.Enable(EnableCap.StencilTest);

            Gl.StencilFunc(StencilFunction.Always, 1, 0xFFFF);
            Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            Gl.ColorMask(false, false, false, false);

            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["modelMatrix"].SetValue(modelMatrix);
            Shaders.SimpleColoredShader["viewMatrix"].SetValue(Matrix4.Identity);
            StencilQuad.Draw();

            Gl.ColorMask(true, true, true, true);
        }

        private static VAO<Vector3> sineLeft;

        public static void DrawSineLeft(Vector3 color, float f, float a = 100f, float t = 0f)
        {
            if (sineLeft == null)
            {
                Vector3[] sineArray = new Vector3[441];
                int[] elementArray = new int[sineArray.Length];
                for (int i = 0; i < sineArray.Length; i++)
                {
                    sineArray[i] = new Vector3(i - 441 / 2f, 288, 0);
                    elementArray[i] = i;
                }
                VBO<Vector3> sineListVBO = new VBO<Vector3>(sineArray);
                VBO<int> elementListVBO = new VBO<int>(elementArray, BufferTarget.ElementArrayBuffer);
                sineLeft = new VAO<Vector3>(Shaders.SineShader, sineListVBO, "in_position", elementListVBO);
                sineLeft.DrawMode = BeginMode.LineStrip;
            }

            Shaders.SineShader.Use();
            Shaders.SineShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
            Shaders.SineShader["viewMatrix"].SetValue(Matrix4.Identity);
            Shaders.SineShader["modelMatrix"].SetValue(Matrix4.CreateTranslation(new Vector3(72 + 441 / 2f, 0, 0)));
            Shaders.SineShader["color"].SetValue(color);
            Shaders.SineShader["timeAmplitudeFrequency"].SetValue(new Vector3(t, a, f));
            Gl.LineWidth(2f);
            sineLeft.Draw();
        }

        private static float[] fft = new float[441];

        public static void DrawFFTLeft(float[] data)
        {
            if (data.Length != 882) throw new ArgumentException("The argument data was not the correct length.");

            double[] fftw = FilterTools.fftw.FFTW(FilterTools.WindowFunction.ApplyWindowFunction(data, data.Length, FilterTools.WindowFunction.WindowType.BlackmanHarris));
            
            // normalize the FFT to 1
            double max = 20;
            /*for (int i = 0; i < fftw.Length; i++)
                if (fftw[i] > max) max = fftw[i];*/

            for (int i = 0; i < fft.Length; i++)
                fft[i] = (float)(fftw[i] / max);

            DrawPlotLeft(fft, new Vector3(1, 0, 0), true);
        }

        private static VAO<Vector3> fftVAO;
        private static VBO<Vector3> fftVBO;
        private static Vector3[] fftData = new Vector3[441];
        private static GCHandle fftHandle;

        public static void DrawPlotLeft(float[] data, Vector3 color, bool log = false)
        {
            Draw3DPlotLeft(data, 0, color, Matrix4.Identity, log);
        }

        public static void Draw3DPlotLeft(float[] data, float depth, Vector3 color, Matrix4 viewMatrix, bool log = false)
        {
            if (data.Length < 441) throw new ArgumentException("The argument data was not the correct length.");

            for (int i = 0; i < fftData.Length; i++)
                fftData[i] = new Vector3((log ? Math.Log10(i) * 166 : i) - 441 / 2f, Math.Max(-200, Math.Min(200, 200 * data[i])), depth);
                //fftData[i] = new Vector3(i - 441 / 2f, Math.Max(-200, Math.Min(200, 200 * data[i])), depth);

            if (fftVAO == null)
            {
                int[] array = new int[441];
                for (int i = 0; i < array.Length; i++) array[i] = i;

                fftHandle = GCHandle.Alloc(fftData, GCHandleType.Pinned);
                fftVBO = BufferData(fftVBO, fftData, fftHandle);
                fftVAO = new VAO<Vector3>(Shaders.SimpleColoredShader, fftVBO, "in_position", new VBO<int>(array, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw));
                fftVAO.DrawMode = BeginMode.LineStrip;
            }
            else fftVBO = BufferData(fftVBO, fftData, fftHandle);

            Shaders.SimpleColoredShader.Use();
            Shaders.SimpleColoredShader["projectionMatrix"].SetValue(Program.uiProjectionMatrix);
            Shaders.SimpleColoredShader["viewMatrix"].SetValue(viewMatrix);
            Shaders.SimpleColoredShader["modelMatrix"].SetValue(Matrix4.CreateTranslation(new Vector3(72 + 441 / 2f, 288, 0)));
            Shaders.SimpleColoredShader["color"].SetValue(color);
            fftVAO.Draw();
        }

        public static VBO<Vector3> BufferData(VBO<Vector3> vbo, Vector3[] data, GCHandle handle)
        {
            if (vbo == null) return new VBO<Vector3>(data, BufferTarget.ArrayBuffer, BufferUsageHint.StaticDraw);

            vbo.BufferSubDataPinned(BufferTarget.ArrayBuffer, 12 * data.Length, handle.AddrOfPinnedObject());
            return vbo;
        }
    }
}
