using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FormHandling.Interfaces;
using Ziewaar.RAD.Doodads.FormsValidation.HTML;

namespace FormHandling.Validation
{
    public class FormValidator(IFormFieldObfuscator obfuscator)
    {
        public (string originalName, string submittedName)[] GetSubmitErrors(ParsedForm form, IReadOnlyDictionary<string, object> postedValues)
        {
            var errorFields = new HashSet<(string originalName, string submittedName)>();

            void Register(ParsedFormField field)
            {
                errorFields.Add((field.OriginalName, field.OriginalName));
            }
            
            foreach (var field in form.Fields)
            {
                postedValues.TryGetValue(field.NameInRequest, out var valueObj);

                var stringValue = ExtractFirstValue(valueObj);
                var allValues = ExtractAllValues(valueObj);

                if (field.Required && string.IsNullOrWhiteSpace(stringValue))
                    Register(field);
                if (stringValue?.Length < field.MinLength)
                    Register(field);
                if (stringValue?.Length > field.MaxLength)
                    Register(field);
                if (!string.IsNullOrEmpty(field.Pattern) && !Regex.IsMatch(stringValue ?? "", $"^{field.Pattern}$"))
                    Register(field);
                if (allValues.Any(v => !field.Options.Contains(v)))
                    Register(field);
            }

            return errorFields.ToArray();
        }

        private string ExtractFirstValue(object? value) => value switch
        {
            null => "",
            string s => s,
            IEnumerable<string> multi => multi.FirstOrDefault() ?? "",
            _ => value.ToString() ?? ""
        };

        private List<string> ExtractAllValues(object? value) => value switch
        {
            string s => new List<string> { s },
            IEnumerable<string> multi => multi.ToList(),
            _ => new List<string> { value?.ToString() ?? "" }
        };
    }
}