using System.Reflection;
using Luban;

namespace ET
{
    public class CodeTypes: Singleton<CodeTypes>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly UnOrderMultiMapSet<Type, Type> types = new();
        
        public void Awake(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = assemblies.Types();
            foreach ((string fullName, Type type) in addTypes)
            {
                allTypes[fullName] = type;
                
                if (type.IsAbstract) continue;
                
                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);
                foreach (object obj in objects)
                {
                    types.Add(obj.GetType(), type);
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            allTypes.TryGetValue(typeName, out Type type);
            return type;
        }
        
        public void CreateCode()
        {
            var hashSet = GetTypes(typeof(CodeAttribute));
            foreach (Type type in hashSet)
            {
                object obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj)?.Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }

        public void CreateConfig()
        {
            var hashSet = GetTypes(typeof(ConfigAttribute));
            foreach (Type type in hashSet)
            {
                string s;
                switch (type.Namespace)
                {
                    case "ET": s = "ClientServer"; break;
                    case "ET.Client": s = "Client"; break;
                    case "ET.Server": s = "Server"; break;
                    default: continue;
                }
                byte[] bytes = File.ReadAllBytes($"Config\\Luban\\Binary\\{s}\\{type.Name}.bytes");
                object obj = Activator.CreateInstance(type, new ByteBuf(bytes));
                ((ISingletonAwake)obj)?.Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}