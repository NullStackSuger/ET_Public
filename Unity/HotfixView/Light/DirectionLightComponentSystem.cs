using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(DirectionLightComponent))]
public static partial class DirectionLightComponentSystem
{
    [EntitySystem]
    private static void Awake(this DirectionLightComponent self, float a, Color b, OrthographicCameraComponent c)
    {
        self.intensity = a;
        self.color = b;
        self.camera = c;
    }
    
    public static ViewObject ViewObject(this DirectionLightComponent self)
    {
        return self.GetParent<ViewObject>();
    }

    public static Matrix4x4 View(this DirectionLightComponent self)
    {
        OrthographicCameraComponent camera = self.camera;
        if (camera == null)
        {
            return Matrix4x4.Identity;
        }
        else
        {
            return camera.View();
        }
    }
    
    public static Matrix4x4 Projection(this DirectionLightComponent self)
    {
        OrthographicCameraComponent camera = self.camera;
        if (camera == null)
        {
            return Matrix4x4.Identity;
        }
        else
        {
            return camera.Projection();
        }
    }
    
    public static Frustum Frustum(this DirectionLightComponent self)
    {
        return new Frustum(self.Projection() * self.View());
    }
}