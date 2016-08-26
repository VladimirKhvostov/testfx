// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.VisualStudio.TestPlatform.MSTestAdapter.UnitTests.Execution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.Execution;
    using Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

    using UTF = Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTestOutcome = Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.ObjectModel.UnitTestOutcome;

    [TestClass]
    public class UnitTestResultTest
    {
        [TestMethod]
        public void UnitTestResultConstrutorWithOutcomeAndErrorMessageShouldSetRequiredFields()
        {
            UnitTestResult result = new UnitTestResult(UnitTestOutcome.Error, "DummyMessage");

            Assert.AreEqual(UnitTestOutcome.Error, result.Outcome);
            Assert.AreEqual("DummyMessage", result.ErrorMessage);
        }

        [TestMethod]
        public void UnitTestResultConstrutorWithTestFailedExceptionShouldSetRequiredFields()
        {
            var stackTrace = new StackTraceInformation("trace", "filePath", 2, 3);
            TestFailedException ex = new TestFailedException(UnitTestOutcome.Error, "DummyMessage", stackTrace);

            UnitTestResult result = new UnitTestResult(ex);

            Assert.AreEqual(UnitTestOutcome.Error, result.Outcome);
            Assert.AreEqual("DummyMessage", result.ErrorMessage);
            Assert.AreEqual("trace", result.ErrorStackTrace);
            Assert.AreEqual("filePath", result.ErrorFilePath);
            Assert.AreEqual(2, result.ErrorLineNumber);
            Assert.AreEqual(3, result.ErrorColumnNumber);
        }

        [TestMethod]
        public void ToTestResultShouldReturnConvertedTestResultWithFieldsSet()
        {
            var stackTrace = new StackTraceInformation("DummyStackTrace", "filePath", 2, 3);
            TestFailedException ex = new TestFailedException(UnitTestOutcome.Error, "DummyMessage", stackTrace);
            var dummyTimeSpan = new TimeSpan(20);
            UnitTestResult result = new UnitTestResult(ex)
                                        {
                                            DisplayName = "DummyDisplayName",
                                            Duration = dummyTimeSpan
                                        };
           
            TestCase testCase = new TestCase("Foo", new Uri("Uri", UriKind.Relative), Assembly.GetExecutingAssembly().FullName);
            var startTime = DateTimeOffset.Now;
            var endTime = DateTimeOffset.Now;

            // Act
            var testResult = result.ToTestResult(testCase, startTime, endTime);

            // Validate
            Assert.AreEqual(testCase, testResult.TestCase);
            Assert.AreEqual("DummyDisplayName", testResult.DisplayName);
            Assert.AreEqual(dummyTimeSpan, testResult.Duration);
            Assert.AreEqual(TestOutcome.Failed, testResult.Outcome);
            Assert.AreEqual("DummyMessage", testResult.ErrorMessage);
            Assert.AreEqual("DummyStackTrace", testResult.ErrorStackTrace);
            Assert.AreEqual(startTime, testResult.StartTime);
            Assert.AreEqual(endTime, testResult.EndTime);
            Assert.AreEqual(0, testResult.Messages.Count);
        }

        [TestMethod]
        public void ToTestResultForUniTestResultWithStandardOutShouldReturnTestResultWithStdOutMessage()
        {
            UnitTestResult result = new UnitTestResult()
            {
                StandardOut = "DummyOutput"
            };
            TestCase testCase = new TestCase("Foo", new Uri("Uri", UriKind.Relative), Assembly.GetExecutingAssembly().FullName);
            var testresult = result.ToTestResult(testCase, DateTimeOffset.Now, DateTimeOffset.Now);
            Assert.IsTrue(testresult.Messages.All(m => m.Text.Contains("DummyOutput") && m.Category.Equals("StdOutMsgs")));
        }
        
        [TestMethod]
        public void ToTestResultForUniTestResultWithDebugTraceShouldReturnTestResultWithDebugTraceStdOutMessage()
        {
            UnitTestResult result = new UnitTestResult()
            {
                DebugTrace = "DummyDebugTrace"
            };
            TestCase testCase = new TestCase("Foo", new Uri("Uri", UriKind.Relative), Assembly.GetExecutingAssembly().FullName);
            var testresult = result.ToTestResult(testCase, DateTimeOffset.Now, DateTimeOffset.Now);
            Assert.IsTrue(testresult.Messages.All(m => m.Text.Contains("\n\nDebug Trace:\nDummyDebugTrace") && m.Category.Equals("StdOutMsgs")));
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomePassedShouldReturnTestOutcomePassed()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.Passed);
            Assert.AreEqual(TestOutcome.Passed, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeFailedShouldReturnTestOutcomeFailed()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.Failed);
            Assert.AreEqual(TestOutcome.Failed, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeErrorShouldReturnTestOutcomeFailed()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.Error);
            Assert.AreEqual(TestOutcome.Failed, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeNotRunnableShouldReturnTestOutcomeFailed()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.NotRunnable);
            Assert.AreEqual(TestOutcome.Failed, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeTimeoutShouldReturnTestOutcomeFailed()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.Timeout);
            Assert.AreEqual(TestOutcome.Failed, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeIgnoredShouldReturnTestOutcomeSkipped()
        { 
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.Ignored);
            Assert.AreEqual(TestOutcome.Skipped, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeInconclusiveShouldReturnTestOutcomeSkipped()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.Inconclusive);
            Assert.AreEqual(TestOutcome.Skipped, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeNotFoundShouldReturnTestOutcomeNotFound()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.NotFound);
            Assert.AreEqual(TestOutcome.NotFound, resultOutcome);
        }

        [TestMethod]
        public void UniTestHelperToTestOutcomeForUnitTestOutcomeInProgressShouldReturnTestOutcomeNone()
        {
            var resultOutcome = UnitTestOutcomeHelper.ToTestOutcome(UnitTestOutcome.InProgress);
            Assert.AreEqual(TestOutcome.None, resultOutcome);
        }
    }
}