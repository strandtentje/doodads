namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public interface ITypeAdaptor
{
    Type TypeToAdapt { get; }
    object DownConvert(object source);
}