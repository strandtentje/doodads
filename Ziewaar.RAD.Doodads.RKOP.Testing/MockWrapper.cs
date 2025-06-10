using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Testing;

public class MockWrapper : IInstanceWrapper
{
    public void Cleanup()
    {

    }

    public void SetDefinition<TResult>(CursorText atPosition, string typename, object? primaryValue, SortedList<string, object> constants,
        SortedList<string, ServiceExpression<TResult>> wrappers) where TResult : class, IInstanceWrapper, new()
    {

    }

    public void SetSoftLink<TResult>(ServiceExpression<TResult> redirectsTo) where TResult : class, IInstanceWrapper, new()
    {
                
    }
    public void SetHardLink<TResult>(ServiceExpression<TResult> redirectsTo) where TResult : class, IInstanceWrapper, new()
    {
                
    }
    public void SetUnconditionalSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new()
    {
                
    }
    public void SetAlternativeSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new()
    {
                
    }
    public void SetContinueSequence<TResult>(ServiceExpression<TResult>[] sequence) where TResult : class, IInstanceWrapper, new()
    {
                
    }
    public void SetDefinition<TResult>(CursorText atPosition, string typename, TResult? nextInLine, TResult? continuation, SortedList<string, object> constants, SortedList<string, TResult> wrappers) where TResult : IInstanceWrapper, new()
    {
                
    }
}