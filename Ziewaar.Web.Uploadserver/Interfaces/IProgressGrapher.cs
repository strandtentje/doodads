namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IProgressGrapher
{
    void Run(UploadBox selectedUploadBox, IHttpLifecycleWrapper lifecycle);
}