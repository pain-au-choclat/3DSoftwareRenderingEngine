using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DSoftwareRenderingEngine.JsonObject
{

    public class Rootobject
    {
        public Producer producer { get; set; }
        public bool autoClear { get; set; }
        public float[] clearColor { get; set; }
        public float[] gravity { get; set; }
        public object[] materials { get; set; }
        public object[] multiMaterials { get; set; }
        public object[] skeletons { get; set; }
        public Mesh[] meshes { get; set; }
        public object[] morphTargetManagers { get; set; }
        public Camera[] cameras { get; set; }
        public string activeCameraID { get; set; }
        public Light[] lights { get; set; }
        public object[] shadowGenerators { get; set; }
    }

    public class Producer
    {
        public string name { get; set; }
        public string version { get; set; }
        public string exporter_version { get; set; }
        public string file { get; set; }
    }

    public class Mesh
    {
        public string name { get; set; }
        public string id { get; set; }
        public int billboardMode { get; set; }
        public int[] position { get; set; }
        public int[] rotation { get; set; }
        public int[] scaling { get; set; }
        public bool isVisible { get; set; }
        public bool freezeWorldMatrix { get; set; }
        public bool isEnabled { get; set; }
        public bool checkCollisions { get; set; }
        public bool receiveShadows { get; set; }
        public bool pickable { get; set; }
        public string tags { get; set; }
        public float[] positions { get; set; }
        public float[] normals { get; set; }
        public float[] uvs { get; set; }
        public int[] indices { get; set; }
        public Submesh[] subMeshes { get; set; }
        public object[] instances { get; set; }
    }

    public class Submesh
    {
        public int materialIndex { get; set; }
        public int verticesStart { get; set; }
        public int verticesCount { get; set; }
        public int indexStart { get; set; }
        public int indexCount { get; set; }
    }

    public class Camera
    {
        public string name { get; set; }
        public string id { get; set; }
        public float[] position { get; set; }
        public float[] rotation { get; set; }
        public float fov { get; set; }
        public float minZ { get; set; }
        public int maxZ { get; set; }
        public int speed { get; set; }
        public float inertia { get; set; }
        public bool checkCollisions { get; set; }
        public bool applyGravity { get; set; }
        public float[] ellipsoid { get; set; }
        public int cameraRigMode { get; set; }
        public float interaxial_distance { get; set; }
        public string type { get; set; }
    }

    public class Light
    {
        public string name { get; set; }
        public string id { get; set; }
        public int type { get; set; }
        public float[] position { get; set; }
        public int range { get; set; }
        public int intensityMode { get; set; }
        public int intensity { get; set; }
        public int[] diffuse { get; set; }
        public int[] specular { get; set; }
        public float radius { get; set; }
    }
}
