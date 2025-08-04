namespace ET;

public class TypeSystems
{
    public class OneTypeSystems
    {
        public readonly UnOrderMultiMap<Type, SystemObject> Map = new();
        // 这里不用hash，数量比较少，直接for循环速度更快
        public readonly List<Type> ClassType = new();
    }
        
    private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

    public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
    {
        this.typeSystemsMap.TryGetValue(type, out var systems);
        if (systems != null)
        {
            return systems;
        }

        systems = new OneTypeSystems();
        this.typeSystemsMap.Add(type, systems);
        return systems;
    }

    public OneTypeSystems GetOneTypeSystems(Type type)
    {
        this.typeSystemsMap.TryGetValue(type, out var systems);
        return systems;
    }

    public List<SystemObject> GetSystems(Type type, Type systemType)
    {
        if (!this.typeSystemsMap.TryGetValue(type, out var oneTypeSystems))
        {
            return null;
        }

        if (!oneTypeSystems.Map.TryGetValue(systemType, out List<SystemObject> systems))
        {
            return null;
        }

        return systems;
    }
}