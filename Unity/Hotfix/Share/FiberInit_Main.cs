namespace ET;

[Invoke(SceneType.Main)]
public class FiberInit_Main: AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit fiberInit)
    {
        Scene root = fiberInit.Fiber.Root;
        
        // 这里控制Client Server ClientServer模式
        // Share
        await EventSystem.Instance.PublishAsync(root, new EntryShare());
        // Server
        await EventSystem.Instance.PublishAsync(root, new EntryServer());
        // Client
        await EventSystem.Instance.PublishAsync(root, new EntryClient());
    }
}

public struct EntryShare
{
}   
    
public struct EntryServer
{
} 
    
public struct EntryClient
{
}