using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
