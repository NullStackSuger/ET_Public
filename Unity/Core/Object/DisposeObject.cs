using System.ComponentModel;

namespace ET;

public abstract class DisposeObject: Object, IDisposable
{
    public virtual void Dispose()
    {
    }
    
    /// <summary>
    /// 序列化开始之前使用
    /// </summary>
    public virtual void OnSerialize()
    {
    }
        
    /// <summary>
    /// 反序列化之后使用
    /// (正常是序列化之后使用的)
    /// </summary>
    public virtual void OnDeserialize()
    {
    }
}

public interface IPool : IDisposable
{
    bool IsFromPool { get; set; }
}