// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.CommonUxComponents.Controls;
using AccessibilityInsights.SharedUx.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace AccessibilityInsights.Modes
{
    /// <summary>
    /// Interaction logic for DialogContainerModeControl.xaml
    /// </summary>
    public partial class DialogContainerModeControl : UserControl
    {
        public DialogContainerModeControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Override LocalizedControlType
        /// </summary>
        /// <returns></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CustomControlOverridingAutomationPeer(this, Properties.Resources.LocalizedControlType_Page);
        }

        public void HideControl(ContainedDialog dialog) => Dispatcher.Invoke(() =>
        {
            if (gdContainer.Children.Contains(dialog))
            {
                gdContainer.Children.Remove(dialog);
            }

            if (gdContainer.Children.Count == 0)
            {
                this.Visibility = Visibility.Collapsed;
            }
        });

        private void ShowControl(ContainedDialog containedDialog) => Dispatcher.InvokeAsync(() =>
        {
            this.Visibility = Visibility.Visible;
            gdContainer.Children.Add(containedDialog);
        });

        public async Task<bool> ShowDialog(ContainedDialog containedDialog)
        {
            if (containedDialog == null)
                throw new ArgumentNullException(nameof(containedDialog));

            ShowControl(containedDialog);

            return await containedDialog.ShowDialog(HideControl).ConfigureAwait(false);
        }

        public void Show(ContainedDialog containedDialog)
        {
            if (containedDialog == null)
                throw new ArgumentNullException(nameof(containedDialog));

            ShowControl(containedDialog);

            containedDialog.Show();
        }
    }
}
