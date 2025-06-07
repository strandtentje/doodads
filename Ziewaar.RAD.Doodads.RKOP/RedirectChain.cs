namespace Ziewaar.RAD.Doodads.RKOP;

public class RedirectChain<TResult>(ServiceDescription<TResult> cause) : ChainingPayload<TResult>(cause, "_redirect") where TResult : class, IInstanceWrapper, new();
