namespace Ziewaar.RAD.Doodads.RKOP;

public class ConcatChain<TResult>(ServiceDescription<TResult> cause) : ChainingPayload<TResult>(cause, "_concat") where TResult : class, IInstanceWrapper, new();
