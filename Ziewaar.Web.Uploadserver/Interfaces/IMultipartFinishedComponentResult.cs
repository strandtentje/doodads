namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IMultipartFinishedComponentResult
{
    string Name { get; }
    string[] Values { get; }
    bool IsFile { get; }
}