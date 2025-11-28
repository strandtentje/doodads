using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP;

public interface IInstanceWrapper
{
    void Cleanup();
#nullable enable
    void SetDefinition<TResult>(
        CursorText atPosition,
        string typename,
        object? primaryValue,
        IReadOnlyDictionary<string, object> constants, 
        SortedList<string, ServiceExpression<TResult>> wrappers) 
        where TResult : class, IInstanceWrapper, new();
    void SetUnconditionalSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new();
    void SetAlternativeSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new();
    void SetContinueSequence<TResult>((bool isOmni, ServiceExpression<TResult> service)[] sequence) where TResult : class, IInstanceWrapper, new();
}
