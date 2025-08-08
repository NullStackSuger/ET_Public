using System.Numerics;
using Assimp;
using Matrix4x4 = Assimp.Matrix4x4;
using Quaternion = Assimp.Quaternion;

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
        
        self.root = new JointComponent.JointInfo(boneMap[jointRootNode.Name], jointRootNode.Transform);
        jointRootNode.Transform.DecomposeNoScaling(out Quaternion rotation, out Vector3D position);
        jointMap.Add(jointRootNode.Name, self.root);
        foreach (Node child in jointRootNode.Children)
        {
            BuildInner(child, self.root, boneMap, ref jointMap);
        }

        self.UpdateModel();
        self.UpdateOffset();

        self.UpdateLocal();
        self.UpdateModel();

        static void BuildInner(Node node, JointComponent.JointInfo parent, Dictionary<string, Bone> boneMap, ref Dictionary<string, JointComponent.JointInfo> jointMap)
        {
            JointComponent.JointInfo joint = new JointComponent.JointInfo(boneMap[node.Name], node.Transform);
            jointMap.Add(joint.name, joint);
            joint.parent = parent;
            parent.children.Add(joint);
            
            foreach (Node child in node.Children)
            {
                BuildInner(child, joint, boneMap, ref jointMap);
            }
        }
    }

    public static void UpdateModel(this JointComponent self)
    {
        UpdateInner(self.root);
        
        static void UpdateInner(JointComponent.JointInfo joint)
        {
            Matrix4x4 parentMat = joint.parent?.model ?? Matrix4x4.Identity;
            joint.model = parentMat * joint.local;
            foreach (var child in joint.children)
            {
                UpdateInner(child);
            }
        }
    }

    /// <summary>
    /// Awake里计算的Model就是初始状态的根关节到目标关节的矩阵, 反转一下就是offset
    /// </summary>
    static void UpdateOffset(this JointComponent self)
    {
        UpdateInner(self.root);
        
        static void UpdateInner(JointComponent.JointInfo joint)
        {
            var tmp = joint.model;
            joint.model.Inverse();
            joint.offset = joint.model;
            joint.model = tmp;
            foreach (var child in joint.children)
            {
                UpdateInner(child);
            }
        }
    }

    static void UpdateLocal(this JointComponent self)
    {
        UpdateInner(self.root);
        
        static void UpdateInner(JointComponent.JointInfo joint)
        {
            switch (joint.name)
            {
                case "Torso":
                {
                    joint.local = CreateMat(new Vector3(0f,0f,3.210999f), new System.Numerics.Quaternion(0.72961134f,0f,0f,0.68386203f), Vector3.One);

                } break;
                case "Chest":
                {
                    joint.local = CreateMat(new Vector3(0f,1.482405f,0f), new System.Numerics.Quaternion(-0.07503736f,2.3774655E-07f,-1.7890315E-08f,0.99718076f), Vector3.One);

                } break;
                case "Neck":
                {
                    joint.local = CreateMat(new Vector3(1.42109E-14f,1.29516f,2.9802298E-08f), new System.Numerics.Quaternion(-2.4895629E-08f,0.9882558f,0.1528088f,3.1022967E-07f), Vector3.One);

                } break;
                case "Head":
                {
                    joint.local = CreateMat(new Vector3(-1.42109E-14f,0.3023291f,0f), new System.Numerics.Quaternion(0.1193361f,-1.2154938E-07f,-2.9691858E-08f,0.99285394f), Vector3.One);

                } break;
                case "Upper_Arm_L":
                {
                    joint.local = CreateMat(new Vector3(0.9250668f,0.7715869f,-0.1557154f), new System.Numerics.Quaternion(0.0041127955f,-0.09395892f,0.94700044f,-0.30715665f), Vector3.One);

                } break;
                case "Lower_Arm_L":
                {
                    joint.local = CreateMat(new Vector3(2.38419E-07f,1.480745f,0f), new System.Numerics.Quaternion(0.13054077f,-0.6928811f,-0.16665824f,0.68927485f), Vector3.One);

                } break;
                case "Hand_L":
                {
                    joint.local = CreateMat(new Vector3(-5.9604602E-08f,1.083281f,-4.7683702E-07f), new System.Numerics.Quaternion(-0.094932295f,-0.020755189f,0.059691954f,0.99347574f), Vector3.One);

                } break;
                case "Upper_Arm_R":
                {
                    joint.local = CreateMat(new Vector3(-0.9250666f,0.7715869f,-0.1557163f), new System.Numerics.Quaternion(0.07212171f,-0.11837647f,0.9442591f,0.29859746f), Vector3.One);

                } break;
                case "Lower_Arm_R":
                {
                    joint.local = CreateMat(new Vector3(-9.5367403E-07f,1.480746f,2.38419E-07f), new System.Numerics.Quaternion(0.18049242f,-0.47458532f,-0.11062696f,0.85437286f), Vector3.One);

                } break;
                case "Hand_R":
                {
                    joint.local = CreateMat(new Vector3(-1.19209005E-07f,1.083282f,0f), new System.Numerics.Quaternion(0.07497221f,-0.09154523f,0.08339298f,0.98946667f), Vector3.One);

                } break;
                case "Upper_Leg_L":
                {
                    joint.local = CreateMat(new Vector3(0.5668193f,-0.001348972f,-0.1043749f), new System.Numerics.Quaternion(1.3521011E-06f,0.07257247f,0.997347f,-0.005763582f), Vector3.One);

                } break;
                case "Lower_Leg_L":
                {
                    joint.local = CreateMat(new Vector3(-5.9604602E-08f,1.282774f,-1.4901198E-08f), new System.Numerics.Quaternion(0.020502178f,0.9948227f,-0.09952753f,0.001316804f), Vector3.One);

                } break;
                case "Foot_L":
                {
                    joint.local = CreateMat(new Vector3(5.9604602E-08f,1.493334f,-1.4901198E-08f), new System.Numerics.Quaternion(0.011929968f,0.8051487f,-0.592888f,-0.008784322f), Vector3.One);

                } break;
                case "Upper_Leg_R":
                {
                    joint.local = CreateMat(new Vector3(-0.5668193f,-0.001348972f,-0.1043749f), new System.Numerics.Quaternion(-0.0008330113f,0.07257726f,0.9973466f,0.0057030604f), Vector3.One);

                } break;
                case "Lower_Leg_R":
                {
                    joint.local = CreateMat(new Vector3(0f,1.282774f,2.9802298E-08f), new System.Numerics.Quaternion(-0.020419128f,0.99482346f,-0.0995446f,-0.00048784429f), Vector3.One);

                } break;
                case "Foot_R":
                {
                    joint.local = CreateMat(new Vector3(-5.9604602E-08f,1.493333f,1.4901198E-08f), new System.Numerics.Quaternion(-0.011929619f,0.80514866f,-0.592888f,0.008784805f), Vector3.One);

                } break;
            }
            foreach (var child in joint.children)
            {
                UpdateInner(child);
            }
        }

        static Matrix4x4 CreateMat(System.Numerics.Vector3 position, System.Numerics.Quaternion rotation, System.Numerics.Vector3 scale)
        {
            var mat = (System.Numerics.Matrix4x4.CreateScale(scale) *
                       System.Numerics.Matrix4x4.CreateFromQuaternion(rotation) *
                       System.Numerics.Matrix4x4.CreateTranslation(position)).ToMatrix4x4();
            return mat;
        }
    }
}