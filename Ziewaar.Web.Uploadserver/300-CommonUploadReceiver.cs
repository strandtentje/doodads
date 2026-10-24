namespace Ziewaar.Web.Uploadserver;

public class CommonUploadReceiver : IUploadReceiver
{
    private readonly IUploadSizeGovernor SizeGovernor;
    private readonly IUploadValidator Validator;
    private readonly ITransferRateConstraints TransferRate;

    public CommonUploadReceiver(IUploadSizeGovernor sizeGovernor, IUploadValidator validator, ITransferRateConstraints transferRate)
    {
        this.SizeGovernor = sizeGovernor;
        this.Validator = validator;
        this.TransferRate = transferRate;
    }

    private static readonly Log
        ValidatedUpload = Log.Tech(Department.Upload,
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
            selectedUploadBox.TrySetStopMessage("File too big");
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
            return ReceiveLoop(inputStream, selectedUploadBox);
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
        MainReceive = Log.Tech(Department.Upload,
            "Main receive routine: {part}/{cum} of {total} at {rate}"),
        ReceiveStop = Log.Warn(
            "Receive upload stop likely due to too much data"),
        AuxReceive = Log.Tech(Department.Upload,
            "Trying to top up; {part}/{cum} of {total} at {rate}"),
        ExceptionBrokeLoop = Log.Fail(
            "Exception {broke} upload receive loop");

    private bool ReceiveLoop(Stream inputStream, UploadBox selectedUploadBox)
    {
        byte[] buffer;
        int totalReceiveCount = 0;
        var dataRate = new DataRateCollection(7);
        try
        {
            using var ingestor = new CommonMultipartComponentIngestor(selectedUploadBox.BoundaryBytes,
                selectedUploadBox.TargetDirectory, selectedUploadBox.AcceptedFields, Validator);
            var worker = new CommonMultipartUploadParserWorker(new VirtualInspectionBuffer(), ingestor);
            int burst = TransferRate.Burst;

            try
            {
                byte[] workingReceiveBuffer = new byte[TransferRate.BufferSize];
                int readCount, allocatedBuffers = 1;

                for (readCount = inputStream.Read(
                         workingReceiveBuffer, 0, workingReceiveBuffer.Length);
                     readCount > 0;
                     readCount = inputStream.Read(
                         workingReceiveBuffer, 0, workingReceiveBuffer.Length))
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
                    while (readCount > 0 && fillUpCount < workingReceiveBuffer.Length)
                    {
                        readCount = inputStream.Read(
                            workingReceiveBuffer, fillUpCount, workingReceiveBuffer.Length - fillUpCount);
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

                    // the buffer we supplied may not have been to the liking of 
                    // the parser, in which case we should stop immediately. it could also mean 
                    // an eof has shifted into view, but that's likely happening below.
                    if (!worker.PushBuffer(workingReceiveBuffer, fillUpCount, out workingReceiveBuffer))
                        break;

                    // when trying to fill up we might have read an eof. then we shouldn't read again and skip
                    // to finish. 
                    if (readCount == 0)
                        break;

                    if (workingReceiveBuffer.Length != TransferRate.BufferSize)
                    {
                        allocatedBuffers++;
                        if (allocatedBuffers > 3) // this assertion should normally not be hit.
                            throw new InsufficientMemoryException(
                                "multipart inspection not returning used buffers properly");
                        workingReceiveBuffer = new byte[TransferRate.BufferSize];
                    }

                    if (burst > 0)
                    {
                        burst--;
                    } else
                    {
                        burst = this.TransferRate.Burst;
                        if (this.TransferRate.Sleep >= 0)
                            Thread.Sleep(this.TransferRate.Sleep);
                    }

                }

                // we left the loop due to an eof, and our inspector has two buffer. let's super flush those out
                // entirely, to be sure.
                if (readCount == 0 && worker.PushBuffer(Array.Empty<byte>(), 0, out workingReceiveBuffer))
                {
                    if (worker.PushBuffer(Array.Empty<byte>(), 0, out workingReceiveBuffer))
                        throw new InvalidOperationException( // this assertion should also never occur. 
                            "inspector claims to be not done even though its entirely flushed");
                }
            }
            catch(Exception ex)
            {
                selectedUploadBox.Faults.Add("Transmission Fault");
            }
            finally
            {
                ingestor.Finish(selectedUploadBox.Values, selectedUploadBox.Faults);
            }

            if (selectedUploadBox.Faults.Count == 0)
            {
                selectedUploadBox.TrySetStopMessage("Upload done");
                return true;
            }
            else
            {
                StringBuilder failureMessage = new StringBuilder();
                var firstFailure = selectedUploadBox.Faults.FirstOrDefault() ?? "Unknown Failure";
                failureMessage.Append("Upload failed; ").AppendLine(firstFailure);
                if (selectedUploadBox.Faults.Count > 1)
                {
                    failureMessage.AppendFormat("And {0} more faults occurred", selectedUploadBox.Faults.Count - 1)
                        .AppendLine();
                }

                selectedUploadBox.TrySetStopMessage(failureMessage.ToString());
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Post(ExceptionBrokeLoop, ex);
            StringBuilder failureMessage = new StringBuilder();
            var firstFailure = selectedUploadBox.Faults.FirstOrDefault() ?? "Unknown Failure";
            failureMessage.Append("Upload failed; ").AppendLine(firstFailure);
            if (selectedUploadBox.Faults.Count > 1)
            {
                failureMessage.AppendFormat("And {0} more faults occurred", selectedUploadBox.Faults.Count - 1)
                    .AppendLine();
            }

            selectedUploadBox.TrySetStopMessage(failureMessage.ToString());

            return false;
        }
    }
}