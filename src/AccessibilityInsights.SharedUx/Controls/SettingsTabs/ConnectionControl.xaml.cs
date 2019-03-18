// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Extensions.Interfaces.IssueReporting;
using AccessibilityInsights.SharedUx.FileBug;
using AccessibilityInsights.SharedUx.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AccessibilityInsights.SharedUx.Controls.SettingsTabs
{
    /// <summary>
    /// Interaction logic for ConnectionControl.xaml
    /// </summary>
    public partial class ConnectionControl : UserControl
    {
        public ConnectionControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Represents different states for whether user has connected to server yet
        /// </summary>
        private enum States
        {
            NoServer,       // First screen with "next"
            EditingServer,  // Second screen with treeview
            HasServer       // Third screen with selected team
        };

        /// <summary>
        /// Delegates
        /// </summary>
        public Action UpdateSaveButton { get; set; }
        public Action<Uri, bool, Action> HandleLoginRequest { get; set; }
        public Action<Action> HandleLogoutRequest { get; set; }
        public Action<bool> ShowSaveButton { get; set; }

        IssueConfigurationControl issueConfigurationControl = null;
        IIssueReporting selectedIssueReporter = null;

        #region configuration updating code
        /// <summary>
        /// Updates the MRU list of servers from the configuration
        /// Depending on whether we are logged in, we update the server combo box selection to
        /// the MRU server or to the currently connected server
        /// </summary>
        /// <param name="configuration"></param>
        public void UpdateFromConfig()
        {
                InitializeView();
        }

        private RadioButton CreateRadioButton(IIssueReporting reporter)
        {
            RadioButton issueReportingOption = new RadioButton();
            issueReportingOption.Content = reporter.ServiceName;
            issueReportingOption.Tag =reporter.StableIdentifier;
            issueReportingOption.Margin = new Thickness(2, 2, 2, 2);
            issueReportingOption.Checked += IssueReporterChecked;
            return issueReportingOption;
        }

        //AK TODO RENAME THIS
        private void IssueReporterChecked(object sender, RoutedEventArgs e)
        {
            if (issueConfigurationControl != null) {
                selectServerGrid.Children.Remove(issueConfigurationControl);
                issueConfigurationControl = null;
                UpdateSaveButton();
            }
            Guid clickedButton  = (Guid)((RadioButton)sender).Tag;
            IssueReporterManager.GetInstance().GetIssueFilingOptionsDict().TryGetValue(clickedButton, out selectedIssueReporter);
            issueConfigurationControl = selectedIssueReporter.RetrieveConfigurationControl(this.UpdateSaveButton);
            Grid.SetRow(issueConfigurationControl, 3);
            selectServerGrid.Children.Add(issueConfigurationControl);
        }

        /// <summary>
        /// Adds the currently selected connection to the configuration so it is persisted
        /// in the MRU cache as well as the auto-startup connection
        /// </summary>
        /// <param name="configuration"></param>
        public void UpdateConfigFromSelections(ConfigurationModel configuration)
        {
            if (issueConfigurationControl.CanSave)
            {
                configuration.SelectedIssueReporter = selectedIssueReporter.StableIdentifier;
                string seralizedConfigs = configuration.IssueReporterSerializedConfigs;
                Dictionary<Guid, string> configs = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(seralizedConfigs);

                string newConfigs = issueConfigurationControl.OnSave();

                configs[selectedIssueReporter.StableIdentifier] = newConfigs;
                configuration.IssueReporterSerializedConfigs = JsonConvert.SerializeObject(configs);
                IssueReporterManager.GetInstance().SetIssueReporter(selectedIssueReporter.StableIdentifier);
                issueConfigurationControl.OnDismiss();
            }
        }

        /// <summary>
        /// For this control we want SaveAndClose to be enabled if any team project
        /// is selected, regardless of whether it differs from the current configuration.
        /// </summary>
        public bool IsConfigurationChanged()
        {
            return issueConfigurationControl != null ? issueConfigurationControl.CanSave : false;
        }
        #endregion

        /// <summary>
        /// Routes to the correct state given whether the user has logged into the server or not.
        /// If the user is connected to the server but they haven't chosen a team project / team yet, we 
        /// allow them to select their team project / team without having to re-connect.
        public void InitializeView()
        {
            // TODO - move "chose team project yet" state into server integration
            IReadOnlyDictionary<Guid, IIssueReporting> options = BugReporter.GetIssueReporters();
            availableIssueReporters.Children.Clear();
            Guid selectedGUID = BugReporter.IssueReporting != null ? BugReporter.IssueReporting.StableIdentifier : default(Guid);
            foreach (var reporter in options)
            {
                RadioButton rb = CreateRadioButton(reporter.Value);
                if (reporter.Key.Equals(selectedGUID))
                {
                    rb.IsChecked = true;
                    issueConfigurationControl = reporter.Value.RetrieveConfigurationControl(this.UpdateSaveButton);
                    Grid.SetRow(issueConfigurationControl, 3);
                    selectServerGrid.Children.Add(issueConfigurationControl);
                }
                availableIssueReporters.Children.Add(rb);
            }

            this.selectServerGrid.Visibility = Visibility.Visible;
        }
    }
}