using System.Numerics;
using ET.Client;
using PhysX;
using Veldrid;

namespace ET.Editor;

[Invoke(SceneType.Editor)]
public class FiberInit_Editor : AInvokeHandler<FiberInit, ETTask>
{
    public override async ETTask Handle(FiberInit args)
    {
        Scene scene = args.Fiber.Root;
        
        scene.AddComponent<TimerComponent>();
        scene.AddComponent<CoroutineLockComponent>();
        scene.AddComponent<ObjectWait>();
        scene.AddComponent<OpenAiComponent>();
        
        var window = scene.AddComponent<WindowComponent>();
        scene.AddComponent<InputComponent>();
        scene.AddComponent<ViewObjectComponent>();
        var render = scene.AddComponent<RenderComponent, Type[]>([typeof(PrepareRenderPass), typeof(ShadowRenderPass), typeof(ShadingRenderPass), typeof(EditorWindowRenderPass)]);
        scene.AddComponent<EditorWindowComponent, int, int, GraphicsDevice, OutputDescription>(window.window.Width, window.window.Height, render.device, render.device.MainSwapchain.Framebuffer.OutputDescription);

        scene.AddComponent<EditorComponent>();
        
        TestScene(scene);
        
        await ETTask.CompletedTask;
    }
    
    private static void TestScene(Scene currentScene)
    {
        var window = currentScene.GetComponent<WindowComponent>();
        var editorWindows = currentScene.GetComponent<EditorWindowComponent>();
        var objs = currentScene.GetComponent<ViewObjectComponent>();
        
        ViewObject phyScene = objs.AddChild<ViewObject, string>("phyScene");
        phyScene.AddComponent<TransformComponent, Vector3>(Vector3.Zero);
        PhysicsSceneComponent.Main = phyScene.AddComponent<PhysicsSceneComponent>();
        
        ViewObject camera = objs.AddChild<ViewObject, string>("camera");
        camera.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 4, -2.5f), Quaternion.Identity, Vector3.One);
        PerspectiveCameraComponent.Main = camera.AddComponent<PerspectiveCameraComponent, float, float, float, float>(60, window.Aspect(), 0.1f, 100);
        
        ViewObject light = objs.AddChild<ViewObject, string>("light");
        light.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 10, 10), new Vector3(90, 0, 0).ToQuaternion(), Vector3.One);
        var lightCamera = light.AddComponent<OrthographicCameraComponent, float, float, float, float>(window.Aspect(), 10, 0.1f, 100f);
        DirectionLightComponent.Main = light.AddComponent<DirectionLightComponent, float, Color, OrthographicCameraComponent>(10f, Color.White, lightCamera);
        
        ViewObject cube = MeshComponentSystem.Load("Objs/cube.obj"/*"Objs/model.dae"*/, objs);
        TransformComponent transformComponent = cube.GetComponent<TransformComponent>();
        transformComponent.localPosition = new Vector3(0, 8, 10);
        transformComponent.localRotation = new Vector3(45, 0, 45).ToQuaternion();
        /*Material mat = PhysicsComponent.Instance.physics.CreateMaterial(0.1f, 0.1f, 0.1f);
        cube.AddComponent<PhysicsRigidActorComponent, Vector3, float, Material, float>(Vector3.Zero, 1f, mat, 1f).callback = typeof(DefaultCollisionHandler);*/

        ViewObject plane = MeshComponentSystem.Load("Objs/cube.obj", objs);
        plane.name = "Plane";
        transformComponent = plane.GetComponent<TransformComponent>();
        transformComponent.localPosition = new Vector3(0, 0, 10);
        transformComponent.localRotation = new Vector3(0, 0, 0).ToQuaternion();
        transformComponent.localScale = new Vector3(10, 0.1f, 10);
        /*ViewObject plane = objs.AddChild<ViewObject, string>("plane");
        plane.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, -3, 10), new Vector3(-20, 0, 0).ToQuaternion(), new Vector3(3, 0.5f, 3));
        plane.AddComponent<MeshComponent, string>("Objs/cube.obj");*/
        /*plane.AddComponent<PhysicsRigidActorComponent, Vector3, Vector3>(Vector3.Zero, new Vector3(1, 1, 1)).callback = typeof(DefaultCollisionHandler);*/
        
        /*ViewObject player = objs.AddChild<ViewObject, string>("player");
        player.AddComponent<TransformComponent, Vector3>(new Vector3(10, 10, 10));
        player.AddComponent<PhysicsCharacterComponent, Material, Vector3>(mat, Vector3.One);
        player.AddComponent<AudioComponent>()/*.PlayImmediately(new SignalGenerator()
        {
            Gain = 0.2f,
            Frequency = 500,
            Type = SignalGeneratorType.Sin
        }.Take(TimeSpan.FromSeconds(5)))#1#;

        /*var animator = cube.GetComponent<AnimatorComponent>();
        animator.Play("Default Animation");#1#*/
        
        editorWindows.AddComponent<BackGroundComponent>();
        editorWindows.AddComponent<DetailEditorComponent>();
        editorWindows.AddComponent<FileEditorComponent, string[]>([".ignore"]);
        editorWindows.AddComponent<LogEditorComponent>();
        editorWindows.AddComponent<SceneEditorComponent>();
        editorWindows.AddComponent<ViewEditorComponent>();
        
        Log.Instance.Trace("Trace");
        Log.Instance.Debug("Debug1");
        Log.Instance.Debug("Debug2");
        Log.Instance.Info("Info");
        Log.Instance.Warning("Warning");
    }
}