using Microsoft.Tools.TeamMate.Foundation.ComponentModel;
using System;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Model
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ConditionalFormattingRule : ObservableObjectBase
    {
        private bool isOneOrMoreItems;
        private bool hasFilter;
        private bool isCustomFilter;

        public event EventHandler Changed;

        public ConditionType ConditionType { get; private set; }
        public int Target { get; private set; }
        public int Target2 { get; private set; }

        public bool IsOneOrMoreItems
        {
            get { return this.isOneOrMoreItems; }
            set { SetProperty(ref this.isOneOrMoreItems, value); }
        }

        public bool HasFilter
        {
            get { return this.hasFilter; }
            set { SetProperty(ref this.hasFilter, value); }
        }

        public bool IsCustomFilter
        {
            get { return this.isCustomFilter; }
            set { SetProperty(ref this.isCustomFilter, value); }
        }

        public void Configure(ConditionType type, int target)
        {
            this.ConditionType = type;
            this.Target = target;
            OnChanged();
        }

        public void Configure(ConditionType type, int target, int target2)
        {
            this.ConditionType = type;
            this.Target = target;
            this.Target2 = target2;
            OnChanged();
        }

        public void Clear()
        {
            this.ConditionType = ConditionType.None;
            OnChanged();
        }

        private void OnChanged()
        {
            HasFilter = (this.ConditionType != ConditionType.None);
            IsOneOrMoreItems = (this.ConditionType == ConditionType.IsGreaterThan && Target == 0);
            IsCustomFilter = (HasFilter && !IsOneOrMoreItems);

            Changed?.Invoke(this, EventArgs.Empty);
        }

        // TODO: This might return a more interesting result in the future, e.g. the Style or color that should be applied
        public bool Evaluate(int value)
        {
            switch (ConditionType)
            {
                case ConditionType.None:
                    return false;

                case ConditionType.Equals:
                    return value == Target;

                case ConditionType.DoesNotEqual:
                    return value != Target;

                case ConditionType.IsGreaterThan:
                    return value > Target;

                case ConditionType.IsGreaterThanOrEqualTo:
                    return value >= Target;

                case ConditionType.IsLessThan:
                    return value < Target;

                case ConditionType.IsLessThanOrEqualTo:
                    return value <= Target;

                case ConditionType.IsBetween:
                    return Target <= value && value <= Target2;

                case ConditionType.IsNotBetween:
                    return value < Target || value > Target2;

                default:
                    throw new NotSupportedException("Unexpected enum value: " + ConditionType);
            }
        }
    }

    public enum ConditionType
    {
        None,
        Equals,
        DoesNotEqual,
        IsGreaterThan,
        IsGreaterThanOrEqualTo,
        IsLessThan,
        IsLessThanOrEqualTo,
        IsBetween,
        IsNotBetween
    }
}
