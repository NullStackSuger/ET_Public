namespace ET.Analyzer
{
    public static class Definition
    {
        public const string EntityType = "ET.Entity";
        
        public const string ETTask = "ETTask";

        public const string ETTaskFullName = "ET.ETTask";

        public static readonly string[] AddChildMethods = ["AddChild", "AddChildWithId"];

        public static readonly string[] ComponentMethod = ["AddComponent","GetComponent"];

        public const string ISystemType = "ET.ISystemType";
        
        public const string BaseAttribute = "ET.BaseAttribute";

        public const string EnableMethodAttribute = "ET.EnableMethodAttribute";
        
        public const string FriendOfAttribute = "ET.FriendOfAttribute";
        
        public const string UniqueIdAttribute = "ET.UniqueIdAttribute";

        public const string ChildOfAttribute = "ET.ChildOfAttribute";

        public const string ComponentOfAttribute = "ET.ComponentOfAttribute";
        
        public const string EnableAccessEntityChildAttribute = "ET.EnableAccessEntityChildAttribute";

        public const string StaticFieldAttribute = "ET.StaticFieldAttribute";

        public const string ETCancellationToken = "ET.ETCancellationToken";

        public const string ETTaskCompleteTask = "ETTask.CompletedTask";

        public const string ETClientNameSpace = "ET.Client";

        public const string ClientDirInServer = @"Unity\Hotfix\Client\";

        public const string EntitySystemAttribute = "EntitySystem";
        public const string EntitySystemAttributeMetaName = "ET.EntitySystemAttribute";

        public const string EntitySystemOfAttribute = "ET.EntitySystemOfAttribute";
        public const string EntitySystemInterfaceSequence = "EntitySystemInterfaceSequence";

        public const string IAwakeInterface = "ET.IAwake";
        public const string AwakeMethod = "Awake";

        public const string IDestroyInterface = "ET.IDestroy";
        public const string DestroyMethod = "Destroy";

        public const string ISerializeInterface = "ET.ISerialize";
        public const string SerializeMethod = "Serialize";
        
        public const string IDeserializeInterface = "ET.IDeserialize";
        public const string DeserializeMethod = "Deserialize";

        public const string IUpdateInterface = "ET.IUpdate";
        public const string UpdateMethod = "Update";
        
        public const string ILateUpdateInterface = "ET.ILateUpdate";
        public const string LateUpdateMethod = "LateUpdate";

        public const string ETLog = "ET.Log";

        public const string IMessageInterface = "ET.IMessage";

        public const string EntityRefType = "EntityRef";
        
        public const string EntityWeakRefType = "EntityWeakRef";

        public const string DisableNewAttribute = "ET.DisableNewAttribute";

        public const string EnableClassAttribute = "ET.EnableClassAttribute";
    }
}

