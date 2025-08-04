namespace ET.Editor;

public abstract class AFileDetailHandler : HandlerObject
{
    public abstract void Draw(DetailEditorComponent self, string filePath);
}