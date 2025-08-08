namespace ET.Client;

[EntitySystemOf(typeof(ViewObject))]
public static partial class ViewObjectSystem
{
    [EntitySystem]
    private static void Awake(this ViewObject self, string a)
    {
        self.name = a;
    }
    
    public static IEnumerable<ViewObject> Foreach(this ViewObject self)
    {
        foreach (ViewObject child in self.Children.Values)
        {
            foreach (ViewObject descendant in ForeachInner(child))
            {
                yield return descendant;
            }
        }

        IEnumerable<ViewObject> ForeachInner(ViewObject obj)
        {
            yield return obj;

            foreach (ViewObject child in obj.Children.Values)
            {
                foreach (ViewObject descendant in ForeachInner(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    public static ViewObject Find(this ViewObject self, string name)
    {
        foreach (ViewObject child in self.Foreach())
        {
            if (child.name == name) return child;
        }
        return null;
    }
}