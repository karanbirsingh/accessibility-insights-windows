// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace AccessibilityInsights.SharedUx.Controls.CustomControls
{
    public class MenuItemControlElementByVisibility : MenuItem
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MenuItemControlElementByVisibilityAutomationPeer(this);
        }

    }

    public class MenuItemControlElementByVisibilityAutomationPeer : MenuItemAutomationPeer
    {
        private readonly MenuItemControlElementByVisibility owner;
        public MenuItemControlElementByVisibilityAutomationPeer(MenuItemControlElementByVisibility ownerItem) : base(ownerItem)
        {
            this.owner = ownerItem;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return base.GetPattern(patternInterface);
        }

        protected override bool IsControlElementCore() => owner.Visibility == System.Windows.Visibility.Visible;
    }

}
