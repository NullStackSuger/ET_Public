using System.Diagnostics;
using NLog;

namespace ET;

// 注意发布时, 要把Config/NLog/NLog.config
// <targets async="false"> 改为true
// 因为是异步写入, 所以和Console.Write顺序不对, 而且无法设置颜色
public class Log : Singleton<Log>, ISingletonAwake<string, int, int>
{
    public const int TraceLevel = 1;
    public const int DebugLevel = 2;
    public const int InfoLevel = 3;
    public const int WarningLevel = 4;
    public const int ErrorLevel = 5;

    private Logger logger;
    private readonly UnOrderMultiMap<int, LogInfo> logs = new();

    static Log()
    {
        LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("Config/NLog/NLog.config");
        LogManager.Configuration.Variables["currentDir"] = Environment.CurrentDirectory;
    }
    
    public void Awake(string name, int process, int fiber)
    {
        logger = LogManager.GetLogger($"{(uint)process:000000}.{(uint)fiber:0000000000}.{name}");
    }
    
    public List<LogInfo> this[int level]
    {
        get
        {
            var list = level == 0 ? logs.GetAll() : logs.GetAll(level);
            list.Sort((a, b) => a.time.CompareTo(b.time));
            return list;
        }
    }

    public void Clear()
    {
        logs.Clear();
    }

    [Conditional("DEBUG")]
    public void Trace(string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Options.Instance.LogLevel > TraceLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        logger.Trace($"{msg}\n{st}");
        Console.ResetColor();
        
        logs.Add(TraceLevel, new LogInfo(TraceLevel, msg, st));
    }
    
    [Conditional("DEBUG")]
    public void Debug(string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Options.Instance.LogLevel > DebugLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        logger.Debug(msg);
        Console.ResetColor();
        
        logs.Add(DebugLevel, new LogInfo(DebugLevel, msg, st));
    }
    
    public void Info(string msg, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Options.Instance.LogLevel > InfoLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        logger.Info(msg);
        Console.ResetColor();
        
        logs.Add(InfoLevel, new LogInfo(InfoLevel, msg, st));
    }

    public void Warning(string msg, ConsoleColor color = ConsoleColor.Yellow)
    {
        if (Options.Instance.LogLevel > WarningLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        logger.Warn(msg);
        Console.ResetColor();
        
        logs.Add(WarningLevel, new LogInfo(WarningLevel, msg, st));
    }

    public void Error(string msg, ConsoleColor color = ConsoleColor.Red)
    {
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        logger.Error($"{msg}\n{st}");
        Console.ResetColor();
        
        logs.Add(ErrorLevel, new LogInfo(ErrorLevel, msg, st));
        
        Console.WriteLine(msg);
    }

    public void Error(Exception e)
    {
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = ConsoleColor.Red;
        logger.Error(e.ToString());
        Console.ResetColor();
        
        logs.Add(ErrorLevel, new LogInfo(ErrorLevel, e.ToString(), st));
        
        Console.WriteLine(e);
    }

    [Conditional("DEBUG")]
    public void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler msg, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Options.Instance.LogLevel > TraceLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        string str = msg.ToStringAndClear();
        logger.Trace($"{str}\n{st}");
        Console.ResetColor();
        
        logs.Add(TraceLevel, new LogInfo(TraceLevel, str, st));
    }
    
    [Conditional("DEBUG")]
    public void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler msg, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Options.Instance.LogLevel > DebugLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        string str = msg.ToStringAndClear();
        logger.Debug(str);
        Console.ResetColor();
        
        logs.Add(DebugLevel, new LogInfo(DebugLevel, str, st));
    }

    public void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler msg, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Options.Instance.LogLevel > InfoLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        string str = msg.ToStringAndClear();
        logger.Info(str);
        Console.ResetColor();
        
        logs.Add(InfoLevel, new LogInfo(InfoLevel, str, st));
    }
    
    public void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler msg, ConsoleColor color = ConsoleColor.Yellow)
    {
        if (Options.Instance.LogLevel > WarningLevel)
        {
            return;
        }
        
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        string str = msg.ToStringAndClear();
        logger.Warn(str);
        Console.ResetColor();
        
        logs.Add(WarningLevel, new LogInfo(WarningLevel, str, st));
    }
    
    public void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler msg, ConsoleColor color = ConsoleColor.Red)
    {
        StackTrace st = new(1, true);
        
        Console.ForegroundColor = color;
        string str = msg.ToStringAndClear();
        logger.Error($"{str}\n{st}");
        Console.ResetColor();
        
        logs.Add(ErrorLevel, new LogInfo(ErrorLevel, str, st));
        
        Console.WriteLine(str);
    }
}