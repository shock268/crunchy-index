using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WebAutomation
{
    public class WebAccessor : IDisposable
    {
        public ChromeDriver Driver { get; private set; }

        public WebAccessor(
            bool useAdBlockPlusExtension = false,
            bool useCurrentUserProfile = false,
            int browserPositionX = 960,
            int browserPositionY = 0,
            int browserWidth = 960,
            int browserHeight = 1032)
        {
            Trace.TraceInformation("initializing browser...");

            try
            {
                var options = new ChromeOptions();

                // default options uses "--ignore-certificate-errors" argument
                // which may impact stability and security, so this prevents it
                options.AddArgument("--test-type");

                if (useAdBlockPlusExtension)
                {
                    options.AddExtension("adblockplus_1_7_4.crx");
                }

                if (useCurrentUserProfile)
                {
                    string chromeUserProfile = @"%USERPROFILE%\AppData\Local\Google\Chrome\User Data\Default\";
                    options.AddArgument("user-data-dir=" + chromeUserProfile);
                }

                this.Driver = new ChromeDriver(); // options

                /*
                this.Driver.Manage().Window.Position = new Point(browserPositionX, browserPositionY);
                this.Driver.Manage().Window.Size = new Size(browserWidth, browserHeight);
                */

                if (useAdBlockPlusExtension)
                {
                    // installing the extension opens a tab which takes focus - close it
                    this.CloseTabWithUrlContains("firstRun.html");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }

            Trace.TraceInformation("initialized");
        }

        public void NavigateTo(string url)
        {
            Trace.TraceInformation("navigating to \"{0}\"...", url);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                this.Driver.Navigate().GoToUrl(url);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }

            stopwatch.Stop();
            Trace.TraceInformation("navigated in {0}ms", stopwatch.ElapsedMilliseconds);
        }

        public bool ElementExists(string xpath)
        {
            try
            {
                this.Driver.FindElementByXPath(xpath);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public bool Click(string xpath)
        {
            return this.Click(this.Driver.FindElementByXPath(xpath));
        }

        private bool Click(IWebElement element)
        {
            bool clicked = false;
            Trace.TraceInformation("clicking {0}...", element.TagName);

            this.WaitFor(() => element.Displayed, "element to be visible");
            
            if (element.Displayed)
            {
                element.Click();
                clicked = true;
                BotDetectionMitigation.RandomizedWait();
            }

            Trace.TraceInformation("clicked = {0}", clicked);
            return clicked;
        }

        public void Type(string text, string xpath, bool maskText = false)
        {
            Trace.TraceInformation("typing \"{0}\" in \"{1}\"...", maskText ? "***" : text, xpath);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            IWebElement element;

            try
            {
                element = this.Driver.FindElementByXPath(xpath);
                element.SendKeys(text);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                throw;
            }

            stopwatch.Stop();
            Trace.TraceInformation("typed in {0}ms", stopwatch.ElapsedMilliseconds);
        }

        public void WaitFor(Func<bool> condition, string description, int maxWaitMs = 15000)
        {
            Trace.TraceInformation("waiting for {0}...", description);
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < maxWaitMs)
            {
                if (condition())
                {
                    Trace.TraceInformation(
                        "finished waiting for {0} after {1}ms",
                        description,
                        stopwatch.ElapsedMilliseconds);
                    return;
                }

                Trace.TraceWarning("still waiting for {0}...", description);
                BotDetectionMitigation.RandomizedWait();
            }

            stopwatch.Stop();
            Trace.TraceError(
                "gave up waiting for {0} after {1}ms",
                description,
                stopwatch.ElapsedMilliseconds);
        }

        public void Dispose()
        {
            if (Driver != null)
            {
                Trace.TraceInformation("closing browser...");

                try
                {
                    this.Driver.Dispose();
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.ToString());
                    throw;
                }

                Trace.TraceInformation("closed");
            }
        }

        private void CloseTabWithUrlContains(string partialUrl, int maxWaitMs = 15000)
        {
            string matchingWindowHandle = null;

            this.WaitFor(() =>
            {
                foreach (var windowHandle in this.Driver.WindowHandles)
                {
                    if (this.Driver.SwitchTo().Window(windowHandle).Url.Contains(partialUrl))
                    {
                        matchingWindowHandle = windowHandle;
                        break;
                    }
                }

                return matchingWindowHandle != null;
            }, "tab to appear");
            
            while (this.Driver.WindowHandles.Contains(matchingWindowHandle))
            {
                this.Driver.SwitchTo().Window(matchingWindowHandle);
                this.Driver.Close();
                BotDetectionMitigation.RandomizedWait();
            }

            this.Driver.SwitchTo().Window(this.Driver.WindowHandles[0]);
        }
    }
}
