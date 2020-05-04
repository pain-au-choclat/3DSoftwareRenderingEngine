namespace _3DSoftwareRenderingEngine.JsonObject
{
    public class Rootobject
    {
        public Producer Producer { get; set; }
        public bool AutoClear { get; set; }
        public float[] ClearColor { get; set; }
        public float[] Gravity { get; set; }
        public Material[] Materials { get; set; }
        public object[] MultiMaterials { get; set; }
        public object[] Skeletons { get; set; }
        public Mesh[] Meshes { get; set; }
        public object[] MorphTargetManagers { get; set; }
        public object[] Cameras { get; set; }
        public Light[] Lights { get; set; }
        public object[] ShadowGenerators { get; set; }
    }

    public class Producer
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Exporter_version { get; set; }
        public string File { get; set; }
    }

    public class Material
    {
        public string Name { get; set; }
        public string DiffuseTextureName { get; set; }
        public string Id { get; set; }
        public string CustomType { get; set; }
        public bool BackFaceCulling { get; set; }
        public bool CheckReadyOnlyOnce { get; set; }
        public int MaxSimultaneousLights { get; set; }
        public int EnvironmentIntensity { get; set; }
        public float[] Albedo { get; set; }
        public float[] Reflectivity { get; set; }
        public int Metallic { get; set; }
    }

    public class Mesh
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string MaterialId { get; set; }
        public int BillboardMode { get; set; }
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] Scaling { get; set; }
        public bool IsVisible { get; set; }
        public bool FreezeWorldMatrix { get; set; }
        public bool IsEnabled { get; set; }
        public bool CheckCollisions { get; set; }
        public bool ReceiveShadows { get; set; }
        public bool Pickable { get; set; }
        public string Tags { get; set; }
        public float[] Positions { get; set; }
        public float[] Normals { get; set; }
        public float[] Uvs { get; set; }
        public int[] Indices { get; set; }
        public Submesh[] SubMeshes { get; set; }
        public Animation[] Animations { get; set; }
        public Range[] Ranges { get; set; }
        public object[] Instances { get; set; }
        public string ParentId { get; set; }
    }

    public class Submesh
    {
        public int MaterialIndex { get; set; }
        public int VerticesStart { get; set; }
        public int VerticesCount { get; set; }
        public int IndexStart { get; set; }
        public int IndexCount { get; set; }
    }

    public class Animation
    {
        public int DataType { get; set; }
        public int FramePerSecond { get; set; }
        public Key[] Keys { get; set; }
        public int LoopBehavior { get; set; }
        public string Name { get; set; }
        public string Property { get; set; }
    }

    public class Key
    {
        public int Frame { get; set; }
        public float[] Values { get; set; }
    }

    public class Range
    {
        public string Name { get; set; }
        public int From { get; set; }
        public int To { get; set; }
    }

    public class Light
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Type { get; set; }
        public float[] Position { get; set; }
        public float[] Direction { get; set; }
        public int IntensityMode { get; set; }
        public float Intensity { get; set; }
        public float[] Diffuse { get; set; }
        public float[] Specular { get; set; }
        public float Radius { get; set; }
        public int Range { get; set; }
    }
}
