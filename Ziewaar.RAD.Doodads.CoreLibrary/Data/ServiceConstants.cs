namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public class ServiceConstants : SortedList<string, object>
{    
    public ServiceConstants() : base()
    {

    }
    public ServiceConstants(SortedList<string,object> origin) : base(origin)
    {

    }
    public TimeSpan LastChange;
}
