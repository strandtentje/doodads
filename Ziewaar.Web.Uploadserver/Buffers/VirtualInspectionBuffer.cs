namespace Ziewaar.Web.Uploadserver.Buffers;

/// <summary>
/// A double buffer utility that maintains a Window- and a Peek-buffer
/// Allows seeking against the Window buffer, and allows those seeks to 
/// overflow into the Peek buffer. New data buffers get shifted into
/// the Peek-buffer first, the preceeding Peek-buffer gets shifted into 
/// the Window buffer, and the preceeding Window-buffer becomes available 
/// again for other purposes, and cannot be sought against anymore.
/// 
/// The interface against this utility class will always communicate relative
/// to the very first buffer that was put in, so indexes are for 
/// all the data this InspectionBuffer has seen, rather than only the 
/// buffers that are currently in view.
/// </summary>
public class VirtualInspectionBuffer
{
    private FixedBuffer Window = FixedBuffer.Null;
    private FixedBuffer Peek = FixedBuffer.Null;
    public long WindowOffset;
    /// <summary>
    /// Exclusive ending position of the window in view.
    /// </summary>
    public long WindowEnd;
    public long PeekEnd;
    /// <summary>
    /// Buffer currently in view.
    /// </summary>
    public FixedBuffer ReadingWindow { get { return Window; } }

    /// <summary>
    /// Provide the next frame of data to the Inspection Buffer,
    /// pushing the Peek Frame into the Window, and pushing the 
    /// currently active window, out of view.
    /// </summary>
    /// <param name="buffer">Data buffer to stage</param>
    /// <param name="count">Amount of useful bytes in this buffer</param>
    /// <returns>Buffer that fell out of the inspector 
    /// and can be re-used.</returns>
    public byte[] Push(byte[] buffer, int count)
    {
        WindowOffset += Window.Count;

        var retiree = Window;
        Window = Peek;
        Peek = new FixedBuffer(buffer, count);

        WindowEnd = WindowOffset + Window.Count;
        PeekEnd = WindowOffset + Window.Count + Peek.Count;

        return retiree.Allocation;
    }

    /// <summary>
    /// True if the active window was pushed into EOF
    /// </summary>
    public bool IsEof
    {
        get
        {
            return (Window != FixedBuffer.Null && Window.Count == 0);
        }
    }

    /// <summary>
    /// Works like a combination of IndexOf and the StartsWith
    /// against ByteSequnceNode. Will report back with the first
    /// position at which some route on the tree was satisfiably matched.
    /// </summary>
    /// <param name="optionTree">Tree of possible sequences</param>
    /// <param name="startAt">Start looking at this position</param>
    /// <param name="resultStartAt">Start of resulting match</param>
    /// <param name="resultEndAt">End of resulting match (exclusive)</param>
    /// <param name="tag">Byte Sequence Tag that was matched</param>
    /// <returns>True if some sequence tree was matched, false otherwise.</returns>
    public bool IndexOf(
        ByteSequenceNode optionTree,
        long initiallyStartAt,
        out long resultStartAt,
        out long resultEndAt,
        out ByteSequenceNodeTag tag)
    {
        for (var startAt = initiallyStartAt; startAt < this.WindowEnd; startAt++)
        {
            resultStartAt = this.IndexOf(optionTree.Sequence, startAt);
            if (resultStartAt < 0)
            {
                resultEndAt = -1;
                tag = optionTree.Tag;
                return false;
            }
            else
            {
                if (StartsWith(optionTree, resultStartAt, out resultEndAt, out tag))
                    return true;
            }
            startAt = resultStartAt;
        }
        resultStartAt = -1;
        resultEndAt = -1;
        tag = optionTree.Tag;
        return false;
    }

    /// <summary>
    /// Works like byte-array StartsWith, but accepts a
    /// ByteSequenceNode, so as to evaluate multiple possible 
    /// byte sequences, and report which one came out.
    /// </summary>
    /// <param name="optionTree">Tree of possible sequences</param>
    /// <param name="startAt">Position absolute to start of this 
    /// Inspection buffer at which to look</param>
    /// <param name="resultEndAt">Where the matching result ended
    /// (Exclusive)</param>
    /// <param name="tag">Sequence Node Tag at which the search ended
    /// </param>
    /// <returns>True if some Byte Sequence Tree was fully matched,
    /// false otherwise.</returns>
    public bool StartsWith(
        ByteSequenceNode optionTree,
        long startAt,
        out long resultEndAt,
        out ByteSequenceNodeTag tag)
    {
        resultEndAt = startAt + optionTree.Sequence.Length;
        tag = optionTree.Tag;
        if (this.StartsWith(optionTree.Sequence, startAt))
        {
            var continueAt = resultEndAt;
            foreach (var item in optionTree.NextOptions)
            {
                if (this.StartsWith(item, continueAt, out resultEndAt, out tag))
                    return true;
            }
            return optionTree.CanEndHere;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Combines the single-byte IndexOf and StartsWith methods, 
    /// so as to find the first index at which a sequence of bytes 
    /// occurs, starting at some position.
    /// </summary>
    /// <param name="sequence">Sequence to look for</param>
    /// <param name="startAt">Start looking from this position absolute
    /// to the Inspection Buffer</param>
    /// <returns>Position absolute to start of Inspection buffer at which 
    /// the sequence first occured, or some value lower than 0 if 
    /// it did not occur.</returns>
    public long IndexOf(byte[] sequence, long initiallyStartAt)
    {
        if (sequence.Length == 0)
            throw new ArgumentOutOfRangeException("cannot find empty thing");
        else if (initiallyStartAt < WindowOffset)
            throw new ArgumentOutOfRangeException("startAt below window offset");
        else if (initiallyStartAt >= WindowEnd)
            throw new ArgumentOutOfRangeException("startAt above window");

        for (var startAt = initiallyStartAt; startAt < WindowEnd; startAt++)
        {
            var firstCharacterMatchStartsAt = this.IndexOf(sequence[0], startAt);
            if (firstCharacterMatchStartsAt < 0)
                return -1;

            if (this.StartsWith(sequence, firstCharacterMatchStartsAt))
                return firstCharacterMatchStartsAt;

            startAt = firstCharacterMatchStartsAt;
        }
        return -1;
    }

    /// <summary>
    /// Checks if the provided byte sequence occurs exactly in this
    /// inspection buffer, and starts at exactly some provided position.
    /// </summary>
    /// <param name="sequence">Sequence that must be matched</param>
    /// <param name="startAt">Position to check at, absolute to start
    /// of inspection buffer</param>
    /// <returns>True when the sequence was matched at the given
    /// position, otherwise, false.</returns>
    public bool StartsWith(byte[] sequence, long startAt)
    {
        int sequencePosition;
        for (sequencePosition = 0;
            sequencePosition < sequence.Length &&
            (startAt + sequencePosition) < PeekEnd &&
            this[startAt + sequencePosition] == sequence[sequencePosition];
            sequencePosition++) ;
        return sequencePosition == sequence.Length;
    }

    private long _indexerVirtualPosition;

    /// <summary>
    /// Retrieve a single byte from the inspection buffer, relative
    /// to its start. Allows pulling from both the ActiveWindow and the
    /// Peek buffer.
    /// </summary>
    /// <param name="position">Position absolute to start of inspection
    /// buffer.</param>
    /// <returns>Byte value</returns>
    public byte this[long position]
    {
        get
        {
            _indexerVirtualPosition = position - WindowOffset;

            if (_indexerVirtualPosition < Window.Count)
                return Window.Allocation[_indexerVirtualPosition];

            _indexerVirtualPosition -= Window.Count;

            return Peek.Allocation[_indexerVirtualPosition];
        }
    }

    /// <summary>
    /// Works like IndexOf, but also includes the Peek buffer 
    /// in the search. This defeats the purpose of having a Peek
    /// buffer, so avoid using this.
    /// </summary>
    /// <param name="b">Byte to find</param>
    /// <param name="startAt">Position to start looking from, relative
    /// to the start of the inspection buffer.</param>
    /// <returns></returns>
    public long PeekIndexOf(byte b, long startAt)
    {
        if (Window == FixedBuffer.Null)
            return -1;
        var virtualStart = (int)(startAt - WindowOffset);
        if (virtualStart < 0)
            throw new ArgumentOutOfRangeException(
                "this part of the inspection window is out of view");
        if (virtualStart < Window.Count)
        {
            var result = Array.IndexOf(Window.Allocation, b, virtualStart, Window.Count);
            if (result >= virtualStart)
                return WindowOffset + result;
        }
        if (Peek == FixedBuffer.Null)
            return -1;
        // in case startAt is in the Peek buffer,
        // we'd have to offset our virtualStart with the 
        // size of the Window buffer first, so that we have 
        // an accurate position in the peek buffer.
        // if startAt was in the Window buffer, but nothing was
        // found there, we can clamp it to 0.
        virtualStart -= Window.Count;
        virtualStart = Math.Max(virtualStart, 0);
        if (virtualStart < Peek.Count)
        {
            var result = Array.IndexOf(Peek.Allocation, b, virtualStart, Peek.Count);
            if (result >= virtualStart)
                return WindowOffset + Window.Count + result;
        }
        return -1;
    }

    /// <summary>
    /// Finds the first occurence of a given byte in the currently
    /// Active Window, starting at some position relative to the 
    /// start of this inspection buffer; not the Active Window itself.
    /// </summary>
    /// <param name="b">Byte to find</param>
    /// <param name="startAt">Position to start looking at</param>
    /// <returns>Index relative to start of Inspection Buffer at 
    /// which the byte was found, or a value below 0 if none was found
    /// </returns>
    public long IndexOf(byte b, long startAt)
    {
        if (Window == FixedBuffer.Null)
            return -1;
        var virtualStart = (int)(startAt - WindowOffset);
        if (virtualStart < 0)
            throw new ArgumentOutOfRangeException(
                "this part of the inspection window has passed");
        if (virtualStart >= Window.Count)
            throw new ArgumentOutOfRangeException(
                "this part of the inspection window han't yet come");

        var result = Array.IndexOf(
            Window.Allocation, b, virtualStart,
            Window.Count - virtualStart);
        if (result < 0)
            return -1;

        return WindowOffset + result;
    }

    /// <summary>
    /// Turn a range of bytes spanning over both the 
    /// Window and its Successor (Peek) into a string.
    /// </summary>
    /// <param name="byteStart">
    /// First byte to start turning into ASCII string
    /// </param>
    /// <param name="byteEnd">
    /// Exclusive end of byte range to turn into ASCII string.
    /// </param>
    /// <returns></returns>
    public string ExtractString(long byteStart, long byteEnd)
    {
        StringBuilder sbl = new StringBuilder();
        var virtualPosition = byteStart - WindowOffset;
        var initiallyRequestedCount = byteEnd - byteStart;
        var remainingCount = initiallyRequestedCount;
        if (virtualPosition < 0)
            throw new ArgumentOutOfRangeException(
                "Start position is out of view");
        if (initiallyRequestedCount < 0)
            throw new ArgumentOutOfRangeException(
                "Start position cannot come after end position");
        if (byteEnd >= (WindowOffset + Window.Count + Peek.Count))
            throw new ArgumentOutOfRangeException(
                "End position is out of view");
        if (virtualPosition < this.Window.Count)
        {
            // we take what we can from the Window
            var availableCount = Math.Min(
                this.Window.Count - virtualPosition, remainingCount);
            // this must be at most the requested count, 
            // and that cannot exceed the Window size minus the 
            // seek position.
            sbl.Append(Encoding.ASCII.GetString(
                this.Window.Allocation,
                (int)virtualPosition,
                (int)availableCount));
            // what we managed to take, we subtract from the 
            // requestedCount so that we know what remainder
            // (if any) we need to pull from the Peek buffer.
            remainingCount -= availableCount;
        }

        virtualPosition -= this.Window.Count;
        virtualPosition = Math.Max(virtualPosition, 0);

        if (virtualPosition < this.Peek.Count)
        {
            // we take what we need to (and can) from the Peek.
            var availableCount = Math.Min(
                this.Peek.Count - virtualPosition, remainingCount);
            // this must be at most the requestedCount (which 
            // may have become less because we read from the 
            // Window already), and it cannot exceed Peek size minus
            // contd. seek position;
            sbl.Append(Encoding.ASCII.GetString(
                this.Peek.Allocation,
                (int)virtualPosition,
                (int)availableCount));
        }

        // theoretically, this assertion gets caught earlier
        // as an ArgumentException, it's however a great canary
        // in case of programmer error.
        if (initiallyRequestedCount != sbl.Length)
            throw new InvalidOperationException(
                "Extracted string had a different amount of data " +
                "than the range specified with byteStart/byteEnd");
        return sbl.ToString();
    }
}