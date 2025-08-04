using System.Collections.Concurrent;

namespace ET;

// 一个Fiber一个固定的线程
internal class ThreadScheduler: IScheduler
{
    private readonly FiberManager fiberManager;
    
    private readonly ConcurrentDictionary<int, Thread> dictionary = new();

    public ThreadScheduler(FiberManager fiberManager)
    {
        this.fiberManager = fiberManager;
    }

    private void Loop(int fiberId)
    {
        Fiber fiber = fiberManager.Get(fiberId);
        Fiber.Instance = fiber;
        SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);

        while (true)
        {
            if (fiberManager.IsDisposed()) return;
            
            fiber = fiberManager.Get(fiberId);
            if (fiber == null || fiber.IsDisposed)
            {
                dictionary.Remove(fiberId, out _);
                return;
            }
            
            fiber.Update();
            fiber.LateUpdate();
            fiber.FrameFinishUpdate();
            
            Thread.Sleep(1);
        }
    }

    public void Dispose()
    {
        foreach ((int fiberId, Thread thread) in this.dictionary.ToArray())
        {
            thread.Join();
        }
    }

    public void Add(int fiberId)
    {
        Thread thread = new(() => this.Loop(fiberId));
        dictionary.TryAdd(fiberId, thread);
        thread.Start();
    }
}