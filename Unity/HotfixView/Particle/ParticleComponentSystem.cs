using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(ParticleComponent))]
public static partial class ParticleComponentSystem
{
    [EntitySystem]
    private static void Awake(this ParticleComponent self, float time, string resourcesPath)
    {
        self.time = time;
        self.resourcesPath = resourcesPath;
    }
    
    [EntitySystem]
    private static void Serialize(this ParticleComponent self)
    {
    }

    [EntitySystem]
    private static void Deserialize(this ParticleComponent self)
    {
    }

    [EntitySystem]
    private static void LateUpdate(this ParticleComponent self)
    {
        // TODO 粒子系统/删除有问题
        /*long deltaTime = Time.Instance.DeltaTime;
        ViewObjectComponent viewObjectComponent = self.Scene().GetComponent<ViewObjectComponent>();
        while (self.objs.TryDequeue(out var objRef, out long currentTime))
        {
            ViewObject obj = objRef;
            if (obj.IsDisposed) continue;
            
            currentTime += deltaTime;
            
            if (currentTime >= self.time)
            {
                viewObjectComponent.RemoveChild(obj.Id);
            }
            else
            {
                // 更新位置等操作
            
                self.objs.Enqueue(objRef, currentTime);
            }
        }*/
    }
    
    public static void Play(this ParticleComponent self, int count)
    {
        if (count <= 0) return;
        
        ViewObjectComponent viewObjectComponent = self.Scene().GetComponent<ViewObjectComponent>();
        Random random = new((int)DateTime.UtcNow.Ticks);
        
        ViewObject rowObj = MeshComponentSystem.Load(self.resourcesPath, viewObjectComponent);
        rowObj.name = "Particle";
        TransformComponent rowTransform = rowObj.GetComponent<TransformComponent>();
        rowTransform.localPosition = new Vector3(random.Next(-3, 3), random.Next(-3, 3), random.Next(-3, 3));
        rowTransform.localRotation = new Vector3(random.Next(-45, 45), random.Next(-45, 45), random.Next(-45, 45)).ToQuaternion();
        rowTransform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        self.objs.Enqueue(rowObj, 0);
        
        for (int i = 1; i < count; i++)
        {
            ViewObject obj = MeshComponentSystem.Clone(rowObj, $"{rowObj.name} {i}");
            obj.name = $"{self.resourcesPath} {i}";
            TransformComponent transform = obj.GetComponent<TransformComponent>();
            transform.localPosition = new Vector3(random.Next(-3, 3), random.Next(-3, 3), random.Next(-3, 3));
            transform.localRotation = new Vector3(random.Next(-45, 45), random.Next(-45, 45), random.Next(-45, 45)).ToQuaternion();
            transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            self.objs.Enqueue(obj, 0);
        }
    }
}