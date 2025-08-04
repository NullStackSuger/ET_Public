using System.Collections.Concurrent;

namespace ET;

internal class ThreadPoolScheduler: IScheduler
{
    private readonly FiberManager fiberManager;
    
    private readonly List<Thread> threads;
    private readonly ConcurrentQueue<int> idQueue = new();

    public ThreadPoolScheduler(FiberManager fiberManager)
    {
        this.fiberManager = fiberManager;
        int threadCount = Environment.ProcessorCount;
        threads = new List<Thread>(threadCount);
        for (int i = 0; i < threadCount; ++i)
        {
            Thread thread = new(this.Loop);
            threads.Add(thread);
            thread.Start();
        }
    }

    private void Loop()
    {
        int count = 0;
        while (true)
        {
            if (count <= 0)
            {
                Thread.Sleep(1);
                    
                // count最小为1
                count = fiberManager.Count() / threads.Count + 1;
            }
            
            --count;
            
            if (fiberManager.IsDisposed()) return;

            if (!idQueue.TryDequeue(out int id))
            {
                Thread.Sleep(1);
                continue;
            }
            
            Fiber fiber = fiberManager.Get(id);
            if (fiber == null) continue;
            if (fiber.IsDisposed) continue;
            
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
            fiber.Update();
            fiber.LateUpdate();
            fiber.FrameFinishUpdate();
            SynchronizationContext.SetSynchronizationContext(null);
            Fiber.Instance = null;

            idQueue.Enqueue(id);
        }
    }

    public void Dispose()
    {
        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    public void Add(int fiberId)
    {
        idQueue.Enqueue(fiberId);
    }
}