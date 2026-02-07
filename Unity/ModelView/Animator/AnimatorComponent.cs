using Assimp;

namespace ET.Client;

public class AnimatorComponent : Entity, IAwake<List<Animation>>, ISerialize, IDeserialize, ILateUpdate
{
    public Dictionary<string, Animation> animations;
    
    public ShadingVertex[] vertices;
    public string currentName = "";
    public float currentTime;
}