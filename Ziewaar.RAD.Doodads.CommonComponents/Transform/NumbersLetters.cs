using Define.Doodads.Expo.Timeline;
using System.Globalization;
using System.Text;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

[Category("Printing & Formatting")]
[Title("Split register into numeric and textual component")]
[Description("""
             Takes the text in register and consumes the (decimal) numeric part it starts with, 
             sticks it into memory, then takes the remaining non-numeric suffix and sticks it into 
             memory too. Will always produce output based on defaults. Useful for ie. parsing apart
             expressions like 10cm or 50W
             """)]
public class NumbersLetters : BasicService
{
    [PrimarySetting("Memory name prefix; resulting numbers will be in [prefix]numbers, letters in [prefix]letters.")]
    private readonly UpdatingPrimaryValue MemoryPrefix = new UpdatingPrimaryValue();
    [NamedSetting("defaultnumbers", "If no numbers could be derived from register, use this default value")]
    private readonly UpdatingKeyValue DefaultNumberPart = new UpdatingKeyValue("defaultnumbers");
    [NamedSetting("defaultletters", "If no letters could be derived from the tail end of the register, use these default letters.")]
    private readonly UpdatingKeyValue DefaultLetterPart = new UpdatingKeyValue("defaultletters");
    [EventOccasion("""
        After splitting has happened, will have numbers and letters in memory. Memory names will be prefixed according to
        primary constant.
        """)]
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
