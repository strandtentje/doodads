using System.Runtime.InteropServices.JavaScript;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class TypeValidatingCollectionFactory : IValidatingCollectionFactory
{
    private readonly IValidatingCollectionFactory TrueFactory;
    public TypeValidatingCollectionFactory(string fieldTag, string fieldType)
    {
        if (fieldType == "file")
        {
            TrueFactory = new ValidatingFileCollectionFactory();
        }
        else if (fieldTag == "input")
        {
            TrueFactory = fieldType switch
            {
                "color" => new ValidatingColorCollectionFactory(),
                "date" => new ValidatingDateOnlyCollectionFactory(),
                "datetime-local" => new ValidatingDateTimeCollectionFactory(),
                "email" => new ValidatingEmailCollectionFactory(),
                "month" => new ValidatingMonthCollectionFactory(),
                "number" or "range" => new ValidatingNumberCollectionFactory(),
                "time" => new ValidatingTimeCollectionFactory(),
                "week" => new ValidatingWeekCollectionFactory(),
                _ => new NonValidatingCollectionFactory(),
            };
        }
        else
        {
            TrueFactory = new NonValidatingCollectionFactory();
        }
    }
    public IValidatingCollection Create() => TrueFactory.Create();
}