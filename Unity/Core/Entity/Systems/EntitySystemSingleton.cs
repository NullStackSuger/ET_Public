namespace ET;

[Code]
public class EntitySystemSingleton: Singleton<EntitySystemSingleton>, ISingletonAwake
{
    public TypeSystems TypeSystems { get; private set; }
        
    public void Awake()
    {
        this.TypeSystems = new TypeSystems();

        foreach (Type type in CodeTypes.Instance.GetTypes(typeof (EntitySystemAttribute)))
        {
            SystemObject obj = (SystemObject)Activator.CreateInstance(type);

            if (obj is ISystemType iSystemType)
            {
                TypeSystems.OneTypeSystems oneTypeSystems = this.TypeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                oneTypeSystems.Map.Add(iSystemType.SystemType(), obj);
                    
                if (iSystemType is IClassEventSystem)
                {
                    oneTypeSystems.ClassType.Add(iSystemType.SystemType());
                }
            }
        }
    }
        
    public void Serialize(Entity entity)
    {
        if (entity is not ISerialize)
        {
            return;
        }
            
        List<SystemObject> iSerializeSystems = this.TypeSystems.GetSystems(entity.GetType(), typeof (ISerializeSystem));
        if (iSerializeSystems == null)
        {
            return;
        }

        foreach (ISerializeSystem serializeSystem in iSerializeSystems)
        {
            if (serializeSystem == null)
            {
                continue;
            }

            try
            {
                serializeSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }
        
    public void Deserialize(Entity entity)
    {
        if (entity is not IDeserialize)
        {
            return;
        }
            
        List<SystemObject> iDeserializeSystems = this.TypeSystems.GetSystems(entity.GetType(), typeof (IDeserializeSystem));
        if (iDeserializeSystems == null)
        {
            return;
        }

        foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
        {
            if (deserializeSystem == null)
            {
                continue;
            }

            try
            {
                deserializeSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }
        
    public void Awake(Entity entity)
    {
        if (entity is not IAwake)
        {
            return;
        }
            
        List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(entity.GetType(), typeof (IAwakeSystem));
        if (iAwakeSystems == null)
        {
            return;
        }

        foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
        {
            if (aAwakeSystem == null)
            {
                continue;
            }

            try
            {
                aAwakeSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }

    public void Awake<P1>(Entity component, P1 p1)
    {
        if (component is not IAwake<P1>)
        {
            return;
        }
            
        List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1>));
        if (iAwakeSystems == null)
        {
            return;
        }

        foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
        {
            if (aAwakeSystem == null)
            {
                continue;
            }

            try
            {
                aAwakeSystem.Run(component, p1);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }

    public void Awake<P1, P2>(Entity component, P1 p1, P2 p2)
    {
        if (component is not IAwake<P1, P2>)
        {
            return;
        }
            
        List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2>));
        if (iAwakeSystems == null)
        {
            return;
        }

        foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
        {
            if (aAwakeSystem == null)
            {
                continue;
            }

            try
            {
                aAwakeSystem.Run(component, p1, p2);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }

    public void Awake<P1, P2, P3>(Entity component, P1 p1, P2 p2, P3 p3)
    {
        if (component is not IAwake<P1, P2, P3>)
        {
            return;
        }
            
        List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3>));
        if (iAwakeSystems == null)
        {
            return;
        }

        foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
        {
            if (aAwakeSystem == null)
            {
                continue;
            }

            try
            {
                aAwakeSystem.Run(component, p1, p2, p3);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }

    public void Awake<P1, P2, P3, P4>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4)
    {
        if (component is not IAwake<P1, P2, P3, P4>)
        {
            return;
        }
            
        List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4>));
        if (iAwakeSystems == null)
        {
            return;
        }

        foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
        {
            if (aAwakeSystem == null)
            {
                continue;
            }

            try
            {
                aAwakeSystem.Run(component, p1, p2, p3, p4);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }

    public void Awake<P1, P2, P3, P4, P5>(Entity component, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
    {
        if (component is not IAwake<P1, P2, P3, P4, P5>)
        {
            return;
        }
            
        List<SystemObject> iAwakeSystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IAwakeSystem<P1, P2, P3, P4, P5>));
        if (iAwakeSystems == null)
        {
            return;
        }

        foreach (IAwakeSystem<P1, P2, P3, P4, P5> aAwakeSystem in iAwakeSystems)
        {
            if (aAwakeSystem == null)
            {
                continue;
            }

            try
            {
                aAwakeSystem.Run(component, p1, p2, p3, p4, p5);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }

    public void Destroy(Entity component)
    {
        if (component is not IDestroy)
        {
            return;
        }
            
        List<SystemObject> iDestroySystems = this.TypeSystems.GetSystems(component.GetType(), typeof (IDestroySystem));
        if (iDestroySystems == null)
        {
            return;
        }

        foreach (IDestroySystem iDestroySystem in iDestroySystems)
        {
            if (iDestroySystem == null)
            {
                continue;
            }

            try
            {
                iDestroySystem.Run(component);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    }
}