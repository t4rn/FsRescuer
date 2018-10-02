using FS.Core.Models;
using NLog;
using System;
using System.IO;

namespace FS.Core.Services
{
    public class FsService
    {
        private readonly Logger _log;

        public FsService(Logger log)
        {
            _log = log;
        }

        public Duplex<bool, decimal?> CheckOnFs(string appNumber, string fsPath)
        {
            Duplex<bool, decimal?> result = new Duplex<bool, decimal?>();

            string fsPathWithAppNumber = GetFullPath(appNumber, fsPath);

            result.Item1 = Directory.Exists(fsPathWithAppNumber);
            if (result.Item1)
                result.Item2 = CalculateDirectorySize(fsPathWithAppNumber);

            _log.Debug($"'{appNumber}' in '{fsPathWithAppNumber}' -> '{result.Item1}'| size '{result.Item2}'");

            return result;
        }

        public string GetFullPath(string appNumber, string fsPath)
        {
            string path = $"{fsPath}\\{appNumber.Substring(0, 5)}\\{appNumber}";

            return path;
        }

        /// <summary>
        /// Returns directory size in MB rounded to given decimal places
        /// </summary>
        private decimal CalculateDirectorySize(string newFsFullPath, int decimals = 3)
        {
            string[] files = Directory.GetFiles(newFsFullPath, "*.*", SearchOption.AllDirectories);

            decimal b = 0;
            foreach (string name in files)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }

            if (b > 0)
                b = Math.Round(b / 1024 / 1024, decimals);

            return b;
        }

        public void MoveDirectory(string appNumber, string source, string destination)
        {
            string fsSourcePath = GetFullPath(appNumber, source);
            string fsDestinationPath = GetFullPath(appNumber, destination);

            _log.Debug($"'{appNumber}' moving '{fsSourcePath}' -> '{fsDestinationPath}'");

            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(fsSourcePath, fsDestinationPath);

            //Directory.Move(fsSourcePath, fsDestinationPath); <- doesn't work between different volumes
        }

        public void CopyDirectory(string appNumber, string source, string destination)
        {
            string fsSourcePath = GetFullPath(appNumber, source);
            string fsDestinationPath = GetFullPath(appNumber, destination);

            _log.Debug($"'{appNumber}' copying '{fsSourcePath}' -> '{fsDestinationPath}'");

            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(fsSourcePath, fsDestinationPath, true);
        }

        private void CopyDirectoryOld(string source, string destination, bool copySubDirs)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(source);

                if (!dir.Exists)
                    return;

                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);


                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(destination, file.Name);
                    file.CopyTo(tempPath, false);
                }

                if (copySubDirs)
                {
                    DirectoryInfo[] dirs = dir.GetDirectories();

                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destination, subdir.Name);
                        CopyDirectoryOld(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
            catch (IOException)
            {
                //file exists
            }
            catch (Exception ex)
            {
                _log.Debug($"[DirectoryCopy] Ex for source '{source}' dest {destination} copySubDirs '{copySubDirs}: {ex}");
            }
        }
    }
}
