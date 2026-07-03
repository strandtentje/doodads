using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Define.Doodads.Expo.Timeline
{
    public abstract class BasicService : IService
    {
        public virtual event CallForInteraction? OnThen;
        public virtual event CallForInteraction? OnElse;
        public virtual event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            try
            {
                TryEnter(constants, interaction);
            }
            catch (BasicException ex)
            {
                OnException?.Invoke(this, interaction.AppendRegister(ex.Message));
            }
        }

        public abstract void TryEnter(StampedMap constants, IInteraction interaction);

        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
