namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class PartReader(Stream stream, byte[] boundary)
{
    private bool FirstPartSeen = false;
    public MultipartPayload? ReadNextPart()
    {
        if (!FirstPartSeen)
        {
            if (!stream.SkipToFirstBoundary(boundary))
                return null;
        }
        else
        {
            if (!stream.ExpectNextBoundary(boundary))
                return null;
        }
        FirstPartSeen = true;
        return new MultipartPayload(
            MultipartHeader.Parse(stream),
            new MultipartPartStream(stream, boundary));
    }
}