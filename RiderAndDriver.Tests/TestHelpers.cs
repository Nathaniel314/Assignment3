using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace RiderAndDriver.Tests
{
    public static class TestHelpers
    {
        public const string BaseURL = "https://localhost:7038";

        //Generates a unique username
        public static string UniqueUsername(string prefix)
        {
            return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        //returns datetime x days in the future (for booking trips)
        public static DateTime FutureDate(int daysFromToday)
        {
            return DateTime.Today.AddDays(daysFromToday);
        }

        //Returns date time x as string formated for HTML date inputs
        public static string FutureDateString(int daysFromToday)
        {
            return DateTime.Today.AddDays(daysFromToday).ToString("yyyy-MM-dd");
        }

        public static void Delay(int milliseconds = 1000)
        {
            Thread.Sleep(milliseconds);
        }

        //Logs in via login form
        public static void LoginAs(IWebDriver browser, string username, string password)
        {
            browser.FindElement(By.Id("Username")).Clear();
            browser.FindElement(By.Id("Username")).SendKeys(username);

            browser.FindElement(By.Id("Password")).Clear();
            browser.FindElement(By.Id("Password")).SendKeys(password);

            browser.FindElement(By.CssSelector("button[type='submit']")).Click();

            Delay(500);
        }

        public static void Logout(IWebDriver browser)
        {
            var logoutButton = browser.FindElement(
                By.XPath("//button[contains(text(), 'Logout')]"));

            logoutButton.Click();

            Delay(500);
        }

        //closes and disposes the browser. called from [TestCleanup]
        public static void CloseBrowser(IWebDriver browser)
        {
            if (browser == null) return;

            try
            {
                browser.Quit();
                browser.Dispose();
            }

            catch(Exception) 
            {
                
            }
        }
    }
}
