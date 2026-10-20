namespace Ziewaar.Web.Uploadserver.Pages;

public class DynamicPages
{
    public static readonly DynamicPage 
        UploadForm = DynamicPage.For(
            Path.Combine(SpecialPaths.AssemblyDirectory, "Html/200-UploadPage.html")),
        ProgressPage = DynamicPage.For(
            Path.Combine(SpecialPaths.AssemblyDirectory, "Html/210-ProgressPage.html"));
}