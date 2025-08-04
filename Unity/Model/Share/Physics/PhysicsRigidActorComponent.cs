using System.Numerics;
using PhysX;

namespace ET;

public class PhysicsRigidActorComponent : Entity, ISerialize, IDeserialize, IDestroy,
    IAwake<RigidActor>,
    IAwake<Vector3, float, Material, float>, IAwake<Vector3, Material, float>, // Sphere
    IAwake<Vector3, float, Material, Vector3>, IAwake<Vector3, Material, Vector3>, // Box
    IAwake<Vector3, float, Material, float, float>, IAwake<Vector3, Material, float, float>, // Capsule
    IAwake<Vector3, Material>, // Plane
    IAwake<Vector3, float>, IAwake<Vector3, Vector3>, IAwake<Vector3, float, float> // Trigger
{
    public RigidActor rigid;
    public Type callback;
}