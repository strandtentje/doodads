#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;

public class AnySet : BasicService
{
    public override event CallForInteraction? OnThen;
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