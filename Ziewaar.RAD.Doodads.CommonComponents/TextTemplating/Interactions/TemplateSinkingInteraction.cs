namespace Ziewaar.RAD.Doodads.CommonComponents;

public class TemplateSinkingInteraction<TDataType>(IInteraction parent, ITaggedData<TDataType> data) :
    ISinkingInteraction<TDataType>
{
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables => parent.Variables;
    public ITaggedData<TDataType> TaggedData { get; } = data;
    public string Delimiter => "";
}