using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ChaosMonkeyViewModel : ViewModelBase
    {
        private ICollection<ChaosScenario> scenarios;

        public bool IsEnabled
        {
            get { return ChaosMonkey.IsEnabled; }
            set { ChaosMonkey.IsEnabled = value; }
        }

        public ICollection<ChaosScenario> Scenarios
        {
            get
            {
                if (this.scenarios == null)
                {
                    this.scenarios =
                        ChaosScenario.LoadScenariosFromStaticFields(typeof(ChaosScenarios))
                        .OrderBy(s => s.Name)
                        .ToList();
                }

                return this.scenarios;
            }
        }
    }
}
