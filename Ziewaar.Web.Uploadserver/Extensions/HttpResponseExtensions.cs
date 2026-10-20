namespace Ziewaar.Web.Uploadserver.Extensions;

public static class HttpResponseExtensions
{
    public static void WriteUtf8Bytes(
        this HttpListenerResponse response,
        int code,
        params byte[][] data)
    {
        response.StatusCode = code;
        response.ContentLength64 = data.Sum(x => x.Length);
        response.ContentType = "text/html";
        response.ContentEncoding = Encoding.UTF8;
        foreach (var dataPart in data)
        {
            response.OutputStream.Write(dataPart, 0, dataPart.Length);
        }
    }
}