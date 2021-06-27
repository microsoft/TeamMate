// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Runtime.InteropServices;
using System;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public static class OfficeUtilities
    {
        private static readonly Guid OutlookApplication = new Guid("00063001-0000-0000-C000-000000000046");
        private static readonly Guid ExcelApplication = new Guid("000208D5-0000-0000-C000-000000000046");
        private static readonly Guid OneNoteApplication = new Guid("452AC71A-B655-4967-A208-A4CC39DD7949");
        private static readonly Guid PowerPointApplication = new Guid("91493442-5A91-11CF-8700-00AA0060263B");
        private static readonly Guid WordApplication = new Guid("00020970-0000-0000-C000-000000000046");

        private static readonly Guid UCOfficeIntegration = new Guid("6A222195-F65E-467F-8F77-EB180BD85288");

        public static bool IsOutlookInstalled()
        {
            return InteropUtilities.IsInterfaceRegistered(OutlookApplication);
        }

        public static bool IsExcelInstalled()
        {
            return InteropUtilities.IsInterfaceRegistered(ExcelApplication);
        }

        public static bool IsOneNoteInstalled()
        {
            return InteropUtilities.IsInterfaceRegistered(OneNoteApplication);
        }

        public static bool IsPowerPointInstalled()
        {
            return InteropUtilities.IsInterfaceRegistered(PowerPointApplication);
        }

        public static bool IsWordInstalled()
        {
            return InteropUtilities.IsInterfaceRegistered(WordApplication);
        }

        public static bool IsLyncInstalled()
        {
            return InteropUtilities.IsInterfaceRegistered(UCOfficeIntegration);
        }
    }
}
