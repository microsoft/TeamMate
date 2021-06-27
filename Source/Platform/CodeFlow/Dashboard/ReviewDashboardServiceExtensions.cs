using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Platform.CodeFlow.Dashboard
{
    public static class ReviewDashboardServiceExtensions
    {
        public static int GetTotalPages(this QueryReviewSummariesResult firstPage)
        {
            Assert.ParamIsNotNull(firstPage, "result");

            if (firstPage.TotalResults > 0 && firstPage.Reviews != null && firstPage.Reviews.Length > 0 && firstPage.TotalResults > firstPage.Reviews.Length)
            {
                return (int)Math.Ceiling(firstPage.TotalResults / (double)firstPage.Reviews.Length);
            }

            return 1;
        }

        public static QueryReviewSummariesResult QueryAllReviewSummaries(this IReviewDashboardService service, CodeReviewQuery query)
        {
            Assert.ParamIsNotNull(service, "service");
            Assert.ParamIsNotNull(query, "query");

            return QueryAllReviewSummariesAsync(service, query).Result;
        }

        public static async Task<QueryReviewSummariesResult> QueryAllReviewSummariesAsync(this IReviewDashboardService service, CodeReviewQuery query)
        {
            // TODO: THis is not truly fully async. We should be using Task.Run() to fork out work to another thread.
            // Right now, some of the work is happening in the same synchronization context, which is unnecessary.
            Assert.ParamIsNotNull(service, "service");
            Assert.ParamIsNotNull(query, "query");

            DateTime start = DateTime.Now;
            var firstResult = await service.QueryReviewSummariesAsync(query);

            int totalPages = firstResult.GetTotalPages();
            if (totalPages > 1)
            {
                List<Task<QueryReviewSummariesResult>> moreResultTasks = new List<Task<QueryReviewSummariesResult>>();

                for (int i = 1; i < totalPages; i++)
                {
                    var nextQuery = SerializationUtilities.Clone(query);
                    nextQuery.PageIndex = i;

                    var nextResult = service.QueryReviewSummariesAsync(nextQuery);
                    moreResultTasks.Add(nextResult);
                }

                var moreResults = await Task.WhenAll(moreResultTasks);

                TimeSpan queryDuration = DateTime.Now - start;
                List<QueryReviewSummariesResult> allResults = new List<QueryReviewSummariesResult>();
                allResults.Add(firstResult);
                allResults.AddRange(moreResults);

                QueryReviewSummariesResult aggregateResult = new QueryReviewSummariesResult();
                aggregateResult.QueryDuration = queryDuration;
                aggregateResult.Reviews = allResults.Select(rs => rs.Reviews).Where(r => r != null).SelectMany(r => r).ToArray();
                aggregateResult.TotalResults = aggregateResult.Reviews.Length;
                aggregateResult.ExtensionData = firstResult.ExtensionData;
                return aggregateResult;
            }

            return firstResult;
        }

        public static bool HasActioned(this Reviewer reviewer)
        {
            return reviewer.Status.IsActioned();
        }

        public static bool IsParticipating(this Reviewer reviewer)
        {
            return reviewer.Status.IsParticipating();
        }

        public static bool IsActioned(this ReviewerStatus status)
        {
            return status == ReviewerStatus.SignedOff || status == ReviewerStatus.Waiting || status == ReviewerStatus.Declined;
        }

        public static bool IsParticipating(this ReviewerStatus status)
        {
            return status == ReviewerStatus.Started || status == ReviewerStatus.Reviewing || status.IsActioned();
        }

        public static bool ActionedAfter(this CodeReviewSummary review, DateTime date)
        {
            return review.Reviewers.Any(reviewer => reviewer.HasActioned() && reviewer.LastUpdatedOn > date);
        }

        public static bool ActionedBy(this CodeReviewSummary review, string alias)
        {
            var reviewer = review.GetReviewer(alias);
            return (reviewer != null) ? reviewer.HasActioned() : false;
        }

        public static bool ActionedByAfter(this CodeReviewSummary review, string alias, DateTime date)
        {
            var reviewer = review.GetReviewer(alias);
            return (reviewer != null) ? (reviewer.HasActioned() && reviewer.LastUpdatedOn > date) : false;
        }

        public static IEnumerable<Reviewer> ActionsAfter(this CodeReviewSummary review, Reviewer aReview)
        {
            return review.Reviewers.Where(reviewer => reviewer != aReview && reviewer.HasActioned() && reviewer.LastUpdatedOn > aReview.LastUpdatedOn);
        }

        public static ReviewerStatus? GetStatus(this CodeReviewSummary review, string alias)
        {
            var reviewer = review.GetReviewer(alias);
            return (reviewer != null) ? reviewer.Status : (ReviewerStatus?)null;
        }

        public static Reviewer GetReviewer(this CodeReviewSummary review, string alias)
        {
            return review.Reviewers.FirstOrDefault(reviewer => Is(reviewer, alias));
        }

        public static bool IsActive(this CodeReviewSummary summary)
        {
            return (summary.Status == CodeReviewStatus.Created || summary.Status == CodeReviewStatus.Active);
        }

        public static CodeFlowReviewReference GetReference(this CodeReviewSummary summary)
        {
            return new CodeFlowReviewReference(summary.Key);
        }

        public static int CountReviewerStatus(this CodeReviewSummary review, ReviewerStatus status)
        {
            return review.Reviewers.Count(reviewer => reviewer.Status == status);
        }

        public static bool HasReviewerStatus(this CodeReviewSummary review, ReviewerStatus status)
        {
            return review.Reviewers.Any(reviewer => reviewer.Status == status);
        }

        public static bool IsRequired(this CodeReviewSummary review, string alias)
        {
            var reviewer = review.GetReviewer(alias);
            return (reviewer != null) ? reviewer.Required : false;
        }

        public static bool Is(this Reviewer reviewer, string alias)
        {
            return AliasesAreEqual(reviewer.Name, alias);
        }

        public static bool Is(this Author author, string alias)
        {
            return AliasesAreEqual(author.Name, alias);
        }

        public static bool IsMe(this Reviewer reviewer)
        {
            var myAccount = CodeFlowContext.MyCodeFlowAccount;
            return myAccount != null && reviewer.Is(myAccount);
        }

        public static bool IsMe(this Author author)
        {
            var myAccount = CodeFlowContext.MyCodeFlowAccount;
            return myAccount != null && author.Is(myAccount);
        }

        private static bool AliasesAreEqual(string alias1, string alias2)
        {
            return String.Equals(alias1, alias2, StringComparison.OrdinalIgnoreCase);
        }

        public static ReviewerStatus MyFeedbackStatus(this CodeReviewSummary review)
        {
            var reviewer = review.MyReview();
            return (reviewer != null) ? reviewer.Status : ReviewerStatus.NotStarted;
        }

        public static Reviewer MyReview(this CodeReviewSummary review)
        {
            return review.Reviewers.FirstOrDefault(r => r.IsMe());
        }

        public static DateTime? SignedOffOn(this CodeReviewSummary summary)
        {
            var signedOff = summary.Reviewers.Where(r => r.Status == ReviewerStatus.SignedOff).ToArray();
            return (signedOff.Any()) ? signedOff.Min(r => r.LastUpdatedOn) : (DateTime?)null;
        }

        public static Uri GetWebViewUri(this CodeReviewSummary review)
        {
            // Move to extension method
            return CodeFlowUriBuilder.WebView(review.Key);
        }

        public static Uri GetLaunchClientUri(this CodeReviewSummary review)
        {
            return CodeFlowUriBuilder.LaunchClient(review.Key);
        }

        public static Uri GetLaunchVisualStudioUri(this CodeReviewSummary review)
        {
            return CodeFlowUriBuilder.LaunchVisualStudio(review.Key);
        }

        public static string GetFullTitle(this CodeReviewSummary review)
        {
            return String.Format("CodeFlow Review: {0}", review.Name);
        }

        public static string GetCreatedChangeDescription(this CodeReviewSummary summary)
        {
            return String.Format("{0} created a code review", summary.Author.DisplayName);
        }

        public static string GetChangeDescription(this CodeReviewSummary summary)
        {
            var author = summary.Author;

            if (author.Status == AuthorStatus.Completed)
            {
                return String.Format("{0} completed a code review", author.DisplayName);
            }

            Reviewer lastReviewer = summary.GetLastReviewerWhoActioned();
            var lastAuthorUpdate = summary.GetLastAuthorUpdate();

            // TODO: This logic is quite ugly, refactor
            if (lastReviewer != null && lastReviewer.LastUpdatedOn > lastAuthorUpdate)
            {
                string authorDisplayName = (author.IsMe()) ? "your" : String.Format("{0}'s", author.DisplayName);
                string reviewerDisplayName = lastReviewer.DisplayName;

                // Prefer reviewer action
                var status = lastReviewer.Status;
                if (status == ReviewerStatus.Declined)
                {
                    return String.Format("{0} declined {1} code review", reviewerDisplayName, authorDisplayName);
                }
                else if (status == ReviewerStatus.SignedOff)
                {
                    return String.Format("{0} signed off {1} code review", reviewerDisplayName, authorDisplayName);
                }
                else if (status == ReviewerStatus.Waiting)
                {
                    return String.Format("{0} is waiting on {1} code review", reviewerDisplayName, authorDisplayName);
                }
                else
                {
                    return String.Format("{0} gave feedback on {1} code review", reviewerDisplayName, authorDisplayName);
                }
            }
            else
            {
                if (!summary.HasLastUpdatedOn() || !author.HasLastUpdatedOn())
                {
                    return summary.GetCreatedChangeDescription();
                }
                else
                {
                    return String.Format("{0} updated a code review", author.DisplayName);
                }
            }
        }

        public static Reviewer GetLastReviewerWhoActioned(this CodeReviewSummary summary)
        {
            Reviewer lastReviewer = summary.Reviewers.Where(r => r.Status != ReviewerStatus.InviteOnly && r.Status != ReviewerStatus.NotStarted && r.HasLastUpdatedOn())
                .OrderByDescending(r => r.LastUpdatedOn).FirstOrDefault();

            return lastReviewer;
        }

        public static bool HasLastUpdatedOn(this Author author)
        {
            return IsValidDate(author.LastUpdatedOn);
        }

        public static bool HasLastUpdatedOn(this Reviewer reviewer)
        {
            return IsValidDate(reviewer.LastUpdatedOn);
        }

        public static bool HasLastUpdatedOn(this CodeReviewSummary summary)
        {
            return IsValidDate(summary.LastUpdatedOn);
        }

        private static bool IsValidDate(DateTime date)
        {
            return date != DateTime.MinValue;
        }

        public static LastReviewChange GetLastChange(this CodeReviewSummary summary)
        {
            DateTime lastAuthorUpdate = summary.GetLastAuthorUpdate();
            Reviewer lastReviewer = summary.GetLastReviewerWhoActioned();

            if (lastReviewer == null || lastReviewer.LastUpdatedOn < lastAuthorUpdate)
            {
                return new LastReviewChange(summary.Author, lastAuthorUpdate);
            }
            else
            {
                return new LastReviewChange(lastReviewer, lastReviewer.LastUpdatedOn);
            }
        }

        public static DateTime GetLastAuthorUpdate(this CodeReviewSummary summary)
        {
            Author author = summary.Author;
            DateTime authorDate = (author.HasLastUpdatedOn()) ? Max(author.LastUpdatedOn, summary.CreatedOn) : summary.CreatedOn;
            return authorDate;
        }

        private static DateTime Max(DateTime d1, DateTime d2)
        {
            return (d1 > d2) ? d1 : d2;
        }

        public static bool IsSignedOffOrWaiting(this CodeReviewSummary summary)
        {
            return summary.Reviewers.Any(r => r.Status == ReviewerStatus.SignedOff || r.Status == ReviewerStatus.Waiting);
        }

        public static bool NotReviewedByMeOrUpdatedAfterReview(this CodeReviewSummary summary)
        {
            var myReview = summary.MyReview();

            // If I have not reviewed yet, or I reviewed as "Waiting" and the author made updates...
            return (myReview == null || !myReview.Status.IsActioned()
                || (myReview.Status == ReviewerStatus.Waiting && myReview.LastUpdatedOn < summary.Author.LastUpdatedOn));
        }
    }

    public class LastReviewChange
    {
        public LastReviewChange(Author author, DateTime changeDate)
        {
            this.Author = author;
            this.ChangeDate = changeDate;
        }

        public LastReviewChange(Reviewer reviewer, DateTime changeDate)
        {
            this.Reviewer = reviewer;
            this.ChangeDate = changeDate;
        }

        public Author Author { get; private set; }
        public Reviewer Reviewer { get; private set; }
        public DateTime ChangeDate { get; private set; }

        public bool IsMe()
        {
            return (Author != null) ? Author.IsMe() : Reviewer.IsMe();
        }
    }
}
