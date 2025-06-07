namespace Ziewaar.RAD.Doodads.RKOP;

public class ContinueChain<TResult>(ServiceDescription<TResult> cause) : ChainingPayload<TResult>(cause, "_continue") where TResult : class, IInstanceWrapper, new();
