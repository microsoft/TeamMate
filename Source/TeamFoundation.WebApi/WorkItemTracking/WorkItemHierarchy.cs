using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public class WorkItemHierarchy
    {
        public bool IsLinkQuery { get; private set; }

        public bool IsTreeQuery { get; private set; }

        public int NodeCount { get; private set; }

        public ICollection<WorkItemHierarchyNode> Roots { get; } = new List<WorkItemHierarchyNode>();

        public void Visit(Action<WorkItemHierarchyNode> action)
        {
            foreach (var root in this.Roots)
            {
                root.Visit(action);
            }
        }

        public static WorkItemHierarchy Create(ICollection<WorkItem> workItems, ICollection<WorkItemLink> linkInfo, bool isTreeQuery)
        {
            WorkItemHierarchy hierarchy = new WorkItemHierarchy();
            hierarchy.NodeCount = linkInfo.Count;
            hierarchy.IsTreeQuery = isTreeQuery;
            hierarchy.IsLinkQuery = !isTreeQuery;

            var workItemMap = new Dictionary<int, WorkItem>();
            foreach (WorkItem workItem in workItems)
            {
                workItemMap[workItem.Id.Value] = workItem;
            }

            int maxLevels = (hierarchy.IsTreeQuery) ? Int32.MaxValue : 1;

            var rootLinks = linkInfo.Where(li => li.Source == null);
            foreach (var rootLink in rootLinks)
            {
                WorkItemHierarchyNode rootNode = CreateNode(rootLink, linkInfo, workItemMap, 0, maxLevels);
                hierarchy.Roots.Add(rootNode);
            }

            return hierarchy;
        }


        private static WorkItemHierarchyNode CreateNode(WorkItemLink linkInfo, ICollection<WorkItemLink> allLinkInfo, Dictionary<int, WorkItem> workItemMap, int currentLevel, int maxLevels)
        {
            int workItemId = linkInfo.Target.Id;
            WorkItemHierarchyNode node = new WorkItemHierarchyNode();
            node.Link = linkInfo;
            node.WorkItem = workItemMap[workItemId];
            node.Level = currentLevel;

            if (maxLevels > 0)
            {
                var targetLinks = allLinkInfo.Where(li => li.Source?.Id == workItemId);
                foreach (var targetLink in targetLinks)
                {
                    WorkItemHierarchyNode childNode = CreateNode(targetLink, allLinkInfo, workItemMap, currentLevel + 1, maxLevels - 1);
                    childNode.Parent = node;
                    node.Children.Add(childNode);
                }
            }

            return node;
        }

        public IEnumerable<WorkItemHierarchyNode> AllNodes
        {
            get
            {
                return FlattenNodes(this.Roots);
            }
        }

        public ICollection<WorkItem> AllWorkItems
        {
            get
            {
                return this.AllNodes.Select(node => node.WorkItem).Distinct(WorkItemComparer.Instance).OrderBy(wi => wi.Id).ToList();
            }
        }

        private static IEnumerable<WorkItemHierarchyNode> FlattenNodes(IEnumerable<WorkItemHierarchyNode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;

                if (node.Children.Any())
                {
                    foreach (var item in FlattenNodes(node.Children))
                    {
                        yield return item;
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var node in Roots)
            {
                AppendNode(sb, node);
            }

            return sb.ToString();
        }

        private static void AppendNode(StringBuilder sb, WorkItemHierarchyNode node)
        {
            if (node.Level > 0)
            {
                sb.Append(' ', node.Level * 2);
            }

            sb.AppendLine(node.WorkItem.Id.ToString());

            foreach (var child in node.Children)
            {
                AppendNode(sb, child);
            }
        }
    }
}
