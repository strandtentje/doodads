namespace Ziewaar.RAD.Doodads.RKOP;

public interface IInstanceWrapper
{
    void Cleanup();
#nullable enable
    void SetDefinition<TResult>(
        CursorText atPosition,
        string typename,
        TResult? nextInLine,
        TResult? continuation,
        SortedList<string, object> constants, 
        SortedList<string, TResult> wrappers) where TResult : IInstanceWrapper, new();
    void SetReference<TResult>(ServiceDescription<TResult> redirectsTo) where TResult : class, IInstanceWrapper, new();
}
