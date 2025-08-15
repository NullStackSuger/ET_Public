using System.Numerics;

namespace ET.Client;

public class BvhSpace
{
    public BvhNode root { get; private set; }
    
    private readonly List<BvhNode> nodes = new();
    private readonly Dictionary<ViewObject, BvhNode> objNodeMap = new();
    
    //添加节点
    public BvhNode AddNode(ViewObject obj)
    {
        var leaf = new BvhNode(obj);
        objNodeMap[obj] = leaf;
        BuildBvh(leaf);
        return leaf;
    }
    
    // 移除节点
    public bool RemoveNode(ViewObject obj)
    {
        if (!objNodeMap.TryGetValue(obj, out BvhNode node)) return false;
        
        nodes.Remove(node);
        nodes.Remove(node.GetSibling());
        BvhNode subtree = BvhNode.SeparateNodes(node);
        if (subtree.IsLeaf())
        {
            nodes.Add(subtree);
            objNodeMap[subtree.obj] = node;
        }
        
        return true;
    }
    
    private void BuildBvh(BvhNode leaf)
    {
        if (root == null)
        {
            root = leaf;
            nodes.Add(leaf);
        }
        else
        {
            var targetNode = SAH(leaf) ?? nodes[0];
            var newNode = BvhNode.CombineNodes(targetNode, leaf);
            nodes.Remove(targetNode);
            nodes.Add(leaf);
            nodes.Add(newNode);
            objNodeMap[newNode.obj] = newNode;
            root = newNode.GetRoot();
        }
    }
    
    private BvhNode SAH(BvhNode newLeaf)
    {
        var minCost = float.MaxValue;
        BvhNode minCostNode = null;

        //遍历所有叶子节点
        foreach (BvhNode leaf in nodes)
        {
            var newBranchAABB = AABB.Encapsulate(leaf.aabb, newLeaf.aabb);
            //新增的分支节点表面积
            var deltaCost = newBranchAABB.SurfaceArea;
            var wholeCost = deltaCost;
            var parent = leaf.parent;
            //统计所有祖先节点的表面积差
            while (parent != null)
            {
                var s2 = parent.aabb.SurfaceArea;
                var unionAABB = AABB.Encapsulate(parent.aabb, newLeaf.aabb);
                var s3 = unionAABB.SurfaceArea;
                deltaCost = s3 - s2;
                wholeCost += deltaCost;
                parent = parent.parent;
            }

            //返回最小的目标
            if (wholeCost < minCost)
            {
                minCostNode = leaf;
                minCost = wholeCost;
            }
        }

        return minCostNode;
    }  

    // 二分分割
    private void BinaryPartition(BvhNode node, List<ViewObject> objs, int startIndex, int endIndex, int depth)
    {
        if (depth <= 0) return;
        
        var halfIndex = (endIndex + startIndex) / 2;
        var leftNode = new BvhNode(viewObject: null);
        var rightNode = new BvhNode(viewObject: null);
        
        for (int i = startIndex; i < halfIndex; i++)
        {
            var obj = objs[i];
            MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
            TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
            
            var aabb = AABB.GetAABB(meshComponent).Transform(transformComponent.Model);
            leftNode.aabb.Encapsulate(aabb);
        }
        
        for (int i = halfIndex; i < endIndex; i++)
        {
            var obj = objs[i];
            MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
            TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
            
            var aabb = AABB.GetAABB(meshComponent).Transform(transformComponent.Model);
            rightNode.aabb.Encapsulate(aabb);
        }
        
        node.SetLeaf(leftNode, rightNode);
        
        BinaryPartition(leftNode, objs, startIndex, halfIndex, depth - 1);
        BinaryPartition(rightNode, objs, halfIndex, endIndex, depth - 1);
    }
    
    //最大方差轴分割
    private void AxisPartition(BvhNode node, List<ViewObject> objs, int depth)
    {
        if (depth <= 0) return;
        
        var leftNode = new BvhNode(viewObject: null);
        var rightNode = new BvhNode(viewObject: null);
        
        var leftObjs = new List<ViewObject>();
        var rightObjs = new List<ViewObject>();
        
        var mode = PickVariance(objs);
        switch (mode)
        {
            case 0: // x
                float middleX = node.aabb.Center.X;
                foreach (ViewObject obj in objs)
                {
                    MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
                    TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
                    
                    var aabb = AABB.GetAABB(meshComponent).Transform(transformComponent.Model);

                    if (transformComponent.worldPosition.X <= middleX)
                    {
                        leftObjs.Add(obj);
                        leftNode.aabb.Encapsulate(aabb);
                    }
                    else
                    {
                        rightObjs.Add(obj);
                        rightNode.aabb.Encapsulate(aabb);
                    }
                }
                break;
            case 1: // y
                float middleY = node.aabb.Center.Y;
                foreach (ViewObject obj in objs)
                {
                    MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
                    TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
                    
                    var aabb = AABB.GetAABB(meshComponent).Transform(transformComponent.Model);
                    
                    if (transformComponent.worldPosition.Y <= middleY)
                    {
                        leftObjs.Add(obj);
                        leftNode.aabb.Encapsulate(aabb);
                    }
                    else
                    {
                        rightObjs.Add(obj);
                        rightNode.aabb.Encapsulate(aabb);
                    }
                }
                break;
            case 2: // z
                float middleZ = node.aabb.Center.Z;
                foreach (ViewObject obj in objs)
                {
                    MeshComponent meshComponent = obj.GetComponent<MeshComponent>();
                    TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
                    
                    var aabb = AABB.GetAABB(meshComponent).Transform(transformComponent.Model);
                    
                    if (transformComponent.worldPosition.Z <= middleZ)
                    {
                        leftObjs.Add(obj);
                        leftNode.aabb.Encapsulate(aabb);
                    }
                    else
                    {
                        rightObjs.Add(obj);
                        rightNode.aabb.Encapsulate(aabb);
                    }
                }
                break;
        }
        
        node.SetLeaf(leftNode, rightNode);
        
        AxisPartition(leftNode, leftObjs, depth - 1);
        AxisPartition(rightNode, rightObjs, depth - 1);
    }
    
    //寻找最大方差轴
    private int PickVariance(List<ViewObject> objs)
    {
        //统计期望
        var mean_x = 0.0f;
        var mean_y = 0.0f;
        var mean_z = 0.0f;
        foreach (ViewObject obj in objs)
        {
            TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
            
            Vector3 position = transformComponent.worldPosition;
            mean_x += position.X;
            mean_y += position.Y;
            mean_z += position.Z;
        }
        mean_x /= objs.Count;
        mean_y /= objs.Count;
        mean_z /= objs.Count;
        
        //统计方差
        var variance_x = 0.0f;
        var variance_y = 0.0f;
        var variance_z = 0.0f;
        foreach (ViewObject obj in objs)
        {
            TransformComponent transformComponent = obj.GetComponent<TransformComponent>();
            
            Vector3 position = transformComponent.worldPosition;
            variance_x += (float)Math.Pow(position.X - mean_x, 2);
            variance_y += (float)Math.Pow(position.Y - mean_y, 2);
            variance_z += (float)Math.Pow(position.Z - mean_z, 2);
        }
        variance_x /= objs.Count - 1;
        variance_y /= objs.Count - 1;
        variance_z /= objs.Count - 1;
        
        //x轴
        if (variance_x > variance_y && variance_x > variance_z)
            return 0;
        //y轴
        if (variance_y > variance_z && variance_y > variance_x)
            return 1;
        //z轴
        if (variance_z > variance_x && variance_z > variance_y)
            return 2;
        return 0;
    }
}