namespace ET;

public class EventAttribute: BaseAttribute
{
	public int SceneType { get; }

	public EventAttribute(int sceneType)
	{
		SceneType = sceneType;
	}
}