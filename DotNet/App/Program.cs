namespace ET.Server;

internal static class Program
{
    private static void Main(string[] args)
    {
        Init init = new();
        init.Start();
        while (!Options.Instance.NeedClose)
        {
            Thread.Sleep(1);
            try
            {
                init.Update();
                init.LateUpdate();
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
                throw;
            }
        }
        init.Dispose();
    }
}