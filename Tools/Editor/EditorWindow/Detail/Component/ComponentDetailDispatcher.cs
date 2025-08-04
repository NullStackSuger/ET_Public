using ET.Client;

namespace ET.Editor;

[Code]
public class ComponentDetailDispatcher : Singleton<ComponentDetailDispatcher>, ISingletonAwake
{
    private readonly Dictionary<Type, IComponentDetailHandler> handlers = new();
    public IComponentDetailHandler this[Type type] => this.handlers.TryGetValue(type, out var handler) ? handler : null;

    public void Handler(DetailEditorComponent self, ViewObject viewObject, Entity component)
    {
        if (handlers.TryGetValue(component.GetType(), out var handler))
        {
            handler.Handler(self, viewObject, component);
        }
        // 使用默认的显示
        else
        {
            Default_ComponentDetailHandler.DrawEntity(component);
        }
    }
    
    public void Awake()
    {
        var types = CodeTypes.Instance.GetTypes(typeof(AComponentDetailAttribute));
        foreach (Type type in types)
        {
            IComponentDetailHandler handler = Activator.CreateInstance(type) as IComponentDetailHandler;
            if (handler == null)
            {
                Log.Instance.Error($"Type is not IComponentDetailHandler: {type.Name}");
                continue;
            }
            
            object[] attrs = type.GetCustomAttributes(typeof(AComponentDetailAttribute), false);
            foreach (AComponentDetailAttribute attr in attrs)
            {
                handlers.Add(attr.Type, handler);
            }
        }
    }
}