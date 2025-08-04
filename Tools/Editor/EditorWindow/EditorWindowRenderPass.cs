using ET.Client;
using Veldrid;

namespace ET.Editor;

public class EditorWindowRenderPass : ARenderPassHandler
{
    public override void Awake(RenderComponent renderComponent)
    {
    }

    public override void Update(RenderComponent renderComponent)
    {
        Scene scene = renderComponent.Scene();
        EditorWindowComponent editorWindows = scene.GetComponent<EditorWindowComponent>();
        InputSnapshot snapshot = scene.GetComponent<InputComponent>().snapshot;
        
        renderComponent.commandList.SetFramebuffer(renderComponent.device.MainSwapchain.Framebuffer);
        renderComponent.commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        
        editorWindows.uiController.Update(0.33f, snapshot);

        foreach (Entity entity in editorWindows.Components.Values)
        {
            if (entity is not IUi)
            {
                continue;
            }
            
            UiHelper.UiUpdate(entity);
        }
        
        editorWindows.uiController.Render(renderComponent.device, renderComponent.commandList);
    }
}