using System.Numerics;

namespace ET.Client;

public class PrepareRenderPass : ARenderPassHandler
{
    // 1.对所有脏Mesh的Shader触发Awake
    // 2.获取所有Mesh
    // 3.进行视锥体剔除
    // 4.进行深度排序

    public override void Awake(RenderComponent renderComponent)
    {
        
    }

    public override void Update(RenderComponent renderComponent)
    {
        Queue<EntityRef<ViewObject>> dirtyMeshes = renderComponent.GetComponent<DirtyMeshComponent>().dirtyMeshes;
        if (dirtyMeshes.Count <= 0) return; // 如果没有DirtyMesh就不需要重建
        
        // 1.对所有脏Mesh的Shader触发Awake
        while (dirtyMeshes.Count > 0)
        {
            ViewObject obj = dirtyMeshes.Dequeue();
            if (obj == null) continue;
            if (!obj.GetComponent(out MeshComponent meshComponent)) continue;

            foreach ((Type renderPassType, Type shaderType) in meshComponent.shaders)
            {
                AShaderHandler handler = ShaderDispatcher.Instance[renderPassType];
                if (handler == null) continue;
                
                var info = handler.Awake(renderComponent, meshComponent);
                if (info == null)
                {
                    Log.Instance.Error($"Shader Error: {handler.GetType().Name}");
                    continue;
                }
                meshComponent.renderInfos[shaderType] = info;
            }
        }
            
        // 2.获取所有Mesh
        // 3.进行视锥体剔除
        List<ViewObject> objs = new();
        
        Scene scene = renderComponent.Scene();
        
        PerspectiveCameraComponent camera = PerspectiveCameraComponent.Main;
        if (camera == null) return;
        Frustum frustum = camera.Frustum();
        
        ViewObjectComponent viewObjectComponent = scene.GetComponent<ViewObjectComponent>();
        
        foreach (ViewObject child in viewObjectComponent.Foreach())
        {
            if (!child.GetComponent(out MeshComponent meshComponent)) continue;
            if (!child.GetComponent(out TransformComponent transformComponent)) continue;
            
            AABB aabb = meshComponent.AABB().Transform(transformComponent.Model());

            
            objs.Add(child);
            
            /*if (frustum.Intersects(aabb))
            {
                objs.Add(child);
            }*/
        }
        
        // 4.进行深度排序
        Vector3 cameraWorldPosition = camera.ViewObject().GetComponent<TransformComponent>().GetWorldPosition();
        objs.Sort((a, b) =>
        {
            float da = Vector3.DistanceSquared(cameraWorldPosition, a.GetComponent<TransformComponent>().GetWorldPosition());
            float db = Vector3.DistanceSquared(cameraWorldPosition, b.GetComponent<TransformComponent>().GetWorldPosition());
            return da.CompareTo(db); // 近 -> 远
        });

        renderComponent.Set("Objs", objs.ToArray());
    }
}