using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeveloperTest181217
{
    public class IntegersSetCsvLoader
    {
        public IntegersSetCsv Load(string iFilePath)
        {
            StreamReader csvFile = File.OpenText(iFilePath);
            string[] intArrayAsStrings = csvFile.ReadToEnd().Split(',');
            csvFile.Close();

            List<int> intArray = new List<int>(intArrayAsStrings.Length);
            foreach (string intStr in intArrayAsStrings)
            {
                intArray.Add(int.Parse(intStr));
            }

            return new IntegersSetCsv(iFilePath, intArray);
        }
    }
}
