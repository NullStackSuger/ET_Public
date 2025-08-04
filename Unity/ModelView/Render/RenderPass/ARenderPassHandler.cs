namespace ET.Client;

[ARenderPass]
public abstract class ARenderPassHandler : HandlerObject
{
    public abstract void Awake(RenderComponent renderComponent);
    
    public abstract void Update(RenderComponent renderComponent);
}