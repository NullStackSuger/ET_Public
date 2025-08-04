using Veldrid;
using Veldrid.Sdl2;

namespace ET.Client;

public class ShadowRenderPass : ARenderPassHandler
{
    public override void Awake(RenderComponent renderComponent)
    {
        // TODO 渲染系统/ShadowMap大小需要计算
        Sdl2Window window = renderComponent.Scene().GetComponent<WindowComponent>().window;
        Texture shadowMap = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        renderComponent.Add("ShadowMap", shadowMap);
        renderComponent.Add("ShadowFramebuffer", renderComponent.device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadowMap)));
    }

    public override void Update(RenderComponent renderComponent)
    {
        Framebuffer framebuffer = renderComponent.Get<Framebuffer>("ShadowFramebuffer");
        if (framebuffer == null) return;
        renderComponent.commandList.SetFramebuffer(framebuffer);
        renderComponent.commandList.ClearDepthStencil(1, 0);
        
        ViewObject[] objs = renderComponent.Get<ViewObject[]>("Objs");
        if (objs == null) return;
        foreach (ViewObject obj in objs)
        {
            MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
            Type shaderType = meshComponent.shaders[typeof(ShadowRenderPass)];
            MeshRenderInfo info = meshComponent.renderInfos[shaderType];
            
            AShaderHandler handler = ShaderDispatcher.Instance[typeof(ShadowRenderPass)];
            handler.Update(renderComponent, meshComponent, info);
            
            renderComponent.commandList.SetPipeline(info.pipeline);
            renderComponent.commandList.SetVertexBuffer(0, info.vertexBuffer);
            renderComponent.commandList.SetIndexBuffer(info.indexBuffer, IndexFormat.UInt16);
            renderComponent.commandList.SetGraphicsResourceSet(0, info.resourceSet);
            renderComponent.commandList.DrawIndexed((uint)meshComponent.meshInfo.indices.Length, 1, 0, 0, 0);
        }
    }
}