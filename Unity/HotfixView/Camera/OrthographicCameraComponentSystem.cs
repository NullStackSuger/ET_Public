using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(OrthographicCameraComponent))]
public static partial class OrthographicCameraComponentSystem
{
    [EntitySystem]
    private static void Awake(this OrthographicCameraComponent self, float aspect, float width, float near, float far)
    {
        float height = width / aspect;
            
        self.left = -width / 2;
        self.right = width / 2;
        self.bottom = -height / 2;
        self.top = height / 2;
        self.near = near;
        self.far = far;
    }
    
    public static ViewObject ViewObject(this OrthographicCameraComponent self)
    {
        return self.GetParent<ViewObject>();
    }
    
    public static Matrix4x4 View(this OrthographicCameraComponent self)
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
    
    public static Matrix4x4 Projection(this OrthographicCameraComponent self)
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat.M11 = 2.0f / (self.right - self.left);
        mat.M22 = 2.0f / (self.top - self.bottom);
        mat.M33 = 1.0f / (self.far - self.near);
        mat.M41 = -(self.right + self.left) / (self.right - self.left);
        mat.M42 = -(self.top + self.bottom) / (self.top - self.bottom);
        mat.M43 = -self.near / (self.far - self.near);
        return mat;
    }
    
    public static Frustum Frustum(this OrthographicCameraComponent self)
    {
        return new Frustum(self.Projection() * self.View());
    }
}