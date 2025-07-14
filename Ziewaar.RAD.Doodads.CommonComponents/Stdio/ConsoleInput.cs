#pragma warning disable 67
#nullable enable
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
[Category("Input from source")]
public class ConsoleInput : IService
{
    private static readonly Stream StandardInput = System.Console.OpenStandardInput();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ConsoleSourceInteraction(interaction, StandardInput));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}


public class DumpToConsole : IService
{
    private readonly UpdatingPrimaryValue DumpNameConstant = new();
    private string? CurrentDumpName;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DumpNameConstant).IsRereadRequired(out string? dumpName) && dumpName != null)
        {
            this.CurrentDumpName = dumpName;
        }
        if (CurrentDumpName == null)
            Console.WriteLine("Untitled dump");
        else
            Console.WriteLine("Dumping {0}", CurrentDumpName);
        for (var working = interaction; working!= null && working is not StopperInteraction; working = working.Stack)
        {
            Console.Write(JsonConvert.SerializeObject(new
            {
                Type = working.GetType().Name,
                Memory = working.Memory,
                Register = working.Register
            }, Formatting.Indented));
        }
        Console.WriteLine("Before OnThen");
        OnThen?.Invoke(this, interaction);
        Console.WriteLine("After OnThen");
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}
