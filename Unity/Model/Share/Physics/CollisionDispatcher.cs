namespace ET;

[Code]
public class CollisionDispatcher : Singleton<CollisionDispatcher>, ISingletonAwake
{
    private readonly Dictionary<Type, ICollisionHandler> handlers = new();

    public ICollisionHandler this[Type type]
    {
        get
        {
            if (type == null)
            {
                return null;
            }
            else
            {
                return this.handlers.TryGetValue(type, out var handler) ? handler : null;
            }
        }
    }
    
    public void Awake()
    {
        var types = CodeTypes.Instance.GetTypes(typeof(ACollisionAttribute));
        foreach (Type type in types)
        {
            ICollisionHandler handler = Activator.CreateInstance(type) as ICollisionHandler;
            if (handler == null)
            {
                Log.Instance.Error($"Type is not ICollisionHandler: {type.Name}");
                continue;
            }
            
            handlers.Add(type, handler);
        }
    }
}