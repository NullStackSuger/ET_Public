/*using System.Numerics;
using ET.Client;
using ImGuiNET;

namespace ET.Editor;

[AComponentDetail(typeof(TransformComponent))]
public class Transform_ComponentDetailHandler : AComponentDetailHandler<TransformComponent>
{
    protected override void Draw(DetailEditorComponent self, ViewObject viewObject, TransformComponent component)
    {
        ImGui.DragFloat3("Position", ref component.position);
        
        Vector3 rot = component.rotation.ToVector3();
        if (ImGui.DragFloat3("Rotation", ref rot))
        {
            component.rotation = rot.ToQuaternion();
        }
        
        ImGui.DragFloat3("Scale", ref component.scale);
    }
}*/