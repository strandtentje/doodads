using Ziewaar.TtLog;

namespace Ziewaar.Web.Uploadserver;

public class CommonMultipartUploadParserWorker
{
    // private readonly FixedBufferPool Receiving, Processing;
    private readonly VirtualInspectionBuffer Inspector;
    private readonly IMultipartComponentIngestor Ingestor;
    private readonly MultipartByteSequenceGovernor Boundaries;
    private readonly MultipartParsingPositionsCollection Positions;
    public string? Fault { get; private set; }

    public CommonMultipartUploadParserWorker(VirtualInspectionBuffer inspector, IMultipartComponentIngestor ingestor)
    {
        this.Inspector = inspector;
        this.Ingestor = ingestor;
        this.Boundaries = new MultipartByteSequenceGovernor();
        this.Positions = new MultipartParsingPositionsCollection();
    }

    public bool PushBuffer(byte[] availableData, int availableDataCount, out byte[] reusableBuffer)
    {
        if (this.Inspector.IsEof || !this.Ingestor.TryHaveBoundary(out var currentBoundary))
        {
            reusableBuffer = Array.Empty<byte>();
            return false;
        }

        Boundaries.Rescope(currentBoundary);
        reusableBuffer = this.Inspector.Push(availableData, availableDataCount);
        
        return ProcessNewBuffers();
    }

    private static readonly Log
        FlushNoBoundary = Log.Tech(Department.Upload,
            "No boundaries between {start} and {end}"),
        NoContentToFlush = Log.Warn(
            "No content to flush, or no receptive field"),
        BoundaryDetected = Log.Tech(Department.Upload,
            "Boundary detect"),
        RemainingContent = Log.Tech(Department.Upload,
            "Boundary was preceeded by unflushed data");

    private bool ProcessNewBuffers()
    {
        ByteSequenceNodeTag tag;
        while (Positions.ConsumedUntil < this.Inspector.WindowEnd)
        {
            if (!this.Inspector.IndexOf(Boundaries.Current,
                    Positions.ConsumedUntil,
                    out Positions.BoundaryStart,
                    out Positions.BoundaryEnd, out tag))
            {
                Log.Post(FlushNoBoundary, Positions.ConsumedUntil,
                    this.Inspector.WindowEnd);
                if (this.Ingestor.IsExpectingContent &&
                    Positions.ContentStart > 0)
                {
                    var remainderStart = Math.Max(Positions.ContentStart,
                        Positions.Break);
                    FlushWindow(remainderStart, this.Inspector.WindowEnd);
                }
                else
                {
                    Log.Post(NoContentToFlush);
                }
                Positions.Break = this.Inspector.WindowEnd;
                Boundaries.WithCrlf();
            }
            else
            {
                Log.Post(BoundaryDetected);
                Positions.ContentEnd = Positions.BoundaryStart;
                if (this.Ingestor.IsExpectingContent &&
                    Positions.ContentStart > 0)
                {
                    Log.Post(RemainingContent);
                    var remainderStart = Math.Max(Positions.ContentStart,
                        Positions.Break);
                    FlushWindow(remainderStart, Positions.ContentEnd);
                }

                if (!ProcessBoundary(tag))
                    return false;
            }
        }
        return true;
    }

    private static readonly Log
        ClosingBoundary = Log.Tech(Department.Upload,"Closing {boundary} detected"),
        OpeningHeaders = Log.Tech(Department.Upload,"Header announcement detected"),
        HeadersRead = Log.Tech(Department.Upload,"Multipart Headers {read}");

    private bool ProcessBoundary(ByteSequenceNodeTag tag)
    {
        switch (tag)
        {
            case ByteSequenceNodeTag.DashDashBoundaryDashDash:
                Log.Post(ClosingBoundary, this.Boundaries.BoundaryTree);
                if (!this.Ingestor.TryRetireBoundary())
                {
                    Fault = "Closing boundary while there was no boundary";
                    return false;
                }
                Boundaries.WithoutCrlf();
                Positions.ContentStart = 0;
                break;

            case ByteSequenceNodeTag.DashDashBoundaryCrLf:
                Log.Post(OpeningHeaders);
                Positions.HeaderStart = Positions.BoundaryEnd;
                if (!this.Inspector.IndexOf(
                        ByteSequenceNode.DoubleCrlf, Positions.HeaderStart,
                        out Positions.HeaderEnd, out Positions.ContentStart, out tag) ||
                    tag != ByteSequenceNodeTag.SecondHeaderCrlf)
                {
                    Fault = string.Format(
                        "Expected double CRLF denoting end of header " +
                        "section, but it wasn't found at {0}",
                        Positions.BoundaryEnd);
                    return false;
                }
                else
                {
                    var headerString = this.Inspector.ExtractString(
                        Positions.HeaderStart, Positions.HeaderEnd);
                    Log.Post(HeadersRead, headerString);
                    if (this.Ingestor.GiveHeaderString(headerString))
                    {
                        Boundaries.WithCrlf();
                    }
                    else
                    {
                        Fault = string.Format("Ingestor failed at; {0}",
                            Positions.ConsumedUntil);
                        return false;
                    }
                }

                break;

            default:
                Fault = string.Format(
                    "Unexpected multipart directive; {0}",
                    tag.ToString());
                return false;
        }
        return true;
    }

    private void FlushWindow(long contentStart, long contentEnd)
    {
        var readingWindowDataStart = contentStart - this.Inspector.WindowOffset;
        readingWindowDataStart = Math.Max(0, readingWindowDataStart);
        var readingWindowDataEnd = contentEnd - this.Inspector.WindowOffset;
        var readingWindowDataCount =
            readingWindowDataEnd - readingWindowDataStart;

        if (readingWindowDataCount > this.Inspector.ReadingWindow.Count)
        {
            throw new InvalidOperationException(
                "Remaining data stretched outside ReadingWindow");
        }
        if (readingWindowDataCount > 0)
        {
            this.Ingestor.GiveBinaryData(
                this.Inspector.ReadingWindow.Allocation,
                (int)readingWindowDataStart, (int)readingWindowDataCount);
        }
    }
}