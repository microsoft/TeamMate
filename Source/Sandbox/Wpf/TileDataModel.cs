// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Tools.TeamMate.Sandbox.Wpf
{
    public class TileDataModel
    {
        public TileDataModel()
        {
        }

        public TileDataModel(int id, int priority, string title)
        {
            this.Id = id;
            this.Priority = priority;
            this.Title = title;
        }

        public int Id { get; set; }
        public int Priority { get; set; }
        public string Title { get; set; }

    }
}
