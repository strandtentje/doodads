#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
public class AddProgram : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var fileInfo = new FileInfo(tsi.ReadAllText());
        if (!fileInfo.Exists)
        {
            try
            {
                using (var x = fileInfo.CreateText())
                {
                    x.WriteLine();
                }
                OnThen?.Invoke(this, new CommonInteraction(interaction, fileInfo));
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            }
        }
        else
        {
            OnElse?.Invoke(this, new CommonInteraction(interaction, fileInfo));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}