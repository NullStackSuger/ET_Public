using System.Numerics;
using Veldrid;

namespace ET.Client;

[Event(SceneType.Main)]
public class EntryClientHandler : AEvent<Scene, EntryClient>
{
    protected override async ETTask Run(Scene root, EntryClient args)
    {
        Scene currentScene = new Scene(root.Fiber(), SceneType.Current, root.InstanceId, IdGenerator.Instance.GenerateInstanceId());
        
        WindowComponent window = currentScene.AddComponent<WindowComponent>();
        currentScene.AddComponent<InputComponent>();
        currentScene.AddComponent<ViewObjectComponent>();
        RenderComponent render = currentScene.AddComponent<RenderComponent, Type[]>([typeof(PrepareRenderPass), typeof(ShadowRenderPass), typeof(ShadingRenderPass), typeof(UiRenderPass)]);
        currentScene.AddComponent<UiComponent, int, int, GraphicsDevice>(window.window.Width, window.window.Height, render.device);

        TestScene(currentScene);
        
        await ETTask.CompletedTask;
    }

    private static void TestScene(Scene currentScene)
    {
        var window = currentScene.GetComponent<WindowComponent>();
        var uis = currentScene.GetComponent<UiComponent>();
        var objs = currentScene.GetComponent<ViewObjectComponent>();
        
        ViewObject phyScene = objs.AddChild<ViewObject, string>("phyScene");
        phyScene.AddComponent<TransformComponent, Vector3>(Vector3.Zero);
        PhysicsSceneComponent.Main = phyScene.AddComponent<PhysicsSceneComponent>();
        
        ViewObject camera = objs.AddChild<ViewObject, string>("camera");
        camera.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 0, -2.5f), Quaternion.Identity, Vector3.One);
        PerspectiveCameraComponent.Main = camera.AddComponent<PerspectiveCameraComponent, float, float, float, float>(60, window.Aspect(), 0.1f, 100);
        
        ViewObject light = objs.AddChild<ViewObject, string>("light");
        light.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 10, 10), new Vector3(90, 0, 0).ToQuaternion(), Vector3.One);
        var lightCamera = light.AddComponent<OrthographicCameraComponent, float, float, float, float>(window.Aspect(), 10, 0.1f, 100f);
        DirectionLightComponent.Main = light.AddComponent<DirectionLightComponent, float, Color, OrthographicCameraComponent>(0.8f, Color.White, lightCamera);

        ViewObject cube = objs.AddChild<ViewObject, string>("cube");
        cube.AddComponent<TransformComponent, Vector3, Quaternion, Vector3>(new Vector3(0, 10, 0), new Vector3(45, 0, 45).ToQuaternion(), Vector3.One);
        cube.AddComponent<MeshComponent, string>("Objs/cube.obj");
        PhysX.Material mat = PhysicsComponent.Instance.physics.CreateMaterial(0.5f, 0.5f, 0.6f);
        cube.AddComponent<PhysicsRigidActorComponent, Vector3, float, PhysX.Material, float>(Vector3.Zero, 1f, mat, 1f);
        
        ViewObject plane = objs.AddChild<ViewObject, string>("plane");
        plane.AddComponent<TransformComponent, Vector3>(Vector3.Zero);
        plane.AddComponent<PhysicsRigidActorComponent, Vector3, PhysX.Material, float>(Vector3.Zero, mat, 1f);


        uis.AddComponent<UiShowComponent>();
        uis.AddComponent<UiTestComponent>();
    }
}