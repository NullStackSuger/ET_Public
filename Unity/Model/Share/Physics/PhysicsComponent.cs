using PhysX;

namespace ET;

[Code]
public class PhysicsComponent : Singleton<PhysicsComponent>, ISingletonAwake
{
    public Physics physics;
    
    public Material staticMaterial;
    public void Awake()
    {
        physics = new Physics(new Foundation(new ConsoleErrorCallback()), checkRuntimeFiles: true);
        staticMaterial = physics.CreateMaterial(0, 0, 0);
    }

    protected override void Destroy()
    {
        base.Destroy();
        
        physics?.Dispose();
    }
}