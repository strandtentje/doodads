namespace Ziewaar.RAD.Doodads.CommonComponents;

[Flags]
public enum TemplateCommandType
{
    LiteralSource =
        0b0_0000,

    VariableSource =
        0b0_0001,

    CallOutSource =
        0b0_0010,

    ContextCallOutSource =
        0b0_0100,

    CallOutOrVariable =
        0b0_1000,

    ConstantSource =
        0b1_0000,
    
    AllSources = 0b1_1111,

    NoFilter =
        0b0_0000_0000,

    HtmlFilter =
        0b0_0010_0000,

    UrlFilter =
        0b0_0100_0000,

    AttributeFilter =
        0b0_1000_0000,

    JsFilter =
        0b1_0000_0000,
    
    AllFilters = 0b1_1110_0000,
}