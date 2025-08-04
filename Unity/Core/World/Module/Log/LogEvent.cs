using System.Diagnostics;

namespace ET;

public struct LogInfo
{
    public int level;
    public string message;
    public StackTrace st;
    public long time;
    
    public LogInfo(int level, string message, StackTrace st)
    {
        this.level = level;
        this.message = message;
        this.st = st;
        this.time = Time.Instance.NowTime;
    }
}