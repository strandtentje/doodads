namespace Ziewaar.RAD.Doodads.CommonComponents;

public class Template : IService
{
    private readonly TemplateParser Parser = new();

    [NamedBranch] public event EventHandler<IInteraction> RequestTemplateData;
    [WildcardBranch] public event EventHandler<IInteraction> PlaceholderDataRequested;
    [SuggestedWildcards] public string[] Wildcards { get; private set; } = [];

    public void Enter(ServiceConstants constants, IInteraction interaction)
    {
        var templateRequestInteraction =
            new TemplateSinkingInteraction<Stream>(interaction, this.Parser.CurrentTemplateData);
        RequestTemplateData?.Invoke(this, templateRequestInteraction);
        if (Parser.RefreshTemplateData(templateRequestInteraction.TaggedData))
            Wildcards = Parser.TemplateCommands.Where(x => (x.Type | TemplateCommandType.AllSources) > 0)
                .Select(x => x.Payload).ToArray();

        var outputSinker = interaction.ResurfaceWriter();
        outputSinker.TaggedData.Tag.IsTainted = true;
        var writer = outputSinker.TaggedData.Data;

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
                    PlaceholderDataRequested?.Invoke(this,
                        new TemplateWildcardInteraction(outputSinker, segment.Payload));
                    break;
                case TemplateCommandType.ContextCallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) == TemplateCommandType.NoFilter:
                    PlaceholderDataRequested?.Invoke(this,
                        new TemplateWildcardWithVariableInteraction(outputSinker, segment.Payload));
                    break;
                case TemplateCommandType.CallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) != TemplateCommandType.NoFilter:
                    var callOutIntermediate = new RawStringAlwaysSinkingWildcardInteraction(interaction, segment.Payload);
                    PlaceholderDataRequested?.Invoke(this, callOutIntermediate);
                    writer.Write(segment.Type.ApplyFilterTo(callOutIntermediate.GetFullString()));
                    break;
                case TemplateCommandType.ContextCallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) != TemplateCommandType.NoFilter:
                    var contextCallOutIntermediate = new RawStringAlwaysWithVariableSinkingWildcardInteraction(interaction, segment.Payload);
                    PlaceholderDataRequested?.Invoke(this, contextCallOutIntermediate);
                    writer.Write(segment.Type.ApplyFilterTo(contextCallOutIntermediate.GetFullString()));
                    break;
                case TemplateCommandType.CallOutOrVariable:
                    var optionalCallOutOrIntermediate =
                        new RawStringAlwaysSinkingWildcardInteraction(interaction, segment.Payload);
                    PlaceholderDataRequested?.Invoke(this, optionalCallOutOrIntermediate);
                    if (optionalCallOutOrIntermediate.TaggedData.Tag.IsTainted &&
                        interaction.Variables.TryGetValue(segment.Payload, out var rawAlternative) &&
                        rawAlternative is string rawAltText)
                        writer.Write(segment.Type.ApplyFilterTo(rawAltText));
                    else
                        writer.Write(segment.Type.ApplyFilterTo(optionalCallOutOrIntermediate.GetFullString()));
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