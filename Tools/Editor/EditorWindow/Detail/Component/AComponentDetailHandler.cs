using ET.Client;

namespace ET.Editor;

public interface IComponentDetailHandler
{
    void Handler(DetailEditorComponent self, ViewObject viewObject, Entity entity);
}

public abstract class AComponentDetailHandler<T> : HandlerObject, IComponentDetailHandler where T : Entity
{
    protected abstract void Draw(DetailEditorComponent self, ViewObject viewObject, T component);
    
    public void Handler(DetailEditorComponent self, ViewObject viewObject, Entity entity)
    {
        if (entity is not T component)
        {
            Log.Instance.Error($"类型转换错误: {entity.GetType().FullName} to {typeof (T).Name}");
            return;
        }
        
        Draw(self, viewObject, component);
    }
}