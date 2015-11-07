using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace Presentation
{
    public static class Shaders
    {
        public static ShaderProgram SimpleTexturedShader;
        public static ShaderProgram SimpleColoredShader;
        public static ShaderProgram FontShader;
        public static ShaderProgram Font3DShader;
        public static ShaderProgram SineShader;

        public enum ShaderVersion
        {
            GLSL120,
            GLSL140
        }

        public static ShaderVersion Version = ShaderVersion.GLSL140;

        public static void InitShaders(ShaderVersion version = ShaderVersion.GLSL140)
        {
            Version = version;

            try
            {
                SimpleTexturedShader = InitShader(SimpleTexturedVertexShader, SimpleTexturedFragmentShader);
                SimpleColoredShader = InitShader(SimpleColoredVertexShader, SimpleColoredFragmentShader);
                SineShader = InitShader(SineVertexShader, SimpleColoredFragmentShader);

                FontShader = InitShader(FontVertexSource, FontFragmentSource);
                Font3DShader = InitShader(Font3DVertexSource, FontFragmentSource);
            }
            catch (Exception e)
            {
                Console.WriteLine("Your GPU does not support programmable shaders.");
            }
        }

        public static ShaderProgram InitShader(string vertexSource, string fragmentSource)
        {
            if (Version == ShaderVersion.GLSL120)
            {
                vertexSource = ConvertShader(vertexSource, true);
                fragmentSource = ConvertShader(fragmentSource, false);
            }

            ShaderProgram program = new ShaderProgram(vertexSource, fragmentSource);

            return program;
        }

        #region Conversion from GLSL 1.4 to 1.2
        private static char[] newlineChar = new char[] { '\n' };
        private static char[] unixNewlineChar = new char[] { '\r' };

        public static string ConvertShader(string shader, bool vertexShader)
        {
            // there are a few rules to convert a shader from 140 to 120
            // the first is to remove the keywords 'in' and 'out' and replace with 'attribute'
            // the next is to remove camera uniform blocks
            StringBuilder sb = new StringBuilder();

            string[] lines = shader.Split(newlineChar);

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim(unixNewlineChar);
                if (lines[i].StartsWith("uniform Camera"))
                {
                    i += 3;

                    sb.AppendLine("uniform mat4 projectionMatrix;");
                    sb.AppendLine("uniform mat4 viewMatrix;");
                }
                else if (lines[i].StartsWith("#version 140")) sb.AppendLine("#version 130");
                else if (lines[i].StartsWith("in ")) sb.AppendLine((vertexShader ? "attribute " : "varying ") + lines[i].Substring(3));
                else if (lines[i].StartsWith("out ") && vertexShader) sb.AppendLine("varying " + lines[i].Substring(4));
                else sb.AppendLine(lines[i]);
            }

            return sb.ToString();
        }
        #endregion

        #region Shader Source
        private static string SimpleTexturedVertexShader = @"
#version 140

uniform Camera {
   mat4 projectionMatrix;
   mat4 viewMatrix;
};
uniform mat4 modelMatrix;

in vec3 in_position;
in vec2 in_uv;

out vec2 uv;

void main(void)
{
   uv = in_uv;
   gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_position, 1);
}
";

        private static string SimpleTexturedFragmentShader = @"
#version 140

in vec2 uv;

uniform sampler2D active_texture;

void main(void)
{
  gl_FragColor = texture(active_texture, uv);
}";

        private static string SimpleColoredVertexShader = @"
#version 140

uniform Camera {
   mat4 projectionMatrix;
   mat4 viewMatrix;
};
uniform mat4 modelMatrix;

in vec3 in_position;

void main(void)
{
   gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_position, 1);
}
";

        private static string SimpleColoredFragmentShader = @"
#version 140

uniform vec3 color;

void main(void)
{
  gl_FragColor = vec4(color, 1);
}
";

        private static string Font3DVertexSource = @"
#version 140

uniform Camera {
   mat4 projectionMatrix;
   mat4 viewMatrix;
};
uniform mat4 modelMatrix;

in vec3 in_position;
in vec2 in_uv;

out vec2 uv;

void main(void)
{
  uv = in_uv;
  gl_Position = projectionMatrix * (viewMatrix * modelMatrix * vec4(0, 0, 0, 1) + vec4(in_position.x, in_position.y, 0, 0));
}";

        private static string FontVertexSource = @"
#version 140

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

in vec3 in_position;
in vec2 in_uv;

out vec2 uv;

void main(void)
{
  uv = in_uv;
  gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_position.x, in_position.y, 0, 1);
}";

        private static string FontFragmentSource = @"
#version 140

uniform sampler2D active_texture;

in vec2 uv;
uniform vec3 color;

void main(void)
{
  vec4 t = texture2D(active_texture, uv);
  gl_FragColor = vec4(color, t.r);
}";

        private static string SineVertexShader = @"
#version 140

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

attribute vec3 in_position;

uniform float f;
uniform float t;
uniform float a;

void main(void)
{
   vec3 position = vec3(in_position.x, 20 * a * sin(in_position.x * f + t) + in_position.y, 0);
   gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position, 1);
}";
        #endregion
    }
}
