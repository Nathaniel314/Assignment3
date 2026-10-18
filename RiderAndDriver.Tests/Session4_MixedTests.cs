using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RiderAndDriver.Tests;

namespace RiderAndDriver.Tests
{
    [TestClass]
    public sealed class Session4_MixedTests
    {
        private readonly IWebDriver _browser;

        public Session4_MixedTests()
        {
            _browser = new ChromeDriver();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelpers.CloseBrowser(_browser);
        }

        [TestMethod]
        public void FullLifecycle_DriverCreates_RiderBooks_DriverCancels()
        {
            string password = "TestPassword123!";
            string testDate = "2026-10-29";
            string uniquePlate = "PLATE" + DateTime.Now.ToString("HHmm");

            //Creates Driver
            string driverUsername = TestHelpers.UniqueUsername("driver");
            string driverEmail = driverUsername + "@test.com";

            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Account/Register");
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Username")).SendKeys(driverUsername);
            _browser.FindElement(By.Id("Email")).SendKeys(driverEmail);
            _browser.FindElement(By.Id("Password")).SendKeys(password);
            var roleDropdown = _browser.FindElement(By.Id("Role"));
            roleDropdown.SendKeys("Driver");
            _browser.FindElement(By.CssSelector("button[type='submit']")).Click();
            TestHelpers.Delay();

            //login as Driver
            TestHelpers.LoginAs(_browser, driverUsername, password);
            TestHelpers.Delay();

            var day29= _browser.FindElement(By.XPath($"//div[contains(@class, 'calendar-day') and normalize-space(text())='29']"));
            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].click();", day29);
            TestHelpers.Delay(1000);

            //Fills ride form
            _browser.FindElement(By.Id("DepartureTime")).SendKeys("10:00");

            _browser.FindElement(By.Id("FromLocation")).Clear();
            _browser.FindElement(By.Id("FromLocation")).SendKeys("Wellington");

            _browser.FindElement(By.Id("ToLocation")).Clear();
            _browser.FindElement(By.Id("ToLocation")).SendKeys("Petone");

            _browser.FindElement(By.Id("SeatsAvailable")).Clear();
            _browser.FindElement(By.Id("SeatsAvailable")).SendKeys("4");

            _browser.FindElement(By.Id("Cost")).Clear();
            _browser.FindElement(By.Id("Cost")).SendKeys("20.00");

            _browser.FindElement(By.Id("NumberPlate")).Clear();
            _browser.FindElement(By.Id("NumberPlate")).SendKeys(uniquePlate);

            //Submit the form
            _browser.FindElement(By.CssSelector("form[action*='CreateTrip'] button[type='submit']")).Click();
            TestHelpers.Delay(2000);

            //Logout
            TestHelpers.Logout(_browser);
            TestHelpers.Delay();

            //Register a Rider
            string riderUsername = TestHelpers.UniqueUsername("rider");
            string riderEmail = riderUsername + "@test.com";
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Account/Register");
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Username")).SendKeys(riderUsername);
            _browser.FindElement(By.Id("Email")).SendKeys(riderEmail);
            _browser.FindElement(By.Id("Password")).SendKeys(password);
            roleDropdown = _browser.FindElement(By.Id("Role"));
            roleDropdown.SendKeys("Rider");
            _browser.FindElement(By.CssSelector("button[type='submit']")).Click();
            TestHelpers.Delay();

            //Login as Rider
            TestHelpers.LoginAs(_browser, riderUsername, password);
            TestHelpers.Delay();

            //Navigates to the rider search page for the trip date
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/RiderOnly/Index?date=" +testDate);
            TestHelpers.Delay(1500);

            //Finds the specific trip card and books
            var targetCard = _browser.FindElement(By.XPath("//div[contains(@class, 'ride-card') and contains (., 'Wellington')]"));
            var bookButton = targetCard.FindElement(By.XPath(".//button[contains(text(), 'Book Ride')]"));


            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].scrollIntoView(true);", bookButton);
            TestHelpers.Delay(500);

            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].click();", bookButton);

            TestHelpers.Delay(2000);

            TestHelpers.Logout(_browser);
            TestHelpers.Delay();

            //login as Driver
            TestHelpers.LoginAs(_browser, driverUsername, password);
            TestHelpers.Delay();

            //Go to My Trip
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Trip");
            TestHelpers.Delay();

            var driverPageText=_browser.FindElement(By.TagName("body")).Text;

            //verifies trip is there
            Assert.IsTrue(driverPageText.Contains(uniquePlate), "Driver cannot see the trip they created.");

            //cancels trip
            var cancelBtn=_browser.FindElement(By.XPath($"//tr[contains(., '{uniquePlate}')]//button[contains(text(), 'Cancel')]"));

            //safe click
            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", cancelBtn);
            TestHelpers.Delay(500);

            //((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].click();", cancelBtn);
            cancelBtn.Click();
            TestHelpers.Delay(1000);

            try { _browser.SwitchTo().Alert().Accept(); } catch (NoAlertPresentException) { Console.WriteLine("No alert"); }
            TestHelpers.Delay(2000);

            //Logout Driver
            TestHelpers.Logout(_browser);
            TestHelpers.Delay(3000);

            //Login as Rider
            TestHelpers.LoginAs(_browser, riderUsername, password);
            TestHelpers.Delay(1500);

            //verify booking
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Booking/MyBookings");
            
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(_browser, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.TagName("body")).Text.Contains("Cancelled"));

            var riderPageText = _browser.FindElement(By.TagName("body")).Text;


            TestHelpers.Delay(4500);

            //Verifies the booking is cancels
            Assert.IsTrue(riderPageText.Contains("Petone"), "Rider cannot see their booking history");

            //Check that status has changed
            Assert.IsTrue(riderPageText.Contains("Cancelled"), $"The booking status did not update to Cancelled. Page text: \n{riderPageText}");
        }

    }
}
