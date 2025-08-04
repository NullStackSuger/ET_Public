using PhysX;

namespace ET;

public interface ICollisionHandler
{
    void OnCollisionTestBegin(Entity entity);
    void OnCollisionTestEnd(Entity entity);
    
    void OnCollisionEnter(Entity entity, RigidActor other);
    void OnCollisionExit(Entity entity, RigidActor other);
    void OnCollisionStay(Entity entity, RigidActor other);
    
    void OnTriggerEnter(Entity entity, RigidActor other);
    void OnTriggerExit(Entity entity, RigidActor other);
    void OnTriggerStay(Entity entity, RigidActor other);
}

[ACollision]
public abstract class ACollisionHandler<T> : HandlerObject, ICollisionHandler where T : Entity
{
    public void OnCollisionTestBegin(Entity entity)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnCollisionTestBegin(component);
    }
    protected virtual void OnCollisionTestBegin(T self){}
    
    public void OnCollisionTestEnd(Entity entity)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnCollisionTestBegin(component);
    }
    protected virtual void OnCollisionTestEnd(T self){}
    
    public void OnCollisionEnter(Entity entity, RigidActor other)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnCollisionEnter(component, other);
    }
    protected virtual void OnCollisionEnter(T self, RigidActor other){}

    public void OnCollisionExit(Entity entity, RigidActor other)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnCollisionExit(component, other);
    }
    protected virtual void OnCollisionExit(T self, RigidActor other){}

    public void OnCollisionStay(Entity entity, RigidActor other)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnCollisionStay(component, other);
    }
    protected virtual void OnCollisionStay(T self, RigidActor other){}

    public void OnTriggerEnter(Entity entity, RigidActor other)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnTriggerEnter(component, other);
    }
    protected virtual void OnTriggerEnter(T self, RigidActor other){}

    public void OnTriggerExit(Entity entity, RigidActor other)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnTriggerExit(component, other);
    }
    protected virtual void OnTriggerExit(T self, RigidActor other){}

    public void OnTriggerStay(Entity entity, RigidActor other)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        OnTriggerStay(component, other);
    }
    protected virtual void OnTriggerStay(T self, RigidActor other){}
}