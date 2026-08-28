#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;

[Category("Input & Validation")]
[Title("Check for all memory places being set")]
[Description("""
    Provide multiple memory names in the primary constant, to check if each memory name has a value assigned.
    When not all names are assigned, will terminate & send OnElse with the name of the first missing memory name 
    in the register. In case all names are assigned and not false, sends OnThen.
    """)]
public class AllSet : BasicService
{
    [EventOccasion("When all names are set")]
    public override event CallForInteraction? OnThen;
    [EventOccasion("When not all names are set, has the first unset name in register")]
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
            if (!interaction.TryFindVariable(name, out object? val) ||
                val is not { } notNullVal)
            {
                OnElse?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else if (notNullVal is string strVal && string.IsNullOrWhiteSpace(strVal))
            {
                OnElse?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else if (notNullVal is bool blnVal && blnVal == false)
            {
                OnElse?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else if (notNullVal is IEnumerable ieVal && !ieVal.OfType<object>().Any())
            {
                OnElse?.Invoke(this, interaction.AppendRegister(name));
                return;
            }
            else
            {
                if (notNullVal.GetType().IsAssignableFrom(typeof(object)))
                {
                    OnElse?.Invoke(this, interaction.AppendRegister(name));
                    return;
                }
                if (notNullVal.ToString() is string ts && string.IsNullOrWhiteSpace(ts))
                {
                    OnElse?.Invoke(this, interaction.AppendRegister(name));
                    return;
                }
            }
        }
        OnThen?.Invoke(this, interaction);
    }
}
