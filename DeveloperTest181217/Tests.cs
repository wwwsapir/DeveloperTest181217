using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeveloperTest181217
{
    public class Tests
    {
        private readonly string testPassedStr = "{0} Test {1} ok.";
        private readonly string testFailedStr = "{0} Test {1} Failed!";

        public void RunTests()
        {
            IntegersSetCsvLoader.Tests loaderTests = new IntegersSetCsvLoader.Tests();
            loaderTests.Run(testPassedStr, testFailedStr);
        }
    }
}
