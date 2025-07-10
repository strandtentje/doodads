using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;

public class HtmlForm : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnValid;
    public event CallForInteraction? OnInvalid;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var targetSink) || targetSink == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "may only do this where there's another sink"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction, textEncoding: targetSink.TextEncoding);
        OnThen?.Invoke(this, tsi);
        HtmlDocument doc = new();
        tsi.SinkBuffer.Position = 0;
        doc.Load(tsi.SinkBuffer, targetSink.TextEncoding);
        var fieldset = ValidatingInputFieldSet.Parse(doc);

        ICsrfFields? csrfFields = interaction.TryGetClosest<ICsrfTokenSourceInteraction>(out var csrf) && csrf != null ?
            csrf.Fields : null;

        if (interaction.TryFindVariable<string>("method", out string? candidateMethod) && candidateMethod != null &&
            interaction.TryFindVariable<string>("url", out string? candiateUrl) && candiateUrl != null &&
            fieldset.IsValidationRequired(candidateMethod, candiateUrl))
        {
            IReadOnlyDictionary<string, IEnumerable> parsedForm;
            if (fieldset.Method == HttpMethod.Post
                && interaction.TryGetClosest<ISourcingInteraction>(out var bodySource)
                && bodySource != null)
            {
                using (var reader = new StreamReader(bodySource.SourceBuffer, bodySource.TextEncoding))
                {
                    parsedForm = new FormDataDictionary(reader.ReadToEnd());
                }
            }
            else if (fieldset.Method == HttpMethod.Get
                && interaction.TryFindVariable<string>("query", out string? queryString) && queryString != null)
                parsedForm = new FormDataDictionary(queryString);
            else
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "bad method or no data"));
                return;
            }
            if (csrfFields != null)
                parsedForm = new DeobfuscatedFormDictionary(parsedForm, csrfFields, fieldset.ToString());
            try
            {
                var result = fieldset.Validate(parsedForm);
                if (!result.Any(x => x.isError))
                {
                    SortedList<string, object> saneValues = new();
                    foreach (var item in result)
                    {
                        if (item.value is IEnumerable enumerable)
                        {
                            var instances = enumerable.OfType<object>().ToArray();
                            if (instances.Length == 1)
                            {
                                saneValues[item.name] = instances[0];
                            } else if (instances.Length > 1)
                            {
                                saneValues[item.name] = instances;
                            }
                        } else if (item.value is object singleItem)
                        {
                            saneValues[item.name] = singleItem;
                        }
                    }
                    OnValid?.Invoke(this, new CommonInteraction(interaction, saneValues));
                } else
                {
                    foreach (var item in result.Where(x=>x.isError))
                    {
                        doc.SetErrorSpan(item.name);
                    }
                    OnInvalid?.Invoke(this, interaction);
                }
            }
            finally
            {
                if (parsedForm is IDisposable disposable) disposable.Dispose();
            }
        }

        if (csrfFields != null)
            fieldset.ApplyObfuscation(csrfFields);

        doc.Save(targetSink.SinkBuffer, targetSink.TextEncoding);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}