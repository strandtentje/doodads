using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class PlaylistMemberInteraction(RepeatInteraction ri, PlaylistMember member) : IInteraction
{
    public IInteraction Stack => ri;
    public object Register => ri.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = EmptyReadOnlyDictionary.Instance;
}