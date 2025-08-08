namespace ET.Client;

[EntitySystemOf(typeof(ViewObjectComponent))]
public static partial class ViewObjectComponentSystem
{
    [EntitySystem]
    private static void Awake(this ViewObjectComponent self)
    {
        
    }

    public static IEnumerable<ViewObject> Foreach(this ViewObjectComponent self)
    {
        foreach (ViewObject child in self.Children.Values)
        {
            yield return child;
            foreach (ViewObject descendant in child.Foreach())
            {
                yield return descendant;
            }
        }
    }
}