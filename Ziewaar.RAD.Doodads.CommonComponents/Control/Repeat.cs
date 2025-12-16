#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Repeat instructions while Continue is being encountered")]
[Description("""
             It's recommended not to use this, and always prefer recursing using the Call-method.
             Repeat won't loop unless it encounters a "Continue". Repeat will play nice with
             Calls to other Definitions, but it is hard to understand if it works. Only use locally.
             """)]
public class Repeat : IteratingService
{
    protected override bool RunElse => false;
    [PrimarySetting("Description of this repeat block, must be the same for Repeat and related Continue")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    [NamedSetting("deep", "When Continue is encountered, bring its Buffer, Memory and Stack into the repetition.")]
    private readonly UpdatingKeyValue IsDeepConstant = new("deep");
    [EventOccasion("Pre-test to use with continue to detect if repeating is neccesary")]
    public override event CallForInteraction? OnElse;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {        
        (constants, IsDeepConstant).IsRereadRequired(out bool? mustGoDeep);

        var ri = (RepeatInteraction)repeater;
        ri.IsDeep = mustGoDeep == true;
                
        OnElse?.Invoke(this, ri);

        while (ri.IsRunning && !ri.CancellationToken.IsCancellationRequested)
            yield return ri.ContinueFrom ?? ri;
    }
}
