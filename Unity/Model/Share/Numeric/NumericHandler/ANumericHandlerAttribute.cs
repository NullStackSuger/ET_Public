namespace ET;

public class ANumericHandlerAttribute : BaseAttribute
{
    public int SceneType { get; }
    public int NumericType { get; }

    public ANumericHandlerAttribute(int sceneType, int numericType)
    {
        SceneType = sceneType;
        NumericType = numericType;
    }
}