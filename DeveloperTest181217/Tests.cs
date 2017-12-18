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

            IntegersSetCsvManager.Tests managerTests = new IntegersSetCsvManager.Tests();
            managerTests.Run(testPassedStr, testFailedStr);
        }
    }
}
