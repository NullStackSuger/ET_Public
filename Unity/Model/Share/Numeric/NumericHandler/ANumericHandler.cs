namespace ET;

public interface INumericHandler
{
    ETTask Handler(Entity entity, NumericChange args);
}

public abstract class ANumericHandler<T> : HandlerObject, INumericHandler where T : Entity
{
    protected abstract ETTask Run(T self, NumericChange args);
    
    public async ETTask Handler(Entity entity, NumericChange args)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        
        await Run(component, args);
    }
}