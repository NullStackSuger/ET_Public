namespace ET;
    
public class Fiber : IDisposable
{
    // 该字段只能框架使用，绝对不能改成public，改了后果自负
    [StaticField]
    [ThreadStatic]
    internal static Fiber Instance;
    
    public bool IsDisposed { get; private set; }

    public int Id { get; }
    public int Zone { get; }
    public int Process => Options.Instance.Process;
    public Address Address => new(this.Process, this.Id);
    
    public Scene Root { get; }
    public EntitySystem EntitySystem { get; }
    public Mailboxes Mailboxes { get; private set; }
    public ThreadSynchronizationContext ThreadSynchronizationContext { get; }
    private readonly Queue<ETTask> frameFinishTasks = new();

    internal Fiber(int id, int zone, int sceneType)
    {
        Id = id;
        Zone = zone;
        
        EntitySystem = new EntitySystem();
        Mailboxes = new Mailboxes();
        ThreadSynchronizationContext = new ThreadSynchronizationContext();
        
        Root = new Scene(this, sceneType, id, 1);
    }

    internal void Update()
    {
        try
        {
            EntitySystem.Publish(new UpdateEvent());
        }
        catch (Exception e)
        {
            Log.Instance.Error(e);
        }
    }
        
    internal void LateUpdate()
    {
        try
        {
            EntitySystem.Publish(new LateUpdateEvent());
        }
        catch (Exception e)
        {
            Log.Instance.Error(e);
        }
    }

    internal void FrameFinishUpdate()
    {
        while (this.frameFinishTasks.Count > 0)
        {
            ETTask task = frameFinishTasks.Dequeue();
            task.SetResult();
        }
        ThreadSynchronizationContext.Update();
    }
    
    public async ETTask WaitFrameFinish()
    {
        ETTask task = ETTask.Create(true);
        frameFinishTasks.Enqueue(task);
        await task;
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        
        IsDisposed = true;
        
        Root.Dispose();
    }
}