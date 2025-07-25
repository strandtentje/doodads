﻿using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

namespace Ziewaar.RAD.Doodads.ModuleLoader;
#nullable enable
public class ServiceBuilder : IInstanceWrapper, IEntryPoint
{
    public IAmbiguousServiceWrapper? CurrentService { get; private set; }
    public void Cleanup()
    {
        CurrentService?.Cleanup();
        CurrentService = null;
    }
    private TResult EnsureCurrent<TResult>() where TResult : class, IAmbiguousServiceWrapper, new()
    {
        if (this.CurrentService is not TResult result)
        {
            Cleanup();
            result = new TResult();
        }
        return result;
    }
    private ServiceBuilder Cast<TResult>(TResult? item) where TResult : class, IInstanceWrapper, new() =>
        (ServiceBuilder)(IInstanceWrapper)(item ?? throw new ArgumentNullException(nameof(item)));
    public void SetDefinition<TResult>(
        CursorText atPosition,
        string typename,
        object? primaryValue,
        IReadOnlyDictionary<string, object> constants,
        SortedList<string, ServiceExpression<TResult>> wrappers)
        where TResult : class, IInstanceWrapper, new()
    {
        var wrapper = EnsureCurrent<DefinedServiceWrapper>();
        var castWrappers = wrappers.ToDictionary(
            x => x.Key,
            x => Cast(x.Value.ResultSink));
        wrapper.Update(atPosition, typename, primaryValue, constants, castWrappers);
        CurrentService = wrapper;
    }
    public void SetUnconditionalSequence<TResult>(ServiceExpression<TResult>[] sequence)
        where TResult : class, IInstanceWrapper, new()
    {
        var wrapper = EnsureCurrent<DoNextOnDoneWrapper>();
        wrapper.SetTarget(sequence.Select(x => Cast(x.ResultSink)).ToArray());
        CurrentService = wrapper;
    }
    public void SetAlternativeSequence<TResult>(ServiceExpression<TResult>[] sequence)
        where TResult : class, IInstanceWrapper, new()
    {
        var wrapper = EnsureCurrent<DoNextOnElseWrapper>();
        wrapper.SetTarget(sequence.Select(x => Cast(x.ResultSink)).ToArray());
        CurrentService = wrapper;
    }
    public void SetContinueSequence<TResult>(ServiceExpression<TResult>[] sequence)
        where TResult : class, IInstanceWrapper, new()
    {
        var wrapper = EnsureCurrent<DoNextOnThenWrapper>();
        wrapper.SetTarget(sequence.Select(x => Cast(x.ResultSink)).ToArray());
        CurrentService = wrapper;
    }
    [DebuggerHidden]
    public void Run(object sender, IInteraction interaction)
    {
        CurrentService!.Run(sender, interaction);
    }
}