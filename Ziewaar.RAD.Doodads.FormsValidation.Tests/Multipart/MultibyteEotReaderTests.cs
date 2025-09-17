using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;
using System.Collections;
namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;
[TestClass]
public class MultibyteEotReaderTests
{
    private class ByteEnumerator : ICountingEnumerator<byte>
    {
        private readonly byte[] _data;
        private int _index = -1;
        public ByteEnumerator(params byte[] data)
        {
            _data = data;
        }
        public byte Current => _data[_index];
        object IEnumerator.Current => Current;
        public long Cursor => _index;
        public bool AtEnd { get; private set; }
        public string? ErrorState { get; set; }
        public bool MoveNext()
        {
            if (_index + 1 >= _data.Length)
            {
                AtEnd = true;
                return false;
            }

            _index++;
            return true;
        }
        public void Reset()
        {
            _index = -1;
            AtEnd = false;
            ErrorState = null;
        }
        public void Dispose()
        {
        }
    }
    [TestMethod]
    public void ShouldMoveNextUntilEotSequenceIsDetected()
    {
        // Arrange: the data contains bytes ending with CRLF
        var data = new byte[] { 65, 66, 67, 13, 10 }; // ABC\r\n
        using var reader = MultibyteEotReader.CreateForCrlf(new ByteEnumerator(data));

        var output = new List<byte>();

        // Act: move until EOT
        while (reader.MoveNext())
        {
            output.Add(reader.Current);
        }

        // Assert
        CollectionAssert.AreEqual(new byte[] { 65, 66, 67 }, output, "Should emit all bytes before EOT marker");
        Assert.IsTrue(reader.AtEnd, "Reader should detect AtEnd = true when EOT is matched");
        Assert.IsFalse(reader.MoveNext(), "Reader should stop after detecting EOT");
    }
    [TestMethod]
    public void ShouldContinueAfterReset()
    {
        // Arrange: input has two blocks separated by CRLF
        var data = new byte[] { 88, 89, 13, 10, 90, 91, 13, 10 }; // XY\r\nZ[\r\n
        using var reader = MultibyteEotReader.CreateForCrlf(new ByteEnumerator(data));
        var results = new List<byte[]>();

        // Act: first block
        var buffer = new List<byte>();
        while (reader.MoveNext())
            buffer.Add(reader.Current);
        results.Add(buffer.ToArray());

        reader.Reset();

        buffer.Clear();
        while (reader.MoveNext())
            buffer.Add(reader.Current);
        results.Add(buffer.ToArray());

        // Assert
        CollectionAssert.AreEqual(new byte[] { 88, 89 }, results[0], "First block should be parsed before first EOT");
        CollectionAssert.AreEqual(new byte[] { 90, 91 }, results[1], "After reset, should continue after previous EOT");
    }
    [TestMethod]
    public void ShouldNotMatchNearMissSequences()
    {
        // Arrange: Similar-looking but invalid sequences
        var data = new byte[] { 65, 13, 13, 10 }; // A\r\r\n (only one CRLF)
        using var reader = MultibyteEotReader.CreateForCrlf(new ByteEnumerator(data));

        var output = new List<byte>();

        // Act
        while (reader.MoveNext())
            output.Add(reader.Current);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 65, 13 }, output, "Should not prematurely detect EOT on near match");
        Assert.IsTrue(reader.AtEnd, "Should reach AtEnd after consuming full EOT sequence");
    }
    [TestMethod]
    public void ShouldReturnCurrentCorrectly()
    {
        // Arrange
        var data = new byte[] { 0x41, 0x42, 13, 10 }; // A, B, \r, \n
        using var reader = MultibyteEotReader.CreateForCrlf(new ByteEnumerator(data));

        var chars = new List<char>();

        // Act
        while (reader.MoveNext())
            chars.Add((char)reader.Current);

        // Assert
        CollectionAssert.AreEqual(new[] { 'A', 'B' }, chars, "Current should reflect correct byte values");
    }
}