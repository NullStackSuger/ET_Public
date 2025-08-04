using PhysX;

namespace ET.Client;

public class DefaultCollisionHandler : ACollisionHandler<ViewObject>
{
    protected override void OnCollisionEnter(ViewObject self, RigidActor other)
    {
        base.OnCollisionEnter(self, other);
        
        if (other is RigidStatic)
            Log.Instance.Warning("Enter");
    }

    protected override void OnCollisionExit(ViewObject self, RigidActor other)
    {
        base.OnCollisionExit(self, other);
        
        if (other is RigidStatic)
            Log.Instance.Warning("Exit");
    }

    protected override void OnCollisionStay(ViewObject self, RigidActor other)
    {
        base.OnCollisionStay(self, other);
        
        if (other is RigidStatic)
            Log.Instance.Warning("Stay");
    }

    protected override void OnTriggerEnter(ViewObject self, RigidActor other)
    {
        base.OnTriggerEnter(self, other);
        
        if (other is RigidDynamic)
            Log.Instance.Warning("Trigger Enter");
    }

    protected override void OnTriggerStay(ViewObject self, RigidActor other)
    {
        base.OnTriggerStay(self, other);
        
        if (other is RigidDynamic)
            Log.Instance.Warning("Trigger Stay");
    }

    protected override void OnTriggerExit(ViewObject self, RigidActor other)
    {
        base.OnTriggerExit(self, other);
        
        if (other is RigidDynamic)
            Log.Instance.Warning("Trigger Exit");
    }
}