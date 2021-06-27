// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation;
using Microsoft.Tools.TeamMate.Office.Outlook;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard;
using Microsoft.Tools.TeamMate.Platform.CodeFlow.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Microsoft.Tools.TeamMate.Platform.CodeFlow
{
    public class CodeFlowMailGenerator
    {
        public MailMessage GenerateEmail(CodeReviewSummary review)
        {
            MailMessage message = new MailMessage();

            if (!!review.Author.IsMe())
            {
                message.To.Add(review.Author.EmailAddress);
            }

            var reviewers = review.Reviewers.Where(r => !r.IsMe());
            var actionedEmails = reviewers.Where(r => r.IsParticipating()).Select(r => r.EmailAddress);
            var unactionedEmails = reviewers.Where(r => !r.IsParticipating()).Select(r => r.EmailAddress);

            foreach (var emailAddress in actionedEmails)
            {
                message.CC.Add(emailAddress);
            }

            StringBuilder htmlBody = new StringBuilder();
            htmlBody.AppendFormat(
                @"<b>Open CodeFlow Review in: [<a href='{0}'>CodeFlow</a>]&nbsp;[<a href='{1}'>Browser</a>]&nbsp;[<a href='{2}'>Visual Studio</a>]</b>
                      <br/>&#8204;<br/>&#8204;",
                review.GetLaunchClientUri(), review.GetWebViewUri(), review.GetLaunchVisualStudioUri());

            message.Subject = review.GetFullTitle();
            message.HtmlBody = MailMessage.WrapHtmlInDefaultFont(htmlBody.ToString());
            return message;
        }

        public MailMessage GenerateCompleteReviewsEmail(IEnumerable<CodeReviewSummary> reviews)
        {
            XDocument document = CreateDocument(reviews);
            StringWriter writer = new StringWriter();
            Transform(document, CodeFlowResources.ReviewsStylesheet, writer);


            MailMessage message = new MailMessage();
            message.Subject = "Please complete your active CodeFlow Reviews";
            message.HtmlBody = writer.ToString();

            var distinctAuthorEmails = reviews.Select(r => r.Author.EmailAddress).Distinct();
            foreach (var emailAddress in distinctAuthorEmails)
            {
                message.To.Add(emailAddress);
            }

            return message;
        }

        private void Transform(XDocument doc, byte[] xslStyleSheet, TextWriter writer)
        {
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(XmlReader.Create(new MemoryStream(xslStyleSheet)));

            XsltArgumentList args = new XsltArgumentList();

            // Execute the transform and output the results to a writer.
            xslt.Transform(doc.CreateReader(), args, writer);
        }

        private XDocument CreateDocument(IEnumerable<CodeReviewSummary> reviews)
        {
            var reviewsByAuthor = reviews.GroupBy(r => r.Author.Name).OrderBy(rg => rg.First().Author.DisplayName);

            XDocument doc = new XDocument(
                new XElement("Reviews", reviewsByAuthor.Select(rg =>
                    new XElement("ReviewGroup",
                        new XElement("Author",
                            new XAttribute("Name", rg.First().Author.Name),
                            new XAttribute("DisplayName", rg.First().Author.DisplayName),
                            new XAttribute("EmailAddress", rg.First().Author.EmailAddress)
                        ),
                        new XElement("Reviews", rg.Select(r =>
                            new XElement("Review",
                                new XElement("Name", r.Name),
                                new XElement("Key", r.Key),
                                new XElement("Status", r.Status),
                                new XElement("CreatedOn", r.CreatedOn),
                                new XElement("FriendlyCreatedOn", r.CreatedOn.ToFriendlyElapsedTimeStringWithAgo()),
                                new XElement("LastUpdatedOn", r.LastUpdatedOn),
                                new XElement("FriendlyLastUpdatedOn", r.LastUpdatedOn.ToFriendlyElapsedTimeStringWithAgo()),
                                new XElement("IterationCount", r.IterationCount),
                                new XElement("CodeFlowUrl", r.GetLaunchClientUri()),
                                new XElement("VsClientUrl", r.GetLaunchVisualStudioUri()),
                                new XElement("WebUrl", r.GetWebViewUri()),
                                new XElement("Author",
                                    new XAttribute("Name", r.Author.Name),
                                    new XAttribute("DisplayName", r.Author.DisplayName),
                                    new XAttribute("EmailAddress", r.Author.EmailAddress),
                                    new XAttribute("Status", r.Author.Status),
                                    new XAttribute("LastUpdatedOn", r.Author.LastUpdatedOn)
                                ),
                                new XElement("Reviewers", r.Reviewers.Select(rv =>
                                    new XElement("Reviewer",
                                        new XAttribute("Name", rv.Name),
                                        new XAttribute("DisplayName", rv.DisplayName),
                                        new XAttribute("EmailAddress", rv.EmailAddress),
                                        new XAttribute("LastUpdatedOn", rv.LastUpdatedOn),
                                        new XAttribute("Status", rv.Status),
                                        new XAttribute("Required", rv.Required)
                                    )
                                ))
                            )
                        ))
                    ))
                )
            );

            return doc;
        }
    }
}
