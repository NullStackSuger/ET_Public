using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(TransformComponent))]
public static partial class TransformComponentSystem
{
    [EntitySystem]
    private static void Awake(this TransformComponent self, Vector3 a, Quaternion b, Vector3 c)
    {
        self.position = a;
        self.rotation = b;
        self.scale = c;
    }

    [EntitySystem]
    private static void Awake(this TransformComponent self, Vector3 a)
    {
        self.position = a;
        self.rotation = Quaternion.Identity;
        self.scale = Vector3.One;
    }

    public static Vector3 Forward(this TransformComponent self)
    {
        return Vector3.Transform(Vector3.UnitZ, Quaternion.Normalize(self.rotation));
    }
    public static Vector3 Up(this TransformComponent self)
    {
        return Vector3.Transform(Vector3.UnitY, Quaternion.Normalize(self.rotation));
    }

    public static Matrix4x4 Local(this TransformComponent self)
    {
        return Matrix4x4.CreateScale(self.scale) * Matrix4x4.CreateFromQuaternion(self.rotation) * Matrix4x4.CreateTranslation(self.position);
    }
    public static Matrix4x4 World(this TransformComponent self)
    {
        Matrix4x4 matrix = Matrix4x4.Identity;
        Entity current = self.Parent.Parent;
        while (current != null)
        {
            if (current.GetComponent(out TransformComponent transform))
            {
                matrix *= transform.Local();
            }
            current = current.Parent;
        }
        return matrix;
    }
    public static Matrix4x4 Model(this TransformComponent self)
    {
        return self.Local() * self.World();
    }

    public static Vector3 GetWorldPosition(this TransformComponent self)
    {
        return Vector3.Transform(Vector3.Zero, self.Model());   
    }
    public static void SetWorldPosition(this TransformComponent self, Vector3 position)
    {
        Matrix4x4 world = self.World();
        if (!Matrix4x4.Invert(world, out Matrix4x4 invWorld))
        {
            Log.Instance.Error(new InvalidOperationException("无法求父对象的逆矩阵 可能是缩放为0或矩阵不可逆"));
        }
        Vector3 localPos = Vector3.Transform(position, invWorld);
        self.position = localPos;
    }
}