namespace ET.Client;

public abstract class AShaderHandler : HandlerObject
{
    public abstract MeshRenderInfo Awake(RenderComponent renderComponent, MeshComponent meshComponent);
    
    public abstract void Update(RenderComponent renderComponent, MeshComponent meshComponent, MeshRenderInfo info);
}