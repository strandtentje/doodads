using System.Runtime.CompilerServices;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Stage;
public class GlWindow : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var wndThread = new Thread(RunWindow);
        wndThread.SetApartmentState(ApartmentState.STA);
        wndThread.Start();
        void RunWindow(object? obj)
        {
            using (var wnd = new FixedPipelineGLWindow())
            {
                wnd.Load += WndOnLoad;
                void WndOnLoad()
                {
                    OnThen?.Invoke(this, new GlWindowInteraction(interaction, wnd));
                }
                wnd.Run();
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}