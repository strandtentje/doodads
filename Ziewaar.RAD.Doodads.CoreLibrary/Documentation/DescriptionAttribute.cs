namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Class)]
public class DescriptionAttribute(string descriptionText) : Attribute
{
    public string Description => descriptionText;
}