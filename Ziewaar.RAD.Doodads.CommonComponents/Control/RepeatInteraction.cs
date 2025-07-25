﻿#nullable enable

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class RepeatInteraction(string repeatName, IInteraction parent) : IInteraction
{
    public string RepeatName => repeatName;
    public IInteraction Stack => parent;
    public bool IsDeep = false;
    public object Register => Stack.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public bool IsRunning = true;
    internal IInteraction? ContinueFrom;
}
