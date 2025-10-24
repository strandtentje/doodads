using System.ComponentModel;
using System.Threading.Channels;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Graphics;
public class GraphicsView : IService, IDisposable
{
    private readonly UpdatingPrimaryValue TitleConstant = new();
    private readonly UpdatingKeyValue UpdateRateConstant = new("updaterate");
    private readonly UpdatingKeyValue VsyncModeConstant = new("vsync");
    private readonly UpdatingKeyValue NumberOfSamplesConstant = new("samples");
    private readonly UpdatingKeyValue ClientWidthConstant = new("width");
    private readonly UpdatingKeyValue ClientHeightConstant = new("height");
    private readonly UpdatingKeyValue WindowStateConstant = new("windowstate");
    private decimal CurrentUpdateFrequency = 100, CurrentNumberOfSamples = 4, CurrentWidth = 800, CurrentHeight = 600;
    private string? CurrentVsync = "Adaptive", CurrentWindowState = "Normal", CurrentTitle = "Window";
    private GraphicsViewWindow? CurrentWindow = null;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, TitleConstant).IsRereadRequired(() => "Title", out CurrentTitle)
            || (constants, UpdateRateConstant).IsRereadRequired(() => 100, out CurrentUpdateFrequency)
            || (constants, VsyncModeConstant).IsRereadRequired(() => "Adaptive", out CurrentVsync)
            || (constants, NumberOfSamplesConstant).IsRereadRequired(() => 4, out CurrentNumberOfSamples)
            || (constants, ClientWidthConstant).IsRereadRequired(() => 800, out CurrentWidth)
            || (constants, ClientHeightConstant).IsRereadRequired(() => 600, out CurrentHeight)
            || (constants, WindowStateConstant).IsRereadRequired(out CurrentWindowState)
            || this.CurrentWindow == null)
        {
            this.CurrentWindow?.Dispose();
            GameWindowSettings x = new GameWindowSettings()
            {
                UpdateFrequency = (double)CurrentUpdateFrequency
            };
            NativeWindowSettings y = new()
            {
                Vsync = Enum.Parse<VSyncMode>(CurrentVsync ?? "Adaptive"),
                NumberOfSamples = (int)CurrentNumberOfSamples,
                ClientSize = (X: (int)CurrentWidth, Y: (int)CurrentHeight),
                WindowState = Enum.Parse<WindowState>(CurrentWindowState ?? "Normal"),
                Title = CurrentTitle ?? "Title"
            };
            this.CurrentWindow = new GraphicsViewWindow(x, y);
            Task.Run(() => { this.CurrentWindow?.Run(); });
        }
        OnThen?.Invoke(this, new GraphicsViewInteraction(interaction, this.CurrentWindow));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose() => Task.Run(() =>
    {
        this.CurrentWindow?.Close();
        this.CurrentWindow?.Dispose();
        this.CurrentWindow = null;
    });
}
public class KeyboardInput : IService
{
    private UpdatingKeyValue KeyCodeDownConstant = new("down");
    private UpdatingKeyValue KeyCodeUpConstant = new("up");
    private string? CurrentKeyCodeDown;
    private string? CurrentKeyCodeUp;
    private Keys CurrentDownKey = Keys.Unknown, CurrentUpKey = Keys.Unknown;
    private Channel<long>? KeyDownChannel;
    private Channel<long>? KeyUpChannel;
    private Keys PreviousDownKey, PreviousUpKey;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, KeyCodeDownConstant).IsRereadRequired(out string? keyCodeDown))
            this.CurrentKeyCodeDown = keyCodeDown;
        if ((constants, KeyCodeUpConstant).IsRereadRequired(out string? keyCodeUp))
            this.CurrentKeyCodeUp = keyCodeUp;
        if ((this.CurrentKeyCodeDown == null || !Enum.TryParse<Keys>(CurrentKeyCodeDown, out CurrentDownKey)) &&
            (this.CurrentKeyCodeUp == null || !Enum.TryParse<Keys>(CurrentKeyCodeUp, out CurrentUpKey)))
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("either up or down must be specified as member of opentk keys"));
            return;
        }
        if (!interaction.TryGetClosest<GraphicsViewInteraction>(out GraphicsViewInteraction? view) || view == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("graphics view interaction required to catch keys"));
            return;
        }
        if (CurrentDownKey > Keys.Unknown && PreviousDownKey != CurrentDownKey)
        {
            KeyDownChannel?.Writer.Complete();
            if (view.Window.DownBindings.TryGetValue(PreviousDownKey, out var lastList) && KeyDownChannel != null)
                lastList.Remove(KeyDownChannel.Writer.WriteAsync);
            KeyDownChannel = Channel.CreateUnbounded<long>();
            var kbi = new KeyBindingInteraction(interaction);
            Task.Run(() =>
            {
                while (!KeyDownChannel.Reader.Completion.IsCompleted)
                {
                    kbi.Timestamp = KeyDownChannel.Reader.ReadAsync().Result;
                    OnThen?.Invoke(this, kbi);
                }
            });
            if (!view.Window.DownBindings.TryGetValue(CurrentDownKey, out var list))
                view.Window.DownBindings[CurrentDownKey] = list = new();
            list.Add(KeyDownChannel.Writer.WriteAsync);
        }
        if (CurrentUpKey > Keys.Unknown && PreviousUpKey != CurrentUpKey)
        {
            KeyUpChannel?.Writer.Complete();
            if (view.Window.UpBindings.TryGetValue(PreviousUpKey, out var lastList) && KeyUpChannel != null)
                lastList.Remove(KeyUpChannel.Writer.WriteAsync);
            KeyUpChannel = Channel.CreateUnbounded<long>();
            var kbi = new KeyBindingInteraction(interaction);
            Task.Run(() =>
            {
                while (!KeyUpChannel.Reader.Completion.IsCompleted)
                {
                    kbi.Timestamp = KeyUpChannel.Reader.ReadAsync().Result;
                    OnThen?.Invoke(this, kbi);
                }
            });
            if (!view.Window.UpBindings.TryGetValue(CurrentUpKey, out var list))
                view.Window.UpBindings[CurrentUpKey] = list = new();
            list.Add(KeyUpChannel.Writer.WriteAsync);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class KeyBindingInteraction(IInteraction parent) : IInteraction
{
    public long Timestamp;
    public IInteraction Stack => parent;
    public object Register => (decimal)Timestamp;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}
public class GraphicsViewWindow : GameWindow
{
    public readonly SortedList<Keys, List<Func<long, CancellationToken, ValueTask>>> UpBindings = new(),
        DownBindings = new();
    public float[] TriangleVertices = new float[3 * 1024 * 256];
    public float[] QuadVertices = new float[4 * 1024 * 256];
    public GraphicsViewWindow(
        GameWindowSettings gameWindowSettings,
        NativeWindowSettings nativeWindowSettings) :
        base(gameWindowSettings, nativeWindowSettings)
    {
    }
    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
    }
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        var millisecond = GlobalStopwatch.Instance.ElapsedMilliseconds;
        UpBindings.Where(binding => KeyboardState.IsKeyReleased(binding.Key)).SelectMany(x => x.Value).ToList()
            .ForEach(bindingAction => bindingAction(millisecond, CancellationToken.None));
        DownBindings.Where(binding => KeyboardState.IsKeyPressed()(binding.Key)).SelectMany(x => x.Value).ToList()
            .ForEach(bindingAction => bindingAction(millisecond, CancellationToken.None));
    }
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        SwapBuffers();
    }
}