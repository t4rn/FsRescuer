using FS.Core.SoapNotify;
using NLog;
using System;

namespace FS.Core.Services
{
    public class NotifyService
    {
        private readonly Logger _log;
        private readonly DictionaryItemDTO _system;
        private readonly string _hash;

        public NotifyService(Logger log, string notifyName)
        {
            _log = log;
            _system = new DictionaryItemDTO()
            {
                Code = notifyName,
                Name = notifyName
            };
            _hash = "xxx";
        }

        public void AddError(string exMsg, string exStackTrace, string description)
        {
            ErrorDTO error = new ErrorDTO()
            {
                ErrorMessage = exMsg,
                StackTrace = exStackTrace,
                Description = description,
                OccuranceDate = DateTime.Now,
                System = _system
            };

            AddNotification(error);
        }

        public void AddMsg(string description)
        {
            MsgLogDTO msg = new MsgLogDTO()
            {
                Description = description,
                OccuranceDate = DateTime.Now,
                System = _system
            };

            AddNotification(msg);
        }

        private void AddNotification(NotificationDTO notification)
        {
            try
            {
                using (var client = new NotificationManager())
                {
                    ResultDTO r = client.AddNotification(notification, _hash);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[AddNotification] Exception: {ex.GetBaseException()}");
            }
        }
    }
}
