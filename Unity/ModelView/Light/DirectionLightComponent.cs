namespace ET.Client;

public class DirectionLightComponent : Entity, IAwake<float, Color, OrthographicCameraComponent>, ISerialize, IDeserialize
{
    public float intensity;
    public Color color;
    public EntityRef<OrthographicCameraComponent> camera;

    [StaticField]
    public static EntityRef<DirectionLightComponent> Main;
}