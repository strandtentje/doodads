#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;

[Category("Reflection & Documentation")]
[Title("Get all the definitions that exist in the file.")]
[Description("""
             Provided with a full file path in Register, will enumerate the definitions that exist in it.
             Enumeration goes into memory at `names`, path to file will be kept in register, and memory, at `path`.
             """)]
public class DefinitionsInFile : IteratingService
{
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants,
        IInteraction repeater)
    {
        var definitions = ProgramRepository.Instance
            .GetForFile(repeater.Register.ToString()).Definitions ?? [];
        foreach (ProgramDefinition programDefinition in definitions)
            yield return repeater.AppendRegister(programDefinition.Name)
                .AppendMemory([
                    (ReflectionKeys.ServiceExpression,
                        programDefinition.CurrentSeries)
                ]);
    }
}