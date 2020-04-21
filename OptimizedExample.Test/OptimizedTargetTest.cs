using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimizedExample.Utils;
using Rhino.Mocks;

namespace OptimizedExample.Test
{
    [TestClass]
    public class OptimizedTargetTest
    {
        [TestMethod]
        public void InitializeConfigFile_InstalledFileNotExists_ReturnFalse()
        {
            string installedFilePath = "installedFilePath";
            string userFilePath = "userFilePath";
            IFileSystemOperation stubFileSystemOperator = MockRepository.GenerateStub<IFileSystemOperation>();
            stubFileSystemOperator.Stub(s => s.FileExists(installedFilePath)).Return(false);
            OptimizedTarget target = new OptimizedTarget(null, stubFileSystemOperator);

            bool installedFileNotExistsResult = target.InitializeConfigFile(installedFilePath, userFilePath);

            Assert.IsFalse(installedFileNotExistsResult);
        }

        [TestMethod]
        public void InitializeConfigFile_InstalledFileExists_UserFileNotExists_CopyFileAndReturnTrue()
        {
            string installedFilePath = "installedFilePath";
            string userFilePath = "userFilePath";
            IFileSystemOperation mockFileSystemOperator = MockRepository.GenerateMock<IFileSystemOperation>();
            mockFileSystemOperator.Stub(s => s.FileExists(installedFilePath)).Return(true);
            mockFileSystemOperator.Stub(s => s.FileExists(userFilePath)).Return(false);
            OptimizedTarget target = new OptimizedTarget(null, mockFileSystemOperator);

            bool installedFileExistsUserFileNotExistsResult = target.InitializeConfigFile(installedFilePath, userFilePath);

            mockFileSystemOperator.AssertWasCalled(s=>s.FileCopy(installedFilePath, userFilePath));
            Assert.IsTrue(installedFileExistsUserFileNotExistsResult);
        }

        [TestMethod]
        public void InitializeConfigFile_InstalledFileExists_UserFileNotExists_ThrowException_ReturnFalse()
        {
            string installedFilePath = "installedFilePath";
            string userFilePath = "userFilePath";
            IFileSystemOperation mockFileSystemOperator = MockRepository.GenerateMock<IFileSystemOperation>();
            mockFileSystemOperator.Stub(s => s.FileExists(installedFilePath)).Return(true);
            mockFileSystemOperator.Stub(s => s.FileExists(userFilePath)).Return(false);
            mockFileSystemOperator.Stub(s => s.FileCopy("", "")).IgnoreArguments().Throw(new Exception());
            OptimizedTarget target = new OptimizedTarget(null, mockFileSystemOperator);

            bool throwExceptionResult = target.InitializeConfigFile(installedFilePath, userFilePath);

            mockFileSystemOperator.AssertWasCalled(s => s.FileCopy(installedFilePath, userFilePath));
            Assert.IsFalse(throwExceptionResult);
        }

        [TestMethod]
        public void InitializeConfigFile_InstalledFileExists_UserFileExists_ReturnTrue()
        {
            string installedFilePath = "installedFilePath";
            string userFilePath = "userFilePath";
            IFileSystemOperation stubFileSystemOperator = MockRepository.GenerateStub<IFileSystemOperation>();
            stubFileSystemOperator.Stub(s => s.FileExists("")).IgnoreArguments().Repeat.Any().Return(true);
            OptimizedTarget target = new OptimizedTarget(null, stubFileSystemOperator);

            bool fileExistResult = target.InitializeConfigFile(installedFilePath, userFilePath);

            Assert.IsTrue(fileExistResult);
        }

        [TestMethod]
        public void SendRequest_NullInput_ReturnFalse()
        {
            IHttpClientWrapper httpClientWrapper = MockRepository.GenerateStub<IHttpClientWrapper>();
            OptimizedTarget target = new OptimizedTarget(httpClientWrapper,null);

            bool nullInputResult = target.SendRequest(null, out string _);

            Assert.IsFalse(nullInputResult);
        }

        [TestMethod]
        public void SendRequest_HttpClientFailed_ReturnFalse()
        {
            IHttpClientWrapper httpClientWrapper = MockRepository.GenerateStub<IHttpClientWrapper>();
            httpClientWrapper.Stub(s => s.SendRequest(null, out string _)).IgnoreArguments().Return(false);
            OptimizedTarget target = new OptimizedTarget(httpClientWrapper, null);

            bool nullInputResult = target.SendRequest(new HttpRequestMessage(), out string _);

            Assert.IsFalse(nullInputResult);
        }

        [TestMethod]
        public void SendRequest_HttpClientThrowException_ReturnFalse()
        {
            IHttpClientWrapper httpClientWrapper = MockRepository.GenerateStub<IHttpClientWrapper>();
            httpClientWrapper.Stub(s => s.SendRequest(null, out string _)).IgnoreArguments().Throw(new Exception());
            OptimizedTarget target = new OptimizedTarget(httpClientWrapper, null);

            bool throwExceptionResult = target.SendRequest(new HttpRequestMessage(), out string _);

            Assert.IsFalse(throwExceptionResult);
        }

        [TestMethod]
        public void SendRequest_HttpClientSucceed_ReturnFalse()
        {
            IHttpClientWrapper httpClientWrapper = MockRepository.GenerateStub<IHttpClientWrapper>();
            httpClientWrapper.Stub(s => s.SendRequest(null, out string _)).IgnoreArguments().Return(true);
            OptimizedTarget target = new OptimizedTarget(httpClientWrapper, null);

            bool httpClientSucceedResult = target.SendRequest(new HttpRequestMessage(), out string _);

            Assert.IsTrue(httpClientSucceedResult);
        }
    }
}