using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

    [TestClass]
    public class StreamByteEnumeratorTests
    {
        [TestMethod]
        public void StreamByteEnumerator_ReadsAllBytes()
        {
            var data = new byte[] { 10, 20, 30, 40 };
            using var stream = new MemoryStream(data);
            var enumerator = new StreamByteEnumerator(stream);

            var result = new List<byte>();
            while (enumerator.MoveNext())
            {
                result.Add(enumerator.Current);
            }

            CollectionAssert.AreEqual(data, result);
        }

        [TestMethod]
        public void StreamByteEnumerator_ThrowsIfAccessedBeforeMoveNext()
        {
            var stream = new MemoryStream(new byte[] { 1 });
            var enumerator = new StreamByteEnumerator(stream);
            Assert.ThrowsException<InvalidOperationException>(() => { var _ = enumerator.Current; });
        }

        [TestMethod]
        public void StreamByteEnumerator_Dispose_ClosesStream()
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var enumerator = new StreamByteEnumerator(stream);
            enumerator.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => stream.ReadByte());
        }
    }