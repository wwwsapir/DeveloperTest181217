using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest181217
{
    public class CsvManager
    {
        private IntegersSetCsvLoader mLoader = new IntegersSetCsvLoader();


        // This method copies CSV files from Source to Dest Dir.
        // Each csv File in Source and Aux directories should contain only one set of integers in one row, separated by commas.
        // The function copies only the files from source dir that contain at least iMinEqualNums (number) integers 
        // that are identical to integers in at least one of the files in the Aux dir.
        public void CopySimilarCsvs(string iSourceDirPath, string iAuxDirPath, string oDestDirPath, int iMinEqualNums)
        {
            
        }
    }
}
