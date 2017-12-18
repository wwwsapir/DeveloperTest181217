using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace DeveloperTest181217
{
    // A class to manage csv files, each contains only one set of integers in one row, separated by commas.
    public class IntegersSetCsvManager
    {
        private IntegersSetCsvLoader mLoader = new IntegersSetCsvLoader();
        private readonly AutoResetEvent mroContinueSignal = new AutoResetEvent(false);
        private readonly AutoResetEvent mroFinishSignal = new AutoResetEvent(false);

        // This method copies CSV files from Source to Dest Dir.
        // If dest dir doesn't exist, it will be created.
        // The function copies only the files from source dir that contain at least iMinEqualNumsForSimilarity (number) integers 
        // that are identical to integers of at least one of the files in the Aux dir.
        public void CopySimilarCsvs(string iSourceDirPath, string iAuxDirPath, string iDestDirPath, int iMinEqualNumsForSimilarity)
        {
            string errorMsg;
            if (!isInputValid(iSourceDirPath, iAuxDirPath, out errorMsg))
            {
                Console.WriteLine(errorMsg);
                return;
            }

            ConcurrentQueue< KeyValuePair<string, int>? > _scoresQueue = new ConcurrentQueue< KeyValuePair<string, int>? >();

            Thread scoreAdderThread = new Thread(
                () => getPathsToCopyAndScores(iSourceDirPath, iAuxDirPath, iMinEqualNumsForSimilarity, _scoresQueue));
            scoreAdderThread.Name = "Score Adder Thread";
            Thread filesCopyThread = new Thread(
                () => writeFilesAndScoresToDestDir(iDestDirPath, _scoresQueue));
            filesCopyThread.Name = "files Copy Thread";

            scoreAdderThread.Start();
            filesCopyThread.Start();

            scoreAdderThread.Join();
            filesCopyThread.Join();
        }

        private void writeFilesAndScoresToDestDir(string iDestDirPath, ConcurrentQueue<KeyValuePair<string, int>?> iScoresDict)
        {
            DirectoryInfo destDir = Directory.CreateDirectory(iDestDirPath);

            StreamWriter destScoresTextFile = File.CreateText(Path.Combine(iDestDirPath, "scores.txt"));

            while (!mroFinishSignal.WaitOne(1))
            {
                string[] currSplittedFilePath;
                string currFileName;
                string currFilePath;
                // wait to be notified
                mroContinueSignal.WaitOne();

                KeyValuePair<string, int>? score = null;
                while (iScoresDict.TryDequeue(out score))
                {
                    currSplittedFilePath = score.Value.Key.Split('\\');
                    currFileName = currSplittedFilePath[currSplittedFilePath.Length - 1];
                    currFilePath = Path.Combine(iDestDirPath, currFileName);
                    if (File.Exists(currFilePath))  // Delete old file if it already exists
                        File.Delete(currFilePath);
                    File.Copy(score.Value.Key, currFilePath);
                    destScoresTextFile.WriteLine(currFileName + "   max intersection score: " + score.Value);
                }

            }
            destScoresTextFile.Close();
        }

        private void getPathsToCopyAndScores(
            string iSourceDirPath,
            string iAuxDirPath,
            int iMinEqualNumsForSimilarity,
            ConcurrentQueue<KeyValuePair<string, int>?> iScoresDict)
        {
            string[] iSourceDirFiles = Directory.GetFiles(iSourceDirPath);
            string[] iAuxDirFiles = Directory.GetFiles(iAuxDirPath);
            IntegersSetCsv currCsvAux, currCsvSource;
            int currSimilarityScore;
            foreach (string sourceFilePath in iSourceDirFiles)
            {
                if (!loadFileAndCheckInput(sourceFilePath, out currCsvSource))
                    continue;

                foreach (string auxFilePath in iAuxDirFiles)
                {
                    if (!loadFileAndCheckInput(auxFilePath, out currCsvAux))
                        continue;

                    if (areIntegerSetCsvsSimilar(currCsvAux, currCsvSource, iMinEqualNumsForSimilarity, out currSimilarityScore))
                    {
                        iScoresDict.Enqueue(new KeyValuePair<string, int>(sourceFilePath, currSimilarityScore));
                        // notify the waiting thread
                        mroContinueSignal.Set();
                        break;
                    }
                }
            }
            mroFinishSignal.Set();
        }

        // Linear complexity function to check how many identical numbers are between two integer-set-csv files
        // The function returns true if the number of identical numbers is equal or larger than iMinEqualNumsForSimilarity
        private bool areIntegerSetCsvsSimilar(IntegersSetCsv csv1,
            IntegersSetCsv csv2,
            int iMinEqualNumsForSimilarity,
            out int oSimilarityScore)
        {
            oSimilarityScore = 0;
            int i1 = 0, i2 = 0;
            while ( (i1 < csv1.Array.Count) && (i2 < csv2.Array.Count) )
            {
                if (csv1.Array[i1] == csv2.Array[i2])
                {
                    oSimilarityScore++;
                    i1++;
                    i2++;
                }
                else if (csv1.Array[i1] > csv2.Array[i2])
                {
                    i2++;
                }
                else
                {
                    i1++;
                }
            }

            return (oSimilarityScore >= iMinEqualNumsForSimilarity);
        }

        private bool loadFileAndCheckInput(string iFilePath, out IntegersSetCsv oLoadedCsv)
        {
            bool bFileValid = true;
            oLoadedCsv = null;
            if (iFilePath.EndsWith(".csv"))
            {
                try
                {
                    oLoadedCsv = mLoader.Load(iFilePath);
                }
                catch (Exception)
                {
                    bFileValid = false;
                }
            }
            else
                bFileValid = false;

            return bFileValid;
        }

        private bool isInputValid(string iSourceDirPath, string iAuxDirPath, out string oErrorMsg)
        {
            string dirNotFoundMsg = "ERROR: path '{0}' is not a directory.";
            bool bInputValid = true;
            oErrorMsg = "OK";

            if (!Directory.Exists(iSourceDirPath))
            {
                oErrorMsg = string.Format(dirNotFoundMsg, iSourceDirPath);
                bInputValid = false;
            }
            else if (!Directory.Exists(iAuxDirPath))
            {
                oErrorMsg = string.Format(dirNotFoundMsg, iAuxDirPath);
                bInputValid = false;
            }

            return bInputValid;
        }


        // --------------------------------------------Nested tests class--------------------------------------------
        public class Tests
        {
            string mClassName = "IntegersSetCsvManager Class";

            public void Run(string passedStr, string failedStr)
            {
                if (test1())
                    Console.WriteLine(String.Format(passedStr, mClassName, "1"));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, "1"));
                /*
                if (testIntegerSetCsvLoaderNotEqual())
                    Console.WriteLine(String.Format(passedStr, mClassName, "NOT_EQUAL"));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, "NOT_EQUAL"));

                if (testIntegerSetCsvLoaderErrorLoading())
                    Console.WriteLine(String.Format(passedStr, mClassName, "ERROR_LOADING"));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, "ERROR_LOADING"));*/
            }

            private bool test1(bool iEraseFilesAtTestEnd = false)
            {
                bool testRes = false;

                // Create Source Dir
                string sourceDirPath = Path.Combine(Directory.GetCurrentDirectory(), "src");
                string[] sourceContents = { "", "1,2", "1,2,3", "1,2,3,4" };
                createCsvDir(sourceDirPath, sourceContents);

                // Create Aux Dir
                string auxDirPath = Path.Combine(Directory.GetCurrentDirectory(), "auxillery");
                string[] auxContents = { "", "1,2", "1,2,3", "1,2,3,4" };
                createCsvDir(auxDirPath, auxContents);

                // Test
                IntegersSetCsvManager manager = new IntegersSetCsvManager();
                string destDirPath = Path.Combine(Directory.GetCurrentDirectory(), "dest");
                manager.CopySimilarCsvs(sourceDirPath, auxDirPath, destDirPath, 3);
                if (Directory.GetFiles(destDirPath).Length == 4)
                    testRes = true;

                if (iEraseFilesAtTestEnd)
                {
                    // Erase Created Directories recursivly
                    Directory.Delete(sourceDirPath, true);
                    Directory.Delete(auxDirPath, true);
                    if (Directory.Exists(destDirPath))
                        Directory.Delete(destDirPath, true);
                }
 
                return testRes;
            }

            private void createCsvDir(string iDirPath, string[] iContents)
            {
                Directory.CreateDirectory(iDirPath);

                for (int i = 0; i < iContents.Length; i++)
                {
                    createNewCsvFile(iDirPath, i, iContents[i]);
                }
            }

            private void createNewCsvFile(string iDirPath, int iFileId, string iFileContent)
            {
                string testFile1Path = Path.Combine(iDirPath, "text_csv" + iFileId + ".csv");
                StreamWriter testFile1Writer = File.CreateText(testFile1Path);
                testFile1Writer.WriteLine(iFileContent);
                testFile1Writer.Close();
            }
        }
    }
}
