using System.Numerics;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Loaders;

namespace ET.Client;

[EntitySystemOf(typeof(MeshComponent))]
public static partial class MeshComponentSystem
{
    [EntitySystem]
    private static void Awake(this MeshComponent self, string a)
    {
        if (!LoadObj(a, out self.meshInfo.indices, out self.meshInfo.positions, out self.meshInfo.uvs, out self.meshInfo.normals))
        {
            Log.Instance.Error($"Load Obj Error: {a}");
        }

        self.shaders = new Dictionary<Type, Type>
        {
            { typeof(ShadowRenderPass), typeof(DefaultShadowShader) },
            { typeof(ShadingRenderPass), typeof(DefaultShadingShader) }
        };

        self.AddToDirtyMesh();
    }

    [EntitySystem]
    private static void Awake(this MeshComponent self, ushort[] a, Vector3[] b, Vector2[] c, Vector3[] d, Dictionary<Type, Type> e)
    {
        self.meshInfo.indices = a;
        self.meshInfo.positions = b;
        self.meshInfo.uvs = c;
        self.meshInfo.normals = d;

        self.shaders = e;
            
        self.AddToDirtyMesh();
    }

    public static AABB AABB(this MeshComponent self)
    {
        if (self.meshInfo.indices == null || self.meshInfo.indices.Length == 0 || self.meshInfo.positions == null || self.meshInfo.positions.Length == 0)
        {
            return ET.Client.AABB.None;
        }

        AABB aabb = ET.Client.AABB.None;
        
        foreach (ushort i in self.meshInfo.indices)
        {
            Vector3 p = self.meshInfo.positions[i];
            aabb.Encapsulate(p);
        }
        
        return aabb;
    }

    public static void AddToDirtyMesh(this MeshComponent self)
    {
        self.Scene().GetComponent<RenderComponent>().GetComponent<DirtyMeshComponent>().dirtyMeshes.Enqueue(self.GetParent<ViewObject>());
    }
    
    public static LoadResult LoadObj(string path)
    {
        var loader = new ObjLoaderFactory().Create();
        using var fs = new FileStream(path, FileMode.Open);
        return loader.Load(fs);
    }
    public static bool LoadObj(string path, out ushort[] indices, out Vector3[] positions, out Vector2[] uvs, out Vector3[] normals)
    {
        var vertexDict = new Dictionary<FaceVertex, ushort>(); // 去重
        var positionList = new List<Vector3>();
        var uvList = new List<Vector2>();
        var normalList = new List<Vector3>();
        var indexList = new List<ushort>();
        
        var result = LoadObj(path);
        foreach (var group in result.Groups)
        {
            foreach (var face in group.Faces)
            {
                for(int i = 0; i < face.Count; ++i)
                {
                    var faceVertex = face[i];
                    
                    if (!vertexDict.TryGetValue(faceVertex, out ushort index))
                    {
                        // Position
                        var v = result.Vertices[faceVertex.VertexIndex - 1];
                        var position = new Vector3(v.X, v.Y, v.Z);

                        // UV
                        Vector2 uv = Vector2.Zero;
                        if (faceVertex.TextureIndex > 0 && faceVertex.TextureIndex <= result.Textures.Count)
                        {
                            var t = result.Textures[faceVertex.TextureIndex - 1];
                            uv = new Vector2(t.X, t.Y);
                        }
                        
                        // Normal
                        Vector3 normal = Vector3.UnitZ;
                        if (faceVertex.NormalIndex > 0 && faceVertex.NormalIndex <= result.Normals.Count)
                        {
                            var n = result.Normals[faceVertex.NormalIndex - 1];
                            normal = new Vector3(n.X, n.Y, n.Z);
                        }

                        positionList.Add(position);
                        uvList.Add(uv);
                        normalList.Add(normal);
                        index = (ushort)(positionList.Count - 1);
                        vertexDict[faceVertex] = index;
                    }

                    indexList.Add(index);
                }
            }
        }
        
        positions = positionList.ToArray();
        uvs = uvList.ToArray();
        normals = normalList.ToArray();
        indices = indexList.ToArray();

        return true;
    }
}