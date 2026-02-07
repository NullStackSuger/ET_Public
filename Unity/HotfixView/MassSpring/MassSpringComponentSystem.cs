using System.Numerics;

namespace ET.Client;

[EntitySystemOf(typeof(MassSpringComponent))]
public static partial class MassSpringComponentSystem
{
    [EntitySystem]
    private static void Awake(this MassSpringComponent self, Vector3 g, float step)
    {
        self.g = g;
        self.step = step;
    }

    [EntitySystem]
    private static void Update(this MassSpringComponent self)
    {
        foreach (var p in self.particles)
        {
            p.Force = Vector3.Zero;
        }
        
        // 重力
        foreach (var p in self.particles)
        {
            if (!p.Fixed)
            {
                p.Force += self.g * p.Mass;
            }
        }
        
        // 弹力 摩擦力
        foreach (var s in self.springs)
        {
            s.ApplyForces();
        }
        
        foreach (var p in self.particles)
        {
            if (p.Fixed) continue;

            Vector3 acceleration = p.Force / p.Mass; // a = F / m，得到质点的加速度
            p.Velocity += acceleration * self.step; // 半隐式欧拉：v_{t+dt} = v_t + a * dt
            p.Position += p.Velocity * self.step; // x_{t+dt} = x_t + v_{t+dt} * dt（使用新速度）
        }
    }

    [EntitySystem]
    private static void Serialize(this MassSpringComponent self)
    {
        
    }
    
    [EntitySystem]
    private static void Deserialize(this MassSpringComponent self)
    {
        
    }
    
    public static void AddParticle(this MassSpringComponent self, Particle p) => self.particles.Add(p);
    public static void AddSpring(this MassSpringComponent self, Spring s) => self.springs.Add(s);

    public static void Test(this MassSpringComponent self)
    {
        Particle[,] particles = new Particle[4, 4];
        for (int x = 0; x < particles.GetLength(0); x++)
        {
            for (int y = 0; y < particles.GetLength(1); y++)
            {
                if ((x == 0 && y == 3)  || (x == 3 && y == 3))
                    particles[x, y] = new Particle(new Vector3(x, 0, y), 1, true);
                else
                    particles[x, y] = new Particle(new Vector3(x, 0, y));
                self.particles.Add(particles[x, y]);
            }
        }
        for (int x = 0; x < particles.GetLength(0); x++)
        {
            for (int y = 0; y < particles.GetLength(1); y++)
            {
                if (x != particles.GetLength(0) - 1 )
                {
                    self.AddSpring(new Spring(particles[ x + 1, y], particles[x, y]));
                    if(y!=0) self.AddSpring(new Spring(particles[ x + 1, y-1], particles[x, y]));
                    if(y!=particles.GetLength(1) - 1) self.AddSpring(new Spring(particles[ x + 1, y+1], particles[x, y]));
                    if(x!=particles.GetLength(0) - 2) self.AddSpring(new Spring(particles[ x + 2, y], particles[x, y]));
                }

                if (y != particles.GetLength(1) - 1)
                {
                    self.AddSpring(new Spring(particles[ x, y+1], particles[x, y]));
                    if(y!=particles.GetLength(1) - 2)  self.AddSpring(new Spring(particles[ x, y+2], particles[x, y]));
                }
            }
        }

        Console.WriteLine(self.springs.Count);
        foreach (Spring spring in self.springs)
        {
            Console.WriteLine($"({spring.A.Position.X}, {spring.A.Position.Z}) | ({spring.B.Position.X}, {spring.B.Position.Z})");
        }
    }
}