
namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public interface IValidatingInputField
{
    /// <summary>
    /// expects the set to set this to 0 or 1 depending on is required, or increment after merge
    /// </summary>
    int MinExpectedValues { get; set; }
    /// <summary>
    /// expects the set to set this to 1 and increment after merge
    /// </summary>
    int MaxExpectedValues { get; set; }
    string Name { get; set; }
    List<IValidatingInputField> AltValidators { get; }
    bool IsRequired { get; }
    bool IsMaxUnbound { get; }

    event EventHandler<(string oldName, string newName)>? NameChanged;

    bool TryValidate(string[] submittedValue, out IEnumerable result);
    /// <summary>
    /// Use merge for name collisions
    /// On true, expects the set to merge the expected value counts
    /// On false, expects the set to append the arg to the alt validators
    /// </summary>
    /// <param name="otherFieldInSet"></param>
    /// <returns></returns>
    bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet);
}