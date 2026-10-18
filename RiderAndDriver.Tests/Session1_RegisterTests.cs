using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace RiderAndDriver.Tests
{
    [TestClass]
    public sealed class Session1_RegisterTests
    {
        private readonly IWebDriver _browser;

        public Session1_RegisterTests()
        {
            _browser = new ChromeDriver();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelpers.CloseBrowser(_browser);
        }

        [TestMethod]
        public void Register_ThenLogin()
        {
            //Register a Rider
            string riderUsername = TestHelpers.UniqueUsername("rider");
            string riderEmail = riderUsername + "@test.com";
            string password = "TestPassword123!";

            _browser.Navigate().GoToUrl(TestHelpers.BaseURL+"/Account/Register");
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Username")).SendKeys(riderUsername);
            _browser.FindElement(By.Id("Email")).SendKeys(riderEmail);
            _browser.FindElement(By.Id("Password")).SendKeys(password);
            var roleDropdown = _browser.FindElement(By.Id("Role"));
            roleDropdown.SendKeys("Rider");
            _browser.FindElement(By.CssSelector("button[type='submit']")).Click();
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/Account/Login"), $"After rider register, expected Login. Actual URL: {_browser.Url}");

            //Register a Driver
            string driverUsername = TestHelpers.UniqueUsername("driver");
            string driverEmail = driverUsername + "@test.com";

            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Account/Register");
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Username")).SendKeys(driverUsername);
            _browser.FindElement(By.Id("Email")).SendKeys(driverEmail);
            _browser.FindElement(By.Id("Password")).SendKeys(password);
            roleDropdown = _browser.FindElement(By.Id("Role"));
            roleDropdown.SendKeys("Driver");
            _browser.FindElement(By.CssSelector("button[type='submit']")).Click();
            TestHelpers.Delay();
            
            Assert.IsTrue(_browser.Url.Contains("/Account/Login"), $"After Driver register, expected Login. Actual URL: {_browser.Url}");

            //Login as Rider
            TestHelpers.LoginAs(_browser, riderUsername, password);
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/RiderOnly"), $"Expected to land on RiderOnly. Actual URL: {_browser.Url}");

            //Logout
            TestHelpers.Logout(_browser);
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/Account/Login"), $"After logout, expected Login. Actual URL: {_browser.Url}");

            //Login as Driver
            TestHelpers.LoginAs(_browser, driverUsername, password);
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/DriverOnly"), $"Expected to land on DriverOnly. Actual URL: {_browser.Url}");
        }        
    }
}
