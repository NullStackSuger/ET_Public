using System.Diagnostics;

namespace ET.Luban;

public static class LubanGenerator
{
    public static void Main(string[] args)
    {
        NumericGenerator.Create();
        
        // 运行三个bat文件
        RunBat("LubanClientGen.bat");
        RunBat("LubanServerGen.bat");
        RunBat("LubanClientServerGen.bat");
    }
    
    private static void RunBat(string batFileName)
    {
        string workSpace = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."));
        
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c {batFileName}",
                WorkingDirectory = workSpace,
            }
        };
        process.Start();
        process.WaitForExit();
    }
}