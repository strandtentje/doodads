using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Composite;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
public class FormStructureMemberBuilder
{
    private IValidatingCollectionFactory?
        FieldTypeValidator,
        OptionsValidator,
        LengthBoundsValidator,
        ValueBoundsValidator,
        TextPatternValidator,
        ValueCountValidator;
    private InputClass[] InputClasses = [];
    private string[]? IntersectedAccepts;
    private string? Name;
    private bool OnlyOptions;
    public FormStructureMemberBuilder SetTypes(InputClass[] inputTypes)
    {
        if (inputTypes.Any(x => x.Type == "file") && inputTypes.Any(x => x.Type != "file"))
            throw new ArgumentException("Form declares a field that's both a file and a regular input");
        this.InputClasses = inputTypes;
        IValidatingCollectionFactory[] fieldTypeValidators =
            [..inputTypes.Distinct().Select(field => new TypeValidatingCollectionFactory(field.Tag, field.Type))];
        this.FieldTypeValidator = new AllValidCollectionsFactory(fieldTypeValidators);
        return this;
    }
    public FormStructureMemberBuilder SetOptions(string[] validOptions)
    {
        this.OptionsValidator = new OptionsValidatingCollectionFactory(validOptions);
        return this;
    }
    public FormStructureMemberBuilder CanOnlyFitOptions(bool isOptionType)
    {
        this.OnlyOptions = isOptionType;
        return this;
    }
    public FormStructureMemberBuilder AddAccepts(string[] accepts)
    {
        foreach (var accept in accepts)
        {
            IntersectedAccepts =
                IntersectedAccepts == null
                    ? accept.Split(",", (StringSplitOptions)3)
                    : IntersectedAccepts.Intersect(accept.Split(",", (StringSplitOptions)3)).ToArray();
        }
        return this;
    }
    public FormStructureMemberBuilder SetLengthBounds(uint minLength, uint maxLength)
    {
        this.LengthBoundsValidator = new LengthValidatorCollectionFactory(minLength, maxLength);
        return this;
    }
    public FormStructureMemberBuilder SetValueBounds(string[] minValues, string[] maxValues)
    {
        this.ValueBoundsValidator = new AllValidCollectionsFactory([
            ..
            InputClasses.Select(x => new BoundsValidatingCollectionFactory(x.Type, minValues, maxValues))
        ]);
        return this;
    }
    public FormStructureMemberBuilder SetPatternConstraints(string[] patternConstraints)
    {
        this.TextPatternValidator = new AllValidCollectionsFactory([
            ..
            patternConstraints.Select(x => new PatternValidationCollectionFactory(x))
        ]);
        return this;
    }
    public FormStructureMemberBuilder SetValueCountLimits(int lowerValueCountLimit, int upperValueCountLimit)
    {
        this.ValueCountValidator = new CountingValidatingCollectionFactory(lowerValueCountLimit, upperValueCountLimit);
        return this;
    }
    public FormStructureMemberBuilder SetName(string inputGroupKey)
    {
        this.Name = inputGroupKey;
        return this;
    }
    public FormStructureMember Build(bool isMultipart, bool isFile)
    {
        if (string.IsNullOrWhiteSpace(this.Name))
            throw new ArgumentException("Form member must have a name");
        if (isMultipart && isFile)
        {
            var validator = new AllValidCollectionsFactory(
                new MultipartFileValidationCollectionFactory(this.Name, string.Join(',', IntersectedAccepts ?? [])),
                this.LengthBoundsValidator,
                this.FieldTypeValidator,
                this.ValueCountValidator
            );
            return new FormStructureMember(this.Name, validator);
        }
        else if (isMultipart && !isFile)
        {
            var validator = new AllValidCollectionsFactory(
                new MultipartParameterValidationCollectionFactory(this.Name),
                this.LengthBoundsValidator,
                this.FieldTypeValidator,
                this.ValueCountValidator,
                new AnyValidCollectionFactory(
                    this.OptionsValidator,
                    this.ValueBoundsValidator,
                    this.TextPatternValidator
                )
            );
            return new FormStructureMember(this.Name, validator);
        }
        else if (!isMultipart && !isFile)
        {
            var validator = new AllValidCollectionsFactory(
                this.LengthBoundsValidator,
                this.FieldTypeValidator,
                this.ValueCountValidator,
                new AnyValidCollectionFactory(
                    (OnlyOptions, this.OptionsValidator?.CanConstrain == true) switch
                    {
                        (false, false) => null, // free entry fields & no options to validate against
                        (true, false) => null, // restricted entry fields, but no options to validate against
                        (false, true) => null, // free entry fields, but also some options to validate against
                        (true, true) => this.OptionsValidator
                    },
                    this.ValueBoundsValidator,
                    this.TextPatternValidator
                )
            );
            return new FormStructureMember(this.Name, validator);
        }
        else
        {
            throw new InvalidOperationException("Cannot use file field in non-multipart");
        }
    }
}