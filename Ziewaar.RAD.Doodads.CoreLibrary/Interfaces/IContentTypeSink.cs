namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IContentTypeSink
{
    string[] Accept { get; }
    string ContentType { get; set; }
}
