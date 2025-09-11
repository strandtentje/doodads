namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;
public static class ValidatingInputFieldInSetExtensions
{
    public static bool IsExactValueCountCorrect(this IValidatingInputField vifis, IEnumerable result)
    {
        var enumerable = result as object[] ?? result.Cast<object>().ToArray();
        if (vifis is ValidatingTextInput vti && vti.TextInputType == "password" && enumerable.OfType<string>().Count() == 1)
            return true;

        var ct = enumerable.Length;
        return ct >= vifis.MinExpectedValues && ct <= vifis.MaxExpectedValues;
    }
}