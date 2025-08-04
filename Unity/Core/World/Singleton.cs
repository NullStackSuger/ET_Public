namespace ET;
    
public abstract class ASingleton: DisposeObject
{
    internal abstract void Register();
}
    
public abstract class Singleton<T>: ASingleton where T: Singleton<T>
{
    private bool isDisposed;

    [StaticField] 
    public static T Instance { get; private set; }

    internal override void Register()
    {
        Instance = (T)this;
    }

    public bool IsDisposed()
    {
        return this.isDisposed;
    }

    protected virtual void Destroy()
    {
            
    }

    public override void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }
            
        this.isDisposed = true;

        this.Destroy();
            
        Instance = null;
    }
}