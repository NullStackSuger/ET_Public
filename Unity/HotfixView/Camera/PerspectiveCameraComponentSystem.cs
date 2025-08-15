using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(PerspectiveCameraComponent))]
public static partial class PerspectiveCameraComponentSystem
{
    [EntitySystem]
    private static void Awake(this PerspectiveCameraComponent self, float fovY, float aspect, float near, float far)
    {
        self.fovY = fovY;
        self.aspect = aspect;
        self.near = near;
        self.far = far;
    }

    public static ViewObject ViewObject(this PerspectiveCameraComponent self)
    {
        return self.GetParent<ViewObject>();
    }

    public static Matrix4x4 View(this PerspectiveCameraComponent self)
    {
        if (!self.ViewObject().GetComponent(out TransformComponent transform))
        {
            return Matrix4x4.Identity;
        }
        
        Vector3 zAxis = Vector3.Normalize(transform.Forward);
        Vector3 xAxis = Vector3.Normalize(Vector3.Cross(transform.Up, zAxis));
        Vector3 yAxis = Vector3.Cross(zAxis, xAxis);
        
        return new Matrix4x4
        (
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3.Dot(xAxis, transform.localPosition), -Vector3.Dot(yAxis, transform.localPosition), -Vector3.Dot(zAxis, transform.localPosition), 1
        );
    }
    
    public static Matrix4x4 Projection(this PerspectiveCameraComponent self)
    {
        float tanHalfFovY = MathF.Tan(self.fovY * MathHelper.Deg2Rad / 2.0f);
        Matrix4x4 mat = new Matrix4x4
        {
            [0, 0] = 1.0f / (self.aspect * tanHalfFovY),
            [1, 1] = 1.0f / tanHalfFovY,
            [2, 2] = self.far / (self.far - self.near),
            [2, 3] = 1.0f,
            [3, 2] = -(self.far * self.near) / (self.far - self.near)
        };
        return mat;
    }

    public static Frustum Frustum(this PerspectiveCameraComponent self)
    {
        return new Frustum(self.Projection() * self.View());
    }
}