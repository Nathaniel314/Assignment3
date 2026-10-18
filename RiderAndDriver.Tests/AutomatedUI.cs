using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace RiderAndDriver.Tests
{
    [TestClass]
    public sealed class AutomatedUI
    {
        private readonly IWebDriver _driver;

        public AutomatedUI()
        {
            _driver= new ChromeDriver();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelpers.CloseBrowser(_driver);
        }

        [TestMethod]
        public void launchbrowser()
        {
            _driver.Navigate().GoToUrl("https://localhost:7038");
        }
    }
}
