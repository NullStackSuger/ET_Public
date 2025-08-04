using PhysX;

namespace ET;

[EnableClass]
public class ETSimulationFilterShader : SimulationFilterShader
{
    public override FilterResult Filter(int attributes0, FilterData filterData0, int attributes1, FilterData filterData1)
    {
        return new FilterResult
        {
            FilterFlag = FilterFlag.Default,
            // 触发器不支持NotifyTouchPersists
            PairFlags = PairFlag.ContactDefault | PairFlag.TriggerDefault | PairFlag.NotifyTouchFound | PairFlag.NotifyTouchLost/* | PairFlag.NotifyTouchPersists*/,
        };
    }
}