using _3DSoftwareRenderingEngine.Structs;
using System.Numerics;

namespace _3DSoftwareRenderingEngine.Classes
{
    public class Mesh
    {
        public string Name { get; set; }

        public Vertex[] Vertices { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Face[] Faces { get; set; }

        public Texture Texture { get; set; }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
            Name = name;
        }
    }
}
