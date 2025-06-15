namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Field)]
public class PrimarySettingAttribute(string text) : Attribute, IHaveText
{
    public string Text => text;
}