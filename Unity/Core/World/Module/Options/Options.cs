using CommandLine;

namespace ET;

public class Options : Singleton<Options>
{
    [Option("Process", Required = false, Default = 1)]
    public int Process { get; set; }
    
    [Option("LogLevel", Required = false, Default = 0)]
    public int LogLevel { get; set; }

    public bool NeedClose { get; set; }
}