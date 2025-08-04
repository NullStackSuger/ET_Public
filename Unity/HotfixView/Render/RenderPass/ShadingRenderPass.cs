using Veldrid;
using Veldrid.Sdl2;

namespace ET.Client;

public class ShadingRenderPass : ARenderPassHandler
{
    public override void Awake(RenderComponent renderComponent)
    {
        Sdl2Window window = renderComponent.Scene().GetComponent<WindowComponent>().window;
        Texture shadingResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        renderComponent.Add("ShadingResult", shadingResult);
        // ShadowMap是从光源画的深度图, 这是从相机画的深度图
        Texture shadingDepthResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil));
        renderComponent.Add("ShadingDepthResult", shadingDepthResult);
        renderComponent.Add("ShadingFramebuffer", renderComponent.device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadingDepthResult, shadingResult)));
    }

    public override void Update(RenderComponent renderComponent)
    {
        Framebuffer framebuffer = renderComponent.Get<Framebuffer>("ShadingFramebuffer");
        if (framebuffer == null) return;
        renderComponent.commandList.SetFramebuffer(framebuffer);
        renderComponent.commandList.ClearColorTarget(0, new RgbaFloat(0.1f, 0.1f, 0.1f, 1.0f));
        renderComponent.commandList.ClearDepthStencil(1, 0);

        ViewObject[] objs = renderComponent.Get<ViewObject[]>("Objs");
        if (objs == null) return;
        foreach (ViewObject obj in objs)
        {
            MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
            Type shaderType = meshComponent.shaders[typeof(ShadingRenderPass)];
            MeshRenderInfo info = meshComponent.renderInfos[shaderType];
            
            AShaderHandler handler = ShaderDispatcher.Instance[typeof(ShadingRenderPass)];
            handler.Update(renderComponent, meshComponent, info);
            
            renderComponent.commandList.SetPipeline(info.pipeline);
            renderComponent.commandList.SetVertexBuffer(0, info.vertexBuffer);
            renderComponent.commandList.SetIndexBuffer(info.indexBuffer, IndexFormat.UInt16);
            renderComponent.commandList.SetGraphicsResourceSet(0, info.resourceSet);
            renderComponent.commandList.DrawIndexed((uint)meshComponent.meshInfo.indices.Length, 1, 0, 0, 0);
        }
    }
}