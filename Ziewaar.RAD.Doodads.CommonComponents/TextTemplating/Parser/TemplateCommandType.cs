namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

[Flags]
public enum TemplateCommandType
{
    LiteralSource =
        0b0_0000,

    VariableSource =
        0b0_0001,

    CallOutSource =
        0b0_0010,

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

    UrlDataFilter = 
        0b10_0000_0000,
    
    AllFilters = 0b11_1110_0000,
}