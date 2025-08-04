namespace ET.Client;

[Code]
public class RenderPassDispatcher : Singleton<RenderPassDispatcher>, ISingletonAwake
{
    private readonly Dictionary<Type, ARenderPassHandler> handlers = new();
    public ARenderPassHandler this[Type type] => this.handlers.TryGetValue(type, out var handler) ? handler : null;

    public void Awake()
    {
        var types = CodeTypes.Instance.GetTypes(typeof(ARenderPassAttribute));
        foreach (Type type in types)
        {
            ARenderPassHandler handler = Activator.CreateInstance(type) as ARenderPassHandler;
            if (handler == null)
            {
                Log.Instance.Error($"Type is not ARenderPassHandler: {type.Name}");
                continue;
            }
            handlers.Add(type, handler);
        }
    }
}