using System.Collections.Concurrent;

namespace ET;

internal class MainThreadScheduler: IScheduler
{
    private readonly FiberManager fiberManager;
    private readonly ConcurrentQueue<int> idQueue = new();
    private readonly ConcurrentQueue<int> addIds = new();

    public MainThreadScheduler(FiberManager fiberManager)
    {
        this.fiberManager = fiberManager;
    }

    public void Update()
    {
        int count = idQueue.Count;
        while (count-- > 0)
        {
            if (!idQueue.TryDequeue(out int fiberId)) continue;
            
            Fiber fiber = this.fiberManager.Get(fiberId);
            if (fiber == null) continue;
            if (fiber.IsDisposed) continue;
            
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
            fiber.Update();
            Fiber.Instance = null;
            
            idQueue.Enqueue(fiberId);
        }
    }

    public void LateUpdate()
    {
        int count = idQueue.Count;
        while (count-- > 0)
        {
            if (!idQueue.TryDequeue(out int fiberId)) continue;
            
            Fiber fiber = this.fiberManager.Get(fiberId);
            if (fiber == null) continue;
            if (fiber.IsDisposed) continue;
            
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
            fiber.LateUpdate();
            fiber.FrameFinishUpdate();
            Fiber.Instance = null;
            
            idQueue.Enqueue(fiberId);
        }
        
        while (addIds.Count > 0)
        {
            addIds.TryDequeue(out int fiberId);
            idQueue.Enqueue(fiberId);
        }
    }

    public void Dispose()
    {
        addIds.Clear();
        idQueue.Clear();
    }

    public void Add(int fiberId)
    {
        addIds.Enqueue(fiberId);
    }
}