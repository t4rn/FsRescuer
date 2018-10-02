using FS.Core.Models;
using FS.Core.Services;
using FS.Core.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FS.ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //string fsSourcePath = @"C:\zzz_1";
            //string fsDestinationPath = @"D:\zzz_2";

            //Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(fsSourcePath, fsDestinationPath);
            //Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(fsSourcePath, fsDestinationPath);
            //return;
            Console.WriteLine("Pick: 1 - analyse | 2 - move | 3 - copy local");
            string line = Console.ReadLine();

            int value = int.Parse(line);

            switch (value)
            {
                case 1:
                    AnalyseFs();
                    break;
                case 2:
                    Console.WriteLine("How many applications?");
                    string appCountStr = Console.ReadLine();
                    int appCount = int.Parse(appCountStr);
                    MoveDirectories(appCount);
                    break;
                case 3:
                    CopyDirectories();
                    break;
                default:
                    break;
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadLine();
        }

        private static void AnalyseFs()
        {
            Console.WriteLine($"Starting analysing at '{DateTime.Now}'...");

            Stopwatch watch = Stopwatch.StartNew();
            Result analyseResult = new AnalyseService().Analyse();
            TimeSpan elapsed = watch.Elapsed;

            Console.WriteLine($"Analysis ended in '{elapsed}' - result:\n{analyseResult.Serialize()}");

            Console.WriteLine();
        }

        private static void MoveDirectories(int appCount)
        {
            Console.WriteLine($"Starting moving at '{DateTime.Now}'...");

            RescueAppResult rescueResult = new RescueService().StartMoving(appCount);

            ShowResult(rescueResult);
        }

        private static void CopyDirectories()
        {
            Console.WriteLine($"Starting copying at '{DateTime.Now}'...");

            RescueAppResult rescueResult = new RescueService().StartCopying();

            ShowResult(rescueResult);
        }

        private static void ShowResult(RescueAppResult rescueResult)
        {
            foreach (var wniosek in rescueResult.Applications)
            {
                Console.WriteLine($"Application '{wniosek.ApplicationNumber}' -> isOk = '{wniosek.IsOk}' | msg = '{wniosek.Message}");
            }

            Console.WriteLine($"\nEnded at '{DateTime.Now}' - {rescueResult.Message} -> elapsed '{rescueResult.Elapsed} for '{rescueResult.Applications.Count}' -> correct '{rescueResult.Applications.Where(x => x.IsOk == true).Count()}'. Details:\n");
        }
    }
}
