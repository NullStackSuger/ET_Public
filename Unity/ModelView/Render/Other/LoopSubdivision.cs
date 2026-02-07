using System.Numerics;

namespace ET.Client;

public static class LoopSubdivision
{
    private class OldVertexInfo
    {
        public Vector3 position;        // 位置 //
        public int degree;              // 顶点的度 //
        public OldVertexInfo[] linkedPoints;  // 相连接的点 //
        
        private static int COUNTER;
        private readonly int hashCode;

        public OldVertexInfo()
        {
            hashCode = COUNTER++;
            linkedPoints = null;
        }

        public static bool operator == (OldVertexInfo vertex0, OldVertexInfo vertex1)
        {
            return !ReferenceEquals(vertex0, null) && 
                   !ReferenceEquals(vertex1, null) &&
                   MathHelper.Equals(vertex0.position, vertex1.position);
        }

        public static bool operator != (OldVertexInfo vertex0, OldVertexInfo vertex1)
        {
            return !(vertex0 == vertex1);
        }

        public override bool Equals(object obj)
        {
            OldVertexInfo oldVertexInfo = obj as OldVertexInfo;
            return oldVertexInfo != null && oldVertexInfo == this;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
    
    private class NewVertexInfo
    {
        public Vector3 position;
        public int edgeIndex;
        public int linkedRealVertexIndex0;
        public int linkedRealVertexIndex1;
        public Vector3 linkedPosition0;
        public Vector3 linkedPosition1;
        public Vector3 oppositePosition0;
        public Vector3 oppositePosition1;

        private static int COUNTER;
        private readonly int hashCode;

        public NewVertexInfo()
        {
            hashCode = COUNTER++;
        }

        public static bool operator == (NewVertexInfo vertex0, NewVertexInfo vertex1)
        {
            return !ReferenceEquals(vertex0, null) && !ReferenceEquals(vertex1, null) && MathHelper.Equals(vertex0.position, vertex1.position);
        }

        public static bool operator != (NewVertexInfo vertex0, NewVertexInfo vertex1)
        {
            return !(vertex0 == vertex1);
        }

        public override bool Equals(object obj)
        {
            NewVertexInfo vertexInfo = obj as NewVertexInfo;
            return vertexInfo != null && vertexInfo == this;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
    
    public static (Vector3[], ushort[]) Subdivide(Vector3[] vertices, ushort[] indices)
    {
        RemoveDuplicateVertex(vertices, out Vector3[] distinctPositions, out ushort[] vertexToDistinctPosIndices);

        RemapTriangles(indices, vertexToDistinctPosIndices, out ushort[] distinctTriangles);
            
        // 初始化旧顶点 //
        InitOldVertexInfo(vertices, indices, distinctPositions, vertexToDistinctPosIndices, distinctTriangles, out OldVertexInfo[] oldVertexInfos);

        // 初始化新顶点 //
        InitNewVertexInfo(vertices, indices, distinctTriangles, out NewVertexInfo[] newVertexInfos);

        // 计算新mesh的vertices和triangle //
        CalculateVertexAndTriangle(indices, newVertexInfos, oldVertexInfos, out Vector3[] newMeshVertices, out ushort[] newMeshTriangle);

        // // 测试 //
        // m_newMeshVertices = newMeshVertices;
        // m_oldMeshVertices = new Vector3[oldVertexInfos.Length];
        // for (int i = 0; i < oldVertexInfos.Length; i++)
        // {
        //     m_oldMeshVertices[i] = oldVertexInfos[i].position;
        // }
        //
        // m_newMeshVertices = new Vector3[newVertexInfos.Length];
        // for (int i = 0; i < newVertexInfos.Length; i++)
        // {
        //     m_newMeshVertices[i] = newVertexInfos[i].position;
        // }

        return (newMeshVertices, newMeshTriangle);
    }
    
    // 去重 //
    private static void RemoveDuplicateVertex(Vector3[] vertices, out Vector3[] distinctPositions, out ushort[] vertexRemapIndex)
    {
        if (vertices == null)
        {
            distinctPositions = null;
            vertexRemapIndex = null;
            return;
        }
        
        vertexRemapIndex = new ushort[vertices.Length];
        List<Vector3> distinctVertexList = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 curVertex = vertices[i];
            bool repeqated = false;
            foreach (var v in distinctVertexList)
            {
                if (MathHelper.Equals(curVertex, v))
                {
                    repeqated = true;
                    break;
                }
            }

            if (!repeqated)
            {
                distinctVertexList.Add(vertices[i]);
            }

            vertexRemapIndex[i] = (ushort)distinctVertexList.IndexOf(vertices[i]);
        }

        distinctPositions = distinctVertexList.ToArray();
    }
    
    // 去重后的索引 //
    private static void RemapTriangles(ushort[] triangles, ushort[] distinctVertexIndices, out ushort[] remappedTriangles)
    {
        if (triangles == null)
        {
            remappedTriangles = null;
            return;
        }

        remappedTriangles = new ushort[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            int originVertexIndex = triangles[i];
            ushort distinctVertexIndex = distinctVertexIndices[originVertexIndex];
            remappedTriangles[i] = distinctVertexIndex;
        }
    }

    private static void InitOldVertexInfo(Vector3[] vertices, ushort[] triangles, Vector3[] distinctPositions, ushort[] vertexToDistinctPosIndices, ushort[] remappedTriangle, out OldVertexInfo[] vertexInfos)
    {
        if (vertices == null || triangles == null || distinctPositions == null || vertexToDistinctPosIndices == null || remappedTriangle == null || vertices.Length != vertexToDistinctPosIndices.Length || remappedTriangle.Length != triangles.Length)
        {
            vertexInfos = null;
            return;
        }

        List<int>[] temp = new List<int>[distinctPositions.Length]; // 存放真实顶点相连接的顶点位置索引 //
        foreach (var realVertexIndex in remappedTriangle)
        {
            List<int> list;
            if (temp[realVertexIndex] == null)
            {
                list = new List<int>();
                temp[realVertexIndex] = list;
            }
            else
            {
                list = temp[realVertexIndex];
            }
            
            for (int j = 0; j < remappedTriangle.Length; j += 3)
            {
                int flag = 7; // 111 //
                if (remappedTriangle[j] == realVertexIndex)
                {
                    flag = 6; // 110 //
                }

                if (remappedTriangle[j + 1] == realVertexIndex)
                {
                    flag = 5; // 101 //
                }

                if (remappedTriangle[j + 2] == realVertexIndex)
                {
                    flag = 3; // 011 //
                }

                int offset = 0;
                while (flag != 7 && offset < 3)
                {
                    if ((flag & (1 << offset)) > 0)
                    {
                        int distinctPosIndex = remappedTriangle[j + offset];
                        if (!list.Contains(distinctPosIndex))
                        {
                            list.Add(distinctPosIndex);
                        }
                    }

                    offset++;
                }
            }
        }

        vertexInfos = new OldVertexInfo[vertices.Length];
        for (int i = 0; i < vertexInfos.Length; i++)
        {
            OldVertexInfo info = new OldVertexInfo();
            info.position = vertices[i];
            int realVertexIndex = vertexToDistinctPosIndices[i];
            if (realVertexIndex < temp.Length && temp[realVertexIndex] != null)
            {
                int degree = temp[realVertexIndex].Count;
                info.degree = degree;
                info.linkedPoints = new OldVertexInfo[degree];
                for (int j = 0; j < degree; j++)
                {
                    int realPosIndex = temp[realVertexIndex][j];
                    info.linkedPoints[j] = new OldVertexInfo() { position = distinctPositions[realPosIndex] };
                }
            }

            vertexInfos[i] = info;
        }

        AdjustVertexPositionOld(vertexInfos);
    }
    
    private static void AdjustVertexPositionOld(OldVertexInfo[] vertexInfos)
    {
        if (vertexInfos == null)
        {
            return;
        }

        const float _3_16 = 3.0f / 16.0f;
        foreach (var info in vertexInfos)
        {
            if (info == null)
            {
                continue;
            }

            int degree = info.degree;
            Vector3 neighborPosSum = Vector3.Zero;
            foreach (var t in info.linkedPoints)
            {
                neighborPosSum += t.position;
            }
                
            float u = degree == 3 ? _3_16 : 3.0f / (8 * degree);
            info.position = (1 - degree * u) * info.position + u * neighborPosSum;
        }
    }

    private static void InitNewVertexInfo(Vector3[] vertices, ushort[] triangles, ushort[] realVertexIndices, out NewVertexInfo[] newVertexInfos)
    {
        if (vertices == null || triangles == null || realVertexIndices == null || triangles.Length != realVertexIndices.Length)
        {
            newVertexInfos = null;
            return;
        }

        newVertexInfos = new NewVertexInfo[triangles.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int pointIndex0 = triangles[i];
            int pointIndex1 = triangles[i + 1];
            int pointIndex2 = triangles[i + 2];

            int realVertexIndex0 = realVertexIndices[i];
            int realVertexIndex1 = realVertexIndices[i + 1];
            int realVertexIndex2 = realVertexIndices[i + 2];

            int faceIndex = i / 3 * 10;
            NewVertexInfo info0 = new NewVertexInfo()
            {
                position = (vertices[pointIndex0] + vertices[pointIndex1]) * 0.5f,
                edgeIndex = faceIndex,
                linkedRealVertexIndex0 = realVertexIndex0,
                linkedRealVertexIndex1 = realVertexIndex1,
                linkedPosition0 = vertices[pointIndex0],
                linkedPosition1 = vertices[pointIndex1],
                oppositePosition0 = vertices[pointIndex2],
            };

            NewVertexInfo info1 = new NewVertexInfo()
            {
                position = (vertices[pointIndex1] + vertices[pointIndex2]) * 0.5f,
                edgeIndex = faceIndex + 1,
                linkedRealVertexIndex0 = realVertexIndex1,
                linkedRealVertexIndex1 = realVertexIndex2,
                linkedPosition0 = vertices[pointIndex1],
                linkedPosition1 = vertices[pointIndex2],
                oppositePosition0 = vertices[pointIndex0],
            };

            NewVertexInfo info2 = new NewVertexInfo()
            {
                position = (vertices[pointIndex2] + vertices[pointIndex0]) * 0.5f,
                edgeIndex = faceIndex + 2,
                linkedRealVertexIndex0 = realVertexIndex2,
                linkedRealVertexIndex1 = realVertexIndex0,
                linkedPosition0 = vertices[pointIndex2],
                linkedPosition1 = vertices[pointIndex0],
                oppositePosition0 = vertices[pointIndex1],
            };

            newVertexInfos[i] = info0;
            newVertexInfos[i + 1] = info1;
            newVertexInfos[i + 2] = info2;
        }

        for (int i = 0; i < newVertexInfos.Length; i++)
        {
            NewVertexInfo srcInfo = newVertexInfos[i];
            for (int j = 0; j < newVertexInfos.Length; j++)
            {
                if (j == i)
                {
                    continue;
                }

                NewVertexInfo dstInfo = newVertexInfos[j];
                if (IsOnSameEdge(srcInfo, dstInfo))
                {
                    srcInfo.oppositePosition1 = dstInfo.oppositePosition0;
                    // dstInfo.oppositePosition1 = srcInfo.oppositePosition0;
                }
            }
        }

        AdjustVertexPositionNew(newVertexInfos);
    }
    
    private static void AdjustVertexPositionNew(NewVertexInfo[] vertexInfos)
    {
        if (vertexInfos == null)
        {
            return;
        }

        foreach (var info in vertexInfos)
        {
            if (info == null)
            {
                continue;
            }
                
            info.position = 0.375f * (info.linkedPosition0 + info.linkedPosition1) + 
                            0.125f * (info.oppositePosition0 + info.oppositePosition1);
        }
    }

    private static void CalculateVertexAndTriangle(ushort[] triangles, NewVertexInfo[] newVertexInfos, OldVertexInfo[] oldVertexInfos, out Vector3[] newMeshVertices, out ushort[] newMeshTriangle)
    {
        if (triangles == null || newVertexInfos == null || oldVertexInfos == null || triangles.Length != newVertexInfos.Length)
        {
            newMeshVertices = null;
            newMeshTriangle = null;
            return;
        }

        ushort oldVertexCount = (ushort)oldVertexInfos.Length;
        newMeshVertices = new Vector3[oldVertexCount + newVertexInfos.Length];
        for (int i = 0; i < oldVertexCount; i++)
        {
            newMeshVertices[i] = oldVertexInfos[i].position;
        }

        for (int i = 0; i < newVertexInfos.Length; i++)
        {
            newMeshVertices[i + oldVertexCount] = newVertexInfos[i].position;
        }

        //      1
        //      |   \
        //      A   -   B
        //      |           \
        //      0   -   C   -   2
        // 顺时针为正方向 //
        newMeshTriangle = new ushort[triangles.Length * 4];
        int triangleIndex = 0;
        for (ushort i = 0; i < triangles.Length; i += 3)
        {
            // 0 A C //
            newMeshTriangle[triangleIndex++] = triangles[i];
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i);
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i + 2);

            // A 1 B //
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i);
            newMeshTriangle[triangleIndex++] = triangles[i + 1];
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i + 1);

            // C B 2 //
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i + 2);
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i + 1);
            newMeshTriangle[triangleIndex++] = triangles[i + 2];

            // C A B //
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i + 2);
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i);
            newMeshTriangle[triangleIndex++] = (ushort)(oldVertexCount + i + 1);
        }
    }

    private static bool IsOnSameEdge(NewVertexInfo info0, NewVertexInfo info1)
    {
        return info0 != null && info1 != null &&
               (info0.linkedRealVertexIndex0 == info1.linkedRealVertexIndex0 &&
                info0.linkedRealVertexIndex1 == info1.linkedRealVertexIndex1 ||
                info0.linkedRealVertexIndex0 == info1.linkedRealVertexIndex1 &&
                info0.linkedRealVertexIndex1 == info1.linkedRealVertexIndex0); 
            
        // 等同于以下逻辑，就是有点啰嗦 //
        // if (info0 != null && info1 != null)
        // {
        //     int index00 = info0.linkedRealVertexIndex0;
        //     int index01 = info0.linkedRealVertexIndex1;
        //     int index10 = info1.linkedRealVertexIndex0;
        //     int index11 = info1.linkedRealVertexIndex1;
        //     if (index00 == index10 && index01 == index11 ||
        //         index00 == index11 && index01 == index10)
        //     {
        //         return true;
        //     }
        // }
        // return false;
    }
}