namespace ET;

[Event(SceneType.Main)]
public class EntryShareHandler : AEvent<Scene, EntryShare>
{
    protected override async ETTask Run(Scene root, EntryShare args)
    {
        root.AddComponent<TimerComponent>();
        root.AddComponent<CoroutineLockComponent>();
        root.AddComponent<ObjectWait>();
        
        await ETTask.CompletedTask;
    }
}