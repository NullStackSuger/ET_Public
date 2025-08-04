using Veldrid;

namespace ET.Client;

public class UiRenderPass : ARenderPassHandler
{
    public override void Awake(RenderComponent renderComponent)
    {
    }

    public override void Update(RenderComponent renderComponent)
    {
        Scene scene = renderComponent.Scene();
        UiComponent uis = scene.GetComponent<UiComponent>();
        InputSnapshot snapshot = scene.GetComponent<InputComponent>().snapshot;
        
        Framebuffer framebuffer = renderComponent.Get<Framebuffer>("ShadingFramebuffer");
        if (framebuffer == null) return;
        renderComponent.commandList.SetFramebuffer(framebuffer);
        renderComponent.commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        
        uis.uiController.Update(0.33f, snapshot);

        foreach (Entity entity in uis.Components.Values)
        {
            if (entity is not IUi)
            {
                continue;
            }
            
            UiHelper.UiUpdate(entity);
        }
        
        uis.uiController.Render(renderComponent.device, renderComponent.commandList);
    }
}