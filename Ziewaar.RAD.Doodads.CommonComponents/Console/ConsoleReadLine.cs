using System;
using System.Collections.Generic;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio
{
    public class ConsoleReadLine : IService
    {
        public event EventHandler<IInteraction> OnError;
        public event EventHandler<IInteraction> Variable;
        [DefaultBranch]
        public event EventHandler<IInteraction> Continue;
        public static Stream StandardInput = Console.OpenStandardInput();
        public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
        {
            string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
                (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);

            var outputTo = SourceSetting(Variable, "variable", "readline");
            var lineRead = Console.ReadLine();

            Continue?.Invoke(this,
                new ConsoleToVariableInteraction(interaction, outputTo, lineRead));
        }
    }
}
