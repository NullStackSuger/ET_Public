using System.Numerics;
using ImGuiNET;

namespace ET.Editor;

[EntitySystemOf(typeof(BackGroundComponent))]
public static partial class BackGroundComponentSystem
{
    [EntitySystem]
    private static void Ui(this BackGroundComponent self)
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos);
        ImGui.SetNextWindowSize(viewport.Size);
        ImGui.SetNextWindowViewport(viewport.ID);
            
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero); 
            
        ImGui.Begin(nameof(BackGroundComponent), ImGuiWindowFlags.NoTitleBar |
                                                 ImGuiWindowFlags.NoCollapse |
                                                 ImGuiWindowFlags.NoResize |
                                                 ImGuiWindowFlags.NoMove |
                                                 ImGuiWindowFlags.NoBringToFrontOnFocus |
                                                 ImGuiWindowFlags.NoNavFocus | 
                                                 ImGuiWindowFlags.NoBackground);
        ImGui.DockSpace(ImGui.GetID(nameof(BackGroundComponent)), Vector2.Zero, ImGuiDockNodeFlags.None);
        ImGui.End();
            
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(2);
    }
}

public class BackGroundComponent : Entity, IAwake, IUi
{
}