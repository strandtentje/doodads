#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Call : IService
{
    private readonly UpdatingPrimaryValue ModuleNameConstant = new();
    private string? CurrentModuleName;
    public event EventHandler<IInteraction>? ModuleName;
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ModuleNameConstant).IsRereadRequired(out this.CurrentModuleName);
        var desiredModuleName = this.CurrentModuleName;
        if (desiredModuleName == "*")
            desiredModuleName = interaction.Register as string;
        if (desiredModuleName == null)
        {
            var textSink = new TextSinkingInteraction(interaction);
            ModuleName?.Invoke(this, textSink);
            using var textReader = textSink.GetDisposingSinkReader();
            desiredModuleName = textReader.ReadToEnd();
        }
        if (string.IsNullOrWhiteSpace(desiredModuleName))
            OnException?.Invoke(this, new CommonInteraction(interaction, "no module name provided"));
        var ci = new CallingInteraction(interaction, constants.NamedItems);
        ci.OnThen += OnThen;
        ci.OnElse += OnElse;
        ProgramRepository.Instance.GetEntryPointForFile(desiredModuleName).Run(ci);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}