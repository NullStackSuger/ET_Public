namespace ET.Client;

public class PerspectiveCameraComponent : Entity, IAwake<float, float, float, float>, ISerialize, IDeserialize
{
    public float fovY;
    public float aspect;
    public float near;
    public float far;
    
    [StaticField]
    public static EntityRef<PerspectiveCameraComponent> Main;
}