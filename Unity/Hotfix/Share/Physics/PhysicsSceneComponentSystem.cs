using System.Numerics;
using PhysX;

namespace ET;

[EntitySystemOf(typeof(PhysicsSceneComponent))]
public static partial class PhysicsSceneComponentSystem
{
    [EntitySystem]
    private static void Awake(this PhysicsSceneComponent self)
    {
        self.Awake(new Vector3(0, -9.81f, 0));
    }

    [EntitySystem]
    private static void Awake(this PhysicsSceneComponent self, Vector3 g)
    {
        SceneDesc desc = new()
        {
            Gravity = g,
            FilterShader = new ETSimulationFilterShader(),
            SimulationEventCallback = new ETSimulationEventCallback()
        };
        self.physicsScene = PhysicsComponent.Instance.physics.CreateScene(desc);
        self.controllerManager = self.physicsScene.CreateControllerManager();
        self.controllerManager.SetOverlapRecoveryModule(true);
        self.controllerManager.SetPreventVerticalSlidingAgainstCeiling(true);
        self.lastTime = Time.Instance.NowTime;
    }

    [EntitySystem]
    private static void Destroy(this PhysicsSceneComponent self)
    {
        self.physicsScene?.Dispose();
    }

    [EntitySystem]
    private static void Update(this PhysicsSceneComponent self)
    {
        long now = Time.Instance.NowTime;
        while ((now - self.lastTime) > PhysicsSceneComponent.TickRateMs)
        {
            self.lastTime = now;
            if (self.physicsScene == null) return;
            self.physicsScene.Simulate(PhysicsSceneComponent.TickRate);
            self.physicsScene.FetchResults(true);

            // 处理碰撞回调
            
            // Begin
            foreach (Actor actor in self.physicsScene.GetActors(ActorTypeFlag.RigidStatic | ActorTypeFlag.RigidDynamic))
            {
                if (!CheckHandler(actor, out Entity entity, out ICollisionHandler handler)) continue;
                handler.OnCollisionTestBegin(entity);
            }
            
            // 要注意弹性系数, 当我们认为2个物体接触时, 因为弹性的问题, 导致物体实际上是不断的跳起落下, 导致一直调用Start->Stay->Exit
            
            // 取Now差集是Enter
            var collisionEnter = self.nowCollision.Except(self.lastCollision);
            foreach (var (rigid0, rigid1) in collisionEnter)
            {
                if (CheckHandler(rigid0, out Entity entity0, out ICollisionHandler handler0))
                {
                    handler0.OnCollisionEnter(entity0, rigid1); // 这里不检查rigid1, 因为我认为它在回调过程中被删除是可以的, 我不确定对不对
                }
            }
                
            // 取交集是Stay
            var collisionStay = self.lastCollision.Intersect(self.nowCollision);
            foreach (var (rigid0, rigid1) in collisionStay)
            {
                if (CheckHandler(rigid0, out Entity entity0, out ICollisionHandler handler0))
                {
                    handler0.OnCollisionStay(entity0, rigid1);
                }
            }
                
            // 取Last差集是Exit
            var collisionExit = self.lastCollision.Except(self.nowCollision);
            foreach (var (rigid0, rigid1) in collisionExit)
            {
                if (CheckHandler(rigid0, out Entity entity0, out ICollisionHandler handler0))
                {
                    handler0.OnCollisionExit(entity0, rigid1);
                }
            }
            
            // TriggerEnter
            while (self.enterTriggers.TryDequeue(out var pair))
            {
                RigidActor rigid0 = pair.Item1;
                RigidActor rigid1 = pair.Item2;
                
                if (CheckHandler(rigid0, out Entity entity, out ICollisionHandler handler))
                {
                    handler.OnTriggerEnter(entity, rigid1);
                    self.activeTriggers.Add(pair);
                }
            }
            
            // TriggerStay
            foreach (var (rigid0, rigid1) in self.activeTriggers)
            {
                if (CheckHandler(rigid0, out Entity entity, out ICollisionHandler handler))
                {
                    handler.OnTriggerStay(entity, rigid1);
                }
            }
            
            // TriggerExit
            while (self.exitTriggers.TryDequeue(out var pair))
            {
                RigidActor rigid0 = pair.Item1;
                RigidActor rigid1 = pair.Item2;
                
                if (CheckHandler(rigid0, out Entity entity, out ICollisionHandler handler))
                {
                    handler.OnTriggerExit(entity, rigid1);
                    self.activeTriggers.Remove(pair);
                }
            }
            
            // End
            foreach (Actor actor in self.physicsScene.GetActors(ActorTypeFlag.RigidStatic | ActorTypeFlag.RigidDynamic))
            {
                if (!CheckHandler(actor, out Entity entity, out ICollisionHandler handler)) continue;
                handler.OnCollisionTestEnd(entity);
            }
            
            self.lastCollision.Clear();
            (self.lastCollision, self.nowCollision) = (self.nowCollision, self.lastCollision);
        }

        static bool CheckHandler(Actor self, out Entity entity, out ICollisionHandler handler)
        {
            entity = null;
            handler = null;
            
            if (self.UserData is not PhysicsRigidActorComponent rigidComponent) return false;

            entity = rigidComponent.Parent;
            handler = CollisionDispatcher.Instance[rigidComponent.callback];
            if (handler == null) return false;
            handler.OnCollisionTestBegin(entity);
            return true;
        }
    }

    [EntitySystem]
    private static void Serialize(this PhysicsSceneComponent self)
    {
        
    }

    [EntitySystem]
    private static void Deserialize(this PhysicsSceneComponent self)
    {
        self.Awake();
    }
}