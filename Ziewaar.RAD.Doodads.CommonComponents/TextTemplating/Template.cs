namespace Ziewaar.RAD.Doodads.CommonComponents;
#nullable enable
public class Template : IService
{
    private TextSinkingInteraction? templatefile = null;
    private readonly TemplateParser Parser = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var output) || output == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "No sink found to template into."));
            return;
        }

        if (templatefile != null)
        {
            var updateChecker = new CheckUpdateRequiredInteraction(interaction, templatefile);
            OnThen?.Invoke(this, updateChecker);
            if (updateChecker.IsRequired)
            {
                templatefile.SinkBuffer.Dispose();
                templatefile = null;
            }
        }

        if (templatefile == null)
        {
            templatefile = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
            OnThen?.Invoke(this, templatefile);
            Parser.RefreshTemplateData(templatefile.GetDisposingSinkReader());
        }

        if (templatefile.SinkTrueContentType?.Contains('*') == false)
            output.SinkTrueContentType ??= templatefile.SinkTrueContentType;
        using var writer = output.GetWriter(templatefile.SinkTrueContentType);

        foreach (var segment in Parser.TemplateCommands)
        {
            switch (segment.Type | TemplateCommandType.AllSources)
            {
                case TemplateCommandType.LiteralSource:
                    writer.Write(segment.Payload);
                    break;

                case TemplateCommandType.VariableSource
                    when interaction.TryFindVariable<object>(segment.Payload, out var rawValue):
                    if (rawValue is string rawText)
                        writer.Write(segment.Type.ApplyFilterTo(rawText));
                    else
                        writer.Write(segment.Type.ApplyFilterTo(Convert.ToString(rawValue)));
                    break;

                case TemplateCommandType.CallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) == TemplateCommandType.NoFilter:
                    OnElse?.Invoke(this, new CommonInteraction(interaction, segment.Payload));
                    break;

                case TemplateCommandType.CallOutSource
                    when (segment.Type & TemplateCommandType.AllFilters) != TemplateCommandType.NoFilter:
                    var callOutIntermediate = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
                    OnElse?.Invoke(this, callOutIntermediate);
                    writer.Write(segment.Type.ApplyFilterTo(callOutIntermediate.GetDisposingSinkReader().ReadToEnd()));
                    break;

                case TemplateCommandType.CallOutOrVariable:
                    if (!interaction.TryFindVariable<object>(segment.Payload, out var defaultValue))
                    {
                        var attemptedCallOut = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
                        OnElse?.Invoke(this, attemptedCallOut);
                        var text = attemptedCallOut.GetDisposingSinkReader().ReadToEnd();
                        writer.Write(segment.Type.ApplyFilterTo(text));
                        if (text.Length > 0)
                            break;
                    }
                    if (defaultValue is string defaultString)
                        writer.Write(segment.Type.ApplyFilterTo(defaultString));
                    else
                        writer.Write(segment.Type.ApplyFilterTo(Convert.ToString(defaultValue)));
                    
                    break;

                case TemplateCommandType.ConstantSource:
                    if (!constants.NamedItems.TryGetValue(segment.Payload, out var rawObjectConstant))
                        break;
                    if (rawObjectConstant is string rawStringConstant)
                        writer.Write(segment.Type.ApplyFilterTo(rawStringConstant));
                    else 
                        writer.Write(segment.Type.ApplyFilterTo(Convert.ToString(rawObjectConstant)));
                    break;
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}