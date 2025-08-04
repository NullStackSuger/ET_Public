using System.Reflection;
using System.Runtime.Loader;

namespace ET.Client;

public class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
{
    private AssemblyLoadContext hotfixLoadContext;
    private AssemblyLoadContext hotfixViewLoadContext;
    
    private Assembly model;
    private Assembly modelView;
    
    public void Awake()
    {
        (model, modelView) = LoadModel();
        (Assembly hotfix, Assembly hotfixView) = LoadHotfix();
        GC.Collect();
        
        World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof(World).Assembly /*Core*/, typeof(Init).Assembly /*Loader*/, model, modelView, hotfix, hotfixView]);
        
        StaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
        start.Run();
    }

    public void Reload()
    {
        (Assembly hotfix, Assembly hotfixView) = LoadHotfix();
        GC.Collect();
        
        World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof(World).Assembly /*Core*/, typeof(Init).Assembly /*Loader*/, model, modelView, hotfix, hotfixView]);
        
        Log.Instance.Debug($"reload dll finish!");
    }

    private (Assembly, Assembly) LoadModel()
    {
        Assembly model = Assembly.LoadFrom("Unity.Model.dll");
        Assembly modelView = Assembly.LoadFrom("Unity.ModelView.dll");
        return (model, modelView);
    }

    private (Assembly, Assembly) LoadHotfix()
    {
        hotfixLoadContext?.Unload();
        hotfixLoadContext = new AssemblyLoadContext("Unity.Hotfix", true);
        byte[] hotfixDllBytes = File.ReadAllBytes("./Unity.Hotfix.dll");
        byte[] hotfixPdbBytes = File.ReadAllBytes("./Unity.Hotfix.pdb");
        Assembly hotfix = hotfixLoadContext.LoadFromStream(new MemoryStream(hotfixDllBytes), new MemoryStream(hotfixPdbBytes));
        
        hotfixViewLoadContext?.Unload();
        hotfixViewLoadContext = new AssemblyLoadContext("Unity.HotfixView", true);
        byte[] hotfixViewDllBytes = File.ReadAllBytes("./Unity.HotfixView.dll");
        byte[] hotfixViewPdbBytes = File.ReadAllBytes("./Unity.HotfixView.pdb");
        Assembly hotfixView = hotfixLoadContext.LoadFromStream(new MemoryStream(hotfixViewDllBytes), new MemoryStream(hotfixViewPdbBytes));

        return (hotfix, hotfixView);
    }
}