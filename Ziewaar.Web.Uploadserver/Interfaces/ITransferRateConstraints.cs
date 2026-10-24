namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface ITransferRateConstraints
{
    int Sleep { get; }
    int BufferSize { get; }
    int Burst { get; }
}