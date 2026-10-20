namespace Ziewaar.Web.Uploadserver;

public class CommonUploadReceiver : IUploadReceiver
{
    private readonly IUploadSizeGovernor SizeGovernor;
    private readonly IMultipartUploadParserFactory ParserFactory;

    public CommonUploadReceiver(
        IUploadSizeGovernor sizeGovernor,
        IMultipartUploadParserFactory parserFactory)
    {
        this.SizeGovernor = sizeGovernor;
        this.ParserFactory = parserFactory;
    }

    private static readonly Log
        ValidatedUpload = Log.Tech(
            "Validated {uploadBoxId} into {targetDir} " +
            "with {contentLength} and {boundary} " +
            "having {temporaryFile}");

    private const string
        MULTIPART_FORM_DATA = "multipart/form-data",
        BOUNDARY_EQ = "boundary=";

    public bool TryValidateUpload(string? contentType, long contentLength, UploadBox selectedUploadBox,
        out PreValidationState state)
    {
        if (contentType == null)
        {
            state = PreValidationState.BadStart;
            return false;
        }

        var contentTypeParts = contentType.Split(
            new char[] { ';' },
            2, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        var expectsMultipartFormData = contentTypeParts.ElementAtOrDefault(0);
        var expectsBoundary = contentTypeParts.ElementAtOrDefault(1);
        if (string.IsNullOrEmpty(expectsMultipartFormData) ||
            !StringComparer.OrdinalIgnoreCase.Equals(
                expectsMultipartFormData,
                MULTIPART_FORM_DATA) ||
            string.IsNullOrEmpty(expectsBoundary) ||
            !expectsBoundary.StartsWith(BOUNDARY_EQ,
                StringComparison.OrdinalIgnoreCase) ||
            (expectsBoundary.Length - BOUNDARY_EQ.Length) < 3 ||
            contentLength < 1)
        {
            state = PreValidationState.BadStart;
            return false;
        }

        if (!SizeGovernor.ValidateContentLength(
                selectedUploadBox.TargetDirectory, contentLength))
        {
            state = PreValidationState.TooBig;
            return false;
        }

        var boundaryString = expectsBoundary.Substring(BOUNDARY_EQ.Length);
        if (!selectedUploadBox.TryInitiateUploading(contentLength, boundaryString))
        {
            state = PreValidationState.AlreadyUploading;
            return false;
        }

        Log.Post(ValidatedUpload,
            selectedUploadBox.UploadBoxId, selectedUploadBox.TargetDirectory,
            selectedUploadBox.ContentLengthAnnounced,
            boundaryString, selectedUploadBox.TempFile);
        state = PreValidationState.NoFaults;
        return true;
    }

    private static readonly Log UploadFailure = Log.Fail(
        "Failed to finish upload {guid} to {target} due to {exception}");

    public bool TryReceiveUpload(Stream inputStream, UploadBox selectedUploadBox)
    {
        try
        {
            using (var receiveParser = ParserFactory.Create(
                       selectedUploadBox.BoundaryBytes,
                       selectedUploadBox.TargetDirectory,
                       selectedUploadBox.AcceptedFields))
            {
                return ReceiveLoop(inputStream, receiveParser, selectedUploadBox);
            }
        }
        catch (Exception ex)
        {
            Log.Post(UploadFailure, selectedUploadBox.UploadBoxId,
                selectedUploadBox.TargetDirectory, ex);
            selectedUploadBox.TrySetStopMessage(
                "Failed");
            return false;
        }
    }

    private static readonly Log
        MainReceive = Log.Tech(
            "Main receive routine: {part}/{cum} of {total} at {rate}"),
        ReceiveStop = Log.Warn(
            "Receive upload stop likely due to too much data"),
        AuxReceive = Log.Tech(
            "Trying to top up; {part}/{cum} of {total} at {rate}");

    private bool ReceiveLoop(Stream inputStream, IMultipartUploadParser receiveParser,
        UploadBox selectedUploadBox)
    {
        byte[] buffer;
        int totalReceiveCount = 0;
        var dataRate = new DataRateCollection(7);

        for (int readCount = inputStream.Read(
                 buffer = receiveParser.PopForReceiving(), 0, buffer.Length);
             readCount > 0;
             readCount = inputStream.Read(
                 buffer = receiveParser.PopForReceiving(), 0, buffer.Length))
        {
            // we report received size and data rate here
            // this might fail if ie. there was too much data
            // or some other cancellation condition was met.
            totalReceiveCount += readCount;
            if (!selectedUploadBox.TryUpdateProgress(
                    totalReceiveCount, dataRate.Add(totalReceiveCount)))
            {
                Log.Post(ReceiveStop);
                selectedUploadBox.TrySetStopMessage("Cancelled");
                return false;
            }

            Log.Post(MainReceive, readCount, totalReceiveCount,
                selectedUploadBox.ContentLengthAnnounced,
                selectedUploadBox.UploadBytesPerSecond);

            // for best boundary finding at the receive parser,
            // we would like to hand in the fullest possible 
            // receive buffer. no wimpy 20 byte stuff.
            // Stream.Read doesn't guarantee that, but we can
            // Read some more.
            int fillUpCount = readCount;
            while (readCount > 0 && fillUpCount < buffer.Length)
            {
                readCount = inputStream.Read(
                    buffer, fillUpCount, buffer.Length - fillUpCount);
                fillUpCount += readCount;

                // we report received size and data rate again, here.
                // good for resolution and early failure when the
                // client is giving is too much.
                totalReceiveCount += readCount;
                if (!selectedUploadBox.TryUpdateProgress(
                        totalReceiveCount, dataRate.Add(totalReceiveCount)))
                {
                    selectedUploadBox.TrySetStopMessage("Cancelled");
                    return false;
                }

                Log.Post(AuxReceive, readCount, totalReceiveCount,
                    selectedUploadBox.ContentLengthAnnounced,
                    selectedUploadBox.UploadBytesPerSecond);
            }

            receiveParser.PushForProcessing(buffer, fillUpCount);

            // when trying to fill up we might have read an 
            // eof. then we shouldn't read again and skip
            // to finish
            if (readCount == 0) break;
        }

        receiveParser.Finish(selectedUploadBox.Values, selectedUploadBox.Faults);
        selectedUploadBox.TrySetStopMessage(string.Format(
            "Done with {0} errors", selectedUploadBox.Faults.Count));
        return true;
    }
}