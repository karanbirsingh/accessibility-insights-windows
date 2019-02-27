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
        [TestMethod()]
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
        /*
        [TestMethod()]
        public void TestCapture_Succeeded_NoModification()
        {
            using (ShimsContext.Create())
            {
                var controller = new CaptureController();

                Actions.Fakes.ShimCaptureAction.SetTestModeDataContextGuidDataContextModeTreeViewModeBoolean = (g, da, tv, f) => false;

                var result = controller.Test(Guid.NewGuid()) as StatusCodeResult;

                Assert.AreEqual(HttpStatusCode.NotModified, result.StatusCode);
            }
        }

        [TestMethod()]
        public void TestCapture_Failed_BadRequestAtException()
        {
            using (ShimsContext.Create())
            {
                var controller = new CaptureController();

                Actions.Fakes.ShimCaptureAction.SetTestModeDataContextGuidDataContextModeTreeViewModeBoolean = (g, da, tv, f) => throw new Exception();

                var result = controller.Test(Guid.NewGuid());
                Assert.IsTrue(result is BadRequestErrorMessageResult);
            }
        }*/
    }
}
