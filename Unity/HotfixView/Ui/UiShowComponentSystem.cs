using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace ET.Client;

[EntitySystemOf(typeof(UiShowComponent))]
public static partial class UiShowComponentSystem
{
    [EntitySystem]
    private static void Awake(this UiShowComponent self)
    {
        Scene scene = self.Scene();
        UiComponent uis = scene.GetComponent<UiComponent>();
        RenderComponent render = scene.GetComponent<RenderComponent>();
        Texture result = render.Get<Texture>("ShadingResult");
        self.renderView = uis.uiController.GetOrCreateImGuiBinding(render.device.ResourceFactory, render.device.ResourceFactory.CreateTextureView(result));
        self.size = new Vector2(result.Width, result.Height);
    }

    [EntitySystem]
    private static void Ui(this UiShowComponent self)
    {
        ImGui.Begin(nameof(UiShowComponent), ImGuiWindowFlags.MenuBar);
        ImGui.Image(self.renderView, self.size);
        ImGui.End();
    }
}