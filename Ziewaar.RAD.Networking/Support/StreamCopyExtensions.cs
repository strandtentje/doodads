using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking
{
    public static class StreamCopyExtensions
    {
        public static Thread CopyGently(this (Stream from, Stream to) streams, Func<bool> test, string description)
        {
            var thr = new Thread(() =>
            {
                try
                {
                    byte[] copyBuffer = new byte[8192 * 16];
                    using (streams.from)
                    using (streams.to)
                        while (test())
                        {
                            var actuallyRead =
                                streams.from.Read(copyBuffer, 0, copyBuffer.Length);
                            if (actuallyRead > 0)
                            {
                                streams.to.Write(copyBuffer, 0, actuallyRead);
                            }
                            else
                            {
                                return;
                            }
                        }
                }
                catch (Exception ex)
                {
                    // GlobalLog.Instance?.Warning(ex, $"{description} during copy");
                }
                finally
                {
                    try
                    {
                        streams.to.Close();
                    }
                    catch (Exception ex)
                    {
                        // GlobalLog.Instance?.Warning(ex, $"{description} during close");
                    }

                    GlobalLog.Instance?.Information("Pipe {description} stopped", description);
                }
            });
            thr.Start();
            return thr;
        }
    }
}