namespace ET;

public static partial class SceneType
{
    public const int All = 0;
    
    public const int Main = 1001; // 代码入口
    
    public const int NetInner = 1002; // 内网(Server)
    public const int NetClient = 1003; // 客户端网络Fiber
    public const int Location = 3001; // 位置服 (Entity.Id, Entity.InstanceId)

    public const int Realm = 9001; // 账号服
    
    public const int Gate = 9002; // 网关, 转发消息
    
    public const int Router = 9003; // 路由
    public const int RouterManager = 9004;

    public const int Http = 10001;
    
    public const int Map = 10002; // 服务端地图Fiber
    
    public const int Robot = 10003;

    public const int Current = 10021; // 客户端当前Scene, 因为没法说传个ChangePosition消息到Map1这种, 你换成Map2不就完了, 所以需要标记Current
}