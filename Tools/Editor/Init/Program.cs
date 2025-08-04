using System.Reflection;
using CommandLine;

namespace ET.Editor;

internal static class Program
{
    private static void Main(string[] args)
    {
        Start(args).NoContext();
        Time.Instance.Start();
        while (!Options.Instance.NeedClose)
        {
            Time.Instance.Update();
            FiberManager.Instance.Update();
            FiberManager.Instance.LateUpdate();
        }
    }

    private static async ETTask Start(string[] args)
    {
        try
        {
            // 命令行参数
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                .WithParsed((o)=>World.Instance.AddSingleton(o));
        
            World.Instance.AddSingleton<Log, string, int, int>("Editor", Options.Instance.Process, 0);
        
            ETTask.ExceptionHandler += Log.Instance.Error;
        
            World.Instance.AddSingleton<Time>();
            World.Instance.AddSingleton<FiberManager>();
        
            World.Instance.AddSingleton<CodeTypes, Assembly[]>(Assemblies());
        
            WinPeriod.Init();
            MongoRegister.Init();
        
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<IdGenerator>();
        
            CodeTypes.Instance.CreateCode();
            CodeTypes.Instance.CreateConfig();
        
            await FiberManager.Instance.Create(SchedulerType.Main, SceneType.Editor, 0, SceneType.Editor);
        }
        catch (Exception e)
        {
            Log.Instance.Error(e);
        }
    }
    
    private static Assembly[] Assemblies()
    {
        Assembly model = Assembly.LoadFrom("Unity.Model.dll");
        Assembly modelView = Assembly.LoadFrom("Unity.ModelView.dll");
        Assembly hotfix = Assembly.LoadFrom("Unity.Hotfix.dll");
        Assembly hotfixView = Assembly.LoadFrom("Unity.HotfixView.dll");
        return [typeof(World).Assembly, typeof(Program).Assembly, hotfix, hotfixView, model, modelView];
    }
}