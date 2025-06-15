namespace Ziewaar.RAD.Doodads.CommonComponents;
#nullable enable
[Category("Output to Sink")]
[Title("Templating service")]
[Description("""
             This service will use the OnThen-branch to buffer some text, and passes it 
             down to the closest sink. In case this buffered text contains template tags, templating
             will happen. The basic construction of template tags is {% tagname %}
             Keys in the template tags may be prefixed with modifiers to alter behaviour of 
             the templating routine;
             
             Source modifiers: 
              - `<` : Prefixing with a left arrow will restrict sourcing to taking from memory; t
                agnames become memory names.
              - `>` : Prefixing with a right arrow will restrict sourcing to letting an OnElse-
                service write based on the Register-content; tagname will be put into register.
              - `#` : Prefixing with a pound sign restricts value sources to the service constants 
                ie. Template(tagname = "cheese") 
              - No source modifier means it will first look in memory, then call out.
             
             Filter modifiers:
              - `&` : Prefixing with an ampersand will escape strings to be safe for HTML pages.
              - `%` : Prefixing with a percent sign will escape strings to be safe for URL usage.
              - `=` : Prefixing with an equals-sign will escape strings to be safe for HTML attribute usage.
              - `;` : Prefixing with a semicolon will escape strings to be safe for JS string literal usage.
              
             Exactly one filter modifier and one source modifier may be combined. When omitted, the engine
             defaults to sourcing from memory first, and never filtering.
             """)]
public class Template : IService
{
    private TextSinkingInteraction? templatefile = null;
    private readonly TemplateParser Parser = new();
    [EventOccasion("When the template needs to buffer an updated version of the template text")]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   For tags that started with an `>`, or didn't start with anything and weren't found in memory. 
                   Will put the tag name in Register.
                   """)]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Likely happens when the template couldn't find a place to write the result to.
                   """)]
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
            switch (segment.Type & TemplateCommandType.AllSources)
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