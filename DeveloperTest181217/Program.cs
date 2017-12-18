using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeveloperTest181217
{
    public class Program
    {
        static void Main()
        {
            // Run tests:
            /*
            Tests mTestObject = new Tests();
            mTestObject.RunTests();
            Console.ReadLine(); // This line is here so we can see the result in the console when operating from VS
            */

            // Run user console interface:
            string sourceDirPath, auxDirPath, destDirPath, minNumForSimilarityStr;
            int minNumForSimilarity;

            do
            {
                Console.WriteLine("Please enter a valid full source directory path:");
                sourceDirPath = Console.ReadLine();
            } while (!Directory.Exists(sourceDirPath));
                
            do
            {
                Console.WriteLine("Please enter valid full auxillery directory path:");
                auxDirPath = Console.ReadLine();
            } while (!Directory.Exists(auxDirPath));

            Console.WriteLine("Please enter full destination directory path:");
            destDirPath = Console.ReadLine();

            do
            {
                Console.WriteLine("Please enter a number between 0 and 100,000 for minimal identical numbers to be counted as similarity:");
                minNumForSimilarityStr = Console.ReadLine();
            } while (!int.TryParse(minNumForSimilarityStr, out minNumForSimilarity) ||
                    minNumForSimilarity < 0 ||
                    minNumForSimilarity > 100000);

            Console.WriteLine("Running...");
            IntegersSetCsvManager manager = new IntegersSetCsvManager();
            try
            {
                manager.CopySimilarCsvs(sourceDirPath, auxDirPath, destDirPath, minNumForSimilarity);
            }
            catch (Exception)
            {
                Console.WriteLine("Unknown error occured");
            }
            Console.WriteLine("Finished. Check destination folder for results. Press enter to exit.");
            Console.ReadLine(); // This line is here so we can see the result in the console when operating from VS
        }
    }
}
