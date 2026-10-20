namespace Ziewaar.Web.Uploadserver.Pages;

public class StaticPages
{
    public static readonly StaticPages Instance = new StaticPages();
    private string
        Pg100BadMethodFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/100-BadMethod.html"),
        Pg110BadGetRequestFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/110-BadGetRequest.html"),
        Pg120UploaderGoneFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/120-UploaderGone.html"),
        Pg130BadGetFunctionFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/130-BadGetFunction.html"),
        Pg140BadUploadStartFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/140-BadUploadStart.html"),
        Pg150ConflictingUploadFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/150-ConflictingUpload.html"),
        Pg160UploadTooBigFile =
            Path.Combine(SpecialPaths.AssemblyDirectory, 
                "Html/160-UploadTooBig.html")
        ;
    public readonly byte[] Pg100BadMethod, Pg110BadGetRequest, Pg120UploaderGone,
        Pg130BadGetFunction, Pg140BadUploadStart, Pg150ConflictingUpload,
        Pg160UploadTooBig;
    private StaticPages()
    {
        Pg100BadMethod = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg100BadMethodFile));
        Pg110BadGetRequest = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg110BadGetRequestFile));
        Pg120UploaderGone = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg120UploaderGoneFile));
        Pg130BadGetFunction = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg130BadGetFunctionFile));
        Pg140BadUploadStart = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg140BadUploadStartFile));
        Pg150ConflictingUpload = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg150ConflictingUploadFile));
        Pg160UploadTooBig = Encoding.UTF8.GetBytes(
            File.ReadAllText(Pg160UploadTooBigFile));
    }
}