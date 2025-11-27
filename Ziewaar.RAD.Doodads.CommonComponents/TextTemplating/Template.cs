using System.Globalization;
using System.Xml.Schema;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
#nullable enable

[Category("Printing & Formatting")]
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

             Scalar values such as numbers, datetimes and timespans may be formatted using a colon and then
             a format.

             Examples:
              - {% <uptime:hh:mm %} makes 12:30 for 12 hours and 30 minutes of uptime\
              - {% <today:yyyy-MM-dd %} makes 2025-07-09 for the 9th of july 2025
              - {% <sheepcount:0000 $} makes 0010 if the sheepcount is 10
              - {% &bananas %} makes &lt;bananas&gt; if bananas says `<bananas>`
              - {% %cake %} makes hello%20world if cake is `hello world`

             Exactly one filter modifier and one source modifier may be combined. When omitted, the engine
             defaults to sourcing from memory first, and never filtering.
             """)]
public class Template : IService
{
    private TextSinkingInteraction? templatefile = null;

    [NamedSetting("contenttype",
        "Force the content type to be the specified MIME instead of deriving it from the file")]
    private readonly UpdatingKeyValue ForceContentTypeConstant = new("contenttype");

    private readonly TemplateParser Parser = new();
    private string? ContentTypeOverride;

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
                // OnException?.Invoke(this, new CommonInteraction(interaction, "Template Cache Miss"));
                GlobalLog.Instance?.Warning("Template Cache Miss; {file}", templatefile);
                templatefile.SinkBuffer.Dispose();
                templatefile = null;
            }
        }

        if (interaction.TryGetClosest<ICheckUpdateRequiredInteraction>(out var checkUpdateRequiredInteraction) &&
            checkUpdateRequiredInteraction != null)
        {
            checkUpdateRequiredInteraction.IsRequired = true;
            return;
        }

        if (templatefile == null)
        {
            templatefile = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
            OnThen?.Invoke(this, templatefile);
            Parser.RefreshTemplateData(templatefile.GetDisposingSinkReader());
        }

        if (templatefile.SinkTrueContentType?.Contains('*') == false)
            output.SinkTrueContentType ??= templatefile.SinkTrueContentType;
        StreamWriter writer;

        if ((constants, ForceContentTypeConstant).IsRereadRequired(out string? contentTypeOverrideCandidate))
        {
            this.ContentTypeOverride = contentTypeOverrideCandidate;
        }

        try
        {
            writer = output.GetWriter(ContentTypeOverride ?? templatefile.SinkTrueContentType);
        }
        catch (ContentTypeMismatchException ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            return;
        }

        // default to the server locale
        string requestedLocale = CultureInfo.CurrentCulture.Name;
        if (interaction.TryGetClosest(out LocaleInteraction? li) && li != null)
            // if we have a different locale in scope, lets use that for now
            requestedLocale = li.Locale;

        // however, if the template defines locales in particular,
        // but none of them match the one we desire, 
        // lets select the first true locale it does implement and roll with that.
        if (Parser.Locales.Any() && !Parser.Locales.Any(x => LocaleMatcher.IsLocaleMatch(requestedLocale, x)))
        {
            OnException?.Invoke(
                this,
                new CommonInteraction(
                    interaction,
                    $"""
                     Template render request for locale "{requestedLocale}" was received,
                     however, this template only supports "{string.Join(@""", """, Parser.Locales)}".
                     We will default to "{Parser.Locales.First()}"
                     """));
            requestedLocale = Parser.Locales.First();
        }

        try
        {
            var applicableGroups =
                Parser.CommandsByLocale.Where(group => LocaleMatcher.IsLocaleMatch(group.Key, requestedLocale));
            var applicableSegments = applicableGroups.SelectMany(x => x).OrderBy(x => x.Position);
            foreach (var segment in applicableSegments)
            {
                switch (segment.Type & TemplateCommandType.AllSources)
                {
                    case TemplateCommandType.LiteralSource:
                        writer.Write(segment.PayloadText);
                        break;

                    case TemplateCommandType.VariableSource
                        when interaction.TryFindVariable<object>(segment.PayloadText, out var rawValue):
                        if (rawValue is string rawText)
                            writer.Write(segment.Type.ApplyFilterTo(rawText));
                        else
                            writer.Write(
                                segment.Type.ApplyFilterTo(Convert.ToString(segment.GetFormattedPayload(rawValue))));
                        break;

                    case TemplateCommandType.CallOutSource
                        when (segment.Type & TemplateCommandType.AllFilters) == TemplateCommandType.NoFilter:
                        OnElse?.Invoke(this, new CommonInteraction(interaction, segment.PayloadText));
                        break;

                    case TemplateCommandType.CallOutSource
                        when (segment.Type & TemplateCommandType.AllFilters) != TemplateCommandType.NoFilter:
                        var callOutIntermediate = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
                        OnElse?.Invoke(this, callOutIntermediate);
                        writer.Write(
                            segment.Type.ApplyFilterTo(callOutIntermediate.GetDisposingSinkReader().ReadToEnd()));
                        break;

                    case TemplateCommandType.CallOutOrVariable:
                        if (!interaction.TryFindVariable<object>(segment.PayloadText, out var defaultValue))
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
                        else if (segment.Formatter != null && defaultValue is not null)
                            writer.Write(segment.Type.ApplyFilterTo(segment.Formatter(defaultValue)));
                        else
                            writer.Write(segment.Type.ApplyFilterTo(Convert.ToString(defaultValue)));

                        break;

                    case TemplateCommandType.ConstantSource:
                        if (!constants.NamedItems.TryGetValue(segment.PayloadText, out var rawObjectConstant))
                            break;
                        if (rawObjectConstant is string rawStringConstant)
                            writer.Write(segment.Type.ApplyFilterTo(rawStringConstant));
                        else if (segment.Formatter != null && rawObjectConstant is not null)
                            writer.Write(segment.Type.ApplyFilterTo(segment.Formatter(rawObjectConstant)));
                        else
                            writer.Write(segment.Type.ApplyFilterTo(Convert.ToString(rawObjectConstant)));
                        break;
                }
            }
        }
        finally
        {
            writer.Flush();
            writer.Close();
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}