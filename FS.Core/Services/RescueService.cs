using FS.Core.Models;
using FS.Core.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FS.Core.Services
{
    public class RescueService
    {
        private readonly string _mainFs;
        private readonly string _oldFS;
        private readonly string _newFs;
        private readonly Logger _log;
        private readonly ApplicationRepository _applicationRepository;
        private readonly FsService _fsService;
        private readonly string _localAppDir;
        private readonly NotifyService _notifyService;

        public RescueService()
        {
            _log = LogService.move_log;
            _oldFS = Properties.Settings.Default.OldFs;
            _mainFs = Properties.Settings.Default.MainFs;
            _newFs = Properties.Settings.Default.NewFs;
            _localAppDir = Properties.Settings.Default.LocalAppDir;
            _applicationRepository = new ApplicationRepository();
            _fsService = new FsService(_log);
            _notifyService = new NotifyService(_log, Properties.Settings.Default.NotifyName);
        }

        public RescueAppResult StartMoving(int appCount)
        {
            RescueAppResult result = new RescueAppResult();

            string source = _mainFs;
            string destination = _newFs;

            try
            {
                _log.Debug($"Starting moving for '{appCount}' apps...");
                Stopwatch watch = Stopwatch.StartNew();

                // get applications for moving
                IEnumerable<ApplicationInfo> applicationsForMoving = _applicationRepository.GetApplicationsForMoving(appCount);

                decimal totalMegabytesMoved = 0;
                int processedApplication = 1;
                int applicationsForMovingCount = applicationsForMoving?.Count() ?? 0;

                _log.Debug($"'{applicationsForMovingCount}' for moving");

                foreach (ApplicationInfo application in applicationsForMoving)
                {
                    string appNumber = application.AppNumber;

                    try
                    {
                        _log.Debug($"{processedApplication}/{applicationsForMovingCount} - Start '{appNumber}'");

                        Duplex<bool, decimal?> destinationFsResult = _fsService.CheckOnFs(appNumber, destination);

                        if (destinationFsResult.Item1 == true)
                        {
                            // exists on destination FS
                            _log.Error($"'{appNumber}' exists on destination FS -> '{destination}'");

                            application.MoveStatus = "ERR";
                            application.MoveMessage = "Exists on destination FS";

                            _applicationRepository.UpdateMoveInfo(application);

                            result.Applications.Add(new WniosekResult { ExistedOnOldFS = true, Message = application.MoveMessage, ApplicationNumber = appNumber });
                        }
                        else
                        {
                            // local copy of directory
                            //_fsService.CopyDirectory(appNumber, source, _localAppDir);

                            // move from source FS to destination FS
                            _fsService.MoveDirectory(appNumber, source, destination);

                            // check size on destination FS after moving
                            destinationFsResult = _fsService.CheckOnFs(appNumber, destination);

                            decimal megabytesMoved = destinationFsResult.Item2 ?? 0;
                            application.IsMoved = true;
                            application.MoveStatus = "DONE";
                            application.MovedMb = megabytesMoved;
                            totalMegabytesMoved += megabytesMoved;
                            _applicationRepository.UpdateMoveInfo(application);

                            result.Applications.Add(new WniosekResult { IsOk = true, Message = "OK", ApplicationNumber = appNumber });
                        }

                        _log.Debug($"End for '{appNumber}'");
                    }
                    catch (Exception ex)
                    {
                        string logErrorMsg = PrepareErrorMsg(ex, appNumber);

                        _log.Error(logErrorMsg);
                        application.MoveStatus = "ERR";
                        application.MoveMessage = logErrorMsg;

                        _applicationRepository.UpdateMoveInfo(application);

                        result.Applications.Add(new WniosekResult { Message = ex.GetBaseException().Message, ApplicationNumber = appNumber });

                        _notifyService.AddError(ex.GetBaseException().Message, ex.GetBaseException().StackTrace,
                            $"{nameof(StartMoving)} Exception for appNumber = '{appNumber}': {logErrorMsg}");
                    }

                    processedApplication++;
                }

                TimeSpan elapsed = watch.Elapsed;
                string logMsg = $"Moving ended for '{applicationsForMovingCount}' applications - correct '{result.Applications.Where(x => x.IsOk == true).Count()}' - elapsed '{elapsed}' - moved {totalMegabytesMoved} MB";
                _log.Debug(logMsg);

                result.IsOk = true;
                result.Message = $"Ended successfully - moved {totalMegabytesMoved} MB!";
                result.Elapsed = elapsed;

                _notifyService.AddError(result.Message, logMsg, logMsg);
            }
            catch (Exception ex)
            {
                _log.Error($"Global Ex occured: {ex.GetBaseException()}");
                result.Message = ex.GetBaseException().ToString();
                _notifyService.AddError(ex.GetBaseException().Message, ex.GetBaseException().StackTrace,
                    $"{nameof(StartMoving)} Exception for source = '{source}' and destination = '{destination}'");
            }

            return result;
        }

        private string PrepareErrorMsg(Exception ex, string appNumber)
        {
            if (ex is IOException)
            {
                var ioEx = ex as IOException;
                string serializedData = ioEx.Data.Serialize();
                return $"Ex for '{appNumber}': {ex.GetBaseException()}\n-> IOException: {serializedData}";
            }
            else
                return $"Ex for '{appNumber}': {ex.GetBaseException()}";
        }

        public RescueAppResult StartCopying()
        {
            RescueAppResult result = new RescueAppResult();

            try
            {
                _log.Debug("Starting copying...");
                Stopwatch watch = Stopwatch.StartNew();

                // get applications for copying
                IEnumerable<ApplicationInfo> applicationsForCopy = _applicationRepository.GetApplicationsForCopy();

                _log.Debug($"'{applicationsForCopy?.Count()}' applications for copy");

                foreach (ApplicationInfo application in applicationsForCopy)
                {
                    string appNumber = application.AppNumber;

                    try
                    {
                        var oldFsResult = _fsService.CheckOnFs(appNumber, _oldFS);

                        if (oldFsResult.Item1 == false)
                        {
                            // doesn't exist on old FS
                            _log.Error($"'{appNumber}' doesn't exist on old FS -> '{_oldFS}'");

                            application.MoveStatus = "ERR";
                            application.MoveMessage = "Doesn't exist on old FS";

                            _applicationRepository.UpdateMoveInfo(application);

                            result.Applications.Add(new WniosekResult { ExistedOnOldFS = false, Message = application.MoveMessage, ApplicationNumber = appNumber });
                        }
                        else
                        {
                            // local copy of directory
                            _fsService.CopyDirectory(appNumber, _oldFS, _localAppDir);

                            //application.IsMoved = true;
                            //application.MoveStatus = "DONE";
                            //_applicationRepository.UpdateMoveInfo(application);

                            result.Applications.Add(new WniosekResult { IsOk = true, Message = "OK", ApplicationNumber = appNumber });
                        }

                        _log.Debug($"End for '{appNumber}'");
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Ex for '{appNumber}': {ex.GetBaseException()}");
                        application.MoveStatus = "ERR";
                        application.MoveMessage = ex.GetBaseException().Message;

                        _applicationRepository.UpdateMoveInfo(application);

                        result.Applications.Add(new WniosekResult { Message = ex.GetBaseException().Message, ApplicationNumber = appNumber });
                    }
                }

                var elapsed = watch.Elapsed;
                _log.Debug($"Copying ended for '{applicationsForCopy.Count()}' applications - correct '{result.Applications.Where(x => x.IsOk == true).Count()}' - elapsed '{elapsed}'");

                result.IsOk = true;
                result.Message = "Ended successfully!";
                result.Elapsed = elapsed;
            }
            catch (Exception ex)
            {
                _log.Error($"Global Ex occured: {ex.GetBaseException()}");
                result.Message = ex.GetBaseException().ToString();
                _notifyService.AddError(ex.GetBaseException().Message, ex.GetBaseException().StackTrace, $"{nameof(StartMoving)} Exception");
            }

            return result;
        }

        private void CreateDirIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _log.Debug($"Created directory '{path}'");
            }
        }
    }
}
