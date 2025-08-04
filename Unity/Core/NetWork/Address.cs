namespace ET;

public partial struct Address
{
    public int Process;
    public int FiberId;
    
    public Address(int process, int fiber)
    {
        this.Process = process;
        this.FiberId = fiber;
    }
    
    public bool Equals(Address other)
    {
        return this.Process == other.Process && this.FiberId == other.FiberId;
    }
    public override bool Equals(object obj)
    {
        return obj is Address other && Equals(other);
    }
    public static bool operator ==(Address left, Address right)
    {
        return left.Process == right.Process && left.FiberId == right.FiberId;
    }
    public static bool operator !=(Address left, Address right)
    {
        return !(left == right);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Process, this.FiberId);
    }
    public override string ToString()
    {
        return $"{this.Process}:{this.FiberId}";
    }
}