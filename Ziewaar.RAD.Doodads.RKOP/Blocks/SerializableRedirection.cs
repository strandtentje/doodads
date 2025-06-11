#nullable enable

using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
public class SerializableRedirection<TResultSink>() : ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    private string? RefersToName;
    private ServiceExpression<TResultSink>? RefersToService;
    protected override bool ProtectedUpdateFrom(ref CursorText text)
    {
        var newPosition = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Underscore, out var token);

        if (!token.IsValid)
            return false;

        text = newPosition.ValidateToken(
            TokenDescription.IdentifierWithoutUnderscore,
            "keep in mind references may only start with a single underscore, and only a letter may come after the underscore.",
            out var referenceNameWithoutUnderscore);
        var candidateName = $"_{referenceNameWithoutUnderscore.Text}";
        var candidateReference = text[candidateName] as ServiceExpression<TResultSink>;
        
        ResultSink ??= new();
        RefersToService = candidateReference;
        RefersToName = candidateName;

        return true;
    }
    public override void HandleChanges()
    {
        ResultSink?.SetSoftLink(RefersToService ?? throw new ArgumentException("no refer service"));
    }
    public override void Purge()
    {
        RefersToName = null;
        // we dont purge this service cos its a reference.
        RefersToService = null;
    }
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        writer.Write(RefersToService?.CurrentNameInScope);
    }

    public override IEnumerable<TResult> Query<TResult>(Func<TResult, bool>? predicate = null)
    {
        predicate ??= x => true;
        IEnumerable<TResult> result = [];
        if (this is TResult maybe && predicate(maybe))
            result = [maybe];
        if (RefersToService != null)
            result = result.Concat(RefersToService.Query(predicate));
        return result;
    }
}