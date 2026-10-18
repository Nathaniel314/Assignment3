using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace RiderAndDriver.Tests
{
    [TestClass]
    public sealed class Session2_DriverTests
    {
        private readonly IWebDriver _browser;

        public Session2_DriverTests()
        {
            _browser = new ChromeDriver();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelpers.CloseBrowser(_browser);
        }

        [TestMethod]
        public void DriverJourney_CreateTrip_Cancel()
        {
            //Register Driver
            string driverUsername = TestHelpers.UniqueUsername("driver");
            string driverEmail = driverUsername + "@test.com";
            string password = "TestPassword123!";

            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Account/Register");
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Username")).SendKeys(driverUsername);
            _browser.FindElement(By.Id("Email")).SendKeys(driverEmail);
            _browser.FindElement(By.Id("Password")).SendKeys(password);
            var roleDropdown = _browser.FindElement(By.Id("Role"));
            roleDropdown.SendKeys("Driver");
            _browser.FindElement(By.CssSelector("button[type='submit']")).Click();
            TestHelpers.Delay();

            //Login as Driver
            TestHelpers.LoginAs(_browser, driverUsername, password);
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/DriverOnly"), $"Expected to land on DriverOnly. Actual URL: {_browser.Url}");

            //click day
            int lastDay = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);

            var futureDay = _browser.FindElement(By.XPath($"//div[contains(@class, 'calendar-day') and normalize-space(text())='{lastDay}']"));
            futureDay.Click();

            TestHelpers.Delay(1500);

            //Fills ride form
            _browser.FindElement(By.Id("DepartureTime")).SendKeys("8:30");
            
            _browser.FindElement(By.Id("FromLocation")).Clear();
            _browser.FindElement(By.Id("FromLocation")).SendKeys("Lower Hutt");

            _browser.FindElement(By.Id("ToLocation")).Clear();
            _browser.FindElement(By.Id("ToLocation")).SendKeys("Wellington");

            _browser.FindElement(By.Id("SeatsAvailable")).Clear();
            _browser.FindElement(By.Id("SeatsAvailable")).SendKeys("4");

            _browser.FindElement(By.Id("Cost")).Clear();
            _browser.FindElement(By.Id("Cost")).SendKeys("12.50");

            _browser.FindElement(By.Id("NumberPlate")).Clear();
            _browser.FindElement(By.Id("NumberPlate")).SendKeys("ABC123");

            //Submit the form
            _browser.FindElement(By.CssSelector("form[action*='CreateTrip'] button[type='submit']")).Click();
            TestHelpers.Delay(2000);

            //Verify the trip appears
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL+"/Trip");
            TestHelpers.Delay();

            var pageText = _browser.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(pageText.Contains("Lower Hutt") && pageText.Contains("Wellington"), $"Expected trip to appear in my Trips Page text was: \n{pageText}");
            Assert.IsTrue(pageText.Contains("ABC123"), $"Expected plate 'ABC123' in my trips. Page text was:  \n{pageText}");

            //Edit Trip

            var detialsLink = _browser.FindElement(By.XPath("//*[contains(text(), 'Details')]"));
            detialsLink.Click();
            TestHelpers.Delay();

            var updateLink = _browser.FindElement(By.XPath("//*[contains(text(), 'Update')]"));
            updateLink.Click();
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Cost")).Clear();
            _browser.FindElement(By.Id("Cost")).SendKeys("24.99");

            var saveLink = _browser.FindElement(By.XPath("//*[contains(text(), 'Save Changes')]"));
            saveLink.Click();
            TestHelpers.Delay();

            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Trip");
            TestHelpers.Delay();

            pageText = _browser.FindElement(By.TagName("body")).Text;
            Assert.IsTrue(pageText.Contains("24.99"), $"Expected update to cost 24.99 in my Trips. Page text was: \n{pageText}");

            //Cancel Trip
            var cancelButton = _browser.FindElement(By.XPath("//button[contains(text(), 'Cancel')]"));
            cancelButton.Click();
            TestHelpers.Delay(1000);

            try
            {
                var alert = _browser.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
                
            }
            TestHelpers.Delay(2000);

            //Verify the trip is gone
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL+"/Trip");
            TestHelpers.Delay();

            pageText = _browser.FindElement(By.TagName("body")).Text;

            Assert.IsFalse(pageText.Contains("ABC123"), $"Trip should be cancelled and removed from My Trips. Page Text: /n{pageText}");
        }
    }
}
