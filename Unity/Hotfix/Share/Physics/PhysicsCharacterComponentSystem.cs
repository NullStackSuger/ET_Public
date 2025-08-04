using System.Numerics;
using PhysX;

namespace ET;

[EntitySystemOf(typeof(PhysicsCharacterComponent))]
public static partial class PhysicsCharacterComponentSystem
{
    // UserControllerHitReport和ControllerBehaviorCallback 自己去实现, 我暂时用不到
    
    [EntitySystem]
    private static void Awake(this PhysicsCharacterComponent self, Material material, float radius, float halfHeight)
    {
        PhysicsSceneComponent scene = PhysicsSceneComponent.Main;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        self.controller = scene.controllerManager.CreateController<CapsuleController>(new CapsuleControllerDesc()
        {
            Position = transform.position,
            UpDirection = Vector3.Transform(Vector3.UnitY, transform.rotation),
            Radius = radius,
            Height = halfHeight * 2,
            Material = material,
        });
    }

    [EntitySystem]
    private static void Awake(this PhysicsCharacterComponent self, Material material, Vector3 halfSize)
    {
        PhysicsSceneComponent scene = PhysicsSceneComponent.Main;
        TransformComponent transform = self.Parent.GetComponent<TransformComponent>();
        self.controller = scene.controllerManager.CreateController<BoxController>(new BoxControllerDesc()
        {
            Position = transform.position,
            UpDirection = Vector3.Transform(Vector3.UnitY, transform.rotation),
            HalfSideExtent = halfSize.X,
            HalfHeight = halfSize.Y,
            HalfForwardExtent = halfSize.Z,
            Material = material,
        });
    }

    [EntitySystem]
    private static void Update(this PhysicsCharacterComponent self)
    {
        
    }

    [EntitySystem]
    private static void Serialize(this PhysicsCharacterComponent self)
    {
        
    }

    [EntitySystem]
    private static void Deserialize(this PhysicsCharacterComponent self)
    {
        
    }

    [EntitySystem]
    private static void Destroy(this PhysicsCharacterComponent self)
    {
        self.controller?.Dispose();
    }
}