using Assimp;

namespace ET.Client;

public class JointComponent : Entity, IAwake<List<Bone>, Assimp.Scene>, ISerialize, IDeserialize
{
    public JointInfo root;
    public Dictionary<string, JointInfo> jointMap;
    
    public class JointInfo
    {
        public string name;
        public List<VertexWeight> vertexWeights;

        /// <summary>
        /// 当前时间目标关节矩阵
        /// </summary>
        public Matrix4x4 local;
        /// <summary>
        /// 当前时间目标关节到模型空间矩阵
        /// </summary>
        public Matrix4x4 model;
        /// <summary>
        /// 初始时间模型空间到目标关节矩阵
        /// </summary>
        public Matrix4x4 offset;
        
        public JointInfo parent = null;
        public List<JointInfo> children = new();

        public JointInfo(Bone bone, Matrix4x4 local)
        {
            name = bone.Name;
            vertexWeights = bone.VertexWeights;
            this.local = local;
            model = Matrix4x4.Identity;
            //offset = bone.OffsetMatrix; // TODO 动画系统/加载进来的offsetMatrix好像不对
        }
    }
}