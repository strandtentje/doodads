namespace Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
[AttributeUsage(AttributeTargets.Field)]
public class NamedSettingAttribute(string name, string text) : Attribute
{
    public string Name => name;
    public string Text => text;
}