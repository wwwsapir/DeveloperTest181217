﻿using System;
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


        // --------------------------------------------Nested tests class--------------------------------------------
        public class Tests
        {
            string mClassName = "IntegerSetCsvLoader Class";

            public void Run(string passedStr, string failedStr)
            {
                if (equalTest())
                    Console.WriteLine(String.Format(passedStr, mClassName, "EQUAL"));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, "EQUAL"));

                if (notEqualTest())
                    Console.WriteLine(String.Format(passedStr, mClassName, "NOT_EQUAL"));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, "NOT_EQUAL"));

                if (errorLoadingTest())
                    Console.WriteLine(String.Format(passedStr, mClassName, "ERROR_LOADING"));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, "ERROR_LOADING"));
            }

            private bool equalTest()
            {
                string testFile1Path = Path.Combine(Directory.GetCurrentDirectory(), "text_csv1.csv");
                IntegersSetCsvLoader loader = new IntegersSetCsvLoader();

                StreamWriter testFile1Writer = File.CreateText(testFile1Path);
                testFile1Writer.WriteLine("33,21,68,34,67,222,3,4,1");
                testFile1Writer.Close();

                IntegersSetCsv testFile1 = loader.Load(testFile1Path);
                List<int> expectedRes = new List<int> { 33, 21, 68, 34, 67, 222, 3, 4, 1 };
                if (Enumerable.SequenceEqual(testFile1.Array, expectedRes) &&
                    (testFile1.FilePath == testFile1Path) )
                    return true;
                else
                    return false;
            }

            private bool notEqualTest()
            {
                string testFile1Path = Path.Combine(Directory.GetCurrentDirectory(), "text_csv1.csv");
                IntegersSetCsvLoader loader = new IntegersSetCsvLoader();

                StreamWriter testFile1Writer = File.CreateText(testFile1Path);
                testFile1Writer.WriteLine("33,21,68,34,67,222,3,4,1,5");
                testFile1Writer.Close();

                IntegersSetCsv testFile1 = loader.Load(testFile1Path);
                List<int> notExpectedRes = new List<int> { 33, 21, 68, 34, 67, 222, 3, 4, 1 };
                if (!Enumerable.SequenceEqual(testFile1.Array, notExpectedRes))
                    return true;
                else
                    return false;
            }

            private bool errorLoadingTest()
            {
                string testFile1Path = Path.Combine(Directory.GetCurrentDirectory(), "text_csv1.csv");
                IntegersSetCsvLoader loader = new IntegersSetCsvLoader();

                StreamWriter testFile1Writer = File.CreateText(testFile1Path);
                testFile1Writer.WriteLine("33,21,68,34,67,222,3,4,1, sdjksjsdlk");
                testFile1Writer.Close();

                try
                {
                    IntegersSetCsv testFile1 = loader.Load(testFile1Path);
                }
                catch (Exception)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
