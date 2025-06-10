#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

public class ReadLinesInteraction(IInteraction parent, string name, StreamReader reader) : IInteraction
{
    public string Name => name;
    public event EventHandler<object>? EndOfStream;
    public IInteraction Stack => parent;
#pragma warning disable CS8974 // Converting method group to non-delegate type is exactly what we're doing here.
    public object Register => Enumerate;
#pragma warning restore CS8974 // Go on, call the police, i dare u.
    private IEnumerable<string> Enumerate()
    {
        string currentLine = "";

        while (true)
        {
            try
            {
                if (!reader.EndOfStream && reader.ReadLine() is { } line)
                {
                    currentLine = line;
                } else
                {
                    break;
                }
            } catch(Exception ex)
            {
                break;
            }
            yield return currentLine;     
        }
        EndOfStream?.Invoke(this, new object());
    }
    public void Close() => reader.Close();
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}