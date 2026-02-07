namespace ET.Client;

public class ParticleComponent : Entity, IAwake<float, string>, ISerialize, IDeserialize, ILateUpdate
{
    public float time;
    public string resourcesPath;
    public PriorityQueue<EntityRef<ViewObject>, long> objs = new();
}