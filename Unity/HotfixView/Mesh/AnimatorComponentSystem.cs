using System.Numerics;
using Assimp;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

namespace ET.Client;

[EntitySystemOf(typeof(AnimatorComponent))]
public static partial class AnimatorComponentSystem
{
    [EntitySystem]
    private static void Awake(this AnimatorComponent self, List<Animation> animations)
    {
        self.animations = new();
        foreach (Animation animation in animations)
        {
            self.animations.Add(animation.Name, animation);
        }
        
        MeshComponent meshComponent = self.Parent.GetComponent<MeshComponent>();
        self.positions = meshComponent.meshInfo.positions;
    }

    [EntitySystem]
    private static void Serialize(this AnimatorComponent self)
    {
        
    }

    [EntitySystem]
    private static void Deserialize(this AnimatorComponent self)
    {
        
    }

    [EntitySystem]
    private static void LateUpdate(this AnimatorComponent self)
    {
        if (self.tmp > 30) return;
        self.PlayTest(self.tmp);
        self.tmp += 0.1f;
        
        Console.WriteLine(self.tmp);
    }

    public static void PlayTest(this AnimatorComponent self, float time)
    {
        MeshComponent meshComponent = self.Parent.GetComponent<MeshComponent>();
        JointComponent jointComponent = self.Parent.GetComponent<JointComponent>();
        
        var positions = meshComponent.meshInfo.positions;
        var jointMap = jointComponent.jointMap;

        // TODO 动画系统/动画的数据我不清楚怎么使用
        /*jointMap["Head"].local.B4 += -10 * MathF.Cos(time * MathF.PI / 180);
        jointMap["Head"].local.C4 += 10 * MathF.Cos(time * MathF.PI / 180);*/
        jointMap["Head"].local.A4 = time;
        
        jointComponent.UpdateModel();
        
        self.positions = new Vector3[positions.Length];
        foreach (var joint in jointMap.Values)
        {
            foreach (VertexWeight vertexWeight in joint.vertexWeights)
            {
                var vertex = positions[vertexWeight.VertexID];
                
                self.positions[vertexWeight.VertexID] += vertexWeight.Weight * (joint.model * joint.offset * vertex.ToVector3()).ToVector3();
            }
        }
    }

    public static void Play(this AnimatorComponent self, float time)
    {
        MeshComponent meshComponent = self.Parent.GetComponent<MeshComponent>();
        JointComponent jointComponent = self.Parent.GetComponent<JointComponent>();
     
        var animation = self.animations.Values.First();
        var positions = meshComponent.meshInfo.positions;
        var jointMap = jointComponent.jointMap;
        
        foreach (NodeAnimationChannel channel in animation.NodeAnimationChannels)
        {
            string name = channel.NodeName;
            Vector3 position = GetVector3(time, channel.PositionKeys);
            Quaternion rotation = GetQuaternion(time, channel.RotationKeys);
            Vector3 scale = GetVector3(time, channel.ScalingKeys);
            JointComponent.JointInfo joint = jointMap[name];
            
            joint.local = (Matrix4x4.CreateScale(scale) *
                           Matrix4x4.CreateFromQuaternion(rotation) *
                           Matrix4x4.CreateTranslation(position)).ToMatrix4x4();   
        }

        jointComponent.UpdateModel();

        self.positions = new Vector3[positions.Length];
        foreach (var joint in jointMap.Values)
        {
            foreach (VertexWeight vertexWeight in joint.vertexWeights)
            {
                var vertex = positions[vertexWeight.VertexID];
                
                self.positions[vertexWeight.VertexID] += vertexWeight.Weight * (joint.model * joint.offset * vertex.ToVector3()).ToVector3();
            }
        }

        static Vector3 GetVector3(float time, List<VectorKey> keys)
        {
            int preFrame = -1;
            int nextFrame = -1;
            
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Time <= time)
                {
                    preFrame = i;
                }
                // 这里是>=能在关键帧上时preFrame = nextFrame
                if (keys[i].Time >= time)
                {
                    nextFrame = i;
                    break;
                }
            }
            if (preFrame == -1 || nextFrame == -1 || nextFrame < preFrame)
            {
                Log.Instance.Error($"动画计算关键帧有问题: PreFrame: {preFrame}, NextFrame: {nextFrame}");
            }

            if (preFrame == nextFrame)
            {
                return keys[preFrame].Value.ToVector3();
            }

            float lerp = (float)((time - keys[preFrame].Time) / (keys[nextFrame].Time - keys[preFrame].Time));
            return Vector3.Lerp(keys[preFrame].Value.ToVector3(), keys[nextFrame].Value.ToVector3(), lerp);
        }
        static Quaternion GetQuaternion(float time, List<QuaternionKey> keys)
        {
            int preFrame = -1;
            int nextFrame = -1;
            
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Time <= time)
                {
                    preFrame = i;
                }
                // 这里是>=能在关键帧上时preFrame = nextFrame
                if (keys[i].Time >= time)
                {
                    nextFrame = i;
                    break;
                }
            }
            if (preFrame == -1 || nextFrame == -1 || nextFrame < preFrame)
            {
                Log.Instance.Error($"动画计算关键帧有问题: PreFrame: {preFrame}, NextFrame: {nextFrame}");
            }
            
            if (preFrame == nextFrame)
            {
                return keys[preFrame].Value.ToQuaternion();
            }

            float lerp = (float)((time - keys[preFrame].Time) / (keys[nextFrame].Time - keys[preFrame].Time));
            return Quaternion.Slerp(keys[preFrame].Value.ToQuaternion(), keys[nextFrame].Value.ToQuaternion(), lerp);
        }
    }
}