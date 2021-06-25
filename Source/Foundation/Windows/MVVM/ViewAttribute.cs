using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM
{
    public class ViewAttribute : Attribute
    {
        public Type ViewModelType { get; set; }

        public ViewAttribute(Type viewModelType)
        {
            Assert.ParamIsNotNull(viewModelType, "viewModelType");

            this.ViewModelType = viewModelType;
        }
    }
}
