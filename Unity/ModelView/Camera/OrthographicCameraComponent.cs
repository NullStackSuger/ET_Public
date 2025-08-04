namespace ET.Client;

public class OrthographicCameraComponent : Entity, IAwake<float, float, float, float>, ISerialize, IDeserialize
{
    public float left;
    public float right;
    public float bottom;
    public float top;
    public float near;
    public float far;
}