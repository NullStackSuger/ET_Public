namespace ET;

[ComponentOf(typeof(Scene))]
public class CoroutineLockComponent: Entity, IAwake, IScene, IUpdate
{
    public Fiber Fiber { get; set; }
    public int SceneType { get; set; }
        
    public readonly Queue<(long, long, int)> nextFrameRun = new();
}