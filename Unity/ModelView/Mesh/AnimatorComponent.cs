using System.Numerics;
using Assimp;

namespace ET.Client;

public class AnimatorComponent : Entity, IAwake<List<Animation>>, ISerialize, IDeserialize, ILateUpdate
{
    public Dictionary<string, Animation> animations;
    public float tmp;
    public Vector3[] positions;
}