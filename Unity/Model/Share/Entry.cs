namespace ET;

public static class Entry
{
    public static void Start()
    {
        StartAsync().NoContext();
    }

    private static async ETTask StartAsync()
    {
        try
        {
            WinPeriod.Init();
            MongoRegister.Init();
        
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<IdGenerator>();
        
            CodeTypes.Instance.CreateCode();
            CodeTypes.Instance.CreateConfig();
        
            await FiberManager.Instance.Create(SchedulerType.Main, SceneType.Main, 0, SceneType.Main);
        }
        catch (Exception e)
        {
            Log.Instance.Error(e);
        }
    }
}