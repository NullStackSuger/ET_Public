using System.Numerics;
using Assimp;
using Face = Assimp.Face;
using Matrix4x4 = Assimp.Matrix4x4;

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
            obj.AddComponent<TransformComponent, Vector3, System.Numerics.Quaternion, Vector3>(position.ToVector3(), rotation.ToQuaternion(), scale.ToVector3());
            
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
            });
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
    public static Matrix4x4 FromQuaternion(this Assimp.Quaternion q)
    {
        // 标准化四元数
        float length = MathF.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
        if (length > 0)
        {
            q.X /= length;
            q.Y /= length;
            q.Z /= length;
            q.W /= length;
        }
    
        // 四元数到旋转矩阵的转换（列主序）
        float xx = q.X * q.X;
        float xy = q.X * q.Y;
        float xz = q.X * q.Z;
        float xw = q.X * q.W;
        float yy = q.Y * q.Y;
        float yz = q.Y * q.Z;
        float yw = q.Y * q.W;
        float zz = q.Z * q.Z;
        float zw = q.Z * q.W;
    
        return new Matrix4x4(
            1.0f - 2.0f * (yy + zz), 2.0f * (xy - zw), 2.0f * (xz + yw), 0.0f,
            2.0f * (xy + zw), 1.0f - 2.0f * (xx + zz), 2.0f * (yz - xw), 0.0f,
            2.0f * (xz - yw), 2.0f * (yz + xw), 1.0f - 2.0f * (xx + yy), 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
    }
    public static System.Numerics.Quaternion ToQuaternion(this Assimp.Quaternion q)
    {
        return new System.Numerics.Quaternion(q.X, q.Y, q.Z, q.W);
    }
    public static Assimp.Quaternion ToQuaternion(this System.Numerics.Quaternion q)
    {
        return new Assimp.Quaternion(q.W, q.X, q.Y, q.Z);
    }
    public static Vector4 ToVector4(this Color4D c)
    {
        return new Vector4(c.R, c.G, c.B, c.A);
    }
}