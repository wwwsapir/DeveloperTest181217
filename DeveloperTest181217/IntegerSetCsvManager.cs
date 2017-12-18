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

            // The read-write queue between threads will be made from nullable KeyValuePairs
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
            bool bContinueReading = true;

            while (bContinueReading)
            {
                string[] currSplittedFilePath;
                string currFileName;
                string currFilePath;
                // wait to be notified
                mroContinueSignal.WaitOne();

                KeyValuePair<string, int>? score = null;
                while (iScoresDict.TryDequeue(out score))
                {
                    if (score.Value.Key == "END")   // Signal from writer to stop the reading from queue
                    {
                        bContinueReading = false;
                        break;
                    }

                    currSplittedFilePath = score.Value.Key.Split('\\');
                    currFileName = currSplittedFilePath[currSplittedFilePath.Length - 1];
                    currFilePath = Path.Combine(iDestDirPath, currFileName);
                    if (File.Exists(currFilePath))  // Delete old file if it already exists
                        File.Delete(currFilePath);
                    File.Copy(score.Value.Key, currFilePath);
                    destScoresTextFile.WriteLine(currFileName + "   max intersection score: " + score.Value.Value);
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
            int currSimilarityScore, maxSimilarityScoreForFile;
            bool bNeedCopyFile = false;
            foreach (string sourceFilePath in iSourceDirFiles)
            {
                if (!loadFileAndCheckInput(sourceFilePath, out currCsvSource))
                    continue;

                maxSimilarityScoreForFile = 0;
                bNeedCopyFile = false;
                foreach (string auxFilePath in iAuxDirFiles)
                {
                    if (!loadFileAndCheckInput(auxFilePath, out currCsvAux))
                        continue;

                    if (areIntegerSetCsvsSimilar(currCsvAux, currCsvSource, iMinEqualNumsForSimilarity, out currSimilarityScore))
                    {
                        bNeedCopyFile = true;
                        if (maxSimilarityScoreForFile < currSimilarityScore)
                            maxSimilarityScoreForFile = currSimilarityScore;
                    }
                }

                if (bNeedCopyFile)
                {
                    // If file is "similar" - add it to queue so that reader thread will see it and its score
                    iScoresDict.Enqueue(new KeyValuePair<string, int>(sourceFilePath, maxSimilarityScoreForFile));
                    // notify waiting copy files thread that a file is ready in the queue
                    mroContinueSignal.Set();
                }
            }

            // Signal the reading thread that queue has ended
            iScoresDict.Enqueue(new KeyValuePair<string, int>("END", -1));
            mroContinueSignal.Set();
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
            string dirNotFoundMsg = "Invalid Input: path '{0}' is not a directory.";
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
            int testsCount = 1;

            public void Run(string passedStr, string failedStr)
            {
                if (sameSrcAndAuxTest(testsCount, 0))
                    Console.WriteLine(String.Format(passedStr, mClassName, testsCount));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, testsCount));
                testsCount++;
                if (sameSrcAndAuxTest(testsCount, 2))
                    Console.WriteLine(String.Format(passedStr, mClassName, testsCount));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, testsCount));
                testsCount++;
                if (sameSrcAndAuxTest(testsCount, 8))
                    Console.WriteLine(String.Format(passedStr, mClassName, testsCount));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, testsCount));
                testsCount++;
                if (differentSrcAndAuxTest(testsCount))
                    Console.WriteLine(String.Format(passedStr, mClassName, testsCount));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, testsCount));
                testsCount++;
                if (invalidInputTest(testsCount))
                    Console.WriteLine(String.Format(passedStr, mClassName, testsCount));
                else
                    Console.WriteLine(String.Format(failedStr, mClassName, testsCount));
                testsCount++;
            }

            private bool sameSrcAndAuxTest(int iTestsCount, int iMinNumToSimilarity, bool iEraseFilesAtTestEnd = false)
            {
                bool testRes = false;

                // Create Source Dir
                string sourceDirPath = Path.Combine(Directory.GetCurrentDirectory(), "src" + iTestsCount);
                string[] sourceContents = { "", "1", "1,2", "1,2,3", "1,2,3,4" };
                createCsvDir(sourceDirPath, sourceContents);

                // Create Aux Dir
                string auxDirPath = Path.Combine(Directory.GetCurrentDirectory(), "aux" + iTestsCount);
                string[] auxContents = { "", "1", "1,2", "1,2,3", "1,2,3,4" };
                createCsvDir(auxDirPath, auxContents);

                // Test
                IntegersSetCsvManager manager = new IntegersSetCsvManager();
                string destDirPath = Path.Combine(Directory.GetCurrentDirectory(), "dest" + iTestsCount);
                manager.CopySimilarCsvs(sourceDirPath, auxDirPath, destDirPath, iMinNumToSimilarity);
                if (Directory.GetFiles(destDirPath).Length == Math.Max((6 - iMinNumToSimilarity), 1))
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

            private bool differentSrcAndAuxTest(int iTestsCount, bool iEraseFilesAtTestEnd = false)
            {
                bool testRes = false;

                // Create Source Dir
                string sourceDirPath = Path.Combine(Directory.GetCurrentDirectory(), "src" + iTestsCount);
                string[] sourceContents = { "", "8", "1,7", "1,2,3", "1,2,4,8" };
                createCsvDir(sourceDirPath, sourceContents);

                // Create Aux Dir
                string auxDirPath = Path.Combine(Directory.GetCurrentDirectory(), "aux" + iTestsCount);
                string[] auxContents = { "", "1", "1,2", "1,2,3", "1,2,3,4" };
                createCsvDir(auxDirPath, auxContents);

                // Test
                IntegersSetCsvManager manager = new IntegersSetCsvManager();
                string destDirPath = Path.Combine(Directory.GetCurrentDirectory(), "dest" + iTestsCount);
                manager.CopySimilarCsvs(sourceDirPath, auxDirPath, destDirPath, 2);
                if (Directory.GetFiles(destDirPath).Length == 3)
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

            private bool invalidInputTest(int iTestsCount)
            {
                IntegersSetCsvManager manager = new IntegersSetCsvManager();
                string sourceDirPath = Path.Combine(Directory.GetCurrentDirectory(), "src" + (iTestsCount - 1));   // Exists from previous test
                string auxDirPath = Path.Combine(Directory.GetCurrentDirectory(), "aux" + iTestsCount);  // Doesn't Exist
                string destDirPath = Path.Combine(Directory.GetCurrentDirectory(), "aux" + iTestsCount); // Shouldn't be created
                manager.CopySimilarCsvs(sourceDirPath, auxDirPath, destDirPath, 2);
                if (!Directory.Exists(destDirPath))
                    return true;
                else
                    return false;
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
