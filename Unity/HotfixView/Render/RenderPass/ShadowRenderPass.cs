using System.Numerics;
using Veldrid;

namespace ET.Client;

public class ShadowRenderPass : ARenderPassHandler
{
    public override void Awake(RenderComponent renderComponent)
    {
        WindowComponent window = renderComponent.Scene().GetComponent<WindowComponent>();
        Texture shadowMap = renderComponent.device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)window.window.Width, (uint)window.window.Height, 1, 1, PixelFormat.D24_UNorm_S8_UInt, TextureUsage.DepthStencil | TextureUsage.Sampled));
        renderComponent.Add("ShadowMap", shadowMap);
        renderComponent.Add("ShadowFramebuffer", renderComponent.device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(shadowMap)));
        
        DirectionLightComponent light = DirectionLightComponent.Main;
        if (light == null) return;
        PerspectiveCameraComponent camera = PerspectiveCameraComponent.Main;
        if (camera == null) return;
        
        // CSM
        // 1.已知主相机视锥体, 把它分成4段
        // 2.计算出每段的8个顶点, *光源相机View矩阵
        // 3.计算AABB, 得到光源相机Projection矩阵
        // 4.计算ShadowMap
        // Shading 
        // 1.根据当前深度, 找到要用哪张ShadowMap
        // 2.把worldPos*光源View矩阵
        // 3.计算采样ShadowMap的uv(和当前片元的uv不同)
        
        // 这里我只取第0段的了
        GetVP(camera, light, 0);
    }
    
    static (Matrix4x4, Matrix4x4) GetVP(PerspectiveCameraComponent camera, DirectionLightComponent light, int i)
    {
        // 这里得到的是世界坐标系下
        Vector3[] corners = GetFrustumCornersWorld(camera.Projection(), camera.View());
        float near = i / 4.0f;
        float far = (i + 1) / 4.0f;
        Vector3[] subs = GetSubFrustumCorners(corners, near, far);

        Vector3 center = new Vector3();
        foreach (Vector3 sub in subs)
        {
            center += sub;
        }
        center /= subs.Length;

        TransformComponent lightTransform = light.GetParent<ViewObject>().GetComponent<TransformComponent>();
        Vector3 lightDir = lightTransform.Forward;
        float radius = 0;
        foreach (Vector3 sub in subs)
        {
            radius = Math.Max(radius, Vector3.Distance(center, sub));
        }
        Vector3 eye = center - lightDir * radius;
        lightTransform.worldPosition = eye;
        Matrix4x4 lightView = light.View();
            
        AABB aabb = new AABB();
        foreach (Vector3 sub in subs)
        {
            Vector3 ls = Vector4.Transform(sub.ToVector4(), lightView).ToVector3();
            aabb.Encapsulate(ls);
        }
        const float z_mult = 10.0f;
        if (aabb.Min.Z < 0)
        {
            aabb.Min.Z *= z_mult;
        }
        else
        {
            aabb.Min.Z /= z_mult;
        }
        if (aabb.Max.Z < 0)
        {
            aabb.Max.Z /= z_mult;
        }
        else
        {
            aabb.Max.Z *= z_mult;
        }

        light.SetProjection(aabb.Min.X, aabb.Max.X, aabb.Min.Y, aabb.Max.Y, aabb.Min.Z, aabb.Max.Z);
        Matrix4x4 lightProjection = light.Projection();
        
        return (lightView, lightProjection);
        
        static Vector3[] GetFrustumCornersWorld(Matrix4x4 proj, Matrix4x4 view) 
        {
            Matrix4x4.Invert(view * proj, out Matrix4x4 invVP);
            List<Vector3> corners = new(8);
            for (int k = 0; k < 2; k++)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector4 ndcPos = new Vector4(2.0f * i - 1.0f, 2.0f * j - 1.0f, 2.0f * k - 1.0f, 1.0f);
                        Vector4 worldPos = Vector4.Transform(ndcPos, invVP);
                        worldPos /= worldPos.W;
                        corners.Add(new Vector3(worldPos.X, worldPos.Y, worldPos.Z));
                    }
                }
            }

            return corners.ToArray();
        }
        static Vector3[] GetSubFrustumCorners(Vector3[] worldCorners, float near, float far)
        {
            Vector3[] outSub = new Vector3[8];
            for (int i = 0; i < 4; ++i)
            {
                outSub[i] = Vector3.Lerp(worldCorners[i], worldCorners[4 + i], near);
                outSub[4 + i] = Vector3.Lerp(worldCorners[i], worldCorners[4 + i], far);
            }
            return outSub;
        }
    }
    
    public override void Update(RenderComponent renderComponent)
    {
        if (!renderComponent.TryGet("ShadowFramebuffer", out Framebuffer framebuffer)) return;
        renderComponent.commandList.SetFramebuffer(framebuffer);
        renderComponent.commandList.ClearDepthStencil(1, 0);
        
        if (!renderComponent.TryGet("Objs", out ViewObject[] objs)) return;
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

    public override void LateUpdate(RenderComponent renderComponent)
    {
        
    }
}