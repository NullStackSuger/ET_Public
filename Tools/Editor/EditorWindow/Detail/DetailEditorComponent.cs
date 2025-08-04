using ET.Client;
using ImGuiNET;

namespace ET.Editor;

// TODO 这样写肯定是有问题的, 难道我以后想加一个还要接着写吗

[Event(SceneType.Editor)]
public class ClickFileItem_ShowDetail : AEvent<Scene, ClickFileItem>
{
    protected override async ETTask Run(Scene scene, ClickFileItem a)
    {
        var detail = scene.GetComponent<EditorWindowComponent>().GetComponent<DetailEditorComponent>();
        detail.filePath = a.path;
        detail.viewObject = null;
        detail.needRebuild = true;
        await ETTask.CompletedTask;
    }
}

[Event(SceneType.Editor)]
public class ClickViewObject_ShowDetail : AEvent<Scene, ClickViewObject>
{
    protected override async ETTask Run(Scene scene, ClickViewObject a)
    {
        var detail = scene.GetComponent<EditorWindowComponent>().GetComponent<DetailEditorComponent>();
        detail.filePath = "";
        detail.viewObject = a.viewObject;
        detail.needRebuild = true;
        await ETTask.CompletedTask;
    }
}

[EntitySystemOf(typeof(DetailEditorComponent))]
public static partial class DetailEditorComponentSystem
{
    [EntitySystem]
    private static void Ui(this DetailEditorComponent self)
    {
        ImGui.Begin(nameof(DetailEditorComponent), ImGuiWindowFlags.MenuBar);
        if (self.filePath != "")
        {
            self.DrawFile(self.filePath);
        }
        else if ((ViewObject)self.viewObject != null)
        {
            self.DrawViewObject(self.viewObject);
        }
        ImGui.End();

        self.needRebuild = false;
    }

    private static void DrawFile(this DetailEditorComponent self, string filePath)
    {
        // 对所有显示名称
        ImGui.SetWindowFontScale(2);
        ImGui.Text($"{Path.GetFileNameWithoutExtension(filePath)}");
        ImGui.SetWindowFontScale(1);
        
        ImGui.Separator();
        
        // 对后缀名进行分发
        string extension = Path.GetExtension(filePath);
        FileDetailDispatcher.Instance[extension]?.Draw(self, filePath);
    }

    private static void DrawViewObject(this DetailEditorComponent self, ViewObject viewObject)
    {
        // 对所有显示名称
        ImGui.SetWindowFontScale(2);
        ImGui.Text(viewObject.name);
        ImGui.SetWindowFontScale(1);
        
        ImGui.Separator();

        foreach (Entity component in viewObject.Components.Values)
        {
            string componentName = component.GetType().Name.TrimEnd("Component".ToCharArray());
            if (ImGui.CollapsingHeader(componentName))
            {
                ComponentDetailDispatcher.Instance.Handler(self, viewObject, component);
            
                ImGui.Separator();   
            }
        }
    }
}

public class DetailEditorComponent : Entity, IAwake, IUi
{
    public bool needRebuild = true;
    
    public string filePath = "";
    public string txtContentTmp;
    
    public EntityRef<ViewObject> viewObject;
}