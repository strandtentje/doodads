#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Call : IService
{
    private readonly UpdatingPrimaryValue ModuleNameConstant = new();
    private string? CurrentModuleName;
    public event CallForInteraction? ModuleName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ModuleNameConstant).IsRereadRequired(out object? candidateModuleName);
        this.CurrentModuleName = candidateModuleName?.ToString();
        var desiredModuleName = this.CurrentModuleName;
        if (desiredModuleName == "*")
            desiredModuleName = interaction.Register as string;
        if (desiredModuleName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "configure a file for a Call"));
            return;
        }
        if (string.IsNullOrWhiteSpace(desiredModuleName))
            OnException?.Invoke(this, new CommonInteraction(interaction, "no module name provided"));
        var ci = new CallingInteraction(interaction, constants.NamedItems);
        ci.OnThen += OnThen;
        ci.OnElse += OnElse;
        ProgramRepository.Instance.GetEntryPointForFile(desiredModuleName).Run(this, ci);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}