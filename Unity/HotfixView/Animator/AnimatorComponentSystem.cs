using Assimp;

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
            if (animation.Name == "")
                animation.Name = "Default Animation";
            self.animations.Add(animation.Name, animation);
        }
        
        MeshComponent meshComponent = self.Parent.GetComponent<MeshComponent>();
        self.positions = [..meshComponent.meshInfo.positions];
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
        if (!self.animations.TryGetValue(self.currentName, out Animation animation)) return;
        
        float maxTime = (float)(animation.DurationInTicks / animation.TicksPerSecond);
        self.currentTime = Math.Clamp(self.currentTime + 0.01f, 0, maxTime); // TODO 动画系统/动画增量应该用DeltaTime
        
        self.Play(self.currentName, self.currentTime);

        if (self.currentTime >= maxTime) self.currentTime = 0;
    }

    public static void Play(this AnimatorComponent self, string animationName)
    {
        if (!self.animations.TryGetValue(animationName, out Animation animation)) return;
        
        self.currentName = animationName;
        self.currentTime = 0;
    }

    public static void Play(this AnimatorComponent self, string animationName, float time)
    {
        if (!self.animations.TryGetValue(animationName, out Animation animation)) return;
        
        MeshComponent meshComponent = self.Parent.GetComponent<MeshComponent>();
        JointComponent jointComponent = self.Parent.GetComponent<JointComponent>();
        var positions = meshComponent.meshInfo.positions;
        var jointMap = jointComponent.jointMap;
        var modelMap = new Dictionary<string, Matrix4x4>();
        
        foreach (NodeAnimationChannel channel in animation.NodeAnimationChannels)
        {
            string name = channel.NodeName;
            Vector3D position = Lerp(time, channel.PositionKeys).ToVector3();
            Quaternion rotation = SLerp(time, channel.RotationKeys).ToQuaternion();
            Vector3D scale = Lerp(time, channel.ScalingKeys).ToVector3();
            JointComponent.JointInfo joint = jointMap[name];
            joint.local = Matrix4x4.FromScaling(scale) * rotation.FromQuaternion() * Matrix4x4.FromTranslation(position);
        }
        
        for (int i = 0; i < self.positions.Count; i++)
        {
            self.positions[i] = System.Numerics.Vector3.Zero;
        }
        foreach (var joint in jointMap.Values)
        {
            foreach (VertexWeight vertexWeight in joint.vertexWeights)
            {
                var vertex = positions[vertexWeight.VertexID];

                if (!modelMap.ContainsKey(joint.name))
                {
                    modelMap.Add(joint.name, joint.LocalToWorld());
                }
                
                self.positions[vertexWeight.VertexID] += vertexWeight.Weight * (joint.offset * modelMap[joint.name] * vertex.ToVector3()).ToVector3();
            }
        }
        
        static System.Numerics.Vector3 Lerp(float time, List<VectorKey> keys)
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
            return System.Numerics.Vector3.Lerp(keys[preFrame].Value.ToVector3(), keys[nextFrame].Value.ToVector3(), lerp);
        }
        static System.Numerics.Quaternion SLerp(float time, List<QuaternionKey> keys)
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
            return System.Numerics.Quaternion.Slerp(keys[preFrame].Value.ToQuaternion(), keys[nextFrame].Value.ToQuaternion(), lerp);
        }
    }
}