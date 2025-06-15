namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Class)]
public class TitleAttribute(string titleText) : Attribute
{
    public string Title => titleText;
}