// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Tools.TeamMate.Services
{
    public class SearchService
    {
        private const int MaxItemsFromTfs = 25;

        public const string TfsSource = "TFS";

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        public async Task<SearchResults> LocalSearch(SearchExpression searchExpression, CancellationToken cancellationToken)
        {
            var queries = GetLocalQueries();

            await ChaosMonkey.ChaosAsync(ChaosScenarios.LocalSearch);
            return await Task.Run(() => DoLocalSearch(searchExpression, queries));
        }

        private static SearchResults DoLocalSearch(SearchExpression searchExpression, IEnumerable<QueryViewModelBase> queries)
        {
            List<SearchResult> resultList = new List<SearchResult>();

            if (!searchExpression.IsEmpty)
            {
                foreach (var query in queries)
                {
                    WorkItemQueryViewModel workItemQuery = query as WorkItemQueryViewModel;
                    CodeFlowQueryViewModel codeFlowQuery = query as CodeFlowQueryViewModel;

                    var source = new SearchResultSource(query);

                    if (workItemQuery != null && workItemQuery.WorkItems != null)
                    {
                        var matches = workItemQuery.WorkItems.Where(item => searchExpression.Matches(item));
                        resultList.AddRange(matches.Select(match => new SearchResult(match, source)));
                    }
                    else if (codeFlowQuery != null && codeFlowQuery.Reviews != null)
                    {
                        var matches = codeFlowQuery.Reviews.Where(item => searchExpression.Matches(item));
                        resultList.AddRange(matches.Select(match => new SearchResult(match, source)));
                    }
                }
            }

            var results = new SearchResults(resultList);
            return results;
        }

        private ICollection<QueryViewModelBase> GetLocalQueries()
        {
            // TODO: This should be cleaner, out of a directly accessible view model, or session
            return this.WindowService.MainWindow.ViewModel.HomePage.TileCollection.Tiles.Select(t => t.Query).ToArray();
        }

        public async Task<SearchResults> TfsSearch(SearchExpression searchExpression, CancellationToken cancellationToken)
        {
            var pc = this.SessionService.Session.ProjectContext;

            var query = new WorkItemQuery
            {
                ProjectName = pc.ProjectName,
                Wiql = searchExpression.ToTfsWiql(),
                RequiredFields = pc.RequiredWorkItemFieldNames,
                MaxItemsToFetch = MaxItemsFromTfs
            };

            await ChaosMonkey.ChaosAsync(ChaosScenarios.TfsSearch);

            var result = await pc.WorkItemTrackingClient.QueryAsync(query);
            var workItems = result.WorkItems.Select(wi => CreateWorkItemViewModel(wi));
            var searchResults = workItems.Select(wi => new SearchResult(wi, SearchResultSource.Tfs)).ToArray();
            return new SearchResults(searchResults, result.QueryResult.WorkItems.Count());
        }

        private WorkItemRowViewModel CreateWorkItemViewModel(WorkItem workItem)
        {
            WorkItemRowViewModel viewModel = ViewModelFactory.Create<WorkItemRowViewModel>();
            viewModel.OverrideIsRead = true;
            viewModel.WorkItem = workItem;
            return viewModel;
        }
    }

    public class SearchResults
    {
        public static readonly SearchResults Empty = new SearchResults(new SearchResult[0]);

        public SearchResults(IList<SearchResult> results)
            : this(results, results.Count)
        {

        }

        public SearchResults(IList<SearchResult> results, int totalCount)
        {
            this.Results = results;
            this.TotalCount = totalCount;
        }

        public IList<SearchResult> Results { get; private set; }
        public int TotalCount { get; private set; }
    }

    public class SearchResult
    {
        public SearchResult(object result, SearchResultSource source)
        {
            this.Item = result;
            this.Source = source;
        }

        public object Item { get; set; }
        public SearchResultSource Source { get; set; }
    }

    public class SearchResultSource : IComparable
    {
        private QueryViewModelBase source;
        private string sourceName;

        public static readonly SearchResultSource Tfs = new SearchResultSource("TFS");

        public SearchResultSource(QueryViewModelBase source)
        {
            this.source = source;
        }

        private SearchResultSource(string sourceName)
        {
            this.sourceName = sourceName;
        }

        public bool IsLocal
        {
            get { return this != Tfs; }
        }

        public int CompareTo(object obj)
        {
            SearchResultSource that = (SearchResultSource)obj;

            int result = this.GetRanking() - that.GetRanking();
            if (result == 0 && this.source != null && that.source != null)
            {
                result = this.source.Name.CompareTo(that.source.Name);
            }

            return result;
        }

        private int GetRanking()
        {
            if (this == Tfs)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return (source != null) ? source.Name : sourceName;
        }
    }
}
