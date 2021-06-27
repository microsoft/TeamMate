using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Microsoft.Tools.TeamMate.Foundation.Chaos
{
    /// <summary>
    /// Defines a chaos scenario and its metadata for use by the Chaos moneky.
    /// </summary>
    public class ChaosScenario : INotifyPropertyChanged
    {
        public const double NeverFail = 0.0;
        public const double FailSeldomly = 0.25;
        public const double FailSometimes = 0.5;
        public const double FailFrequently = 0.75;
        public const double FailAlways = 1.0;

        public const int Slow = 1000;
        public const int VerySlow = 3000;
        public const int SuperSlow = 10000;

        private double failureRate;
        private int delay;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChaosScenario"/> class.
        /// </summary>
        /// <param name="name">The scenario name.</param>
        /// <param name="failureRate">The failure rate (from 0.0 to 1.0).</param>
        /// <param name="delay">The delay (in milliseconds).</param>
        public ChaosScenario(string name, double failureRate = 0, int delay = 0)
        {
            this.Name = name;
            this.FailureRate = failureRate;
            this.Delay = delay;
        }

        /// <summary>
        /// Gets the scenario name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the current failure rate for this scenario (from 0.0 to 1.0, 1.0 being 100% failure).
        /// </summary>
        public double FailureRate
        {
            get { return this.failureRate; }

            set
            {
                if (value != this.failureRate)
                {
                    if (value < 0 || value > 1.0)
                    {
                        throw new ArgumentOutOfRangeException("FailureRate", "Failure rate must be between 0.0 and 1.0");
                    }

                    this.failureRate = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FailureRate)));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current time delay, in milliseconds, for this scenario.
        /// </summary>
        public int Delay
        {
            get { return this.delay; }

            set
            {
                if (value != this.delay)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException("Delay", "Delay must be greater or equal to 0.");
                    }

                    this.delay = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Delay)));
                }
            }
        }

        /// <summary>
        /// Resets the failure rate and delay for this scenario back to 0.
        /// </summary>
        public void Reset()
        {
            this.FailureRate = 0;
            this.Delay = 0;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public static ICollection<ChaosScenario> LoadScenariosFromStaticFields(Type type)
        {
            var allFields = type.GetTypeInfo().DeclaredFields;
            var scenarioFields = allFields.Where(f => f.IsPublic && f.IsStatic && f.FieldType == typeof(ChaosScenario));
            var scenarios = scenarioFields.Select(f => f.GetValue(null)).OfType<ChaosScenario>().OrderBy(s => s.Name).ToList();
            return scenarios;
        }
    }
}
