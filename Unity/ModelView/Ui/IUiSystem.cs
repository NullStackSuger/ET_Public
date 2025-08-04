namespace ET;

public interface IUi
{
}

public interface IUiSystem : ISystemType
{
    void Run(Entity o);
}

[EntitySystem]
public abstract class UiSystem<T> : SystemObject, IUiSystem where T : Entity, IUi
{
    public void Run(Entity o)
    {
        this.Ui((T)o);
    }
    
    public Type SystemType()
    {
        return typeof(IUiSystem);
    }
    
    public Type Type()
    {
        return typeof(T);
    }

    protected abstract void Ui(T self);
}