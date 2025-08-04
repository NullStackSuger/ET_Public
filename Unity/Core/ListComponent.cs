namespace ET;

public class ListComponent<T>: List<T>, IDisposable
{
    /// <summary>
    /// 注意这里必须是public, 不然对象池反射时出问题
    /// </summary>
    public ListComponent()
    {
    }
        
    public static ListComponent<T> Create()
    {
        return ObjectPool.Fetch(typeof (ListComponent<T>)) as ListComponent<T>;
    }

    public void Dispose()
    {
        if (this.Capacity > 64) // 超过64，让gc回收
        {
            return;
        }
        this.Clear();
        ObjectPool.Recycle(this);
    }
}