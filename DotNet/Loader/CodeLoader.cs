using System.Reflection;
using System.Runtime.Loader;

namespace ET.Server;

public class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
{
    private AssemblyLoadContext hotfixLoadContext;

    private Assembly model;
    
    public void Awake()
    {
        model = LoadModel();
        Assembly hotfix = LoadHotfix();
        GC.Collect();

        World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof(World).Assembly /*Core*/, typeof(Init).Assembly /*Loader*/, model, hotfix]);

        StaticMethod start = new StaticMethod(this.model, "ET.Entry", "Start");
        start.Run();
    }

    public void Reload()
    {
        Assembly hotfix = LoadHotfix();
        GC.Collect();
        
        World.Instance.AddSingleton<CodeTypes, Assembly[]>([typeof(World).Assembly /*Core*/, typeof(Init).Assembly /*Loader*/, model, hotfix]);
        
        Log.Instance.Debug($"reload dll finish!");
    }

    private Assembly LoadModel()
    {
        return Assembly.LoadFrom("DotNet.Model.dll");
    }

    private Assembly LoadHotfix()
    {
        hotfixLoadContext?.Unload();
        hotfixLoadContext = new AssemblyLoadContext("DotNet.Hotfix", true);
        byte[] dllBytes = File.ReadAllBytes("./DotNet.Hotfix.dll");
        byte[] pdbBytes = File.ReadAllBytes("./DotNet.Hotfix.pdb");
        Assembly hotfix = hotfixLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
        return hotfix;
    }
}