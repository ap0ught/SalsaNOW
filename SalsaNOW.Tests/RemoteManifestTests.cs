using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SalsaNOW.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class RemoteManifestTests
    {
        private const string EnvName = "SALSANOW_MANIFEST_BASE";
        private static string IniPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SalsaNOW.manifest.ini");

        [TestInitialize]
        public void TestInitialize()
        {
            RemoteManifest.ResetForTests();
            Environment.SetEnvironmentVariable(EnvName, null);
            if (File.Exists(IniPath))
                File.Delete(IniPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            RemoteManifest.ResetForTests();
            Environment.SetEnvironmentVariable(EnvName, null);
            if (File.Exists(IniPath))
                File.Delete(IniPath);
        }

        [TestMethod]
        public void Url_Throws_WhenInitializeNotCalled()
        {
            try
            {
                RemoteManifest.Url("jsons/x.json");
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod]
        public void Initialize_Throws_When_NoEnv_And_NoIni()
        {
            try
            {
                RemoteManifest.Initialize();
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod]
        public void Initialize_FromEnvironment_BuildsUrls()
        {
            Environment.SetEnvironmentVariable(EnvName, "https://cdn.example.com");

            RemoteManifest.Initialize();

            Assert.AreEqual("https://cdn.example.com/jsons/kaka.json", RemoteManifest.Url("jsons/kaka.json"));
            Assert.AreEqual("https://cdn.example.com/jsons/kaka.json", RemoteManifest.Url("/jsons/kaka.json"));
        }

        [TestMethod]
        public void Initialize_FromIni_WhenEnvUnset()
        {
            File.WriteAllText(IniPath, "# comment\r\nManifestBaseUrl=https://ini-host.test\r\n");

            RemoteManifest.Initialize();

            Assert.AreEqual("https://ini-host.test/jsons/apps.json", RemoteManifest.Url("jsons/apps.json"));
        }

        [TestMethod]
        public void Initialize_EnvironmentOverridesIni()
        {
            Environment.SetEnvironmentVariable(EnvName, "https://env-wins.test/");
            File.WriteAllText(IniPath, "ManifestBaseUrl=https://ini-loses.test\r\n");

            RemoteManifest.Initialize();

            Assert.AreEqual("https://env-wins.test/USG/bleh.exe", RemoteManifest.Url("USG/bleh.exe"));
        }

        [TestMethod]
        public void Url_Throws_WhenRelativePathEmpty()
        {
            Environment.SetEnvironmentVariable(EnvName, "https://x.test");
            RemoteManifest.Initialize();

            try
            {
                RemoteManifest.Url("");
                Assert.Fail("Expected ArgumentException for empty");
            }
            catch (ArgumentException) { }

            try
            {
                RemoteManifest.Url(null);
                Assert.Fail("Expected ArgumentException for null");
            }
            catch (ArgumentException) { }
        }
    }
}
