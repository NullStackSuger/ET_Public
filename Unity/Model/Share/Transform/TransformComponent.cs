using System.Numerics;

namespace ET;

public class TransformComponent : Entity, IAwake<Vector3, Quaternion, Vector3>, IAwake<Vector3>, ISerialize, IDeserialize
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}