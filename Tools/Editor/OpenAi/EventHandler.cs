namespace ET.Editor;

[Event(SceneType.Editor)]
public class AiResponseUpdateHandler : AEvent<Scene, AiResponseUpdate>
{
    protected override async ETTask Run(Scene scene, AiResponseUpdate a)
    {
        await ETTask.CompletedTask;
        Log.Instance.Debug(a.append);
    }
}

[Event(SceneType.Editor)]
public class AiResponseFinishHandler : AEvent<Scene, AiResponseFinish>
{
    protected override async ETTask Run(Scene scene, AiResponseFinish a)
    {
        await ETTask.CompletedTask;
        Log.Instance.Debug(a.message);
    }
}