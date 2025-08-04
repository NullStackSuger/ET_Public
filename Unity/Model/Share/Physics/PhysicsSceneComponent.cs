using System.Numerics;
using PhysX;

namespace ET;

public class PhysicsSceneComponent : Entity, IAwake, IAwake<Vector3>, IDestroy, IUpdate, ISerialize, IDeserialize
{
    public PhysX.Scene physicsScene;
    public ControllerManager controllerManager;
    
    public long lastTime;
    public const float TickRate = 0.1f;
    public const float TickRateMs = TickRate * 1000f;

    public List<(RigidActor, RigidActor)> lastCollision = new();
    public List<(RigidActor, RigidActor)> nowCollision = new();

    public Queue<(RigidActor, RigidActor)> enterTriggers = new();
    public Queue<(RigidActor, RigidActor)> exitTriggers = new();
    public HashSet<(RigidActor, RigidActor)> activeTriggers = new();
    
    
    [StaticField]
    public static EntityRef<PhysicsSceneComponent> Main;
}