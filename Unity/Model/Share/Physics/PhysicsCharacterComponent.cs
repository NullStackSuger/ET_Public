using System.Numerics;
using PhysX;

namespace ET;

public class PhysicsCharacterComponent : Entity, IAwake<Material, float, float>, IAwake<Material, Vector3>, IUpdate, ISerialize, IDeserialize, IDestroy
{
    public Controller controller;
}