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
        /// 初始时间模型空间到目标关节矩阵
        /// </summary>
        public Matrix4x4 offset;
        
        public JointInfo parent = null;
        public List<JointInfo> children = new();

        public JointInfo(Bone bone, JointInfo parent, Matrix4x4 local)
        {
            name = bone.Name;
            vertexWeights = bone.VertexWeights;
            
            parent?.children.Add(this);
            this.parent = parent;
            
            this.local = local;
            offset = LocalToWorld();
            offset.Inverse();
        }
        
        public Matrix4x4 LocalToWorld()
        {
            if (this.parent == null)
            {
                return this.local;
            }
            return this.local * this.parent.LocalToWorld();
        }
    }
}