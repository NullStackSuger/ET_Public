using System.Numerics;
using Assimp;
using Quaternion = System.Numerics.Quaternion;

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

    private static void TwoBoneIK(TransformComponent a, TransformComponent b, TransformComponent c, Vector3 target)
    {
        const float eps = 0.01f;

        Vector3 aPos = a.worldPosition;
        Vector3 bPos = b.worldPosition;
        Vector3 cPos = c.worldPosition;
        Quaternion aRot = a.worldRotation;
        Quaternion bRot = b.worldRotation;
        
        float lengthAB = Vector3.Distance(aPos, bPos);
        float lengthBC = Vector3.Distance(bPos, cPos);
        float lengthAT = Math.Clamp(Vector3.Distance(aPos, target), eps, lengthAB + lengthBC - eps);
        
        float angleBAC0 = MathF.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(cPos - aPos), Vector3.Normalize(bPos - aPos)),   -1f, 1f));
        float angleABC0 = MathF.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(aPos - bPos), Vector3.Normalize(cPos - bPos)),   -1f, 1f));
        float angleCAT0 = MathF.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(cPos - aPos), Vector3.Normalize(target - aPos)), -1f, 1f));
        
        float angleBAC1 = MathF.Acos(Math.Clamp((lengthBC * lengthBC - lengthAB * lengthAB - lengthAT * lengthAT) / (-2f * lengthAB * lengthAT), -1f, 1f));
        float angleABC1 = MathF.Acos(Math.Clamp((lengthAT * lengthAT - lengthAB * lengthAB - lengthBC * lengthBC) / (-2f * lengthAB * lengthBC), -1f, 1f));

        Vector3 axis0 = Vector3.Normalize(Vector3.Cross(cPos - aPos, bPos - aPos));
        Vector3 axis1 = Vector3.Normalize(Vector3.Cross(cPos - aPos, target - aPos));
        
        Vector3 localAxis0 = MathHelper.Rotate(Quaternion.Inverse(aRot), axis0);
        Vector3 localAxis1 = MathHelper.Rotate(Quaternion.Inverse(aRot), axis1);
        Vector3 localAxisB = MathHelper.Rotate(Quaternion.Inverse(bRot), axis0);
        
        Quaternion r0 = MathHelper.AngleAxis(MathHelper.Rad2Deg * (angleBAC1 - angleBAC0), localAxis0);
        Quaternion r1 = MathHelper.AngleAxis(MathHelper.Rad2Deg * (angleABC1 - angleABC0), localAxisB);
        Quaternion r2 = MathHelper.AngleAxis(MathHelper.Rad2Deg * angleCAT0, localAxis1);
        
        a.localRotation *= r0 * r2;
        b.localRotation *= r1;
    }
}