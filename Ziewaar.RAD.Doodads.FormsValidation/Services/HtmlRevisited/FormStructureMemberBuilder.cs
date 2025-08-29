namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class FormStructureMemberBuilder
{
    private IValidatingCollectionFactory
        FieldTypeValidator = NonValidatingCollectionFactory.Instance,
        OptionsValidator = NonValidatingCollectionFactory.Instance,
        LengthBoundsValidator = NonValidatingCollectionFactory.Instance,
        ValueBoundsValidator = NonValidatingCollectionFactory.Instance,
        TextPatternValidator = NonValidatingCollectionFactory.Instance,
        ValueCountValidator = NonValidatingCollectionFactory.Instance;
    private InputClass[] InputClasses = [];
    private bool AllFixed;
    private string Name;
    public FormStructureMemberBuilder SetTypes(InputClass[] inputTypes)
    {
        if (inputTypes.Any(x => x.Type == "file") && inputTypes.Any(x => x.Type != "file"))
            throw new ArgumentException("Form declares a field that's both a file and a regular input");
        this.InputClasses = inputTypes;
        IValidatingCollectionFactory[] fieldTypeValidators =
            [..inputTypes.Select(field => new TypeValidatingCollectionFactory(field.Tag, field.Type))];
        this.FieldTypeValidator = new AllValidCollectionsFactory(fieldTypeValidators);
        return this;
    }
    public FormStructureMemberBuilder SetOptions(string[] validOptions)
    {
        this.OptionsValidator = new OptionsValidatingCollectionFactory(validOptions);
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
    public FormStructureMember Build()
    {
        var validator = new AllValidCollectionsFactory(
            this.FieldTypeValidator,
            this.ValueCountValidator,
            new AnyValidCollectionFactory(
                this.OptionsValidator,
                new AllValidCollectionsFactory(
                    this.LengthBoundsValidator,
                    this.ValueBoundsValidator,
                    this.TextPatternValidator)));

        return new FormStructureMember(this.Name, validator);
    }
}