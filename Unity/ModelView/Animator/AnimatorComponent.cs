using System.Numerics;
using Assimp;

namespace ET.Client;

public class AnimatorComponent : Entity, IAwake<List<Animation>>, ISerialize, IDeserialize, ILateUpdate
{
    public Dictionary<string, Animation> animations;
    
    public List<Vector3> positions;
    public string currentName = "";
    public float currentTime;
}