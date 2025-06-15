namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Field)]
public class NamedSettingAttribute(string key, string text) : Attribute, IHaveText
{
    public string Key => key;
    public string Text => text;
}