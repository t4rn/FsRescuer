using NLog;

namespace FS.Core.Services
{
    public static class LogService
    {
        internal static Logger analyse_log = LogManager.GetLogger("ANALYSE");
        internal static Logger move_log = LogManager.GetLogger("MOVE");
    }
}
