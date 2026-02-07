using System.Numerics;

namespace ET.Client;

public class MassSpringComponent : Entity, IAwake<Vector3, float>, IUpdate, ISerialize, IDeserialize
{
    public Vector3 g;
    public float step;
    
    public readonly List<Particle> particles = new();
    public readonly List<Spring> springs = new();
}

public class Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Force;
        public readonly float Mass;
        public readonly bool Fixed; // 是否被固定（固定的质点不参与积分更新）

        // 构造函数：创建一个质点，初始化位置、质量和是否固定
        public Particle(Vector3 pos, float mass = 1f, bool fixedPoint = false)
        {
            Position = pos;
            Velocity = Vector3.Zero;
            Force = Vector3.Zero;
            Mass = mass;
            Fixed = fixedPoint;
        }
    }

public class Spring
{
    public readonly Particle A;
    public readonly Particle B;
    public readonly float RestLength; // 弹簧的静止长度
    public readonly float Stiffness; // k
    public readonly float Damping; // c

    public Spring(Particle a, Particle b, float stiffness = 100f, float damping = 1f)
    {
        A = a;
        B = b;
        RestLength = Vector3.Distance(a.Position, b.Position);
        Stiffness = stiffness;
        Damping = damping;
    }

    // 计算弹簧对两端质点产生的弹力与阻尼力，并将力累加到质点的 Force 字段上
    // 注意：这里只计算并累加力，不做积分（积分在系统的 Step 中统一做）
    public void ApplyForces()
    {
        Vector3 dir = B.Position - A.Position;
        float currentLength = dir.Length();
        if (currentLength <= 1e-6f) return;

        Vector3 n = dir / currentLength;

        float displacement = currentLength - RestLength; // ▲X

        Vector3 springForce = -Stiffness * displacement * n; // F_s = -k * x * n（作用在 A 上为 +F，B 上为 -F）

        Vector3 relativeVel = B.Velocity - A.Velocity; // 两端相对速度（用于计算沿弹簧方向的阻尼）
        float velAlong = Vector3.Dot(relativeVel, n); // 相对速度在弹簧方向上的分量（标量）
        Vector3 dampingForce = -Damping * velAlong * n; // 摩擦力：沿方向抑制相对速度

        Vector3 totalForce = springForce + dampingForce; // 两个分量相加得到弹簧总外加力（作用在 A）

        A.Force += totalForce; // 将作用在 A 上的力累加到 A.Force（外力求和）
        B.Force -= totalForce; // 作用在 B 上的力方向相反，故减去 same force
    }
}