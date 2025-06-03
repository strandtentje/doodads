using System.Collections.Generic;


namespace Ziewaar.RAD.Doodads.RKOP;

public class ServiceConstantsDescription : IParityParser
{
    public string Key;
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
            lastState = Members[memCounter++].UpdateFrom(ref text);
            finalState |= lastState;
            text = text.TakeToken(TokenDescription.ArgumentSeparator, out comma);            
        } while (comma.IsValid);

        while (memCounter < Members.Count)
        {
            Members.RemoveAt(memCounter - 1);            
        }

        if (oCounter != Members.Count)
            finalState |= ParityParsingState.Changed;

        text[$"const_{Key}"] = this;

        return finalState;
    }
}
