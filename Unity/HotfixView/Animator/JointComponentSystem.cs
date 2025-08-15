using Assimp;

namespace ET.Client;

[EntitySystemOf(typeof(JointComponent))]
public static partial class JointComponentSystem
{
    [EntitySystem]
    private static void Awake(this JointComponent self, List<Bone> bones, Assimp.Scene scene)
    {
        Build(self, scene, bones, out self.jointMap);
    }

    [EntitySystem]
    private static void Serialize(this JointComponent self)
    {
        
    }
    
    [EntitySystem]
    private static void Deserialize(this JointComponent self)
    {
        
    }

    private static void Build(JointComponent self, Assimp.Scene scene, List<Bone> bones, out Dictionary<string, JointComponent.JointInfo> jointMap)
    {
        Node jointRootNode = scene.RootNode.FindNode(bones[0].Name);

        Dictionary<string, Bone> boneMap = new();
        foreach (Bone bone in bones)
        {
            boneMap[bone.Name] = bone;
        }

        jointMap = new();
        
        self.root = new JointComponent.JointInfo(boneMap[jointRootNode.Name], null, jointRootNode.Transform);
        jointMap.Add(jointRootNode.Name, self.root);
        foreach (Node child in jointRootNode.Children)
        {
            BuildInner(child, self.root, boneMap, ref jointMap);
        }

        static void BuildInner(Node node, JointComponent.JointInfo parent, Dictionary<string, Bone> boneMap, ref Dictionary<string, JointComponent.JointInfo> jointMap)
        {
            JointComponent.JointInfo joint = new JointComponent.JointInfo(boneMap[node.Name], parent, node.Transform);
            var mat = joint.LocalToWorld();
            mat.Inverse();
            jointMap.Add(joint.name, joint);
            
            foreach (Node child in node.Children)
            {
                BuildInner(child, joint, boneMap, ref jointMap);
            }
        }
    }
}