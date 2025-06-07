namespace Ziewaar.RAD.Doodads.RKOP;

public class NonChaining<TResult>() : ChainingPayload<TResult>(null) where TResult : class, IInstanceWrapper, new();
