namespace ET;

[Code]
public class NumericHandlerDispatcher : Singleton<NumericHandlerDispatcher>, ISingletonAwake
{
    private readonly MultiDictionary<Type, (int, int), List<INumericHandler>> handlers = new();
    
    public void Awake()
    {
        var types = CodeTypes.Instance.GetTypes(typeof(ANumericHandlerAttribute));
        foreach (var type in types)
        {
            INumericHandler handler = Activator.CreateInstance(type) as INumericHandler;
            if (handler == null)
            {
                Log.Instance.Error($"Type is not INumericHandler: {type.Name}");
                continue;
            }
            
            // 1.EntityType
            Type entityType = type.BaseType?.GetGenericArguments()[0];
            if (entityType == null)
            {
                Log.Instance.Error($"Type is not INumericHandler: {type.Name}");
                continue;
            }
            
            object[] attrs = type.GetCustomAttributes(typeof(ANumericHandlerAttribute), false);
            foreach (ANumericHandlerAttribute attr in attrs)
            {
                // 2.SceneType
                int sceneType = attr.SceneType;
                // 3.NumericType
                int numericType = attr.NumericType;

                if (!handlers.TryGetValue(entityType, (sceneType, numericType), out List<INumericHandler> list))
                {
                    list = [];
                }
                list.Add(handler);
            }
        }
    }

    public async ETTask Run(NumericChange args)
    {
        Entity entity = args.entity;
        if (entity == null) return;
        Type entityType = entity.GetType();
        int sceneType = entity.IScene.SceneType;
        int numericType = args.numericType;

        if (!handlers.ContainSubKey(entityType, (sceneType, numericType)))
        {
            if (handlers.ContainSubKey(entityType, (0, numericType)))
            {
                sceneType = 0;
            }
            else
            {
                return;
            }
        }
        handlers.TryGetValue(entityType, (sceneType, numericType), out List<INumericHandler> list);
        
        using ListComponent<ETTask> tasks = ListComponent<ETTask>.Create();
        foreach (INumericHandler handler in list)
        {
            tasks.Add(handler.Handler(entity, args));
        }

        try
        {
            await ETTaskHelper.WaitAll(tasks);
        }
        catch (Exception e)
        {
            Log.Instance.Error(e);
        }
    }
}