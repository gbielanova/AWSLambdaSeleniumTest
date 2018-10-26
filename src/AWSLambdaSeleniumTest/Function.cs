using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using OpenQA.Selenium.Chrome;

using Xunit;
using Xunit.Runners;

// Assembly attribute to enable the Lambda function"s JSON input to be converted into a .NET class.
// [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambdaSeleniumTest
{
    public class Function
    {
        ManualResetEvent finished = new ManualResetEvent(false);

        object consoleLock = new object();

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public string FunctionHandler(string input, ILambdaContext context)
        {   
            var testAssembly = System.Reflection.Assembly.GetCallingAssembly();

            using (var runner = AssemblyRunner.WithoutAppDomain("AWSLambdaSeleniumTest.dll"))
            {
                runner.OnDiscoveryComplete = OnDiscoveryComplete;
                runner.OnExecutionComplete = OnExecutionComplete;
                runner.OnTestFailed = OnTestFailed;
                runner.OnTestSkipped = OnTestSkipped;

                LambdaLogger.Log("Discovering tests...\n");
                runner.Start();

                finished.WaitOne();  // A ManualResetEvent
                finished.Dispose();
            }

            return "";
        }

        private void OnTestSkipped(TestSkippedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                LambdaLogger.Log($"[SKIP] {info.TestDisplayName}: {info.SkipReason}\n");
                Console.ResetColor();
            }
        }

        private void OnTestFailed(TestFailedInfo info)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                LambdaLogger.Log($"[FAIL] {info.TestDisplayName}: {info.ExceptionMessage}\n");
                if (info.ExceptionStackTrace != null)
                    LambdaLogger.Log(info.ExceptionStackTrace);

                Console.ResetColor();
            }
        }

        private void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (consoleLock)
                LambdaLogger.Log($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)\n");

            finished.Set();
        }

        private void OnDiscoveryComplete(DiscoveryCompleteInfo obj)
        {
            lock (consoleLock)
                LambdaLogger.Log($"Running {obj.TestCasesToRun} of {obj.TestCasesDiscovered} tests...\n");
        }
    }

    public class SeleniumTests
    {
        [Fact]  
        public void GetsHeader()
        {
            var options = new OpenQA.Selenium.Chrome.ChromeOptions();
            options.AddArguments("--headless", 
                                "--disable-gpu", 
                                "windows-size=1280x1696", 
                                "--no-sandbox",
                                "--user-data-dir=/tmp/user-data",
                                "--hide-scrollbars",
                                "--enable-logging",
                                "--log-level=0",
                                "--v=99",
                                "--single-process",
                                "--data-path=/tmp/data-path",
                                "--ignore-certificate-errors",
                                "--homedir=/tmp",
                                "--disk-cache-dir=/tmp/cache-dir");
            options.BinaryLocation = "/var/task/chrome";

            var webdriver = new ChromeDriver(options);
            webdriver.Url = "http://www.google.com";

            string Title = webdriver.Title;
            LambdaLogger.Log($"Running test: Title for page {webdriver.Url} is: {Title}\n");

            webdriver.Quit();

            Assert.Equal("Google", Title);
        }
    }
}
