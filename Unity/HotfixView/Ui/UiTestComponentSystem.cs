using ImGuiNET;

namespace ET.Client;

[EntitySystemOf(typeof(UiTestComponent))]
public static partial class UiTestComponentSystem
{
    [EntitySystem]
    private static void Ui(this UiTestComponent self)
    {
        ImGui.Begin(nameof(UiTestComponent));
        ImGui.Text("Test");
        ImGui.End();
    }
}