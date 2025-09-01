namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic
{
    public class FormValidationInteraction(IInteraction stack, IReadOnlyDictionary<string, object> data, bool isValid)
        : IInteraction
    {
        public bool IsValid => isValid;
        public IInteraction Stack => stack;
        public object Register => stack.Register;
        public IReadOnlyDictionary<string, object> Memory => data;
        public static FormValidationInteractionBuilder Build() => new();

        public class FormValidationInteractionBuilder
        {
            private IInteraction Stack = StopperInteraction.Instance;
            private readonly SortedList<string, object> DataOutputs = new(), InfoOutputs = new();
            public FormValidationInteractionBuilder SetStack(IInteraction stack)
            {
                this.Stack = stack;
                return this;
            }
            public FormValidationInteractionBuilder SetHeadFailure(string headFailure)
            {
                this.InfoOutputs["Validation State"] = "failed";
                return this;
            }
            private static bool IsFieldnameConflicting(string fieldKey) =>
                fieldKey == "Validation" || fieldKey.EndsWith(" State", StringComparison.Ordinal);
            public FormValidationInteractionBuilder SetFieldFailure(string fieldKey)
            {
                if (IsFieldnameConflicting(fieldKey))
                {
                    this.InfoOutputs["Validation State"] = "conflict";
                }
                else
                {
                    this.InfoOutputs[$"{fieldKey} State"] = "failed";
                }

                return this;
            }
            public FormValidationInteractionBuilder NoValuesFor(string fieldKey)
            {
                if (IsFieldnameConflicting(fieldKey))
                {
                    this.InfoOutputs["Validation State"] = "conflict";
                }
                else
                {
                    this.InfoOutputs[$"{fieldKey} State"] = "empty";
                }

                return this;
            }
            public FormValidationInteractionBuilder SingleValueFor(string fieldKey, object single)
            {
                if (IsFieldnameConflicting(fieldKey))
                {
                    this.InfoOutputs["Validation State"] = "conflict";
                }
                else
                {
                    this.DataOutputs[fieldKey] = single;
                    this.InfoOutputs[$"{fieldKey} State"] = "single";
                }

                return this;
            }
            public FormValidationInteractionBuilder MultipleValuesFor(string fieldKey, object[] validatedValues)
            {
                if (IsFieldnameConflicting(fieldKey))
                {
                    this.InfoOutputs["Validation State"] = "conflict";
                }
                else
                {
                    this.DataOutputs[fieldKey] = validatedValues;
                    this.InfoOutputs[$"{fieldKey} State"] = "multiple";
                }

                return this;
            }
            public FormValidationInteraction Build()
            {
                if (this.InfoOutputs.ContainsKey("Validation State"))
                {
                    return new FormValidationInteraction(this.Stack, InfoOutputs, false);
                }
                else
                {
                    InfoOutputs["Validation State"] = "success";
                    return new FormValidationInteraction(this.Stack, new FallbackReadOnlyDictionary(this.InfoOutputs, this.DataOutputs), true);
                }
            }
        }
    }
}