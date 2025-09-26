#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions
{
#pragma warning disable 67
    [Category("Sourcing & Sinking")]
    [Title("Copy incoming data to outgoing data")]
    [Description("""
                 Provided a sourcing and sinking interaction, copies one to the other.
                 """)]
    public class SourceToSink : IService
    {
        [EventOccasion("After the copy was done")] 
        public event CallForInteraction? OnThen;
        [EventOccasion("Before the copy was done")]
        public event CallForInteraction? OnElse;
        [EventOccasion("Likely when copy failed or source/sink was missing")]
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            if (!interaction.TryGetClosest<ISourcingInteraction>(out ISourcingInteraction? dataSource) ||
                dataSource == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "requires sourcing interaction"));
            }
            else if (!interaction.TryGetClosest<ISinkingInteraction>(out ISinkingInteraction? dataSink) ||
                     dataSink == null)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "requires sinking interaction"));
            }
            else
            {
                OnElse?.Invoke(this, interaction);
                dataSource.SourceBuffer.CopyTo(dataSink.SinkBuffer);
                OnThen?.Invoke(this, interaction);
            }
        }

        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}