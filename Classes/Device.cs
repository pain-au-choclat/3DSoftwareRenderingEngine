using _3DSoftwareRenderingEngine.JsonObject;
using _3DSoftwareRenderingEngine.Structs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace _3DSoftwareRenderingEngine.Classes
{
    public class Device
    {
        private readonly byte[] _backBuffer;
        private readonly float[] _depthBuffer;
        private readonly WriteableBitmap _bmp;
        private const bool WiremeshMode = true;
        private readonly int _renderWidth;
        private readonly int _renderHeight;
        private readonly IBuffer _pixelBuffer;
        private readonly object[] _lockBuffer;

        public Device(WriteableBitmap bmp)
        {
            _bmp = bmp;
            _renderWidth = bmp.PixelWidth;
            _renderHeight = bmp.PixelHeight;
            _pixelBuffer = bmp.PixelBuffer;

            _backBuffer = new byte[_renderWidth * _renderHeight * 4];
            _depthBuffer = new float[_renderWidth * _renderHeight];
            _lockBuffer = new object[_renderWidth * _renderHeight];

            for (var i = 0; i < _lockBuffer.Length; i++)
            {
                _lockBuffer[i] = new object();
            }
        }

        /// <summary>
        /// Clear is used to clear the back buffer with a specific color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (var index = 0; index < _backBuffer.Length; index += 4)
            {
                _backBuffer[index] = b;
                _backBuffer[index + 1] = g;
                _backBuffer[index + 2] = r;
                _backBuffer[index + 3] = a;
            }

            for (var index = 0; index < _depthBuffer.Length; index++)
            {
                _depthBuffer[index] = float.MaxValue;
            }
        }

        /// <summary>
        /// Present pushes the back buffer into the front buffer
        /// </summary>
        public void Present()
        {
            using (var stream = _pixelBuffer.AsStream())
            {
                stream.Write(_backBuffer, 0, _backBuffer.Length);
            }

            _bmp.Invalidate();
        }

        /// <summary>
        /// PutPixel is used to put a pixel on the screen at specific coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void PutPixel(double x, double y, Color color)
        {
            int index = (int)(x + y * _renderWidth) * 4;

            _backBuffer[index] = color.B;
            _backBuffer[index + 1] = color.G;
            _backBuffer[index + 2] = color.R;
            _backBuffer[index + 3] = color.A;
        }

        public void PutPixelVector3(int x, int y, float z, Color color)
        {
            var index = x + y * _renderWidth;
            var index4 = index * 4;

            lock (_lockBuffer[index])
            {
                if (_depthBuffer[index] < z)
                {
                    return; // Discard
                }

                _depthBuffer[index] = z;

                _backBuffer[index4] = color.B;
                _backBuffer[index4 + 1] = color.G;
                _backBuffer[index4 + 2] = color.R;
                _backBuffer[index4 + 3] = color.A;
            }
        }

        /// <summary>
        /// Project takes 3d coordinates and transforms them into 2d coordinates using transformation matrix
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="transMat"></param>
        /// <returns></returns>
        public Vector2 ProjectVector2(Vector3 coord, Matrix4x4 transMat)
        {
            var point = Vector3.TransformNormal(coord, transMat);

            var x = point.X * _renderWidth + _renderWidth / 2.0f;
            var y = -point.Y * _renderHeight + _renderHeight / 2.0f;

            return new Vector2(x, y);
        }

        public Vector3 ProjectVector3(Vector3 coord, Matrix4x4 transMat)
        {
            var point = Vector3.TransformNormal(coord, transMat);

            var x = point.X * _renderWidth + _renderWidth / 2.0f;
            var y = -point.Y * _renderHeight + _renderHeight / 2.0f;

            return (new Vector3(x, y, point.Z));
        }

        /// <summary>
        ///  Project vertex takes 3d coordinates and transforms them in to 2d
        ///  It also transfomrs them and the normals to the vertex
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="transMat"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public Vertex ProjectVertex(Vertex vertex, Matrix4x4 transMat, Matrix4x4 world)
        {
            var point2d = Vector3.TransformNormal(vertex.Coordinates, transMat);
            var point3dWorld = Vector3.TransformNormal(vertex.Coordinates, world);
            var normal3dWorld = Vector3.TransformNormal(vertex.Normal, world);

            var x = point2d.X * _renderWidth + _renderWidth / 2.0f;
            var y = -point2d.Y * _renderHeight + _renderHeight / 2.0f;

            return new Vertex()
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = normal3dWorld,
                WorldCoordinates = point3dWorld,
                TextureCoordinates = vertex.TextureCoordinates
            };
        }

        /// <summary>
        /// DrawPoint clips what is visible on screen then calls put pixel
        /// </summary>
        /// <param name="point"></param>
        public void DrawPoint(Vector2 point, bool antiAliasing)
        {
            if (antiAliasing)
            {
                for (var roundedx = Math.Floor(point.X); roundedx <= Math.Ceiling(point.X); roundedx++)
                {
                    for (var roundedy = Math.Floor(point.Y); roundedy <= Math.Ceiling(point.Y); roundedy++)
                    {
                        var percent_x = 1 - Math.Abs(point.X - roundedx);
                        var percent_y = 1 - Math.Abs(point.Y - roundedy);
                        var percent = percent_x * percent_y;
                        //DrawPixel(coordinates roundedx, roundedy, color percent(range 0 - 1))

                        var color = GetColorFromPercentage(percent);

                        if (point.X >= 0 && point.Y >= 0 && point.X < _renderWidth && point.Y < _renderHeight)
                        {
                            PutPixel(roundedx, roundedy, color);
                        }
                    }
                }
            }
            else
            {
                if (point.X >= 0 && point.Y >= 0 && point.X < _renderWidth && point.Y < _renderHeight)
                {
                    var color = GetColorFromPercentage(0);
                    PutPixel((int)point.X, (int)point.Y, color);
                }
            }
        }

        public void DrawPointVector3(Vector3 point, Color color)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < _renderWidth && point.Y < _renderHeight)
            {
                PutPixelVector3((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        /// <summary>
        /// Gets the specific color from the percentage
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public Color GetColorFromPercentage(double percent)
        {
            Color color = Color.Yellow;

            if (percent > 0)
            {
                var r = color.R;
                var g = color.G;
                var b = color.B;

                Color newColor = Color.FromArgb(255, r * (int)percent, g * (int)percent, b * (int)percent);

                return newColor;
            }

            return color;
        }

        /// <summary>
        /// DrawLine recursively draws a line between 2 points
        /// </summary>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        public void DrawLine(Vector2 point0, Vector2 point1)
        {
            var distance = (point1 - point0).Length();

            if (distance < 2)
            {
                return;
            }

            Vector2 middlePoint = point0 + (point1 - point0) / 2;

            DrawPoint(middlePoint, true);

            DrawLine(point0, middlePoint);
            DrawLine(middlePoint, point1);
        }

        /// <summary>
        /// DrawBline is a more effcient way of drawing lines
        /// It uses http://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        /// </summary>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        public void DrawBline(Vector2 point0, Vector2 point1)
        {
            var x0 = (int)point0.X;
            var y0 = (int)point0.Y;
            var x1 = (int)point1.X;
            var y1 = (int)point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector2(x0, y0), false);

                if ((x0 == x1) && (y0 == y1))
                {
                    break;
                }

                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>
        /// Sorting the points in order to always have this order on screen p1, p2 & p3
        /// with p1 always up (thus having the Y the lowest possible to be near the top screen)
        /// then p2 between p1 & p3
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="color"></param>
        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color color, Texture texture)
        {
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates;
            Vector3 p2 = v2.Coordinates;
            Vector3 p3 = v3.Coordinates;

            Vector3 lightPos = new Vector3(0, 10, 10);

            float nl1 = ComputeNDotl(v1.WorldCoordinates, v1.Normal, lightPos);
            float nl2 = ComputeNDotl(v2.WorldCoordinates, v2.Normal, lightPos);
            float nl3 = ComputeNDotl(v3.WorldCoordinates, v3.Normal, lightPos);

            var data = new ScanLineData { };

            // inverse slopes
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing inverse slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl2;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v2.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v2.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v1, v2, color, texture);
                    }
                    else
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl2;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v2.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v2.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v2, v3, color, texture);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl2;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v2.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v2.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v2, v1, v3, color, texture);
                    }
                    else
                    {
                        data.ndotla = nl2;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v2.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v2.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v2, v3, v1, v3, color, texture);
                    }
                }
            }
        }

        /// <summary>
        ///  Compute the cosine of the angle between the light vector and the normal vector
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="normal"></param>
        /// <param name="lightPos"></param>
        /// <returns></returns>
        public float ComputeNDotl(Vector3 vertex, Vector3 normal, Vector3 lightPos)
        {
            var lightDir = lightPos - vertex;

            var norm = Vector3.Normalize(normal);
            var dir = Vector3.Normalize(lightDir);

            return Math.Max(0, Vector3.Dot(norm, dir));
        }

        /// <summary>
        /// ProcessScanLine draws a line between 2 points from left to right
        /// papb -> pcpd
        /// pa, pb, pc, pd must be sorted before
        /// </summary>
        /// <param name="data"></param>
        /// <param name="va"></param>
        /// <param name="vb"></param>
        /// <param name="vc"></param>
        /// <param name="vd"></param>
        /// <param name="color"></param>
        public void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color color, Texture texture)
        {
            Vector3 pa = va.Coordinates;
            Vector3 pb = vb.Coordinates;
            Vector3 pc = vc.Coordinates;
            Vector3 pd = vd.Coordinates;

            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(data.ndotla, data.ndotlb, gradient1);
            var enl = Interpolate(data.ndotlc, data.ndotld, gradient2);

            // Interpolate texture coordinates on Y
            var su = Interpolate(data.ua, data.ub, gradient1);
            var eu = Interpolate(data.uc, data.ud, gradient2);
            var sv = Interpolate(data.va, data.vb, gradient1);
            var ev = Interpolate(data.vc, data.vd, gradient2);

            for (var x = sx; x < ex; x++)
            {
                var gradient = (x - sx) / (float)(ex - sx);

                // Interpolationg Z, normal and texture coordinates on X
                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                var u = Interpolate(su, eu, gradient);
                var v = Interpolate(sv, ev, gradient);

                Color textureColor;

                if (texture != null)
                {
                    textureColor = texture.Map(u, v);
                }
                else
                {
                    textureColor = Color.FromArgb(255, 255, 255, 255);
                }

                var r = textureColor.R * ndotl * (color.R / 255);
                var g = textureColor.G * ndotl * (color.G / 255);
                var b = textureColor.B * ndotl * (color.B / 255);
                var a = textureColor.A * ndotl * (color.A / 255);

                Color newColor;
                newColor = Color.FromArgb((int)a, (int)r, (int)g, (int)b);

                DrawPointVector3(new Vector3(x, data.currentY, z), newColor);
            }
        }

        /// <summary>
        /// Interpolating the value between 2 vertices, min is the starting point max is the end
        /// and gradient is percentage diff between
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="gradient"></param>
        /// <returns></returns>
        public float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        /// <summary>
        /// Clamps values to keep them between 0 and 1
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        /// <summary>
        /// Render re-computes each vertex projection during each frame
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="meshes"></param>
        public void Render(Camera camera, params Mesh[] meshes)
        {
            var viewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(2.5f, 1, 2.9f, 10.0f);

            foreach (Mesh mesh in meshes)
            {
                var worldMatrix = Matrix4x4.CreateFromYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix4x4.CreateTranslation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                if (WiremeshMode)
                {
                    foreach (var face in mesh.Faces)
                    {
                        var vertexA = mesh.Vertices[face.A];
                        var vertexB = mesh.Vertices[face.B];
                        var vertexC = mesh.Vertices[face.C];

                        var pixelA = ProjectVector2(vertexA.Coordinates, transformMatrix);
                        var pixelB = ProjectVector2(vertexB.Coordinates, transformMatrix);
                        var pixelC = ProjectVector2(vertexC.Coordinates, transformMatrix);

                        DrawBline(pixelA, pixelB);
                        DrawBline(pixelB, pixelC);
                        DrawBline(pixelC, pixelA);
                    }
                }
                else
                {
                    Parallel.For(0, mesh.Faces.Length, faceIndex =>
                    {
                        var face = mesh.Faces[faceIndex];

                        var vertexA = mesh.Vertices[face.A];
                        var vertexB = mesh.Vertices[face.B];
                        var vertexC = mesh.Vertices[face.C];

                        var pixelA = ProjectVertex(vertexA, transformMatrix, worldMatrix);
                        var pixelB = ProjectVertex(vertexB, transformMatrix, worldMatrix);
                        var pixelC = ProjectVertex(vertexC, transformMatrix, worldMatrix);

                        //var color = 0.25f + (faceIndex % mesh.Faces.Length) * 0.75f / mesh.Faces.Length;
                        //color *= 255;
                        Color newColor;
                        newColor = Color.White;

                        DrawTriangle(pixelA, pixelB, pixelC, newColor, mesh.Texture);

                    });
                }
            }
        }

        /// <summary>
        /// Loads the mesh in via json export from blender
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Mesh[] LoadJSONFileAsync(string fileName)
        {
            var meshes = new List<Mesh>();
            var materials = new Dictionary<string, Material>();

            StorageFolder location = Package.Current.InstalledLocation;

            var data = File.ReadAllText(Path.Combine($"{location.Path}\\Assets", fileName));

            Rootobject jsonObject = JsonConvert.DeserializeObject<Rootobject>(data);

            for (var materialIndex = 0; materialIndex < jsonObject.Materials.Length; materialIndex++)
            {
                var material = new Material()
                {
                    Name = jsonObject.Materials[materialIndex].Id,
                    ID = jsonObject.Materials[materialIndex].Name
                };

                if (!string.IsNullOrWhiteSpace(jsonObject.Materials[materialIndex].DiffuseTextureName))
                {
                    material.DiffuseTextureName = jsonObject.Materials[materialIndex].DiffuseTextureName;
                }

                materials.Add(material.ID, material);
            }

            for (var meshIndex = 0; meshIndex<jsonObject.Meshes.Length; meshIndex++)
            {
                if (jsonObject.Meshes[meshIndex].Positions != null)
                {
                    var verticesArray = jsonObject.Meshes[meshIndex].Positions;

                    var indicesArray = jsonObject.Meshes[meshIndex].Indices;

                    var normals = jsonObject.Meshes[meshIndex].Normals;

                    var uvArray = jsonObject.Meshes[meshIndex].Uvs;

                    // the number of interesting vertices information for us
                    var verticesCount = verticesArray.Length / 3;

                    // number of faces is logically the size of the array divided by 3 (A, B, C)
                    var facesCount = indicesArray.Length / 3;
                    var mesh = new Mesh(jsonObject.Meshes[meshIndex].Name, verticesCount, facesCount);

                    // Filling the Vertices array of our mesh first
                    var verticesCounter = 0;

                    while (verticesCounter != verticesCount)
                    {
                        var arrayPosition = verticesCounter * 3;
                        var uvArrayPosition = verticesCounter * 2;

                        var x = verticesArray[arrayPosition];
                        var y = verticesArray[arrayPosition + 1];
                        var z = verticesArray[arrayPosition + 2];

                        // Load up the normals
                        var nx = normals[arrayPosition];
                        var ny = normals[arrayPosition + 1];
                        var nz = normals[arrayPosition + 2];

                        mesh.Vertices[verticesCounter] = new Vertex
                        {
                            Coordinates = new Vector3(x, y, z),
                            Normal = new Vector3(nx, ny, nz)
                        };

                        if (uvArray.Length > 0)
                        {
                            float u = uvArray[uvArrayPosition];
                            float v = uvArray[uvArrayPosition + 1];

                            mesh.Vertices[verticesCounter].TextureCoordinates = new Vector2(u, v);
                        }
                        verticesCounter++;
                    }

                    // Then filling the Faces array
                    var facesCounter = 0;

                    while (facesCounter != facesCount)
                    {
                        var arrayPosition = facesCounter * 3;

                        var a = indicesArray[arrayPosition];
                        var b = indicesArray[arrayPosition + 1];
                        var c = indicesArray[arrayPosition + 2];

                        mesh.Faces[facesCounter] = new Face
                        {
                            A = a,
                            B = b,
                            C = c
                        };
                        facesCounter++;
                    }

                    // Getting the position you've set in Blender
                    var position = jsonObject.Meshes[meshIndex].Position;
                    mesh.Position = new Vector3(position[0], position[1], position[2]);

                    if (uvArray.Length > 0)
                    {
                        // Texture
                        var meshTextureId = jsonObject.Meshes[meshIndex].MaterialId;
                        var meshTextureName = string.IsNullOrWhiteSpace(materials[meshTextureId].DiffuseTextureName) ? materials[meshTextureId].ID 
                            : materials[meshTextureId].DiffuseTextureName;

                        mesh.Texture = new Texture(meshTextureName, 512, 512);
                    }

                    meshes.Add(mesh);
                }
            }

            return meshes.ToArray();
        }
    }
}
