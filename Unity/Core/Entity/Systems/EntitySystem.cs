namespace ET;

public class EntitySystem
{
    private readonly Dictionary<Type, Queue<EntityRef<Entity>>> queues = new();
        
    public Queue<EntityRef<Entity>> GetQueue(Type type)
    {
        if (!this.queues.TryGetValue(type, out var queue))
        {
            queue = new Queue<EntityRef<Entity>>();
            this.queues.Add(type, queue);
        }

        return queue;
    }
        
    public virtual void RegisterSystem(Entity component)
    {
        Type type = component.GetType();

        TypeSystems.OneTypeSystems oneTypeSystems = EntitySystemSingleton.Instance.TypeSystems.GetOneTypeSystems(type);
        if (oneTypeSystems == null)
        {
            return;
        }

        foreach (Type queueType in oneTypeSystems.ClassType)
        {
            var queue = this.GetQueue(queueType);
            queue.Enqueue(component);
        }
    }
        
    public void Publish<T>(T t) where T: struct
    {
        Type systemType = typeof(AClassEventSystem<T>);
        Queue<EntityRef<Entity>> queue = this.GetQueue(systemType);
        int count = queue.Count;
        while (count-- > 0)
        {
            Entity entity = queue.Dequeue();
            if (entity == null)
            {
                continue;
            }
            if (entity.IsDisposed)
            {
                continue;
            }
            if (entity is not IClassEvent<T>)
            {
                continue;
            }
            try
            {
                List<SystemObject> systems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(entity.GetType(), systemType);
                if (systems == null)
                {
                    continue;
                }

                queue.Enqueue(entity);

                foreach (AClassEventSystem<T> classSystem in systems)
                {
                    try
                    {
                        classSystem.Run(entity, t);
                    }
                    catch (Exception e)
                    {
                        Log.Instance.Error(e);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"entity system update fail: {entity.GetType().FullName}", e);
            }

        }
    }
}