using System.Collections.Generic;

namespace DeveloperTest181217
{
    public class IntegersSetCsv
    {
        public string FilePath { get; }
        public List<int> Array { get; }

        public IntegersSetCsv(string iFilePath, List<int> iArray)
        {
            FilePath = iFilePath;
            Array = iArray;
        }
    }
}
