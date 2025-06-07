using System;
namespace Ziewaar.RAD.Doodads.RKOP;

public abstract class ChainingPayload<TResult>(ServiceDescription<TResult>? source, string? suffix = null) where TResult : class, IInstanceWrapper, new()
{
    public string Suffix => suffix ?? throw new NullReferenceException("No source provided");
    public ServiceDescription<TResult> Source => source ?? throw new NullReferenceException("No source provided");
    public string NewKey => $"{Source.ConstantsDescription.BranchKey}{Suffix}";
}
