using System;
using System.Runtime.Serialization;

namespace Microsoft.Internal.Tools.TeamMate.Platform.CodeFlow.Dashboard
{
    internal static class WcfUtilities
    {
        // http://stackoverflow.com/questions/14121343/datetime-kind-is-not-returned-in-wcf
        // http://stackoverflow.com/questions/1297506/roundtrip-xml-serialization-of-datetime-and-xsddate
        public static DateTime NormalizeUtc(DateTime value)
        {
            return (value.Kind == DateTimeKind.Unspecified) ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
        }

        public static DateTime? NormalizeUtc(DateTime? value)
        {
            return (value != null) ? NormalizeUtc(value.Value) : (DateTime?)null;
        }
    }

    public partial class CodeReviewSummary : IDeserializationCallback
    {
        public void OnDeserialization(object sender)
        {
            this.CreatedOnField = WcfUtilities.NormalizeUtc(this.CreatedOnField);
            this.LastUpdatedOnField = WcfUtilities.NormalizeUtc(this.LastUpdatedOnField);
            this.CompletedOnField = WcfUtilities.NormalizeUtc(this.CompletedOnField);

            if (this.ReviewersField == null)
            {
                this.ReviewersField = new Reviewer[0];
            }
        }
    }

    public partial class Author : IDeserializationCallback
    {
        public void OnDeserialization(object sender)
        {
            this.LastUpdatedOnField = WcfUtilities.NormalizeUtc(this.LastUpdatedOnField);
        }
    }

    public partial class Reviewer : IDeserializationCallback
    {
        public void OnDeserialization(object sender)
        {
            this.LastUpdatedOnField = WcfUtilities.NormalizeUtc(this.LastUpdatedOnField);
        }
    }
}
