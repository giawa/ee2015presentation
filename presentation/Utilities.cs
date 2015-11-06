using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

using OpenGL;

namespace Presentation
{
    public static class Utilities
    {
        #region Fast Matrix Calculations
        /// <summary>
        /// Returns a translation and scaling matrix that centers the VoxelChunk (subtracts 16 * scale from x and z).
        /// </summary>
        /// <param name="position">The position of the centered VoxelChunk.</param>
        /// <param name="scale">The scaling to be applied to the centered VoxelChunk.</param>
        /// <returns>A matrix consisting of both translation and scaling.</returns>
        public static Matrix4 FastCenteredMatrix4(Vector3 position, float scale)
        {
            return Utilities.FastMatrix4(new Vector3(position.x - 16 * scale + 0.5, position.y, position.z - 16 * scale + 0.5), new Vector3(scale, scale, scale));
        }

        /// <summary>
        /// Returns a translation and scaling matrix quickly.
        /// </summary>
        /// <param name="position">The translation to apply to this matrix.</param>
        /// <param name="scale">The scaling to apply to this matrix.</param>
        /// <returns>A matrix consisting of both translation and scaling.</returns>
        public static Matrix4 FastMatrix4(Vector3 position, Vector3 scale)
        {
            return new Matrix4(new Vector4(scale.x, 0, 0, 0), new Vector4(0, scale.y, 0, 0), new Vector4(0, 0, scale.z, 0), new Vector4(position.x, position.y, position.z, 1));
        }

        /// <summary>
        /// Returns a combined translation, rotation and scaling matrix quickly.
        /// </summary>
        /// <param name="position">The translation to apply to this matrix.</param>
        /// <param name="scale">The scaling to apply to this matrix.</param>
        /// <param name="axis">The axis angle to apply during the rotation.</param>
        /// <param name="angle">The angle (in radians) of the rotation.</param>
        /// <returns>A matrix consisting of translation, rotation and scaling.</returns>
        public static Matrix4 FastMatrix4(Vector3 position, Vector3 scale, Vector3 axis, float angle)
        {
            Matrix4 matrix = Matrix4.CreateRotation(axis, angle);
            matrix[0] *= scale.x;
            matrix[1] *= scale.y;
            matrix[2] *= scale.z;
            matrix[3] = new Vector4(position.x, position.y, position.z, 1);
            return matrix;
        }

        /// <summary>
        /// Returns a combined translation, rotation and scaling matrix quickly.
        /// Note:  Does not modify the rotation matrix.
        /// </summary>
        /// <param name="position">The translation to apply to this matrix.</param>
        /// <param name="scale">The scaling to apply to this matrix.</param>
        /// <param name="rotation">An existing rotation matrix which will not be modified.</param>
        /// <returns>A matrix consisting of translation, rotation and scaling.</returns>
        public static Matrix4 FastMatrix4(Vector3 position, Vector3 scale, Matrix4 rotation)
        {
            Matrix4 matrix = new Matrix4(rotation);
            matrix[0] *= scale.x;
            matrix[1] *= scale.y;
            matrix[2] *= scale.z;
            matrix[3] = new Vector4(position.x, position.y, position.z, 1);
            return matrix;
        }

        /// <summary>
        /// Returns a combined translation and rotation matrix quickly.
        /// </summary>
        /// <param name="position">The translation to apply to this matrix.</param>
        /// <param name="axis">The axis angle to apply during the rotation.</param>
        /// <param name="angle">The angle (in radians) of the rotation.</param>
        /// <returns>A matrix consisting of a translation and rotation.</returns>
        public static Matrix4 FastMatrix4(Vector3 position, Vector3 axis, float angle)
        {
            Matrix4 matrix = Matrix4.CreateRotation(axis, angle);
            matrix[3] = new Vector4(position.x, position.y, position.z, 1);
            return matrix;
        }

        /// <summary>
        /// Returns a combined translation, rotation and scaling matrix quickly.
        /// </summary>
        /// <param name="position">The translation to apply to this matrix.</param>
        /// <param name="scale">The scaling to apply to this matrix.</param>
        /// <param name="orientation">The orientation as stored by a quaternion.</param>
        /// <returns>A matrix consisting of translation, rotation and scaling.</returns>
        public static Matrix4 FastMatrix4(Vector3 position, Vector3 scale, Quaternion orientation)
        {
            Matrix4 matrix = orientation.Matrix4;
            matrix[0] *= scale.x;
            matrix[1] *= scale.y;
            matrix[2] *= scale.z;
            matrix[3] = new Vector4(position.x, position.y, position.z, 1);
            return matrix;
        }

        /// <summary>
        /// Returns a combined translation and rotation matrix quickly.
        /// </summary>
        /// <param name="position">The translation to apply to this matrix.</param>
        /// <param name="orientation">The orientation as stored by a quaternion.</param>
        /// <returns>A matrix consisting of translation and rotation.</returns>
        public static Matrix4 FastMatrix4(Vector3 position, Quaternion orientation)
        {
            Matrix4 matrix = orientation.Matrix4;
            matrix[3] = new Vector4(position.x, position.y, position.z, 1);
            return matrix;
        }
        #endregion

        #region OpenGL Helpers (ShaderLogs, CalculateNormals, etc)
        /// <summary>
        /// Prints the shader log, vertex shader log and fragment shader log to the Console.
        /// </summary>
        /// <param name="program">The ShaderProgram whose logs should be printed to the Console.</param>
        public static void PrintShaderLogs(ShaderProgram program)
        {
            Console.WriteLine("Shader Program Log:\n" + program.ProgramLog);
            Console.WriteLine("Vertex Shader Log:\n" + program.VertexShader.ShaderLog);
            Console.WriteLine("Fragment Shader Log:\n" + program.FragmentShader.ShaderLog);
        }

        /// <summary>
        /// Calculate the array of vertex normals based on vertex and face information (assuming triangle polygons).
        /// </summary>
        /// <param name="vertices">The vertex data to find the normals for.</param>
        /// <param name="elements">The element array describing the order in which vertices are drawn.</param>
        /// <returns></returns>
        public static Vector3[] CalculateNormals(Vector3[] vertices, int[] elements)
        {
            Vector3 b1, b2, normal;
            Vector3[] normalData = new Vector3[vertices.Length];
            Vector3[] vertexData = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) vertexData[i] = new Vector3(Math.Floor(vertices[i].x), Math.Floor(vertices[i].y), Math.Floor(vertices[i].z));

            for (int i = 0; i < elements.Length / 3; i++)
            {
                int cornerA = elements[i * 3];
                int cornerB = elements[i * 3 + 1];
                int cornerC = elements[i * 3 + 2];

                b1 = vertexData[cornerB] - vertexData[cornerA];
                b2 = vertexData[cornerC] - vertexData[cornerA];

                normal = Vector3.Cross(b1, b2).Normalize();

                normalData[cornerA] += normal;
                normalData[cornerB] += normal;
                normalData[cornerC] += normal;
            }

            for (int i = 0; i < normalData.Length; i++) normalData[i] = normalData[i].Normalize();

            return normalData;
        }

        /// <summary>
        /// Calculate the array of vertex normals based on vertex and face information.
        /// Note:  This method is optimized to deal only with voxels with integer normals.
        /// This method is also optimized for quads that have 6 elements and 4 vertices packed together.
        /// </summary>
        /// <param name="vertices">The vertex data with 4 vertices per quad.</param>
        /// <param name="elements">The element array with 6 ints per quad.</param>
        /// <returns>The integer normal of the quad (up, down, left, right, front, back).</returns>
        public static Vector3[] CalculateNormalsFast(Vector3[] vertices, int[] elements, int vertexCount)
        {
            Vector3 b1, b2, normal;
            Vector3[] normalData = new Vector3[vertices.Length];
            int elementLength = vertexCount / 4 * 6;

            for (int i = 0; i < /*elements.Length*/elementLength / 6; i++)
            {
                // we only need 3 corners per quad to find the normals
                // assuming that quads are packed together
                int cornerA = elements[i * 6];
                int cornerB = elements[i * 6 + 1];
                int cornerC = elements[i * 6 + 2];

                b1 = vertices[cornerB] - vertices[cornerA];
                b2 = vertices[cornerC] - vertices[cornerA];

                // find the normal of the triangle as usual
                normal = Vector3.Cross(b1, b2).Normalize();

                // the normal is still wrong because of the fractional portion of the vertex position
                // however, rounding the normal instead of creating a new vertex array and taking the floor
                // of it is faster
                normal = new Vector3(Math.Round(normal.x), Math.Round(normal.y), Math.Round(normal.z));

                // apply the normal to the quad
                normalData[i * 4] = normal;
                normalData[i * 4 + 1] = normal;
                normalData[i * 4 + 2] = normal;
                normalData[i * 4 + 3] = normal;
            }

            return normalData;
        }
        #endregion

        #region Geometry
        /// <summary>
        /// Create a basic quad by storing two triangles into a VAO.
        /// This quad includes UV co-ordinates from 0,0 to 1,1.
        /// </summary>
        /// <param name="program">The ShaderProgram assigned to this quad.</param>
        /// <returns>The VAO object representing this quad.</returns>
        public static VAO CreateQuad(ShaderProgram program)//, Vector2 location, Vector2 size)
        {
            VBO<Vector3> vertices = new VBO<Vector3>(new Vector3[] { new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0) });
            VBO<Vector2> uvs = new VBO<Vector2>(new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            VBO<int> indices = new VBO<int>(new int[] { 0, 1, 3, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);

            return new VAO(program, vertices, uvs, indices);
        }

        /// <summary>
        /// Create a basic quad by storing two triangles into a VAO.
        /// This quad includes normal co-ordinates.
        /// </summary>
        /// <param name="program">The ShaderProgram assigned to this quad.</param>
        /// <param name="location">The location of the VAO (assigned to the vertices).</param>
        /// <param name="size">The size of the VAO (assigned to the vertices).</param>
        /// <returns>The VAO object representing this quad.</returns>
        public static VAO CreateQuad(ShaderProgram program, Vector2 location, Vector2 size, Vector3 color)
        {
            Vector3[] vertices = new Vector3[] { new Vector3(location.x, location.y, 0), new Vector3(location.x + size.x, location.y, 0),
                new Vector3(location.x + size.x, location.y + size.y, 0), new Vector3(location.x, location.y + size.y, 0) };
            for (int i = 0; i < vertices.Length; i++) vertices[i] += color;
            int[] indices = new int[] { 0, 1, 2, 3 };//new int[] { 0, 1, 3, 1, 3, 2 };

            Vector3[] normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = new Vector3(0, 255, 0);

            var vao = new VAO(program, new VBO<Vector3>(vertices), new VBO<Vector3>(normals), new VBO<int>(indices, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw));
            vao.DrawMode = BeginMode.Quads;
            return vao;
        }

        /// <summary>
        /// Creates a very simple cube that has uniform normals for each face.
        /// This means we actually need to build it using 24 vertices instead
        /// of the usual minimum of 8 vertices.
        /// </summary>
        /// <param name="program">The shader program to use when creating the VAO.</param>
        /// <param name="min">The minimum point on the cube.</param>
        /// <param name="max">The maximum point on the cube.</param>
        /// <returns>A vertex array object containing the cube object, which has vertices, normals and elements.</returns>
        public static VAO CreateCubeWithNormals(ShaderProgram program, Vector3 min, Vector3 max)
        {
            Vector3[] vertexList = new Vector3[] {
                new Vector3(min.x, min.y, max.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, min.y, min.z)
            };

            int[] quadList = new int[] {
                 0, 1, 3, 2,
                 1, 4, 5, 3,
                 7, 4, 1, 0,
                 2, 3, 5, 6,
                 4, 7, 6, 5,
                 7, 0, 2, 6,
             };

            List<Vector3> vertex = new List<Vector3>();
            List<int> element = new List<int>();

            for (int i = 0; i < quadList.Length / 4; i++)
                AddFace(vertexList[quadList[i * 4]], vertexList[quadList[i * 4 + 1]], vertexList[quadList[i * 4 + 2]], vertexList[quadList[i * 4 + 3]], vertex, element, true, false);

            Vector3[] vertices = vertex.ToArray();
            int[] elements = element.ToArray();

            Vector3[] normal = CalculateNormalsFast(vertices, elements, vertices.Length);

            return new VAO(program, new VBO<Vector3>(vertices), new VBO<Vector3>(normal), new VBO<int>(elements, BufferTarget.ElementArrayBuffer, BufferUsageHint.StaticDraw));
        }

        /// <summary>
        /// Adds a face to the list of vertices and elements.  Can take care of flipping (to fix face culling)
        /// and inversion (to take care of anistropy when performing ambient occlusion).
        /// </summary>
        /// <param name="v1">The first vertex that makes up the face.</param>
        /// <param name="v2">The second vertex that makes up the face.</param>
        /// <param name="v3">The third vertex that makes up the face.</param>
        /// <param name="v4">The fourth vertex that makes up the face.</param>
        /// <param name="vertices">The list of vertices that make up this voxel chunk.</param>
        /// <param name="elements">The list of elements that make up this voxel chunk.</param>
        /// <param name="flip">True to flip the face winding.</param>
        /// <param name="invert">True to invert the face polygon to fix anisotropy.</param>
        private static void AddFace(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> elements, bool flip = false, bool invert = false)
        {
            // Note:  Originally this method used array constructors and AddRange,
            // but just adding the vertices/elements directly is quite a bit faster.
            // We sped up chunk generation by around 10%!
            int offset = vertices.Count;

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            if (invert)
            {
                elements.Add((flip ? 1 : 0) + offset);
                elements.Add(3 + offset);
                elements.Add((flip ? 0 : 1) + offset);
                elements.Add((flip ? 1 : 3) + offset);
                elements.Add(2 + offset);
                elements.Add((flip ? 3 : 1) + offset);
            }
            else
            {
                elements.Add((flip ? 0 : 2) + offset);
                elements.Add(1 + offset);
                elements.Add((flip ? 2 : 0) + offset);
                elements.Add((flip ? 2 : 0) + offset);
                elements.Add(3 + offset);
                elements.Add((flip ? 0 : 2) + offset);
            }
        }

        /// <summary>
        /// Create a flat mesh (or plane) with a defined number of segments and UV tiling.
        /// </summary>
        /// <param name="program">The shader program that will be used with this mesh.</param>
        /// <param name="width">The width of the mesh.</param>
        /// <param name="height">The depth of the mesh.</param>
        /// <param name="xSegments">The number of segments in the x direction (width).</param>
        /// <param name="ySegments">The number of segments in the z direction (depth)</param>
        /// <param name="xTile">The amount to tile the UV coordinates in the x direction.</param>
        /// <param name="zTile">The amount to tile the UV coordinates in the z direction.</param>
        /// <returns>A VAO containing the mesh (vertex, UV and index) as defined by the input parameters.</returns>
        public static VAO CreateMesh(ShaderProgram program, float width, float height, int xSegments, int zSegments, float xTile, float zTile)
        {
            Vector3[] vertices = new Vector3[xSegments * zSegments * 2 * 3];
            Vector2[] uvs = new Vector2[xSegments * zSegments * 2 * 3];
            int[] indices = new int[xSegments * zSegments * 2 * 3];

            float UVdelX = xTile / xSegments, UVdelY = zTile / zSegments;
            float delX = width / xSegments, delY = height / zSegments;
            float px = -width / 2, py = -height / 2, puvx = 0, puvy = 0;

            for (int z = 0, index = 0; z < zSegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    // First triangle
                    vertices[index] = new Vector3(px, 0, py);
                    uvs[index++] = new Vector2(puvx, puvy);
                    vertices[index] = new Vector3(px + delX, 0, py);
                    uvs[index++] = new Vector2(puvx + UVdelX, puvy);
                    vertices[index] = new Vector3(px, 0, py + delY);
                    uvs[index++] = new Vector2(puvx, puvy + UVdelY);

                    // Second triangle
                    vertices[index] = new Vector3(px + delX, 0, py);
                    uvs[index++] = new Vector2(puvx + UVdelX, puvy);
                    vertices[index] = new Vector3(px + delX, 0, py + delY);
                    uvs[index++] = new Vector2(puvx + UVdelX, puvy + UVdelY);
                    vertices[index] = new Vector3(px, 0, py + delY);
                    uvs[index++] = new Vector2(puvx, puvy + UVdelY);

                    px += delX; puvx += UVdelX;     // increment the x values
                }

                px = -width / 2; puvx = 0;          // reset the x values
                py += delY; puvy += UVdelY;         // increment the y values
            }

            for (int i = 0; i < indices.Length; i++) indices[i] = i;

            return new VAO(program, new VBO<Vector3>(vertices), new VBO<Vector2>(uvs), new VBO<int>(indices, BufferTarget.ElementArrayBuffer));
        }
        #endregion

        #region Common Operations
        private static Stack<FBO> framebufferStack = new Stack<FBO>();

        public static FBO CurrentBuffer()
        {
            return (framebufferStack.Count == 0 ? null : framebufferStack.Peek());
        }

        public static void PushFramebuffer(FBO fbo)
        {
            framebufferStack.Push(fbo);
            fbo.Enable();
        }

        public static FBO PopFramebuffer()
        {
            var fbo = framebufferStack.Pop();
            fbo.Disable();
            if (framebufferStack.Count != 0) framebufferStack.Peek().Enable(false);
            else Gl.Viewport(0, 0, Program.Width, Program.Height);
            return fbo;
        }

        public static void BufferSubData<T>(this VBO<T> vbo, T[] data) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                vbo.BufferSubDataPinned(data, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public static void BufferSubData<T>(this VBO<T> vbo, T[] data, int size) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                vbo.BufferSubDataPinned(BufferTarget.ArrayBuffer, size, handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public static void BufferSubDataPinned<T>(this VBO<T> vbo, T[] data, IntPtr pinnedObject) where T : struct
        {
            Gl.BindBuffer(vbo);
            Gl.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr)(Marshal.SizeOf(data[0]) * data.Length), pinnedObject);
        }

        public static void BufferSubDataPinned<T>(this VBO<T> vbo, BufferTarget target, int size, IntPtr data) where T : struct
        {
            Gl.BindBuffer(vbo);
            Gl.BufferSubData(target, IntPtr.Zero, (IntPtr)size, data);
        }

        public static void BufferSubData<T>(uint vboID, BufferTarget target, T[] data, int length)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Gl.BindBuffer(target, vboID);
                Gl.BufferSubData(target, IntPtr.Zero, (IntPtr)(Marshal.SizeOf(data[0]) * length), handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public static void BufferSubData(uint vboID, BufferTarget target, Vector3[] data, int length)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Gl.BindBuffer(target, vboID);
                Gl.BufferSubData(target, IntPtr.Zero, (IntPtr)(12 * length), handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Draws a VAO object with the Font3DShader.
        /// Requires a valid BMFont object to pull the texture from.
        /// </summary>
        /// <param name="font">The BMFont object with a valid texture file.</param>
        /// <param name="fontVAO">A VAO containing vertices/UV, compiled against the font object passed as 'font'.</param>
        /// <param name="transform">The transform to apply to the model matrix when rendering the text.</param>
        public static void DrawWithFontShader(BMFont font, VAO<Vector3, Vector2> fontVAO, Matrix4 transform)
        {
            Shaders.Font3DShader.Use();
            Shaders.Font3DShader["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(0, 2.3f, 0)) * transform);

            Gl.BindTexture(font.FontTexture);

            Gl.Enable(EnableCap.Blend);
            fontVAO.Draw();
            Gl.Disable(EnableCap.Blend);
        }
        #endregion

        #region Extension Methods
        /// <summary>
        /// Transforms a Vector3 by a supplied Matrix4.
        /// </summary>
        /// <param name="v">The vector to transform.</param>
        /// <param name="m">The matrix to transform the vector by.</param>
        /// <returns>A transformed Vector3 using the formula (vec4(v, 1) * m).xyz / (vec4(v, 1) * m).w</returns>
        public static Vector3 Transform(this Vector3 v, Matrix4 m)
        {
            //need a 4-part vector in order to multiply by a 4x4 matrix
            Vector4 temp = new Vector4(v, 1f) * m;

            //view projection matrices make use of the w component
            return temp.Xyz / temp.w;
        }

        /// <summary>
        /// Clamps a vector between a maximum and minimum vector.
        /// </summary>
        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(Math.Max(min.x, Math.Min(max.x, v.x)), Math.Max(min.y, Math.Min(max.y, v.y)), Math.Max(min.z, Math.Min(max.z, v.z)));
        }

        private static VAO cubeVAO;

        /// <summary>
        /// Draws a simple wireframe bounding box (triangle based) around an axis aligned bounding box.
        /// </summary>
        /// <param name="box">The bounding box to draw a wireframe mesh around.</param>
        public static void Draw(this AxisAlignedBoundingBox box)
        {
            // draw a bounding box around the object
            if (cubeVAO == null)
                cubeVAO = Utilities.CreateCubeWithNormals(Shaders.SimpleColoredShader, Vector3.Zero, Vector3.UnitScale);

            float xscale = box.Max.x - box.Min.x;
            float yscale = box.Max.y - box.Min.y;
            float zscale = box.Max.z - box.Min.z;

            Shaders.SimpleColoredShader.Use();
            Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            Shaders.SimpleColoredShader["color"].SetValue(Vector3.UnitScale);
            Shaders.SimpleColoredShader["model_matrix"].SetValue(Matrix4.CreateScaling(new Vector3(xscale, yscale, zscale)) * Matrix4.CreateTranslation(box.Min));
            cubeVAO.Draw();
            Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        /// <summary>
        /// Returns the floor of a Vector3.
        /// </summary>
        public static Vector3 Floor(this Vector3 v)
        {
            return new Vector3(Math.Floor(v.x), Math.Floor(v.y), Math.Floor(v.z));
        }

        /// <summary>
        /// Returns the ceiling of a Vector3.
        /// </summary>
        public static Vector3 Ceiling(this Vector3 v)
        {
            return new Vector3(Math.Ceiling(v.x), Math.Ceiling(v.y), Math.Ceiling(v.z));
        }
        #endregion
    }
}
