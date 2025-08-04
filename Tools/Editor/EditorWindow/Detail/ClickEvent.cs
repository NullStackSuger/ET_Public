using ET.Client;

namespace ET.Editor;

public struct ClickFileItem
{
    public string path;
    public string name;
    public string extension;
}

public struct ClickViewObject
{
    public EntityRef<ViewObject> viewObject;
}