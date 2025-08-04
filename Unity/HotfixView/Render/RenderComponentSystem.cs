using Veldrid;
using Veldrid.StartupUtilities;

namespace ET.Client;

// 需要注意的几个地方
// 左手坐标系 相机坐标系和其他物体相同(Forward指向Z+) Matrix4x4是列主序的

[EntitySystemOf(typeof(RenderComponent))]
public static partial class RenderComponentSystem
{
    [EntitySystem]
    private static void Awake(this RenderComponent self, Type[] types)
    {
        self.AddComponent<DirtyMeshComponent>();
            
        var window = self.Scene().GetComponent<WindowComponent>().window;
            
        self.device = VeldridStartup.CreateGraphicsDevice
        (
            window, new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, true, true)
            //,GraphicsBackend.Vulkan // TODO Vulkan_ERROR/窗口关闭时窗口大小会被更改的很大
        );
        self.commandList = self.device.ResourceFactory.CreateCommandList();
            
        self.types = types;
        foreach (Type type in types)
        {
            ARenderPassHandler handler = RenderPassDispatcher.Instance[type];
            handler.Awake(self);
        }
    }

    [EntitySystem]
    private static void LateUpdate(this RenderComponent self)
    {
        self.commandList.Begin();
            
        foreach (Type type in self.types)
        {
            ARenderPassHandler handler = RenderPassDispatcher.Instance[type];
            handler.Update(self);
        }
            
        self.commandList.End();
            
        self.device.SubmitCommands(self.commandList);
        self.device.SwapBuffers();
    }

    public static T Get<T>(this RenderComponent self, string name)
    {
        if (self.dic.ContainsKey(name))
        {
            return (T)self.dic[name];
        }
        else
        {
            return default;
        }
    }

    public static void Add(this RenderComponent self, string name, object value)
    {
        if (!self.dic.TryAdd(name, value))
        {
            Log.Instance.Error($"RenderComponent already contains: {name}");
        }
    }

    public static void Set(this RenderComponent self, string name, object value)
    {
        self.dic[name] = value;
    }
}