using System.Collections.Concurrent;

namespace ET;

public class FiberManager : Singleton<FiberManager>, ISingletonAwake, ISingletonReverseDispose
{
    private readonly IScheduler[] schedulers = new IScheduler[3];
    private MainThreadScheduler mainThreadScheduler;
    private ConcurrentDictionary<int, Fiber> fibers = new();
    
    public void Awake()
    {
        mainThreadScheduler = new MainThreadScheduler(this);
        
        schedulers[(int)SchedulerType.Main] = mainThreadScheduler;
        
        #if EDITOR
        schedulers[(int)SchedulerType.Thread] = mainThreadScheduler;
        schedulers[(int)SchedulerType.ThreadPool] = mainThreadScheduler;
        #else
        schedulers[(int)SchedulerType.Thread] = new ThreadScheduler(this);
        schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler(this);
        #endif
    }

    public void Update()
    {
        mainThreadScheduler.Update();
    }

    public void LateUpdate()
    {
        mainThreadScheduler.LateUpdate();
    }
    
    protected override void Destroy()
    {
        foreach (IScheduler scheduler in schedulers)
        {
            scheduler.Dispose();
        }
            
        foreach ((int fiberId, Fiber fiber) in fibers)
        {
            fiber.Dispose();
        }

        fibers = null;
    }

    public async ETTask<int> Create(SchedulerType schedulerType, int fiberId, int zone, int sceneType)
    {
        try
        {
            Fiber fiber = new(fiberId, zone, sceneType);
        
            if (!fibers.TryAdd(fiberId, fiber))
            {
                throw new Exception($"same fiber already existed, if you remove, please await Remove then Create fiber! {fiberId}");
            }
        
            schedulers[(int) schedulerType].Add(fiberId);
        
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            fiber.ThreadSynchronizationContext.Post(() => { Action().NoContext(); });
            await tcs.Task;
        
            return fiberId;
            
            async ETTask Action()
            {
                try
                {
                    // 根据Fiber的SceneType分发Init,必须在Fiber线程中执行
                    await EventSystem.Instance.Invoke<FiberInit, ETTask>(sceneType, new FiberInit() { Fiber = fiber });
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    Log.Instance.Error($"init fiber fail: {sceneType} {e}");
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"create fiber error: {fiberId} {sceneType}", e);
        }
    }
    public async ETTask<int> Create(SchedulerType schedulerType, int zone, int sceneType)
    {
        return await Create(schedulerType, IdGenerator.Instance.GenerateFiberId(), zone, sceneType);
    }

    public async ETTask Remove(int fiberId)
    {
        Fiber fiber = Get(fiberId);
        TaskCompletionSource<bool> tcs = new();
        // 要扔到fiber线程执行，否则会出现线程竞争
        fiber.ThreadSynchronizationContext.Post(() =>
        {
            if (fibers.Remove(fiberId, out Fiber f))
            {
                f.Dispose();
            }
            tcs.SetResult(true);
        });
        await tcs.Task;
    }
    
    // 不允许外部调用，容易出现多线程问题, 只能通过消息通信，不允许直接获取其它Fiber引用
    internal Fiber Get(int fiberId)
    {
        fibers.TryGetValue(fiberId, out Fiber fiber);
        return fiber;
    }
    
    public int Count()
    {
        return fibers.Count;
    }
}