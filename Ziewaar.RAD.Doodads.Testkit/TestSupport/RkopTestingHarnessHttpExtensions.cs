using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Testkit;
public static class RkopTestingHarnessHttpExtensions
{
    public static HttpTestingHarness HttpRequest(
        this RkopTestingHarness harness,
        HttpMethod method,
        string[] urlParts,
        string requestBodyMimeType,
        long requestBodyLength,
        Stream requestBody,
        string[] accepts)
    {
        var rootInteraction = new RootInteraction("", new SortedList<string, object>());
        var httpHead = new TestingHttpHeadInteraction(rootInteraction, method, string.Concat(urlParts));
        var httpReq =
            new TestingHttpRequestInteraction(httpHead, new CookieCollection(), requestBodyMimeType, requestBodyLength,
                requestBody);
        var httpRes =
            new TestingHttpResponseInteraction(httpReq, accepts);

        return new HttpTestingHarness(
            Task.Run(() =>
            {
                try
                {
                    return harness.Run(httpRes);
                }
                finally
                {
                    httpRes.Pipe.Writer.Complete();
                }
            }),
            httpRes.Pipe.Reader.AsStream(leaveOpen: false),
            httpRes);
    }
    public static HttpTestingHarness GetRequest(
        this RkopTestingHarness harness,
        string path,
        IReadOnlyDictionary<string, string> queryParams) =>
        harness.HttpRequest(
            HttpMethod.Get, [path, "?", queryParams.ToQueryString()], "", 0, new MemoryStream(0), ["*/*"]);
    public static HttpTestingHarness PostWwwForm(
        this RkopTestingHarness harness,
        string path,
        IReadOnlyDictionary<string, string> queryParams,
        IReadOnlyDictionary<string, string> formBody)
    {
        var urlEncoded = formBody.ToQueryString();
        var bytes = Encoding.ASCII.GetBytes(urlEncoded);
        return harness.HttpRequest(
            HttpMethod.Post, [path, "?", queryParams.ToQueryString()],
            "application/x-www-form-urlencoded", bytes.Length, new MemoryStream(bytes), ["*/*"]);
    }
    public static HttpTestingHarness PostMultipartForm(
        this RkopTestingHarness harness,
        string path,
        IReadOnlyDictionary<string, string> queryParams,
        IList<MultipartMember> formBody)
    {
        var tempMulti = Path.GetTempFileName();
        string fullContentType;
        using (var multiPart = new MultipartFormDataContent($"--------{Guid.NewGuid()}"))
        {
            foreach (var item in formBody)
            {
                if (item.ContentType != null && item.File != null)
                {
                    using var fis = item.File.OpenRead();
                    var sc = new StreamContent(fis);
                    sc.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType);
                    multiPart.Add(sc, item.Name, item.TextOrFilename ?? item.File.Name);
                }
                else if (item.TextOrFilename != null)
                {
                    multiPart.Add(new StringContent(item.TextOrFilename), item.Name);
                }
                else
                {
                    throw new ArgumentException("Incomplete multipart member");
                }
            }

            using (var multiFile = File.OpenWrite(tempMulti))
            {
                fullContentType = multiPart.Headers.ContentType?.ToString()!;
                multiPart.CopyTo(multiFile, null, CancellationToken.None);
            }
        }

        var tempMultiInfo = new FileInfo(tempMulti);
        using (var x = tempMultiInfo.OpenRead())
        {
            return harness.HttpRequest(
                HttpMethod.Post, [path, "?", queryParams.ToQueryString()],
                fullContentType, tempMultiInfo.Length, x, ["*/*"]
            );
        }
    }
    public static IList<MultipartMember> AndText(this IList<MultipartMember> multi, string name, string value)
    {
        multi.Add(new(name, value, null, null));
        return multi;
    }
    public static IList<MultipartMember> AndFile(this IList<MultipartMember> multi, string name, string path, string ct)
    {
        var x = new FileInfo(path);
        if (!x.Exists) throw new IOException($"test file `{path}` doesnt exist");
        multi.Add(new(name, null, ct, x));
        return multi;
    }
}