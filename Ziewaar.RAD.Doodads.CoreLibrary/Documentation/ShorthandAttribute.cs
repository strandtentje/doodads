namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

[AttributeUsage(AttributeTargets.Class)]
public class ShorthandAttribute(string shorthandFormat) : Attribute
{
    public string Format => shorthandFormat;
}