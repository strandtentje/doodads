namespace Ziewaar.RAD.Doodads.RKOP;

public interface IInstanceWrapper
{
    void Cleanup();
#nullable enable
    void SetDefinition<TResult>(
        CursorText atPosition,
        string typename,
        object? primaryValue,
        SortedList<string, object> constants, 
        SortedList<string, ServiceExpression<TResult>> wrappers) 
        where TResult : class, IInstanceWrapper, new();
    void SetSoftLink<TResult>(ServiceExpression<TResult> redirectsTo) where TResult : class, IInstanceWrapper, new();
    void SetHardLink<TResult>(ServiceExpression<TResult> redirectsTo) where TResult : class, IInstanceWrapper, new();
    void SetUnconditionalSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new();
    void SetAlternativeSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new();
    void SetContinueSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new();
}
