namespace ET.Server;

[Event(SceneType.Main)]
public class EntryServerHandler : AEvent<Scene, EntryServer>
{
    protected override async ETTask Run(Scene scene, EntryServer a)
    {
        await ETTask.CompletedTask;
    }
}