using System.Numerics;

namespace ET;

public class TransformComponent : Entity, IAwake<Vector3, Quaternion, Vector3>, IAwake<Vector3>, ISerialize, IDeserialize
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;

    public Vector3 worldPosition
    {
        set
        {
            Matrix4x4 world = World;
            if (!Matrix4x4.Invert(world, out Matrix4x4 invWorld))
            {
                Log.Instance.Error(new InvalidOperationException("无法求父对象的逆矩阵 可能是缩放为0或矩阵不可逆"));
            }
            Vector3 localPos = Vector3.Transform(value, invWorld);
            localPosition = localPos;
        }
        get => Vector3.Transform(Vector3.Zero, Model);
    }

    public Matrix4x4 Local => Matrix4x4.CreateScale(localScale) * 
                              Matrix4x4.CreateFromQuaternion(localRotation) * 
                              Matrix4x4.CreateTranslation(localPosition);
    public Matrix4x4 World
    {
        get
        {
            Matrix4x4 matrix = Matrix4x4.Identity;
            Entity obj = this.Parent;
            Entity current = obj.Parent;
            while (current != null)
            {
                if (current.GetComponent(out TransformComponent transform))
                {
                    matrix *= transform.Local;
                }
                current = current.Parent;
            }
            return matrix;
        }
    }
    public Matrix4x4 Model => Local * World;

    public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Quaternion.Normalize(localRotation));
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Quaternion.Normalize(localRotation));
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Quaternion.Normalize(localRotation));
}