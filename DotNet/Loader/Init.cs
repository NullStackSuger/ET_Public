using CommandLine;

namespace ET.Server;

public class Init
{
    public Init()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Instance.Error(e.ExceptionObject.ToString());
        };
        
        // 命令行参数
        Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
            .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
            .WithParsed((o)=>World.Instance.AddSingleton(o));

        World.Instance.AddSingleton<Log, string, int, int>("Server", Options.Instance.Process, 0);
        
        ETTask.ExceptionHandler += Log.Instance.Error;
        
        // 这几个不能放在Entry里, Entry是异步的, 如果Update时还没被挂载怎么办?
        World.Instance.AddSingleton<Time>();
        World.Instance.AddSingleton<FiberManager>();

        World.Instance.AddSingleton<CodeLoader>();
    }

    public void Start()
    {
        // 如果写在Awake里, 第一帧的DeltaTime就太大了
        Time.Instance.Start();
    }
    
    public void Update()
    {
        Time.Instance.Update();
        FiberManager.Instance.Update();
    }

    public void LateUpdate()
    {
        FiberManager.Instance.LateUpdate();
    }

    public void Dispose()
    {
        World.Instance.Dispose();
    }
}