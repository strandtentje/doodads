namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;
public interface ICountingEnumerator<TType> : IEnumerator<TType>
{
    public bool AtEnd { get; }
    public int Cursor { get; }
    public string? ErrorState { get; set; }
}