namespace ET.Client;

public class BvhNode
{
    public BvhNode leftNode { get; private set; }
    public BvhNode rightNode { get; private set; }
    public BvhNode parent { get; private set; }
    
    public AABB aabb { get; private set; }
    
    public ViewObject obj { get; private set; }

    public BvhNode(ViewObject viewObject)
    {
        obj = viewObject;
        if (viewObject != null)
        {
            MeshComponent meshComponent = viewObject.GetComponent<MeshComponent>();
            TransformComponent transformComponent = viewObject.GetComponent<TransformComponent>();
            aabb = AABB.GetAABB(meshComponent).Transform(transformComponent.Model);   
        }
        else
        {
            aabb = new AABB();
        }
    }
    public BvhNode(BvhNode source)
    {
        this.aabb = source.aabb;
        this.leftNode = source.leftNode;
        this.rightNode = source.rightNode;
        this.obj = source.obj;
    }

    public void SetLeaf(BvhNode leftNode, BvhNode rightNode)
    {
        this.leftNode = leftNode;
        this.rightNode = rightNode;
        if (leftNode != null)
        {
            leftNode.parent = this;
        }
        if (rightNode != null)
        {
            rightNode.parent = this;
        }
    }
    public bool IsLeaf()
    {
        return leftNode != null;
    }
    
    //获取兄弟节点
    public BvhNode GetSibling()
    {
        if (this.parent?.leftNode == this) return this.parent.rightNode;
        if (this.parent?.rightNode == this) return this.parent.leftNode;
        return null;
    }
    // 获取根节点
    public BvhNode GetRoot()
    {
        if (this.parent == null) return this;
        return this.parent.GetRoot();
    }
    
    //合并两个node
    public static BvhNode CombineNodes(BvhNode targetNode, BvhNode insertNode)
    {
        //复制目标node信息
        var newNode = new BvhNode(targetNode);
        //合并 aabb
        targetNode.aabb.Encapsulate(insertNode.aabb);
        //向上传播
        targetNode.AABBBroadCast();
        //重新设置左右子节点
        targetNode.SetLeaf(newNode, insertNode);

        return newNode;
    }
    //分离一个节点
    public static BvhNode SeparateNodes(BvhNode separateNode)
    {
        var parent = separateNode.parent;
        if (parent != null && parent.Contains(separateNode))
        {
            var siblingNode = separateNode.GetSibling();
            var siblingAABB = siblingNode.aabb;

            //把兄弟节点的子树丢给父节点
            parent.SetLeaf(siblingNode.leftNode, siblingNode.rightNode);
            //设置AABB
            parent.aabb = siblingAABB;
            //向上传播
            parent.AABBBroadCast();
            //绑定场景物体
            parent.obj = siblingNode.obj;

            return parent;
        }
        else
        {
            Log.Instance.Error("分离节点失败，目标节点父级为null或者父级不含有目标节点");
        }
        return null;
    }
    
    //aabb的向上广播
    private void AABBBroadCast()
    {
        if (this.parent != null)
        {
            this.parent.aabb = new AABB();
            if (this.parent.leftNode != null)
            {
                this.parent.aabb.Encapsulate(this.parent.leftNode.aabb);
            }
            if (this.parent.rightNode != null)
            {
                this.parent.aabb.Encapsulate(this.parent.rightNode.aabb);
            }
            
            this.parent.AABBBroadCast();
        }
    }

    //检查节点是否为子节点
    private bool Contains(BvhNode node)
    {
        return this.leftNode == node || this.rightNode == node;
    }
}