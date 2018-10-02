using FS.Core.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FS.Core.Services
{
    public class AnalyseService
    {
        private readonly string _newFS;
        private readonly string _oldFS;
        private readonly Logger _log;
        private readonly ApplicationRepository _applicationRepository;
        private readonly FsService _fsService;

        public AnalyseService()
        {
            _log = LogService.analyse_log;
            _oldFS = Properties.Settings.Default.OldFs;
            _newFS = Properties.Settings.Default.MainFs;
            _applicationRepository = new ApplicationRepository();
            _fsService = new FsService(_log);
        }

        public Result Analyse()
        {
            Result result = new Result();

            try
            {
                _log.Debug("Starting analysis...");
                Stopwatch watch = Stopwatch.StartNew();

                // get applications for analysis
                IEnumerable<ApplicationInfo> applicationsForAnalysis = _applicationRepository.GetAppInfoList();
                _log.Debug($"'{applicationsForAnalysis?.Count()}' applications for analysis");

                foreach (ApplicationInfo application in applicationsForAnalysis)
                {
                    string appNumber = application.AppNumber;
                    try
                    {
                        _log.Debug($"Start '{appNumber}'");

                        var oldFsResult = _fsService.CheckOnFs(appNumber, _oldFS);
                        application.IsOnOldFs = oldFsResult.Item1;
                        application.OldFsSize = oldFsResult.Item2;

                        var newFsResult = _fsService.CheckOnFs(appNumber, _newFS);
                        application.IsOnNewFs = newFsResult.Item1;
                        application.NewFsSize = newFsResult.Item2;

                        application.AnalyseStatus = "DONE";

                        _applicationRepository.UpdateAnalyseInfo(application);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Ex for '{appNumber}': {ex.GetBaseException()}");
                        application.AnalyseStatus = "ERR";
                        application.AnalyseMessage = ex.GetBaseException().Message;

                        _applicationRepository.UpdateAnalyseInfo(application);
                    }
                }

                result.IsOk = true;
                result.Message = $"Done for '{applicationsForAnalysis.Count()}'.";
                _log.Debug($"Ending analysis -> for '{applicationsForAnalysis.Count()}' elapsed '{watch.Elapsed}'");

            }
            catch (Exception ex)
            {
                _log.Error($"Global Ex occured: {ex.GetBaseException()}");
                result.Message = ex.GetBaseException().ToString();
            }

            return result;
        }
    }
}
