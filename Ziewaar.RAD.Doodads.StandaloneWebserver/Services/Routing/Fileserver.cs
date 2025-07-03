using System;
using System.Collections.Generic;
using System.Text;
using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO
{
    public class Fileserver : IService
    {
        private readonly UpdatingPrimaryValue DirectoryToServe = new UpdatingPrimaryValue();
        private string? CurrentDirectory;

        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            if ((constants, DirectoryToServe).IsRereadRequired(out FileInWorkingDirectory? fiw) && fiw != null)
            {
                this.CurrentDirectory = fiw.ToString();
            } else if ((constants, DirectoryToServe).IsRereadRequired(out string? newDirectory) && !string.IsNullOrWhiteSpace(newDirectory))
            {
                this.CurrentDirectory = newDirectory;
            }
            if (string.IsNullOrWhiteSpace(this.CurrentDirectory))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "No newDirectory specified for fileserver."));
                return;
            }
            if (interaction is not RelativeRouteInteraction routeEval || routeEval.HttpHead is not HttpHeadInteraction head)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "Routing information is not valid for file serving."));
                return;
            }

            var components = routeEval.Remaining.ToArray();
            var filePath = System.IO.Path.Combine(this.CurrentDirectory, string.Join(System.IO.Path.DirectorySeparatorChar, components));
            if (System.IO.File.Exists(filePath))
            {
                var fileContent = System.IO.File.ReadAllBytes(filePath);
                head.Context.Response.ContentType = "application/octet-stream"; // Default content type
                head.Context.Response.ContentLength64 = fileContent.Length;
                head.Context.Response.OutputStream.Write(fileContent, 0, fileContent.Length);
                OnThen?.Invoke(this, interaction);
            }
            else
            {
                OnElse?.Invoke(this, interaction);
            }
        }
        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
