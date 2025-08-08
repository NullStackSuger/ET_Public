using System.Numerics;
using Assimp;

namespace ET.Client;

public class MeshInfo
{
    public ushort[] indices;
    public Vector3[] positions;
    public Vector3[] normals;
    public Vector3[] tangents;
    public Vector2[] uvs;
    public Vector4[] colors;
    public Material material;
}