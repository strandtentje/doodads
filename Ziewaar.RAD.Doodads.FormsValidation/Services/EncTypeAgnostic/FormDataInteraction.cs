using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic
{
    public class FormDataInteraction(
        IInteraction interaction,
        FormStructureInteraction structure,
        IEnumerable<IGrouping<string, object>> data) : IInteraction
    {
        public IInteraction Stack { get; }
        public object Register { get; }
        public IReadOnlyDictionary<string, object> Memory { get; }
        public IEnumerable<IGrouping<string, object>> Data => data;
    }
}