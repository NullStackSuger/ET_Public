using PhysX;

namespace ET;

[EnableClass]
public class ETSimulationEventCallback : SimulationEventCallback
{
    public override void OnContact(ContactPairHeader pairHeader, ContactPair[] pairs)
    {
        base.OnContact(pairHeader, pairs);

        // 我测试这种方法只有碰撞开始 没有碰撞结束, 不清楚为什么
        /*foreach (var pair in pairs)
        {
            if ((pair.Flags & ContactPairFlag.ActorPairHasFirstTouch) != 0)
            {
                Console.WriteLine("碰撞开始");
            }
        
            if ((pair.Flags & ContactPairFlag.ActorPairLostTouch) != 0)
            {
                Console.WriteLine("碰撞结束");
            }
        }*/
        
        var rigid0 = pairHeader.Actor0;
        var rigid1 = pairHeader.Actor1;
        if (rigid0 == null || rigid1 == null) return;

        PhysicsSceneComponent scene = PhysicsSceneComponent.Main;
        scene.nowCollision.Add((rigid0, rigid1));
        scene.nowCollision.Add((rigid1, rigid0));
    }

    public override void OnTrigger(TriggerPair[] pairs)
    {
        base.OnTrigger(pairs);

        PhysicsSceneComponent scene = PhysicsSceneComponent.Main;
        foreach (TriggerPair pair in pairs)
        {
            if ((pair.Status & PairFlag.NotifyTouchFound) != 0)
            {
                scene.enterTriggers.Enqueue((pair.TriggerActor, pair.OtherActor));
            }
        
            if ((pair.Status & PairFlag.NotifyTouchLost) != 0)
            {
                scene.exitTriggers.Enqueue((pair.TriggerActor, pair.OtherActor));
            }
        }
    }
}