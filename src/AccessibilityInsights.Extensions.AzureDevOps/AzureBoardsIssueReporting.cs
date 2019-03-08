// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Extensions.Interfaces.IssueReporting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AccessibilityInsights.Extensions.AzureDevOps.Models;
using AccessibilityInsights.Extensions.AzureDevOps.FileIssue;
using System.Windows;

namespace AccessibilityInsights.Extensions.AzureDevOps
{
    [Export(typeof(IIssueReporting))]
    public class AzureBoardsIssueReporting : IIssueReporting
    {
        private AzureDevOpsIntegration AzureDevOps = AzureDevOpsIntegration.GetCurrentInstance();

        private ExtensionConfiguration Configuration => AzureDevOps.Configuration;

        public bool IsConnected => AzureDevOps.ConnectedToAzureDevOps;

        public string ServiceName => "Azure Boards";

        public Guid StableIdentifier => new Guid("73D8F6EB-E98A-4285-9BA3-B532A7601CC4");

        public bool IsConfigured => false;

        public IEnumerable<byte> Logo => AzureDevOps.Avatar;

        public string LogoText => null;

        public IssueConfigurationControl ConfigurationControl { get; } = new ConfigurationControl();

        public bool CanAttachFiles => true;

        public Task RestoreConfigurationAsync(string serializedConfig)
        {
            if (!String.IsNullOrEmpty(serializedConfig))
            {
                Configuration.LoadFromSerializedString(serializedConfig);
            }

            return AzureDevOps.HandleLoginAsync();
        }

        public Task<IIssueResult> FileIssueAsync(IssueInformation issueInfo)
        {
            return new Task<IIssueResult>(() => {

                Action<int> updateZoom = (int x) => Configuration.ZoomLevel = x;
                (int? issueId, string newIssueId) = FileIssueAction.FileNewIssue(issueInfo, Configuration.SavedConnection,
                    Application.Current.MainWindow.Topmost, Configuration.ZoomLevel, updateZoom);

                // Check whether issue was filed once dialog closed & process accordingly
                if (issueId.HasValue)
                {
                    try
                    {
                        var success = FileIssueAction.AttachIssueData(issueInfo, newIssueId, issueId.Value).Result;
                        if (!success)
                        {
                            //MessageDialog.Show(Properties.Resources.HierarchyControl_FileIssue_There_was_an_error_identifying_the_created_issue_This_may_occur_if_the_ID_used_to_create_the_issue_is_removed_from_its_Azure_DevOps_description_Attachments_have_not_been_uploaded);
                        }
                        return new IssueResult() { DisplayText = newIssueId, IssueLink = null };
                    }
                    catch (Exception)
                    {
                    }
                }
                return null;
            });
        }
    }
}