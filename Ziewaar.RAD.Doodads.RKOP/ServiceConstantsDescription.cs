namespace Ziewaar.RAD.Doodads.RKOP;
public class ServiceConstantsDescription : IParityParser
{
    public List<ServiceConstantsMember> Members = new();
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        int oCounter = Members.Count;

        int memCounter = 0;
        ParityParsingState lastState = ParityParsingState.Void, finalState = ParityParsingState.Void;
        Token comma;
        do
        {
            if (Members.Count == memCounter)
                Members.Add(new ServiceConstantsMember());
            lastState = Members[memCounter].UpdateFrom(ref text);
            if (lastState > ParityParsingState.Void)
                memCounter++;
            finalState |= lastState;
            text = text.TakeToken(TokenDescription.ArgumentSeparator, out comma);
        } while (comma.IsValid);

        while (memCounter < Members.Count)
        {
            Members.RemoveAt(memCounter);
        }

        if (oCounter != Members.Count)
            finalState |= ParityParsingState.Changed;
        
        return finalState;
    }
    public void WriteTo(StreamWriter writer)
    {
        for (int i = 0; i < Members.Count; i++)
        {
            Members[i].WriteTo(writer);
            if (i < Members.Count - 1)
                writer.Write(", ");
        }
    }
    public SortedList<string, object> ToSortedList() => new SortedList<string, object>(
        Members.ToDictionary(x => x.Key, x => x.Value.GetValue()));

    public void Set(string reqName, ConstantType type, string reqValue, string workingDir)
    {
        var toChange = Members.SingleOrDefault(x => x.Key == reqName);
        if (toChange == null)
            Members.Add(toChange = new ServiceConstantsMember());

        toChange.Value.ConstantType = type;
        switch (type)
        {
            case ConstantType.String:
                toChange.Value.TextValue = reqValue;
                break;
            case ConstantType.Bool:
                toChange.Value.BoolValue = Convert.ToBoolean(reqValue);
                break;
            case ConstantType.Number:
                toChange.Value.NumberValue = Convert.ToDecimal(reqValue);
                break;
            case ConstantType.Path:
                toChange.Value.PathValue = (workingDir, reqValue);
                break;
            default:
                break;
        }

    }
}
