using MongoDB.Bson.Serialization.Attributes;

namespace ET;

public abstract partial class Entity: DisposeObject, IPool
{
    [BsonIgnoreIfDefault]
    [BsonDefaultValue(0L)]
    [BsonElement]
    [BsonId]
    public long Id { get; protected set; }
    
    [BsonIgnore]
    public long InstanceId { get; protected set; }
     
    [BsonIgnore]
    private IScene iScene;
    [BsonIgnore]
    public IScene IScene
    {
        get => iScene;
        set
        {
            if (value == null)
            {
                if (this.IsDisposed)
                {
                    iScene = null;
                    return;
                }
                else
                {
                    throw new Exception($"iScene cant set null: {this.GetType().FullName}");
                }
            }
            
            if (this.iScene == value)
            {
                return;
            }
            
            if (this.iScene != null)
            {
                this.iScene = value;
                return;
            }
            else
            {
                this.iScene = value;
                this.RegisterSystem();

                //TODO 反序列化出来的Entity初始化放在这里合适吗
                // 反序列化出来的
                if (!this.IsNew)
                {
                    if (this.InstanceId == 0)
                    {
                        this.InstanceId = IdGenerator.Instance.GenerateInstanceId();
                    }
                    
                    if (this.ComponentsCount() != 0)
                    {
                        foreach (Entity component in this.Components.Values)
                        {
                            component.IsComponent = true;
                            component.Parent = this;
                        }
                    }

                    if (this.ChildrenCount() != 0)
                    {
                        foreach (Entity child in this.Children.Values)
                        {
                            child.IsComponent = false;
                            child.Parent = this;
                        }
                    }
                    
                    EntitySystemSingleton.Instance.Deserialize(this);
                }
            }
        }
    }
    
    [BsonIgnore]
    public bool IsDisposed => this.InstanceId == 0;
    
    #region Status
    [Flags]
    private enum EntityStatus: byte
    {
        None = 0,
        IsFromPool = 1,
        IsComponent = 1 << 1,
        IsNew = 1 << 2,
    }

    [BsonIgnore]
    private EntityStatus status = EntityStatus.None;
    
    [BsonIgnore]
    public bool IsFromPool
    {
        get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
        set
        {
            if (value)
            {
                this.status |= EntityStatus.IsFromPool;
            }
            else
            {
                this.status &= ~EntityStatus.IsFromPool;
            }
        }
    }
    [BsonIgnore]
    public bool IsComponent
    {
        get => (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
        set
        {
            if (value)
            {
                this.status |= EntityStatus.IsComponent;
            }
            else
            {
                this.status &= ~EntityStatus.IsComponent;
            }
        }
    }
    [BsonIgnore]
    public bool IsNew
    {
        get => (this.status & EntityStatus.IsNew) == EntityStatus.IsNew;
        set
        {
            if (value)
            {
                this.status |= EntityStatus.IsNew;
            }
            else
            {
                this.status &= ~EntityStatus.IsNew;
            }
        }
    }
    #endregion

    #region Parent Child Component
    
    [BsonIgnore]
    private Entity parent;
    [BsonIgnore]
    public Entity Parent
    {
        get => parent;
        set
        {
            if (value == null)
            {
                if (this.IsDisposed)
                {
                    parent = null;
                    return;
                }
                else
                {
                    throw new Exception($"Parent cant set null: {this.GetType().FullName}");
                }
            }
            if (value == this)
            {
                throw new Exception($"cant set parent self: {this.GetType().FullName}");
            }
            // 严格限制parent必须要有iScene,也就是说parent必须在数据树上面
            if (value!.IScene == null)
            {
                throw new Exception($"cant set parent because parent iScene is null: {this.GetType().FullName} {value.GetType().FullName}");
            }

            if (this.parent != null) // 之前有parent
            {
                if (this.parent == value) // parent相同，不设置
                {
                    Log.Instance.Error($"重复设置了Parent: {this.GetType().FullName} parent: {this.parent.GetType().FullName}");
                    return;
                }

                if (this.IsComponent)
                {
                    this.parent.RemoveComponent(this.GetType(), false);
                }
                else
                {
                    this.parent.RemoveChild(this.Id, false);
                }
            }
            
            this.parent = value;
            if (this.IsComponent)
            {
                this.parent.AddComponent(this);
            }
            else
            {
                this.parent.AddChild(this);
            }

            if (this is IScene scene)
            {
                scene.Fiber = this.parent.IScene.Fiber;
                this.IScene = scene;
            }
            else
            {
                this.IScene = this.parent.IScene;
            }
        }
    }
    
    [BsonElement]
    [BsonIgnoreIfNull]
    private ChildrenCollection children;
    [BsonIgnore]
    public ChildrenCollection Children
    {
        get
        {
            return this.children ??= ObjectPool.Fetch<ChildrenCollection>();
        }
    }
    public int ChildrenCount()
    {
        if (this.children == null)
        {
            return 0;
        }
        return this.Children.Count;
    }
    
    [BsonElement]
    [BsonIgnoreIfNull]
    private ComponentsCollection components;
    [BsonIgnore]
    public ComponentsCollection Components
    {
        get
        {
            return this.components ??= ObjectPool.Fetch<ComponentsCollection>();
        }
    }
    public int ComponentsCount()
    {
        if (this.components == null)
        {
            return 0;
        }
        return this.Components.Count;
    }
    
    #endregion
    
    #region Get
    
    public T GetParent<T>() where T : Entity
    {
        return this.Parent as T;
    }

    public K GetChild<K>(long id) where K : Entity
    {
        if (this.ChildrenCount() == 0) return null;

        if (!this.Children.TryGetValue(id, out Entity child))
        {
            return null;    
        }
        
        return child as K;
    }
    
    public Entity GetComponent(Type type)
    {
        if (this.ComponentsCount() == 0) return null;

        if (!this.Components.TryGetValue(type.GetLongHashCode(), out Entity component))
        {
            return null;
        }
        
        return component;
    }
    public K GetComponent<K>() where K : Entity
    {
        if (this.ComponentsCount() == 0) return null;
        
        if (!this.Components.TryGetValue(typeof(K).GetLongHashCode(), out Entity component))
        {
            return null;
        }
        
        return component as K;
    }

    public bool GetComponent<K>(out K k) where K : Entity
    {
        if (this.ComponentsCount() == 0)
        {
            k = null;
            return false;
        }
        
        if (!this.Components.TryGetValue(typeof(K).GetLongHashCode(), out Entity component))
        {
            k = null;
            return false;
        }
        
        k = component as K;
        return true;
    }
    
    #endregion

    #region Add

    public void AddChild(Entity child)
    {
        this.Children.TryAdd(child.Id, child);
    }
    public Entity AddChild(Type type, bool isFromPool = false)
    {
        return this.AddChildWithId(IdGenerator.Instance.GenerateId(), type, isFromPool);
    }
    public T AddChild<T>(bool isFromPool = false) where T : Entity, IAwake
    {
        return this.AddChildWithId<T>(IdGenerator.Instance.GenerateId(), isFromPool);
    }
    public T AddChild<T, A>(A a, bool isFromPool = false) where T : Entity, IAwake<A>
    {
        return this.AddChildWithId<T, A>(IdGenerator.Instance.GenerateId(), a, isFromPool);
    }
    public T AddChild<T, A, B>(A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
    {
        return this.AddChildWithId<T, A, B>(IdGenerator.Instance.GenerateId(), a, b, isFromPool);
    }
    public T AddChild<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
    {
        return this.AddChildWithId<T, A, B, C>(IdGenerator.Instance.GenerateId(), a, b, c, isFromPool);
    }
    public T AddChild<T, A, B, C, D>(A a, B b, C c, D d, bool isFromPool = false) where T : Entity, IAwake<A, B, C, D>
    {
        return this.AddChildWithId<T, A, B, C, D>(IdGenerator.Instance.GenerateId(), a, b, c, d, isFromPool);
    }
    public T AddChild<T, A, B, C, D, E>(A a, B b, C c, D d, E e, bool isFromPool = false) where T : Entity, IAwake<A, B, C, D, E>
    {
        return this.AddChildWithId<T, A, B, C, D, E>(IdGenerator.Instance.GenerateId(), a, b, c, d, e, isFromPool);
    }
    
    public Entity AddChildWithId(long id, Type type, bool isFromPool = false)
    {
        Entity child = Create(type, isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child);
        
        return child;
    }
    public T AddChildWithId<T>(long id, bool isFromPool = false) where T : Entity, IAwake
    {
        Entity child = Create(typeof(T), isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child);
        
        return child as T;
    }
    public T AddChildWithId<T, A>(long id, A a, bool isFromPool = false) where T : Entity, IAwake<A>
    {
        Entity child = Create(typeof(T), isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child, a);
        
        return child as T;
    }
    public T AddChildWithId<T, A, B>(long id, A a, B b, bool isFromPool = false) where T : Entity, IAwake<A, B>
    {
        Entity child = Create(typeof(T), isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child, a, b);
        
        return child as T;
    }
    public T AddChildWithId<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false) where T : Entity, IAwake<A, B, C>
    {
        Entity child = Create(typeof(T), isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child, a, b, c);
        
        return child as T;
    }
    public T AddChildWithId<T, A, B, C, D>(long id, A a, B b, C c, D d, bool isFromPool = false) where T : Entity, IAwake<A, B, C, D>
    {
        Entity child = Create(typeof(T), isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child, a, b, c, d);
        
        return child as T;
    }
    public T AddChildWithId<T, A, B, C, D, E>(long id, A a, B b, C c, D d, E e, bool isFromPool = false) where T : Entity, IAwake<A, B, C, D, E>
    {
        Entity child = Create(typeof(T), isFromPool);
        child.Id = id;
        child.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(child, a, b, c, d, e);
        
        return child as T;
    }

    public void AddComponent(Entity component)
    {
        this.Components.TryAdd(component.GetLongHashCode(), component);
    }
    public Entity AddComponent(Type type, bool isFromPool = false)
    {
        return this.AddComponentWithId(this.Id, type, isFromPool);
    }
    public K AddComponent<K>(bool isFromPool = false) where K : Entity, IAwake, new()
    {
        return this.AddComponentWithId<K>(this.Id, isFromPool);
    }
    public K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
    {
        return this.AddComponentWithId<K, P1>(this.Id, p1, isFromPool);
    }
    public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
    {
        return this.AddComponentWithId<K, P1, P2>(this.Id, p1, p2, isFromPool);
    }
    public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
    {
        return this.AddComponentWithId<K, P1, P2, P3>(this.Id, p1, p2, p3, isFromPool);
    }
    public K AddComponent<K, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4>, new()
    {
        return this.AddComponentWithId<K, P1, P2, P3, P4>(this.Id, p1, p2, p3, p4, isFromPool);
    }
    public K AddComponent<K, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4, P5>, new()
    {
        return this.AddComponentWithId<K, P1, P2, P3, P4, P5>(this.Id, p1, p2, p3, p4, p5, isFromPool);
    }

    public Entity AddComponentWithId(long id, Type type, bool isFromPool = false)
    {
        Entity component = Create(type, isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component);
        
        return component;
    }
    public K AddComponentWithId<K>(long id, bool isFromPool = false) where K : Entity, IAwake, new()
    {
        Entity component = Create(typeof(K), isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component);
        
        return component as K;
    }
    public K AddComponentWithId<K, P1>(long id, P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
    {
        Entity component = Create(typeof(K), isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component, p1);
        
        return component as K;
    }
    public K AddComponentWithId<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
    {
        Entity component = Create(typeof(K), isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component, p1, p2);
        
        return component as K;
    }
    public K AddComponentWithId<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
    {
        Entity component = Create(typeof(K), isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component, p1, p2, p3);
        
        return component as K;
    }
    public K AddComponentWithId<K, P1, P2, P3, P4>(long id, P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4>, new()
    {
        Entity component = Create(typeof(K), isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component, p1, p2, p3, p4);
        
        return component as K;
    }
    public K AddComponentWithId<K, P1, P2, P3, P4, P5>(long id, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4, P5>, new()
    {
        Entity component = Create(typeof(K), isFromPool);
        component.Id = id;
        component.IsComponent = true;
        component.Parent = this;
        
        EntitySystemSingleton.Instance.Awake(component, p1, p2, p3, p4, p5);
        
        return component as K;
    }

    #endregion
    
    #region Remove
    
    public void RemoveChild(long id, bool needDispose = true)
    {
        if (this.ChildrenCount() == 0) return;

        if (!this.Children.Remove(id, out Entity child))
        {
            return;
        }

        if (this.ChildrenCount() == 0)
        {
            this.Children.Dispose();
            this.children = null;
        }

        if (needDispose)
        {
            child.Dispose();
        }
    }
    
    public void RemoveComponent(Type type, bool needDispose = true)
    {
        if (this.ComponentsCount() == 0) return;

        if (!this.Components.Remove(type.GetLongHashCode(), out Entity component))
        {
            return;
        }

        if (this.ComponentsCount() == 0)
        {
            this.Components.Dispose();
            this.components = null;
        }

        if (needDispose)
        {
            component.Dispose();
        }
    }
    public void RemoveComponent<K>(bool needDispose = true) where K : Entity
    {
        if (this.ComponentsCount() == 0) return;
        
        if (!this.Components.Remove(typeof(K).GetLongHashCode(), out Entity component))
        {
            return;
        }
        
        if (this.ComponentsCount() == 0)
        {
            this.Components.Dispose();
            this.components = null;
        }
        
        if (needDispose)
        {
            component.Dispose();
        }
    }
    
    #endregion
    
    private static Entity Create(Type type, bool isFromPool)
    {
        Entity entity = ObjectPool.Fetch(type, isFromPool) as Entity;
        entity!.InstanceId = IdGenerator.Instance.GenerateInstanceId();
        entity.IsFromPool = isFromPool;
        entity.IsNew = true;
        return entity;
    }
    
    protected void RegisterSystem()
    {
        this.Fiber().EntitySystem.RegisterSystem(this);
    }
    
    protected Entity()
    {
    }
    
    public override void Dispose()
    {
        if (this.IsDisposed) return;
        
        // 触发Destroy事件
        if (this is IDestroy)
        {
            EntitySystemSingleton.Instance.Destroy(this);
        }
        
        this.Id = 0;
        this.InstanceId = 0;

        // 清理Children
        if (this.ChildrenCount() != 0)
        {
            foreach (Entity child in this.Children.Values)
            {
                child.Dispose();
            }
            
            this.Children.Dispose();
            this.children = null;
        }
        
        // 清理Component
        if (this.ComponentsCount() != 0)
        {
            foreach (Entity component in this.Components.Values)
            {
                component.Dispose();
            }
            
            this.Components.Dispose();
            this.components = null;
        }
    
        this.Parent = null;
        this.IScene = null;
        
        bool isFromPool = this.IsFromPool;
        this.status = EntityStatus.None;
        this.IsFromPool = isFromPool;
        ObjectPool.Recycle(this);
    }
    
    public override void OnSerialize()
    {
        // 如果没有挂到树上，不用执行
        if (this.IScene == null)
        {
            return;
        }

        if (this is not ISerialize)
        {
            return;
        }
        
        EntitySystemSingleton.Instance.Serialize(this);

        if (this.ComponentsCount() != 0)
        {
            foreach (Entity component in this.Components.Values)
            {
                component.OnSerialize();
            }
        }

        if (this.ChildrenCount() != 0)
        {
            foreach (Entity child in this.Children.Values)
            {
                child.OnSerialize();
            }
        }
    }

    public override void OnDeserialize()
    {
        if (this.ChildrenCount() != 0)
        {
            foreach (Entity child in this.Children.Values)
            {
                child.OnDeserialize();
            }
        }
        
        if (this.ComponentsCount() != 0)
        {
            foreach (Entity component in this.Components.Values)
            {
                component.OnDeserialize();
            }
        }
        
        if (this is not IDeserialize)
        {
            return;
        }
        
        EntitySystemSingleton.Instance.Deserialize(this);
    }
}