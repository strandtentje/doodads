namespace Ziewaar.RAD.Doodads.CommonComponents.Module
{
    public class Call : IService
    {
        [NamedBranch]
        public event EventHandler<IInteraction> OnError;
        [NamedBranch]
        public event EventHandler<IInteraction> Continue;
        [NamedBranch]
        public event EventHandler<IInteraction> ModuleName;
        public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
        {
            string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
                (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);

            var moduleName = SourceSetting(ModuleName, "modulename", "no_module");

            if (Definition.NamedDefinitions.TryGetValue(moduleName, out var found))
            {
                found.Enter(serviceConstants, new CallingInteraction(interaction, resultInteraction =>
                {
                    Continue?.Invoke(this, resultInteraction);
                }));
            }
        }
    }
}
