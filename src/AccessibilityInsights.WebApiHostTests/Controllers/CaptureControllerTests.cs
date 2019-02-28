// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Web.Http.Results;
using Pose;
using AccessibilityInsights.Actions;

namespace AccessibilityInsights.WebApiHost.Controllers.Tests
{
    [TestClass()]
    public class CaptureControllerTests
    {
        [TestMethod()] // TargetInvocationException
        public void TestCapture_Succeeded_Modification()
        {
            var controller = new CaptureController();
            Shim captureShim = Shim.Replace(() => CaptureAction.SetTestModeDataContext(Is.A<Guid>(), Is.A<Actions.Enums.DataContextMode>(), Is.A<Core.Enums.TreeViewMode>(), Is.A<bool>()))
                .With(delegate (Guid g, Actions.Enums.DataContextMode dm, Core.Enums.TreeViewMode tvm, bool b)
                {
                    return true;
                });
            System.Web.Http.IHttpActionResult result = null;
            PoseContext.Isolate(() =>
            {
                result = controller.Test(Guid.NewGuid());
            }, captureShim);
            Assert.IsTrue(result is OkResult);
        }
        
        [TestMethod()]
        public void TestCapture_Succeeded_NoModification()
        {
            var controller = new CaptureController();
            Shim captureShim = Shim.Replace(() => CaptureAction.SetTestModeDataContext(Is.A<Guid>(), Is.A<Actions.Enums.DataContextMode>(), Is.A<Core.Enums.TreeViewMode>(), Is.A<bool>()))
                .With(delegate (Guid g, Actions.Enums.DataContextMode dm, Core.Enums.TreeViewMode tvm, bool b)
                {
                    return false;
                });
            StatusCodeResult result = null;
            PoseContext.Isolate(() =>
            {
                result = controller.Test(Guid.NewGuid()) as StatusCodeResult;
            }, captureShim);
            Assert.AreEqual(HttpStatusCode.NotModified, result.StatusCode);
        }

        [TestMethod()] // similar issue: https://github.com/tonerdo/pose/issues/29 but fix not working here
        public void TestCapture_Failed_BadRequestAtException()
        {
            var controller = new CaptureController();
            Shim captureShim = Shim.Replace(() => CaptureAction.SetTestModeDataContext(Is.A<Guid>(), Is.A<Actions.Enums.DataContextMode>(), Is.A<Core.Enums.TreeViewMode>(), Is.A<bool>()))
                .With(delegate (Guid g, Actions.Enums.DataContextMode dm, Core.Enums.TreeViewMode tvm, bool b)
                {
                    throw new Exception();
                    return true;
                });
            System.Web.Http.IHttpActionResult result = null;
            PoseContext.Isolate(() =>
            {
                result = controller.Test(Guid.NewGuid());
            }, captureShim);
            Assert.IsTrue(result is BadRequestErrorMessageResult);
        }
    }
}
