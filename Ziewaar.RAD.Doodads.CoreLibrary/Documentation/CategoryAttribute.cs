namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Class)]
public class CategoryAttribute(string categoryName) : Attribute
{
    public string Name => categoryName;
}
[AttributeUsage(AttributeTargets.Class)]
public class ShorthandAttribute(string shorthandFormat) : Attribute
{
    public string Format => shorthandFormat;
}