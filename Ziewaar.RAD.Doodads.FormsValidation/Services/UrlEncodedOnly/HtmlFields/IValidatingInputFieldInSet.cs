namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;
public interface IValidatingInputFieldInSet : IValidatingInputField
{
    static abstract bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set);
}