using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public interface IValidatingInputFieldInSet : IValidatingInputField
{
    static abstract bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set);
}