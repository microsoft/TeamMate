// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.Tools.TeamMate.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Tools.TeamMate.Utilities
{
    public class CustomToastManager : IToastManager
    {
        private SoundPlayer notificationSound;

        public static readonly DependencyProperty SlotProperty = DependencyProperty.RegisterAttached(
            "Slot", typeof(int), typeof(CustomToastManager)
        );

        public static void SetSlot(DependencyObject element, int value)
        {
            element.SetValue(SlotProperty, value);
        }

        public static int GetSlot(DependencyObject element)
        {
            return (int)element.GetValue(SlotProperty);
        }

        private static readonly TimeSpan TimeBetweenToasts = TimeSpan.FromSeconds(3);
        private const int MaxSlots = 3;

        private ICollection<int> busySlots = new List<int>();

        private Queue<ToastViewModel> toastQueue = new Queue<ToastViewModel>();

        private DispatcherTimer dispatcherTimer;
        private object queueLock = new object();
        private SettingsService settingsService;

        public event EventHandler<ToastActivatedEventArgs> ToastActivated;

        public CustomToastManager(Dispatcher dispatcher, SettingsService settingsService)
        {
            this.settingsService = settingsService;

            this.dispatcherTimer = new DispatcherTimer(DispatcherPriority.Background, dispatcher);
            this.dispatcherTimer.Tick += HandleCheckForToastsTick;

            this.settingsService.Settings.PropertyChanged += HandleSettingChanged;
        }

        private void HandleToastActivated(object sender, EventArgs e)
        {
            if (this.ToastActivated != null)
            {
                string activationArgsString = ((ToastViewModel)sender).ActivationArguments;
                this.ToastActivated(this, new ToastActivatedEventArgs(activationArgsString));
            }
        }

        public int MaxToasts { get; set; }

        private void Queue(ToastViewModel toast)
        {
            lock (queueLock)
            {
                toastQueue.Enqueue(toast);
            }

            if (!this.dispatcherTimer.IsEnabled)
            {
                this.dispatcherTimer.Interval = TimeSpan.Zero;
                this.dispatcherTimer.IsEnabled = true;
            }
        }

        private void HandleCheckForToastsTick(object sender, EventArgs e)
        {
            CheckQueue();
        }

        private void CheckQueue()
        {
            // Reset time between toasts, this might have been set to zero the first time a toast was queued
            this.dispatcherTimer.Interval = TimeBetweenToasts;

            bool hasItems;
            ToastViewModel itemToDisplay = null;

            // Dequeue and display the first toast
            lock (queueLock)
            {
                hasItems = toastQueue.Any();
                if (hasItems && busySlots.Count < MaxSlots)
                {
                    itemToDisplay = toastQueue.Dequeue();
                }
            }

            if (hasItems)
            {
                if (itemToDisplay != null)
                {
                    Display(itemToDisplay);
                }
            }
            else
            {
                // No items left in th equeue, disable the timer till further queueing
                this.dispatcherTimer.IsEnabled = false;
            }
        }

        private void Display(ToastViewModel toast)
        {
            CustomToastWindow window = new CustomToastWindow();
            window.Loaded += HandleWindowLoaded;
            window.Closed += HandleWindowClosed;
            window.DataContext = toast;

            int slot = 0;

            while (busySlots.Contains(slot))
            {
                // TODO: This will end up busy waiting in the case where all slots are kept up forever, improve.
                // This CAN happen if you mouse over all slots that are visible like crazy.
                slot = (++slot % MaxSlots);
            }

            busySlots.Add(slot);

            SetSlot(window, slot);

            window.Show();

            this.PlayNotificationSound();
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            CustomToastWindow window = (CustomToastWindow)sender;
            window.Loaded -= HandleWindowLoaded;

            int slot = GetSlot(window);
            WindowUtilities.MoveToToastLocation(window, slot);
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            CustomToastWindow window = (CustomToastWindow)sender;
            window.Closed -= HandleWindowClosed;

            int slot = GetSlot(window);
            busySlots.Remove(slot);
        }

        private void PlayNotificationSound()
        {
            SoundPlayer sound = GetNotificationSound();

            if (sound != null)
            {
                sound.Play();
            }
        }

        private SoundPlayer GetNotificationSound()
        {
            var settings = this.settingsService.Settings;

            if (settings.PlayNotificationSound)
            {
                if (this.notificationSound == null)
                {
                    // Lazy load the default notification sound
                    this.notificationSound = new SoundPlayer(TeamMateResources.NotificationSoundStream);
                }

                return this.notificationSound;
            }

            return null;
        }

        private void HandleSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PlayNotificationSound")
            {
                // Invalidate cached notification sound, it will be reloaded lazily later
                this.notificationSound = null;
            }
        }


        public void Dispose()
        {
            this.notificationSound = null;
            this.dispatcherTimer.Stop();
        }

        public void Show(ToastInfo toast)
        {
            ToastViewModel viewModel = CreateToastViewModel(toast);
            this.Queue(viewModel);
        }

        private ToastViewModel CreateToastViewModel(ToastInfo toast)
        {
            ToastViewModel viewModel = new ToastViewModel();
            viewModel.Icon = TeamMateResources.ToastIcon;
            viewModel.Activated += HandleToastActivated;
            viewModel.Title = toast.Title;
            viewModel.Description = toast.Description;
            viewModel.ActivationArguments = toast.Arguments;
            return viewModel;
        }
    }
}
