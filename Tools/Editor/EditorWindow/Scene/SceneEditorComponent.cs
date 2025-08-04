using ET.Client;
using ImGuiNET;

namespace ET.Editor;

[EntitySystemOf(typeof(SceneEditorComponent))]
public static partial class SceneEditorComponentSystem
{
    [EntitySystem]
    private static void Awake(this SceneEditorComponent self)
    {
        //self.Read($"{PathHelper.Asset}\\Scene 1752068573287");
    }
    
    [EntitySystem]
    private static void Ui(this SceneEditorComponent self)
    {
        ImGui.Begin(nameof(SceneEditorComponent), ImGuiWindowFlags.MenuBar);
            
        if (ImGui.BeginPopupContextWindow("FilePanelContextMenu", ImGuiPopupFlags.MouseButtonRight))
        {
            if (ImGui.MenuItem("Save"))
            {
                self.Save();
            }
            ImGui.EndPopup();
        }
            
        self.Draw();

        ImGui.End();
    }

    // 渲染当前Scene的ViewObjectComponent
    private static void Draw(this SceneEditorComponent self)
    {
        ViewObjectComponent objs = self.Scene().GetComponent<ViewObjectComponent>();
        if (objs != null)
        {
            ListComponent<(EntityRef<ViewObject>, long)> waitToRemove = ListComponent<(EntityRef<ViewObject>, long)>.Create();
            foreach (var entity in objs.Children.Values)
            {
                if (entity is ViewObject obj)
                {
                    DrawInner(self, obj, waitToRemove);
                }
            }

            foreach (var kv in waitToRemove)
            {
                ViewObject parent = kv.Item1;
                if (parent == null)
                {
                    continue;
                }
                parent.RemoveChild(kv.Item2);
            }
        }
        
        static void DrawInner(SceneEditorComponent self, ViewObject obj, ListComponent<(EntityRef<ViewObject>, long)> waitToRemove)
        {
            // 判断是否有子节点
            bool hasChild = false;
            foreach (var entity in obj.Children.Values)
            {
                if (entity is ViewObject)
                {
                    hasChild = true;
                    break;
                }
            }

            string label = $"{obj.name}##{obj.Id}";
            bool open = hasChild ? ImGui.TreeNode(label) : ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
            
            if (ImGui.IsItemClicked())
            {
                EventSystem.Instance.Publish(self.Scene(), new ClickViewObject() { viewObject = obj });
            }
            
            if (ImGui.BeginPopupContextItem(label))
            {
                if (ImGui.MenuItem("Add"))
                {
                    obj.AddChild<ViewObject, string>("view object");
                }
                if (ImGui.MenuItem("Remove"))
                {
                    waitToRemove.Add((obj.GetParent<ViewObject>(), obj.Id));
                }
                ImGui.EndPopup();
            }
            // 只有有子节点且被展开时才递归
            if (hasChild && open)
            {
                foreach (var entity in obj.Children.Values)
                {
                    if (entity is ViewObject child)
                    {
                        DrawInner(self, child, waitToRemove);
                    }
                }
                ImGui.TreePop();
            }
        }
    }

    // 保存当前Scene的ViewObjectComponent
    private static void Save(this SceneEditorComponent self)
    {
        Scene scene = self.Scene();
        
        ViewObjectComponent objs = scene.GetComponent<ViewObjectComponent>();
        if (objs == null)
        {
            return;
        }
        byte[] bytes = MongoHelper.Serialize(objs);
        
        // 去FileEditorComponent中找到当前名字对应的路径, 如果没有, 保存到Asset
        var files = self.GetParent<EditorWindowComponent>().GetComponent<FileEditorComponent>().files;
        if (files.TryGetValue(".prefab", $"{self.name}.prefab", out string outPath))
        {
            File.WriteAllBytes(outPath, bytes);
        }
        else
        {
            self.name = $"Scene {Time.Instance.NowTime}";
            File.WriteAllBytes($"{PathHelper.Asset}\\{self.name}.prefab", bytes);
        }
    }

    // 读取ViewObjectComponent到当前Scene
    private static void Read(this SceneEditorComponent self, string path)
    {
        if (File.Exists($"{path}.prefab"))
        {
            Scene scene = self.Scene();
            
            byte[] bytes = File.ReadAllBytes($"{path}.prefab");
            ViewObjectComponent objs = MongoHelper.Deserialize<ViewObjectComponent>(bytes);
            objs.IsComponent = true;
            scene.RemoveComponent<ViewObjectComponent>();
            objs.Parent = self.Scene();
            SetState(objs);
            
            // TODO 这里硬写有问题
            PerspectiveCameraComponent.Main = objs.GetComponent<PerspectiveCameraComponent>();
            DirectionLightComponent.Main = objs.GetComponent<DirectionLightComponent>();
        }

        static void SetState(Entity entity)
        {
            entity.IsComponent = entity is not ViewObject;
            
            foreach (Entity component in entity.Components.Values)
            {
                SetState(component);
            }

            foreach (Entity child in entity.Children.Values)
            {
                SetState(child);
            }
        }
    }
}

public class SceneEditorComponent : Entity, IAwake, IUi
{
    public string name = "";
}