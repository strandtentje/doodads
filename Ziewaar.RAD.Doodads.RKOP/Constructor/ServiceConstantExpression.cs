using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;

public class ServiceConstantExpression : IParityParser
{
    public ConstantType ConstantType;
    public string TextValue;
    public FileInWorkingDirectory PathValue;
    public bool BoolValue;
    public decimal NumberValue;
    private ServiceConstantExpression[] ArrayItems = [];

    public object GetValue() => ConstantType switch
    {
        ConstantType.String => TextValue,
        ConstantType.Bool => BoolValue,
        ConstantType.Number => NumberValue,
        ConstantType.Path => PathValue,
        ConstantType.Array => ArrayItems.Select(x => x.GetValue()).ToArray(),
        _ => throw new InvalidOperationException(),
    };

    public ParityParsingState UpdateFrom(ref CursorText inText)
    {
        var text = inText.SkipWhitespace();

        text = text.TakeToken(TokenDescription.TrueOrFalse, out var bln);
        if (bln.IsValid)
        {
            var newValue = bool.Parse(bln.Text);
            var state = ConstantType != ConstantType.Bool || newValue != BoolValue ? ParityParsingState.Changed : ParityParsingState.Unchanged;
            SetBoolValue(newValue);
            inText = text;
            return state;
        }

        text = text.TakeToken(TokenDescription.ArrayOpen, out var aop);
        if (aop.IsValid)
        {
            ParityParsingState arrRes = ConsumeRemainingArrayIncludingCloser(ref text);
            inText = text;
            return arrRes;
        }

        text = text.TakeToken(TokenDescription.RelativePathAnnouncement, out var rpa);
        if (rpa.IsValid)
        {
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathValue(text.WorkingDirectory, x));
            inText = text;
            return pathRes;
        }

        text = text.TakeToken(TokenDescription.RelativeSearchPathAnnouncement, out var spa);
        if (spa.IsValid)
        {
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathFoundAbove(
                text, text.WorkingDirectory, x));
            inText = text;
            return pathRes;
        }

        text = text.TakeToken(TokenDescription.ConfigurationPathAnnouncement, out var cpa);
        if (cpa.IsValid)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appdataDirInfo = new DirectoryInfo(appdata);
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathValue(appdataDirInfo, x));
            inText = text;
            return pathRes;
        }
        text = text.TakeToken(TokenDescription.ProfilePathAnnouncement, out var ppa);
        if (ppa.IsValid)
        {
            var profilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var profileDirInfo = new DirectoryInfo(profilepath);
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathValue(profileDirInfo, x));
            inText = text;
            return pathRes;
        }

        text = text.TakeToken(TokenDescription.QueryPathAnnouncement, out var tpa);
        if (tpa.IsValid)
        {
            var queryDirectory = text.WorkingDirectory.GetDirectories().SingleOrDefault(x => x.Name == "queries")
                ?? text.WorkingDirectory.CreateSubdirectory("queries");
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathValue(queryDirectory, x));
            inText = text;
            return pathRes;
        }

        text = text.TakeToken(TokenDescription.TemplatePathAnnouncement, out var tppa);
        if (tppa.IsValid)
        {
            var templateDirectory = text.WorkingDirectory.GetDirectories().SingleOrDefault(x => x.Name == "templates")
                ?? text.WorkingDirectory.CreateSubdirectory("templates");
            var pathRes = ConsumeRemainingStringIncludingQuotes(ref text, x => SetPathValue(templateDirectory, x));
            inText = text;
            return pathRes;
        }

        text = text.TakeToken(TokenDescription.DoubleQuotes, out var dqt);
        if (dqt.IsValid)
        {
            var stringRes = ConsumeRemainingStringIncludingQuotes(ref text, SetStringValue);
            inText = text;
            return stringRes;
        }

        text = text.TakeToken(TokenDescription.MaybeNegativeNumbers, out var wholeNums);
        if (wholeNums.IsValid && int.TryParse(wholeNums.Text, out int res))
        {
            decimal candidate = res;
            text = text.TakeToken(TokenDescription.DecimalSeparator, out var dp);
            if (dp.IsValid)
            {
                text = text.TakeToken(TokenDescription.Numbers, out var decimals);
                if (decimals.IsValid &&
                    decimal.TryParse(
                        $"0.{decimals.Text}",
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture.NumberFormat,
                        out decimal decimalValue))
                {
                    inText = text;
                    return SetDecimalValue(candidate + decimalValue);
                }
                else
                {
                    throw new ParsingException(text, $"Saw decimal point, but no remaining decimals");
                }
            }
            else
            {
                inText = text;
                return SetDecimalValue(candidate);
            }
        }
        else if (wholeNums.IsValid)
        {
            throw new ParsingException(text, $"Unable to parse number.");
        }

        return ParityParsingState.Void;
    }

    private ParityParsingState ConsumeRemainingArrayIncludingCloser(ref CursorText text)
    {
        List<ServiceConstantExpression> newArrayExpressions = new();
        while (true)
        {
            text = text.SkipWhitespace().TakeToken(TokenDescription.ArrayClose, out var closer);
            if (closer.IsValid)
                break;
            var newItem = new ServiceConstantExpression();
            newArrayExpressions.Add(newItem);
            newItem.UpdateFrom(ref text);
            Token comma;
            do
            {
                text = text.SkipWhitespace().TakeToken(TokenDescription.ArgumentSeparator, out comma);
            } while (comma.IsValid);
        }
        ParityParsingState state = ParityParsingState.Unchanged;
        if (ConstantType != ConstantType.Array)
            state = ParityParsingState.Changed;
        else if (ArrayItems.Length != newArrayExpressions.Count)
            state = ParityParsingState.Changed;
        else if (ArrayItems.Zip(newArrayExpressions, (x, y) => (x, y)).Any(p => p.x.Mismatches(p.y)))
            state = ParityParsingState.Changed;
        ConstantType = ConstantType.Array;
        ArrayItems = newArrayExpressions.ToArray();
        return state;
    }

    private ParityParsingState SetPathValue(DirectoryInfo workingDirectory, string x)
    {
        var state = ParityParsingState.Unchanged;
        if (ConstantType != ConstantType.Path) state |= ParityParsingState.Changed;
        if (PathValue != (workingDirectory.FullName, x)) state |= ParityParsingState.Changed;
        ConstantType = ConstantType.Path;
        PathValue = (workingDirectory.FullName, x);

        return state;
    }

    private ParityParsingState SetPathFoundAbove(CursorText ct, DirectoryInfo workingDirectory, string subPath)
    {
        DirectoryInfo actualDirectory = workingDirectory;
        var state = ParityParsingState.Unchanged;
        if (ConstantType != ConstantType.Path) state |= ParityParsingState.Changed;

        var searchPath = subPath;

        if (searchPath.Contains('@'))
        {
            // if there's an @ in there, something like s"somefile.rkop @ someplace" is happening.
            searchPath = subPath.Split(['@'], 
                StringSplitOptions.RemoveEmptyEntries).
                Select(x => x.Trim()).ElementAtOrDefault(0) ?? "";
        }

        while (
            !Directory.Exists(Path.Combine(actualDirectory.FullName, searchPath)) &&
            !File.Exists(Path.Combine(actualDirectory.FullName, searchPath)))
        {
            if (actualDirectory.Parent != null)
                actualDirectory = actualDirectory.Parent;
            else
                throw new ParsingException(ct,
                    $"Could not find sub-path `{searchPath}` in any parent directory of `{workingDirectory}`");
        }

        if (PathValue != (actualDirectory.FullName, subPath)) state |= ParityParsingState.Changed;
        ConstantType = ConstantType.Path;
        PathValue = (actualDirectory.FullName, subPath);

        return state;
    }

    private ParityParsingState SetDecimalValue(decimal v)
    {
        var state = ParityParsingState.Unchanged;
        if (ConstantType != ConstantType.Number) state |= ParityParsingState.Changed;
        if (NumberValue != v) state |= ParityParsingState.Changed;

        ConstantType = ConstantType.Number;
        NumberValue = v;

        return state;
    }
    private ParityParsingState SetBoolValue(bool resultBool)
    {
        var state = ParityParsingState.Unchanged;
        if (ConstantType != ConstantType.Bool) state |= ParityParsingState.Changed;
        if (BoolValue != resultBool) state |= ParityParsingState.Changed;

        ConstantType = ConstantType.Bool;
        BoolValue = resultBool;

        return state;
    }

    private ParityParsingState ConsumeRemainingStringIncludingQuotes(
        ref CursorText inText, Func<string, ParityParsingState> process)
    {
        StringBuilder guts = new StringBuilder();
        var text = inText;
        while (true)
        {
            text = text.TakeToken(TokenDescription.StringGuts, out var stringSegment);
            guts.Append(stringSegment.Text);
            text = text.TakeToken(TokenDescription.EscapeSequence, out var slash);
            if (!slash.IsValid)
            {
                inText = text.ValidateToken(
                    TokenDescription.DoubleQuotes,
                    "this is an unlikely error. rob may need a coffee if you tell him about this.",
                    out var _);
                return process(guts.ToString());
            }
            else
            {
                guts.Append(slash.Text[1]);
            }
        }
    }

    private ParityParsingState SetStringValue(string v)
    {
        var state = ParityParsingState.Unchanged;
        if (ConstantType != ConstantType.String) state |= ParityParsingState.Changed;
        if (TextValue != v) state |= ParityParsingState.Changed;

        ConstantType = ConstantType.String;
        TextValue = v;

        return state;
    }

    internal bool Mismatches(ServiceConstantExpression value)
    {
        if (ConstantType != value.ConstantType) return true;

        switch (ConstantType)
        {
            case ConstantType.String:
                return TextValue != value.TextValue;
            case ConstantType.Bool:
                return BoolValue != value.BoolValue;
            case ConstantType.Number:
                return NumberValue != value.NumberValue;
            case ConstantType.Path:
                return PathValue != value.PathValue;
            case ConstantType.Array:
                if (ArrayItems.Length != value.ArrayItems.Length)
                    return true;
                return ArrayItems.Zip(value.ArrayItems, (x, y) => (x, y)).Any(p => p.x.Mismatches(p.y));
            default:
                return true;
        }
    }
    public void WriteTo(StreamWriter writer)
    {
        switch (ConstantType)
        {
            case ConstantType.String:
                writer.Write('"');
                writer.Write(TextValue.Replace(@"\", @"\\").Replace(@"""", @"\"""));
                writer.Write('"');
                break;
            case ConstantType.Bool:
                writer.Write(BoolValue ? "True" : "False");
                break;
            case ConstantType.Number:
                if (Math.Floor(NumberValue) == NumberValue)
                    writer.Write(((int)NumberValue).ToString());
                else
                    writer.Write(NumberValue.ToString(CultureInfo.InvariantCulture));
                break;
            case ConstantType.Path:
                writer.Write(@"f""");
                writer.Write(PathValue.RelativePath.Replace(@"\", @"\\").Replace(@"""", @"\"""));
                writer.Write(@"""");
                break;
            case ConstantType.Array:
                writer.Write("[");
                for (int i = 0; i < ArrayItems.Length; i++)
                {
                    ArrayItems[i].WriteTo(writer);
                    if (i + 1 < ArrayItems.Length)
                        writer.Write(", ");
                }
                writer.Write("]");
                break;
            default:
                break;
        }
    }
}
