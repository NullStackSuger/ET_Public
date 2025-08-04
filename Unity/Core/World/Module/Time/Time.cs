namespace ET;

// NowTime: 时间精确, 更新不规则(各种小数), 性能低 如: 网络消息、日志记录
// FrameTime: 时间不精确, 更新连续, 性能高, 如: 技能cd, buff时长, 移动
// ServerTime & ServerFrameTime
// 区别: 客户端可以预测服务端时间, 比如技能cd2s, 我服务端发到客户端0.5s, 就是客户端收到时其实只用等待1.5s的cd而不是2s, 那你表现层显示2s的话是不是坑人了
public class Time : Singleton<Time>, ISingletonAwake
{
    // 时区
    private int timeZone;
    public int TimeZone
    {
        get => timeZone;
        set
        {
            timeZone = value;
            dt = dt1970.AddHours(timeZone);
        }
    }
    private DateTime dt1970;
    private DateTime dt;

    private long frameTime;
    private long preFrameTime;
    private readonly object timeLock = new();
    
    public long NowTime => (DateTime.UtcNow.Ticks - dt1970.Ticks) / 10000;
    public long FrameTime => frameTime;
    public long DeltaTime
    {
        get
        {
            lock (timeLock)
            {
                return frameTime - preFrameTime;
            }
        }
    }
    
    #if UNITY
    // ping消息从服务端到客户端时间
    public long HalfRTT { get; set; }
    public long ServerFrameTime => FrameTime + HalfRTT;
    public long ServerNowTime => NowTime + HalfRTT;
    #elif DOTNET
    public long ServerFrameTime => FrameTime;
    public long ServerNowTime => NowTime;
    #endif

    public void Awake()
    {
        dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    public void Start()
    {
        long now = NowTime;
        preFrameTime = now;
        frameTime = now;
    }

    public void Update()
    {
        lock (timeLock)
        {
            preFrameTime = frameTime;
            frameTime = (DateTime.UtcNow.Ticks - dt1970.Ticks) / 10000;
        }
    }
}