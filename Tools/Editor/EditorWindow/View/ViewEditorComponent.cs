using System.Numerics;
using ET.Client;
using ImGuiNET;
using Veldrid;

namespace ET.Editor;

[EntitySystemOf(typeof(ViewEditorComponent))]
public static partial class ViewEditorComponentSystem
{
    [EntitySystem]
    private static void Awake(this ViewEditorComponent self)
    {
        Scene scene = self.Scene();
        EditorWindowComponent windows = scene.GetComponent<EditorWindowComponent>();
        RenderComponent render = scene.GetComponent<RenderComponent>();
        Texture result = render.Get<Texture>("ShadingResult");
        self.renderView = windows.uiController.GetOrCreateImGuiBinding(render.device.ResourceFactory, render.device.ResourceFactory.CreateTextureView(result));
    }

    [EntitySystem]
    private static void Ui(this ViewEditorComponent self)
    {
        Scene scene = self.Scene();
        RenderComponent render = scene.GetComponent<RenderComponent>();
        Texture result = render.Get<Texture>("ShadingResult");
            
        ImGui.Begin(nameof(ViewEditorComponent), ImGuiWindowFlags.MenuBar);
        ImGui.Image(self.renderView, new Vector2(0.5f * result.Width, 0.5f * result.Height));
        ImGui.End();
    }
}

public class ViewEditorComponent : Entity, IAwake, IUi
{
    public IntPtr renderView;
}