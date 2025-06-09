#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public class SerializableRedirection<TResultSink>() : ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    private string? RefersToName;
    private ServiceExpression<TResultSink>? RefersToService;
    protected override ParityParsingState ProtectedUpdateFrom(ref CursorText text)
    {
        var newPosition = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Underscore, out var token);

        if (!token.IsValid)
            return ParityParsingState.Void;

        text = newPosition.ValidateToken(TokenDescription.IdentifierWithoutUnderscore,
            out var referenceNameWithoutUnderscore);
        var candidateName = $"_{referenceNameWithoutUnderscore.Text}";
        var candidateReference = text[candidateName] as ServiceExpression<TResultSink>;

        if (this.RefersToService == null || this.RefersToName == null || this.ResultSink == null)
        {
            this.ResultSink = new();
            this.RefersToService = candidateReference;
            this.RefersToName = candidateName;
            return ParityParsingState.New;
        }
        else if (this.RefersToName != candidateName)
        {
            this.RefersToService = candidateReference;
            this.RefersToName = candidateName;
            return ParityParsingState.Changed;
        }
        else if (this.RefersToService == candidateReference)
        {
            this.RefersToService = candidateReference;
            this.RefersToName = candidateName;
            return ParityParsingState.Unchanged;
        }
        else
        {
            this.RefersToService = candidateReference;
            this.RefersToName = candidateName;
            return ParityParsingState.Changed;
        }
    }
    public override void HandleChanges()
    {
        this.ResultSink?.SetSoftLink(RefersToService ?? throw new ArgumentException("no refer service"));
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
}