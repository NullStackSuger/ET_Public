using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace ET.Client;

[AShader(typeof(ShadingRenderPass))]
public class DefaultShadingShader : AShaderHandler
{
    public override MeshRenderInfo Awake(RenderComponent renderComponent, MeshComponent meshComponent)
    {
        MeshRenderInfo info = new();
        
        // Update Vertex Input
        ShadingVertex[] vs = new ShadingVertex[meshComponent.meshInfo.positions.Length];
        for (int j = 0; j < vs.Length; j++)
        {
            vs[j] = new ShadingVertex() { position = meshComponent.meshInfo.positions[j], uv = meshComponent.meshInfo.uvs[j], normal = meshComponent.meshInfo.normals[j] };
        }
        info.indexBuffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(meshComponent.meshInfo.indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        renderComponent.device.UpdateBuffer(info.indexBuffer, 0, meshComponent.meshInfo.indices);
        info.vertexBuffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<ShadingVertex>()), BufferUsage.VertexBuffer));
        renderComponent.device.UpdateBuffer(info.vertexBuffer, 0, vs);
        renderComponent.Set("Vertices", vs);
        
        // Update Uniform Buffer
        DirectionLightComponent light = DirectionLightComponent.Main;
        if (light == null) return null;
        PerspectiveCameraComponent camera = PerspectiveCameraComponent.Main;
        if (camera == null) return null;
        TransformComponent transform = meshComponent.GetParent<ViewObject>().GetComponent<TransformComponent>();
        TransformComponent lightTransform = light.GetParent<ViewObject>().GetComponent<TransformComponent>();
        TransformComponent cameraTransform = camera.GetParent<ViewObject>().GetComponent<TransformComponent>();
        
        (DeviceBuffer mBuffer, ResourceLayoutElementDescription mElement) = renderComponent.device.CreateUniform("M", new Shading_MUniform() { model = transform.Model });
        info.uniformBuffers["M"] = mBuffer;
        info.binds.Add(mBuffer);
        info.elements.Add(mElement);
        
        (DeviceBuffer vpBuffer, ResourceLayoutElementDescription vpElement) = renderComponent.device.CreateUniform("VP", new Shading_VPUniform() { view = camera.View(), projection = camera.Projection() });
        info.uniformBuffers["VP"] = vpBuffer;
        info.binds.Add(vpBuffer);
        info.elements.Add(vpElement);
        
        (DeviceBuffer lightBuffer, ResourceLayoutElementDescription lightElement) = renderComponent.device.CreateUniform("Light", new Shading_LightUniform() { view = light.View(), projection = light.Projection(), dir = lightTransform.Forward, color = light.color, intensity = light.intensity, worldPos = lightTransform.worldPosition.ToVector4() });
        info.uniformBuffers["Light"] = lightBuffer;
        info.binds.Add(lightBuffer);
        info.elements.Add(lightElement);
        
        (DeviceBuffer cameraBuffer, ResourceLayoutElementDescription cameraElement) = renderComponent.device.CreateUniform("Camera", new Shading_CameraUniform() { worldPos = cameraTransform.worldPosition.ToVector4() });
        info.uniformBuffers["Camera"] = cameraBuffer;
        info.binds.Add(cameraBuffer);
        info.elements.Add(cameraElement);

        Texture shadowMap = renderComponent.Get<Texture>("ShadowMap");
        (Sampler shadowMapSampler, ResourceLayoutElementDescription textureElement, ResourceLayoutElementDescription samplerElement) = renderComponent.device.CreateTexture("shadowMap", shadowMap);
        info.textures["shadowMap"] = shadowMap;
        info.samplers["shadowMap"] = shadowMapSampler;
        info.elements.Add(textureElement);
        info.elements.Add(samplerElement);
        info.binds.Add(shadowMap);
        info.binds.Add(shadowMapSampler);
        
        var resourceLayout = renderComponent.device.CreateResourceLayout(info.elements.ToArray());
        info.resourceSet = renderComponent.device.CreateResourceSet(resourceLayout, info.binds.ToArray());

        // Update Pipeline
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
                [ShadingVertex.GetLayout()],
                renderComponent.device.ResourceFactory.CreateFromSpirv
                (
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"Shaders\\Shading.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"Shaders\\Shading.frag.spv"), "main")
                )
            ),
            Outputs = renderComponent.Get<Framebuffer>("ShadingFramebuffer").OutputDescription,
        });
        
        return info;
    }

    public override void Update(RenderComponent renderComponent, MeshComponent meshComponent, MeshRenderInfo info)
    {
        // Update Vertex
        if (meshComponent.Parent.GetComponent(out AnimatorComponent animatorComponent))
        {
            var vs = renderComponent.Get<ShadingVertex[]>("Vertices");
            for (int i = 0; i < animatorComponent.positions.Count; i++)
            {
                vs[i].position = animatorComponent.positions[i];
            }
            renderComponent.device.UpdateBuffer(info.vertexBuffer, 0, vs);
        }
        
        // Update M VP Uniform Buffer
        DirectionLightComponent light = DirectionLightComponent.Main;
        if (light == null) return;
        PerspectiveCameraComponent camera = PerspectiveCameraComponent.Main;
        if (camera == null) return;
        TransformComponent transform = meshComponent.GetParent<ViewObject>().GetComponent<TransformComponent>();
        TransformComponent lightTransform = light.GetParent<ViewObject>().GetComponent<TransformComponent>();
        TransformComponent cameraTransform = camera.GetParent<ViewObject>().GetComponent<TransformComponent>();
        
        renderComponent.device.UpdateUniform(info.uniformBuffers["M"], new Shading_MUniform() { model = transform.Model });
        renderComponent.device.UpdateUniform(info.uniformBuffers["VP"], new Shading_VPUniform() { view = camera.View(), projection = camera.Projection() });
        renderComponent.device.UpdateUniform(info.uniformBuffers["Light"], new Shading_LightUniform() { view = light.View(), projection = light.Projection(), dir = lightTransform.Forward, color = light.color, intensity = light.intensity, worldPos = lightTransform.worldPosition.ToVector4() });
        renderComponent.device.UpdateUniform(info.uniformBuffers["Camera"], new Shading_CameraUniform() { worldPos = cameraTransform.worldPosition.ToVector4() });
    }
}