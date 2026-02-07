using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;

namespace ET.Client;

public class PostProcessRenderPass : ARenderPassHandler
{
    public override void Awake(RenderComponent renderComponent)
    {
        Sdl2Window window = renderComponent.Scene().GetComponent<WindowComponent>().window;
        Texture postProcessResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
        renderComponent.Add("PostProcessResult", postProcessResult);
        Texture postProcessDepthResult = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.Width, (uint)window.Height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        renderComponent.Add("PostProcessDepthResult", postProcessDepthResult);
        renderComponent.Add("PostProcessFramebuffer", renderComponent.device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(postProcessDepthResult, postProcessResult)));
        
        PerspectiveCameraComponent camera = PerspectiveCameraComponent.Main;
        if (camera == null) return;
        TransformComponent cameraTransform = camera.GetParent<ViewObject>().GetComponent<TransformComponent>();
        DirectionLightComponent light = DirectionLightComponent.Main;
        if (light == null) return;
        TransformComponent lightTransform = light.GetParent<ViewObject>().GetComponent<TransformComponent>();
        
        MeshRenderInfo info = new();
        
        ushort[] indices = [0, 1, 2, 2, 1, 3];
        info.indexBuffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        renderComponent.device.UpdateBuffer(info.indexBuffer, 0, indices);

        static (Vector3, Vector3, Vector3, Vector3) GetViewDir(PerspectiveCameraComponent camera, TransformComponent cameraTransform)
        {
            float halfFov = camera.fovY * 0.5f;
            float aspect = camera.aspect;
            float near = camera.near;
            float far = camera.far;
            
            Vector3 toRight = cameraTransform.Right * near * MathF.Tan(halfFov * MathHelper.Deg2Rad) * aspect;
            Vector3 toTop = cameraTransform.Up * near * MathF.Tan(halfFov * MathHelper.Deg2Rad);
            
            Vector3 topLeft = cameraTransform.Forward * near - toRight + toTop;
            float scale = topLeft.Length() * far / near;
            topLeft = Vector3.Normalize(topLeft);
            topLeft *= scale;
            Vector3 topRight = cameraTransform.Forward * near + toRight + toTop;
            topRight = Vector3.Normalize(topRight);
            topRight *= scale;
            Vector3 bottomRight = cameraTransform.Forward * near + toRight - toTop;
            bottomRight = Vector3.Normalize(bottomRight);
            bottomRight *= scale;
            Vector3 bottomLeft = cameraTransform.Forward * near - toRight - toTop;
            bottomLeft = Vector3.Normalize(bottomLeft);
            bottomLeft *= scale;
            
            return (topLeft, topRight, bottomRight, bottomLeft);
        }
        (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) = GetViewDir(camera, cameraTransform);
        PostProcessVertex[] vs =
        [
            new() { position = new Vector2(-1, 1), dir =  topLeft },
            new() { position = new Vector2(1, 1), dir = topRight },
            new() { position = new Vector2(-1, -1), dir = bottomLeft },
            new() { position = new Vector2(1, -1), dir = bottomRight }
        ];
        info.vertexBuffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<PostProcessVertex>()), BufferUsage.VertexBuffer));
        renderComponent.device.UpdateBuffer(info.vertexBuffer, 0, vs);
        
        Texture shadingResult = renderComponent.Get<Texture>("ShadingResult");
        (Sampler shadingResultSampler, ResourceLayoutElementDescription shadingResultElement, ResourceLayoutElementDescription shadingResultSamplerElement) = renderComponent.device.CreateTexture("shadingResult", shadingResult);
        info.textures["shadingResult"] = shadingResult;
        info.samplers["shadingResult"] = shadingResultSampler;
        info.elements.Add(shadingResultElement);
        info.elements.Add(shadingResultSamplerElement);
        info.binds.Add(shadingResult);
        info.binds.Add(shadingResultSampler);
        
        Texture shadingDepthResult = renderComponent.Get<Texture>("ShadingDepthResult");
        var (shadingDepthResultSampler, shadingDepthResultElement, shadingDepthResultSamplerElement) = renderComponent.device.CreateTexture("shadingDepthResult", shadingDepthResult);
        info.textures["shadingDepthResult"] = shadingDepthResult;
        info.samplers["shadingDepthResult"] = shadingDepthResultSampler;
        info.elements.Add(shadingDepthResultElement);
        info.elements.Add(shadingDepthResultSamplerElement);
        info.binds.Add(shadingDepthResult);
        info.binds.Add(shadingDepthResultSampler);
        
        Texture shadingNormalResult = renderComponent.Get<Texture>("ShadingNormalResult");
        var (shadingNormalResultSampler, shadingNormalResultElement, shadingNormalResultSamplerElement) = renderComponent.device.CreateTexture("shadingNormalResult", shadingNormalResult);
        info.textures["shadingNormalResult"] = shadingNormalResult;
        info.samplers["shadingNormalResult"] = shadingNormalResultSampler;
        info.elements.Add(shadingNormalResultElement);
        info.elements.Add(shadingNormalResultSamplerElement);
        info.binds.Add(shadingNormalResult);
        info.binds.Add(shadingNormalResultSampler);
        
        Texture shadingPositionResult = renderComponent.Get<Texture>("ShadingPositionResult");
        var (shadingPositionResultSampler, shadingPositionResultElement, shadingPositionResultSamplerElement) = renderComponent.device.CreateTexture("shadingPositionResult", shadingPositionResult);
        info.textures["shadingPositionResult"] = shadingPositionResult;
        info.samplers["shadingPositionResult"] = shadingPositionResultSampler;
        info.elements.Add(shadingPositionResultElement);
        info.elements.Add(shadingPositionResultSamplerElement);
        info.binds.Add(shadingPositionResult);
        info.binds.Add(shadingPositionResultSampler);
        
        (DeviceBuffer cameraBuffer, ResourceLayoutElementDescription cameraElement) = renderComponent.device.CreateUniform("Camera", new PostProcess_CameraUniform() { view = camera.View(), projection = camera.Projection(), worldPos = cameraTransform.worldPosition.ToVector4() });
        info.uniformBuffers["Camera"] = cameraBuffer;
        info.binds.Add(cameraBuffer);
        info.elements.Add(cameraElement);
        
        (DeviceBuffer lightBuffer, ResourceLayoutElementDescription lightElement) = renderComponent.device.CreateUniform("Light", new PostProcess_LightUniform() { view = light.View(), projection = light.Projection(), dir = lightTransform.Forward, color = light.color, intensity = light.intensity, worldPos = lightTransform.worldPosition.ToVector4() });
        info.uniformBuffers["Light"] = lightBuffer;
        info.binds.Add(lightBuffer);
        info.elements.Add(lightElement);
        
        /*(DeviceBuffer atmosParamsBuffer, ResourceLayoutElementDescription atmosParamsElement) = renderComponent.device.CreateUniform("AtmosParams", new PostProcess_AtmosParamsUniform()
        {
            planetRadius = 6371000f,
            topRadius = 6451000f,
            rayleighScaleH = 8000,
            mieScaleH = 1200,
            betaRayleigh = new Vector3(5.8e-6f, 13.5e-6f, 33.1e-6f),
            betaMie = new Vector3(2e-5f, 2e-5f, 2e-5f),
            mieG = 0.8f,
        });
        info.uniformBuffers["AtmosParams"] = atmosParamsBuffer;
        info.binds.Add(atmosParamsBuffer);
        info.elements.Add(atmosParamsElement);*/
        
        var resourceLayout = renderComponent.device.CreateResourceLayout(info.elements.ToArray());
        info.resourceSet = renderComponent.device.CreateResourceSet(resourceLayout, info.binds.ToArray());
        
        info.pipeline = renderComponent.device.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = new DepthStencilStateDescription()
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual,
            },
            RasterizerState = new RasterizerStateDescription
            (
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                true,
                false
            ),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = [resourceLayout],
            ShaderSet = new ShaderSetDescription
            (
                [PostProcessVertex.GetLayout()],
                renderComponent.device.ResourceFactory.CreateFromSpirv
                (
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"Shaders\\PostProcess.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"Shaders\\PostProcess.frag.spv"), "main")
                )
            ),
            Outputs = renderComponent.Get<Framebuffer>("PostProcessFramebuffer").OutputDescription,
        });
        
        renderComponent.Add("PostProcessInfo", info);
    }

    public override void Update(RenderComponent renderComponent)
    {
        if (!renderComponent.TryGet("PostProcessFramebuffer", out Framebuffer framebuffer)) return;
        renderComponent.commandList.SetFramebuffer(framebuffer);
        renderComponent.commandList.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 1.0f));
        renderComponent.commandList.ClearDepthStencil(1, 0);
        
        if (!renderComponent.TryGet("PostProcessInfo", out MeshRenderInfo info)) return;
        
        renderComponent.commandList.SetPipeline(info.pipeline);
        renderComponent.commandList.SetVertexBuffer(0, info.vertexBuffer);
        renderComponent.commandList.SetIndexBuffer(info.indexBuffer, IndexFormat.UInt16);
        renderComponent.commandList.SetGraphicsResourceSet(0, info.resourceSet);
        renderComponent.commandList.DrawIndexed(6, 1, 0, 0, 0);
    }

    public override void LateUpdate(RenderComponent renderComponent)
    {
        
    }
}