using System.Numerics;
using PhysX;

namespace ET;

[EntitySystemOf(typeof(PhysicsRigidActorComponent))]
public static partial class PhysicsRigidActorComponentSystem
{
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, RigidActor rigid)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, float mass, Material material, float radius)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidDynamic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new SphereGeometry(radius), material);
        shape.LocalPosePosition = offset;
        rigid.SetMassAndUpdateInertia(mass);
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
        
    }
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, Material material, float radius)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new SphereGeometry(radius), material);
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }

    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, float mass, Material material, Vector3 halfExtend)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidDynamic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new BoxGeometry(halfExtend), material);
        shape.LocalPosePosition = offset;
        rigid.SetMassAndUpdateInertia(mass);
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, Material material, Vector3 halfExtend)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new BoxGeometry(halfExtend), material);
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, float mass, Material material, float radius, float halfHeight)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidDynamic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new CapsuleGeometry(radius, halfHeight), material);
        shape.LocalPosePosition = offset;
        rigid.SetMassAndUpdateInertia(mass);
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
        
        rigid.AddForce(new Vector3(0, 1000, 0), ForceMode.Force, true);
    }
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, Material material, float radius, float halfHeight)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new CapsuleGeometry(radius, halfHeight), material);
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, Material material)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new PlaneGeometry(), material);
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }

    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, float radius)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new SphereGeometry(radius), PhysicsComponent.Instance.staticMaterial);
        Console.WriteLine(shape.Flags);
        shape.Flags &= ~ShapeFlag.SimulationShape;
        shape.Flags &= ~ShapeFlag.SceneQueryShape;
        shape.Flags |= ShapeFlag.TriggerShape;
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, Vector3 halfExtend)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new BoxGeometry(halfExtend), PhysicsComponent.Instance.staticMaterial);
        shape.Flags &= ~ShapeFlag.SimulationShape;
        shape.Flags &= ~ShapeFlag.SceneQueryShape;
        shape.Flags |= ShapeFlag.TriggerShape;
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    
    [EntitySystem]
    private static void Awake(this PhysicsRigidActorComponent self, Vector3 offset, float radius, float halfHeight)
    {
        PhysicsSceneComponent physicsScene = PhysicsSceneComponent.Main;
        var rigid = PhysicsComponent.Instance.physics.CreateRigidStatic();
        var shape = RigidActorExt.CreateExclusiveShape(rigid, new CapsuleGeometry(radius, halfHeight), PhysicsComponent.Instance.staticMaterial);
        shape.Flags &= ~ShapeFlag.SimulationShape;
        shape.Flags &= ~ShapeFlag.SceneQueryShape;
        shape.Flags |= ShapeFlag.TriggerShape;
        shape.LocalPosePosition = offset;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        rigid.GlobalPose = new Transform(transform.rotation, transform.position).ToMatrix();
        rigid.UserData = self;
        physicsScene.physicsScene.AddActor(rigid);
        self.rigid = rigid;
    }
    
    // TODO 物理系统/如何用Bson序列化
    // 可能需要自己写BSON 序列化器, 这个我不了解 之后再说
    [EntitySystem]
    private static void Serialize(this PhysicsRigidActorComponent self)
    {
        
    }

    [EntitySystem]
    private static void Deserialize(this PhysicsRigidActorComponent self)
    {
        
    }

    [EntitySystem]
    private static void Destroy(this PhysicsRigidActorComponent self)
    {
        self.rigid?.Dispose();
    }
}