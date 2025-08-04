using ImGuiNET;
using Veldrid;

namespace ET.Client;

// 注意 这个是游戏Ui, 不是编辑器Ui, 不要乱用
[EntitySystemOf(typeof(UiComponent))]
public static partial class UiComponentSystem
{
    [EntitySystem]
    private static void Awake(this UiComponent self, int a, int b, GraphicsDevice c)
    {
        Framebuffer framebuffer = self.Scene().GetComponent<RenderComponent>().Get<Framebuffer>("ShadingFramebuffer");
        if (framebuffer == null)
        {
            Log.Instance.Error($"Framebuffer is null");
            return;
        }

        self.uiController = new ImGuiController(c, framebuffer.OutputDescription, a, b);

        // 注意 所有的ImGui操作都要在创建完ImGuiController后执行
        var style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.WindowBg] = Color.Gray; // 窗口背景色
        style.Colors[(int)ImGuiCol.TitleBg] = Color.Gray; // 页标颜色
        style.Colors[(int)ImGuiCol.TitleBgActive] = Color.Gray; // 活动页表颜色
        style.Colors[(int)ImGuiCol.DockingPreview] = new Color(0.3f); // 吸附到窗口颜色
        style.Colors[(int)ImGuiCol.DockingEmptyBg] = Color.Gray; // 吸附到空地颜色
        style.Colors[(int)ImGuiCol.TabHovered] = new Color(0.5f); // 页签悬停颜色
        style.Colors[(int)ImGuiCol.Tab] = Color.Gray; // 页签默认颜色
        style.Colors[(int)ImGuiCol.TabSelected] = new Color(0.15f); // 选中页签颜色
        style.Colors[(int)ImGuiCol.TabSelectedOverline] = new Color(0.5f); // 选中页签的线的颜色
        style.Colors[(int)ImGuiCol.TabDimmed] = Color.Gray; // 没选中的页签颜色
        style.Colors[(int)ImGuiCol.TabDimmedSelected] = Color.Gray; // 没选中的页签颜色(仅之前选过的)
        style.Colors[(int)ImGuiCol.ResizeGrip] = Color.Gray; // 窗口右下角调整大小图标的颜色
        style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Color(0.5f); // 窗口横向调整大小的颜色
        style.Colors[(int)ImGuiCol.ResizeGripActive] = new Color(0.5f); // 窗口竖向调整大小的颜色
        style.Colors[(int)ImGuiCol.Button] = new Color(0.3f); // 按钮颜色
        style.Colors[(int)ImGuiCol.ButtonHovered] = new Color(0.4f); // 按钮悬停
        style.Colors[(int)ImGuiCol.ButtonActive] = new Color(0.15f); // 按钮点击
        style.Colors[(int)ImGuiCol.FrameBg] = new Color(0.4f); // 输入背景色
        style.Colors[(int)ImGuiCol.FrameBgHovered] = new Color(0.5f); // 输入悬停颜色
        style.Colors[(int)ImGuiCol.FrameBgActive] = new Color(0.4f); // 输入点击颜色
        style.Colors[(int)ImGuiCol.Header] = new Color(0.3f); // 鼠标选中颜色
        style.Colors[(int)ImGuiCol.HeaderHovered] = new Color(0.4f); // 鼠标悬停颜色
        style.Colors[(int)ImGuiCol.HeaderActive] = new Color(0.5f); // 鼠标点击颜色
        style.Colors[(int)ImGuiCol.MenuBarBg] = Color.Gray; // 工具栏背景色
        style.Colors[(int)ImGuiCol.CheckMark] = Color.Gray; // 工具栏背景色
    }
}