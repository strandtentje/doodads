#nullable enable
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.Services;

[Category("Databases & Querying")]
[Title("Define placeholder text for query")]
[Description("""
             For queries that contain a PLACEHOLDER_name, define the text here using the constants.
             """)]
[ShortNames("qplh")]
public class QueryPlaceholder : IService
{
    [EventOccasion("Placeholder definition comes out here")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction) =>
        OnThen?.Invoke(this,
            new QueryPlaceholderInteraction(interaction, constants.PrimaryConstant, constants.NamedItems));

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}