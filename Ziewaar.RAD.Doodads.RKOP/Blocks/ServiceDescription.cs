using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Constructor.Shorthands;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
#nullable enable
public class ServiceDescription<TResultSink> : ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public ISerializableConstructor[] AvailableConstructors =
    [
        new ContextValueManipulationConstructor(),
        new PrefixedShorthandConstructor(),
        new CapturedShorthandConstructor(),
        new RegularNamedConstructor(),
    ];
    public ISerializableConstructor? CurrentConstructor;
    public SerializableBranchBlock<TResultSink> Children = new();
    public CursorText TextScope = CursorText.Empty;
    protected override bool ProtectedUpdateFrom(ref CursorText text)
    {
        TextScope = text;

        foreach (var constructorCandidate in AvailableConstructors)
        {
            if (constructorCandidate.UpdateFrom(ref text))
            {
                CurrentConstructor = constructorCandidate;
                ResultSink ??= new();
                Children.UpdateFrom(ref text);
                return true;
            }
        }

        ResultSink = null;
        CurrentConstructor = null;
        return false;
    }
    public override void HandleChanges()
    {        
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));
        if (CurrentConstructor?.ServiceTypeName == null)
            throw new ArgumentException("no service type or bad constructor", nameof(CurrentConstructor.ServiceTypeName));
        ResultSink.SetDefinition(
            TextScope,
            CurrentConstructor.ServiceTypeName,
            CurrentConstructor.PrimarySettingValue,
            CurrentConstructor.ConstantsList,
            Children.Convert());
    }
    public override void Purge()
    {
        Children.Purge();
        ResultSink?.Cleanup();
    }
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        if (CurrentConstructor == null)
            throw new Exception("Bad constructor");
        CurrentConstructor.WriteTo(writer, indentation);
        Children.WriteTo(writer, indentation);
    }
    public override IEnumerable<TResult> Query<TResult>(Func<TResult, bool>? predicate = null)
    {
        predicate ??= x => true;
        IEnumerable<TResult> results = [];
        if (this is TResult maybe && predicate(maybe))
            results = [maybe];
        if (Children.Branches != null)
            results = results.Concat(Children.Branches.SelectMany(x => x.value.Query(predicate)));
        return results;
    }
}