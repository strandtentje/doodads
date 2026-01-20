namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

[AttributeUsage(AttributeTargets.Class)]
public class ShortNamesAttribute(params string[] names) : Attribute
{
    public string[] Names => names;
}