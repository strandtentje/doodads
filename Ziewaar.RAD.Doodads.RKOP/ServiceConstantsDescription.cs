using System;
using System.Collections.Generic;
using System.IO;


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

        text[$"const_{Key}"] = this;

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
}
