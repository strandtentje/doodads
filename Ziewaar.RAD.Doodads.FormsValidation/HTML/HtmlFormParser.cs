using System.Dynamic;
using System.Text;
using FormHandling.Interfaces;
using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class HtmlFormParser(IFormFieldObfuscator csrfModifier)
{
    public bool TryParse(HtmlDocument document, [NotNullWhen(true)] out ParsedForm? resultForm)
    {
        var formNode = document.DocumentNode.SelectSingleNode("//form");
        if (formNode == null)
        {
            resultForm = null;
            return false;
        }

        var methodFound = formNode.GetAttributeValue("method", "GET").ToUpperInvariant();
        if (methodFound != "POST" && methodFound != "GET")
        {
            resultForm = null;
            return false;
        }
        resultForm = new ParsedForm
        {
            FullRoute = formNode.GetAttributeValue("action", ""),
            Method = HttpMethod.Parse(methodFound),
        };

        var inputNodes = formNode.SelectNodes(".//input|.//select|.//textarea");
        if (inputNodes != null)
        {
            foreach (var input in inputNodes)
            {
                var name = input.GetAttributeValue("name", "");
                if (string.IsNullOrEmpty(name)) continue;

                var field = new ParsedFormField
                {
                    OriginalName = name,
                    NameInRequest = csrfModifier.Obfuscate(name),
                    Required = input.Attributes.FirstOrDefault(x => x.Name == "required") != null,
                    MinLength = input.GetAttributeValue("minlength", 0),
                    MaxLength = input.GetAttributeValue("maxlength", 32),
                    MinValue = input.GetAttributeValue("min", ""),
                    MaxValue = input.GetAttributeValue("max", ""),
                    Pattern = input.GetAttributeValue("pattern", ""),
                    Multiple = input.Name == "select" &&
                               input.Attributes.FirstOrDefault(x => x.Name == "multiple") != null || 
                    input.,
                };

                if (input.Name == "select")
                    field.Options = input.SelectNodes(".//option")
                        ?.Select(o => o.GetAttributeValue("value", o.InnerText)).ToList() ?? [];

                resultForm.Fields.Add(field);
            }
        }
        return true;
    }
    public void ObfuscateFormHtml(HtmlDocument document)
    {
        var formNode = document.DocumentNode.SelectSingleNode("//form") ??
                       throw new InvalidOperationException("No <form> tag found.");
        var inputNodes = formNode.SelectNodes(".//input|.//select|.//textarea")?.ToArray() ?? [];
        foreach (var input in inputNodes)
        {
            var name = input.GetAttributeValue("name", "");
            if (string.IsNullOrEmpty(name)) continue;
            var obfuscated = csrfModifier.Obfuscate(name);
            input.SetAttributeValue("name", obfuscated);
        }
    }
    public void InjectValidationErrors(HtmlDocument document, Dictionary<string, string> errors)
    {
        foreach (var kv in errors)
            document.GetElementbyId($"error_{kv.Key}").InnerHtml = System.Net.WebUtility.HtmlEncode(kv.Value);
    }
}