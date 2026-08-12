using Define.Doodads.Expo.Timeline;
using System.Globalization;
using System.Text;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

public class NumbersLetters : BasicService
{
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        string resultKeyPrefix = "";
        if (constants.PrimaryConstant.IsntJustAnObject() &&
            constants.PrimaryConstant.ToString() is string prefixCandidate &&
            !string.IsNullOrWhiteSpace(prefixCandidate))
            resultKeyPrefix = prefixCandidate;

        string defaultNumbers = constants.NamedItems.TryGetValue("defaultnumbers", out var defaultNumberObject)
            && defaultNumberObject.IsntJustAnObject()
            && defaultNumberObject.ToString() is string defaultNumberCandidate
            && decimal.TryParse(defaultNumberCandidate, NumberStyles.Any, CultureInfo.InvariantCulture, out var defaultDecim)
            ? defaultDecim.ToString(CultureInfo.InvariantCulture) : "0";
        string defaultLetters = constants.NamedItems.TryGetValue("defaultletters", out var defaultLettersObject)
            && defaultLettersObject.IsntJustAnObject()
            && defaultLettersObject.ToString() is string defaultLettersCandidate
            ? defaultLettersCandidate : "";

        var characters = new Queue<char>(interaction.Register.ToString());
        StringBuilder numberPart = new StringBuilder();
        StringBuilder letterPart = new StringBuilder();

        while (characters.Any() && characters.Peek() is char character
            && char.IsWhiteSpace(character))
            characters.Dequeue();

        if (characters.Any() && characters.Peek() is char firstCharacter
            && (char.IsDigit(firstCharacter) || firstCharacter == '.' || firstCharacter == ',' || firstCharacter == '-'))
        {
            numberPart.Append(firstCharacter);
            characters.Dequeue();
        }

        while (characters.Any() && characters.Peek() is char character
            && (char.IsDigit(character) || character == '.' || character == ','))
        {
            numberPart.Append(character);
            characters.Dequeue();
        }

        while (characters.Any() && characters.Peek() is char character
            && char.IsWhiteSpace(character))
            characters.Dequeue();

        if (characters.Any() && characters.Peek() is char mustBeLetter
            && (!char.IsDigit(mustBeLetter)))
        {
            letterPart.Append(mustBeLetter);
            characters.Dequeue();

            while (characters.Any() && characters.Peek() is char character)
            {
                letterPart.Append(character);
                characters.Dequeue();
            }
        }

        OnThen?.Invoke(this, interaction.AppendMemory(
            new NumbersLettersMemory(resultKeyPrefix, numberPart, letterPart, defaultNumbers, defaultLetters)));
    }
}
