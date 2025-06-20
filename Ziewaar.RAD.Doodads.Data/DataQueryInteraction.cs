#nullable enable
#pragma warning disable 67

using System.Collections.Generic;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Data;

public class DataQueryInteraction(IInteraction cause, IDataReader reader) : IInteraction
{
    public IInteraction Stack => cause;
    public object Register => cause.Register;
    public IReadOnlyDictionary<string, object> Memory => new DictionaryAroundReader(reader);
}
