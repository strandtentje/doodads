#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;
public class PrefixedShorthandConstructor : ISerializableConstructor
{
    public enum ShorthandType
    {
        NoShorthand,
        Store,
        Load,
        Format,
        InvalidShorthand
    };
    public ShorthandType CurrentShorthandType { get; private set; } = ShorthandType.InvalidShorthand;
    public string? ServiceTypeName
    {
        get =>
            CurrentShorthandType != ShorthandType.InvalidShorthand
                ? Enum.GetName(typeof(ShorthandType), CurrentShorthandType)
                : throw new Exception("Unknown Shorthand");
        set
        {
            CurrentShorthandType = (ShorthandType)Enum.Parse(typeof(ShorthandType), value ?? "InvalidShorthand");
            if (CurrentShorthandType is ShorthandType.InvalidShorthand or ShorthandType.NoShorthand)
                throw new Exception("May only set this to Definition, Call or Case");
        }
    }
    public object PrimarySettingValue => PrimaryExpression.GetValue();
    public IReadOnlyDictionary<string, object> ConstantsList => EmptyReadOnlyDictionary.Instance;
    private ServiceConstantExpression PrimaryExpression = new();
    public bool UpdateFrom(ref CursorText text)
    {
        var temporaryCursorPosition = text
            .SkipWhile(char.IsWhiteSpace)
            .TakeToken(TokenDescription.StoreShorthand, out var exclamation)
            .TakeToken(TokenDescription.LoadShorthand, out var question)
            .TakeToken(TokenDescription.FormatShorthand, out var dollar);

        this.CurrentShorthandType = (exclamation.IsValid, question.IsValid, dollar.IsValid) switch
        {
            (false, false, false) => ShorthandType.NoShorthand,
            (true, false, false) => ShorthandType.Store,
            (false, true, false) => ShorthandType.Load,
            (false, false, true) => ShorthandType.Format,
            _ => ShorthandType.InvalidShorthand,
        };

        if (this.CurrentShorthandType == ShorthandType.NoShorthand) return false;
        if (this.CurrentShorthandType == ShorthandType.InvalidShorthand)
            throw new SyntaxException(text, "Strange shorthand configuration; likely to do with !'s, ?'s and $'s.");

        text = temporaryCursorPosition;

        PrimaryExpression.UpdateFrom(ref text);

        return true;
    }
    public void WriteTo(StreamWriter writer, int indentation)
    {
        writer.Write(CurrentShorthandType switch
        {
            ShorthandType.Format => '$',
            ShorthandType.Store => '!',
            ShorthandType.Load => '?',
            _ => throw new Exception("invalid shorthand"),
        });
        PrimaryExpression.WriteTo(writer);
    }
}