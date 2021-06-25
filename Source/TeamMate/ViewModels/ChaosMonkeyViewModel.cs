using Microsoft.Internal.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
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
