#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;

[Category("Input & Validation")]
[Title("Check for any memory places being set")]
[Description("""
    Provide multiple memory names in the primary constant, to check if any memory name has a value assigned.
    When any name is populated, will terminate & send OnThen with the name of the first present memory name 
    in the register. In case no name has been assigned to, OnElse will trigger.
    """)]
public class AnySet : BasicService
{
    [EventOccasion("When any name was set, has the memory name in register")]
    public override event CallForInteraction? OnThen;
    [EventOccasion("When no names were set.")]
    public override event CallForInteraction? OnElse;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (constants.PrimaryConstant is not IEnumerable items)
            throw new BasicException("checking multiple variables using allset requires providing multiple as arg");
        var names = items.OfType<string>().ToArray();
        if (names.Length < 1)
            throw new BasicException("checking multiple variables using allset requires providing multiple as arg");
        foreach (var name in names)
        {
            if (!interaction.TryFindVariable(name, out object? val) || val is not { } notNullVal)
                continue;

            if (notNullVal is string strVal && !string.IsNullOrWhiteSpace(strVal))
            {
                OnThen?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else if (notNullVal is bool blnVal && blnVal == true)
            {
                OnThen?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else if (notNullVal is IEnumerable ieVal && ieVal.OfType<object>().Any())
            {
                OnThen?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else
            {
                if (notNullVal.GetType().IsAssignableFrom(typeof(object)))
                {
                    continue;
                }
                if (notNullVal.ToString() is string ts && !string.IsNullOrWhiteSpace(ts))
                {
                    OnThen?.Invoke(this, interaction.AppendRegister(name));
                    return;
                }
            }
        }
        OnElse?.Invoke(this, interaction);
    }
}