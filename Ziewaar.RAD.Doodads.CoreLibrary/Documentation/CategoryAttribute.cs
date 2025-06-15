namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

[AttributeUsage(AttributeTargets.Class)]
public class CategoryAttribute(string categoryName) : Attribute
{
    public string Name => categoryName;
}