#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Call : IService
{
    private readonly UpdatingPrimaryValue ModuleNameConstant = new();
    private string? CurrentModuleName;    
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
        var program = ProgramRepository.Instance.GetForFile(desiredModuleName);
        if (program.EntryPoint == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "non-critical, but program hasn't been loaded and will continue when it is."));
            var previousOnReady = program.OnReady;
            program.OnReady = () =>
            {
                if (previousOnReady != null) previousOnReady();
                program.EntryPoint?.Run(this, ci);
            };
        } else
        {
            program.EntryPoint.Run(this, ci);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}