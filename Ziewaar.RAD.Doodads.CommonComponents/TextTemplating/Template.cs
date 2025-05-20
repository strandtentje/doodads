namespace Ziewaar.RAD.Doodads.CommonComponents;

public class Template : IService
{
    private readonly TemplateParser Parser = new();

    [NamedBranch] public event EventHandler<IInteraction> Format;
    [NamedBranch] public event EventHandler<IInteraction> Placeholder;
    public event EventHandler<IInteraction> OnError;
    public void Enter(ServiceConstants constants, IInteraction interaction)
    {
        var templateRequestInteraction =
            new TemplateSinkingInteraction<Stream>(interaction, this.Parser.CurrentTemplateData);
        Format?.Invoke(this, templateRequestInteraction);
        Parser.RefreshTemplateData(templateRequestInteraction.TaggedData);

        var wildcardName = constants.InsertIgnore("wildcardname", "placeholder");
        var wildcardValueFormat = constants.InsertIgnore("wildcardvalue", "{0}");

        var outputSinker = interaction.ResurfaceWriter();
        outputSinker.TaggedData.Tag.IsTainted = true;
        var writer = outputSinker.TaggedData.Data;

        VariablesInteraction CreateWildcardForSegment(TemplateCommand segment) => 
            VariablesInteraction.
            CreateBuilder(outputSinker).
            Add(wildcardName, string.Format(wildcardValueFormat, segment.Payload)).
            Build();

        foreach (var segment in Parser.TemplateCommands)
        {
            switch (segment.Type | TemplateCommandType.AllSources)
            {
                case TemplateCommandType.LiteralSource:
                    writer.Write(segment.Payload);
                    break;

                case TemplateCommandType.VariableSource
                    when interaction.Variables.TryGetValue(segment.Payload, out var rawObject) &&
                         rawObject is string rawText:
                    writer.Write(segment.Type.ApplyFilterTo(rawText));
                    break;

                case TemplateCommandType.CallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) == TemplateCommandType.NoFilter:
                    Placeholder?.Invoke(this, CreateWildcardForSegment(segment));
                    break;

                case TemplateCommandType.CallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) != TemplateCommandType.NoFilter:
                    var callOutIntermediate = new RawStringSinkingInteraction(CreateWildcardForSegment(segment));
                    Placeholder?.Invoke(this, callOutIntermediate);
                    writer.Write(segment.Type.ApplyFilterTo(callOutIntermediate.GetFullString()));
                    break;

                case TemplateCommandType.CallOutOrVariable:
                    var perhapsCallout = new RawStringSinkingInteraction(CreateWildcardForSegment(segment));
                    Placeholder?.Invoke(this, perhapsCallout);

                    if (!perhapsCallout.TaggedData.Tag.IsTainted &&
                        interaction.Variables.TryGetValue(segment.Payload, out var rawAlternative) &&
                        rawAlternative is string rawAltText)
                        writer.Write(segment.Type.ApplyFilterTo(rawAltText));
                    else
                        writer.Write(segment.Type.ApplyFilterTo(perhapsCallout.GetFullString()));
                    break;

                case TemplateCommandType.ConstantSource:
                    if (!constants.TryGetValue(segment.Payload, out var rawObjectConstant))
                        constants.Add(segment.Payload, rawObjectConstant = "");
                    if (rawObjectConstant is not string rawStringConstant)
                        constants[segment.Payload] = rawStringConstant = "";
                    writer.Write(segment.Type.ApplyFilterTo(rawStringConstant));
                    break;

            }
        }

        
    }
}