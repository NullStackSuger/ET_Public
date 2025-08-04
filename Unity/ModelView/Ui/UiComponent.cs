using Veldrid;

namespace ET.Client;

// TODO UI系统/需要写套UGUI, 用Mesh,Vulkan,SixLabors.Fonts
public class UiComponent : Entity, IAwake<int, int, GraphicsDevice>
{
    public ImGuiController uiController;
}