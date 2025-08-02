#nullable enable
using Newtonsoft.Json;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
#pragma warning disable 67
[Category("Console")]
[Title("Dump the full context to console")]
[Description("Dont do this in prod.")]
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