#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined
{
#pragma warning disable 67
    public class ContentTypeProbingInteraction(IInteraction parent, ContentTypeAggregation aggregation)
        : IProbeContentTypeInteraction
    {
        public IInteraction Stack => parent;
        public object Register => parent.Register;
        public IReadOnlyDictionary<string, object> Memory => parent.Memory;
        private string? CurrentContentType;
        public void PushContentType(string contentType)
        {
            switch (aggregation)
            {
                case ContentTypeAggregation.First when CurrentContentType is null:
                case ContentTypeAggregation.Last:
                case ContentTypeAggregation.Strict when CurrentContentType is null:
                case ContentTypeAggregation.Strict when CurrentContentType is not null &&
                                                        ContentTypeMatcher.IsSubset(contentType, CurrentContentType):
                    CurrentContentType = contentType;
                    break;
                case ContentTypeAggregation.Strict when CurrentContentType is not null &&
                                                        !ContentTypeMatcher.IsSubset(CurrentContentType, contentType):
                    throw new ContentTypeMismatchException(CurrentContentType, contentType);
                default:
                    break;
            }
        }
        public bool TryGetContentType(out string contentType)
        {
            contentType = CurrentContentType ?? string.Empty;
            contentType = (contentType.Contains('*'), contentType.StartsWith("text")) switch
            {
                (true, true) => "text/plain",
                (true, false) => "application/octet-stream",
                _ => contentType
            };
            return !string.IsNullOrWhiteSpace(contentType);
        }
    }
}