namespace ET.Editor;

[Code]
public class FileDetailDispatcher : Singleton<FileDetailDispatcher>, ISingletonAwake
{
    private readonly Dictionary<string, AFileDetailHandler> handlers = new();

    public AFileDetailHandler this[string extension] => this.handlers.TryGetValue(extension, out var handler) ? handler : null;

    public void Awake()
    {
        var types = CodeTypes.Instance.GetTypes(typeof(AFileDetailAttribute));
        foreach (Type type in types)
        {
            AFileDetailHandler handler = Activator.CreateInstance(type) as AFileDetailHandler;
            if (handler == null)
            {
                Log.Instance.Error($"Type is not AFileDetailHandler: {type.Name}");
                continue;
            }
            
            object[] attrs = type.GetCustomAttributes(typeof(AFileDetailAttribute), false);
            foreach (AFileDetailAttribute attr in attrs)
            {
                handlers.Add(attr.Extension, handler);
            }
        }
    }
}