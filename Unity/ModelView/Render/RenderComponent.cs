using Veldrid;

namespace ET.Client;

public class RenderComponent : Entity, IAwake<Type[]>, ILateUpdate
{
    public GraphicsDevice device;
    public CommandList commandList;
    public Dictionary<string, object> dic = new(); // TODO 渲染系统/需要替换成WOW中的Dic
    public Type[] types;
}