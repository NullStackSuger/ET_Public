namespace ET.Editor;

[Code]
public class SelectedComponent : Singleton<SelectedComponent>, ISingletonAwake
{
    private object selected;
    
    public void Awake()
    {
        
    }

    public T Get<T>()
    {
        return selected == null ? default : (T)selected;
    }

    public void Set(object selected)
    {
        this.selected = selected;
    }
}