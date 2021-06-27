// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    internal class CompoundFilter<T>
    {
        private ObservableCollection<Predicate<T>> filters = new ObservableCollection<Predicate<T>>();

        public event EventHandler Changed;

        public CompoundFilter()
        {
            this.filters.CollectionChanged += HandleCollectionChanged;
        }

        public ICollection<Predicate<T>> Predicates
        {
            get { return this.filters; }
        }

        public bool Filter(T item)
        {
            if (filters.Count == 0)
                return true;

            if (OrMode)
            {
                bool result = false;
                for (int i = 0; i < filters.Count && !result; i++)
                {
                    result |= filters[i](item);
                }
                return result;
            }
            else
            {
                bool result = true;
                for (int i = 0; i < filters.Count && result; i++)
                {
                    result &= filters[i](item);
                }
                return result;
            }
        }

        private bool orMode;

        public bool OrMode
        {
            get { return this.orMode; }
            set
            {
                if (this.orMode != value)
                {
                    this.orMode = value;

                    if (filters.Any())
                    {
                        FireChanged();
                    }
                }
            }
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireChanged();
        }

        public void Invalidate()
        {
            FireChanged();
        }

        private void FireChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
