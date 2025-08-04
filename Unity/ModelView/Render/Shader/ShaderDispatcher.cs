namespace ET.Client;

[Code]
public class ShaderDispatcher : Singleton<ShaderDispatcher>, ISingletonAwake
{
    private readonly Dictionary<Type, AShaderHandler> handlers = new();
    public AShaderHandler this[Type type] => this.handlers.TryGetValue(type, out var handler) ? handler : null;
    
    public void Awake()
    {
        var types = CodeTypes.Instance.GetTypes(typeof(AShaderAttribute));
        foreach (Type type in types)
        {
            AShaderHandler handler = Activator.CreateInstance(type) as AShaderHandler;
            if (handler == null)
            {
                Log.Instance.Error($"Type is not AShaderHandler: {type.Name}");
                continue;
            }
            
            object[] attrs = type.GetCustomAttributes(typeof(AShaderAttribute), false);
            foreach (AShaderAttribute attr in attrs)
            {
                handlers.Add(attr.Type, handler);
            }
        }
    }
}