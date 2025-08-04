namespace ET.Client;

public static class UiHelper
{
    public static void UiUpdate(Entity entity)
    {
        if (entity is not IUi)
        {
            return;
        }

        List<SystemObject> iUiSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(entity.GetType(), typeof(IUiSystem));
        if (iUiSystems == null)
        {
            return;
        }

        foreach (IUiSystem uiSystem in iUiSystems)
        {
            if (uiSystem == null)
            {
                continue;
            }

            try
            {
                uiSystem.Run(entity);
            }
            catch (Exception e)
            {
                Log.Instance.Error(e);
            }
        }
    } 
}