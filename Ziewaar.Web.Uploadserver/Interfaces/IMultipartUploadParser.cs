namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IMultipartUploadParser : IDisposable
{
    /// <summary>
    /// Get a buffer usable for receiving data into
    /// </summary>
    /// <returns>A byte array allocation of some size.</returns>
    byte[] PopForReceiving();
    /// <summary>
    /// Push back a receive buffer that has been used for receiving.
    /// </summary>
    /// <param name="receivedBuffer">An allocation previously provided by PopReceiveBuffer, that has now been filled with data.</param>
    /// <param name="count">The amount of bytes starting at 0 that has actually been written.</param>
    void PushForProcessing(byte[] receivedBuffer, int count);
    /// <summary>
    /// Tell the parser we're finished.
    /// </summary>
    void Finish(
        IDictionary<string, IMultipartFinishedComponentResult> values,
        IList<string> faults);
}