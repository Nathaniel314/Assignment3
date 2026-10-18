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
    public sealed class Session3_RiderTests
    {
        private readonly IWebDriver _browser;

        public Session3_RiderTests()
        {
            _browser = new ChromeDriver();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelpers.CloseBrowser(_browser);
        }

        [TestMethod]
        public void RiderJourney_SearchFliterSort_ThenBook()
        {
            //Register a Rider
            string riderUsername = TestHelpers.UniqueUsername("rider");
            string riderEmail = riderUsername + "@test.com";
            string password = "TestPassword123!";

            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Account/Register");
            TestHelpers.Delay();

            _browser.FindElement(By.Id("Username")).SendKeys(riderUsername);
            _browser.FindElement(By.Id("Email")).SendKeys(riderEmail);
            _browser.FindElement(By.Id("Password")).SendKeys(password);
            var roleDropdown = _browser.FindElement(By.Id("Role"));
            roleDropdown.SendKeys("Rider");
            _browser.FindElement(By.CssSelector("button[type='submit']")).Click();
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/Account/Login"), $"After rider register, expected Login. Actual URL: {_browser.Url}");
            TestHelpers.Delay();

            //Login as Rider
            TestHelpers.LoginAs(_browser, riderUsername, password);
            TestHelpers.Delay();

            Assert.IsTrue(_browser.Url.Contains("/RiderOnly"), $"Expected to land on RiderOnly. Actual URL: {_browser.Url}");

            //Navigate to premade trips
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/RiderOnly/Index?date=2026-10-30");
            TestHelpers.Delay();

            var pageText = _browser.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(pageText.Contains("Lower Hutt") && pageText.Contains("Upper Hutt") && pageText.Contains("Petone"), $"Expected all 3 Trips on 30 Oct. Page Text: \n{pageText}");

            //Test Search
            _browser.FindElement(By.Name("FromLocation")).Clear();
            _browser.FindElement(By.Name("FromLocation")).SendKeys("Lower Hutt");

            //_browser.FindElement(By.XPath("//button[contains(text(), 'Apply')]")).Click();
            var applySortBtn = _browser.FindElement(By.XPath("//button[contains(text(), 'Apply')]"));
            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applySortBtn);
            TestHelpers.Delay(500);

            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].click();", applySortBtn);
            TestHelpers.Delay(1500);

            pageText = _browser.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(pageText.Contains("Lower Hutt"), $"Search failed: Lower Hutt should be visible. Page: \n{pageText}");
            Assert.IsFalse(pageText.Contains("Upper Hutt"), $"Search failed: Upper Hutt should be hidden. Page: \n{pageText}");

            //Test Sort
            _browser.FindElement(By.Name("FromLocation")).Clear();
            _browser.FindElement(By.Name("SortBy")).SendKeys("Cost");
            _browser.FindElement(By.Name("SortOrder")).SendKeys("Lowest / Earliest");

            var applySortBtn2 = _browser.FindElement(By.XPath("//button[contains(text(), 'Apply')]"));
            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applySortBtn2);
            TestHelpers.Delay(500);

            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].click();", applySortBtn2);
            TestHelpers.Delay(1500);

            var firstRideCard = _browser.FindElement(By.CssSelector(".ride-card"));
            var firstRideText = firstRideCard.Text;

            Assert.IsTrue(firstRideText.Contains("Upper Hutt"), $"Sort failed: Expected $10 Trip to be first. The first card is: \n{firstRideText}");

            //Seat count check (Initial count)
            var cardBefore = _browser.FindElement(By.XPath("//div[contains(@class, 'ride-card') and contains(., 'Lower Hutt')]"));
            var seatsBeforeText = cardBefore.Text;
            var seatsBefore = int.Parse(System.Text.RegularExpressions.Regex.Match(seatsBeforeText, @"Seats:\s*(\d+)").Groups[1].Value);

            //Book ride
            var targetCard = _browser.FindElement(By.XPath("//div[contains(@class, 'ride-card') and contains (., 'Lower Hutt')]"));
            var bookButton= targetCard.FindElement(By.XPath(".//button[contains(text(), 'Book Ride')]"));

            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].scrollIntoView(true);", bookButton);
            TestHelpers.Delay(500);

            ((IJavaScriptExecutor)_browser).ExecuteScript("arguments[0].click();", bookButton);
            
            TestHelpers.Delay(2000);

            //Seat count check (-1)
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/RiderOnly/Index?date=2026-10-30");
            TestHelpers.Delay(1500);

            var cardAfter = _browser.FindElement(By.XPath("//div[contains(@class, 'ride-card') and contains (., 'Lower Hutt')]"));
            var seatsAfterText= cardAfter.Text;
            var seatsAfter = int.Parse(System.Text.RegularExpressions.Regex.Match(seatsAfterText, @"Seats:\s*(\d+)").Groups[1].Value);

            Assert.AreEqual(seatsBefore - 1, seatsAfter, $"Expected seat count to decrease by one. Was {seatsBefore}, Now {seatsAfter}");

            //verify booking
            _browser.Navigate().GoToUrl(TestHelpers.BaseURL + "/Booking/MyBookings");
            TestHelpers.Delay();

            pageText = _browser.FindElement(By.TagName("body")).Text;

            Assert.IsTrue(pageText.Contains("Confirmed"), $"Expected booking to show as Confirmed. Page text: \n{pageText}");
            Assert.IsTrue(pageText.Contains("Lower Hutt"), $"Expected booking to for Lower Hutt. Page text: \n{pageText}");
        }
    }
}


