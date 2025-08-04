namespace ET;

public partial struct ActorId
{
    public int Process;
    public int FiberId;
    public long InstanceId;
    
    public ActorId(int process, int fiberId, int instanceId = 1)
    {
        Process = process;
        FiberId = fiberId;
        InstanceId = instanceId;
    }
    public ActorId(Address address, int instanceId = 1)
    {
        Process = address.Process;
        FiberId = address.FiberId;
        InstanceId = instanceId;
    }
    
    public bool Equals(ActorId other)
    {
        return this.Process == other.Process && this.FiberId == other.FiberId && this.InstanceId == other.InstanceId;
    }
    public override bool Equals(object obj)
    {
        return obj is ActorId other && Equals(other);
    }
    public static bool operator ==(ActorId left, ActorId right)
    {
        return left.Process == right.Process && left.FiberId == right.FiberId && left.InstanceId == right.InstanceId;
    }
    public static bool operator !=(ActorId left, ActorId right)
    {
        return !(left == right);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Process, this.FiberId, this.InstanceId);
    }
    public override string ToString()
    {
        return $"{this.Process}:{this.FiberId}:{this.InstanceId}";
    }
}