using Microsoft.VisualStudio.Services.WebApi;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi
{
    public static class ReferenceLinksExtensions
    {
        public static string HtmlLink(this ReferenceLinks links)
        {
            object link;
            if (links.Links.TryGetValue("html", out link))
            {
                ReferenceLink referenceLink = link as ReferenceLink;
                if (referenceLink != null)
                {
                    return referenceLink.Href;
                }
            }

            return null;
        }
    }
}
