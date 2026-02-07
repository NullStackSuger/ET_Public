using System.Numerics;
using ET.Client;
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
        var objs = scene.AddComponent<ViewObjectComponent>();
        
        ViewObject phyScene = objs.AddChild<ViewObject, string>("phyScene");
        phyScene.AddComponent<TransformComponent, Vector3>(Vector3.Zero);
        PhysicsSceneComponent.Main = phyScene.AddComponent<PhysicsSceneComponent>();
        ViewObject camera = objs.AddChild<ViewObject, string>("camera");
        camera.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 4, -2.5f), Quaternion.Identity, Vector3.One);
        PerspectiveCameraComponent.Main = camera.AddComponent<PerspectiveCameraComponent, float, float, float, float>(60, window.Aspect(), 0.1f, 100);
        ViewObject light = objs.AddChild<ViewObject, string>("light");
        light.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(Vector3.Zero, new Vector3(90, 0, 0).ToQuaternion(), Vector3.One);
        var lightCamera = light.AddComponent<OrthographicCameraComponent, float, float, float, float>(window.Aspect(), 10, 0.1f, 100f);
        DirectionLightComponent.Main = light.AddComponent<DirectionLightComponent, float, Color, OrthographicCameraComponent>(2.5f, new Color(1, 1, 1/*23.47f, 21.31f, 20.79f*/), lightCamera);
        
        var render = scene.AddComponent<RenderComponent, Type[]>([typeof(PrepareRenderPass), typeof(ShadowRenderPass), typeof(ShadingRenderPass), typeof(PostProcessRenderPass), typeof(EditorWindowRenderPass)]);
        var editorWindows = scene.AddComponent<EditorWindowComponent, int, int, GraphicsDevice, OutputDescription>(window.window.Width, window.window.Height, render.device, render.device.MainSwapchain.Framebuffer.OutputDescription);

        scene.AddComponent<EditorComponent>();
        
        ViewObject cube = MeshComponentSystem.Load("Objs/cube.obj", objs);
        TransformComponent cubeTransform = cube.GetComponent<TransformComponent>();
        cubeTransform.localPosition = new Vector3(0, 8, 10);
        cubeTransform.localRotation = new Vector3(45, 0, 45).ToQuaternion();
        /*Material mat = PhysicsComponent.Instance.physics.CreateMaterial(0.1f, 0.1f, 0.1f);
        cube.AddComponent<PhysicsRigidActorComponent, Vector3, float, Material, float>(Vector3.Zero, 1f, mat, 1f).callback = typeof(DefaultCollisionHandler);*/

        ViewObject plane = MeshComponentSystem.Load("Objs/cube.obj", objs);
        plane.name = "Plane";
        TransformComponent planeTransform = plane.GetComponent<TransformComponent>();
        planeTransform.localPosition = new Vector3(0, 0, 10);
        planeTransform.localRotation = new Vector3(0, 0, 0).ToQuaternion();
        planeTransform.localScale = new Vector3(10, 0.1f, 10);
        
        /*ViewObject player = MeshComponentSystem.Load("Objs/model.dae", objs);
        player.name = "Player";
        TransformComponent transformComponent1 = player.GetComponent<TransformComponent>();
        transformComponent1.localPosition = new Vector3(0, 8, 15);
        transformComponent1.localRotation = new Vector3(-90, 0, 0).ToQuaternion();
        var animator = player.GetComponent<AnimatorComponent>();
        animator.Play("Default Animation");*/
        /*player.AddComponent<AudioComponent>().PlayImmediately(new SignalGenerator()
        {
            Gain = 0.2f,
            Frequency = 500,
            Type = SignalGeneratorType.Sin
        }.Take(TimeSpan.FromSeconds(5)));*/
        
        //scene.AddComponent<ParticleComponent, float, string>(5, "Objs/cube.obj").Play(50);

        //scene.AddComponent<MassSpringComponent, Vector3, float>(new Vector3(0, -9.81f, 0), 1f / 60f).Test();

        // 这里如果给meshInfo赋值回去会有问题, 因为normals等没有重新计算导致数组越界
        /*MeshComponent meshComponent = cube.GetComponent<MeshComponent>();
        MeshInfo meshInfo = meshComponent.meshInfo;
        (Vector3[] vertices, ushort[] indices) = LoopSubdivision.Subdivide(meshInfo.positions, meshInfo.indices);*/
        
        editorWindows.AddComponent<BackGroundComponent>();
        editorWindows.AddComponent<DetailEditorComponent>();
        editorWindows.AddComponent<FileEditorComponent, string[]>([".ignore"]);
        editorWindows.AddComponent<LogEditorComponent>();
        editorWindows.AddComponent<SceneEditorComponent>();
        editorWindows.AddComponent<ViewEditorComponent>();
        
        await ETTask.CompletedTask;
    }
}