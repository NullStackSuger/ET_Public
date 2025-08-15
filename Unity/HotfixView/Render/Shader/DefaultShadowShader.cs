using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.SPIRV;

namespace ET.Client;

[AShader(typeof(ShadowRenderPass))]
public class DefaultShadowShader : AShaderHandler
{
    public override MeshRenderInfo Awake(RenderComponent renderComponent, MeshComponent meshComponent)
    {
        MeshRenderInfo info = new();
        
        // Update Vertex Input
        ShadowVertex[] vs = new ShadowVertex[meshComponent.meshInfo.positions.Length];
        for (int j = 0; j < vs.Length; j++)
        {
            vs[j] = new ShadowVertex() { position = meshComponent.meshInfo.positions[j] };
        }
        info.indexBuffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(meshComponent.meshInfo.indices.Length * sizeof(ushort)), BufferUsage.IndexBuffer));
        renderComponent.device.UpdateBuffer(info.indexBuffer, 0, meshComponent.meshInfo.indices);
        info.vertexBuffer = renderComponent.device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(vs.Length * Marshal.SizeOf<ShadowVertex>()), BufferUsage.VertexBuffer));
        renderComponent.device.UpdateBuffer(info.vertexBuffer, 0, vs);
        
        // Update Uniform Buffer
        DirectionLightComponent light = DirectionLightComponent.Main;
        if (light == null) return null;
        TransformComponent transform = meshComponent.GetParent<ViewObject>().GetComponent<TransformComponent>();
        
        (DeviceBuffer mBuffer, ResourceLayoutElementDescription mElement) = renderComponent.device.CreateUniform("M", new Shadow_MUniform() { model = transform.Model });
        info.uniformBuffers["M"] = mBuffer;
        info.binds.Add(mBuffer);
        info.elements.Add(mElement);
        
        (DeviceBuffer vpBuffer, ResourceLayoutElementDescription vpElement) = renderComponent.device.CreateUniform("VP", new Shadow_VPUniform() { view = light.View(), projection = light.Projection() });
        info.uniformBuffers["VP"] = vpBuffer;
        info.binds.Add(vpBuffer);
        info.elements.Add(vpElement);
        
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
                [ShadowVertex.GetLayout()],
                renderComponent.device.ResourceFactory.CreateFromSpirv
                (
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes($"Shaders\\Shadow.vert.spv"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes($"Shaders\\Shadow.frag.spv"), "main")
                )
            ),
            Outputs = renderComponent.Get<Framebuffer>("ShadowFramebuffer").OutputDescription,
        });

        return info;
    }

    public override void Update(RenderComponent renderComponent, MeshComponent meshComponent, MeshRenderInfo info)
    {
        // Update M VP Uniform Buffer
        DirectionLightComponent light = DirectionLightComponent.Main;
        if (light == null) return;
        TransformComponent transform = meshComponent.GetParent<ViewObject>().GetComponent<TransformComponent>();
        
        renderComponent.device.UpdateUniform(info.uniformBuffers["M"], new Shadow_MUniform() { model = transform.Model });
        renderComponent.device.UpdateUniform(info.uniformBuffers["VP"], new Shadow_VPUniform() { view = light.View(), projection = light.Projection() });
    }
}