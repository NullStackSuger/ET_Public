using System.Numerics;
using Assimp;
using Face = Assimp.Face;
using Quaternion = System.Numerics.Quaternion;

namespace ET.Client;

[EntitySystemOf(typeof(MeshComponent))]
public static partial class MeshComponentSystem
{
    [EntitySystem]
    private static void Awake(this MeshComponent self, MeshInfo a, Dictionary<Type, Type> b)
    {
        self.meshInfo = a;

        self.shaders = b;
            
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

    public static ViewObject Load(string path, Entity parent)
    {
        using var context = new AssimpContext();
        Assimp.Scene scene = context.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices);

        if (scene.HasMeshes)
        {
            List<Vector2> vec2Tmp = new();
            List<Vector3> vec3Tmp = new();
            List<Vector4> vec4Tmp = new();
            List<ushort> ushortTmp = new();
            
            Mesh mesh = scene.Meshes[0];
            ViewObject obj = parent.AddChild<ViewObject, string>(mesh.Name);

            // Transform
            scene.RootNode.FindNode(mesh.Name).Transform.Decompose(out Vector3D scale, out Assimp.Quaternion rotation, out Vector3D position);
            obj.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(position.ToVector3(), rotation.ToQuaternion(), scale.ToVector3());
            
            #region Mesh
            MeshInfo meshInfo = new MeshInfo();
            
            // Index
            foreach (Face face in mesh.Faces)
            {
                foreach (ushort index in face.Indices)
                {
                    ushortTmp.Add(index);
                }
            }
            meshInfo.indices = ushortTmp.ToArray();
            ushortTmp.Clear();

            // Position
            foreach (Vector3D vertex in mesh.Vertices)
            {
                vec3Tmp.Add(vertex.ToVector3());
            }

            meshInfo.positions = vec3Tmp.ToArray();
            vec3Tmp.Clear();

            // Normal
            foreach (Vector3D normal in mesh.Normals)
            {
                vec3Tmp.Add(normal.ToVector3());
            }

            meshInfo.normals = vec3Tmp.ToArray();
            vec3Tmp.Clear();

            // Tangent
            foreach (Vector3D tangent in mesh.Tangents)
            {
                vec3Tmp.Add(tangent.ToVector3());
            }

            meshInfo.tangents = vec3Tmp.ToArray();
            vec3Tmp.Clear();

            // Uv
            if (mesh.HasTextureCoords(0))
            {
                foreach (Vector3D uv in mesh.TextureCoordinateChannels[0])
                {
                    vec2Tmp.Add(uv.ToVector2());
                }

                meshInfo.uvs = vec2Tmp.ToArray();
                vec2Tmp.Clear();
            }

            // Color
            if (mesh.HasVertexColors(0))
            {
                foreach (Color4D color in mesh.VertexColorChannels[0])
                {
                    vec4Tmp.Add(color.ToVector4());
                }

                meshInfo.colors = vec4Tmp.ToArray();
                vec4Tmp.Clear();
            }

            // Material
            meshInfo.material = scene.Materials[mesh.MaterialIndex];
            
            // Mesh
            obj.AddComponent<MeshComponent, MeshInfo, Dictionary<Type, Type>>(meshInfo, new Dictionary<Type, Type>
            {
                { typeof(ShadowRenderPass), typeof(DefaultShadowShader) },
                { typeof(ShadingRenderPass), typeof(DefaultShadingShader) }
            }).AddToDirtyMesh();
            #endregion

            if (mesh.HasBones)
            {
                obj.AddComponent<JointComponent, List<Bone>, Assimp.Scene>(mesh.Bones, scene);
            }

            if (scene.HasAnimations)
            {
                obj.AddComponent<AnimatorComponent, List<Animation>>(scene.Animations);
            }
            
            return obj;
        }
        return null;
    }

    public static Vector3 ToVector3(this Vector3D v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }
    public static Vector3D ToVector3(this Vector3 v)
    {
        return new Vector3D(v.X, v.Y, v.Z);
    }
    public static Vector2 ToVector2(this Vector3D v)
    {
        return new Vector2(v.X, v.Y);
    }
    public static Vector2 ToVector2(this Vector2D v)
    {
        return new Vector2(v.X, v.Y);
    }
    public static Quaternion ToQuaternion(this Assimp.Quaternion q)
    {
        return new Quaternion(q.X, q.Y, q.Z, q.W);
    }
    public static Vector4 ToVector4(this Assimp.Color4D c)
    {
        return new Vector4(c.R, c.G, c.B, c.A);
    }
    public static System.Numerics.Matrix4x4 ToMatrix4x4(this Assimp.Matrix4x4 mat)
    {
        return new System.Numerics.Matrix4x4(mat.A1, mat.A2, mat.A3, mat.A4, mat.B1, mat.B2, mat.B3, mat.B4, mat.C1, mat.C2, mat.C3, mat.C4, mat.D1, mat.D2, mat.D3, mat.D4);
    }
    public static Assimp.Matrix4x4 ToMatrix4x4(this System.Numerics.Matrix4x4 mat)
    {
        return new Assimp.Matrix4x4(mat.M11, mat.M21, mat.M31, mat.M41, mat.M12, mat.M22, mat.M32, mat.M42, mat.M13, mat.M23, mat.M33, mat.M43, mat.M14, mat.M24, mat.M34, mat.M44);
    }
}