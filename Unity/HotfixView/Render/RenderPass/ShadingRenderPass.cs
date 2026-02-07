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
        Texture shadingDepthResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        renderComponent.Add("ShadingDepthResult", shadingDepthResult);
        Texture shadingNormalResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        renderComponent.Add("ShadingNormalResult", shadingNormalResult);
        Texture shadingPositionResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        renderComponent.Add("ShadingPositionResult", shadingPositionResult);
        renderComponent.Add("ShadingFramebuffer", renderComponent.device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadingDepthResult, shadingResult, shadingNormalResult, shadingPositionResult)));
        
        //ComputeHelper.TestComputeHandler(renderComponent);
        //ComputeHelper.TestCompute1Handler(renderComponent);
        //ComputeHelper.Atmosphere(renderComponent);
    }

    public override void Update(RenderComponent renderComponent)
    {
        if (!renderComponent.TryGet("ShadingFramebuffer", out Framebuffer framebuffer)) return;
        renderComponent.commandList.SetFramebuffer(framebuffer);
        renderComponent.commandList.ClearColorTarget(0, new RgbaFloat(0f, 0f, 0f, 1.0f));
        renderComponent.commandList.ClearColorTarget(1, new RgbaFloat(0f, 0f, 0f, 1.0f));
        renderComponent.commandList.ClearColorTarget(2, new RgbaFloat(0f, 0f, 0f, 1.0f));
        renderComponent.commandList.ClearDepthStencil(1, 0);
        
        if (!renderComponent.TryGet("Objs", out ViewObject[] objs)) return;
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

    public override void LateUpdate(RenderComponent renderComponent)
    {
        
    }
}